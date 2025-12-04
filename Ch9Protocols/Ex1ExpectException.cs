using System.Runtime.ExceptionServices;

namespace Ch9Protocols;

// ex1
// Create a context manager that works like pytest.raises from the pytest module,
// i.e., that does nothing if an expected exception is raised within its scope but fails with an assertion error if that kind of exception is not raised.
// 创建一个上下文管理器，使其工作方式类似于 pytest 模块中的 pytest.raises ，
// 即在其作用范围内如果引发预期异常则不执行任何操作，但如果未引发该类型的异常则通过断言错误失败。
public class Ex1ExpectException<T> : IDisposable where T : Exception
{
    public Ex1ExpectException(string? message = null, bool thrown = false)
    {
        Message = message;
        ExceptionThrown = thrown;
        Handler = OnFirstChanceException!;
        AppDomain.CurrentDomain.FirstChanceException += Handler;
    }

    private string? Message { get; }
    private bool ExceptionThrown { get; set; }
    private T? CaughtException { get; set; }
    private EventHandler<FirstChanceExceptionEventArgs>? Handler { get; set; }


    public void Dispose()
    {
        if (Handler is not null)
        {
            AppDomain.CurrentDomain.FirstChanceException -= Handler;
            Handler = null;
        }

        if (!ExceptionThrown)
            throw new AssertException(
                $"Expected exception of type {nameof(T)} was not thrown.");

        if (!string.IsNullOrEmpty(Message)
            && CaughtException is not null
            && CaughtException.Message.Contains(Message!))
            throw new AssertException($"""
                                       Exception was thrown but message does not contain expected text.
                                       Expected to contain: {Message}
                                       Actual message: {CaughtException.Message}
                                       """);
    }

    private void OnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
    {
        if (e.Exception is T ex && !ExceptionThrown)
        {
            ExceptionThrown = true;
            CaughtException = ex;
        }
    }
}

public class AssertException(string message) : Exception(message);