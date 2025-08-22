# State and Serialization

The state management system in SemanticKernel.Graph provides a robust foundation for data flow, execution tracking, and persistence. This reference covers the core state classes, serialization capabilities, and utility methods for working with graph state.

## Overview

The state system is built around `GraphState`, which wraps `KernelArguments` with enhanced functionality for execution tracking, metadata management, validation, and serialization. The system supports versioning, integrity checks, compression, and advanced merge operations for parallel execution scenarios.

## Key Concepts

**GraphState**: Enhanced wrapper around `KernelArguments` that provides execution tracking, metadata, validation, and serialization capabilities.

**ISerializableState**: Interface that defines standard methods for state serialization with version control and integrity checks.

**StateVersion**: Semantic versioning system for state compatibility control and migration support.

**SerializationOptions**: Configurable options for controlling serialization behavior, compression, and metadata inclusion.

**StateHelpers**: Utility methods for common state operations including serialization, merging, validation, and checkpointing.

## Core Classes

### GraphState

The primary state container that wraps `KernelArguments` with additional graph-specific functionality.

#### Properties

* **`KernelArguments`**: The underlying `KernelArguments` instance
* **`StateId`**: Unique identifier for this state instance
* **`Version`**: Current state version for compatibility control
* **`CreatedAt`**: Timestamp when the state was created
* **`LastModified`**: Timestamp of the last state modification
* **`ExecutionHistory`**: Read-only collection of execution steps
* **`ExecutionStepCount`**: Total number of recorded execution steps
* **`IsModified`**: Indicates whether the state has been modified since creation

#### Constructors

```csharp
// GraphState provides convenient constructors for creating state instances.
public class GraphState
{
    // Create an empty state with default KernelArguments
    public GraphState() { /* implementation omitted */ }

    // Create state initialized from existing KernelArguments
    public GraphState(KernelArguments kernelArguments) { /* implementation omitted */ }
}
```

**Example:**
```csharp
// Create state with existing arguments and read values.
var arguments = new KernelArguments
{
    ["input"] = "Hello World",
    ["timestamp"] = DateTimeOffset.UtcNow
};

// Initialize GraphState with KernelArguments
var graphState = new GraphState(arguments);

// Safely access the underlying KernelArguments
var kernelArgs = graphState.KernelArguments;
var input = kernelArgs.GetValue<string>("input");
```

#### State Access Methods

```csharp
// Example method signatures for accessing values in GraphState.
public class GraphState
{
    public T? GetValue<T>(string name) { /* returns typed value or default */ throw null!; }
    public bool TryGetValue<T>(string name, out T? value) { value = default; return false; }
    public void SetValue(string name, object? value) { }
    public bool RemoveValue(string name) { return false; }
    public bool ContainsValue(string name) { return false; }
    public IEnumerable<string> GetParameterNames() { yield break; }
}
```

**Example:**
```csharp
// Set and retrieve values using the GraphState API.
graphState.SetValue("userName", "Alice");
graphState.SetValue("age", 30);

// Type-safe retrieval
var userName = graphState.GetValue<string>("userName"); // returns "Alice"
var age = graphState.GetValue<int>("age"); // returns 30

// Safe retrieval with TryGetValue
if (graphState.TryGetValue<string>("email", out var email) && email is not null)
{
    Console.WriteLine($"Email: {email}");
}

// Check existence
if (graphState.ContainsValue("userName"))
{
    Console.WriteLine("Username is set");
}
```

#### Metadata Methods

```csharp
// Metadata helpers on GraphState.
public class GraphState
{
    public T? GetMetadata<T>(string key) { /* returns typed metadata */ throw null!; }
    public void SetMetadata(string key, object value) { }
    public bool RemoveMetadata(string key) { return false; }
}
```

**Example:**
```csharp
// Store and retrieve metadata on the state.
graphState.SetMetadata("source", "user_input");
graphState.SetMetadata("priority", "high");

var source = graphState.GetMetadata<string>("source");
var priority = graphState.GetMetadata<string>("priority");
Console.WriteLine($"source={source}, priority={priority}");
```

