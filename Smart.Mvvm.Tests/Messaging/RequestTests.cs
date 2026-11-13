namespace Smart.Mvvm.Messaging;

public sealed class RequestTests
{
    [Fact]
    public void IsCancelWithoutListenerReturnsFalse()
    {
        var request = new CancelRequest();

        Assert.False(request.IsCancel());
    }

    [Fact]
    public void IsCancelReturnsListenerResult()
    {
        var request = new CancelRequest();
        object? sender = null;
        request.Requested += (s, e) =>
        {
            sender = s;
            e.Cancel = true;
        };

        Assert.True(request.IsCancel());
        Assert.Same(request, sender);
    }

    [Fact]
    public void ResolveWithoutListenerReturnsNull()
    {
        Assert.Null(new ResolveRequest<string>().Resolve());
        Assert.Null(new ResolveRequest<int?>().Resolve());
    }

    [Fact]
    public void ResolveWithoutListenerForValueTypeReturnsDefault()
    {
        Assert.Equal(0, new ResolveRequest<int>().Resolve());
    }

    [Fact]
    public void ResolveWithoutResultForValueTypeReturnsDefault()
    {
        var request = new ResolveRequest<int>();
        request.Requested += static (_, _) => { };

        Assert.Equal(0, request.Resolve());
    }

    [Fact]
    public void ResolveWithOtherTypeResultThrows()
    {
        var request = new ResolveRequest<int>();
        request.Requested += static (_, e) => e.Result = "5";

        Assert.Throws<InvalidCastException>(() => request.Resolve());
    }

    [Fact]
    public void ResolveReturnsListenerResult()
    {
        var request = new ResolveRequest<int>();
        request.Requested += static (_, e) => e.Result = 5;

        Assert.Equal(5, request.Resolve());
    }

    [Fact]
    public void RequestsWithoutListenerDoNotAllocate()
    {
        var cancel = new CancelRequest();
        var resolve = new ResolveRequest<string>();
        Run(cancel, resolve);

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 100; i++)
        {
            Run(cancel, resolve);
        }

        Assert.InRange(GC.GetAllocatedBytesForCurrentThread() - before, 0, 99);

        static void Run(CancelRequest cancel, ResolveRequest<string> resolve)
        {
            Assert.False(cancel.IsCancel());
            Assert.Null(resolve.Resolve());
        }
    }
}
