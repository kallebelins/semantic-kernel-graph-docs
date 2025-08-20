# Execution

Execution defines how graphs are processed, including sequential, parallel and distributed modes.

## Concepts and Techniques

**Graph Execution**: Process of navigating through graph nodes following routing rules and executing defined operations.

**Execution Cycle**: Sequence of events that occurs during execution: Before → Execute → After.

**Checkpointing**: Ability to save and restore execution state for recovery and analysis.

## Execution Modes

### Sequential Execution
* **Linear Processing**: Nodes execute one after another
* **Dependencies Respected**: Order based on graph structure
* **Shared State**: Data passes from one node to the next
* **Simple Debug**: Easy execution flow tracking

### Parallel Execution (Fork/Join)
* **Simultaneous Processing**: Multiple nodes execute at the same time
* **Deterministic Scheduler**: Guarantee of reproducibility
* **State Merging**: Combination of parallel execution results
* **Concurrency Control**: Resource limits and policies

### Distributed Execution
* **Remote Processing**: Execution in separate processes or machines
* **Asynchronous Communication**: Message exchange between components
* **Fault Tolerance**: Recovery from network or process failures
* **Load Balancing**: Balanced work distribution

## Main Components

### GraphExecutor
```csharp
var executor = new GraphExecutor(
    options: new GraphExecutionOptions
    {
        MaxExecutionTime = TimeSpan.FromMinutes(5),
        EnableCheckpointing = true,
        MaxParallelNodes = 4
    }
);

var result = await executor.ExecuteAsync(graph, arguments);
```

### StreamingGraphExecutor
```csharp
var streamingExecutor = new StreamingGraphExecutor(
    options: new StreamingExecutionOptions
    {
        BufferSize = 1000,
        EnableBackpressure = true,
        EventTimeout = TimeSpan.FromSeconds(30)
    }
);

var eventStream = await streamingExecutor.ExecuteStreamingAsync(graph, arguments);
```

### CheckpointManager
```csharp
var checkpointManager = new CheckpointManager(
    options: new CheckpointOptions
    {
        AutoCheckpointInterval = TimeSpan.FromSeconds(30),
        MaxCheckpoints = 10,
        CompressionEnabled = true
    }
);
```

## Execution Cycle

### Before Phase
```csharp
// Input validation and preparation
await node.BeforeExecutionAsync(context);
// Precondition verification
// Resource initialization
```

### Execute Phase
```csharp
// Main node execution
var result = await node.ExecuteAsync(context);
// Business logic processing
// State update
```

### After Phase
```csharp
// Cleanup and finalization
await node.AfterExecutionAsync(context);
// Resource release
// Metrics logging
```

## State Management

### Execution State
```csharp
var executionState = new ExecutionState
{
    CurrentNode = nodeId,
    ExecutionPath = new[] { "start", "process", "current" },
    Variables = new Dictionary<string, object>(),
    Metadata = new ExecutionMetadata()
};
```

### Execution History
```csharp
var executionHistory = new ExecutionHistory
{
    Steps = new List<ExecutionStep>(),
    Timestamps = new List<DateTime>(),
    PerformanceMetrics = new Dictionary<string, TimeSpan>()
};
```

## Recovery and Checkpointing

### Saving State
```csharp
// Save current state
var checkpoint = await checkpointManager.CreateCheckpointAsync(
    graphId: graph.Id,
    executionId: context.ExecutionId,
    state: context.State
);
```

### Restoring State
```csharp
// Restore execution from a checkpoint
var restoredContext = await checkpointManager.RestoreFromCheckpointAsync(
    checkpointId: checkpoint.Id
);

var result = await executor.ExecuteAsync(graph, restoredContext);
```

## Streaming and Events

### Execution Events
```csharp
var events = new[]
{
    new GraphExecutionEvent
    {
        Type = ExecutionEventType.NodeStarted,
        NodeId = "process",
        Timestamp = DateTime.UtcNow,
        Data = new { input = "data" }
    },
    new GraphExecutionEvent
    {
        Type = ExecutionEventType.NodeCompleted,
        NodeId = "process",
        Timestamp = DateTime.UtcNow,
        Data = new { output = "result" }
    }
};
```

### Consuming Events
```csharp
await foreach (var evt in eventStream)
{
    switch (evt.Type)
    {
        case ExecutionEventType.NodeStarted:
            Console.WriteLine($"Node {evt.NodeId} started");
            break;
        case ExecutionEventType.NodeCompleted:
            Console.WriteLine($"Node {evt.NodeId} completed");
            break;
    }
}
```

## Configuration and Options

### GraphExecutionOptions
```csharp
var options = new GraphExecutionOptions
{
    MaxExecutionTime = TimeSpan.FromMinutes(10),
    EnableCheckpointing = true,
    MaxParallelNodes = 8,
    EnableMetrics = true,
    EnableLogging = true,
    RetryPolicy = new ExponentialBackoffRetryPolicy(maxRetries: 3)
};
```

### StreamingExecutionOptions
```csharp
var streamingOptions = new StreamingExecutionOptions
{
    BufferSize = 1000,
    EnableBackpressure = true,
    EventTimeout = TimeSpan.FromSeconds(60),
    BatchSize = 100,
    EnableCompression = true
};
```

## Monitoring and Metrics

### Performance Metrics
* **Execution Time**: Total latency and per node
* **Throughput**: Number of nodes executed per second
* **Resource Utilization**: CPU, memory, and I/O
* **Success Rate**: Percentage of successful executions

### Logging and Tracing
```csharp
var logger = new SemanticKernelGraphLogger();
logger.LogExecutionStart(graph.Id, context.ExecutionId);
logger.LogNodeExecution(node.Id, context.ExecutionId, stopwatch.Elapsed);
logger.LogExecutionComplete(graph.Id, context.ExecutionId, result);
```

## See Also

* [Execution Model](../concepts/execution-model.md)
* [Checkpointing](../concepts/checkpointing.md)
* [Streaming](../concepts/streaming.md)
* [Metrics and Observability](../how-to/metrics-and-observability.md)
* [Execution Examples](../examples/execution-guide.md)
* [Streaming Execution Examples](../examples/streaming-execution.md)

## References

* `GraphExecutor`: Main graph executor
* `StreamingGraphExecutor`: Executor with event streaming
* `CheckpointManager`: Checkpoint manager
* `GraphExecutionOptions`: Execution options
* `StreamingExecutionOptions`: Streaming options
* `ExecutionState`: Execution state
* `GraphExecutionEvent`: Execution events
