# Data Models

## ScriptableObject Models

### ProjectSettingSO

**File**: `ProjectSettingSO.cs`  
**Namespace**: `DADC.Clogs`  
**Type**: `ScriptableObject`

#### Purpose

Stores persistent project-wide settings for Clever Clogs, including search paths, project location, and where generated code goes.

#### Properties

##### `projectPath`
- **Type**: `string`
- **Purpose**: Stores the directory path where the project settings asset is located
- **Auto-updated**: Yes, through `OnValidate()`

##### `searchFolders`
- **Type**: `string[]`
- **Purpose**: Array of folder paths to search for UXML files
- **Default**: `["Assets"]` (entire project)

##### `outputFolder`
- **Type**: `string`
- **Purpose**: Project-relative folder that generated helper classes are written to
- **Default**: `"Assets/CleverClogsGenerated"`

##### `generatedNamespace`
- **Type**: `string`
- **Purpose**: Namespace for generated helper classes; empty means the global namespace
- **Default**: `""`

##### `autoRegenerate`
- **Type**: `bool`
- **Purpose**: Whether recorded helper classes are regenerated when UXML changes
- **Default**: `true`

##### `generated`
- **Type**: `List<GeneratedHelper>`
- **Purpose**: One record per generated helper class, used for regeneration
- **Default**: Empty

Settings assets created before these fields existed pick up the defaults automatically.

Because the records live in this asset, commit it to version control along with the generated classes so that teammates get the same regeneration behaviour.

#### Methods

##### `OnValidate()`

**Purpose**: Automatically updates `projectPath` when the asset is moved or renamed.

**Behavior**:
- Executes when the ScriptableObject is validated in the editor
- Compares current asset path with stored `projectPath`
- Updates `projectPath` if different
- Marks asset as dirty to ensure changes are saved

**C# Feature**: Uses range operator `[..(lastSlash + 1)]` (C# 8.0+)

#### Usage Example

````````

---

### GeneratedHelper

**File**: `GeneratedHelper.cs`  
**Namespace**: `DADC.Clogs`  
**Type**: Serializable class, stored inside `ProjectSettingSO`

#### Purpose

Remembers how a helper class was generated so it can be regenerated when its UXML changes.

#### Fields

##### `uxmlGuid`
- **Type**: `string`
- **Purpose**: Asset ID of the source UXML file, so the record survives the file being moved or renamed

##### `className`
- **Type**: `string`
- **Purpose**: Name of the generated class, already converted to a valid identifier

##### `outputPath`
- **Type**: `string`
- **Purpose**: Asset path of the generated `.cs` file. Also the key that identifies the record

##### `namespaceName`
- **Type**: `string`
- **Purpose**: Namespace the class was generated with; later changes to the setting do not affect it

##### `excludedElements`
- **Type**: `string[]`
- **Purpose**: Names of elements that were unticked. Storing exclusions means elements added later are included by default

---

### UICollectionSO

**File**: `UICollectionSO.cs`  
**Type**: `ScriptableObject`

#### Purpose

Planned container for managing collections of UI windows and elements.

#### Properties

##### `window`
- **Type**: `WindowSO`
- **Access**: Private with serialization
- **Purpose**: Reference to a WindowSO configuration

#### Unity Menu

````````markdown
Creates the asset via:
````````

#### Current Status

Minimal implementation suggests this is a placeholder for future functionality to organize multiple UI windows and their configurations.

---

### WindowSO

**File**: `WindowSO.cs`  
**Type**: `ScriptableObject`

#### Purpose

Configuration container for individual UI windows. Currently a placeholder for future implementation.

#### Properties

No properties currently defined.

#### Unity Menu

````````

#### Future Usage

Expected to store window-specific settings such as:
- Window dimensions
- UXML asset references
- USS stylesheet references
- Element configurations
- Layout preferences

---

## Struct Models

### Element

**File**: `UXMLScan.cs`  
**Namespace**: `DADC.Clogs`  
**Type**: `struct`

#### Purpose

Represents a single UI element discovered in a UXML file with its metadata.

#### Properties

##### `name`
- **Type**: `string`
- **Purpose**: The element's name attribute from UXML (e.g., "ScanButton")

##### `type`
- **Type**: `string`
- **Purpose**: The element's type name (e.g., "Button", "Label", "DropdownField"), used for display

##### `elementType`
- **Type**: `System.Type`
- **Purpose**: The element's runtime type, used by `Generate` to type the generated property
- **Default**: `null` (generates a `VisualElement` property)

##### `selected`
- **Type**: `bool`
- **Purpose**: Whether the element is ticked in the editor window's element list; only selected elements are generated
- **Default**: `false` (the window sets every element to `true` when a file is selected)

#### Constructor

````````

#### Usage Example

````````

---

### UXMLAsset

**File**: `UXMLAsset.cs`  
**Namespace**: None (global namespace)  
**Type**: `struct`

⚠️ **Warning**: This struct is defined in the global namespace. Consider adding `namespace DADC.Clogs` for better organization.

#### Purpose

Represents metadata about a UXML asset file for UI presentation and selection.

