# Execution Model

This guide explains how the `GraphExecutor` orchestrates graph execution, including the complete lifecycle, navigation logic, execution limits, and built-in safeguards against infinite loops.

## Overview

The `GraphExecutor` is the central orchestrator that manages the complete execution flow of a graph. It handles node lifecycle management, navigation between nodes, parallel execution, error recovery, and provides comprehensive safeguards to ensure reliable and predictable execution.

## Execution Lifecycle

### 1. Execution Initialization

Before execution begins, the executor performs several setup steps:

```csharp
// Create execution context with immutable options snapshot
var graphState = arguments.GetOrCreateGraphState();
var context = new GraphExecutionContext(kernel, graphState, cancellationToken, arguments.GetExecutionSeed());

// Validate graph integrity (optional, controlled by per-execution options)
if (context.ExecutionOptions.ValidateGraphIntegrity)
{
    var validationResult = ValidateGraphIntegrity();
    if (!validationResult.IsValid)
    {
        throw new InvalidOperationException($"Graph validation failed: {validationResult.CreateSummary()}");
    }
}

// Enable plan compilation and caching (optional)
if (context.ExecutionOptions.EnablePlanCompilation)
{
    _ = GraphPlanCompiler.ComputeSignature(this);
    _ = GraphExecutionPlanCache.GetOrAdd(this);
}
```

### 2. Node Execution Lifecycle

Each node follows a consistent lifecycle pattern:

#### Before Execution (`OnBeforeExecuteAsync`)
* **Middleware Pipeline**: Custom middlewares execute before the node
* **Node Hook**: The node's `OnBeforeExecuteAsync` method runs
* **Resource Acquisition**: Resource permits are acquired based on node cost and priority
* **Performance Tracking**: Execution timing begins
* **Debug Hooks**: Breakpoint and step-mode checks

```csharp
// Execute lifecycle: Before (middlewares then node hook)
// Await calls use ConfigureAwait(false) for library code to avoid capturing the
// synchronization context in consumer applications.
await InvokeBeforeMiddlewaresAsync(context, execNode).ConfigureAwait(false);
await execNode.OnBeforeExecuteAsync(context.Kernel, context.GraphState.KernelArguments, effectiveCt).ConfigureAwait(false);

// Resource governance: acquire permits
using var lease = _resourceGovernor != null
    ? await _resourceGovernor.AcquireAsync(nodeCost, priority, context.CancellationToken)
    : default;

// Start performance tracking
var nodeTracker = _performanceMetrics?.StartNodeTracking(currentNode.NodeId, currentNode.Name, context.ExecutionId);
```

#### Main Execution (`ExecuteAsync`)
* **Function Execution**: The node's core logic runs
* **Result Processing**: Output is captured and stored
* **State Updates**: Graph state is modified based on results

#### After Execution (`OnAfterExecuteAsync`)
* **Node Hook**: The node's `OnAfterExecuteAsync` method runs
* **Middleware Pipeline**: Custom middlewares execute after the node
* **Performance Completion**: Execution timing is finalized
* **Success Registration**: Node success is recorded for self-healing

```csharp
// Execute the node
// Use ConfigureAwait(false) so example library code does not capture contexts.
var result = await execNode.ExecuteAsync(context.Kernel, context.GraphState.KernelArguments, effectiveCt).ConfigureAwait(false);

// Execute lifecycle: After (node hook then middlewares)
await execNode.OnAfterExecuteAsync(context.Kernel, context.GraphState.KernelArguments, result, effectiveCt).ConfigureAwait(false);
await InvokeAfterMiddlewaresAsync(context, execNode, result).ConfigureAwait(false);

// Record successful completion
context.RecordNodeCompleted(execNode, result);

// Self-healing: register success
RegisterNodeSuccess(execNode.NodeId);
```

#### Error Handling (`OnExecutionFailedAsync`)
* **Failure Recording**: Node failure is logged and tracked
* **Error Recovery**: Recovery engine attempts to restore execution
* **Policy Application**: Error handling policies determine retry/skip behavior
* **Self-Healing**: Failed nodes may be quarantined

