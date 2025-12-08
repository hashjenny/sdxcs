using System.IO.Abstractions.TestingHelpers;
using static Ch10FileArchiver.FileArchiver;

namespace Ch10FileArchiver.Tests;

public class FileArchiverTests
{
    private readonly MockFileSystem fs;

    public FileArchiverTests()
    {
        var files = new Dictionary<string, MockFileData>
        {
            ["a.txt"] = new("aaa"),
            ["b.txt"] = new("bbb"),
            ["sub_dir/c.txt"] = new("ccc")
        };

        fs = new MockFileSystem(files);
    }

    [Fact]
    public void TestNestedExample()
    {
        Assert.True(fs.File.Exists("a.txt"));
        Assert.True(fs.File.Exists("b.txt"));
        Assert.True(fs.File.Exists("sub_dir/c.txt"));
    }

    [Fact]
    public void TestDeletionExample()
    {
        Assert.True(fs.File.Exists("a.txt"));

        // 删除文件
        fs.File.Delete("a.txt");

        Assert.False(fs.File.Exists("a.txt"));
    }

    [Fact]
    public void TestHashing()
    {
        var results = HashAll(fs, ".");

        List<string> files = ["a.txt", "b.txt", "sub_dir/c.txt"];

        Assert.Equal(results.Count, files.Count);
        foreach (var tuple in files.Zip(results))
        {
            Assert.Equal(tuple.First, tuple.Second.FileName);
            Assert.Equal(16, tuple.Second.FileHash.Length);
        }
    }

    [Fact]
    public void TestChanging()
    {
        var results = HashAll(fs, ".");
        var a = results[0].FileName;
        var hash = results[0].FileHash;

        fs.File.AppendAllText(a, "append");
        var results2 = HashAll(fs, ".");
        var hash2 = results2[0].FileHash;
        Assert.Equal("a.txt", a);
        Assert.NotEqual(hash, hash2);
    }

    [Fact]
    public void TestFileArchiver()
    {
        var archiver = new FileArchiver(fs);
        Assert.Equal(3, archiver.Manifest.Count);
        archiver.Backup(fs);
        var backupFiles = fs.Directory.GetFiles("./.backup");
        Assert.Equal(4, backupFiles.Length);
        Assert.Equal(3, backupFiles.Where(file => file.Contains(".bck")).ToList().Count);

        fs.File.AppendAllText(archiver.Manifest[0].FileName, "bbbb");
        Assert.Equal(3, archiver.Manifest.Count);
        archiver.Backup(fs);
        var backup2 = fs.Directory.GetFiles("./.backup");
        Assert.Equal(6, backup2.Length);
        Assert.Equal(4, backup2.Where(file => file.Contains(".bck")).ToList().Count);
        Assert.Equal(2, backup2.Where(file => file.Contains(".csv")).ToList().Count);
    }
}