namespace Smart.Mvvm.ViewModels;

using System.ComponentModel;

using Smart.Mvvm;
using Smart.Mvvm.Messaging;

#pragma warning disable IDE0032
// ReSharper disable ReplaceWithFieldKeyword
public abstract class ViewModelBase : ObservableObject, IDisposable
{
    private static readonly PropertyChangedEventArgs ErrorsChangedEventArgs = new(nameof(Errors));

    private Disposables? disposables;

    // ------------------------------------------------------------
    // Disposables
    // ------------------------------------------------------------

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

    protected ViewModelBase()
        : this(Smart.Mvvm.ViewModels.BusyState.Default, Smart.Mvvm.Messaging.Messenger.Default)
    {
    }

    protected ViewModelBase(IBusyState busyState)
        : this(busyState, Smart.Mvvm.Messaging.Messenger.Default)
    {
    }

    protected ViewModelBase(IMessenger messenger)
        : this(Smart.Mvvm.ViewModels.BusyState.Default, messenger)
    {
    }

    protected ViewModelBase(IBusyState busyState, IMessenger messenger)
    {
        BusyState = busyState;
        Messenger = messenger;
        Errors.Handler = OnErrorChanged;
    }

    ~ViewModelBase()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            disposables?.Dispose();
            disposables = null;

            Errors.Dispose();
        }
    }

    private void OnErrorChanged()
    {
        RaisePropertyChanged(ErrorsChangedEventArgs);
    }
}
