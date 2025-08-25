# Resource Governance and Concurrency

Resource governance and concurrency management in SemanticKernel.Graph provide fine-grained control over resource allocation, execution priorities, and parallel processing capabilities. This guide covers priority-based scheduling, node cost management, resource limits, and parallel execution strategies.

## What You'll Learn

* How to configure resource governance with CPU and memory limits
* Setting execution priorities and managing node costs
* Configuring parallel execution with fork/join patterns
* Implementing adaptive rate limiting and backpressure
* Managing resource budgets and preventing exhaustion
* Best practices for production resource management

## Concepts and Techniques

**ResourceGovernor**: Lightweight in-process resource governor providing adaptive rate limiting and cooperative scheduling based on CPU/memory and execution priority.

**ExecutionPriority**: Priority levels (Critical, High, Normal, Low) that affect resource allocation and scheduling decisions.

**Node Cost Weight**: Relative cost factor for each node that determines resource consumption and scheduling priority.

**Parallel Execution**: Fork/join execution model that allows multiple nodes to execute concurrently while maintaining deterministic behavior.

**Resource Leases**: Temporary resource permits that must be acquired before node execution and released afterward.

**Adaptive Rate Limiting**: Dynamic adjustment of execution rates based on system load (CPU, memory) and resource availability.

## Prerequisites

* [First Graph Tutorial](../first-graph-5-minutes.md) completed
* Basic understanding of graph execution concepts
* Familiarity with parallel programming concepts
* Understanding of resource management principles

## Resource Governance Configuration

### Basic Resource Governance Setup

Enable resource governance at the graph level:

```csharp
// Example: Create a GraphExecutor and enable basic in-process resource governance.
// Comments describe the intent of each option in plain English for clarity.
using SemanticKernel.Graph.Core;

// Create a graph executor instance (name and description are optional metadata).
var graph = new GraphExecutor("ResourceControlledGraph", "Graph with resource governance");

// Configure resource governance with sensible defaults for development.
graph.ConfigureResources(new GraphResourceOptions
{
    EnableResourceGovernance = true,            // Turn on the governor to enforce permits
    BasePermitsPerSecond = 50.0,                // Base rate of permits granted per second
    MaxBurstSize = 100,                         // Maximum tokens allowed in a burst
    CpuHighWatermarkPercent = 85.0,             // If CPU > this, apply strong backpressure
    CpuSoftLimitPercent = 70.0,                 // If CPU > this, gradually reduce permits
    MinAvailableMemoryMB = 512.0,               // Minimum free memory before aggressive throttling
    DefaultPriority = ExecutionPriority.Normal  // Default execution priority when none supplied
});
```

### Advanced Resource Configuration

Configure comprehensive resource management:

```csharp
// Advanced configuration example: tune governor for mixed workloads.
var advancedOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,

    // Rate limiting: higher base permits and larger bursts for throughput.
    BasePermitsPerSecond = 100.0,
    MaxBurstSize = 200,

    // CPU thresholds: fine-grained control for backpressure behavior.
    CpuHighWatermarkPercent = 90.0,    // Aggressive backpressure above 90%
    CpuSoftLimitPercent = 75.0,        // Begin throttling above 75%

    // Memory threshold expressed in megabytes.
    MinAvailableMemoryMB = 1024.0,     // Require at least 1GB free before heavy workloads

    // Default execution priority to favor higher importance work.
    DefaultPriority = ExecutionPriority.High,

    // Node-specific cost weights: map node identifiers to relative costs.
    NodeCostWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
    {
        ["heavy_processing"] = 3.0,     // Mark heavy nodes as 3x cost
        ["light_validation"] = 0.5,     // Light validators are cheaper
        ["api_call"] = 2.0              // External calls consume more budget
    },

    // Allow cooperative preemption so higher-priority tasks can preempt lower ones.
    EnableCooperativePreemption = true,

    // Prefer an external metrics collector when present for better decisions.
    PreferMetricsCollector = true
};

graph.ConfigureResources(advancedOptions);
```

