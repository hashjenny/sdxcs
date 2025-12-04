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
        var f = Fake.FakeIt(nameof(Adder.Add), value: 99);
        Assert.Equal(99, f.Call());
    }

    [Fact]
    public void test_fake_records_calls()
    {
        var f = Fake.FakeIt(nameof(Adder.Add), value: 99);
        Assert.Equal(99, f.Call(2, 3));
        Assert.Equal(99, f.Call(3, 4));
        Assert.Equal(2, f.CallArgs.Count);
    }

    [Fact]
    public void test_fake_calculates_result()
    {
        var f = Fake.FakeIt(nameof(Adder.Add), arr => 10 * (int)arr[0] + (int)arr[1]);
        Assert.Equal(23, f.Call(2, 3));
    }

    [Fact]
    public void test_context()
    {
        using var f = new ContextFake(nameof(Adder.Add), arr => 10 * (int)arr[0] + (int)arr[1]);
        Assert.Equal(23, f.Call(2, 3));
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
}