using System.Text.RegularExpressions;

namespace Cli4Fody.Arguments
{
    public partial class TargetPath
    {
        private string[]? _projectPaths;

        private TargetPath(string value)
        {
            if (!Path.IsPathRooted(value))
            {
                value = Path.Combine(System.IO.Directory.GetCurrentDirectory(), NativePath(value));
            }
            Value = value;
            Directory = Path.GetDirectoryName(value)!;
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
            if (_projectPaths == null)
            {
                if (!IsSolution)
                {
                    _projectPaths = [Value];
                }
                else
                {
                    var solutionContent = File.ReadAllText(Value);
                    var matches = ProjectMatcher().Matches(solutionContent);

                    _projectPaths = matches.Select(x => Path.Combine(Directory, NativePath(x.Groups[1].Value))).ToArray();
                }
            }

            return _projectPaths;
        }

        public static TargetPath Parse(string targetPath)
        {
            if (!File.Exists(targetPath)) throw new ArgumentException($"The target path does not exist.");

            return new(targetPath);
        }

        private static string NativePath(string path) => path.Replace('\\', Path.DirectorySeparatorChar);

        [GeneratedRegex(@"Project\("".*""\) = "".*"", ""([^""]+\.csproj)""")]
        private static partial Regex ProjectMatcher();
    }
}
