namespace Smart.Mvvm.Generator.Models;

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
    bool IsViewModelBase,
    string OptionOwner,
    LocationInfo? OptionLocation,
    string TypeName,
    LocationInfo? TypeLocation,
    // Property signature
    string Signature,
    string PropertyType,
    string PropertyName,
    string? Getter,
    string Setter,
    // Notification targets
    EquatableArray<string> NotifyAlso);
