namespace Fody.Cli.Arguments
{
    public class Fody(string targetPath)
    {
        public TargetPath TargetPath { get; } = TargetPath.Parse(targetPath);

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

        public void AddAddin(Addin addin)
        {
            Addins.Add(addin);
        }
    }
}
