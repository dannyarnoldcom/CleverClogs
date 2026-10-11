# Contributing to Clever Clogs

Thank you for considering contributing to Clever Clogs! This document outlines the development priorities and guidelines.

---

## Priority Implementation Tasks

The following work is outstanding, ordered by priority:

### Critical Priority (P0)

None outstanding. The two former P0 items are done:

- **Generate.cs Implementation**: `Generate` is a static class that builds and writes helper classes. See `Documentation/Helper-Utilities.md`.
- **Element Analysis Display**: the main panel lists the named elements of the selected file and lets the user choose which to generate.

The generator's automated tests pass on Unity 2022.3 and 6000.5. The editor window has only been used on 6000.5 and still needs manual testing on Unity 2022 and 2023.

---

### High Priority (P1)

#### 3. Add Namespaces to Structs

**Status**: `UXMLAsset` and `SearchFolder` lack namespace declarations  
**Required**: Add `namespace DADC.Clogs` for consistency

**Files to Update**:
- `Assets/Imported/CleverClogs/Editor/Helpers/UXMLAsset.cs`
- `Assets/Imported/CleverClogs/Editor/Helpers/SearchFolder.cs`

**Changes**:
```csharp
// Add to both files
namespace DADC.Clogs
{
    public struct UXMLAsset { /* existing code */ }
}

namespace DADC.Clogs
{
    public struct SearchFolder { /* existing code */ }
}
```

**Impact**: May require adding `using DADC.Clogs;` to files that reference these structs

**Estimated Effort**: 30 minutes

---

#### 4. About Panel Implementation

**Status**: Placeholder method, UXML exists but unused  
**Required**: Display version info, credits, and links

**Files**:
- `ShowAboutPopup()` method in `CleverClogsWindow.cs`
- `AboutContainer.uxml` exists but not loaded

**Required Content**:
- Tool version and build date
- Unity version compatibility
- Author/organization credits
- Link to documentation
- Link to repository (if applicable)
- License information

**Estimated Effort**: 2-3 hours

---

### Medium Priority (P2)

#### 5. Async Scanning Support

**Status**: All operations are synchronous  
**Issue**: Large projects cause editor freeze

**Current Problem**:
```csharp
private void ScanButton_clicked()
{
    statusText.text = "Scanning...";
    List<string> assetPaths = PathScan.GetAssetPaths(); // Blocks UI
    // ...
}
```

**Required Implementation**:
- Implement async/await pattern for scanning
- Add progress bar with cancellation support
- Use `Task.Run()` for background processing
- Return to main thread for Unity API calls
- Add status updates during scan

**Estimated Effort**: 4-5 hours

---

#### 6. Error Handling in PathScan

**Status**: No try-catch blocks  
**Issue**: Crashes on AssetDatabase errors

**Files**: `PathScan.cs`

**Required Changes**:
```csharp
public static List<string> GetAssetPaths(string[] foldersToSearch = null)
{
    try
    {
        // existing code
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Error scanning for UXML assets: {ex.Message}");
        return new List<string>();
    }
}
```

**Estimated Effort**: 1 hour

---

#### 7. UXMLElementTreeView Implementation

**Status**: Empty class that inherits from CleverClogsWindow  
**File**: `UXMLElementTreeView.cs`

**Current Code**:
```csharp
public class UXMLElementTreeView : CleverClogsWindow
{
}
```

**Purpose**: Unclear - should either be implemented or removed

**Options**:
1. Implement as a tree view for element hierarchy
2. Remove if not needed
3. Document intended purpose for future implementation

**Estimated Effort**: TBD (depends on intended purpose)

---

### Low Priority (P3)

#### 8. WindowSO and UICollectionSO Implementation

**Status**: Minimal implementations with no properties  
**Files**: `WindowSO.cs`, `UICollectionSO.cs`

**Required**: Define purpose and implement features

**Suggested Properties for WindowSO**:
```csharp
public class WindowSO : ScriptableObject
{
    public string windowName;
    public Vector2 minSize;
    public VisualTreeAsset uxmlAsset;
    public StyleSheet styleSheet;
    public List<Element> elements;
}
```

**Estimated Effort**: 2-3 hours

---

## Code Style Guidelines

### General Principles

1. **Follow existing patterns** in the codebase
2. **Use consistent naming conventions** (PascalCase for public, camelCase for private)
3. **Add XML documentation comments** for all public APIs
4. **Include error handling** with meaningful error messages
5. **Write defensive code** with null checks

### Naming Conventions

```csharp
// Public methods - PascalCase
public static List<string> GetAssetPaths()

// Private fields - camelCase
private ProjectSettingSO projectSettings;

// Properties - PascalCase
public string ProjectPath { get; set; }

// Constants - PascalCase
private const string projectName = "CleverClogs";

// Namespaces
namespace DADC.Clogs
```

### Documentation Comments

All public APIs should have XML documentation:

```csharp
/// <summary>
/// Scans specified folders for VisualTreeAsset files.
/// </summary>
/// <param name="foldersToSearch">Array of folder paths to search. Defaults to ["Assets"] if null.</param>
/// <returns>List of asset paths to all discovered UXML files.</returns>
public static List<string> GetAssetPaths(string[] foldersToSearch = null)
```

