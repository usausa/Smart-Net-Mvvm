namespace Smart.Mvvm.Internal;

public sealed class PooledListTest
{
    [Fact]
    public void IndexerReturnsElementWithinCount()
    {
        using var list = new PooledList<string>(16);
        list.Add("a");
        list.Add("b");

        Assert.Equal("a", list[0]);
        Assert.Equal("b", list[1]);
    }
}