#### ISerializableState Implementation

```csharp
// Typical ISerializableState implementation surface.
public interface ISerializableState
{
    StateVersion Version { get; }
    string StateId { get; }
    DateTimeOffset CreatedAt { get; }
    DateTimeOffset LastModified { get; }
    string Serialize(SerializationOptions? options = null);
    ValidationResult ValidateIntegrity();
    string CreateChecksum();
}
```

**Example:**
```csharp
// Serialize and validate state integrity.
var serialized = graphState.Serialize();

var options = new SerializationOptions
{
    Indented = true,
    EnableCompression = true,
    IncludeMetadata = true
};

var verboseSerialized = graphState.Serialize(options);

// Validate integrity and report any errors
var validation = graphState.ValidateIntegrity();
if (!validation.IsValid)
{
    foreach (var error in validation.Errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}

// Compute a checksum for later verification
var checksum = graphState.CreateChecksum();
```

### ISerializableState

Interface that defines standard methods for state serialization with version control and integrity checks.

#### Interface Methods

```csharp
// Example interface members for a serializable state.
public interface ISerializableState
{
    StateVersion Version { get; }
    string StateId { get; }
    DateTimeOffset CreatedAt { get; }
    DateTimeOffset LastModified { get; }
    string Serialize(SerializationOptions? options = null);
    ValidationResult ValidateIntegrity();
    string CreateChecksum();
}
```

### SerializationOptions

Configurable options for controlling serialization behavior.

#### Properties

* **`Indented`**: Whether to use indented formatting
* **`EnableCompression`**: Whether to enable compression for large states
* **`IncludeMetadata`**: Whether to include metadata in serialization
* **`IncludeExecutionHistory`**: Whether to include execution history
* **`CompressionLevel`**: Compression level to use
* **`JsonOptions`**: Custom JSON serializer options
* **`ValidateIntegrity`**: Whether to validate integrity after serialization

#### Factory Methods

```csharp
// Common factory presets for SerializationOptions.
public class SerializationOptions
{
    public bool Indented { get; set; }
    public bool EnableCompression { get; set; }
    public bool IncludeMetadata { get; set; }
    public bool IncludeExecutionHistory { get; set; }
    public System.IO.Compression.CompressionLevel? CompressionLevel { get; set; }
    public bool ValidateIntegrity { get; set; }

    public static SerializationOptions Default => new SerializationOptions();

    public static SerializationOptions Compact => new SerializationOptions
    {
        Indented = false,
        EnableCompression = true,
        IncludeMetadata = false,
        IncludeExecutionHistory = false
    };

    public static SerializationOptions Verbose => new SerializationOptions
    {
        Indented = true,
        EnableCompression = false,
        IncludeMetadata = true,
        IncludeExecutionHistory = true,
        ValidateIntegrity = true
    };
}
```

**Example:**
```csharp
// Select a predefined options preset or create a custom one.
var compactOptions = SerializationOptions.Compact;
var verboseOptions = SerializationOptions.Verbose;

var customOptions = new SerializationOptions
{
    Indented = true,
    EnableCompression = true,
    IncludeMetadata = true,
    IncludeExecutionHistory = false,
    CompressionLevel = System.IO.Compression.CompressionLevel.Fastest
};
```

### StateVersion

Represents the state version for compatibility control and migration.

#### Properties

* **`Major`**: Major version number
* **`Minor`**: Minor version number
* **`Patch`**: Patch version number
* **`IsCompatible`**: Indicates if this version is compatible with the current version
* **`RequiresMigration`**: Indicates if this version requires migration

#### Constants

```csharp
// Example version constants used by the state system.
public static class StateVersionConstants
{
    public static readonly StateVersion Current = new StateVersion(1, 1, 0);
    public static readonly StateVersion MinimumSupported = new StateVersion(1, 0, 0);
}
```

#### Constructors

```csharp
// Construct a state version instance.
public record StateVersion(int Major, int Minor, int Patch)
{
    public StateVersion(int major, int minor, int patch) : this(major, minor, patch) { }
    public bool IsCompatible => Major == StateVersionConstants.Current.Major;
    public bool RequiresMigration => this < StateVersionConstants.Current;
}
```

