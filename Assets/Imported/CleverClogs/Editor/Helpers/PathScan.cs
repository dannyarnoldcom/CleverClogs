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
    }
}