namespace Ch4MatchingPatterns;

// Match Sets of Characters
// 匹配字符集
// Add a new matching class that matches any character from a set, so that Charset('aeiou') matches any lower-case vowel.
// 添加一个新的匹配类，该类可以匹配来自集合的任何字符，以便 Charset('aeiou') 匹配任何小写元音字母。
// Create a matcher that matches a range of characters. For example, Range("a", "z") matches any single lower-case Latin alphabetic character. (This is just a convenience matcher: ranges can always be spelled out in full.)
// 创建一个匹配字符范围的匹配器。例如， Range("a", "z") 匹配任何单个小写拉丁字母字符。（这只是个方便的匹配器：范围总是可以完全拼写出来。）

public class CharSet : Match
{
    public CharSet(string str, Match? rest = null)
    {
        Set = [];
        foreach (var c in str) Set.Add(c);

        Rest = rest ?? new Null();
    }

    public CharSet(IEnumerable<char> collection, Match? rest = null)
    {
        Set = new HashSet<char>(collection);
        Rest = rest ?? new Null();
    }

    public CharSet(Match? rest = null, params char[] collection)
    {
        Set = new HashSet<char>(collection);
        Rest = rest ?? new Null();
    }

    private HashSet<char> Set { get; }

    public override MatchResult? MatchIndex(string text, int start = 0)
    {
        if (start >= text.Length) return null;
        if (!Set.TryGetValue(text[start], out var value)) return null;

        var result = Rest.MatchIndex(text, start + 1);
        if (result is null) return null;

        var captures = new List<string> { value.ToString() };
        captures.AddRange(result.Captures);
        return new MatchResult(result.End, captures);
    }
}

public class Range : Match
{
    public Range(char start, char end, Match? rest = null)
    {
        Start = start;
        End = end;
        Rest = rest ?? new Null();
    }

    private int Start { get; }
    private int End { get; }

    public override MatchResult? MatchIndex(string text, int start = 0)
    {
        if (start >= text.Length) return null;
        var value = (int)text[start];
        if (value < Start || value > End) return null;

        var result = Rest.MatchIndex(text, start + 1);
        if (result is null) return null;

        var captures = new List<string> { text[start].ToString() };
        captures.AddRange(result.Captures);
        return new MatchResult(result.End, captures);
    }
}