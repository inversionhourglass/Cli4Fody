namespace FodyWeavers.Cli.Arguments
{
    public enum ShareMode
    {
        /// <summary>
        /// Share the FodyWeavers.xml file with the solution.
        /// </summary>
        Solution,
        /// <summary>
        /// Generate a FodyWeavers.xml file for each project.
        /// </summary>
        Project
    }
}
