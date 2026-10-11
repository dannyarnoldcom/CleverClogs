# User Guide

## Getting Started

### Installation

1. Import the Clever Clogs package into your Unity project
2. The tool will be available under `DADCTools > Clever Clogs` in the Unity menu bar

### Opening the Tool

**Menu Path**: `DADCTools > Clever Clogs`

The window will open with a minimum size of 380x450 pixels.

---

## User Interface Overview

### Main Panel

The main panel consists of several key areas:

1. **Status Bar** (Top)
   - Displays current operation status
   - Shows scan results count
   - Toolbar menu for navigation

2. **Scan Section** (Center)
   - Scan button to discover UXML files
   - Dropdown to select discovered files
   - Helper name text field

3. **Results Section** (Bottom)
   - Lists the named elements in the selected file, with a tick box, type and name for each
   - Generate button

### Settings Panel

Access via: `Toolbar Menu > Settings`

Configurable options:
- **Project Settings**: ScriptableObject reference
- **Project Path**: Auto-updated path to settings location
- **Output Folder**: Where generated helper classes are written (default: `Assets/CleverClogsGenerated`)
- **Namespace**: Namespace for generated helper classes (default: none)
- **Search Folders**: Folders to include in UXML scans

---

## Basic Workflow

### Step 1: Initial Setup

1. Open Clever Clogs window
2. Navigate to Settings (Toolbar Menu > Settings)
3. Configure search folders if needed (default: entire Assets folder)
4. Set the output folder and namespace for generated classes if the defaults don't suit your project

### Step 2: Scanning for UXML Files

1. Return to main panel (Toolbar Menu > Scan)
2. Click the **Scan** button
3. Wait for scan completion
   - Status will show "Scanning..."
   - Upon completion: "Found X assets"

### Step 3: Selecting a UXML File

1. Once scan completes, the dropdown becomes visible
2. Click the dropdown menu
3. Select your desired UXML file from the list
4. The helper name field auto-populates with a generated name
5. The results section lists every named element in the file, and the status bar shows "Found X named elements"

### Step 4: Choosing Elements

Each row in the list shows a tick box, the element's type and its name. All elements start ticked.

- Untick any element you don't want in the helper class
- Edit the **Helper Class Name** field if you want a different class name

Only elements with a `name` attribute are listed. Elements inside template instances are included.

### Step 5: Generating the Helper Class

1. Click **Generate**
2. The class is written to `<Output Folder>/<Helper Class Name>.cs` and imported
3. The status bar shows "Generated" followed by the file path

Generating again for the same file overwrites the previous output. The tool refuses to overwrite a `.cs` file it did not generate.

### Step 6: Using the Helper Class

For `MainPanel.uxml` with a `ScanButton` button and a `status-text` label, the generated class looks like this:

```csharp
public sealed class MainPanelUXML
{
    public enum Element
    {
        ScanButton,
        StatusText,
    }

    public const string UxmlPath = "Assets/UI/MainPanel.uxml";

    public static string NameOf(Element element);
    public static T Q<T>(VisualElement root, Element element) where T : VisualElement;

    public VisualElement Root { get; }
    public Button ScanButton { get; }
    public Label StatusText { get; }

    public MainPanelUXML(VisualElement root);

    public VisualElement Get(Element element);
    public T Get<T>(Element element) where T : VisualElement;
}
```

There are two ways to use it:

```csharp
// 1. Typed properties. The constructor queries every element once.
var ui = new MainPanelUXML(rootVisualElement);
ui.ScanButton.clicked += OnScanClicked;
ui.StatusText.text = "Ready";

// 2. The enum in place of a string literal, without creating an instance.
Label status = MainPanelUXML.Q<Label>(rootVisualElement, MainPanelUXML.Element.StatusText);
string name = MainPanelUXML.NameOf(MainPanelUXML.Element.StatusText); // "status-text"
```

