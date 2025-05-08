namespace Smart.Mvvm.Generator.Models;

internal sealed record TypeOptionModel(
    string Namespace,
    string ClassName,
    bool Sealed,
    bool Reactive,
    bool ViewModel);
