using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Xunit.Abstractions;

namespace Ch10FileArchiver.Tests;

public class FileArchiverTests
{
    private readonly MockFileSystem fs;
    private readonly FileSystem fs2;
    private readonly ITestOutputHelper output;

    public FileArchiverTests(ITestOutputHelper outputHelper)
    {
        var files = new Dictionary<string, MockFileData>
        {
            ["a.txt"] = new("aaa"),
            ["b.txt"] = new("bbb"),
            ["sub_dir/c.txt"] = new("ccc")
        };

        fs = new MockFileSystem(files);
        fs2 = new FileSystem();
        output = outputHelper;

        SourceDir = fs2.Path.Combine(AppContext.BaseDirectory, "SampleText");
        BackupDir = fs2.Path.Combine(SourceDir, ".backup");
        archiver = new FileArchiver(fs, sourceDir: ".");
    }

    private string SourceDir { get; }
    private string BackupDir { get; }

    private FileArchiver archiver { get; }


    public void Dispose()
    {
        if (fs2.Directory.Exists(SourceDir)) fs2.Directory.Delete(SourceDir);
    }

    [Fact]
    public void TestFileExist()
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
        var results = archiver.HashAll();

        List<string> files = ["a.txt", "b.txt", "sub_dir/c.txt"];

