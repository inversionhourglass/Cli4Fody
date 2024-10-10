using System.Xml.Linq;

namespace Fody.Cli.Arguments
{
    public class Order
    {
        private const string OTHERS = "_others_";

        private readonly Dictionary<string, int> _orderMap;

        public Order(string value)
        {
            Value = value;
            _orderMap = value.Split(',').Select((x, i) => (x, i)).ToDictionary(x => x.x.Trim(), x => x.i * 100);
            if (!_orderMap.ContainsKey(OTHERS))
            {
                _orderMap[OTHERS] = _orderMap.Count;
            }
        }

        public string Value { get; }

        public void Sort(XDocument document)
        {
            var root = document.Root;
            if (root == null) return;
            var elements = root.Elements().ToList();
            var others = 0;
            var map = new Dictionary<XElement, int>();
            foreach (var element in elements)
            {
                var name = element.Name.LocalName;
                if (!_orderMap.TryGetValue(name, out var order))
                {
                    order = _orderMap[OTHERS] + others++;
                }
                map[element] = order;
            }
            elements.Sort((a, b) => map[a].CompareTo(map[b]));

            root.RemoveNodes();
            foreach (var element in elements)
            {
                root.Add(element);
            }
        }
    }
}
