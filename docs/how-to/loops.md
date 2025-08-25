# Loops

This guide explains how to implement iterative workflows using loop nodes in SemanticKernel.Graph. You'll learn how to create while loops, foreach loops, and implement proper loop control with iteration limits and break/continue logic.

## Overview

Loop nodes enable you to create iterative workflows that can:
- **Repeat operations** until conditions are met
- **Process collections** of data items
- **Implement retry logic** with exponential backoff
- **Control iteration flow** with break and continue statements

## Basic Loop Types

### While Loops

Use `WhileLoopGraphNode` to create loops that continue while a condition is true. The snippets below use the documented runtime types and minimal helper APIs so they are runnable when placed in an examples project.

```csharp
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;

// Create a while loop with iteration limits
var whileNode = new WhileLoopGraphNode(
    condition: state => state.GetValue<int>("attempt") < 3,
    maxIterations: 5,
    nodeId: "retry_loop"
);

// Add the loop to your graph executor or graph representation
graph.AddNode(whileNode)
     .AddEdge("start", "retry_loop")
     .AddEdge("retry_loop", "process")
     .AddEdge("process", "retry_loop"); // Loop back
```

### ForEach Loops

Use `ForeachLoopGraphNode` to iterate over collections. The node accepts a collection accessor (or a template) and exposes the current item and index in the graph state each iteration.

```csharp
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;

var foreachNode = new ForeachLoopGraphNode(
    collectionAccessor: state => state.GetValue<IEnumerable<object>>("items_to_process"),
    itemVariableName: "current_item",
    indexVariableName: "current_index",
    maxIterations: 100,
    nodeId: "process_items"
);

graph.AddNode(foreachNode)
     .AddEdge("start", "process_items")
     .AddEdge("process_items", "process_single_item")
     .AddEdge("process_single_item", "process_items"); // Continue loop
```

## Loop Control Patterns

### Break and Continue Logic

Implement break and continue logic by evaluating state variables and wiring conditional edges. Use `GetValue<T>` to read typed values from `GraphState`.

```csharp
var whileNode = new WhileLoopGraphNode(
    condition: state =>
    {
        var attempt = state.GetValue<int>("attempt");
        var maxAttempts = state.GetValue<int>("max_attempts");
        var shouldContinue = state.GetValue<bool>("should_continue");

        return attempt < maxAttempts && shouldContinue;
    },
    maxIterations: 10,
    nodeId: "controlled_loop"
);

// Add conditional routing based on state
graph.AddConditionalEdge("controlled_loop", "break_loop",
    condition: state => !state.GetValue<bool>("should_continue"))
     .AddConditionalEdge("controlled_loop", "continue_loop",
    condition: state => state.GetValue<bool>("should_continue"));
```

### Iteration Counting

Track and control iteration progress. Read and update iteration counters from the `GraphState` in a `FunctionGraphNode`.

```csharp
var loopNode = new WhileLoopGraphNode(
    condition: state =>
    {
        var iteration = state.GetValue<int>("iteration");
        var maxIterations = state.GetValue<int>("max_iterations");
        var hasMoreWork = state.GetValue<bool>("has_more_work");

        return iteration < maxIterations && hasMoreWork;
    },
    maxIterations: 15,
    nodeId: "counting_loop"
);

// Increment counter in each iteration (implemented in a FunctionGraphNode)
graph.AddEdge("counting_loop", "increment_counter")
     .AddEdge("increment_counter", "process_work")
     .AddEdge("process_work", "check_work_status")
     .AddEdge("check_work_status", "counting_loop");
```

## Advanced Loop Patterns

### Retry Loops with Backoff

Implement retry logic with backoff. Use typed accessors and helper function nodes to compute delay.

