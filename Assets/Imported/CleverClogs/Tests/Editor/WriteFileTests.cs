using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;

namespace DADC.Clogs.Tests
{
    public class WriteFileTests
    {
        private const string source = Generate.Header + "\n// test output\n";

        private string folder;

        [SetUp]
        public void SetUp()
        {
            folder = "Assets/ClogsWriteFileTests_" + Guid.NewGuid().ToString("N");
            // Holds back the import WriteFile asks for, so the test output never reaches the compiler.
            AssetDatabase.StartAssetEditing();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            if (File.Exists(folder + ".meta")) File.Delete(folder + ".meta");
            AssetDatabase.StopAssetEditing();
        }

        [Test]
        public void WritesSourceAndReturnsAssetPath()
        {
            string path = Generate.WriteFile(folder, "SampleUXML", source);

            Assert.That(path, Is.EqualTo(folder + "/SampleUXML.cs"));
            Assert.That(File.ReadAllText(path), Is.EqualTo(source));
        }

        [Test]
        public void WritesWithoutByteOrderMark()
        {
            string path = Generate.WriteFile(folder, "SampleUXML", source);

            Assert.That(File.ReadAllBytes(path)[0], Is.EqualTo((byte)'/'));
        }

        [Test]
        public void CreatesNestedFolders()
        {
            string path = Generate.WriteFile(folder + "/A/B", "SampleUXML", source);

            Assert.That(File.Exists(path), Is.True);
        }

        [Test]
        public void NormalizesFolderSeparators()
        {
            string path = Generate.WriteFile(folder.Replace('/', '\\') + "\\", "SampleUXML", source);

            Assert.That(path, Is.EqualTo(folder + "/SampleUXML.cs"));
        }

        [Test]
        public void SanitizesFileName()
        {
            string path = Generate.WriteFile(folder, "my panel-v2", source);

            Assert.That(path, Is.EqualTo(folder + "/MyPanelV2.cs"));
        }

        [Test]
        public void OverwritesAGeneratedFile()
        {
            Generate.WriteFile(folder, "SampleUXML", source);
            string path = Generate.WriteFile(folder, "SampleUXML", source + "// second\n");

            Assert.That(File.ReadAllText(path), Does.EndWith("// second\n"));
        }

        [Test]
        public void RefusesToOverwriteAFileItDidNotGenerate()
        {
            Directory.CreateDirectory(folder);
            string path = folder + "/SampleUXML.cs";
            File.WriteAllText(path, "// handwritten\n");

            Assert.Throws<IOException>(() => Generate.WriteFile(folder, "SampleUXML", source));
            Assert.That(File.ReadAllText(path), Is.EqualTo("// handwritten\n"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void ThrowsWhenFolderIsNotSet(string emptyFolder)
        {
            Assert.Throws<ArgumentException>(() => Generate.WriteFile(emptyFolder, "SampleUXML", source));
        }
    }
}
