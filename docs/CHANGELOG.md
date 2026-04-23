# Changelog

All notable changes to HumanLike Typer will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

No unreleased changes yet.

## [1.1.3] - 2026-04-23

### Changed
- Windows installer now has explicit HumanType version metadata for Explorer and installer details.
- Local scratch directories are ignored more broadly.

### Verified
- Installer executable uses the same HumanType associated icon as the app executable.

## [1.1.2] - 2026-04-23

### Added
- GitHub Releases update checking for the native Windows app
- Release notes dialog shown from the settings window
- Tray menu item for checking updates
- One-time release notes display after an installed version changes

### Changed
- Windows settings sliders can be edited by dragging or typing exact values
- Slider rendering now avoids clipped handles and end-cap artifacts
- Close-to-tray behavior now intercepts the system close command before the normal close/cancel cycle
- Windows packaging now embeds version metadata used by the updater

### Fixed
- Slider thumb clipping at maximum values
- Extra visual bits at slider track edges
- Suffix clipping in slider value pills
- Flashing when closing the settings window with the X button

## [1.1.1] - 2025-10-31

### Added
- **Windows Support**: Full Windows version using AutoHotkey v2
- Windows-specific README ([README_WINDOWS.md](../README_WINDOWS.md))
- Windows installation guide ([INSTALL_WINDOWS.md](INSTALL_WINDOWS.md))
- Cross-platform .gitignore for both macOS and Windows

### Changed
- **Windows Hotkey**: Uses `Ctrl+Alt+V`
- **macOS Hotkey**: Remains `Ctrl+Alt+Cmd+V`
- Separated installation guides by platform (INSTALL_MAC.md, INSTALL_WINDOWS.md)
- Updated main README to be macOS-focused with prominent Windows link
- Direct download link for AutoHotkey v2: https://www.autohotkey.com/download/ahk-v2.exe

### Features (Both Platforms)
- Realistic typing simulation (90-130 WPM variable speed)
- QWERTY-based typos (60% adjacent-key, 40% transpositions)
- Automatic typo correction with realistic delays
- Smart punctuation conversion (smart quotes → ASCII)
- GUI settings panel with adjustable speed and typo rate
- System tray/menubar integration
- Non-blocking typing engine
- ESC to cancel typing

### Technical Details

**Windows Version:**
- Built with AutoHotkey v2
- System tray icon with context menu
- Hotkey: `Ctrl+Alt+V`
- Settings GUI with dark theme
- Can be compiled to standalone .exe

**macOS Version:**
- Built with Hammerspoon (Lua)
- Menubar icon (○ idle, ● typing)
- Hotkey: `Ctrl+Alt+Cmd+V`
- Webview-based settings panel

## [1.0.0] - 2025-10-30

### Added
- Initial release
- macOS version with Hammerspoon
- Basic typing simulation
- QWERTY typo patterns
- Settings GUI

