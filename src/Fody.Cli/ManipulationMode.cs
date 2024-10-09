namespace Fody.Cli
{
    public enum ManipulationMode
    {
        /// <summary>
        /// Delete the existing addin configuration and create a new one.
        /// </summary>
        New,
        /// <summary>
        /// Overwrite the addin configuration with the given command-line arguments.
        /// </summary>
        Overwrite,
        /// <summary>
        /// Only append the configuration if it does not already exist.
        /// </summary>
        Append
    }
}
