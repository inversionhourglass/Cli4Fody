using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cli4Fody.Arguments
{
    public class Node(string path)
    {
        public string Path => path;

        public List<Attribute> Attributes { get; } = [];

        public string? Value { get; set; }

        public void Build(XElement parent)
        {
            var element= BuildElement(parent);

            if (Value != null)
            {
                element.Value = Value;
            }
            foreach (var attribute in Attributes)
            {
                element.Add(new XAttribute(attribute.Key, attribute.Value));
            }
        }

        private XElement BuildElement(XElement parent)
        {
            var paths = path.Split(':', StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < paths.Length - 1; i++)
            {
                var p = paths[i];
                var ele = parent.Element(p);
                if (ele == null)
                {
                    ele = new XElement(p);
                    parent.Add(ele);
                }
                parent = ele;
            }

            var element = new XElement(paths[^1]);
            parent.Add(element);

            return element;
        }
    }
}
