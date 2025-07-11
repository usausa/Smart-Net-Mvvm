namespace Smart.Mvvm.ViewModels;

using Smart.Mvvm.Messaging;

public class ViewModelOptions : IViewModelOptions
{
    public IBusyState BusyState { get; set; } = Smart.Mvvm.ViewModels.BusyState.Default;

    public IMessenger Messenger { get; set; } = Smart.Mvvm.Messaging.Messenger.Default;
}
