namespace Smart.Mvvm.Messaging;

using System.ComponentModel;
using System.Diagnostics;

[DebuggerDisplay("Reference = {" + nameof(ReferenceCount) + "}")]
public sealed class CancelRequest : IEventRequest<CancelEventArgs>
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public event EventHandler<CancelEventArgs>? Requested;

    private int ReferenceCount => Requested?.GetInvocationList().Length ?? 0;

    public bool IsCancel()
    {
        var args = new CancelEventArgs();
        Requested?.Invoke(this, args);
        return args.Cancel;
    }
}
