namespace Smart.Mvvm;

using System;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ObservablePropertyAttribute : Attribute
{
    public string[] NotifyAlso { get; set; } = [];
}
