namespace Smart.Mvvm.Collections;

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public sealed class ObservableViewTests
{
    private static readonly IComparer<Item> Ascending = Comparer<Item>.Create(static (x, y) => x.Value.CompareTo(y.Value));

    private static readonly IComparer<Item> Descending = Comparer<Item>.Create(static (x, y) => y.Value.CompareTo(x.Value));

    private static List<Item> Expected(IEnumerable<Item> source, Func<Item, bool>? filter, IComparer<Item>? comparer)
    {
        var query = source.Where(x => filter?.Invoke(x) ?? true);
        return (comparer is null ? query : query.OrderBy(static x => x, comparer)).ToList();
    }

    private static ObservableCollection<Item> Source(params int[] values) => [.. values.Select(static x => new Item(x))];

    //------------------------------------------------------------
    // Build
    //------------------------------------------------------------

    [Fact]
    public void BuildsFilteredAndSortedView()
    {
        // Arrange
        var source = Source(5, 3, 8, 1);

        // Act
        using var view = new ObservableView<Item>(source);
        view.Filter = static x => x.Value >= 3;
        view.Comparer = Ascending;

        // Assert
        Assert.Equal([3, 5, 8], view.Select(static x => x.Value));
    }

    [Fact]
    public void KeepsSourceOrderForEqualKeys()
    {
        // Arrange
        var source = Source(1, 1, 0);

        // Act
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;

        // Assert
        Assert.Equal([source[2], source[0], source[1]], view);
    }

    //------------------------------------------------------------
    // Source
    //------------------------------------------------------------

    [Fact]
    public void InsertsAddedItemAtSortedPosition()
    {
        // Arrange
        var source = Source(1, 3, 5);
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        var replica = new Replica<Item>(view);

        // Act
        source.Add(new Item(4));

        // Assert
        Assert.Equal([NotifyCollectionChangedAction.Add], replica.Actions);
        Assert.Equal([1, 3, 4, 5], view.Select(static x => x.Value));
        Assert.Equal(view, replica.Items);
    }

    [Fact]
    public void IgnoresAddedItemNotMatchingFilter()
    {
        // Arrange
        var source = Source(2, 4);
        using var view = new ObservableView<Item>(source);
        view.Filter = static x => x.Value % 2 == 0;
        var replica = new Replica<Item>(view);

        // Act
        source.Add(new Item(3));

        // Assert
        Assert.Empty(replica.Actions);
        Assert.Equal(2, view.Count);
    }

    [Fact]
    public void RemovesRemovedItem()
    {
        // Arrange
        var source = Source(1, 2, 3);
        using var view = new ObservableView<Item>(source);
        var replica = new Replica<Item>(view);

        // Act
        source.RemoveAt(1);

        // Assert
        Assert.Equal([NotifyCollectionChangedAction.Remove], replica.Actions);
        Assert.Equal(source, view);
        Assert.Equal(view, replica.Items);
    }

    [Fact]
    public void ReplacesItem()
    {
        // Arrange
        var source = Source(1, 2, 3);
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        var replica = new Replica<Item>(view);

        // Act
        source[0] = new Item(9);

        // Assert
        Assert.Equal(Expected(source, null, Ascending), view);
        Assert.Equal(view, replica.Items);
    }

    [Fact]
    public void MovesItemWithoutComparer()
    {
        // Arrange
        var source = Source(1, 2, 3);
        using var view = new ObservableView<Item>(source);
        var replica = new Replica<Item>(view);

        // Act
        source.Move(0, 2);

        // Assert
        Assert.Equal([NotifyCollectionChangedAction.Move], replica.Actions);
        Assert.Equal(source, view);
        Assert.Equal(view, replica.Items);
    }

    [Fact]
    public void IgnoresSourceMoveWithComparer()
    {
        // Arrange
        var source = Source(1, 2, 3);
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        var replica = new Replica<Item>(view);

        // Act
        source.Move(0, 2);

        // Assert
        Assert.Empty(replica.Actions);
    }

    [Fact]
    public void ResetsWhenSourceCleared()
    {
        // Arrange
        var source = Source(1, 2, 3);
        using var view = new ObservableView<Item>(source);
        var replica = new Replica<Item>(view);

        // Act
        source.Clear();

        // Assert
        Assert.Equal([NotifyCollectionChangedAction.Reset], replica.Actions);
        Assert.Empty(view);
        Assert.Empty(replica.Items);
    }

    [Fact]
    public void HandlesDuplicateValues()
    {
        // Arrange
        var source = new ObservableCollection<int> { 1, 2, 2, 3, 4 };
        using var view = new ObservableView<int>(source);
        view.Filter = static x => x % 2 == 0;
        var replica = new Replica<int>(view);

        // Act
        source.Remove(2);
        source.Add(2);

        // Assert
        Assert.Equal([2, 4, 2], view);
        Assert.Equal(view, replica.Items);
    }

    //------------------------------------------------------------
    // Filter / Comparer
    //------------------------------------------------------------

    [Fact]
    public void AppliesFilterChangeAsDifference()
    {
        // Arrange
        var source = Source(1, 2, 3, 4, 5, 6);
        using var view = new ObservableView<Item>(source);
        view.Filter = static x => x.Value % 2 == 0;
        var replica = new Replica<Item>(view);

        // Act
        view.Filter = static x => x.Value > 3;

        // Assert
        Assert.DoesNotContain(NotifyCollectionChangedAction.Reset, replica.Actions);
        Assert.Equal([4, 5, 6], view.Select(static x => x.Value));
        Assert.Equal(view, replica.Items);
    }

    [Fact]
    public void AppliesComparerChangeAsMoves()
    {
        // Arrange
        var source = Source(3, 1, 2);
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        var replica = new Replica<Item>(view);

        // Act
        view.Comparer = Descending;

        // Assert
        Assert.All(replica.Actions, static x => Assert.Equal(NotifyCollectionChangedAction.Move, x));
        Assert.Equal([3, 2, 1], view.Select(static x => x.Value));
        Assert.Equal(view, replica.Items);
    }

    [Fact]
    public void RefreshReevaluatesFilter()
    {
        // Arrange
        var threshold = new StrongBox<int>(2);
        var source = Source(1, 2, 3, 4);
        using var view = new ObservableView<Item>(source);
        view.Filter = x => x.Value >= threshold.Value;
        var replica = new Replica<Item>(view);

        // Act
        threshold.Value = 4;
        view.Refresh();

        // Assert
        Assert.Equal([4], view.Select(static x => x.Value));
        Assert.Equal(view, replica.Items);
    }

    [Fact]
    public void KeepsDuplicateValuesWhenInserting()
    {
        // Arrange
        var source = new ObservableCollection<int> { 5, 5 };
        using var view = new ObservableView<int>(source);
        var replica = new Replica<int>(view);

        // Act
        source.Insert(0, 1);

        // Assert
        Assert.Equal([NotifyCollectionChangedAction.Add], replica.Actions);
        Assert.Equal([1, 5, 5], view);
    }

    [Fact]
    public void RefreshWithoutChangesRaisesNothing()
    {
        // Arrange
        var source = Source(3, 1, 2);
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        var replica = new Replica<Item>(view);
        var properties = new List<string?>();
        view.PropertyChanged += (_, e) => properties.Add(e.PropertyName);

        // Act
        view.Refresh();

        // Assert
        Assert.Empty(replica.Actions);
        Assert.Empty(properties);
        Assert.Equal([1, 2, 3], view.Select(static x => x.Value));
    }

    //------------------------------------------------------------
    // ObserveProperty
    //------------------------------------------------------------

    [Fact]
    public void ReevaluatesItemWhenObservedPropertyChanged()
    {
        // Arrange
        var source = Source(1, 2, 3, 4);
        using var view = new ObservableView<Item>(source);
        view.Filter = static x => x.Value > 1;
        view.Comparer = Ascending;
        view.ObserveProperty(nameof(Item.Value));
        var replica = new Replica<Item>(view);

        // Act
        source[1].Value = 9;
        source[0].Value = 5;

        // Assert
        Assert.Equal([3, 4, 5, 9], view.Select(static x => x.Value));
        Assert.Equal(view, replica.Items);
    }

    [Fact]
    public void IgnoresChangeOfUnobservedProperty()
    {
        // Arrange
        var source = Source(1, 2);
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        view.ObserveProperty(nameof(Item.Value));
        var replica = new Replica<Item>(view);

        // Act
        source[0].Name = "changed";

        // Assert
        Assert.Empty(replica.Actions);
    }

    [Fact]
    public void RaisesNothingWhenObservedChangeKeepsOrder()
    {
        // Arrange
        var source = Source(1, 5, 9);
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        view.ObserveProperty(nameof(Item.Value));
        var replica = new Replica<Item>(view);

        // Act
        source[1].Value = 6;

        // Assert
        Assert.Empty(replica.Actions);
        Assert.Equal([1, 6, 9], view.Select(static x => x.Value));
    }

    [Fact]
    public void ObservesItemsAddedLater()
    {
        // Arrange
        var source = Source(1, 2);
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        view.ObserveProperty(nameof(Item.Value));
        var item = new Item(3);
        source.Add(item);
        var replica = new Replica<Item>(view);

        // Act
        item.Value = 0;

        // Assert
        Assert.Equal([0, 1, 2], view.Select(static x => x.Value));
        Assert.Equal(view, replica.Items);
    }

    [Fact]
    public void StopsObservingRemovedItems()
    {
        // Arrange
        var source = Source(1, 2);
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        view.ObserveProperty(nameof(Item.Value));
        var item = source[0];
        source.Remove(item);
        var replica = new Replica<Item>(view);

        // Act
        item.Value = 5;

        // Assert
        Assert.Empty(replica.Actions);
    }

    [Fact]
    public void KeepsObservingDuplicateUntilLastRemoved()
    {
        // Arrange
        var item = new Item(3);
        var source = new ObservableCollection<Item> { new(1), item, new(2), item };
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        view.ObserveProperty(nameof(Item.Value));
        source.RemoveAt(3);
        var replica = new Replica<Item>(view);

        // Act
        item.Value = 0;

        // Assert
        Assert.Equal([0, 1, 2], view.Select(static x => x.Value));
        Assert.Equal(view, replica.Items);
    }

    [Fact]
    public void StopsObservingDuplicateWhenLastRemoved()
    {
        // Arrange
        var item = new Item(3);
        var source = new ObservableCollection<Item> { new(1), item, item };
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        view.ObserveProperty(nameof(Item.Value));

        // Act
        source.Remove(item);
        source.Remove(item);

        // Assert
        Assert.False(item.HasSubscribers);
    }

    [Fact]
    public void ObservesReplacingItem()
    {
        // Arrange
        var source = Source(1, 2);
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        view.ObserveProperty(nameof(Item.Value));
        var item = new Item(3);
        source[0] = item;
        var replica = new Replica<Item>(view);

        // Act
        item.Value = 0;

        // Assert
        Assert.Equal([0, 2], view.Select(static x => x.Value));
        Assert.Equal(view, replica.Items);
    }

    [Fact]
    public void StopsObservingReplacedItem()
    {
        // Arrange
        var source = Source(1, 2);
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        view.ObserveProperty(nameof(Item.Value));
        var item = source[0];

        // Act
        source[0] = new Item(3);

        // Assert
        Assert.False(item.HasSubscribers);
    }

    [Fact]
    public void KeepsObservingItemReplacedWithItself()
    {
        // Arrange
        var source = Source(1, 2);
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        view.ObserveProperty(nameof(Item.Value));
        var item = source[1];
        source[1] = item;
        var replica = new Replica<Item>(view);

        // Act
        item.Value = 0;

        // Assert
        Assert.Equal([0, 1], view.Select(static x => x.Value));
        Assert.Equal(view, replica.Items);
    }

    [Fact]
    public void ObservesItemsAfterReset()
    {
        // Arrange
        var source = new ResetCollection<Item>([new Item(1), new Item(2)]);
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        view.ObserveProperty(nameof(Item.Value));
        var item = new Item(5);
        source.ResetTo([new Item(4), item]);
        var replica = new Replica<Item>(view);

        // Act
        item.Value = 0;

        // Assert
        Assert.Equal([0, 4], view.Select(static x => x.Value));
        Assert.Equal(view, replica.Items);
    }

    [Fact]
    public void StopsObservingItemsRemovedByReset()
    {
        // Arrange
        var source = new ResetCollection<Item>([new Item(1), new Item(2)]);
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        view.ObserveProperty(nameof(Item.Value));
        var item = source[0];

        // Act
        source.ResetTo([new Item(4)]);

        // Assert
        Assert.False(item.HasSubscribers);
    }

    //------------------------------------------------------------
    // DeferRefresh
    //------------------------------------------------------------

    [Fact]
    public void DefersRefreshUntilDisposed()
    {
        // Arrange
        var source = Source(5, 1);
        using var view = new ObservableView<Item>(source);
        view.Comparer = Ascending;
        var replica = new Replica<Item>(view);

        // Act
        using (view.DeferRefresh())
        {
            using (view.DeferRefresh())
            {
                source.Add(new Item(3));
                view.Filter = static x => x.Value < 5;
            }

            source.Add(new Item(2));
            Assert.Empty(replica.Actions);
        }

        // Assert
        Assert.Equal([1, 2, 3], view.Select(static x => x.Value));
        Assert.Equal(view, replica.Items);
    }

    [Fact]
    public void ResetsAfterDeferredClear()
    {
        // Arrange
        var source = Source(1, 2);
        using var view = new ObservableView<Item>(source);
        var replica = new Replica<Item>(view);

        // Act
        using (view.DeferRefresh())
        {
            source.Clear();
            source.Add(new Item(7));
        }

        // Assert
        Assert.Equal([NotifyCollectionChangedAction.Reset], replica.Actions);
        Assert.Equal(source, view);
        Assert.Equal(view, replica.Items);
    }

    //------------------------------------------------------------
    // Dispose
    //------------------------------------------------------------

    [Fact]
    public void StopsTrackingAfterDispose()
    {
        // Arrange
        var source = Source(1, 2);
        var view = new ObservableView<Item>(source) { Comparer = Ascending };
        view.ObserveProperty(nameof(Item.Value));
        var replica = new Replica<Item>(view);

        // Act
        view.Dispose();
        source.Add(new Item(3));
        source[0].Value = 9;

        // Assert
        Assert.Empty(replica.Actions);
        Assert.Equal(2, view.Count);
    }

    //------------------------------------------------------------
    // Notification
    //------------------------------------------------------------

    [Fact]
    public void RaisesCountAndIndexerChanges()
    {
        // Arrange
        var source = Source(1, 2);
        using var view = new ObservableView<Item>(source);
        var names = new List<string?>();
        view.PropertyChanged += (_, e) => names.Add(e.PropertyName);

        // Act
        source.Add(new Item(3));
        source.Move(0, 2);

        // Assert
        Assert.Equal(["Count", "Item[]", "Item[]"], names);
    }

    [Fact]
    public void IsReadOnlyList()
    {
        // Arrange
        var source = Source(1, 2);
        using var view = new ObservableView<Item>(source);
        IList list = view;

        // Act & Assert
        Assert.True(list.IsReadOnly);
        Assert.Same(source[1], list[1]);
        Assert.Equal(1, list.IndexOf(source[1]));
        Assert.True(list.Contains(source[0]));
        Assert.Throws<NotSupportedException>(() => list.Add(new Item(3)));
        Assert.Throws<NotSupportedException>(() => list.RemoveAt(0));
    }

    //------------------------------------------------------------
    // Random
    //------------------------------------------------------------

    [Theory]
    [InlineData(12345u)]
    [InlineData(1u)]
    [InlineData(987654321u)]
    public void KeepsRandomOperationsInSync(uint seed)
    {
        // Arrange
        var random = new SequenceGenerator(seed);
        Func<Item, bool>?[] filters = [null, static x => x.Value % 2 == 0, static x => x.Value >= 50, static x => (x.Value < 30) || (x.Value > 70)];
        IComparer<Item>?[] comparers = [null, Ascending, Descending];
        var source = new ObservableCollection<Item>();
        using var view = new ObservableView<Item>(source);
        view.ObserveProperty(nameof(Item.Value));
        var replica = new Replica<Item>(view);
        Func<Item, bool>? filter = null;
        IComparer<Item>? comparer = null;

        void ChangeSource()
        {
            switch (random.Next(6))
            {
                case 0:
                    source.Add(new Item(random.Next(100)));
                    break;
                case 1:
                    source.Insert(random.Next(source.Count + 1), new Item(random.Next(100)));
                    break;
                case 2 when source.Count > 0:
                    source.RemoveAt(random.Next(source.Count));
                    break;
                case 3 when source.Count > 1:
                    source.Move(random.Next(source.Count), random.Next(source.Count));
                    break;
                case 4 when source.Count > 0:
                    source[random.Next(source.Count)] = new Item(random.Next(100));
                    break;
                case 5 when source.Count > 0:
                    source[random.Next(source.Count)].Value = random.Next(100);
                    break;
            }
        }

        // Act & Assert
        for (var i = 0; i < 2000; i++)
        {
            switch (random.Next(10))
            {
                case 0:
                    filter = filters[random.Next(filters.Length)];
                    view.Filter = filter;
                    break;
                case 1:
                    comparer = comparers[random.Next(comparers.Length)];
                    view.Comparer = comparer;
                    break;
                case 2:
                    using (view.DeferRefresh())
                    {
                        ChangeSource();
                        ChangeSource();
                        ChangeSource();
                    }

                    break;
                case 3 when (source.Count > 40) && (random.Next(10) == 0):
                    source.Clear();
                    break;
                default:
                    ChangeSource();
                    break;
            }

            Assert.Equal(Expected(source, filter, comparer), view);
            Assert.Equal(view, replica.Items);
        }
    }

    private sealed class Item : INotifyPropertyChanged
    {
        public Item(int value)
        {
            Value = value;
            Name = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Value
        {
            get;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public string Name
        {
            get;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public bool HasSubscribers => PropertyChanged is not null;
    }

    private sealed class ResetCollection<T> : ObservableCollection<T>
    {
        public ResetCollection(IEnumerable<T> items)
            : base(items)
        {
        }

        public void ResetTo(IEnumerable<T> items)
        {
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    private sealed class Replica<T>
        where T : notnull
    {
        private readonly ObservableView<T> view;

        public Replica(ObservableView<T> view)
        {
            this.view = view;
            Items.AddRange(view);
            view.CollectionChanged += OnCollectionChanged;
        }

        public List<T> Items { get; } = [];

        public List<NotifyCollectionChangedAction> Actions { get; } = [];

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Actions.Add(e.Action);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Items.Insert(e.NewStartingIndex, (T)e.NewItems![0]!);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Assert.Equal(e.OldItems![0], Items[e.OldStartingIndex]);
                    Items.RemoveAt(e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Move:
                    Move(e);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    Items.AddRange(view);
                    break;
            }
        }

        private void Move(NotifyCollectionChangedEventArgs e)
        {
            var item = Items[e.OldStartingIndex];
            Assert.Equal(e.OldItems![0], item);
            Items.RemoveAt(e.OldStartingIndex);
            Items.Insert(e.NewStartingIndex, item);
        }
    }

    private sealed class SequenceGenerator(uint seed)
    {
        private uint state = seed;

        public int Next(int max)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return (int)(state % (uint)max);
        }
    }
}
