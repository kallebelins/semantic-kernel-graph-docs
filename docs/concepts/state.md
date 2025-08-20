# State Management

State management is the foundation of data flow in SemanticKernel.Graph. This guide explains how to work with `GraphState`, `KernelArguments`, execution history, metadata, validation, and serialization to build robust and maintainable workflows.

## What You'll Learn

* How `GraphState` wraps `KernelArguments` for enhanced functionality
* Managing execution history and metadata
* State validation and versioning
* Serialization and deserialization capabilities
* Best practices for state design and management

## Concepts and Techniques

**GraphState**: An enhanced wrapper around `KernelArguments` that provides execution tracking, metadata management, validation, and serialization capabilities.

**KernelArguments**: The core Semantic Kernel container for key-value pairs that represents the execution state and context.

**Execution History**: A chronological record of all execution steps, including timing, results, and metadata for each node execution.

**State Validation**: Built-in integrity checks that ensure state consistency, serializability, and version compatibility.

**Serialization**: The ability to save and restore graph state with compression, version migration, and integrity validation.

## Prerequisites

* [First Graph Tutorial](../first-graph-5-minutes.md) completed
* Basic understanding of SemanticKernel.Graph concepts
* Familiarity with `KernelArguments` from Semantic Kernel

## Core State Components

### GraphState: Enhanced State Wrapper

`GraphState` is the primary state container that wraps `KernelArguments` with additional graph-specific functionality:

```csharp
using SemanticKernel.Graph.State;
using Microsoft.SemanticKernel;

// Create state with existing KernelArguments
var arguments = new KernelArguments
{
    ["input"] = "Hello World",
    ["timestamp"] = DateTimeOffset.UtcNow
};

var graphState = new GraphState(arguments);

// Access the underlying KernelArguments
var kernelArgs = graphState.KernelArguments;

// Access enhanced properties
var stateId = graphState.StateId;
var version = graphState.Version;
var createdAt = graphState.CreatedAt;
var lastModified = graphState.LastModified;
```

### State Properties and Metadata

Every `GraphState` instance includes built-in metadata:

```csharp
// State identification
var stateId = graphState.StateId;           // Unique identifier
var version = graphState.Version;           // State version (e.g., "1.1.0")
var createdAt = graphState.CreatedAt;       // Creation timestamp
var lastModified = graphState.LastModified; // Last modification timestamp
var isModified = graphState.IsModified;     // Whether state has been modified

// Execution tracking
var executionStepCount = graphState.ExecutionStepCount;
var executionHistory = graphState.ExecutionHistory;
```

## State Access and Manipulation

### Reading State Values

Use type-safe methods to read values from state:

```csharp
// Type-safe reading with defaults
var name = graphState.GetValue<string>("userName");
var age = graphState.GetValue<int>("userAge");
var isActive = graphState.GetValue<bool>("isActive");

// Safe reading with fallback values
var department = graphState.GetValueOrDefault("department", "Unknown");
var score = graphState.GetValueOrDefault("score", 0.0);

// Try-pattern for conditional reading
if (graphState.TryGetValue<string>("email", out var email))
{
    // Process email
}
```

### Writing to State

Update state values and track modifications:

```csharp
// Set values (automatically updates LastModified)
graphState.SetValue("processed", true);
graphState.SetValue("result", "Success");
graphState.SetValue("score", 95.5);

// Remove values
var wasRemoved = graphState.RemoveValue("temporaryData");

// Check modification status
if (graphState.IsModified)
{
    Console.WriteLine($"State modified at: {graphState.LastModified}");
}
```

### Complex Object Management

Store and retrieve complex objects in state:

