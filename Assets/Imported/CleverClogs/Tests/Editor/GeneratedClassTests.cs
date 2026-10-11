using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace DADC.Clogs.Tests
{
    /// <summary>
    /// Fixtures/SampleFixtureUXML.cs is generator output that is compiled with the tests.
    /// These tests keep it in step with the generator and check it against the real UXML.
    /// </summary>
    public class GeneratedClassTests
    {
        private VisualElement root;
        private SampleFixtureUXML ui;

        [SetUp]
        public void SetUp()
        {
            root = Fixtures.Instantiate("SampleFixture.uxml");
            ui = new SampleFixtureUXML(root);
        }

        [Test]
        public void CheckedInClassMatchesGeneratorOutput()
        {
            string expected = Generate.BuildSource(Fixtures.SampleClassName, Fixtures.SampleUxmlPath, Fixtures.SampleElements(), Fixtures.Namespace, out List<string> warnings);
            string actual = File.ReadAllText(Fixtures.PathOf(Fixtures.SampleClassName + ".cs"));

            Assert.That(Fixtures.NormalizeNewlines(actual), Is.EqualTo(Fixtures.NormalizeNewlines(expected)),
                "Generator output changed. Regenerate Fixtures/SampleFixtureUXML.cs and check it still compiles.");
            Assert.That(warnings, Has.Count.EqualTo(3), "Root clash, ok_button collision, duplicate StatusText");
        }

        [Test]
        public void ConstructorFindsEveryElement()
        {
            Assert.That(ui.Root, Is.SameAs(root));
            Assert.That(ui.Header, Is.Not.Null);
            Assert.That(ui.TitleLabel, Is.Not.Null);
            Assert.That(ui.RootElement, Is.Not.Null);
            Assert.That(ui.StatusText, Is.Not.Null);
            Assert.That(ui.OkButton, Is.Not.Null);
            Assert.That(ui.OkButtonElement, Is.Not.Null);
            Assert.That(ui._2ndLine, Is.Not.Null);
        }

        [Test]
        public void PropertiesPointAtTheNamedElements()
        {
            Assert.That(ui.TitleLabel.text, Is.EqualTo("Title"));
            Assert.That(ui.OkButton.text, Is.EqualTo("OK"));
            Assert.That(ui.OkButtonElement.text, Is.EqualTo("Cancel"));
            Assert.That(ui._2ndLine.text, Is.EqualTo("Second"));
            Assert.That(ui.RootElement.name, Is.EqualTo("Root"));
        }

        [Test]
        public void DuplicateNameResolvesToFirstMatch()
        {
            Assert.That(ui.StatusText.text, Is.EqualTo("Status"));
        }

        [Test]
        public void NameOfReturnsUxmlName()
        {
            Assert.That(SampleFixtureUXML.NameOf(SampleFixtureUXML.Element.OkButton), Is.EqualTo("ok-button"));
            Assert.That(SampleFixtureUXML.NameOf(SampleFixtureUXML.Element.OkButtonElement), Is.EqualTo("ok_button"));
            Assert.That(SampleFixtureUXML.NameOf(SampleFixtureUXML.Element._2ndLine), Is.EqualTo("2nd-line"));
            Assert.That(SampleFixtureUXML.NameOf(SampleFixtureUXML.Element.RootElement), Is.EqualTo("Root"));
        }

        [Test]
        public void EveryEnumValueHasANameAndAnElement()
        {
            foreach (SampleFixtureUXML.Element element in Enum.GetValues(typeof(SampleFixtureUXML.Element)))
            {
                string name = SampleFixtureUXML.NameOf(element);

                Assert.That(name, Is.Not.Empty, element.ToString());
                Assert.That(ui.Get(element), Is.Not.Null, element.ToString());
                Assert.That(ui.Get(element).name, Is.EqualTo(name), element.ToString());
            }
        }

        [Test]
        public void StaticQueryMatchesProperty()
        {
            Button queried = SampleFixtureUXML.Q<Button>(root, SampleFixtureUXML.Element.OkButton);

            Assert.That(queried, Is.SameAs(ui.OkButton));
        }

        [Test]
        public void TypedGetMatchesProperty()
        {
            Assert.That(ui.Get<Label>(SampleFixtureUXML.Element.TitleLabel), Is.SameAs(ui.TitleLabel));
        }

        [Test]
        public void TypedGetThrowsOnWrongType()
        {
            Assert.Throws<InvalidCastException>(() => ui.Get<Button>(SampleFixtureUXML.Element.TitleLabel));
        }

        [Test]
        public void GetThrowsOnUndefinedEnumValue()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ui.Get((SampleFixtureUXML.Element)999));
        }

        [Test]
        public void ConstructorThrowsOnNullRoot()
        {
            Assert.Throws<ArgumentNullException>(() => new SampleFixtureUXML(null));
        }

        [Test]
        public void PropertiesAreNullWhenTreeLacksTheElements()
        {
            SampleFixtureUXML empty = new SampleFixtureUXML(new VisualElement());

            Assert.That(empty.OkButton, Is.Null);
            Assert.That(empty.TitleLabel, Is.Null);
        }
    }
}
