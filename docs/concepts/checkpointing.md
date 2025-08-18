# Checkpointing and Recovery

Checkpointing and recovery are essential features in SemanticKernel.Graph that enable fault tolerance, execution resumption, and state persistence. This guide explains how to use `StateHelpers`, manage state versions, implement compression, ensure integrity, and configure recovery policies.

## What You'll Learn

- How to save and restore graph execution state using `StateHelpers`
- Managing state versions and compatibility across different releases
- Implementing compression for efficient storage and transmission
- Ensuring data integrity and validation during checkpoint operations
- Configuring recovery policies and automatic cleanup
- Building fault-tolerant workflows with automatic recovery

## Concepts and Techniques

**Checkpointing**: The process of saving the current state of a graph execution at specific points, enabling recovery and resumption from any saved state.

**State Persistence**: `StateHelpers` provides utilities for serializing and deserializing `GraphState` objects with compression and integrity validation.

**Recovery Mechanisms**: Automatic and manual recovery from checkpoints with consistency validation and risk assessment.

**State Versioning**: Semantic versioning system that ensures compatibility and enables migration between different state formats.

**Compression and Integrity**: Built-in compression for storage efficiency and checksum validation for data integrity.

## Prerequisites

- [State Management](state.md) guide completed
- Basic understanding of `GraphState` and `KernelArguments`
- Graph memory service configured (required for checkpointing)
- Checkpoint support enabled in your kernel

## Core Checkpointing Components

### StateHelpers: Core Checkpoint Utilities

`StateHelpers` provides the foundation for all checkpointing operations:

```csharp
using SemanticKernel.Graph.State;

// Save state to a checkpoint
var checkpointId = StateHelpers.SaveCheckpoint(graphState, "my-checkpoint");

// Restore state from a checkpoint
var restoredState = StateHelpers.RestoreCheckpoint(graphState, checkpointId);

// Serialize state for storage
var serializedState = StateHelpers.SerializeState(graphState);

// Deserialize state from storage
var deserializedState = StateHelpers.DeserializeState(serializedState);
```

### CheckpointManager: Centralized Checkpoint Management

`CheckpointManager` handles storage, retrieval, and lifecycle management:

```csharp
using SemanticKernel.Graph.Core;

var checkpointManager = kernel.Services.GetRequiredService<ICheckpointManager>();

// Create a new checkpoint
var checkpoint = await checkpointManager.CreateCheckpointAsync(
    executionId: "exec-123",
    nodeId: "process-node",
    state: graphState,
    metadata: new Dictionary<string, object>
    {
        ["step"] = "data-processing",
        ["timestamp"] = DateTimeOffset.UtcNow
    }
);

// Retrieve a checkpoint
var retrievedCheckpoint = await checkpointManager.GetCheckpointAsync(checkpointId);

// List checkpoints for an execution
var checkpoints = await checkpointManager.ListCheckpointsAsync("exec-123", limit: 10);
```

## State Versioning and Compatibility

### StateVersion: Semantic Versioning

`StateVersion` ensures compatibility across different releases:

```csharp
using SemanticKernel.Graph.State;

// Current version information
var currentVersion = StateVersion.Current;           // "1.1.0"
var minSupported = StateVersion.MinimumSupported;    // "1.0.0"

// Check version compatibility
var stateVersion = graphState.Version;
var isCompatible = stateVersion.IsCompatible;        // Compatible with current version
var requiresMigration = stateVersion.RequiresMigration; // Needs migration

// Version comparison and parsing
var version = StateVersion.Parse("1.2.3");
if (version < StateVersion.Current)
{
    Console.WriteLine("State version is older than current");
}
```

### Version Migration

Automatic migration handles state format changes:

```csharp
// During deserialization, migration is automatic
var deserializedResult = SerializableStateFactory.Deserialize<GraphState>(
    serializedData,
    json => JsonSerializer.Deserialize<GraphState>(json)
);

if (deserializedResult.WasMigrated)
{
    Console.WriteLine($"Migrated from version {deserializedResult.OriginalVersion}");
    Console.WriteLine($"Migrated to version {deserializedResult.State.Version}");
}
```

