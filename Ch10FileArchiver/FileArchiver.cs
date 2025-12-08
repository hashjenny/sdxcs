using System.Globalization;
using System.IO.Abstractions;
using System.Security.Cryptography;
using CsvHelper;

namespace Ch10FileArchiver;

public record HashPair(string FileName, string FileHash);

public enum Filetype
{
    Csv,
    Json
}

public class FileArchiver
{
    private const int HASH_LEN = 16;

    public FileArchiver(IFileSystem fs, Filetype type = Filetype.Csv, string sourceDir = ".",
        string backupDir = "./.backup")
    {
        FType = type;
        SourceDir = sourceDir;
        BackupDir = backupDir;
        Manifest = HashAll(fs);
    }

    public List<HashPair> Manifest { get; private set; }

    private Filetype FType { get; }

    private string SourceDir { get; }
    private string BackupDir { get; }

    private List<HashPair> HashAll(IFileSystem fs)
    {
        return HashAll(fs, SourceDir, BackupDir);
    }

    public static List<HashPair> HashAll(IFileSystem fs, string rootDir, string skipDir = "./.backup")
    {
        fs.Directory.CreateDirectory(skipDir);
        List<HashPair> results = [];
        var files = fs.Directory.GetFiles(rootDir, "*", SearchOption.AllDirectories);
        var normalizedSkipDir = fs.Path.GetFullPath(skipDir).TrimEnd(fs.Path.DirectorySeparatorChar);

        foreach (var file in files)
        {
            var fullFilePath = fs.Path.GetFullPath(file);
            if (fullFilePath.StartsWith(normalizedSkipDir, StringComparison.OrdinalIgnoreCase)) continue;

            using var f = fs.File.OpenRead(file);
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(f);
            results.Add(new HashPair(fs.Path.GetRelativePath(rootDir, file), Convert.ToHexString(hash)[..HASH_LEN]));
        }

        return results.OrderBy(r => r.FileName).ToList();
    }

    public void Backup(IFileSystem fs)
    {
        Manifest = HashAll(fs);
        Backup(fs, FType, SourceDir, BackupDir);
    }

    public static void Backup(IFileSystem fs, Filetype type, string sourceDir, string backupDir)
    {
        var manifest = HashAll(fs, sourceDir, backupDir);
        var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        switch (type)
        {
            case Filetype.Csv:
                WriteManifestByCsv(fs, backupDir, timeStamp, manifest);
                break;
            case Filetype.Json:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        CopyFiles(fs, sourceDir, backupDir, manifest);
    }

    private static void WriteManifestByCsv(IFileSystem fs, string backupDir, long timestamp, List<HashPair> manifest)
    {
        fs.Directory.CreateDirectory(backupDir);
        var manifestFile = Path.Combine(backupDir, $"{timestamp}.csv");
        CsvTool.WriteCsv(fs, manifestFile, manifest);
    }

    private static void CopyFiles(IFileSystem fs, string sourceDir, string backupDir, List<HashPair> manifest)
    {
        foreach (var pair in manifest)
        {
            var source = fs.Path.Combine(sourceDir, pair.FileName);
            var backup = fs.Path.Combine(backupDir, $"{pair.FileHash}.bck");
            if (!fs.File.Exists(backup)) fs.File.Copy(source, backup);
        }
    }
}

public static class CsvTool
{
    public static void WriteCsv(string csvPath, List<HashPair> manifest)
    {
        WriteCsv(new FileSystem(), csvPath, manifest);
    }

    public static void WriteCsv(IFileSystem fs, string csvPath, List<HashPair> manifest)
    {
        using var writer = fs.File.OpenWrite(csvPath);
        using var csv = new CsvWriter(new StreamWriter(writer), CultureInfo.InvariantCulture);
        csv.WriteHeader<HashPair>();
        csv.NextRecord();
        csv.WriteRecords(manifest);
    }

    public static List<HashPair> ReadCsv(string csvPath)
    {
        return ReadCsv(new FileSystem(), csvPath);
    }

    public static List<HashPair> ReadCsv(IFileSystem fs, string csvPath)
    {
        using var reader = fs.File.OpenRead(csvPath);
        using var csv = new CsvReader(new StreamReader(reader), CultureInfo.InvariantCulture);
        return csv.GetRecords<HashPair>().ToList();
    }
}