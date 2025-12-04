using System.Diagnostics;

namespace Ch9Protocols;

// ex2
// Create a context manager called Timer that reports how long it has been since a block of code started running:
// 创建一个名为 Timer 的上下文管理器，用于报告代码块开始运行后经过的时间
public class Ex2Timer : IDisposable
{
    public Ex2Timer()
    {
        Watcher = new Stopwatch();
        Watcher.Start();
    }

    private Stopwatch Watcher { get; }

    public void Dispose()
    {
        Watcher.Stop();
        Console.WriteLine($"run {Watcher.Elapsed.Milliseconds} ms.");
    }
}