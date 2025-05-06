namespace BunnyTail.ObservableProperty.Generator;

using Microsoft.CodeAnalysis;

internal static class Diagnostics
{
    public static DiagnosticDescriptor InvalidPropertyDefinition => new(
        id: "BTOP0001",
        title: "Invalid property definition",
        messageFormat: "Property must be partial. property=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor PropertySetterRequired => new(
        id: "BTOP0002",
        title: "Property setter is required",
        messageFormat: "Property setter is required. property=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor TypeMustImplementNotifyPropertyChanged => new(
        id: "BTOP0003",
        title: "Type must implement INotifyPropertyChanged",
        messageFormat: "Type must implement INotifyPropertyChanged. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor TypeMustImplementObservableObject => new(
        id: "BTOP0004",
        title: "Type must implement IObservableObject",
        messageFormat: "Type must implement IObservableObject. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
