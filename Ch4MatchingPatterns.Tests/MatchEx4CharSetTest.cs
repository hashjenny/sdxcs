namespace Ch4MatchingPatterns.Tests;

public class MatchEx4CharSetTest
{
    #region CharSet

    [Fact]
    public void test_charset_all_empty()
    {
        // [] doesn't match ""
        Assert.False(new CharSet("").IsMatch(""));
    }

    [Fact]
    public void test_charset_empty()
    {
        // [asd] doesn't match ""
        Assert.False(new CharSet("asd").IsMatch(""));
    }

    [Fact]
    public void test_charset_not_match()
    {
        // [asd] doesn't match "z"
        Assert.False(new CharSet("asd").IsMatch("z"));
    }

    [Fact]
    public void test_charset_simple_match()
    {
        // [asd] matches "a"
        Assert.True(new CharSet("asd").IsMatch("a"));
    }

    [Fact]
    public void test_charset_prefix()
    {
        // [asd]s matches "as"
        Assert.True(new CharSet("asd", new Literal("s")).IsMatch("as"));
    }

    [Fact]
    public void test_charset_prefix_not_match()
    {
        // [asd]s doesn't match "bs"
        Assert.False(new CharSet("asd", new Literal("s")).IsMatch("bs"));
    }

    [Fact]
    public void test_charset_subfix()
    {
        // a[asd] matches "as"
        Assert.True(new Literal("a", new CharSet("asd")).IsMatch("as"));
    }

    #endregion

    #region Range

    [Fact]
    public void test_range_empty()
    {
        // [d-m] doesn't match ""
        Assert.False(new Range('d', 'm').IsMatch(""));
    }

    [Fact]
    public void test_range_not_match()
    {
        // [d-m] doesn't match "z"
        Assert.False(new Range('d', 'm').IsMatch("z"));
    }

    [Fact]
    public void test_range_simple_match()
    {
        // [d-m] matches "j"
        Assert.True(new Range('d', 'm').IsMatch("j"));
    }

    [Fact]
    public void test_range_prefix()
    {
        // [d-m]s matches "gs"
        Assert.True(new Range('d', 'm', new Literal("s")).IsMatch("gs"));
    }

    [Fact]
    public void test_range_prefix_not_match()
    {
        // [d-m]s doesn't match "bs"
        Assert.False(new Range('d', 'm', new Literal("s")).IsMatch("bs"));
    }

    [Fact]
    public void test_range_subfix()
    {
        // a[d-m] matches "ak"
        Assert.True(new Literal("a", new Range('d', 'm')).IsMatch("ak"));
    }

    [Fact]
    public void test_range_subfix_not_match()
    {
        // a[d-m] doesn't match "as"
        Assert.False(new Literal("a", new Range('d', 'm')).IsMatch("as"));
    }

    #endregion
}