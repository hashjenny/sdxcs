using System.Collections.Concurrent;
using System.Numerics;
using System.Reflection;
using HarmonyLib;

namespace Ch9Protocols;

public class Fake : IDisposable
{
    private static readonly ConcurrentDictionary<string, Fake> Instances = new();

    public Fake(MethodInfo original, MethodInfo patchMethod)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(patchMethod);
        Name = original.Name;

        if (original.IsGenericMethodDefinition)
            throw new NotSupportedException("原始方法是泛型定义，请先使用 MakeGenericMethod 构造闭合方法再传入 Fake。");

        Instances[Name] = this;
        Package = new Harmony($"com.fake.{Name}");

        Package.Patch(original, new HarmonyMethod(patchMethod));
    }

    public List<object[]> CallArgs { get; } = [];

    private string Name { get; }
    private Harmony Package { get; }

    public void Dispose()
    {
        Instances.TryRemove(Name, out _);
        Package.UnpatchAll($"com.fake.{Name}");
        Console.WriteLine($"The fake of {Name} is out.");
    }

    public static void RecordArgs(MethodBase? method, params object[] args)
    {
        if (method is null) return;

        if (!Instances.TryGetValue(method.Name, out var fake)) return;

        var copy = new object[args.Length];
        for (var i = 0; i < args.Length; i++) copy[i] = args[i];
        fake.CallArgs.Add(copy);
    }
}

public static class Adder
{
    public static T Add<T>(params T[] args) where T : INumber<T>
    {
        return args.Aggregate((a, b) => a + b);
    }
}

public static class FakeAdder
{
    public static bool PrefixAdd(ref int __result, int[] args, MethodBase __originalMethod)
    {
        Fake.RecordArgs(__originalMethod, args);
        __result = 2;
        return false; // 跳过原始实现
    }
}

public static class Env
{
    private static readonly MethodInfo Method = typeof(Adder).GetMethod(nameof(Adder.Add))!;
    public static readonly MethodInfo Patched = typeof(FakeAdder).GetMethod(nameof(FakeAdder.PrefixAdd))!;

    public static Fake FakeIt()
    {
        return new Fake(Method.MakeGenericMethod(typeof(int)), Patched);
    }
}