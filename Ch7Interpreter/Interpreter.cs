#define TRACE
#undef TRACE

using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Ch7Interpreter;

public static class Interpreter
{
    private static Dictionary<string, Func<Dictionary<string, object>, List<Node>, object>> FuncDict { get; } =
        // Assembly
        //     .GetExecutingAssembly()
        //     .GetTypes()
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(asm => asm.GetTypes())
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
            .Where(m => m.GetCustomAttribute<MethodAttribute>() is not null
                        && m.ReturnType == typeof(object)
                        && m.GetParameters().Length == 2
                        && m.GetParameters()[0].ParameterType == typeof(Dictionary<string, object>)
            )
            .ToDictionary(m => m.Name.ToLowerInvariant()[2..],
                m => (Func<Dictionary<string, object>, List<Node>, object>)((env2, list) =>
                {
                    var args = new object[] { env2, list };
                    return m.Invoke(null, args)!;
                }));

    public static Node ParseElement(JsonElement el)
    {
        return el.ValueKind switch
        {
            JsonValueKind.Array => new ArrayNode(el.EnumerateArray().Select(ParseElement).ToList()),
            JsonValueKind.String => new StringNode(el.GetString()!),
            JsonValueKind.Number => new NumberNode(el.GetDouble()),
            JsonValueKind.True => new BoolNode(true),
            JsonValueKind.False => new BoolNode(false),
            JsonValueKind.Null => new NullNode(),
            JsonValueKind.Undefined => new NullNode(),
            _ => throw new NotSupportedException($"Unsupported JSON token: {el.ValueKind}")
        };
    }

    public static void PrintEnv(Dictionary<string, object> env)
    {
        Console.WriteLine(">>>>>>>>Env<<<<<<<<");
        foreach (var (key, value) in env)
        {
            Console.Write($"{key} --->  ");
            switch (value)
            {
                case double d:
                    Console.WriteLine(d);
                    break;
                case string s:
                    Console.WriteLine(s);
                    break;
                case bool b:
                    Console.WriteLine(b);
                    break;
                case null:
                    Console.WriteLine("None");
                    break;
                case double[] arr:
                    Console.Write("[");
                    foreach (var d in arr) Console.Write($"{d}, ");

                    Console.WriteLine("]");
                    break;
                case ArrayNode nodes:
                    Console.WriteLine(Format(nodes));
                    break;
            }
        }
    }

    private static string Format(Node expr)
    {
        return expr switch
        {
            NumberNode num => $"{num.Value}, ",
            StringNode s => $"{s.Value}, ",
            BoolNode b => $"{b.Value}, ",
            ArrayNode arr => FormatList(arr.Items),
            _ => "None"
        };
    }

    private static string FormatList(List<Node> args)
    {
        var sb = new StringBuilder();
        sb.Append('[');
        for (var i = 0; i < args.Count; i++)
        {
            var node = args[i];
            var str = node switch
            {
                NumberNode num => $"{num.Value}",
                StringNode s => $"{s.Value}",
                BoolNode b => $"{b.Value}",
                ArrayNode arr => FormatList(arr.Items),
                _ => "None"
            };
            sb.Append(str);
            if (i != args.Count - 1) sb.Append(", ");
        }

        sb.Append(']');
        return sb.ToString();
    }

    // ex2
    // Define a new exception class called TLLException.
    // 定义一个名为 TLLException 的新异常类。
    // Write a utility function called check that raises a TLLException with a useful error message when there’s a problem.
    // 编写一个名为 check 的工具函数，当出现问题时，它将引发一个 TLLException 并附带一个有用的错误消息。
    // Add a catch statement to handle these errors.
    // 添加一个 catch 语句来处理这些错误。
    public static void Check(bool value, string msg)
    {
        if (!value) throw new TLLException(msg);
    }

    // ex4
    // Add a --trace command-line flag to the interpreter.
    // When enabled, it makes TLL print a message showing each function call and its result.
    // 为解释器添加一个 --trace 命令行标志。启用后，它会使 TLL 打印出显示每个函数调用及其结果的消息。
    public static object Do(Dictionary<string, object> env, Node expr)
    {
        switch (expr)
        {
            case NumberNode num:
                return num.Value;
            case StringNode str:
                return str.Value;
            case BoolNode b:
                return b.Value;
            case NullNode:
                return "None";
            case ArrayNode list:
                if (list.Items[0] is not StringNode stringNode) throw new Exception("Unknown operation");

                if (FuncDict.TryGetValue(stringNode.Value, out var func))
                {
                    var result = func(env, list.Items[1..]);
#if TRACE
                    Console.Write(Format(list));
                    Console.WriteLine($"  |>  {result}");
#endif
                    return result;
                }

                break;
        }

        throw new Exception("Unknown operation");
    }

    public static T DoAs<T>(Dictionary<string, object> env, Node expr)
    {
        var result = Do(env, expr);
        if (result is T t) return t;
        throw new Exception($"the type of element should be {nameof(T)}");
    }

