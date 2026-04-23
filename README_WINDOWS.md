<div align="center">

<img src="assets/branding/humantype-logo-source.png" alt="HumanType Logo" width="128" height="128">

# HumanType for Windows

**A native Windows app for realistic human typing automation**

![License](https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-blue?style=for-the-badge)
![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![Version](https://img.shields.io/badge/version-1.1.3-green?style=for-the-badge)

[Download Latest Release](https://github.com/ethanstoner/humanlike-typer/releases/latest) | [Install Guide](docs/INSTALL_WINDOWS.md) | [Changelog](docs/CHANGELOG.md)

</div>

## Overview

HumanType types clipboard text into the active app with natural rhythm, variable WPM, realistic typo correction, smart punctuation cleanup, and random pauses. The Windows edition is a native packaged app with a system tray menu, global hotkey, polished settings window, and GitHub release update checks.

## Download

Download `HumanType-Installer.exe` from the latest GitHub release:

https://github.com/ethanstoner/humanlike-typer/releases/latest

The installed app includes its own runtime. You do not need AutoHotkey or .NET installed separately.

## Features

| Feature | Description |
| --- | --- |
| Native Windows app | Packaged WinForms app with installer and tray integration |
| Realistic typing | Variable speed, rhythm variation, and punctuation-aware pauses |
| Editable settings | Sliders support both dragging and direct typed values |
| Typo simulation | Adjacent-key and transposition mistakes with automatic correction |
| Global hotkey | Default `Ctrl+Alt+V` to type clipboard contents |
| Pause/resume | Press `Esc` to pause, then `Esc` again to resume |
| Updates | Installs newer GitHub Releases automatically and shows release notes |

## Quick Start

1. Install HumanType with `HumanType-Installer.exe`.
2. Copy text with `Ctrl+C`.
3. Click into the target text field.
4. Press `Ctrl+Alt+V`, or use the tray menu's `Type Clipboard`.
5. Press `Esc` to pause or resume an active typing run.

## Settings

Open the settings window from the tray icon. Changes apply immediately and are stored locally.

| Setting | Range |
| --- | --- |
| Minimum WPM | 10-260 |
| Maximum WPM | 10-260 |
| Typo Rate | 0-30% |
| Pause Frequency | 0-100% |
| Pause Minimum | 0-1200 ms |
| Pause Maximum | 0-2000 ms |

## Updates

HumanType checks GitHub Releases in the background when it starts. If a newer release includes `HumanType-Installer.exe`, HumanType downloads it, runs the installer silently, exits the old app, and reopens after the update.

You can also open the settings window and use:

- `Check Updates` to manually check GitHub.
- `Release Notes` to view the latest release notes.
- `Install` or `Open Release` when a newer release is available.

For direct update downloads, each GitHub release should include `HumanType-Installer.exe`.

## Troubleshooting

### Nothing happens when I press the hotkey

- Make sure HumanType is running in the system tray.
- Click into the target text field before starting.
- Some elevated or protected apps block simulated input. Run HumanType with the same privilege level as the target app if needed.

### The update button opens GitHub instead of downloading

The release is missing `HumanType-Installer.exe`. Upload the installer asset to the GitHub release and the app will use it directly.

### Settings do not look right after updating

Exit HumanType from the tray menu and relaunch it. If the app was open while installing, Windows may keep the old process running until it is closed.

## Build From Source

Install prerequisites:

- .NET 8 SDK
- Inno Setup 6

Build release artifacts:

```powershell
powershell -ExecutionPolicy Bypass -File windows\build-release.ps1 -Version 1.1.3
```

Artifacts are written to:

- `windows\dist\HumanType.exe`
- `windows\dist\HumanType-Installer.exe`

## License

HumanType is licensed under CC BY-NC-SA 4.0. See [LICENSE](LICENSE).
