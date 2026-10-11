using System;

namespace DADC.Clogs
{
    /// <summary>
    /// Remembers how a helper class was generated so it can be regenerated when its UXML changes.
    /// </summary>
    [Serializable]
    public class GeneratedHelper
    {
        public string uxmlGuid;
        public string className;
        public string outputPath;
        public string namespaceName;
        public string[] excludedElements = new string[0];
    }
}
