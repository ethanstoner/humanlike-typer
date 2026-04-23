# HumanType v1.1.3

This release makes the Windows update path automatic and tightens installer presentation.

## Improved

- When HumanType starts and a newer GitHub Release has `HumanType-Installer.exe`, it downloads the installer, runs it silently, exits the old app, and relaunches after installation.
- The manual update dialog now uses `Install` when a direct Windows installer asset is available.
- The Windows installer now has explicit HumanType metadata in file details.
- The installer executable uses the same associated HumanType icon as the app executable.
- Local scratch directories are ignored more consistently in git.

## Release Assets

For Windows users, download and run:

- `HumanType-Installer.exe`

Exit any running HumanType instance from the tray before installing the update.
