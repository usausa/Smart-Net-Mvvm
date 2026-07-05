namespace Smart.Mvvm.ViewModels;

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Smart.Mvvm.Internal;

#pragma warning disable IDE0032
[DebuggerDisplay("Reference = {" + nameof(PropertyChangedReferenceCount) + "}, HasError = {" + nameof(HasError) + "}")]
public sealed class ErrorInfo : ObservableObject, IDisposable
{
    private const int DefaultCapacityPerKey = 16;

    private static readonly PropertyChangedEventArgs ItemsChangedEventArgs = new("Item[]");
    private static readonly PropertyChangedEventArgs HasErrorChangedEventArgs = new(nameof(HasError));

    private Dictionary<string, PooledList<string>>? errors;

    private bool hasError;

    // ReSharper disable once ConvertToAutoProperty
    public bool HasError => hasError;

    public string? this[string key] =>
        (errors is not null) && errors.TryGetValue(key, out var values) && (values.Count > 0) ? values[0] : null;

    internal Action? Handler { get; set; }

    public void Dispose()
    {
        if (errors is not null)
        {
            foreach (var kvp in errors)
            {
                kvp.Value.Dispose();
            }

            errors.Clear();
        }
    }

    public bool Contains(string key) =>
        (errors is not null) && errors.TryGetValue(key, out var values) && (values.Count > 0);

    public IEnumerable<string> GetKeys() =>
        hasError ? GetKeysInternal() : [];

    private IEnumerable<string> GetKeysInternal()
    {
        if (errors is not null)
        {
            foreach (var kvp in errors)
            {
                if (kvp.Value.Count == 0)
                {
                    continue;
                }

                yield return kvp.Key;
            }
        }
    }

    public IReadOnlyList<string> GetErrors(string key) =>
        (errors is not null) && errors.TryGetValue(key, out var values) && (values.Count > 0) ? values : [];

    public IEnumerable<string> GetAllErrors() =>
        hasError ? GetAllErrorsInternal() : [];

    private IEnumerable<string> GetAllErrorsInternal()
    {
        if (errors is not null)
        {
            foreach (var kvp in errors)
            {
                if (kvp.Value.Count == 0)
                {
                    continue;
                }

                foreach (var value in kvp.Value)
                {
                    yield return value;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void NotifyHandler()
    {
        Handler?.Invoke();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateHasError()
    {
        var previousError = hasError;
        hasError = (errors is not null) && (errors.Count > 0);
        if (previousError != hasError)
        {
            RaisePropertyChanged(HasErrorChangedEventArgs);
        }
    }

    private PooledList<string> PrepareList(string key)
    {
        errors ??= [];

        if (!errors.TryGetValue(key, out var values))
        {
#pragma warning disable IDE0028
            values = new PooledList<string>(DefaultCapacityPerKey);
#pragma warning restore IDE0028
            errors.Add(key, values);
        }

        return values;
    }

    private void RemoveList(string key)
    {
        if ((errors is not null) && errors.Remove(key, out var values))
        {
            values.Dispose();
        }
    }

    public void AddError(string key, string message)
    {
        var values = PrepareList(key);

        values.Add(message);

        RaisePropertyChanged(ItemsChangedEventArgs);

        UpdateHasError();

        NotifyHandler();
    }

    public void AddErrors(string key, IEnumerable<string> messages)
    {
        var values = default(PooledList<string>);
        foreach (var message in messages)
        {
            values ??= PrepareList(key);

            values.Add(message);
        }

        if (values is not null)
        {
            RaisePropertyChanged(ItemsChangedEventArgs);

            UpdateHasError();

            NotifyHandler();
        }
    }

    public void UpdateError(string key, string message)
    {
        var values = PrepareList(key);

        values.Clear();
        values.Add(message);

        RaisePropertyChanged(ItemsChangedEventArgs);

        UpdateHasError();

        NotifyHandler();
    }

    public void UpdateErrors(string key, IEnumerable<string> messages)
    {
        var values = default(PooledList<string>);
        foreach (var message in messages)
        {
            if (values is null)
            {
                values = PrepareList(key);
                values.Clear();
            }

            values.Add(message);
        }

        if (values is null)
        {
            RemoveList(key);
        }

        RaisePropertyChanged(ItemsChangedEventArgs);

        UpdateHasError();

        NotifyHandler();
    }

    public void ClearErrors(string key)
    {
        if ((errors is null) || !errors.Remove(key, out var values))
        {
            return;
        }

        values.Dispose();

        RaisePropertyChanged(ItemsChangedEventArgs);

        UpdateHasError();

        NotifyHandler();
    }

    public void ClearAllErrors()
    {
        if ((errors is null) || (errors.Count == 0))
        {
            return;
        }

        foreach (var kvp in errors)
        {
            kvp.Value.Dispose();
        }

        errors.Clear();

        RaisePropertyChanged(ItemsChangedEventArgs);

        UpdateHasError();

        NotifyHandler();
    }
}
#pragma warning restore IDE0032
