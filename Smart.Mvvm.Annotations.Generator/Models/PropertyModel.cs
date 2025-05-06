namespace Smart.Mvvm.Annotations.Generator.Models;

using Microsoft.CodeAnalysis;

using SourceGenerateHelper;

internal sealed record PropertyModel(
    string Namespace,
    string ClassName,
    bool IsValueType,
    Accessibility PropertyAccessibility,
    string PropertyType,
    string PropertyName,
    bool HasGetter,
    Accessibility? GetterAccessibility,
    Accessibility? SetterAccessibility,
    EquatableArray<string> NotifyAlso);