```csharp
// Store complex objects
var userProfile = new
{
    Name = "Alice",
    Department = "Engineering",
    Skills = new[] { "C#", ".NET", "AI" },
    Experience = 5
};

graphState.SetValue("userProfile", userProfile);

// Retrieve and work with complex objects
var profile = graphState.GetValue<object>("userProfile");
var metadata = graphState.GetValue<Dictionary<string, object>>("metadata");

// Store collections
var tags = new List<string> { "AI", "Machine Learning", "Graphs" };
graphState.SetValue("tags", tags);
```

## Execution History and Tracking

### Execution Steps

`GraphState` automatically tracks execution history through `ExecutionStep` objects:

```csharp
// Each execution step includes:
var steps = graphState.ExecutionHistory;
foreach (var step in steps)
{
    Console.WriteLine($"Step: {step.StepId}");
    Console.WriteLine($"Node: {step.NodeId}");
    Console.WriteLine($"Function: {step.FunctionName}");
    Console.WriteLine($"Status: {step.Status}");
    Console.WriteLine($"Duration: {step.Duration}");
    Console.WriteLine($"Result: {step.Result}");
}
```

### Execution Status Types

Execution steps can have different statuses:

```csharp
public enum ExecutionStatus
{
    Running,      // Currently executing
    Completed,    // Successfully completed
    Failed,       // Execution failed
    Cancelled     // Execution was cancelled
}
```

### Adding Execution Metadata

Nodes can add custom metadata to execution steps:

```csharp
// In your node execution
var step = new ExecutionStep("nodeId", "functionName", DateTimeOffset.UtcNow);

// Add custom metadata
step.AddMetadata("inputSize", input.Length);
step.AddMetadata("processingTime", stopwatch.ElapsedMilliseconds);
step.AddMetadata("confidence", 0.95);

// Mark completion
step.MarkCompleted(result);
```

## State Validation and Integrity

### Built-in Validation

`GraphState` implements `ISerializableState` with comprehensive validation:

```csharp
// Validate state integrity
var validationResult = graphState.ValidateIntegrity();

if (validationResult.IsValid)
{
    Console.WriteLine("State is valid");
}
else
{
    Console.WriteLine($"Validation failed: {validationResult.ErrorCount} errors");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"Error: {error.Message}");
    }
}
```

### Validation Result Details

Validation results provide detailed information:

```csharp
var result = graphState.ValidateIntegrity();

Console.WriteLine($"Valid: {result.IsValid}");
Console.WriteLine($"Errors: {result.ErrorCount}");
Console.WriteLine($"Warnings: {result.WarningCount}");
Console.WriteLine($"Total Issues: {result.TotalIssues}");

// Get detailed summary
var summary = result.CreateSummary(includeDetails: true);
Console.WriteLine(summary);
```

### Custom Validation

Extend validation with custom rules:

```csharp
public static ValidationResult ValidateCustomRules(GraphState state)
{
    var result = new ValidationResult();
    
    // Check required fields
    if (!state.TryGetValue<string>("userId", out _))
    {
        result.AddError("Required field 'userId' is missing");
    }
    
    // Validate data types
    if (state.TryGetValue<int>("age", out var age) && age < 0)
    {
        result.AddWarning("Age should be non-negative", "data_validation");
    }
    
    // Check business rules
    if (state.TryGetValue<string>("email", out var email) && !email.Contains("@"))
    {
        result.AddError("Invalid email format");
    }
    
    return result;
}
```

## State Versioning and Compatibility

### Version Management

`GraphState` includes semantic versioning for compatibility control:

```csharp
// Current version information
var currentVersion = StateVersion.Current;           // "1.1.0"
var minSupported = StateVersion.MinimumSupported;    // "1.0.0"

// Check version compatibility
var stateVersion = graphState.Version;
var isCompatible = stateVersion.IsCompatible;        // Compatible with current version
var requiresMigration = stateVersion.RequiresMigration; // Needs migration

// Version comparison
if (stateVersion < StateVersion.Current)
{
    Console.WriteLine("State version is older than current");
}
```

### Version Parsing

Create version objects from strings:

