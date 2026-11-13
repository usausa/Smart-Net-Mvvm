namespace Smart.Mvvm.ViewModels;

using System.ComponentModel;

public sealed class BusyStateExtensionsTests
{
    [Fact]
    public void UsingReleasesWhenExecuteThrows()
    {
        var state = new BusyState();

        Assert.Throws<InvalidOperationException>(() => state.Using(Throw));

        Assert.False(state.IsBusy);

        static void Throw() => throw new InvalidOperationException();
    }

    [Fact]
    public void UsingResultIsBusyInsideAndReturnsValue()
    {
        var state = new BusyState();

        var result = state.Using(() => state.IsBusy ? 1 : 0);

        Assert.Equal(1, result);
        Assert.False(state.IsBusy);
    }

    [Fact]
    public void UsingAsyncKeepsBusyUntilTaskCompletes()
    {
        var state = new BusyState();
        var tcs = new TaskCompletionSource();

        var task = state.UsingAsync(() => tcs.Task);
        Assert.True(state.IsBusy);

        tcs.SetResult();

        Assert.True(task.IsCompletedSuccessfully);
        Assert.False(state.IsBusy);
    }

    [Fact]
    public void UsingAsyncReleasesWhenTaskFaults()
    {
        var state = new BusyState();

        var task = state.UsingAsync(static () => Task.FromException<int>(new InvalidOperationException()));

        Assert.True(task.IsFaulted);
        Assert.IsType<InvalidOperationException>(task.Exception.InnerException);
        Assert.False(state.IsBusy);
    }

    [Fact]
    public void UsingDoesNotReleaseWhenRequireThrows()
    {
        var state = new FailingState();

        Assert.Throws<InvalidOperationException>(() => state.Using(static () => { }));
        var task = state.UsingAsync(static () => Task.CompletedTask);

        Assert.True(task.IsFaulted);
        Assert.Equal(0, state.ReleaseCount);
    }

    [Fact]
    public void BeginDoesNotAllocate()
    {
        var state = new BusyState();
        Run(state);

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 100; i++)
        {
            Run(state);
        }

        Assert.InRange(GC.GetAllocatedBytesForCurrentThread() - before, 0, 99);

        static void Run(BusyState state)
        {
            using (state.Begin())
            {
                Assert.True(state.IsBusy);
            }
        }
    }

    [Fact]
    public void DefaultScopeDisposeDoesNothing()
    {
        default(BusyStateScope).Dispose();
    }

    [Fact]
    public void UsingDoesNotAllocate()
    {
        var state = new BusyState();
        Run(state);

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 100; i++)
        {
            Run(state);
        }

        Assert.InRange(GC.GetAllocatedBytesForCurrentThread() - before, 0, 99);

        static void Run(BusyState state) => state.Using(static () => { });
    }

    private sealed class FailingState : IBusyState
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsBusy => false;

        public int ReleaseCount { get; private set; }

        public void Require() => throw new InvalidOperationException();

        public void Release()
        {
            ReleaseCount++;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
        }

        public void Reset()
        {
        }
    }
}
