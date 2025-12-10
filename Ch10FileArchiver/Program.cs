using System.IO.Abstractions;
using Ch10FileArchiver;
using Microsoft.CodeAnalysis.CSharp.Scripting;

var config = new ArchiverConfiguration
{
    OutputType = OutputFormat.Json,
    ManifestType = ManifestStyle.Number,
    MyHashFunction = HashFunction.Default
};

// var archiver = new FileArchiver(config: config, sourceDir: "./SampleText");
// foreach (var pair in archiver.Manifest) Console.WriteLine($"{pair.Key}-{pair.Value}");
// Console.WriteLine(new string('=', 20));
// archiver.Backup();
//
// FileArchiver.Migrate(new FileSystem(), Path.Combine(AppContext.BaseDirectory, "Code/.backup/00000001.csv"), false);

// var archiver = new FileArchiver(config: config, sourceDir: "SampleText");
// archiver.Backup();
// FileArchiver.Migrate(new FileSystem(), Path.Combine(AppContext.BaseDirectory, "SampleText/.backup/00000001.json"),
//     false);
//
// File.AppendAllText(Path.Combine(archiver.SourceDir, "a.txt"), "hhh");
// File.Move(Path.Combine(archiver.SourceDir, "b.txt"),
//     Path.Combine(archiver.SourceDir, "move.txt"));
// File.Delete(Path.Combine(archiver.SourceDir, "sub_dir/c.txt"));
// File.WriteAllText(Path.Combine(archiver.SourceDir, "sub_dir/d.txt"), "ddd");
// archiver.Backup();
// archiver.CompareManifest("00000001.json", "00000002.json");

var result = await CSharpScript.EvaluateAsync<int>(
    """
    var f = () => 1;
    return f();
    """
    );
Console.WriteLine(result);