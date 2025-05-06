namespace Smart.Mvvm.Messaging;

public sealed class ResolveEventArgs : EventArgs
{
    public object? Result { get; set; }
}
