# Schema Typing and Validation

SemanticKernel.Graph provides a comprehensive type system and validation framework that ensures data consistency, enables compile-time checks, and supports seamless state evolution through migrations. This guide covers the complete schema typing ecosystem including type inference, validation, and state migration capabilities.

## Overview

The schema typing and validation system consists of several key components:

* **GraphTypeInferenceEngine**: Automatically infers input/output schemas for untyped nodes
* **StateValidator**: Comprehensive validation of graph state integrity and consistency
* **TypedSchema System**: Strong typing for graph parameters with primitive and .NET type support
* **State Migration System**: Automatic state evolution between different versions
* **Compile-time Validation**: Early detection of schema incompatibilities and errors

## Core Components

### GraphTypeInferenceEngine

The `GraphTypeInferenceEngine` performs lightweight type inference over graphs using available typed schemas. It propagates known output types from source nodes to targets that don't declare typed schemas.

```csharp
public static class GraphTypeInferenceEngine
{
    /// <summary>
    /// Infers input schemas for nodes that do not implement ITypedSchemaNode.
    /// When possible, derives input parameter names and types from upstream nodes' outputs by name.
    /// </summary>
    public static IReadOnlyDictionary<string, GraphIOSchema> InferInputSchemas(GraphExecutor graph);
}
```

**Key Features:**
* **Conservative inference**: Only infers types when confident about the relationship
* **Name-based propagation**: Maps parameters by name across node boundaries
* **Upstream analysis**: Examines predecessor nodes to determine input requirements
* **Fallback handling**: Provides untyped fallbacks when type information is unavailable

**Usage Example:**
```csharp
var graph = new GraphExecutor("inference-example");

// Add typed source node
var sourceNode = new TypedSourceNode();
graph.AddNode(sourceNode);

// Add untyped target node
var targetNode = new UntypedTargetNode();
graph.AddNode(targetNode);

graph.Connect(sourceNode.NodeId, targetNode.NodeId);

// Infer schemas for untyped nodes
var inferredSchemas = GraphTypeInferenceEngine.InferInputSchemas(graph);

// The target node now has inferred input schemas based on source outputs
var targetSchema = inferredSchemas[targetNode.NodeId];
```

### StateValidator

The `StateValidator` provides comprehensive integrity checks for graph states, ensuring data consistency and identifying potential issues early.

```csharp
public static class StateValidator
{
    /// <summary>
    /// Validates the complete integrity of a GraphState.
    /// </summary>
    public static ValidationResult ValidateState(GraphState state);
    
    /// <summary>
    /// Validates only the critical properties of the state.
    /// </summary>
    public static bool ValidateCriticalProperties(GraphState state);
}
```

**Validation Categories:**

1. **Basic Properties**: State ID, name, and metadata validation
2. **Data Validation**: Parameter names, values, and type consistency
3. **Execution History**: History size limits and integrity checks
4. **Version Validation**: Compatibility and migration requirements
5. **Size Validation**: Memory usage and serialization size limits

**Usage Example:**
```csharp
var state = new GraphState("validation-test");

// Add some data
state.SetValue("user_id", 123);
state.SetValue("user_name", "John Doe");

// Validate the state
var validationResult = StateValidator.ValidateState(state);

if (validationResult.IsValid)
{
    Console.WriteLine("State is valid");
}
else
{
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"Validation error: {error.Message}");
    }
    
    foreach (var warning in validationResult.Warnings)
    {
        Console.WriteLine($"Validation warning: {warning.Message}");
    }
}
```

## Typed Schema System

### GraphType

The `GraphType` class represents parameter types using either primitive classifications or .NET runtime types for stricter validation.

```csharp
public sealed class GraphType
{
    public static GraphType FromPrimitive(GraphPrimitiveType primitive);
    public static GraphType FromDotNetType(Type type);
    
    public GraphPrimitiveType Primitive { get; }
    public Type? DotNetType { get; }
    
    public bool IsValueCompatible(object? value);
    public bool IsAssignableTo(GraphType target);
}
```

