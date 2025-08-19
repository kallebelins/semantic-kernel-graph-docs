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

- **`KernelArguments`**: The underlying `KernelArguments` instance
- **`StateId`**: Unique identifier for this state instance
- **`Version`**: Current state version for compatibility control
- **`CreatedAt`**: Timestamp when the state was created
- **`LastModified`**: Timestamp of the last state modification
- **`ExecutionHistory`**: Read-only collection of execution steps
- **`ExecutionStepCount`**: Total number of recorded execution steps
- **`IsModified`**: Indicates whether the state has been modified since creation

#### Constructors

```csharp
// Create empty state
public GraphState()

// Create state with existing KernelArguments
public GraphState(KernelArguments kernelArguments)
```

**Example:**
```csharp
// Create state with existing arguments
var arguments = new KernelArguments
{
    ["input"] = "Hello World",
    ["timestamp"] = DateTimeOffset.UtcNow
};

var graphState = new GraphState(arguments);

// Access underlying arguments
var kernelArgs = graphState.KernelArguments;
```

#### State Access Methods

```csharp
// Get typed value
public T? GetValue<T>(string name)

// Try to get typed value
public bool TryGetValue<T>(string name, out T? value)

// Set value
public void SetValue(string name, object? value)

// Remove value
public bool RemoveValue(string name)

// Check if value exists
public bool ContainsValue(string name)

// Get all parameter names
public IEnumerable<string> GetParameterNames()
```

**Example:**
```csharp
// Set and retrieve values
graphState.SetValue("userName", "Alice");
graphState.SetValue("age", 30);

// Type-safe retrieval
var userName = graphState.GetValue<string>("userName"); // "Alice"
var age = graphState.GetValue<int>("age"); // 30

// Safe retrieval with TryGetValue
if (graphState.TryGetValue<string>("email", out var email))
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
// Get metadata value
public T? GetMetadata<T>(string key)

// Set metadata value
public void SetMetadata(string key, object value)

// Remove metadata value
public bool RemoveMetadata(string key)
```

**Example:**
```csharp
// Store metadata
graphState.SetMetadata("source", "user_input");
graphState.SetMetadata("priority", "high");

// Retrieve metadata
var source = graphState.GetMetadata<string>("source"); // "user_input"
var priority = graphState.GetMetadata<string>("priority"); // "high"
```

#### ISerializableState Implementation

```csharp
// Serialize state
public string Serialize(SerializationOptions? options = null)

// Validate integrity
public ValidationResult ValidateIntegrity()

// Create checksum
public string CreateChecksum()
```

**Example:**
```csharp
// Serialize with default options
var serialized = graphState.Serialize();

// Serialize with custom options
var options = new SerializationOptions
{
    Indented = true,
    EnableCompression = true,
    IncludeMetadata = true
};
var verboseSerialized = graphState.Serialize(options);

// Validate integrity
var validation = graphState.ValidateIntegrity();
if (!validation.IsValid)
{
    foreach (var error in validation.Errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}

// Create checksum for integrity verification
var checksum = graphState.CreateChecksum();
```

### ISerializableState

Interface that defines standard methods for state serialization with version control and integrity checks.

#### Interface Methods

```csharp
// Get current version
StateVersion Version { get; }

// Get unique identifier
string StateId { get; }

// Get creation timestamp
DateTimeOffset CreatedAt { get; }

// Get last modification timestamp
DateTimeOffset LastModified { get; }

// Serialize state
string Serialize(SerializationOptions? options = null);

// Validate integrity
ValidationResult ValidateIntegrity();

// Create checksum
string CreateChecksum();
```

### SerializationOptions

Configurable options for controlling serialization behavior.

#### Properties

- **`Indented`**: Whether to use indented formatting
- **`EnableCompression`**: Whether to enable compression for large states
- **`IncludeMetadata`**: Whether to include metadata in serialization
- **`IncludeExecutionHistory`**: Whether to include execution history
- **`CompressionLevel`**: Compression level to use
- **`JsonOptions`**: Custom JSON serializer options
- **`ValidateIntegrity`**: Whether to validate integrity after serialization

#### Factory Methods

```csharp
// Default options
public static SerializationOptions Default => new();

// Compact options (no indentation, compression enabled)
public static SerializationOptions Compact => new()
{
    Indented = false,
    EnableCompression = true,
    IncludeMetadata = false,
    IncludeExecutionHistory = false
};

// Verbose options (indented, all metadata included)
public static SerializationOptions Verbose => new()
{
    Indented = true,
    EnableCompression = false,
    IncludeMetadata = true,
    IncludeExecutionHistory = true,
    ValidateIntegrity = true
};
```

