namespace Smart.Mvvm.ViewModels;

using System.ComponentModel;

using Smart.Mvvm.Internal;

public sealed class ErrorInfo : ObservableObject, IDisposable
{
    private const int DefaultCapacity = 16;

    private static readonly PropertyChangedEventArgs ItemsChangedEventArgs = new("Item[]");
    private static readonly PropertyChangedEventArgs HasErrorChangedEventArgs = new(nameof(HasError));

    private Dictionary<string, PooledList<string>>? errors;

    private bool hasError;

    // ReSharper disable once ConvertToAutoProperty
    public bool HasError => hasError;

    public string? this[string key] =>
        (errors is not null) && errors.TryGetValue(key, out var values) && (values.Count > 0) ? values[0] : null;

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

    private PooledList<string> PrepareList(string key, bool clear)
    {
        errors ??= new Dictionary<string, PooledList<string>>();

        if (!errors.TryGetValue(key, out var values))
        {
            values = new PooledList<string>(DefaultCapacity);
            errors.Add(key, values);
        }
        else if (clear)
        {
            values.Clear();
        }

        return values;
    }

    public void AddError(string key, string message)
    {
        var values = PrepareList(key, false);

        values.Add(message);

        RaisePropertyChanged(ItemsChangedEventArgs);

        var previousError = hasError;
        hasError = true;
        if (previousError != hasError)
        {
            RaisePropertyChanged(HasErrorChangedEventArgs);
        }
    }

    public void AddErrors(string key, IEnumerable<string> messages)
    {
        var values = default(PooledList<string>);
        var added = false;
        foreach (var message in messages)
        {
            if (values is null)
            {
                values = PrepareList(key, false);
                added = true;
            }

            values.Add(message);
        }

        RaisePropertyChanged(ItemsChangedEventArgs);

        if (added)
        {
            var previousError = hasError;
            hasError = true;
            if (previousError != hasError)
            {
                RaisePropertyChanged(HasErrorChangedEventArgs);
            }
        }
    }

    public void UpdateError(string key, string message)
    {
        var values = PrepareList(key, true);

        values.Add(message);

        RaisePropertyChanged(ItemsChangedEventArgs);

        var previousError = hasError;
        hasError = true;
        if (previousError != hasError)
        {
            RaisePropertyChanged(HasErrorChangedEventArgs);
        }
    }

    public void UpdateErrors(string key, IEnumerable<string> messages)
    {
        var values = default(PooledList<string>);
        var errorExist = false;
        foreach (var message in messages)
        {
            if (values is null)
            {
                values = PrepareList(key, true);
                errorExist = true;
            }

            values.Add(message);
        }

        if (!errorExist && (errors is not null))
        {
            foreach (var kvp in errors)
            {
                if (kvp.Value.Count > 0)
                {
                    errorExist = true;
                    break;
                }
            }
        }

        var previousError = hasError;
        hasError = errorExist;
        if (previousError != hasError)
        {
            RaisePropertyChanged(HasErrorChangedEventArgs);
        }
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
    }
}
