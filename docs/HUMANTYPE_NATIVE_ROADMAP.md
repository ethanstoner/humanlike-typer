# HumanType Native Roadmap

This document defines the transition from script-based automation to native packaged apps.

## Brand

- Product name: `HumanType`
- Primary mark: use the keyboard logo supplied by the user in chat
- Platforms:
  - macOS: native `.app`
  - Windows: native `.exe`

## Product Direction

The goal is a consistent user-facing application on macOS and Windows while keeping the final typing output identical to the source text.

That means:

- temporary mistakes are allowed
- delayed correction is allowed
- variable pacing is allowed
- final output rewriting is not allowed

## Platform Packaging

### macOS

Target shape:

- menu bar app
- drag into `Applications`
- Accessibility permission prompt
- settings window
- global hotkey
- update checker

Recommended stack:

- SwiftUI app shell
- CGEvent-based typing engine
- Sparkle for auto-updates

### Windows

Target shape:

- tray app or lightweight desktop app
- packaged `.exe`
- installer for end users
- settings window
- global hotkey
- update checker

Recommended stack:

- keep AutoHotkey as the behavior prototype
- move toward a packaged native-feeling release flow
- use a signed installer and release updater

## Update Strategy

### Short Term

- script-level updater for `.ahk`
- script-level updater for `init.lua`
- GitHub Releases as the single source of truth

### Long Term

- macOS: Sparkle app updates
- Windows: installer-aware update flow for the packaged executable

## Release Structure

Each GitHub Release should eventually contain:

- `HumanType-macOS.zip` or `.dmg`
- `HumanType-Installer.exe`
- release notes
- version tag shared across both platforms

## Asset Work Still Needed

The provided keyboard icon has now been exported into:

- `assets/branding/humantype-logo-source.png`
- `macos/HumanType/Resources/Assets.xcassets/AppIcon.appiconset`
- `windows/assets/HumanType.ico`

Still needed:

- menu bar / tray variants optimized for small monochrome display
- installer graphics
- signed production packaging assets

## Current Repo State

- existing script engines still live in `scripts/`
- native macOS scaffold now lives in `macos/HumanType/`
- Windows installer scaffold now lives in `windows/`
