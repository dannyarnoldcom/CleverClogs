using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DADC.Clogs.Tests
{
    public class BuildSourceTests
    {
        private const string className = "SampleUXML";
        private const string uxmlPath = "Assets/UI/Sample.uxml";

        private static Element E(string name, Type type)
        {
            return new Element(name, type == null ? null : type.Name, type);
        }

        private static string Build(out List<string> warnings, params Element[] elements)
        {
            return Generate.BuildSource(className, uxmlPath, elements, null, out warnings);
        }

        private static string Build(params Element[] elements)
        {
            return Build(out _, elements);
        }

        private static int Count(string source, string text)
        {
            return Regex.Matches(source, Regex.Escape(text)).Count;
        }

        [Test]
        public void StartsWithHeader()
        {
            Assert.That(Build(E("Title", typeof(Label))), Does.StartWith(Generate.Header));
        }

        [Test]
        public void EmitsEnumPropertyQueryAndCaseForEachElement()
        {
            string source = Build(E("submit-button", typeof(Button)));

            Assert.That(source, Does.Contain("        SubmitButton,"));
            Assert.That(source, Does.Contain("public Button SubmitButton { get; }"));
            Assert.That(source, Does.Contain("SubmitButton = root.Q<Button>(\"submit-button\");"));
            Assert.That(source, Does.Contain("case Element.SubmitButton: return SubmitButton;"));
        }

        [Test]
        public void NameTableKeepsOriginalNamesInOrder()
        {
            string source = Fixtures.NormalizeNewlines(Build(E("submit-button", typeof(Button)), E("Title", typeof(Label))));

            Assert.That(source, Does.Contain("        \"submit-button\",\n        \"Title\",\n"));
            Assert.That(source, Does.Contain("        SubmitButton,\n        Title,\n"));
        }

        [Test]
        public void WritesUxmlPathConstant()
        {
            Assert.That(Build(E("Title", typeof(Label))), Does.Contain("public const string UxmlPath = \"" + uxmlPath + "\";"));
        }

        [Test]
        public void SanitizesClassName()
        {
            string source = Generate.BuildSource("my panel-v2", uxmlPath, new[] { E("Title", typeof(Label)) }, null, out _);

            Assert.That(source, Does.Contain("public sealed class MyPanelV2"));
            Assert.That(source, Does.Contain("public MyPanelV2(VisualElement root)"));
        }

        [Test]
        public void NoElementsStillProducesAClass()
        {
            string source = Build(out List<string> warnings);

            Assert.That(source, Does.Contain("public sealed class " + className));
            Assert.That(source, Does.Contain("public enum Element"));
            Assert.That(warnings, Is.Empty);
        }

        [Test]
        public void IgnoresSelectedFlag()
        {
            Element element = E("Title", typeof(Label));
            element.selected = false;

            Assert.That(Build(element), Does.Contain("public Label Title { get; }"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void OmitsNamespaceWhenNotGiven(string namespaceName)
        {
            string source = Generate.BuildSource(className, uxmlPath, new[] { E("Title", typeof(Label)) }, namespaceName, out _);

            Assert.That(source, Does.Not.Contain("namespace"));
            Assert.That(source, Does.Contain("\npublic sealed class " + className));
        }

        [Test]
        public void WrapsInTrimmedNamespace()
        {
            string source = Generate.BuildSource(className, uxmlPath, new[] { E("Title", typeof(Label)) }, "  My.Game ", out _);

            Assert.That(Fixtures.NormalizeNewlines(source), Does.Contain("namespace My.Game\n{\n    public sealed class " + className));
        }

        [TestCase("Element")]
        [TestCase("UxmlPath")]
        [TestCase("Root")]
        [TestCase("NameOf")]
        [TestCase("Q")]
        [TestCase("Get")]
        [TestCase("Equals")]
        [TestCase("GetHashCode")]
        [TestCase("GetType")]
        [TestCase("ToString")]
        [TestCase(className)]
        public void RenamesElementThatClashesWithAMember(string name)
        {
            string source = Build(out List<string> warnings, E(name, typeof(Label)));

            Assert.That(source, Does.Contain("public Label " + name + "Element { get; }"));
            Assert.That(source, Does.Contain(name + "Element = root.Q<Label>(\"" + name + "\");"));
            Assert.That(warnings, Has.Count.EqualTo(1));
            Assert.That(warnings[0], Does.Contain(name + "Element"));
        }

        [Test]
        public void RenamesElementsThatProduceTheSameIdentifier()
        {
            string source = Build(out List<string> warnings,
                E("ok-button", typeof(Button)), E("ok_button", typeof(Button)), E("ok.button", typeof(Button)));

            Assert.That(source, Does.Contain("OkButton = root.Q<Button>(\"ok-button\");"));
            Assert.That(source, Does.Contain("OkButtonElement = root.Q<Button>(\"ok_button\");"));
            Assert.That(source, Does.Contain("OkButton2 = root.Q<Button>(\"ok.button\");"));
            Assert.That(warnings, Has.Count.EqualTo(2));
        }

        [Test]
        public void GeneratesDuplicateNameOnce()
        {
            string source = Build(out List<string> warnings, E("Title", typeof(Label)), E("Title", typeof(Button)));

            Assert.That(Count(source, "root.Q<"), Is.EqualTo(2), "one in the static Q helper, one for Title");
            Assert.That(source, Does.Contain("public Label Title { get; }"));
            Assert.That(warnings, Has.Count.EqualTo(1));
            Assert.That(warnings[0], Does.Contain("\"Title\""));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void SkipsElementsWithoutAName(string name)
        {
            string source = Build(out List<string> warnings, E(name, typeof(Label)), E("Title", typeof(Label)));

            Assert.That(Count(source, "{ get; }"), Is.EqualTo(2), "Root and Title");
            Assert.That(warnings, Is.Empty);
        }

        [Test]
        public void PrefixesNameStartingWithDigit()
        {
            Assert.That(Build(E("2nd-item", typeof(Toggle))), Does.Contain("_2ndItem = root.Q<Toggle>(\"2nd-item\");"));
        }

        [Test]
        public void EscapesQuotesAndBackslashesInNames()
        {
            Assert.That(Build(E("a\"b\\c", typeof(Label))), Does.Contain("ABC = root.Q<Label>(\"a\\\"b\\\\c\");"));
        }

        [Test]
        public void UsesShortNameForUIElementsTypes()
        {
            Assert.That(Build(E("Field", typeof(TextField))), Does.Contain("public TextField Field { get; }"));
        }

        [Test]
        public void QualifiesTypesOutsideUIElements()
        {
            Assert.That(Build(E("Custom", typeof(CustomElement))),
                Does.Contain("public global::" + Fixtures.Namespace + ".CustomElement Custom { get; }"));
        }

        [Test]
        public void QualifiesNestedTypesWithDots()
        {
            Assert.That(Build(E("Nested", typeof(Outer.NestedElement))),
                Does.Contain("public global::" + Fixtures.Namespace + ".Outer.NestedElement Nested { get; }"));
        }

        [Test]
        public void FallsBackToPublicBaseOfInternalType()
        {
            Assert.That(Build(E("Hidden", typeof(InternalButton))), Does.Contain("public Button Hidden { get; }"));
        }

        [Test]
        public void FallsBackToNonGenericBaseOfGenericType()
        {
            Assert.That(Build(E("Generic", typeof(GenericLabel<int>))), Does.Contain("public Label Generic { get; }"));
        }

        [Test]
        public void FallsBackToVisualElementWhenTypeIsUnknown()
        {
            Assert.That(Build(E("Unknown", null)), Does.Contain("public VisualElement Unknown { get; }"));
        }

        [Test]
        public void FallsBackToVisualElementWhenTypeIsNotAnElement()
        {
            Assert.That(Build(E("Wrong", typeof(string))), Does.Contain("public VisualElement Wrong { get; }"));
        }

        [Test]
        public void WrapsFileWhenAnElementTypeIsEditorOnly()
        {
            string source = Build(E("Title", typeof(Label)), E("Picker", typeof(ObjectField)));

            Assert.That(source, Does.Contain("#if UNITY_EDITOR"));
            Assert.That(source.TrimEnd(), Does.EndWith("#endif"));
            Assert.That(source, Does.Contain("public global::UnityEditor.UIElements.ObjectField Picker { get; }"));
        }

        [Test]
        public void DoesNotWrapFileWhenAllTypesAreRuntime()
        {
            string source = Build(E("Title", typeof(Label)), E("Ok", typeof(Button)));

            Assert.That(source, Does.Not.Contain("#if"));
            Assert.That(source, Does.Not.Contain("#endif"));
        }
    }
}
