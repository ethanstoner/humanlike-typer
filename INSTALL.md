# Quick Installation Guide

Get started with HumanLike Typer in 3 simple steps!

## Prerequisites

- **macOS 12.0+** (Monterey or later)
- **Hammerspoon** - Download from [hammerspoon.org](https://www.hammerspoon.org/)

## Installation

### 1. Install Hammerspoon

```bash
# Option 1: Download manually
# Visit https://www.hammerspoon.org/ and download the latest version

# Option 2: Install via Homebrew
brew install --cask hammerspoon
```

Launch Hammerspoon and grant **Accessibility permissions**:
- System Settings → Privacy & Security → Accessibility → Enable Hammerspoon

### 2. Install HumanLike Typer

**Quick Install (Recommended):**

```bash
# Backup existing config (if any)
[ -f ~/.hammerspoon/init.lua ] && mv ~/.hammerspoon/init.lua ~/.hammerspoon/init.lua.backup

# Download and install
curl -o ~/.hammerspoon/init.lua https://raw.githubusercontent.com/YOUR_USERNAME/auto-typer/main/init.lua
```

**Manual Install:**

1. Download [init.lua](https://raw.githubusercontent.com/YOUR_USERNAME/auto-typer/main/init.lua)
2. Move it to `~/.hammerspoon/init.lua`
3. Or open Hammerspoon and click "Open Config" → replace/create `init.lua`

### 3. Reload Hammerspoon

Click the Hammerspoon menubar icon → **Reload Config**

✅ You should see a **○** icon appear in your menubar!

## Quick Start

1. **Copy some text** (Cmd+C)
2. **Click into a text field**
3. **Press `Ctrl+Alt+Cmd+V`** or click ○ → "Type Clipboard"
4. **Watch it type!**

## Configuration

Click **○ icon → Settings…** to configure:
- Typing speed (WPM range)
- Typo frequency
- Reset to defaults

## Troubleshooting

### Nothing happens when I press the shortcut?

**Fix Accessibility permissions:**
1. System Settings → Privacy & Security → Accessibility
2. Find Hammerspoon in the list
3. Toggle it OFF then ON again
4. Restart Hammerspoon

### Settings button does nothing?

1. Open Hammerspoon Console (menubar icon → Console)
2. Click Settings button
3. Look for error messages
4. Report the issue on GitHub

### Typing is weird/wrong?

Make sure you're clicking into a **text field** before typing. The script types wherever your cursor is focused.

## Uninstall

```bash
# Remove the config
rm ~/.hammerspoon/init.lua

# Restore backup (if you made one)
[ -f ~/.hammerspoon/init.lua.backup ] && mv ~/.hammerspoon/init.lua.backup ~/.hammerspoon/init.lua

# Reload Hammerspoon
# Click menubar icon → Reload Config
```

## Next Steps

- Read the full [README.md](README.md) for features and usage
- Join discussions on GitHub
- Star the repo if you find it useful! ⭐

## Support

- **Issues**: [Report bugs](https://github.com/YOUR_USERNAME/auto-typer/issues)
- **Questions**: [Discussions](https://github.com/YOUR_USERNAME/auto-typer/discussions)

---

**Happy typing!** 🎯