```csharp
// Execute lifecycle: Failed
// Ensure ConfigureAwait(false) on awaited calls.
await currentNode.OnExecutionFailedAsync(context.Kernel, context.GraphState.KernelArguments, ex, context.CancellationToken).ConfigureAwait(false);

// Record node failure
context.RecordNodeFailed(currentNode, ex);

// Self-healing: register failure and potentially quarantine
RegisterNodeFailure(currentNode.NodeId);

// Apply error handling policies
if (_metadata.TryGetValue(nameof(IErrorHandlingPolicy), out var epObj) && epObj is IErrorHandlingPolicy errorPolicy)
{
    if (errorPolicy.ShouldRetry(currentNode, ex, context, out var delay))
    {
        // Retry logic
    }
    if (errorPolicy.ShouldSkip(currentNode, ex, context))
    {
        // Skip logic
    }
}
```

## Navigation Logic

### Next Node Selection

After a node executes, the executor determines which nodes to execute next:

```csharp
// Find next nodes to execute
var nextNodes = GetCombinedNextNodes(execNode, result, context.GraphState).ToList();

if (_routingEngine != null && nextNodes.Count > 0)
{
    // Use dynamic routing engine for intelligent node selection
    // Use ConfigureAwait(false) when awaiting in library code examples.
    currentNode = await _routingEngine.SelectNextNodeAsync(nextNodes, execNode,
        context.GraphState, result, context.CancellationToken).ConfigureAwait(false);
}
else
{
    if (nextNodes.Count <= 1 || !_concurrencyOptions.EnableParallelExecution)
    {
        // Sequential execution with deterministic ordering
        var ordered = context.WorkQueue.OrderDeterministically(nextNodes).ToList();
        currentNode = ordered.FirstOrDefault();
    }
    else
    {
        // Parallel fork/join execution
        // ... parallel execution logic
    }
}
```

### Conditional Edge Evaluation

Edges are evaluated to determine valid transitions:

```csharp
private IEnumerable<IGraphNode> GetCombinedNextNodes(IGraphNode node, FunctionResult? result, GraphState graphState)
{
    var nextNodes = new List<IGraphNode>();
    
    // Get nodes from the node's own navigation logic
    var nodeNextNodes = node.GetNextNodes(result, graphState);
    nextNodes.AddRange(nodeNextNodes);
    
    // Get nodes from conditional edges
    var edgeNextNodes = GetOutgoingEdges(node)
        .Where(edge => edge.EvaluateCondition(graphState))
        .Select(edge => edge.TargetNode);
    nextNodes.AddRange(edgeNextNodes);
    
    return nextNodes.DistinctBy(n => n.NodeId);
}
```

## Execution Limits and Safeguards

### 1. Maximum Iterations

The executor enforces a configurable limit on the total number of execution steps:

```csharp
// Respect per-execution options for max steps, fall back to structural bound when needed
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

### 2. Execution Timeout

Overall execution timeout prevents runaway graphs:

```csharp
// Apply overall timeout when configured
if (context.ExecutionOptions.ExecutionTimeout > TimeSpan.Zero)
{
    var elapsed = DateTimeOffset.UtcNow - context.StartTime;
    if (elapsed > context.ExecutionOptions.ExecutionTimeout)
    {
        throw new OperationCanceledException($"Graph execution exceeded configured timeout of {context.ExecutionOptions.ExecutionTimeout}");
    }
}
```

### 3. Node-Level Timeouts

Individual nodes can have configurable timeouts:

```csharp
private CancellationTokenSource? CreateNodeTimeoutCts(GraphExecutionContext context, IGraphNode node)
{
    if (_metadata.TryGetValue(nameof(ITimeoutPolicy), out var tpObj) && tpObj is ITimeoutPolicy timeoutPolicy)
    {
        var timeout = timeoutPolicy.GetNodeTimeout(node, context.GraphState);
        if (timeout.HasValue && timeout.Value > TimeSpan.Zero)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
            cts.CancelAfter(timeout.Value);
            return cts;
        }
    }
    return null;
}
```

### 4. Circuit Breakers

Self-healing mechanisms automatically quarantine failing nodes:

```csharp
// Self-healing: skip quarantined nodes
if (IsNodeQuarantined(currentNode.NodeId))
{
    var skipCandidates = GetCombinedNextNodes(currentNode, lastResult, context.GraphState).ToList();
    currentNode = await SelectNextNodeAsync(currentNode,
        context.WorkQueue.OrderDeterministically(skipCandidates).ToList(),
        context, lastResult).ConfigureAwait(false);
    continue;
}
```

### 5. Execution Depth Tracking

The executor tracks execution depth to detect excessive nesting:

```csharp
// Record execution path for metrics
if (_performanceMetrics != null && context.ExecutionPath.Count > 0)
{
    var executionPath = context.ExecutionPath.Select(n => n.NodeId).ToList();
    var totalDuration = context.Duration ?? TimeSpan.Zero;
    var success = lastResult != null;

    _performanceMetrics.RecordExecutionPath(context.ExecutionId, executionPath, totalDuration, success);
}
```

## Parallel Execution

### Fork/Join Pattern

The executor supports parallel execution of multiple branches:

```csharp
// Parallel fork/join: execute all next nodes concurrently, merge state, then continue
var branchNodes = context.WorkQueue.OrderDeterministically(nextNodes).ToList();

