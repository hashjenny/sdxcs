using System.Globalization;
using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CsvHelper;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace Ch10FileArchiver;

public record HashPair(string FileName, string FileHash);

// ex2
// Modify so that it can save JSON manifests as well as CSV manifests 
// 使其能够保存 JSON 格式的清单文件以及 CSV 格式的清单文件。
// 
// Write another program called migrate.py that converts a set of manifests from CSV to JSON. 
// 编写另一个名为 migrate.py 的程序，用于将一组清单文件从 CSV 格式转换为 JSON 格式。
public enum OutputFormat
{
    Csv,
    Json
}

// ex1
// Modify the backup program so that manifests are numbered sequentially as 00000001.csv, 00000002.csv, and so on rather than being timestamped. 
// 修改备份程序，使得清单按 00000001.csv、00000002.csv 等顺序编号，而不是按时间戳编号。
public enum ManifestStyle
{
    Number,
    Timestamp
}

// ex3
// Modify the file backup program so that it uses a function called ourHash to hash files.
// 修改文件备份程序，使其使用名为 ourHash 的函数来哈希文件。
// 
// Create a replacement that returns some predictable value, such as the first few characters of the data.
// 创建一个替换方案，返回一个可预测的值，例如数据的前几个字符。
public enum HashFunction
{
    Default,
    OurHash
}

public class ArchiverConfiguration
{
    public OutputFormat OutputType { get; init; } = OutputFormat.Csv;
    public HashFunction MyHashFunction { get; init; } = HashFunction.Default;
    public ManifestStyle ManifestType { get; init; } = ManifestStyle.Number;
}

public class FileArchiver
{
    public FileArchiver(IFileSystem? fs = null,
        ArchiverConfiguration? config = null,
        string sourceDir = ".",
        string backupDir = ".backup")
    {
        OutputType = config?.OutputType ?? OutputFormat.Csv;
        HashType = config?.MyHashFunction ?? HashFunction.Default;
        ManifestType = config?.ManifestType ?? ManifestStyle.Number;
        Fs = fs ?? new FileSystem();
        SourceDir = sourceDir;
        BackupDir = Fs.Path.Combine(SourceDir, backupDir);
        Extension = OutputType switch
        {
            OutputFormat.Csv => "csv",
            OutputFormat.Json => "json",
            _ => throw new ArgumentOutOfRangeException()
        };
        Manifest = HashAll();

        Fs.Directory.CreateDirectory(BackupDir);
    }

    public Dictionary<string, string> Manifest { get; private set; }

    private OutputFormat OutputType { get; }
    private HashFunction HashType { get; }
    private ManifestStyle ManifestType { get; }

    private IFileSystem Fs { get; }
    public string SourceDir { get; }
    public string BackupDir { get; }

    private string Extension { get; }

    public Dictionary<string, string> HashAll(IFileSystem? fs = null, string? sourceDir = null)
    {
        fs ??= Fs;
        sourceDir ??= SourceDir;
        var files = fs.Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
        var normalizedSkipDir = Fs.Path.GetFullPath(BackupDir).TrimEnd(Fs.Path.DirectorySeparatorChar);
        var results = new Dictionary<string, string>();
        using var sha = SHA256.Create();
        using var md5 = MD5.Create();
        foreach (var file in files)
        {
            var fullFilePath = Fs.Path.GetFullPath(file);
            if (fullFilePath.StartsWith(normalizedSkipDir, StringComparison.OrdinalIgnoreCase) ||
                Fs.Path.GetFileName(file) == ".DS_Store") continue;

            using var f = Fs.File.OpenRead(file);
            var hash = HashType switch
            {
                HashFunction.Default => HashTool.Default(f, sha),
                HashFunction.OurHash => HashTool.OurHash(f, md5),
                _ => throw new ArgumentOutOfRangeException(nameof(HashType), HashType, null)
            };
            results.Add(Fs.Path.GetRelativePath(SourceDir, file), hash);
        }

        return results;
    }

    public void Backup()
    {
        Manifest = HashAll();
        WriteManifest();
        CopyFiles();
    }

