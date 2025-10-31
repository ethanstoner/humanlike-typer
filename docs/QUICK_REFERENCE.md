# Quick Reference Guide

Handy one-page reference for HumanLike Typer.

## ⌨️ Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+Alt+Cmd+V` | Type clipboard immediately |
| `ESC` | Cancel typing in progress |

## 🖱️ Menubar Menu

Click the **○** icon to access:

| Menu Item | Action |
|-----------|--------|
| Type Clipboard | Types clipboard contents with human-like speed |
| Settings… | Opens configuration panel |
| Reload Config | Reloads Hammerspoon configuration |

## 📊 Status Icons

| Icon | Meaning |
|------|---------|
| ○ | Idle - ready to type |
| ● | Typing in progress |

## ⚙️ Settings Panel

| Setting | Range | Default | Description |
|---------|-------|---------|-------------|
| Min WPM | 10-260 | 90 | Minimum typing speed |
| Max WPM | 10-260 | 130 | Maximum typing speed |
| Typo Rate | 0.00-0.30 | 0.05 | Frequency of typos (5%) |

## 🎯 Typo Patterns

### Adjacent Key Mistakes (60%)
- Hits a nearby key on QWERTY keyboard
- **Example**: `hello` → `hrllo` → backspace → `hello`

### Transpositions (40%)
- Types next letter first, then corrects
- **Example**: `the` → `teh` → backspace → `the`

## 📝 Smart Punctuation Conversion

| Input | Output | Description |
|-------|--------|-------------|
| `'` | `'` | Smart apostrophe → straight |
| `"` `"` | `"` | Smart quotes → straight |
| `–` | `-` | En-dash → hyphen |
| `—` | `--` | Em-dash → double hyphen |
| `…` | `...` | Ellipsis → three dots |

## 🚀 Common Use Cases

### 1. Type a Long Document
```
1. Copy entire document (Cmd+C)
2. Click into target text field
3. Press Ctrl+Alt+Cmd+V
4. Wait for completion
```

### 2. Adjust Speed
```
1. Click ○ icon → Settings…
2. Change Min/Max WPM
3. Click Save
4. Test with new speed
```

### 3. Disable Typos
```
1. Click ○ icon → Settings…
2. Set Typo Rate to 0.00
3. Click Save
4. Now types perfectly
```

### 4. Emergency Stop
```
Press ESC key immediately
```

## 🐛 Quick Troubleshooting

| Problem | Solution |
|---------|----------|
| Nothing types | Check Accessibility permissions |
| Settings won't open | Check Hammerspoon Console for errors |
| Too fast/slow | Adjust WPM in Settings |
| Too many typos | Lower Typo Rate in Settings |
| Wrong text types | Make sure text field is focused |
| Special chars missing | Script only types ASCII |

## 📂 File Locations

| File | Path |
|------|------|
| Config | `~/.hammerspoon/init.lua` |
| Console | Menubar icon → Console |

## 🔄 Quick Commands

### Reload Configuration
```
Click ○ icon → Reload Config
```

### Open Config File
```bash
open ~/.hammerspoon/init.lua
```

### Open Console
```
Click Hammerspoon menubar icon → Console
```

### View Logs
```lua
-- In Console
hs.printf("Check logs here")
```

## 💡 Pro Tips

1. **Test Before Using**: Try with short text first
2. **Focus Matters**: Always click into text field before typing
3. **Speed Tuning**: Start slower (60-90 WPM) for more realistic effect
4. **Typo Rate**: 0.05 (5%) is realistic; 0.10 (10%) is noticeable
5. **Emergency Stop**: ESC key stops immediately - use if needed
6. **Smart Quotes**: Automatically converted - no need to worry
7. **Multiple Paragraphs**: Works with line breaks (keeps them)
8. **Code Typing**: Great for demonstrations and tutorials

## 🔗 Quick Links

- [Full README](README.md) - Complete documentation
- [Installation Guide](INSTALL.md) - Step-by-step setup
- [Contributing](CONTRIBUTING.md) - How to contribute
- [Issues](https://github.com/ethanstoner/auto-typer/issues) - Report bugs
- [Discussions](https://github.com/ethanstoner/auto-typer/discussions) - Ask questions

---

**Print this page for quick reference!** 📄

