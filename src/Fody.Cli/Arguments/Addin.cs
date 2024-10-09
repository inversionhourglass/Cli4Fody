namespace Fody.Cli.Arguments
{
    public class Addin(string name)
    {
        public string Name => name;

        public ManipulationMode Mode { get; private set; } = ManipulationMode.New;

        public List<Attribute> Attributes { get; } = [];

        public string? Value { get; set; }

        public List<Node> Nodes { get; } = [];

        public void SetMode(string mode)
        {
            Mode = mode.ToLower() switch
            {
                "new" => ManipulationMode.New,
                "append" => ManipulationMode.Append,
                "overwrite" => ManipulationMode.Overwrite,
                _ => throw new ArgumentException($"Invalid mode value: {mode}")
            };
        }
    }
}
