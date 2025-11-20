namespace Ch4MatchingPatterns.Tests;

public class MatchEx9
{
    [Fact]
    public void MatchIndex_Lazy_Any_Test()
    {
        var match = new Any(new Literal("c")).MatchIndex("abccccd");
        Assert.True(match?.End == 3);
        Assert.True(match.Captures.Count == 2);
        Assert.Contains("ab", match.Captures);
        Assert.Contains("c", match.Captures);
    }

    [Fact]
    public void MatchIndex_Greedy_Any_Test()
    {
        var match = new Any(new Literal("c"), MatchOption.Greedy).MatchIndex("abccccd");
        Assert.True(match?.End == 6);
        Assert.True(match.Captures.Count == 2);
        Assert.Contains("abccc", match.Captures);
        Assert.Contains("c", match.Captures);
    }
}