```csharp
// Parse version strings
var version = StateVersion.Parse("1.2.3");
Console.WriteLine($"Major: {version.Major}");    // 1
Console.WriteLine($"Minor: {version.Minor}");    // 2
Console.WriteLine($"Patch: {version.Patch}");    // 3

// Try-parse for safe version handling
if (StateVersion.TryParse("invalid", out var parsedVersion))
{
    // Use parsed version
}
else
{
    Console.WriteLine("Invalid version format");
}
```

## Serialization and Persistence

### Basic Serialization

Serialize state to string representation:

```csharp
// Serialize with default options
var serialized = graphState.Serialize();

// Serialize with custom options
var options = new SerializationOptions
{
    Indented = true,
    EnableCompression = true,
    IncludeMetadata = true,
    IncludeExecutionHistory = true
};

var verboseSerialized = graphState.Serialize(options);
```

### Serialization Options

Control what gets serialized:

```csharp
var options = new SerializationOptions
{
    Indented = false,                    // Compact JSON
    EnableCompression = true,            // Enable compression for large states
    IncludeMetadata = true,              // Include state metadata
    IncludeExecutionHistory = false,     // Exclude execution history
    ValidateIntegrity = true             // Validate before serialization
};
```

### Deserialization

Restore state from serialized data:

```csharp
using SemanticKernel.Graph.State;

// Deserialize with factory method
var deserializedResult = SerializableStateFactory.Deserialize<GraphState>(
    serializedData,
    json => JsonSerializer.Deserialize<GraphState>(json)
);

if (deserializedResult.IsSuccessful)
{
    var restoredState = deserializedResult.State;
    Console.WriteLine("State restored successfully");
    
    // Check if migration was applied
    if (deserializedResult.WasMigrated)
    {
        Console.WriteLine($"Migrated from version {deserializedResult.OriginalVersion}");
    }
}
else
{
    Console.WriteLine("Deserialization failed");
    var summary = deserializedResult.CreateSummary();
    Console.WriteLine(summary);
}
```

### Integrity Checks

Validate serialized data integrity:

```csharp
// Create checksum for integrity verification
var checksum = graphState.CreateChecksum();

// Validate integrity
var validationResult = graphState.ValidateIntegrity();
if (!validationResult.IsValid)
{
    throw new InvalidOperationException("State integrity validation failed");
}
```

## State Extensions and Utilities

### KernelArguments Extensions

Use extension methods to work with state:

```csharp
using SemanticKernel.Graph.Extensions;

var arguments = new KernelArguments
{
    ["input"] = "Hello World"
};

// Convert to GraphState
var graphState = arguments.ToGraphState();

// Check if already has GraphState
if (arguments.HasGraphState())
{
    var existingState = arguments.GetOrCreateGraphState();
}

// Set GraphState explicitly
arguments.SetGraphState(graphState);
```

### State Cloning and Merging

Create copies and merge states:

```csharp
using SemanticKernel.Graph.Extensions;

// Clone state (deep copy)
var clonedState = graphState.Clone();

// Merge states (other state takes priority)
var mergedState = graphState.MergeFrom(otherState);

// Check if states are equal
var areEqual = graphState.Equals(otherState);
```

### State Helpers

Use utility methods for common operations:

```csharp
using SemanticKernel.Graph.State;

// Estimate serialized size
var estimatedSize = StateHelpers.EstimateSerializedSize(graphState);

// Validate serializability
var serializationValidation = StateValidator.ValidateSerializability(graphState);

// Get parameter names
var parameterNames = graphState.GetParameterNames();
```

## Advanced State Patterns

### State Composition

Build complex state structures:

