namespace Smart.Mvvm.Generator.Tests;

public sealed class DiagnosticTests
{
    [Fact]
    public void Smv0001NonPartialPropertyEmitsDiagnostic()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class MyViewModel : ObservableObject
            {
                [ObservableProperty]
                public string Name { get; set; } = default!;
            }
            """;

        var diagnostics = GeneratorTestHelper.GetDiagnostics(source);

        Assert.Contains(diagnostics, d => d.Id == "SMV0001");
    }

    [Fact]
    public void Smv0002PropertyWithoutSetterEmitsDiagnostic()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class MyViewModel : ObservableObject
            {
                [ObservableProperty]
                public partial string Name { get; }
            }
            """;

        var diagnostics = GeneratorTestHelper.GetDiagnostics(source);

        Assert.Contains(diagnostics, d => d.Id == "SMV0002");
    }

    [Fact]
    public void Smv0003ClassNotExtendingObservableObjectEmitsDiagnostic()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class PlainClass
            {
                [ObservableProperty]
                public partial string Name { get; set; }
            }
            """;

        var diagnostics = GeneratorTestHelper.GetDiagnostics(source);

        Assert.Contains(diagnostics, d => d.Id == "SMV0003");
    }

    [Fact]
    public void Smv0004NonPartialContainingTypeEmitsDiagnostic()
    {
        const string source =
            """
            using Smart.Mvvm;

            public class Outer
            {
                public partial class Inner : ObservableObject
                {
                    [ObservableProperty]
                    public partial string Name { get; set; }
                }
            }
            """;

        var diagnostics = GeneratorTestHelper.GetDiagnostics(source);

        Assert.Contains(diagnostics, d => d.Id == "SMV0004");
    }

    [Fact]
    public void Smv0004AllPartialContainingTypesEmitsNoDiagnostic()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class Outer
            {
                public partial class Inner : ObservableObject
                {
                    [ObservableProperty]
                    public partial string Name { get; set; }
                }
            }
            """;

        var diagnostics = GeneratorTestHelper.GetDiagnostics(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == "SMV0004");
    }

    [Fact]
    public void Smv0005ViewModelOptionWithoutReactiveEmitsDiagnostic()
    {
        const string source =
            """
            using Smart.Mvvm;
            using Smart.Mvvm.ViewModels;

            [ObservableGeneratorOption(ViewModel = true)]
            public sealed partial class MyVm : ViewModelBase
            {
                [ObservableProperty]
                public partial string Title { get; set; }
            }
            """;

        var diagnostics = GeneratorTestHelper.GetDiagnostics(source);

        Assert.Contains(diagnostics, d => d.Id == "SMV0005");
    }

    [Fact]
    public void Smv0005ViewModelOptionWithReactiveEmitsNoDiagnostic()
    {
        const string source =
            """
            using Smart.Mvvm;
            using Smart.Mvvm.ViewModels;

            [ObservableGeneratorOption(Reactive = true, ViewModel = true)]
            public sealed partial class MyVm : ViewModelBase
            {
                [ObservableProperty]
                public partial string Title { get; set; }
            }
            """;

        var diagnostics = GeneratorTestHelper.GetDiagnostics(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == "SMV0005");
    }
}