**Example:**
```csharp
// Create version
var version = new StateVersion(1, 2, 3);

// Check compatibility
var isCompatible = version.IsCompatible; // true if compatible
var needsMigration = version.RequiresMigration; // true if needs migration

// Compare versions
if (version < StateVersion.Current)
{
    Console.WriteLine("State version is older than current");
}
```

#### Static Methods

```csharp
// Parsing helpers for StateVersion.
public static class StateVersionParser
{
    public static StateVersion Parse(string version) =>
        TryParse(version, out var v) ? v : throw new FormatException("Invalid version format");

    public static bool TryParse(string? version, out StateVersion result)
    {
        result = default!;
        if (string.IsNullOrWhiteSpace(version)) return false;
        var parts = version.Split('.');
        if (parts.Length != 3) return false;
        if (int.TryParse(parts[0], out var major) && int.TryParse(parts[1], out var minor) && int.TryParse(parts[2], out var patch))
        {
            result = new StateVersion(major, minor, patch);
            return true;
        }
        return false;
    }
}
```

**Example:**
```csharp
// Parse version strings
var version = StateVersion.Parse("1.2.3");
Console.WriteLine($"Major: {version.Major}");    // 1
Console.WriteLine($"Minor: {version.Minor}");    // 2
Console.WriteLine($"Patch: {version.Patch}");    // 3

// Safe parsing
if (StateVersion.TryParse("invalid", out var parsedVersion))
{
    // Use parsed version
}
else
{
    Console.WriteLine("Invalid version format");
}
```

### StateHelpers

Utility methods for common state operations including serialization, merging, validation, and checkpointing.

#### Serialization Methods

```csharp
// Helpers for serializing and deserializing GraphState instances.
public static class StateHelpers
{
    public static string SerializeState(GraphState state, bool indented = false, bool enableCompression = true, bool useCache = true)
    {
        // Implementation delegates to GraphState.Serialize with a SerializationOptions instance.
        var options = new SerializationOptions { Indented = indented, EnableCompression = enableCompression };
        return state.Serialize(options);
    }

    public static string SerializeState(GraphState state, bool indented, bool enableCompression, bool useCache, out SerializationMetrics metrics)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = SerializeState(state, indented, enableCompression, useCache);
        sw.Stop();
        metrics = new SerializationMetrics { Duration = sw.Elapsed };
        return result;
    }

    public static GraphState DeserializeState(string serializedData)
    {
        // Delegates to GraphState deserialization logic (implementation omitted for brevity).
        throw new NotImplementedException();
    }
}
```

**Example:**
```csharp
// Basic serialization using helpers and optional metrics.
var serialized = StateHelpers.SerializeState(graphState);

var serializedWithMetrics = StateHelpers.SerializeState(graphState, indented: true, enableCompression: true, useCache: true, out var metrics);
Console.WriteLine($"Serialization took: {metrics.Duration}");

// Deserialization (might throw if unimplemented in docs)
// var restoredState = StateHelpers.DeserializeState(serialized);
```

#### State Management Methods

```csharp
// High-level state management helpers. Implementations should preserve immutability where appropriate.
public static class StateHelpers
{
    public static GraphState CloneState(GraphState state) => new GraphState(state.KernelArguments);

    public static GraphState MergeStates(GraphState baseState, GraphState overlayState, StateMergeConflictPolicy policy)
    {
        // Simplified merge: overlay takes precedence by default.
        var merged = CloneState(baseState);
        foreach (var name in overlayState.GetParameterNames())
        {
            if (overlayState.TryGetValue<object>(name, out var v)) merged.SetValue(name, v);
        }
        return merged;
    }

    public static GraphState MergeStates(GraphState baseState, GraphState overlayState, StateMergeConfiguration configuration) =>
        MergeStates(baseState, overlayState, configuration.DefaultPolicy);

    public static StateMergeResult MergeStatesWithConflictDetection(GraphState baseState, GraphState overlayState, StateMergeConfiguration configuration, bool detectConflicts = true)
    {
        // Returns a simple result indicating no conflicts in this example.
        return new StateMergeResult { HasConflicts = false };
    }
}
```

