# Parallelism and Fork/Join

Parallelism and fork/join in SemanticKernel.Graph provide sophisticated mechanisms for executing multiple graph branches concurrently while maintaining deterministic behavior and proper state management. This system enables efficient parallel processing with configurable merge strategies and reproducible execution patterns.

## What You'll Learn

- How to enable and configure parallel execution in graphs
- Understanding deterministic work scheduling and ordering
- Configuring state merge strategies and conflict resolution policies
- Implementing work-stealing for load balancing
- Ensuring reproducible execution with stable seeds
- Best practices for parallel graph design and performance optimization

## Concepts and Techniques

**DeterministicWorkQueue**: Thread-safe work queue that maintains stable, monotonic work item IDs and deterministic ordering for reproducible execution across runs.

**Fork/Join Execution**: Automatic parallel execution of multiple next nodes when available, with state cloning, concurrent processing, and deterministic state merging.

**StateMergeConflictPolicy**: Configurable policies for resolving conflicts when merging states from parallel branches (PreferFirst, PreferSecond, LastWriteWins, Reduce, CRDT-like).

**StateMergeConfiguration**: Per-key and per-type merge strategies that allow fine-grained control over how different parts of the state are combined.

**Work Stealing**: Load balancing mechanism where idle workers can steal work from busy queues while maintaining execution determinism.

**Execution Seed**: Stable random seed that ensures reproducible behavior across executions, enabling reliable replay and debugging.

## Prerequisites

- [First Graph Tutorial](../first-graph-5-minutes.md) completed
- Basic understanding of graph execution concepts
- Familiarity with state management and conditional routing
- Understanding of concurrent programming concepts

## Enabling Parallel Execution

### Basic Parallel Configuration

Enable parallel execution when multiple next nodes are available:

```csharp
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;

// Create executor with parallel execution enabled
var executor = new GraphExecutor("ParallelGraph", "Graph with parallel execution")
    .ConfigureConcurrency(new GraphConcurrencyOptions
    {
        EnableParallelExecution = true,
        MaxDegreeOfParallelism = Environment.ProcessorCount,
        MergeConflictPolicy = StateMergeConflictPolicy.PreferSecond,
        FallbackToSequentialOnCycles = true
    });

// Add nodes and connections
var startNode = new FunctionGraphNode("start", "Start");
var branchA = new FunctionGraphNode("branchA", "Branch A");
var branchB = new FunctionGraphNode("branchB", "Branch B");
var joinNode = new FunctionGraphNode("join", "Join");

// Create fork/join structure
executor.AddNode(startNode)
    .AddNode(branchA)
    .AddNode(branchB)
    .AddNode(joinNode)
    .Connect("start", "branchA")
    .Connect("start", "branchB")
    .Connect("branchA", "join")
    .Connect("branchB", "join")
    .SetStartNode("start");
```

### Advanced Concurrency Options

Configure detailed parallel execution behavior:

```csharp
// Configure advanced concurrency options
var concurrencyOptions = new GraphConcurrencyOptions
{
    EnableParallelExecution = true,
    MaxDegreeOfParallelism = 4, // Limit to 4 concurrent branches
    MergeConflictPolicy = StateMergeConflictPolicy.LastWriteWins,
    FallbackToSequentialOnCycles = true
};

executor.ConfigureConcurrency(concurrencyOptions);

// Configure resource governance for parallel execution
executor.ConfigureResources(new GraphResourceOptions
{
    EnableResourceGovernance = true,
    MaxBurstSize = 2, // Allow burst of 2 parallel executions
    BasePermitsPerSecond = 10
});
```

## Deterministic Work Scheduling

### Work Queue Management

The deterministic work queue ensures stable execution ordering:

```csharp
using SemanticKernel.Graph.Execution;

// Create execution context with deterministic work queue
var context = new GraphExecutionContext(kernelWrapper, graphState, cancellationToken);

// The work queue automatically orders nodes deterministically
var nextNodes = context.WorkQueue.OrderDeterministically(availableNodes);

// Enqueue work items with stable IDs
foreach (var node in nextNodes)
{
    var workItem = context.WorkQueue.Enqueue(node, priority: 0);
    Console.WriteLine($"Enqueued: {workItem.WorkId} for node {node.NodeId}");
}
```

### Work Stealing for Load Balancing

Enable work stealing to balance load across parallel workers:

```csharp
// Configure work stealing in the executor
var executor = new GraphExecutor("WorkStealingGraph")
    .ConfigureConcurrency(new GraphConcurrencyOptions
    {
        EnableParallelExecution = true,
        MaxDegreeOfParallelism = 4
    });

// The executor automatically handles work stealing between parallel branches
// Workers can steal work from busy queues while maintaining determinism
```

