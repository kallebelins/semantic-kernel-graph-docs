# Graph Metrics Example

This example demonstrates how to collect, monitor, and analyze performance metrics in Semantic Kernel Graph workflows. It shows how to implement comprehensive observability for graph execution, including node-level metrics, execution paths, and performance analysis.

## Objective

Learn how to implement comprehensive metrics and monitoring in graph-based workflows to:
- Collect real-time performance metrics during graph execution
- Monitor node execution times, success rates, and resource usage
- Analyze execution paths and identify performance bottlenecks
- Export metrics to various monitoring systems and dashboards
- Implement custom metrics and alerting for production workflows

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Metrics Concepts](../concepts/metrics.md)

## Key Components

### Concepts and Techniques

- **Performance Metrics**: Collection and analysis of execution performance data
- **Node Monitoring**: Real-time monitoring of individual node execution
- **Execution Path Analysis**: Tracking and analyzing execution flow through graphs
- **Resource Monitoring**: Monitoring CPU, memory, and other resource usage
- **Metrics Export**: Exporting metrics to monitoring systems and dashboards

### Core Classes

- `GraphPerformanceMetrics`: Core metrics collection and management
- `NodeExecutionMetrics`: Individual node performance tracking
- `MetricsDashboard`: Real-time metrics visualization and analysis
- `GraphMetricsExporter`: Export metrics to external systems

## Running the Example

### Command Line

```bash
# Navigate to examples project
cd semantic-kernel-graph/src/SemanticKernel.Graph.Examples

# Run the Graph Metrics example
dotnet run -- --example graph-metrics
```

### Programmatic Execution

```csharp
// Run the example directly
await GraphMetricsExample.RunAsync();

// Or run with custom kernel
var kernel = CreateCustomKernel();
await GraphMetricsExample.RunAsync(kernel);
```

## Step-by-Step Implementation

### 1. Basic Metrics Collection

This example demonstrates basic metrics collection during graph execution.

```csharp
// Create kernel with mock configuration
var kernel = CreateKernel();

// Create metrics-enabled workflow
var metricsWorkflow = new GraphExecutor("MetricsWorkflow", "Basic metrics collection", logger);

// Configure metrics collection
var metricsOptions = new GraphMetricsOptions
{
    EnableNodeMetrics = true,
    EnableExecutionMetrics = true,
    EnableResourceMetrics = true,
    MetricsCollectionInterval = TimeSpan.FromMilliseconds(100),
    EnableRealTimeMetrics = true
};

metricsWorkflow.ConfigureMetrics(metricsOptions);

// Sample processing node with metrics
var sampleProcessor = new FunctionGraphNode(
    "sample-processor",
    "Process sample data with metrics",
    async (context) =>
    {
        var startTime = DateTime.UtcNow;
        var inputData = context.GetValue<string>("input_data");
        
        // Simulate processing
        await Task.Delay(Random.Shared.Next(100, 500));
        
        var result = $"Processed: {inputData}";
        var processingTime = DateTime.UtcNow - startTime;
        
        // Record custom metrics
        context.SetValue("processing_time_ms", processingTime.TotalMilliseconds);
        context.SetValue("input_size", inputData.Length);
        context.SetValue("processing_result", result);
        
        return result;
    });

// Metrics collection node
var metricsCollector = new FunctionGraphNode(
    "metrics-collector",
    "Collect and aggregate metrics",
    async (context) =>
    {
        var processingTime = context.GetValue<double>("processing_time_ms");
        var inputSize = context.GetValue<int>("input_size");
        
        // Collect execution metrics
        var executionMetrics = new Dictionary<string, object>
        {
            ["total_processing_time"] = processingTime,
            ["average_input_size"] = inputSize,
            ["execution_count"] = 1,
            ["success_rate"] = 1.0,
            ["timestamp"] = DateTime.UtcNow
        };
        
        context.SetValue("execution_metrics", executionMetrics);
        
        return "Metrics collected successfully";
    });

// Add nodes to workflow
metricsWorkflow.AddNode(sampleProcessor);
metricsWorkflow.AddNode(metricsCollector);

// Set start node
metricsWorkflow.SetStartNode(sampleProcessor.NodeId);

// Test metrics collection
var testData = new[]
{
    "Sample data 1",
    "Sample data 2",
    "Sample data 3"
};

foreach (var data in testData)
{
    var arguments = new KernelArguments
    {
        ["input_data"] = data
    };

    Console.WriteLine($"📊 Testing metrics collection: {data}");
    var result = await metricsWorkflow.ExecuteAsync(kernel, arguments);
    
    var processingTime = result.GetValue<double>("processing_time_ms");
    var inputSize = result.GetValue<int>("input_size");
    var executionMetrics = result.GetValue<Dictionary<string, object>>("execution_metrics");
    
    Console.WriteLine($"   Processing Time: {processingTime:F2} ms");
    Console.WriteLine($"   Input Size: {inputSize} characters");
    Console.WriteLine($"   Metrics Collected: {executionMetrics.Count} metrics");
    Console.WriteLine();
}
```

