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
// Create a configured GraphExecutor with sensible defaults.
// This demonstrates initializing the executor and running a graph.
var executorOptions = new GraphExecutionOptions
{
    // Maximum time allowed for a single graph execution
    MaxExecutionTime = TimeSpan.FromMinutes(5),
    // Enable automatic checkpointing for recovery scenarios
    EnableCheckpointing = true,
    // Maximum number of nodes to run in parallel when branching
    MaxParallelNodes = 4
};

// Construct the executor (implementation may be provided by the library).
var executor = new GraphExecutor(options: executorOptions);

// Execute the graph. Use ConfigureAwait(false) in library code to avoid
// capturing synchronization contexts in consumer applications.
var result = await executor.ExecuteAsync(graph, arguments).ConfigureAwait(false);
```

### StreamingGraphExecutor
```csharp
// Create a streaming executor useful for real-time monitoring of execution events.
var streamingOptions = new StreamingExecutionOptions
{
    // How many events to buffer before applying backpressure
    BufferSize = 1000,
    // Allow the executor to signal backpressure to producers when overloaded
    EnableBackpressure = true,
    // Maximum time to wait for the next event before timing out
    EventTimeout = TimeSpan.FromSeconds(30)
};

var streamingExecutor = new StreamingGraphExecutor(options: streamingOptions);

// Execute the graph in streaming mode and obtain an asynchronous event stream.
var eventStream = await streamingExecutor.ExecuteStreamingAsync(graph, arguments).ConfigureAwait(false);
```

### CheckpointManager
```csharp
// Configure the checkpoint manager to automatically persist execution state.
var checkpointOptions = new CheckpointOptions
{
    // Periodically persist execution state to enable recovery
    AutoCheckpointInterval = TimeSpan.FromSeconds(30),
    // Limit stored checkpoints to avoid unbounded storage growth
    MaxCheckpoints = 10,
    // Enable compression to reduce checkpoint size
    CompressionEnabled = true
};

var checkpointManager = new CheckpointManager(options: checkpointOptions);
```

## Execution Cycle

### Before Phase
```csharp
// The "Before" phase prepares the node for execution.
// Typical steps: validate inputs, acquire resources, and run middleware hooks.
await node.BeforeExecutionAsync(context).ConfigureAwait(false);

// Example: validate required input exists and throw a clear exception early.
if (!context.KernelArguments.ContainsKey("input"))
{
    throw new InvalidOperationException("Missing required argument 'input'");
}
```

### Execute Phase
```csharp
// The core node logic runs in the Execute phase. Implementations should be
// asynchronous and return a result object that can be persisted into state.
var result = await node.ExecuteAsync(context).ConfigureAwait(false);

// After the node runs, apply any business-specific state updates in a
// deterministic and explicit manner.
context.State.Set("lastResult", result);
```

### After Phase
```csharp
// The After phase is used to release resources, run post-processing hooks,
// and emit metrics. Keep post-execution logic idempotent where possible.
await node.AfterExecutionAsync(context).ConfigureAwait(false);

// Example: release transient permits or log completion metrics
// (actual implementations depend on the registered metrics provider).
```

## State Management

### Execution State
```csharp
// Strongly typed execution state captures the runtime position, variables,
// and any metadata useful for debugging or resuming execution.
var executionState = new ExecutionState
{
    // Identifier of the currently executing node
    CurrentNode = nodeId,
    // Ordered list of nodes visited so far (useful for diagnostics)
    ExecutionPath = new[] { "start", "process", "current" },
    // Arbitrary variables persisted across nodes; prefer explicit keys
    Variables = new Dictionary<string, object>(StringComparer.Ordinal)
    {
        ["input"] = "initial-value"
    },
    // Optional metadata for tooling and analysis
    Metadata = new ExecutionMetadata()
};
```

### Execution History
```csharp
// History object records the sequence of steps and timing information for
// post-mortem analysis and metrics reporting.
var executionHistory = new ExecutionHistory
{
    Steps = new List<ExecutionStep>(),
    Timestamps = new List<DateTime>(),
    PerformanceMetrics = new Dictionary<string, TimeSpan>(StringComparer.Ordinal)
};
```

## Recovery and Checkpointing

### Saving State
```csharp
// Persist the current execution state so the run can be resumed or inspected
// later. The checkpoint manager is responsible for serialization and storage.
var checkpoint = await checkpointManager.CreateCheckpointAsync(
    graphId: graph.Id,
    executionId: context.ExecutionId,
    state: context.State
).ConfigureAwait(false);

Console.WriteLine($"Checkpoint saved (id={checkpoint.Id})");
```

### Restoring State
```csharp
// Restore a previously saved checkpoint and resume execution. Restored
// context should be validated before resuming.
var restoredContext = await checkpointManager.RestoreFromCheckpointAsync(
    checkpointId: checkpoint.Id
).ConfigureAwait(false);

// Optionally validate restored context
if (restoredContext == null)
{
    throw new InvalidOperationException("Failed to restore checkpoint; context is null.");
}

var resumedResult = await executor.ExecuteAsync(graph, restoredContext).ConfigureAwait(false);
```

## Streaming and Events

### Execution Events
```csharp
// Events emitted during execution provide fine-grained visibility into the
// runtime. Consumers can subscribe to streams or persist events for auditing.
var events = new[]
{
    new GraphExecutionEvent
    {
        Type = ExecutionEventType.NodeStarted,
        NodeId = "process",
        Timestamp = DateTime.UtcNow,
        // Use simple anonymous objects for event payloads in examples
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
// Consume an asynchronous stream of execution events. Use a switch to handle
// different event types; keep handlers small and non-blocking.
await foreach (var evt in eventStream.ConfigureAwait(false))
{
    switch (evt.Type)
    {
        case ExecutionEventType.NodeStarted:
            Console.WriteLine($"Node {evt.NodeId} started at {evt.Timestamp:O}");
            break;
        case ExecutionEventType.NodeCompleted:
            Console.WriteLine($"Node {evt.NodeId} completed at {evt.Timestamp:O}");
            break;
        default:
            Console.WriteLine($"Event {evt.Type} for node {evt.NodeId}");
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
// Example logging calls. Replace with your preferred logging framework
// (Microsoft.Extensions.Logging.ILogger is recommended for real apps).
var logger = new SemanticKernelGraphLogger();

// Log the start of execution, per-node execution, and completion.
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
