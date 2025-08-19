# Execution Context and Events

This reference covers the execution context, events, limits, and priorities that govern graph execution in SemanticKernel.Graph.

## GraphExecutionContext

The `GraphExecutionContext` maintains the execution state for a single graph run, tracking execution progress, managing resources, and providing coordination services.

### Core Properties

```csharp
public sealed class GraphExecutionContext
{
    // Execution identification
    public string ExecutionId { get; }                    // Unique execution identifier
    public int ExecutionSeed { get; }                     // Seed for reproducible randomness
    public Random Random { get; }                         // Seeded random generator
    
    // Kernel and state
    public Kernel Kernel { get; }                         // Semantic kernel instance
    public IKernelWrapper KernelWrapper { get; }          // Kernel wrapper
    public GraphState GraphState { get; set; }            // Current graph state
    
    // Execution control
    public CancellationToken CancellationToken { get; }   // Effective cancellation token
    public GraphExecutionOptions ExecutionOptions { get; } // Immutable execution options
    
    // Timing and status
    public DateTimeOffset StartTime { get; }              // Execution start timestamp
    public DateTimeOffset? EndTime { get; }               // Execution end timestamp
    public TimeSpan? Duration { get; }                    // Total execution duration
    public GraphExecutionStatus Status { get; }           // Current execution status
    
    // Execution state
    public bool IsPaused { get; }                         // Whether execution is paused
    public string? PauseReason { get; }                   // Reason for pause
    public IGraphNode? CurrentNode { get; }               // Currently executing node
    public IReadOnlyList<IGraphNode> ExecutionPath { get; } // Execution path taken
    public int NodesExecuted { get; }                     // Number of nodes executed
    
    // Work management
    public DeterministicWorkQueue WorkQueue { get; }      // Deterministic work queue
}
```

### Execution Status

The `GraphExecutionStatus` enum represents the current state of execution:

```csharp
public enum GraphExecutionStatus
{
    NotStarted = 0,    // Execution has not started yet
    Running = 1,       // Execution is currently running
    Completed = 2,     // Execution completed successfully
    Failed = 3,        // Execution failed with an error
    Cancelled = 4,     // Execution was cancelled
    Paused = 5         // Execution is paused awaiting continuation
}
```

### State Management Methods

```csharp
// Execution lifecycle
public void MarkAsStarted(IGraphNode startingNode)           // Mark execution as started
public void MarkAsCompleted(FunctionResult result)           // Mark execution as completed
public void MarkAsFailed(Exception error)                    // Mark execution as failed
public void MarkAsCancelled()                                // Mark execution as cancelled

// Node tracking
public void RecordNodeStarted(IGraphNode node)               // Record node execution start
public void RecordNodeCompleted(IGraphNode node, FunctionResult result) // Record node completion
public void RecordNodeFailed(IGraphNode node, Exception exception)     // Record node failure

// Work queue management
public void EnqueueNextNodes(IEnumerable<IGraphNode> candidates) // Enqueue next nodes

// Pause/resume control
public void Pause(string reason)                             // Pause execution
public void Resume()                                          // Resume execution
public Task WaitIfPausedAsync(CancellationToken cancellationToken) // Wait if paused
```

### Property Management

```csharp
public void SetProperty<T>(string key, T value)              // Set custom property
public T? GetProperty<T>(string key)                         // Get custom property
public bool TryGetProperty<T>(string key, out T? value)      // Try get property
public void RemoveProperty(string key)                        // Remove property
```

## Execution Events

The execution system emits real-time events that provide visibility into execution progress and state changes.

### GraphExecutionEvent Base Class

All execution events inherit from `GraphExecutionEvent`:

```csharp
public abstract class GraphExecutionEvent
{
    public string EventId { get; }                          // Unique event identifier
    public string ExecutionId { get; }                      // Associated execution ID
    public DateTimeOffset Timestamp { get; }                // Event timestamp
    public abstract GraphExecutionEventType EventType { get; } // Event type
    public long HighPrecisionTimestamp { get; }             // High-precision timestamp
    public long HighPrecisionFrequency { get; }             // Timer frequency
}
```

### Event Types

The `GraphExecutionEventType` enum defines all possible event types:

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

### Key Event Classes

#### GraphExecutionStartedEvent
```csharp
public sealed class GraphExecutionStartedEvent : GraphExecutionEvent
{
    public IGraphNode StartNode { get; }                    // Starting node
    public GraphState InitialState { get; }                 // Initial graph state
}
```

#### NodeExecutionStartedEvent
```csharp
public sealed class NodeExecutionStartedEvent : GraphExecutionEvent
{
    public IGraphNode Node { get; }                         // Node that started
    public GraphState CurrentState { get; }                 // Current state
}
```

