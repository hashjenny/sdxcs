namespace Ch4MatchingPatterns;

// Create a matcher that doesn’t match a specified pattern. For example,
// Not(Lit("abc")) only succeeds if the text isn’t “abc”.
// 创建一个不匹配指定模式的匹配器。
// 例如， Not(Lit("abc")) 只有在文本不是“abc”时才成功
public class Not : Match
{
    public Not(Match pattern, Match? rest)
    {
        Pattern = pattern;
        Rest = rest ?? new Null();
    }

    private Match Pattern { get; }

    public override int? MatchIndex(string text, int start = 0)
    {
        var end = Pattern.MatchIndex(text, start);
        return end is null ? end : null;
    }
}