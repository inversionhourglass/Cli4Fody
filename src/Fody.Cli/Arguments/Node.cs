using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fody.Cli.Arguments
{
    public class Node(string path)
    {
        public string Path => path;

        public List<Attribute> Attributes { get; } = [];

        public string? Value { get; set; }

        public void Write( ManipulationMode mode)
        {

        }
    }
}
