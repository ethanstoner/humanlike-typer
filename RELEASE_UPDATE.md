# Release Notes - v1.1.1

## 🎉 Windows Support is Here!

HumanLike Typer is now available for **both Windows and macOS**!

---

## 🆕 What's New

### Windows Version Added
- **Full Windows support** using AutoHotkey v2
- System tray integration with context menu
- Dark-themed settings GUI
- Hotkey: `Ctrl+Shift+V`
- Easily compile to standalone `.exe` if desired

### Platform-Specific Documentation
- Separate READMEs for [Windows](README_WINDOWS.md) and [macOS](README.md)
- Detailed installation guides for each platform
- Cross-platform troubleshooting sections

---

## ✨ Features (Both Platforms)

- ⌨️ **Realistic Typing**: Variable speed (90-130 WPM) with natural rhythm
- 🎯 **QWERTY Typos**: Adjacent-key mistakes (60%) + transpositions (40%)
- ✏️ **Auto-Correction**: Realistic backspace and correction delays
- 📝 **Smart Punctuation**: Converts smart quotes, em-dashes, ellipses to ASCII
- ⚙️ **Easy Configuration**: GUI settings panel for speed and typo rate
- 🚫 **ESC to Cancel**: Stop typing anytime

---

## 📥 Installation

### Windows
1. Download `init.ahk` from this release
2. Install [AutoHotkey v2](https://www.autohotkey.com/download/ahk-v2.exe)
3. Run the script (double-click `init.ahk`)
4. Use `Ctrl+Shift+V` to type!

**Optional:** Right-click `init.ahk` → "Compile Script" to create a standalone `.exe`

[📖 Full Windows Installation Guide](INSTALL_WINDOWS.md)

### macOS
1. Download `init.lua` from this release
2. Install [Hammerspoon](https://www.hammerspoon.org/)
3. Place `init.lua` in `~/.hammerspoon/`
4. Reload Hammerspoon
5. Use `Ctrl+Alt+Cmd+V` to type!

[📖 Full macOS Installation Guide](INSTALL_MAC.md)

---

## 🔧 Changes from v1.0.0

### Added
- Windows version with AutoHotkey v2
- Platform-specific READMEs and installation guides
- CHANGELOG.md for version tracking
- Enhanced .gitignore for cross-platform development

### Changed
- Windows hotkey: `Ctrl+Shift+V` (avoids Windows key conflicts)
- macOS hotkey: `Ctrl+Alt+Cmd+V` (unchanged)
- Improved documentation structure
- Direct AutoHotkey v2 download link

### Fixed
- Smart quote parsing errors in AutoHotkey script
- Global variable declarations for proper function scope
- Settings window initialization on different Hammerspoon builds
- Enhanced error handling in settings GUI

---

## 📋 System Requirements

### Windows
- Windows 10/11 (64-bit recommended)
- AutoHotkey v2.0+ (download above)

### macOS
- macOS 12.0 (Monterey) or later
- Hammerspoon 0.9.100+

---

## 📦 Release Assets

- **`init.ahk`** - Windows AutoHotkey v2 script (compile to .exe if desired)
- **`init.lua`** - macOS Hammerspoon script
- **Full source code** (zip/tar.gz)

---

## 🙏 Thank You!

Thank you for using HumanLike Typer! If you find this useful, please:
- ⭐ Star the repository
- 🐛 Report bugs via [Issues](https://github.com/ethanstoner/humanlike-typer/issues)
- 💡 Suggest features in [Discussions](https://github.com/ethanstoner/humanlike-typer/discussions)

---

## 📄 License

CC BY-NC-SA 4.0 - Free for personal/educational use. See [LICENSE](LICENSE) for details.

---

**Full Changelog**: https://github.com/ethanstoner/humanlike-typer/blob/main/CHANGELOG.md

