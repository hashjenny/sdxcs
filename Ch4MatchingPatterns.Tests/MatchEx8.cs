namespace Ch4MatchingPatterns.Tests;

public class MatchEx8
{
    [Fact]
    public void MatchIndex_Null_Test()
    {
        var match = new Null().MatchIndex("abc");
        Assert.True(match?.Captures.Count == 0);
    }

    [Fact]
    public void MatchIndex_Literal_Test()
    {
        var match = new Literal("ab").MatchIndex("abc");
        Assert.True(match?.End == 2);
        Assert.True(match.Captures.Count == 1);
        Assert.Contains("ab", match.Captures);
    }

    [Fact]
    public void MatchIndex_Literal_Test2()
    {
        var match = new Literal("b").MatchIndex("abc", 1);
        Assert.True(match?.End == 2);
        Assert.True(match.Captures.Count == 1);
        Assert.Contains("b", match.Captures);
    }

    [Fact]
    public void MatchIndex_Any_Test()
    {
        var match = new Any().MatchIndex("abc");
        Assert.True(match?.End == 3);
        Assert.True(match.Captures.Count == 1);
        Assert.Contains("abc", match.Captures);
    }

    [Fact]
    public void MatchIndex_Any_Test2()
    {
        var match = new Any(new Literal("c")).MatchIndex("abc");
        Assert.True(match?.End == 3);
        Assert.True(match.Captures.Count == 2);
        Assert.Contains("ab", match.Captures);
        Assert.Contains("c", match.Captures);
    }

    [Fact]
    public void MatchIndex_Any_Test3()
    {
        var match = new Any(new Literal("d")).MatchIndex("abc");
        Assert.True(match is null);
    }

    [Fact]
    public void MatchIndex_Either_Test()
    {
        var match = new Either(new Literal("dbc"), new Literal("ab")).MatchIndex("abc");
        Assert.True(match?.End == 2);
        Assert.True(match.Captures.Count == 1);
        Assert.Contains("ab", match.Captures);
    }

    [Fact]
    public void MatchIndex_OneMore_Test()
    {
        var match = new OneMore().MatchIndex("abc");
        Assert.True(match?.End == 3);
        Assert.True(match.Captures.Count == 1);
        Assert.Contains("abc", match.Captures);
    }

    [Fact]
    public void MatchIndex_OneMore_Test2()
    {
        var match = new OneMore().MatchIndex("");
        Assert.True(match is null);
    }

    [Fact]
    public void MatchIndex_OneMore_Test3()
    {
        var match = new Literal("a", new OneMore()).MatchIndex("abc");
        Assert.True(match?.End == 3);
        Assert.True(match.Captures.Count == 2);
        Assert.Contains("a", match.Captures);
        Assert.Contains("bc", match.Captures);
    }

    [Fact]
    public void MatchIndex_CharSet_Test()
    {
        var match = new CharSet("aeiou").MatchIndex("abc");
        Assert.True(match?.End == 1);
        Assert.True(match.Captures.Count == 1);
        Assert.Contains("a", match.Captures);
    }

    [Fact]
    public void MatchIndex_Range_Test()
    {
        var match = new Range('a', 'm', new Literal("b")).MatchIndex("abc");
        Assert.True(match?.End == 2);
        Assert.True(match.Captures.Count == 2);
        Assert.Contains("a", match.Captures);
        Assert.Contains("b", match.Captures);
    }

    [Fact]
    public void MatchIndex_Not_Test()
    {
        var match = new Not(new Literal("z"), new Literal("a")).MatchIndex("abc");
        Assert.True(match?.End == 1);
        Assert.True(match.Captures.Count == 2);
        Assert.Contains("", match.Captures);
        Assert.Contains("a", match.Captures);
    }

    [Fact]
    public void MatchIndex_Not_Test2()
    {
        var match = new Not(new Literal("a"), new Literal("a")).MatchIndex("abc");
        Assert.True(match is null);
    }
}