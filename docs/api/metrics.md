# Metrics APIs Reference

This reference documents the comprehensive metrics and performance monitoring APIs in SemanticKernel.Graph, which provide detailed insights into graph execution performance, resource usage, and operational health.

## GraphPerformanceMetrics

Comprehensive performance metrics collector for graph execution. Tracks node-level metrics, execution paths, resource usage, and performance indicators.

### Properties

* `TotalExecutions`: Total number of graph executions tracked
* `Uptime`: Time since metrics collection started
* `NodeMetrics`: Dictionary of metrics per node
* `PathMetrics`: Dictionary of metrics per execution path
* `CircuitBreakerMetrics`: Dictionary of circuit breaker metrics per node
* `ResourceUsage`: Current system resource usage (CPU, memory)
* `LastSampleTime`: Timestamp of the last resource sample

### Methods

#### StartNodeTracking

```csharp
public NodeExecutionTracker StartNodeTracking(string nodeId, string nodeName, string executionId)
```

Starts tracking a node execution and returns a tracker for completion.

**Parameters:**
* `nodeId`: Node identifier
* `nodeName`: Node name
* `executionId`: Execution identifier

**Returns:** Tracking token for completion

#### CompleteNodeTracking

```csharp
public void CompleteNodeTracking(NodeExecutionTracker tracker, bool success, object? result = null, Exception? exception = null)
```

Completes node execution tracking and records metrics.

**Parameters:**
* `tracker`: Node execution tracker
* `success`: Whether execution was successful
* `result`: Execution result (optional)
* `exception`: Exception if failed (optional)

#### GetNodeMetrics

```csharp
public NodeExecutionMetrics? GetNodeMetrics(string nodeId)
```

Retrieves metrics for a specific node.

**Parameters:**
* `nodeId`: Node identifier

**Returns:** Node metrics or null if not found

#### GetPerformanceSummary

```csharp
public GraphPerformanceSummary GetPerformanceSummary()
```

Generates a comprehensive performance summary with aggregated statistics.

**Returns:** Performance summary with key metrics

### Configuration

```csharp
var options = new GraphMetricsOptions
{
    EnableResourceMonitoring = true,
    ResourceSamplingInterval = TimeSpan.FromSeconds(5),
    MaxSampleHistory = 10000,
    EnableDetailedPathTracking = true,
    EnablePercentileCalculations = true,
    MetricsRetentionPeriod = TimeSpan.FromHours(24)
};

var metrics = new GraphPerformanceMetrics(options);
```

## NodeExecutionMetrics

Tracks execution metrics for a specific graph node. Provides detailed statistics about node performance and behavior.

### Properties

* `NodeId`: Node identifier
* `NodeName`: Node name
* `TotalExecutions`: Total number of executions
* `SuccessfulExecutions`: Number of successful executions
* `FailedExecutions`: Number of failed executions
* `SuccessRate`: Success rate as a percentage (0-100)
* `AverageExecutionTime`: Average execution duration
* `MinExecutionTime`: Minimum execution duration
* `MaxExecutionTime`: Maximum execution duration
* `FirstExecution`: Timestamp of first execution
* `LastExecution`: Timestamp of last execution

### Methods

#### RecordExecution

```csharp
public void RecordExecution(TimeSpan duration, bool success, object? result = null, Exception? exception = null)
```

Records a single execution with its outcome and timing.

**Parameters:**
* `duration`: Execution duration
* `success`: Whether execution succeeded
* `result`: Execution result (optional)
* `exception`: Exception if failed (optional)

#### GetPercentiles

```csharp
public Dictionary<string, TimeSpan> GetPercentiles(params int[] percentiles)
```

Calculates execution time percentiles (P50, P95, P99, etc.).

**Parameters:**
* `percentiles`: Array of percentile values (0-100)

**Returns:** Dictionary mapping percentile to execution time

## OpenTelemetry Meter Integration

SemanticKernel.Graph integrates with OpenTelemetry's `Meter` for standardized metrics collection and export.

