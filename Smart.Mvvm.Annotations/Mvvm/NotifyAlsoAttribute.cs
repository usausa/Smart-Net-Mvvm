namespace Smart.Mvvm;

using System;

[AttributeUsage(AttributeTargets.Property)]
public sealed class NotifyAlsoAttribute : Attribute
{
    public string[] PropertyNames { get; }

#pragma warning disable CA1019
    public NotifyAlsoAttribute(string propertyName, params string[] otherPropertyNames)
    {
        PropertyNames = new[] { propertyName }.Concat(otherPropertyNames).ToArray();
    }
#pragma warning restore CA1019
}
