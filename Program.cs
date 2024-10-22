using Argsharp;

namespace Get;

public static class Program
{
    internal const string GetVersion = "1.0";

    public static void Main(string[] args)
    {
        var help = new Flag("h", "help", "Shows the commands");
        var version = new Flag("v", "version", "Shows the current GET version");

        var parser = new Parser(args, [help, version], "get", "The interpreter for the Get language");

        (ResultMap, string[]) parserResult;
        try
        {
            parserResult = parser.Parse();
        }
        catch (ArgsharpParseException e)
        {
            Console.WriteLine(e);
            return;
        }

        if (parserResult.Item1.TryFlag(help))
        {
            Console.WriteLine(parser.Help());
            return;
        }
        else if (parserResult.Item1.TryFlag(version))
        {
            Console.WriteLine(GetVersion);
            return;
        }
        else
        if (parserResult.Item2.Length == 0)
        {
            Console.WriteLine($"expected: get <path> {parser.FlagUsage()}");
            return;
        }

        string path = parserResult.Item2[0];

        string content;
        try
        {
            content = File.ReadAllText(path);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"path {path} cannot be found");
            return;
        }

        var interpreter = new Interpreter(content);
        var (_, err) = interpreter.Interpret();
        if (err != null)
        {
            Console.WriteLine(err);
            return;
        }
    }
}