using NUnit.Framework;

namespace DADC.Clogs.Tests
{
    public class ToIdentifierTests
    {
        [TestCase("ScanButton", "ScanButton")]
        [TestCase("scanButton", "ScanButton")]
        [TestCase("submit-button", "SubmitButton")]
        [TestCase("submit_button", "SubmitButton")]
        [TestCase("unity-text-input", "UnityTextInput")]
        [TestCase("my panel v2", "MyPanelV2")]
        [TestCase("a_b.c", "ABC")]
        [TestCase("--leading-and-trailing--", "LeadingAndTrailing")]
        [TestCase("item2", "Item2")]
        public void ConvertsToPascalCase(string name, string expected)
        {
            Assert.That(Generate.ToIdentifier(name), Is.EqualTo(expected));
        }

        [TestCase("2nd-item", "_2ndItem")]
        [TestCase("42", "_42")]
        public void PrefixesLeadingDigit(string name, string expected)
        {
            Assert.That(Generate.ToIdentifier(name), Is.EqualTo(expected));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("--")]
        [TestCase("   ")]
        public void ReturnsUnderscoreWhenNothingUsable(string name)
        {
            Assert.That(Generate.ToIdentifier(name), Is.EqualTo("_"));
        }

        [Test]
        public void LeavesCaseAfterFirstLetterAlone()
        {
            Assert.That(Generate.ToIdentifier("projectSettingsSO"), Is.EqualTo("ProjectSettingsSO"));
        }
    }
}
