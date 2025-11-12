namespace Ch4MatchingPatterns;

// Rewrite the matchers so that a top-level object manages a list of matchers, none of which know about any of the others.
// 重写匹配器，使顶层对象管理一组匹配器，其中没有任何一个匹配器知道其他任何匹配器。
public class Manager(List<IMatch> patterns)
{
    public List<IMatch> Patterns { get; } = patterns;
    // public int CurrentIndex { get; set; } = 0;

    public bool IsMatch(string text)
    {
        return MatchFrom(0, text, 0);
    }

    private bool MatchFrom(int patternIndex, string text, int textIndex)
    {
        if (patternIndex >= Patterns.Count) return textIndex == text.Length;

        var pattern = Patterns[patternIndex];
        return pattern.MatchIndex(text, textIndex, (next, text) => MatchFrom(patternIndex + 1, text, next)) is not null;
    }
}

public interface IMatch
{
    int? MatchIndex(string text, int start, Func<int, string, bool> restMatch);
}

public class Lit(string pattern) : IMatch
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

public class EitherThing(IMatch left, IMatch right) : IMatch
{
    private IMatch Left { get; } = left;
    private IMatch Right { get; } = right;

    public int? MatchIndex(string text, int start, Func<int, string, bool> restMatch)
    {
        foreach (var part in new[] { Left, Right })
        {
            var end = part.MatchIndex(text, start, restMatch);
            if (end is not null) return end;
        }

        return null;
    }
}