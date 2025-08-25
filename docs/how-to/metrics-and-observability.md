# Metrics and Observability

Metrics and observability in SemanticKernel.Graph provide comprehensive insights into graph execution performance, resource usage, and operational health. This guide covers performance metrics collection, export capabilities, execution tracing, and monitoring dashboards.

## What You'll Learn

* How to configure and enable comprehensive metrics collection
* Understanding node-level and path-level performance metrics
* Exporting metrics to various monitoring systems and dashboards
* Setting up execution tracing and correlation
* Monitoring system resources and performance indicators
* Best practices for production observability

## Concepts and Techniques

**GraphPerformanceMetrics**: Comprehensive metrics collector that tracks node execution times, success rates, execution paths, and system resource usage.

**NodeExecutionMetrics**: Individual node performance tracking including execution counts, timing percentiles (p50, p95, p99), and success/failure rates.

**ExecutionPathMetrics**: Analysis of complete execution routes through the graph, including path frequency and performance characteristics.

**Metrics Exporters**: Specialized export capabilities for various monitoring systems including Prometheus, Grafana, and custom dashboards.

**Execution Tracing**: OpenTelemetry-based tracing with correlation between execution spans and streaming events.

**Resource Monitoring**: CPU and memory usage tracking with configurable sampling intervals.

## Prerequisites

* [First Graph Tutorial](../first-graph-5-minutes.md) completed
* Basic understanding of graph execution concepts
* Familiarity with metrics and monitoring concepts
* Microsoft.Extensions.Logging configured (optional but recommended)

## Enabling Metrics Collection

### Basic Metrics Configuration

Enable metrics collection at the kernel level:

```csharp
// Create and configure a Kernel with graph support and metrics enabled.
// Note: when calling network or long-running operations use async methods on the kernel.
using SemanticKernel.Graph.Extensions;

var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddGraphSupport(options =>
    {
        options.EnableMetrics = true;
        options.EnableLogging = true;
    })
    .Build();
```

### Graph-Level Metrics Configuration

Configure detailed metrics collection for specific graphs:

```csharp
// Create a GraphExecutor and configure metrics collection. Use development metrics
// for detailed sampling during debugging; use production options to reduce overhead.
using SemanticKernel.Graph.Core;

// Create graph with metrics enabled
var graph = new GraphExecutor("PerformanceGraph", "High-performance workflow");

// Enable development metrics (detailed tracking, frequent sampling)
graph.EnableDevelopmentMetrics();

// Or use production metrics (optimized for performance)
// graph.EnableProductionMetrics();

// Or customize metrics options
var customMetricsOptions = new GraphMetricsOptions
{
    EnableResourceMonitoring = true,
    ResourceSamplingInterval = TimeSpan.FromSeconds(10),
    MaxSampleHistory = 5000,
    EnableDetailedPathTracking = true,
    EnablePercentileCalculations = true,
    MetricsRetentionPeriod = TimeSpan.FromDays(7)
};

graph.ConfigureMetrics(customMetricsOptions);
```

### Preset Configurations

Use predefined configurations for common scenarios:

```csharp
// Development environment (detailed tracking)
var devOptions = GraphMetricsOptions.CreateDevelopmentOptions();
graph.ConfigureMetrics(devOptions);

// Production environment (performance optimized)
var prodOptions = GraphMetricsOptions.CreateProductionOptions();
graph.ConfigureMetrics(prodOptions);

// High-performance scenario (minimal overhead)
var perfOptions = GraphMetricsOptions.CreatePerformanceOptions();
graph.ConfigureMetrics(perfOptions);
```

## Performance Metrics Collection

### Node-Level Metrics

Track individual node performance characteristics:

```csharp
// Get metrics for a specific node
var nodeMetrics = graph.GetNodeMetrics("processing_node");
if (nodeMetrics != null)
{
    Console.WriteLine($"Node: {nodeMetrics.NodeName}");
    Console.WriteLine($"Total Executions: {nodeMetrics.TotalExecutions}");
    Console.WriteLine($"Success Rate: {nodeMetrics.SuccessRate:F1}%");
    Console.WriteLine($"Average Time: {nodeMetrics.AverageExecutionTime.TotalMilliseconds:F2}ms");
    
    // Get percentile performance
    var p50 = nodeMetrics.GetPercentile(50);
    var p95 = nodeMetrics.GetPercentile(95);
    var p99 = nodeMetrics.GetPercentile(99);
    
    Console.WriteLine($"P50: {p50.TotalMilliseconds:F2}ms");
    Console.WriteLine($"P95: {p95.TotalMilliseconds:F2}ms");
    Console.WriteLine($"P99: {p99.TotalMilliseconds:F2}ms");
    
    // Performance classification
    var rating = nodeMetrics.GetPerformanceClassification();
    Console.WriteLine($"Performance Rating: {rating}");
}
```

