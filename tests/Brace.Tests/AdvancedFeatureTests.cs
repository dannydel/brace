using Bunit;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace Brace.Tests;

/// <summary>
/// Tests for advanced features including performance, enumerables, and complex objects
/// </summary>
public class AdvancedFeatureTests : BunitContext
{
    #region Performance Tests

    [Fact]
    public void PerformanceTest_ShouldHandleManyParametersEfficiently()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act - Create component with 10 parameters
        Render<ComponentWithManyParameters>(parameters => parameters
            .Add(p => p.Param1, "Value1")
            .Add(p => p.Param2, "Value2")
            .Add(p => p.Param3, "Value3")
            .Add(p => p.Param4, "Value4")
            .Add(p => p.Param5, "Value5")
            .Add(p => p.Param6, "Value6")
            .Add(p => p.Param7, "Value7")
            .Add(p => p.Param8, "Value8")
            .Add(p => p.Param9, "Value9")
            .Add(p => p.Param10, "Value10"));

        stopwatch.Stop();

        // Assert - Should complete in reasonable time (< 100ms)
        Assert.True(stopwatch.ElapsedMilliseconds < 100,
            $"Initial render took {stopwatch.ElapsedMilliseconds}ms, expected < 100ms");
    }

    [Fact]
    public void PerformanceTest_ShouldHandleMultipleUpdatesEfficiently()
    {
        // Arrange
        var component = Render<TestComponentWithString>(parameters => parameters
            .Add(p => p.Value, "Initial"));

        var stopwatch = Stopwatch.StartNew();

        // Act - Update parameter 100 times
        for (int i = 0; i < 100; i++)
        {
            component.Render(parameters => parameters
                .Add(p => p.Value, $"Update{i}"));
        }

        stopwatch.Stop();

        // Assert - Should complete in reasonable time (< 500ms for 100 updates)
        Assert.True(stopwatch.ElapsedMilliseconds < 500,
            $"100 updates took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
        Assert.Equal("Update99", component.Instance.GetValueState());
    }

    #endregion

    #region List<T> Tests

    [Fact]
    public void ListParameter_ShouldCaptureInitialList()
    {
        // Arrange & Act
        var initialList = new List<string> { "Item1", "Item2", "Item3" };
        var component = Render<TestComponentWithList>(parameters => parameters
            .Add(p => p.Items, initialList));

        // Assert
        Assert.NotNull(component.Instance.GetItemsState());
        Assert.Equal(3, component.Instance.GetItemsState()?.Count);
        Assert.Equal("Item1", component.Instance.GetItemsState()?[0]);
    }

    [Fact]
    public void ListParameter_ShouldDetectListChange()
    {
        // Arrange
        var changeDetected = false;
        var initialList = new List<string> { "Item1" };
        var component = Render<TestComponentWithList>(parameters => parameters
            .Add(p => p.Items, initialList)
            .Add(p => p.OnChange, () => changeDetected = true));

        // Act - Pass a different list reference
        var newList = new List<string> { "Item1", "Item2" };
        component.Render(parameters => parameters
            .Add(p => p.Items, newList));

        // Assert
        Assert.True(changeDetected);
        Assert.Equal(2, component.Instance.GetItemsState()?.Count);
    }

    [Fact]
    public void ListParameter_ShouldNotDetectChangeWhenListContentsSame()
    {
        // Arrange
        var changeDetected = false;
        var initialList = new List<string> { "Item1", "Item2" };
        var component = Render<TestComponentWithList>(parameters => parameters
            .Add(p => p.Items, initialList)
            .Add(p => p.OnChange, () => changeDetected = true));

        // Act - Pass the same list reference
        component.Render(parameters => parameters
            .Add(p => p.Items, initialList));

        // Assert - No change because it's the same reference
        Assert.False(changeDetected);
    }

    [Fact]
    public void ListParameter_ShouldHandleEmptyList()
    {
        // Arrange & Act
        var emptyList = new List<string>();
        var component = Render<TestComponentWithList>(parameters => parameters
            .Add(p => p.Items, emptyList));

        // Assert
        Assert.NotNull(component.Instance.GetItemsState());
        Assert.Empty(component.Instance.GetItemsState()!);
    }

    [Fact]
    public void ListParameter_ShouldHandleNullList()
    {
        // Arrange & Act
        var component = Render<TestComponentWithList>(parameters => parameters
            .Add(p => p.Items, (List<string>?)null));

        // Assert
        Assert.Null(component.Instance.GetItemsState());
    }

    #endregion

    #region Dictionary<TKey, TValue> Tests

    [Fact]
    public void DictionaryParameter_ShouldCaptureInitialDictionary()
    {
        // Arrange & Act
        var initialDict = new Dictionary<string, int>
        {
            ["Key1"] = 1,
            ["Key2"] = 2
        };
        var component = Render<TestComponentWithDictionary>(parameters => parameters
            .Add(p => p.Data, initialDict));

        // Assert
        Assert.NotNull(component.Instance.GetDataState());
        Assert.Equal(2, component.Instance.GetDataState()?.Count);
        Assert.Equal(1, component.Instance.GetDataState()?["Key1"]);
    }

    [Fact]
    public void DictionaryParameter_ShouldDetectDictionaryChange()
    {
        // Arrange
        var changeDetected = false;
        var initialDict = new Dictionary<string, int> { ["Key1"] = 1 };
        var component = Render<TestComponentWithDictionary>(parameters => parameters
            .Add(p => p.Data, initialDict)
            .Add(p => p.OnChange, () => changeDetected = true));

        // Act - Pass a different dictionary reference
        var newDict = new Dictionary<string, int> { ["Key1"] = 1, ["Key2"] = 2 };
        component.Render(parameters => parameters
            .Add(p => p.Data, newDict));

        // Assert
        Assert.True(changeDetected);
        Assert.Equal(2, component.Instance.GetDataState()?.Count);
    }

    #endregion

    #region Complex Object Tests

    [Fact]
    public void ComplexObjectParameter_ShouldCaptureInitialObject()
    {
        // Arrange & Act
        var person = new Person
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            Age = 30,
            Address = new Address
            {
                Street = "123 Main St",
                City = "Springfield",
                ZipCode = "12345"
            },
            Tags = new List<string> { "developer", "architect" }
        };

        var component = Render<TestComponentWithComplexObject>(parameters => parameters
            .Add(p => p.Person, person));

        // Assert
        var statePerson = component.Instance.GetPersonState();
        Assert.NotNull(statePerson);
        Assert.Equal("John Doe", statePerson?.Name);
        Assert.Equal(30, statePerson?.Age);
        Assert.Equal("Springfield", statePerson?.Address?.City);
        Assert.Equal(2, statePerson?.Tags?.Count);
    }

    [Fact]
    public void ComplexObjectParameter_ShouldDetectObjectChange()
    {
        // Arrange
        var changeDetected = false; 
        var person1 = new Person { Id = 1, Name = "John" };
        var component = Render<TestComponentWithComplexObject>(parameters => parameters
            .Add(p => p.Person, person1)
            .Add(p => p.OnChange, () => changeDetected = true));

        // Act - Pass a different object reference
        var person2 = new Person { Id = 2, Name = "Jane" };
        component.Render(parameters => parameters
            .Add(p => p.Person, person2));

        // Assert
        Assert.True(changeDetected);
        Assert.Equal("Jane", component.Instance.GetPersonState()?.Name);
    }

    [Fact]
    public void ComplexObjectParameter_ShouldUseCustomComparer()
    {
        // Arrange
        var changeDetected = false; 
        var person1 = new Person { Id = 1, Name = "John", Age = 30 };
        var component = Render<TestComponentWithComplexObjectAndComparer>(parameters => parameters
            .Add(p => p.Person, person1)
            .Add(p => p.OnChange, () => changeDetected = true));

        // Act - Pass different object with same ID (custom comparer checks ID only)
        var person2 = new Person { Id = 1, Name = "John Updated", Age = 31 };
        component.Render(parameters => parameters
            .Add(p => p.Person, person2));

        // Assert - Should not detect change because IDs are the same
        Assert.False(changeDetected);
    }

    [Fact]
    public void ComplexObjectParameter_ShouldHandleNestedObjectChanges()
    {
        // Arrange
        var changeDetected = false;
        var person1 = new Person
        {
            Id = 1,
            Name = "John",
            Address = new Address { City = "Boston" }
        };
        var component = Render<TestComponentWithComplexObject>(parameters => parameters
            .Add(p => p.Person, person1)
            .Add(p => p.OnChange, () => changeDetected = true));

        // Act - Change nested address object
        var person2 = new Person
        {
            Id = 1,
            Name = "John",
            Address = new Address { City = "New York" }
        };
        component.Render(parameters => parameters
            .Add(p => p.Person, person2));

        // Assert - Should detect change because it's a different object reference
        Assert.True(changeDetected);
        Assert.Equal("New York", component.Instance.GetPersonState()?.Address?.City);
    }

    #endregion
}

