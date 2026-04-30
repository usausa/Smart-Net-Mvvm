namespace Smart.Mvvm.Internal;

using System.Buffers;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0032
public sealed class PooledList<T> : IReadOnlyList<T>, IDisposable
{
    private const int DefaultCapacity = 16;

    private static readonly T[] EmptyArray = [];

    private T[] items;
    private int size;

    // Read-only property describing how many elements are in the List.
    public int Count => size;

    public T this[int index] => items[index];

    public PooledList()
    {
        items = EmptyArray;
    }

    public PooledList(int capacity)
    {
        items = ArrayPool<T>.Shared.Rent(capacity);
    }

    public void Dispose()
    {
        if (items.Length > 0)
        {
            ArrayPool<T>.Shared.Return(items, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            items = EmptyArray;
        }
        size = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Add(T item)
    {
        var array = items;
        var length = size;
        if ((uint)length < (uint)array.Length)
        {
            size = length + 1;
            array[length] = item;
        }
        else
        {
            AddWithResize(item);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddWithResize(T item)
    {
        var length = size;
        Grow(length + 1);
        size = length + 1;
        items[length] = item;
    }

    private void Grow(int requiredCapacity)
    {
        var length = items.Length == 0 ? DefaultCapacity : items.Length * 2;
        if ((uint)length < (uint)requiredCapacity)
        {
            length = requiredCapacity;
        }

        var oldItems = items;
        var newItems = ArrayPool<T>.Shared.Rent(length);
        var count = size;
        if (count > 0)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                oldItems.AsSpan(0, count).CopyTo(newItems);
            }
            else
            {
                ref var source = ref MemoryMarshal.GetArrayDataReference(oldItems);
                ref var destination = ref MemoryMarshal.GetArrayDataReference(newItems);
                Unsafe.CopyBlockUnaligned(
                    ref Unsafe.As<T, byte>(ref destination),
                    ref Unsafe.As<T, byte>(ref source),
                    (uint)(count * Unsafe.SizeOf<T>()));
            }
        }

        if (oldItems.Length > 0)
        {
            ArrayPool<T>.Shared.Return(oldItems, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }
        items = newItems;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            var length = size;
            size = 0;
            if (length > 0)
            {
                items.AsSpan(0, length).Clear();
            }
        }
        else
        {
            size = 0;
        }
    }

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => new EnumeratorObject(this);

    private sealed class EnumeratorObject : IEnumerator
    {
        private readonly T[] items;
        private readonly int count;
        private int index;

        public EnumeratorObject(PooledList<T> list)
        {
            items = list.items;
            count = list.size;
            index = -1;
        }

        public object? Current => items[index];

        public bool MoveNext()
        {
            var next = index + 1;
            if ((uint)next < (uint)count)
            {
                index = next;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            index = -1;
        }
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly PooledList<T> list;
        private int index;
        private T? current;

        internal Enumerator(PooledList<T> list)
        {
            this.list = list;
            current = default;
        }

        public readonly void Dispose()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool MoveNext()
        {
            var localList = list;
            if ((uint)index < (uint)localList.size)
            {
                current = localList.items[index];
                index++;
                return true;
            }

            current = default;
            return false;
        }

        void IEnumerator.Reset()
        {
            index = 0;
            current = default;
        }

        public readonly T Current => current!;

        readonly object? IEnumerator.Current => current;
    }
}
#pragma warning restore IDE0032
