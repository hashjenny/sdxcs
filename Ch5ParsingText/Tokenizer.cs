using System.Text;
using Ch4MatchingPatterns;

namespace Ch5ParsingText;

// ex1
// Modify the parser to handle escape characters, so that (for example) \* is interpreted as a literal ‘*’ character and \\ is interpreted as a literal backslash.
// 修改解析器以处理转义字符，以便（例如） \* 被解释为字面量‘*’字符，而 \\ 被解释为字面量反斜杠。

// ex2
// Modify the parser so that expressions like [xyz] are interpreted
// to mean “match any one of those three characters”.
// (Note that this is a shorthand for {x,y,z}.)
// 修改解析器，以便像 [xyz] 这样的表达式被解释为“匹配这三个字符中的任意一个”。（注意，这是 {x,y,z} 的简写。）

// ex3
// Modify the parser so that [!abc] is interpreted as “match anything except one of those three characters”.
// 修改解析器，以便 [!abc] 被解释为“匹配除了这三个字符之外的所有字符”。

public enum Token
{
    Literal,
    Any,
    EitherStart,
    EitherEnd,
    NotCharSetStart,
    NotCharSetEnd
}

public class Tokenizer
{
    // 可见 ASCII 字符：32..126, 包含空格和 '~'，共95个字符
    private static HashSet<char> AllChars { get; } =
        Enumerable.Range(32, 95).Select(x => (char)x).ToHashSet();

    private List<(Token, string?)> Results { get; } = [];

    private bool IsEscape { get; set; }

    private bool IsCharSet2 { get; set; }

    private bool IsNegation { get; set; }

    private bool IsInner { get; set; }

    private int SubCount { get; set; }

    private string Current { get; set; } = string.Empty;

    public List<(Token, string?)> Process(string text)
    {
        foreach (var ch in text)
            if (IsEscape)
            {
                IsEscape = false;
                Current += ch;
            }
            else
            {
                switch (ch)
                {
                    case '\\':
                        IsEscape = true;
                        break;
                    case '*':
                        AddToken(Token.Any);
                        break;
                    case '{':
                        IsInner = true;
                        AddToken(Token.EitherStart);
                        break;
                    case ',':
                        AddToken(null);
                        break;
                    case '}':
                        IsInner = false;
                        var sub = Results.GetRange(Results.Count - SubCount, SubCount);
                        Results.RemoveRange(Results.Count - SubCount - 1, SubCount + 1);
                        Results.Add((Token.EitherStart, (SubCount + 1).ToString()));
                        Results.AddRange(sub);
                        SubCount = 0;
                        AddToken(Token.EitherEnd);
                        break;
                    case '[':
                        IsInner = true;
                        IsCharSet2 = true;
                        AddToken(Token.EitherStart);
                        break;
                    case '!':
                        IsCharSet2 = false;
                        IsNegation = true;
                        Results.RemoveAt(Results.Count - 1);
                        AddToken(Token.NotCharSetStart);
                        break;
                    case ']':
                    {
                        IsInner = false;
                        var slice = Results.GetRange(Results.Count - SubCount, SubCount);
                        Results.RemoveRange(Results.Count - SubCount - 1, SubCount + 1);
                        if (IsNegation)
                        {
                            IsNegation = false;
                            Results.Add((Token.NotCharSetStart, SubCount.ToString()));
                            Results.AddRange(slice);
                            SubCount = 0;

                            AddToken(Token.NotCharSetEnd);
                        }

                        if (IsCharSet2)
                        {
                            IsCharSet2 = false;
                            Results.Add((Token.EitherStart, SubCount.ToString()));
                            Results.AddRange(slice);
                            SubCount = 0;
                            AddToken(Token.EitherEnd);
                        }

                        break;
                    }
                    default:
                    {
                        if (AllChars.Contains(ch))
                        {
                            Current += ch;
                            if (IsCharSet2 || IsNegation) AddToken(null);
                        }
                        else
                        {
                            throw new ArgumentException("can't process the token");
                        }

                        break;
                    }
                }
            }

        AddToken(null);
        return Results;
    }


    private void AddToken(Token? token)
    {
        if (Current.Length > 0)
        {
            Results.Add((Token.Literal, Current));
            Current = string.Empty;
            if (IsInner 
                && token is not (Token.EitherStart or Token.NotCharSetStart)) SubCount++;
        }

        if (token is not null) Results.Add((token.Value, null));
    }
}

public static class Parser
{
    public static Match Parse(List<(Token, string?)> list)
    {
        if (list.Count == 0) return new Null();

        var current = list[0];
        var restList = list[1..];
        return current.Item1 switch
        {
            Token.Any => ParseAny(current.Item2, restList),
            Token.Literal => ParseLiteral(current.Item2, restList),
            Token.EitherStart => ParseEitherStart(current.Item2, restList),
            Token.NotCharSetStart => ParseNotCharSetStart(current.Item2, restList),
            _ => throw new ArgumentException("Unknown token type")
        };
    }

    private static Any ParseAny(string? text, List<(Token, string?)> restList)
    {
        return new Any(Parse(restList));
    }

    private static Literal ParseLiteral(string? text, List<(Token, string?)> restList)
    {
        return new Literal(text ?? string.Empty, Parse(restList));
    }

    private static Either ParseEitherStart(string? text, List<(Token, string?)> restList)
    {
        HashSet<Match> patterns = [];
        if (int.TryParse(text, out var size))
        {
            if (restList.Count < size + 1) throw new Exception("badly-formatted Either");
            for (var i = 0; i < size; i++)
                if (restList[i].Item1 is not Token.Literal)
                    throw new Exception("badly-formatted Either");
                else
                    patterns.Add(new Literal(restList[i].Item2!));

            if (restList[size].Item1 is not Token.EitherEnd) throw new Exception("badly-formatted Either");
        }

        var rest = restList.Count > size + 2 ? restList[(size + 2)..] : [];

        return new Either(patterns, Parse(rest));
    }

    private static NotCharSet ParseNotCharSetStart(string? text, List<(Token, string?)> restList)
    {
        StringBuilder patterns = new();
        if (int.TryParse(text, out var size))
        {
            if (restList.Count < size + 1) throw new Exception("badly-formatted Either");
            for (var i = 0; i < size; i++)
                if (restList[i].Item1 is not Token.Literal)
                    throw new Exception("badly-formatted Either");
                else
                    patterns.Append(restList[i].Item2!);

            if (restList[size].Item1 is not Token.NotCharSetEnd) throw new Exception("badly-formatted Either");
        }

        var rest = restList.Count > size + 2 ? restList[(size + 2)..] : [];

        return new NotCharSet(patterns.ToString(), Parse(rest));
    }
}