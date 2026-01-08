using Microsoft.AspNetCore.Components;

namespace Brace;

/// <summary>
/// Fluent builder for configuring a ParameterState instance.
/// </summary>
/// <typeparam name="T">The type of the parameter value.</typeparam>
public class ParameterStateBuilder<T>
{
    private readonly ParameterState<T> _parameterState;

    internal ParameterStateBuilder(ParameterState<T> parameterState)
    {
        _parameterState = parameterState;
    }

    /// <summary>
    /// Specifies the getter function to retrieve the current parameter value.
    /// </summary>
    /// <param name="parameterGetter">A function that returns the current parameter value.</param>
    /// <returns>The builder for method chaining.</returns>
    public ParameterStateBuilder<T> WithParameter(Func<T?> parameterGetter)
    {
        _parameterState.SetParameterGetter(parameterGetter);
        return this;
    }

    /// <summary>
    /// Specifies the EventCallback getter for two-way binding support.
    /// The EventCallback will be automatically invoked when the parameter value changes.
    /// </summary>
    /// <param name="eventCallbackGetter">A function that returns the EventCallback.</param>
    /// <returns>The builder for method chaining.</returns>
    public ParameterStateBuilder<T> WithEventCallback(Func<EventCallback<T>> eventCallbackGetter)
    {
        _parameterState.SetEventCallbackGetter(eventCallbackGetter);
        return this;
    }

    /// <summary>
    /// Specifies an asynchronous change handler to execute when the parameter value changes.
    /// </summary>
    /// <param name="changeHandler">An async function that receives the old and new values.</param>
    /// <returns>The builder for method chaining.</returns>
    public ParameterStateBuilder<T> WithChangeHandler(Func<T?, T?, Task> changeHandler)
    {
        _parameterState.SetAsyncChangeHandler(changeHandler);
        return this;
    }

    /// <summary>
    /// Specifies a synchronous change handler to execute when the parameter value changes.
    /// </summary>
    /// <param name="changeHandler">An action that receives the old and new values.</param>
    /// <returns>The builder for method chaining.</returns>
    public ParameterStateBuilder<T> WithChangeHandler(Action<T?, T?> changeHandler)
    {
        _parameterState.SetSyncChangeHandler(changeHandler);
        return this;
    }

    /// <summary>
    /// Specifies a custom equality comparer for change detection.
    /// </summary>
    /// <param name="comparer">The equality comparer to use.</param>
    /// <returns>The builder for method chaining.</returns>
    public ParameterStateBuilder<T> WithComparer(IEqualityComparer<T> comparer)
    {
        _parameterState.SetComparer(comparer);
        return this;
    }

    /// <summary>
    /// Returns the configured ParameterState instance.
    /// This is called implicitly when the builder is assigned to a variable.
    /// </summary>
    public static implicit operator ParameterState<T>(ParameterStateBuilder<T> builder)
    {
        return builder._parameterState;
    }
}
