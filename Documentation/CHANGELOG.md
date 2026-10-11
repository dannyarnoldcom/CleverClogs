# Changelog

All notable changes to Clever Clogs will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added
- Helper class generation in `Generate.cs`: `BuildSource()`, `WriteFile()` and `ToIdentifier()`
- Generated classes contain an `Element` enum of the UXML names, a typed property per element, and `NameOf`, `Q<T>` and `Get` accessors
- Element list in the main panel showing each named element of the selected file with a tick box, type and name
- Generate button wired to the generator
- `outputFolder` and `generatedNamespace` settings on `ProjectSettingSO`, with fields in the settings panel
- `elementType` field on `Element`
- Assembly definition `DADC.Clogs.Editor` for the tool
- Unity package manifest (`com.dannyarnold.clever-clogs`), so the tool can be installed from a git URL or OpenUPM
- `ProjectSettingSO.FindAll()` and `LoadOrCreate()`
- MIT licence
- EditMode test suite for `Generate`, `Regenerator` and `UXMLScan` in `Tests/Editor/`, with its own assembly definition
- Auto-regeneration: generated classes are recorded in the settings asset and rewritten when their UXML changes (`Regenerator`, `UxmlPostprocessor`, `GeneratedHelper`)
- `autoRegenerate` and `generated` on `ProjectSettingSO`, with a "Regenerate on UXML save" toggle in the settings panel
- Selecting a UXML file that already has a helper class restores its class name and tick boxes

### Changed
- `Generate` is now a static class in `DADC.Clogs` instead of an empty `MonoBehaviour`
- `Element` constructor takes an optional third `System.Type` argument
- The settings asset is now created at `Assets/CleverClogsSettings.asset` instead of inside the tool's folder, which is read-only when installed as a package
- `PathScan.GetInstallPath()` takes no argument and also finds the tool under `Packages`
- Scanning now uses the search folders from the project settings; previously it always searched all of `Assets`
- The dropdown value change callback is registered once per panel instead of on every scan
- Minimum Unity version documented as 2022.1 (required by `MultiColumnTreeView`, which the main panel already used)

### Fixed
- The `hidden` USS class was set to `display: flex`, so the dropdown and results section were never hidden
- Selecting a file used only its file name; files with the same name in different folders are now told apart by path
- `UXMLScan.ListElements()` threw if the asset could not be loaded; it now returns an empty list
- `UXMLScan` returned a null type name for element types without a namespace
- The Settings panel threw "SearchFolders not found" and never opened, because it looked for a `Foldout` that did not exist. Search folders are now an editable list
- The tool's own UXML referred to its stylesheets and templates by absolute `Assets/...` paths, which logged warnings when installed as a package. They are now relative

### Removed
- Commented-out element analysis loop in `CleverClogsWindow.ScanButton_clicked()`

### To Be Implemented
- About panel content
- Progress indicators for long scans
- Async scanning support for better performance
- Batch generation for several UXML files at once
- Complete `UICollectionSO` and `WindowSO` implementations

### Known Issues
- The automated tests pass on Unity 2022.3 and 6000.5, but the editor window has only been used on 6000.5; Unity 2022.1, 2022.2 and 2023 are untested
- `UXMLElementTreeView.cs` is unused (empty class)
- `UICollectionSO` and `WindowSO` are placeholder implementations
- Several structs (`UXMLAsset`, `SearchFolder`) lack namespace declarations
- The editor window has no automated tests
- Custom element types defined in an editor-only assembly definition are not detected as editor-only, so the generated file is not wrapped in `#if UNITY_EDITOR`
- Large project scans may freeze the editor (synchronous operations)
- No error handling in `PathScan.cs` for AssetDatabase failures

## [1.0.0] - December 2025

### Added
- Initial release
- UXML file scanning functionality via `PathScan.cs`
- Configurable search paths via `ProjectSettingSO`
- Project settings persistence using ScriptableObject
- Element extraction from UXML files via `UXMLScan.cs`
- Element sorting by type via `SortElements.cs`
- Helper name generation (filename.uxml → filenameUXML)
- Main panel and settings panel UI
- Base architecture with separation of concerns
- Three-panel navigation system (Main, Settings, About)
- Dynamic UXML/USS loading system
- Menu item integration (`DADCTools > Clever Clogs`)

### Architecture
- EditorWindow pattern for Unity integration
- Static utility pattern for helper classes
- ScriptableObject-based configuration
- UI Toolkit for editor interface
- Modular panel system

### Files Structure
- `CleverClogsWindow.cs` - Main editor window (213 lines)
- `PathScan.cs` - Asset discovery utilities
- `UXMLScan.cs` - UXML parsing and element extraction
- `SortElements.cs` - Element categorization
- `ProjectSettingSO.cs` - Settings persistence
- `BasePanel.uxml`, `SettingsPanel.uxml`, `CleverClogs.uxml` - UI layouts
- `CleverClogs.uss` - Styling definitions

### Fixed
- None (initial release)

### Changed
- None (initial release)

### Deprecated
- None

### Removed
- None

### Security
- None

---

## Version History

| Version | Date | Status |
|---------|------|--------|
| 1.0.0 | December 2025 | Current |
| Unreleased | TBD | In Development |

---

## Migration Guide

### 1.0.0 to Unreleased

1. **`Generate` changed from MonoBehaviour to static class**
   - Old: `public class Generate : MonoBehaviour` in the global namespace (non-functional)
   - New: `public static class Generate` in `DADC.Clogs`
   - Nothing could usefully reference the old class, so no code changes are expected

2. **New public APIs**
   ```csharp
   Generate.BuildSource(string className, string uxmlPath, IList<Element> elements, string namespaceName, out List<string> warnings)
   Generate.WriteFile(string folder, string className, string source)
   Generate.ToIdentifier(string name)
   ```

3. **`Element` gained an `elementType` field**
   - The two-argument constructor still compiles; `elementType` is then `null` and the generated property is typed `VisualElement`

4. **The `hidden` USS class now hides elements**
   - Custom UXML that relied on `hidden` having no effect will need the class removed

### Future

1. **Structs will gain namespaces**
   - `UXMLAsset` will move to `DADC.Clogs` namespace
   - `SearchFolder` will move to `DADC.Clogs` namespace
   - This may require adding `using` statements in custom code

---

**Maintained by**: DADC  
**Documentation Version**: 1.0  
**Last Updated**: October 11, 2026