#### Properties

##### `path`
- **Type**: `string`
- **Purpose**: Full asset path to the UXML file
- **Example**: `"Assets/UI/MainPanel.uxml"`

##### `name`
- **Type**: `string`
- **Purpose**: Display name or filename for UI dropdown menus
- **Example**: `"Main Panel"` or `"MainPanel.uxml"`

##### `selected`
- **Type**: `bool`
- **Purpose**: Selection state for batch operations or UI interactions
- **Default**: Set via constructor

#### Constructor

```csharp
public UXMLAsset(string path, string name, bool selected)
{
    this.path = path;
    this.name = name;
    this.selected = selected;
}
```

#### Usage Example

```csharp
// Create asset reference for UI display
var asset = new UXMLAsset(
    "Assets/UI/Panels/MainPanel.uxml",
    "Main Panel",
    false  // Not selected initially
);

// Use in dropdown population
List<UXMLAsset> assets = new List<UXMLAsset>();
foreach (string assetPath in discoveredPaths)
{
    string filename = System.IO.Path.GetFileName(assetPath);
    assets.Add(new UXMLAsset(assetPath, filename, false));
}
```

#### Notes

- Currently not used in `CleverClogsWindow.cs` - implementation uses `List<string>` directly
- Consider using this struct in future refactoring for better type safety
- Add namespace declaration for consistency

---

### SearchFolder

**File**: `SearchFolder.cs`  
**Namespace**: None (global namespace)  
**Type**: `struct`

⚠️ **Warning**: This struct is defined in the global namespace. Consider adding `namespace DADC.Clogs` for better organization.

#### Purpose

Represents a folder path that can be included or excluded from UXML scanning operations.

#### Properties

##### `path`
- **Type**: `string`
- **Purpose**: Folder path relative to project root
- **Example**: `"Assets/UI"`, `"Assets/Editor/Resources/UXML"`

##### `selected`
- **Type**: `bool`
- **Purpose**: Whether this folder should be included in scans
- **Note**: Property name is `selected`, not `enabled`

#### Constructor

```csharp
public SearchFolder(string path, bool selected)
{
    this.path = path;
    this.selected = selected;
}
```

#### Usage Example

```csharp
// Define multiple search folders with selection states
List<SearchFolder> searchPaths = new List<SearchFolder>
{
    new SearchFolder("Assets/UI/Panels", true),      // Include
    new SearchFolder("Assets/UI/Widgets", true),     // Include
    new SearchFolder("Assets/ThirdParty", false)     // Exclude
};

// Filter to get only selected paths
string[] selectedPaths = searchPaths
    .Where(folder => folder.selected)
    .Select(folder => folder.path)
    .ToArray();

// Use in scanning
List<string> assetPaths = PathScan.GetAssetPaths(selectedPaths);
```

#### Notes

- Currently not used in `ProjectSettingSO` - uses `string[]` directly
- No GUID tracking in current implementation
- Consider using this struct in future refactoring for UI folder selection
- Add namespace declaration for consistency

---

## Code Organization Recommendations

### Current Namespace Issues

Several structs in the codebase lack namespace declarations:

| Struct | Current Namespace | Recommended |
|--------|------------------|-------------|
| `UXMLAsset` | Global | `DADC.Clogs` |
| `SearchFolder` | Global | `DADC.Clogs` |
| `Element` | `DADC.Clogs` | ✓ Correct |

### Recommended Refactoring

```csharp
// UXMLAsset.cs
namespace DADC.Clogs
{
    public struct UXMLAsset 
    {
        public string path;
        public string name;
        public bool selected;
        
        public UXMLAsset(string path, string name, bool selected)
        {
            this.path = path;
            this.name = name;
            this.selected = selected;
        }
    }
}

// SearchFolder.cs
namespace DADC.Clogs
{
    public struct SearchFolder 
    {
        public string path;
        public bool selected;
        
        public SearchFolder(string path, bool selected)
        {
            this.path = path;
            this.selected = selected;
        }
    }
}
```

### Benefits

1. **Prevents naming conflicts** with other tools or assets
2. **Improves code organization** and discoverability
3. **Maintains consistency** with other project files
4. **Enables better IntelliSense** support in IDEs
5. **Follows C# best practices** and Unity conventions

### Implementation Priority

**High Priority**: Add namespaces to all structs to prevent potential conflicts in larger projects.

---

## Data Persistence

### ScriptableObject Assets

All ScriptableObject-based models persist as `.asset` files in the project:

- **Location**: Anywhere under `Assets`. Never inside the tool's own folder, which is read-only when installed as a package
- **Format**: Unity's binary asset format
- **Serialization**: Automatic through Unity's serialization system
- **Version Control**: Should be committed to source control

### Default Settings

The tool creates a default settings file at:
```
Assets/CleverClogsSettings.asset
```

### User Settings

Users can create custom settings via:
```csharp
[MenuItem("Assets/Create/Tools/Clever Clogs/Project Settings")]
// Creates new ProjectSettingSO asset
```

---

**Next**: [User Guide](User-Guide.md)
