namespace Smart.Mvvm.Collections;

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

using Smart.Mvvm.Internal;

public sealed class ObservableView<T> : IReadOnlyList<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
    where T : notnull
{
    private readonly IReadOnlyList<T> source;

    private readonly IEqualityComparer<T> itemComparer;

    private readonly HashSet<string> observedProperties = [];

    private HashSet<INotifyPropertyChanged> subscribedItems = new(ReferenceEqualityComparer.Instance);

    private List<T> view;

    private int deferCount;

    private bool refreshPending;

    private bool resetPending;

    public ObservableView(IReadOnlyList<T> source)
    {
        this.source = source;
        itemComparer = typeof(T).IsValueType ? EqualityComparer<T>.Default : (IEqualityComparer<T>)(object)ReferenceEqualityComparer.Instance;
        view = Build();

        if (source is INotifyCollectionChanged notify)
        {
            notify.CollectionChanged += OnSourceCollectionChanged;
        }
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Func<T, bool>? Filter
    {
        get;
        set
        {
            field = value;
            Refresh();
        }
    }

    public IComparer<T>? Comparer
    {
        get;
        set
        {
            field = value;
            Refresh();
        }
    }

    public int Count => view.Count;

    public T this[int index] => view[index];

    bool IList.IsFixedSize => false;

    bool IList.IsReadOnly => true;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    object? IList.this[int index]
    {
        get => view[index];
        set => throw new NotSupportedException();
    }

    public void Dispose()
    {
        if (source is INotifyCollectionChanged notify)
        {
            notify.CollectionChanged -= OnSourceCollectionChanged;
        }

        foreach (var item in subscribedItems)
        {
            item.PropertyChanged -= OnItemPropertyChanged;
        }

        subscribedItems.Clear();
    }

    public IEnumerator<T> GetEnumerator() => view.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => view.GetEnumerator();

    int IList.Add(object? value) => throw new NotSupportedException();

    void IList.Clear() => throw new NotSupportedException();

    bool IList.Contains(object? value) => (value is T item) && view.Contains(item);

    int IList.IndexOf(object? value) => value is T item ? view.IndexOf(item) : -1;

    void IList.Insert(int index, object? value) => throw new NotSupportedException();

    void IList.Remove(object? value) => throw new NotSupportedException();

    void IList.RemoveAt(int index) => throw new NotSupportedException();

    void ICollection.CopyTo(Array array, int index) => ((ICollection)view).CopyTo(array, index);

    public void ObserveProperty(string propertyName)
    {
        if (observedProperties.Add(propertyName) && (observedProperties.Count == 1))
        {
            SyncSubscriptions();
        }
    }

    public void Refresh()
    {
        if (deferCount > 0)
        {
            refreshPending = true;
            return;
        }

        Apply(Build());
    }

    public IDisposable DeferRefresh()
    {
        deferCount++;
        return new DeferScope(EndDefer);
    }

    private void EndDefer()
    {
        deferCount--;
        if (deferCount > 0)
        {
            return;
        }

        if (resetPending)
        {
            resetPending = false;
            refreshPending = false;
            Reset();
        }
        else if (refreshPending)
        {
            refreshPending = false;
            Apply(Build());
        }
    }

    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (observedProperties.Count > 0)
        {
            SyncSubscriptions();
        }

        if (e.Action != NotifyCollectionChangedAction.Reset)
        {
            Refresh();
        }
        else if (deferCount > 0)
        {
            resetPending = true;
        }
        else
        {
            Reset();
        }
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (String.IsNullOrEmpty(e.PropertyName) || observedProperties.Contains(e.PropertyName))
        {
            Refresh();
        }
    }

    private void SyncSubscriptions()
    {
        var items = new HashSet<INotifyPropertyChanged>(ReferenceEqualityComparer.Instance);
        foreach (var item in source)
        {
            if (item is INotifyPropertyChanged notify)
            {
                items.Add(notify);
            }
        }

        foreach (var item in subscribedItems)
        {
            if (!items.Contains(item))
            {
                item.PropertyChanged -= OnItemPropertyChanged;
            }
        }

        foreach (var item in items)
        {
            if (!subscribedItems.Contains(item))
            {
                item.PropertyChanged += OnItemPropertyChanged;
            }
        }

        subscribedItems = items;
    }

    private List<T> Build()
    {
        var filter = Filter;
        var list = new List<T>(source.Count);
        foreach (var item in source)
        {
            if ((filter is null) || filter(item))
            {
                list.Add(item);
            }
        }

        var comparer = Comparer;
        if ((comparer is null) || (list.Count < 2))
        {
            return list;
        }

        var items = list.ToArray();
        var order = new int[items.Length];
        for (var i = 0; i < order.Length; i++)
        {
            order[i] = i;
        }

        Array.Sort(order, (x, y) =>
        {
            var result = comparer.Compare(items[x], items[y]);
            return result != 0 ? result : x.CompareTo(y);
        });

        for (var i = 0; i < order.Length; i++)
        {
            list[i] = items[order[i]];
        }

        return list;
    }

    private void Apply(List<T> desired)
    {
        if ((CollectionChanged is null) && (PropertyChanged is null))
        {
            view = desired;
            return;
        }

        var next = new int[desired.Count];
        var first = new Dictionary<T, int>(itemComparer);
        for (var i = desired.Count - 1; i >= 0; i--)
        {
            next[i] = first.GetValueOrDefault(desired[i], -1);
            first[desired[i]] = i;
        }

        var targets = new List<int>(view.Count);
        foreach (var item in view)
        {
            if (first.TryGetValue(item, out var index))
            {
                targets.Add(index);
                if (next[index] >= 0)
                {
                    first[item] = next[index];
                }
                else
                {
                    first.Remove(item);
                }
            }
            else
            {
                targets.Add(-1);
            }
        }

        for (var i = view.Count - 1; i >= 0; i--)
        {
            if (targets[i] < 0)
            {
                targets.RemoveAt(i);
                RemoveAt(i);
            }
        }

        var exists = new bool[desired.Count];
        var stays = new bool[desired.Count];
        var sequence = FindLongestIncreasing(targets);
        for (var i = 0; i < targets.Count; i++)
        {
            exists[targets[i]] = true;
            stays[targets[i]] = sequence[i];
        }

        var anchor = -1;
        for (var i = desired.Count - 1; i >= 0; i--)
        {
            if (!stays[i])
            {
                var to = anchor < 0 ? view.Count : targets.IndexOf(anchor);
                if (exists[i])
                {
                    var from = targets.IndexOf(i);
                    var index = from < to ? to - 1 : to;
                    if (from != index)
                    {
                        targets.RemoveAt(from);
                        targets.Insert(index, i);
                        Move(from, index);
                    }
                }
                else
                {
                    targets.Insert(to, i);
                    Insert(to, desired[i]);
                }
            }

            anchor = i;
        }
    }

    private static bool[] FindLongestIncreasing(List<int> values)
    {
        var mask = new bool[values.Count];
        if (values.Count == 0)
        {
            return mask;
        }

        var tails = new int[values.Count];
        var previous = new int[values.Count];
        var length = 0;
        for (var i = 0; i < values.Count; i++)
        {
            var low = 0;
            var high = length;
            while (low < high)
            {
                var middle = (low + high) >>> 1;
                if (values[tails[middle]] < values[i])
                {
                    low = middle + 1;
                }
                else
                {
                    high = middle;
                }
            }

            previous[i] = low > 0 ? tails[low - 1] : -1;
            tails[low] = i;
            if (low == length)
            {
                length++;
            }
        }

        for (var i = tails[length - 1]; i >= 0; i = previous[i])
        {
            mask[i] = true;
        }

        return mask;
    }

    private void Insert(int index, T item)
    {
        view.Insert(index, item);
        PropertyChanged?.Invoke(this, CollectionEventArgs.CountChanged);
        PropertyChanged?.Invoke(this, CollectionEventArgs.IndexerChanged);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    private void RemoveAt(int index)
    {
        var item = view[index];
        view.RemoveAt(index);
        PropertyChanged?.Invoke(this, CollectionEventArgs.CountChanged);
        PropertyChanged?.Invoke(this, CollectionEventArgs.IndexerChanged);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
    }

    private void Move(int oldIndex, int newIndex)
    {
        var item = view[oldIndex];
        view.RemoveAt(oldIndex);
        view.Insert(newIndex, item);
        PropertyChanged?.Invoke(this, CollectionEventArgs.IndexerChanged);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
    }

    private void Reset()
    {
        var count = view.Count;
        view = Build();
        if (count != view.Count)
        {
            PropertyChanged?.Invoke(this, CollectionEventArgs.CountChanged);
        }

        PropertyChanged?.Invoke(this, CollectionEventArgs.IndexerChanged);
        CollectionChanged?.Invoke(this, CollectionEventArgs.Reset);
    }

    private sealed class DeferScope(Action end) : IDisposable
    {
        private Action? callback = end;

        public void Dispose()
        {
            var action = callback;
            callback = null;
            action?.Invoke();
        }
    }
}
