# Architecture Overview

## System Architecture

Clever Clogs follows a modular architecture with clear separation of concerns between UI, business logic, and data persistence.

## High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────┐
│                Unity Editor Menu                     │
│              (DADCTools > Clever Clogs)              │
└──────────────────┬──────────────────────────────────┘
                   │ MenuItem Trigger
                   ▼
┌─────────────────────────────────────────────────────┐
│         Presentation Layer (EditorWindow)            │
│  ┌─────────────────────────────────────────────┐   │
│  │          CleverClogsWindow.cs                │   │
│  │  • Panel Management (Main/Settings/About)    │   │
│  │  • UI Event Handling                        │   │
│  │  • UXML/USS Loading                         │   │
│  └──────────────┬──────────────────────────────┘   │
│                 │ Calls                              │
└─────────────────┼──────────────────────────────────┘
                  │
┌─────────────────┼──────────────────────────────────┐
│                 ▼     Business Logic Layer          │
│  ┌──────────────────┐    ┌──────────────────┐     │
│  │   PathScan.cs    │    │  UXMLScan.cs     │     │
│  │  • GetAssetPaths │◄───┤ • ListElements   │     │
│  │  • GetInstallPath│    │                  │     │
│  └──────────────────┘    └─────────┬────────┘     │
│                                    │               │
│  ┌──────────────────┐    ┌─────────▼────────┐     │
│  │  Generate.cs     │    │ SortElements.cs  │     │
│  │  • BuildSource   │    │ • Sort()         │     │
│  │  • WriteFile     │    │                  │     │
│  │  • ToIdentifier  │    │                  │     │
│  └──────────────────┘    └──────────────────┘     │
└─────────────────┬──────────────────────────────────┘
                  │ Reads/Writes
┌─────────────────▼──────────────────────────────────┐
│              Data Layer (Persistence)               │
│  ┌──────────────────────────────────────────────┐  │
│  │   ProjectSettingSO.cs (ScriptableObject)     │  │
│  │   • projectPath                              │  │
│  │   • searchFolders[]                          │  │
│  │   • outputFolder                             │  │
│  │   • generatedNamespace                       │  │
│  │   • autoRegenerate                           │  │
│  │   • generated[]                              │  │
│  │   • OnValidate()                             │  │
│  └──────────────────────────────────────────────┘  │
│                                                     │
│  ┌──────────────────────────────────────────────┐  │
│  │   UICollectionSO / WindowSO (Placeholders)   │  │
│  │   • Minimal implementation                   │  │
│  └──────────────────────────────────────────────┘  │
│                                                     │
│  ┌──────────────────────────────────────────────┐  │
│  │   Structs: Element, UXMLAsset, SearchFolder  │  │
│  │   • In-memory data structures                │  │
│  └──────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────┘
```

## Component Layers

### 1. Presentation Layer

**Responsibility**: Handle user interface and user interactions

**Components**:
- `CleverClogsWindow.cs` - Main editor window controller
- `BasePanel.uxml` - Base layout structure
- `SettingsPanel.uxml` - Settings interface
- `CleverClogs.uss` - Styling definitions

**Key Responsibilities**:
- Rendering UI panels
- Handling user input events
- Updating UI state
- Panel navigation and management

### 2. Business Logic Layer

**Responsibility**: Core functionality and processing

**Components**:
- `PathScan.cs` - Asset discovery and path resolution
- `UXMLScan.cs` - UXML file parsing and element extraction
- `SortElements.cs` - Element organization and categorization
- `Generate.cs` - Helper class generation (enum of element names plus typed properties)
- `Regenerator.cs` - Records generated classes and regenerates them
- `UxmlPostprocessor.cs` - Triggers regeneration when UXML is saved

**Key Responsibilities**:
- Scanning project assets
- Parsing UXML documents
- Analyzing UI element hierarchies
- Generating helper code

### 3. Data Layer

**Responsibility**: Data persistence and configuration

**Components**:
- `ProjectSettingSO.cs` - Project-wide settings
- `UICollectionSO.cs` - UI collection management
- `WindowSO.cs` - Window configurations
- Data structures: `Element`, `UXMLAsset`, `SearchFolder`

**Key Responsibilities**:
- Storing user preferences
- Managing project configuration
- Persisting search paths
- Persisting the output folder and namespace for generated code
- Maintaining asset references

## Design Patterns

### 1. Editor Window Pattern

The main `CleverClogsWindow` class extends `EditorWindow`, following Unity's standard editor extension pattern. This provides:
- Integration with Unity's editor UI
- Lifecycle management
- Menu item registration

### 2. Panel Management Pattern

Dynamic panel loading allows switching between different views (Main, Settings, About) without recreating the entire window.

### 3. Eager Initialization Pattern

Settings are loaded in the `Awake()` method, which executes when the EditorWindow is instantiated:

```csharp
private void Awake()
{
    assetPath = PathScan.GetInstallPath();
    editorPath = assetPath + "Editor/";
    
    if (projectSettings == null)
    {
        projectSettings = ProjectSettingSO.LoadOrCreate();
        projectPath = projectSettings.projectPath;
    }
}
```

**Pattern Benefits**:
- Ensures settings are available before UI creation
- Creates default settings if missing
- Resolves installation path dynamically
- Happens once per window lifetime

### 4. Static Utility Pattern

Helper classes like `PathScan`, `UXMLScan`, `SortElements`, and `Generate` use static methods for stateless operations:

```csharp
// PathScan - Asset discovery
List<string> paths = PathScan.GetAssetPaths(new string[] { "Assets/UI" });
string installPath = PathScan.GetInstallPath();

