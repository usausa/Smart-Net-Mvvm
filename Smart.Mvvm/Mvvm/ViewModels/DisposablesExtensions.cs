namespace Smart.Mvvm.ViewModels;

public static class DisposablesExtensions
{
    public static T Add<T>(this T disposable, Disposables disposables)
        where T : IDisposable
    {
        disposables.Add(disposable);
        return disposable;
    }
}
