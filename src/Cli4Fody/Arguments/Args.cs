using System.Xml;
using System.Xml.Linq;

namespace Cli4Fody.Arguments
{
    public class Args(string solutionOrProjectPath)
    {
        private const string FILE_NAME = "FodyWeavers.xml";
        private const string ROOT = "Weavers";

        public TargetPath SolutionOrProjectPath { get; } = TargetPath.Parse(solutionOrProjectPath);

        public ShareMode Share { get; private set; } = ShareMode.Project;

        public Order? Order { get; private set; }

        public List<Addin> Addins { get; } = [];

        public void SetShare(string share)
        {
            Share = share.ToLower() switch
            {
                "project" => ShareMode.Project,
                "solution" => ShareMode.Solution,
                _ => throw new ArgumentException($"Invalid share value: {share}")
            };
        }

        public void SetOrder(string order)
        {
            Order = new(order);
        }

        public void Install()
        {
            foreach (var projectPath in SolutionOrProjectPath.GetProjectPaths())
            {
                foreach (var addin in Addins)
                {
                    addin.Install(projectPath);
                }
            }
        }

        public void Build()
        {
            var projectPaths = SolutionOrProjectPath.GetProjectPaths();
            if (Share == ShareMode.Project && SolutionOrProjectPath.IsSolution)
            {
                foreach (var projectPath in projectPaths)
                {
                    Build(Path.GetDirectoryName(projectPath)!);
                }
            }
            else
            {
                Build(SolutionOrProjectPath.Directory);
            }
        }

        private void Build(string directory)
        {
            var filePath = Path.Combine(directory, FILE_NAME);
            var document = File.Exists(filePath) ? XDocument.Load(filePath) : new XDocument();

            if (document.Root != null && document.Root.Name.LocalName != ROOT)
            {
                document.Root.Remove();
            }
            if (document.Root == null)
            {
                document.Add(
                    new XElement(ROOT,
                        new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                        new XAttribute(XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance") + "noNamespaceSchemaLocation", "FodyWeavers.xsd"))
                );
            }

            foreach (var addin in Addins)
            {
                addin.Build(document);
            }

            Order?.Sort(document);

            document.Declaration = null;

            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                IndentChars = "  "
            };
            using var writer = XmlWriter.Create(filePath, settings);
            document.Save(writer);
        }
    }
}
