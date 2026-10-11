# Core Components

## CleverClogsWindow (Main Window)

**File**: `CleverClogsWindow.cs`  
**Type**: `EditorWindow`  
**Namespace**: `DADC.Clogs`

### Purpose

The main editor window that provides the user interface for scanning, analyzing, and working with UXML files in Unity projects.

### Key Responsibilities

1. Window lifecycle management
2. UI panel orchestration
3. User interaction handling
4. Integration with helper utilities
5. Settings management

### Class Structure

### Key Methods

#### Window Management

##### `ShowWindow()`

- Registers the menu item in Unity's menu bar
- Creates or focuses the editor window
- Sets window title and minimum size (380x450)

##### `CreateGUI()`

- Called by Unity when the window is created
- Initializes the root visual element
- Loads the base panel
- Sets up the main panel view

#### Panel Management

##### `loadPanel()`

- Dynamically loads UXML and USS files
- Clears existing content
- Instantiates new visual tree
- Applies styling

**Parameters**:
- `root`: Target visual element container
- `uxml`: Path to UXML layout file
- `uss`: Path to USS stylesheet file

##### `ShowMainPanel()`

- Displays the main scanning interface
- Sets up UI element references
- Configures toolbar menu actions

##### `ShowSettings()`

- Displays the settings panel
- Configures project settings UI
- Sets up data binding

##### `ShowAbout()`

- Displays the About panel
- Fills in the version from `PathScan.GetVersion()`
- Wires the Patreon and Documentation buttons to `Application.OpenURL()`
- Sets the toolbar menu to Scan and Settings

#### Setup Methods

##### `SetupBasePanel()`

- Queries and caches base panel UI elements
- Validates required elements exist
- Throws exceptions if elements are missing

##### `SetupMainElements()`

- Queries main panel UI elements
- Registers the Scan and Generate button click events
- Sets the Open in Editor toggle from `EditorPrefs` and saves it back when it changes
- Registers the dropdown value change callback
- Calls `SetupElementTreeView()`

##### `SetupElementTreeView()`

- Assigns `makeCell` and `bindCell` for the three columns of the element list (`selected`, `type`, `name`)
- The `selected` column is a `Toggle` that writes back to the matching entry in `scannedElements`

##### `SetupSettingsElements()`

- Queries settings panel UI elements
- Sets up data binding to ScriptableObject (search folders, project path, output folder, namespace, regenerate on save)
- Configures change event handlers

#### Core Functionality

##### `ScanButton_clicked()`

**Workflow**:
1. Updates status to "Scanning..."
2. Calls `PathScan.GetAssetPaths()` with the search folders from the project settings
3. Stores the full asset paths in `scannedPaths`
4. Updates status with results count
5. Populates dropdown with discovered files

##### `DropdownField_ValueChanged()`

- Handles UXML file selection
- Looks up the full asset path in `scannedPaths` by dropdown index
- Looks up an existing record with `Regenerator.Find()`
- Uses the recorded class name, or generates one (e.g., "MyPanel.uxml" -> "MyPanelUXML")
- Shows results container
- Calls `UXMLScan.ListElements()` and passes the result to `ShowElements()`, with the recorded exclusions if there are any

##### `ShowElements()`

- Stores the elements in `scannedElements` and marks them as selected unless they are in the exclusions
- Fills the element list and rebuilds it
- Updates status with the number of named elements

##### `GenerateButton_clicked()`

**Workflow**:
1. Collects the selected elements; stops with "No elements selected" if there are none
2. Calls `Regenerator.GenerateAndRecord()` with the project settings, the UXML path, the helper name and all scanned elements
3. That builds the source, writes it to the output folder, and records the class for auto-regeneration
4. If the Open in Editor toggle is ticked, opens the class with `AssetDatabase.OpenAsset()`
5. Logs each generator warning to the Console
6. Updates status with the generated file path

I/O and argument errors are caught, logged to the Console, and reported as "Generate failed, see console".

##### `PopulateDropdown()`

- Extracts filenames from full paths
- Adds "Select a file" placeholder
- Sets dropdown choices and default index

### State Management

The window maintains state through:
- **ProjectSettingSO**: Persistent settings across sessions
- **UI Element References**: Cached for performance
- **Project Path**: Resolved dynamically based on asset location
- **`scannedPaths`**: Asset paths from the last scan, in dropdown order
- **`scannedElements`**: Named elements of the selected file, including their selection state
- **`selectedPath`**: Asset path of the file chosen in the dropdown

### Initialization Sequence

### UI Element Query Pattern

All UI elements are queried using Unity's UI Toolkit query system:

This pattern ensures:
- Type safety
- Early error detection
- Clear error messages

### Event Registration

Events are registered programmatically:

### CSS Class Management

Dynamic CSS class manipulation for show/hide functionality:

### Data Binding

UI elements bind to ScriptableObject properties:

### Unused Methods

##### `ResetForm()`

Currently not called but prepared for form reset functionality.

---

**Next**: [Helper Utilities](Helper-Utilities.md)
