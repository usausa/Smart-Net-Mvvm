namespace Smart.Mvvm.ViewModels;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Smart.Mvvm;
using Smart.Mvvm.Internal;
using Smart.Mvvm.Messaging;
using Smart.Mvvm.State;

#pragma warning disable IDE0032
// ReSharper disable ReplaceWithFieldKeyword
public abstract class ViewModelBase : ObservableObject, IDataErrorInfo, IDisposable
{
    private ListDisposable? disposables;

    // ------------------------------------------------------------
    // Disposables
    // ------------------------------------------------------------

    protected ICollection<IDisposable> Disposables => disposables ??= [];

    // ------------------------------------------------------------
    // Busy
    // ------------------------------------------------------------

    public IBusyState BusyState { get; }

    // ------------------------------------------------------------
    // Messenger
    // ------------------------------------------------------------

    public IMessenger Messenger { get; }

    // ------------------------------------------------------------
    // Validation
    // ------------------------------------------------------------

    public string this[string columnName]
    {
        get
        {
            var pi = GetType().GetProperty(columnName);
            if (pi is null)
            {
                return string.Empty;
            }

            var results = new List<ValidationResult>();
            if (Validator.TryValidateProperty(
                pi.GetValue(this, null),
                new ValidationContext(this, null, null) { MemberName = columnName },
                results))
            {
                return string.Empty;
            }

            return results.First().ErrorMessage ?? string.Empty;
        }
    }

    public string Error
    {
        get
        {
            var results = new List<ValidationResult>();
            if (Validator.TryValidateObject(
                this,
                new ValidationContext(this, null, null),
                results))
            {
                return string.Empty;
            }

            return String.Join(Environment.NewLine, results.Select(static r => r.ErrorMessage));
        }
    }

    // ------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------

    protected ViewModelBase()
        : this(Smart.Mvvm.State.BusyState.Default, Smart.Mvvm.Messaging.Messenger.Default)
    {
    }

    protected ViewModelBase(IBusyState busyState)
        : this(busyState, Smart.Mvvm.Messaging.Messenger.Default)
    {
    }

    protected ViewModelBase(IMessenger messenger)
        : this(Smart.Mvvm.State.BusyState.Default, messenger)
    {
    }

    protected ViewModelBase(IBusyState busyState, IMessenger messenger)
    {
        BusyState = busyState;
        Messenger = messenger;
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
        }
    }
}
