# Graph Metrics Example

This example demonstrates how to collect, monitor, and analyze performance metrics in Semantic Kernel Graph workflows. It shows how to implement comprehensive observability for graph execution, including node-level metrics, execution paths, and performance analysis.

## Objective

Learn how to implement comprehensive metrics and monitoring in graph-based workflows to:
* Collect real-time performance metrics during graph execution
* Monitor node execution times, success rates, and resource usage
* Analyze execution paths and identify performance bottlenecks
* Export metrics to various monitoring systems and dashboards
* Implement custom metrics and alerting for production workflows

## Prerequisites

* **.NET 8.0** or later
* **OpenAI API Key** configured in `appsettings.json`
* **Semantic Kernel Graph package** installed
* Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Metrics Concepts](../concepts/metrics.md)

## Key Components

### Concepts and Techniques

* **Performance Metrics**: Collection and analysis of execution performance data
* **Node Monitoring**: Real-time monitoring of individual node execution
* **Execution Path Analysis**: Tracking and analyzing execution flow through graphs
* **Resource Monitoring**: Monitoring CPU, memory, and other resource usage
* **Metrics Export**: Exporting metrics to monitoring systems and dashboards

### Core Classes

* `GraphPerformanceMetrics`: Core metrics collection and management
* `NodeExecutionMetrics`: Individual node performance tracking
* `MetricsDashboard`: Real-time metrics visualization and analysis
* `GraphMetricsExporter`: Export metrics to external systems

## Running the Example

### Getting Started

This example demonstrates metrics collection and performance monitoring with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Basic Metrics Collection

This example demonstrates basic metrics collection during graph execution.

```csharp
// Create development-friendly metrics options (short intervals for demos)
var options = GraphMetricsOptions.CreateDevelopmentOptions();
options.EnableResourceMonitoring = false; // keep the demo deterministic

// Create the metrics collector using the options
using var metrics = new GraphPerformanceMetrics(options);

// Simulate a few node executions and record metrics for each
for (int i = 0; i < 5; i++)
{
    var execId = $"exec-{i}";

    // Start tracking a node execution
    var tracker = metrics.StartNodeTracking($"node-{i}", $"node-name-{i}", execId);

    // Simulate work
    await Task.Delay(10 + i * 5);

    // Mark completion and record an execution path for analysis
    metrics.CompleteNodeTracking(tracker, success: true, result: null);
    metrics.RecordExecutionPath(execId, new[] { tracker.NodeId }, TimeSpan.FromMilliseconds(10 + i * 5), success: true);
}

// Export a preview using the metrics exporter
using var exporter = new GraphMetricsExporter();
var json = exporter.ExportMetrics(metrics, MetricsExportFormat.Json, TimeSpan.FromMinutes(10));
Console.WriteLine("\n--- JSON Export Preview ---\n");
Console.WriteLine(json.Substring(0, Math.Min(800, json.Length)));

var prometheus = exporter.ExportMetrics(metrics, MetricsExportFormat.Prometheus, TimeSpan.FromMinutes(10));
Console.WriteLine("\n--- Prometheus Export Preview ---\n");
Console.WriteLine(prometheus.Substring(0, Math.Min(800, prometheus.Length)));
```

### 2. Advanced Performance Monitoring

Demonstrates comprehensive performance monitoring with detailed metrics collection.

```csharp
// Create development options and a metrics collector suitable for profiling demos
var options = GraphMetricsOptions.CreateDevelopmentOptions();
using var metrics = new GraphPerformanceMetrics(options);

// Simulate an expensive operation and record detailed timing information
var execId = "advanced-exec-1";
var tracker = metrics.StartNodeTracking("performance-node", "performance-node", execId);
var stopwatch = System.Diagnostics.Stopwatch.StartNew();

// Simulate CPU-bound work with occasional awaits to avoid blocking the thread pool
int iterations = 10_000;
long result = 0;
for (int i = 0; i < iterations; i++)
{
    result += i * i;
    if (i % 1000 == 0) await Task.Delay(1); // cooperative pause to keep responsiveness
}

stopwatch.Stop();
metrics.CompleteNodeTracking(tracker, success: true, result: result);

// Use exporter to inspect metrics after the simulated workload
using var exporter = new GraphMetricsExporter();
Console.WriteLine(exporter.ExportMetrics(metrics, MetricsExportFormat.Json, TimeSpan.FromMinutes(10)));
```

