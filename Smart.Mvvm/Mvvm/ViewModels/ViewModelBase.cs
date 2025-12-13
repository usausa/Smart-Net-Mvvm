namespace Smart.Mvvm.ViewModels;

using System.ComponentModel;

using Smart.Mvvm;
using Smart.Mvvm.Messaging;

public abstract class ViewModelBase : ObservableObject, IDisposable
{
    private static readonly PropertyChangedEventArgs ErrorsChangedEventArgs = new(nameof(Errors));

    private static readonly ViewModelOptions DefaultOptions = new();

    private int disposed;

    private Disposables? disposables;

    // ------------------------------------------------------------
    // Dispose
    // ------------------------------------------------------------

    public bool IsDisposed => Interlocked.CompareExchange(ref disposed, 0, 0) == 1;

    protected Disposables Disposables => disposables ??= new Disposables();

    // ------------------------------------------------------------
    // ErrorInfo
    // ------------------------------------------------------------

    public ErrorInfo Errors { get; } = new();

    // ------------------------------------------------------------
    // Busy
    // ------------------------------------------------------------

    public IBusyState BusyState { get; }

    // ------------------------------------------------------------
    // Messenger
    // ------------------------------------------------------------

    public IMessenger Messenger { get; }

    // ------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------

    protected ViewModelBase(IViewModelOptions? options = null)
    {
        options ??= DefaultOptions;
        BusyState = options.BusyState;
        Messenger = options.Messenger;
        Errors.Handler = OnErrorChanged;
    }

#pragma warning disable CA1063
    ~ViewModelBase()
    {
        if (Interlocked.CompareExchange(ref disposed, 1, 0) == 0)
        {
            Dispose(false);
        }
    }
#pragma warning restore CA1063

#pragma warning disable CA1063
    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref disposed, 1, 0) == 0)
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
#pragma warning restore CA1063

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Release managed resources
            disposables?.Dispose();
            disposables = null;

            Errors.Dispose();
        }

        // Release unmanaged resources
    }

    private void OnErrorChanged()
    {
        RaisePropertyChanged(ErrorsChangedEventArgs);
    }
}