// Clone base state for each branch to avoid write conflicts
var branchStates = branchNodes
    .Select(_ => StateHelpers.CloneState(context.GraphState))
    .ToList();

var semaphore = new SemaphoreSlim(Math.Max(1, _concurrencyOptions.MaxDegreeOfParallelism));

var branchTasks = branchNodes.Select(async (branchNode, index) =>
{
    await semaphore.WaitAsync(context.CancellationToken);
    try
    {
        // Execute branch node with isolated state
        var branchArgs = branchStates[index].KernelArguments;
        var branchResult = await branchNode.ExecuteAsync(context.Kernel, branchArgs, context.CancellationToken);
        
        return (Node: branchNode, State: branchStates[index], Result: branchResult, Error: (Exception?)null);
    }
    finally
    {
        semaphore.Release();
    }
});

var branchResults = await Task.WhenAll(branchTasks);

// Merge states using edge-specific configurations
var merged = StateHelpers.CloneState(originalGraphState);
foreach (var br in branchResults)
{
    if (br.Error == null && br.Result != null)
    {
        var edgeConfiguration = GetEdgeMergeConfiguration(execNode, br.Node, context.GraphState);
        merged = StateHelpers.MergeStates(merged, br.State, edgeConfiguration);
    }
}
```

## Error Recovery and Resilience

### Recovery Engine Integration

The executor integrates with a recovery engine for automatic error handling:

```csharp
// Attempt error recovery if recovery engine is available
if (_recoveryEngine != null)
{
    try
    {
        // Create error context
        var errorContext = new ErrorHandlingContext
        {
            Exception = ex,
            ErrorType = CategorizeError(ex),
            Severity = DetermineErrorSeverity(ex),
            FailedNode = currentNode,
            AttemptNumber = 1,
            IsTransient = IsTransientError(ex),
            AdditionalContext = new Dictionary<string, object>
            {
                ["CurrentNodeId"] = currentNode.NodeId,
                ["CurrentNodeName"] = currentNode.Name,
                ["IterationCount"] = iterations,
                ["ExecutionId"] = context.GraphState.KernelArguments.GetExecutionId(),
                ["ErrorTimestamp"] = DateTimeOffset.UtcNow
            }
        };

        // Apply recovery policy
        var recoveryResult = await _recoveryEngine.ApplyRecoveryPolicyAsync(errorContext, context.GraphState);
        
        if (recoveryResult.Success)
        {
            // Handle recovery based on type (retry, rollback, continue)
            switch (recoveryResult.RecoveryType)
            {
                case RecoveryType.Retry:
                    iterations--; // Don't count retry as iteration
                    continue;
                case RecoveryType.Rollback:
                    if (recoveryResult.RestoredState != null)
                    {
                        context.GraphState = recoveryResult.RestoredState;
                    }
                    break;
            }
        }
    }
    catch (Exception recoveryEx)
    {
        // Recovery failed, continue with original error
        context.GraphState.KernelArguments["RecoveryError"] = recoveryEx.Message;
        context.GraphState.KernelArguments["RecoveryFailed"] = true;
    }
}
```

### Error Policy Framework

Configurable policies determine how to handle different types of errors:

```csharp
// Pluggable error policy decisions (retry/skip)
if (_metadata.TryGetValue(nameof(IErrorHandlingPolicy), out var epObj) && epObj is IErrorHandlingPolicy errorPolicy)
{
    if (errorPolicy.ShouldRetry(currentNode, ex, context, out var delay))
    {
        if (delay.HasValue && delay.Value > TimeSpan.Zero)
        {
            await Task.Delay(delay.Value, context.CancellationToken).ConfigureAwait(false);
        }
        iterations--; // retry does not count iteration
        continue; // retry current node
    }
    if (errorPolicy.ShouldSkip(currentNode, ex, context))
    {
        // Select next node without executing current
        var nextCandidates = GetCombinedNextNodes(currentNode, lastResult, context.GraphState).ToList();
        currentNode = await SelectNextNodeAsync(currentNode, nextCandidates, context, lastResult);
        continue;
    }
}
```

## Middleware Pipeline

### Execution Middleware

Custom middlewares can intercept execution at various points:

```csharp
private async Task InvokeBeforeMiddlewaresAsync(GraphExecutionContext context, IGraphNode node)
{
    var ordered = GetOrderedMiddlewares();
    foreach (var m in ordered)
    {
        await m.OnBeforeNodeAsync(context, node, context.CancellationToken).ConfigureAwait(false);
    }
}

