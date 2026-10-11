using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace DADC.Clogs.Tests
{
    public class UXMLScanTests
    {
        [Test]
        public void ListsNamedElementsInDocumentOrder()
        {
            List<Element> elements = UXMLScan.ListElements(Fixtures.PathOf("SampleFixture.uxml"));
            List<Element> expected = Fixtures.SampleElements();

            Assert.That(elements.Select((element) => element.name), Is.EqualTo(expected.Select((element) => element.name)));
            Assert.That(elements.Select((element) => element.elementType), Is.EqualTo(expected.Select((element) => element.elementType)));
            Assert.That(elements.Select((element) => element.type), Is.EqualTo(expected.Select((element) => element.type)));
        }

        [Test]
        public void ElementsStartUnselected()
        {
            List<Element> elements = UXMLScan.ListElements(Fixtures.PathOf("SampleFixture.uxml"));

            Assert.That(elements.Any((element) => element.selected), Is.False);
        }

        [Test]
        public void IncludesElementsInsideTemplateInstances()
        {
            List<Element> elements = UXMLScan.ListElements(Fixtures.PathOf("TemplateHost.uxml"));
            Dictionary<string, System.Type> types = elements.ToDictionary((element) => element.name, (element) => element.elementType);

            Assert.That(types.Keys, Is.EquivalentTo(new[] { "outer-label", "child-instance", "inner-label" }));
            Assert.That(types["outer-label"], Is.EqualTo(typeof(Label)));
            Assert.That(types["child-instance"], Is.EqualTo(typeof(TemplateContainer)));
            Assert.That(types["inner-label"], Is.EqualTo(typeof(Label)));
        }

        [Test]
        public void ReturnsEmptyListForMissingAsset()
        {
            Assert.That(UXMLScan.ListElements("Assets/DoesNotExist.uxml"), Is.Empty);
        }

        [Test]
        public void ReturnsEmptyListForAssetThatIsNotUxml()
        {
            Assert.That(UXMLScan.ListElements(Fixtures.PathOf(Fixtures.SampleClassName + ".cs")), Is.Empty);
        }

        [Test]
        public void ScanResultGeneratesTheCheckedInClass()
        {
            List<Element> elements = UXMLScan.ListElements(Fixtures.PathOf("SampleFixture.uxml"));
            string fromScan = Generate.BuildSource(Fixtures.SampleClassName, Fixtures.SampleUxmlPath, elements, Fixtures.Namespace, out _);
            string fromList = Generate.BuildSource(Fixtures.SampleClassName, Fixtures.SampleUxmlPath, Fixtures.SampleElements(), Fixtures.Namespace, out _);

            Assert.That(fromScan, Is.EqualTo(fromList));
        }
    }
}
