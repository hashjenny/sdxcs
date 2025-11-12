namespace Ch4MatchingPatterns.Tests;

public class MatchEx3OneMoreTest
{
    [Fact]
    public void test_one_more_matches_empty()
    {
        // + doesn't match ""
        Assert.False(new OneMore().IsMatch(""));
    }

    [Fact]
    public void test_one_more_matches_entire_string()
    {
        // + matches "abc"
        Assert.True(new OneMore().IsMatch("abc"));
    }

    [Fact]
    public void test_one_more_matches_as_prefix()
    {
        // +def matches "abcdef"
        Assert.True(new OneMore(new Literal("def")).IsMatch("abcdef"));
    }

    [Fact]
    public void test_one_more_matches_as_prefix2()
    {
        // +def doesn't match "def"
        Assert.False(new OneMore(new Literal("def")).IsMatch("def"));
    }

    [Fact]
    public void test_one_more_matches_as_suffix()
    {
        // abc+ matches "abcdef"
        Assert.True(new Literal("abc", new OneMore()).IsMatch("abcdef"));
    }

    [Fact]
    public void test_one_more_matches_as_suffix2()
    {
        // abc+ doesn't match "abc"
        Assert.False(new Literal("abc", new OneMore()).IsMatch("abc"));
    }

    [Fact]
    public void test_one_more_matches_interior()
    {
        // a+c matches "abc"
        Assert.True(new Literal("a", new OneMore(new Literal("c"))).IsMatch("abc"));
    }
}