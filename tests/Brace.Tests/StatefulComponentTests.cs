using Bunit;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Brace.Tests;

public class StatefulComponentTests
{
    [Fact]
    public void ParameterState_ShouldCaptureInitialValue()
    {
        // Arrange & Act
        using var ctx = new TestContext();
        var component = ctx.Render<TestComponent>(parameters => parameters
            .Add(p => p.Name, "John"));

        // Assert
        Assert.Equal("John", component.Instance.GetNameStateValue());
    }

    [Fact]
    public void ParameterState_ShouldUpdateWhenParameterChanges()
    {
        // Arrange
        using var ctx = new TestContext();
        var component = ctx.Render<TestComponent>(parameters => parameters
            .Add(p => p.Name, "John"));

        // Act
        component.Render(parameters => parameters
            .Add(p => p.Name, "Jane"));

        // Assert
        Assert.Equal("Jane", component.Instance.GetNameStateValue());
    }

    [Fact]
    public void ParameterState_ShouldNotUpdateWhenParameterValueIsSame()
    {
        // Arrange
        var changeCount = 0;
        using var ctx = new TestContext();
        var component = ctx.Render<TestComponentWithChangeHandler>(parameters => parameters
            .Add(p => p.Name, "John")
            .Add(p => p.OnChange, () => changeCount++));

        var initialCount = changeCount;

        // Act - set to same value
        component.Render(parameters => parameters
            .Add(p => p.Name, "John"));

        // Assert
        Assert.Equal(initialCount, changeCount);
    }

    [Fact]
    public void ParameterState_ShouldInvokeChangeHandlerWhenValueChanges()
    {
        // Arrange
        var changeCount = 0;
        using var ctx = new TestContext();
        var component = ctx.Render<TestComponentWithChangeHandler>(parameters => parameters
            .Add(p => p.Name, "John")
            .Add(p => p.OnChange, () => changeCount++));

        // Act
        component.Render(parameters => parameters
            .Add(p => p.Name, "Jane"));

        // Assert
        Assert.Equal(1, changeCount);
    }

    [Fact]
    public async Task ParameterState_ShouldInvokeEventCallbackWhenValueChanges()
    {
        // Arrange
        var callbackInvoked = false;
        var newValue = string.Empty;

        using var ctx = new TestContext();
        var component = ctx.Render<TestComponent>(parameters => parameters
            .Add(p => p.Name, "John")
            .Add(p => p.NameChanged, EventCallback.Factory.Create<string>(this, (value) =>
            {
                callbackInvoked = true;
                newValue = value;
            })));

        // Act
        component.Render(parameters => parameters
            .Add(p => p.Name, "Jane"));

        // Assert
        Assert.True(callbackInvoked);
        Assert.Equal("Jane", newValue);
    }

    [Fact]
    public void ParameterState_ShouldHandleMultipleParameters()
    {
        // Arrange
        using var ctx = new TestContext();
        var component = ctx.Render<TestComponentMultipleParams>(parameters => parameters
            .Add(p => p.Name, "John")
            .Add(p => p.Age, 30)
            .Add(p => p.IsActive, true));

        // Assert
        Assert.Equal("John", component.Instance.GetNameStateValue());
        Assert.Equal(30, component.Instance.GetAgeStateValue());
        Assert.True(component.Instance.GetIsActiveStateValue());
    }

    [Fact]
    public void ParameterState_ShouldUseCustomComparer()
    {
        // Arrange
        var changeCount = 0;
        using var ctx = new TestContext();
        var component = ctx.Render<TestComponentWithCustomComparer>(parameters => parameters
            .Add(p => p.Name, "John")
            .Add(p => p.OnChange, () => changeCount++));

        // Act - set to same value with different case (custom comparer ignores case)
        component.Render(parameters => parameters
            .Add(p => p.Name, "JOHN"));

        // Assert - should not trigger change because comparer ignores case
        Assert.Equal(0, changeCount);
    }

    [Fact]
    public void ParameterState_ShouldHandleNullValues()
    {
        // Arrange
        using var ctx = new TestContext();
        var component = ctx.Render<TestComponent>(parameters => parameters
            .Add(p => p.Name, (string?)null));

        // Assert
        Assert.Null(component.Instance.GetNameStateValue());

        // Act
        component.Render(parameters => parameters
            .Add(p => p.Name, "Jane"));

        // Assert
        Assert.Equal("Jane", component.Instance.GetNameStateValue());
    }

