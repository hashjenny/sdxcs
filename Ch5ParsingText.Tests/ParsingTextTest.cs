using Ch4MatchingPatterns;

namespace Ch5ParsingText.Tests;

public class ParsingTextTest
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
            (Token.EitherStart, 2.ToString()),
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
        var actual = Parser.Parse(result) as Either;
        var expect = new Either([new Literal("abc"), new Literal("def")]);
        Assert.True(expect == actual);
    }

    [Fact]
    public void test_ex1_escape1()
    {
        var t = new Tokenizer().Process(@"\\abc\{\}\*");
        var expected = new List<(Token, string?)>
        {
            (Token.Literal, @"\abc{}*")
        };
        Assert.Single(t);
        Assert.Equal(expected, t);
    }


    [Fact]
    public void test_ex1_escape2()
    {
        var t = new Tokenizer().Process(@"\\abc\{\}\*{a,b,c}");
        var expected = new List<(Token, string?)>
        {
            (Token.Literal, @"\abc{}*"),
            (Token.EitherStart, 3.ToString()),
            (Token.Literal, "a"),
            (Token.Literal, "b"),
            (Token.Literal, "c"),
            (Token.EitherEnd, null)
        };
        Assert.Equal(6, t.Count);
        Assert.Equal(expected, t);
    }

    [Fact]
    public void test_ex2_charset1()
    {
        var t = new Tokenizer().Process("*[qwe]");
        var expected = new List<(Token, string?)>
        {
            (Token.Any, null),
            (Token.EitherStart, 3.ToString()),
            (Token.Literal, "q"),
            (Token.Literal, "w"),
            (Token.Literal, "e"),
            (Token.EitherEnd, null)
        };
        Assert.Equal(6, t.Count);
        Assert.Equal(expected, t);
    }

    [Fact]
    public void test_ex2_charset2()
    {
        var t = new Tokenizer().Process("[qwe]");
        var actual = Parser.Parse(t) as Either;
        var expect = new Either([new Literal("q"), new Literal("w"), new Literal("e")]);
        Assert.True(expect == actual);
    }

    [Fact]
    public void test_ex3_negation1()
    {
        var t = new Tokenizer().Process("*[!qwe]");
        var expected = new List<(Token, string?)>
        {
            (Token.Any, null),
            (Token.NotCharSetStart, 3.ToString()),
            (Token.Literal, "q"),
            (Token.Literal, "w"),
            (Token.Literal, "e"),
            (Token.NotCharSetEnd, null)
        };
        Assert.Equal(6, t.Count);
        Assert.Equal(expected, t);
    }


    [Fact]
    public void test_ex4_list()
    {
        var ex = Record.Exception(() => new NestedList().Process("{1,2,3}"));
        Assert.NotNull(ex);
    }

    [Fact]
    public void test_ex4_list2()
    {
        var result = new ListParser("[1, [2, [3, 4], 5]]").Parse();
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        var str = ListParser.FormatList(result);
        Assert.Equal("1, 2, 3, 4, 5", str);
    }

    [Fact]
    public void test_ex5_calc1()
    {
        var result = new CalcTokenizer("1+  2 * 3.2 / 4");
        Assert.NotNull(result);
        var parser = result.Parser();
        Assert.NotNull(parser);
        Assert.Equal(3, parser.Count);
    }

    [Fact]
    public void test_ex5_calc2()
    {
        var result = new CalcTokenizer("1+  2 * 3.2 / 4");
        Assert.Equal(2.6, result.Calculate());
    }

    [Fact]
    public void test_ex5_calc3()
    {
        var result = new CalcTokenizer("1 + 2 * 3");
        Assert.Equal(7, result.Calculate());
    }

    [Fact]
    public void test_ex5_calc4()
    {
        var result = new CalcTokenizer("1 + 2 +3");
        Assert.Equal(6, result.Calculate());
    }

    [Fact]
    public void test_ex5_calc5()
    {
        var result = new CalcTokenizer("1 + 2 +3 * 4 + 5 *6");
        Assert.Equal(45, result.Calculate());
    }
}