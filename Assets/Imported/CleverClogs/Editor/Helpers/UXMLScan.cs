using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace DADC.Clogs
{
    public class UXMLScan
    {
        public static List<Element> ListElements(string tree)
        {
            List<Element> elements = new List<Element>();
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(tree);
            if (visualTree == null) return elements;

            VisualElement root = visualTree.Instantiate();

            root.Query().ForEach((element) =>
            {
                Element value = GetNamedElement(element);
                //Debug.Log(element.ToString());

                if(value.name != null && value.name != string.Empty && value.name != " ") elements.Add(value);

            });

            return elements;
        }

        private static Element GetNamedElement(VisualElement element)
        {
            string type = deliminateType(element.GetType().ToString());

           return new Element(element.name,type,element.GetType());
        }
        private static string deliminateType(string type)
        {
            int dotIndex = type.LastIndexOf('.');
            if (dotIndex < 1) return type;
            return type.Substring(dotIndex + 1);
        }
    }

    public struct Element
    {
        public string name;
        public string type;
        public System.Type elementType;
        public bool selected;
        public Element(string name,string type,System.Type elementType = null)
        {
            this.name = name;
            this.type = type;
            this.elementType = elementType;
            this.selected = false;
        }
    }

}
