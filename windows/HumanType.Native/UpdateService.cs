using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HumanType.Native;

public sealed class UpdateService
{
    private const string LatestReleaseUrl = "https://api.github.com/repos/ethanstoner/humanlike-typer/releases/latest";
    private const string ReleasesApiUrl = "https://api.github.com/repos/ethanstoner/humanlike-typer/releases";
    private const string ReleasesUrl = "https://github.com/ethanstoner/humanlike-typer/releases";

    private readonly HttpClient httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(12)
    };

    public UpdateService()
    {
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("HumanType-Windows-Updater");
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
    }

    public string CurrentVersion => NormalizeVersion(
        Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ??
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ??
        "0.0.0");

    public async Task<UpdateCheckResult> CheckLatestAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(LatestReleaseUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var release = await JsonSerializer.DeserializeAsync<GitHubRelease>(stream, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("GitHub returned an empty release response.");

        var latestVersion = NormalizeVersion(release.TagName);
        var installerAsset = release.Assets.FirstOrDefault(asset =>
            asset.Name.Equals("HumanType-Installer.exe", StringComparison.OrdinalIgnoreCase)) ??
            release.Assets.FirstOrDefault(asset =>
                asset.Name.Contains("installer", StringComparison.OrdinalIgnoreCase) &&
                asset.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

        return new UpdateCheckResult(
            CurrentVersion,
            latestVersion,
            IsNewerVersion(latestVersion, CurrentVersion),
            release.Name,
            release.Body,
            release.HtmlUrl,
            installerAsset is not null,
            installerAsset?.BrowserDownloadUrl ?? release.HtmlUrl);
    }

    public async Task<IReadOnlyList<ReleaseNoteItem>> GetReleaseHistoryAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(ReleasesApiUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var releases = await JsonSerializer.DeserializeAsync<List<GitHubRelease>>(stream, cancellationToken: cancellationToken) ?? [];

        return releases
            .Where(release => !string.IsNullOrWhiteSpace(release.TagName))
            .Select(release => new ReleaseNoteItem(
                NormalizeVersion(release.TagName),
                string.IsNullOrWhiteSpace(release.Name) ? release.TagName : release.Name,
                release.Body,
                release.HtmlUrl))
            .ToArray();
    }

    public async Task<string> DownloadInstallerAsync(UpdateCheckResult release, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
    {
        if (!release.HasInstallerAsset)
        {
            throw new InvalidOperationException("The latest GitHub release does not include a Windows installer asset.");
        }

        var updateDirectory = Path.Combine(Path.GetTempPath(), "HumanType", "updates", release.LatestVersion);
        Directory.CreateDirectory(updateDirectory);

        var installerPath = Path.Combine(updateDirectory, "HumanType-Installer.exe");
        using var response = await httpClient.GetAsync(release.InstallerUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength;
        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var target = File.Create(installerPath);

        var buffer = new byte[1024 * 128];
        long totalRead = 0;
        while (true)
        {
            var read = await source.ReadAsync(buffer, cancellationToken);
            if (read == 0)
            {
                break;
            }

            await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            totalRead += read;

            if (totalBytes is > 0)
            {
                progress?.Report((int)Math.Clamp(totalRead * 100 / totalBytes.Value, 0, 100));
            }
        }

        progress?.Report(100);
        return installerPath;
    }

    public static void StartInstallerAndRelaunch(string installerPath)
    {
        if (!File.Exists(installerPath))
        {
            throw new FileNotFoundException("Downloaded installer was not found.", installerPath);
        }

        var appExePath = Environment.ProcessPath ?? Application.ExecutablePath;
        var scriptPath = Path.Combine(Path.GetTempPath(), "HumanType", "updates", $"apply-update-{Guid.NewGuid():N}.cmd");
        Directory.CreateDirectory(Path.GetDirectoryName(scriptPath)!);

        var script = $"""
@echo off
setlocal
set "INSTALLER={installerPath}"
set "APP={appExePath}"

timeout /t 2 /nobreak >nul
"%INSTALLER%" /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /CLOSEAPPLICATIONS
if errorlevel 1 exit /b %errorlevel%
if exist "%APP%" start "" "%APP%"
exit /b 0
""";

        File.WriteAllText(scriptPath, script);
        Process.Start(new ProcessStartInfo
        {
            FileName = scriptPath,
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });
    }

    public static void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = string.IsNullOrWhiteSpace(url) ? ReleasesUrl : url,
            UseShellExecute = true
        });
    }

    public static bool IsSameVersion(string left, string right)
    {
        return string.Equals(NormalizeVersion(left), NormalizeVersion(right), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNewerVersion(string latest, string current)
    {
        return ParseVersion(latest).CompareTo(ParseVersion(current)) > 0;
    }

    private static Version ParseVersion(string value)
    {
        var normalized = NormalizeVersion(value);
        var parts = normalized.Split('.', StringSplitOptions.RemoveEmptyEntries);
        while (parts.Length < 3)
        {
            normalized += ".0";
            parts = normalized.Split('.', StringSplitOptions.RemoveEmptyEntries);
        }

        return Version.TryParse(normalized, out var version) ? version : new Version(0, 0, 0);
    }

    private static string NormalizeVersion(string value)
    {
        var normalized = value.Trim();
        if (normalized.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[1..];
        }

        var metadataStart = normalized.IndexOfAny(['+', '-']);
        return metadataStart >= 0 ? normalized[..metadataStart] : normalized;
    }

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = ReleasesUrl;

        [JsonPropertyName("assets")]
        public List<GitHubAsset> Assets { get; set; } = [];
    }

    private sealed class GitHubAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = string.Empty;
    }
}

public sealed record UpdateCheckResult(
    string CurrentVersion,
    string LatestVersion,
    bool IsUpdateAvailable,
    string ReleaseName,
    string ReleaseNotes,
    string ReleasePageUrl,
    bool HasInstallerAsset,
    string InstallerUrl);

public sealed record ReleaseNoteItem(
    string Version,
    string Name,
    string Notes,
    string ReleasePageUrl);