### Preset Configurations

Use predefined configurations for common scenarios:

```csharp
// Preset configurations for common deployment scenarios.
// Development: permissive quotas to make iteration easy.
var devOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 200.0,
    MaxBurstSize = 500,
    CpuHighWatermarkPercent = 95.0,
    MinAvailableMemoryMB = 256.0
};

// Production: conservative defaults for stability under load.
var prodOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 50.0,
    MaxBurstSize = 100,
    CpuHighWatermarkPercent = 80.0,
    CpuSoftLimitPercent = 65.0,
    MinAvailableMemoryMB = 2048.0,
    DefaultPriority = ExecutionPriority.Normal
};

// High-performance: lean but permissive for latency-sensitive workloads.
var perfOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 1000.0,
    MaxBurstSize = 2000,
    CpuHighWatermarkPercent = 98.0,
    MinAvailableMemoryMB = 128.0,
    EnableCooperativePreemption = false
};

// Apply the preset that best matches your environment.
graph.ConfigureResources(devOptions);
```

## Execution Priorities

### Priority Levels and Effects

Configure execution priorities to control resource allocation:

```csharp
// Example: set an execution priority on KernelArguments before starting the graph.
// This gives critical work preference when permits are scarce.
var arguments = new KernelArguments();
arguments.SetExecutionPriority(ExecutionPriority.Critical); // Mark this execution as critical

// Execute the graph using the kernel and the prioritized arguments.
var result = await graph.ExecuteAsync(kernel, arguments);
```

### Priority-Based Cost Adjustment

Priorities affect resource consumption:

```csharp
// Priority factors (lower factor = lower resource cost)
// Critical: 0.5x cost (highest priority, lowest resource consumption)
// High: 0.6x cost
// Normal: 1.0x cost (default)
// Low: 1.5x cost (lowest priority, highest resource consumption)

// Example: Critical priority work gets resource preference
var criticalArgs = new KernelArguments();
criticalArgs.SetExecutionPriority(ExecutionPriority.Critical);

var normalArgs = new KernelArguments();
normalArgs.SetExecutionPriority(ExecutionPriority.Normal);

// Critical work will consume fewer permits and execute faster
var criticalResult = await graph.ExecuteAsync(kernel, criticalArgs);
var normalResult = await graph.ExecuteAsync(kernel, normalArgs);
```

### Custom Priority Policies

Implement custom priority logic:

```csharp
// Custom policy example: derive node cost and priority from runtime arguments.
public class BusinessPriorityPolicy : ICostPolicy
{
    // Determine a relative cost weight for the node (higher => more permits consumed).
    public double? GetNodeCostWeight(IGraphNode node, GraphState state)
    {
        if (state.KernelArguments.TryGetValue("business_value", out var value))
        {
            var businessValue = Convert.ToDouble(value);
            // Scale business value into a cost weight with a sensible minimum of 1.0.
            return Math.Max(1.0, businessValue / 100.0);
        }

        // Returning null means "use default cost"
        return null;
    }

    // Optionally map business tiers to execution priorities.
    public ExecutionPriority? GetNodePriority(IGraphNode node, GraphState state)
    {
        if (state.KernelArguments.TryGetValue("customer_tier", out var tier))
        {
            return tier.ToString() switch
            {
                "premium" => ExecutionPriority.Critical,
                "gold" => ExecutionPriority.High,
                "silver" => ExecutionPriority.Normal,
                _ => ExecutionPriority.Low
            };
        }

        return null; // Use default priority when no tier is provided
    }
}

// Register the policy so the executor can consult it at runtime.
graph.AddMetadata(nameof(ICostPolicy), new BusinessPriorityPolicy());
```

## Node Cost Management

### Setting Node Costs

Configure costs for different types of nodes:

