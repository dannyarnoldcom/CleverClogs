using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace DADC.Clogs.Tests
{
    internal static class Fixtures
    {
        public const string Namespace = "DADC.Clogs.Tests";
        public const string SampleClassName = "SampleFixtureUXML";
        // Only written into the generated file, the tests locate fixtures with PathOf.
        public const string SampleUxmlPath = "Assets/Imported/CleverClogs/Tests/Editor/Fixtures/SampleFixture.uxml";

        /// <summary>
        /// The named elements of SampleFixture.uxml in document order, as UXMLScan should report them.
        /// </summary>
        public static List<Element> SampleElements()
        {
            return new List<Element>
            {
                new Element("header", "VisualElement", typeof(VisualElement)),
                new Element("title-label", "Label", typeof(Label)),
                new Element("Root", "VisualElement", typeof(VisualElement)),
                new Element("StatusText", "Label", typeof(Label)),
                new Element("ok-button", "Button", typeof(Button)),
                new Element("ok_button", "Button", typeof(Button)),
                new Element("2nd-line", "Label", typeof(Label)),
                new Element("StatusText", "Label", typeof(Label)),
            };
        }

        /// <summary>
        /// Finds a fixture by file name so the tests don't depend on where the plugin is installed.
        /// </summary>
        public static string PathOf(string fileName)
        {
            int dot = fileName.LastIndexOf('.');
            string name = dot < 0 ? fileName : fileName.Substring(0, dot);

            foreach (string guid in AssetDatabase.FindAssets(name))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("/Tests/Editor/Fixtures/" + fileName)) return path;
            }

            Assert.Fail("Fixture " + fileName + " not found");
            return null;
        }

        public static VisualElement Instantiate(string fileName)
        {
            VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PathOf(fileName));
            Assert.That(asset, Is.Not.Null, fileName + " did not import as a VisualTreeAsset");
            return asset.Instantiate();
        }

        public static string NormalizeNewlines(string text)
        {
            return text.Replace("\r\n", "\n");
        }
    }

    // Element types the generator can't name directly, and ones it has to qualify.
    internal class InternalButton : Button { }
    public class GenericLabel<T> : Label { }
    public class CustomElement : VisualElement { }
    public class Outer
    {
        public class NestedElement : VisualElement { }
    }
}
