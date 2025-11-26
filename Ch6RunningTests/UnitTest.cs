using System.Reflection;

namespace Ch6RunningTests;

public static class UnitTest
{
    public static void RunAllTests(IEnumerable<(string, Action)> tests)
    {
        var results = new Dictionary<Result, int>
        {
            [Result.Pass] = 0,
            [Result.Fail] = 0,
            [Result.Error] = 0
        };
        foreach (var (name, test) in tests)
            try
            {
                test();
                results[Result.Pass]++;
            }
            // 通过反射调用测试方法时会抛出 TargetInvocationException，
            // 若不在调用处或 RunAllTests 中拆包，会把原始的测试失败当做“Error”而不是“Fail”。
            catch (TargetInvocationException tie) when (tie.InnerException is not null)
            {
                HandleException(name, tie.InnerException, results);
            }
            catch (Exception e)
            {
                HandleException(name, e, results);
            }

        Console.WriteLine();
        Console.WriteLine("Test run summary:");
        Console.WriteLine($"  Pass:  {results[Result.Pass]}");
        Console.WriteLine($"  Fail:  {results[Result.Fail]}");
        Console.WriteLine($"  Error: {results[Result.Error]}");
    }

    private static void HandleException(string testName, Exception e, Dictionary<Result, int> results)
    {
        if (e is TestException te)
        {
            Console.WriteLine($"  -> Fail: {testName}: {te.Message}");
            results[Result.Fail]++;
        }
        else
        {
            Console.WriteLine($"  -> Error: {testName}: {e.Message}");
            results[Result.Error]++;
        }
    }

    public static void NotNull(object? o)
    {
        if (o is null) throw new TestException($"{nameof(o)} should not be null!");
    }

    public static void Equal(object want, object actual)
    {
        if (!want.Equals(actual)) throw new TestException($"wanted {want} but got {actual}!");
    }
}

public enum Result
{
    Pass,
    Fail,
    Error
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class TestAttribute : Attribute
{
}

public class TestException(string message) : Exception(message)
{
}