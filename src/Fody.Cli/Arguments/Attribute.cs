namespace Fody.Cli.Arguments
{
    public class Attribute(string key, string value)
    {
        public string Key => key;

        public string Value => value;

        public static Attribute Parse(string value)
        {
            var index = value.IndexOf('=');
            if (index == -1) throw new ArgumentException($"Incorrect attribute format: {value}");

            return new(value[..index], value[++index..]);
        }
    }
}
