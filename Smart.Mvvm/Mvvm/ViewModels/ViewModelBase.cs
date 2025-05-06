namespace Smart.Mvvm.ViewModels;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Smart.Mvvm.Internal;
using Smart.Mvvm.Messaging;

#pragma warning disable IDE0032
// ReSharper disable ReplaceWithFieldKeyword
public abstract class ViewModelBase : ObservableObject, IDataErrorInfo, IDisposable
{
    private ListDisposable? disposables;

    private IBusyState? busyState;

    private IMessenger? messenger;

    // ------------------------------------------------------------
    // Disposables
    // ------------------------------------------------------------

    protected ICollection<IDisposable> Disposables => disposables ??= [];

    // ------------------------------------------------------------
    // Busy
    // ------------------------------------------------------------

    public IBusyState BusyState => busyState ??= new BusyState();

    // ------------------------------------------------------------
    // Messenger
    // ------------------------------------------------------------

    public IMessenger Messenger => messenger ??= new Messenger();

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
    {
    }

    protected ViewModelBase(IBusyState busyState)
    {
        this.busyState = busyState;
    }

    protected ViewModelBase(IMessenger messenger)
    {
        this.messenger = messenger;
    }

    protected ViewModelBase(IBusyState busyState, IMessenger messenger)
    {
        this.busyState = busyState;
        this.messenger = messenger;
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
