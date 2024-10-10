using System.CommandLine.Invocation;
using Fody.Cli.Arguments;

namespace Fody.Cli
{
    public class Handler
    {
        public static void Handle(InvocationContext context)
        {
            try
            {
                var args = ParseArgs(context);
                args.Build();
            }
            catch (Exception ex)
            {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = originalColor;
                Environment.Exit(1);
            }
        }

        private static Args ParseArgs(InvocationContext context)
        {
            var tokens = context.ParseResult.Tokens;

            var args = new Args(tokens[0].Value);
            Addin? addin = null;
            Node? node = null;
            var count = tokens.Count;
            var i = 1;
            while (i < count)
            {
                var token = tokens[i].Value;
                switch (token)
                {
                    case "--share":
                        TryEndUpAddinAddNode(args, ref addin, ref node);
                        CheckIndex(i++, count, "share");
                        args.SetShare(tokens[i].Value);
                        break;
                    case "--order":
                        TryEndUpAddinAddNode(args, ref addin, ref node);
                        CheckIndex(i++, count, "sort");
                        args.SetOrder(tokens[i].Value);
                        break;
                    case "--addin":
                        TryEndUpAddinAddNode(args, ref addin, ref node);
                        CheckIndex(i++, count, "addin");
                        addin = new Addin(tokens[i].Value);
                        break;
                    case "-m":
                    case "--mode":
                        EnsureAddinNotNull(addin, "mode");
                        CheckIndex(i++, count, "mode");
                        addin!.SetMode(tokens[i].Value);
                        break;
                    case "-n":
                    case "--node":
                        EnsureAddinNotNull(addin, "node");
                        TryEndUpNode(addin!, ref node);
                        CheckIndex(i++, count, "node");
                        node = new(tokens[i].Value);
                        break;
                    case "-a":
                    case "--attribute":
                        EnsureAddinNotNull(addin, "attribute");
                        CheckIndex(i++, count, "attribute");
                        var attribute = new Arguments.Attribute(tokens[i].Value);
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
                        CheckIndex(i++, count, "value");
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
                        throw new ArgumentException($"Unrecognized argument({i}): {token}");
                }
                i++;
            }

            TryEndUpAddinAddNode(args, ref addin, ref node);

            return args;
        }

        private static void TryEndUpAddinAddNode(Args args, ref Addin? addin, ref Node? node)
        {
            if (addin != null)
            {
                TryEndUpNode(addin, ref node);
                args.Addins.Add(addin);
                addin = null;
            }
        }

        private static void TryEndUpNode(Addin addin, ref Node? node)
        {
            if (node != null)
            {
                addin.Nodes.Add(node);
                node = null;
            }
        }

        private static void EnsureAddinNotNull(Addin? addin, string argument)
        {
            if (addin == null) throw new ArgumentException($"The argument `{argument}` can only apply to a node.");
        }

        private static void CheckIndex(int index, int limit, string argument)
        {
            if (index >= limit) throw new ArgumentException($"The argument value of {argument} is missing");
        }
    }
}
