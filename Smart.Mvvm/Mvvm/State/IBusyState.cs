namespace Smart.Mvvm.State;

using System.ComponentModel;

public interface IBusyState : INotifyPropertyChanged
{
    bool IsBusy { get; }

    void Require();

    void Release();

    void Reset();
}