#### NodeExecutionCompletedEvent
```csharp
public sealed class NodeExecutionCompletedEvent : GraphExecutionEvent
{
    public IGraphNode Node { get; }                         // Node that completed
    public FunctionResult Result { get; }                   // Execution result
    public GraphState UpdatedState { get; }                 // Updated state
    public TimeSpan ExecutionDuration { get; }              // Execution duration
}
```

#### NodeExecutionFailedEvent
```csharp
public sealed class NodeExecutionFailedEvent : GraphExecutionEvent
{
    public IGraphNode Node { get; }                         // Node that failed
    public Exception Exception { get; }                     // Exception that occurred
    public GraphState CurrentState { get; }                 // Current state
    public TimeSpan ExecutionDuration { get; }              // Execution duration
}
```

## Execution Limits and Safeguards

The execution system provides multiple layers of protection against runaway executions and resource exhaustion.

### Execution Options

```csharp
public sealed class GraphExecutionOptions
{
    public bool EnableLogging { get; }                      // Whether logging is enabled
    public bool EnableMetrics { get; }                      // Whether metrics are enabled
    public int MaxExecutionSteps { get; }                   // Maximum execution steps
    public bool ValidateGraphIntegrity { get; }             // Whether to validate graph
    public TimeSpan ExecutionTimeout { get; }               // Overall execution timeout
    public bool EnablePlanCompilation { get; }              // Whether to compile plans
}
```

### Default Limits

```csharp
public static GraphExecutionOptions CreateDefault()
{
    return new GraphExecutionOptions(
        enableLogging: true,
        enableMetrics: true,
        maxExecutionSteps: 1000,                            // Default: 1000 steps
        validateGraphIntegrity: true,
        executionTimeout: TimeSpan.FromMinutes(10),         // Default: 10 minutes
        enablePlanCompilation: true
    );
}
```

### Execution Timeout

The system automatically enforces execution timeouts:

```csharp
// Build effective cancellation token honoring overall execution timeout
if (ExecutionOptions.ExecutionTimeout > TimeSpan.Zero)
{
    _executionTimeoutCts = new CancellationTokenSource(ExecutionOptions.ExecutionTimeout);
    var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _executionTimeoutCts.Token);
    _effectiveCancellationToken = linked.Token;
}
```

### Step Limits

Execution is automatically terminated if the maximum step count is exceeded:

```csharp
// Respect per-execution options for max steps
var maxIterations = Math.Max(1, context.ExecutionOptions.MaxExecutionSteps);
var iterations = 0;

while (currentNode != null && iterations < maxIterations)
{
    // ... execution logic
    iterations++;
}

if (iterations >= maxIterations)
{
    throw new InvalidOperationException($"Graph execution exceeded maximum steps ({maxIterations}). Possible infinite loop detected.");
}
```

## Execution Priorities

The system supports priority-based resource allocation and scheduling through the `ExecutionPriority` enum.

### Priority Levels

```csharp
public enum ExecutionPriority
{
    Low = 0,        // Low priority (consumes more resources)
    Normal = 1,     // Normal priority (default)
    High = 2,       // High priority (consumes fewer resources)
    Critical = 3    // Critical priority (highest priority)
}
```

### Priority-Based Resource Allocation

```csharp
// Determine cost and priority
var priority = context.GraphState.KernelArguments.GetExecutionPriority() ?? _resourceOptions.DefaultPriority;
var nodeCost = 1.0;

// Priority affects resource consumption
var priorityFactor = priority switch
{
    ExecutionPriority.Critical => 0.5,  // 50% resource consumption
    ExecutionPriority.High => 0.6,      // 60% resource consumption
    ExecutionPriority.Normal => 1.0,    // 100% resource consumption
    _ => 1.5                            // 150% resource consumption
};

var adjustedCost = Math.Max(0.5, workCostWeight * priorityFactor);
```

### Setting Execution Priority

Priorities can be set at the execution level:

```csharp
// Set priority for the entire execution
arguments.SetExecutionPriority(ExecutionPriority.High);

// Set priority for specific nodes
arguments.SetNodePriority("nodeId", ExecutionPriority.Critical);
```

## Deterministic Work Queue

The `DeterministicWorkQueue` provides stable, reproducible scheduling for graph execution.

### Key Features

```csharp
public sealed class DeterministicWorkQueue
{
    public string ExecutionId { get; }                      // Associated execution ID
    public int Count { get; }                               // Pending work items
    
    // Enqueue operations
    public ScheduledNodeWorkItem Enqueue(IGraphNode node, int priority = 0)
    public IReadOnlyList<ScheduledNodeWorkItem> EnqueueRange(IEnumerable<IGraphNode> nodes, int priority = 0)
    
    // Dequeue operations
    public bool TryDequeue(out ScheduledNodeWorkItem? item)
    
    // Deterministic ordering
    public IReadOnlyList<IGraphNode> OrderDeterministically(IEnumerable<IGraphNode> nodes)
    
    // Work stealing (for parallel execution)
    public IReadOnlyList<ScheduledNodeWorkItem> TryStealFrom(DeterministicWorkQueue victim, int maxItemsToSteal = 1, int minPriority = int.MinValue)
}
```