## State Merging and Conflict Resolution

### Basic State Merging

Configure how states are merged when parallel branches complete:

```csharp
using SemanticKernel.Graph.State;

// Configure merge policy for the entire graph
executor.ConfigureConcurrency(new GraphConcurrencyOptions
{
    EnableParallelExecution = true,
    MergeConflictPolicy = StateMergeConflictPolicy.PreferSecond
});

// Or configure per-edge merge strategies
var edge = new ConditionalEdge(startNode, branchA, "true");
edge.MergeConfiguration = new StateMergeConfiguration
{
    DefaultPolicy = StateMergeConflictPolicy.LastWriteWins,
    KeyPolicies = 
    {
        ["critical_data"] = StateMergeConflictPolicy.ThrowOnConflict,
        ["accumulator"] = StateMergeConflictPolicy.Reduce
    }
};
```

### Advanced Merge Strategies

Configure sophisticated merge strategies for different data types:

```csharp
// Create comprehensive merge configuration
var mergeConfig = new StateMergeConfiguration
{
    DefaultPolicy = StateMergeConflictPolicy.PreferSecond,
    
    // Per-key policies
    KeyPolicies = 
    {
        ["user_id"] = StateMergeConflictPolicy.PreferFirst, // Never change user ID
        ["transaction_count"] = StateMergeConflictPolicy.Reduce, // Sum up counts
        ["status"] = StateMergeConflictPolicy.LastWriteWins, // Latest status wins
        ["metadata"] = StateMergeConflictPolicy.CrdtLike // Merge metadata intelligently
    },
    
    // Per-type policies
    TypePolicies = 
    {
        [typeof(int)] = StateMergeConflictPolicy.Reduce, // Sum integers
        [typeof(List<string>)] = StateMergeConflictPolicy.CrdtLike, // Union lists
        [typeof(Dictionary<string, object>)] = StateMergeConflictPolicy.CrdtLike // Merge dictionaries
    }
};

// Apply to specific edge
var edge = new ConditionalEdge(branchA, joinNode, "true");
edge.MergeConfiguration = mergeConfig;
```

### Conflict Detection and Resolution

Handle merge conflicts explicitly when needed:

```csharp
using SemanticKernel.Graph.State;

// Merge states with conflict detection
var mergeResult = StateHelpers.MergeStatesWithConflictDetection(
    baseState, 
    overlayState, 
    mergeConfig, 
    detectConflicts: true);

if (mergeResult.HasConflicts)
{
    Console.WriteLine($"Detected {mergeResult.Conflicts.Count} conflicts:");
    foreach (var conflict in mergeResult.Conflicts)
    {
        Console.WriteLine($"  Key: {conflict.Key}");
        Console.WriteLine($"    Base: {conflict.BaseValue}");
        Console.WriteLine($"    Overlay: {conflict.OverlayValue}");
        Console.WriteLine($"    Policy: {conflict.Policy}");
        Console.WriteLine($"    Resolved: {conflict.WasResolved}");
    }
}

// Use the merged state
var finalState = mergeResult.MergedState;
```

## Parallel Execution Patterns

### Simple Fork/Join

Create a basic parallel execution pattern:

```csharp
// Create nodes that will execute in parallel
var startNode = new FunctionGraphNode("start", "Start");
var parallelA = new FunctionGraphNode("parallelA", "Parallel A");
var parallelB = new FunctionGraphNode("parallelB", "Parallel B");
var joinNode = new FunctionGraphNode("join", "Join");

// Configure parallel execution
parallelA.SetMetadata("AfterExecute", (Action<Kernel, KernelArguments, FunctionResult>)((k, ka, r) => 
{
    ka["result_a"] = "Result from A";
    ka["timestamp_a"] = DateTimeOffset.UtcNow;
}));

parallelB.SetMetadata("AfterExecute", (Action<Kernel, KernelArguments, FunctionResult>)((k, ka, r) => 
{
    ka["result_b"] = "Result from B";
    ka["timestamp_b"] = DateTimeOffset.UtcNow;
}));

// Build graph with parallel execution
var executor = new GraphExecutor("SimpleForkJoin")
    .AddNode(startNode)
    .AddNode(parallelA)
    .AddNode(parallelB)
    .AddNode(joinNode)
    .Connect("start", "parallelA")
    .Connect("start", "parallelB")
    .Connect("parallelA", "join")
    .Connect("parallelB", "join")
    .SetStartNode("start")
    .ConfigureConcurrency(new GraphConcurrencyOptions
    {
        EnableParallelExecution = true,
        MaxDegreeOfParallelism = 2
    });
```

