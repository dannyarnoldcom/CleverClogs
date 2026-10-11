using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DADC.Clogs
{
    public class CleverClogsWindow : EditorWindow
    {
        private ProjectSettingSO projectSettings;
        private string projectPath;
        private const string projectName = "CleverClogs";
        private string assetPath;
        private string editorPath;
        private VisualElement root;
        private VisualElement dropDownContainer;
        private VisualElement resultsContainer;
        private Button scanButton;
        private DropdownField dropdownField;
        private Label statusText;
        private TextField helperNameText;
        private ToolbarMenu toolbarMenu;
        private VisualElement bodyContainer;
        private ObjectField projectSettingsSO;
        private TextField projectPathTextField;
        private Foldout searchFolders;
        private TextField outputFolderTextField;
        private TextField namespaceTextField;
        private Button generateButton;
        private MultiColumnTreeView elementTreeView;
        private List<string> scannedPaths = new List<string>();
        private List<Element> scannedElements = new List<Element>();
        private string selectedPath;
        private string status;
        private void Awake()
        {
            assetPath = PathScan.GetInstallPath(projectName);
            editorPath = assetPath + "Editor/";
      
            if (projectSettings == null)
            {
                projectSettings = AssetDatabase.LoadAssetAtPath<ProjectSettingSO>(assetPath + "DefaultProjectSetting.asset");
              
                if (projectSettings == null)
                {
                    Debug.Log("ProjectSettings not found, creating new one");
                    projectSettings = ScriptableObject.CreateInstance<ProjectSettingSO>();
                    AssetDatabase.CreateAsset(projectSettings, assetPath + "DefaultProjectSetting.asset");
                    AssetDatabase.SaveAssets();
                }
                projectPath = projectSettings.projectPath;
            }
        }
        private void SetupBasePanel(VisualElement root)
        {
            toolbarMenu = root.Q<ToolbarMenu>("ToolbarMenu");
            if (toolbarMenu == null) throw new System.Exception("ToolbarMenu not found");
            bodyContainer = root.Q<VisualElement>("BodyContainer");
            if (bodyContainer == null) throw new System.Exception("BodyContainer not found");
            statusText = root.Q<Label>("StatusText");
            if (statusText == null) throw new System.Exception("StatusText not found");
        }
        private void SetupMainElements(VisualElement root)
        {
            dropDownContainer = root.Q<VisualElement>("DropdownContainer");
            if (dropDownContainer == null) throw new System.Exception("DropdownContainer not found");
            dropdownField = root.Q<DropdownField>("PathsDropdown");
            if (dropdownField == null) throw new System.Exception("DropdownField not found");
            resultsContainer = root.Q<VisualElement>("ResultsContainer");
            if (resultsContainer == null) throw new System.Exception("ResultsContainer not found");
            helperNameText = root.Q<TextField>("HelperNameText");
            if (helperNameText == null) throw new System.Exception("HelperNameText not found");
            scanButton = root.Q<Button>("ScanButton");
            if(scanButton == null) throw new System.Exception("ScanButton not found");
            scanButton.clicked += ScanButton_clicked;
            generateButton = root.Q<Button>("GenerateButton");
            if (generateButton == null) throw new System.Exception("GenerateButton not found");
            generateButton.clicked += GenerateButton_clicked;
            elementTreeView = root.Q<MultiColumnTreeView>();
            if (elementTreeView == null) throw new System.Exception("MultiColumnTreeView not found");
            dropdownField.RegisterValueChangedCallback(DropdownField_ValueChanged);
            SetupElementTreeView();
        }
        private void SetupElementTreeView()
        {
            elementTreeView.columns["selected"].makeCell = () => new Toggle();
            elementTreeView.columns["selected"].bindCell = (cell, index) =>
            {
                Toggle toggle = (Toggle)cell;
                int item = elementTreeView.GetItemDataForIndex<int>(index);
                toggle.SetValueWithoutNotify(scannedElements[item].selected);
                // userData holds the callback so a recycled cell doesn't keep toggling its previous element
                if (toggle.userData is EventCallback<ChangeEvent<bool>> previous) toggle.UnregisterValueChangedCallback(previous);
                EventCallback<ChangeEvent<bool>> callback = (evt) =>
                {
                    Element element = scannedElements[item];
                    element.selected = evt.newValue;
                    scannedElements[item] = element;
                };
                toggle.userData = callback;
                toggle.RegisterValueChangedCallback(callback);
            };
            elementTreeView.columns["type"].makeCell = () => new Label();
            elementTreeView.columns["type"].bindCell = (cell, index) =>
            {
                ((Label)cell).text = scannedElements[elementTreeView.GetItemDataForIndex<int>(index)].type;
            };
            elementTreeView.columns["name"].makeCell = () => new Label();
            elementTreeView.columns["name"].bindCell = (cell, index) =>
            {
                ((Label)cell).text = scannedElements[elementTreeView.GetItemDataForIndex<int>(index)].name;
            };
        }
        private void SetupSettingsElements()
        {
            searchFolders = root.Q<Foldout>("SearchFolders");
            if (searchFolders == null) throw new System.Exception("SearchFolders not found");

            projectPathTextField = root.Q<TextField>("ProjectPathTextField");
            if (projectPathTextField == null) throw new System.Exception("ProjectPathTextField not found");

            projectSettingsSO = root.Q<ObjectField>("ProjectSettingsSO");
            if (projectSettingsSO == null) throw new System.Exception("ProjectSettingsSO not found");

            outputFolderTextField = root.Q<TextField>("OutputFolderTextField");
            if (outputFolderTextField == null) throw new System.Exception("OutputFolderTextField not found");

            namespaceTextField = root.Q<TextField>("NamespaceTextField");
            if (namespaceTextField == null) throw new System.Exception("NamespaceTextField not found");

            projectSettingsSO.objectType = typeof(ProjectSettingSO);
            projectSettingsSO.value = projectSettings;
            projectSettingsSO.RegisterCallback<ChangeEvent<UnityEngine.Object>>((evt) =>
            {
                projectSettings = (ProjectSettingSO)evt.newValue;
                projectPathTextField.bindingPath = "projectPath";
                BindFields();
            });

            BindFields();            
        }

        private void BindFields()
        {
            searchFolders.bindingPath = "searchFolders";
            searchFolders.Bind(new SerializedObject(projectSettings));

            projectPathTextField.bindingPath = "projectPath";
            projectPathTextField.Bind(new SerializedObject(projectSettings));

            outputFolderTextField.bindingPath = "outputFolder";
            outputFolderTextField.Bind(new SerializedObject(projectSettings));

            namespaceTextField.bindingPath = "generatedNamespace";
            namespaceTextField.Bind(new SerializedObject(projectSettings));

        }


        [MenuItem("DADCTools/Clever Clogs")]
        public static void ShowWindow()
        {
            CleverClogsWindow win = GetWindow<CleverClogsWindow>();
            win.titleContent = new GUIContent("Clever Clogs");
            win.minSize = new Vector2(380, 450);
        }
        public void CreateGUI()
        {
            root = rootVisualElement;
            loadPanel(root, editorPath + "BasePanel.uxml", editorPath + "CleverClogs.uss");
            SetupBasePanel(root);
            ShowMainPanel(bodyContainer);
        }
       
        private void ShowSettings(VisualElement root)
        {
            loadPanel(root, editorPath + "SettingsPanel.uxml", editorPath + "CleverClogs.uss");
            SetupSettingsElements();
            toolbarMenu.menu.ClearItems();
            toolbarMenu.menu.AppendAction("Scan", (a) => { ShowMainPanel(bodyContainer); });
            toolbarMenu.menu.AppendAction("About", (a) => { ShowAboutPopup(); });
        }
        private void ShowMainPanel(VisualElement root)
        {
            loadPanel(root, editorPath + "CleverClogs.uxml", editorPath + "CleverClogs.uss");
            SetupMainElements(root);
            toolbarMenu.menu.ClearItems();
            toolbarMenu.menu.AppendAction("Settings", (a) => { ShowSettings(bodyContainer); });
            toolbarMenu.menu.AppendAction("About", (a) => { ShowAboutPopup(); });
        }
        private void loadPanel(VisualElement root,string uxml,string uss)
        {
            root.Clear();
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxml);
            root.Add(visualTree.Instantiate());
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(uss);
            root.styleSheets.Add(styleSheet);
        }
        private void ScanButton_clicked()
        {
            statusText.text = "Scanning...";
            string[] folders = projectSettings.searchFolders;
            List<string> assetPaths = PathScan.GetAssetPaths(folders != null && folders.Length > 0 ? folders : null);
            scannedPaths = assetPaths ?? new List<string>();
            if (assetPaths != null && assetPaths.Count > 0)
            {
                statusText.text = "Found " + assetPaths.Count + " assets";
                PopulateDropdown(assetPaths);
                dropDownContainer.RemoveFromClassList("hidden");
            }
            else
            {
                statusText.text = "No assets found";
            }
        }

        private void DropdownField_ValueChanged(ChangeEvent<string> evt)
        {
            int index = dropdownField.index - 1;
            if(index < 0 || index >= scannedPaths.Count)
            {
                return;
            }
            resultsContainer.RemoveFromClassList("hidden");
            string choice = evt.newValue;
            selectedPath = scannedPaths[index];
            helperNameText.value =  choice.Replace(".uxml", "UXML");
            ShowElements(UXMLScan.ListElements(selectedPath));
        }

        private void ShowElements(List<Element> elements)
        {
            scannedElements = elements;
            List<TreeViewItemData<int>> items = new List<TreeViewItemData<int>>();
            for (int i = 0; i < scannedElements.Count; i++)
            {
                Element element = scannedElements[i];
                element.selected = true;
                scannedElements[i] = element;
                items.Add(new TreeViewItemData<int>(i, i));
            }
            elementTreeView.SetRootItems(items);
            elementTreeView.Rebuild();
            statusText.text = "Found " + scannedElements.Count + " named elements";
        }

        private void GenerateButton_clicked()
        {
            List<Element> selected = scannedElements.FindAll((element) => element.selected);
            if (selectedPath == null || selected.Count == 0)
            {
                statusText.text = "No elements selected";
                return;
            }

            try
            {
                string source = Generate.BuildSource(helperNameText.value, selectedPath, selected, projectSettings.generatedNamespace, out List<string> warnings);
                string path = Generate.WriteFile(projectSettings.outputFolder, helperNameText.value, source);
                foreach (string warning in warnings) Debug.LogWarning(projectName + ": " + warning);
                statusText.text = "Generated " + path;
            }
            catch (System.Exception e) when (e is System.IO.IOException || e is System.ArgumentException || e is System.UnauthorizedAccessException)
            {
                statusText.text = "Generate failed, see console";
                Debug.LogError(projectName + ": " + e.Message);
            }
        }

        private string GetFilename(string path)
        {
            int lastSlash = path.LastIndexOf("/");
            return path.Substring(lastSlash +1);
        }
        private void PopulateDropdown(List<string> assetPaths)
        {
            List<string> filenames = new List<string>();
            filenames.Add("Select a file");
            foreach (string path in assetPaths)
            {
                filenames.Add(GetFilename(path));
            }
            
            dropdownField.choices = filenames;
            dropdownField.index = 0;
        }
        private void ResetForm()
        {
            statusText.text = "";
            dropdownField.choices = new List<string>();
            dropDownContainer.AddToClassList("hidden");
            resultsContainer.AddToClassList("hidden");
            helperNameText.value = "";  
            scanButton.SetEnabled(true);
        }

        private void ShowAboutPopup()
        {
            Debug.Log("About");
        }  
    }
    
}
