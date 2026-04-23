<div align="center">

<img src="assets/branding/humantype-logo-source.png" alt="HumanType Logo" width="128" height="128">

# HumanType

**Realistic human typing automation for Windows and macOS**

![License](https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-blue?style=for-the-badge)
![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![macOS](https://img.shields.io/badge/macOS-12.0+-blue?style=for-the-badge&logo=apple&logoColor=white)
![Version](https://img.shields.io/badge/version-1.1.3-green?style=for-the-badge)

[Download Latest Release](https://github.com/ethanstoner/humanlike-typer/releases/latest) | [Windows Guide](README_WINDOWS.md) | [macOS Guide](docs/INSTALL_MAC.md) | [Changelog](docs/CHANGELOG.md)

</div>

## Overview

HumanType types clipboard text with natural rhythm, variable speed, typo simulation, and smart punctuation cleanup. It is useful for demos, tutorials, presentations, and workflows where text should appear as if it is being typed by a person.

The Windows edition is now a native packaged app with a tray menu, settings panel, installer, and GitHub release update checks. The macOS edition uses Hammerspoon.

## Features

| Feature | Windows | macOS |
| --- | --- | --- |
| Type clipboard text | Yes | Yes |
| Variable WPM | Yes | Yes |
| QWERTY typo simulation | Yes | Yes |
| Smart punctuation cleanup | Yes | Yes |
| Settings UI | Native app | Hammerspoon webview |
| System tray / menu bar | Yes | Yes |
| Release update checks | Yes | Basic latest-release check |

## Download

### Windows

Download `HumanType-Installer.exe` from the latest release:

https://github.com/ethanstoner/humanlike-typer/releases/latest

See [README_WINDOWS.md](README_WINDOWS.md) for the full Windows guide.

### macOS

Install Hammerspoon, then install the macOS script:

```bash
curl -o ~/.hammerspoon/init.lua https://raw.githubusercontent.com/ethanstoner/humanlike-typer/main/scripts/init.lua
```

See [docs/INSTALL_MAC.md](docs/INSTALL_MAC.md) for the full macOS guide.

## Quick Start

### Windows

1. Install HumanType with `HumanType-Installer.exe`.
2. Copy text with `Ctrl+C`.
3. Click into the target text field.
4. Press `Ctrl+Alt+V`, or use `Type Clipboard` from the tray menu.
5. Press `Esc` to pause or resume an active typing run.

### macOS

1. Copy text with `Cmd+C`.
2. Click into the target text field.
3. Press `Ctrl+Alt+Cmd+V`, or use the menu bar item.
4. Press `Esc` to stop typing.

## How It Works

HumanType sends keystrokes with randomized timing. It can introduce realistic mistakes, pause, backspace, and correct them automatically.

Examples:

```text
hello -> hrllo -> backspace -> hello
the   -> teh   -> backspace -> the
```

Smart punctuation is normalized before typing:

| Input | Output |
| --- | --- |
| Smart quotes | Straight quotes |
| En dash | `-` |
| Em dash | `--` |
| Ellipsis | `...` |

## Build Windows Release

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

## Repository Layout

| Path | Purpose |
| --- | --- |
| `windows/HumanType.Native` | Native Windows app |
| `windows/installer` | Inno Setup installer |
| `windows/dist` | Built Windows release artifacts |
| `macos/HumanType` | Native macOS project scaffold |
| `scripts/init.lua` | Hammerspoon script |
| `scripts/init.ahk` | Legacy AutoHotkey script |
| `docs` | Installation docs, changelog, release notes |

## Troubleshooting

### Windows typing does nothing

Make sure the target app accepts simulated keyboard input and that the text caret is focused. Some elevated or protected apps require HumanType to run with the same privilege level.

### Windows update check opens GitHub instead of downloading

The GitHub release is missing `HumanType-Installer.exe`. Upload that asset to the release for direct in-app download links.

### macOS shortcut does nothing

Check Hammerspoon Accessibility permissions in System Settings.

## License

HumanType is licensed under CC BY-NC-SA 4.0. See [LICENSE](LICENSE).

## Contributing

Reports and pull requests are welcome. See [docs/CONTRIBUTING.md](docs/CONTRIBUTING.md).
