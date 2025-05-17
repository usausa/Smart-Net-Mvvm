namespace Smart.Mvvm.ViewModels;

using System.ComponentModel;
using System.Diagnostics;

using Smart.Mvvm.Internal;

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

    private PooledList<string> PrepareList(string key)
    {
        errors ??= new Dictionary<string, PooledList<string>>();

        if (!errors.TryGetValue(key, out var values))
        {
            values = new PooledList<string>(DefaultCapacityPerKey);
            errors.Add(key, values);
        }

        return values;
    }

    public void AddError(string key, string message)
    {
        var values = PrepareList(key);

        values.Add(message);

        RaisePropertyChanged(ItemsChangedEventArgs);

        var previousError = hasError;
        hasError = true;
        if (previousError != hasError)
        {
            RaisePropertyChanged(HasErrorChangedEventArgs);
        }

        Handler?.Invoke();
    }

    public void AddErrors(string key, IEnumerable<string> messages)
    {
        var values = default(PooledList<string>);
        var added = false;
        foreach (var message in messages)
        {
            if (values is null)
            {
                values = PrepareList(key);
                added = true;
            }

            values.Add(message);
        }

        if (added)
        {
            RaisePropertyChanged(ItemsChangedEventArgs);

            var previousError = hasError;
            hasError = true;
            if (previousError != hasError)
            {
                RaisePropertyChanged(HasErrorChangedEventArgs);
            }

            Handler?.Invoke();
        }
    }

    public void UpdateError(string key, string message)
    {
        var values = PrepareList(key);

        values.Clear();
        values.Add(message);

        RaisePropertyChanged(ItemsChangedEventArgs);

        var previousError = hasError;
        hasError = true;
        if (previousError != hasError)
        {
            RaisePropertyChanged(HasErrorChangedEventArgs);
        }

        Handler?.Invoke();
    }

    public void UpdateErrors(string key, IEnumerable<string> messages)
    {
        var values = default(PooledList<string>);
        var errorExist = false;
        foreach (var message in messages)
        {
            if (values is null)
            {
                values = PrepareList(key);
                values.Clear();
                errorExist = true;
            }

            values.Add(message);
        }

        if (!errorExist && (errors is not null))
        {
            if (errors.TryGetValue(key, out values))
            {
                values.Clear();
            }

            foreach (var kvp in errors)
            {
                if (kvp.Value.Count > 0)
                {
                    errorExist = true;
                    break;
                }
            }
        }

        RaisePropertyChanged(ItemsChangedEventArgs);

        var previousError = hasError;
        hasError = errorExist;
        if (previousError != hasError)
        {
            RaisePropertyChanged(HasErrorChangedEventArgs);
        }

        Handler?.Invoke();
    }

    public void ClearErrors(string key)
    {
        if ((errors is null) || !errors.TryGetValue(key, out var values))
        {
            return;
        }

        values.Clear();

        var errorExist = false;
        foreach (var kvp in errors)
        {
            if (kvp.Value.Count > 0)
            {
                errorExist = true;
                break;
            }
        }

        RaisePropertyChanged(ItemsChangedEventArgs);

        var previousError = hasError;
        hasError = errorExist;
        if (previousError != hasError)
        {
            RaisePropertyChanged(HasErrorChangedEventArgs);
        }

        Handler?.Invoke();
    }

    public void ClearAllErrors()
    {
        if (errors is null)
        {
            return;
        }

        foreach (var kvp in errors)
        {
            kvp.Value.Clear();
        }

        RaisePropertyChanged(ItemsChangedEventArgs);

        var previousError = hasError;
        hasError = false;
        if (previousError != hasError)
        {
            RaisePropertyChanged(HasErrorChangedEventArgs);
        }

        Handler?.Invoke();
    }
}
