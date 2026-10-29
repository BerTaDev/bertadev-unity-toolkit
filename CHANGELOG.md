# Changelog

## [1.2.0] - 2026-10-30

### Added
- Prefab Replacer: swap the selection for a prefab, keeping position, parent and Hierarchy order.
- Find References: assets and open-scene objects (including prefab instances and script users) that use an asset.
- Align & Distribute: min/center/max on X, Y, Z by bounds or pivots, relative to the selection or the active object.
- Favorites: per-project, per-user list of assets with drag in, drag out and double-click to open.
- UI Scroll UV (runtime): endless RawImage scrolling that tiles at the texture's size.

### Changed
- The runtime assembly now references `UnityEngine.UI`; the package depends on `com.unity.ugui`.

## [1.1.0] - 2026-10-30

### Added
- Runtime assembly `BerTaDEV.Toolkit` with three helpers:
  - `[ReadOnlyInspector]` attribute: greyed-out Inspector fields; structs stay expandable.
  - `UIFollowToWorld`: keeps a UI element over a world target, optional camera, hides behind the camera.
  - `BetaGUI`: build info overlay (name, company, version, FPS, build number), optional Development Builds only.
- README illustrations.

## [1.0.0] - 2026-10-29

### Added
- Hierarchy Decorator: header rows, active toggles, component icons, zebra rows.
- Scene Switcher window and Play From First Build Scene toggle.
- Missing Script Finder for scenes, Prefab Mode and prefab assets.
- Batch Renamer with find/replace, regex, prefix/suffix, numbering and case options.
- Drop To Ground (End) and Drop To Ground And Align (Shift+End).
- Selection History (Alt+Left / Alt+Right).
- Game view screenshots (F12) with 1x-4x resolution.
- Scene Auto Save (off by default).
- Preferences page under Edit > Preferences > BerTaDEV Toolkit.