```csharp
var retryNode = new WhileLoopGraphNode(
    condition: state =>
    {
        var attempt = state.GetValue<int>("attempt");
        var maxAttempts = state.GetValue<int>("max_attempts");
        var lastError = state.GetValue<string>("last_error");
        var isRetryable = IsRetryableError(lastError);

        return attempt < maxAttempts && isRetryable;
    },
    maxIterations: 10,
    nodeId: "retry_with_backoff"
);

// Calculate backoff delay using a function node
var backoffNode = new FunctionGraphNode(
    kernelFunction: CalculateBackoffDelay,
    nodeId: "calculate_backoff"
);

graph.AddNode(retryNode)
     .AddNode(backoffNode)
     .AddEdge("retry_with_backoff", "attempt_operation")
     .AddEdge("attempt_operation", "check_result")
     .AddConditionalEdge("check_result", "success",
        condition: state => state.GetValue<bool>("operation_succeeded"))
     .AddConditionalEdge("check_result", "calculate_backoff",
        condition: state => !state.GetValue<bool>("operation_succeeded"))
     .AddEdge("calculate_backoff", "wait_delay")
     .AddEdge("wait_delay", "retry_with_backoff");
```

### Batch Processing Loops

Process data in batches with loop control. Use `GraphState` to track progress and a function node to process each batch.

```csharp
var batchLoop = new WhileLoopGraphNode(
    condition: state =>
    {
        var processedCount = state.GetValue<int>("processed_count");
        var totalCount = state.GetValue<int>("total_count");
        var batchSize = state.GetValue<int>("batch_size");

        return processedCount < totalCount;
    },
    maxIterations: 1000,
    nodeId: "batch_processor"
);

var batchProcessor = new FunctionGraphNode(
    kernelFunction: ProcessBatch,
    nodeId: "process_batch"
);

graph.AddNode(batchLoop)
     .AddNode(batchProcessor)
     .AddEdge("batch_processor", "process_batch")
     .AddEdge("process_batch", "update_progress")
     .AddEdge("update_progress", "batch_processor");
```

### Nested Loops

Implement nested loop structures for complex processing by composing loop nodes.

```csharp
var outerLoop = new WhileLoopGraphNode(
    condition: state => state.GetValue<int>("outer_iteration") < 10,
    maxIterations: 15,
    nodeId: "outer_loop"
);

var innerLoop = new WhileLoopGraphNode(
    condition: state => state.GetValue<int>("inner_iteration") < 5,
    maxIterations: 10,
    nodeId: "inner_loop"
);

graph.AddNode(outerLoop)
     .AddNode(innerLoop)
     .AddEdge("outer_loop", "inner_loop")
     .AddEdge("inner_loop", "process_item")
     .AddEdge("process_item", "inner_loop")
     .AddEdge("inner_loop", "outer_loop");
```

## Loop State Management

### State Initialization

Properly initialize loop state variables using a function node that sets up required keys in `GraphState`.

```csharp
var initializeLoop = new FunctionGraphNode(
    kernelFunction: (KernelArguments args) =>
    {
        var state = args.GetOrCreateGraphState();
        state.SetValue("iteration", 0);
        state.SetValue("max_iterations", 10);
        state.SetValue("accumulator", 0);
        state.SetValue("items_processed", new List<string>());
        return "Loop initialized";
    },
    nodeId: "initialize_loop"
);

var loopNode = new WhileLoopGraphNode(
    condition: state => state.GetValue<int>("iteration") < state.GetValue<int>("max_iterations"),
    maxIterations: 15,
    nodeId: "main_loop"
);

graph.AddNode(initializeLoop)
     .AddNode(loopNode)
     .AddEdge("start", "initialize_loop")
     .AddEdge("initialize_loop", "main_loop");
```

### State Updates

Update loop state in each iteration using a function node that reads/writes typed values.

```csharp
var updateState = new FunctionGraphNode(
    kernelFunction: (KernelArguments args) =>
    {
        var state = args.GetOrCreateGraphState();
        var currentIteration = state.GetValue<int>("iteration");
        state.SetValue("iteration", currentIteration + 1);

        var currentAccumulator = state.GetValue<int>("accumulator");
        var newValue = state.GetValue<int>("current_value");
        state.SetValue("accumulator", currentAccumulator + newValue);

        return $"Iteration {currentIteration + 1} completed";
    },
    nodeId: "update_state"
);

graph.AddEdge("main_loop", "process_work")
     .AddEdge("process_work", "update_state")
     .AddEdge("update_state", "main_loop");
```