**Example:**
```csharp
// Use predefined options
var compactOptions = SerializationOptions.Compact;
var verboseOptions = SerializationOptions.Verbose;

// Create custom options
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

- **`Major`**: Major version number
- **`Minor`**: Minor version number
- **`Patch`**: Patch version number
- **`IsCompatible`**: Indicates if this version is compatible with the current version
- **`RequiresMigration`**: Indicates if this version requires migration

#### Constants

```csharp
// Current state version
public static readonly StateVersion Current = new(1, 1, 0);

// Minimum supported version for compatibility
public static readonly StateVersion MinimumSupported = new(1, 0, 0);
```

#### Constructors

```csharp
public StateVersion(int major, int minor, int patch)
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
// Parse version string
public static StateVersion Parse(string version)

// Try to parse version string
public static bool TryParse(string? version, out StateVersion result)
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
// Serialize state with options
public static string SerializeState(GraphState state, bool indented = false, 
    bool enableCompression = true, bool useCache = true)

// Serialize state with metrics
public static string SerializeState(GraphState state, bool indented, 
    bool enableCompression, bool useCache, out SerializationMetrics metrics)

// Deserialize state
public static GraphState DeserializeState(string serializedData)
```

**Example:**
```csharp
// Basic serialization
var serialized = StateHelpers.SerializeState(graphState);

// Serialization with metrics
var serialized = StateHelpers.SerializeState(graphState, 
    indented: true, 
    enableCompression: true, 
    useCache: true, 
    out var metrics);

Console.WriteLine($"Serialization took: {metrics.Duration}");
Console.WriteLine($"Compression ratio: {metrics.CompressionRatio:P2}");

// Deserialization
var restoredState = StateHelpers.DeserializeState(serialized);
```

#### State Management Methods

```csharp
// Clone state
public static GraphState CloneState(GraphState state)

// Merge states with policy
public static GraphState MergeStates(GraphState baseState, GraphState overlayState, 
    StateMergeConflictPolicy policy)

// Merge states with configuration
public static GraphState MergeStates(GraphState baseState, GraphState overlayState, 
    StateMergeConfiguration configuration)

// Merge states with conflict detection
public static StateMergeResult MergeStatesWithConflictDetection(
    GraphState baseState, GraphState overlayState, 
    StateMergeConfiguration configuration, bool detectConflicts = true)
```

**Example:**
```csharp
// Clone state
var clonedState = StateHelpers.CloneState(graphState);

// Simple merge
var mergedState = StateHelpers.MergeStates(baseState, overlayState, 
    StateMergeConflictPolicy.PreferSecond);

// Advanced merge with configuration
var config = new StateMergeConfiguration
{
    DefaultPolicy = StateMergeConflictPolicy.Reduce
};
config.SetKeyPolicy("counters", StateMergeConflictPolicy.Reduce);

var mergedState = StateHelpers.MergeStates(baseState, overlayState, config);

// Merge with conflict detection
var mergeResult = StateHelpers.MergeStatesWithConflictDetection(
    baseState, overlayState, config, detectConflicts: true);

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
// Validate required parameters
public static IList<string> ValidateRequiredParameters(GraphState state, 
    IEnumerable<string> requiredParameters)

// Validate parameter types
public static IList<string> ValidateParameterTypes(GraphState state, 
    IDictionary<string, Type> typeConstraints)
```

**Example:**
```csharp
// Validate required parameters
var required = new[] { "userName", "email", "age" };
var missing = StateHelpers.ValidateRequiredParameters(graphState, required);

if (missing.Count > 0)
{
    Console.WriteLine($"Missing required parameters: {string.Join(", ", missing)}");
}

// Validate parameter types
var typeConstraints = new Dictionary<string, Type>
{
    ["userName"] = typeof(string),
    ["age"] = typeof(int),
    ["isActive"] = typeof(bool)
};

var violations = StateHelpers.ValidateParameterTypes(graphState, typeConstraints);
if (violations.Count > 0)
{
    foreach (var violation in violations)
    {
        Console.WriteLine($"Type violation: {violation}");
    }
}
```

#### Transaction Methods

```csharp
// Begin transaction
public static string BeginTransaction(GraphState state)

// Rollback transaction
public static GraphState RollbackTransaction(GraphState state, string transactionId)

// Commit transaction
public static void CommitTransaction(GraphState state, string transactionId)
```

**Example:**
```csharp
// Start transaction
var transactionId = StateHelpers.BeginTransaction(graphState);

try
{
    // Make changes
    graphState.SetValue("tempValue", "will be rolled back");
    
    // Validate changes
    if (someCondition)
    {
        // Commit transaction
        StateHelpers.CommitTransaction(graphState, transactionId);
    }
    else
    {
        // Rollback transaction
        var rolledBackState = StateHelpers.RollbackTransaction(graphState, transactionId);
        graphState = rolledBackState;
    }
}
catch (Exception)
{
    // Rollback on error
    var rolledBackState = StateHelpers.RollbackTransaction(graphState, transactionId);
    graphState = rolledBackState;
}
```

#### Checkpoint Methods

```csharp
// Create checkpoint
public static string CreateCheckpoint(GraphState state, string checkpointName)