### 2. Advanced Performance Monitoring

Demonstrates comprehensive performance monitoring with detailed metrics collection.

```csharp
// Create advanced metrics workflow
var advancedMetricsWorkflow = new GraphExecutor("AdvancedMetricsWorkflow", "Advanced performance monitoring", logger);

// Configure advanced metrics
var advancedMetricsOptions = new GraphMetricsOptions
{
    EnableNodeMetrics = true,
    EnableExecutionMetrics = true,
    EnableResourceMetrics = true,
    EnableCustomMetrics = true,
    EnablePerformanceProfiling = true,
    MetricsCollectionInterval = TimeSpan.FromMilliseconds(50),
    EnableRealTimeMetrics = true,
    EnableMetricsPersistence = true,
    MetricsStoragePath = "./metrics-data"
};

advancedMetricsWorkflow.ConfigureMetrics(advancedMetricsOptions);

// Performance-intensive node
var performanceNode = new FunctionGraphNode(
    "performance-node",
    "Performance-intensive processing",
    async (context) =>
    {
        var startTime = DateTime.UtcNow;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var inputData = context.GetValue<string>("input_data");
        var complexity = context.GetValue<int>("complexity_level");
        
        // Simulate complex processing
        var iterations = complexity * 1000;
        var result = 0;
        
        for (int i = 0; i < iterations; i++)
        {
            result += i * i;
            if (i % 100 == 0)
            {
                await Task.Delay(1); // Simulate async work
            }
        }
        
        stopwatch.Stop();
        var processingTime = DateTime.UtcNow - startTime;
        
        // Record detailed metrics
        context.SetValue("processing_time_ms", processingTime.TotalMilliseconds);
        context.SetValue("stopwatch_time_ms", stopwatch.ElapsedMilliseconds);
        context.SetValue("iterations", iterations);
        context.SetValue("result_value", result);
        context.SetValue("complexity_level", complexity);
        context.SetValue("cpu_usage", GetCurrentCpuUsage());
        context.SetValue("memory_usage", GetCurrentMemoryUsage());
        
        return $"Processed {iterations} iterations in {processingTime.TotalMilliseconds:F2} ms";
    });

// Metrics analyzer node
var metricsAnalyzer = new FunctionGraphNode(
    "metrics-analyzer",
    "Analyze collected metrics",
    async (context) =>
    {
        var processingTime = context.GetValue<double>("processing_time_ms");
        var stopwatchTime = context.GetValue<long>("stopwatch_time_ms");
        var iterations = context.GetValue<int>("iterations");
        var complexity = context.GetValue<int>("complexity_level");
        var cpuUsage = context.GetValue<double>("cpu_usage");
        var memoryUsage = context.GetValue<double>("memory_usage");
        
        // Calculate performance metrics
        var throughput = iterations / (processingTime / 1000.0); // iterations per second
        var efficiency = (double)stopwatchTime / processingTime.TotalMilliseconds;
        var resourceIntensity = (cpuUsage + memoryUsage) / 2.0;
        
        var analysis = new Dictionary<string, object>
        {
            ["throughput_ops_per_sec"] = throughput,
            ["efficiency_ratio"] = efficiency,
            ["resource_intensity"] = resourceIntensity,
            ["performance_score"] = CalculatePerformanceScore(throughput, efficiency, resourceIntensity),
            ["bottleneck_analysis"] = AnalyzeBottlenecks(processingTime, cpuUsage, memoryUsage),
            ["optimization_suggestions"] = GenerateOptimizationSuggestions(throughput, efficiency, resourceIntensity)
        };
        
        context.SetValue("performance_analysis", analysis);
        
        return $"Performance analysis completed. Score: {analysis["performance_score"]:F2}";
    });

// Add nodes to advanced workflow
advancedMetricsWorkflow.AddNode(performanceNode);
advancedMetricsWorkflow.AddNode(metricsAnalyzer);

// Set start node
advancedMetricsWorkflow.SetStartNode(performanceNode.NodeId);

// Test advanced performance monitoring
var performanceTestScenarios = new[]
{
    new { Data = "Low complexity task", Complexity = 1 },
    new { Data = "Medium complexity task", Complexity = 5 },
    new { Data = "High complexity task", Complexity = 10 }
};

foreach (var scenario in performanceTestScenarios)
{
    var arguments = new KernelArguments
    {
        ["input_data"] = scenario.Data,
        ["complexity_level"] = scenario.Complexity
    };

    Console.WriteLine($"🚀 Testing performance monitoring: {scenario.Data}");
    Console.WriteLine($"   Complexity Level: {scenario.Complexity}");
    
    var result = await advancedMetricsWorkflow.ExecuteAsync(kernel, arguments);
    
    var processingTime = result.GetValue<double>("processing_time_ms");
    var iterations = result.GetValue<int>("iterations");
    var performanceAnalysis = result.GetValue<Dictionary<string, object>>("performance_analysis");
    
    Console.WriteLine($"   Processing Time: {processingTime:F2} ms");
    Console.WriteLine($"   Iterations: {iterations:N0}");
    Console.WriteLine($"   Throughput: {performanceAnalysis["throughput_ops_per_sec"]:F0} ops/sec");
    Console.WriteLine($"   Performance Score: {performanceAnalysis["performance_score"]:F2}");
    Console.WriteLine($"   Bottleneck: {performanceAnalysis["bottleneck_analysis"]}");
    Console.WriteLine();
}
```

