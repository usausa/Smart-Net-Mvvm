namespace Smart.Mvvm.Generator.Models;

using Microsoft.CodeAnalysis;

using SourceGenerateHelper;

internal sealed record PropertyModel(
    string Namespace,
    string TypeKey,
    EquatableArray<string> ContainingTypes,
    bool IsSealed,
    bool IsReactive,
    bool IsViewModel,
    Accessibility PropertyAccessibility,
    string PropertyType,
    string PropertyName,
    bool HasGetter,
    Accessibility? GetterAccessibility,
    Accessibility? SetterAccessibility,
    EquatableArray<string> NotifyAlso);