## Performance Optimization

### Loop Efficiency

Optimize loop performance with proper state management and caching of expensive computations.

```csharp
var optimizedLoop = new WhileLoopGraphNode(
    condition: state =>
    {
        // Cache expensive calculations
        if (state.TryGetValue("cached_condition", out var cached) && cached is bool cachedBool)
        {
            return cachedBool;
        }

        var result = EvaluateComplexCondition(state);
        state.SetValue("cached_condition", result);
        return result;
    },
    maxIterations: 50,
    nodeId: "optimized_loop"
);
```

### Batch Processing

Process multiple items in each iteration:

```csharp
var batchLoop = new WhileLoopGraphNode(
    condition: state =>
    {
        var remainingItems = state.GetValue<List<string>>("remaining_items");
        return remainingItems != null && remainingItems.Count > 0;
    },
    maxIterations: 100,
    nodeId: "efficient_batch_loop"
);

var batchProcessor = new FunctionGraphNode(
    kernelFunction: (KernelArguments args) =>
    {
        var state = args.GetOrCreateGraphState();
        var remainingItems = state.GetValue<List<string>>("remaining_items") ?? new List<string>();
        var batchSize = Math.Min(10, remainingItems.Count);
        var batch = remainingItems.Take(batchSize).ToList();

        // Process batch
        var results = ProcessBatch(batch);

        // Update remaining items
        state.SetValue("remaining_items", remainingItems.Skip(batchSize).ToList());
        state.SetValue("processed_results", results);

        return $"Processed {batch.Count} items";
    },
    nodeId: "process_batch"
);
```

## Error Handling in Loops

### Loop Error Recovery

Handle errors gracefully within loops by routing to an error handler node and applying recovery strategies.

```csharp
var errorHandlingLoop = new WhileLoopGraphNode(
    condition: state =>
    {
        var attempt = state.GetValue<int>("attempt");
        var maxAttempts = state.GetValue<int>("max_attempts");
        var lastError = state.GetValue<string>("last_error");

        return attempt < maxAttempts && !string.IsNullOrEmpty(lastError);
    },
    maxIterations: 10,
    nodeId: "error_recovery_loop"
);

var errorHandler = new ErrorHandlerGraphNode(
    errorTypes: new[] { ErrorType.Transient, ErrorType.External },
    recoveryAction: RecoveryAction.Retry,
    maxRetries: 2,
    nodeId: "loop_error_handler"
);

graph.AddNode(errorHandlingLoop)
     .AddNode(errorHandler)
     .AddEdge("error_recovery_loop", "attempt_operation")
     .AddEdge("attempt_operation", "loop_error_handler")
     .AddEdge("loop_error_handler", "error_recovery_loop");
```

### Graceful Degradation

Implement fallback logic when loops fail:

```csharp
var fallbackLoop = new WhileLoopGraphNode(
    condition: state =>
    {
        var primaryMethodFailed = state.GetValue<bool>("primary_method_failed");
        var fallbackAttempts = state.GetValue<int>("fallback_attempts");
        var maxFallbackAttempts = state.GetValue<int>("max_fallback_attempts");

        return primaryMethodFailed && fallbackAttempts < maxFallbackAttempts;
    },
    maxIterations: 5,
    nodeId: "fallback_loop"
);

graph.AddConditionalEdge("attempt_primary", "primary_success",
    condition: state => state.GetValue<bool>("primary_succeeded"))
    .AddConditionalEdge("attempt_primary", "fallback_loop",
    condition: state => !state.GetValue<bool>("primary_succeeded"))
    .AddEdge("fallback_loop", "attempt_fallback")
    .AddEdge("attempt_fallback", "fallback_loop");
```

## Monitoring and Metrics

### Loop Performance Tracking

Monitor loop performance and efficiency:

