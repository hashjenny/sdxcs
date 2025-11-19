namespace Ch4MatchingPatterns.Tests;

public class MatchEx5NotTest
{
    #region Not

    [Fact]
    public void test_not_all_empty()
    {
        // ^"" !=> ""
        Assert.False(new Not(new Literal("")).IsMatch(""));
    }

    [Fact]
    public void test_not_empty()
    {
        // ^"aaa" => ""
        Assert.True(new Not(new Literal("aaa")).IsMatch(""));
    }

    [Fact]
    public void test_not_empty2()
    {
        // ^""  => "aaa"
        Assert.False(new Not(new Literal("")).IsMatch("aaa"));
    }
    
    [Fact]
    public void test_charset_not_match()
    {
        // ^[asd]z => "z"
        Assert.True(new Not(new CharSet("asd"),new Literal("z")).IsMatch("z"));
    }

    [Fact]
    public void test_not_literal_match()
    {
        // not "a"b matches "b"
        Assert.True(new Not(new Literal("a"), new Literal("b")).IsMatch("b"));
    }

    [Fact]
    public void test_not_literal_not_match()
    {
        // not "a" doesn't match "a"
        Assert.False(new Not(new Literal("a")).IsMatch("a"));
    }

    [Fact]
    public void test_not_charset_not_match()
    {
        // not [asd] doesn't match "a"
        Assert.False(new Not(new CharSet("asd")).IsMatch("a"));
    }

    [Fact]
    public void test_not_charset_prefix_match()
    {
        // not [asd]s !=> "bs"
        Assert.False(new Not(new CharSet("asd", new Literal("s"))).IsMatch("bs"));
    }

    [Fact]
    public void test_not_charset_prefix_not_match()
    {
        // not [asd]s doesn't match "as" (because [asd]s matches "as")
        Assert.False(new Not(new CharSet("asd", new Literal("s"))).IsMatch("as"));
    }

    [Fact]
    public void test_not_subfix_match_and_not_match()
    {
        // a[asd] matches "as", so not a[asd] should not match "as"
        var neg = new Not(new Literal("a", new CharSet("asd")));
        Assert.False(neg.IsMatch("as"));
    }

    [Fact]
    public void test_double_not_behaviour()
    {
        // double negation: not(not "a") should behave like "a"
        var doubleNot = new Not(new Not(new Literal("a")));
        Assert.False(doubleNot.IsMatch("a"));
        Assert.False(doubleNot.IsMatch("b")); 
    }

    [Fact]
    public void test_either_not()
    {
        // not aa|bba !=> bba 
        Assert.False(new Not(new Either(new Literal("aa"), new Literal("bba"))).IsMatch("bba"));
    }

    [Fact]
    public void test_range_not()
    {
        // not [a-v] ... => z
        Assert.True(new Not(new Range('a', 'v'), new Any()).IsMatch("z"));
    }
    
    [Fact]
    public void test_range_not2()
    {
        // not [a-v] ... !=> g
        Assert.False(new Not(new Range('a', 'v'), new Any()).IsMatch("g"));
    }
    
    #endregion
}