```csharp
// Create hierarchical state
var state = new GraphState(new KernelArguments
{
    ["user"] = new Dictionary<string, object>
    {
        ["profile"] = new
        {
            Name = "Alice",
            Email = "alice@example.com"
        },
        ["preferences"] = new Dictionary<string, object>
        {
            ["theme"] = "dark",
            ["language"] = "en"
        }
    },
    ["session"] = new Dictionary<string, object>
    {
        ["startTime"] = DateTimeOffset.UtcNow,
        ["activeTab"] = "dashboard"
    }
});
```

### State Lifecycle Management

Manage state throughout execution:

```csharp
public class StateAwareNode : IGraphNode
{
    public async Task<FunctionResult> ExecuteAsync(GraphState state)
    {
        // Read input state
        var input = state.GetValue<string>("input");
        
        // Process and update state
        var result = await ProcessInput(input);
        state.SetValue("processedResult", result);
        state.SetValue("processingTimestamp", DateTimeOffset.UtcNow);
        
        // Add execution metadata
        state.AddExecutionStep("StateAwareNode", "ProcessInput", DateTimeOffset.UtcNow);
        
        return new FunctionResult(result);
    }
}
```

### State Validation Patterns

Implement comprehensive validation:

```csharp
public static class StateValidationRules
{
    public static ValidationResult ValidateUserProfile(GraphState state)
    {
        var result = new ValidationResult();
        
        // Required fields validation
        var requiredFields = new[] { "userId", "userName", "email" };
        foreach (var field in requiredFields)
        {
            if (!state.TryGetValue<string>(field, out var value) || string.IsNullOrEmpty(value))
            {
                result.AddError($"Required field '{field}' is missing or empty");
            }
        }
        
        // Data type validation
        if (state.TryGetValue<int>("age", out var age))
        {
            if (age < 0 || age > 150)
            {
                result.AddWarning("Age value seems unrealistic", "data_validation");
            }
        }
        
        // Business rule validation
        if (state.TryGetValue<string>("email", out var email))
        {
            if (!email.Contains("@") || !email.Contains("."))
            {
                result.AddError("Invalid email format");
            }
        }
        
        return result;
    }
}
```

## Best Practices

### State Design Principles

1. **Keep State Simple**: Use clear, descriptive key names
2. **Type Safety**: Use `GetValue<T>()` for type-safe access
3. **Validation**: Validate state at key points in your workflow
4. **Metadata**: Add meaningful metadata for debugging and monitoring
5. **Serialization**: Consider what needs to be persisted vs. transient

### Performance Considerations

1. **Large Objects**: Be mindful of serialization costs for large objects
2. **Execution History**: Limit history size for long-running workflows
3. **Compression**: Enable compression for large states
4. **Validation**: Use validation selectively to avoid performance impact

### Error Handling

1. **Graceful Degradation**: Handle missing state gracefully
2. **Validation**: Validate state before critical operations
3. **Logging**: Log state changes for debugging
4. **Recovery**: Implement state recovery mechanisms

## Troubleshooting

### Common Issues

**State Key Not Found**
```
System.Collections.Generic.KeyNotFoundException: The given key 'missingKey' was not present
```
**Solution**: Use `GetValueOrDefault()` or check with `ContainsKey()` before reading.

**Serialization Errors**
```
System.Text.Json.JsonException: The JSON value could not be converted to type
```
**Solution**: Ensure all objects in state are JSON-serializable.

**Version Compatibility Issues**
```
State version 1.0.0 is not compatible with current version 1.1.0
```
**Solution**: Use state migration or update your workflow to handle version differences.

**Large State Performance Issues**
```
Serialized state too large: 50,000,000 bytes
```
**Solution**: Enable compression, exclude unnecessary data, or split large states.

## See Also

* [State Quickstart](../state-quickstart.md) - Quick introduction to state management
* [State Tutorial](../state-tutorial.md) - Comprehensive state management tutorial
* [Execution Model](execution-model.md) - How state flows through execution
* [Checkpointing](../checkpointing-quickstart.md) - Saving and restoring state
* [Graph Concepts](graph-concepts.md) - Core graph concepts and patterns
