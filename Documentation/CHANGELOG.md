# Changelog

All notable changes to Clever Clogs will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [1.0.0] - Unreleased

Initial release.

### Added

**Helper class generation**
- Generates a C# helper class from a UXML file, containing an `Element` enum of the element names, a typed property per element, and `NameOf`, `Q<T>` and `Get` accessors
- Converts UXML names to valid identifiers, resolves naming clashes and duplicates, and reports each as a warning
- Resolves element types to ones generated code can refer to, and wraps the file in `#if UNITY_EDITOR` when an element type is editor-only
- Refuses to overwrite a `.cs` file it did not generate
- Public API in `Generate`: `BuildSource()`, `WriteFile()` and `ToIdentifier()`

**Auto-regeneration**
- Generated classes are recorded in the settings asset and rewritten when their UXML changes, and after each script reload
- A class is only rewritten if its content would differ, so unrelated UXML edits cause no recompile
- Remembers the class name, namespace and unticked elements of each generated class
- Public API in `Regenerator`: `GenerateAndRecord()`, `Regenerate()`, `RegenerateAll()`, `Find()` and `OutputMoved()`

**Editor window** (`DADCTools > Clever Clogs`)
- Scans the project for UXML files via `PathScan`, limited to the configured search folders
- Lists the named elements of the selected file via `UXMLScan`, with a tick box, type and name for each
- Generates a helper class for the ticked elements
- Selecting a file that already has a helper class restores its class name and tick boxes
- Main panel and settings panel, switched from the toolbar menu

**Settings** (`ProjectSettingSO`, created at `Assets/CleverClogsSettings.asset`)
- Search folders
- Output folder and namespace for generated classes
- "Regenerate on UXML save" toggle

**Packaging**
- Unity package manifest (`com.dannyarnold.clever-clogs`), installable from a git URL or OpenUPM
- Works when installed as a package or copied under `Assets`
- Assembly definition `DADC.Clogs.Editor`
- MIT licence

**Tests**
- EditMode test suite for `Generate`, `Regenerator`, `UXMLScan`, `PathScan` and the tool's own panels, in `Tests/Editor/` with its own assembly definition
- Passing on Unity 2022.3 and 6000.5

### Known Issues
- The editor window has only been used on Unity 6000.5; Unity 2022.1, 2022.2 and 2023 are untested
- The editor window has no automated tests beyond checking that its panels contain the expected elements
- Custom element types defined in an editor-only assembly definition are not detected as editor-only, so the generated file is not wrapped in `#if UNITY_EDITOR`
- Large project scans may freeze the editor (synchronous operations)
- No error handling in `PathScan.cs` for AssetDatabase failures
- The About menu item does nothing yet
- `UICollectionSO` and `WindowSO` are placeholder implementations
- `UXMLElementTreeView.cs` is unused (empty class)
- Several structs (`UXMLAsset`, `SearchFolder`) lack namespace declarations

### Planned
- About panel content
- Progress indicators for long scans
- Async scanning support for better performance
- Batch generation for several UXML files at once
- Complete `UICollectionSO` and `WindowSO` implementations
- Move `UXMLAsset` and `SearchFolder` into the `DADC.Clogs` namespace, which may require adding `using` statements in code that refers to them

---

## Version History

| Version | Date | Status |
|---------|------|--------|
| 1.0.0 | Unreleased | In Development |

---

**Maintained by**: Danny Arnold  
**Last Updated**: October 11, 2026
