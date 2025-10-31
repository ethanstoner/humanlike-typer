# Windows Installation Guide

Get started with HumanLike Typer on Windows in 3 simple steps!

> **🍎 Looking for macOS?** See [INSTALL_MAC.md](INSTALL_MAC.md)

## 📦 Quick Download

**[Download Latest Release (v1.1.1) →](https://github.com/ethanstoner/humanlike-typer/releases/latest)**

Includes the `.ahk` script. Compile to `.exe` yourself if desired!

---

## Prerequisites

- **Windows 10/11** (64-bit recommended)
- **AutoHotkey v2.0+** - Download from [autohotkey.com/download/ahk-v2.exe](https://www.autohotkey.com/download/ahk-v2.exe)

## Installation

### Option 1: Run as Script (Recommended for Testing)

**Step 1: Install AutoHotkey v2**

Download and install from [autohotkey.com/download/ahk-v2.exe](https://www.autohotkey.com/download/ahk-v2.exe)

**Step 2: Download the Script**

```powershell
# Download init.ahk to your desired location
curl -o "%USERPROFILE%\Documents\HumanLikeTyper.ahk" https://raw.githubusercontent.com/ethanstoner/humanlike-typer/main/init.ahk
```

Or manually:
1. Download [init.ahk](https://raw.githubusercontent.com/ethanstoner/humanlike-typer/main/init.ahk)
2. Save it anywhere (e.g., `Documents\HumanLikeTyper.ahk`)

**Step 3: Run the Script**

- **Double-click** `init.ahk` to run
- Look for the keyboard icon in your system tray

### Option 2: Compile to Executable (Recommended for Daily Use)

**Step 1: Install AutoHotkey v2** (same as above)

**Step 2: Compile the Script**

1. Right-click `init.ahk`
2. Select **"Compile Script"** from the context menu
3. This creates `init.exe` in the same folder

**Step 3: Run the Executable**

- Double-click `init.exe`
- No AutoHotkey installation needed on other PCs!

**Optional: Add to Startup**

To run automatically on Windows startup:
1. Press `Win + R`
2. Type `shell:startup` and press Enter
3. Copy `init.exe` (or a shortcut to it) into the Startup folder

## Quick Start

1. **Copy some text** (`Ctrl+C`)
2. **Click into a text field**
3. **Press `Ctrl+Shift+V`** or right-click tray icon → "Type Clipboard"
4. **Watch it type!**

## Configuration

Right-click the tray icon and select **Settings...** to configure:
- Typing speed (WPM range)
- Typo frequency
- Reset to defaults

## Tray Icon

- **Keyboard icon** - Tray icon for HumanLike Typer
- **Right-click** to open menu
  - Type Clipboard
  - Settings...
  - Reload
  - Exit

## Troubleshooting

### Script doesn't run?

**Make sure you have AutoHotkey v2 installed:**
- Download and install from [autohotkey.com/download/ahk-v2.exe](https://www.autohotkey.com/download/ahk-v2.exe)

**If you see "This script requires AutoHotkey v2.0":**
- Uninstall AutoHotkey v1.x
- Install AutoHotkey v2.0+ from [autohotkey.com/download/ahk-v2.exe](https://www.autohotkey.com/download/ahk-v2.exe)

### Nothing happens when I press the hotkey?

1. Check if the script is running (look for tray icon)
2. Some applications block hotkeys (games, elevated apps)
3. Try right-clicking tray icon → "Type Clipboard" instead

### Settings window doesn't appear?

1. Make sure script is running as administrator if you're in an elevated app
2. Try reloading the script (right-click tray icon → Reload)

### Typing is strange or has issues?

Make sure you're focused in a **text input field** before triggering the typing. The script types wherever your cursor is focused.

### Script conflicts with other AutoHotkey scripts?

If you have other AHK scripts running:
- They should coexist fine
- If conflicts occur, check for duplicate hotkeys
- Consider using `#SingleInstance Force` in your scripts

## Security Notes

- **Always review scripts before running** - You can open `init.ahk` in any text editor
- The script only:
  - Reads your clipboard when you trigger it
  - Simulates keyboard typing
  - Creates a settings GUI window
  - Shows a tray icon
- **No network access, no file access, no data collection**

## Uninstall

**If running as script:**
1. Right-click tray icon → Exit
2. Delete `init.ahk`

**If using compiled .exe:**
1. Right-click tray icon → Exit
2. Delete `init.exe`
3. Remove from Startup folder if you added it there

**Restore original files:**
```powershell
# Remove the script
del "%USERPROFILE%\Documents\HumanLikeTyper.ahk"
```

## Hotkeys

| Hotkey | Action |
|--------|--------|
| `Ctrl+Shift+V` | Type clipboard contents |
| `Esc` | Stop typing (while typing in progress) |

## Next Steps

- Read the full [README.md](README.md) for features and detailed usage
- Check out [QUICK_REFERENCE.md](QUICK_REFERENCE.md) for a cheat sheet
- Report issues on [GitHub](https://github.com/ethanstoner/humanlike-typer/issues)
- Star the repo if you find it useful! ⭐

---

**Happy typing!** ⌨️