```csharp
// Example: declare static node cost overrides for common node identifiers.
var resourceOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 100.0,
    NodeCostWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
    {
        // Heavy compute: consume substantially more permits.
        ["image_processing"] = 5.0,
        ["ml_inference"] = 3.0,
        ["data_aggregation"] = 2.5,

        // Lightweight checks: cheap to run.
        ["input_validation"] = 0.3,
        ["format_check"] = 0.2,

        // External calls and DB queries are costlier than in-memory ops.
        ["openai_api"] = 2.0,
        ["database_query"] = 1.5,

        // Fallback default weight for unspecified nodes.
        ["*"] = 1.0
    }
};

graph.ConfigureResources(resourceOptions);
```

### Dynamic Cost Calculation

Calculate costs based on runtime state:

```csharp
// Adaptive cost policy: compute costs from runtime arguments (data size, complexity).
public class AdaptiveCostPolicy : ICostPolicy
{
    public double? GetNodeCostWeight(IGraphNode node, GraphState state)
    {
        // If a data size hint is present, map it to a cost weight.
        if (state.KernelArguments.TryGetValue("data_size_mb", out var sizeObj))
        {
            var sizeMB = Convert.ToDouble(sizeObj);
            if (sizeMB > 100) return 5.0;  // Very large payloads are expensive
            if (sizeMB > 10) return 2.0;   // Medium payloads have moderate cost
            return 0.5;                     // Small payloads are cheap
        }

        // Use a separate complexity hint if provided.
        if (state.KernelArguments.TryGetValue("complexity_level", out var complexityObj))
        {
            var complexity = Convert.ToInt32(complexityObj);
            return Math.Max(0.5, complexity * 0.5);
        }

        // Returning null uses the default cost mapping.
        return null;
    }
}

// Register the adaptive policy so it is consulted during execution.
graph.AddMetadata(nameof(ICostPolicy), new AdaptiveCostPolicy());
```

### Cost Override via Arguments

Override costs at execution time:

```csharp
// Override node cost in arguments
var arguments = new KernelArguments();
arguments.SetEstimatedNodeCostWeight(2.5); // 2.5x cost for this execution

// Execute with custom cost
var result = await graph.ExecuteAsync(kernel, arguments);
```

## Parallel Execution Configuration

### Basic Parallel Execution

Enable parallel execution for independent branches:

```csharp
// Configure concurrency options to safely run independent branches in parallel.
graph.ConfigureConcurrency(new GraphConcurrencyOptions
{
    EnableParallelExecution = true,                         // Allow parallel fork/join execution
    MaxDegreeOfParallelism = 4,                             // Max concurrent branches to run
    MergeConflictPolicy = StateMergeConflictPolicy.PreferSecond, // Merge strategy for state joins
    FallbackToSequentialOnCycles = true                     // Be conservative when cycles are detected
});
```

### Advanced Parallel Configuration

Configure sophisticated parallel execution:

```csharp
var concurrencyOptions = new GraphConcurrencyOptions
{
    EnableParallelExecution = true,
    
    // Parallelism limits
    MaxDegreeOfParallelism = Environment.ProcessorCount * 2, // 2x CPU cores
    
    // Conflict resolution policies
    MergeConflictPolicy = StateMergeConflictPolicy.PreferSecond,
    
    // Cycle handling
    FallbackToSequentialOnCycles = true
};

graph.ConfigureConcurrency(concurrencyOptions);
```

### Fork/Join Execution

Create parallel execution patterns:

```csharp
// Fork/join example: create parallel branches A,B,C that join into a merge node.
// Here we construct nodes, connect them and enable both concurrency and resource governance.
var graph = new GraphExecutor("ParallelGraph", "Graph with parallel execution");

// Add nodes (example delegates or kernel functions are represented by placeholders).
graph.AddNode(new FunctionGraphNode(ProcessDataA, "process_a"));
graph.AddNode(new FunctionGraphNode(ProcessDataB, "process_b"));
graph.AddNode(new FunctionGraphNode(ProcessDataC, "process_c"));
graph.AddNode(new FunctionGraphNode(MergeResults, "merge"));

// Wire the graph edges: start -> process_a,b,c and each process -> merge
graph.Connect("start", "process_a");
graph.Connect("start", "process_b");
graph.Connect("start", "process_c");
graph.Connect("process_a", "merge");
graph.Connect("process_b", "merge");
graph.Connect("process_c", "merge");
graph.SetStartNode("start");

// Enable parallel execution and resource governance suitable for this small fork/join.
graph.ConfigureConcurrency(new GraphConcurrencyOptions
{
    EnableParallelExecution = true,
    MaxDegreeOfParallelism = 3
});

graph.ConfigureResources(new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 100.0,
    MaxBurstSize = 3
});

// Execute the graph (assumes 'kernel' and 'arguments' are prepared in scope).
var result = await graph.ExecuteAsync(kernel, arguments);
```

## Resource Monitoring and Adaptation

### System Load Monitoring

Monitor and adapt to system conditions:

```csharp
// Enable lightweight development metrics to observe resource usage during tests.
graph.EnableDevelopmentMetrics();

// Let the resource governor prefer metrics when available for adaptive decisions.
var resourceOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    PreferMetricsCollector = true,
    CpuHighWatermarkPercent = 85.0,
    CpuSoftLimitPercent = 70.0,
    MinAvailableMemoryMB = 1024.0
};

graph.ConfigureResources(resourceOptions);
```

### Manual Load Updates

Manually update system load information:

```csharp
// Retrieve current performance metrics and optionally feed them into the governor.
var metrics = graph.GetPerformanceMetrics();
if (metrics != null)
{
    var cpuUsage = metrics.CurrentCpuUsage;                    // Current CPU percent
    var availableMemory = metrics.CurrentAvailableMemoryMB;    // Available memory in MB

    // If the governor is exposed, allow manual updates when automatic metrics are not used.
    if (graph.GetResourceGovernor() is ResourceGovernor governor)
    {
        governor.UpdateSystemLoad(cpuUsage, availableMemory);
    }
}
```

### Budget Exhaustion Handling

Handle resource budget exhaustion:

```csharp
// Handle budget exhaustion events to implement alerting or graceful degradation.
if (graph.GetResourceGovernor() is ResourceGovernor governor)
{
    governor.BudgetExhausted += (sender, args) =>
    {
        // Log basic information about the exhaustion event.
        Console.WriteLine($"🚨 Resource budget exhausted at {args.Timestamp}");
        Console.WriteLine($"   CPU: {args.CpuUsage:F1}%");
        Console.WriteLine($"   Memory: {args.AvailableMemoryMB:F0} MB");
        Console.WriteLine($"   Exhaustion count: {args.ExhaustionCount}");

        // Placeholder calls for user-defined handling strategies.
        // Implementers should replace these with real telemetry/alerting code.
        LogResourceExhaustion(args);
        SendResourceAlert(args);
    };
}
```

## Performance Optimization

### Resource Governor Tuning

Optimize resource governor for your workload:

```csharp
// Performance tuning presets for different workload characteristics.
var highThroughputOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 500.0,
    MaxBurstSize = 1000,
    CpuHighWatermarkPercent = 95.0,
    CpuSoftLimitPercent = 85.0,
    MinAvailableMemoryMB = 256.0,
    EnableCooperativePreemption = false
};

var lowLatencyOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 50.0,
    MaxBurstSize = 100,
    CpuHighWatermarkPercent = 70.0,
    CpuSoftLimitPercent = 50.0,
    MinAvailableMemoryMB = 2048.0,
    EnableCooperativePreemption = true
};

// Apply a configuration suitable for your scenario.
graph.ConfigureResources(highThroughputOptions);
```