**Primitive Types:**
* **Any**: Accepts any value (default)
* **String**: String values only
* **Integer**: Integer values (int, long, etc.)
* **Number**: Numeric values (int, double, decimal, etc.)
* **Boolean**: Boolean values only
* **Object**: Object instances
* **Array**: Array or collection values
* **Json**: JSON-formatted strings

**Usage Example:**
```csharp
// Create primitive types
var stringType = GraphType.FromPrimitive(GraphPrimitiveType.String);
var intType = GraphType.FromPrimitive(GraphPrimitiveType.Integer);
var numberType = GraphType.FromPrimitive(GraphPrimitiveType.Number);

// Create .NET type-based types
var userType = GraphType.FromDotNetType(typeof(User));
var listType = GraphType.FromDotNetType(typeof(List<string>));

// Check compatibility
Console.WriteLine(stringType.IsValueCompatible("hello")); // True
Console.WriteLine(stringType.IsValueCompatible(123));     // False
Console.WriteLine(intType.IsValueCompatible(42));         // True
Console.WriteLine(intType.IsValueCompatible(3.14));       // False
Console.WriteLine(numberType.IsValueCompatible(3.14));    // True

// Check type assignability
Console.WriteLine(intType.IsAssignableTo(numberType));    // True (int → number)
Console.WriteLine(numberType.IsAssignableTo(intType));    // False (number ↛ int)
```

### GraphParameterSchema

Defines the structure and constraints for individual graph parameters.

```csharp
public sealed class GraphParameterSchema
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool Required { get; init; }
    public GraphType Type { get; init; } = GraphType.FromPrimitive(GraphPrimitiveType.Any);
}
```

**Usage Example:**
```csharp
var parameterSchema = new GraphParameterSchema
{
    Name = "user_id",
    Description = "Unique identifier for the user",
    Required = true,
    Type = GraphType.FromPrimitive(GraphPrimitiveType.Integer)
};

var userSchema = new GraphParameterSchema
{
    Name = "user_data",
    Description = "Complete user information",
    Required = false,
    Type = GraphType.FromDotNetType(typeof(User))
};
```

### GraphIOSchema

Defines the complete input/output schema for a node, organizing parameters by direction.

```csharp
public sealed class GraphIOSchema
{
    public IReadOnlyDictionary<string, GraphParameterSchema> Inputs { get; init; }
    public IReadOnlyDictionary<string, GraphParameterSchema> Outputs { get; init; }
    
    public bool TryGetInput(string name, out GraphParameterSchema? schema);
    public bool TryGetOutput(string name, out GraphParameterSchema? schema);
}
```

**Usage Example:**
```csharp
var inputSchema = new Dictionary<string, GraphParameterSchema>
{
    ["query"] = new GraphParameterSchema
    {
        Name = "query",
        Description = "Search query string",
        Required = true,
        Type = GraphType.FromPrimitive(GraphPrimitiveType.String)
    },
    ["limit"] = new GraphParameterSchema
    {
        Name = "limit",
        Description = "Maximum number of results",
        Required = false,
        Type = GraphType.FromPrimitive(GraphPrimitiveType.Integer)
    }
};

var outputSchema = new Dictionary<string, GraphParameterSchema>
{
    ["results"] = new GraphParameterSchema
    {
        Name = "results",
        Description = "Search results array",
        Required = true,
        Type = GraphType.FromPrimitive(GraphPrimitiveType.Array)
    },
    ["total_count"] = new GraphParameterSchema
    {
        Name = "total_count",
        Description = "Total number of matching results",
        Required = true,
        Type = GraphType.FromPrimitive(GraphPrimitiveType.Integer)
    }
};

var nodeSchema = new GraphIOSchema
{
    Inputs = inputSchema,
    Outputs = outputSchema
};
```

### ITypedSchemaNode Interface

Nodes can implement this interface to expose their input/output schemas for validation and type inference.