### 3. Real-Time Metrics Dashboard

Shows how to implement real-time metrics visualization and monitoring.

```csharp
// Demonstrate basic real-time sampling using the metrics collector's resource sampling
var options = GraphMetricsOptions.CreateDevelopmentOptions();
options.EnableResourceMonitoring = true;
using var metrics = new GraphPerformanceMetrics(options);

// Simulate a stream of short executions and display sampled CPU/memory
for (int i = 0; i < 10; i++)
{
    var execId = $"rt-{i}";
    var tracker = metrics.StartNodeTracking("data-generator", "data-generator", execId);

    // Simulate some processing latency
    await Task.Delay(Random.Shared.Next(50, 200));

    metrics.CompleteNodeTracking(tracker, success: true);

    // Read current sampled system metrics (collector updates them periodically)
    Console.WriteLine($"Iteration {i + 1}: CPU={metrics.CurrentCpuUsage:F1}% Memory={metrics.CurrentAvailableMemoryMB:F0} MB");

    await Task.Delay(500); // throttle updates for the demo
}

Console.WriteLine("✅ Real-time sampling demo completed");
```

### 4. Metrics Export and Integration

Demonstrates exporting metrics to external monitoring systems and dashboards.

```csharp
// Export example using the metrics exporter directly
using var exporter = new GraphMetricsExporter(new GraphMetricsExportOptions { IndentedOutput = true });

// Export JSON
var jsonExport = exporter.ExportMetrics(metrics, MetricsExportFormat.Json, TimeSpan.FromMinutes(10));
Console.WriteLine("--- JSON Export ---");
Console.WriteLine(jsonExport);

// Export CSV
var csvExport = exporter.ExportMetrics(metrics, MetricsExportFormat.Csv, TimeSpan.FromMinutes(10));
Console.WriteLine("--- CSV Export ---");
Console.WriteLine(csvExport.Split('\n').Take(20)); // preview first lines

// Export Prometheus
var promExport = exporter.ExportMetrics(metrics, MetricsExportFormat.Prometheus, TimeSpan.FromMinutes(10));
Console.WriteLine("--- Prometheus Export ---");
Console.WriteLine(promExport);
```

## Expected Output

### Basic Metrics Collection Example

```
📊 Testing metrics collection: Sample data 1
   Processing Time: 234.56 ms
   Input Size: 12 characters
   Metrics Collected: 5 metrics

📊 Testing metrics collection: Sample data 2
   Processing Time: 187.23 ms
   Input Size: 12 characters
   Metrics Collected: 5 metrics
```

### Advanced Performance Monitoring Example

```
🚀 Testing performance monitoring: Low complexity task
   Complexity Level: 1
   Processing Time: 156.78 ms
   Iterations: 1,000
   Throughput: 6,374 ops/sec
   Performance Score: 85.67
   Bottleneck: CPU-bound

🚀 Testing performance monitoring: High complexity task
   Complexity Level: 10
   Processing Time: 1,234.56 ms
   Iterations: 10,000
   Throughput: 8,101 ops/sec
   Performance Score: 92.34
   Bottleneck: Memory-bound
```

### Real-Time Metrics Dashboard Example

```
📊 Starting real-time metrics collection...
   Dashboard will update every 500ms
   Press any key to stop...
   Iteration 1: Current: 87.45, Avg: 87.45, Trend: stable
   Iteration 2: Current: 112.34, Avg: 99.90, Trend: increasing
   Iteration 3: Current: 95.67, Avg: 98.49, Trend: decreasing
✅ Real-time metrics collection completed
```

### Metrics Export Example