### Parallel Execution Optimization

Optimize parallel execution patterns:

```csharp
// Optimize for CPU-bound workloads
var cpuOptimizedOptions = new GraphConcurrencyOptions
{
    EnableParallelExecution = true,
    MaxDegreeOfParallelism = Environment.ProcessorCount, // Match CPU cores
    MergeConflictPolicy = StateMergeConflictPolicy.PreferSecond,
    FallbackToSequentialOnCycles = false  // Allow complex parallel patterns
};

// Optimize for I/O-bound workloads
var ioOptimizedOptions = new GraphConcurrencyOptions
{
    EnableParallelExecution = true,
    MaxDegreeOfParallelism = Environment.ProcessorCount * 4, // Higher parallelism for I/O
    MergeConflictPolicy = StateMergeConflictPolicy.PreferSecond,
    FallbackToSequentialOnCycles = true   // Conservative for I/O
};

graph.ConfigureConcurrency(cpuOptimizedOptions);
```

## Best Practices

### Resource Governance Configuration

* **Start Conservative**: Begin with lower permit rates and increase based on performance
* **Monitor System Load**: Use metrics integration for automatic adaptation
* **Set Reasonable Thresholds**: CPU thresholds should align with your SLOs
* **Memory Management**: Set memory thresholds based on available system resources
* **Priority Strategy**: Use priorities to ensure critical work gets resources

### Parallel Execution

* **Identify Independent Branches**: Only parallelize truly independent work
* **Manage State Conflicts**: Choose appropriate merge conflict policies
* **Limit Parallelism**: Don't exceed reasonable parallelism limits
* **Handle Cycles**: Use fallback to sequential execution for complex cycles
* **Resource Coordination**: Ensure resource governance works with parallel execution

### Performance Tuning

* **Profile Your Workload**: Understand resource consumption patterns
* **Adjust Burst Sizes**: Balance responsiveness with stability
* **Monitor Exhaustion**: Track budget exhaustion events
* **Adapt to Load**: Use automatic load adaptation when possible
* **Test Under Load**: Validate performance under expected load conditions

### Production Considerations

* **Resource Limits**: Set conservative limits for production stability
* **Monitoring**: Implement comprehensive resource monitoring
* **Alerting**: Set up alerts for resource exhaustion
* **Scaling**: Plan for horizontal scaling when resource limits are reached
* **Fallbacks**: Implement graceful degradation when resources are constrained

## Troubleshooting

### Common Issues

**High resource consumption**: Reduce `BasePermitsPerSecond` and `MaxBurstSize`, enable resource monitoring.

**Frequent budget exhaustion**: Increase resource thresholds, reduce node costs, or implement better resource management.

**Poor parallel performance**: Check `MaxDegreeOfParallelism`, verify independent branches, and monitor resource contention.

**Memory pressure**: Increase `MinAvailableMemoryMB`, reduce `MaxBurstSize`, or implement memory cleanup.

### Performance Optimization

```csharp
// Optimize for resource-constrained environments
var optimizedOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 25.0,           // Lower base rate
    MaxBurstSize = 50,                     // Smaller burst allowance
    CpuHighWatermarkPercent = 75.0,        // Early backpressure
    CpuSoftLimitPercent = 60.0,            // Early throttling
    MinAvailableMemoryMB = 2048.0,         // Higher memory threshold
    EnableCooperativePreemption = true,    // Enable for responsiveness
    PreferMetricsCollector = true          // Use metrics for adaptation
};

graph.ConfigureResources(optimizedOptions);
```

## See Also

* [Metrics and Observability](metrics-and-observability.md) - Monitoring resource usage and performance
* [Graph Execution](../concepts/execution.md) - Understanding execution lifecycle and patterns
* [State Management](../concepts/state.md) - Managing state in parallel execution
* [Examples](../../examples/) - Practical examples of resource governance and concurrency
