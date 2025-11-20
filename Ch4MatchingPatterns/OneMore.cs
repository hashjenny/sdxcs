namespace Ch4MatchingPatterns;

// Extend the regular expression matcher to support +, meaning “match one or more characters”.
// 扩展正则表达式匹配器以支持 + ，表示“匹配一个或多个字符”。
public class OneMore : Match
{
    public OneMore(Match? rest = null)
    {
        Rest = rest ?? new Null();
    }

    public override MatchResult? MatchIndex(string text, int start = 0)
    {
        if (text.Length <= start) return null;
        if (Rest is Null)
        {
            var end = text.Length;
            var list = new List<string> { text[start..] };
            return new MatchResult(text.Length, list);
        }

        for (var i = start + 1; i <= text.Length; i++)
        {
            var result = Rest.MatchIndex(text, i);
            if (result is null) continue;

            var captured = text[start..i];
            var captures = new List<string> { captured };
            captures.AddRange(result.Captures);
            return new MatchResult(result.End, captures);
        }

        return null;
    }
}