### Execution Path Metrics

Analyze complete execution routes through the graph:

```csharp
// Get all execution path metrics
var pathMetrics = graph.GetAllPathMetrics();
foreach (var path in pathMetrics.OrderByDescending(p => p.Value.ExecutionCount))
{
    var metrics = path.Value;
    Console.WriteLine($"Path: {metrics.PathKey}");
    Console.WriteLine($"  Executions: {metrics.ExecutionCount}");
    Console.WriteLine($"  Success Rate: {metrics.SuccessRate:F1}%");
    Console.WriteLine($"  Average Time: {metrics.AverageExecutionTime.TotalMilliseconds:F2}ms");
    Console.WriteLine($"  Path Length: {metrics.PathLength} nodes");
    Console.WriteLine($"  Frequency: {metrics.ExecutionsPerHour:F2}/hour");
    
    // Get path-specific percentiles
    var p95 = metrics.GetPercentile(95);
    Console.WriteLine($"  P95: {p95.TotalMilliseconds:F2}ms");
}
```

### Overall Performance Summary

Get comprehensive performance overview:

```csharp
// Retrieve a performance summary (synchronous accessor). This call is typically
// fast because it reads in-memory collected metrics; guard against null when
// metrics are not yet available (e.g., immediately after startup).
// Get performance summary for the last hour
var summary = graph.GetPerformanceSummary(TimeSpan.FromHours(1));
if (summary != null)
{
    Console.WriteLine("📊 PERFORMANCE SUMMARY");
    Console.WriteLine("".PadRight(50, '-'));
    Console.WriteLine($"Total Executions: {summary.TotalExecutions}");
    Console.WriteLine($"Success Rate: {summary.SuccessRate:F1}%");
    Console.WriteLine($"Average Execution Time: {summary.AverageExecutionTime.TotalMilliseconds:F2}ms");
    Console.WriteLine($"Min/Max Time: {summary.MinExecutionTime.TotalMilliseconds:F2}ms / {summary.MaxExecutionTime.TotalMilliseconds:F2}ms");
    Console.WriteLine($"Throughput: {summary.Throughput:F2} executions/second");
    Console.WriteLine($"Current CPU Usage: {summary.CurrentCpuUsage:F1}%");
    Console.WriteLine($"Available Memory: {summary.CurrentAvailableMemoryMB:F0} MB");
    
    // System health assessment
    var isHealthy = summary.IsHealthy();
    Console.WriteLine($"System Health: {(isHealthy ? "🟢 HEALTHY" : "🔴 NEEDS ATTENTION")}");
    
    if (!isHealthy)
    {
        var alerts = summary.GetPerformanceAlerts();
        Console.WriteLine("Performance Alerts:");
        foreach (var alert in alerts)
        {
            Console.WriteLine($"  - {alert}");
        }
    }
}
```

## Resource Monitoring

### System Resource Tracking

Monitor CPU and memory usage during graph execution:

```csharp
// Enable resource monitoring
var resourceOptions = new GraphMetricsOptions
{
    EnableResourceMonitoring = true,
    ResourceSamplingInterval = TimeSpan.FromSeconds(5)
};

graph.ConfigureMetrics(resourceOptions);

// Access current resource metrics
var metrics = graph.GetPerformanceMetrics();
if (metrics != null)
{
    Console.WriteLine($"Current CPU Usage: {metrics.CurrentCpuUsage:F1}%");
    Console.WriteLine($"Available Memory: {metrics.CurrentAvailableMemoryMB:F0} MB");
    Console.WriteLine($"Overall Throughput: {metrics.OverallThroughput:F2} executions/sec");
    Console.WriteLine($"Average Latency: {metrics.AverageExecutionLatency.TotalMilliseconds:F2}ms");
}
```

### Resource Sampling Configuration

Configure resource monitoring behavior:

```csharp
var resourceOptions = new GraphMetricsOptions
{
    EnableResourceMonitoring = true,
    ResourceSamplingInterval = TimeSpan.FromSeconds(10), // Sample every 10 seconds
    MaxSampleHistory = 10000,                           // Keep 10K samples
    MetricsRetentionPeriod = TimeSpan.FromDays(7)       // Retain for 7 days
};

graph.ConfigureMetrics(resourceOptions);
```

