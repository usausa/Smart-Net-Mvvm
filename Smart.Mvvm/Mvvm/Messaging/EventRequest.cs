namespace Smart.Mvvm.Messaging;

using System.ComponentModel;
using System.Diagnostics;

[DebuggerDisplay("Reference = {" + nameof(ReferenceCount) + "}")]
public sealed class EventRequest : IEventRequest<ParameterEventArgs>
{
    private static readonly ParameterEventArgs EmptyArgs = new(null);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public event EventHandler<ParameterEventArgs>? Requested;

    private int ReferenceCount => Requested?.GetInvocationList().Length ?? 0;

    public void Request()
    {
        Requested?.Invoke(this, EmptyArgs);
    }
}

[DebuggerDisplay("Reference = {" + nameof(ReferenceCount) + "}")]
public sealed class EventRequest<T> : IEventRequest<ParameterEventArgs>
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public event EventHandler<ParameterEventArgs>? Requested;

    private int ReferenceCount => Requested?.GetInvocationList().Length ?? 0;

    public void Request(T value)
    {
        Requested?.Invoke(this, new ParameterEventArgs(value));
    }
}
