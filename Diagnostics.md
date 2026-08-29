# Diagnostics

| ID | Severity | Description | How to fix |
|---|---|---|---|
| SMV0001 | ❌ Error | `[ObservableProperty]` property is not declared partial | Declare the property as `partial` |
| SMV0002 | ❌ Error | `[ObservableProperty]` property has no setter | Add a setter to the property |
| SMV0003 | ❌ Error | Type declaring the `[ObservableProperty]` does not extend `ObservableObject` | Derive the type from `ObservableObject` |
| SMV0004 | ❌ Error | Type containing the `[ObservableProperty]` is not declared partial | Declare the containing type as `partial` |
| SMV0005 | ⚠️ Warning | `ViewModel` option has no effect unless the `Reactive` option is also enabled | Enable the `Reactive` option, or remove the `ViewModel` option |