```
📤 Testing metrics export: 10 executions
   Success Rate: 90.0%
   Average Time: 150.00 ms
   Export Formats: json, csv, prometheus, monitoring

📤 Testing metrics export: 50 executions
   Success Rate: 96.0%
   Average Time: 150.00 ms
   Export Formats: json, csv, prometheus, monitoring
```

## Configuration Options

### Metrics Configuration

```csharp
var metricsOptions = new GraphMetricsOptions
{
    EnableNodeMetrics = true,                        // Enable node-level metrics
    EnableExecutionMetrics = true,                   // Enable execution-level metrics
    EnableResourceMetrics = true,                    // Enable resource usage metrics
    EnableCustomMetrics = true,                      // Enable custom metrics
    EnablePerformanceProfiling = true,               // Enable performance profiling
    EnableRealTimeMetrics = true,                    // Enable real-time metrics
    EnableMetricsStreaming = true,                   // Enable metrics streaming
    EnableMetricsDashboard = true,                   // Enable metrics dashboard
    EnableMetricsExport = true,                      // Enable metrics export
    EnableMetricsPersistence = true,                 // Enable metrics persistence
    MetricsCollectionInterval = TimeSpan.FromMilliseconds(100), // Collection interval
    DashboardUpdateInterval = TimeSpan.FromMilliseconds(500),   // Dashboard update interval
    ExportInterval = TimeSpan.FromSeconds(5),        // Export interval
    MetricsStoragePath = "./metrics-data",           // Metrics storage path
    ExportFormats = new[] { "json", "csv", "prometheus" },     // Export formats
    EnableMetricsCompression = true,                 // Enable metrics compression
    MaxMetricsHistory = 10000,                       // Maximum metrics history
    EnableMetricsAggregation = true,                 // Enable metrics aggregation
    AggregationInterval = TimeSpan.FromMinutes(1)    // Aggregation interval
};
```

### Performance Profiling Configuration

```csharp
var profilingOptions = new PerformanceProfilingOptions
{
    EnableDetailedProfiling = true,                  // Enable detailed profiling
    EnableMemoryProfiling = true,                    // Enable memory profiling
    EnableCpuProfiling = true,                       // Enable CPU profiling
    EnableNetworkProfiling = true,                   // Enable network profiling
    ProfilingSamplingRate = 0.1,                     // Profiling sampling rate (10%)
    EnableHotPathAnalysis = true,                    // Enable hot path analysis
    EnableBottleneckDetection = true,                // Enable bottleneck detection
    ProfilingOutputPath = "./profiling-data",         // Profiling output path
    EnableProfilingVisualization = true,             // Enable profiling visualization
    MaxProfilingDataSize = 100 * 1024 * 1024        // Max profiling data size (100MB)
};
```

## Troubleshooting

### Common Issues

#### Metrics Not Being Collected
```bash
# Problem: Metrics are not being collected
# Solution: Enable metrics collection and check configuration
EnableNodeMetrics = true;
EnableExecutionMetrics = true;
MetricsCollectionInterval = TimeSpan.FromMilliseconds(100);
```

#### Performance Impact
```bash
# Problem: Metrics collection impacts performance
# Solution: Adjust collection interval and enable sampling
MetricsCollectionInterval = TimeSpan.FromSeconds(1);
EnableMetricsSampling = true;
SamplingRate = 0.1; // 10% sampling
```

#### Memory Issues
```bash
# Problem: Metrics consume too much memory
# Solution: Enable compression and limit history
EnableMetricsCompression = true;
MaxMetricsHistory = 1000;
EnableMetricsAggregation = true;
```

### Debug Mode

Enable detailed logging for troubleshooting:

```csharp
// Enable debug logging
var logger = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
}).CreateLogger<GraphMetricsExample>();

// Configure metrics with debug logging
var debugMetricsOptions = new GraphMetricsOptions
{
    EnableNodeMetrics = true,
    EnableExecutionMetrics = true,
    EnableResourceMetrics = true,
    EnableDebugLogging = true,
    LogMetricsCollection = true,
    LogMetricsExport = true
};
```

## Advanced Patterns

### Custom Metrics Collection

