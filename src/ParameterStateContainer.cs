namespace Brace;

/// <summary>
/// Internal container that manages all registered parameter states for a component.
/// Orchestrates parameter synchronization during Blazor lifecycle methods.
/// </summary>
internal class ParameterStateContainer
{
    private readonly List<IParameterState> _parameterStates = [];

    /// <summary>
    /// Registers a parameter state with the container.
    /// </summary>
    internal void Register(IParameterState parameterState)
    {
        _parameterStates.Add(parameterState);
    }

    /// <summary>
    /// Called during OnParametersSetAsync to synchronize parameter state before rendering.
    /// </summary>
    internal async Task OnParametersSetAsync()
    {
        await UpdateAllStatesAsync();
    }

    /// <summary>
    /// Updates all registered parameter states.
    /// </summary>
    private async Task UpdateAllStatesAsync()
    {
        foreach (var state in _parameterStates)
        {
            await state.UpdateAsync();
        }
    }
}
