using System.Diagnostics;
using System.Reflection;

namespace Ch6RunningTests;

// ex2
// Modify the test framework so that it reports which tests passed, failed, or had errors
// and also reports a summary of how many tests produced each result.
// 修改测试框架，使其报告哪些测试通过、失败或出错，并报告每个结果产生数量的摘要。

// ex3
// Modify the testing tool in this chapter so that if a file of tests contains a function called setup then the tool calls it exactly once before running each test in the file. Add a similar way to register a teardown function.
// 修改本章中的测试工具，以便如果测试文件包含一个名为 setup 的函数，则工具在运行该文件中的每个测试之前调用它一次。添加一种类似的方式来注册 teardown 函数。

// ex4
// Modify the testing tool so that it records how long it takes to run each test. 
// 修改测试工具，使其记录运行每个测试所需的时间。

// ex5
// Modify the testing tool so that if a user provides -s pattern or --select pattern on the command line then the tool only runs tests that contain the string pattern in their name.
// 修改测试工具，以便如果用户在命令行中提供 -s pattern 或 --select pattern ，则该工具仅运行名称中包含字符串 pattern 的测试。
public static class UnitTest
{
    public static event Action? SetUp;
    public static event Action? TearDown;

    public static void RunAllTests(IEnumerable<(string, Action)> tests, string? pattern = null)
    {
        SetUp?.Invoke();
        var results = new Dictionary<Result, List<(string, double)>>
        {
            [Result.Pass] = [],
            [Result.Fail] = [],
            [Result.Error] = []
        };
        Result? resultType = null;
        foreach (var (name, test) in tests)
        {
            if (pattern is not null && !name.Contains(pattern))
            {
                continue;
            }
            var sw = Stopwatch.StartNew();
            try
            {
                test();
                resultType = Result.Pass;
            }
            // 通过反射调用测试方法时会抛出 TargetInvocationException，
            // 若不在调用处或 RunAllTests 中拆包，会把原始的测试失败当做“Error”而不是“Fail”。
            catch (TargetInvocationException tie) when (tie.InnerException is not null)
            {
                HandleException(name, tie.InnerException, out resultType);
            }
            catch (Exception e)
            {
                HandleException(name, e, out resultType);
            }
            finally
            {
                sw.Stop();
                var time = sw.Elapsed.TotalMicroseconds;
                results[resultType!.Value].Add((name, time));
            }
        }


        PrintResult(results);

        TearDown?.Invoke();
    }

    private static void PrintResult(Dictionary<Result, List<(string, double)>> results)
    {
        Console.WriteLine();
        Console.WriteLine("Test run summary:");
        PrintList(Result.Pass, results[Result.Pass]);
        PrintList(Result.Fail, results[Result.Fail]);
        PrintList(Result.Error, results[Result.Error]);
    }

    private static void PrintList(Result resultType, List<(string, double)> resultList)
    {
        var msg = resultType switch
        {
            Result.Pass => "Pass",
            Result.Fail => "Fail",
            Result.Error => "Error",
            _ => throw new ArgumentOutOfRangeException(nameof(resultType), resultType, null)
        };
        Console.WriteLine($"  {msg}:  {resultList.Count}");
        Console.Write("      ");
        foreach (var result in resultList) Console.Write($"<name: {result.Item1}, time: {result.Item2}us>");

        Console.WriteLine();
    }

    private static void HandleException(string testName, Exception e, out Result? resultType)
    {
        if (e is TestException te)
        {
            Console.WriteLine($"  -> Fail: {testName}: {te.Message}");
            resultType = Result.Fail;
        }
        else
        {
            Console.WriteLine($"  -> Error: {testName}: {e.Message}");
            resultType = Result.Error;
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