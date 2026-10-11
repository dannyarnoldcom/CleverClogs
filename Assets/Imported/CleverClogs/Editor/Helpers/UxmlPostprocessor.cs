using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DADC.Clogs
{
    /// <summary>
    /// Regenerates recorded helper classes when UXML is saved, and after each script reload.
    /// </summary>
    internal class UxmlPostprocessor : AssetPostprocessor
    {
        private static bool pending;

        [InitializeOnLoadMethod]
        private static void OnScriptsReloaded()
        {
            // Catches changes a UXML import can't see, such as a custom element class being renamed.
            Schedule();
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            for (int i = 0; i < movedAssets.Length; i++)
            {
                if (!movedFromAssetPaths[i].EndsWith(".cs")) continue;
                foreach (ProjectSettingSO settings in FindSettings())
                {
                    Regenerator.OutputMoved(settings, movedFromAssetPaths[i], movedAssets[i]);
                }
            }

            if (ContainsUxml(importedAssets) || ContainsUxml(movedAssets)) Schedule();
        }

        private static void Schedule()
        {
            if (pending || Application.isBatchMode) return;

            pending = true;
            // Writing scripts from inside an import is asking for trouble, so wait until it has finished.
            EditorApplication.delayCall += RegenerateAll;
        }

        private static void RegenerateAll()
        {
            pending = false;
            // With broken scripts, custom elements can't be resolved and would be generated wrongly.
            if (EditorUtility.scriptCompilationFailed) return;

            foreach (ProjectSettingSO settings in FindSettings())
            {
                Regenerator.RegenerateAll(settings);
            }
        }

        private static bool ContainsUxml(string[] paths)
        {
            foreach (string path in paths)
            {
                if (path.EndsWith(".uxml")) return true;
            }
            return false;
        }

        private static List<ProjectSettingSO> FindSettings()
        {
            List<ProjectSettingSO> found = new List<ProjectSettingSO>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(ProjectSettingSO)))
            {
                ProjectSettingSO settings = AssetDatabase.LoadAssetAtPath<ProjectSettingSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (settings != null) found.Add(settings);
            }
            return found;
        }
    }
}
