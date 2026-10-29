<p align="center">
  <img src="Documentation~/images/banner.svg" alt="BerTaDEV Toolkit: nine everyday editor tools for Unity" width="100%">
</p>

# BerTaDEV Toolkit for Unity

Small editor tools that remove everyday friction in Unity. Editor-only (nothing ends up in your builds), no third-party dependencies.

<p align="center">
  <img src="Documentation~/images/hierarchy.svg" alt="Hierarchy before and after: header rows, active toggles, component icons and missing script warnings" width="100%">
</p>

| Tool | What it does | Where |
| --- | --- | --- |
| **Hierarchy Decorator** | Section headers (`--- Name ---`), an active checkbox and component icons on every row, optional zebra rows | Automatic. *GameObject > BerTaDEV > Hierarchy Header* |
| **Scene Switcher** | Every scene in the project in one searchable list: open, open additively, or play with one click. Right-click to add/remove from the build list | *Tools > BerTaDEV > Scene Switcher* |
| **Play From First Build Scene** | Play Mode always starts from build scene #0, and you come back to the scenes you were editing | *Tools > BerTaDEV > Play From First Build Scene* |
| **Missing Script Finder** | Finds "Missing (Mono Script)" components in open scenes, Prefab Mode and every prefab under `Assets/`, and removes them | *Tools > BerTaDEV > Missing Script Finder* |
| **Batch Renamer** | Rename many GameObjects or assets at once: find/replace (regex), prefix/suffix, numbering, case, live preview, undo | *Tools > BerTaDEV > Batch Renamer* |
| **Drop To Ground** | Drops the selection onto the collider below it. `Shift+End` also rotates it to match the surface | `End` / `Shift+End` |
| **Selection History** | Back and forward through what you selected, like a browser | `Alt+Left` / `Alt+Right` |
| **Screenshot** | Saves the Game view to `<Project>/Screenshots`, 1x to 4x resolution | `F12` |
| **Scene Auto Save** | Saves modified scenes every N minutes (off by default, per project) | *Edit > Preferences > BerTaDEV Toolkit* |

All options live in **Edit > Preferences > BerTaDEV Toolkit**. Every shortcut can be changed in **Edit > Shortcuts** (search for "BerTaDEV").

## A closer look

<p align="center">
  <img src="Documentation~/images/scene-switcher.svg" alt="Scene Switcher window with the right-click menu open" width="49%">
  <img src="Documentation~/images/missing-scripts.svg" alt="Missing Script Finder listing scene objects and prefabs" width="49%">
</p>

**Scene Switcher** (left): click a name to ping it, *Open* / *+* / *▶* to open, add or play. Right-click a row to add it to or remove it from the build. **Missing Script Finder** (right): scene objects and prefab assets in one list, removable one by one or all at once.

<p align="center">
  <img src="Documentation~/images/batch-renamer.svg" alt="Batch Renamer renaming five cubes to Enemy_01 through Enemy_05" width="520">
</p>

**Batch Renamer**: the preview updates as you type, and numbering follows the order you see in the Hierarchy (or the Project window for assets).

<p align="center">
  <img src="Documentation~/images/shortcuts.svg" alt="Shortcuts: End to drop to ground, Alt+arrows for selection history, F12 for screenshots" width="100%">
</p>

## Install

**Window > Package Manager > + > Add package from git URL...** and paste:

```
https://github.com/BerTaDev/bertadev-unity-toolkit.git
```

Or add this line to `Packages/manifest.json`:

```json
"com.bertadev.toolkit": "https://github.com/BerTaDev/bertadev-unity-toolkit.git"
```

Requires Unity 2022.3 or newer. Built and compiled against Unity 6.3.

## Notes

- **Hierarchy headers** are plain empty GameObjects whose name starts with `---`. They're tiny, but they do exist at runtime; keep gameplay objects out of them if you don't want them parented to an empty.
- **Drop To Ground** uses physics raycasts, so the ground needs a collider (planes, terrains and imported meshes with "Generate Colliders" have one).
- **Screenshot** captures on the Game view's next render. If the Game view is hidden behind another tab, it will be brought up.
- **Missing Script Finder** can undo scene changes. Prefab assets are saved directly, so it asks first.

## Türkçe

Unity'de her gün uğraştığın küçük işleri hızlandıran editör araçları. Sadece editörde çalışır, build'e hiçbir şey girmez.

- **Hiyerarşi**: `--- İsim ---` diye adlandırdığın objeler başlık olur; her satırda aktif/pasif kutusu ve component ikonları.
- **Scene Switcher**: Projedeki tüm sahneler tek listede; tek tıkla aç, ekle ya da oynat.
- **Play From First Build Scene**: Play'e her zaman ilk build sahnesinden gir.
- **Missing Script Finder**: Sahnelerde ve prefablarda kayıp script'leri bulur, siler.
- **Batch Renamer**: Toplu yeniden adlandırma (bul/değiştir, regex, önek/sonek, numaralandırma, önizleme, geri al).
- **Drop To Ground**: `End` ile seçili objeleri yere indir, `Shift+End` ile yüzeye hizala.
- **Selection History**: `Alt+Sol` / `Alt+Sağ` ile seçim geçmişinde gez.
- **Screenshot**: `F12` ile Game view ekran görüntüsü (1x-4x).
- **Scene Auto Save**: Sahneleri belirli aralıklarla otomatik kaydet (varsayılan kapalı).

Kurulum: Package Manager > + > *Add package from git URL* > `https://github.com/BerTaDev/bertadev-unity-toolkit.git`

## License

[MIT](LICENSE.md)
