namespace BunnyTail.ObservableProperty;

using System.ComponentModel;

public interface IObservableObject
{
#pragma warning disable CA1030
    void RaisePropertyChanged(PropertyChangedEventArgs args);
#pragma warning restore CA1030
}
