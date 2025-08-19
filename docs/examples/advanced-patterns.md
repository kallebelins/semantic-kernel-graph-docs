# Advanced Patterns Example

This example demonstrates the comprehensive integration of advanced architectural patterns in Semantic Kernel Graph, including academic patterns, machine learning optimizations, and enterprise integration capabilities.

## Objective

Learn how to implement and orchestrate advanced patterns in graph-based workflows to:
- Configure and use academic patterns (Circuit Breaker, Bulkhead, Cache-Aside)
- Enable machine learning-based performance prediction and anomaly detection
- Implement enterprise integration patterns for distributed systems
- Orchestrate multiple patterns in real-world scenarios
- Monitor and diagnose pattern performance comprehensively

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Execution Model](../concepts/execution-model.md)
- Familiarity with [Error Handling and Resilience](../how-to/error-handling-and-resilience.md)

## Key Components

### Concepts and Techniques

- **Academic Patterns**: Enterprise-grade resilience patterns including Circuit Breaker, Bulkhead, and Cache-Aside
- **Machine Learning Optimization**: Performance prediction and anomaly detection using historical execution data
- **Enterprise Integration**: Message routing, content-based routing, publish-subscribe, and aggregation patterns
- **Pattern Orchestration**: Coordinated execution of multiple patterns with comprehensive diagnostics

### Core Classes

- `GraphExecutor`: Enhanced executor with advanced pattern support via `WithAllAdvancedPatterns`
- `AcademicPatterns`: Circuit breaker, bulkhead, and cache-aside implementations
- `MachineLearningOptimizer`: Performance prediction and anomaly detection engine
- `EnterpriseIntegrationPatterns`: Message routing and processing patterns
- `GraphPerformanceMetrics`: Comprehensive performance tracking and analysis

## Running the Example

### Getting Started

This example demonstrates advanced patterns and optimizations with the Semantic Kernel Graph package. The code snippets below show you how to implement these patterns in your own applications.

## Step-by-Step Implementation

### 1. Creating Advanced Graph Executor

The example starts by creating a graph executor with all advanced patterns enabled.

```csharp
// Create executor base using constructor with Kernel
var executor = new GraphExecutor(kernel, graphLogger);

// Configure all advanced patterns
executor.WithAllAdvancedPatterns(config =>
{
    // Academic patterns
    config.EnableAcademicPatterns = true;
    config.Academic.EnableCircuitBreaker = true;
    config.Academic.EnableBulkhead = true;
    config.Academic.EnableCacheAside = true;

    // Circuit breaker configuration
    config.Academic.CircuitBreakerOptions.FailureThreshold = 3;
    config.Academic.CircuitBreakerOptions.OpenTimeout = TimeSpan.FromSeconds(10);

    // Bulkhead configuration
    config.Academic.BulkheadOptions.MaxConcurrency = 5;
    config.Academic.BulkheadOptions.AcquisitionTimeout = TimeSpan.FromSeconds(15);

    // Cache-aside configuration
    config.Academic.CacheAsideOptions.MaxCacheSize = 1000;
    config.Academic.CacheAsideOptions.DefaultTtl = TimeSpan.FromMinutes(10);

    // Advanced optimizations
    config.EnableAdvancedOptimizations = true;
    config.OptimizationOptions.OptimizationInterval = TimeSpan.FromMinutes(5);
    config.OptimizationOptions.HotPathThreshold = 50;
    config.OptimizationOptions.HighLatencyThreshold = TimeSpan.FromSeconds(1);

    // Machine learning
    config.EnableMachineLearning = true;
    config.MLOptions.RetrainingInterval = TimeSpan.FromHours(2);
    config.MLOptions.EnableIncrementalLearning = true;
    config.MLOptions.AnomalyThreshold = 2.0;

    // Enterprise integration
    config.EnableEnterpriseIntegration = true;
    config.IntegrationOptions.AggregationOptions.MinMessagesForAggregation = 2;
    config.IntegrationOptions.AggregationOptions.AggregationTimeout = TimeSpan.FromSeconds(30);
});
```

### 2. Academic Patterns Demonstration

#### Circuit Breaker Pattern

```csharp
var circuitBreakerTest = await executor.ExecuteWithCircuitBreakerAsync(
    operation: async () =>
    {
        await Task.Delay(100); // Simulates operation
        Console.WriteLine("  ✅ Operation executed successfully");
        return "Success";
    },
    fallback: async () =>
    {
        Console.WriteLine("  🔄 Fallback operation executed");
        return "Fallback";
    });
```

#### Bulkhead Pattern