        Assert.Equal(results.Count, files.Count);
        foreach (var tuple in files.Zip(results))
        {
            Assert.Equal(tuple.First, tuple.Second.Key);
            Assert.Equal(16, tuple.Second.Value.Length);
        }
    }

    [Fact]
    public void TestChanging()
    {
        var results = archiver.HashAll();
        var hash = results.GetValueOrDefault("a.txt");
        Assert.NotNull(hash);

        fs.File.AppendAllText("a.txt", "append");
        var results2 = archiver.HashAll();
        var hash2 = results2.GetValueOrDefault("a.txt");
        Assert.NotNull(hash2);
        Assert.NotEqual(hash, hash2);
    }

    [Fact]
    public async Task TestFileArchiver()
    {
        Assert.Equal(3, archiver.Manifest.Count);
        await archiver.BackupAsync();
        var backupFiles = fs.Directory.GetFiles("./.backup");
        Assert.Equal(4, backupFiles.Length);
        Assert.Equal(3, backupFiles.Where(file => file.Contains(".bck")).ToList().Count);

        await fs.File.AppendAllTextAsync("a.txt", "bbbb");
        Assert.Equal(3, archiver.Manifest.Count);
        await archiver.BackupAsync();
        var backup2 = fs.Directory.GetFiles("./.backup");
        Assert.Equal(6, backup2.Length);
        Assert.Equal(4, backup2.Where(file => file.Contains(".bck")).ToList().Count);
        Assert.Equal(2, backup2.Where(file => file.Contains(".csv")).ToList().Count);
    }

    [Fact]
    public async Task TestFileArchiverCompare()
    {
        await archiver.BackupAsync();
        output.WriteLine(await fs.File.ReadAllTextAsync(fs.Path.Combine(archiver.BackupDir, "00000001.csv")));

        await fs.File.AppendAllTextAsync(fs.Path.Combine(archiver.SourceDir, "a.txt"), "hhh");
        fs.File.Move(fs.Path.Combine(archiver.SourceDir, "b.txt"), "move.txt");
        fs.File.Delete(fs.Path.Combine(archiver.SourceDir, "sub_dir/c.txt"));
        await fs.File.WriteAllTextAsync(fs.Path.Combine(archiver.SourceDir, "sub_dir/d.txt"), "ddd");
        await archiver.BackupAsync();
        output.WriteLine(new string('-', 30));
        output.WriteLine(await fs.File.ReadAllTextAsync(fs.Path.Combine(archiver.BackupDir, "00000002.csv")));

        var (changeList, renameList, deleteList, addList) = archiver.CompareManifest("00000001.csv", "00000002.csv");
        Assert.Single(changeList);
        Assert.Equal("a.txt", changeList[0].FileName);
        Assert.Single(renameList);
        Assert.Equal("b.txt", renameList[0].FileName);
        Assert.Single(deleteList);
        Assert.Equal("sub_dir/c.txt", deleteList[0].FileName);
        Assert.Single(addList);
        Assert.Equal("sub_dir/d.txt", addList[0].FileName);
    }

    [Fact]
    public async Task TestFileArchiverFromTo()
    {
        await archiver.BackupAsync();
        output.WriteLine(await fs.File.ReadAllTextAsync(Path.Combine(archiver.BackupDir, "00000001.csv")));
        Assert.True(fs.File.Exists("a.txt"));
        Assert.True(fs.File.Exists("b.txt"));
        Assert.True(fs.File.Exists("sub_dir/c.txt"));
        Assert.False(fs.File.Exists("sub_dir/d.txt"));
        Assert.Equal("aaa", await fs.File.ReadAllTextAsync("a.txt"));
        Assert.Equal("bbb", await fs.File.ReadAllTextAsync("b.txt"));
        Assert.Equal("ccc", await fs.File.ReadAllTextAsync("sub_dir/c.txt"));

        await fs.File.AppendAllTextAsync("a.txt", "hhh");
        fs.File.Move("b.txt", "move.txt");
        fs.File.Delete("sub_dir/c.txt");
        await fs.File.WriteAllTextAsync("sub_dir/d.txt", "ddd");

        await archiver.BackupAsync();
        output.WriteLine(new string('-', 30));
        output.WriteLine(await fs.File.ReadAllTextAsync(Path.Combine(archiver.BackupDir, "00000002.csv")));
        Assert.True(fs.File.Exists("a.txt"));
        Assert.False(fs.File.Exists("b.txt"));
        Assert.True(fs.File.Exists("move.txt"));
        Assert.False(fs.File.Exists("sub_dir/c.txt"));
        Assert.True(fs.File.Exists("sub_dir/d.txt"));
        Assert.Equal("aaahhh", await fs.File.ReadAllTextAsync("a.txt"));
        Assert.Equal("bbb", await fs.File.ReadAllTextAsync("move.txt"));
        Assert.Equal("ddd", await fs.File.ReadAllTextAsync("sub_dir/d.txt"));

        await archiver.FromToAsync(archiver.SourceDir, "00000001.csv");
        Assert.True(fs.File.Exists("a.txt"));
        Assert.True(fs.File.Exists("b.txt"));
        Assert.True(fs.File.Exists("sub_dir/c.txt"));
        Assert.False(fs.File.Exists("sub_dir/d.txt"));
        Assert.Equal("aaa", await fs.File.ReadAllTextAsync("a.txt"));
        Assert.Equal("bbb", await fs.File.ReadAllTextAsync("b.txt"));
        Assert.Equal("ccc", await fs.File.ReadAllTextAsync("sub_dir/c.txt"));
    }

    [Fact]
    public async Task TestFileArchiverHistory()
    {
        await archiver.BackupAsync();
        Assert.True(fs.File.Exists("a.txt"));
        Assert.Equal("aaa", await fs.File.ReadAllTextAsync("a.txt"));

        fs.File.Move("b.txt", "move.txt");
        await archiver.BackupAsync();
        Assert.True(fs.File.Exists("a.txt"));

        await fs.File.AppendAllTextAsync("a.txt", "hhh");
        await archiver.BackupAsync();
        Assert.True(fs.File.Exists("a.txt"));
        Assert.Equal("aaahhh", await fs.File.ReadAllTextAsync("a.txt"));

        fs.File.Delete("sub_dir/c.txt");
        await archiver.BackupAsync();
        Assert.True(fs.File.Exists("a.txt"));
        Assert.Equal("aaahhh", await fs.File.ReadAllTextAsync("a.txt"));

        await fs.File.AppendAllTextAsync("a.txt", "vvv");
        await archiver.BackupAsync();
        Assert.True(fs.File.Exists("a.txt"));
        Assert.Equal("aaahhhvvv", await fs.File.ReadAllTextAsync("a.txt"));

        fs.File.Delete("a.txt");
        await archiver.BackupAsync();
        Assert.False(fs.File.Exists("a.txt"));

        await fs.File.WriteAllTextAsync("sub_dir/d.txt", "ddd");
        await archiver.BackupAsync();
        Assert.False(fs.File.Exists("a.txt"));

        var history = archiver.GetFileHistory("a.txt");
        output.WriteLine(history);
    }

    [Fact]
    public async Task TestFileArchiverPreCommit()
    {
        await fs.File.WriteAllTextAsync("PreCommit.csx", """
                                                         var f = () => false;
                                                         return f();
                                                         """);
        var r = await Record.ExceptionAsync(() => archiver.BackupAsync());
        Assert.NotNull(r);

        await fs.File.WriteAllTextAsync("PreCommit.csx", """
                                                         var f = () => true;
                                                         return f();
                                                         """);
        var r2 = await Record.ExceptionAsync(() => archiver.BackupAsync());
        Assert.Null(r2);
    }
}