using Coremats.PSP;
using System.CommandLine;
using System.IO.Compression;

namespace Unpoka;

internal class Program
{
    static int Main(string[] args)
    {
        string version = typeof(Program).Assembly.GetName().Version!.ToString(3);
        Console.WriteLine($"unpoka v{version}\n");

        var inputOption = new Option<string>("-i")
        {
            HelpName = "Input directory",
            Description = "The input directory containing POKAPOKA.BHD.",
            DefaultValueFactory = pr => ".",
        };
        var outputOption = new Option<string>("-o")
        {
            HelpName = "Output directory",
            Description = "The output directory to unpack files to.",
            DefaultValueFactory = pr => "POKAPOKA",
        };

        var rootCommand = new RootCommand("Unpacks gamedata from the PSP versions of Monhan Nikki and Monhan Nikki G.");
        rootCommand.Options.Add(inputOption);
        rootCommand.Options.Add(outputOption);
        rootCommand.SetAction(pr =>
        {
            string inputDir = pr.GetValue(inputOption) ?? ".";
            string outputDir = pr.GetValue(outputOption) ?? "unpacked";
            try
            {
                Unpack(inputDir, outputDir);
                return 0;
            }
            catch (FriendlyException ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        });

        int result = rootCommand.Parse(args).Invoke();
        Console.Write("\nPress any key to exit... ");
        Console.ReadKey();
        return result;
    }

    private record Context(string OutputDir, UMDL Bhd, FileStream Bnd);

    private static void AssertFileExists(string path)
    {
        if (!File.Exists(path))
            throw new FriendlyException($"File not found:\n  {path}");
    }

    private static void Unpack(string inputDir, string outputDir)
    {
        inputDir = Path.GetFullPath(inputDir);
        outputDir = Path.GetFullPath(outputDir);

        Console.WriteLine($"Using input directory:\n  {inputDir}");
        Console.WriteLine($"Using output directory:\n  {outputDir}");

        string bhdPath = Path.Combine(inputDir, "POKAPOKA.BHD");
        string bndPath = Path.Combine(inputDir, "POKAPOKA.BND");
        string fatPath = Path.Combine(inputDir, "POKAPOKA.FAT");

        AssertFileExists(bhdPath);
        AssertFileExists(bndPath);
        AssertFileExists(fatPath);

        Console.WriteLine("Loading POKAPOKA.BHD...");
        var bhd = UMDL.Read(bhdPath);
        Console.WriteLine("Opening POKAPOKA.BND...");
        using var bnd = File.OpenRead(bndPath);
        Console.WriteLine("Loading POKAPOKA.FAT...");
        var fat = FAT.Read(fatPath);

        Console.WriteLine("Unpacking files...");

        var ctx = new Context(outputDir, bhd, bnd);
        foreach (var dir in fat.RootDirectory.Directories)
            UnpackDir(ctx, "", dir);

        foreach (var file in fat.RootDirectory.Files)
            UnpackFile(ctx, "", file);

        Console.WriteLine("Done!");
    }

    private static void UnpackDir(Context ctx, string outputDir, FAT.Directory dir)
    {
        outputDir = Path.Combine(outputDir, dir.Name);
        Directory.CreateDirectory(Path.Combine(ctx.OutputDir, outputDir));

        foreach (var subdir in dir.Directories)
            UnpackDir(ctx, outputDir, subdir);

        foreach (var file in dir.Files)
            UnpackFile(ctx, outputDir, file);
    }

    private static void UnpackFile(Context ctx, string outputDir, FAT.File file)
    {
        string outputPath = Path.Combine(outputDir, file.Name);
        Console.WriteLine($"  {outputPath}");

        var header = ctx.Bhd.Files[file.Index];
        var uncompressed = new byte[header.LengthUncompressed];

        ctx.Bnd.Position = header.DataBlock * 0x400;
        if (header.LengthCompressed == 0)
        {
            ctx.Bnd.ReadExactly(uncompressed);
        }
        else
        {
            var compressed = new byte[header.LengthCompressed];
            ctx.Bnd.ReadExactly(compressed);

            using var ms = new MemoryStream(compressed);
            using var zs = new ZLibStream(ms, CompressionMode.Decompress);
            zs.ReadExactly(uncompressed);
        }

        File.WriteAllBytes(Path.Combine(ctx.OutputDir, outputPath), uncompressed);
    }
}
