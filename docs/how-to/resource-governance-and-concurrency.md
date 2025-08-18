# Resource Governance and Concurrency

Resource governance and concurrency management in SemanticKernel.Graph provide fine-grained control over resource allocation, execution priorities, and parallel processing capabilities. This guide covers priority-based scheduling, node cost management, resource limits, and parallel execution strategies.

## What You'll Learn

- How to configure resource governance with CPU and memory limits
- Setting execution priorities and managing node costs
- Configuring parallel execution with fork/join patterns
- Implementing adaptive rate limiting and backpressure
- Managing resource budgets and preventing exhaustion
- Best practices for production resource management

## Concepts and Techniques

**ResourceGovernor**: Lightweight in-process resource governor providing adaptive rate limiting and cooperative scheduling based on CPU/memory and execution priority.

**ExecutionPriority**: Priority levels (Critical, High, Normal, Low) that affect resource allocation and scheduling decisions.

**Node Cost Weight**: Relative cost factor for each node that determines resource consumption and scheduling priority.

**Parallel Execution**: Fork/join execution model that allows multiple nodes to execute concurrently while maintaining deterministic behavior.

**Resource Leases**: Temporary resource permits that must be acquired before node execution and released afterward.

**Adaptive Rate Limiting**: Dynamic adjustment of execution rates based on system load (CPU, memory) and resource availability.

## Prerequisites

- [First Graph Tutorial](../first-graph-5-minutes.md) completed
- Basic understanding of graph execution concepts
- Familiarity with parallel programming concepts
- Understanding of resource management principles

## Resource Governance Configuration

### Basic Resource Governance Setup

Enable resource governance at the graph level:

```csharp
using SemanticKernel.Graph.Core;

var graph = new GraphExecutor("ResourceControlledGraph", "Graph with resource governance");

// Configure basic resource governance
graph.ConfigureResources(new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 50.0,           // Base execution rate
    MaxBurstSize = 100,                    // Maximum concurrent executions
    CpuHighWatermarkPercent = 85.0,        // CPU threshold for backpressure
    CpuSoftLimitPercent = 70.0,            // CPU threshold for rate limiting
    MinAvailableMemoryMB = 512.0,          // Memory threshold for throttling
    DefaultPriority = ExecutionPriority.Normal
});
```

### Advanced Resource Configuration

Configure comprehensive resource management:

```csharp
var advancedOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    
    // Rate limiting configuration
    BasePermitsPerSecond = 100.0,
    MaxBurstSize = 200,
    
    // CPU thresholds
    CpuHighWatermarkPercent = 90.0,        // Strong backpressure above 90%
    CpuSoftLimitPercent = 75.0,            // Start throttling above 75%
    
    // Memory thresholds
    MinAvailableMemoryMB = 1024.0,         // 1GB minimum available memory
    
    // Priority configuration
    DefaultPriority = ExecutionPriority.High,
    
    // Node-specific cost weights
    NodeCostWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
    {
        ["heavy_processing"] = 3.0,        // 3x cost for heavy nodes
        ["light_validation"] = 0.5,        // 0.5x cost for light nodes
        ["api_call"] = 2.0                 // 2x cost for external API calls
    },
    
    // Cooperative preemption
    EnableCooperativePreemption = true,    // Allow higher priority work to preempt
    
    // Metrics integration
    PreferMetricsCollector = true          // Use performance metrics for load detection
};

graph.ConfigureResources(advancedOptions);
```

### Preset Configurations

Use predefined configurations for common scenarios:

```csharp
// Development environment (permissive)
var devOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 200.0,          // High throughput
    MaxBurstSize = 500,                    // Large burst allowance
    CpuHighWatermarkPercent = 95.0,        // High CPU tolerance
    MinAvailableMemoryMB = 256.0           // Lower memory threshold
};

// Production environment (conservative)
var prodOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 50.0,           // Controlled throughput
    MaxBurstSize = 100,                    // Moderate burst allowance
    CpuHighWatermarkPercent = 80.0,        // Conservative CPU threshold
    CpuSoftLimitPercent = 65.0,            // Early throttling
    MinAvailableMemoryMB = 2048.0,         // Higher memory threshold
    DefaultPriority = ExecutionPriority.Normal
};

// High-performance scenario (minimal overhead)
var perfOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 1000.0,         // Very high throughput
    MaxBurstSize = 2000,                   // Large burst allowance
    CpuHighWatermarkPercent = 98.0,        // Very high CPU tolerance
    MinAvailableMemoryMB = 128.0,          // Minimal memory threshold
    EnableCooperativePreemption = false    // Disable for performance
};

graph.ConfigureResources(devOptions);      // Choose appropriate preset
```

