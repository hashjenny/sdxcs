using Ch4MatchingPatterns;

return;
// ex1
// Modify the parser to handle escape characters, so that (for example) \* is interpreted as a literal ‘*’ character and \\ is interpreted as a literal backslash.
//     修改解析器以处理转义字符，以便（例如） \* 被解释为字面量‘*’字符，而 \\ 被解释为字面量反斜杠。

public enum Token
{
    Literal,
    Any,
    EitherStart,
    EitherEnd
}

public class Tokenizer
{
    // 可见 ASCII 字符：32..126, 包含空格和 '~'，共95个字符
    private static HashSet<char> AllChars { get; } =
        Enumerable.Range(32, 95).Select(x => (char)x).ToHashSet();

    private List<(Token, string?)> Results { get; } = [];

    private bool IsEscape { get; set; }

    private string Current { get; set; } = string.Empty;

    public List<(Token, string?)> Process(string text)
    {
        foreach (var ch in text)
            switch (ch)
            {
                case '*':
                    if (IsEscape)
                    {
                        IsEscape = false;
                        goto default;
                    }

                    addToken(Token.Any);
                    break;
                case '{':
                    if (IsEscape)
                    {
                        IsEscape = false;
                        goto default;
                    }

                    addToken(Token.EitherStart);
                    break;
                case ',':
                    if (IsEscape)
                    {
                        IsEscape = false;
                        goto default;
                    }

                    addToken(null);
                    break;
                case '}':
                    if (IsEscape)
                    {
                        IsEscape = false;
                        goto default;
                    }

                    addToken(Token.EitherEnd);
                    break;
                case '\\':
                    if (IsEscape)
                    {
                        IsEscape = false;
                        goto default;
                    }

                    IsEscape = true;
                    break;
                default:
                    if (AllChars.Contains(ch))
                        Current += ch;
                    else
                        throw new ArgumentException("can't process the token");
                    break;
            }

        addToken(null);
        return Results;
    }

    private void addToken(Token? token)
    {
        if (Current.Length > 0)
        {
            Results.Add((Token.Literal, Current));
            Current = string.Empty;
        }

        if (token is not null) Results.Add((token.Value, null));
    }
}

public static class Parser
{
    public static Match parse(List<(Token, string?)> list)
    {
        if (list.Count == 0) return new Null();

        var current = list[0];
        var restList = list[1..];
        return current.Item1 switch
        {
            Token.Any => parseAny(current.Item2, restList),
            Token.Literal => parseLiteral(current.Item2, restList),
            Token.EitherStart => parseEitherStart(current.Item2, restList),
            _ => throw new ArgumentException("Unknown token type")
        };
    }

    private static Any parseAny(string? text, List<(Token, string?)> restList)
    {
        return new Any(parse(restList));
    }

    private static Literal parseLiteral(string? text, List<(Token, string?)> restList)
    {
        return new Literal(text ?? string.Empty, parse(restList));
    }

    private static Either parseEitherStart(string? text, List<(Token, string?)> restList)
    {
        if (restList.Count < 3
            || restList[0].Item1 is not Token.Literal
            || restList[1].Item1 is not Token.Literal
            || restList[2].Item1 is not Token.EitherEnd)
            throw new Exception("badly-formatted Either");

        var left = new Literal(restList[0].Item2!);
        var right = new Literal(restList[1].Item2!);
        var rest = restList.Count > 3 ? restList[3..] : [];

        return new Either([left, right], parse(rest));
    }
}