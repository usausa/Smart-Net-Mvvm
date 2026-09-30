# Diagnostics

| ID | Severity | Description | How to fix |
|---|---|---|---|
| SMV0001 | ❌ Error | `[ObservableProperty]` property is not an instance `partial` property without an implementation (indexers are not supported) | Declare the property as an instance `partial` property, and leave the implementation to the generator |
| SMV0002 | ❌ Error | `[ObservableProperty]` property has no setter | Add a setter to the property |
| SMV0003 | ❌ Error | Type declaring the `[ObservableProperty]` does not extend `ObservableObject` | Derive the type from `ObservableObject` |
| SMV0004 | ❌ Error | Type containing the `[ObservableProperty]` is not declared partial, or is file-local | Declare the containing type as `partial`, and do not declare it `file` |
| SMV0005 | ⚠️ Warning | `ViewModel` option has no effect unless the `Reactive` option is also enabled and the type derives from `ViewModelBase`; reported once at the option attribute | Enable the `Reactive` option and derive from `ViewModelBase`, or remove the `ViewModel` option |
| SMV0006 | ❌ Error | Type name differs only in case from another type, so the generated file names collide; only the first type (in ordinal order) is generated | Rename one of the types |
