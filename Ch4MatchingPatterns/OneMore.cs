namespace Ch4MatchingPatterns;

// Extend the regular expression matcher to support +, meaning “match one or more characters”.
// 扩展正则表达式匹配器以支持 + ，表示“匹配一个或多个字符”。
public class OneMore : Match
{
    public OneMore(Match? rest = null)
    {
        Rest = rest ?? new Null();
    }

    public override int? MatchIndex(string text, int start = 0)
    {
        if (text.Length <= start) return null;
        foreach (var i in Enumerable.Range(start + 1, text.Length + 1 - start))
        {
            var end = Rest.MatchIndex(text, i);
            if (end == text.Length) return end;
        }

        return null;
    }
}