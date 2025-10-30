# HumanLike Typer

A realistic human typing emulator for macOS that perfectly mimics natural typing patterns, complete with QWERTY-based typos, variable speed, rhythm variations, and smart punctuation handling.

![Status](https://img.shields.io/badge/status-active-success.svg)
![License](https://img.shields.io/badge/license-CC%20BY--NC--SA%204.0-blue.svg)
![macOS](https://img.shields.io/badge/macOS-12.0%2B-blue.svg)
![Hammerspoon](https://img.shields.io/badge/Hammerspoon-0.9.100%2B-orange.svg)

## ✨ Features

- **🎯 Realistic Human Typing**: Variable speed (90-130 WPM default), natural pauses, and rhythm variations
- **⌨️ QWERTY-Based Typos**: 
  - 60% adjacent-key mistakes (e.g., 'r' → 'f', then corrected)
  - 40% letter transpositions (e.g., 'teh' → 'the')
- **📝 Smart Punctuation**: Automatically converts smart quotes, em-dashes, and ellipses to ASCII
- **🎛️ Configurable Settings**: Adjust WPM range and typo rate via GUI
- **⏸️ Easy Control**: 
  - Menubar icon (○ idle, ● typing)
  - ESC to cancel instantly
  - Keyboard shortcut for quick typing
- **🎨 Clean Interface**: Centered settings window with dark theme

## 📋 Requirements

- macOS 12.0 or later
- [Hammerspoon](https://www.hammerspoon.org/) installed

## 🚀 Installation

### Step 1: Install Hammerspoon

1. Download Hammerspoon from [hammerspoon.org](https://www.hammerspoon.org/)
2. Drag Hammerspoon.app to your Applications folder
3. Launch Hammerspoon
4. Grant Accessibility permissions when prompted:
   - System Settings → Privacy & Security → Accessibility → Enable Hammerspoon

### Step 2: Install HumanLike Typer

**Option A: Direct Installation**

```bash
# Download the init.lua file
curl -o ~/.hammerspoon/init.lua https://raw.githubusercontent.com/ethanstoner/auto-typer/main/init.lua

# Reload Hammerspoon
# Click the Hammerspoon menubar icon → Reload Config
```

**Option B: Manual Installation**

1. Download `init.lua` from this repository
2. Place it in your Hammerspoon config directory:
   - Open Finder
   - Press `Cmd+Shift+G`
   - Type: `~/.hammerspoon/`
   - Copy `init.lua` to this folder (replace existing if any)
3. Reload Hammerspoon (click menubar icon → Reload Config)

### Step 3: Verify Installation

1. Look for a **○** icon in your macOS menubar
2. Click it to see the menu with "Type Clipboard", "Settings…", "Reload Config"
3. Success! ✅

## 🎮 Usage

### Quick Start

1. **Copy text** you want to type (Cmd+C)
2. **Click into a text field** where you want the typing to appear
3. **Press `Ctrl+Alt+Cmd+V`** or click ○ icon → "Type Clipboard"
4. **Watch it type** with realistic human-like patterns!

### Menubar Controls

Click the **○** icon to access:

- **Type Clipboard** - Types your clipboard contents immediately
- **Settings…** - Opens configuration panel
- **Reload Config** - Reloads Hammerspoon configuration

### Keyboard Shortcuts

- **`Ctrl+Alt+Cmd+V`** - Type clipboard immediately
- **`ESC`** - Cancel typing in progress

### Adjusting Settings

1. Click ○ icon → **Settings…**
2. Configure:
   - **Min WPM / Max WPM** - Typing speed range (10-260)
   - **Typo Rate** - How often typos occur (0.00-0.30)
3. Click **Save** to apply or **Reset Defaults** to restore

### Status Indicator

- **○** (hollow) - Idle, ready to type
- **●** (filled) - Currently typing

## 🎯 How It Works

### Realistic Typing Simulation

The emulator uses several techniques to mimic human typing:

1. **Variable Speed**: Random WPM within your configured range for each character
2. **Natural Pauses**: Slight delays after spaces and punctuation
3. **Rhythm Variation**: No two keystrokes are exactly the same speed

### QWERTY-Based Typos

When typos occur (controlled by Typo Rate setting):

- **Adjacent Key Mistakes** (60%): Types a nearby key on QWERTY keyboard
  - Example: `hello` → `hrllo` → backspace → `hello`
  
- **Transpositions** (40%): Types next letter first
  - Example: `the` → `teh` → backspace → `the`

### Smart Punctuation Handling

Automatically converts common smart punctuation to ASCII:

| Smart Character | Converts To |
|----------------|-------------|
| `'` (smart quote) | `'` (apostrophe) |
| `"` `"` (smart quotes) | `"` (straight quotes) |
| `–` (en-dash) | `-` (hyphen) |
| `—` (em-dash) | `--` (double hyphen) |
| `…` (ellipsis) | `...` (three dots) |
| Non-breaking space | Regular space |

## ⚙️ Configuration

### Default Settings

```lua
Min WPM: 90
Max WPM: 130
Typo Rate: 0.05 (5%)
Space Pause Chance: 0.08 (8%)
```

### Customization

Edit `~/.hammerspoon/init.lua` to change defaults:

```lua
local DEFAULT_MIN_WPM, DEFAULT_MAX_WPM = 90, 130
local DEFAULT_TYPO_RATE = 0.05
local DEFAULT_SPACE_PAUSE = 0.08
```

Or use the Settings GUI for runtime changes.

## 🐛 Troubleshooting

### Nothing Types When I Press the Shortcut

**Check Accessibility Permissions:**
1. System Settings → Privacy & Security → Accessibility
2. Ensure Hammerspoon is enabled
3. If listed but not working, remove and re-add it

**Check Secure Input:**
- Some apps (1Password, Terminal with sudo) block keyboard simulation
- Close password managers and try again

### Settings Window Doesn't Appear

1. Open Hammerspoon Console (menubar icon → Console)
2. Click ○ → Settings…
3. Look for error messages in red
4. Report any errors as an issue

### Typing Is Too Fast/Slow

1. Click ○ → Settings…
2. Adjust Min WPM and Max WPM sliders
3. Click Save
4. Test again

### Too Many/Few Typos

1. Click ○ → Settings…
2. Adjust Typo Rate (0.00 = no typos, 0.30 = 30% of letters get typos)
3. Click Save

### Special Characters Don't Type

The script only types ASCII characters (a-z, 0-9, common punctuation). Non-ASCII characters are automatically stripped or converted.

## 📝 Examples

### Example 1: Typing a Message

**Input (copied to clipboard):**
```
Hey! How's it going? I'll send you the report—it's almost done...
```

**What types:**
```
Hey! How's it going? I'll send you the report--it's almost done...
```
*(Note: Smart quotes and em-dash converted to ASCII)*

**How it types:**
- Variable speed for each letter
- Occasional typo like `goign` → backspace → `going`
- Natural pauses after punctuation

### Example 2: Code Typing

**Input:**
```python
def hello_world():
    print("Hello, world!")
```

**Result:** Types with realistic timing, handles quotes and syntax correctly

## 🤝 Contributing

Contributions are welcome! Here are some ways you can help:

- Report bugs or issues
- Suggest new features
- Improve documentation
- Submit pull requests

## 📄 License

**HumanLike Typer** is licensed under Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International (CC BY-NC-SA 4.0)

- ✅ **Free to use** for personal, educational, and non-commercial purposes
- ✅ **Free to modify** and create derivative works
- ✅ **Free to share** with proper attribution
- ❌ **Cannot be sold** or used for commercial purposes
- 🔄 **Share-alike** - Derivatives must use the same license

This ensures the software remains free and open for everyone while preventing commercial exploitation.

For the full license text, see [LICENSE](LICENSE) or visit [Creative Commons](https://creativecommons.org/licenses/by-nc-sa/4.0/).

## 🙏 Acknowledgments

- Built with [Hammerspoon](https://www.hammerspoon.org/)
- QWERTY adjacency mapping for realistic typos
- Inspired by human typing research

## 📧 Support

- **Issues**: [GitHub Issues](https://github.com/ethanstoner/auto-typer/issues)
- **Discussions**: [GitHub Discussions](https://github.com/ethanstoner/auto-typer/discussions)

## 🔄 Changelog

### v1.0.0 (2025)
- Initial release
- Realistic human typing simulation
- QWERTY-based typos (adjacent keys + transpositions)
- Smart punctuation handling
- Configurable settings GUI
- Menubar integration
- ESC to cancel

---

**Made with ❤️ for automation with a human touch**

