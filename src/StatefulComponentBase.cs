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
    /// Overrides OnParametersSetAsync to synchronize parameter states before rendering.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await _container.OnParametersSetAsync();
    }
}