## Metrics Export and Integration

### GraphMetricsExporter

Export metrics to various monitoring systems:

```csharp
using SemanticKernel.Graph.Core;

var exporter = new GraphMetricsExporter(
    new GraphMetricsExportOptions
    {
        IndentedOutput = true,
        UseCamelCase = true,
        IncludePercentileData = true,
        IncludeTrendAnalysis = true,
        IncludeRecommendations = true
    }
);

// Export in different formats
var metrics = graph.GetPerformanceMetrics();
if (metrics != null)
{
    // JSON format for web dashboards
    var jsonMetrics = exporter.ExportMetrics(metrics, MetricsExportFormat.Json);
    
    // Prometheus format for monitoring systems
    var prometheusMetrics = exporter.ExportMetrics(metrics, MetricsExportFormat.Prometheus);
    
    // CSV format for spreadsheet analysis
    var csvMetrics = exporter.ExportMetrics(metrics, MetricsExportFormat.Csv);
    
    // XML format for legacy systems
    var xmlMetrics = exporter.ExportMetrics(metrics, MetricsExportFormat.Xml);
}
```

### Dashboard Integration

Export metrics specifically formatted for popular dashboards:

```csharp
// Export for Grafana
var grafanaMetrics = exporter.ExportForDashboard(metrics, DashboardType.Grafana);

// Export for Chart.js
var chartJsMetrics = exporter.ExportForDashboard(metrics, DashboardType.ChartJs);

// Export for custom dashboards
var customMetrics = exporter.ExportForDashboard(metrics, DashboardType.Custom);
```

### Prometheus Integration

Export metrics in Prometheus format for monitoring systems:

```csharp
// Export Prometheus metrics
var prometheusMetrics = exporter.ExportMetrics(metrics, MetricsExportFormat.Prometheus);

// Example output:
// # HELP graph_node_execution_total Total number of node executions
// # TYPE graph_node_execution_total counter
// graph_node_execution_total{node_id="processing_node",node_name="Processing"} 150
// graph_node_execution_total{node_id="decision_node",node_name="Decision"} 75
// 
// # HELP graph_node_execution_duration_seconds Node execution duration in seconds
// # TYPE graph_node_execution_duration_seconds histogram
// graph_node_execution_duration_seconds_bucket{node_id="processing_node",le="0.1"} 45
// graph_node_execution_duration_seconds_bucket{node_id="processing_node",le="0.5"} 120
// graph_node_execution_duration_seconds_bucket{node_id="processing_node",le="1.0"} 150
```

## Execution Tracing and Correlation

### OpenTelemetry Integration

Enable distributed tracing with correlation:

```csharp
using System.Diagnostics;

// Configure ActivitySource for tracing
var activitySource = new ActivitySource("SemanticKernel.Graph");

// Enable tracing in graph options
var graphOptions = new GraphOptions
{
    EnableMetrics = true,
    EnableLogging = true
};

// GraphExecutor automatically creates tracing spans
var graph = new GraphExecutor("TracedGraph", "Graph with tracing enabled");

// Execute with automatic tracing
using var activity = activitySource.StartActivity("Graph.Execute");
if (activity != null)
{
    activity.SetTag("graph.id", graph.GraphId);
    activity.SetTag("graph.name", graph.Name);
    
    var result = await graph.ExecuteAsync(kernel, arguments);
    
    activity.SetTag("execution.success", true);
    activity.SetTag("execution.result", result.GetValue<string>());
}
```

### Span Correlation

Correlate execution spans with streaming events:

```csharp
// Execute with streaming and tracing
var stream = streamingExecutor.ExecuteStreamAsync(kernel, arguments);

await foreach (var evt in stream)
{
    // Each event includes correlation information
    Console.WriteLine($"Event: {evt.EventType}");
    Console.WriteLine($"Execution ID: {evt.ExecutionId}");
    Console.WriteLine($"Node ID: {evt.NodeId}");
    Console.WriteLine($"Correlation ID: {evt.CorrelationId}");
    
    // Use correlation ID to link with tracing spans
    if (Activity.Current != null)
    {
        Activity.Current.SetTag("event.correlation_id", evt.CorrelationId);
        Activity.Current.SetTag("event.node_id", evt.NodeId);
    }
}
```

### Custom Tracing

Add custom tracing to your graph nodes:

```csharp
public class CustomTracingNode : IGraphNode
{
    public async Task<FunctionResult> ExecuteAsync(Kernel kernel, KernelArguments arguments)
    {
        using var activity = ActivitySource.StartActivity("CustomNode.Execute");
        if (activity != null)
        {
            activity.SetTag("node.type", "CustomTracing");
            activity.SetTag("node.custom_data", "example");
        }
        
        try
        {
            // Node execution logic
            var result = await ProcessDataAsync(arguments);
            
            activity?.SetTag("execution.success", true);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetTag("execution.success", false);
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);
            throw;
        }
    }
}
```

## Performance Analysis and Optimization

### Identifying Performance Bottlenecks

Analyze node performance to find bottlenecks:

```csharp
// Get all node metrics and identify slow nodes
var allNodeMetrics = graph.GetAllNodeMetrics();
var slowNodes = allNodeMetrics
    .Where(n => n.Value.AverageExecutionTime.TotalMilliseconds > 1000) // > 1 second
    .OrderByDescending(n => n.Value.AverageExecutionTime.TotalMilliseconds);

Console.WriteLine("🐌 SLOW NODES (>1s average)");
foreach (var node in slowNodes)
{
    var metrics = node.Value;
    Console.WriteLine($"Node: {metrics.NodeName}");
    Console.WriteLine($"  Average Time: {metrics.AverageExecutionTime.TotalMilliseconds:F2}ms");
    Console.WriteLine($"  P95 Time: {metrics.GetPercentile(95).TotalMilliseconds:F2}ms");
    Console.WriteLine($"  Success Rate: {metrics.SuccessRate:F1}%");
    Console.WriteLine($"  Total Executions: {metrics.TotalExecutions}");
}
```

### Path Performance Analysis

Analyze execution path performance:

```csharp
// Find the most frequently executed paths
var frequentPaths = graph.GetAllPathMetrics()
    .OrderByDescending(p => p.Value.ExecutionCount)
    .Take(5);

Console.WriteLine("🛤️ MOST FREQUENT EXECUTION PATHS");
foreach (var path in frequentPaths)
{
    var metrics = path.Value;
    Console.WriteLine($"Path: {metrics.PathKey}");
    Console.WriteLine($"  Frequency: {metrics.ExecutionsPerHour:F2}/hour");
    Console.WriteLine($"  Success Rate: {metrics.SuccessRate:F1}%");
    Console.WriteLine($"  Average Time: {metrics.AverageExecutionTime.TotalMilliseconds:F2}ms");
    
    // Check if path has performance issues
    if (metrics.SuccessRate < 90 || metrics.AverageExecutionTime.TotalMilliseconds > 5000)
    {
        Console.WriteLine("  ⚠️  Performance issues detected!");
    }
}
```

### Trend Analysis

Monitor performance trends over time:

```csharp
// Get performance summary for different time windows
var timeWindows = new[]
{
    TimeSpan.FromMinutes(5),
    TimeSpan.FromMinutes(15),
    TimeSpan.FromHours(1),
    TimeSpan.FromHours(6)
};

Console.WriteLine("📈 PERFORMANCE TRENDS");
foreach (var window in timeWindows)
{
    var summary = graph.GetPerformanceSummary(window);
    if (summary != null)
    {
        Console.WriteLine($"\n{window} Window:");
        Console.WriteLine($"  Executions: {summary.TotalExecutions}");
        Console.WriteLine($"  Success Rate: {summary.SuccessRate:F1}%");
        Console.WriteLine($"  Average Time: {summary.AverageExecutionTime.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Throughput: {summary.Throughput:F2}/sec");
    }
}
```

## Monitoring and Alerting

### Health Checks

Implement automated health monitoring:

```csharp
public class GraphHealthMonitor
{
    public async Task<HealthReport> CheckHealthAsync(GraphExecutor graph)
    {
        var summary = graph.GetPerformanceSummary(TimeSpan.FromMinutes(5));
        if (summary == null)
        {
            return new HealthReport(HealthStatus.Unhealthy, "No metrics available");
        }
        
        var issues = new List<string>();
        
        // Check success rate
        if (summary.SuccessRate < 95)
        {
            issues.Add($"Low success rate: {summary.SuccessRate:F1}%");
        }
        
        // Check response time
        if (summary.AverageExecutionTime.TotalMilliseconds > 5000)
        {
            issues.Add($"High response time: {summary.AverageExecutionTime.TotalMilliseconds:F0}ms");
        }
        
        // Check throughput
        if (summary.Throughput < 1.0)
        {
            issues.Add($"Low throughput: {summary.Throughput:F2}/sec");
        }
        
        // Check system resources
        if (summary.CurrentCpuUsage > 80)
        {
            issues.Add($"High CPU usage: {summary.CurrentCpuUsage:F1}%");
        }
        
        if (summary.CurrentAvailableMemoryMB < 1000)
        {
            issues.Add($"Low memory: {summary.CurrentAvailableMemoryMB:F0} MB available");
        }
        
        var status = issues.Count == 0 ? HealthStatus.Healthy : HealthStatus.Unhealthy;
        return new HealthReport(status, string.Join("; ", issues));
    }
}

// Usage
var healthMonitor = new GraphHealthMonitor();
var health = await healthMonitor.CheckHealthAsync(graph);

if (health.Status == HealthStatus.Unhealthy)
{
    Console.WriteLine($"🔴 Health Check Failed: {health.Description}");
    // Send alert, log error, etc.
}
else
{
    Console.WriteLine("🟢 Health Check Passed");
}
```

