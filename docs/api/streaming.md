# Streaming APIs Reference

This reference documents the streaming execution APIs in SemanticKernel.Graph, which enable real-time monitoring and event-driven processing of graph execution.

## IStreamingGraphExecutor

Interface for streaming graph executors that emit events during execution.

### Methods

#### ExecuteStreamAsync

```csharp
IGraphExecutionEventStream ExecuteStreamAsync(
    Kernel kernel,
    KernelArguments arguments,
    StreamingExecutionOptions? options = null,
    CancellationToken cancellationToken = default)
```

Executes a graph and returns a stream of execution events.

**Parameters:**
* `kernel`: Semantic kernel instance
* `arguments`: Initial arguments for execution
* `options`: Optional streaming execution options
* `cancellationToken`: Cancellation token

**Returns:** Stream of execution events

#### ExecuteStreamFromAsync

```csharp
IGraphExecutionEventStream ExecuteStreamFromAsync(
    string startNodeId,
    Kernel kernel,
    KernelArguments arguments,
    StreamingExecutionOptions? options = null,
    CancellationToken cancellationToken = default)
```

Executes a graph starting from a specific node and returns a stream of execution events.

**Parameters:**
* `startNodeId`: ID of the node to start from
* `kernel`: Semantic kernel instance
* `arguments`: Initial arguments for execution
* `options`: Optional streaming execution options
* `cancellationToken`: Cancellation token

**Returns:** Stream of execution events

## IGraphExecutionEventStream

Represents a stream of graph execution events in real-time. Provides asynchronous iteration over execution events as they occur.

### Properties

* `ExecutionId`: Gets the execution identifier for this stream
* `Status`: Gets the current status of the execution stream
* `CreatedAt`: Gets the timestamp when the stream was created
* `EventCount`: Gets the total number of events emitted by this stream
* `IsCompleted`: Indicates whether the stream has completed
* `CompletionResult`: Gets the completion result (if completed)

### Events

* `StatusChanged`: Event fired when the stream status changes
* `EventEmitted`: Event fired when a new event is emitted to the stream
* `SerializedEventEmitted`: Event fired when a serialized payload for an event is available

### Methods

#### WaitForCompletionAsync

```csharp
Task<StreamCompletionResult> WaitForCompletionAsync(TimeSpan timeout)
```

Waits for the stream to complete with a timeout.

**Parameters:**
* `timeout`: Maximum time to wait for completion

**Returns:** Completion result

## StreamingExecutionOptions

Configuration options for streaming execution.

### Buffer Configuration

```csharp
public int BufferSize { get; set; } = 100;                    // Initial buffer size
public int MaxBufferSize { get; set; } = 1000;                // Maximum buffer size before backpressure
public int ProducerBatchSize { get; set; } = 1;               // Producer-side batch size before flushing
public TimeSpan? ProducerFlushInterval { get; set; }          // Optional flush interval
```

### Reconnection Settings

```csharp
public bool EnableAutoReconnect { get; set; } = true;         // Enable automatic reconnection
public int MaxReconnectAttempts { get; set; } = 3;            // Maximum reconnection attempts
public TimeSpan InitialReconnectDelay { get; set; } = TimeSpan.FromSeconds(1);  // Initial delay
public TimeSpan MaxReconnectDelay { get; set; } = TimeSpan.FromSeconds(30);     // Maximum delay
```

### Event Configuration

```csharp
public bool IncludeStateSnapshots { get; set; } = false;      // Include intermediate state snapshots
public GraphExecutionEventType[]? EventTypesToEmit { get; set; }  // Types of events to emit
public List<IGraphExecutionEventHandler> EventHandlers { get; set; } = new();  // Custom event handlers
```

### Heartbeat Configuration

```csharp
public bool EnableHeartbeat { get; set; } = false;            // Enable heartbeat events
public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(30);  // Heartbeat interval
```

### Compression and Serialization

```csharp
public bool EnableEventCompression { get; set; } = false;     // Enable event compression
public CompressionAlgorithm CompressionAlgorithm { get; set; } = CompressionAlgorithm.Gzip;  // Compression algorithm
public int CompressionThresholdBytes { get; set; } = 8 * 1024;  // Minimum size before compression
public bool AdaptiveEventCompressionEnabled { get; set; } = true;  // Adaptive compression decision
public int AdaptiveEventCompressionWindowSize { get; set; } = 32;  // Sliding window size
public double AdaptiveEventCompressionMinSavingsRatio { get; set; } = 0.10;  // Minimum savings ratio
```

### Memory-Mapped Buffer Options

