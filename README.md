<div>
   <div style="display:flex; justify-content: center; align-items: center; gap:16px; ">
      <div>
         <img src="resources/Brace.svg" alt="brace logo" style="height:50px;" />
      </div>
      <div>
         <span style="font-size:xxx-large">Brace</span>
      </div>
   </div>
   <div>
      <h2>Blazor component state simplified</h2>
   </div>
   <hr/>
</div>

A Blazor library that simplifies parameter state management by abstracting away the complexity of lifecycle methods and change detection.

This was heavily influenced by [MudBlazor](https://www.mudblazor.com) and how they are handling their state within components.

## The Problem

In Blazor components, updating parameter values outside of lifecycle methods can cause unexpected side effects:
- Parameters may update when you don't want them to
- Parameters may not update when you expect them to
- The recommended Microsoft pattern (private backing fields + OnParametersSet/SetParametersAsync) is tedious and error-prone

## The Solution

Brace provides a fluent API for managing parameter state that automatically integrates with Blazor's lifecycle hooks, giving you:
- ✨ Clean, declarative parameter registration
- 🔄 Automatic change detection
- 🎯 Type-safe parameter state management
- 🔔 Built-in support for EventCallbacks and change handlers
- 🛡️ Safe access to parameter values throughout component lifetime

## Quick Start

### 1. Inherit from StatefulComponentBase

```razor
@inherits StatefulComponentBase

<div>
    <p>Current Value: @_valueState.Value</p>
</div>

@code {
    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    private ParameterState<string?> _valueState = null!;

    public MyComponent()
    {
        using var registerScope = CreateComponentParameterStateScope();

        _valueState = registerScope
            .RegisterParameter<string?>(nameof(Value))
            .WithParameter(() => Value)
            .WithEventCallback(() => ValueChanged);
    }
}
```

### 2. Register Parameters in Constructor

Use the fluent API to configure your parameter state:

```csharp
public MyComponent()
{
    using var registerScope = CreateComponentParameterStateScope();

    _nameState = registerScope
        .RegisterParameter<string?>(nameof(Name))
        .WithParameter(() => Name)                    // Required: parameter getter
        .WithEventCallback(() => NameChanged)         // Optional: two-way binding
        .WithChangeHandler(OnNameChangedAsync)        // Optional: custom logic
        .WithComparer(StringComparer.OrdinalIgnoreCase); // Optional: custom equality
}
```

### 3. Access State Anywhere

```csharp
void SomeMethod()
{
    var currentValue = _nameState.Value;
    // Value is always safe to access after registration
}
```

## Features

### Automatic Change Detection

The library automatically detects when parameter values change using `EqualityComparer<T>.Default` by default:

```csharp
_valueState = registerScope
    .RegisterParameter<int>(nameof(Count))
    .WithParameter(() => Count)
    .WithEventCallback(() => CountChanged);
// EventCallback is only invoked when Count actually changes
```

### Custom Comparers

For complex types, provide a custom comparer:

```csharp
_personState = registerScope
    .RegisterParameter<Person>(nameof(Person))
    .WithParameter(() => Person)
    .WithComparer(new PersonEqualityComparer());
```

### Change Handlers

Execute custom logic when parameters change:

```csharp
// Async handler
_nameState = registerScope
    .RegisterParameter<string?>(nameof(Name))
    .WithParameter(() => Name)
    .WithChangeHandler(async (oldValue, newValue) =>
    {
        Console.WriteLine($"Name changed from '{oldValue}' to '{newValue}'");
        await LogChangeAsync(oldValue, newValue);
    });

// Sync handler
_countState = registerScope
    .RegisterParameter<int>(nameof(Count))
    .WithParameter(() => Count)
    .WithChangeHandler((oldValue, newValue) =>
    {
        Console.WriteLine($"Count: {oldValue} → {newValue}");
    });
```

### Two-Way Binding

Automatically invoke EventCallbacks for two-way binding:

```csharp
_valueState = registerScope
    .RegisterParameter<string?>(nameof(Value))
    .WithParameter(() => Value)
    .WithEventCallback(() => ValueChanged);
// ValueChanged is automatically invoked when Value changes
```

### Multiple Parameters

Register multiple parameters in the same scope:

```csharp
public MyComponent()
{
    using var registerScope = CreateComponentParameterStateScope();

    _nameState = registerScope
        .RegisterParameter<string?>(nameof(Name))
        .WithParameter(() => Name)
        .WithEventCallback(() => NameChanged);

    _countState = registerScope
        .RegisterParameter<int>(nameof(Count))
        .WithParameter(() => Count)
        .WithEventCallback(() => CountChanged);

    _isActiveState = registerScope
        .RegisterParameter<bool>(nameof(IsActive))
        .WithParameter(() => IsActive)
        .WithEventCallback(() => IsActiveChanged);
}
```

## Complete Example

```razor
@inherits StatefulComponentBase

<div class="user-profile">
    <h3>User Profile</h3>
    <p>Name: @_nameState.Value</p>
    <p>Age: @_ageState.Value</p>
    <p>Status: @(_isActiveState.Value ? "Active" : "Inactive")</p>

    <button @onclick="IncrementAge">Birthday!</button>
</div>

@code {
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

    private ParameterState<string?> _nameState = null!;
    private ParameterState<int> _ageState = null!;
    private ParameterState<bool> _isActiveState = null!;

    public UserProfile()
    {
        using var registerScope = CreateComponentParameterStateScope();

        _nameState = registerScope
            .RegisterParameter<string?>(nameof(Name))
            .WithParameter(() => Name)
            .WithEventCallback(() => NameChanged)
            .WithChangeHandler(OnNameChangedAsync);

        _ageState = registerScope
            .RegisterParameter<int>(nameof(Age))
            .WithParameter(() => Age)
            .WithEventCallback(() => AgeChanged)
            .WithChangeHandler((oldAge, newAge) =>
            {
                Console.WriteLine($"Age updated: {oldAge} → {newAge}");
            });

        _isActiveState = registerScope
            .RegisterParameter<bool>(nameof(IsActive))
            .WithParameter(() => IsActive)
            .WithEventCallback(() => IsActiveChanged);
    }

    private async Task OnNameChangedAsync(string? oldName, string? newName)
    {
        if (!string.IsNullOrEmpty(newName))
        {
            await LogUserUpdateAsync($"Name changed to {newName}");
        }
    }

    private async Task IncrementAge()
    {
        await AgeChanged.InvokeAsync(_ageState.Value + 1);
    }

    private Task LogUserUpdateAsync(string message)
    {
        // Your logging logic here
        return Task.CompletedTask;
    }
}
```

## API Reference

### StatefulComponentBase

Base class for components using parameter state management.

**Methods:**
- `CreateComponentParameterStateScope()` - Creates a registration scope for use in the constructor

### ParameterStateScope

Disposable scope for registering parameter states.

**Methods:**
- `RegisterParameter<T>(string parameterName)` - Registers a parameter and returns a builder

### ParameterStateBuilder<T>

Fluent builder for configuring parameter states.

**Methods:**
- `.WithParameter(Func<T?> getter)` - **(Required)** Specifies the parameter getter
- `.WithEventCallback(Func<EventCallback<T>> getter)` - Specifies the EventCallback for two-way binding
- `.WithChangeHandler(Func<T?, T?, Task> handler)` - Specifies an async change handler
- `.WithChangeHandler(Action<T?, T?> handler)` - Specifies a sync change handler
- `.WithComparer(IEqualityComparer<T> comparer)` - Specifies a custom equality comparer

### ParameterState<T>

Holds the current state of a parameter.

**Properties:**
- `Value` - Gets the current parameter value
- `ParameterName` - Gets the name of the parameter

## How It Works

1. **Registration**: In the constructor, you register parameters within a `ParameterStateScope`
2. **Initial Capture**: The library immediately captures the initial parameter value
3. **Lifecycle Integration**: The base class automatically hooks into `SetParametersAsync`, `OnInitialized`, and `OnParametersSet`
4. **Change Detection**: On each lifecycle method, the library checks if parameter values have changed
5. **Automatic Updates**: When a change is detected:
   - The state value is updated
   - Sync change handlers are invoked
   - Async change handlers are awaited
   - EventCallbacks are invoked for two-way binding

## Project Structure

```
Brace/
├── Brace.sln              # Solution file
├── README.md                        # This file
├── src/                            # Source code
│   ├── Brace.csproj       # Main library project
│   ├── StatefulComponentBase.cs    # Base component class
│   ├── ParameterStateContainer.cs  # State orchestration
│   ├── ParameterStateScope.cs      # Registration scope
│   ├── ParameterState.cs           # State holder
│   ├── ParameterStateBuilder.cs    # Fluent API builder
│   ├── _Imports.razor              # Razor imports
│   └── Examples/                   # Example components
│       ├── ExampleStatefulComponent.razor
│       └── SimpleExample.razor
└── tests/                          # Unit tests
    └── Brace.Tests/
        ├── Brace.Tests.csproj
        └── StatefulComponentTests.cs (9 passing tests)
```

## Building and Testing

```bash
# Build the solution
dotnet build Brace.sln

# Run tests
dotnet test Brace.sln

# Build the library only
dotnet build src/Brace.csproj
```

## Requirements

- .NET 10.0 or later
- Blazor (Server or WebAssembly)

## License

This project is licensed under MIT. See `LICENSE` for details.
