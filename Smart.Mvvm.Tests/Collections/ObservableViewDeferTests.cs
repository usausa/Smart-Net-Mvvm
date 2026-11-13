namespace Smart.Mvvm.Collections;

using System.Collections.ObjectModel;
using System.Collections.Specialized;

public sealed class ObservableViewDeferTests
{
    [Fact]
    public void DeferRefreshDisposeTwiceKeepsOuterScope()
    {
        var source = new ObservableCollection<int>([3, 1]);
        using var view = new ObservableView<int>(source);
        var actions = new List<NotifyCollectionChangedAction>();
        view.CollectionChanged += (_, e) => actions.Add(e.Action);

        var outer = view.DeferRefresh();
        var inner = view.DeferRefresh();
        inner.Dispose();
        inner.Dispose();
        source.Add(2);

        Assert.Empty(actions);
        Assert.Equal([3, 1], view);

        outer.Dispose();

        Assert.Equal([3, 1, 2], view);
        Assert.Single(actions);
    }

    [Fact]
    public void DeferRefreshDoesNotAllocateDelegate()
    {
        using var view = new ObservableView<int>(new ObservableCollection<int>());
        Run(view);

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 100; i++)
        {
            Run(view);
        }

        Assert.InRange(GC.GetAllocatedBytesForCurrentThread() - before, 0, 3200);

        static void Run(ObservableView<int> view)
        {
            using (view.DeferRefresh())
            {
            }
        }
    }
}
