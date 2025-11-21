using Ch4MatchingPatterns;

namespace Ch5ParsingText.Tests;

public class UnitTest1
{
    [Fact]
    public void test_tok_empty_string()
    {
        Assert.Empty(new Tokenizer().Process(""));
    }

    [Fact]
    public void test_tok_any_either()
    {
        var t = new Tokenizer().Process("*{abc,def}");
        var expected = new List<(Token, string?)>
        {
            (Token.Any, null),
            (Token.EitherStart, null),
            (Token.Literal, "abc"),
            (Token.Literal, "def"),
            (Token.EitherEnd, null)
        };
        Assert.Equal(5, t.Count);
        Assert.Equal(expected, t);
    }

    [Fact]
    public void test_parse_either_two_lit()
    {
        var result = new Tokenizer().Process("{abc,def}");
        var actual = Parser.parse(result) as Either;
        var expect = new Either([new Literal("abc"), new Literal("def")]);
        Assert.True(expect == actual);
    }

    [Fact]
    public void test_ex1_escape()
    {
        var t = new Tokenizer().Process(@"\\abc\{\}\*");
        var expected = new List<(Token, string?)>
        {
            (Token.Literal, @"\abc{}*")
        };
        Assert.Single(t);
        Assert.Equal(expected, t);
    }
}