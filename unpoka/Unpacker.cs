using Coremats.PSP;
using System.IO.Compression;

namespace Unpoka;

internal class Unpacker
{
    private record Context(string OutputDir, UMDL Bhd, FileStream Bnd);

    public static void Unpack(string inputDir, string outputDir)
    {
        inputDir = Path.GetFullPath(inputDir);
        outputDir = Path.GetFullPath(outputDir);

        Console.WriteLine($"Using input directory: {inputDir}");
        Console.WriteLine($"Using output directory: {outputDir}");

        string bhdPath = Path.Combine(inputDir, "POKAPOKA.BHD");
        string bndPath = Path.Combine(inputDir, "POKAPOKA.BND");
        string fatPath = Path.Combine(inputDir, "POKAPOKA.FAT");

        AssertFileExists(bhdPath);
        AssertFileExists(bndPath);
        AssertFileExists(fatPath);

        Console.WriteLine("Loading POKAPOKA.BHD...");
        var bhd = DoFriendly(() => UMDL.Read(bhdPath),
            "Failed to load POKAPOKA.BHD.");
        Console.WriteLine("Opening POKAPOKA.BND...");
        using var bnd = DoFriendly(() => File.OpenRead(bndPath),
            "Failed to open POKAPOKA.BND.");
        Console.WriteLine("Loading POKAPOKA.FAT...");
        var fat = DoFriendly(() => FAT.Read(fatPath),
            "Failed to load POKAPOKA.FAT.");

        Console.WriteLine("Unpacking files...");

        var ctx = new Context(outputDir, bhd, bnd);
        foreach (var dir in fat.RootDirectory.Directories)
            UnpackDir(ctx, "", dir);

        foreach (var file in fat.RootDirectory.Files)
            UnpackFile(ctx, "", file);

        Console.WriteLine("Done!");
    }

    private static void UnpackDir(Context ctx, string outputSubdir, FAT.Directory dir)
    {
        outputSubdir = Path.Combine(outputSubdir, dir.Name);
        string outputDir = Path.Combine(ctx.OutputDir, outputSubdir);
        DoFriendly(() => Directory.CreateDirectory(outputDir),
            $"Failed to create directory: {outputDir}");

        foreach (var subdir in dir.Directories)
            UnpackDir(ctx, outputSubdir, subdir);

        foreach (var file in dir.Files)
            UnpackFile(ctx, outputSubdir, file);
    }

    private static void UnpackFile(Context ctx, string outputSubdir, FAT.File file)
    {
        string outputSubpath = Path.Combine(outputSubdir, file.Name);
        Console.WriteLine($"  {outputSubpath}");

        byte[] data = DoFriendly(() => ReadFileData(ctx, file),
            $"Failed to read file data: {outputSubpath}");
        string outputPath = Path.Combine(ctx.OutputDir, outputSubpath);
        DoFriendly(() => File.WriteAllBytes(outputPath, data),
            $"Failed to write file: {outputPath}");
    }

    private static byte[] ReadFileData(Context ctx, FAT.File file)
    {
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

        return uncompressed;
    }

    private static void DoFriendly(Action action, string message)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            throw new FriendlyException(message, ex);
        }
    }

    private static T DoFriendly<T>(Func<T> func, string message)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            throw new FriendlyException(message, ex);
        }
    }

    private static void AssertFileExists(string path)
    {
        if (!File.Exists(path))
            throw new FriendlyException($"File not found: {path}");
    }
}