// Restore checkpoint
public static GraphState RestoreCheckpoint(GraphState state, string checkpointId)
```

**Example:**
```csharp
// Create checkpoint
var checkpointId = StateHelpers.CreateCheckpoint(graphState, "before_processing");

// Make changes
graphState.SetValue("processed", true);

// Restore checkpoint if needed
if (needToRollback)
{
    var restoredState = StateHelpers.RestoreCheckpoint(graphState, checkpointId);
    graphState = restoredState;
}
```

#### Compression Methods

```csharp
// Get compression statistics
public static CompressionStats GetCompressionStats(string data)

// Get adaptive compression threshold
public static int GetAdaptiveCompressionThreshold()

// Reset adaptive compression
public static void ResetAdaptiveCompression()

// Get adaptive compression state
public static AdaptiveCompressionState GetAdaptiveCompressionState()
```

**Example:**
```csharp
// Check compression effectiveness
var stats = StateHelpers.GetCompressionStats(serializedData);
Console.WriteLine($"Original size: {stats.OriginalSizeBytes} bytes");
Console.WriteLine($"Compressed size: {stats.CompressedSizeBytes} bytes");
Console.WriteLine($"Compression ratio: {stats.CompressionRatio:P2}");

// Get adaptive compression information
var threshold = StateHelpers.GetAdaptiveCompressionThreshold();
var adaptiveState = StateHelpers.GetAdaptiveCompressionState();

Console.WriteLine($"Current threshold: {threshold} bytes");
Console.WriteLine($"Benefit rate: {adaptiveState.BenefitRate:P2}");
Console.WriteLine($"Average savings: {adaptiveState.AverageSavingsRatio:P2}");
```

## Usage Patterns

### Basic State Creation and Management

```csharp
// Create state with initial values
var arguments = new KernelArguments
{
    ["input"] = "Hello World",
    ["timestamp"] = DateTimeOffset.UtcNow,
    ["user"] = "Alice"
};

var graphState = new GraphState(arguments);

// Add metadata
graphState.SetMetadata("source", "user_input");
graphState.SetMetadata("priority", "normal");

// Access values
var input = graphState.GetValue<string>("input");
var user = graphState.GetValue<string>("user");

// Check state information
Console.WriteLine($"State ID: {graphState.StateId}");
Console.WriteLine($"Version: {graphState.Version}");
Console.WriteLine($"Created: {graphState.CreatedAt}");
Console.WriteLine($"Modified: {graphState.LastModified}");
```

### State Serialization and Persistence

```csharp
// Serialize with different options
var compactSerialized = graphState.Serialize(SerializationOptions.Compact);
var verboseSerialized = graphState.Serialize(SerializationOptions.Verbose);

// Custom serialization options
var customOptions = new SerializationOptions
{
    Indented = true,
    EnableCompression = true,
    IncludeMetadata = true,
    IncludeExecutionHistory = false,
    CompressionLevel = System.IO.Compression.CompressionLevel.Fastest
};

var customSerialized = graphState.Serialize(customOptions);

// Save to file
await File.WriteAllTextAsync("state.json", customSerialized);

// Load from file
var loadedData = await File.ReadAllTextAsync("state.json");
var restoredState = StateHelpers.DeserializeState(loadedData);
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

- **Serialization Caching**: Use `useCache: true` for repeated serialization of the same state
- **Compression**: Enable compression for large states to reduce storage and transfer costs
- **Adaptive Compression**: The system automatically adjusts compression thresholds based on observed benefits
- **Validation**: Use validation sparingly in production; consider caching validation results
- **Metadata**: Keep metadata lightweight to avoid serialization overhead

## Thread Safety

- **GraphState**: Thread-safe for concurrent reads; external synchronization required for concurrent writes
- **StateHelpers**: Static methods are thread-safe; use appropriate locking for shared state
- **Serialization**: Cached serialization is thread-safe with internal locking

## Error Handling

- **Validation**: Always validate state integrity after deserialization
- **Checksums**: Use checksums to detect state corruption
- **Transactions**: Implement proper error handling and rollback logic
- **Migration**: Handle version incompatibilities gracefully with migration logic

## See Also

- [State Management Guide](../concepts/state.md)
- [Checkpointing Guide](../how-to/checkpointing.md)
- [State Quickstart](../state-quickstart.md)
- [State Tutorial](../state-tutorial.md)
- [ConditionalEdge](conditional-edge.md)
- [StateMergeConfiguration](state-merge-configuration.md)
- [StateMergeConflictPolicy](state-merge-conflict-policy.md)
