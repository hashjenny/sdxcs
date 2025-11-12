namespace Ch4MatchingPatterns;

// Rewrite the matchers so that a top-level object manages a list of matchers, none of which know about any of the others.
// 重写匹配器，使顶层对象管理一组匹配器，其中没有任何一个匹配器知道其他任何匹配器。
public class Manager(params IMatch[] patterns)
{
    private List<IMatch> Patterns { get; } = patterns.ToList();

    public bool IsMatch(string text)
    {
        return MatchFrom(0, text, 0);
    }

    private bool MatchFrom(int patternIndex, string text, int textIndex)
    {
        if (patternIndex >= Patterns.Count) return textIndex == text.Length;

        var pattern = Patterns[patternIndex];
        return pattern.MatchIndex(text, textIndex, (next, str) => MatchFrom(patternIndex + 1, str, next)) is not null;
    }
}

public interface IMatch
{
    int? MatchIndex(string text, int start, Func<int, string, bool> restMatch);
}

public class LiteralThing(string pattern) : IMatch
{
    private string Pattern { get; } = pattern;

    public int? MatchIndex(string text, int start, Func<int, string, bool> restMatch)
    {
        var end = start + Pattern.Length;
        if (end > text.Length) return null;
        if (text[start..end] != Pattern) return null;
        return restMatch(end, text) ? end : null;
    }
}

public class Anything : IMatch
{
    public int? MatchIndex(string text, int start, Func<int, string, bool> restMatch)
    {
        for (var end = start; end <= text.Length; end++)
            if (restMatch(end, text))
                return end;

        return null;
    }
}

public class EitherThing : IMatch
{
    public EitherThing(IMatch left, IMatch right)
    {
        Patterns = [left, right];
    }

    public EitherThing(List<IMatch> patterns)
    {
        Patterns = patterns;
    }

    private List<IMatch> Patterns { get; }

    public int? MatchIndex(string text, int start, Func<int, string, bool> restMatch)
    {
        foreach (var part in Patterns)
        {
            var end = part.MatchIndex(text, start, restMatch);
            if (end is not null) return end;
        }

        return null;
    }
}

public class OneMoreThing : IMatch
{
    public int? MatchIndex(string text, int start, Func<int, string, bool> restMatch)
    {
        for (var end = start + 1; end <= text.Length; end++)
            if (restMatch(end, text))
                return end;

        return null;
    }
}

public class CharSetTing(string str) : IMatch
{
    private HashSet<char> Set { get; } = str.Select(x => x).ToHashSet();

    public int? MatchIndex(string text, int start, Func<int, string, bool> restMatch)
    {
        if (start >= text.Length) return null;
        if (Set.All(x => x != text[start])) return null;
        return restMatch(start + 1, text) ? start + 1 : null;
    }
}

public class RangeThing(char start, char end) : IMatch
{
    private int Start { get; } = start;

    private int End { get; } = end;

    public int? MatchIndex(string text, int start, Func<int, string, bool> restMatch)
    {
        if (start >= text.Length) return null;

        var value = (int)text[start];
        if (value < Start || value > End) return null;

        return restMatch(start + 1, text) ? start + 1 : null;
    }
}