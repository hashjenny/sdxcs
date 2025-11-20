namespace Ch4MatchingPatterns;

public enum MatchOption
{
    Lazy,
    Greedy
}

public class MatchResult(int end, IEnumerable<string>? captures = null)
{
    public int End { get; } = end;
    public List<string> Captures { get; } = captures is null ? [] : [..captures];
}

public abstract class Match
{
    protected Match Rest { get; init; }

    private MatchResult? GetMatchResult(string text)
    {
        return MatchIndex(text);
    }

    public bool IsMatch(string text)
    {
        var result = GetMatchResult(text);
        return result is not null && result.End == text.Length;
    }

    public abstract MatchResult? MatchIndex(string text, int start = 0);
}

public class Null : Match
{
    public Null()
    {
        Rest = null;
    }

    public override MatchResult? MatchIndex(string text, int start = 0)
    {
        return new MatchResult(start);
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

    public override MatchResult? MatchIndex(string text, int start = 0)
    {
        var end = start + Pattern.Length;
        if (end > text.Length || text[start..end] != Pattern) return null;

        var result = Rest.MatchIndex(text, end);
        if (result is null) return null;

        var captures = new List<string> { text[start..end] };
        captures.AddRange(result.Captures);
        return new MatchResult(result.End, captures);
    }
}

public class Any : Match
{
    public Any(Match? rest = null, MatchOption option = MatchOption.Lazy)
    {
        Rest = rest ?? new Null();
        Option = option;
    }

    public MatchOption Option { get; }

    public override MatchResult? MatchIndex(string text, int start = 0)
    {
        // 因为索引表示的是字符之间的位置（从 0 到 text.Length），所以结束位置需要包含 text.Length 本身。
        // 举例：对于 "abc"，有效的位置是 0,1,2,3（3 表示在末尾），Any 要尝试从 start 到包括 text.Length 的所有结束位置，
        // 才能允许匹配 0 个字符或匹配到结尾。
        if (Rest is Null)
        {
            var end = text.Length;
            var list = new List<string> { text[start..] };
            return new MatchResult(text.Length, list);
        }

        switch (Option)
        {
            case MatchOption.Lazy:
                for (var i = start; i <= text.Length; i++)
                {
                    var result = Rest.MatchIndex(text, i);
                    if (result is null) continue;

                    var captured = text[start..i];
                    var captures = new List<string> { captured };
                    captures.AddRange(result.Captures);
                    return new MatchResult(result.End, captures);
                }

                break;
            case MatchOption.Greedy:
                for (var i = text.Length; i >= start; i--)
                {
                    var result = Rest.MatchIndex(text, i);
                    if (result is null) continue;

                    var captured = text[start..i];
                    var captures = new List<string> { captured };
                    captures.AddRange(result.Captures);
                    return new MatchResult(result.End, captures);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }


        return null;
    }

    // Rewrite Any so that it does not repeatedly re-match text.
    // 重写 Any ，使其不再重复匹配文本。
    // public override MatchResult? MatchIndex(string text, int start = 0)
    // {
    //     for (var i = text.Length; i >= start; i--)
    //     {
    //         var end = Rest.MatchIndex(text, i);
    //         if (end.Item1 == text.Length)
    //             return end;
    //     }
    //
    //     return (null, string.Empty);
    // }
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

    public override MatchResult? MatchIndex(string text, int start = 0)
    {
        foreach (var part in Patterns)
        {
            var partResult = part.MatchIndex(text, start);
            if (partResult is null) continue;
            var restResult = Rest.MatchIndex(text, partResult.End);
            if (restResult is null) continue;

            var captures = new List<string>();
            captures.AddRange(partResult.Captures);
            captures.AddRange(restResult.Captures);
            return new MatchResult(restResult.End, captures);
        }

        return null;
    }
}