    [Fact]
    public async Task ParameterState_ShouldInvokeAsyncChangeHandler()
    {
        // Arrange
        var asyncChangeHandlerInvoked = false;
        using var ctx = new TestContext();
        var component = ctx.Render<TestComponentWithAsyncChangeHandler>(parameters => parameters
            .Add(p => p.Name, "John")
            .Add(p => p.OnAsyncChange, async () =>
            {
                await Task.Delay(10);
                asyncChangeHandlerInvoked = true;
            }));

        // Act
        component.Render(parameters => parameters
            .Add(p => p.Name, "Jane"));

        // Give async handler time to complete
        await Task.Delay(50);

        // Assert
        Assert.True(asyncChangeHandlerInvoked);
    }
}

// Test Components

public class TestComponent : StatefulComponentBase
{
    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public EventCallback<string?> NameChanged { get; set; }

    private ParameterState<string?> _nameState = null!;

    public TestComponent()
    {
        using var registerScope = CreateComponentParameterStateScope();

        _nameState = registerScope
            .RegisterParameter<string?>(nameof(Name))
            .WithParameter(() => Name)
            .WithEventCallback(() => NameChanged);
    }

    public string? GetNameStateValue() => _nameState.Value;
}

public class TestComponentWithChangeHandler : StatefulComponentBase
{
    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public Action? OnChange { get; set; }

    private ParameterState<string?> _nameState = null!;

    public TestComponentWithChangeHandler()
    {
        using var registerScope = CreateComponentParameterStateScope();

        _nameState = registerScope
            .RegisterParameter<string?>(nameof(Name))
            .WithParameter(() => Name)
            .WithChangeHandler((oldValue, newValue) =>
            {
                OnChange?.Invoke();
            });
    }

    public string? GetNameStateValue() => _nameState.Value;
}

public class TestComponentWithAsyncChangeHandler : StatefulComponentBase
{
    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public Func<Task>? OnAsyncChange { get; set; }

    private ParameterState<string?> _nameState = null!;

    public TestComponentWithAsyncChangeHandler()
    {
        using var registerScope = CreateComponentParameterStateScope();

        _nameState = registerScope
            .RegisterParameter<string?>(nameof(Name))
            .WithParameter(() => Name)
            .WithChangeHandler(async (oldValue, newValue) =>
            {
                if (OnAsyncChange != null)
                {
                    await OnAsyncChange();
                }
            });
    }

    public string? GetNameStateValue() => _nameState.Value;
}

public class TestComponentMultipleParams : StatefulComponentBase
{
    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public int Age { get; set; }

    [Parameter]
    public bool IsActive { get; set; }

    private ParameterState<string?> _nameState = null!;
    private ParameterState<int> _ageState = null!;
    private ParameterState<bool> _isActiveState = null!;

    public TestComponentMultipleParams()
    {
        using var registerScope = CreateComponentParameterStateScope();

        _nameState = registerScope
            .RegisterParameter<string?>(nameof(Name))
            .WithParameter(() => Name);

        _ageState = registerScope
            .RegisterParameter<int>(nameof(Age))
            .WithParameter(() => Age);

        _isActiveState = registerScope
            .RegisterParameter<bool>(nameof(IsActive))
            .WithParameter(() => IsActive);
    }

    public string? GetNameStateValue() => _nameState.Value;
    public int GetAgeStateValue() => _ageState.Value;
    public bool GetIsActiveStateValue() => _isActiveState.Value;
}

public class TestComponentWithCustomComparer : StatefulComponentBase
{
    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public Action? OnChange { get; set; }

    private ParameterState<string?> _nameState = null!;

    public TestComponentWithCustomComparer()
    {
        using var registerScope = CreateComponentParameterStateScope();

        _nameState = registerScope
            .RegisterParameter<string?>(nameof(Name))
            .WithParameter(() => Name)
            .WithComparer(StringComparer.OrdinalIgnoreCase)
            .WithChangeHandler((oldValue, newValue) =>
            {
                OnChange?.Invoke();
            });
    }

    public string? GetNameStateValue() => _nameState.Value;
}
