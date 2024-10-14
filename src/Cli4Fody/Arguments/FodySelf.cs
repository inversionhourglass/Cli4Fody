using System.Xml.Linq;

namespace Cli4Fody.Arguments
{
    public class FodySelf() : Addin("Fody")
    {
        public override string PackageName => Name;

        public override void Build(XDocument document)
        {
            
        }
    }
}
