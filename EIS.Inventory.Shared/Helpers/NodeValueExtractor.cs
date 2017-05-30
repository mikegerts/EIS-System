using System;
using System.Linq;
using System.Xml;

namespace EIS.Inventory.Shared.Helpers
{
    public class NodeValueExtractor
    {
        private XmlNode _row;

        public NodeValueExtractor(XmlNode row)
        {
            _row = row;
        }

        public T GetValue<T>(string name)
        {
            var node = _row.ChildNodes.Cast<XmlNode>()
                .FirstOrDefault(n => n.Name.Equals(name));
            return (node == null || string.IsNullOrEmpty(node.InnerText)) 
                ? default(T) : (T)Convert.ChangeType(node.InnerText, typeof(T));
        }
    }
}