**Example:**
```csharp
// Clone and merge states using helpers.
var clonedState = StateHelpers.CloneState(graphState);
var mergedState = StateHelpers.MergeStates(baseState, overlayState, StateMergeConflictPolicy.PreferSecond);

// Using a configuration-based merge
var config = new StateMergeConfiguration { DefaultPolicy = StateMergeConflictPolicy.Reduce };
config.SetKeyPolicy("counters", StateMergeConflictPolicy.Reduce);
var advancedMergedState = StateHelpers.MergeStates(baseState, overlayState, config);

// Merge with conflict detection
var mergeResult = StateHelpers.MergeStatesWithConflictDetection(baseState, overlayState, config, detectConflicts: true);
if (mergeResult.HasConflicts)
{
    foreach (var conflict in mergeResult.Conflicts)
    {
        Console.WriteLine($"Conflict on '{conflict.Key}': {conflict.BaseValue} vs {conflict.OverlayValue}");
    }
}
```

#### Validation Methods

```csharp
// Validation helpers to check required parameters and enforce type constraints.
public static class StateHelpers
{
    public static IList<string> ValidateRequiredParameters(GraphState state, IEnumerable<string> requiredParameters)
    {
        var missing = new List<string>();
        foreach (var p in requiredParameters)
        {
            if (!state.ContainsValue(p)) missing.Add(p);
        }
        return missing;
    }

    public static IList<string> ValidateParameterTypes(GraphState state, IDictionary<string, Type> typeConstraints)
    {
        var violations = new List<string>();
        foreach (var kvp in typeConstraints)
        {
            if (state.TryGetValue<object>(kvp.Key, out var val) && val != null && !kvp.Value.IsInstanceOfType(val))
            {
                violations.Add($"{kvp.Key} expected {kvp.Value.Name} but got {val.GetType().Name}");
            }
        }
        return violations;
    }
}
```

**Example:**
```csharp
var required = new[] { "userName", "email", "age" };
var missing = StateHelpers.ValidateRequiredParameters(graphState, required);
if (missing.Count > 0)
{
    Console.WriteLine($"Missing required parameters: {string.Join(", ", missing)}");
}

var typeConstraints = new Dictionary<string, Type>
{
    ["userName"] = typeof(string),
    ["age"] = typeof(int),
    ["isActive"] = typeof(bool)
};

var violations = StateHelpers.ValidateParameterTypes(graphState, typeConstraints);
if (violations.Count > 0)
{
    foreach (var v in violations) Console.WriteLine($"Type violation: {v}");
}
```

#### Transaction Methods

```csharp
// Transaction helpers: begin, commit and rollback.
public static class StateHelpers
{
    public static string BeginTransaction(GraphState state) => Guid.NewGuid().ToString();
    public static GraphState RollbackTransaction(GraphState state, string transactionId) => CloneState(state);
    public static void CommitTransaction(GraphState state, string transactionId) { /* commit changes */ }
}
```

**Example:**
```csharp
var transactionId = StateHelpers.BeginTransaction(graphState);
try
{
    // Perform transient changes
    graphState.SetValue("tempValue", "will be rolled back");

    // Placeholder for domain validation
    var valid = true; // replace with real validation
    if (valid)
    {
        StateHelpers.CommitTransaction(graphState, transactionId);
    }
    else
    {
        graphState = StateHelpers.RollbackTransaction(graphState, transactionId);
    }
}
catch (Exception)
{
    graphState = StateHelpers.RollbackTransaction(graphState, transactionId);
}
```

#### Checkpoint Methods

```csharp
// Checkpoint helpers create and restore lightweight snapshots of state.
public static class StateHelpers
{
    public static string CreateCheckpoint(GraphState state, string checkpointName) => Guid.NewGuid().ToString();
    public static GraphState RestoreCheckpoint(GraphState state, string checkpointId) => CloneState(state);
}
```

