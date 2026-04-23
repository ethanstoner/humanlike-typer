using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HumanType.Native;

public sealed class UpdateService
{
    private const string LatestReleaseUrl = "https://api.github.com/repos/ethanstoner/humanlike-typer/releases/latest";
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