### Conditional Parallel Execution

Execute branches conditionally based on state:

```csharp
// Create conditional parallel execution
var startNode = new FunctionGraphNode("start", "Start");
var conditionNode = new ConditionalGraphNode("condition", "Check Condition");
var branchA = new FunctionGraphNode("branchA", "Branch A");
var branchB = new FunctionGraphNode("branchB", "Branch B");
var joinNode = new FunctionGraphNode("join", "Join");

// Configure conditional routing
conditionNode.AddCondition("value > 100", branchA);
conditionNode.AddCondition("value <= 100", branchB);

// Both branches connect to join node
executor.Connect("branchA", "join");
executor.Connect("branchB", "join");

// Configure merge strategy for the join
var joinEdge = new ConditionalEdge(branchA, joinNode, "true");
joinEdge.MergeConfiguration = new StateMergeConfiguration
{
    DefaultPolicy = StateMergeConflictPolicy.LastWriteWins,
    KeyPolicies = 
    {
        ["processed_value"] = StateMergeConflictPolicy.Reduce
    }
};
```

### Complex Parallel Workflows

Build sophisticated parallel workflows with multiple join points:

```csharp
// Create complex parallel workflow
var startNode = new FunctionGraphNode("start", "Start");
var parallel1 = new FunctionGraphNode("parallel1", "Parallel 1");
var parallel2 = new FunctionGraphNode("parallel2", "Parallel 2");
var parallel3 = new FunctionGraphNode("parallel3", "Parallel 3");
var join1 = new FunctionGraphNode("join1", "Join 1");
var parallel4 = new FunctionGraphNode("parallel4", "Parallel 4");
var finalJoin = new FunctionGraphNode("finalJoin", "Final Join");

// First level parallel execution
executor.Connect("start", "parallel1");
executor.Connect("start", "parallel2");
executor.Connect("start", "parallel3");

// First join point
executor.Connect("parallel1", "join1");
executor.Connect("parallel2", "join1");
executor.Connect("parallel3", "join1");

// Second level parallel execution
executor.Connect("join1", "parallel4");
executor.Connect("parallel3", "parallel4"); // Direct connection

// Final join
executor.Connect("parallel4", "finalJoin");

// Configure different merge strategies for different join points
var join1Edge = new ConditionalEdge(parallel1, join1, "true");
join1Edge.MergeConfiguration = new StateMergeConfiguration
{
    DefaultPolicy = StateMergeConflictPolicy.PreferSecond,
    KeyPolicies = 
    {
        ["intermediate_result"] = StateMergeConflictPolicy.Reduce
    }
};

var finalJoinEdge = new ConditionalEdge(parallel4, finalJoin, "true");
finalJoinEdge.MergeConfiguration = new StateMergeConfiguration
{
    DefaultPolicy = StateMergeConflictPolicy.LastWriteWins
};
```

## Reproducible Execution

### Execution Seeds

Ensure reproducible behavior across executions:

```csharp
// Create execution context with stable seed
var executionSeed = 42; // Fixed seed for reproducible execution
var context = new GraphExecutionContext(
    kernelWrapper, 
    graphState, 
    cancellationToken, 
    executionSeed);

// The same seed will produce identical execution patterns
// Useful for debugging, testing, and deterministic workflows
```

### Replay and Debugging

Use deterministic execution for reliable replay:

```csharp
// Enable detailed logging for replay
var graphOptions = new GraphOptions
{
    LogLevel = LogLevel.Debug,
    EnableExecutionTracing = true,
    EnableDeterministicLogging = true
};

var graphLogger = new SemanticKernelGraphLogger(logger, graphOptions);

// Execute with the same seed multiple times
for (int i = 0; i < 3; i++)
{
    var replayContext = new GraphExecutionContext(
        kernelWrapper, 
        graphState.Clone(), 
        cancellationToken, 
        executionSeed: 42);
    
    var result = await executor.ExecuteAsync(replayContext);
    
    // Results should be identical across runs
    Console.WriteLine($"Replay {i + 1}: {result.Result}");
}
```

## Performance Optimization

### Parallelism Tuning

Optimize parallel execution for your workload:

```csharp
// Profile and tune parallelism settings
var optimizedOptions = new GraphConcurrencyOptions
{
    EnableParallelExecution = true,
    MaxDegreeOfParallelism = Environment.ProcessorCount * 2, // Over-subscribe for I/O bound work
    MergeConflictPolicy = StateMergeConflictPolicy.PreferSecond, // Fastest merge policy
    FallbackToSequentialOnCycles = false // Disable if you know your graph is acyclic
};

executor.ConfigureConcurrency(optimizedOptions);

// Monitor performance metrics
var metrics = await executor.GetPerformanceMetricsAsync();
Console.WriteLine($"Parallel branches executed: {metrics.ParallelBranchesExecuted}");
Console.WriteLine($"Average merge time: {metrics.AverageStateMergeTime}");
Console.WriteLine($"Total execution time: {metrics.TotalExecutionTime}");
```

