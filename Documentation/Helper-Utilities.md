# Helper Utilities

## PathScan

**File**: `PathScan.cs`  
**Namespace**: `DADC.Clogs`

### Purpose

Provides static utility methods for scanning and locating VisualTreeAsset (UXML) files within Unity projects.

### Methods

#### `GetAssetPaths()`

**Purpose**: Scans specified folders (or entire Assets directory) for UXML files.

**Parameters**:
- `foldersToSearch`: Optional array of folder paths to search. Defaults to `["Assets"]` if null.

**Returns**: `List<string>` of asset paths to all discovered UXML files.

**Implementation**:
1. Determines search folders (default or provided)
2. Uses `AssetDatabase.FindAssets("t:VisualTreeAsset")` to find GUIDs
3. Converts GUIDs to asset paths
4. Returns list of paths

**Example Usage**:

```csharp
using DADC.Clogs;
using UnityEngine;

public class Example : MonoBehaviour
{
    void Start()
    {
        // Example 1: Scan entire Assets directory
        List<string> allUXMLFiles = PathScan.GetAssetPaths(null);
        
        // Example 2: Scan specific folders
        string[] folders = { "Assets/UI", "Assets/Widgets" };
        List<string> specificUXMLFiles = PathScan.GetAssetPaths(folders);
    }
}
```

#### `GetInstallPath()`

**Purpose**: Locates the installation directory of the tool by searching for the project folder name.

**Parameters**:
- `projectName`: Name of the project folder to locate (e.g., "CleverClogs")

**Returns**: `string` path to the installation directory with trailing slash, or `null` if not found.

**Implementation**:
1. Finds all VisualTreeAssets in Assets folder
2. Iterates through paths looking for `projectName`
3. Extracts and returns the path up to and including the project name

**Example**:

```csharp
using DADC.Clogs;

public class ExampleUsage
{
    void ExampleMethod()
    {
        // Assuming the project name is "MyUnityProject"
        string installPath = PathScan.GetInstallPath("MyUnityProject");
        
        // installPath should now hold the path to the project directory
        // e.g., "C:/Projects/MyUnityProject/"
    }
}
```

**Use Case**: Resolves relative paths for loading resources (UXML, USS, ScriptableObject)

## SortElements

**File**: `SortElements.cs`  
**Namespace**: `DADC.Clogs`

**Purpose**: Groups elements by their type into a dictionary for easy categorization and analysis.

### Methods

#### `Sort()`

**Signature**:
```csharp
public static Dictionary<string, List<Element>> Sort(List<Element> elements)
```

**Parameters**:
- `elements`: List of Element structs to sort

**Returns**: Dictionary where keys are element types (e.g., "Button", "Label") and values are lists of elements of that type.

**Implementation**:
```csharp
public static Dictionary<string, List<Element>> Sort(List<Element> elements)
{
    Dictionary<string, List<Element>> sorted = new Dictionary<string, List<Element>>();
    foreach (Element element in elements)
    {
        string type = element.type;
        if (sorted.ContainsKey(type))
        {
            sorted[type].Add(element);
        }
        else
        {
            sorted.Add(type, new List<Element>() { element });
        }
    }
    return sorted;
}
```

**Performance**: O(n) time complexity where n is the number of elements.

**Example Usage**:

```csharp
using System.Collections.Generic;
using DADC.Clogs;

public class UIElementProcessor
{
    public void AnalyzeUIFile(string uxmlPath)
    {
        // Step 1: Extract elements
        List<Element> elements = UXMLScan.ListElements(uxmlPath);
        
        // Step 2: Sort by type
        Dictionary<string, List<Element>> sortedByType = SortElements.Sort(elements);
        
        // Step 3: Process by type
        foreach (var kvp in sortedByType)
        {
            string elementType = kvp.Key;
            List<Element> elementsOfType = kvp.Value;
            
            Debug.Log($"Found {elementsOfType.Count} elements of type {elementType}:");
            foreach (Element element in elementsOfType)
            {
                Debug.Log($"  - {element.name}");
            }
        }
    }
}
```

**Output Example**:
```
Found 3 elements of type Button:
  - ScanButton
  - CancelButton
  - SubmitButton
Found 2 elements of type Label:
  - StatusText
  - TitleLabel
Found 1 elements of type DropdownField:
  - PathsDropdown
```

