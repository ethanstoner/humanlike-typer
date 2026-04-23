using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HumanType.Native;

public sealed class HumanTypeAppContext : ApplicationContext
{
    private const int HotkeyId = 0x4854;

    private readonly SettingsStore settingsStore = new();
    private readonly TypingEngine typingEngine = new();
    private readonly UpdateService updateService = new();
    private readonly NotifyIcon trayIcon;
    private readonly HotkeyWindow hotkeyWindow;
    private readonly ContextMenuStrip trayMenu;
    private readonly GlobalKeyboardHook keyboardHook;
    private AppSettings settings;
    private SettingsForm? settingsForm;
    private UpdateCheckResult? latestRelease;
    private IntPtr lastExternalWindow;

    public HumanTypeAppContext()
    {
        settings = settingsStore.Load();
        trayMenu = BuildMenu();
        trayIcon = new NotifyIcon
        {
            Text = "HumanType",
            Visible = true,
            Icon = LoadTrayIcon()
        };

        trayIcon.MouseUp += OnTrayMouseUp;

        hotkeyWindow = new HotkeyWindow(() => _ = TypeClipboardAsync(NativeMethods.GetForegroundWindow(), useRetargetDelay: false));
        RegisterConfiguredHotkey(showWarning: true);

        keyboardHook = new GlobalKeyboardHook(() =>
        {
            if (typingEngine.IsTyping)
            {
                if (typingEngine.Pause())
                {
                    settingsForm?.SetStatusText("Paused");
                    return true;
                }
            }

            if (typingEngine.IsPaused)
            {
                _ = ResumeTypingAsync();
                return true;
            }

            return false;
        });

        ShowSettings();
        _ = RunStartupUpdateChecksAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            NativeMethods.UnregisterHotKey(hotkeyWindow.Handle, HotkeyId);
            hotkeyWindow.Dispose();
            keyboardHook.Dispose();
            settingsForm?.Dispose();
            trayMenu.Dispose();
            trayIcon.Visible = false;
            trayIcon.Dispose();
            typingEngine.Stop();
        }

