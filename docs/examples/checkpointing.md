# Checkpointing Example

This example demonstrates execution state persistence and recovery using the Semantic Kernel Graph checkpointing system. It shows how to save, restore, and manage execution state for resilient workflows.

## Objective

Learn how to implement checkpointing in graph-based workflows to:
* Save execution state at critical points
* Restore workflows from previous checkpoints
* Implement automatic checkpoint management
* Handle distributed checkpoint storage
* Monitor and optimize checkpoint performance

## Prerequisites

* **.NET 8.0** or later
* **OpenAI API Key** configured in `appsettings.json`
* **Semantic Kernel Graph package** installed
* Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [State Management](../concepts/state.md)

## Key Components

### Concepts and Techniques

* **Checkpointing**: Saving execution state at specific points for later restoration
* **State Serialization**: Converting graph state to persistent storage format
* **Recovery**: Restoring workflow execution from saved checkpoints
* **Distributed Storage**: Managing checkpoints across multiple storage locations

### Core Classes

* `CheckpointManager`: Manages checkpoint creation, storage, and retrieval
* `CheckpointingGraphExecutor`: Executor with built-in checkpointing support
* `StateHelpers`: Utilities for state serialization and validation
* `CheckpointOptions`: Configuration options for checkpoint behavior

## Running the Example

### Getting Started

This example demonstrates checkpointing and state persistence with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Basic Checkpointing System

This example demonstrates basic checkpoint creation and restoration.

```csharp
// Create kernel with checkpointing support
var kernel = CreateKernel();

// Create checkpointing executor
var checkpointingExecutor = new CheckpointingGraphExecutor(
    "CheckpointingExample",
    "Basic checkpointing demonstration",
    logger);

// Configure checkpoint options
var checkpointOptions = new CheckpointOptions
{
    EnableAutoCheckpointing = true,
    CheckpointInterval = 2, // Checkpoint every 2 nodes
    EnableCompression = true,
    MaxCheckpointSize = 1024 * 1024, // 1MB
    StorageProvider = new FileSystemStorageProvider("./checkpoints")
};

checkpointingExecutor.ConfigureCheckpointing(checkpointOptions);

// Create a simple workflow
var workflow = CreateCheckpointingWorkflow();
checkpointingExecutor.AddGraph(workflow);

// Execute with checkpointing
var arguments = new KernelArguments
{
    ["input_data"] = "Sample data for processing",
    ["checkpoint_id"] = Guid.NewGuid().ToString()
};

Console.WriteLine("🚀 Starting workflow execution with checkpointing...");
var result = await checkpointingExecutor.ExecuteAsync(kernel, arguments);

Console.WriteLine($"✅ Workflow completed. Final result: {result.GetValue<string>()}");
Console.WriteLine($"📊 Checkpoints created: {checkpointingExecutor.CheckpointManager.GetCheckpointCount()}");
```

### 2. Checkpoint Recovery Example

Demonstrates how to restore workflow execution from checkpoints.

```csharp
// Simulate workflow interruption and recovery
Console.WriteLine("\n🔄 Simulating workflow interruption...");

// Create a long-running workflow
var longWorkflow = CreateLongRunningWorkflow();
var recoveryExecutor = new CheckpointingGraphExecutor(
    "RecoveryExample",
    "Checkpoint recovery demonstration",
    logger);

recoveryExecutor.ConfigureCheckpointing(new CheckpointOptions
{
    EnableAutoCheckpointing = true,
    CheckpointInterval = 1, // Checkpoint after each node
    EnableCompression = true,
    StorageProvider = new FileSystemStorageProvider("./recovery-checkpoints")
});

recoveryExecutor.AddGraph(longWorkflow);

// Start execution
var recoveryArgs = new KernelArguments
{
    ["workflow_id"] = "recovery_001",
    ["data"] = "Large dataset for processing"
};

try
{
    Console.WriteLine("🚀 Starting long workflow...");
    var recoveryResult = await recoveryExecutor.ExecuteAsync(kernel, recoveryArgs);
    Console.WriteLine($"✅ Workflow completed: {recoveryResult.GetValue<string>()}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("⏸️ Workflow was interrupted. Checkpoints saved.");
    
    // Simulate recovery
    Console.WriteLine("🔄 Recovering from checkpoint...");
    var recoveredResult = await recoveryExecutor.RecoverFromLatestCheckpointAsync(
        kernel, recoveryArgs);
    
    Console.WriteLine($"✅ Recovery successful: {recoveredResult.GetValue<string>()}");
}
```

