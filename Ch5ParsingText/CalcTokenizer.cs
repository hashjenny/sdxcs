using System.Text;
using OneOf;

namespace Ch5ParsingText;

// ex5
// Write a function that accepts a string consisting of numbers and the basic arithmetic operations +, -, *, and /,
// and produces a nested structure showing the operations in the correct order.
// For example, 1 + 2 * 3 should produce ["+", 1, ["*", 2, 3]].
// 编写一个函数，该函数接受一个由数字和基本算术运算符 + 、 - 、 * 和 / 组成的字符串，并生成一个嵌套结构来显示正确的运算顺序。
// 例如， 1 + 2 * 3 应该生成 ["+", 1, ["*", 2, 3]] 。
using TokenPair = (CalcToken, double?);

public enum CalcToken
{
    Number,
    Add,
    Sub,
    Mul,
    Div
}

public class CalcElement : OneOfBase<TokenPair, List<CalcElement>>
{
    private CalcElement(OneOf<TokenPair, List<CalcElement>> input) : base(input)
    {
    }

    public static implicit operator CalcElement(TokenPair value)
    {
        return new CalcElement(value);
    }

    public static implicit operator CalcElement(List<CalcElement> value)
    {
        return new CalcElement(value);
    }
}

public class CalcTokenizer
{
    public CalcTokenizer(string text)
    {
        Process(text);
    }

    private List<TokenPair> TokenPairList { get; } = [];
    private List<CalcElement>? CalcElements { get; set; }
    private string Current { get; set; } = string.Empty;

    private void Process(string text)
    {
        text = text.Replace(" ", string.Empty);
        foreach (var ch in text)
            switch (ch)
            {
                case '+':
                    AddToken(CalcToken.Add);
                    break;
                case '-':
                    AddToken(CalcToken.Sub);
                    break;
                case '*':
                    AddToken(CalcToken.Mul);
                    break;
                case '/':
                    AddToken(CalcToken.Div);
                    break;
                default:
                    if (char.IsDigit(ch)
                        || ch == '.')
                        Current += ch;
                    else
                        throw new ArgumentException("can't process the token");
                    break;
            }

        AddToken(null);
    }

    private void AddToken(CalcToken? token)
    {
        if (Current.Length > 0)
        {
            if (double.TryParse(Current, out var value))
            {
                TokenPairList.Add((CalcToken.Number, value));
                Current = string.Empty;
            }
            else
            {
                throw new ArgumentException("Not a number!");
            }
        }

        if (token is not null) TokenPairList.Add((token.Value, null));
    }

    public List<CalcElement> Parser()
    {
        #region process * and /

        if (TokenPairList.Count <= 0 || TokenPairList[0].Item1 is not CalcToken.Number)
            throw new Exception("not a calc list");
        var stack = new Stack<CalcElement>();
        stack.Push(TokenPairList[0]);
        for (var i = 1; i < TokenPairList.Count; i++)
        {
            var item = TokenPairList[i];

            switch (item.Item1)
            {
                case CalcToken.Number:
                    stack.Push(item);
                    break;
                case CalcToken.Add or CalcToken.Sub:
                    var preItem = stack.Pop();
                    var nextItem = TokenPairList[i + 1];
                    stack.Push(item);
                    stack.Push(preItem);
                    stack.Push(nextItem);
                    i++;
                    break;
                case CalcToken.Mul or CalcToken.Div:
                    var pre = stack.Pop();
                    var next = TokenPairList[i + 1];
                    var newList = new List<CalcElement> { item, pre, next };
                    stack.Push(newList);
                    i++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        var parseResult = stack.Reverse().ToList();

        #endregion

        #region covert to a list with 3 items

        stack = new Stack<CalcElement>();
        var j = 0;
        while (j < parseResult.Count && (parseResult.Count > 3 || stack.Count != 0))
        {
            var item = parseResult[j];
            // put the operator to the bottom of stack
            if (item.IsT0)
            {
                var target = item.AsT0;
                if (target.Item1 is CalcToken.Add or CalcToken.Sub)
                {
                    if (stack.Count == 0)
                    {
                        stack.Push(item);
                        parseResult.RemoveAt(j);
                    }
                    else
                    {
                        j++;
                    }
                }


                if (target.Item1 is CalcToken.Number)
                {
                    if (stack.Count != 0)
                    {
                        stack.Push(item);
                        parseResult.RemoveAt(j);
                    }
                    else
                    {
                        j++;
                    }
                }
            }

            if (item.IsT1)
            {
                if (stack.Count != 0)
                {
                    stack.Push(item);
                    parseResult.RemoveAt(j);
                }
                else
                {
                    j++;
                }
            }


            if (stack.Count >= 3)
            {
                parseResult.Insert(0, stack.Reverse().ToList());
                j = 0;
                stack = new Stack<CalcElement>();
            }
        }

        #endregion

        for (var i = 0; i < parseResult.Count; i++)
        {
            var item = parseResult[i];
            if (item is { IsT0: true, AsT0.Item1: CalcToken.Add or CalcToken.Sub })
            {
                parseResult.Insert(0, item);
                parseResult.RemoveAt(i + 1);
                break;
            }
        }

        return parseResult;
    }

    public static string Format(List<CalcElement> list)
    {
        return FormatList(list) + '\n';
    }

    public void Print()
    {
        CalcElements ??= Parser();
        Console.WriteLine(Format(CalcElements));
    }

    private static string FormatList(List<CalcElement> list)
    {
        StringBuilder sb = new();
        sb.Append('[');
        for (var i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (item.IsT0)
                switch (item.AsT0.Item1)
                {
                    case CalcToken.Number:
                        sb.Append(item.AsT0.Item2);
                        break;
                    case CalcToken.Add:
                        sb.Append('+');
                        break;
                    case CalcToken.Sub:
                        sb.Append('-');
                        break;
                    case CalcToken.Mul:
                        sb.Append('*');
                        break;
                    case CalcToken.Div:
                        sb.Append('/');
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            else if (item.IsT1) sb.Append(FormatList(item.AsT1));

            if (i < list.Count - 1) sb.Append(", ");
        }

        sb.Append(']');
        return sb.ToString();
    }

    public double Calculate()
    {
        CalcElements ??= Parser();
        return CalcList(CalcElements);
    }

    private static double CalcList(CalcElement element)
    {
        if (element.IsT0)
        {
            var pair = element.AsT0;
            return pair.Item1 is CalcToken.Number ? pair.Item2!.Value : throw new Exception("not a list");
        }

        var ele = element.AsT1;
        var opElement = ele[0];
        var num1 = CalcList(ele[1]);
        var num2 = CalcList(ele[2]);

        var op = opElement.AsT0;
        return op.Item1 switch
        {
            CalcToken.Add => num1 + num2,
            CalcToken.Sub => num1 - num2,
            CalcToken.Mul => num1 * num2,
            CalcToken.Div => num1 / num2,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}