## State Compression and Storage

### Compression Options

Configure compression for storage efficiency:

```csharp
// Serialization options with compression
var options = new SerializationOptions
{
    EnableCompression = true,           // Enable compression
    CompressionThreshold = 1024,        // Compress if > 1KB
    IncludeMetadata = true,             // Include state metadata
    IncludeExecutionHistory = false,    // Exclude execution history for storage
    ValidateIntegrity = true            // Validate before serialization
};

var compressedState = graphState.Serialize(options);
```

### Storage Optimization

Optimize storage with selective serialization:

```csharp
// Minimal storage options
var storageOptions = new SerializationOptions
{
    EnableCompression = true,
    IncludeMetadata = false,            // Exclude metadata for storage
    IncludeExecutionHistory = false,    // Exclude execution history
    ValidateIntegrity = true
};

// Full state options for debugging
var debugOptions = new SerializationOptions
{
    EnableCompression = false,          // No compression for debugging
    IncludeMetadata = true,             // Include all metadata
    IncludeExecutionHistory = true,     // Include execution history
    Indented = true                     // Human-readable format
};
```

## Data Integrity and Validation

### Integrity Validation

Ensure checkpoint data integrity:

```csharp
// Validate state before checkpointing
var validationResult = graphState.ValidateIntegrity();
if (!validationResult.IsValid)
{
    Console.WriteLine($"State validation failed: {validationResult.ErrorCount} errors");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"Error: {error.Message}");
    }
    return;
}

// Create checksum for integrity verification
var checksum = graphState.CreateChecksum();
Console.WriteLine($"State checksum: {checksum}");
```

### Checkpoint Validation

Validate checkpoints during restoration:

```csharp
// Validate checkpoint integrity
var checkpointValidation = await checkpointManager.ValidateCheckpointAsync(checkpointId);
if (!checkpointValidation.IsValid)
{
    Console.WriteLine($"Checkpoint validation failed: {checkpointValidation.ErrorMessage}");
    return;
}

// Check checkpoint metadata
Console.WriteLine($"Checkpoint size: {checkpointValidation.SizeInBytes:N0} bytes");
Console.WriteLine($"Compressed: {checkpointValidation.IsCompressed}");
Console.WriteLine($"Checksum: {checkpointValidation.Checksum}");
```

### Consistency Validation

Ensure restored state consistency:

```csharp
// Validate restored state consistency
var consistencyResult = await checkpointManager.ValidateRestoredStateConsistencyAsync(
    restoredState,
    recoveryContext,
    cancellationToken
);

if (!consistencyResult.IsConsistent)
{
    Console.WriteLine($"Consistency validation failed: Score {consistencyResult.ConsistencyScore:P1}");
    foreach (var issue in consistencyResult.Issues)
    {
        Console.WriteLine($"Issue: {issue.Description} (Severity: {issue.Severity})");
    }
}
```

## Checkpointing Graph Executor

### Basic Configuration

Configure checkpointing behavior:

```csharp
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;

// Enable checkpoint support in kernel
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddGraphMemory()  // Required for checkpointing
    .AddCheckpointSupport(options =>
    {
        options.EnableCompression = true;
        options.MaxCacheSize = 100;
    })
    .Build();

// Create checkpointing executor
var executorFactory = kernel.Services.GetRequiredService<ICheckpointingGraphExecutorFactory>();
var executor = executorFactory.CreateExecutor("my-graph", new CheckpointingOptions
{
    CheckpointInterval = 3,  // Create checkpoint every 3 nodes
    CreateInitialCheckpoint = true,
    CreateFinalCheckpoint = true,
    CreateErrorCheckpoints = true
});
```

### Advanced Checkpointing Options

Fine-tune checkpointing behavior:

```csharp
var checkpointingOptions = new CheckpointingOptions
{
    // Interval-based checkpointing
    CheckpointInterval = 5,  // Every 5 nodes
    CheckpointTimeInterval = TimeSpan.FromMinutes(10),  // Or every 10 minutes
    
    // Critical checkpointing
    CreateInitialCheckpoint = true,
    CreateFinalCheckpoint = true,
    CreateErrorCheckpoints = true,
    
    // Critical nodes that always trigger checkpoints
    CriticalNodes = new HashSet<string> { "process", "validate", "output" },
    
    // Automatic cleanup
    EnableAutoCleanup = true,
    FailOnCheckpointError = false,  // Continue execution even if checkpointing fails
    
    // Retention policy
    RetentionPolicy = new CheckpointRetentionPolicy
    {
        MaxAge = TimeSpan.FromHours(24),
        MaxCheckpointsPerExecution = 50,
        MaxTotalStorageBytes = 100 * 1024 * 1024,  // 100MB
        KeepCriticalCheckpoints = true
    }
};
```

## Recovery and Restoration

### Manual Recovery

Implement manual recovery from checkpoints:

```csharp
try
{
    var result = await executor.ExecuteAsync(kernel, arguments);
    Console.WriteLine("Execution completed successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"Execution failed: {ex.Message}");
    
    // Find available checkpoints
    var executionId = executor.LastExecutionId ?? arguments.GetOrCreateGraphState().StateId;
    var checkpoints = await executor.GetExecutionCheckpointsAsync(executionId);
    
    if (checkpoints.Count > 0)
    {
        var latestCheckpoint = checkpoints.First();
        Console.WriteLine($"Latest checkpoint: {latestCheckpoint.CheckpointId}");
        
        // Resume from checkpoint
        var recoveredResult = await executor.ResumeFromCheckpointAsync(
            latestCheckpoint.CheckpointId, 
            kernel
        );
        
        Console.WriteLine($"Recovery successful: {recoveredResult.GetValue<object>()}");
    }
}
```

### Automatic Recovery

Enable automatic recovery with `GraphRecoveryService`:

```csharp
using SemanticKernel.Graph.Core;

var recoveryService = kernel.Services.GetRequiredService<IGraphRecoveryService>();

// Configure recovery options
var recoveryOptions = new RecoveryOptions
{
    MaxRecoveryAttempts = 3,
    EnableAutomaticRecovery = true,
    RecoveryTimeout = TimeSpan.FromMinutes(5),
    PreferredRecoveryStrategy = RecoveryStrategy.CheckpointRestore
};

// Attempt automatic recovery
var recoveryResult = await recoveryService.AttemptRecoveryAsync(
    failureContext,
    kernel,
    recoveryOptions,
    cancellationToken
);

if (recoveryResult.IsSuccessful)
{
    Console.WriteLine($"Recovery successful using {recoveryResult.RecoveryStrategy}");
    Console.WriteLine($"Recovery time: {recoveryResult.RecoveryDuration}");
}
else
{
    Console.WriteLine($"Recovery failed: {recoveryResult.Reason}");
}
```

## Retention and Cleanup Policies

### Retention Policy Configuration

Configure automatic cleanup behavior:

```csharp
var retentionPolicy = new CheckpointRetentionPolicy
{
    MaxAge = TimeSpan.FromDays(7),           // Keep for 7 days
    MaxCheckpointsPerExecution = 100,        // Max 100 per execution
    MaxTotalStorageBytes = 1024 * 1024 * 1024,  // 1GB total storage
    KeepCriticalCheckpoints = true,          // Always keep critical checkpoints
    CriticalCheckpointInterval = 10          // Critical checkpoints every 10 regular ones
};
```

### Cleanup Service Configuration

Configure the automatic cleanup service:

```csharp
using SemanticKernel.Graph.Core;

var cleanupOptions = new CheckpointCleanupOptions
{
    CleanupInterval = TimeSpan.FromHours(1),  // Run cleanup every hour
    EnableAdvancedCleanup = true,
    MaxTotalStorageBytes = 1024 * 1024 * 1024,  // 1GB limit
    AuditRetentionPeriod = TimeSpan.FromDays(30),  // Keep audit logs for 30 days
    EnableDetailedLogging = true
};

// Configure cleanup with retention policy
cleanupOptions.WithRetentionPolicy(
    maxAge: TimeSpan.FromDays(7),
    maxCheckpointsPerExecution: 100,
    maxTotalStorage: 1024 * 1024 * 1024
);
```