### 3. Real-Time Metrics Dashboard

Shows how to implement real-time metrics visualization and monitoring.

```csharp
// Create real-time metrics workflow
var realTimeMetricsWorkflow = new GraphExecutor("RealTimeMetricsWorkflow", "Real-time metrics dashboard", logger);

// Configure real-time metrics
var realTimeMetricsOptions = new GraphMetricsOptions
{
    EnableNodeMetrics = true,
    EnableExecutionMetrics = true,
    EnableResourceMetrics = true,
    EnableRealTimeMetrics = true,
    EnableMetricsStreaming = true,
    MetricsCollectionInterval = TimeSpan.FromMilliseconds(100),
    EnableMetricsDashboard = true,
    DashboardUpdateInterval = TimeSpan.FromMilliseconds(500)
};

realTimeMetricsWorkflow.ConfigureMetrics(realTimeMetricsOptions);

// Real-time data generator
var dataGenerator = new FunctionGraphNode(
    "data-generator",
    "Generate real-time data for metrics",
    async (context) =>
    {
        var iteration = context.GetValue<int>("iteration", 0);
        var baseValue = context.GetValue<double>("base_value", 100.0);
        
        // Generate varying data
        var randomFactor = Random.Shared.NextDouble() * 0.4 + 0.8; // 0.8 to 1.2
        var currentValue = baseValue * randomFactor;
        
        // Simulate processing
        await Task.Delay(Random.Shared.Next(50, 200));
        
        context.SetValue("iteration", iteration + 1);
        context.SetValue("current_value", currentValue);
        context.SetValue("random_factor", randomFactor);
        context.SetValue("generation_timestamp", DateTime.UtcNow);
        
        return $"Generated value: {currentValue:F2} (iteration {iteration + 1})";
    });

// Real-time metrics processor
var realTimeProcessor = new FunctionGraphNode(
    "real-time-processor",
    "Process real-time metrics",
    async (context) =>
    {
        var currentValue = context.GetValue<double>("current_value");
        var iteration = context.GetValue<int>("iteration");
        var timestamp = context.GetValue<DateTime>("generation_timestamp");
        
        // Calculate real-time statistics
        var values = context.GetValue<List<double>>("value_history", new List<double>());
        values.Add(currentValue);
        
        var average = values.Average();
        var min = values.Min();
        var max = values.Max();
        var trend = CalculateTrend(values);
        
        var realTimeMetrics = new Dictionary<string, object>
        {
            ["current_value"] = currentValue,
            ["average_value"] = average,
            ["min_value"] = min,
            ["max_value"] = max,
            ["trend"] = trend,
            ["total_iterations"] = iteration,
            ["last_update"] = timestamp,
            ["value_history"] = values.TakeLast(100).ToList() // Keep last 100 values
        };
        
        context.SetValue("value_history", values);
        context.SetValue("real_time_metrics", realTimeMetrics);
        
        return $"Real-time metrics updated. Average: {average:F2}, Trend: {trend}";
    });

// Metrics dashboard updater
var dashboardUpdater = new FunctionGraphNode(
    "dashboard-updater",
    "Update metrics dashboard",
    async (context) =>
    {
        var realTimeMetrics = context.GetValue<Dictionary<string, object>>("real_time_metrics");
        var iteration = context.GetValue<int>("iteration");
        
        // Update dashboard
        UpdateMetricsDashboard(realTimeMetrics);
        
        // Check for alerts
        var alerts = CheckMetricsAlerts(realTimeMetrics);
        if (alerts.Any())
        {
            context.SetValue("active_alerts", alerts);
            Console.WriteLine($"🚨 Alerts detected: {string.Join(", ", alerts)}");
        }
        
        context.SetValue("dashboard_updated", true);
        context.SetValue("last_dashboard_update", DateTime.UtcNow);
        
        return $"Dashboard updated. Iteration {iteration}, {alerts.Count} active alerts";
    });

// Add nodes to real-time workflow
realTimeMetricsWorkflow.AddNode(dataGenerator);
realTimeMetricsWorkflow.AddNode(realTimeProcessor);
realTimeMetricsWorkflow.AddNode(dashboardUpdater);

// Set start node
realTimeMetricsWorkflow.SetStartNode(dataGenerator.NodeId);

// Test real-time metrics
Console.WriteLine("📊 Starting real-time metrics collection...");
Console.WriteLine("   Dashboard will update every 500ms");
Console.WriteLine("   Press any key to stop...");

var realTimeArguments = new KernelArguments
{
    ["iteration"] = 0,
    ["base_value"] = 100.0
};

// Run real-time metrics for a few iterations
for (int i = 0; i < 10; i++)
{
    var result = await realTimeMetricsWorkflow.ExecuteAsync(kernel, realTimeArguments);
    
    var realTimeMetrics = result.GetValue<Dictionary<string, object>>("real_time_metrics");
    var dashboardUpdated = result.GetValue<bool>("dashboard_updated");
    
    if (realTimeMetrics != null)
    {
        Console.WriteLine($"   Iteration {realTimeMetrics["total_iterations"]}: " +
                         $"Current: {realTimeMetrics["current_value"]:F2}, " +
                         $"Avg: {realTimeMetrics["average_value"]:F2}, " +
                         $"Trend: {realTimeMetrics["trend"]}");
    }
    
    // Update arguments for next iteration
    realTimeArguments["iteration"] = result.GetValue<int>("iteration");
    realTimeArguments["base_value"] = result.GetValue<double>("current_value");
    
    await Task.Delay(1000); // Wait 1 second between iterations
}

Console.WriteLine("✅ Real-time metrics collection completed");
```

