using System.CommandLine;

namespace Unpoka;

internal class Program
{
    static int Main(string[] args)
    {
        string version = typeof(Program).Assembly.GetName().Version!.ToString(3);
        Console.WriteLine($"unpoka v{version}");

        var inputOption = new Option<string>("-i")
        {
            HelpName = "Input Directory",
            Description = "The input directory containing POKAPOKA.BHD.",
            DefaultValueFactory = _ => ".",
        };
        var outputOption = new Option<string>("-o")
        {
            HelpName = "Output Directory",
            Description = "The output directory to unpack files to.",
            DefaultValueFactory = _ => "POKAPOKA",
        };

        var rootCommand = new RootCommand("Unpacks gamedata from the PSP versions of Monhan Nikki and Monhan Nikki G.");
        rootCommand.Options.Add(inputOption);
        rootCommand.Options.Add(outputOption);
        rootCommand.SetAction(pr =>
        {
            string inputDir = pr.GetValue(inputOption)!;
            string outputDir = pr.GetValue(outputOption)!;
            return UnpackAction(inputDir, outputDir);
        });

        int result = rootCommand.Parse(args).Invoke();
        Console.Write("Press any key to exit...");
        Console.ReadKey(true);
        Console.WriteLine();
        return result;
    }

    private static int UnpackAction(string inputDir, string outputDir)
    {
        try
        {
            Unpacker.Unpack(inputDir, outputDir);
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            WriteLineColored(ConsoleColor.Red, ex switch
            {
                FriendlyException when ex.InnerException == null => $"""
                    {ex.Message}
                    """,
                FriendlyException => $"""
                    {ex.Message}
                      Please include the stack trace printed below if submitting a report.

                    {ex}
                    """,
                _ => $"""
                    An unexpected error occurred.
                      Please include the stack trace printed below if submitting a report.
                    
                    {ex}
                    """
            });
            Console.WriteLine();
            return 1;
        }
    }

    private static void WriteLineColored(ConsoleColor color, string message)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = color;
        try
        {
            Console.WriteLine(message);
        }
        finally
        {
            Console.ForegroundColor = prev;
        }
    }
}
