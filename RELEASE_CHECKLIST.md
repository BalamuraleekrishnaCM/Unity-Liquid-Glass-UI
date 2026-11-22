# Unity Liquid Glass UI — Release acceptance checklist

Target: v2.2.0. Candidate package version: 2.2.0-rc.1.
Base: Unity 2022.3 LTS, Unity 6 LTS

## Checks completed

- [x] Validate package metadata, local assembly references, asset GUID uniqueness, and bundled material shader GUIDs in source and packed archive.
- [x] Review the MIT license and update the changelog.
- [x] Document installation, input setup, and troubleshooting.
- [x] Add desktop and world-space scene generators with optional XR Interaction Toolkit wiring.

Run the static checks with `python3 Tools/validate_package.py`. GitHub Actions runs them on both source and the generated UPM tarball. The remaining gates require Unity Editor, player, hardware, or publishing evidence.

## Package integrity
- [ ] package.json dependencies resolve from a clean project
- [ ] Import via Git URL and via local package path
- [ ] No compiler errors in Editor, Runtime or optional URP assemblies
- [ ] No missing .meta files, broken GUID references, or shader import errors
- [ ] No unintended third-party assets or copyrighted material

## UI coverage
- [ ] Panel, Image, Raw Image
- [ ] Button, TMP Text, Input Field, Dropdown
- [ ] Toggle, Slider, Scroll View, Scrollbar
- [ ] Legacy Button, Text, Input Field, Dropdown
- [ ] Layout groups, anchors, masks, nested canvases and scaling
- [ ] Keyboard, mouse, touch and gamepad navigation

## Rendering
- [ ] Built-in RP capture and blur
- [ ] URP 14 compatibility path (Unity 2022.3)
- [ ] URP 17 RenderGraph path (Unity 6)
- [ ] Transparency, tint, rim, refraction, corner clipping and text legibility
- [ ] Camera stacking, resize, dynamic resolution and no-background fallback
- [ ] Profiling and quality presets for mobile and desktop

## XR
- [ ] World-space sample works with the configured XR Origin and XR Interaction Toolkit input
- [ ] Quest 3 single-pass instanced stereo correctness
- [ ] Controller rays and hand/poke interaction
- [ ] World-space readable text, stable blur and comfortable UI depth
- [ ] Performance measurements on target headset

## Documentation and release
- [ ] Screenshot captures from real running Unity scene (concept art clearly marked)
- [ ] GitHub Actions package validation passes for the candidate commit
- [ ] Clean Unity sample project opens and plays
- [ ] Create GitHub tag v2.2.0 and attach UPM package archive
- [ ] Publish release notes with tested Unity/URP/XR versions

## Installation
Package Manager > + > Add package from git URL:
`https://github.com/BalamuraleekrishnaCM/Unity-Liquid-Glass-UI.git`

After tagging, pin an exact version:
`https://github.com/BalamuraleekrishnaCM/Unity-Liquid-Glass-UI.git#v2.2.0`

Do not mark any check complete without actual evidence (Editor logs, screenshots, CI output or device tests).