```csharp
public bool UseMemoryMappedSerializedBuffer { get; set; } = false;  // Use memory-mapped files
public int MemoryMappedSerializedThresholdBytes { get; set; } = 64 * 1024;  // Minimum size for MM buffer
public string MemoryMappedBufferDirectory { get; set; } = Path.GetTempPath();  // Buffer directory
public long MemoryMappedFileSizeBytes { get; set; } = 64L * 1024 * 1024;  // Maximum file size
```

## GraphExecutionEvent

Base class for all graph execution events in the streaming system.

### Properties

* `EventId`: Unique identifier for this event
* `ExecutionId`: Execution identifier this event belongs to
* `Timestamp`: Timestamp when this event occurred
* `EventType`: Type of this event
* `HighPrecisionTimestamp`: Monotonic high-precision timestamp (Stopwatch ticks)
* `HighPrecisionFrequency`: Frequency of the high-precision timer

## Event Types

### Execution Events

#### GraphExecutionStartedEvent

Fired when graph execution starts.

**Properties:**
* `StartNode`: Starting node for execution
* `InitialState`: Initial graph state

#### GraphExecutionCompletedEvent

Fired when graph execution completes successfully.

**Properties:**
* `FinalResult`: Final execution result
* `FinalState`: Final graph state
* `TotalDuration`: Total execution duration
* `NodesExecuted`: Number of nodes executed

#### GraphExecutionFailedEvent

Fired when graph execution fails.

**Properties:**
* `Exception`: Exception that caused the failure
* `FinalState`: Final graph state
* `TotalDuration`: Total execution duration
* `NodesExecuted`: Number of nodes executed before failure

#### GraphExecutionCancelledEvent

Fired when graph execution is cancelled.

**Properties:**
* `FinalState`: Final graph state
* `TotalDuration`: Total execution duration
* `NodesExecuted`: Number of nodes executed before cancellation

### Node Events

#### NodeExecutionStartedEvent

Fired when a node starts executing.

**Properties:**
* `Node`: Node that started executing
* `CurrentState`: Current graph state

#### NodeExecutionCompletedEvent

Fired when a node completes execution successfully.

**Properties:**
* `Node`: Node that completed execution
* `Result`: Execution result
* `UpdatedState`: Updated graph state after node execution
* `ExecutionDuration`: Duration of node execution

#### NodeExecutionFailedEvent

Fired when a node fails execution.

**Properties:**
* `Node`: Node that failed execution
* `Exception`: Exception that occurred during execution
* `CurrentState`: Current graph state at the time of failure
* `ExecutionDuration`: Duration of execution before failure

#### NodeEnteredEvent

Fired when the executor enters a node (selected as current node).

**Properties:**
* `Node`: Node that was entered
* `CurrentState`: Current graph state upon entering the node

#### NodeExitedEvent

Fired when the executor exits a node (after navigation decision).

**Properties:**
* `Node`: Node that was exited
* `UpdatedState`: Updated graph state upon exiting the node

### Conditional and Control Events

#### ConditionEvaluatedEvent

Fired when a condition is evaluated by a conditional node.

**Properties:**
* `NodeId`: ID of the conditional node
* `NodeName`: Name of the conditional node
* `Expression`: Evaluated expression (if template-based)
* `Result`: Boolean evaluation result
* `EvaluationDuration`: Time taken to evaluate the condition
* `State`: Graph state at evaluation time

#### StateMergeConflictEvent

Fired when a state merge conflict is detected during execution.

**Properties:**
* `ConflictKey`: Parameter key where conflict occurred
* `BaseValue`: Value from base state
* `OverlayValue`: Value from overlay state
* `ConflictPolicy`: Merge policy that detected the conflict
* `ResolvedValue`: Value used after conflict resolution
* `NodeId`: Node ID where conflict occurred
* `WasResolved`: Whether the conflict was resolved automatically

### Circuit Breaker Events

#### CircuitBreakerStateChangedEvent

Fired when a circuit breaker state changes.

**Properties:**
* `NodeId`: Node identifier
* `OldState`: Previous circuit breaker state
* `NewState`: New circuit breaker state

#### CircuitBreakerOperationAttemptedEvent

Fired when a circuit breaker operation is attempted.

**Properties:**
* `NodeId`: Node identifier
* `OperationType`: Type of operation attempted
* `CircuitState`: Current circuit breaker state

#### CircuitBreakerOperationBlockedEvent

Fired when a circuit breaker blocks an operation.

**Properties:**
* `NodeId`: Node identifier
* `Reason`: Reason for blocking the operation
* `CircuitState`: Current circuit breaker state
* `FailureCount`: Current failure count

### Resource and Error Policy Events

#### ResourceBudgetExhaustedEvent

