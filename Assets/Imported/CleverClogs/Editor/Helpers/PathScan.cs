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
        public static string GetInstallPath(string projectName)
        {
            string[] guids = AssetDatabase.FindAssets("t:VisualTreeAsset", new string[1] { "Assets" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains(projectName))
                {
                    return path.Substring(0, path.IndexOf(projectName) + projectName.Length)+"/";
                }
            }
            return null;
        }
    }
}