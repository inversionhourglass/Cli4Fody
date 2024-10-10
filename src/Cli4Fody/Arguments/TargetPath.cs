using System.Text.RegularExpressions;

namespace Cli4Fody.Arguments
{
    public partial class TargetPath
    {
        private TargetPath(string value)
        {
            Value = value;
            Directory = Path.GetFullPath(Path.GetDirectoryName(value)!);
            if (value.EndsWith(".sln"))
            {
                IsSolution = true;
            }
            else if (!value.EndsWith(".csproj"))
            {
                throw new ArgumentException("The target path is not a recognized file type.");
            }
        }

        public string Value { get; }

        public bool IsSolution { get; }

        public string Directory { get; }

        public string[] GetProjectPaths()
        {
            if (!IsSolution) return [Value];

            var solutionContent = File.ReadAllText(Value);
            var matches = ProjectMatcher().Matches(solutionContent);

            return matches.Select(x => x.Groups[1].Value).ToArray();
        }

        public static TargetPath Parse(string targetPath)
        {
            if (!File.Exists(targetPath)) throw new ArgumentException($"The target path does not exist.");

            return new(targetPath);
        }

        [GeneratedRegex(@"Project\("".*""\) = "".*"", ""([^""]+\.csproj)""")]
        private static partial Regex ProjectMatcher();
    }
}
