using System.Collections;
using System.Text;

namespace Ch9Protocols;

public class NaiveIterator(List<string> lines) : IEnumerable<char>
{
    private List<string> Lines { get; } = lines;

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