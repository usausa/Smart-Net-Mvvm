namespace Smart.Mvvm.Messaging;

using System.ComponentModel;
using System.Diagnostics;

[DebuggerDisplay("Reference = {" + nameof(ReferenceCount) + "}")]
public sealed class Messenger : IMessenger
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public event EventHandler<MessengerEventArgs>? Received;

    public static IMessenger Default { get; } = new Messenger();

    private int ReferenceCount => Received?.GetInvocationList().Length ?? 0;

    public void Send(string label)
    {
        Received?.Invoke(this, new MessengerEventArgs(label, typeof(object), null));
    }

    public void Send<T>(T message)
    {
        Received?.Invoke(this, new MessengerEventArgs(string.Empty, typeof(T), message));
    }

    public void Send<T>(string label, T parameter)
    {
        Received?.Invoke(this, new MessengerEventArgs(label, typeof(T), parameter));
    }
}
