using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DADC.Clogs 
{ 
    public class PathScan
    {
        public static List<string> GetAssetPaths(string[] foldersToSearch = null)
        {
            List<string> paths = new List<string>();
            string[] searchInFolders;

            if (foldersToSearch == null) 
            {
                searchInFolders = new string[1] { "Assets" }; 
            }
            else
            {
                searchInFolders = foldersToSearch;
            }

            Debug.Log("Scan");
            string[] guids = AssetDatabase.FindAssets("t:VisualTreeAsset", searchInFolders);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                paths.Add(path);
            }
            return paths;
        }
        /// <summary>
        /// Returns the folder the tool is installed in, with a trailing slash.
        /// Works whether it was copied into Assets or installed as a package.
        /// </summary>
        public static string GetInstallPath()
        {
            const string windowScript = "Editor/" + nameof(CleverClogsWindow) + ".cs";

            string[] guids = AssetDatabase.FindAssets(nameof(CleverClogsWindow) + " t:MonoScript", new string[2] { "Assets", "Packages" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("/" + windowScript))
                {
                    return path.Substring(0, path.Length - windowScript.Length);
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the tool's version from its package.json, or null if it can't be read.
        /// </summary>
        public static string GetVersion()
        {
            string installPath = GetInstallPath();
            if (installPath == null) return null;

            UnityEditor.PackageManager.PackageInfo package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(installPath + "package.json");
            if (package != null) return package.version;

            // Copied under Assets the tool isn't a package, but its manifest is still there to read.
            TextAsset manifest = AssetDatabase.LoadAssetAtPath<TextAsset>(installPath + "package.json");
            if (manifest == null) return null;
            return JsonUtility.FromJson<Manifest>(manifest.text).version;
        }

        [System.Serializable]
        private class Manifest
        {
            public string version;
        }
    }
}