### 3. Distributed Backup Example

Shows how to implement distributed checkpoint storage for high availability.

```csharp
// Create distributed storage providers
var localStorage = new FileSystemStorageProvider("./local-checkpoints");
var cloudStorage = new AzureBlobStorageProvider(connectionString, containerName);
var distributedStorage = new DistributedStorageProvider(new[]
{
    localStorage,
    cloudStorage
});

// Configure distributed checkpointing
var distributedExecutor = new CheckpointingGraphExecutor(
    "DistributedExample",
    "Distributed checkpointing demonstration",
    logger);

distributedExecutor.ConfigureCheckpointing(new CheckpointOptions
{
    EnableAutoCheckpointing = true,
    CheckpointInterval = 3,
    EnableCompression = true,
    StorageProvider = distributedStorage,
    ReplicationFactor = 2, // Store in 2 locations
    EnableAsyncBackup = true
});

// Create and execute workflow
var distributedWorkflow = CreateDistributedWorkflow();
distributedExecutor.AddGraph(distributedWorkflow);

var distributedArgs = new KernelArguments
{
    ["workflow_id"] = "distributed_001",
    ["data"] = "Critical data requiring backup"
};

Console.WriteLine("🚀 Starting distributed checkpointing workflow...");
var distributedResult = await distributedExecutor.ExecuteAsync(kernel, distributedArgs);

Console.WriteLine($"✅ Distributed workflow completed: {distributedResult.GetValue<string>()}");
Console.WriteLine($"📊 Checkpoints stored in {distributedStorage.GetActiveProviders().Count()} locations");
```

### 4. Monitoring and Analytics Example

Demonstrates checkpoint monitoring and performance analytics.

```csharp
// Create monitoring-enabled executor
var monitoringExecutor = new CheckpointingGraphExecutor(
    "MonitoringExample",
    "Checkpoint monitoring demonstration",
    logger);

monitoringExecutor.ConfigureCheckpointing(new CheckpointOptions
{
    EnableAutoCheckpointing = true,
    CheckpointInterval = 2,
    EnableCompression = true,
    StorageProvider = new FileSystemStorageProvider("./monitoring-checkpoints"),
    EnableMetrics = true,
    EnableDetailedLogging = true
});

// Subscribe to checkpoint events
monitoringExecutor.CheckpointManager.CheckpointCreated += (sender, e) =>
{
    Console.WriteLine($"📝 Checkpoint created: {e.CheckpointId} at {e.Timestamp}");
    Console.WriteLine($"   Size: {e.SizeBytes} bytes, Compression: {e.CompressionRatio:P1}");
};

monitoringExecutor.CheckpointManager.CheckpointRestored += (sender, e) =>
{
    Console.WriteLine($"🔄 Checkpoint restored: {e.CheckpointId} in {e.RestoreTimeMs}ms");
};

// Execute workflow with monitoring
var monitoringWorkflow = CreateMonitoringWorkflow();
monitoringExecutor.AddGraph(monitoringWorkflow);

var monitoringArgs = new KernelArguments
{
    ["workflow_id"] = "monitoring_001",
    ["data"] = "Data for monitoring demonstration"
};

Console.WriteLine("🚀 Starting monitored workflow...");
var monitoringResult = await monitoringExecutor.ExecuteAsync(kernel, monitoringArgs);

// Display checkpoint analytics
var analytics = monitoringExecutor.CheckpointManager.GetAnalytics();
Console.WriteLine("\n📊 Checkpoint Analytics:");
Console.WriteLine($"   Total checkpoints: {analytics.TotalCheckpoints}");
Console.WriteLine($"   Average size: {analytics.AverageSizeBytes} bytes");
Console.WriteLine($"   Compression ratio: {analytics.AverageCompressionRatio:P1}");
Console.WriteLine($"   Storage efficiency: {analytics.StorageEfficiency:P1}");
```

## Expected Output

### Basic Checkpointing Example