    [Method]
    private static object DoAdd(Dictionary<string, object> env, List<Node> args)
    {
        Check(args.Count >= 2, $"Expected at least two argument, but got {args.Count}");
        var left = DoAs<double>(env, args[0]);
        var right = DoAs<double>(env, args[1]);
        return left + right;
    }

    [Method]
    private static object DoAbs(Dictionary<string, object> env, List<Node> args)
    {
        Check(args.Count >= 1, $"Expected at least one argument, but got {args.Count}");
        var val = DoAs<double>(env, args[0]);
        return Math.Abs(val);
    }

    [Method]
    private static object DoGet(Dictionary<string, object> env, List<Node> args)
    {
        Check(args.Count >= 1, $"Expected at least one argument, but got {args.Count}");
        var x = DoAs<string>(env, args[0]);
        return env.TryGetValue(x, out var value) ? value : throw new Exception("Unknown variable");
    }


    [Method]
    public static object DoSet(Dictionary<string, object> env, List<Node> args)
    {
        Check(args.Count >= 2, $"Expected at least two argument, but got {args.Count}");
        var name = DoAs<string>(env, args[0]);
        var value = Do(env, args[1]);
        if (value is ArrayNode arr
            && arr.Items[0] is StringNode { Value: "func" }
            && env.ContainsKey(name))
            throw new TLLException($"Env already had a function value named {name}");
        env[name] = value;
        return value;
    }

    [Method]
    private static object DoSeq(Dictionary<string, object> env, List<Node> args)
    {
        Check(args.Count >= 1, $"Expected at least one argument, but got {args.Count}");
        object? result = null;
        foreach (var item in args) result = Do(env, item);

        return result!;
    }

    // ex1
    // Implement fixed-size, one-dimensional arrays: ["array", 10] creates an array of 10 elements,
    // while other instructions that you design get and set particular array elements by index.
    // 实现固定大小的单维数组： ["array", 10] 创建一个包含 10 个元素的数组，而您设计的其他指令通过索引获取和设置特定的数组元素。
    [Method]
    private static object DoArray(Dictionary<string, object> env, List<Node> args)
    {
        Check(args.Count >= 1, $"Expected at least one argument, but got {args.Count}");
        var len = (int)DoAs<double>(env, args[0]);
        return new double[len];
    }

    [Method]
    private static object DoGetArr(Dictionary<string, object> env, List<Node> args)
    {
        Check(args.Count >= 2, $"Expected at least two argument, but got {args.Count}");
        var str = DoAs<string>(env, args[0]);
        if (env.TryGetValue(str, out var value)
            && value is double[] arr)
        {
            var index = (int)DoAs<double>(env, args[1]);
            return arr[index];
        }

        throw new Exception("Unknown variable");
    }

    [Method]
    private static object DoSetArr(Dictionary<string, object> env, List<Node> args)
    {
        Check(args.Count >= 3, $"Expected at least three argument, but got {args.Count}");
        var str = DoAs<string>(env, args[0]);
        if (env.TryGetValue(str, out var value)
            && value is double[] arr)
        {
            var index = (int)DoAs<double>(env, args[1]);
            var num = DoAs<double>(env, args[2]);
            arr[index] = num;
            return num;
        }

        throw new Exception("Unknown variable");
    }

    // ex3
    // 为解释器添加if, leq,  print 和 repeat 命令
    [Method]
    private static object DoPrint(Dictionary<string, object> env, List<Node> args)
    {
        foreach (var node in args)
        {
            var result = Do(env, node);
            Console.Write($"{result} ");
        }

        Console.WriteLine();
        return new NullNode();
    }

    [Method]
    private static object DoRepeat(Dictionary<string, object> env, List<Node> args)
    {
        Check(args.Count >= 2, $"Expected at least two argument, but got {args.Count}");
        var times = (int)DoAs<double>(env, args[0]);
        object? result = null;
        for (var i = 0; i < times; i++) result = Do(env, args[1]);
        return result ?? new NullNode();
    }

    [Method]
    private static object DoIf(Dictionary<string, object> env, List<Node> args)
    {
        Check(args.Count >= 3, $"Expected at least three argument, but got {args.Count}");
        var b = DoAs<bool>(env, args[0]);
        return Do(env, b ? args[1] : args[2]);
    }

    [Method]
    private static object DoLeq(Dictionary<string, object> env, List<Node> args)
    {
        Check(args.Count >= 2, $"Expected at least three argument, but got {args.Count}");
        var a = DoAs<double>(env, args[0]);
        var b = DoAs<double>(env, args[1]);
        return a < b;
    }

    // ex5
    // Implement a while loop instruction. Your implementation can use either a Python while loop or recursion.
    // 实现一个 while 循环指令。你的实现可以使用 Python while 循环或递归。
    [Method]
    private static object DoWhile(Dictionary<string, object> env, List<Node> args)
    {
        Check(args.Count >= 2, $"Expected at least two argument, but got {args.Count}");
        object? result = null;
        while (DoAs<bool>(env, args[0])) result = Do(env, args[1]);
        return result ?? new NullNode();
    }
}