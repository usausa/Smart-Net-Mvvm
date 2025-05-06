namespace Smart.Mvvm.State;

using System.ComponentModel;

public class BusyState : ObservableObject, IBusyState
{
    private static readonly PropertyChangedEventArgs IsBusyChangedEventArgs = new(nameof(IsBusy));

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