Fired when resource budget is exhausted.

**Properties:**
* `NodeId`: Node identifier
* `ResourceType`: Type of resource that was exhausted
* `RequestedAmount`: Amount of resource requested
* `AvailableAmount`: Amount of resource available

#### RetryScheduledEvent

Fired when a retry is scheduled due to an error policy decision.

**Properties:**
* `NodeId`: Node identifier
* `NodeName`: Node name
* `AttemptNumber`: Retry attempt number
* `Delay`: Optional delay before retry

#### NodeSkippedDueToErrorPolicyEvent

Fired when a node is skipped due to an error policy decision.

**Properties:**
* `NodeId`: Node identifier
* `NodeName`: Node name
* `Reason`: Reason for skipping the node

## GraphExecutionEventType Enum

Enumeration of different types of graph execution events.

```csharp
public enum GraphExecutionEventType
{
    ExecutionStarted = 0,           // Graph execution started
    NodeStarted = 1,                // Node execution started
    NodeCompleted = 2,              // Node execution completed successfully
    NodeFailed = 3,                 // Node execution failed
    ExecutionCompleted = 4,         // Graph execution completed successfully
    ExecutionFailed = 5,            // Graph execution failed
    ExecutionCancelled = 6,         // Graph execution was cancelled
    NodeEntered = 7,                // Executor entered a node
    NodeExited = 8,                 // Executor exited a node
    ConditionEvaluated = 9,         // Conditional expression evaluated
    StateMergeConflictDetected = 10, // State merge conflict detected
    CircuitBreakerStateChanged = 11, // Circuit breaker state changed
    CircuitBreakerOperationAttempted = 12, // Circuit breaker operation attempted
    CircuitBreakerOperationBlocked = 13,   // Circuit breaker operation blocked
    ResourceBudgetExhausted = 14,   // Resource budget exhausted
    RetryScheduled = 15,            // Retry scheduled
    NodeSkippedDueToErrorPolicy = 16 // Node skipped due to error policy
}
```

## StreamingGraphExecutor

Streaming-enabled graph executor that emits real-time events during execution.

### Constructors

```csharp
public StreamingGraphExecutor(string name, string? description = null, IGraphLogger? logger = null)
public StreamingGraphExecutor(GraphExecutor executor)
```

### Properties

* `Name`: Name of the graph
* `Description`: Description of the graph
* `GraphId`: Unique identifier for the graph
* `IsReadyForExecution`: Whether the graph is ready for execution
* `NodeCount`: Number of nodes in the graph

### Methods

#### Graph Management

```csharp
public void AddNode(IGraphNode node)
public void Connect(string fromNodeId, string toNodeId, ConditionalEdge? edge = null)
public void SetStartNode(string nodeId)
public ValidationResult ValidateGraphIntegrity()
```

#### Execution

```csharp
public Task<FunctionResult> ExecuteAsync(Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken = default)
public IGraphExecutionEventStream ExecuteStreamAsync(Kernel kernel, KernelArguments arguments, StreamingExecutionOptions? options = null, CancellationToken cancellationToken = default)
public IGraphExecutionEventStream ExecuteStreamFromAsync(string startNodeId, Kernel kernel, KernelArguments arguments, StreamingExecutionOptions? options = null, CancellationToken cancellationToken = default)
```

## Streaming Extensions

### Converting Executors

```csharp
// Convert regular executor to streaming
var streamingExecutor = regularExecutor.AsStreamingExecutor();
```

### Creating Options

```csharp
// Create with fluent API
var options = StreamingExtensions.CreateStreamingOptions()
    .Configure()
    .WithBufferSize(100)
    .WithEventTypes(GraphExecutionEventType.ExecutionStarted, GraphExecutionEventType.NodeCompleted)
    .Build();
```

### Stream Processing

```csharp
// Filter events
var filteredStream = eventStream.Filter(GraphExecutionEventType.NodeCompleted);

// Buffer events
var bufferedStream = eventStream.WithBuffering(50);

// Convert to API responses
var apiResponses = eventStream.ToApiResponses().WithHeartbeat(TimeSpan.FromSeconds(5));
```

## Tracing and Correlation

SemanticKernel.Graph provides comprehensive distributed tracing capabilities through OpenTelemetry's `ActivitySource`, enabling correlation between execution events and tracing spans for observability and debugging.

### ActivitySource Integration

The framework automatically creates tracing spans for graph execution and individual node operations using the `ActivitySource` named "SemanticKernel.Graph".

#### Graph-Level Tracing