## Execution Priorities

### Priority Levels and Effects

Configure execution priorities to control resource allocation:

```csharp
// Set priority in kernel arguments
var arguments = new KernelArguments();
arguments.SetExecutionPriority(ExecutionPriority.Critical);

// Execute with high priority
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
public class BusinessPriorityPolicy : ICostPolicy
{
    public double? GetNodeCostWeight(IGraphNode node, GraphState state)
    {
        // Determine cost based on business logic
        if (state.KernelArguments.TryGetValue("business_value", out var value))
        {
            var businessValue = Convert.ToDouble(value);
            return Math.Max(1.0, businessValue / 100.0); // Scale cost by business value
        }
        return null; // Use default
    }

    public ExecutionPriority? GetNodePriority(IGraphNode node, GraphState state)
    {
        // Determine priority based on business context
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
        return null; // Use default
    }
}

// Register custom policy
graph.AddMetadata(nameof(ICostPolicy), new BusinessPriorityPolicy());
```

## Node Cost Management

### Setting Node Costs

Configure costs for different types of nodes:

```csharp
// Configure node costs in resource options
var resourceOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 100.0,
    NodeCostWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
    {
        // Heavy computational nodes
        ["image_processing"] = 5.0,        // 5x cost
        ["ml_inference"] = 3.0,            // 3x cost
        ["data_aggregation"] = 2.5,        // 2.5x cost
        
        // Light validation nodes
        ["input_validation"] = 0.3,        // 0.3x cost
        ["format_check"] = 0.2,            // 0.2x cost
        
        // External API calls
        ["openai_api"] = 2.0,              // 2x cost
        ["database_query"] = 1.5,          // 1.5x cost
        
        // Default cost for unspecified nodes
        ["*"] = 1.0                        // 1x cost
    }
};

graph.ConfigureResources(resourceOptions);
```

### Dynamic Cost Calculation

Calculate costs based on runtime state:

```csharp
public class AdaptiveCostPolicy : ICostPolicy
{
    public double? GetNodeCostWeight(IGraphNode node, GraphState state)
    {
        // Calculate cost based on data size
        if (state.KernelArguments.TryGetValue("data_size_mb", out var sizeObj))
        {
            var sizeMB = Convert.ToDouble(sizeObj);
            if (sizeMB > 100) return 5.0;      // Large data: 5x cost
            if (sizeMB > 10) return 2.0;       // Medium data: 2x cost
            return 0.5;                         // Small data: 0.5x cost
        }
        
        // Calculate cost based on complexity
        if (state.KernelArguments.TryGetValue("complexity_level", out var complexityObj))
        {
            var complexity = Convert.ToInt32(complexityObj);
            return Math.Max(0.5, complexity * 0.5);
        }
        
        return null; // Use default
    }
}

// Register adaptive policy
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
// Configure concurrency options
graph.ConfigureConcurrency(new GraphConcurrencyOptions
{
    EnableParallelExecution = true,                    // Enable parallel execution
    MaxDegreeOfParallelism = 4,                       // Maximum 4 parallel branches
    MergeConflictPolicy = StateMergeConflictPolicy.PreferSecond, // How to handle conflicts
    FallbackToSequentialOnCycles = true               // Fallback for complex cycles
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
// Create a graph with parallel branches
var graph = new GraphExecutor("ParallelGraph", "Graph with parallel execution")
    .AddNode(new FunctionGraphNode(ProcessDataA, "process_a"))
    .AddNode(new FunctionGraphNode(ProcessDataB, "process_b"))
    .AddNode(new FunctionGraphNode(ProcessDataC, "process_c"))
    .AddNode(new FunctionGraphNode(MergeResults, "merge"))
    .Connect("start", "process_a")
    .Connect("start", "process_b")
    .Connect("start", "process_c")
    .Connect("process_a", "merge")
    .Connect("process_b", "merge")
    .Connect("process_c", "merge")
    .SetStartNode("start")
    .ConfigureConcurrency(new GraphConcurrencyOptions
    {
        EnableParallelExecution = true,
        MaxDegreeOfParallelism = 3
    })
    .ConfigureResources(new GraphResourceOptions
    {
        EnableResourceGovernance = true,
        BasePermitsPerSecond = 100.0,
        MaxBurstSize = 3
    });

// Execute with parallel processing
var result = await graph.ExecuteAsync(kernel, arguments);
```

## Resource Monitoring and Adaptation

