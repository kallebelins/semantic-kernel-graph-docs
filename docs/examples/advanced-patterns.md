# Advanced Patterns Example

This example demonstrates the comprehensive integration of advanced architectural patterns in Semantic Kernel Graph, including academic patterns, machine learning optimizations, and enterprise integration capabilities.

## Objective

Learn how to implement and orchestrate advanced patterns in graph-based workflows to:
* Configure and use academic patterns (Circuit Breaker, Bulkhead, Cache-Aside)
* Enable machine learning-based performance prediction and anomaly detection
* Implement enterprise integration patterns for distributed systems
* Orchestrate multiple patterns in real-world scenarios
* Monitor and diagnose pattern performance comprehensively

## Prerequisites

* **.NET 8.0** or later
* **OpenAI API Key** configured in `appsettings.json`
* **Semantic Kernel Graph package** installed
* Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Execution Model](../concepts/execution-model.md)
* Familiarity with [Error Handling and Resilience](../how-to/error-handling-and-resilience.md)

## Key Components

### Concepts and Techniques

* **Academic Patterns**: Enterprise-grade resilience patterns including Circuit Breaker, Bulkhead, and Cache-Aside
* **Machine Learning Optimization**: Performance prediction and anomaly detection using historical execution data
* **Enterprise Integration**: Message routing, content-based routing, publish-subscribe, and aggregation patterns
* **Pattern Orchestration**: Coordinated execution of multiple patterns with comprehensive diagnostics

### Core Classes

* `GraphExecutor`: Enhanced executor with advanced pattern support via `WithAllAdvancedPatterns`
* `AcademicPatterns`: Circuit breaker, bulkhead, and cache-aside implementations
* `MachineLearningOptimizer`: Performance prediction and anomaly detection engine
* `EnterpriseIntegrationPatterns`: Message routing and processing patterns
* `GraphPerformanceMetrics`: Comprehensive performance tracking and analysis

## Running the Example

### Getting Started

This example demonstrates advanced patterns and optimizations with the Semantic Kernel Graph package. The code snippets below show you how to implement these patterns in your own applications.

## Step-by-Step Implementation

### 1. Creating Advanced Graph Executor

The example starts by creating a graph executor with all advanced patterns enabled.

```csharp
// Create a GraphExecutor using the provided Kernel and a graph logger.
// This snippet configures a minimal, safe set of advanced patterns for demos.
var executor = new GraphExecutor(kernel, graphLogger);

// Enable the main academic resilience patterns with conservative defaults.
executor.WithAllAdvancedPatterns(config =>
{
    // Enable academic resilience patterns (circuit breaker, bulkhead, cache-aside).
    config.EnableAcademicPatterns = true;
    config.Academic.EnableCircuitBreaker = true;
    config.Academic.EnableBulkhead = true;
    config.Academic.EnableCacheAside = true;

    // Circuit breaker: trip after 3 failures and keep open briefly.
    config.Academic.CircuitBreakerOptions.FailureThreshold = 3;
    config.Academic.CircuitBreakerOptions.OpenTimeout = TimeSpan.FromSeconds(10);

    // Bulkhead: limit concurrency to avoid resource exhaustion in demos.
    config.Academic.BulkheadOptions.MaxConcurrency = 5;
    config.Academic.BulkheadOptions.AcquisitionTimeout = TimeSpan.FromSeconds(15);

    // Cache-aside: small in-memory cache for demo purposes.
    config.Academic.CacheAsideOptions.MaxCacheSize = 1000;
    config.Academic.CacheAsideOptions.DefaultTtl = TimeSpan.FromMinutes(10);
});
```

### 2. Academic Patterns Demonstration

#### Circuit Breaker Pattern

```csharp
// Execute a protected operation via the executor's circuit breaker helper.
// The operation should be an async delegate; a fallback is executed when the
// circuit is open or failures occur.
var circuitBreakerTest = await executor.ExecuteWithCircuitBreakerAsync(
    operation: async () =>
    {
        // Simulate some work
        await Task.Delay(100);
        Console.WriteLine("Operation executed successfully");
        return "Success";
    },
    fallback: async () =>
    {
        // Minimal fallback implementation for demos
        Console.WriteLine("Fallback operation executed");
        return "Fallback";
    });
```

#### Bulkhead Pattern

```csharp
// Run several operations in parallel using the bulkhead to protect resources.
var bulkheadTasks = Enumerable.Range(1, 3).Select(async i =>
{
    return await executor.ExecuteWithBulkheadAsync(async (cancellationToken) =>
    {
        await Task.Delay(200, cancellationToken);
        Console.WriteLine($"Bulkhead operation {i} completed");
        return $"Result-{i}";
    });
});

var bulkheadResults = await Task.WhenAll(bulkheadTasks);
```

