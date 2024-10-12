using System.CommandLine;
using Cli4Fody.Arguments;

namespace Cli4Fody
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Manipulates Fody addins");

            var targetPathArgument = new Argument<string>("target-path", "The solution path or the project path that is going to be manipulated");
            var shareOption = new Option<ShareMode>("--share", "Generate the FodyWeavers.xml file in the solution folder or the project folder");
            var orderOption = new Option<string>("--order", "Custom addin orders. The addin names are split with `,` and use _others_ for the addins that are not specified");
            var addinOption = new Option<string[]>("--addin", "The addin to use") { Arity = ArgumentArity.ZeroOrMore };
            var packageVersionOption = new Option<string[]>(["--package-version", "-pv"], "The package version of the current addin");
            var modeOption = new Option<ManipulationMode[]>(["--mode", "-m"], "The manipulation mode of the current addin") { Arity = ArgumentArity.ZeroOrMore };
            var nodeOption = new Option<string[]>(["--node", "-n"], "Add a node to the current addin. The node path is split with `:` (e.g., A:B:C)") { Arity = ArgumentArity.ZeroOrMore };
            var attributeOption = new Option<string[]>(["--attribute", "-a"], "Add an attribute to the current addin or node") { Arity = ArgumentArity.ZeroOrMore };
            var valueOption = new Option<string[]>(["--value", "-v"], "Set the value of the current node") { Arity = ArgumentArity.ZeroOrMore };

            rootCommand.AddArgument(targetPathArgument);
            rootCommand.AddOption(shareOption);
            rootCommand.AddOption(orderOption);
            rootCommand.AddOption(addinOption);
            rootCommand.AddOption(packageVersionOption);
            rootCommand.AddOption(modeOption);
            rootCommand.AddOption(nodeOption);
            rootCommand.AddOption(attributeOption);
            rootCommand.AddOption(valueOption);

            rootCommand.SetHandler(Handler.Handle);

            return await rootCommand.InvokeAsync(args);
        }
    }
}
