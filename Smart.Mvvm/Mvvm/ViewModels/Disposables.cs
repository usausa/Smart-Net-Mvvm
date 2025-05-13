namespace Smart.Mvvm.ViewModels;

using Smart.Mvvm.Internal;

public sealed class Disposables : IDisposable
{
    private PooledList<IDisposable>? disposables;

    public void Dispose()
    {
        if (disposables is not null)
        {
            foreach (var d in disposables)
            {
                d.Dispose();
            }

            disposables.Dispose();
            disposables = null;
        }
    }

    public void Add(IDisposable disposable)
    {
        disposables ??= new PooledList<IDisposable>(16);
        disposables.Add(disposable);
    }
}

public static class DisposablesExtensions
{
    public static T Add<T>(this T disposable, Disposables disposables)
        where T : IDisposable
    {
        disposables.Add(disposable);
        return disposable;
    }
}
