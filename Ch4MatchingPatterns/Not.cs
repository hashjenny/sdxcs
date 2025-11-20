namespace Ch4MatchingPatterns;

// Create a matcher that doesn’t match a specified pattern. For example,
// Not(Lit("abc")) only succeeds if the text isn’t “abc”.
// 创建一个不匹配指定模式的匹配器。
// 例如， Not(Lit("abc")) 只有在文本不是“abc”时才成功
public class Not : Match
{
    public Not(Match pattern, Match? rest = null)
    {
        Pattern = pattern;
        Rest = rest ?? new Null();
    }

    private Match Pattern { get; }

    public override MatchResult? MatchIndex(string text, int start = 0)
    {
        var result = Pattern.MatchIndex(text, start);
        if (result is not null) return null;

        var restResult = Rest.MatchIndex(text, start);
        if (restResult is null) return null;

        var captures = new List<string> { string.Empty };
        captures.AddRange(restResult.Captures);
        return new MatchResult(restResult.End, captures);
    }
}