```csharp
var monitoredLoop = new WhileLoopGraphNode(
    condition: state =>
    {
        var startTime = state.GetValue<DateTimeOffset>("loop_start_time");
        var maxDuration = TimeSpan.FromMinutes(5);
        var elapsed = DateTimeOffset.UtcNow - startTime;

        // Track loop metrics
        var iteration = state.GetValue<int>("iteration");
        state.SetValue("loop_metrics", new {
            Iteration = iteration,
            ElapsedTime = elapsed,
            AverageTimePerIteration = elapsed.TotalMilliseconds / Math.Max(1, iteration)
        });

        return elapsed < maxDuration && state.GetValue<bool>("should_continue");
    },
    maxIterations: 100,
    nodeId: "monitored_loop"
);
```

### Loop Health Checks

Implement health checks for long-running loops:

```csharp
var healthCheckLoop = new WhileLoopGraphNode(
    condition: state =>
    {
        var iteration = state.GetValue<int>("iteration");
        var lastHealthCheck = state.GetValue<DateTimeOffset>("last_health_check");
        var healthCheckInterval = TimeSpan.FromSeconds(30);

        // Perform health check if needed
        if (DateTimeOffset.UtcNow - lastHealthCheck > healthCheckInterval)
        {
            var isHealthy = PerformHealthCheck(state);
            state.SetValue("last_health_check", DateTimeOffset.UtcNow);
            state.SetValue("is_healthy", isHealthy);

            if (!isHealthy)
            {
                state.SetValue("health_check_failed", true);
                return false; // Exit loop on health check failure
            }
        }

        return state.GetValue<bool>("should_continue");
    },
    maxIterations: 1000,
    nodeId: "health_checked_loop"
);
```

## Best Practices

### Loop Design

1. **Set iteration limits** - Always define `maxIterations` to prevent infinite loops
2. **Initialize state properly** - Set up loop variables before entering the loop
3. **Handle edge cases** - Provide fallback paths for unexpected conditions
4. **Monitor performance** - Track loop efficiency and resource usage

### State Management

1. **Update state consistently** - Ensure loop variables are updated in each iteration
2. **Cache expensive operations** - Store results to avoid recalculation
3. **Clean up resources** - Properly dispose of resources used in loops
4. **Validate state changes** - Verify that state updates are correct

### Error Handling

1. **Implement retry logic** - Handle transient failures gracefully
2. **Set timeout limits** - Prevent loops from running indefinitely
3. **Log loop progress** - Track iteration progress for debugging
4. **Provide fallbacks** - Have alternative paths when loops fail

## Troubleshooting

### Common Issues

**Infinite loops**: Check loop conditions and ensure they eventually become false

**Performance degradation**: Monitor iteration times and optimize expensive operations

**State corruption**: Validate state updates and implement proper error handling

**Memory leaks**: Clean up resources and avoid accumulating data in loops

### Debugging Tips

1. **Enable loop logging** to trace iteration progress
2. **Add breakpoints** in loop conditions and state updates
3. **Monitor loop metrics** for performance issues
4. **Use loop visualization** to understand execution flow

## Concepts and Techniques

**WhileLoopNode**: A specialized graph node that creates loops that continue while a specified condition is true. It enables iterative workflows with proper iteration limits and state management.

**ForeachLoopNode**: A graph node that iterates over collections of data items. It processes each item in the collection and maintains iteration state throughout the process.

**Loop Control**: The mechanisms for controlling loop execution, including break and continue logic, iteration limits, and state management within iterative workflows.

**Iteration Limits**: Safety mechanisms that prevent infinite loops by setting maximum iteration counts and timeout constraints for loop execution.

**Loop State Management**: The practice of properly initializing, updating, and maintaining state variables throughout loop execution to ensure correct behavior and performance.

## See Also

- [Build a Graph](build-a-graph.md) - Learn the fundamentals of graph construction
- [Conditional Nodes](conditional-nodes.md) - Implement branching logic in your workflows
- [Advanced Routing](advanced-routing.md) - Explore complex routing patterns
- [State Management](../state-quickstart.md) - Understand how to manage data flow between nodes
- [Error Handling and Resilience](error-handling-and-resilience.md) - Learn about error handling in loops
- [Examples: Loop Workflows](../examples/loop-nodes-example.md) - Complete working examples of loop implementations
