namespace Smart.Mvvm.Generator;

using Microsoft.CodeAnalysis;

internal static class Diagnostics
{
    public static DiagnosticDescriptor InvalidPropertyDefinition { get; } = new(
        id: "SMV0001",
        title: "Invalid property definition",
        messageFormat: "Property must be partial. property=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor PropertySetterRequired { get; } = new(
        id: "SMV0002",
        title: "Property setter is required",
        messageFormat: "Property setter is required. property=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidTypeDefinition { get; } = new(
        id: "SMV0003",
        title: "Invalid type definition",
        messageFormat: "Type must extend ObservableObject. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
