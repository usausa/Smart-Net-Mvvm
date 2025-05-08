namespace Smart.Mvvm;

using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class ObservableGeneratorOptionAttribute : Attribute
{
    public bool Reactive { get; set; }

    public bool ViewModel { get; set; }
}
