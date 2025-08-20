# Metrics and Logging Quickstart

Learn how to enable comprehensive metrics collection and structured logging in your SemanticKernel.Graph applications. This guide shows you how to monitor performance, track execution paths, and gain insights into your graph operations.

## Concepts and Techniques

**Metrics Collection**: The `GraphPerformanceMetrics` class tracks node execution times, success rates, and resource usage to help identify performance bottlenecks and optimize your graphs.

**Structured Logging**: `SemanticKernelGraphLogger` provides correlation-aware logging that integrates with Microsoft.Extensions.Logging, making it easy to trace execution flows and debug issues.

**Performance Analysis**: Built-in dashboards and reports help you understand execution patterns, identify slow nodes, and monitor system health in real-time.

## Prerequisites and Minimum Configuration

* .NET 8.0 or later
* SemanticKernel.Graph package installed
* Microsoft.Extensions.Logging configured in your application

## Quick Setup

### 1. Enable Metrics and Logging

Add graph support to your kernel with metrics and logging enabled:

```csharp
using SemanticKernel.Graph.Extensions;

var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddGraphSupport(options =>
    {
        options.EnableLogging = true;
        options.EnableMetrics = true;
    })
    .Build();
```

### 2. Create a Graph with Metrics

Create a graph executor and enable development metrics for detailed tracking:

```csharp
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Nodes;

var graph = new GraphExecutor("MyGraph", "Example graph with metrics");

// Enable development metrics (detailed tracking, frequent sampling)
graph.EnableDevelopmentMetrics();

// Or use production metrics (optimized for performance)
// graph.EnableProductionMetrics();
```

### 3. Add Nodes and Execute

Build your graph and execute it to collect metrics:

```csharp
var node1 = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod(() => "Hello!", "greeting"),
    "greeting",
    "Greeting");

var node2 = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod(() => "Processing...", "processing"),
    "processing",
    "Processing");

graph.AddNode(node1)
     .AddNode(node2)
     .Connect(node1.NodeId, node2.NodeId)
     .SetStartNode(node1.NodeId);

// Execute multiple times to generate meaningful metrics
for (int i = 0; i < 10; i++)
{
    var result = await graph.ExecuteAsync(kernel, new KernelArguments());
}
```

## Viewing Performance Data

### Basic Performance Summary

Get an overview of your graph's performance:

```csharp
var summary = graph.GetPerformanceSummary(TimeSpan.FromMinutes(5));
if (summary != null)
{
    Console.WriteLine($"Total Executions: {summary.TotalExecutions}");
    Console.WriteLine($"Success Rate: {summary.SuccessRate:F1}%");
    Console.WriteLine($"Average Execution Time: {summary.AverageExecutionTime.TotalMilliseconds:F2}ms");
    Console.WriteLine($"Throughput: {summary.Throughput:F2} executions/second");
    
    // Check system health
    var isHealthy = summary.IsHealthy();
    Console.WriteLine($"System Health: {(isHealthy ? "HEALTHY" : "NEEDS ATTENTION")}");
}
```

### Node-Level Metrics

Analyze performance of individual nodes:

```csharp
var nodeMetrics = graph.GetAllNodeMetrics();
foreach (var kvp in nodeMetrics.OrderByDescending(x => x.Value.TotalExecutionTime))
{
    var node = kvp.Value;
    var rating = node.GetPerformanceClassification();
    
    Console.WriteLine($"{node.NodeName}: {node.TotalExecutions} executions, " +
                     $"Avg: {node.AverageExecutionTime.TotalMilliseconds:F2}ms, " +
                     $"Success: {node.SuccessRate:F1}%, Rating: {rating}");
}
```

### Execution Path Analysis

Understand how your graph flows and identify bottlenecks:

```csharp
var pathMetrics = graph.GetPathMetrics();
foreach (var kvp in pathMetrics.OrderByDescending(x => x.Value.ExecutionCount))
{
    var path = kvp.Value;
    Console.WriteLine($"Path: {path.PathKey}");
    Console.WriteLine($"  Executions: {path.ExecutionCount} | " +
                     $"Avg Time: {path.AverageExecutionTime.TotalMilliseconds:F2}ms | " +
                     $"Success: {path.SuccessRate:F1}%");
}
```