    private void WriteManifest()
    {
        var extension = OutputType switch
        {
            OutputFormat.Csv => "csv",
            OutputFormat.Json => "json",
            _ => throw new ArgumentOutOfRangeException()
        };
        var filename = ManifestType switch
        {
            ManifestStyle.Timestamp => $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            ManifestStyle.Number => $"{GetLatestManifestVersion(extension) + 1:D8}",
            _ => throw new ArgumentOutOfRangeException()
        };
        var manifestFile = Path.Combine(BackupDir, $"{filename}.{extension}");
        switch (OutputType)
        {
            case OutputFormat.Csv:
                CsvTool.WriteCsv(Fs, manifestFile, Manifest);
                break;
            case OutputFormat.Json:
                JsonTool.WriteJson(Fs, manifestFile, Manifest);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private long GetLatestManifestVersion(string extension)
    {
        var backupFiles = Fs.Directory.GetFiles(BackupDir, $"*.{extension}", SearchOption.TopDirectoryOnly);
        if (backupFiles.Length == 0) return 0;
        var latest = backupFiles.OrderDescending().Take(1).ToArray()[0];
        var fileName = Fs.Path.GetFileNameWithoutExtension(latest);
        return long.TryParse(fileName, out var result)
            ? result
            : throw new Exception("no latest manifest file.");
    }

    private void CopyFiles()
    {
        foreach (var pair in Manifest)
        {
            var source = Fs.Path.Combine(SourceDir, pair.Key);
            var backup = Fs.Path.Combine(BackupDir, $"{pair.Value}.bck");
            if (!Fs.File.Exists(backup)) Fs.File.Copy(source, backup);
        }
    }

    public static void Migrate(IFileSystem fs, string file, bool deleted = true)
    {
        var extension = fs.Path.GetExtension(file);
        switch (extension)
        {
            case ".csv":
                var manifest = CsvTool.ReadCsv(fs, file);
                var jsonFile = file[..^3] + "json";
                JsonTool.WriteJson(fs, jsonFile, manifest);
                break;
            case ".json":
                var list = JsonTool.ReadJson(fs, file);
                var csvFile = file[..^4] + "csv";
                CsvTool.WriteCsv(fs, csvFile, list);
                break;
            default:
                throw new Exception("unsupported file format");
        }

        if (deleted) fs.File.Delete(file);
    }

    // ex4
    // Write a program compare-manifests.py that reads two manifest files and reports:
    // 编写一个程序 compare-manifests.py ，它读取两个清单文件并报告：
    // Which files have the same names but different hashes (i.e., their contents have changed).
    // 哪些文件具有相同的名称但哈希值不同（即它们的内容已更改）。
    // Which files have the same hashes but different names (i.e., they have been renamed).
    // 哪些文件具有相同的哈希值但名称不同（即它们已被重命名）。
    // Which files are in the first hash but neither their names nor their hashes are in the second (i.e., they have been deleted).
    // 哪些文件在第一个哈希中，但它们的名称和哈希值都不在第二个中（即它们已被删除）。
    // Which files are in the second hash but neither their names nor their hashes are in the first (i.e., they have been added).
    // 哪些文件在第二个哈希表中，但它们的名称和哈希值都不在第一个（即它们已被添加）。
    public (List<HashPair>, List<HashPair>, List<HashPair>, List<HashPair>) CompareManifest(string oldFile,
        string newFile)
    {
        var oldManifest = GetManifest(oldFile);
        var newManifest = GetManifest(newFile);
        return CompareManifest(oldManifest, newManifest);
    }

    public (List<HashPair>, List<HashPair>, List<HashPair>, List<HashPair>) CompareManifest(
        Dictionary<string, string> oldManifest, Dictionary<string, string> newManifest)
    {
        Console.WriteLine("具有相同的名称但哈希值不同（即它们的内容已更改）");
        var list1 = new List<HashPair>();
        foreach (var pair in oldManifest)
            if (newManifest.TryGetValue(pair.Key, out var value) && value != pair.Value)
                list1.Add(new HashPair(pair.Key, pair.Value));
        foreach (var item in list1) Console.Write($"{item}, ");
        Console.WriteLine();

        Console.WriteLine("具有相同的哈希值但名称不同（即它们已被重命名）");
        var list2 = new List<HashPair>();
        foreach (var pair in oldManifest)
            if (newManifest.Any(p => p.Value == pair.Value && p.Key != pair.Key))
                list2.Add(new HashPair(pair.Key, pair.Value));
        foreach (var item in list2) Console.Write($"{item}, ");
        Console.WriteLine();

        Console.WriteLine("在第一个哈希中，但它们的名称和哈希值都不在第二个中（即它们已被删除）");
        var list3 = new List<HashPair>();
        foreach (var pair in oldManifest)
            if (!newManifest.Any(p => p.Key == pair.Key || p.Value == pair.Value))
                list3.Add(new HashPair(pair.Key, pair.Value));
        foreach (var item in list3) Console.Write($"{item}, ");
        Console.WriteLine();

        Console.WriteLine("在第二个哈希表中，但它们的名称和哈希值都不在第一个（即它们已被添加）");
        var list4 = new List<HashPair>();
        foreach (var pair in newManifest)
            if (!oldManifest.Any(p => p.Key == pair.Key || p.Value == pair.Value))
                list4.Add(new HashPair(pair.Key, pair.Value));
        foreach (var item in list4) Console.Write($"{item}, ");
        Console.WriteLine();

        return (list1, list2, list3, list4);
    }

    public Dictionary<string, string> GetManifest(string filename)
    {
        var file = Fs.Path.Combine(BackupDir, filename);
        foreach (var f in Fs.Directory.GetFiles(BackupDir))
            if (f.Contains(".csv") || f.Contains(".json"))
                if (Fs.Path.GetFullPath(f) == Fs.Path.GetFullPath(file))
                    return ReadManifestFile(file);

        return [];
    }

    private Dictionary<string, string> ReadManifestFile(string file)
    {
        Dictionary<string, string> list = [];
        if (file.EndsWith(".csv")) list = CsvTool.ReadCsv(Fs, file);

        if (file.EndsWith(".json")) list = JsonTool.ReadJson(Fs, file);

        return list;
    }

    // ex5
    // Write a program called from_to.py that takes a directory and a manifest file as command-line arguments, then adds, removes, and/or renames files in the directory to restore the state described in the manifest. The program should only perform file operations when it needs to, e.g., it should not delete a file and re-add it if the contents have not changed.
    // 编写一个名为 from_to.py 的程序，该程序接受一个目录和一个清单文件作为命令行参数，然后添加、删除和/或重命名目录中的文件，以恢复清单中描述的状态。该程序应仅在需要时执行文件操作，例如，如果内容没有变化，它不应删除文件并重新添加。
    public void FromTo(string folder, string manifestFile)
    {
        var manifest = GetManifest(manifestFile);
        var fullFolder = Fs.Path.GetFullPath(folder);
        var current = HashAll();
        var (changeList, renameList, deleteList, addList) = CompareManifest(manifest, current);

        foreach (var pair in changeList)
            Fs.File.Copy(Fs.Path.Combine(BackupDir, $"{pair.FileHash}.bck"),
                Fs.Path.Combine(fullFolder, pair.FileName), true);

        foreach (var pair in renameList)
        {
            var filename = current.First(p => p.Value == pair.FileHash).Key;
            Fs.File.Move(Fs.Path.Combine(fullFolder, filename),
                Fs.Path.Combine(fullFolder, pair.FileName), true);
        }

        foreach (var pair in deleteList)
            Fs.File.Copy(Fs.Path.Combine(BackupDir, $"{pair.FileHash}.bck"),
                Fs.Path.Combine(fullFolder, pair.FileName));

        foreach (var pair in addList) Fs.File.Delete(Fs.Path.Combine(fullFolder, pair.FileName));
    }

    // ex6
    // Write a program called file_history.py that takes the name of a file as a command-line argument and displays the history of that file by tracing it back in time through the available manifests.
    // 编写一个名为 file_history.py 的程序，该程序接受文件名作为命令行参数，并通过追踪可用的清单文件来显示该文件的历史记录。
    public string GetFileHistory(string filename)
    {
        var sb = new StringBuilder();
        sb.Append($"{filename}: ");
        var createdFlag = false;
        var deleteFlag = false;
        var oldValue = string.Empty;
        var manifestFiles = Fs.Directory.GetFiles(BackupDir, $"*.{Extension}", SearchOption.TopDirectoryOnly).Order().ToArray();
        for (var i = 0; i < manifestFiles.Length; i++)
        {
            var manifestFile = manifestFiles[i];
            var manifest = GetManifest(manifestFile);
            if (manifest.TryGetValue(filename, out var value))
            {
                sb.Append($"{i}-{value}");
                switch (createdFlag)
                {
                    case false:
                        createdFlag = true;
                        sb.Append("[create]");
                        oldValue = value;
                        break;
                    case true when oldValue == value:
                        sb.Append("[no change]");
                        break;
                    case true when oldValue != value:
                        sb.Append("[change]");
                        oldValue = value;
                        break;
                }
            }
            else if (createdFlag && !deleteFlag)
            {
                sb.Append($"{i}-[delete] -> ");
                deleteFlag = true;
            }
            else if (createdFlag && deleteFlag)
            {
                break;
            }

            if (i != manifestFiles.Length - 2) sb.Append(" -> ");
        }

        sb.Append("latest state");
        return sb.ToString();
    }
    
    // ex7
    // Modify backup.py to load and run a function called pre_commit from a file called pre_commit.py stored in the root directory of the files being backed up. If pre_commit returns True, the backup proceeds; if it returns False or raises an exception, no backup is created.
    // 修改 backup.py 以从正在备份文件的根目录中存储的名为 pre_commit.py 的文件加载并运行名为 pre_commit 的函数。如果 pre_commit 返回 True ，则备份继续；如果返回 False 或引发异常，则不创建备份。
    // private async Task PreCommitAsync()
    // {
    //     var files = Fs.Directory.GetFiles()
    //     if (await CSharpScript.EvaluateAsync<bool>())
    //     {
    //         
    //     }
    // }
}

public static class CsvTool
{
    public static void WriteCsv(string csvPath, Dictionary<string, string> manifest)
    {
        WriteCsv(new FileSystem(), csvPath, manifest);
    }

    public static void WriteCsv(IFileSystem fs, string csvPath, Dictionary<string, string> manifest)
    {
        using var writer = fs.File.OpenWrite(csvPath);
        using var csv = new CsvWriter(new StreamWriter(writer), CultureInfo.InvariantCulture);
        var records = manifest.Select(p => new HashPair(p.Key, p.Value)).ToList();
        csv.WriteRecords(records);
    }

    public static Dictionary<string, string> ReadCsv(string csvPath)
    {
        return ReadCsv(new FileSystem(), csvPath);
    }

    public static Dictionary<string, string> ReadCsv(IFileSystem fs, string csvPath)
    {
        using var reader = fs.File.OpenRead(csvPath);
        using var csv = new CsvReader(new StreamReader(reader), CultureInfo.InvariantCulture);
        var list = csv.GetRecords<HashPair>();
        return list.ToDictionary(p => p.FileName, p => p.FileHash);
    }
}

public static class JsonTool
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public static void WriteJson(string jsonPath, Dictionary<string, string> manifest)
    {
        WriteJson(new FileSystem(), jsonPath, manifest);
    }

    public static void WriteJson(IFileSystem fs, string jsonPath, Dictionary<string, string> manifest)
    {
        var json = JsonSerializer.Serialize(manifest, Options);
        fs.File.WriteAllText(jsonPath, json);
    }

    public static Dictionary<string, string> ReadJson(string jsonPath)
    {
        return ReadJson(new FileSystem(), jsonPath);
    }

    public static Dictionary<string, string> ReadJson(IFileSystem fs, string jsonPath)
    {
        var json = fs.File.ReadAllText(jsonPath);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
    }
}

public static class HashTool
{
    private const int HASH_LEN = 16;

    public static string Default(FileSystemStream f, SHA256 sha)
    {
        return Convert.ToHexString(sha.ComputeHash(f))[..HASH_LEN];
    }

    public static string OurHash(Stream f, MD5 md5)
    {
        return Convert.ToHexString(md5.ComputeHash(f))[..HASH_LEN];
    }
}