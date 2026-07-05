namespace Smart.Mvvm.Generator.Tests;

using Microsoft.CodeAnalysis;

public class ObservablePropertyGeneratorTest
{
    //-----------------------------------------------------------------------
    // Basic
    //-----------------------------------------------------------------------

    [Fact]
    public void BasicStringPropertyGeneratesEqualityComparerCheck()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class MyViewModel : ObservableObject
            {
                [ObservableProperty]
                public partial string Name { get; set; }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.Contains("EqualityComparer<string>", generated, StringComparison.Ordinal);
        Assert.Contains("partial string Name", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void BasicStringPropertySetterCallsRaisePropertyChanged()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class MyViewModel : ObservableObject
            {
                [ObservableProperty]
                public partial string Name { get; set; }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.Contains("RaisePropertyChanged(__NameChangedEventArgs)", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void BasicStringPropertyGetterUsesFieldKeyword()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class MyViewModel : ObservableObject
            {
                [ObservableProperty]
                public partial string Name { get; set; }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.Contains("get => field;", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void BasicStringPropertyStaticEventArgsFieldGenerated()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class MyViewModel : ObservableObject
            {
                [ObservableProperty]
                public partial string Name { get; set; }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.Contains("__NameChangedEventArgs", generated, StringComparison.Ordinal);
        Assert.Contains("PropertyChangedEventArgs", generated, StringComparison.Ordinal);
    }

    //-----------------------------------------------------------------------
    // Type
    //-----------------------------------------------------------------------

    [Fact]
    public void IntPropertyGeneratesEqualityComparerForInt()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class MyViewModel : ObservableObject
            {
                [ObservableProperty]
                public partial int Count { get; set; }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.Contains("EqualityComparer<int>", generated, StringComparison.Ordinal);
        Assert.Contains("partial int Count", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void NullableIntPropertyGeneratesEqualityComparerForNullableInt()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class MyViewModel : ObservableObject
            {
                [ObservableProperty]
                public partial int? Value { get; set; }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.Contains("EqualityComparer<int?>", generated, StringComparison.Ordinal);
        Assert.Contains("partial int? Value", generated, StringComparison.Ordinal);
    }

    //-----------------------------------------------------------------------
    // NotifyAlso
    //-----------------------------------------------------------------------

    [Fact]
    public void NotifyAlsoGeneratesAdditionalRaisePropertyChangedCall()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class MyViewModel : ObservableObject
            {
                [ObservableProperty(NotifyAlso = [nameof(FullName)])]
                public partial string FirstName { get; set; }

                public string FullName => FirstName ?? string.Empty;
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.Contains("RaisePropertyChanged(__FirstNameChangedEventArgs)", generated, StringComparison.Ordinal);
        Assert.Contains("RaisePropertyChanged(__FullNameChangedEventArgs)", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void NotifyAlsoEventArgsFieldGeneratedForNotifiedProperty()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class MyViewModel : ObservableObject
            {
                [ObservableProperty(NotifyAlso = [nameof(FullName)])]
                public partial string FirstName { get; set; }

                public string FullName => FirstName ?? string.Empty;
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.Contains("__FullNameChangedEventArgs", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void NotifyAlsoNonIdentifierStringGeneratesValidCode()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class MyViewModel : ObservableObject
            {
                [ObservableProperty(NotifyAlso = ["Item[]"])]
                public partial string Name { get; set; }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);
        var diagnostics = GeneratorTestHelper.GetDiagnosticsAll(source);

        Assert.Contains("new(\"Item[]\")", generated, StringComparison.Ordinal);
        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void NotifyAlsoStringWithQuoteIsEscaped()
    {
        const string source =
            """"
            using Smart.Mvvm;

            public partial class MyViewModel : ObservableObject
            {
                [ObservableProperty(NotifyAlso = ["A\"B"])]
                public partial string Name { get; set; }
            }
            """";

        var generated = GeneratorTestHelper.GetGeneratedSource(source);
        var diagnostics = GeneratorTestHelper.GetDiagnosticsAll(source);

        Assert.Contains("new(\"A\\\"B\")", generated, StringComparison.Ordinal);
        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    //-----------------------------------------------------------------------
    // Nested type
    //-----------------------------------------------------------------------

    [Fact]
    public void NestedTypeGeneratesNestedPartialDeclarations()
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

        var generated = GeneratorTestHelper.GetGeneratedSource(source);
        var diagnostics = GeneratorTestHelper.GetDiagnosticsAll(source);

        Assert.Contains("partial class Outer", generated, StringComparison.Ordinal);
        Assert.Contains("partial class Inner", generated, StringComparison.Ordinal);
        Assert.Contains("partial string Name", generated, StringComparison.Ordinal);
        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void SameNameNestedTypesInDifferentOuterTypesDoNotCollide()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class OuterA
            {
                public partial class Inner : ObservableObject
                {
                    [ObservableProperty]
                    public partial string Value { get; set; }
                }
            }

            public partial class OuterB
            {
                public partial class Inner : ObservableObject
                {
                    [ObservableProperty]
                    public partial string Value { get; set; }
                }
            }
            """;

        var diagnostics = GeneratorTestHelper.GetDiagnosticsAll(source);

        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    //-----------------------------------------------------------------------
    // ObservableGeneratorOption
    //-----------------------------------------------------------------------

    [Fact]
    public void ReactiveOptionGeneratesObserveMethod()
    {
        const string source =
            """
            using Smart.Mvvm;

            [ObservableGeneratorOption(Reactive = true)]
            public sealed partial class ReactiveVm : ObservableObject
            {
                [ObservableProperty]
                public partial string Title { get; set; }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.Contains("ObserveTitle()", generated, StringComparison.Ordinal);
        Assert.Contains("Observable.FromEvent", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void ReactiveOptionAddsSystemReactiveLinqUsing()
    {
        const string source =
            """
            using Smart.Mvvm;

            [ObservableGeneratorOption(Reactive = true)]
            public sealed partial class ReactiveVm : ObservableObject
            {
                [ObservableProperty]
                public partial string Title { get; set; }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.Contains("using System.Reactive.Linq;", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void ReactiveOptionSealedClassObserveMethodIsPrivate()
    {
        const string source =
            """
            using Smart.Mvvm;

            [ObservableGeneratorOption(Reactive = true)]
            public sealed partial class ReactiveVm : ObservableObject
            {
                [ObservableProperty]
                public partial string Title { get; set; }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.Contains("private global::System.IObservable<string> ObserveTitle()", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void ReactiveOptionNonSealedClassObserveMethodIsProtected()
    {
        const string source =
            """
            using Smart.Mvvm;

            [ObservableGeneratorOption(Reactive = true)]
            public partial class OpenVm : ObservableObject
            {
                [ObservableProperty]
                public partial string Title { get; set; }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.Contains("protected global::System.IObservable<string> ObserveTitle()", generated, StringComparison.Ordinal);
    }

    //-----------------------------------------------------------------------
    // ObservableGeneratorOption
    //-----------------------------------------------------------------------

    [Fact]
    public void ViewModelOptionReactiveAndViewModelGeneratesSubscribeMethod()
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

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.Contains("SubscribeTitle(", generated, StringComparison.Ordinal);
        Assert.Contains("Disposables.Add(", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void ReactiveOnlyOptionWithoutViewModelDoesNotGenerateSubscribeMethod()
    {
        const string source =
            """
            using Smart.Mvvm;

            [ObservableGeneratorOption(Reactive = true, ViewModel = false)]
            public sealed partial class ReactiveVm : ObservableObject
            {
                [ObservableProperty]
                public partial string Title { get; set; }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.DoesNotContain("SubscribeTitle(", generated, StringComparison.Ordinal);
    }

    //-----------------------------------------------------------------------
    // No option
    //-----------------------------------------------------------------------

    [Fact]
    public void NoOptionDoesNotGenerateObserveOrSubscribeMethods()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class PlainVm : ObservableObject
            {
                [ObservableProperty]
                public partial string Name { get; set; }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.DoesNotContain("ObserveName()", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("SubscribeName(", generated, StringComparison.Ordinal);
    }

    //-----------------------------------------------------------------------
    // No errors
    //-----------------------------------------------------------------------

    [Fact]
    public void BasicPropertyOutputCompilationHasNoErrors()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class MyViewModel : ObservableObject
            {
                [ObservableProperty]
                public partial string Name { get; set; }
            }
            """;

        var diagnostics = GeneratorTestHelper.GetDiagnosticsAll(source);

        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void MultiplePropertiesOutputCompilationHasNoErrors()
    {
        const string source =
            """
            using Smart.Mvvm;

            public sealed partial class MultiVm : ObservableObject
            {
                [ObservableProperty(NotifyAlso = [nameof(FullName)])]
                public partial string FirstName { get; set; }

                [ObservableProperty(NotifyAlso = [nameof(FullName)])]
                public partial string LastName { get; set; }

                [ObservableProperty]
                public partial int Age { get; set; }

                [ObservableProperty]
                public partial int? Score { get; set; }

                public string FullName => $"{FirstName} {LastName}";
            }
            """;

        var diagnostics = GeneratorTestHelper.GetDiagnosticsAll(source);

        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    //-----------------------------------------------------------------------
    // Diagnostics
    //-----------------------------------------------------------------------

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
    public void Smv0004AllPartialContainingTypesDoesNotEmitDiagnostic()
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
    public void Smv0005ViewModelOptionWithReactiveDoesNotEmitDiagnostic()
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

    //-----------------------------------------------------------------------
    // Accessibility
    //-----------------------------------------------------------------------

    [Fact]
    public void PublicPropertyGeneratedPartialPropertyIsPublic()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class MyViewModel : ObservableObject
            {
                [ObservableProperty]
                public partial string Name { get; set; }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.Contains("public partial string Name", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void PrivateSetterGeneratedSetterHasPrivateAccessibility()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class MyViewModel : ObservableObject
            {
                [ObservableProperty]
                public partial string Name { get; private set; }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.Contains("private set", generated, StringComparison.Ordinal);
    }

    //-----------------------------------------------------------------------
    // EditorBrowsable
    //-----------------------------------------------------------------------

    [Fact]
    public void GeneratedEventArgsFieldHasEditorBrowsableNever()
    {
        const string source =
            """
            using Smart.Mvvm;

            public partial class MyViewModel : ObservableObject
            {
                [ObservableProperty]
                public partial string Name { get; set; }
            }
            """;

        var generated = GeneratorTestHelper.GetGeneratedSource(source);

        Assert.Contains("EditorBrowsableState.Never", generated, StringComparison.Ordinal);
    }
}
