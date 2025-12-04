using System.Collections;
using System.Text;

namespace Ch9Protocols;

public class NaiveIterator(List<string> lines) : IEnumerable<char>
{
    private List<string> Lines { get; } = lines;

    // ex3
    // Modify the iterator example so that it handles empty strings correctly,
    // i.e., so that iterating over the list ["a", ""] produces ["a"].
    // 修改迭代器示例，使其能正确处理空字符串，即迭代列表 ["a", ""] 产生 ["a"] 。
    public IEnumerator<char> GetEnumerator()
    {
        return Lines.SelectMany(line => line).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static string Gather(NaiveIterator iterator)
    {
        var sb = new StringBuilder();
        foreach (var item in iterator) sb.Append(item);

        return sb.ToString();
    }
}