### Error Handling

```csharp
// Throw exceptions for developer errors (wrong usage)
if (root == null) 
    throw new ArgumentNullException(nameof(root));

// Log errors for runtime issues (missing assets)
if (visualTree == null)
{
    Debug.LogError($"Could not load UXML at path: {uxmlPath}");
    return;
}
```

---

## Development Workflow

### Setting Up Development Environment

1. Clone or open the project in Unity 2022.1 or later
2. Open Clever Clogs window via `DADCTools > Clever Clogs`
3. Review existing code structure
4. Read the architecture documentation

### Making Changes

1. **Create a branch** for your feature/fix
2. **Update code** following style guidelines
3. **Update documentation** to match code changes
4. **Run the automated tests** (see below) and add tests for new generator or scanner behaviour
5. **Test in Unity Editor** with various UXML files
6. **Check for errors** in Unity console

### Automated Tests

The generator and scanner are covered by EditMode tests in `Assets/Imported/CleverClogs/Tests/Editor/`.

**To run them**: open `Window > General > Test Runner`, select the EditMode tab, and click Run All.

| File | Covers |
|------|--------|
| `ToIdentifierTests.cs` | Name to identifier conversion |
| `BuildSourceTests.cs` | Generated source: naming clashes, duplicates, type resolution, namespaces, editor-only wrapping |
| `GeneratedClassTests.cs` | A generated class used against real UXML |
| `UXMLScanTests.cs` | Element scanning, including template instances |
| `WriteFileTests.cs` | File output and the overwrite guard |
| `RegeneratorTests.cs` | Recording generated classes and regenerating them |
| `PathScanTests.cs` | Finding the install folder, UXML files and settings |
| `PanelTests.cs` | The tool's own panels contain the elements the window looks up |

`Fixtures/SampleFixtureUXML.cs` is real generator output for `Fixtures/SampleFixture.uxml`, checked in so that it is compiled with the tests. If you change what the generator emits, `CheckedInClassMatchesGeneratorOutput` fails until that file is regenerated: open the tool, generate from `SampleFixture.uxml` with the class name `SampleFixtureUXML` and the namespace `DADC.Clogs.Tests`, and replace the file with the result.

The editor window itself has no automated tests; use the manual checklist below.

### Manual Testing Checklist

- [ ] Window opens without errors
- [ ] Scan functionality works
- [ ] Settings persist after Unity restart
- [ ] Panel navigation works (Main ↔ Settings)
- [ ] No console errors or warnings
- [ ] Tested with at least 3 different UXML files
- [ ] Tested with empty project (no UXML files)
- [ ] Tested with large project (100+ UXML files)

---

## Pull Request Process

### Before Submitting

1. **Update documentation** in `/Documentation` folder
2. **Update CHANGELOG.md** with your changes
3. **Add examples** if introducing new features
4. **Test thoroughly** in Unity Editor
5. **Verify no breaking changes** to existing APIs

### PR Description Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Documentation update
- [ ] Performance improvement

## Related Issues
Fixes #(issue number)

## Testing
Describe testing performed

## Documentation
- [ ] Updated relevant .md files
- [ ] Updated CHANGELOG.md
- [ ] Added code examples

## Checklist
- [ ] Code follows style guidelines
- [ ] No console errors
- [ ] Documentation updated
- [ ] Tested in Unity 2022.1+
```

---

## Architecture Decisions

### When Adding New Features

Consider these architectural patterns used in the project:

1. **Static Utility Classes**: For stateless operations (PathScan, UXMLScan, SortElements)
2. **ScriptableObjects**: For persistent configuration (ProjectSettingSO)
3. **EditorWindow**: For UI integration (CleverClogsWindow)
4. **Structs**: For lightweight data (Element, UXMLAsset, SearchFolder)

### File Organization

```
Assets/Imported/CleverClogs/
├── Editor/
│   ├── CleverClogsWindow.cs          # Main window - EditorWindow
│   ├── *.uxml                     # UI layouts
│   ├── *.uss                      # Styles
│   └── Helpers/
│       ├── *Scan.cs               # Static utility classes
│       ├── *SO.cs                 # ScriptableObject classes
│       └── *.cs                   # Struct definitions
└── Templates/                     # Optional templates
```

---

## Questions and Support

### Where to Ask Questions

For help using Clever Clogs in your own project, [join my Patreon for support](https://patreon.com/dannyarnold). GitHub issues are for work on the tool itself:

1. **Implementation Questions**: Open an issue with the label "question"
2. **Bug Reports**: Open an issue with the label "bug"
3. **Feature Requests**: Open an issue with the label "enhancement"

### Before Opening an Issue

- Check existing issues for duplicates
- Review the documentation in `/Documentation`
- Check `OPTIMIZATION-SUGGESTIONS.md` for known issues

---

## Recognition

Contributors will be acknowledged in:
- CHANGELOG.md for each release
- Code comments for significant contributions
- README.md contributors section (if added in future)

---

**Thank you for contributing to Clever Clogs!**

**Project Maintainer**: DADC  
**Last Updated**: December 6, 2025
