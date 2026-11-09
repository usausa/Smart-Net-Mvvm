namespace Smart.Mvvm.Internal;

using System.Collections.Specialized;
using System.ComponentModel;

internal static class CollectionEventArgs
{
    public static PropertyChangedEventArgs CountChanged { get; } = new("Count");

    public static PropertyChangedEventArgs IndexerChanged { get; } = new("Item[]");

    public static NotifyCollectionChangedEventArgs Reset { get; } = new(NotifyCollectionChangedAction.Reset);
}
