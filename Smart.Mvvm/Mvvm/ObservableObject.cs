namespace Smart.Mvvm;

using System.ComponentModel;

public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected int PropertyChangedReferenceCount => PropertyChanged?.GetInvocationList().Length ?? 0;

#pragma warning disable CA1030
    protected virtual void RaisePropertyChanged(PropertyChangedEventArgs args)
    {
        PropertyChanged?.Invoke(this, args);
    }

    protected void RaisePropertyChanged(string? propertyName)
    {
        RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
    }
#pragma warning restore CA1030
}
