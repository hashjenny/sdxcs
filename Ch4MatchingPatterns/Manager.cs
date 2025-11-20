namespace Ch4MatchingPatterns;

// Rewrite the matchers so that a top-level object manages a list of matchers, none of which know about any of the others.
// 重写匹配器，使顶层对象管理一组匹配器，其中没有任何一个匹配器知道其他任何匹配器。
public class Manager(params IMatch[] patterns)
{
    private List<IMatch> Patterns { get; } = patterns.ToList();

    public MatchResult? Match(string text)
    {
        return MatchFrom(0, text, 0);
    }

    public bool IsMatch(string text)
    {
        var result = Match(text);
        return result is not null && result.End == text.Length;
    }

    private MatchResult? MatchFrom(int patternIndex, string text, int textIndex)
    {
        if (patternIndex >= Patterns.Count)
            return new MatchResult(textIndex);

        var pattern = Patterns[patternIndex];
        return pattern.MatchIndex(text, textIndex,
            (next, str) => MatchFrom(patternIndex + 1, str, next));
    }
}

public interface IMatch
{
    public MatchResult? MatchIndex(string text, int start, Func<int, string, MatchResult?> restMatch);
}

public class LiteralThing(string pattern) : IMatch
{
    private string Pattern { get; } = pattern;

    public MatchResult? MatchIndex(string text, int start, Func<int, string, MatchResult?> restMatch)
    {
        var end = start + Pattern.Length;
        if (end > text.Length) return null;
        if (text[start..end] != Pattern) return null;

        var result = restMatch(end, text);
        if (result is null) return null;

        var list = new List<string> { text[start..end] };
        list.AddRange(result.Captures);
        return new MatchResult(result.End, list);
    }
}

public class Anything : IMatch
{
    public MatchResult? MatchIndex(string text, int start, Func<int, string, MatchResult?> restMatch)
    {
        // Anything 当前从小到大尝试结束位置，会优先匹配空串（end == start），导致只捕获空字符串。
        // for (var end = start; end <= text.Length; end++)
        // 修复：把循环改为从最大位置向下遍历，优先匹配最长串（能让单独的 Anything 捕获整个输入）。

        for (var end = text.Length; end >= start; end--)
        {
            var result = restMatch(end, text);
            if (result is not null)
            {
                var list = new List<string> { text[start..end] };
                list.AddRange(result.Captures);
                return new MatchResult(result.End, list);
            }
        }

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

    public MatchResult? MatchIndex(string text, int start, Func<int, string, MatchResult?> restMatch)
    {
        foreach (var part in Patterns)
        {
            var result = part.MatchIndex(text, start, restMatch);
            if (result is not null) return result;
        }

        return null;
    }
}

public class OneMoreThing : IMatch
{
    public MatchResult? MatchIndex(string text, int start, Func<int, string, MatchResult?> restMatch)
    {
        if (start >= text.Length) return null;
        for (var end = text.Length; end >= start + 1; end--)
        {
            var result = restMatch(end, text);
            if (result is not null)
            {
                var list = new List<string> { text[start..end] };
                list.AddRange(result.Captures);
                return new MatchResult(result.End, list);
            }
        }

        return null;
    }
}

public class CharSetThing(string str) : IMatch
{
    private HashSet<char> Set { get; } = str.Select(x => x).ToHashSet();

    public MatchResult? MatchIndex(string text, int start, Func<int, string, MatchResult?> restMatch)
    {
        if (start >= text.Length) return null;
        if (!Set.TryGetValue(text[start], out var value)) return null;
        var result = restMatch(start + 1, text);
        if (result is null) return null;

        var captures = new List<string> { value.ToString() };
        captures.AddRange(result.Captures);
        return new MatchResult(result.End, captures);
    }
}

public class RangeThing(char start, char end) : IMatch
{
    private int Start { get; } = start;

    private int End { get; } = end;

    public MatchResult? MatchIndex(string text, int start, Func<int, string, MatchResult?> restMatch)
    {
        if (start >= text.Length) return null;
        var value = (int)text[start];
        if (value < Start || value > End) return null;

        var result = restMatch(start + 1, text);
        if (result is null) return null;

        var captures = new List<string> { text[start].ToString() };
        captures.AddRange(result.Captures);
        return new MatchResult(result.End, captures);
    }
}

public class NotThing(IMatch pattern) : IMatch
{
    private IMatch Pattern { get; } = pattern;

    public MatchResult? MatchIndex(string text, int start, Func<int, string, MatchResult?> restMatch)
    {
        var result = Pattern.MatchIndex(text, start, (next, str) => new MatchResult(0));
        if (result is not null) return null;

        var restResult = restMatch(start, text);
        if (restResult is null) return null;

        var captures = new List<string> { string.Empty };
        captures.AddRange(restResult.Captures);
        return new MatchResult(restResult.End, captures);
    }
}