### Resource-Aware Parallel Execution

Balance parallelism with resource constraints:

```csharp
// Configure resource-aware parallel execution
executor.ConfigureResources(new GraphResourceOptions
{
    EnableResourceGovernance = true,
    MaxBurstSize = 4, // Allow burst of parallel executions
    BasePermitsPerSecond = 20, // Rate limit overall execution
    EnableCostTracking = true
});

// Set node-specific resource costs
parallelA.SetMetadata("ResourceCost", 2); // Higher cost
parallelB.SetMetadata("ResourceCost", 1); // Lower cost

// The resource governor will throttle based on costs and limits
```

## Best Practices

### Parallel Graph Design

- **Keep Branches Independent**: Design parallel branches to minimize shared state and dependencies
- **Balanced Workload**: Ensure parallel branches have similar execution times for optimal performance
- **Clear Join Points**: Define explicit join nodes where parallel execution converges
- **State Isolation**: Use state cloning to prevent write conflicts between parallel branches

### Merge Strategy Selection

- **PreferSecond**: Use for most cases where latest data should win
- **PreferFirst**: Use for immutable or reference data that shouldn't change
- **LastWriteWins**: Use when temporal ordering is important
- **Reduce**: Use for accumulators, counters, and aggregations
- **CRDT-like**: Use for complex data structures that can be intelligently merged
- **ThrowOnConflict**: Use for critical data where conflicts indicate design issues

### Performance Considerations

- **Monitor Resource Usage**: Track CPU, memory, and I/O usage during parallel execution
- **Tune Parallelism**: Adjust MaxDegreeOfParallelism based on your system capabilities
- **Profile Merge Operations**: Ensure state merging doesn't become a bottleneck
- **Use Work Stealing**: Enable work stealing for better load balancing in heterogeneous workloads

### Debugging and Testing

- **Use Stable Seeds**: Always use fixed execution seeds for reproducible debugging
- **Enable Tracing**: Use execution tracing to understand parallel execution flow
- **Test Edge Cases**: Test with various merge policies and conflict scenarios
- **Monitor Metrics**: Track parallel execution metrics for performance analysis

## Troubleshooting

### Common Issues

**Parallel execution not working**: Check that `EnableParallelExecution` is true and multiple next nodes exist.

**State conflicts during merge**: Verify merge policy configuration and ensure appropriate conflict resolution strategies.

**Non-deterministic behavior**: Use stable execution seeds and ensure all random operations use the seeded random number generator.

**Performance degradation**: Monitor resource usage and adjust `MaxDegreeOfParallelism` based on system capabilities.

**Memory issues**: Ensure proper state cloning and cleanup in parallel branches.

### Debugging Parallel Execution

Enable detailed logging for troubleshooting:

```csharp
// Configure detailed parallel execution logging
var graphOptions = new GraphOptions
{
    LogLevel = LogLevel.Debug,
    EnableExecutionTracing = true,
    EnableParallelExecutionLogging = true,
    EnableStateMergeLogging = true
};

var graphLogger = new SemanticKernelGraphLogger(logger, graphOptions);

// Monitor parallel execution events
using var eventStream = executor.CreateStreamingExecutor().CreateEventStream();

eventStream.SubscribeToEvents<GraphExecutionEvent>(event =>
{
    if (event.EventType == GraphExecutionEventType.ParallelBranchStarted)
    {
        var parallelEvent = event as ParallelBranchStartedEvent;
        Console.WriteLine($"Parallel branch started: {parallelEvent.BranchId}");
    }
    
    if (event.EventType == GraphExecutionEventType.StateMergeCompleted)
    {
        var mergeEvent = event as StateMergeCompletedEvent;
        Console.WriteLine($"State merge completed: {mergeEvent.Conflicts.Count} conflicts resolved");
    }
});
```

## See Also

- [Graph Execution](execution.md) - Understanding the execution lifecycle and parallel processing
- [State Management](state-quickstart.md) - Managing state in parallel execution scenarios
- [Error Handling and Resilience](error-handling-and-resilience.md) - Handling failures in parallel branches
- [Resource Governance](resource-governance-and-concurrency.md) - Managing resources during parallel execution
- [Streaming Execution](streaming-quickstart.md) - Real-time monitoring of parallel execution
