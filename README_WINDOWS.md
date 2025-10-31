<div align="center">

<img src="assets/humantyperlogo.png" alt="HumanLike Typer Logo" width="128" height="128">

# ⌨️ HumanLike Typer for Windows

**Realistic human typing automation for Windows**

![License](https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-blue?style=for-the-badge)
![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![AutoHotkey](https://img.shields.io/badge/AutoHotkey-v2.0+-green?style=for-the-badge)

*Simulates realistic human typing with QWERTY-based typos, variable speed, and natural rhythm*

---

### 🍎 **macOS User?** → **[Click here for macOS version](README.md)**

---

[Installation](#-installation) • [Usage](#-usage) • [Configuration](#-configuration) • [License](#-license)

</div>

---

## 🚀 Features

<div align="center">

| Feature | Description |
|---------|-------------|
| Realistic Typing | Variable speed (90-130 WPM) with natural pauses and rhythm |
| QWERTY Typos | 60% adjacent-key mistakes + 40% transpositions, auto-corrected |
| Smart Punctuation | Converts smart quotes, em-dashes, and ellipses to ASCII |
| Easy Configuration | GUI settings panel for speed and typo frequency |
| Simple Controls | System tray integration, keyboard shortcut, ESC to cancel |

</div>

## 🛠️ Tech Stack

<div align="center">

![AutoHotkey](https://img.shields.io/badge/AutoHotkey-334455?style=for-the-badge)
![Windows](https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![Keyboard](https://img.shields.io/badge/Keyboard_Automation-4285F4?style=for-the-badge)

</div>

## 📋 Requirements

- **Windows 10/11** (64-bit recommended)
- **AutoHotkey v2.0+** - [Download here](https://www.autohotkey.com/download/ahk-v2.exe)

## 💾 Installation

### 📦 Download Latest Release

**[Download v1.1.1 from Releases →](https://github.com/ethanstoner/humanlike-typer/releases/latest)**

Get the latest stable version with the `.ahk` script. You can compile it to `.exe` if desired (instructions below).

---

### Step 1: Install AutoHotkey v2

Download and install from [autohotkey.com/download/ahk-v2.exe](https://www.autohotkey.com/download/ahk-v2.exe)

### Step 2: Install HumanLike Typer

**Quick install:**

```powershell
curl -o "%USERPROFILE%\Documents\HumanLikeTyper.ahk" https://raw.githubusercontent.com/ethanstoner/humanlike-typer/main/scripts/init.ahk
```

Then double-click the file to run.

**Manual install:**

1. Download `init.ahk` from this repository
2. Save it anywhere (e.g., `Documents\HumanLikeTyper.ahk`)
3. Double-click to run

You should see a **keyboard icon** appear in your system tray.

### Optional: Compile to Executable

Want to run without installing AutoHotkey?

1. Right-click `init.ahk`
2. Select **"Compile Script"** (requires AutoHotkey v2 installed)
3. This creates `init.exe` - portable and no AHK needed!

> 💡 **Need detailed help?** See [INSTALL_WINDOWS.md](docs/INSTALL_WINDOWS.md) for complete installation guide and troubleshooting.

## 🎮 Usage

### Quick Start

<div align="center">

| Step | Action |
|------|--------|
| 1 | Copy text to clipboard (`Ctrl+C`) |
| 2 | Click into your target text field |
| 3 | Press `Ctrl+Shift+V` or use system tray |
| 4 | Watch realistic typing in action |

</div>

### Keyboard Shortcuts

```
Ctrl+Shift+V  →  Type clipboard
ESC           →  Stop typing
```

### System Tray Controls

Right-click the **keyboard icon** to access:
- **Type Clipboard** - Types your clipboard contents
- **Settings...** - Configure speed and typo rate
- **Reload** - Restart the script
- **Exit** - Close the application

## ⚙️ Configuration

### Default Settings

```
Min WPM: 90
Max WPM: 130
Typo Rate: 0.05 (5%)
Space Pause: 0.08 (8%)
```

### Adjust Settings

1. Right-click system tray icon → **Settings...**
2. Adjust values:
   - **Min/Max WPM** - Typing speed range (10-260)
   - **Typo Rate** - Frequency of typos (0.00-0.30)
3. Click **Save** or **Reset Defaults**

## 🎯 How It Works

### Typing Simulation

<div align="center">

| Component | Function |
|-----------|----------|
| **Variable Speed** | Random WPM within configured range |
| **Natural Pauses** | Delays after spaces and punctuation |
| **Rhythm Variation** | No two keystrokes at same speed |

</div>

### Typo Patterns

**Adjacent-Key Mistakes (60%)**

Types a nearby QWERTY key, pauses, backspaces, and corrects:
```
hello → hrllo → [backspace] → hello
```

**Transpositions (40%)**

Types next letter first, then reorders:
```
the → teh → [backspace] → the
```

### Smart Punctuation

<div align="center">

| Input | Output | Description |
|-------|--------|-------------|
| `'` | `'` | Smart apostrophe → straight |
| `" "` | `"` | Smart quotes → straight |
| `–` | `-` | En-dash → hyphen |
| `—` | `--` | Em-dash → double hyphen |
| `…` | `...` | Ellipsis → three dots |

</div>

## 📝 Examples

### Message with Smart Punctuation

**Input:**
```
Hey! How's it going? I'll send you the report—it's almost done...
```

**Output:**
```
Hey! How's it going? I'll send you the report--it's almost done...
```

Note: Smart punctuation automatically converted to ASCII.

### Code Typing

**Input:**
```python
def hello_world():
    print("Hello, world!")
```

**Result:** Types with realistic timing, handles syntax correctly.

## 🐛 Troubleshooting

<details>
<summary><strong>Script doesn't run or shows "requires AutoHotkey v2" error</strong></summary>

- Make sure you have **AutoHotkey v2.0+** installed (not v1.x)
- Download from [autohotkey.com/download/ahk-v2.exe](https://www.autohotkey.com/download/ahk-v2.exe)
- Uninstall any older v1.x versions first
</details>

<details>
<summary><strong>Nothing happens when I press the hotkey</strong></summary>

1. Check if script is running (look for keyboard icon in system tray)
2. Some applications block hotkeys (games, elevated apps)
3. Try running script as administrator
4. Try right-clicking tray icon → "Type Clipboard" instead
</details>

<details>
<summary><strong>Settings window doesn't appear</strong></summary>

1. Make sure script is running
2. Try running as administrator if you're in an elevated app
3. Right-click tray icon → Reload
</details>

<details>
<summary><strong>Typing speed is wrong</strong></summary>

Adjust Min/Max WPM in Settings. Lower values = slower, higher = faster.
</details>

<details>
<summary><strong>Too many/few typos</strong></summary>

Adjust Typo Rate slider. Set to 0.00 to disable typos completely.
</details>

<details>
<summary><strong>Typing goes to wrong place</strong></summary>

Make sure you click into your target text field **before** triggering the typing. The script types wherever your cursor is currently focused.
</details>

## 💡 Tips

### Run on Startup

1. Press `Win + R`
2. Type `shell:startup` and press Enter
3. Create a shortcut to `init.ahk` (or compile it to `init.exe`)
4. Place the shortcut in the Startup folder

### Compile to Executable

1. Right-click `init.ahk`
2. Select **"Compile Script"**
3. Creates `init.exe` - no AutoHotkey needed on other PCs!

## 📄 License

<div align="center">

**CC BY-NC-SA 4.0**

Licensed under Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International

Use for personal/educational projects  
Modify and share with attribution  
No commercial use or selling  

[View Full License](LICENSE) • [Creative Commons](https://creativecommons.org/licenses/by-nc-sa/4.0/)

</div>

## 🤝 Contributing

Contributions welcome! Here's how you can help:

- Report bugs via [Issues](https://github.com/ethanstoner/humanlike-typer/issues)
- Suggest features in [Discussions](https://github.com/ethanstoner/humanlike-typer/discussions)
- Improve documentation
- Submit pull requests

See [CONTRIBUTING.md](docs/CONTRIBUTING.md) for guidelines.

## 🏆 Credits

<div align="center">

Built with [AutoHotkey v2](https://www.autohotkey.com/download/ahk-v2.exe)

QWERTY adjacency mapping based on standard keyboard layout

---

**⭐ If you find this useful, consider giving it a star!**

</div>

---

<div align="center">

Made by [Ethan Stoner](https://github.com/ethanstoner)

</div>

