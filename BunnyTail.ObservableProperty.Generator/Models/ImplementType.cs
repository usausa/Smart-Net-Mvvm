namespace BunnyTail.ObservableProperty.Generator.Models;

public enum ImplementType
{
    None,
    Event,
    EventTrigger
}

public static class ImplementTypeExtensions
{
    public static bool HasEvent(this ImplementType value) => value is ImplementType.Event or ImplementType.EventTrigger;

    public static bool HasTrigger(this ImplementType value) => value is ImplementType.EventTrigger;
}
