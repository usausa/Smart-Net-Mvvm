// ReSharper disable StringLiteralTypo
namespace Smart.Mvvm;

using Smart.Mvvm.ViewModels;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public abstract class AppViewModel : ViewModelBase
{
}

public sealed partial class ViewModel : AppViewModel
{
    [ObservableProperty(NotifyAlso = [nameof(FullName)])]
    public partial string FirstName { get; set; } = default!;

    [ObservableProperty(NotifyAlso = [nameof(FullName)])]
    public partial string LastName { get; set; } = default!;

    [ObservableProperty]
    public partial int Age { get; set; }

    [ObservableProperty]
    public partial int? Value { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
