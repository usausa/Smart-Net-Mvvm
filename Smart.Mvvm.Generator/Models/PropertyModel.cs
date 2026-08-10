namespace Smart.Mvvm.Generator.Models;

using Microsoft.CodeAnalysis;

using SourceGenerateHelper;

internal sealed record PropertyModel(
    // Containing type
    string Namespace,
    string TypeKey,
    EquatableArray<string> ContainingTypes,
    bool IsSealed,
    // Options
    bool IsReactive,
    bool IsViewModel,
    // Property signature
    Accessibility PropertyAccessibility,
    string PropertyType,
    string PropertyName,
    bool HasGetter,
    Accessibility? GetterAccessibility,
    Accessibility? SetterAccessibility,
    // Notification targets
    EquatableArray<string> NotifyAlso);