### 4. Metrics Export and Integration

Demonstrates exporting metrics to external monitoring systems and dashboards.

```csharp
// Create metrics export workflow
var metricsExportWorkflow = new GraphExecutor("MetricsExportWorkflow", "Metrics export and integration", logger);

// Configure metrics export
var exportMetricsOptions = new GraphMetricsOptions
{
    EnableNodeMetrics = true,
    EnableExecutionMetrics = true,
    EnableResourceMetrics = true,
    EnableMetricsExport = true,
    EnableMetricsPersistence = true,
    MetricsStoragePath = "./exported-metrics",
    ExportFormats = new[] { "json", "csv", "prometheus" },
    ExportInterval = TimeSpan.FromSeconds(5)
};

metricsExportWorkflow.ConfigureMetrics(exportMetricsOptions);

// Metrics aggregator
var metricsAggregator = new FunctionGraphNode(
    "metrics-aggregator",
    "Aggregate metrics for export",
    async (context) =>
    {
        var executionCount = context.GetValue<int>("execution_count", 0);
        var totalProcessingTime = context.GetValue<double>("total_processing_time", 0.0);
        var successCount = context.GetValue<int>("success_count", 0);
        var errorCount = context.GetValue<int>("error_count", 0);
        
        // Aggregate metrics
        var aggregatedMetrics = new Dictionary<string, object>
        {
            ["execution_count"] = executionCount,
            ["total_processing_time_ms"] = totalProcessingTime,
            ["success_count"] = successCount,
            ["error_count"] = errorCount,
            ["success_rate"] = executionCount > 0 ? (double)successCount / executionCount : 0.0,
            ["average_processing_time_ms"] = executionCount > 0 ? totalProcessingTime / executionCount : 0.0,
            ["error_rate"] = executionCount > 0 ? (double)errorCount / executionCount : 0.0,
            ["aggregation_timestamp"] = DateTime.UtcNow,
            ["metrics_version"] = "1.0.0"
        };
        
        context.SetValue("aggregated_metrics", aggregatedMetrics);
        
        return $"Metrics aggregated: {executionCount} executions, {successCount} successes, {errorCount} errors";
    });

// Metrics exporter
var metricsExporter = new FunctionGraphNode(
    "metrics-exporter",
    "Export metrics to external systems",
    async (context) =>
    {
        var aggregatedMetrics = context.GetValue<Dictionary<string, object>>("aggregated_metrics");
        
        // Export to different formats
        var exportResults = new Dictionary<string, string>();
        
        // JSON export
        var jsonExport = await ExportToJson(aggregatedMetrics);
        exportResults["json"] = jsonExport;
        
        // CSV export
        var csvExport = await ExportToCsv(aggregatedMetrics);
        exportResults["csv"] = csvExport;
        
        // Prometheus export
        var prometheusExport = await ExportToPrometheus(aggregatedMetrics);
        exportResults["prometheus"] = prometheusExport;
        
        // Export to monitoring systems
        var monitoringExport = await ExportToMonitoringSystems(aggregatedMetrics);
        exportResults["monitoring"] = monitoringExport;
        
        context.SetValue("export_results", exportResults);
        context.SetValue("export_timestamp", DateTime.UtcNow);
        
        return $"Metrics exported to {exportResults.Count} formats";
    });

// Add nodes to export workflow
metricsExportWorkflow.AddNode(metricsAggregator);
metricsExportWorkflow.AddNode(metricsExporter);

// Set start node
metricsExportWorkflow.SetStartNode(metricsAggregator.NodeId);

// Test metrics export
var exportTestData = new[]
{
    new { Executions = 10, ProcessingTime = 1500.0, Successes = 9, Errors = 1 },
    new { Executions = 25, ProcessingTime = 3200.0, Successes = 24, Errors = 1 },
    new { Executions = 50, ProcessingTime = 7500.0, Successes = 48, Errors = 2 }
};

foreach (var data in exportTestData)
{
    var arguments = new KernelArguments
    {
        ["execution_count"] = data.Executions,
        ["total_processing_time"] = data.ProcessingTime,
        ["success_count"] = data.Successes,
        ["error_count"] = data.Errors
    };

    Console.WriteLine($"📤 Testing metrics export: {data.Executions} executions");
    var result = await metricsExportWorkflow.ExecuteAsync(kernel, arguments);
    
    var aggregatedMetrics = result.GetValue<Dictionary<string, object>>("aggregated_metrics");
    var exportResults = result.GetValue<Dictionary<string, string>>("export_results");
    
    Console.WriteLine($"   Success Rate: {aggregatedMetrics["success_rate"]:P1}");
    Console.WriteLine($"   Average Time: {aggregatedMetrics["average_processing_time_ms"]:F2} ms");
    Console.WriteLine($"   Export Formats: {string.Join(", ", exportResults.Keys)}");
    Console.WriteLine();
}
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

- [Graph Visualization](./graph-visualization.md): Visual metrics representation
- [Performance Optimization](./performance-optimization.md): Using metrics for optimization
- [Streaming Execution](./streaming-execution.md): Real-time metrics streaming
- [Debug and Inspection](./debug-inspection.md): Metrics for debugging

## See Also

- [Metrics and Observability](../concepts/metrics.md): Understanding metrics concepts
- [Performance Monitoring](../how-to/performance-monitoring.md): Performance monitoring patterns
- [Debug and Inspection](../how-to/debug-and-inspection.md): Using metrics for debugging
- [API Reference](../api/): Complete API documentation
