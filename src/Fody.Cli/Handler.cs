using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Fody.Cli
{
    public class Handler
    {
        public static async Task Handle(InvocationContext context)
        {
            var arguments = ParseArguments(context);
        }

        private static async Task HandleCore(Arguments arguments)
        {
            if (arguments.TargetPath.EndsWith(".sln"))
            {
                await HandleSolution(arguments);
            }
            else if (arguments.TargetPath.EndsWith(".csproj"))
            {
                await HandleProject(arguments);
            }
            else
            {
                WriteError("Unrecognized target file type");
            }
        }

        private static async Task HandleSolution(Arguments arguments)
        {
            var document = BuildDocument(arguments);
            var solution = new Solution(arguments.TargetPath);
            foreach (var project in solution.Projects)
            {
                await HandleProject(arguments, project);
            }
        }

        private static async Task HandleProject(Arguments arguments, Project? project = null)
        {
            var document = BuildDocument(arguments);
            var project = project ?? new Project(arguments.TargetPath);
            foreach (var addin in arguments.Addins)
            {
                var element = new XElement(addin.Name);
                if (addin.Value != null)
                {
                    element.Value = addin.Value;
                }
                foreach (var attribute in addin.Attributes)
                {
                    element.Add(new XAttribute(attribute, ""));
                }
                foreach (var node in addin.Nodes)
                {
                    var nodeElement = new XElement(node.Path);
                    if (node.Value != null)
                    {
                        nodeElement.Value = node.Value;
                    }
                    foreach (var attribute in node.Attributes)
                    {
                        nodeElement.Add(new XAttribute(attribute, ""));
                    }
                    element.Add(nodeElement);
                }
                document.Root?.Add(element);
            }
            await project.AddWeavers(document);
        }

        private static XDocument BuildDocument(Arguments arguments)
        {
            var document = new XDocument();
            var root = new XElement("Weavers", new XAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance"), new XAttribute("xsi:noNamespaceSchemaLocation", "FodyWeavers.xsd"));
            return document;
        }

        private static Arguments ParseArguments(InvocationContext context)
        {
            var tokens = context.ParseResult.Tokens;

            var targetPath = tokens[0].Value;
            if (!File.Exists(targetPath)) WriteError($"Target path is not exists: {targetPath}");

            var arguments = new Arguments(targetPath);
            Arguments.Addin? addin = null;
            Arguments.Node? node = null;
            var count = tokens.Count;
            var i = 1;
            while (true)
            {
                var token = tokens[i].Value;
                switch (token)
                {
                    case "--share":
                        TryEndUpAddinAddNode(arguments, ref addin, ref node);
                        CheckIndex(i, count, "share");
                        var share = tokens[i].Value;
                        if (!Enum.TryParse<ShareMode>(share, true, out var shareMode)) WriteError($"Unrecognized share argument value: {share}");
                        arguments.Share = shareMode;
                        break;
                    case "--sort":
                        TryEndUpAddinAddNode(arguments, ref addin, ref node);
                        CheckIndex(i, count, "sort");
                        var sort = tokens[i].Value;
                        arguments.Sort = sort;
                        break;
                    case "--addin":
                        TryEndUpAddinAddNode(arguments, ref addin, ref node);
                        CheckIndex(i, count, "addin");
                        addin = new Arguments.Addin(tokens[i].Value);
                        break;
                    case "--mode":
                        EnsureAddinNotNull(addin, "mode");
                        CheckIndex(i, count, "mode");
                        var mode = tokens[i].Value;
                        if (!Enum.TryParse<ManipulationMode>(mode, true, out var manulationMode)) WriteError($"Unrecognized mode argument value: {mode}");
                        addin!.Mode = manulationMode;
                        break;
                    case "-n":
                    case "--node":
                        EnsureAddinNotNull(addin, "node");
                        CheckIndex(i, count, "node");
                        TryEndUpNode(addin!, ref node);
                        break;
                    case "-a":
                    case "--attribute":
                        EnsureAddinNotNull(addin, "attribute");
                        CheckIndex(i, count, "attribute");
                        var attribute = tokens[i].Value;
                        if (node == null)
                        {
                            addin!.Attributes.Add(attribute);
                        }
                        else
                        {
                            node.Attributes.Add(attribute);
                        }
                        break;
                    case "-v":
                    case "--value":
                        EnsureAddinNotNull(addin, "value");
                        CheckIndex(i, count, "value");
                        var value = tokens[i].Value;
                        if (node == null)
                        {
                            addin!.Value = value;
                        }
                        else
                        {
                            node.Value = value;
                        }
                        break;
                    default:
                        WriteError($"Unrecognized argument({i}): {token}");
                        break;
                }
                i++;
            }
        }

        private static void TryEndUpAddinAddNode(Arguments arguments, ref Arguments.Addin? addin, ref Arguments.Node? node)
        {
            if (addin != null)
            {
                TryEndUpNode(addin, ref node);
                arguments.Addins.Add(addin);
                addin = null;
            }
        }

        private static void TryEndUpNode(Arguments.Addin addin, ref Arguments.Node? node)
        {
            if (node != null)
            {
                addin.Nodes.Add(node);
                node = null;
            }
        }

        private static void EnsureAddinNotNull(Arguments.Addin? addin, string argument)
        {
            if (addin == null) WriteError($"The argument `{argument}` can only apply to a node.");
        }

        private static void CheckIndex(int index, int limit, string argument)
        {
            if (index >= limit) WriteError($"The argument value of {argument} is missing");
        }

        private static void WriteError(string message)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Environment.Exit(1);
        }
    }
}
