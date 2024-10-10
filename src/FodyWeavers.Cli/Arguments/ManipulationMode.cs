namespace FodyWeavers.Cli.Arguments
{
    public enum ManipulationMode
    {
        /// <summary>
        /// Delete the addin configuration if it exists and use the given command-line arguments instead.
        /// </summary>
        Overwrite,
        /// <summary>
        /// Use the given command-line arguments only if the addin configuration is absent.
        /// </summary>
        Default
    }
}
