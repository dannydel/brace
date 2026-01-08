using Microsoft.AspNetCore.Components;

namespace Brace;

/// <summary>
/// Base class for Blazor components that provides automatic parameter state management.
/// Inherit from this class to use the parameter state registration system.
/// </summary>
public abstract class StatefulComponentBase : ComponentBase
{
    private readonly ParameterStateContainer _container = new();

    /// <summary>
    /// Creates a parameter state registration scope for use in the component constructor.
    /// The scope should be disposed after registration is complete (use 'using' statement).
    /// </summary>
    /// <returns>A disposable scope for registering parameter states.</returns>
    protected ParameterStateScope CreateComponentParameterStateScope()
    {
        return new ParameterStateScope(_container);
    }

    /// <summary>
    /// Overrides SetParametersAsync to automatically synchronize registered parameter states.
    /// </summary>
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);
        await _container.OnSetParametersAsync();
    }

    /// <summary>
    /// Overrides OnInitializedAsync to perform initial parameter state synchronization.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await _container.OnInitializedAsync();
    }

    /// <summary>
    /// Overrides OnParametersSetAsync to synchronize parameter states after parameters are set.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await _container.OnParametersSetAsync();
        
        // Required to ensure values render when needed.
        await InvokeAsync(StateHasChanged);
    }
}
