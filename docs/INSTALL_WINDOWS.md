# Windows Installation Guide

Install HumanType on Windows using the packaged native app.

## Download

Download `HumanType-Installer.exe` from the latest release:

https://github.com/ethanstoner/humanlike-typer/releases/latest

## Requirements

- Windows 10 or Windows 11
- No separate runtime required for the installed app

## Install

1. Download `HumanType-Installer.exe`.
2. Run the installer.
3. Choose the default install location unless you need a custom path.
4. Optionally enable a desktop shortcut or launch-at-sign-in shortcut.
5. Launch HumanType.

After launch, HumanType appears in the system tray.

## Update

HumanType checks GitHub Releases automatically after startup. If a newer release includes `HumanType-Installer.exe`, the app downloads the installer, applies it silently, exits the old app, and relaunches HumanType.

To check manually:

1. Open HumanType from the tray.
2. Go to the `Updates` section.
3. Click `Check Updates`.
4. Click `Install` or `Open Release` if a newer version is available.

Release notes are shown in the app after an installed version changes.

## Test Auto Updates

To verify the updater end to end:

1. Install an older HumanType build.
2. Make sure GitHub Releases has a newer tag with `HumanType-Installer.exe` attached.
3. Launch the older HumanType build.
4. Wait for the startup update check.
5. Confirm the app exits, installs the newer release, and reopens.
6. Check `HumanType.exe` file details or the update status in Settings to confirm the new version.

## Use

1. Copy text with `Ctrl+C`.
2. Click into the field where the text should be typed.
3. Press `Ctrl+Alt+V`, or select `Type Clipboard` from the tray menu.
4. Press `Esc` to pause/resume a typing run.

## Build From Source

Prerequisites:

- .NET 8 SDK
- Inno Setup 6

Build:

```powershell
powershell -ExecutionPolicy Bypass -File windows\build-release.ps1 -Version 1.1.3
```

Outputs:

- `windows\dist\HumanType.exe`
- `windows\dist\HumanType-Installer.exe`

## GitHub Release Checklist

Each Windows release should include:

- Tag such as `v1.1.3`
- `HumanType-Installer.exe`
- Release notes copied from `docs/CHANGELOG.md`

The app looks specifically for an `.exe` asset with `installer` in the name, and prefers the exact name `HumanType-Installer.exe`.

## Troubleshooting

### Typing does nothing

Make sure the target app accepts simulated keyboard input and that the text caret is focused.

### Some apps block typing

Elevated or protected apps can block simulated input. Run HumanType with the same privilege level as the target app.

### The installer cannot replace the app

Exit HumanType from the tray before installing an update.
