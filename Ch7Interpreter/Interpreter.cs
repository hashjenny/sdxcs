using System.Diagnostics;
using System.Text.Json;

namespace Ch7Interpreter;

public static class Interpreter
{
    public static Node ParseElement(JsonElement el)
    {
        return el.ValueKind switch
        {
            JsonValueKind.Array => new ArrayNode(el.EnumerateArray().Select(ParseElement).ToList()),
            JsonValueKind.String => new StringNode(el.GetString()!),
            JsonValueKind.Number => new NumberNode(el.GetDouble()),
            // JsonValueKind.True => true,
            // JsonValueKind.False => false,
            // JsonValueKind.Null => null,
            _ => throw new NotSupportedException($"Unsupported JSON token: {el.ValueKind}")
        };
    }

    public static object DoAdd(Dictionary<string, object> env, params List<Node> list)
    {
        Debug.Assert(list.Count == 2);
        var left = Do(env, list[0]) is double
            ? (double)Do(env, list[0])
            : throw new Exception("the type of element should be double");

        var right = Do(env, list[1]) is double
            ? (double)Do(env, list[1])
            : throw new Exception("the type of element should be double");
        return left + right;
    }

    public static object DoAbs(Dictionary<string, object> env, params List<Node> list)
    {
        Debug.Assert(list.Count == 1);
        var val = Do(env, list[0]) is double
            ? (double)Do(env, list[0])
            : throw new Exception("the type of element should be double");
        return Math.Abs(val);
    }

    public static object DoGet(Dictionary<string, object> env, params List<Node> list)
    {
        Debug.Assert(list.Count == 1);
        if (list[0] is StringNode str && env.TryGetValue(str.Value, out var value)) return value;

        throw new Exception("Unknown variable");
    }

    public static object DoSet(Dictionary<string, object> env, params List<Node> args)
    {
        Debug.Assert(args.Count == 2);
        if (args[0] is StringNode str)
        {
            var value = Do(env, args[1]);
            env[str.Value] = value;
            return value;
        }

        throw new Exception("Unknown variable");
    }

    public static object DoSeq(Dictionary<string, object> env, params List<Node> args)
    {
        Debug.Assert(args.Count > 0);
        object? result = null;
        foreach (var item in args) result = Do(env, item);

        return result!;
    }

    public static object Do(Dictionary<string, object> env, Node expr)
    {
        switch (expr)
        {
            case NumberNode num:
                return num.Value;
            case StringNode str:
                return str.Value;
        }

        if (expr is not ArrayNode list) throw new Exception("expr should be array");
        return list.Items[0] switch
        {
            StringNode { Value: "abs" } => DoAbs(env, list.Items[1..]),
            StringNode { Value: "add" } => DoAdd(env, list.Items[1..]),
            StringNode { Value: "get" } => DoGet(env, list.Items[1..]),
            StringNode { Value: "set" } => DoSet(env, list.Items[1..]),
            StringNode { Value: "seq" } => DoSeq(env, list.Items[1..]),
            _ => throw new Exception("Unknown operation")
        };
    }
}