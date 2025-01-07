namespace ObservablePropertyGenerator.Attributes;

using System;

[AttributeUsage(AttributeTargets.Property)]
public sealed class NotifyPropertyChangedForAttribute : Attribute
{
    public string[] PropertyNames { get; }

#pragma warning disable CA1019
    public NotifyPropertyChangedForAttribute(string propertyName, params string[] otherPropertyNames)
    {
        PropertyNames = new[] { propertyName }.Concat(otherPropertyNames).ToArray();
    }
#pragma warning restore CA1019
}