A property is `null` if the tree passed to the constructor doesn't contain that element.

---

## Detailed Features

### Scanning System

#### What Gets Scanned

The tool searches for files of type `VisualTreeAsset` (UXML files) within configured folders.

#### Search Scope

**Default**: Entire `Assets` folder

**Custom**: Configure in Settings panel
- Add/remove folders
- Organize search paths
- Focus on specific directories

#### Scan Results

Results include:
- Full asset path
- File name
- Total count of discovered files

### File Selection

#### Dropdown Behavior

- First item: "Select a file" (placeholder)
- Subsequent items: UXML filenames
- Selecting a file fills in the helper name and lists the file's named elements

#### Helper Name Generation

The tool automatically converts the UXML filename to a code-friendly helper class name using a simple algorithm:

**Algorithm**:
1. Take the selected filename from the dropdown
2. Remove the ".uxml" extension
3. Append "UXML" suffix

**Implementation**:
```csharp
helperNameText.value = choice.Replace(".uxml", "UXML");
```

**Examples**:
- `MainPanel.uxml` → `MainPanelUXML`
- `InventoryItem.uxml` → `InventoryItemUXML`
- `Settings.uxml` → `SettingsUXML`

The field is editable. When you click Generate, the name is converted to a valid C# identifier using the same rules as element names (see below), so `Main Panel-v2` becomes `MainPanelV2`.

**Limitations**:
- Does not check whether another class with the same name already exists in your project

### Element Naming Rules

UXML names are often not valid C# identifiers, so the generator converts them. The enum always maps back to the original name, so lookups still work.

| UXML name | Generated identifier | Rule |
|-----------|---------------------|------|
| `ScanButton` | `ScanButton` | Already valid, left unchanged |
| `submit-button` | `SubmitButton` | Split on anything that isn't a letter or digit, then PascalCase |
| `2nd-item` | `_2ndItem` | Prefixed with `_` if it would start with a digit |
| `Root` | `RootElement` | Suffixed with `Element` if it clashes with a member of the generated class |
| `submit_button` (alongside `submit-button`) | `SubmitButtonElement` | Suffixed if two names produce the same identifier |

Other cases:

- **Duplicate names**: if the same name appears more than once in the file, only the first is generated, because `Q()` only ever returns the first match
- **Editor-only element types** (for example `ObjectField`, `Toolbar`): the whole file is wrapped in `#if UNITY_EDITOR` so it doesn't break player builds
- **Internal or generic element types**: the property uses the nearest public base type, falling back to `VisualElement`

Renames and skipped duplicates are logged as warnings in the Console.

---

## Settings Management

### Project Settings

#### Default Settings

Created automatically on first launch:

#### Custom Settings

Create custom settings via:

Then assign in the Settings panel.

### Search Folders Configuration

#### Adding Folders

1. Open Settings panel
2. Locate "Folders to search" section
3. Add folder paths (e.g., "Assets/UI", "Assets/Editor/Resources")

#### Folder Format

Use project-relative paths:
- ? Correct: `Assets/UI/Panels`
- ? Correct: `Assets/Editor/Windows`
- ? Incorrect: `C:/Users/Dev/Project/Assets/UI` (absolute path)

#### Multiple Folders

Configure multiple search paths to:
- Organize large projects
- Separate concerns (UI, Editor, Plugins)
- Improve scan performance

**Example Configuration**:

---

## Tips and Best Practices

### Organizing UXML Files

#### Recommended Structure

- Keep all UXML files in `Assets/UI/UXML` folder
- Use subfolders for categories (e.g., `Assets/UI/UXML/Buttons`)
- Avoid deep nesting to reduce scan time

#### Naming Conventions

- Use PascalCase: `MainPanel.uxml`
- Be descriptive: `InventoryItemCard.uxml`
- Avoid special characters: `Item-Panel.uxml` ? ? `ItemPanel.uxml` ?

