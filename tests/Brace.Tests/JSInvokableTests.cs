using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
// ReSharper disable AccessToModifiedClosure

namespace Brace.Tests;

/// <summary>
/// Tests for handling parameter updates from JavaScript interop (JSInvokable methods)
/// These tests demonstrate the proper pattern for JSInvokable methods that update parameters
/// </summary>
public class JSInvokableTests : BunitContext
{
    [Fact]
    public void JSInvokable_ShouldWorkWithTwoWayBinding()
    {
        // Arrange
        var currentMessage = "Initial";

        IRenderedComponent<TestComponentWithJSInvokable>? renderedComponent = null;
        var component = Render<TestComponentWithJSInvokable>(parameters => parameters
            .Add(p => p.Message, currentMessage)
            .Add(p => p.MessageChanged,
                EventCallback.Factory.Create<string?>(this, newMsg =>
            {
                // Simulate parent updating its state and re-rendering child
                currentMessage = newMsg ?? string.Empty;
                renderedComponent?.Render(p => p
                    .Add(cp => cp.Message, currentMessage)
                    .Add(cp => cp.MessageChanged, 
                        EventCallback.Factory.Create<string?>(this, msg => 
                            currentMessage = msg ?? string.Empty)));
            })));
        renderedComponent = component;

        // Act - Simulate JS calling the JSInvokable method
        component.Instance.UpdateMessageFromJS("Updated from JS");

        // Assert
        Assert.Equal("Updated from JS", component.Instance.GetMessageState());
    }

    [Fact]
    public async Task JSInvokableAsync_ShouldWorkWithTwoWayBinding()
    {
        // Arrange
        var currentMessage = "Initial";

        IRenderedComponent<TestComponentWithJSInvokable>? renderedComponent = null;
        var component = Render<TestComponentWithJSInvokable>(parameters => parameters
            .Add(p => p.Message, currentMessage)
            .Add(p => p.MessageChanged, EventCallback.Factory.Create<string?>(this, async (newMsg) =>
            {
                currentMessage = newMsg ?? string.Empty;
                await Task.Delay(1);
                renderedComponent?.Render(p => p
                    .Add(cp => cp.Message, currentMessage)
                    .Add(cp => cp.MessageChanged, 
                        EventCallback.Factory.Create<string?>(this, msg
                        => currentMessage = msg ?? string.Empty)));
            })));
        renderedComponent = component;

        // Act
        await component.Instance.UpdateMessageFromJSAsync("Async Update");
        await Task.Delay(10); // Wait for async operations

        // Assert
        Assert.Equal("Async Update", component.Instance.GetMessageState());
    }

    [Fact]
    public void JSInvokable_ShouldTriggerEventCallback()
    {
        // Arrange
        var callbackInvoked = false;
        var callbackValue = string.Empty;
        
        var component = Render<TestComponentWithJSInvokable>(parameters => parameters
            .Add(p => p.Message, "Initial")
            .Add(p => p.MessageChanged, 
                EventCallback.Factory.Create<string?>(this, value =>
            {
                callbackInvoked = true;
                callbackValue = value ?? string.Empty;
            })));

        // Act
        component.Instance.UpdateMessageFromJS("Test");

        // Assert - Verify callback was invoked
        Assert.True(callbackInvoked);
        Assert.Equal("Test", callbackValue);
    }

    [Fact]
    public void JSInvokable_ShouldHandleComplexObjects()
    {
        // Arrange
        var currentPerson = new Person { Id = 1, Name = "John", Age = 30 };

        IRenderedComponent<TestComponentWithJSInvokableComplexObject>? renderedComponent = null;
        var component = Render<TestComponentWithJSInvokableComplexObject>(parameters => parameters
            .Add(p => p.Person, currentPerson)
            .Add(p => p.PersonChanged, 
                EventCallback.Factory.Create<Person?>(this, newPerson =>
            {
                currentPerson = newPerson;
                renderedComponent?.Render(p => p
                    .Add(cp => cp.Person, currentPerson)
                    .Add(cp => cp.PersonChanged, 
                        EventCallback.Factory.Create<Person?>(this, person 
                            => currentPerson = person)));
            })));
        renderedComponent = component;

        // Act
        var updatedPerson = new Person { Id = 2, Name = "Jane", Age = 25 };
        component.Instance.UpdatePersonFromJS(updatedPerson);

        // Assert
        Assert.Equal("Jane", component.Instance.GetPersonState()?.Name);
        Assert.Equal(25, component.Instance.GetPersonState()?.Age);
    }

