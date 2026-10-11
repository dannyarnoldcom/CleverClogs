using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DADC.Clogs
{
    public enum RegenerateResult
    {
        Unchanged,
        Updated,
        Removed,
        Skipped
    }

    public static class Regenerator
    {
        private const string logPrefix = "CleverClogs: ";

        /// <summary>
        /// Generates a helper class from the selected elements and records it in the settings,
        /// so it is kept up to date when the UXML changes. Returns the asset path of the class.
        /// </summary>
        public static string GenerateAndRecord(ProjectSettingSO settings, string uxmlPath, string className, IList<Element> elements, out List<string> warnings)
        {
            List<Element> selected = new List<Element>();
            List<string> excluded = new List<string>();
            foreach (Element element in elements)
            {
                if (element.selected) selected.Add(element);
                else if (!excluded.Contains(element.name)) excluded.Add(element.name);
            }

            string source = Generate.BuildSource(className, uxmlPath, selected, settings.generatedNamespace, out warnings);
            string path = Generate.WriteFile(settings.outputFolder, className, source);

            GeneratedHelper helper = settings.generated.Find((entry) => entry.outputPath == path);
            if (helper == null)
            {
                helper = new GeneratedHelper();
                settings.generated.Add(helper);
            }
            helper.uxmlGuid = AssetDatabase.AssetPathToGUID(uxmlPath);
            helper.className = Generate.ToIdentifier(className);
            helper.outputPath = path;
            helper.namespaceName = settings.generatedNamespace;
            helper.excludedElements = excluded.ToArray();
            Save(settings);

            return path;
        }

        /// <summary>
        /// Returns the first helper recorded for a UXML file, or null.
        /// </summary>
        public static GeneratedHelper Find(ProjectSettingSO settings, string uxmlPath)
        {
            string guid = AssetDatabase.AssetPathToGUID(uxmlPath);
            if (string.IsNullOrEmpty(guid)) return null;
            return settings.generated.Find((entry) => entry.uxmlGuid == guid);
        }

        /// <summary>
        /// Rebuilds one helper class from its UXML and rewrites the file only if the result differs.
        /// </summary>
        public static RegenerateResult Regenerate(GeneratedHelper helper)
        {
            // Deleting the generated file is how a user opts a UXML file back out.
            if (!File.Exists(helper.outputPath)) return RegenerateResult.Removed;

            string uxmlPath = AssetDatabase.GUIDToAssetPath(helper.uxmlGuid);
            if (string.IsNullOrEmpty(uxmlPath) || AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath) == null)
            {
                return RegenerateResult.Skipped;
            }

            List<string> excluded = new List<string>(helper.excludedElements ?? new string[0]);
            List<Element> elements = UXMLScan.ListElements(uxmlPath).FindAll((element) => !excluded.Contains(element.name));
            string source = Generate.BuildSource(helper.className, uxmlPath, elements, helper.namespaceName, out _);

            if (Normalize(File.ReadAllText(helper.outputPath)) == Normalize(source)) return RegenerateResult.Unchanged;

            try
            {
                Generate.WriteFile(Path.GetDirectoryName(helper.outputPath), helper.className, source);
            }
            catch (IOException e)
            {
                Debug.LogWarning(logPrefix + e.Message);
                return RegenerateResult.Skipped;
            }
            return RegenerateResult.Updated;
        }

        /// <summary>
        /// Regenerates every helper recorded in the settings and forgets the ones whose file is gone.
        /// Returns the number of files rewritten.
        /// </summary>
        public static int RegenerateAll(ProjectSettingSO settings)
        {
            if (!settings.autoRegenerate) return 0;

            int updated = 0;
            bool changed = false;
            for (int i = settings.generated.Count - 1; i >= 0; i--)
            {
                GeneratedHelper helper = settings.generated[i];
                RegenerateResult result = Regenerate(helper);
                if (result == RegenerateResult.Updated)
                {
                    updated++;
                    Debug.Log(logPrefix + "regenerated " + helper.outputPath);
                }
                else if (result == RegenerateResult.Removed)
                {
                    settings.generated.RemoveAt(i);
                    changed = true;
                }
            }

            if (changed) Save(settings);
            return updated;
        }

        /// <summary>
        /// Keeps the record pointing at a generated class that was moved or renamed in the project.
        /// </summary>
        public static bool OutputMoved(ProjectSettingSO settings, string from, string to)
        {
            GeneratedHelper helper = settings.generated.Find((entry) => entry.outputPath == from);
            if (helper == null) return false;

            helper.outputPath = to;
            Save(settings);
            return true;
        }

        private static void Save(ProjectSettingSO settings)
        {
            EditorUtility.SetDirty(settings);
            if (AssetDatabase.Contains(settings)) AssetDatabase.SaveAssetIfDirty(settings);
        }

        private static string Normalize(string text)
        {
            return text.Replace("\r\n", "\n");
        }
    }
}