#### Cache-Aside Pattern

```csharp
// Cache-aside pattern: loader is called on cache miss to populate the cache.
var cacheResult1 = await executor.GetOrSetCacheAsync(
    key: "user_profile_123",
    loader: async () =>
    {
        // Simulate slow data source (database)
        await Task.Delay(500);
        Console.WriteLine("Loading from database (cache miss)");
        return new { UserId = 123, Name = "John Doe", Email = "john@example.com" };
    });

// Second call should be a cache hit and not invoke the loader delegate.
var cacheResult2 = await executor.GetOrSetCacheAsync(
    key: "user_profile_123",
    loader: async () =>
    {
        // If this runs in your environment the cache did not work as expected.
        Console.WriteLine("Loader unexpectedly invoked on supposed cache hit");
        return new { UserId = 123, Name = "John Doe", Email = "john@example.com" };
    });
```

### 3. Advanced Optimizations

The example demonstrates performance optimization based on historical metrics.

```csharp
// Create a small metrics object and simulate executions to produce sample data.
var metrics = new GraphPerformanceMetrics(new GraphMetricsOptions(), graphLogger);

for (int i = 0; i < 5; i++)
{
    var tracker = metrics.StartNodeTracking($"node_{i % 2}", $"TestNode{i % 2}", $"exec_{i}");
    await Task.Delay(Random.Shared.Next(50, 150));
    metrics.CompleteNodeTracking(tracker, success: true);
}

// Run a lightweight optimization analysis using the simulated metrics.
var optimizationResult = await executor.OptimizeAsync(metrics);
Console.WriteLine($"Analysis completed in {optimizationResult.AnalysisTime.TotalMilliseconds:F2}ms");
Console.WriteLine($"Total optimizations identified: {optimizationResult.TotalOptimizations}");
```

### 4. Machine Learning Optimization

#### Performance Prediction

```csharp
// Prepare a small graph configuration for a prediction demo.
var graphConfig = new GraphConfiguration
{
    NodeCount = 8,
    AveragePathLength = 3.5,
    ConditionalNodeCount = 2,
    LoopNodeCount = 1,
    ParallelNodeCount = 2
};

// Request a performance prediction from the executor (requires ML enabled).
var prediction = await executor.PredictPerformanceAsync(graphConfig);
Console.WriteLine($"Predicted latency: {prediction.PredictedLatency.TotalMilliseconds:F2}ms");
Console.WriteLine($"Confidence: {prediction.Confidence:P2}");
Console.WriteLine($"Recommended optimizations: {prediction.RecommendedOptimizations.Count}");
```

#### Anomaly Detection

```csharp
// Example anomaly detection input (simulated metrics).
var executionMetrics = new GraphExecutionMetrics
{
    TotalExecutionTime = TimeSpan.FromMilliseconds(5000),
    CpuUsage = 85.0,
    MemoryUsage = 75.0,
    ErrorRate = 2.0,
    ThroughputPerSecond = 10.0
};

var anomalyResult = await executor.DetectAnomaliesAsync(executionMetrics);
Console.WriteLine($"Is anomaly: {anomalyResult.IsAnomaly}");
Console.WriteLine($"Anomaly score: {anomalyResult.AnomalyScore:F2}");
Console.WriteLine($"Confidence: {anomalyResult.Confidence:P2}");
```

### 5. Enterprise Integration Patterns

#### Message Routing

```csharp
// Define a simple message routing rule that forwards 'OrderCreated' messages.
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
Console.WriteLine($"Route configured with id: {routeId}");
```

#### Processing Different Patterns

