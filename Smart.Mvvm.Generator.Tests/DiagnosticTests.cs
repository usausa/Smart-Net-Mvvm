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

    [Fact]
    public void Smv0005ViewModelOptionWithoutViewModelBaseEmitsDiagnostic()
    {
        const string source =
            """
            using Smart.Mvvm;

            [ObservableGeneratorOption(Reactive = true, ViewModel = true)]
            public sealed partial class Model : ObservableObject
            {
                [ObservableProperty]
                public partial string Title { get; set; }
            }
            """;

        var diagnostics = GeneratorTestHelper.GetDiagnostics(source);

        Assert.Contains(diagnostics, d => d.Id == "SMV0005");
        Assert.DoesNotContain("SubscribeTitle", GeneratorTestHelper.GetAllGeneratedSource(source), StringComparison.Ordinal);
    }

    [Fact]
    public void Smv0005InheritedOptionIsReportedOnceAtTheAttribute()
    {
        const string source =
            """
            using Smart.Mvvm;
            using Smart.Mvvm.ViewModels;

            [ObservableGeneratorOption(ViewModel = true)]
            public abstract partial class BaseVm : ViewModelBase
            {
            }

            public sealed partial class FirstVm : BaseVm
            {
                [ObservableProperty]
                public partial string Title { get; set; }
            }

            public sealed partial class SecondVm : BaseVm
            {
                [ObservableProperty]
                public partial string Title { get; set; }
            }
            """;

        var diagnostics = GeneratorTestHelper.GetDiagnostics(source);

        var diagnostic = Assert.Single(diagnostics, static d => d.Id == "SMV0005");
        Assert.Equal(3, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
    }

    [Fact]
    public void Smv0001IndexerEmitsDiagnostic()
    {
        const string source =
            """
            using Smart.Mvvm;

            public sealed partial class Model : ObservableObject
            {
                [ObservableProperty]
                public partial string this[int index] { get; set; }
            }
            """;

        var diagnostics = GeneratorTestHelper.GetDiagnostics(source);

        Assert.Contains(diagnostics, d => d.Id == "SMV0001");
    }

    [Fact]
    public void Smv0004FileLocalTypeEmitsDiagnostic()
    {
        const string source =
            """
            using Smart.Mvvm;

            file sealed partial class Model : ObservableObject
            {
                [ObservableProperty]
                public partial string Title { get; set; }
            }
            """;

        var diagnostics = GeneratorTestHelper.GetDiagnostics(source);

        Assert.Contains(diagnostics, d => d.Id == "SMV0004");
    }

    [Fact]
    public void Smv0006CaseOnlyTypeNamesGenerateTheFirstOnly()
    {
        const string source =
            """
            using Smart.Mvvm;

            public sealed partial class Model : ObservableObject
            {
                [ObservableProperty]
                public partial string Title { get; set; }
            }

            public sealed partial class model : ObservableObject
            {
                [ObservableProperty]
                public partial string Title { get; set; }
            }
            """;

        var problems = GeneratorTestHelper.GetProblemIds(source);

        Assert.Contains("SMV0006", problems);
        Assert.DoesNotContain("CS8785", problems);
    }

    [Fact]
    public void ObsoletePropertyTypeCompilesWithoutWarning()
    {
        const string source =
            """
            using System;
            using Smart.Mvvm;

            [Obsolete]
            public sealed class Legacy
            {
            }

            public sealed partial class Model : ObservableObject
            {
            #pragma warning disable CS0612
                [ObservableProperty]
                public partial Legacy? Value { get; set; }
            #pragma warning restore CS0612
            }
            """;

        Assert.Empty(GeneratorTestHelper.GetProblemIds(source));
    }
}
