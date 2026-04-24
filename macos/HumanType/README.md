# HumanType macOS App

This folder contains the native macOS app scaffold for `HumanType`.

## Goal

Ship a real `HumanType.app` menu bar application that:

- installs like a normal Mac app
- uses the HumanType name and logo
- requests Accessibility permissions
- types clipboard content with humanized timing
- exposes settings, hotkeys, and update checks

## Current Status

This is a source scaffold, not a finished build artifact.

Included here:

- SwiftUI app entry point
- menu bar shell
- settings window
- typing engine interface
- update checker skeleton
- XcodeGen project spec

Not included yet:

- real CGEvent typing engine implementation
- Sparkle updater integration
- code signing / notarization

## Branding

Product name: `HumanType`

Logo source: the keyboard icon provided by the user.

Generated asset paths:

- master source: `assets/branding/humantype-logo-source.png`
- macOS app icon set: `macos/HumanType/Resources/Assets.xcassets/AppIcon.appiconset`
- Windows icon: `windows/assets/HumanType.ico`

## Suggested Build Flow

1. Install Xcode.
2. Install XcodeGen.
3. Run `xcodegen generate` in this folder.
4. Open the generated `HumanType.xcodeproj`.
5. Add the final icon files to `Assets.xcassets`.
6. Implement the real typing engine and updater.

## Relationship To Existing Scripts

The current scripts remain the behavioral reference:

- `scripts/init.lua`
- `scripts/init.ahk`

The native app should match their settings, pacing model, and typo behavior as closely as possible.
