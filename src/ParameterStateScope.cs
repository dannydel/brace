namespace Brace;

/// <summary>
/// A disposable scope for registering component parameter states.
/// This should be used within the component constructor.
/// </summary>
public class ParameterStateScope : IDisposable
{
    private readonly ParameterStateContainer _container;
    private bool _disposed;

    internal ParameterStateScope(ParameterStateContainer container)
    {
        _container = container;
    }

    /// <summary>
    /// Registers a new parameter state with the specified name and returns a builder for configuration.
    /// </summary>
    /// <typeparam name="T">The type of the parameter value.</typeparam>
    /// <param name="parameterName">The name of the parameter (use nameof(ParameterProperty)).</param>
    /// <returns>A builder for configuring the parameter state.</returns>
    public ParameterStateBuilder<T> RegisterParameter<T>(string parameterName)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ParameterStateScope));
        }

        // Get the initial value by attempting to read the parameter via reflection
        // Since we're in the constructor, the parameter will have its default value
        var initialValue = default(T);

        var parameterState = new ParameterState<T>(parameterName, initialValue);
        _container.Register(parameterState);

        return new ParameterStateBuilder<T>(parameterState);
    }

    /// <summary>
    /// Disposes the scope. After disposal, no new parameters can be registered.
    /// </summary>
    public void Dispose()
    {
        _disposed = true;
    }
}
