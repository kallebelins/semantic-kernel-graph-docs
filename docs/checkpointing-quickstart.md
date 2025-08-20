# Checkpointing Quickstart

Learn how to save and restore graph execution state using SemanticKernel.Graph's checkpointing system. This guide shows you how to persist state during long-running operations, recover from failures, and maintain execution history.

## Concepts and Techniques

**Checkpointing**: The process of saving the current state of a graph execution at specific points, enabling recovery and resumption from any saved state.

**State Persistence**: `StateHelpers` provides utilities for serializing and deserializing `GraphState` objects, while `CheckpointManager` handles storage and retrieval.

**Recovery and Replay**: Resume execution from any checkpoint, enabling fault tolerance and the ability to replay execution scenarios.

## Prerequisites and Minimum Configuration

* .NET 8.0 or later
* SemanticKernel.Graph package installed
* Graph memory service configured (required for checkpointing)
* Checkpoint support enabled in your kernel

## Quick Setup

### 1. Enable Checkpoint Support

Add checkpoint support to your kernel with memory integration:

```csharp
using SemanticKernel.Graph.Extensions;

var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddGraphMemory()  // Required for checkpointing
    .AddCheckpointSupport(options =>
    {
        options.EnableCompression = true;
        options.MaxCacheSize = 100;
    })
    .Build();
```

### 2. Create a Checkpointing Graph Executor

Use the checkpointing executor factory to create an executor with checkpoint capabilities:

```csharp
using SemanticKernel.Graph.Core;

var executorFactory = kernel.Services.GetRequiredService<ICheckpointingGraphExecutorFactory>();
var executor = executorFactory.CreateExecutor("my-graph", new CheckpointingOptions
{
    CheckpointInterval = 2,  // Create checkpoint every 2 nodes
    CreateInitialCheckpoint = true,
    CreateFinalCheckpoint = true,
    EnableAutoCleanup = true,
    CriticalNodes = new HashSet<string> { "process", "validate" }
});
```

### 3. Build and Execute Your Graph

Add nodes and execute with automatic checkpointing:

```csharp
using SemanticKernel.Graph.Nodes;

// Add nodes to your graph
var inputNode = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod(() => "Input data", "input"),
    "input", "DataInput");

var processNode = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod(() => "Processed data", "process"),
    "process", "DataProcess");

executor.AddNode(inputNode)
        .AddNode(processNode)
        .Connect("input", "process")
        .SetStartNode("input");

// Execute with automatic checkpointing
var arguments = new KernelArguments();
arguments["input"] = "Process this data";
arguments["counter"] = 0;

var result = await executor.ExecuteAsync(kernel, arguments);
Console.WriteLine($"Execution completed: {result.GetValue<object>()}");
Console.WriteLine($"ExecutionId: {executor.LastExecutionId}");
```

## Manual Checkpoint Management

### Creating Checkpoints

Use `StateHelpers` to manually create and manage checkpoints:

```csharp
using SemanticKernel.Graph.State;

// Get the current graph state
var graphState = arguments.GetOrCreateGraphState();

// Create a checkpoint with a custom name
var checkpointId = StateHelpers.CreateCheckpoint(graphState, "manual-checkpoint");
Console.WriteLine($"Created checkpoint: {checkpointId}");

// The checkpoint is now stored in the state metadata
var checkpoint = graphState.GetMetadata<StateCheckpoint>($"checkpoint_{checkpointId}");
Console.WriteLine($"Checkpoint timestamp: {checkpoint.Timestamp}");
```

### Restoring from Checkpoints

Restore your graph state from any saved checkpoint:

```csharp
try
{
    // Restore state from a specific checkpoint
    var restoredState = StateHelpers.RestoreCheckpoint(graphState, checkpointId);
    
    // Update your arguments with the restored state
    arguments.UpdateFromGraphState(restoredState);
    
    Console.WriteLine("State restored successfully");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Failed to restore checkpoint: {ex.Message}");
}
```

## Advanced Checkpoint Configuration

### Checkpointing Options

Configure detailed checkpoint behavior:

```csharp
var checkpointingOptions = new CheckpointingOptions
{
    CheckpointInterval = 3,  // Every 3 nodes
    CheckpointTimeInterval = TimeSpan.FromMinutes(5),  // Or every 5 minutes
    CreateInitialCheckpoint = true,
    CreateFinalCheckpoint = true,
    CreateErrorCheckpoints = true,  // Save state on errors
    EnableAutoCleanup = true,
    FailOnCheckpointError = false,  // Continue execution even if checkpointing fails
    
    // Define critical nodes that always trigger checkpoints
    CriticalNodes = new HashSet<string> { "process", "validate", "output" },
    
    // Configure retention policy
    RetentionPolicy = new CheckpointRetentionPolicy
    {
        MaxAge = TimeSpan.FromHours(24),
        MaxCheckpointsPerExecution = 50,
        MaxTotalStorage = 100 * 1024 * 1024  // 100MB
    }
};

var executor = executorFactory.CreateExecutor("advanced-graph", checkpointingOptions);
```