### System Load Monitoring

Monitor and adapt to system conditions:

```csharp
// Enable metrics for resource monitoring
graph.EnableDevelopmentMetrics();

// Resource governor automatically adapts to system load
var resourceOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    PreferMetricsCollector = true,         // Use metrics for load detection
    CpuHighWatermarkPercent = 85.0,        // Strong backpressure above 85%
    CpuSoftLimitPercent = 70.0,            // Start throttling above 70%
    MinAvailableMemoryMB = 1024.0          // Throttle below 1GB available
};

graph.ConfigureResources(resourceOptions);
```

### Manual Load Updates

Manually update system load information:

```csharp
// Get current system metrics
var metrics = graph.GetPerformanceMetrics();
if (metrics != null)
{
    var cpuUsage = metrics.CurrentCpuUsage;
    var availableMemory = metrics.CurrentAvailableMemoryMB;
    
    // Manually update resource governor (if not using automatic metrics)
    if (graph.GetResourceGovernor() is ResourceGovernor governor)
    {
        governor.UpdateSystemLoad(cpuUsage, availableMemory);
    }
}
```

### Budget Exhaustion Handling

Handle resource budget exhaustion:

```csharp
// Subscribe to budget exhaustion events
if (graph.GetResourceGovernor() is ResourceGovernor governor)
{
    governor.BudgetExhausted += (sender, args) =>
    {
        Console.WriteLine($"🚨 Resource budget exhausted at {args.Timestamp}");
        Console.WriteLine($"   CPU: {args.CpuUsage:F1}%");
        Console.WriteLine($"   Memory: {args.AvailableMemoryMB:F0} MB");
        Console.WriteLine($"   Exhaustion count: {args.ExhaustionCount}");
        
        // Implement alerting, logging, or fallback strategies
        LogResourceExhaustion(args);
        SendResourceAlert(args);
    };
}
```

## Performance Optimization

### Resource Governor Tuning

Optimize resource governor for your workload:

```csharp
// High-throughput configuration
var highThroughputOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 500.0,          // High base rate
    MaxBurstSize = 1000,                   // Large burst allowance
    CpuHighWatermarkPercent = 95.0,        // High CPU tolerance
    CpuSoftLimitPercent = 85.0,            // Late throttling
    MinAvailableMemoryMB = 256.0,          // Low memory threshold
    EnableCooperativePreemption = false    // Disable for performance
};

// Low-latency configuration
var lowLatencyOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 50.0,           // Controlled rate
    MaxBurstSize = 100,                    // Moderate burst
    CpuHighWatermarkPercent = 70.0,        // Early backpressure
    CpuSoftLimitPercent = 50.0,            // Very early throttling
    MinAvailableMemoryMB = 2048.0,         // High memory threshold
    EnableCooperativePreemption = true     // Enable for responsiveness
};

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

- **Start Conservative**: Begin with lower permit rates and increase based on performance
- **Monitor System Load**: Use metrics integration for automatic adaptation
- **Set Reasonable Thresholds**: CPU thresholds should align with your SLOs
- **Memory Management**: Set memory thresholds based on available system resources
- **Priority Strategy**: Use priorities to ensure critical work gets resources

### Parallel Execution

- **Identify Independent Branches**: Only parallelize truly independent work
- **Manage State Conflicts**: Choose appropriate merge conflict policies
- **Limit Parallelism**: Don't exceed reasonable parallelism limits
- **Handle Cycles**: Use fallback to sequential execution for complex cycles
- **Resource Coordination**: Ensure resource governance works with parallel execution

### Performance Tuning

- **Profile Your Workload**: Understand resource consumption patterns
- **Adjust Burst Sizes**: Balance responsiveness with stability
- **Monitor Exhaustion**: Track budget exhaustion events
- **Adapt to Load**: Use automatic load adaptation when possible
- **Test Under Load**: Validate performance under expected load conditions

### Production Considerations

- **Resource Limits**: Set conservative limits for production stability
- **Monitoring**: Implement comprehensive resource monitoring
- **Alerting**: Set up alerts for resource exhaustion
- **Scaling**: Plan for horizontal scaling when resource limits are reached
- **Fallbacks**: Implement graceful degradation when resources are constrained

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

- [Metrics and Observability](metrics-and-observability.md) - Monitoring resource usage and performance
- [Graph Execution](../concepts/execution.md) - Understanding execution lifecycle and patterns
- [State Management](../concepts/state.md) - Managing state in parallel execution
- [Examples](../../examples/) - Practical examples of resource governance and concurrency
