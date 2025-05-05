namespace BunnyTail.ObservableProperty.Generator.Models;

using Microsoft.CodeAnalysis;

using SourceGenerateHelper;

internal sealed record PropertyModel(
    string Namespace,
    string ClassName,
    bool IsValueType,
    bool HasPropertyChangedEvent,
    Accessibility PropertyAccessibility,
    string PropertyType,
    string PropertyName,
    EquatableArray<string> NotifyAlso);
