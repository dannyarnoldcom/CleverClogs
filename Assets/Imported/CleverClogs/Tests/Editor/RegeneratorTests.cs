using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DADC.Clogs.Tests
{
    public class RegeneratorTests
    {
        private const string className = "RegeneratedUXML";
        private const string stale = Generate.Header + "\n// stale\n";

        private string folder;
        private string uxmlPath;
        private ProjectSettingSO settings;

        [SetUp]
        public void SetUp()
        {
            folder = "Assets/ClogsRegeneratorTests_" + Guid.NewGuid().ToString("N");
            uxmlPath = Fixtures.PathOf("SampleFixture.uxml");
            settings = ScriptableObject.CreateInstance<ProjectSettingSO>();
            settings.outputFolder = folder;
            settings.generatedNamespace = "Sample.Space";
            // Holds back the import WriteFile asks for, so the test output never reaches the compiler.
            AssetDatabase.StartAssetEditing();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            if (File.Exists(folder + ".meta")) File.Delete(folder + ".meta");
            AssetDatabase.StopAssetEditing();
            UnityEngine.Object.DestroyImmediate(settings);
        }

        private List<Element> Scan(params string[] unselected)
        {
            List<Element> elements = UXMLScan.ListElements(uxmlPath);
            for (int i = 0; i < elements.Count; i++)
            {
                Element element = elements[i];
                element.selected = Array.IndexOf(unselected, element.name) < 0;
                elements[i] = element;
            }
            return elements;
        }

        private GeneratedHelper GenerateHelper(string name = className, params string[] unselected)
        {
            string path = Regenerator.GenerateAndRecord(settings, uxmlPath, name, Scan(unselected), out _);
            return settings.generated.Find((entry) => entry.outputPath == path);
        }

        [Test]
        public void GenerateAndRecordWritesTheClass()
        {
            string path = Regenerator.GenerateAndRecord(settings, uxmlPath, className, Scan(), out List<string> warnings);

            Assert.That(path, Is.EqualTo(folder + "/" + className + ".cs"));
            Assert.That(File.ReadAllText(path), Does.Contain("public sealed class " + className));
            Assert.That(warnings, Has.Count.EqualTo(3));
        }

        [Test]
        public void GenerateAndRecordRemembersHowTheClassWasMade()
        {
            GeneratedHelper helper = GenerateHelper();

            Assert.That(settings.generated, Has.Count.EqualTo(1));
            Assert.That(helper.uxmlGuid, Is.EqualTo(AssetDatabase.AssetPathToGUID(uxmlPath)));
            Assert.That(helper.className, Is.EqualTo(className));
            Assert.That(helper.namespaceName, Is.EqualTo("Sample.Space"));
            Assert.That(helper.excludedElements, Is.Empty);
        }

        [Test]
        public void GenerateAndRecordStoresSanitizedClassName()
        {
            GeneratedHelper helper = GenerateHelper("my panel-v2");

            Assert.That(helper.className, Is.EqualTo("MyPanelV2"));
            Assert.That(helper.outputPath, Is.EqualTo(folder + "/MyPanelV2.cs"));
        }

        [Test]
        public void GenerateAndRecordLeavesOutUnselectedElements()
        {
            GeneratedHelper helper = GenerateHelper(className, "header", "ok-button");

            Assert.That(helper.excludedElements, Is.EqualTo(new[] { "header", "ok-button" }));
            string source = File.ReadAllText(helper.outputPath);
            Assert.That(source, Does.Not.Contain("\"header\""));
            Assert.That(source, Does.Not.Contain("\"ok-button\""));
            Assert.That(source, Does.Contain("\"title-label\""));
        }

        [Test]
        public void GeneratingTheSameClassAgainKeepsOneRecord()
        {
            GenerateHelper();
            GeneratedHelper helper = GenerateHelper(className, "header");

            Assert.That(settings.generated, Has.Count.EqualTo(1));
            Assert.That(helper.excludedElements, Is.EqualTo(new[] { "header" }));
        }

        [Test]
        public void GeneratingUnderAnotherNameAddsARecord()
        {
            GenerateHelper();
            GenerateHelper("OtherUXML");

            Assert.That(settings.generated, Has.Count.EqualTo(2));
        }

        [Test]
        public void FindReturnsTheRecordForAUxmlFile()
        {
            GeneratedHelper helper = GenerateHelper();

            Assert.That(Regenerator.Find(settings, uxmlPath), Is.SameAs(helper));
            Assert.That(Regenerator.Find(settings, Fixtures.PathOf("TemplateHost.uxml")), Is.Null);
            Assert.That(Regenerator.Find(settings, "Assets/DoesNotExist.uxml"), Is.Null);
        }

        [Test]
        public void RegenerateLeavesAnUpToDateFileAlone()
        {
            GeneratedHelper helper = GenerateHelper();
            DateTime written = new DateTime(2020, 1, 1);
            File.SetLastWriteTimeUtc(helper.outputPath, written);

            Assert.That(Regenerator.Regenerate(helper), Is.EqualTo(RegenerateResult.Unchanged));
            Assert.That(File.GetLastWriteTimeUtc(helper.outputPath), Is.EqualTo(written));
        }

        [Test]
        public void RegenerateIgnoresLineEndingDifferences()
        {
            GeneratedHelper helper = GenerateHelper();
            File.WriteAllText(helper.outputPath, File.ReadAllText(helper.outputPath).Replace("\r\n", "\n"));

            Assert.That(Regenerator.Regenerate(helper), Is.EqualTo(RegenerateResult.Unchanged));
        }

        [Test]
        public void RegenerateRewritesAStaleFile()
        {
            GeneratedHelper helper = GenerateHelper();
            string original = File.ReadAllText(helper.outputPath);
            File.WriteAllText(helper.outputPath, stale);

            Assert.That(Regenerator.Regenerate(helper), Is.EqualTo(RegenerateResult.Updated));
            Assert.That(File.ReadAllText(helper.outputPath), Is.EqualTo(original));
        }

        [Test]
        public void RegenerateKeepsExclusions()
        {
            GeneratedHelper helper = GenerateHelper(className, "header");
            File.WriteAllText(helper.outputPath, stale);

            Regenerator.Regenerate(helper);

            string source = File.ReadAllText(helper.outputPath);
            Assert.That(source, Does.Not.Contain("\"header\""));
            Assert.That(source, Does.Contain("\"title-label\""));
        }

        [Test]
        public void RegenerateUsesTheRecordedNamespace()
        {
            GeneratedHelper helper = GenerateHelper();
            settings.generatedNamespace = "Changed.Later";
            File.WriteAllText(helper.outputPath, stale);

            Regenerator.Regenerate(helper);

            Assert.That(File.ReadAllText(helper.outputPath), Does.Contain("namespace Sample.Space"));
        }

        [Test]
        public void RegenerateReportsADeletedFile()
        {
            GeneratedHelper helper = GenerateHelper();
            File.Delete(helper.outputPath);

            Assert.That(Regenerator.Regenerate(helper), Is.EqualTo(RegenerateResult.Removed));
            Assert.That(File.Exists(helper.outputPath), Is.False);
        }

        [Test]
        public void RegenerateSkipsWhenTheUxmlIsGone()
        {
            GeneratedHelper helper = GenerateHelper();
            helper.uxmlGuid = "00000000000000000000000000000000";
            File.WriteAllText(helper.outputPath, stale);

            Assert.That(Regenerator.Regenerate(helper), Is.EqualTo(RegenerateResult.Skipped));
            Assert.That(File.ReadAllText(helper.outputPath), Is.EqualTo(stale));
        }

        [Test]
        public void RegenerateWillNotOverwriteAHandwrittenFile()
        {
            GeneratedHelper helper = GenerateHelper();
            File.WriteAllText(helper.outputPath, "// handwritten\n");
            LogAssert.Expect(LogType.Warning, new Regex("not generated by Clever Clogs"));

            Assert.That(Regenerator.Regenerate(helper), Is.EqualTo(RegenerateResult.Skipped));
            Assert.That(File.ReadAllText(helper.outputPath), Is.EqualTo("// handwritten\n"));
        }

        [Test]
        public void RegenerateAllUpdatesStaleFilesAndForgetsDeletedOnes()
        {
            GeneratedHelper staleHelper = GenerateHelper("StaleUXML");
            GeneratedHelper deletedHelper = GenerateHelper("DeletedUXML");
            GenerateHelper("CurrentUXML");
            File.WriteAllText(staleHelper.outputPath, stale);
            File.Delete(deletedHelper.outputPath);

            Assert.That(Regenerator.RegenerateAll(settings), Is.EqualTo(1));
            Assert.That(settings.generated, Has.Count.EqualTo(2));
            Assert.That(settings.generated, Has.No.Member(deletedHelper));
            Assert.That(File.ReadAllText(staleHelper.outputPath), Does.Contain("public sealed class StaleUXML"));
        }

        [Test]
        public void RegenerateAllDoesNothingWhenSwitchedOff()
        {
            GeneratedHelper helper = GenerateHelper();
            File.WriteAllText(helper.outputPath, stale);
            settings.autoRegenerate = false;

            Assert.That(Regenerator.RegenerateAll(settings), Is.EqualTo(0));
            Assert.That(File.ReadAllText(helper.outputPath), Is.EqualTo(stale));
        }

        [Test]
        public void OutputMovedFollowsTheGeneratedFile()
        {
            GeneratedHelper helper = GenerateHelper();
            string from = helper.outputPath;

            Assert.That(Regenerator.OutputMoved(settings, from, "Assets/Elsewhere/" + className + ".cs"), Is.True);
            Assert.That(helper.outputPath, Is.EqualTo("Assets/Elsewhere/" + className + ".cs"));
            Assert.That(Regenerator.OutputMoved(settings, from, "Assets/Again.cs"), Is.False);
        }

        [Test]
        public void SettingsStartWithAutoRegenerateOnAndNoRecords()
        {
            Assert.That(settings.autoRegenerate, Is.True);
            Assert.That(settings.generated, Is.Empty);
        }
    }
}