## Distributed Backup and Storage

### Backup Configuration

Enable distributed backup for critical checkpoints:

```csharp
var backupOptions = new CheckpointBackupOptions
{
    EnableDistributedBackup = true,
    BackupLocations = new[]
    {
        "https://backup1.example.com/checkpoints",
        "https://backup2.example.com/checkpoints"
    },
    BackupInterval = TimeSpan.FromMinutes(30),
    BackupRetentionPeriod = TimeSpan.FromDays(90),
    EnableCompression = true,
    EnableEncryption = false  // Enable for sensitive data
};

// Configure checkpointing with backup
var checkpointingOptions = new CheckpointingOptions
{
    EnableDistributedBackup = true,
    BackupOptions = backupOptions
};
```

### Backup Operations

Manage backup operations:

```csharp
var checkpointManager = kernel.Services.GetRequiredService<ICheckpointManager>();

// Create backup of critical checkpoint
await checkpointManager.CreateBackupAsync(checkpointId, backupOptions);

// List backup locations
var backupLocations = await checkpointManager.GetBackupLocationsAsync(checkpointId);

// Restore from backup if primary is corrupted
var backupCheckpoint = await checkpointManager.RestoreFromBackupAsync(checkpointId, backupLocation);
```

## Monitoring and Observability

### Checkpoint Statistics

Monitor checkpoint performance and usage:

```csharp
// Get checkpoint statistics
var stats = await checkpointManager.GetStatisticsAsync();

Console.WriteLine($"Total checkpoints: {stats.TotalCheckpoints}");
Console.WriteLine($"Total storage used: {stats.TotalStorageBytes / 1024 / 1024:F1} MB");
Console.WriteLine($"Average checkpoint size: {stats.AverageCheckpointSizeBytes / 1024:F1} KB");
Console.WriteLine($"Compression ratio: {stats.AverageCompressionRatio:P1}");
Console.WriteLine($"Cache hit rate: {stats.CacheHitRate:P1}");
```

### Performance Monitoring

Monitor checkpointing performance:

```csharp
// Get execution checkpoints with performance data
var checkpoints = await executor.GetExecutionCheckpointsAsync(executionId);

foreach (var checkpoint in checkpoints)
{
    Console.WriteLine($"Checkpoint: {checkpoint.CheckpointId}");
    Console.WriteLine($"  Node: {checkpoint.NodeId}");
    Console.WriteLine($"  Size: {checkpoint.SizeInBytes / 1024:F1} KB");
    Console.WriteLine($"  Compressed: {checkpoint.IsCompressed}");
    Console.WriteLine($"  Created: {checkpoint.CreatedAt:HH:mm:ss}");
    Console.WriteLine($"  Sequence: {checkpoint.SequenceNumber}");
}
```

## Advanced Patterns

### Conditional Checkpointing

Create checkpoints based on business logic:

```csharp
public class ConditionalCheckpointNode : IGraphNode
{
    public async Task<FunctionResult> ExecuteAsync(GraphState state)
    {
        // Check if checkpoint is needed
        if (ShouldCreateCheckpoint(state))
        {
            var checkpointId = StateHelpers.SaveCheckpoint(state, "conditional-checkpoint");
            state.SetValue("lastCheckpointId", checkpointId);
            state.SetValue("checkpointReason", "business-rule-triggered");
        }
        
        // Continue with normal execution
        return await ProcessData(state);
    }
    
    private bool ShouldCreateCheckpoint(GraphState state)
    {
        var dataSize = state.GetValue<int>("dataSize", 0);
        var processingTime = state.GetValue<TimeSpan>("processingTime", TimeSpan.Zero);
        var errorCount = state.GetValue<int>("errorCount", 0);
        
        // Create checkpoint if data is large, processing is slow, or errors occurred
        return dataSize > 1000 || processingTime > TimeSpan.FromMinutes(5) || errorCount > 0;
    }
}
```