```csharp
public interface ITypedSchemaNode
{
    /// <summary>
    /// Returns the input schema describing required/optional inputs.
    /// </summary>
    GraphIOSchema GetInputSchema();
    
    /// <summary>
    /// Returns the output schema describing values produced by the node.
    /// </summary>
    GraphIOSchema GetOutputSchema();
}
```

**Implementation Example:**
```csharp
public class SearchNode : IGraphNode, ITypedSchemaNode
{
    public GraphIOSchema GetInputSchema()
    {
        return new GraphIOSchema
        {
            Inputs = new Dictionary<string, GraphParameterSchema>
            {
                ["query"] = new GraphParameterSchema
                {
                    Name = "query",
                    Required = true,
                    Type = GraphType.FromPrimitive(GraphPrimitiveType.String)
                }
            },
            Outputs = new Dictionary<string, GraphParameterSchema>
            {
                ["results"] = new GraphParameterSchema
                {
                    Name = "results",
                    Required = true,
                    Type = GraphType.FromPrimitive(GraphPrimitiveType.Array)
                }
            }
        };
    }
    
    public GraphIOSchema GetOutputSchema() => GetInputSchema();
    
    // ... other IGraphNode implementation
}
```

## State Migration System

### StateVersion

Represents the version of graph state for compatibility control and migration.

```csharp
public readonly struct StateVersion : IEquatable<StateVersion>, IComparable<StateVersion>
{
    public static readonly StateVersion Current = new(1, 1, 0);
    public static readonly StateVersion MinimumSupported = new(1, 0, 0);
    
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    
    public bool IsCompatible => this >= MinimumSupported && Major == Current.Major;
    public bool RequiresMigration => this < Current;
}
```

**Version Compatibility:**
* **Major version**: Incompatible changes require migration
* **Minor version**: Backward-compatible additions
* **Patch version**: Bug fixes and minor improvements
* **Compatibility**: Same major version and >= minimum supported

### IStateMigration Interface

Defines the contract for state migrations between different versions.

```csharp
public interface IStateMigration
{
    StateVersion FromVersion { get; }
    StateVersion ToVersion { get; }
    string Description { get; }
    
    bool CanMigrate(StateVersion version);
    string Migrate(string serializedState);
}
```

**Implementation Example:**
```csharp
public class UserProfileMigration_1_0_0_to_1_1_0 : IStateMigration
{
    public StateVersion FromVersion => new(1, 0, 0);
    public StateVersion ToVersion => new(1, 1, 0);
    public string Description => "Adds user_preferences field to user profiles";
    
    public bool CanMigrate(StateVersion version) => version == FromVersion;
    
    public string Migrate(string serializedState)
    {
        try
        {
            // Parse the old state
            var oldState = JsonSerializer.Deserialize<Dictionary<string, object>>(serializedState);
            
            // Add new field with default value
            if (oldState.ContainsKey("user_profile"))
            {
                var profile = oldState["user_profile"] as Dictionary<string, object>;
                if (profile != null && !profile.ContainsKey("user_preferences"))
                {
                    profile["user_preferences"] = new Dictionary<string, object>();
                }
            }
            
            // Return migrated state
            return JsonSerializer.Serialize(oldState);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Migration failed: {ex.Message}", ex);
        }
    }
}
```

### StateMigrationManager

Central registry and manager for state migrations.

```csharp
public static class StateMigrationManager
{
    // Registration
    public static void RegisterMigration(IStateMigration migration);
    public static void RegisterMigrations(IEnumerable<IStateMigration> migrations);
    public static void ClearMigrations();
    
    // Migration operations
    public static bool IsMigrationNeeded(StateVersion version);
    public static string MigrateToCurrentVersion(string serializedState, StateVersion fromVersion);
    public static IList<IStateMigration> GetMigrationPath(StateVersion fromVersion, StateVersion toVersion);
    
    // Query operations
    public static IReadOnlyList<IStateMigration> GetAllMigrations();
    public static IList<IStateMigration> GetMigrationsForVersion(StateVersion version);
    public static MigrationStats GetMigrationStats();
}
```

