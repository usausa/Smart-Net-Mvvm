namespace Smart.Mvvm.Internal;

using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;

public sealed class PooledList<T> : IReadOnlyList<T>, IDisposable
{
    private T[] array;

    private int count;

    public int Count => count;

    public T this[int index] => array[index];

    public PooledList(int initial = 4)
    {
        array = ArrayPool<T>.Shared.Rent(initial);
    }

    public void Dispose()
    {
        if (array.Length > 0)
        {
            ArrayPool<T>.Shared.Return(array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            array = [];
            count = 0;
        }
    }

    public void Add(T item)
    {
        if (count == array.Length)
        {
            var newArray = ArrayPool<T>.Shared.Rent(count * 2);
            Array.Copy(array, newArray, count);
            ArrayPool<T>.Shared.Return(array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            array = newArray;
        }

        array[count] = item;
        count++;
    }

    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            array.AsSpan(0, count).Clear();
        }
        count = 0;
    }

    public IEnumerator<T> GetEnumerator() => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private struct Enumerator : IEnumerator<T>
    {
        private readonly PooledList<T> list;
        private int index;
        private T current;

        public Enumerator(PooledList<T> list)
        {
            this.list = list;
            current = default!;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if ((uint)index < (uint)list.count)
            {
                current = list.array[index];
                index++;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            index = 0;
            current = default!;
        }

        public T Current => current;

        object? IEnumerator.Current => current;
    }
}