```csharp
// Prepare a few sample enterprise messages for routing demonstration.
var testMessages = new[]
{
    new EnterpriseMessage
    {
        MessageType = "OrderCreated",
        Priority = MessagePriority.High,
        Payload = new { OrderId = "ORD-001", CustomerId = "CUST-123", Amount = 299.99 },
        Routing = new RoutingProperties { RoutingKey = "orders", Topic = "order-events" }
    },
    new EnterpriseMessage
    {
        MessageType = "PaymentProcessed",
        Priority = MessagePriority.Normal,
        Payload = new { PaymentId = "PAY-001", OrderId = "ORD-001", Status = "Completed" },
        Routing = new RoutingProperties { RoutingKey = "payments", Topic = "payment-events" }
    }
};

foreach (var message in testMessages)
{
    Console.WriteLine($"Processing message: {message.MessageType}");

    // Message Router
    var routerContext = new ProcessingContext
    {
        ProcessingPattern = IntegrationPattern.MessageRouter,
        RoutingKey = message.Routing.RoutingKey,
        ProcessingTimeout = TimeSpan.FromSeconds(30)
    };

    var routerResult = await executor.ProcessEnterpriseMessageAsync(message, routerContext);
    Console.WriteLine($"Message Router: {(routerResult.Success ? "OK" : "FAIL")} ({routerResult.ProcessingTime.TotalMilliseconds:F2}ms)");

    // Content-Based Router
    var contentContext = new ProcessingContext
    {
        ProcessingPattern = IntegrationPattern.ContentBasedRouter,
        ProcessingTimeout = TimeSpan.FromSeconds(30)
    };

    var contentResult = await executor.ProcessEnterpriseMessageAsync(message, contentContext);
    Console.WriteLine($"Content Router: {(contentResult.Success ? "OK" : "FAIL")} ({contentResult.ProcessingTime.TotalMilliseconds:F2}ms)");

    // Publish-Subscribe
    var pubSubContext = new ProcessingContext
    {
        ProcessingPattern = IntegrationPattern.PublishSubscribe,
        Topic = message.Routing.Topic,
        ProcessingTimeout = TimeSpan.FromSeconds(30)
    };

    var pubSubResult = await executor.ProcessEnterpriseMessageAsync(message, pubSubContext);
    Console.WriteLine($"Pub-Sub: {(pubSubResult.Success ? "OK" : "FAIL")} ({pubSubResult.ProcessingTime.TotalMilliseconds:F2}ms)");
}
```

### 6. Comprehensive Diagnostics

The example concludes with comprehensive diagnostics of all patterns.

```csharp
// Run the comprehensive diagnostics routine and print a compact report.
var diagnosticReport = await executor.RunComprehensiveDiagnosticsAsync(metrics);

Console.WriteLine($"\nDiagnostic Report (Generated at {diagnosticReport.Timestamp:HH:mm:ss})");
Console.WriteLine(new string('=', 60));

Console.WriteLine($"Success: {diagnosticReport.Success}");
Console.WriteLine($"Executor ID: {diagnosticReport.GraphExecutorId}");

if (diagnosticReport.AcademicPatternsStatus != null)
{
    var status = diagnosticReport.AcademicPatternsStatus;
    Console.WriteLine($"Circuit Breaker configured: {status.CircuitBreakerConfigured}");
    Console.WriteLine($"Bulkhead configured: {status.BulkheadConfigured}");
    Console.WriteLine($"Cache-Aside configured: {status.CacheAsideConfigured}");
}

if (diagnosticReport.OptimizationAnalysis != null)
{
    var opt = diagnosticReport.OptimizationAnalysis;
    Console.WriteLine($"Optimization analysis time: {opt.AnalysisTime.TotalMilliseconds:F2}ms");
    Console.WriteLine($"Total optimizations: {opt.TotalOptimizations}");
}
```

## Expected Output

The example produces comprehensive output showing:

* ✅ Advanced Graph Executor creation with all patterns enabled
* 🎓 Academic patterns demonstration (Circuit Breaker, Bulkhead, Cache-Aside)
* ⚡ Advanced optimizations analysis with performance recommendations
* 🤖 Machine learning training and performance prediction
* 🏢 Enterprise integration patterns (Message Router, Content Router, Pub-Sub)
* 🔍 Comprehensive diagnostics report for all patterns

## Troubleshooting

### Common Issues

1. **Pattern Configuration Errors**: Ensure all pattern options are properly configured before calling `WithAllAdvancedPatterns`
2. **ML Training Failures**: Check that sufficient historical data is available for training
3. **Integration Route Errors**: Verify message routing conditions and destination configurations
4. **Performance Issues**: Monitor optimization analysis timing and adjust thresholds as needed

### Debugging Tips

* Enable detailed logging to trace pattern execution
* Use the comprehensive diagnostics to identify configuration issues
* Monitor circuit breaker states and bulkhead concurrency limits
* Check cache hit rates and ML model training status

## See Also

* [Error Handling and Resilience](../how-to/error-handling-and-resilience.md)
* [Resource Governance and Concurrency](../how-to/resource-governance-and-concurrency.md)
* [Metrics and Observability](../how-to/metrics-and-observability.md)
* [Integration and Extensions](../how-to/integration-and-extensions.md)
* [Advanced Routing](../how-to/advanced-routing.md)
