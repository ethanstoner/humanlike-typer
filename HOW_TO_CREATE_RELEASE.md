# How to Create GitHub Release v1.1.1

Follow these steps to create the GitHub release:

## Step 1: Push All Changes

Make sure all your changes are committed and pushed to GitHub:

```bash
git add .
git commit -m "Release v1.1.1 - Add Windows support"
git push origin main
```

## Step 2: Create Release on GitHub

1. Go to your repository: https://github.com/ethanstoner/humanlike-typer
2. Click on **"Releases"** (right sidebar)
3. Click **"Create a new release"**

## Step 3: Fill in Release Details

### Tag Version
- **Tag**: `v1.1.1`
- **Target**: `main`

### Release Title
```
v1.1.1 - Windows Support
```

### Release Description
Copy and paste the contents from **`RELEASE_NOTES_v1.1.1.md`**

## Step 4: Upload Release Assets

Click **"Attach binaries by dropping them here"** and upload:

1. **`init.ahk`** - Windows AutoHotkey script
2. **`init.lua`** - macOS Hammerspoon script

### Optional: Compile Windows .exe

If you want to include a pre-compiled Windows executable:

1. Install AutoHotkey v2 on a Windows machine
2. Right-click `init.ahk`
3. Select **"Compile Script"**
4. Upload the generated `init.exe` to the release

## Step 5: Publish Release

1. Check **"Set as the latest release"**
2. Click **"Publish release"**

## Step 6: Verify

After publishing, verify:
- Release appears in the Releases page
- Download links work: https://github.com/ethanstoner/humanlike-typer/releases/latest
- README links redirect properly

---

## GitHub CLI Alternative (Optional)

If you have GitHub CLI installed:

```bash
# Create release
gh release create v1.1.1 \
  --title "v1.1.1 - Windows Support" \
  --notes-file RELEASE_NOTES_v1.1.1.md \
  init.ahk \
  init.lua

# If you have compiled exe:
gh release upload v1.1.1 init.exe
```

---

## After Release

1. Delete `RELEASE_NOTES_v1.1.1.md` and `HOW_TO_CREATE_RELEASE.md` (they're just helpers)
2. Keep `CHANGELOG.md` for future reference
3. Update social media / announce the release!

✅ Done!

