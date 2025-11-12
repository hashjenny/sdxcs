return;

public abstract class Match
{
    protected Match Rest { get; init; }

    public bool IsMatch(string text)
    {
        var result = MatchIndex(text);
        return result == text.Length;
    }

    public abstract int? MatchIndex(string text, int start = 0);
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
        Left = left;
        Right = right;
        Rest = rest ?? new Null();
    }

    private Match Left { get; }
    private Match Right { get; }

    public override int? MatchIndex(string text, int start = 0)
    {
        foreach (var part in new[] { Left, Right })
        {
            var end = part.MatchIndex(text, start);
            if (end is null) continue;
            end = Rest.MatchIndex(text, end.Value);
            if (end == text.Length) return end;
        }

        return null;
    }
}