```
🚀 Starting workflow execution with checkpointing...
📝 Creating checkpoint after node: data-processor
📝 Creating checkpoint after node: data-validator
📝 Creating checkpoint after node: result-generator
✅ Workflow completed. Final result: Processed data with validation
📊 Checkpoints created: 3
📁 Checkpoints stored in: ./checkpoints/
   - checkpoint_001.json (2.3 KB)
   - checkpoint_002.json (2.1 KB)
   - checkpoint_003.json (1.8 KB)
```

### Recovery Example

```
🚀 Starting long workflow...
📝 Creating checkpoint after node: data-loader
📝 Creating checkpoint after node: data-processor
⏸️ Workflow was interrupted. Checkpoints saved.

🔄 Recovering from checkpoint...
📝 Restoring from checkpoint: checkpoint_002.json
🔄 Resuming execution from node: data-validator
📝 Creating checkpoint after node: data-validator
📝 Creating checkpoint after node: result-generator
✅ Recovery successful: Processed large dataset with recovery
📊 Recovery time: 1.2 seconds
📊 Checkpoints used: 1
```

### Distributed Backup Example

```
🚀 Starting distributed checkpointing workflow...
📝 Creating checkpoint after node: data-processor
   📤 Backing up to local storage
   📤 Backing up to cloud storage
📝 Creating checkpoint after node: data-validator
   📤 Backing up to local storage
   📤 Backing up to cloud storage
📝 Creating checkpoint after node: result-generator
   📤 Backing up to local storage
   📤 Backing up to cloud storage

✅ Distributed workflow completed: Critical data processed with backup
📊 Checkpoints stored in 2 locations
📁 Local storage: 3 checkpoints
☁️ Cloud storage: 3 checkpoints
🔒 Replication factor: 2x
```

### Monitoring Example

```
🚀 Starting monitored workflow...
📝 Checkpoint created: cp_001 at 2025-08-15 10:30:15
   Size: 2048 bytes, Compression: 75.2%
📝 Checkpoint created: cp_002 at 2025-08-15 10:30:18
   Size: 1920 bytes, Compression: 78.1%
📝 Checkpoint created: cp_003 at 2025-08-15 10:30:21
   Size: 1856 bytes, Compression: 79.8%
🔄 Checkpoint restored: cp_002 in 45ms

✅ Monitored workflow completed: Data processed with monitoring
📊 Checkpoint Analytics:
   Total checkpoints: 3
   Average size: 1941 bytes
   Compression ratio: 77.7%
   Storage efficiency: 85.2%
```

## Configuration Options

### Checkpoint Options

```csharp
var checkpointOptions = new CheckpointOptions
{
    EnableAutoCheckpointing = true,           // Automatic checkpointing
    CheckpointInterval = 2,                   // Checkpoint every N nodes
    EnableCompression = true,                 // Compress checkpoint data
    MaxCheckpointSize = 1024 * 1024,         // Maximum checkpoint size
    StorageProvider = storageProvider,        // Storage provider
    ReplicationFactor = 2,                   // Number of storage locations
    EnableAsyncBackup = true,                // Asynchronous backup
    EnableMetrics = true,                    // Enable performance metrics
    EnableDetailedLogging = true,            // Detailed logging
    CompressionLevel = CompressionLevel.Optimal, // Compression level
    EncryptionEnabled = false,               // Enable encryption
    RetentionPolicy = new RetentionPolicy    // Checkpoint retention
    {
        MaxCheckpoints = 100,
        MaxAge = TimeSpan.FromDays(30),
        EnableAutoCleanup = true
    }
};
```

### Storage Provider Configuration

```csharp
// File system storage
var fileStorage = new FileSystemStorageProvider("./checkpoints")
{
    MaxFileSize = 10 * 1024 * 1024,         // 10MB max file size
    EnableFileRotation = true,               // Rotate old files
    CompressionEnabled = true,               // Enable file compression
    EncryptionEnabled = false                // Disable encryption
};

// Azure Blob storage
var azureStorage = new AzureBlobStorageProvider(connectionString, containerName)
{
    BlobTier = BlobTier.Cool,                // Use cool tier for cost
    EnableSoftDelete = true,                 // Enable soft delete
    RetentionDays = 90,                      // 90-day retention
    EnableVersioning = true                  // Enable blob versioning
};

// Distributed storage
var distributedStorage = new DistributedStorageProvider(new[]
{
    fileStorage,
    azureStorage
})
{
    PrimaryProvider = fileStorage,            // Primary storage
    FailoverEnabled = true,                  // Enable failover
    ConsistencyLevel = ConsistencyLevel.Eventual, // Eventual consistency
    RetryPolicy = new ExponentialBackoffRetryPolicy(3, TimeSpan.FromSeconds(1))
};
```

