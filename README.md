# HumanLike Typer

A typing automation tool for macOS that simulates realistic human typing patterns. It includes QWERTY-based typos, variable speed, natural rhythm, and automatic smart punctuation conversion.

![License](https://img.shields.io/badge/license-CC%20BY--NC--SA%204.0-blue.svg)
![macOS](https://img.shields.io/badge/macOS-12.0%2B-blue.svg)

## Features

- **Realistic typing patterns**: Variable speed (90-130 WPM by default) with natural pauses and rhythm variations
- **QWERTY-based typos**: 60% adjacent-key mistakes and 40% letter transpositions, both auto-corrected
- **Smart punctuation**: Automatically converts smart quotes, em-dashes, and ellipses to ASCII equivalents
- **Easy configuration**: GUI for adjusting typing speed and typo frequency
- **Simple controls**: Menubar integration, keyboard shortcut, and ESC to cancel

## Requirements

- macOS 12.0 or later
- [Hammerspoon](https://www.hammerspoon.org/)

## Installation

### Install Hammerspoon

Download from [hammerspoon.org](https://www.hammerspoon.org/) and grant it Accessibility permissions:
- System Settings → Privacy & Security → Accessibility → Enable Hammerspoon

### Install HumanLike Typer

**Quick install:**

```bash
curl -o ~/.hammerspoon/init.lua https://raw.githubusercontent.com/ethanstoner/humanlike-typer/main/init.lua
```

Then reload Hammerspoon from the menubar icon.

**Manual install:**

1. Download `init.lua` from this repository
2. Place it in `~/.hammerspoon/init.lua`
3. Reload Hammerspoon

You should see a small ○ icon appear in your menubar.

## Usage

### Basic usage

1. Copy text to clipboard (Cmd+C)
2. Click into your target text field
3. Press `Ctrl+Alt+Cmd+V` or click the menubar icon → "Type Clipboard"

The text will be typed with realistic human-like patterns.

### Keyboard shortcuts

- `Ctrl+Alt+Cmd+V` - Type clipboard
- `ESC` - Stop typing

### Settings

Click the menubar icon → Settings to adjust:
- **Min/Max WPM**: Typing speed range (10-260)
- **Typo Rate**: Frequency of typos (0.00-0.30)

Changes apply immediately. Click "Reset Defaults" to restore original settings.

### Status indicator

- ○ = Idle
- ● = Typing in progress

## How it works

### Typing simulation

The script sends individual keypress events to mimic real typing:

1. Variable speed within your configured WPM range
2. Small random delays between characters
3. Longer pauses after spaces and punctuation

### Typo simulation

When enabled (5% by default), the script occasionally makes realistic mistakes:

**Adjacent-key errors (60%)**: Types a nearby key on the QWERTY layout, pauses briefly, backspaces, and types the correct character.

Example: `hello` becomes `hrllo` → [backspace] → `hello`

**Transpositions (40%)**: Types the next character first, backspaces, and reorders correctly.

Example: `the` becomes `teh` → [backspace] → `the`

### Punctuation handling

Smart punctuation from rich text editors is automatically converted:

| Input | Output |
|-------|--------|
| ' | ' |
| " " | " |
| – | - |
| — | -- |
| … | ... |

Non-ASCII characters are stripped to prevent typing errors.

## Configuration

Default settings can be edited in `~/.hammerspoon/init.lua`:

```lua
local DEFAULT_MIN_WPM, DEFAULT_MAX_WPM = 90, 130
local DEFAULT_TYPO_RATE = 0.05
local DEFAULT_SPACE_PAUSE = 0.08
```

Or use the Settings GUI to change them at runtime.

## Troubleshooting

**Nothing happens when I press the shortcut**

Check that Hammerspoon has Accessibility permissions:
- System Settings → Privacy & Security → Accessibility
- Make sure Hammerspoon is enabled
- Try toggling it off and on

Also check for Secure Input mode, which blocks keystroke simulation. This is enabled by some password managers and terminal sessions. Quit those apps and try again.

**Settings window doesn't open**

Open the Hammerspoon Console (menubar icon → Console) and look for error messages when clicking Settings.

**Typing speed is wrong**

Adjust the Min/Max WPM sliders in Settings. Lower values for slower typing, higher for faster.

**Too many typos (or not enough)**

Adjust the Typo Rate slider. 0.00 disables typos completely, 0.30 adds them to 30% of characters.

**Special characters don't work**

The script only types ASCII characters. Non-ASCII characters are automatically converted or removed.

## Examples

### Message with smart punctuation

Input:
```
Hey! How's it going? I'll send you the report—it's almost done...
```

Output:
```
Hey! How's it going? I'll send you the report--it's almost done...
```

Note the conversion of smart apostrophes, em-dash, and ellipsis to ASCII equivalents.

### Code

Input:
```python
def hello_world():
    print("Hello, world!")
```

Types correctly with realistic timing and occasional typos that get corrected.

## License

HumanLike Typer is licensed under CC BY-NC-SA 4.0 (Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International).

**You can:**
- Use it for personal, educational, and non-commercial purposes
- Modify and create derivative works
- Share it with proper attribution

**You cannot:**
- Sell it or use it commercially
- Use it in paid products or services

**You must:**
- Give credit to the original author
- Share derivative works under the same license

See [LICENSE](LICENSE) or https://creativecommons.org/licenses/by-nc-sa/4.0/ for details.

## Contributing

Issues and pull requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

- Report bugs: https://github.com/ethanstoner/humanlike-typer/issues
- Discussions: https://github.com/ethanstoner/humanlike-typer/discussions

## Credits

Built with [Hammerspoon](https://www.hammerspoon.org/). QWERTY adjacency mapping based on standard keyboard layout.
