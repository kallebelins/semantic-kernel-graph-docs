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
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Extensions;

// Create kernel with graph support enabled
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey) // Replace with your API key
    .AddGraphSupport(options =>
    {
        options.EnableLogging = true;   // Enable structured logging
        options.EnableMetrics = true;   // Enable performance metrics collection
    })
    .Build();
```

### 2. Create a Graph with Metrics

Create a graph executor and enable development metrics for detailed tracking:

```csharp
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Nodes;

// Create a graph executor with descriptive name and description
var graph = new GraphExecutor("MyGraph", "Example graph with metrics");

// Enable development metrics (detailed tracking, frequent sampling)
// Use this for development and testing environments
graph.EnableDevelopmentMetrics();

// Or use production metrics (optimized for performance)
// Use this for production environments to reduce overhead
// graph.EnableProductionMetrics();
```

### 3. Add Nodes and Execute

Build your graph and execute it to collect metrics:

```csharp
// Create function nodes with descriptive names and simple functions
var node1 = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod(() => "Hello!", "greeting"),
    "greeting",
    "Greeting Node");

var node2 = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod(() => "Processing...", "processing"),
    "processing",
    "Processing Node");

// Build the graph by adding nodes and connecting them
graph.AddNode(node1)
     .AddNode(node2)
     .Connect(node1.NodeId, node2.NodeId)  // Connect greeting -> processing
     .SetStartNode(node1.NodeId);          // Set greeting as the starting node

// Execute multiple times to generate meaningful metrics data
Console.WriteLine("Executing graph to collect metrics...");
for (int i = 0; i < 10; i++)
{
    try
    {
        var result = await graph.ExecuteAsync(kernel, new KernelArguments());
        Console.Write(".");  // Show progress
    }
    catch (Exception ex)
    {
        Console.Write("X");  // Show failed execution
        // In production, log the exception details
    }
}
Console.WriteLine(" Done!");
```

## Viewing Performance Data

### Basic Performance Summary

Get an overview of your graph's performance:

```csharp
// Get performance summary for the last 5 minutes
var summary = graph.GetPerformanceSummary(TimeSpan.FromMinutes(5));
if (summary != null)
{
    Console.WriteLine("📊 PERFORMANCE SUMMARY");
    Console.WriteLine("".PadRight(50, '-'));
    Console.WriteLine($"Total Executions: {summary.TotalExecutions}");
    Console.WriteLine($"Success Rate: {summary.SuccessRate:F1}%");
    Console.WriteLine($"Average Execution Time: {summary.AverageExecutionTime.TotalMilliseconds:F2}ms");
    Console.WriteLine($"Throughput: {summary.Throughput:F2} executions/second");
    
    // Check system health based on performance thresholds
    var isHealthy = summary.IsHealthy();
    Console.WriteLine($"System Health: {(isHealthy ? "🟢 HEALTHY" : "🔴 NEEDS ATTENTION")}");
    
    // Show performance alerts if system needs attention
    if (!isHealthy)
    {
        var alerts = summary.GetPerformanceAlerts();
        Console.WriteLine("Alerts:");
        foreach (var alert in alerts)
        {
            Console.WriteLine($"  - {alert}");
        }
    }
}
else
{
    Console.WriteLine("No performance data available yet. Execute the graph first.");
}
```

### Node-Level Metrics

Analyze performance of individual nodes:

```csharp
// Get metrics for all nodes in the graph
var nodeMetrics = graph.GetAllNodeMetrics();
if (nodeMetrics.Count > 0)
{
    Console.WriteLine("🔧 NODE PERFORMANCE");
    Console.WriteLine("".PadRight(50, '-'));
    Console.WriteLine($"{"Node",-15} {"Executions",-12} {"Avg Time",-12} {"Success %",-10} {"Rating",-12}");
    Console.WriteLine("".PadRight(70, '-'));

    // Sort nodes by total execution time (slowest first)
    foreach (var kvp in nodeMetrics.OrderByDescending(x => x.Value.TotalExecutionTime))
    {
        var node = kvp.Value;
        var rating = node.GetPerformanceClassification(); // Excellent, Good, Fair, Poor
        
        Console.WriteLine($"{node.NodeName.Substring(0, Math.Min(14, node.NodeName.Length)),-15} " +
                         $"{node.TotalExecutions,-12} " +
                         $"{node.AverageExecutionTime.TotalMilliseconds,-12:F2}ms " +
                         $"{node.SuccessRate,-10:F1}% " +
                         $"{rating,-12}");
    }
}
else
{
    Console.WriteLine("No node metrics available yet. Execute the graph first.");
}
```

### Execution Path Analysis

Understand how your graph flows and identify bottlenecks:

```csharp
// Analyze execution paths to understand graph flow patterns
var pathMetrics = graph.GetPathMetrics();
if (pathMetrics.Count > 0)
{
    Console.WriteLine("🛣️ EXECUTION PATHS");
    Console.WriteLine("".PadRight(50, '-'));
    
    // Sort paths by execution count (most frequent first)
    foreach (var kvp in pathMetrics.OrderByDescending(x => x.Value.ExecutionCount))
    {
        var path = kvp.Value;
        Console.WriteLine($"Path: {path.PathKey}");
        Console.WriteLine($"  Executions: {path.ExecutionCount} | " +
                         $"Avg Time: {path.AverageExecutionTime.TotalMilliseconds:F2}ms | " +
                         $"Success: {path.SuccessRate:F1}%");
        
        // Identify potential bottlenecks
        if (path.AverageExecutionTime.TotalMilliseconds > 1000)
        {
            Console.WriteLine($"  ⚠️  Slow path detected - consider optimization");
        }
        if (path.SuccessRate < 95.0)
        {
            Console.WriteLine($"  ❌ Low success rate - investigate failures");
        }
    }
}
else
{
    Console.WriteLine("No path metrics available yet. Execute the graph first.");
}
```

## Advanced Metrics Configuration

### Custom Metrics Options

Configure detailed metrics collection with custom options:

```csharp
// Create production-optimized metrics options
var metricsOptions = GraphMetricsOptions.CreateProductionOptions();