```csharp
// Implement custom metrics collection
public class CustomMetricsCollector : IMetricsCollector
{
    public async Task<Dictionary<string, object>> CollectMetricsAsync(MetricsContext context)
    {
        var customMetrics = new Dictionary<string, object>();
        
        // Collect custom business metrics
        customMetrics["business_value"] = await CalculateBusinessValue(context);
        customMetrics["user_satisfaction"] = await MeasureUserSatisfaction(context);
        customMetrics["cost_per_execution"] = await CalculateCostPerExecution(context);
        
        // Collect domain-specific metrics
        customMetrics["domain_accuracy"] = await MeasureDomainAccuracy(context);
        customMetrics["processing_efficiency"] = await MeasureProcessingEfficiency(context);
        
        return customMetrics;
    }
}
```

### Metrics Aggregation and Analysis

```csharp
// Implement custom metrics aggregation
public class MetricsAggregator : IMetricsAggregator
{
    public async Task<AggregatedMetrics> AggregateMetricsAsync(IEnumerable<MetricsSnapshot> snapshots)
    {
        var aggregated = new AggregatedMetrics();
        
        foreach (var snapshot in snapshots)
        {
            // Aggregate performance metrics
            aggregated.TotalExecutions += snapshot.ExecutionCount;
            aggregated.TotalProcessingTime += snapshot.TotalProcessingTime;
            aggregated.SuccessCount += snapshot.SuccessCount;
            aggregated.ErrorCount += snapshot.ErrorCount;
            
            // Track trends
            aggregated.ExecutionTrends.Add(snapshot.Timestamp, snapshot.ExecutionCount);
            aggregated.PerformanceTrends.Add(snapshot.Timestamp, snapshot.AverageProcessingTime);
        }
        
        // Calculate derived metrics
        aggregated.SuccessRate = (double)aggregated.SuccessCount / aggregated.TotalExecutions;
        aggregated.AverageProcessingTime = aggregated.TotalProcessingTime / aggregated.TotalExecutions;
        aggregated.ErrorRate = (double)aggregated.ErrorCount / aggregated.TotalExecutions;
        
        return aggregated;
    }
}
```

### Real-Time Alerting

```csharp
// Implement real-time metrics alerting
public class MetricsAlerting : IMetricsAlerting
{
    private readonly List<AlertRule> _alertRules;
    
    public async Task<List<Alert>> CheckAlertsAsync(MetricsSnapshot metrics)
    {
        var alerts = new List<Alert>();
        
        foreach (var rule in _alertRules)
        {
            if (await rule.EvaluateAsync(metrics))
            {
                alerts.Add(new Alert
                {
                    RuleId = rule.RuleId,
                    Severity = rule.Severity,
                    Message = rule.GenerateMessage(metrics),
                    Timestamp = DateTime.UtcNow,
                    Metrics = metrics
                });
            }
        }
        
        return alerts;
    }
}

// Example alert rules
public class AlertRule
{
    public string RuleId { get; set; }
    public AlertSeverity Severity { get; set; }
    public Func<MetricsSnapshot, Task<bool>> Condition { get; set; }
    
    public static AlertRule CreateHighErrorRateRule()
    {
        return new AlertRule
        {
            RuleId = "high_error_rate",
            Severity = AlertSeverity.Critical,
            Condition = async (metrics) => metrics.ErrorRate > 0.1 // 10% error rate
        };
    }
}
```

## Related Examples

* [Graph Visualization](./graph-visualization.md): Visual metrics representation
* [Performance Optimization](./performance-optimization.md): Using metrics for optimization
* [Streaming Execution](./streaming-execution.md): Real-time metrics streaming
* [Debug and Inspection](./debug-inspection.md): Metrics for debugging

## See Also

* [Metrics and Observability](../concepts/metrics.md): Understanding metrics concepts
* [Performance Monitoring](../how-to/performance-monitoring.md): Performance monitoring patterns
* [Debug and Inspection](../how-to/debug-and-inspection.md): Using metrics for debugging
* [API Reference](../api/): Complete API documentation
