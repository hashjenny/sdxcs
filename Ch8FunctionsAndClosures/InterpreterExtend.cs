using Ch7Interpreter;
using static Ch7Interpreter.Interpreter;

namespace Ch8FunctionsAndClosures;

public static class InterpreterExtend
{
    // ex3
    // Modify do_func so that if it is given three arguments instead of two, it uses the first one as the function’s name without requiring a separate "set" instruction.
    // 修改 do_func ，使其在接收到三个参数而不是两个时，将第一个参数用作函数的名称，而无需单独的 "set" 指令。
    // ex5
    // Modify do_func so that if it is given more than one argument, it uses all but the first as the body of the function
    // (i.e., treats everything after the parameter list as an implicit "seq").
    // 修改 do_func ，使其在接收到多个参数时，使用除第一个以外的所有参数作为函数的体
    // （即，将参数列表之后的所有内容视为隐式的 "seq" ）。
    // ex6
    // Modify the interpreter so that programs cannot redefine functions, i.e., so that once a function has been assigned to a variable, that variable’s value cannot be changed.
    // 修改解释器，使得程序不能重定义函数，即一旦函数被赋值给一个变量，该变量的值就不能被改变。
    [Method]
    private static object DoFunc(Dictionary<string, object> env, List<Node> args)
    {
        Check(args.Count >= 2, $"Expected at least two argument, but got {args.Count}");
        if (args.Count == 2) return new ArrayNode([new StringNode("func"), args[0], args[1]]);
        // 0 -> name, 1 -> parameter list, 2.. -> body 
        var body = new ArrayNode(new List<Node> { new StringNode("seq") }.Concat(args[2..]).ToList());
        return DoSet(env, [args[0], (DoFunc(env, [args[1], body]) as ArrayNode)!]);
    }


    [Method]
    private static object DoCall(Dictionary<string, object> env, List<Node> args)
    {
        Check(args.Count >= 1, $"Expected at least one argument, but got {args.Count}");

        // set up the call
        var name = DoAs<string>(env, args[0]);
        var evaluatedArgs = args[1..].Select(node => Do(env, node)).ToList();

        // find the function
        if (env.TryGetValue(name, out var f)
            && f is ArrayNode nodes
            && nodes.Items[0] is StringNode { Value: "func" })
        {
            var parameterNames = nodes.Items[1] as ArrayNode ?? throw new TLLException("should be ArrayNode");
            var body = nodes.Items[2];
            Check(evaluatedArgs.Count == parameterNames.Items.Count,
                "the count of params function and call should be equal.");

            var newEnv = parameterNames.Items.Select(x => (x as StringNode)!.Value).Zip(evaluatedArgs).ToDictionary();
            var tempEnv = new Dictionary<string, object>(env, env.Comparer).Concat(newEnv).ToDictionary();
            return Do(tempEnv, body);
        }

        throw new TLLException("should be function");
    }
}