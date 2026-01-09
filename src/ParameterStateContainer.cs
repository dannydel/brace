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
    /// Called during OnInitialized/OnInitializedAsync to perform initial synchronization.
    /// </summary>
    internal async Task OnInitializedAsync()
    {
        await UpdateAllStatesAsync();
    }

    /// <summary>
    /// Called during SetParametersAsync to synchronize parameter changes.
    /// </summary>
    internal async Task OnSetParametersAsync()
    {
        await UpdateAllStatesAsync();
    }

    /// <summary>
    /// Called during OnParametersSet/OnParametersSetAsync to handle post-parameter updates.
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
            // // Use reflection to call UpdateAsync on the generic ParameterState<T>
            // var updateMethod = state.GetType().GetMethod("UpdateAsync",
            //     System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            //
            // if (updateMethod != null)
            // {
            //     var task = updateMethod.Invoke(state, null) as Task;
            //     if (task != null)
            //     {
            //         await task;
            //     }
            // }

        }
    }
}
