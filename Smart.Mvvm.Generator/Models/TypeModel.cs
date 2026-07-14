namespace Smart.Mvvm.Generator.Models;

using SourceGenerateHelper;

internal sealed record TypeModel(
    string Namespace,
    string TypeKey,
    EquatableArray<PropertyModel> Properties);
