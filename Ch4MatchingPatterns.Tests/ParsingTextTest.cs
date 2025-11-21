namespace Ch4MatchingPatterns.Tests;

public class ParsingTextTest
{
    [Fact]
    public void test_notcharset_not_match()
    {
        // [!asd] match "z"
        Assert.True(new NotCharSet("asd").IsMatch("z"));
    }

    [Fact]
    public void test_notcharset_simple_match()
    {
        // [asd] doesn't match "a"
        Assert.False(new NotCharSet("asd").IsMatch("a"));
    }

    [Fact]
    public void test_notcharset_prefix()
    {
        // [asd]s doesn't match "as"
        Assert.False(new NotCharSet("asd", new Literal("s")).IsMatch("as"));
    }

    [Fact]
    public void test_notcharset_prefix_not_match()
    {
        // [asd]s matches "bs"
        Assert.True(new NotCharSet("asd", new Literal("s")).IsMatch("bs"));
    }

    [Fact]
    public void test_notcharset_subfix()
    {
        // a[asd] matches "as"
        Assert.False(new Literal("a", new NotCharSet("asd")).IsMatch("as"));
    }
}