### Recovery from Failures

Implement fault tolerance with automatic checkpoint recovery:

```csharp
try
{
    var result = await executor.ExecuteAsync(kernel, arguments);
    Console.WriteLine("Execution completed successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"Execution failed: {ex.Message}");
    
    // Find the latest checkpoint for recovery
    var executionId = executor.LastExecutionId ?? arguments.GetOrCreateGraphState().StateId;
    var checkpoints = await executor.GetExecutionCheckpointsAsync(executionId);
    
    if (checkpoints.Count > 0)
    {
        var latestCheckpoint = checkpoints.First();
        Console.WriteLine($"Latest checkpoint: {latestCheckpoint.CheckpointId}");
        
        // Resume from checkpoint
        var recoveredResult = await executor.ResumeFromCheckpointAsync(
            latestCheckpoint.CheckpointId, kernel);
        
        Console.WriteLine($"Recovery successful: {recoveredResult.GetValue<object>()}");
    }
}
```

## Checkpoint Monitoring and Management

### View Checkpoint Statistics

Monitor your checkpoint system:

```csharp
// Get checkpoint statistics for the last execution
var executionId = executor.LastExecutionId;
if (!string.IsNullOrEmpty(executionId))
{
    var checkpoints = await executor.GetExecutionCheckpointsAsync(executionId);
    
    Console.WriteLine($"Checkpoint Statistics for {executionId}:");
    Console.WriteLine($"Total checkpoints: {checkpoints.Count}");
    
    foreach (var checkpoint in checkpoints.OrderBy(c => c.SequenceNumber))
    {
        Console.WriteLine($"  {checkpoint.CheckpointId}: " +
                         $"Node {checkpoint.NodeId}, " +
                         $"Size: {checkpoint.SizeInBytes / 1024:F1} KB, " +
                         $"Created: {checkpoint.CreatedAt:HH:mm:ss}");
    }
}
```

### Manual Cleanup

Clean up old checkpoints to manage storage:

```csharp
var checkpointManager = kernel.Services.GetRequiredService<ICheckpointManager>();

// Clean up old checkpoints
var cleanupCount = await checkpointManager.CleanupCheckpointsAsync(
    retentionPolicy: new CheckpointRetentionPolicy
    {
        MaxAge = TimeSpan.FromHours(1),
        MaxCheckpointsPerExecution = 10,
        MaxTotalStorage = 50 * 1024 * 1024  // 50MB
    });

Console.WriteLine($"Cleaned up {cleanupCount} old checkpoints");
```

## Troubleshooting

### Common Issues

**Checkpointing not working**: Ensure you've called `.AddGraphMemory()` and `.AddCheckpointSupport()` when building your kernel.

**Memory service not found**: The checkpointing system requires a graph memory service. Make sure it's properly configured.

**Checkpoints too large**: Enable compression with `options.EnableCompression = true` and consider reducing the data stored in your state.

**Recovery fails**: Verify checkpoint integrity and ensure the checkpoint ID exists before attempting restoration.

### Performance Recommendations

* Use appropriate checkpoint intervals based on your execution time
* Enable compression for large state objects
* Configure retention policies to prevent storage bloat
* Use critical nodes sparingly to avoid excessive checkpointing
* Monitor checkpoint sizes and adjust compression settings accordingly

## See Also

* **Reference**: [CheckpointManager](../api/CheckpointManager.md), [CheckpointingOptions](../api/CheckpointingOptions.md), [StateHelpers](../api/StateHelpers.md)
* **Guides**: [State Management](../guides/state-management.md), [Recovery and Replay](../guides/recovery-replay.md)
* **Examples**: [CheckpointingExample](../examples/checkpointing.md), [AdvancedPatternsExample](../examples/advanced-patterns.md)

## Reference APIs

* **[CheckpointManager](../api/checkpointing.md#checkpoint-manager)**: Checkpoint storage and retrieval
* **[CheckpointingOptions](../api/checkpointing.md#checkpointing-options)**: Checkpoint configuration
* **[StateHelpers](../api/state.md#state-helpers)**: State serialization utilities
* **[ICheckpointingGraphExecutor](../api/checkpointing.md#icheckpointing-graph-executor)**: Checkpointing executor interface
