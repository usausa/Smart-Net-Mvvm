namespace Smart.Mvvm.Generator;

using Microsoft.CodeAnalysis;

internal static class Diagnostics
{
    public static DiagnosticDescriptor InvalidPropertyDefinition { get; } = new(
        id: "SMV0001",
        title: "Invalid property definition",
        messageFormat: "[ObservableProperty] property must be partial. property=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor PropertySetterRequired { get; } = new(
        id: "SMV0002",
        title: "Property setter is required",
        messageFormat: "[ObservableProperty] property has no setter. property=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidTypeDefinition { get; } = new(
        id: "SMV0003",
        title: "Invalid type definition",
        messageFormat: "[ObservableProperty] type must extend ObservableObject. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor PartialContainingTypeRequired { get; } = new(
        id: "SMV0004",
        title: "Partial containing type is required",
        messageFormat: "[ObservableProperty] containing type must be partial. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor ViewModelOptionRequiresReactive { get; } = new(
        id: "SMV0005",
        title: "ViewModel option requires Reactive",
        messageFormat: "ViewModel option has no effect. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
