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
        var handler = Requested;
        if (handler is null)
        {
            return false;
        }

        var args = new CancelEventArgs();
        handler(this, args);
        return args.Cancel;
    }
}
