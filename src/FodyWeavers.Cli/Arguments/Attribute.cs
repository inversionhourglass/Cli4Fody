namespace FodyWeavers.Cli.Arguments
{
    public class Attribute
    {
        public Attribute(string value)
        {
            var index = value.IndexOf('=');
            if (index == -1) throw new ArgumentException($"Incorrect attribute format: {value}");

            Key = value[..index];
            Value = value[++index..];
        }

        public string Key { get; }

        public string Value { get; }
    }
}
