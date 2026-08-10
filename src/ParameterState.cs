using Microsoft.AspNetCore.Components;

namespace Brace;

/// <summary>
/// Represents the state of a component parameter with change tracking and event callback support.
/// </summary>
/// <typeparam name="T">The type of the parameter value.</typeparam>
public class ParameterState<T> : IParameterState
{
    private T? _value;
    private bool _isInitialized;
    private Func<T?>? _parameterGetter;
    private Func<EventCallback<T>>? _eventCallbackGetter;
    private Func<T?, T?, Task>? _asyncChangeHandler;
    private Action<T?, T?>? _syncChangeHandler;
    private IEqualityComparer<T> _comparer;

    internal ParameterState(string parameterName, T? initialValue, IEqualityComparer<T>? comparer = null)
    {
        ParameterName = parameterName;
        _value = initialValue;
        _comparer = comparer ?? EqualityComparer<T>.Default;
        _isInitialized = false;
    }

    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string ParameterName { get; }

    /// <summary>
    /// Gets the current value of the parameter state.
    /// </summary>
    public T? Value => _value;

    internal void SetParameterGetter(Func<T?> getter)
    {
        _parameterGetter = getter;
    }

    internal void SetEventCallbackGetter(Func<EventCallback<T>> getter)
    {
        _eventCallbackGetter = getter;
    }

    internal void SetAsyncChangeHandler(Func<T?, T?, Task> handler)
    {
        _asyncChangeHandler = handler;
    }

    internal void SetSyncChangeHandler(Action<T?, T?> handler)
    {
        _syncChangeHandler = handler;
    }

    internal void SetComparer(IEqualityComparer<T> comparer)
    {
        _comparer = comparer;
    }

    /// <summary>
    /// Updates the state value from the registered component parameter if it has changed.
    /// Parent-driven changes invoke change handlers but do not invoke the two-way binding callback.
    /// </summary>
    public async Task UpdateAsync()
    {
        if (_parameterGetter == null)
        {
            return;
        }

        var newValue = _parameterGetter();

        // On first initialization, just set the value without triggering handlers
        if (!_isInitialized)
        {
            _value = newValue;
            _isInitialized = true;
            return;
        }

        // Check if value has changed
        if (_comparer.Equals(_value, newValue))
        {
            return;
        }

        var oldValue = _value;
        _value = newValue;

        await InvokeChangeHandlersAsync(oldValue, newValue);
    }

    /// <summary>
    /// Sets the state value for a change originating inside the component.
    /// Change handlers and the configured two-way binding callback are invoked when the value changes.
    /// </summary>
    /// <param name="newValue">The new state value.</param>
    public async Task SetValueAsync(T? newValue)
    {
        if (_comparer.Equals(_value, newValue))
        {
            return;
        }

        var oldValue = _value;
        _value = newValue;
        _isInitialized = true;

        await InvokeChangeHandlersAsync(oldValue, newValue);

        if (_eventCallbackGetter != null)
        {
            var eventCallback = _eventCallbackGetter();
            if (eventCallback.HasDelegate)
            {
                await eventCallback.InvokeAsync(newValue);
            }
        }
    }

    private async Task InvokeChangeHandlersAsync(T? oldValue, T? newValue)
    {
        _syncChangeHandler?.Invoke(oldValue, newValue);

        if (_asyncChangeHandler != null)
        {
            await _asyncChangeHandler(oldValue, newValue);
        }
    }
}