        base.Dispose(disposing);
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip
        {
            ShowImageMargin = false,
            ShowCheckMargin = false,
            Renderer = new TrayMenuRenderer(),
            BackColor = Color.FromArgb(14, 18, 28),
            ForeColor = Color.FromArgb(236, 240, 255),
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold)
        };

        menu.Items.Add("Open HumanType", null, (_, _) => ShowSettings());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Type Clipboard", null, async (_, _) => await TypeClipboardAsync(IntPtr.Zero, useRetargetDelay: false));
        menu.Items.Add("Stop Typing", null, (_, _) => StopTyping("Stopped"));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Check for Updates", null, async (_, _) => await CheckForUpdatesAsync(showUpToDate: true));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitThread());
        return menu;
    }

    private void OnTrayMouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ShowSettings();
            return;
        }

        if (e.Button == MouseButtons.Right)
        {
            trayMenu.Show(Cursor.Position);
        }
    }

    private async Task TypeClipboardAsync(IntPtr preferredTarget, bool useRetargetDelay)
    {
        if (typingEngine.IsTyping || typingEngine.IsPaused)
        {
            StopTyping("Restarting...");
        }

        var clipboardText = await ClipboardReader.TryReadTextAsync();
        if (string.IsNullOrWhiteSpace(clipboardText))
        {
            trayIcon.ShowBalloonTip(2500, "HumanType", "Clipboard is empty.", ToolTipIcon.Info);
            return;
        }

        try
        {
            var targetWindow = ResolveTargetWindow(preferredTarget);

            if (targetWindow == IntPtr.Zero)
            {
                trayIcon.ShowBalloonTip(2500, "HumanType", "Select a target app first, then use HumanType again.", ToolTipIcon.Info);
                settingsForm?.SetStatusText("No target selected");
                return;
            }

            settingsForm?.SetStatusText("Typing...");
            await typingEngine.StartTypingAsync(clipboardText, settings, targetWindow);
            settingsForm?.SetStatusText(typingEngine.IsPaused ? "Paused" : "Idle");
        }
        catch (Exception ex)
        {
            settingsForm?.SetStatusText("Typing failed");
            trayIcon.ShowBalloonTip(3500, "HumanType", ex.Message, ToolTipIcon.Error);
        }
    }

    private async Task ResumeTypingAsync()
    {
        try
        {
            settingsForm?.SetStatusText("Typing...");
            await typingEngine.ResumeTypingAsync();
            settingsForm?.SetStatusText(typingEngine.IsPaused ? "Paused" : "Idle");
        }
        catch (Exception ex)
        {
            settingsForm?.SetStatusText("Typing failed");
            trayIcon.ShowBalloonTip(3500, "HumanType", ex.Message, ToolTipIcon.Error);
        }
    }

    private void ShowSettings()
    {
        RememberExternalWindow(NativeMethods.GetForegroundWindow());

        if (settingsForm is null || settingsForm.IsDisposed)
        {
            settingsForm = new SettingsForm(
                settings,
                GetHotkeyText(settings),
                () => TypeClipboardAsync(IntPtr.Zero, useRetargetDelay: false),
                () => StopTyping("Stopped"),
                ApplySettings,
                () => CheckForUpdatesAsync(showUpToDate: true),
                ShowReleaseNotes,
                () => _ = ShowReleaseHistoryAsync(),
                updateService.CurrentVersion);

            settingsForm.Deactivate += (_, _) => RememberExternalWindow(NativeMethods.GetForegroundWindow());
        }

        RefreshUpdateDetails();
        settingsForm.ShowAsPrimaryWindow();
    }

    private async Task RunStartupUpdateChecksAsync()
    {
        await Task.Delay(1800);
        await ShowInstalledReleaseNotesIfNeededAsync();
        await CheckForUpdatesAsync(showUpToDate: false);
    }

    private async Task ShowInstalledReleaseNotesIfNeededAsync()
    {
        var currentVersion = updateService.CurrentVersion;
        if (UpdateService.IsSameVersion(settings.LastSeenVersion, currentVersion))
        {
            return;
        }

        try
        {
            latestRelease = await updateService.CheckLatestAsync();
            PersistLatestRelease(latestRelease);
            if (UpdateService.IsSameVersion(latestRelease.LatestVersion, currentVersion))
            {
                settings.LastSeenVersion = currentVersion;
                settings.LastInstalledAtUtc = DateTime.UtcNow.ToString("O");
                settingsStore.Save(settings);
                RefreshUpdateDetails();
                ShowReleaseDialog(
                    $"HumanType {currentVersion}",
                    "This version is installed. Here is what changed in this release.",
                    latestRelease.ReleaseNotes,
                    "View Release",
                    () => UpdateService.OpenUrl(latestRelease.ReleasePageUrl));
            }
            else
            {
                settings.LastSeenVersion = currentVersion;
                settings.LastInstalledAtUtc = DateTime.UtcNow.ToString("O");
                settingsStore.Save(settings);
                RefreshUpdateDetails();
            }
        }
        catch
        {
            settings.LastSeenVersion = currentVersion;
            settings.LastInstalledAtUtc = DateTime.UtcNow.ToString("O");
            settingsStore.Save(settings);
            RefreshUpdateDetails();
        }
    }

    private async Task CheckForUpdatesAsync(bool showUpToDate)
    {
        try
        {
            settingsForm?.SetUpdateStatusText("Checking GitHub...");
            latestRelease = await updateService.CheckLatestAsync();
            PersistLatestRelease(latestRelease);
            RefreshUpdateDetails();

            if (latestRelease.IsUpdateAvailable)
            {
                settingsForm?.SetUpdateStatusText($"Update available: {latestRelease.LatestVersion}");
                if (!showUpToDate && latestRelease.HasInstallerAsset)
                {
                    await InstallUpdateAndExitAsync(latestRelease);
                    return;
                }

                if (showUpToDate || !UpdateService.IsSameVersion(settings.LastDismissedUpdateVersion, latestRelease.LatestVersion))
                {
                    ShowReleaseDialog(
                        $"HumanType {latestRelease.LatestVersion} is available",
                        latestRelease.HasInstallerAsset
                            ? $"You are running {latestRelease.CurrentVersion}. Install the update from GitHub Releases now."
                            : $"You are running {latestRelease.CurrentVersion}. Open the GitHub release to download the available update.",
                        latestRelease.ReleaseNotes,
                        latestRelease.HasInstallerAsset ? "Install" : "Open Release",
                        latestRelease.HasInstallerAsset
                            ? () => _ = InstallUpdateAndExitAsync(latestRelease)
                            : () => UpdateService.OpenUrl(latestRelease.InstallerUrl));

                    if (!showUpToDate)
                    {
                        settings.LastDismissedUpdateVersion = latestRelease.LatestVersion;
                        settingsStore.Save(settings);
                    }
                }

                return;
            }

            settingsForm?.SetUpdateStatusText($"Up to date: {latestRelease.CurrentVersion}");
            if (showUpToDate)
            {
                ShowReleaseDialog(
                    "HumanType is up to date",
                    $"You are running the latest release: {latestRelease.CurrentVersion}.",
                    latestRelease.ReleaseNotes,
                    "View Release",
                    () => UpdateService.OpenUrl(latestRelease.ReleasePageUrl));
            }
        }
        catch (Exception ex)
        {
            settingsForm?.SetUpdateStatusText("Update check failed");
            settings.LastUpdateCheckUtc = DateTime.UtcNow.ToString("O");
            settingsStore.Save(settings);
            RefreshUpdateDetails();
            if (showUpToDate)
            {
                MessageBox.Show(
                    settingsForm,
                    $"HumanType could not check GitHub Releases right now.\n\n{ex.Message}",
                    "Update Check Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }

    private void ShowReleaseNotes()
    {
        if (latestRelease is not null)
        {
            ShowReleaseDialog(
                $"HumanType {latestRelease.LatestVersion}",
                latestRelease.IsUpdateAvailable
                    ? $"You are running {latestRelease.CurrentVersion}. A newer version is available."
                    : "These are the latest published release notes.",
                latestRelease.ReleaseNotes,
                latestRelease.IsUpdateAvailable
                    ? latestRelease.HasInstallerAsset ? "Install" : "Open Release"
                    : "View Release",
                latestRelease.IsUpdateAvailable && latestRelease.HasInstallerAsset
                    ? () => _ = InstallUpdateAndExitAsync(latestRelease)
                    : () => UpdateService.OpenUrl(latestRelease.IsUpdateAvailable ? latestRelease.InstallerUrl : latestRelease.ReleasePageUrl));
            return;
        }

        _ = CheckForUpdatesAsync(showUpToDate: true);
    }

    private async Task ShowReleaseHistoryAsync()
    {
        try
        {
            settingsForm?.SetUpdateStatusText("Loading release history...");
            var releases = await updateService.GetReleaseHistoryAsync();
            settingsForm?.SetUpdateStatusText("Release history loaded.");
            settingsForm?.ShowReleaseHistoryOverlay(releases);
        }
        catch (Exception ex)
        {
            settingsForm?.SetUpdateStatusText("Release history failed");
            MessageBox.Show(
                settingsForm,
                $"HumanType could not load release history from GitHub.\n\n{ex.Message}",
                "Release History Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void PersistLatestRelease(UpdateCheckResult release)
    {
        settings.LastUpdateCheckUtc = DateTime.UtcNow.ToString("O");
        settings.LastKnownLatestVersion = release.LatestVersion;
        settings.LastKnownReleaseNotes = release.ReleaseNotes;
        settings.LastKnownReleasePageUrl = release.ReleasePageUrl;
        settingsStore.Save(settings);
    }

    private void RefreshUpdateDetails()
    {
        settingsForm?.SetUpdateDetails(
            settings.LastKnownLatestVersion,
            FormatStoredUtc(settings.LastUpdateCheckUtc),
            FormatStoredUtc(settings.LastInstalledAtUtc));
    }

    private static string FormatStoredUtc(string value)
    {
        if (!DateTimeOffset.TryParse(value, out var timestamp))
        {
            return string.Empty;
        }

        return timestamp.ToLocalTime().ToString("MMM d, yyyy h:mm tt");
    }

    private async Task InstallUpdateAndExitAsync(UpdateCheckResult release)
    {
        try
        {
            typingEngine.Stop();
            settingsForm?.SetUpdateStatusText($"Downloading {release.LatestVersion}...");
            settingsForm?.SetStatusText("Updating...");

            var progress = new Progress<int>(percent =>
            {
                settingsForm?.SetUpdateStatusText($"Downloading {release.LatestVersion}: {percent}%");
            });

            var installerPath = await updateService.DownloadInstallerAsync(release, progress);
            settingsForm?.SetUpdateStatusText("Installing update...");
            trayIcon.ShowBalloonTip(2000, "HumanType", $"Installing {release.LatestVersion}. HumanType will reopen automatically.", ToolTipIcon.Info);
            UpdateService.StartInstallerAndRelaunch(installerPath);
            ExitThread();
        }
        catch (Exception ex)
        {
            settingsForm?.SetUpdateStatusText("Automatic update failed");
            MessageBox.Show(
                settingsForm,
                $"HumanType could not install the update automatically.\n\n{ex.Message}",
                "Update Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void ShowReleaseDialog(string title, string subtitle, string notes, string primaryText, Action primaryAction)
    {
        if (settingsForm is null || settingsForm.IsDisposed)
        {
            ShowSettings();
        }

        settingsForm?.ShowReleaseOverlay(title, subtitle, notes, primaryText, primaryAction);
    }

    private IntPtr ResolveTargetWindow(IntPtr preferredTarget)
    {
        if (IsExternalWindow(preferredTarget))
        {
            lastExternalWindow = preferredTarget;
            return preferredTarget;
        }

        var foregroundWindow = NativeMethods.GetForegroundWindow();
        if (IsExternalWindow(foregroundWindow))
        {
            lastExternalWindow = foregroundWindow;
            return foregroundWindow;
        }

        return IsExternalWindow(lastExternalWindow) ? lastExternalWindow : IntPtr.Zero;
    }

    private void RememberExternalWindow(IntPtr handle)
    {
        if (IsExternalWindow(handle))
        {
            lastExternalWindow = handle;
        }
    }

    private bool IsExternalWindow(IntPtr handle)
    {
        return handle != IntPtr.Zero &&
               NativeMethods.IsWindow(handle) &&
               (settingsForm is null || handle != settingsForm.Handle);
    }

    private void StopTyping(string status)
    {
        typingEngine.Stop();
        settingsForm?.SetStatusText(status);
    }

    private void ApplySettings(AppSettings newSettings)
    {
        newSettings.Normalize();
        settings = newSettings;
        settingsStore.Save(settings);
        typingEngine.UpdatePausedSettings(settings);
        RegisterConfiguredHotkey(showWarning: true);
        settingsForm?.UpdateHotkeyText(GetHotkeyText(settings));
        settingsForm?.SetStatusText("Applied");
    }

    private void RegisterConfiguredHotkey(bool showWarning)
    {
        NativeMethods.UnregisterHotKey(hotkeyWindow.Handle, HotkeyId);
        if (TryBuildHotkey(settings, out var modifiers, out var key))
        {
            if (!NativeMethods.RegisterHotKey(hotkeyWindow.Handle, HotkeyId, modifiers, key) && showWarning)
            {
                trayIcon.ShowBalloonTip(4000, "HumanType", $"Failed to register {GetHotkeyText(settings)}.", ToolTipIcon.Warning);
            }
        }
        else if (showWarning)
        {
            trayIcon.ShowBalloonTip(4000, "HumanType", "Hotkey configuration is invalid.", ToolTipIcon.Warning);
        }
    }

    private static bool TryBuildHotkey(AppSettings settings, out uint modifiers, out uint key)
    {
        modifiers = 0;
        key = 0;

        foreach (var part in settings.HotkeyModifiers.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            switch (part.ToLowerInvariant())
            {
                case "ctrl":
                case "control":
                    modifiers |= NativeMethods.ModControl;
                    break;
                case "alt":
                    modifiers |= NativeMethods.ModAlt;
                    break;
                case "shift":
                    modifiers |= NativeMethods.ModShift;
                    break;
            }
        }

        if (Enum.TryParse<Keys>(settings.HotkeyKey, true, out var parsedKey))
        {
            key = (uint)parsedKey;
        }

        return modifiers != 0 && key != 0;
    }

    private static string GetHotkeyText(AppSettings settings)
    {
        return $"{settings.HotkeyModifiers}+{settings.HotkeyKey}";
    }

    private static Icon LoadTrayIcon()
    {
        var exeDirectory = AppContext.BaseDirectory;
        var candidatePaths = new[]
        {
            Path.Combine(exeDirectory, "HumanType.ico"),
            Path.GetFullPath(Path.Combine(exeDirectory, "..", "..", "..", "assets", "HumanType.ico"))
        };

        foreach (var candidate in candidatePaths)
        {
            if (File.Exists(candidate))
            {
                return new Icon(candidate);
            }
        }

        return SystemIcons.Application;
    }

    private sealed class HotkeyWindow : NativeWindow, IDisposable
    {
        private readonly Action onHotkey;

        public HotkeyWindow(Action onHotkey)
        {
            this.onHotkey = onHotkey;
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WmHotKey)
            {
                onHotkey();
            }

            base.WndProc(ref m);
        }

        public void Dispose()
        {
            DestroyHandle();
        }
    }

    private sealed class GlobalKeyboardHook : IDisposable
    {
        private readonly Func<bool> onEscapePressed;
        private readonly NativeMethods.HookProc hookCallback;
        private readonly IntPtr hookHandle;

        public GlobalKeyboardHook(Func<bool> onEscapePressed)
        {
            this.onEscapePressed = onEscapePressed;
            hookCallback = HookProc;

            using var process = Process.GetCurrentProcess();
            using var module = process.MainModule;
            var moduleHandle = NativeMethods.GetModuleHandle(module?.ModuleName);
            hookHandle = NativeMethods.SetWindowsHookEx(NativeMethods.WhKeyboardLl, hookCallback, moduleHandle, 0);
        }

        public void Dispose()
        {
            if (hookHandle != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(hookHandle);
            }
        }

        private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 &&
                (wParam == (IntPtr)NativeMethods.WmKeyDown || wParam == (IntPtr)NativeMethods.WmSysKeyDown))
            {
                var keyInfo = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
                if (keyInfo.vkCode == NativeMethods.VkEscape && onEscapePressed())
                {
                    return (IntPtr)1;
                }
            }

            return NativeMethods.CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }
    }
}

internal static class ClipboardReader
{
    public static Task<string?> TryReadTextAsync()
    {
        var tcs = new TaskCompletionSource<string?>();
        var thread = new Thread(() =>
        {
            try
            {
                tcs.SetResult(Clipboard.ContainsText() ? Clipboard.GetText() : null);
            }
            catch
            {
                tcs.SetResult(null);
            }
            finally
            {
                Application.ExitThread();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return tcs.Task;
    }
}