### Performance Alerts

Set up automated performance monitoring:

```csharp
public class PerformanceAlerting
{
    private readonly GraphExecutor _graph;
    private readonly Timer _alertingTimer;
    
    public PerformanceAlerting(GraphExecutor graph)
    {
        _graph = graph;
        _alertingTimer = new Timer(CheckPerformance, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }
    
    private void CheckPerformance(object? state)
    {
        var summary = _graph.GetPerformanceSummary(TimeSpan.FromMinutes(5));
        if (summary == null) return;
        
        var alerts = summary.GetPerformanceAlerts();
        foreach (var alert in alerts)
        {
            Console.WriteLine($"🚨 PERFORMANCE ALERT: {alert}");
            // Send notification, log alert, etc.
        }
    }
}

// Usage
var alerting = new PerformanceAlerting(graph);
```

## Best Practices

### Metrics Configuration

* **Development**: Use `CreateDevelopmentOptions()` for detailed debugging
* **Production**: Use `CreateProductionOptions()` for performance optimization
* **High-Throughput**: Use `CreatePerformanceOptions()` for minimal overhead
* **Resource Monitoring**: Enable only when needed to avoid performance impact

### Performance Monitoring

* **Sampling Intervals**: Balance accuracy with performance (5-30 seconds for resources)
* **Retention Periods**: Keep metrics long enough for trend analysis (7-30 days)
* **Percentile Tracking**: Focus on p95 and p99 for latency monitoring
* **Path Analysis**: Monitor execution paths for optimization opportunities

### Export and Integration

* **Prometheus**: Use for Kubernetes and cloud-native monitoring
* **Grafana**: Export dashboard-ready metrics for visualization
* **Custom Dashboards**: Use JSON export for web-based monitoring
* **Alerting**: Set up automated alerts for critical performance issues

### Tracing and Correlation

* **Correlation IDs**: Use stable IDs for linking spans and events
* **Span Naming**: Use descriptive names for better observability
* **Tag Strategy**: Add business context to tracing spans
* **Sampling**: Configure appropriate sampling rates for production

## Troubleshooting

### Common Issues

**Metrics not collecting**: Ensure `EnableMetrics` is true in graph options and metrics are properly configured.

**High memory usage**: Reduce `MaxSampleHistory` and `MaxPathHistoryPerPath` in metrics options.

**Performance impact**: Use production-optimized metrics options and disable resource monitoring if not needed.

**Export failures**: Check export format compatibility and ensure metrics data is available.

### Performance Optimization

```csharp
// Optimize metrics collection for high-throughput scenarios
var optimizedOptions = new GraphMetricsOptions
{
    EnableResourceMonitoring = false,        // Disable if not needed
    ResourceSamplingInterval = TimeSpan.FromMinutes(5),
    MaxSampleHistory = 1000,                // Reduce sample history
    EnableDetailedPathTracking = false,     // Disable if not needed
    MaxPathHistoryPerPath = 100,            // Reduce path history
    EnablePercentileCalculations = true,    // Keep percentiles
    MetricsRetentionPeriod = TimeSpan.FromHours(2), // Shorter retention
    EnableRealTimeMetrics = false,          // Disable for performance
    AggregationInterval = TimeSpan.FromMinutes(5)   // Less frequent aggregation
};

graph.ConfigureMetrics(optimizedOptions);
```

## See Also

* [Debug and Inspection](debug-and-inspection.md) - Using metrics for debugging and analysis
* [State Management](../concepts/state.md) - Understanding execution state and context
* [Graph Execution](../concepts/execution.md) - Execution lifecycle and performance
* [Examples](../../examples/) - Practical examples of metrics and monitoring
