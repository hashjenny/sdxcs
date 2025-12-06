using static Ch9Protocols.Env;

namespace Ch9Protocols.Tests;

public class ProtocolsTest
{
    #region Fake

    [Fact]
    public void test_with_real_function()
    {
        Assert.Equal(5, Adder.Add(2, 3));
    }

    [Fact]
    public void test_with_fixed_return_value()
    {
        using var f = FakeIt();
        Assert.Equal(2, Adder.Add(1, 2, 3, 4, 5, 6));
    }

    [Fact]
    public void test_fake_records_calls()
    {
        using var f = FakeIt();

        Assert.Equal(2, Adder.Add(1, 2, 3, 4, 5, 6));
        Assert.Equal(2, Adder.Add(1, 2, 9));
        Assert.Equal(2, f.CallArgs.Count);
    }


    [Fact]
    public void test_context()
    {
        using (var f = FakeIt())
        {
            Assert.Equal(2, Adder.Add(1, 2, 3));
            Assert.Equal(2, Adder.Add(1, 2, 9));
            Assert.Equal(2, f.CallArgs.Count);
        }

        Assert.Equal(6, Adder.Add(1, 2, 3));
    }

    #endregion

    #region Iterator

    [Fact]
    public void test_naive_buffer()
    {
        var buffer = new NaiveIterator(["ab", "c"]);
        Assert.Equal("abc", NaiveIterator.Gather(buffer));
    }

    [Fact]
    public void test_naive_buffer_empty_string()
    {
        var buffer = new NaiveIterator(["a", ""]);
        Assert.Equal("a", NaiveIterator.Gather(buffer));
    }

    #endregion

    #region Ex1

    [Fact]
    public async Task TestEx1Async()
    {
        var exception = await Record.ExceptionAsync(() =>
        {
            using var ex = new Ex1ExpectException<ArgumentException>();
            throw new InvalidOperationException("错误的异常类型");
        });
        Assert.IsType<AssertException>(exception);
    }

    [Fact]
    public async Task TestEx1Async_2()
    {
        var exception = await Record.ExceptionAsync(() =>
        {
            using var ex = new Ex1ExpectException<InvalidOperationException>();
            throw new InvalidOperationException("错误的异常类型");
        });
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public async Task TestEx1Async_3()
    {
        var exception = await Record.ExceptionAsync(() =>
        {
            using var ex = new Ex1ExpectException<DivideByZeroException>();
            var _ = 1 + 2;
            return null;
        });
        Assert.IsType<AssertException>(exception);
    }

    #endregion
}