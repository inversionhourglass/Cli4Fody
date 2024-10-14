using System.Diagnostics;
using System.Xml.Linq;

namespace Cli4Fody.Arguments
{
    public class Addin(string name)
    {
        public string Name => name;

        public virtual string PackageName => $"{name}.Fody";

        public ManipulationMode Mode { get; private set; } = ManipulationMode.Overwrite;

        public string? PackageVersion { get; set; }

        public List<Attribute> Attributes { get; } = [];

        public string? Value { get; set; }

        public List<Node> Nodes { get; } = [];

        public void SetMode(string mode)
        {
            Mode = mode.ToLower() switch
            {
                "overwrite" => ManipulationMode.Overwrite,
                "default" => ManipulationMode.Default,
                _ => throw new ArgumentException($"Invalid mode value: {mode}")
            };
        }

        public void Install(string projectPath)
        {
            if (PackageVersion == null) return;

            var directory = Path.GetDirectoryName(projectPath);
            var fileName = Path.GetFileName(projectPath);

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"add \"{fileName}\" package {PackageName} -v {PackageVersion} --no-restore",
                WorkingDirectory = directory,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            using var process = Process.Start(startInfo);
            process!.WaitForExit();
            if (process.ExitCode != 0) throw new InvalidOperationException("Running `dotnet add package` failed.");
        }

        public virtual void Build(XDocument document)
        {
            var addin = BuildElement(document);
            if (addin == null) return;

            if (Value != null)
            {
                addin.Value = Value;
            }
            foreach (var attribute in Attributes)
            {
                addin.Add(new XAttribute(attribute.Key, attribute.Value));
            }
            foreach (var node in Nodes)
            {
                node.Build(addin);
            }
        }

        private XElement? BuildElement(XDocument document)
        {
            var addin = document.Root!.Element(name);
            if (addin != null)
            {
                if (Mode == ManipulationMode.Default) return null;

                addin.RemoveAll();
            }
            else
            {
                addin = new XElement(name);
                document.Root!.Add(addin);
            }

            return addin;
        }
    }
}
