using System.Numerics;

namespace Ch9Protocols;

public class Fake(Func<object[], object>? function = null, object? value = null)
{
    public readonly List<object[]> CallArgs = [];

    protected static List<string> MethodNames { get; } =
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(asm => asm.GetTypes())
            .SelectMany(t => t.GetMethods())
            .Select(m => m.Name)
            .ToList();

    private Func<object[], object>? Function { get; } = function;
    private object? Value { get; } = value;

    public object? Call(params object[] args)
    {
        CallArgs.Add(args);
        return Function is not null ? Function(args) : Value;
    }

    public static Fake FakeIt(string name, Func<object[], object>? func = null, object? value = null)
    {
        if (!MethodNames.Contains(name)) throw new Exception($"could not find method named {name}");

        var fake = new Fake(func, value);
        return fake;
    }
}

public class ContextFake : Fake, IDisposable
{
    public ContextFake(string name, Func<object[], object>? function = null, object? value = null) : base(function,
        value)
    {
        if (!MethodNames.Contains(name)) throw new Exception($"could not find method named {name}");
        Name = name;
        Original = null;
    }

    public string Name { get; }
    public Func<object[], object>? Original { get; set; }

    public void Dispose()
    {
        Console.WriteLine($"The fake of {Name} is out.");
    }
}

public static class Adder
{
    public static T Add<T>(params T[] args) where T : INumber<T>
    {
        return args.Aggregate((a, b) => a + b);
    }
}