**Use Cases**:
1. Display element statistics by type
2. Generate helper code grouped by element type
3. Validate UXML structure (e.g., ensure required types exist)
4. Create type-specific processing pipelines

---

## Generate

**File**: `Generate.cs`  
**Namespace**: `DADC.Clogs`  
**Type**: `static class`

### Purpose

Builds the C# source of a helper class from the named elements of a UXML file and writes it to disk. The helper class contains an enum of the element names and a typed property for each element.

### Methods

#### `BuildSource()`

**Signature**:
```csharp
public static string BuildSource(string className, string uxmlPath, IList<Element> elements, string namespaceName, out List<string> warnings)
```

**Parameters**:
- `className`: Name for the helper class. Passed through `ToIdentifier()` first.
- `uxmlPath`: Asset path of the source UXML file. Written into the file header and the `UxmlPath` constant.
- `elements`: Elements to generate. Each needs a `name`, and an `elementType` if the property should be more specific than `VisualElement`.
- `namespaceName`: Namespace to wrap the class in. Null or empty generates the class in the global namespace.
- `warnings`: Receives one message per renamed or skipped element.

**Returns**: The complete source of the helper class as a string. Nothing is written to disk.

**Implementation**:
1. Skips elements with an empty name, and any name already seen (with a warning)
2. Converts each name to an identifier with `ToIdentifier()`
3. Renames identifiers that clash with a member of the generated class or with each other (with a warning)
4. Resolves each element type to one that generated code can refer to
5. Emits the enum, the name table, the properties, the constructor and the accessors

**Type resolution**:
- Types in `UnityEngine.UIElements` are written by short name (`Button`)
- Other types are written fully qualified (`global::MyGame.UI.HealthBar`)
- Non-public and generic types are replaced by their nearest public, non-generic base type
- A null type, or one that isn't a `VisualElement`, becomes `VisualElement`
- If any type comes from an editor assembly, the whole file is wrapped in `#if UNITY_EDITOR`

#### `WriteFile()`

**Signature**:
```csharp
public static string WriteFile(string folder, string className, string source)
```

**Parameters**:
- `folder`: Output folder as a project-relative path (e.g. `"Assets/CleverClogsGenerated"`). Created if missing.
- `className`: Used for the file name, after `ToIdentifier()`.
- `source`: The text to write, normally the result of `BuildSource()`.

**Returns**: The asset path of the written file.

**Exceptions**:
- `ArgumentException` if `folder` is null or empty
- `IOException` if the target file exists and does not start with `Generate.Header`, meaning it was not generated by this tool

**Notes**:
- Overwrites a previously generated file of the same name
- Calls `AssetDatabase.ImportAsset()` on the result, which triggers a script recompile

#### `ToIdentifier()`

**Signature**:
```csharp
public static string ToIdentifier(string name)
```

**Purpose**: Turns any string into a PascalCase C# identifier.

**Rules**:
- Characters that aren't letters or digits are removed and treated as word breaks
- The first character of each word is upper-cased; the rest are left as they are
- A leading `_` is added if the result would be empty or start with a digit

**Examples**:
- `"submit-button"` → `"SubmitButton"`
- `"unity-text-input"` → `"UnityTextInput"`
- `"ScanButton"` → `"ScanButton"`
- `"2nd-item"` → `"_2ndItem"`
- `"--"` or `null` → `"_"`

### Constants

#### `Header`

The first line of every generated file (`// <auto-generated>`). `WriteFile()` uses it to recognise files it is allowed to overwrite.

### Reserved Names

These identifiers are used by the generated class itself, so an element whose name converts to one of them gets an `Element` suffix:

`Element`, `UxmlPath`, `Root`, `NameOf`, `Q`, `Get`, `Equals`, `GetHashCode`, `GetType`, `ToString`, `MemberwiseClone`, `Finalize`, and the helper class's own name.

### Example Usage

```csharp
using System.Collections.Generic;
using DADC.Clogs;
using UnityEngine;

public class ExampleUsage
{
    public void GenerateHelper()
    {
        // Scan UXML file
        string uxmlPath = "Assets/UI/MainPanel.uxml";
        List<Element> elements = UXMLScan.ListElements(uxmlPath);

        // Build the source
        string source = Generate.BuildSource("MainPanelUXML", uxmlPath, elements, "MyGame.UI", out List<string> warnings);
        foreach (string warning in warnings) Debug.LogWarning(warning);

        // Save to Assets/CleverClogsGenerated/MainPanelUXML.cs
        string path = Generate.WriteFile("Assets/CleverClogsGenerated", "MainPanelUXML", source);

        // After Unity recompiles you can use the generated class:
        // var ui = new MyGame.UI.MainPanelUXML(rootVisualElement);
        // ui.ScanButton.clicked += OnScanClicked;
    }
}
```