### Meter Configuration

```csharp
// Default meter names used by the framework
var streamingMeter = new Meter("SemanticKernel.Graph.Streaming", "1.0.0");
var distributionMeter = new Meter("SemanticKernel.Graph.Distribution", "1.0.0");
var agentPoolMeter = new Meter("skg.agent_pool", "1.0.0");
```

### Metric Instruments

#### Counters

```csharp
// Event counters with tags
var eventsCounter = meter.CreateCounter<long>("skg.stream.events", unit: "count", 
    description: "Total events emitted by the stream");

// Usage
eventsCounter.Add(1, new KeyValuePair<string, object?>("event_type", "NodeStarted"),
    new KeyValuePair<string, object?>("executionId", executionId),
    new KeyValuePair<string, object?>("graph", graphId),
    new KeyValuePair<string, object?>("node", nodeId));
```

#### Histograms

```csharp
// Latency histograms
var eventLatencyMs = meter.CreateHistogram<double>("skg.stream.event.latency_ms", 
    unit: "ms", description: "Latency per event");

// Payload size histograms
var serializedPayloadBytes = meter.CreateHistogram<long>("skg.stream.event.payload_bytes", 
    unit: "bytes", description: "Serialized payload size per event");

// Usage
eventLatencyMs.Record(elapsedMs, new KeyValuePair<string, object?>("event_type", "NodeCompleted"),
    new KeyValuePair<string, object?>("executionId", executionId),
    new KeyValuePair<string, object?>("graph", graphId),
    new KeyValuePair<string, object?>("node", nodeId));
```

### Standard Metric Tags

All metrics in SemanticKernel.Graph use consistent tagging for correlation and filtering:

#### Core Tags

* **`executionId`**: Unique identifier for each graph execution run
* **`graph`**: Stable identifier for the graph definition
* **`node`**: Stable identifier for the specific node
* **`event_type`**: Type of event or operation being measured

#### Additional Context Tags

* **`workflow.id`**: Multi-agent workflow identifier
* **`workflow.name`**: Human-readable workflow name
* **`agent.id`**: Agent identifier in multi-agent scenarios
* **`operation.type`**: Type of operation being performed
* **`compressed`**: Whether data compression was applied
* **`memory_mapped`**: Whether memory-mapped buffers were used

### Metric Naming Convention

Metrics follow a hierarchical naming pattern:

```
skg.{component}.{metric_name}
```

Examples:
* `skg.stream.events` - Event counter for streaming
* `skg.stream.event.latency_ms` - Event latency histogram
* `skg.stream.event.payload_bytes` - Event payload size histogram
* `skg.stream.producer.flush_ms` - Producer flush latency
* `skg.agent_pool.connections` - Agent pool connection counter
* `skg.work_distributor.tasks` - Work distribution task counter

## Streaming Metrics

### Event Stream Metrics

```csharp
var options = new StreamingExecutionOptions
{
    EnableMetrics = true,
    MetricsMeterName = "MyApp.GraphExecution"
};

var stream = executor.ExecuteStreamAsync(kernel, args, options);
```

**Available Metrics:**
* `skg.stream.events` - Total events emitted (counter)
* `skg.stream.event.latency_ms` - Event processing latency (histogram)
* `skg.stream.event.payload_bytes` - Serialized payload size (histogram)
* `skg.stream.producer.flush_ms` - Producer buffer flush latency (histogram)

### Connection Pool Metrics

```csharp
var poolOptions = new StreamingPoolOptions
{
    EnableMetrics = true,
    MetricsMeterName = "MyApp.StreamingPool"
};
```

**Available Metrics:**
* `skg.stream.pool.connections` - Active connections (counter)
* `skg.stream.pool.requests` - Request count (counter)
* `skg.stream.pool.latency_ms` - Request latency (histogram)

## Multi-Agent Metrics

### Agent Pool Metrics

