namespace Smart.Mvvm.Generator;

using Microsoft.CodeAnalysis;

using SourceGenerateHelper;

internal static class Diagnostics
{
    public static DiagnosticDescriptor InvalidPropertyDefinition { get; } = new(
        id: "SMV0001",
        title: "Invalid property definition",
        messageFormat: "[ObservableProperty] property must be an instance partial property (not an indexer) without an implementation. property=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor PropertySetterRequired { get; } = new(
        id: "SMV0002",
        title: "Property setter is required",
        messageFormat: "[ObservableProperty] property has no setter. property=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor InvalidTypeDefinition { get; } = new(
        id: "SMV0003",
        title: "Invalid type definition",
        messageFormat: "[ObservableProperty] type must extend ObservableObject. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor PartialContainingTypeRequired { get; } = new(
        id: "SMV0004",
        title: "Partial containing type is required",
        messageFormat: "[ObservableProperty] containing type must be partial and not file-local. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor ViewModelOptionRequiresReactive { get; } = new(
        id: "SMV0005",
        title: "ViewModel option has no effect",
        messageFormat: "ViewModel option has no effect without the Reactive option, or on a type that does not derive from ViewModelBase. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor HintNameCollision { get; } = new(
        id: "SMV0006",
        title: "Type name differs only in case",
        messageFormat: "Type name differs only in case from another type, and its source is not generated. type=[{0}], other=[{1}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);
}
