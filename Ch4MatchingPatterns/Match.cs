namespace Ch4MatchingPatterns;

public abstract class Match
{
    protected Match Rest { get; set; }

    public bool IsMatch(string text)
    {
        var result = MatchIndex(text);
        return result == text.Length;
    }

    public abstract int? MatchIndex(string text, int start = 0);

    protected Match AddRest(Match pattern)
    {
        pattern.Rest = Rest;
        return pattern;
    }
}

public class Null : Match
{
    public Null()
    {
        Rest = null;
    }

    public override int? MatchIndex(string text, int start = 0)
    {
        return start;
    }
}

public class Literal : Match
{
    public Literal(string pattern, Match? rest = null)
    {
        Pattern = pattern;
        Rest = rest ?? new Null();
    }

    private string Pattern { get; }

    public override int? MatchIndex(string text, int start = 0)
    {
        var end = start + Pattern.Length;
        if (end > text.Length) return null;
        if (text[start..end] != Pattern) return null;

        return Rest.MatchIndex(text, end);
    }
}

public class Any : Match
{
    public Any(Match? rest = null)
    {
        Rest = rest ?? new Null();
    }

    public override int? MatchIndex(string text, int start = 0)
    {
        // 因为索引表示的是字符之间的位置（从 0 到 text.Length），所以结束位置需要包含 text.Length 本身。
        // 举例：对于 "abc"，有效的位置是 0,1,2,3（3 表示在末尾），Any 要尝试从 start 到包括 text.Length 的所有结束位置，
        // 才能允许匹配 0 个字符或匹配到结尾。
        foreach (var i in Enumerable.Range(start, text.Length + 1 - start))
        {
            var end = Rest.MatchIndex(text, i);
            if (end == text.Length) return end;
        }

        return null;
    }
}

public class Either : Match
{
    public Either(Match left, Match right, Match? rest = null)
    {
        Patterns = [left, right];
        Rest = rest ?? new Null();
    }

    public Either(IEnumerable<Match> patterns, Match? rest = null)
    {
        Patterns = new HashSet<Match>(patterns);
        Rest = rest ?? new Null();
    }

    private HashSet<Match> Patterns { get; }

    public override int? MatchIndex(string text, int start = 0)
    {
        foreach (var part in Patterns)
        {
            var end = part.MatchIndex(text, start);
            if (end is null) continue;
            end = Rest.MatchIndex(text, end.Value);
            if (end == text.Length) return end;
        }

        return null;
    }
}