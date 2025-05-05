// ReSharper disable StringLiteralTypo
namespace Develop;

using System.ComponentModel;

using BunnyTail.ObservableProperty;

internal static class Program
{
    public static void Main()
    {
        var vm = new ViewModel();

        var called = new HashSet<string>();
        vm.PropertyChanged += (_, args) => called.Add(args.PropertyName!);

        vm.FirstName = "Byleth";
        vm.LastName = "Eisner";

        Console.WriteLine(called.Count);
    }
}

internal abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
}

internal partial class ViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyAlso(nameof(FirstName))]
    public partial string FirstName { get; set; }

    [ObservableProperty]
    [NotifyAlso(nameof(FirstName))]
    public partial string LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