```csharp
var agentOptions = new AgentConnectionPoolOptions
{
    EnableMetrics = true,
    MetricsMeterName = "skg.agent_pool"
};
```

**Available Metrics:**
* `skg.agent_pool.connections` - Active agent connections (counter)
* `skg.agent_pool.requests` - Request count (counter)
* `skg.agent_pool.latency_ms` - Request latency (histogram)

### Work Distribution Metrics

```csharp
var distributorOptions = new WorkDistributorOptions
{
    EnableMetrics = true,
    MetricsMeterName = "skg.work_distributor"
};
```

**Available Metrics:**
* `skg.work_distributor.tasks` - Task count (counter)
* `skg.work_distributor.latency_ms` - Task distribution latency (histogram)
* `skg.work_distributor.queue_size` - Queue size (gauge)

## Performance Monitoring

### Resource Monitoring

```csharp
var metricsOptions = new GraphMetricsOptions
{
    EnableResourceMonitoring = true,
    ResourceSamplingInterval = TimeSpan.FromSeconds(5)
};

var metrics = new GraphPerformanceMetrics(metricsOptions);
```

**Monitored Resources:**
* CPU usage percentage
* Available memory (MB)
* Process processor time
* System load indicators

### Execution Path Analysis

```csharp
var pathMetrics = metrics.GetPathMetrics("path_signature");
if (pathMetrics != null)
{
    var avgTime = pathMetrics.AverageExecutionTime;
    var successRate = pathMetrics.SuccessRate;
    var executionCount = pathMetrics.ExecutionCount;
}
```

**Path Metrics:**
* Execution count per path
* Success/failure rates
* Average execution times
* Path-specific performance trends

## Metrics Export and Visualization

### Export Formats

```csharp
var exporter = new GraphMetricsExporter();
var jsonMetrics = exporter.ExportToJson(metrics);
var prometheusMetrics = exporter.ExportToPrometheus(metrics);
```

### Dashboard Integration

```csharp
var dashboard = new MetricsDashboard();
dashboard.RegisterExecution(executionContext, metrics);

var heatmap = dashboard.GeneratePerformanceHeatmap(metrics, visualizationData);
var summary = dashboard.ExportMetricsForVisualization(metrics, MetricsExportFormat.Json);
```

## Configuration Examples

### Development Environment

```csharp
var devOptions = GraphMetricsOptions.CreateDevelopmentOptions();
// High-frequency sampling, detailed tracking, short retention
```

### Production Environment

```csharp
var prodOptions = GraphMetricsOptions.CreateProductionOptions();
// Balanced sampling, comprehensive tracking, extended retention
```

### Performance-Critical Scenarios

```csharp
var minimalOptions = GraphMetricsOptions.CreateMinimalOptions();
// Minimal overhead, basic metrics, short retention
```

## Best Practices

### Metric Tagging

1. **Consistent Tags**: Always use the standard tag set (`executionId`, `graph`, `node`)
2. **Cardinality Management**: Avoid high-cardinality tags that could explode metric series
3. **Semantic Naming**: Use descriptive tag values that aid in debugging and analysis

### Performance Considerations

1. **Sampling**: Use appropriate sampling intervals for resource monitoring
2. **Retention**: Balance historical data needs with memory usage
3. **Export Frequency**: Configure export intervals based on monitoring requirements

### Integration

1. **OpenTelemetry**: Leverage the built-in OpenTelemetry integration for standard observability
2. **Custom Metrics**: Extend with application-specific metrics using the same tagging patterns
3. **Alerting**: Use metric thresholds for proactive monitoring and alerting

## See Also

* [Metrics and Observability Guide](../how-to/metrics-and-observability.md) - Comprehensive observability guide
* [Metrics Quickstart](../metrics-logging-quickstart.md) - Get started with metrics and logging
* [Streaming APIs Reference](./streaming.md) - Streaming execution with metrics
* [Multi-Agent Reference](./multi-agent.md) - Multi-agent metrics and monitoring
* [Graph Options Reference](./graph-options.md) - Metrics configuration options