#region Test Components

public class TestComponentWithString : StatefulComponentBase
{
    [Parameter]
    public string? Value { get; set; }

    private readonly ParameterState<string?> _valueState;

    public TestComponentWithString()
    {
        using var registerScope = CreateComponentParameterStateScope();
        _valueState = registerScope
            .RegisterParameter<string?>(nameof(Value))
            .WithParameter(() => Value);
    }

    public string? GetValueState() => _valueState.Value;
}

public class ComponentWithManyParameters : StatefulComponentBase
{
    [Parameter] public string? Param1 { get; set; }
    [Parameter] public string? Param2 { get; set; }
    [Parameter] public string? Param3 { get; set; }
    [Parameter] public string? Param4 { get; set; }
    [Parameter] public string? Param5 { get; set; }
    [Parameter] public string? Param6 { get; set; }
    [Parameter] public string? Param7 { get; set; }
    [Parameter] public string? Param8 { get; set; }
    [Parameter] public string? Param9 { get; set; }
    [Parameter] public string? Param10 { get; set; }
    
    private ParameterState<string?> _param1State;
    private ParameterState<string?> _param2State;
    private ParameterState<string?> _param3State;
    private ParameterState<string?> _param4State;
    private ParameterState<string?> _param5State;
    private ParameterState<string?> _param6State;
    private ParameterState<string?> _param7State;
    private ParameterState<string?> _param8State;
    private ParameterState<string?> _param9State;
    private ParameterState<string?> _param10State;

