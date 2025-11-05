<div align="center">

<img src="assets/humantyperlogo.png" alt="HumanLike Typer Logo" width="128" height="128">

# HumanLike Typer

**Realistic human typing automation for macOS & Windows**

![License](https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-blue?style=for-the-badge)
![macOS](https://img.shields.io/badge/macOS-12.0+-blue?style=for-the-badge&logo=apple&logoColor=white)
![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![Version](https://img.shields.io/badge/version-1.1.1-green?style=for-the-badge)

*Simulates realistic human typing with QWERTY-based typos, variable speed, and natural rhythm*

---

## Overview

HumanLike Typer is a sophisticated typing automation tool that simulates natural human typing patterns. It features variable typing speeds, realistic typo generation based on QWERTY keyboard layout, smart punctuation conversion, and intuitive GUI controls. Perfect for demonstrations, tutorials, and scenarios where natural typing simulation is required.

### Quick Start

| Platform | Instructions |
|:--------:|:------------:|
| **macOS** | Continue reading below |
| **Windows** | **[Windows Installation →](README_WINDOWS.md)** |

---

[Installation](#-installation) • [Usage](#-usage) • [Configuration](#-configuration) • [Documentation](docs/) • [License](#-license)

</div>

---

## Features

<div align="center">

| Feature | Description |
|---------|-------------|
| Realistic Typing | Variable speed (90-130 WPM) with natural pauses and rhythm |
| QWERTY Typos | 60% adjacent-key mistakes + 40% transpositions, auto-corrected |
| Smart Punctuation | Converts smart quotes, em-dashes, and ellipses to ASCII |
| Easy Configuration | GUI settings panel for speed and typo frequency |
| Simple Controls | Menubar integration, keyboard shortcut, ESC to cancel |

</div>

## Tech Stack

<div align="center">

![Lua](https://img.shields.io/badge/Lua-2C2D72?style=for-the-badge&logo=lua&logoColor=white)
![Hammerspoon](https://img.shields.io/badge/Hammerspoon-FF6600?style=for-the-badge)
![macOS](https://img.shields.io/badge/macOS-000000?style=for-the-badge&logo=apple&logoColor=white)
![Keyboard](https://img.shields.io/badge/Keyboard_Automation-4285F4?style=for-the-badge)

</div>

## Requirements

- **macOS 12.0** (Monterey) or later
- **Hammerspoon** - [Download here](https://www.hammerspoon.org/)

## Installation

### Download Latest Release

**[Download v1.1.1 from Releases →](https://github.com/ethanstoner/humanlike-typer/releases/latest)**

Get the latest stable version with pre-configured settings and bug fixes.

---

### Step 1: Install Hammerspoon

Download from [hammerspoon.org](https://www.hammerspoon.org/) and grant Accessibility permissions:

```
System Settings → Privacy & Security → Accessibility → Enable Hammerspoon
```

### Step 2: Install HumanLike Typer

**Quick install:**

```bash
curl -o ~/.hammerspoon/init.lua https://raw.githubusercontent.com/ethanstoner/humanlike-typer/main/scripts/init.lua
```

Then reload Hammerspoon from the menubar icon.

**Manual install:**

1. Download `init.lua` from this repository
2. Place it in `~/.hammerspoon/init.lua`
3. Reload Hammerspoon

You should see a **○** icon appear in your menubar.

> **Need detailed help?** See [INSTALL_MAC.md](docs/INSTALL_MAC.md) for complete installation guide and troubleshooting.

## Usage

### Quick Start

<div align="center">

| Step | Action |
|------|--------|
| 1 | Copy text to clipboard (`Cmd+C`) |
| 2 | Click into your target text field |
| 3 | Press `Ctrl+Alt+Cmd+V` or use menubar |
| 4 | Watch realistic typing in action |

</div>

### Keyboard Shortcuts

```
Ctrl+Alt+Cmd+V  →  Type clipboard
ESC             →  Stop typing
```

### Menubar Controls

Click the **○** icon to access:
- **Type Clipboard** - Types your clipboard contents
- **Settings…** - Configure speed and typo rate
- **Reload Config** - Restart Hammerspoon

### Status Indicator

- **○** (hollow) = Idle, ready to type
- **●** (filled) = Currently typing

## Configuration

### Default Settings

```lua
Min WPM: 90
Max WPM: 130
Typo Rate: 0.05 (5%)
Space Pause: 0.08 (8%)
```

### Adjust Settings

1. Click menubar icon → **Settings…**
2. Adjust sliders:
   - **Min/Max WPM** - Typing speed range (10-260)
   - **Typo Rate** - Frequency of typos (0.00-0.30)
3. Click **Save** or **Reset Defaults**

## How It Works

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

## Examples

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

## Troubleshooting

### Nothing Happens When I Press the Shortcut

**Solution:**
1. Check Accessibility permissions:
   - System Settings → Privacy & Security → Accessibility
   - Ensure Hammerspoon is enabled
   - Try toggling it off and on
2. Check for Secure Input conflicts:
   - Password managers may interfere
   - Terminal with sudo access can block input
   - Temporarily disable these to test

### Settings Window Doesn't Open

**Solution:**
1. Open Hammerspoon Console (menubar → Console)
2. Click the Settings button
3. Look for error messages in the console
4. Report errors on GitHub if the issue persists

### Typing Speed is Incorrect

**Solution:**
- Adjust Min/Max WPM in Settings
- Lower values = slower typing speed
- Higher values = faster typing speed
- Recommended range: 90-130 WPM for realistic simulation

### Too Many or Too Few Typos

**Solution:**
- Adjust Typo Rate slider in Settings
- Set to 0.00 to disable typos completely
- Default value: 0.05 (5%) for realistic typing
- Range: 0.00-0.30 (0%-30%)

## License

<div align="center">

**CC BY-NC-SA 4.0**

Licensed under Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International

Use for personal/educational projects  
Modify and share with attribution  
No commercial use or selling  

[View Full License](LICENSE) • [Creative Commons](https://creativecommons.org/licenses/by-nc-sa/4.0/)

</div>

## Contributing

Contributions are welcome! Here's how you can help:

- **Report bugs** via [Issues](https://github.com/ethanstoner/humanlike-typer/issues)
- **Suggest features** in [Discussions](https://github.com/ethanstoner/humanlike-typer/discussions)
- **Improve documentation** - Help make the documentation clearer and more comprehensive
- **Submit pull requests** - Code improvements and new features

See [CONTRIBUTING.md](docs/CONTRIBUTING.md) for detailed guidelines and contribution standards.

## Credits

<div align="center">

Built with [Hammerspoon](https://www.hammerspoon.org/)

QWERTY adjacency mapping based on standard keyboard layout

---

**If you find this useful, consider giving it a star!**

</div>

---

<div align="center">

Made by [Ethan Stoner](https://github.com/ethanstoner)

</div>
