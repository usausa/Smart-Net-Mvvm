namespace Smart.Mvvm.ViewModels;

using Smart.Mvvm.Messaging;

public interface IViewModelOptions
{
    IBusyState BusyState { get; }

    IMessenger Messenger { get; }
}