    public ComponentWithManyParameters()
    {
        using var registerScope = CreateComponentParameterStateScope();
        _param1State = registerScope.RegisterParameter<string?>(nameof(Param1)).WithParameter(() => Param1);
        _param2State = registerScope.RegisterParameter<string?>(nameof(Param2)).WithParameter(() => Param2);
        _param3State = registerScope.RegisterParameter<string?>(nameof(Param3)).WithParameter(() => Param3);
        _param4State = registerScope.RegisterParameter<string?>(nameof(Param4)).WithParameter(() => Param4);
        _param5State = registerScope.RegisterParameter<string?>(nameof(Param5)).WithParameter(() => Param5);
        _param6State = registerScope.RegisterParameter<string?>(nameof(Param6)).WithParameter(() => Param6);
        _param7State = registerScope.RegisterParameter<string?>(nameof(Param7)).WithParameter(() => Param7);
        _param8State = registerScope.RegisterParameter<string?>(nameof(Param8)).WithParameter(() => Param8);
        _param9State = registerScope.RegisterParameter<string?>(nameof(Param9)).WithParameter(() => Param9);
        _param10State = registerScope.RegisterParameter<string?>(nameof(Param10)).WithParameter(() => Param10);
    }
}

public class TestComponentWithList : StatefulComponentBase
{
    [Parameter]
    public List<string>? Items { get; set; }

    [Parameter]
    public Action? OnChange { get; set; }

    private readonly ParameterState<List<string>?> _itemsState;

    public TestComponentWithList()
    {
        using var registerScope = CreateComponentParameterStateScope();
        _itemsState = registerScope
            .RegisterParameter<List<string>?>(nameof(Items))
            .WithParameter(() => Items)
            .WithChangeHandler((_, _) => OnChange?.Invoke());
    }

    public List<string>? GetItemsState() => _itemsState.Value;
}

public class TestComponentWithDictionary : StatefulComponentBase
{
    [Parameter]
    public Dictionary<string, int>? Data { get; set; }

    [Parameter]
    public Action? OnChange { get; set; }

    private readonly ParameterState<Dictionary<string, int>?> _dataState;

    public TestComponentWithDictionary()
    {
        using var registerScope = CreateComponentParameterStateScope();
        _dataState = registerScope
            .RegisterParameter<Dictionary<string, int>?>(nameof(Data))
            .WithParameter(() => Data)
            .WithChangeHandler((_, _) => OnChange?.Invoke());
    }

    public Dictionary<string, int>? GetDataState() => _dataState.Value;
}

public class TestComponentWithComplexObject : StatefulComponentBase
{
    [Parameter]
    public Person? Person { get; set; }

    [Parameter]
    public Action? OnChange { get; set; }

    private ParameterState<Person?> _personState = null!;

    public TestComponentWithComplexObject()
    {
        using var registerScope = CreateComponentParameterStateScope();
        _personState = registerScope
            .RegisterParameter<Person?>(nameof(Person))
            .WithParameter(() => Person)
            .WithChangeHandler((_, _) => OnChange?.Invoke());
    }

    public Person? GetPersonState() => _personState.Value;
}

public class TestComponentWithComplexObjectAndComparer : StatefulComponentBase
{
    [Parameter]
    public Person? Person { get; set; }

    [Parameter]
    public Action? OnChange { get; set; }

    private readonly ParameterState<Person?> _personState;

    public TestComponentWithComplexObjectAndComparer()
    {
        using var registerScope = CreateComponentParameterStateScope();
        _personState = registerScope
            .RegisterParameter<Person?>(nameof(Person))
            .WithParameter(() => Person)
            .WithComparer(new PersonIdComparer())
            .WithChangeHandler((_, _) => OnChange?.Invoke());
    }
}

#endregion

#region Test Data Classes

public class Person
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int Age { get; set; }
    public Address? Address { get; set; }
    public List<string>? Tags { get; set; }
}

public class Address
{
    public string? Street { get; set; }
    public string? City { get; init; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
}

public class PersonIdComparer : IEqualityComparer<Person?>
{
    public bool Equals(Person? x, Person? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;
        return x.Id == y.Id;
    }

    public int GetHashCode(Person? obj)
    {
        return obj?.Id.GetHashCode() ?? 0;
    }
}

#endregion
