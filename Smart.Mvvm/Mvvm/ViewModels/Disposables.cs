namespace Smart.Mvvm.ViewModels;

using System.Diagnostics;

using Smart.Mvvm.Internal;

[DebuggerDisplay("Count = {" + nameof(Count) + "}")]
public sealed class Disposables : IDisposable
{
    private const int DefaultCapacity = 32;

    private PooledList<IDisposable>? disposables;

    private int Count => disposables?.Count ?? 0;

    public void Dispose()
    {
        if (disposables is not null)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < disposables.Count; i++)
            {
                disposables[i].Dispose();
            }

            disposables.Dispose();
            disposables = null;
        }
    }

    public void Add(IDisposable disposable)
    {
        disposables ??= new PooledList<IDisposable>(DefaultCapacity);
        disposables.Add(disposable);
    }
}
