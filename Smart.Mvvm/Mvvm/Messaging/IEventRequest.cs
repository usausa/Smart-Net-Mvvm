namespace Smart.Mvvm.Messaging;

// ReSharper disable once TypeParameterCanBeVariant
public interface IEventRequest<TEventArgs>
    where TEventArgs : EventArgs
{
    event EventHandler<TEventArgs> Requested;
}
