using System.Globalization;
using OneOf;

namespace Ch5ParsingText;

// ex4
// Write a function that accepts a string representing nested lists containing numbers and returns the actual list.
// For example, the input [1, [2, [3, 4], 5]] should produce the corresponding Python list.
// 编写一个函数，接受一个表示嵌套列表的字符串，其中包含数字，并返回实际的列表。
// 例如，输入 [1, [2, [3, 4], 5]] 应生成相应的列表。
public enum ListToken
{
    Start,
    End,
    Item
}

public class ListElement : OneOfBase<double, List<ListElement>>
{
    private ListElement(OneOf<double, List<ListElement>> input) : base(input)
    {
    }

    public static implicit operator ListElement(double value)
    {
        return new ListElement(value);
    }

    public static implicit operator ListElement(List<ListElement> value)
    {
        return new ListElement(value);
    }
}

public class NestedList
{
    private List<(ListToken, string?)> Results { get; } = [];
    private string Current { get; set; } = string.Empty;

    public List<(ListToken, string?)> Process(string text)
    {
        text = text.Replace(" ", string.Empty);
        foreach (var ch in text)
            switch (ch)
            {
                case '[':
                    AddToken(ListToken.Start);
                    break;
                case ',':
                    AddToken(null);
                    break;
                case ']':
                    AddToken(ListToken.End);
                    break;
                default:
                    if (char.IsDigit(ch)
                        || ch == '.'
                        || (ch == '-' && Current.Length == 0))
                        Current += ch;
                    else
                        throw new ArgumentException("can't process the token");

                    break;
            }

        AddToken(null);

        return Results;
    }

    private void AddToken(ListToken? token)
    {
        if (Current.Length > 0)
        {
            Results.Add((ListToken.Item, Current));
            Current = string.Empty;
        }

        if (token is not null) Results.Add((token.Value, null));
    }
}

public class ListParser(string text)
{
    private readonly List<(ListToken, string?)> list = new NestedList().Process(text);
    private int Pos { get; set; }

    public List<ListElement> Parse()
    {
        return list.Count == 0 ? throw new ArgumentException("Empty token list") : ParseList();
    }

    private List<ListElement> ParseList()
    {
        if (Pos >= list.Count || list[Pos].Item1 != ListToken.Start)
            throw new ArgumentException("Expected '[' at the start of list");
        var result = new List<ListElement>();
        Pos++;
        while (Pos < list.Count)
        {
            var (token, value) = list[Pos];
            switch (token)
            {
                case ListToken.End:
                    Pos++;
                    return result;
                case ListToken.Start:
                    result.Add(ParseList());
                    break;
                case ListToken.Item:
                    result.Add(ParseNumber(value!));
                    Pos++;
                    break;
                default:
                    throw new ArgumentException($"Unexpected token: {token}");
            }
        }

        throw new ArgumentException("Missing closing ']'");
    }

    private static ListElement ParseNumber(string target)
    {
        return double.TryParse(target, out var result)
            ? result
            : throw new ArgumentException($"Cannot parse number: {target}");
    }

    public static string FormatList(List<ListElement> list)
    {
        var formatItems = new List<string>();
        foreach (var item in list)
        {
            var str = item.Match(
                number => number.ToString(CultureInfo.CurrentCulture),
                FormatList);
            formatItems.Add(str);
        }

        return $"{string.Join(", ", formatItems)}";
    }
}