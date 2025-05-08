// ReSharper disable StringLiteralTypo
namespace Smart.Mvvm;

public class BasicTest
{
    [Fact]
    public void TestBasic()
    {
        using var vm = new ViewModel();

        var called = new HashSet<string>();
        vm.PropertyChanged += (_, args) => called.Add(args.PropertyName!);

        vm.FirstName = "Byleth";
        vm.LastName = "Eisner";

        Assert.Equal(3, called.Count);
    }
}