```csharp
var bulkheadTasks = Enumerable.Range(1, 3).Select(async i =>
{
    var result = await executor.ExecuteWithBulkheadAsync(
        operation: async (cancellationToken) =>
        {
            await Task.Delay(200, cancellationToken);
            Console.WriteLine($"  ✅ Bulkhead operation {i} completed");
            return $"Result-{i}";
        });
    return result;
});

var bulkheadResults = await Task.WhenAll(bulkheadTasks);
```

#### Cache-Aside Pattern

```csharp
var cacheResult1 = await executor.GetOrSetCacheAsync(
    key: "user_profile_123",
    loader: async () =>
    {
        await Task.Delay(500); // Simulates database lookup
        Console.WriteLine("  🔍 Loading from database (cache miss)");
        return new { UserId = 123, Name = "John Doe", Email = "john@example.com" };
    });

var cacheResult2 = await executor.GetOrSetCacheAsync(
    key: "user_profile_123",
    loader: async () =>
    {
        Console.WriteLine("  ⚠️ This should not be called (cache hit expected)");
        return new { UserId = 123, Name = "John Doe", Email = "john@example.com" };
    });
```

### 3. Advanced Optimizations

The example demonstrates performance optimization based on historical metrics.

```csharp
// Create metrics and simulate executions
var metrics = new GraphPerformanceMetrics(new GraphMetricsOptions(), graphLogger);

// Simulate executions to generate data
for (int i = 0; i < 10; i++)
{
    var tracker = metrics.StartNodeTracking($"node_{i % 3}", $"TestNode{i % 3}", $"exec_{i}");
    await Task.Delay(Random.Shared.Next(50, 200)); // Simulates variable latency
    metrics.CompleteNodeTracking(tracker, success: Random.Shared.Next(100) < 95);
}

// Record execution paths
var paths = new[]
{
    new[] { "node_0", "node_1", "node_2" },
    new[] { "node_0", "node_2" },
    new[] { "node_1", "node_2" }
};

foreach (var path in paths)
{
    metrics.RecordExecutionPath($"path_{Array.IndexOf(paths, path)}",
        path, TimeSpan.FromMilliseconds(Random.Shared.Next(100, 500)), true);
}

// Run optimization analysis
var optimizationResult = await executor.OptimizeAsync(metrics);
Console.WriteLine($"  📊 Analysis completed in {optimizationResult.AnalysisTime.TotalMilliseconds:F2}ms");
Console.WriteLine($"  🎯 Total optimizations identified: {optimizationResult.TotalOptimizations}");
```

### 4. Machine Learning Optimization

#### Performance Prediction

```csharp
var graphConfig = new GraphConfiguration
{
    NodeCount = 8,
    AveragePathLength = 3.5,
    ConditionalNodeCount = 2,
    LoopNodeCount = 1,
    ParallelNodeCount = 2
};

var prediction = await executor.PredictPerformanceAsync(graphConfig);
Console.WriteLine($"  📈 Predicted latency: {prediction.PredictedLatency.TotalMilliseconds:F2}ms");
Console.WriteLine($"  🎯 Confidence: {prediction.Confidence:P2}");
Console.WriteLine($"  💡 Recommendations: {prediction.RecommendedOptimizations.Count}");
```

#### Anomaly Detection

```csharp
var executionMetrics = new GraphExecutionMetrics
{
    TotalExecutionTime = TimeSpan.FromMilliseconds(5000), // High anomalous latency
    CpuUsage = 85.0,
    MemoryUsage = 75.0,
    ErrorRate = 2.0,
    ThroughputPerSecond = 10.0
};

var anomalyResult = await executor.DetectAnomaliesAsync(executionMetrics);
Console.WriteLine($"  🎭 Is anomaly: {anomalyResult.IsAnomaly}");
Console.WriteLine($"  📊 Anomaly score: {anomalyResult.AnomalyScore:F2}");
Console.WriteLine($"  🔍 Confidence: {anomalyResult.Confidence:P2}");
```

### 5. Enterprise Integration Patterns

#### Message Routing

```csharp
var messageRoute = new IntegrationRoute
{
    Type = IntegrationRouteType.Message,
    Source = "orders",
    Destination = "fulfillment",
    Conditions = new Dictionary<string, object>
    {
        ["MessageType"] = "OrderCreated",
        ["Priority"] = MessagePriority.High
    }
};

var routeId = await executor.ConfigureIntegrationRouteAsync(messageRoute);
```

#### Processing Different Patterns