**Example:**
```csharp
var checkpointId = StateHelpers.CreateCheckpoint(graphState, "before_processing");
graphState.SetValue("processed", true);
// If rollback needed:
// graphState = StateHelpers.RestoreCheckpoint(graphState, checkpointId);
```

#### Compression Methods

```csharp
// Compression-related helpers to measure and manage adaptive compression.
public static class StateHelpers
{
    public static CompressionStats GetCompressionStats(string data) => new CompressionStats { OriginalSizeBytes = data.Length, CompressedSizeBytes = data.Length };
    public static int GetAdaptiveCompressionThreshold() => 1024;
    public static void ResetAdaptiveCompression() { }
    public static AdaptiveCompressionState GetAdaptiveCompressionState() => new AdaptiveCompressionState();
}
```

**Example:**
```csharp
var stats = StateHelpers.GetCompressionStats(serializedData);
Console.WriteLine($"Original size: {stats.OriginalSizeBytes} bytes");
Console.WriteLine($"Compressed size: {stats.CompressedSizeBytes} bytes");
// Adaptive compression info
var threshold = StateHelpers.GetAdaptiveCompressionThreshold();
var adaptiveState = StateHelpers.GetAdaptiveCompressionState();
```

## Usage Patterns

### Basic State Creation and Management

```csharp
// Example: Create and inspect a GraphState instance.
var arguments = new KernelArguments
{
    ["input"] = "Hello World",
    ["timestamp"] = DateTimeOffset.UtcNow,
    ["user"] = "Alice"
};

var graphState = new GraphState(arguments);
graphState.SetMetadata("source", "user_input");

Console.WriteLine($"State ID: {graphState.StateId}");
Console.WriteLine($"Version: {graphState.Version}");
```

### State Serialization and Persistence

```csharp
// Serialize with built-in presets or a custom options instance.
var compactSerialized = graphState.Serialize(SerializationOptions.Compact);
var verboseSerialized = graphState.Serialize(SerializationOptions.Verbose);

var customOptions = new SerializationOptions
{
    Indented = true,
    EnableCompression = true,
    IncludeMetadata = true,
    IncludeExecutionHistory = false,
    CompressionLevel = System.IO.Compression.CompressionLevel.Fastest
};

var customSerialized = graphState.Serialize(customOptions);

// Save and load example (async context required)
// await File.WriteAllTextAsync("state.json", customSerialized);
// var loadedData = await File.ReadAllTextAsync("state.json");
// var restoredState = StateHelpers.DeserializeState(loadedData);
```

### State Merging and Conflict Resolution

```csharp
// Create states to merge
var baseState = new GraphState(new KernelArguments
{
    ["user"] = "Alice",
    ["count"] = 5,
    ["settings"] = new Dictionary<string, object> { ["theme"] = "dark" }
});

var overlayState = new GraphState(new KernelArguments
{
    ["count"] = 10,
    ["settings"] = new Dictionary<string, object> { ["language"] = "en" }
});

// Simple merge (overlay takes precedence)
var mergedState = StateHelpers.MergeStates(baseState, overlayState, 
    StateMergeConflictPolicy.PreferSecond);

// Advanced merge with configuration
var config = new StateMergeConfiguration
{
    DefaultPolicy = StateMergeConflictPolicy.PreferSecond
};

// Configure specific policies
config.SetKeyPolicy("count", StateMergeConflictPolicy.Reduce);
config.SetTypePolicy(typeof(Dictionary<string, object>), StateMergeConflictPolicy.Reduce);

// Custom merger for dictionaries
config.SetCustomKeyMerger("settings", (baseVal, overlayVal) =>
{
    if (baseVal is Dictionary<string, object> baseDict && 
        overlayVal is Dictionary<string, object> overlayDict)
    {
        var merged = new Dictionary<string, object>(baseDict);
        foreach (var kvp in overlayDict)
        {
            merged[kvp.Key] = kvp.Value;
        }
        return merged;
    }
    return overlayVal;
});

var advancedMergedState = StateHelpers.MergeStates(baseState, overlayState, config);
```

### State Validation and Integrity

