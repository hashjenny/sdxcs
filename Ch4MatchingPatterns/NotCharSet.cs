namespace Ch4MatchingPatterns;

// Ch5ParsingText
public class NotCharSet : Match
{
    public NotCharSet(string str, Match? rest = null)
    {
        Set = [];
        foreach (var c in str) Set.Add(c);

        Rest = rest ?? new Null();
    }

    public NotCharSet(IEnumerable<char> collection, Match? rest = null)
    {
        Set = new HashSet<char>(collection);
        Rest = rest ?? new Null();
    }

    public NotCharSet(Match? rest = null, params char[] collection)
    {
        Set = new HashSet<char>(collection);
        Rest = rest ?? new Null();
    }

    private HashSet<char> Set { get; }

    public override MatchResult? MatchIndex(string text, int start = 0)
    {
        if (start >= text.Length) return null;
        if (Set.TryGetValue(text[start], out var value)) return null;

        var result = Rest.MatchIndex(text, start + 1);
        if (result is null) return null;

        var captures = new List<string> { text[start].ToString() };
        captures.AddRange(result.Captures);
        return new MatchResult(result.End, captures);
    }
    
}