    [Fact]
    public void JSInvokable_ShouldHandleListUpdates()
    {
        // Arrange
        var currentList = new List<string> { "Item1", "Item2" };

        IRenderedComponent<TestComponentWithJSInvokableList>? renderedComponent = null;
        var component = Render<TestComponentWithJSInvokableList>(parameters => parameters
            .Add(p => p.Items, currentList)
            .Add(p => p.ItemsChanged, 
                EventCallback.Factory.Create<List<string>?>(this, newList =>
            {
                currentList = newList ?? [];
                renderedComponent?.Render(p => p
                    .Add(cp => cp.Items, currentList)
                    .Add(cp => cp.ItemsChanged, 
                        EventCallback.Factory.Create<List<string>?>(this, l 
                            => currentList = l ?? [])));

            })));
        renderedComponent = component;

        // Act
        var updatedList = new List<string> { "Item1", "Item2", "Item3", "Item4" };
        component.Instance.UpdateItemsFromJS(updatedList);

        // Assert
        Assert.Equal(4, component.Instance.GetItemsState()?.Count);
        Assert.Contains("Item3", component.Instance.GetItemsState() ?? []);
    }

    [Fact]
    public void JSInvokable_ShouldHandleNullValues()
    {
        // Arrange
        string? currentMessage = "Initial";

        IRenderedComponent<TestComponentWithJSInvokable>? renderedComponent = null;
        var component = Render<TestComponentWithJSInvokable>(
            parameters => parameters
            .Add(p => p.Message, currentMessage)
            .Add(p => p.MessageChanged, EventCallback.Factory.Create<string?>(
                this, newMsg =>
            {
                currentMessage = newMsg;
                renderedComponent?.Render(p => p
                    .Add(cp => cp.Message, currentMessage)
                    .Add(cp => cp.MessageChanged, 
                        EventCallback.Factory.Create<string?>(this, msg 
                            => currentMessage = msg)));
            })));
        renderedComponent = component;

        // Act
        component.Instance.UpdateMessageFromJS(null);

        // Assert
        Assert.Null(component.Instance.GetMessageState());
    }

    [Fact]
    public async Task JSInvokable_ShouldHandleMultipleParametersSimultaneously()
    {
        // Arrange
        var currentName = "John";
        var currentAge = 30;
        var currentIsActive = false;

        var component = Render<TestComponentWithMultipleJSInvokables>(parameters => parameters
            .Add(p => p.Name, currentName)
            .Add(p => p.Age, currentAge)
            .Add(p => p.IsActive, currentIsActive)
            .Add(p => p.NameChanged, EventCallback.Factory.Create<string?>(this, (v) => currentName = v ?? string.Empty))
            .Add(p => p.AgeChanged, EventCallback.Factory.Create<int>(this, (v) => currentAge = v))
            .Add(p => p.IsActiveChanged, EventCallback.Factory.Create<bool>(this, (v) => currentIsActive = v)));

        // Act
        await component.Instance.UpdateAllFromJS("Jane", 25, true);

        // Re-render with updated values
        component.Render(parameters => parameters
            .Add(p => p.Name, currentName)
            .Add(p => p.Age, currentAge)
            .Add(p => p.IsActive, currentIsActive));

        // Assert
        Assert.Equal("Jane", component.Instance.GetNameState());
        Assert.Equal(25, component.Instance.GetAgeState());
        Assert.True(component.Instance.GetIsActiveState());
    }

    [Fact]
    public void JSInvokable_ShouldWorkWithDictionaries()
    {
        // Arrange
        var currentDict = new Dictionary<string, int> { ["Key1"] = 1 };

        IRenderedComponent<TestComponentWithJSInvokableDictionary>? renderedComponent = null;
        var component = Render<TestComponentWithJSInvokableDictionary>(parameters => parameters
            .Add(p => p.Data, currentDict)
            .Add(p => p.DataChanged, EventCallback.Factory.Create<Dictionary<string, int>?>(this, (newDict) =>
            {
                currentDict = newDict ?? new Dictionary<string, int>();
                renderedComponent?.Render(p => p
                    .Add(cp => cp.Data, currentDict)
                    .Add(cp => cp.DataChanged, 
                        EventCallback.Factory.Create<Dictionary<string, int>?>(this, d
                            => currentDict = d ?? new Dictionary<string, int>())));
            })));
        renderedComponent = component;

        // Act
        var updatedDict = new Dictionary<string, int> { ["Key1"] = 1, ["Key2"] = 2, ["Key3"] = 3 };
        component.Instance.UpdateDataFromJS(updatedDict);

        // Assert
        Assert.Equal(3, component.Instance.GetDataState()?.Count);
        Assert.Equal(2, component.Instance.GetDataState()?["Key2"]);
    }
}

#region Test Components for JSInvokable

public class TestComponentWithJSInvokable : StatefulComponentBase
{
    [Parameter]
    public string? Message { get; set; }

    [Parameter]
    public EventCallback<string?> MessageChanged { get; set; }

    private readonly ParameterState<string?> _messageState;

    public TestComponentWithJSInvokable()
    {
        using var registerScope = CreateComponentParameterStateScope();
        _messageState = registerScope
            .RegisterParameter<string?>(nameof(Message))
            .WithParameter(() => Message)
            .WithEventCallback(() => MessageChanged);
    }

