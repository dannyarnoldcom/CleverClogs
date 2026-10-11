using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace DADC.Clogs.Tests
{
    public class PathScanTests
    {
        [Test]
        public void InstallPathPointsAtTheToolFolder()
        {
            string path = PathScan.GetInstallPath();

            Assert.That(path, Is.Not.Null);
            Assert.That(path, Does.EndWith("/"));
            Assert.That(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path + "Editor/BasePanel.uxml"), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<StyleSheet>(path + "Editor/CleverClogs.uss"), Is.Not.Null);
        }

        [Test]
        public void AssetPathsOnlyContainUxml()
        {
            List<string> paths = PathScan.GetAssetPaths(new[] { PathScan.GetInstallPath().TrimEnd('/') });

            Assert.That(paths, Is.Not.Empty);
            Assert.That(paths, Has.All.EndsWith(".uxml"));
            Assert.That(paths, Has.Some.EndsWith("/Tests/Editor/Fixtures/SampleFixture.uxml"));
        }

        [Test]
        public void SettingsAreOnlyLookedForUnderAssets()
        {
            foreach (ProjectSettingSO settings in ProjectSettingSO.FindAll())
            {
                Assert.That(AssetDatabase.GetAssetPath(settings), Does.StartWith("Assets/"));
            }
        }
    }
}