## Troubleshooting

### Common Issues

#### Checkpoint Creation Fails
```bash
# Problem: Checkpoints fail to create
# Solution: Check storage permissions and disk space
EnableDetailedLogging = true;
StorageProvider = new FileSystemStorageProvider("./checkpoints");
```

#### Slow Checkpoint Operations
```bash
# Problem: Checkpoint operations are slow
# Solution: Optimize compression and storage
CompressionLevel = CompressionLevel.Fastest;
EnableAsyncBackup = true;
StorageProvider = new FastStorageProvider();
```

#### Recovery Failures
```bash
# Problem: Checkpoint recovery fails
# Solution: Validate checkpoint integrity and storage
EnableCheckpointValidation = true;
ValidateOnRestore = true;
```

### Debug Mode

Enable detailed logging for troubleshooting:

```csharp
// Enable debug logging
var logger = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
}).CreateLogger<CheckpointingExample>();

// Configure executor with debug logging
var debugExecutor = new CheckpointingGraphExecutor(
    "DebugExample", "Debug checkpointing", logger);

debugExecutor.ConfigureCheckpointing(new CheckpointOptions
{
    EnableDetailedLogging = true,
    EnableMetrics = true,
    LogCheckpointOperations = true,
    LogStorageOperations = true
});
```

## Advanced Patterns

### Custom Checkpoint Triggers

```csharp
// Implement custom checkpoint triggers
var customTrigger = new CustomCheckpointTrigger
{
    ShouldCheckpoint = (context) =>
    {
        // Checkpoint on specific conditions
        var nodeId = context.CurrentNode?.NodeId;
        var executionStep = context.ExecutionStep;
        
        return nodeId == "critical-node" || 
               executionStep % 5 == 0 ||
               context.State.GetValue<int>("data_size") > 1000;
    }
};

checkpointingExecutor.CheckpointTrigger = customTrigger;
```

### Incremental Checkpointing

```csharp
// Implement incremental checkpointing for large states
var incrementalOptions = new IncrementalCheckpointOptions
{
    EnableIncrementalCheckpointing = true,
    IncrementThreshold = 1024 * 1024,        // 1MB threshold
    DeltaCompression = true,                 // Compress deltas
    MergeStrategy = MergeStrategy.Optimistic, // Optimistic merging
    ValidationStrategy = ValidationStrategy.Checksum // Checksum validation
};

checkpointingExecutor.ConfigureIncrementalCheckpointing(incrementalOptions);
```

### Checkpoint Orchestration

```csharp
// Orchestrate checkpoints across multiple workflows
var orchestrator = new CheckpointOrchestrator
{
    GlobalCheckpointInterval = TimeSpan.FromMinutes(5),
    WorkflowDependencies = new Dictionary<string, string[]>
    {
        ["workflow_a"] = new[] { "workflow_b", "workflow_c" },
        ["workflow_b"] = new[] { "workflow_d" },
        ["workflow_c"] = new[] { "workflow_d" }
    },
    CheckpointStrategy = CheckpointStrategy.DependencyAware
};

orchestrator.RegisterWorkflow(checkpointingExecutor);
orchestrator.StartOrchestration();
```

## Related Examples

* [State Management](./state-management.md): Graph state and argument handling
* [Streaming Execution](./streaming-execution.md): Real-time execution monitoring
* [Multi-Agent](./multi-agent.md): Coordinated multi-agent workflows
* [Graph Metrics](./graph-metrics.md): Performance monitoring and optimization

## See Also

* [Checkpointing Concepts](../concepts/checkpointing.md): Understanding state persistence
* [State Management](../concepts/state.md): Graph state fundamentals
* [Performance Monitoring](../how-to/metrics-and-observability.md): Metrics and optimization
* [API Reference](../api/): Complete API documentation