### Performance Optimization

#### Limiting Search Scope

For large projects, configure specific search folders instead of scanning entire Assets directory:

#### Project Organization

- Keep UXML files in dedicated folders
- Separate editor UI from runtime UI
- Use consistent folder structure

### Working with Multiple Settings

Different team members or scenarios may require different settings:

1. Create multiple `ProjectSettingSO` assets
2. Switch between them via Settings panel
3. Store in version control for team sharing

**Example**:
```
Assets/Settings/
├── DefaultProjectSetting.asset      # Default configuration
├── DeveloperSettings.asset          # For development
└── ProductionSettings.asset         # For final builds
```

---

## Toolbar Menu Options

### Main Panel Menu

- **Settings**: Navigate to settings panel
- **About**: Display about information (placeholder)

### Settings Panel Menu

- **Scan**: Return to main scanning panel
- **About**: Display about information (placeholder)

---

## Keyboard Shortcuts

*Currently no keyboard shortcuts are defined*

**Future Enhancement**: Consider requesting shortcuts for:
- Quick scan: `Ctrl/Cmd + Shift + U`
- Toggle settings: `Ctrl/Cmd + ,`

---

## Troubleshooting

### No Assets Found

**Problem**: Scan returns "No assets found"

**Solutions**:
1. Verify UXML files exist in your project
2. Check search folders configuration in Settings
3. Ensure files are in specified search paths
4. Try resetting search folders to default (`["Assets"]`)

### Settings Not Persisting

**Problem**: Settings changes don't save

**Solutions**:
1. Ensure `DefaultProjectSetting.asset` exists
2. Check file isn't read-only
3. Try manually creating new settings asset
4. Verify Unity editor has write permissions

### UI Elements Not Found Errors

**Problem**: Exception: "ToolbarMenu not found" (or similar)

**Solutions**:
1. Verify UXML files exist in correct location
2. Check `CleverClogs.uss` stylesheet is present
3. Reimport Clever Clogs package
4. Check console for asset loading errors

### Dropdown Doesn't Appear

**Problem**: After scanning, dropdown remains hidden

**Solutions**:
1. Check if scan actually found files (status message)
2. Check the Unity Console for errors
3. Try closing and reopening the window

The dropdown and the results section are hidden until a scan finds files; this is expected.

### Generate Failed

**Problem**: Status shows "Generate failed, see console"

**Solutions**:
1. If the Console says the file "already exists and was not generated by Clever Clogs", change the helper class name or move the existing file
2. If the Console says "No output folder set", set Output Folder in Settings
3. Verify the output folder is inside the project and writable

### No Elements Selected

**Problem**: Status shows "No elements selected"

**Solutions**:
1. Tick at least one element in the list
2. If the list is empty, the UXML file has no elements with a `name` attribute

---

## Advanced Usage

### Batch Processing (Planned)

Analyze multiple UXML files simultaneously:
1. Select multiple files from list
2. Generate combined helper classes
3. Export results to file

---

## FAQ

**Q: Can I use this tool with UI Builder?**  
A: Yes! Files created with Unity's UI Builder are fully compatible.

**Q: Does this work with runtime UI Toolkit?**  
A: Yes. The tool itself only runs in the editor, but the generated classes are plain C# and work in player builds, unless the UXML uses editor-only element types.

**Q: Can I customize the generated helper names?**  
A: Yes. Edit the Helper Class Name field before clicking Generate. Property and enum names are derived from the element names in the UXML file.

**Q: Can I edit a generated file?**  
A: Changes are lost the next time you generate for the same UXML file. Rename the element in the UXML file and regenerate instead.

**Q: Is the source code available?**  
A: Yes, all source files are included in the Editor folder.

**Q: Does this support nested UXML templates?**  
A: Yes. Named elements inside template instances are listed and generated along with the file's own elements.

---

**Next**: [API Reference](API-Reference.md)