See the [User Guide](User-Guide.md) for the shape of the generated class.

---

## Regenerator

**File**: `Regenerator.cs`  
**Namespace**: `DADC.Clogs`  
**Type**: `static class`

### Purpose

Generates helper classes on behalf of the editor window, records how each one was made in `ProjectSettingSO.generated`, and regenerates them from those records.

### Methods

#### `GenerateAndRecord()`

**Signature**:
```csharp
public static string GenerateAndRecord(ProjectSettingSO settings, string uxmlPath, string className, IList<Element> elements, out List<string> warnings)
```

**Purpose**: Builds and writes a helper class from the elements whose `selected` flag is set, then adds or updates the matching `GeneratedHelper` record. Names of unselected elements are stored as exclusions.

**Returns**: The asset path of the written class.

**Notes**:
- Uses `settings.outputFolder` and `settings.generatedNamespace`
- Records are keyed by output path, so generating the same class again updates its record, and generating the same UXML under another class name adds a second one
- Throws the same exceptions as `Generate.WriteFile()`

#### `Regenerate()`

**Signature**:
```csharp
public static RegenerateResult Regenerate(GeneratedHelper helper)
```

**Purpose**: Rebuilds one helper class from its UXML and rewrites the file only if the result differs.

**Returns**:

| Result | Meaning |
|--------|---------|
| `Unchanged` | The file already matches; nothing was written. Line-ending differences are ignored |
| `Updated` | The file was rewritten |
| `Removed` | The generated file no longer exists. The caller should drop the record |
| `Skipped` | The UXML could not be loaded, or the target file is not a generated file |

#### `RegenerateAll()`

**Signature**:
```csharp
public static int RegenerateAll(ProjectSettingSO settings)
```

**Purpose**: Runs `Regenerate()` for every record, removes records that report `Removed`, and logs each rewritten file.

**Returns**: The number of files rewritten. Returns 0 without doing anything if `settings.autoRegenerate` is false.

#### `Find()`

**Signature**:
```csharp
public static GeneratedHelper Find(ProjectSettingSO settings, string uxmlPath)
```

**Purpose**: Returns the first record for a UXML file, or `null`. The window uses it to restore the class name and tick boxes.

#### `OutputMoved()`

**Signature**:
```csharp
public static bool OutputMoved(ProjectSettingSO settings, string from, string to)
```

**Purpose**: Points a record at a generated class that was moved or renamed in the project. Returns `false` if no record had the old path.

---

## UxmlPostprocessor

**File**: `UxmlPostprocessor.cs`  
**Namespace**: `DADC.Clogs`  
**Type**: `AssetPostprocessor` (internal)

### Purpose

Triggers `Regenerator.RegenerateAll()` for every `ProjectSettingSO` asset in the project.

### Triggers

- `OnPostprocessAllAssets`, when any `.uxml` file was imported or moved
- `[InitializeOnLoadMethod]`, after every script reload

### Behaviour

- The work is deferred with `EditorApplication.delayCall`, so scripts are never written from inside an asset import. Several triggers in one import collapse into one run.
- Does nothing in batch mode (`Application.isBatchMode`)
- Does nothing while `EditorUtility.scriptCompilationFailed` is true
- When a `.cs` file is moved, calls `Regenerator.OutputMoved()` so records follow generated files

A regeneration that rewrites a file causes a recompile, which triggers one more run. That run finds everything unchanged, so it stops there.

---

## Utility Usage Workflow

1. `PathScan.GetAssetPaths()` finds the UXML files
2. `UXMLScan.ListElements()` lists the named elements of one file
3. `Regenerator.GenerateAndRecord()` calls `Generate.BuildSource()` and `Generate.WriteFile()`, then records the result
4. `UxmlPostprocessor` calls `Regenerator.RegenerateAll()` whenever UXML changes

`SortElements.Sort()` is available for grouping elements by type but is not used by the generation workflow.

---

**Next**: [Data Models](Data-Models.md)
