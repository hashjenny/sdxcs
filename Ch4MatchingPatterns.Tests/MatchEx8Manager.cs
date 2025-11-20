namespace Ch4MatchingPatterns.Tests;

public class MatchEx8Manager
{
    [Fact]
    public void Match_Literal_Test()
    {
        var match = new Manager(new LiteralThing("ab")).Match("abc");
        Assert.True(match?.End == 2);
        Assert.True(match.Captures.Count == 1);
        Assert.Contains("ab", match.Captures);
    }

    [Fact]
    public void Match_Literal_Test2()
    {
        var match = new Manager(new LiteralThing("a"), new LiteralThing("b")).Match("abc");
        Assert.True(match?.End == 2);
        Assert.True(match.Captures.Count == 2);
        Assert.Contains("b", match.Captures);
    }

    [Fact]
    public void Match_Any_Test()
    {
        var match = new Manager(new Anything()).Match("abc");
        Assert.True(match?.End == 3);
        Assert.True(match.Captures.Count == 1);
        Assert.Contains("abc", match.Captures);
    }

    [Fact]
    public void Match_Any_Test2()
    {
        var match = new Manager(new Anything(), new LiteralThing("c")).Match("abc");
        Assert.True(match?.End == 3);
        Assert.True(match.Captures.Count == 2);
        Assert.Contains("ab", match.Captures);
        Assert.Contains("c", match.Captures);
    }

    [Fact]
    public void Match_Any_Test3()
    {
        var match = new Manager(new Anything(), new LiteralThing("d")).Match("abc");
        Assert.True(match is null);
    }

    [Fact]
    public void Match_Either_Test()
    {
        var match = new Manager(new EitherThing(new LiteralThing("dbc"), new LiteralThing("ab"))).Match("abc");
        Assert.True(match?.End == 2);
        Assert.True(match.Captures.Count == 1);
        Assert.Contains("ab", match.Captures);
    }

    [Fact]
    public void Match_OneMore_Test()
    {
        var match = new Manager(new OneMoreThing()).Match("abc");
        Assert.True(match?.End == 3);
        Assert.True(match.Captures.Count == 1);
        Assert.Contains("abc", match.Captures);
    }

    [Fact]
    public void Match_OneMore_Test2()
    {
        var match = new Manager(new OneMoreThing()).Match("");
        Assert.True(match is null);
    }

    [Fact]
    public void Match_OneMore_Test3()
    {
        var match = new Manager(new LiteralThing("a"), new OneMoreThing()).Match("abc");
        Assert.True(match?.End == 3);
        Assert.True(match.Captures.Count == 2);
        Assert.Contains("a", match.Captures);
        Assert.Contains("bc", match.Captures);
    }

    [Fact]
    public void Match_CharSet_Test()
    {
        var match = new Manager(new CharSetThing("aeiou")).Match("abc");
        Assert.True(match?.End == 1);
        Assert.True(match.Captures.Count == 1);
        Assert.Contains("a", match.Captures);
    }

    [Fact]
    public void Match_Range_Test()
    {
        var match = new Manager(new RangeThing('a', 'm'), new LiteralThing("b")).Match("abc");
        Assert.True(match?.End == 2);
        Assert.True(match.Captures.Count == 2);
        Assert.Contains("a", match.Captures);
        Assert.Contains("b", match.Captures);
    }

    [Fact]
    public void Match_Not_Test()
    {
        var match = new Manager(new NotThing(new LiteralThing("z")), new LiteralThing("a")).Match("abc");
        Assert.True(match?.End == 1);
        Assert.True(match.Captures.Count == 2);
        Assert.Contains("", match.Captures);
        Assert.Contains("a", match.Captures);
    }

    [Fact]
    public void Match_Not_Test2()
    {
        var match = new Manager(new NotThing(new LiteralThing("a")), new LiteralThing("a")).Match("abc");
        Assert.True(match is null);
    }
}