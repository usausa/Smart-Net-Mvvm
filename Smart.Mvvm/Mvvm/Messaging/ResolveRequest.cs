namespace Smart.Mvvm.Messaging;

using System.ComponentModel;
using System.Diagnostics;

[DebuggerDisplay("{" + nameof(ReferenceCount) + "}")]
public sealed class ResolveRequest<T> : IEventRequest<ResolveEventArgs>
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public event EventHandler<ResolveEventArgs>? Requested;

    public int ReferenceCount => Requested?.GetInvocationList().Length ?? 0;

    public T Resolve()
    {
        var args = new ResolveEventArgs();
        Requested?.Invoke(this, args);
        return (T)args.Result!;
    }
}