// UXMLScan - Element extraction
List<Element> elements = UXMLScan.ListElements("Assets/UI/Panel.uxml");

// SortElements - Element categorization
Dictionary<string, List<Element>> sorted = SortElements.Sort(elements);

// Generate - Helper class generation
string source = Generate.BuildSource("PanelUXML", "Assets/UI/Panel.uxml", elements, "MyGame.UI", out List<string> warnings);
string path = Generate.WriteFile("Assets/CleverClogsGenerated", "PanelUXML", source);
```

**Design Rationale**:
- No state to maintain between calls
- Thread-safe (though must run on main thread due to Unity APIs)
- Easy to test and reuse
- Clear, focused responsibilities

`Generate.BuildSource()` is kept free of file and AssetDatabase access so the generator can be exercised without writing anything; `Generate.WriteFile()` does the I/O.

## Data Flow

### Scanning Workflow

1. The user clicks Scan
2. `PathScan.GetAssetPaths()` searches the folders in `ProjectSettingSO.searchFolders`
3. The window stores the asset paths and fills the dropdown with the file names

### Element Analysis Workflow

1. The user picks a file in the dropdown
2. `UXMLScan.ListElements()` instantiates the UXML and returns every named element with its type
3. The window shows the elements in the element list, all selected

### Generation Workflow

1. The user unticks unwanted elements and clicks Generate
2. `Generate.BuildSource()` converts the selected elements into the source of a helper class
3. `Generate.WriteFile()` writes it to `ProjectSettingSO.outputFolder` and imports it
4. `Regenerator` records the class in `ProjectSettingSO.generated`
5. Unity recompiles and the helper class becomes available to project code

### Regeneration Workflow

1. A UXML file is saved, or scripts finish reloading
2. `UxmlPostprocessor` schedules a run with `EditorApplication.delayCall`
3. `Regenerator.RegenerateAll()` rebuilds the source for every recorded class
4. Only classes whose source differs are rewritten, so unrelated UXML edits cause no recompile

## Extension Points

### Future Extensibility

The architecture supports several extension points:

1. **Custom Code Generators**: Add further `BuildSource`-style methods to `Generate.cs` for different output shapes
2. **Additional Panels**: Add new UXML panels and corresponding setup methods
3. **Element Processors**: Implement custom element analysis in `UXMLScan.cs`
4. **Data Collectors**: Create new ScriptableObject types for additional configuration

## Threading Model

⚠️ **All operations execute on Unity's main editor thread.**

### Constraints

- **Unity API Requirements**: AssetDatabase, ScriptableObject, and UI Toolkit APIs require main thread
- **No Async/Await**: Current implementation is fully synchronous
- **No Background Processing**: All scans and parsing block the UI thread

### Implications

1. **Scanning Large Projects**: May cause editor freeze on large projects with many UXML files
2. **UI Responsiveness**: Long operations will freeze the editor window
3. **No Progress Reporting**: Cannot update UI during long-running operations

### Future Optimization Opportunities

```csharp
// Potential async implementation
private async void ScanButton_clicked()
{
    statusText.text = "Scanning...";
    
    // Offload to background thread
    var guids = await Task.Run(() => 
    {
        // Collect GUIDs in background
        return AssetDatabase.FindAssets("t:VisualTreeAsset", searchFolders);
    });
    
    // Return to main thread for Unity API calls
    List<string> assetPaths = new List<string>();
    foreach (string guid in guids)
    {
        assetPaths.Add(AssetDatabase.GUIDToAssetPath(guid));
    }
    
    PopulateDropdown(assetPaths);
}
```

**Recommendations**:
1. Add async/await support for scanning operations
2. Implement progress bar for long-running scans
3. Add cancellation token support
4. Consider batching operations for better responsiveness

## Memory Management

- UI elements are cleared and recreated when switching panels using `root.Clear()`
- ScriptableObjects are loaded from disk and cached in memory
- Asset references are resolved on-demand through Unity's AssetDatabase
- No explicit memory management required due to Unity's automatic garbage collection

## Error Handling

The tool uses defensive programming with null checks and exceptions:

This ensures early detection of missing UI elements during development.

---

**Next**: [Core Components](Core-Components.md)