```csharp
var testMessages = new[]
{
    new EnterpriseMessage
    {
        MessageType = "OrderCreated",
        Priority = MessagePriority.High,
        Payload = new { OrderId = "ORD-001", CustomerId = "CUST-123", Amount = 299.99 },
        Routing = new RoutingProperties { RoutingKey = "orders", Topic = "order-events" }
    }
};

foreach (var message in testMessages)
{
    // Message Router
    var routerContext = new ProcessingContext
    {
        ProcessingPattern = IntegrationPattern.MessageRouter,
        RoutingKey = message.Routing.RoutingKey,
        ProcessingTimeout = TimeSpan.FromSeconds(30)
    };

    var routerResult = await executor.ProcessEnterpriseMessageAsync(message, routerContext);
    
    // Content-Based Router
    var contentContext = new ProcessingContext
    {
        ProcessingPattern = IntegrationPattern.ContentBasedRouter,
        ProcessingTimeout = TimeSpan.FromSeconds(30)
    };

    var contentResult = await executor.ProcessEnterpriseMessageAsync(message, contentContext);
    
    // Publish-Subscribe
    var pubSubContext = new ProcessingContext
    {
        ProcessingPattern = IntegrationPattern.PublishSubscribe,
        Topic = message.Routing.Topic,
        ProcessingTimeout = TimeSpan.FromSeconds(30)
    };

    var pubSubResult = await executor.ProcessEnterpriseMessageAsync(message, pubSubContext);
}
```

### 6. Comprehensive Diagnostics

The example concludes with comprehensive diagnostics of all patterns.

```csharp
var diagnosticReport = await executor.RunComprehensiveDiagnosticsAsync(metrics);

Console.WriteLine($"\n📋 Diagnostic Report (Generated at {diagnosticReport.Timestamp:HH:mm:ss})");
Console.WriteLine("=" + new string('=', 50));

Console.WriteLine($"✅ Success: {diagnosticReport.Success}");
Console.WriteLine($"🆔 Executor ID: {diagnosticReport.GraphExecutorId}");

if (diagnosticReport.AcademicPatternsStatus != null)
{
    Console.WriteLine("\n🎓 Academic Patterns Status:");
    var status = diagnosticReport.AcademicPatternsStatus;
    Console.WriteLine($"  🔌 Circuit Breaker: {(status.CircuitBreakerConfigured ? "✅" : "❌")}");
    Console.WriteLine($"  🚧 Bulkhead: {(status.BulkheadConfigured ? "✅" : "❌")}");
    Console.WriteLine($"  💾 Cache-Aside: {(status.CacheAsideConfigured ? "✅" : "❌")}");
}

if (diagnosticReport.OptimizationAnalysis != null)
{
    var opt = diagnosticReport.OptimizationAnalysis;
    Console.WriteLine("\n⚡ Optimization Analysis:");
    Console.WriteLine($"  ⏱️ Analysis Time: {opt.AnalysisTime.TotalMilliseconds:F2}ms");
    Console.WriteLine($"  🎯 Total Optimizations: {opt.TotalOptimizations}");
    Console.WriteLine($"  📈 Path Optimizations: {opt.PathOptimizations.Count}");
    Console.WriteLine($"  🔧 Node Optimizations: {opt.NodeOptimizations.Count}");
}
```

## Expected Output

The example produces comprehensive output showing:

- ✅ Advanced Graph Executor creation with all patterns enabled
- 🎓 Academic patterns demonstration (Circuit Breaker, Bulkhead, Cache-Aside)
- ⚡ Advanced optimizations analysis with performance recommendations
- 🤖 Machine learning training and performance prediction
- 🏢 Enterprise integration patterns (Message Router, Content Router, Pub-Sub)
- 🔍 Comprehensive diagnostics report for all patterns

## Troubleshooting

### Common Issues

1. **Pattern Configuration Errors**: Ensure all pattern options are properly configured before calling `WithAllAdvancedPatterns`
2. **ML Training Failures**: Check that sufficient historical data is available for training
3. **Integration Route Errors**: Verify message routing conditions and destination configurations
4. **Performance Issues**: Monitor optimization analysis timing and adjust thresholds as needed

### Debugging Tips

- Enable detailed logging to trace pattern execution
- Use the comprehensive diagnostics to identify configuration issues
- Monitor circuit breaker states and bulkhead concurrency limits
- Check cache hit rates and ML model training status

## See Also

- [Error Handling and Resilience](../how-to/error-handling-and-resilience.md)
- [Resource Governance and Concurrency](../how-to/resource-governance-and-concurrency.md)
- [Metrics and Observability](../how-to/metrics-and-observability.md)
- [Integration and Extensions](../how-to/integration-and-extensions.md)
- [Advanced Routing](../how-to/advanced-routing.md)
