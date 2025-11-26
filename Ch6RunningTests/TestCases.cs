namespace Ch6RunningTests;

public class TestCases
{
    [Test]
    public static void NotNullPass()
    {
        var a = string.Empty;
        UnitTest.NotNull(a);
    }

    [Test]
    public static void NotNullPail()
    {
        string? a = null;
        UnitTest.NotNull(a);
    }

    [Test]
    public static void EqualPass()
    {
        int[] arr = [1, 2, 3];
        UnitTest.Equal(3, arr.Length);
    }


    [Test]
    public static void EqualPail()
    {
        var a = 2;
        UnitTest.Equal(3, a);
    }
    
    [Test]
    public static void NullReferenceError()
    {
        string? s = null;
        // 触发 NullReferenceException
        _ = s!.ToString();
    }
    
    [Test]
    public static void DivideByZeroError()
    {
        var zero = 0;
        // 运行时触发 DivideByZeroException
        var _ = 1 / zero;
    }
    
    [Test]
    public static void IndexOutOfRangeError()
    {
        var arr = new int[0];
        // 触发 IndexOutOfRangeException
        var _ = arr[1];
    }
}