```csharp
// Graph execution span with correlation tags
using var execActivity = _activitySource.StartActivity("Graph.Execute", ActivityKind.Internal);
if (execActivity is not null)
{
    execActivity.SetTag("graph.id", GraphId);
    execActivity.SetTag("graph.name", Name);
    execActivity.SetTag("execution.id", context.ExecutionId);
    execActivity.SetTag("start.node.id", StartNode?.NodeId);
    execActivity.SetTag("node.count", NodeCount);
}
```

#### Node-Level Tracing

```csharp
// Per-node execution span with correlation tags
using var nodeActivity = _activitySource.StartActivity("Graph.Node.Execute", ActivityKind.Internal);
if (nodeActivity is not null)
{
    nodeActivity.SetTag("graph.id", GraphId);
    nodeActivity.SetTag("graph.name", Name);
    nodeActivity.SetTag("execution.id", context.ExecutionId);
    nodeActivity.SetTag("node.id", currentNode.NodeId);
    nodeActivity.SetTag("node.name", currentNode.Name);
}
```

### Event Correlation

All execution events include correlation identifiers that link them to their corresponding tracing spans:

#### Execution Correlation

* **`ExecutionId`**: Unique identifier for each graph execution run
* **`GraphId`**: Stable identifier for the graph definition
* **`EventId`**: Unique identifier for each individual event

#### Node Correlation

* **`NodeId`**: Stable identifier for the specific node
* **`NodeName`**: Human-readable name for the node
* **`Timestamp`**: Precise timestamp when the event occurred
* **`HighPrecisionTimestamp`**: Monotonic timestamp for precise ordering

### Multi-Agent Distributed Tracing

For multi-agent workflows, the `MultiAgentCoordinator` provides additional tracing capabilities:

```csharp
// Workflow-level tracing span
using var activity = _activitySource?.StartActivity(
    "MultiAgent.ExecuteWorkflow",
    ActivityKind.Internal);
if (activity is not null)
{
    activity.SetTag("workflow.id", workflowId);
    activity.SetTag("workflow.name", workflow.Name);
    activity.SetTag("workflow.task.count", workflow.Tasks.Count);
    activity.SetTag("workflow.agents.count", workflow.RequiredAgents.Count);
}
```

### Tracing Configuration

Enable distributed tracing in multi-agent scenarios through `MultiAgentOptions`:

```csharp
var options = new MultiAgentOptions
{
    EnableDistributedTracing = true,
    DistributedTracingSourceName = "SemanticKernel.Graph.MultiAgent"
};
```

### Correlation Patterns

#### Execution Flow Correlation

```csharp
// Events and spans share the same execution.id
var execStarted = events.OfType<GraphExecutionStartedEvent>().First();
var execSpan = activities.First(a => 
    a.Tags.Any(t => t.Key == "execution.id" && 
    t.Value?.ToString() == execStarted.ExecutionId));
```

#### Node Execution Correlation

```csharp
// Node events correlate with node spans by execution.id and node.id
var nodeStarted = events.OfType<NodeExecutionStartedEvent>().First();
var nodeSpan = activities.First(a => 
    a.Tags.Any(t => t.Key == "execution.id" && 
    t.Value?.ToString() == nodeStarted.ExecutionId) &&
    a.Tags.Any(t => t.Key == "node.id" && 
    t.Value?.ToString() == nodeStarted.Node.NodeId));
```

### High-Precision Timing

Events include high-precision timestamps for accurate performance measurement:

```csharp
public class GraphExecutionEvent
{
    // Monotonic high-precision timestamp (Stopwatch ticks)
    public long HighPrecisionTimestamp { get; }
    
    // Frequency of the high-precision timer
    public long HighPrecisionFrequency { get; }
}
```

### Tracing Best Practices

1. **Correlation IDs**: Always use the `ExecutionId` and `NodeId` for correlating events across systems
2. **Span Naming**: Use consistent span names: `Graph.Execute`, `Graph.Node.Execute`, `MultiAgent.ExecuteWorkflow`
3. **Tag Consistency**: Apply consistent tags across all spans: `graph.id`, `execution.id`, `node.id`
4. **Error Tracking**: Set error tags on spans when exceptions occur
5. **Performance Metrics**: Use high-precision timestamps for accurate latency measurements

## See Also

* [Streaming Quickstart](../streaming-quickstart.md) - Get started with streaming execution
* [Streaming Concepts](../concepts/streaming.md) - Deep dive into streaming concepts
* [Streaming Examples](../examples/streaming-execution.md) - Practical streaming examples
* [GraphExecutor Reference](./graph-executor.md) - Core graph execution APIs
* [State Management Reference](./state.md) - Graph state and serialization
* [Metrics and Observability](../how-to/metrics-and-observability.md) - Comprehensive observability guide
