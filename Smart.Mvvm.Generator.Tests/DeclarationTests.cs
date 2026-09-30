namespace Smart.Mvvm.Generator.Tests;

using System.Reflection;

using Microsoft.CodeAnalysis;

public sealed class DeclarationTests
{
    private const string ValidViewModel = """

        public sealed partial class ValidVm : ObservableObject
        {
            [ObservableProperty]
            public partial int Count { get; set; }
        }
        """;

    // ------------------------------------------------------------
    // Attributes being typed
    // ------------------------------------------------------------

    [Theory]
    [InlineData("[ObservableProperty(NotifyAlso = nameof(Other))]", "")]
    [InlineData("[ObservableProperty(NotifyAlso = null)]", "")]
    [InlineData("[ObservableProperty]", "[ObservableGeneratorOption(Reactive = )]")]
    public void IncompleteAttributeDoesNotStopOtherViewModels(string propertyAttribute, string typeAttribute)
    {
        var source = $$"""
            #nullable enable
            using Smart.Mvvm;

            {{typeAttribute}}
            public sealed partial class BrokenVm : ObservableObject
            {
                {{propertyAttribute}}
                public partial int Value { get; set; }

                public int Other { get; set; }
            }
            """ + ValidViewModel;

        Assert.DoesNotContain("CS8785", GeneratorTestHelper.GetProblemIds(source));
        Assert.Contains("partial int Count", GeneratorTestHelper.GetAllGeneratedSource(source), StringComparison.Ordinal);
    }

    // ------------------------------------------------------------
    // Declaration
    // ------------------------------------------------------------

    [Theory]
    [InlineData("partial string Title { get; set; }")]
    [InlineData("public virtual partial string Title { get; set; }")]
    [InlineData("public required partial string Title { get; set; }")]
    [InlineData("public partial string Title { get; init; }")]
    [InlineData("public partial string Title { get; private set; }")]
    [InlineData("internal partial string? Title { private get; set; }")]
    [InlineData("public partial string @event { get; set; }")]
    public void ImplementationRepeatsDeclaration(string property)
    {
        var source = $$"""
            #nullable enable
            using Smart.Mvvm;

            public partial class DeclarationVm : ObservableObject
            {
                [ObservableProperty]
                {{property}}
            }
            """;

        Assert.Empty(GeneratorTestHelper.GetProblemIds(source));
    }

    [Theory]
    [InlineData("public static partial string Title { get; set; }")]
    [InlineData("public partial string Title { get; set; }\n\n    public partial string Title { get => string.Empty; set { } }")]
    public void Smv0001UnsupportedPropertyEmitsDiagnostic(string property)
    {
        var source = $$"""
            #nullable enable
            using Smart.Mvvm;

            public partial class DeclarationVm : ObservableObject
            {
                [ObservableProperty]
                {{property}}
            }
            """;

        Assert.Contains("SMV0001", GeneratorTestHelper.GetProblemIds(source));
    }

    [Fact]
    public void ReactiveOptionDoesNotDependOnUserNamespaces()
    {
        const string source = """
            using Smart.Mvvm;

            namespace Contoso.System.Reactive.Linq
            {
                public static class Observable
                {
                }
            }

            namespace Contoso.ViewModels
            {
                [ObservableGeneratorOption(Reactive = true, ViewModel = true)]
                public sealed partial class ReactiveVm : Smart.Mvvm.ViewModels.ViewModelBase
                {
                    [ObservableProperty]
                    public partial string Title { get; set; }
                }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.DoesNotContain("using ", generated, StringComparison.Ordinal);
        Assert.Contains("global::System.ObservableExtensions.Subscribe(ObserveTitle(), action)", generated, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------
    // Diagnostics
    // ------------------------------------------------------------

    [Fact]
    public void ErrorsCannotBeSuppressed()
    {
        var descriptors = typeof(ObservablePropertyGenerator).Assembly.GetType("Smart.Mvvm.Generator.Diagnostics", throwOnError: true)!
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(static x => x.PropertyType == typeof(DiagnosticDescriptor))
            .Select(static x => (DiagnosticDescriptor)x.GetValue(null)!)
            .ToList();

        Assert.All(
            descriptors.Where(static x => x.DefaultSeverity == DiagnosticSeverity.Error),
            static x => Assert.Equal([WellKnownDiagnosticTags.NotConfigurable, WellKnownDiagnosticTags.Compiler], x.CustomTags));
        Assert.All(
            descriptors.Where(static x => x.DefaultSeverity != DiagnosticSeverity.Error),
            static x => Assert.Empty(x.CustomTags));
    }
}