**Usage Example:**
```csharp
// Register migrations
StateMigrationManager.RegisterMigration(new UserProfileMigration_1_0_0_to_1_1_0());
StateMigrationManager.RegisterMigration(new UserProfileMigration_1_1_0_to_1_2_0());

// Check if migration is needed
var oldVersion = new StateVersion(1, 0, 0);
if (StateMigrationManager.IsMigrationNeeded(oldVersion))
{
    Console.WriteLine("Migration required");
    
    // Get migration path
    var migrationPath = StateMigrationManager.GetMigrationPath(oldVersion, StateVersion.Current);
    Console.WriteLine($"Migration path: {string.Join(" → ", migrationPath.Select(m => m.ToVersion))}");
}

// Perform migration
try
{
    var migratedState = StateMigrationManager.MigrateToCurrentVersion(
        oldSerializedState, 
        oldVersion
    );
    Console.WriteLine("Migration completed successfully");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Migration failed: {ex.Message}");
}
```

## Compile-time Validation

### Schema Compatibility Validation

The `GraphExecutor` automatically validates schema compatibility across edges when typed schemas are available.

```csharp
// This validation happens automatically during graph integrity checks
var result = graph.ValidateGraphIntegrity();

// Schema compatibility warnings are included in the result
foreach (var warning in result.Warnings)
{
    if (warning.Message.Contains("may not be assignable"))
    {
        Console.WriteLine($"Schema compatibility warning: {warning.Message}");
    }
}
```

**Validation Features:**
* **Type checking**: Ensures output types are compatible with input types
* **Required parameter validation**: Warns about missing required inputs
* **Schema propagation**: Uses type inference to fill gaps in untyped nodes
* **Edge validation**: Checks compatibility across all graph connections

### ValidationResult

Comprehensive validation results including errors, warnings, and success status.

```csharp
public class ValidationResult
{
    public bool IsValid => !Errors.Any();
    public IReadOnlyList<ValidationIssue> Errors { get; }
    public IReadOnlyList<ValidationIssue> Warnings { get; }
    
    public void AddError(string message, string? code = null);
    public void AddWarning(string message, string? code = null);
    public void Merge(ValidationResult other);
}
```

**Usage Example:**
```csharp
var result = new ValidationResult();

// Add validation issues
if (string.IsNullOrEmpty(userName))
{
    result.AddError("User name is required", "USER_NAME_REQUIRED");
}

if (userAge < 13)
{
    result.AddWarning("User may be too young for this service", "AGE_WARNING");
}

// Check results
if (result.IsValid)
{
    Console.WriteLine("Validation passed");
}
else
{
    Console.WriteLine($"Validation failed with {result.Errors.Count} errors");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error: {error.Message} (Code: {error.Code})");
    }
}

if (result.Warnings.Any())
{
    Console.WriteLine($"Validation completed with {result.Warnings.Count} warnings");
    foreach (var warning in result.Warnings)
    {
        Console.WriteLine($"Warning: {warning.Message} (Code: {warning.Code})");
    }
}
```

## Advanced Type Inference Patterns

### Conservative Type Propagation

The type inference engine uses conservative strategies to avoid false positives:

```csharp
// Example: Inferring types across a chain of nodes
var graph = new GraphExecutor("type-inference-chain");

// Node A: Produces typed output
var nodeA = new TypedSourceNode();
nodeA.SetOutputSchema(new GraphIOSchema
{
    Outputs = new Dictionary<string, GraphParameterSchema>
    {
        ["data"] = new GraphParameterSchema
        {
            Name = "data",
            Type = GraphType.FromPrimitive(GraphPrimitiveType.String),
            Required = true
        }
    }
});

// Node B: Untyped, will receive inferred schema
var nodeB = new UntypedNode();

// Node C: Typed input requirements
var nodeC = new TypedTargetNode();
nodeC.SetInputSchema(new GraphIOSchema
{
    Inputs = new Dictionary<string, GraphParameterSchema>
    {
        ["data"] = new GraphParameterSchema
        {
            Name = "data",
            Type = GraphType.FromPrimitive(GraphPrimitiveType.String),
            Required = true
        }
    }
});

graph.AddNode(nodeA).AddNode(nodeB).AddNode(nodeC);
graph.Connect(nodeA.NodeId, nodeB.NodeId);
graph.Connect(nodeB.NodeId, nodeC.NodeId);

// Infer schemas for untyped nodes
var inferredSchemas = GraphTypeInferenceEngine.InferInputSchemas(graph);

// Node B now has inferred input schema based on Node A's output
var nodeBSchema = inferredSchemas[nodeB.NodeId];
Console.WriteLine($"Node B inferred inputs: {nodeBSchema.Inputs.Count}");
```

