namespace Smart.Mvvm.Messaging;

using System.ComponentModel;
using System.Diagnostics;

[DebuggerDisplay("Reference = {" + nameof(ReferenceCount) + "}")]
public sealed class ResolveRequest<T> : IEventRequest<ResolveEventArgs>
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public event EventHandler<ResolveEventArgs>? Requested;

    private int ReferenceCount => Requested?.GetInvocationList().Length ?? 0;

    public T Resolve()
    {
        var args = new ResolveEventArgs();
        Requested?.Invoke(this, args);
        return (T)args.Result!;
    }
}