// Configure resource monitoring (CPU and memory usage)
metricsOptions.EnableResourceMonitoring = true;  // Monitor system resources
metricsOptions.ResourceSamplingInterval = TimeSpan.FromSeconds(10); // Sample every 10 seconds

// Configure data retention and tracking
metricsOptions.MaxSampleHistory = 10000; // Keep last 10,000 samples
metricsOptions.EnableDetailedPathTracking = true; // Track execution paths in detail

// Apply the configuration to the graph
graph.ConfigureMetrics(metricsOptions);

Console.WriteLine("Advanced metrics configuration applied successfully!");
```

### Real-Time Monitoring

Create a dashboard for live metrics monitoring:

```csharp
// Create a metrics dashboard for real-time monitoring
var dashboard = new MetricsDashboard(graph.PerformanceMetrics!);

// Generate real-time metrics snapshot
var realtimeMetrics = dashboard.GenerateRealTimeMetrics();
Console.WriteLine("📈 REAL-TIME METRICS");
Console.WriteLine("".PadRight(50, '='));
Console.WriteLine(realtimeMetrics);

// Generate comprehensive dashboard report
var dashboardReport = dashboard.GenerateDashboard(
    timeWindow: TimeSpan.FromMinutes(10),    // Last 10 minutes of data
    includeNodeDetails: true,                // Include per-node analysis
    includePathAnalysis: true);              // Include execution path analysis

Console.WriteLine("\n📊 COMPREHENSIVE DASHBOARD");
Console.WriteLine("".PadRight(50, '='));
Console.WriteLine(dashboardReport);

// Generate quick status overview
var statusOverview = dashboard.GenerateStatusOverview();
Console.WriteLine("\n⚡ QUICK STATUS");
Console.WriteLine("".PadRight(50, '='));
Console.WriteLine(statusOverview);
```

## Logging Configuration

### Structured Logging Setup

Configure detailed logging with correlation IDs:

```csharp
using Microsoft.Extensions.Logging;
using SemanticKernel.Graph.Integration;

// Configure structured logging with console output
var loggerFactory = LoggerFactory.Create(builder =>
    builder
        .AddConsole()                           // Add console logging provider
        .SetMinimumLevel(LogLevel.Information)  // Set minimum log level
        .AddFilter("SemanticKernel.Graph", LogLevel.Debug)); // Enable debug for graph operations

// Create a graph logger with correlation support
var graphLogger = new SemanticKernelGraphLogger(
    loggerFactory.CreateLogger("MyGraph"),      // Logger instance with category
    new GraphOptions { EnableLogging = true }); // Graph options with logging enabled

// The logger automatically tracks execution context and correlation IDs
Console.WriteLine("✅ Structured logging configured successfully!");
```

### Logging Extensions

Use convenient logging extensions for common scenarios:

```csharp
using SemanticKernel.Graph.Extensions;

// Generate unique execution ID for correlation
var executionId = Guid.NewGuid().ToString();
var nodeId = "greeting-node";

// Log graph-level information with correlation
graphLogger.LogGraphInfo(executionId, "Graph execution started", 
    new Dictionary<string, object> 
    { 
        ["GraphName"] = "MyGraph",
        ["StartTime"] = DateTime.UtcNow 
    });

// Log node-level details with context
graphLogger.LogNodeInfo(executionId, nodeId, "Node processing started",
    new Dictionary<string, object>
    {
        ["NodeType"] = "FunctionGraphNode",
        ["InputParameters"] = "none"
    });

// Log performance metrics with tags for filtering
graphLogger.LogPerformance(executionId, "execution_time", 150.5, "ms", 
    new Dictionary<string, string> 
    { 
        ["node_type"] = "function",
        ["operation"] = "greeting",
        ["environment"] = "development"
    });

// Log completion
graphLogger.LogGraphInfo(executionId, "Graph execution completed successfully");
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