### Type Compatibility Matrix

The system provides a conservative type compatibility matrix:

```csharp
// Primitive type compatibility
var intType = GraphType.FromPrimitive(GraphPrimitiveType.Integer);
var numberType = GraphType.FromPrimitive(GraphPrimitiveType.Number);
var anyType = GraphType.FromPrimitive(GraphPrimitiveType.Any);

// Integer is assignable to Number
Console.WriteLine(intType.IsAssignableTo(numberType)); // True

// Number is not assignable to Integer (loss of precision)
Console.WriteLine(numberType.IsAssignableTo(intType)); // False

// Any type is compatible with everything
Console.WriteLine(anyType.IsAssignableTo(intType)); // True
Console.WriteLine(intType.IsAssignableTo(anyType)); // True

// .NET type compatibility
var baseType = GraphType.FromDotNetType(typeof(Animal));
var derivedType = GraphType.FromDotNetType(typeof(Dog));

// Derived types are assignable to base types
Console.WriteLine(derivedType.IsAssignableTo(baseType)); // True
Console.WriteLine(baseType.IsAssignableTo(derivedType)); // False
```

## Best Practices

### Schema Design

1. **Be specific with types**: Use precise types rather than `Any` when possible
2. **Document parameters**: Always provide descriptions for complex parameters
3. **Version your schemas**: Use semantic versioning for state migrations
4. **Test compatibility**: Validate schema changes with existing data

### Migration Strategy

1. **Backward compatibility**: Maintain compatibility within major versions
2. **Incremental migrations**: Break large changes into smaller, manageable steps
3. **Rollback support**: Ensure migrations can be reversed if needed
4. **Testing**: Test migrations with real data before production

### Performance Considerations

1. **Lazy validation**: Validate schemas only when needed
2. **Cache results**: Cache validation results for repeated checks
3. **Batch operations**: Group related validations together
4. **Async validation**: Use async validation for large datasets

## Troubleshooting

### Common Issues

**Type Inference Not Working**
* Ensure source nodes implement `ITypedSchemaNode`
* Check that parameter names match exactly (case-insensitive)
* Verify that nodes are properly connected in the graph

**Migration Failures**
* Check version compatibility before migration
* Ensure all required migrations are registered
* Validate serialized state format before migration

**Validation Errors**
* Review schema definitions for required fields
* Check type compatibility across node boundaries
* Verify that all required inputs are provided

### Debug Tips

1. **Enable detailed logging**: Set log level to Debug for validation details
2. **Use graph visualization**: Inspect node connections and data flow
3. **Check migration paths**: Use `GetMigrationPath` to understand migration steps
4. **Validate incrementally**: Test individual components before full graph validation

## Concepts and Techniques

**Type Inference**: The process of automatically determining parameter types based on available schema information from connected nodes.

**Schema Validation**: The verification that data structures conform to defined schemas, ensuring type safety and data integrity.

**State Migration**: The process of transforming serialized state data between different versions to maintain compatibility.

**Compile-time Validation**: Early detection of potential issues during graph construction, before execution begins.

## See Also

* [Building a Graph](build-a-graph.md) - Learn how to create graphs with typed schemas
* [State Management](state.md) - Understand graph state and serialization
* [Error Handling and Resilience](error-handling-and-resilience.md) - Handle validation failures gracefully
* [Graph Inspection and Debugging](debug-and-inspection.md) - Debug schema and validation issues
