# Contributing to HumanLike Typer

Thank you for your interest in contributing! This document provides guidelines for contributing to the project.

## How to Contribute

### Reporting Bugs

Before creating a bug report, please check existing issues to avoid duplicates.

**When submitting a bug report, include:**
- macOS version
- Hammerspoon version (`hs.processInfo.version` in Console)
- Steps to reproduce
- Expected vs. actual behavior
- Console logs (if applicable)
- Screenshots/GIFs (if relevant)

### Suggesting Features

Feature requests are welcome! Please:
- Check if the feature has already been requested
- Explain the use case clearly
- Describe how it should work
- Consider if it fits the project's scope (realistic human typing simulation)

### Pull Requests

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/my-new-feature`
3. **Make your changes**
4. **Test thoroughly**
5. **Commit with clear messages**: `git commit -m "Add feature: description"`
6. **Push to your fork**: `git push origin feature/my-new-feature`
7. **Open a Pull Request**

## Development Guidelines

### Code Style

- Use 2-space indentation (already configured in the codebase)
- Keep functions focused and single-purpose
- Add comments for complex logic
- Use descriptive variable names

### Testing

Before submitting a PR, test:
- Basic typing functionality (clipboard, hotkey)
- Settings window (open, edit, save, close)
- Typo simulation (adjacent keys and transpositions)
- Smart punctuation conversion
- ESC cancellation
- Menubar icon state changes

### Documentation

- Update README.md if adding features
- Update INSTALL.md if changing installation process
- Add inline comments for complex code
- Update CHANGELOG.md (if it exists)

## Code of Conduct

### Our Standards

- Be respectful and inclusive
- Welcome newcomers and help them learn
- Focus on constructive feedback
- Accept criticism gracefully

### Unacceptable Behavior

- Harassment or discriminatory comments
- Trolling or insulting comments
- Personal attacks
- Publishing others' private information

## Areas for Contribution

### Features We'd Love to See

- [ ] Multiple typing profiles (speed presets)
- [ ] Statistics tracking (WPM, accuracy)
- [ ] More realistic pause patterns
- [ ] Typing history/undo
- [ ] Alternative keyboard layouts (Dvorak, Colemak)
- [ ] iOS/iPadOS version via automation tools

### Improvements Needed

- [ ] Better error handling
- [ ] More comprehensive testing
- [ ] Performance optimization for long texts
- [ ] Internationalization support
- [ ] Dark/light mode for settings panel

### Documentation

- [ ] Video tutorials
- [ ] GIF demonstrations
- [ ] Use case examples
- [ ] FAQ section expansion
- [ ] Translation to other languages

## Getting Help

- **Questions**: Open a [Discussion](https://github.com/YOUR_USERNAME/auto-typer/discussions)
- **Bugs**: Create an [Issue](https://github.com/YOUR_USERNAME/auto-typer/issues)
- **Real-time**: Consider joining our community chat (if available)

## Development Setup

### Prerequisites

- macOS 12.0+
- Hammerspoon installed
- Basic Lua knowledge
- Text editor (VS Code, Sublime, vim, etc.)

### Local Development

1. Clone the repository:
```bash
git clone https://github.com/YOUR_USERNAME/auto-typer.git
cd auto-typer
```

2. Symlink to Hammerspoon config:
```bash
ln -sf $(pwd)/init.lua ~/.hammerspoon/init.lua
```

3. Open Hammerspoon Console for live debugging
4. Make changes to `init.lua`
5. Reload with Cmd+Alt+Ctrl+R

### Debugging

Add debug logs:
```lua
hs.printf("[DEBUG] Your debug message: %s", variable)
```

View in Hammerspoon Console (menubar → Console)

## Project Structure

```
auto-typer/
├── init.lua           # Main script
├── README.md          # Project documentation
├── INSTALL.md         # Installation guide
├── CONTRIBUTING.md    # This file
├── LICENSE            # MIT License
└── .gitignore         # Git ignore rules
```

## Release Process

1. Update version number in README
2. Update CHANGELOG.md
3. Test all features thoroughly
4. Create a release tag: `git tag v1.0.0`
5. Push tag: `git push origin v1.0.0`
6. Create GitHub release with notes

## Questions?

Don't hesitate to ask questions! Open a discussion or comment on an issue.

---

**Thank you for contributing!** 🙏

