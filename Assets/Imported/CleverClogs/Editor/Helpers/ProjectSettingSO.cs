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
