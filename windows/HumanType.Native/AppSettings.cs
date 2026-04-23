namespace HumanType.Native;

public sealed class AppSettings
{
    public int MinWpm { get; set; } = 90;
    public int MaxWpm { get; set; } = 130;
    public double TypoRate { get; set; } = 0.05;
    public bool RandomPausesEnabled { get; set; } = true;
    public double PauseChance { get; set; } = 0.08;
    public int PauseMinMs { get; set; } = 40;
    public int PauseMaxMs { get; set; } = 140;
    public string HotkeyModifiers { get; set; } = "Ctrl+Alt";
    public string HotkeyKey { get; set; } = "V";
    public string LastSeenVersion { get; set; } = string.Empty;
    public string LastDismissedUpdateVersion { get; set; } = string.Empty;
    public string LastInstalledAtUtc { get; set; } = string.Empty;
    public string LastUpdateCheckUtc { get; set; } = string.Empty;
    public string LastKnownLatestVersion { get; set; } = string.Empty;
    public string LastKnownReleaseNotes { get; set; } = string.Empty;
    public string LastKnownReleasePageUrl { get; set; } = string.Empty;

    public void Normalize()
    {
        MinWpm = Math.Clamp(MinWpm, 10, 260);
        MaxWpm = Math.Clamp(MaxWpm, 10, 260);

        if (MinWpm > MaxWpm)
        {
            (MinWpm, MaxWpm) = (MaxWpm, MinWpm);
        }

        TypoRate = Math.Clamp(TypoRate, 0.0, 0.30);
        PauseChance = Math.Clamp(PauseChance, 0.0, 1.0);
        PauseMinMs = Math.Clamp(PauseMinMs, 0, 2000);
        PauseMaxMs = Math.Clamp(PauseMaxMs, 0, 4000);

        if (PauseMinMs > PauseMaxMs)
        {
            (PauseMinMs, PauseMaxMs) = (PauseMaxMs, PauseMinMs);
        }

        HotkeyModifiers = string.IsNullOrWhiteSpace(HotkeyModifiers) ? "Ctrl+Alt" : HotkeyModifiers.Trim();
        HotkeyKey = string.IsNullOrWhiteSpace(HotkeyKey) ? "V" : HotkeyKey.Trim().ToUpperInvariant();
        LastSeenVersion = LastSeenVersion.Trim();
        LastDismissedUpdateVersion = LastDismissedUpdateVersion.Trim();
        LastInstalledAtUtc = LastInstalledAtUtc.Trim();
        LastUpdateCheckUtc = LastUpdateCheckUtc.Trim();
        LastKnownLatestVersion = LastKnownLatestVersion.Trim();
        LastKnownReleaseNotes = LastKnownReleaseNotes.Trim();
        LastKnownReleasePageUrl = LastKnownReleasePageUrl.Trim();
    }
}
