namespace Brace;

/// <summary>
/// Stores ParameterState to avoid using reflection to update state.
/// </summary>
internal interface IParameterState
{
    Task UpdateAsync();
}