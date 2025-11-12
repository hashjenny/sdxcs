namespace Ch4MatchingPatterns.Tests;

public class MatchEx1Test
{
    #region Lit

    [Fact]
    public void test_literal_match_entire_string()
    {
        // /abc/ matches "abc"
        Assert.True(new Manager([new Lit("abc")]).IsMatch("abc"));
    }

    [Fact]
    public void test_literal_substring_alone_no_match()
    {
        // /ab/ doesn't match "abc"
        Assert.False(new Manager([new Lit("ab")]).IsMatch("abc"));
    }


    [Fact]
    public void test_literal_superstring_no_match()
    {
        // /abc/ doesn't match "ab"
        Assert.False(new Manager([new Lit("abc")]).IsMatch("ab"));
    }

    [Fact]
    public void test_literal_followed_by_literal_match()
    {
        // /a/+/b/ matches "ab"
        Assert.True(new Manager([new Lit("a"), new Lit("b")]).IsMatch("ab"));
    }

    [Fact]
    public void test_literal_followed_by_literal_no_match()
    {
        // /a/+/b/ doesn't match "ac"
        Assert.False(new Literal("a", new Literal("b")).IsMatch("ac"));
    }

    #endregion

    #region Any

    [Fact]
    public void test_any_matches_empty()
    {
        // * matches ""
        Assert.True(new Manager([new Anything()]).IsMatch(""));
    }

    [Fact]
    public void test_any_matches_entire_string()
    {
        // * matches "abc"
        Assert.True(new Manager([new Anything()]).IsMatch("abc"));
    }

    [Fact]
    public void test_any_matches_as_prefix()
    {
        // *def matches "abcdef"
        Assert.True(new Manager([new Anything(), new Lit("def")]).IsMatch("abcdef"));
    }

    [Fact]
    public void test_any_matches_as_suffix()
    {
        // abc* matches "abcdef"
        Assert.True(new Manager([new Lit("abc"), new Anything()]).IsMatch("abcdef"));
    }

    [Fact]
    public void test_any_matches_interior()
    {
        // a*c matches "abc"
        Assert.True(new Manager([new Lit("a"), new Anything(), new Lit("c")]).IsMatch("abc"));
    }

    #endregion
    
    #region Either

    [Fact]
    public void test_either_two_literals_first()
    {
        // {a, b} matches "a"
        Assert.True(new Manager([new EitherThing(new Lit("a"), new Lit("b"))]).IsMatch("a"));
    }
    
    [Fact]
    public void test_either_two_literals_not_both()
    {
        // {a, b} doesn't matches "ab"
        Assert.False(new Manager(
            [new EitherThing(new Lit("a"), new Lit("b"))]
            ).IsMatch("ab"));
    }

    [Fact]
    public void test_either_followed_by_literal_match()
    {
        // /{a,b}c/ matches "ac"
        Assert.True(new Manager([new EitherThing(new Lit("a"), new Lit("b")), new Lit("c")])
            .IsMatch("ac"));
    }

    [Fact]
    public void test_either_followed_by_literal_no_match()
    {
        // /{a,b}c/ doesn't match "ax"
        Assert.False(new Manager([new EitherThing(new Lit("a"), new Lit("b")), new Lit("c")])
            .IsMatch("ax"));
    }
 

    #endregion
}