```csharp
// Validate state integrity
var validation = graphState.ValidateIntegrity();
if (!validation.IsValid)
{
    Console.WriteLine("State validation failed:");
    foreach (var error in validation.Errors)
    {
        Console.WriteLine($"  Error: {error}");
    }
    foreach (var warning in validation.Warnings)
    {
        Console.WriteLine($"  Warning: {warning}");
    }
}

// Create and verify checksum
var originalChecksum = graphState.CreateChecksum();

// Make changes
graphState.SetValue("modified", true);

// Verify integrity
var newChecksum = graphState.CreateChecksum();
if (originalChecksum != newChecksum)
{
    Console.WriteLine("State has been modified");
}

// Validate required parameters
var required = new[] { "user", "email", "age" };
var missing = StateHelpers.ValidateRequiredParameters(graphState, required);

if (missing.Count > 0)
{
    throw new InvalidOperationException(
        $"Missing required parameters: {string.Join(", ", missing)}");
}

// Validate parameter types
var typeConstraints = new Dictionary<string, Type>
{
    ["user"] = typeof(string),
    ["age"] = typeof(int),
    ["isActive"] = typeof(bool)
};

var violations = StateHelpers.ValidateParameterTypes(graphState, typeConstraints);
if (violations.Count > 0)
{
    throw new InvalidOperationException(
        $"Type violations: {string.Join("; ", violations)}");
}
```

### State Transactions and Checkpointing

```csharp
// Create checkpoint
var checkpointId = StateHelpers.CreateCheckpoint(graphState, "initial_state");

// Begin transaction
var transactionId = StateHelpers.BeginTransaction(graphState);

try
{
    // Make changes within transaction
    graphState.SetValue("temp1", "value1");
    graphState.SetValue("temp2", "value2");
    
    // Validate changes
    if (ValidateChanges(graphState))
    {
        // Commit transaction
        StateHelpers.CommitTransaction(graphState, transactionId);
        Console.WriteLine("Transaction committed successfully");
    }
    else
    {
        // Rollback transaction
        var rolledBackState = StateHelpers.RollbackTransaction(graphState, transactionId);
        graphState = rolledBackState;
        Console.WriteLine("Transaction rolled back due to validation failure");
    }
}
catch (Exception ex)
{
    // Rollback on error
    var rolledBackState = StateHelpers.RollbackTransaction(graphState, transactionId);
    graphState = rolledBackState;
    Console.WriteLine($"Transaction rolled back due to error: {ex.Message}");
}

// Restore checkpoint if needed
if (needToRestore)
{
    var restoredState = StateHelpers.RestoreCheckpoint(graphState, checkpointId);
    graphState = restoredState;
    Console.WriteLine("State restored from checkpoint");
}
```

## Performance Considerations

* **Serialization Caching**: Use `useCache: true` for repeated serialization of the same state
* **Compression**: Enable compression for large states to reduce storage and transfer costs
* **Adaptive Compression**: The system automatically adjusts compression thresholds based on observed benefits
* **Validation**: Use validation sparingly in production; consider caching validation results
* **Metadata**: Keep metadata lightweight to avoid serialization overhead

## Thread Safety

* **GraphState**: Thread-safe for concurrent reads; external synchronization required for concurrent writes
* **StateHelpers**: Static methods are thread-safe; use appropriate locking for shared state
* **Serialization**: Cached serialization is thread-safe with internal locking

## Error Handling

* **Validation**: Always validate state integrity after deserialization
* **Checksums**: Use checksums to detect state corruption
* **Transactions**: Implement proper error handling and rollback logic
* **Migration**: Handle version incompatibilities gracefully with migration logic

## See Also

* [State Management Guide](../concepts/state.md)
* [Checkpointing Guide](../how-to/checkpointing.md)
* [State Quickstart](../state-quickstart.md)
* [State Tutorial](../state-tutorial.md)
* [ConditionalEdge](conditional-edge.md)
* [StateMergeConfiguration](state-merge-configuration.md)
* [StateMergeConflictPolicy](state-merge-conflict-policy.md)
