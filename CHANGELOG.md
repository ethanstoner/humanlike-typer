# Changelog

All notable changes to HumanLike Typer will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.1] - 2025-10-31

### Added
- **Windows Support**: Full Windows version using AutoHotkey v2
- Windows-specific README ([README_WINDOWS.md](README_WINDOWS.md))
- Windows installation guide ([INSTALL_WINDOWS.md](INSTALL_WINDOWS.md))
- Cross-platform .gitignore for both macOS and Windows

### Changed
- **Windows Hotkey**: Uses `Ctrl+Shift+V` (avoids Windows key)
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
- Hotkey: `Ctrl+Shift+V`
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

