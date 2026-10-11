using System.Collections.Generic;

namespace DADC.Clogs
{
    public class SortElements
    {
        public static Dictionary<string, List<Element>> Sort(List<Element> elements)
        {
            Dictionary<string, List<Element>> sorted = new Dictionary<string, List<Element>>();
            foreach (Element element in elements)
            {
                string type = element.type;
                if (sorted.ContainsKey(type))
                {
                    sorted[type].Add(element);
                }
                else
                {
                    sorted.Add(type, new List<Element>() { element });
                }
            }
            return sorted;
        }
    }
}