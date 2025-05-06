// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable StringLiteralTypo
#pragma warning disable CA1812
namespace Develop;

using Smart.Mvvm;

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

internal sealed partial class ViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyAlso(nameof(FullName))]
    public partial string FirstName { get; set; } = default!;

    [ObservableProperty]
    [NotifyAlso(nameof(FullName))]
    public partial string LastName { get; set; } = default!;

    [ObservableProperty]
    public partial int Age { get; set; }

    [ObservableProperty]
    public partial int? Value { get; set; }

    [ObservableProperty]
    public partial Data? Data { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}

internal sealed class Data
{
    public int Value { get; set; }
}