## Advanced Metrics Configuration

### Custom Metrics Options

Configure detailed metrics collection with custom options:

```csharp
var metricsOptions = GraphMetricsOptions.CreateProductionOptions();
metricsOptions.EnableResourceMonitoring = true;  // Monitor CPU and memory
metricsOptions.ResourceSamplingInterval = TimeSpan.FromSeconds(10);
metricsOptions.MaxSampleHistory = 10000;
metricsOptions.EnableDetailedPathTracking = true;

graph.ConfigureMetrics(metricsOptions);
```

### Real-Time Monitoring

Create a dashboard for live metrics monitoring:

```csharp
var dashboard = new MetricsDashboard(graph.PerformanceMetrics!);

// Generate real-time metrics
var realtimeMetrics = dashboard.GenerateRealTimeMetrics();
Console.WriteLine(realtimeMetrics);

// Or get a comprehensive dashboard report
var dashboardReport = dashboard.GenerateDashboard(
    timeWindow: TimeSpan.FromMinutes(10),
    includeNodeDetails: true,
    includePathAnalysis: true);
Console.WriteLine(dashboardReport);
```

## Logging Configuration

### Structured Logging Setup

Configure detailed logging with correlation IDs:

```csharp
using Microsoft.Extensions.Logging;
using SemanticKernel.Graph.Integration;

// Configure logging in your host
var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Information));

var graphLogger = new SemanticKernelGraphLogger(
    loggerFactory.CreateLogger("MyGraph"), 
    new GraphOptions { EnableLogging = true });

// The logger automatically tracks execution context and correlation
```

### Logging Extensions

Use convenient logging extensions for common scenarios:

```csharp
using SemanticKernel.Graph.Extensions;

// Log graph-level information
graphLogger.LogGraphInfo(executionId, "Graph execution started");

// Log node-level details
graphLogger.LogNodeInfo(executionId, nodeId, "Node processing started");

// Log performance metrics
graphLogger.LogPerformance(executionId, "execution_time", 150.5, "ms", 
    new Dictionary<string, string> { ["node_type"] = "function" });
```

## Troubleshooting

### Common Issues

**Metrics not showing**: Ensure `options.EnableMetrics = true` is set when adding graph support.

**Performance counters fail**: On some systems, resource monitoring requires elevated permissions. Use `EnableResourceMonitoring = false` if you encounter issues.

**High memory usage**: Reduce `MaxSampleHistory` and `MaxPathHistoryPerPath` in production environments.

**Logs too verbose**: Configure logging levels appropriately - use `LogLevel.Information` for production and `LogLevel.Debug` for development.

### Performance Recommendations

* Use `CreateProductionOptions()` for production environments
* Enable resource monitoring only when needed
* Set appropriate retention periods based on your analysis requirements
* Monitor memory usage when collecting detailed metrics

## See Also

* **Reference**: [GraphPerformanceMetrics](../api/GraphPerformanceMetrics.md), [GraphMetricsOptions](../api/GraphMetricsOptions.md), [SemanticKernelGraphLogger](../api/SemanticKernelGraphLogger.md)
* **Guides**: [Performance Monitoring](../guides/performance-monitoring.md), [Debugging and Inspection](../guides/debugging-inspection.md)
* **Examples**: [GraphMetricsExample](../examples/graph-metrics.md), [AdvancedPatternsExample](../examples/advanced-patterns.md)

## Reference APIs

* **[GraphPerformanceMetrics](../api/metrics.md#graph-performance-metrics)**: Performance metrics collection
* **[GraphMetricsOptions](../api/metrics.md#graph-metrics-options)**: Metrics configuration options
* **[SemanticKernelGraphLogger](../api/logging.md#semantic-kernel-graph-logger)**: Structured logging system
* **[MetricsDashboard](../api/metrics.md#metrics-dashboard)**: Real-time metrics visualization