### Checkpoint Chaining

Create linked checkpoints for complex workflows:

```csharp
public class CheckpointChainingExample
{
    public async Task RunChainedCheckpointsAsync()
    {
        var kernel = CreateKernelWithCheckpointing();
        var executor = CreateCheckpointingExecutor(kernel);
        
        var state = new KernelArguments
        {
            ["workflow"] = "data-pipeline",
            ["stage"] = "initialization"
        };
        
        // Stage 1: Initialization
        state.SetValue("stage", "initialization");
        var stage1Checkpoint = StateHelpers.SaveCheckpoint(state.ToGraphState(), "stage1-init");
        
        // Stage 2: Data Processing
        state.SetValue("stage", "processing");
        state.SetValue("previousCheckpoint", stage1Checkpoint);
        var stage2Checkpoint = StateHelpers.SaveCheckpoint(state.ToGraphState(), "stage2-processing");
        
        // Stage 3: Validation
        state.SetValue("stage", "validation");
        state.SetValue("previousCheckpoint", stage2Checkpoint);
        var stage3Checkpoint = StateHelpers.SaveCheckpoint(state.ToGraphState(), "stage3-validation");
        
        // Chain checkpoints for rollback capability
        state.SetValue("checkpointChain", new[]
        {
            stage1Checkpoint,
            stage2Checkpoint,
            stage3Checkpoint
        });
        
        Console.WriteLine("Checkpoint chain created successfully");
    }
}
```

## Best Practices

### Checkpoint Design Principles

1. **Strategic Placement**: Place checkpoints at logical boundaries and after expensive operations
2. **Size Management**: Monitor checkpoint sizes and use compression for large states
3. **Retention Planning**: Configure retention policies based on business requirements
4. **Error Handling**: Always handle checkpoint failures gracefully
5. **Validation**: Validate state integrity before and after checkpoint operations

### Performance Considerations

1. **Compression**: Enable compression for storage efficiency
2. **Selective Serialization**: Exclude unnecessary data from checkpoints
3. **Cleanup**: Configure automatic cleanup to prevent storage bloat
4. **Caching**: Use in-memory caching for frequently accessed checkpoints
5. **Background Operations**: Perform checkpoint operations asynchronously when possible

### Recovery Strategies

1. **Multiple Recovery Points**: Maintain multiple checkpoints for different recovery scenarios
2. **Consistency Validation**: Always validate restored state consistency
3. **Rollback Capability**: Implement rollback to previous checkpoints if recovery fails
4. **Monitoring**: Monitor recovery success rates and performance
5. **Documentation**: Document recovery procedures and expected outcomes

## Troubleshooting

### Common Issues

**Checkpoint Creation Fails**
```
Failed to create checkpoint: State validation failed
```
**Solution**: Validate state integrity before checkpointing and check for non-serializable objects.

**Restoration Fails**
```
Failed to restore checkpoint: Checksum mismatch
```
**Solution**: Check for data corruption and verify checkpoint integrity.

**Storage Quota Exceeded**
```
Checkpoint storage quota exceeded: 1GB limit reached
```
**Solution**: Configure retention policies and enable automatic cleanup.

**Version Compatibility Issues**
```
State version 1.0.0 is not compatible with current version 1.1.0
```
**Solution**: Use state migration or update your workflow to handle version differences.

**Recovery Performance Issues**
```
Recovery taking too long: 5 minutes elapsed
```
**Solution**: Optimize checkpoint sizes, use compression, and configure appropriate retention policies.

## See Also

- [State Management](state.md) - Core state management concepts
- [Checkpointing Quickstart](../checkpointing-quickstart.md) - Quick introduction to checkpointing
- [Execution Model](execution-model.md) - How execution flows through graphs
- [Graph Recovery Service](../api/graph-recovery-service.md) - API reference for recovery operations
- [Checkpointing Examples](../examples/checkpointing-examples.md) - Practical checkpointing examples
