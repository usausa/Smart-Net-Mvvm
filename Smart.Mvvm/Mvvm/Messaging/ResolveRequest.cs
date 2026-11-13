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
        var handler = Requested;
        if (handler is null)
        {
            return default!;
        }

        var args = new ResolveEventArgs();
        handler(this, args);
        return args.Result is null ? default! : (T)args.Result;
    }
}
