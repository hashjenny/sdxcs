

// var dir = Path.Combine(AppContext.BaseDirectory, "tests");
// Console.WriteLine(Directory.Exists(dir));
// var fileList = Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly).ToArray();
// foreach (var file in fileList)
// {
//     Console.WriteLine(file);
//     Console.WriteLine(Path.GetFileName(file));
//     Console.WriteLine(Path.GetRelativePath(AppContext.BaseDirectory, file));
// }

using System.Numerics;
using System.Security.Cryptography;
using System.Text;

var groups = await FindGroupsAsync(AppContext.BaseDirectory);
foreach (var filenames in groups.Values)
{
    var duplicates = await FindDuplicatesAsync(filenames.ToArray());
    foreach (var tuple in duplicates)
    {
        Console.WriteLine($"{tuple.Item1} - {tuple.Item2}");
    }

}

var example = "hashing"u8.ToArray();
foreach (var i in Enumerable.Range(1, example.Length))
{
    var sub = example[..i];
    var hash = NaiveHash(sub);
    var hash2 = SHA256.HashData(sub);
    Console.WriteLine($"{hash,2} {Encoding.UTF8.GetString(sub)} {BitConverter.ToString(hash2).Replace("-", "").ToLower()}");

}

const string filePath = "Program.cs";
var fileHashes = ComputeUniqueLineHash(filePath);
PlotHistogram(fileHashes, "hist.png");


return;

async Task<List<(string, string)>> FindDuplicatesAsync(string[] filenames)
{
    var matches = new List<(string, string)>();
    foreach (var leftIndex in Enumerable.Range(0, filenames.Length))
    {
        var left = filenames[leftIndex];
        foreach (var rightIndex in Enumerable.Range(0, leftIndex))
        {
            var right = filenames[rightIndex];
            if (await SameBytesAsync(left, right))
            {
                matches.Add((Path.GetFileName(left), Path.GetFileName(right)));
            }
        }
    }

    return matches;
}

async Task<bool> SameBytesAsync(string left, string right)
{
    var leftBytes = await File.ReadAllBytesAsync(left);
    var rightBytes = await File.ReadAllBytesAsync(right);
    return leftBytes.SequenceEqual(rightBytes);
}

int NaiveHash(byte[] data)
{
    var sum = data.Sum(b => b);
    return sum % 13;
}

async Task<Dictionary<string, HashSet<string>>> FindGroupsAsync(string dir)
{
    var filenames = Directory.EnumerateFiles(dir, "tests/*", SearchOption.TopDirectoryOnly).ToArray();
    var groups = new Dictionary<string, HashSet<string>>();
    foreach (var filename in filenames)
    {
        var stream = File.OpenRead(filename);
        using var hasher = IncrementalHash.CreateHash((HashAlgorithmName.SHA256));
        var buffer = new byte[1024];
        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
           hasher.AppendData(buffer.AsSpan(0, bytesRead)); 
        }
        var code = BitConverter.ToString(hasher.GetHashAndReset());
        if (!groups.TryGetValue(code, out var value))
        {
            value = [];
            groups[code] = value;
        }

        value.Add(filename);

    }
    return groups;
}

// Write a function that calculates the SHA-256 hash code of each unique line of a text file.
// Convert the hex digests of those hash codes to integers.
// Plot a histogram of those integer values with 20 bins.
// 编写一个函数，计算文本文件中每行唯一行的 SHA-256 哈希码。
// 将这些哈希码的十六进制摘要转换为整数。
// 绘制这些整数值的直方图，使用 20 个箱子。

List<BigInteger> ComputeUniqueLineHash(string path)
{
    var uniqueLines = new HashSet<string>(StringComparer.Ordinal);
    foreach (var line in File.ReadLines(path))
    {
        uniqueLines.Add(line);
    }
    var result = new List<BigInteger>(uniqueLines.Count);
    foreach (var line in uniqueLines)
    {
        var bytes = Encoding.UTF8.GetBytes(line);
        var hash = SHA256.HashData(bytes);
        var big = new BigInteger(hash, isUnsigned: true, isBigEndian: true);
        result.Add(big);
    }

    return result;
}

void PlotHistogram(List<BigInteger> hashes, string path, int bins = 20)
{
    if (hashes is null || hashes.Count == 0)
    {
        throw new ArgumentException("没有要绘制的数据。");
    }

    var mask = (BigInteger.One << 53) - 1;
    var values = hashes.Select(h => (double)(h & mask)).ToArray();

    var plt = new ScottPlot.Plot();
    var hist = ScottPlot.Statistics.Histogram.WithBinCount(bins, values);
    var barPlot = plt.Add.Bars(hist.Bins, hist.Counts);

    foreach (var bar in barPlot.Bars)
    {
        bar.Size = hist.FirstBinSize * .8;
    }
    plt.Axes.Margins(bottom:0);
    plt.YLabel("value");
    plt.XLabel("hash");
    plt.SavePng(path, 800, 500);
}