### Deterministic Ordering

The queue ensures stable ordering across executions:

```csharp
public IReadOnlyList<IGraphNode> OrderDeterministically(IEnumerable<IGraphNode> nodes)
{
    return nodes
        .Where(n => n != null)
        .OrderBy(n => n.NodeId, StringComparer.Ordinal)      // Primary: NodeId
        .ThenBy(n => n.Name, StringComparer.Ordinal)         // Secondary: Name
        .ToList()
        .AsReadOnly();
}
```

### Work Item Structure

```csharp
public sealed class ScheduledNodeWorkItem
{
    public string WorkId { get; }                           // Unique work identifier
    public string ExecutionId { get; }                      // Associated execution
    public long SequenceNumber { get; }                     // Monotonic sequence
    public IGraphNode Node { get; }                         // Node to execute
    public int Priority { get; }                            // Execution priority
    public DateTimeOffset ScheduledAt { get; }              // Scheduling timestamp
}
```

## Resource Governance

The execution context integrates with resource governance to manage CPU, memory, and API quotas.

### Resource Acquisition

```csharp
// Acquire resource permits before node execution
using var lease = _resourceGovernor != null
    ? await _resourceGovernor.AcquireAsync(nodeCost, priority, context.CancellationToken)
    : default;

// Resource permits are automatically released when the lease is disposed
```

### Resource Options

```csharp
public sealed class GraphResourceOptions
{
    public bool EnableResourceGovernance { get; set; }      // Enable/disable governance
    public double CpuHighWatermarkPercent { get; set; }     // CPU threshold (default: 85%)
    public double CpuSoftLimitPercent { get; set; }         // Soft CPU limit (default: 70%)
    public double MinAvailableMemoryMB { get; set; }        // Min memory (default: 512MB)
    public double BasePermitsPerSecond { get; set; }        // Base rate (default: 50/s)
    public int MaxBurstSize { get; set; }                   // Max burst (default: 100)
    public ExecutionPriority DefaultPriority { get; set; }  // Default priority
}
```

## Usage Examples

### Basic Execution Context

```csharp
// Create execution context
var graphState = arguments.GetOrCreateGraphState();
var context = new GraphExecutionContext(kernel, graphState, cancellationToken);

// Monitor execution progress
Console.WriteLine($"Execution {context.ExecutionId} started at {context.StartTime}");
Console.WriteLine($"Current status: {context.Status}");
Console.WriteLine($"Nodes executed: {context.NodesExecuted}");

// Check execution limits
if (context.ExecutionOptions.MaxExecutionSteps > 0)
{
    Console.WriteLine($"Max steps: {context.ExecutionOptions.MaxExecutionSteps}");
}
if (context.ExecutionOptions.ExecutionTimeout > TimeSpan.Zero)
{
    Console.WriteLine($"Timeout: {context.ExecutionOptions.ExecutionTimeout}");
}
```

### Setting Execution Priority

```csharp
// Set high priority for critical operations
arguments.SetExecutionPriority(ExecutionPriority.High);

// Set specific node priorities
arguments.SetNodePriority("dataProcessing", ExecutionPriority.Critical);
arguments.SetNodePriority("logging", ExecutionPriority.Low);
```

### Monitoring Execution Events

```csharp
// Subscribe to execution events
eventStream.EventEmitted += (sender, @event) =>
{
    switch (@event)
    {
        case NodeExecutionStartedEvent started:
            Console.WriteLine($"Node {started.Node.Name} started executing");
            break;
            
        case NodeExecutionCompletedEvent completed:
            Console.WriteLine($"Node {completed.Node.Name} completed in {completed.ExecutionDuration}");
            break;
            
        case NodeExecutionFailedEvent failed:
            Console.WriteLine($"Node {failed.Node.Name} failed: {failed.Exception.Message}");
            break;
    }
};
```

### Resource-Aware Execution

```csharp
// Configure resource governance
var resourceOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    CpuHighWatermarkPercent = 80.0,
    MinAvailableMemoryMB = 1024.0,
    DefaultPriority = ExecutionPriority.Normal
};

// The executor will automatically manage resource allocation
// based on node costs and priorities
```

## See Also

- [Execution Model](../concepts/execution-model.md) - Core execution concepts and lifecycle
- [Resource Governance](../how-to/resource-governance-and-concurrency.md) - Resource management and concurrency
- [Streaming Execution](../concepts/streaming.md) - Real-time execution monitoring
- [Error Handling](../how-to/error-handling-and-resilience.md) - Error policies and recovery
- [GraphExecutor API](./graph-executor.md) - Main executor interface