    [JSInvokable]
    public void UpdateMessageFromJS(string? newMessage)
    {
        Message = newMessage;
        _ = MessageChanged.InvokeAsync(newMessage);
        InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public async Task UpdateMessageFromJSAsync(string? newMessage)
    {
        Message = newMessage;
        await MessageChanged.InvokeAsync(newMessage);
        await InvokeAsync(StateHasChanged);
    }

    public string? GetMessageState() => _messageState.Value;
}

public class TestComponentWithJSInvokableComplexObject : StatefulComponentBase
{
    [Parameter]
    public Person? Person { get; set; }

    [Parameter]
    public EventCallback<Person?> PersonChanged { get; set; }

    private readonly ParameterState<Person?> _personState;

    public TestComponentWithJSInvokableComplexObject()
    {
        using var registerScope = CreateComponentParameterStateScope();
        _personState = registerScope
            .RegisterParameter<Person?>(nameof(Person))
            .WithParameter(() => Person)
            .WithEventCallback(() => PersonChanged);
    }

    [JSInvokable]
    public void UpdatePersonFromJS(Person? newPerson)
    {
        Person = newPerson;
        _ = PersonChanged.InvokeAsync(newPerson);
        InvokeAsync(StateHasChanged);
    }

    public Person? GetPersonState() => _personState.Value;
}

public class TestComponentWithJSInvokableList : StatefulComponentBase
{
    [Parameter]
    public List<string>? Items { get; set; }

    [Parameter]
    public EventCallback<List<string>?> ItemsChanged { get; set; }

    private readonly ParameterState<List<string>?> _itemsState;

    public TestComponentWithJSInvokableList()
    {
        using var registerScope = CreateComponentParameterStateScope();
        _itemsState = registerScope
            .RegisterParameter<List<string>?>(nameof(Items))
            .WithParameter(() => Items)
            .WithEventCallback(() => ItemsChanged);
    }

    [JSInvokable]
    public void UpdateItemsFromJS(List<string>? newItems)
    {
        Items = newItems;
        _ = ItemsChanged.InvokeAsync(newItems);
        InvokeAsync(StateHasChanged);
    }

    public List<string>? GetItemsState() => _itemsState.Value;
}

public class TestComponentWithMultipleJSInvokables : StatefulComponentBase
{
    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public EventCallback<string?> NameChanged { get; set; }

    [Parameter]
    public int Age { get; set; }

    [Parameter]
    public EventCallback<int> AgeChanged { get; set; }

    [Parameter]
    public bool IsActive { get; set; }

    [Parameter]
    public EventCallback<bool> IsActiveChanged { get; set; }

    private readonly ParameterState<string?> _nameState;
    private readonly ParameterState<int> _ageState;
    private readonly ParameterState<bool> _isActiveState;

    public TestComponentWithMultipleJSInvokables()
    {
        using var registerScope = CreateComponentParameterStateScope();

        _nameState = registerScope
            .RegisterParameter<string?>(nameof(Name))
            .WithParameter(() => Name)
            .WithEventCallback(() => NameChanged);

        _ageState = registerScope
            .RegisterParameter<int>(nameof(Age))
            .WithParameter(() => Age)
            .WithEventCallback(() => AgeChanged);

        _isActiveState = registerScope
            .RegisterParameter<bool>(nameof(IsActive))
            .WithParameter(() => IsActive)
            .WithEventCallback(() => IsActiveChanged);
    }

    [JSInvokable]
    public async Task UpdateAllFromJS(string? newName, int newAge, bool newIsActive)
    {
        Name = newName;
        Age = newAge;
        IsActive = newIsActive;

        await NameChanged.InvokeAsync(newName);
        await AgeChanged.InvokeAsync(newAge);
        await IsActiveChanged.InvokeAsync(newIsActive);

        await InvokeAsync(StateHasChanged);
    }

    public string? GetNameState() => _nameState.Value;
    public int GetAgeState() => _ageState.Value;
    public bool GetIsActiveState() => _isActiveState.Value;
}

public class TestComponentWithJSInvokableDictionary : StatefulComponentBase
{
    [Parameter]
    public Dictionary<string, int>? Data { get; set; }

    [Parameter]
    public EventCallback<Dictionary<string, int>?> DataChanged { get; set; }

    private readonly ParameterState<Dictionary<string, int>?> _dataState;

    public TestComponentWithJSInvokableDictionary()
    {
        using var registerScope = CreateComponentParameterStateScope();
        _dataState = registerScope
            .RegisterParameter<Dictionary<string, int>?>(nameof(Data))
            .WithParameter(() => Data)
            .WithEventCallback(() => DataChanged);
    }

    [JSInvokable]
    public void UpdateDataFromJS(Dictionary<string, int>? newData)
    {
        Data = newData;
        _ = DataChanged.InvokeAsync(newData);
        InvokeAsync(StateHasChanged);
    }

    public Dictionary<string, int>? GetDataState() => _dataState.Value;
}

#endregion
