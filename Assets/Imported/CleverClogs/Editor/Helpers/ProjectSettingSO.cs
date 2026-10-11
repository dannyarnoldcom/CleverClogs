using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace DADC.Clogs
{
    
    public class ProjectSettingSO : ScriptableObject
    {
        public string projectPath;
        public string[] searchFolders = { "Assets" };
        public string outputFolder = "Assets/CleverClogsGenerated";
        public string generatedNamespace = "";
        public bool autoRegenerate = true;
        public List<GeneratedHelper> generated = new List<GeneratedHelper>();

        // Settings live in the project, never in the tool's own folder, which is read-only when installed as a package.
        public const string DefaultPath = "Assets/CleverClogsSettings.asset";

        /// <summary>
        /// Returns every settings asset under Assets.
        /// </summary>
        public static List<ProjectSettingSO> FindAll()
        {
            List<ProjectSettingSO> found = new List<ProjectSettingSO>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(ProjectSettingSO), new string[1] { "Assets" }))
            {
                ProjectSettingSO settings = AssetDatabase.LoadAssetAtPath<ProjectSettingSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (settings != null) found.Add(settings);
            }
            return found;
        }

        /// <summary>
        /// Returns the project's settings asset, creating one at DefaultPath if there is none.
        /// </summary>
        public static ProjectSettingSO LoadOrCreate()
        {
            List<ProjectSettingSO> found = FindAll();
            if (found.Count > 0) return found[0];

            Debug.Log("Clever Clogs settings not found, creating " + DefaultPath);
            ProjectSettingSO settings = CreateInstance<ProjectSettingSO>();
            AssetDatabase.CreateAsset(settings, DefaultPath);
            AssetDatabase.SaveAssets();
            return settings;
        }

        
        #if UNITY_EDITOR
                void OnValidate()
                {
                    var path = AssetDatabase.GetAssetPath(this);
                    if (path != projectPath)
                    {
                        int lastSlash = path.LastIndexOf('/');
                        projectPath = path[..(lastSlash + 1)];
                        EditorUtility.SetDirty(this);
                    }
                }
        #endif
    }
        
}
