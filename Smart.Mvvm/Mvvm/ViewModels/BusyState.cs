namespace Smart.Mvvm.ViewModels;

using System.ComponentModel;
using System.Diagnostics;

[DebuggerDisplay("Reference = {" + nameof(PropertyChangedReferenceCount) + "}, IsBusy = {" + nameof(IsBusy) + "}, Counter = {" + nameof(counter) + "}")]
public class BusyState : ObservableObject, IBusyState
{
    private static readonly PropertyChangedEventArgs IsBusyChangedEventArgs = new(nameof(IsBusy));

    public static IBusyState Default { get; } = new BusyState();

    private int counter;

    public bool IsBusy => counter > 0;

    public void Require()
    {
        var current = IsBusy;
        counter++;
        if (current != IsBusy)
        {
            RaisePropertyChanged(IsBusyChangedEventArgs);
        }
    }

    public void Release()
    {
        var current = IsBusy;
        counter--;
        if (current != IsBusy)
        {
            RaisePropertyChanged(IsBusyChangedEventArgs);
        }
    }

    public void Reset()
    {
        var current = IsBusy;
        counter = 0;
        if (current != IsBusy)
        {
            RaisePropertyChanged(IsBusyChangedEventArgs);
        }
    }
}
