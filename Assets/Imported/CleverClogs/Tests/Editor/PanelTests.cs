using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DADC.Clogs.Tests
{
    /// <summary>
    /// The tool's own UXML has to resolve its templates wherever the tool is installed.
    /// </summary>
    public class PanelTests
    {
        private static VisualElement Load(string panel)
        {
            VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PathScan.GetInstallPath() + "Editor/" + panel);
            Assert.That(asset, Is.Not.Null, panel + " not found");
            return asset.Instantiate();
        }

        [Test]
        public void BasePanelHasTheElementsTheWindowLooksUp()
        {
            VisualElement root = Load("BasePanel.uxml");

            Assert.That(root.Q<ToolbarMenu>("ToolbarMenu"), Is.Not.Null);
            Assert.That(root.Q<VisualElement>("BodyContainer"), Is.Not.Null);
            Assert.That(root.Q<Label>("StatusText"), Is.Not.Null);
        }

        [Test]
        public void MainPanelHasTheElementsTheWindowLooksUp()
        {
            VisualElement root = Load("CleverClogs.uxml");

            Assert.That(root.Q<VisualElement>("DropdownContainer"), Is.Not.Null);
            Assert.That(root.Q<DropdownField>("PathsDropdown"), Is.Not.Null);
            Assert.That(root.Q<VisualElement>("ResultsContainer"), Is.Not.Null);
            Assert.That(root.Q<TextField>("HelperNameText"), Is.Not.Null);
            Assert.That(root.Q<Button>("ScanButton"), Is.Not.Null);
            Assert.That(root.Q<Button>("GenerateButton"), Is.Not.Null);
            Assert.That(root.Q<Toggle>("OpenInEditorToggle"), Is.Not.Null);
            Assert.That(root.Q<Toggle>("OpenInEditorToggle").text, Is.EqualTo("Open in Editor"));
        }

        [Test]
        public void MainPanelResolvesItsElementListTemplate()
        {
            MultiColumnTreeView list = Load("CleverClogs.uxml").Q<MultiColumnTreeView>();

            Assert.That(list, Is.Not.Null);
            Assert.That(list.columns["selected"], Is.Not.Null);
            Assert.That(list.columns["type"], Is.Not.Null);
            Assert.That(list.columns["name"], Is.Not.Null);
        }

        [Test]
        public void AboutPanelHasTheElementsTheWindowLooksUp()
        {
            VisualElement root = Load("AboutPanel.uxml");

            Assert.That(root.Q<Label>("VersionText"), Is.Not.Null);
            Assert.That(root.Q<Button>("PatreonButton"), Is.Not.Null);
            Assert.That(root.Q<Button>("PatreonButton").text, Is.EqualTo("Join my Patreon for support"));
            Assert.That(root.Q<Button>("DocumentationButton"), Is.Not.Null);
        }

        [Test]
        public void SettingsPanelHasTheElementsTheWindowLooksUp()
        {
            VisualElement root = Load("SettingsPanel.uxml");

            Assert.That(root.Q<ObjectField>("ProjectSettingsSO"), Is.Not.Null);
            Assert.That(root.Q<TextField>("ProjectPathTextField"), Is.Not.Null);
            Assert.That(root.Q<TextField>("OutputFolderTextField"), Is.Not.Null);
            Assert.That(root.Q<TextField>("NamespaceTextField"), Is.Not.Null);
            Assert.That(root.Q<Toggle>("AutoRegenerateToggle"), Is.Not.Null);
        }

        [Test]
        public void SettingsPanelShowsTheSearchFoldersOfABoundSettingsAsset()
        {
            VisualElement root = Load("SettingsPanel.uxml");
            PropertyField folders = root.Q<PropertyField>("SearchFolders");
            ProjectSettingSO settings = ScriptableObject.CreateInstance<ProjectSettingSO>();

            Assert.That(folders, Is.Not.Null);
            Assert.That(folders.bindingPath, Is.EqualTo("searchFolders"));
            Assert.That(new SerializedObject(settings).FindProperty(folders.bindingPath).isArray, Is.True);

            Object.DestroyImmediate(settings);
        }
    }
}