private async Task InvokeAfterMiddlewaresAsync(GraphExecutionContext context, IGraphNode node, FunctionResult result)
{
    var ordered = GetOrderedMiddlewares();
    for (int i = ordered.Count - 1; i >= 0; i--)
    {
        await ordered[i].OnAfterNodeAsync(context, node, result, context.CancellationToken).ConfigureAwait(false);
    }
}

private async Task InvokeFailureMiddlewaresAsync(GraphExecutionContext context, IGraphNode node, Exception exception)
{
    var ordered = GetOrderedMiddlewares();
    for (int i = ordered.Count - 1; i >= 0; i--)
    {
        await ordered[i].OnNodeFailedAsync(context, node, exception, context.CancellationToken).ConfigureAwait(false);
    }
}
```

## Resource Governance

### Resource Acquisition

The executor manages resource allocation based on node cost and priority:

```csharp
// Determine cost and priority (pluggable via DI)
var priority = context.GraphState.KernelArguments.GetExecutionPriority() ?? _resourceOptions.DefaultPriority;
var nodeCost = 1.0;

if (_metadata.TryGetValue(nameof(ICostPolicy), out var cpObj) && cpObj is ICostPolicy costPolicy)
{
    nodeCost = costPolicy.GetNodeCostWeight(currentNode, context.GraphState) ?? nodeCost;
    var p = costPolicy.GetNodePriority(currentNode, context.GraphState);
    if (p.HasValue) priority = p.Value;
}

// Acquire resource permits
using var lease = _resourceGovernor != null
    ? await _resourceGovernor.AcquireAsync(nodeCost, priority, context.CancellationToken).ConfigureAwait(false)
    : default;
```

## Best Practices

### Execution Configuration

* **Set Reasonable Limits**: Configure `MaxExecutionSteps` and `ExecutionTimeout` based on your workflow complexity
* **Use Middleware**: Implement custom middlewares for cross-cutting concerns like logging, monitoring, and security
* **Configure Resource Limits**: Set appropriate resource governance to prevent resource exhaustion
* **Enable Validation**: Use `ValidateGraphIntegrity` in development to catch structural issues early

### Error Handling

* **Implement Recovery Policies**: Create custom error handling policies for your domain
* **Use Circuit Breakers**: Leverage self-healing to automatically handle failing nodes
* **Monitor Execution Paths**: Track execution paths to identify performance bottlenecks
* **Set Node Timeouts**: Configure appropriate timeouts for long-running operations

### Performance

* **Enable Plan Compilation**: Use `EnablePlanCompilation` for complex graphs to improve performance
* **Configure Parallel Execution**: Use parallel execution for independent branches
* **Monitor Resource Usage**: Track CPU and memory usage to optimize resource allocation
* **Use Deterministic Ordering**: Ensure reproducible execution with `DeterministicWorkQueue`

## See Also

* [Graph Concepts](graph-concepts.md) - Fundamental graph concepts and components
* [Node Types](nodes.md) - Available node implementations and their lifecycle
* [State Management](state.md) - How execution state is managed and propagated
* [Error Handling](error-handling.md) - Advanced error recovery and resilience patterns
* [Performance Tuning](performance-tuning.md) - Optimizing graph execution performance
* [Examples](../examples/) - Practical examples of execution patterns
