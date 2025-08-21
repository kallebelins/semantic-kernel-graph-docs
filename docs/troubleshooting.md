---
title: Troubleshooting
---

# Troubleshooting

Guide for resolving common problems and diagnosing issues in SemanticKernel.Graph.

## Concepts and Techniques

**Troubleshooting**: Systematic process of identifying, diagnosing and resolving problems in computational graph systems.

**Diagnosis**: Analysis of symptoms, logs and metrics to determine the root cause of a problem.

**Recovery**: Strategies to restore normal functionality after problem resolution.

## Execution Problems

### Execution Pauses or is Slow

**Symptoms**:
* Graph doesn't progress after a specific node
* Execution time much longer than expected
* Application seems "frozen"

**Probable Causes**:
* Infinite or very long loops
* Nodes with very high timeout
* Blocking on external resources
* Routing conditions that are never met

**Diagnosis**:
```csharp
// Enable detailed metrics and monitoring
var executionOptions = GraphExecutionOptions.CreateDefault();

// Create a graph with performance monitoring
var graph = new GraphExecutor("performance-test-graph");

// Add nodes to the graph
var slowNode = new ActionGraphNode("slow-operation", "Slow Operation", "Simulates a slow operation");
var fastNode = new ActionGraphNode("fast-operation", "Fast Operation", "Simulates a fast operation");

graph.AddNode(slowNode);
graph.AddNode(fastNode);

// Set the start node for execution
graph.SetStartNode(slowNode);

// Execute with performance monitoring
var startTime = DateTimeOffset.UtcNow;
var arguments = new KernelArguments();
arguments["input"] = "test input";

var result = await graph.ExecuteAsync(kernel, arguments, CancellationToken.None);
var executionTime = DateTimeOffset.UtcNow - startTime;

Console.WriteLine($"Graph execution completed in {executionTime.TotalMilliseconds:F2}ms");
```

**Solution**:
```csharp
// Configure execution options with performance monitoring
var executionOptions = GraphExecutionOptions.CreateDefault();

// Set appropriate timeouts and limits
var graph = new GraphExecutor("optimized-graph");
graph.ConfigureMetrics(new GraphMetricsOptions
{
    EnableRealTimeMetrics = true,
    MetricsRetentionPeriod = TimeSpan.FromHours(24)
});

// Add nodes with proper configuration
var optimizedNode = new ActionGraphNode("optimized-operation", "Optimized Operation", "Fast operation with monitoring");
graph.AddNode(optimizedNode);
graph.SetStartNode(optimizedNode);
```

**Prevention**:
* Always set start nodes for graphs
* Configure appropriate timeouts
* Use metrics to monitor performance
* Implement circuit breakers for external resources

### Missing Service or Null Provider

**Symptoms**:
* `NullReferenceException` when executing graphs
* "Service not registered" error or similar
* Specific functionalities don't work

**Probable Causes**:
* `AddGraphSupport()` was not called
* Dependencies not registered in DI container
* Incorrect order of service registration

**Diagnosis**:
```csharp
// Check if graph support is properly configured
var serviceProvider = kernel.Services;
var graphExecutorFactory = serviceProvider.GetService<IGraphExecutorFactory>();

if (graphExecutorFactory == null)
{
    Console.WriteLine("Graph support not enabled! This will cause errors.");
    
    // Demonstrate the correct way to configure services
    Console.WriteLine("Correct configuration should include:");
    Console.WriteLine("builder.AddGraphSupport(options => {");
    Console.WriteLine("    options.EnableMetrics = true;");
    Console.WriteLine("    options.EnableCheckpointing = true;");
    Console.WriteLine("});");
}
else
{
    Console.WriteLine("Graph support is properly configured");
}

// Check for other essential services
var checkpointManager = serviceProvider.GetService<ICheckpointManager>();
var errorRecoveryEngine = serviceProvider.GetService<ErrorRecoveryEngine>();
var metricsExporter = serviceProvider.GetService<GraphMetricsExporter>();

Console.WriteLine("Service availability check:");
Console.WriteLine($"- GraphExecutorFactory: {(graphExecutorFactory != null ? "Available" : "Missing")}");
Console.WriteLine($"- CheckpointManager: {(checkpointManager != null ? "Available" : "Missing")}");
Console.WriteLine($"- ErrorRecoveryEngine: {(errorRecoveryEngine != null ? "Available" : "Missing")}");
Console.WriteLine($"- MetricsExporter: {(metricsExporter != null ? "Available" : "Missing")}");
```

**Solution**:
```csharp
// Correct configuration
var builder = Kernel.CreateBuilder();

// Add graph support BEFORE other services
builder.AddGraphSupport(options => {
    options.EnableMetrics = true;
    options.EnableCheckpointing = true;
    options.EnableLogging = true;
    options.MaxExecutionSteps = 1000;
    options.ExecutionTimeout = TimeSpan.FromMinutes(10);
});

// Add other services
builder.AddOpenAIChatCompletion("gpt-4", "your-api-key");

var kernel = builder.Build();
```

**Prevention**:
* Always call `AddGraphSupport()` before adding other services
* Verify service registration order
* Test service availability during startup
* Use dependency injection properly

### Failed in REST Tools

**Symptoms**:
* HTTP call timeouts
* Authentication failures
* Unexpected API responses

**Probable Causes**:
* Incorrect validation schemas
* Very low timeouts
* Authentication issues
* External APIs unavailable

**Diagnosis**:
```csharp
// Check service availability
var serviceProvider = kernel.Services;
var restApiService = serviceProvider.GetService<GraphRestApi>();

if (restApiService == null)
{
    Console.WriteLine("REST API service not available");
}
else
{
    Console.WriteLine("REST API service is properly configured");
}

// Check logging configuration
var logger = serviceProvider.GetService<ILogger<GraphExecutor>>();
if (logger != null)
{
    logger.LogInformation("Graph execution logging is properly configured");
}
```

**Solution**:
```csharp
// Configure REST API with proper settings
builder.AddGraphSupport(options => {
    options.EnableLogging = true;
    options.Logging.ConfigureForProduction();
});

// Configure HTTP client with appropriate timeouts
builder.Services.AddHttpClient("GraphRestApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "SemanticKernel.Graph/1.0");
});
```

**Prevention**:
* Test external APIs before using
* Implement circuit breakers
* Configure realistic timeouts
* Validate input/output schemas

## State and Checkpoint Problems

### Checkpoint Not Restored

**Symptoms**:
* Lost state between executions
* Error restoring checkpoint
* Inconsistent data after recovery

**Probable Causes**:
* Checkpointing extensions not configured
* Database collection does not exist
* Version incompatibility of state
* Serialization issues

**Diagnosis**:
```csharp
// Test checkpointing functionality
var serviceProvider = kernel.Services;
var checkpointManager = serviceProvider.GetService<ICheckpointManager>();

if (checkpointManager != null)
{
    // Test checkpoint creation
    var testState = new GraphState();
    testState.SetValue("test_key", "test_value");
    testState.SetValue("test_number", 42);

    var checkpoint = await checkpointManager.CreateCheckpointAsync(
        "test-execution", 
        testState, 
        "test-node", 
        null, 
        CancellationToken.None);

    Console.WriteLine($"Checkpoint created successfully: {checkpoint.CheckpointId}");

    // Test checkpoint restoration
    var restoredState = await checkpointManager.RestoreFromCheckpointAsync(
        checkpoint.CheckpointId, 
        CancellationToken.None);

    if (restoredState != null)
    {
        var restoredValue = restoredState.GetValue<string>("test_key");
        Console.WriteLine($"Checkpoint restored successfully. Value: {restoredValue}");
    }
    else
    {
        Console.WriteLine("Failed to restore checkpoint");
    }
}
else
{
    Console.WriteLine("Checkpointing service not available");
}
```

**Solution**:
```csharp
// Configure checkpointing correctly
builder.AddGraphSupport(options => {
    options.EnableCheckpointing = true;
    options.Checkpointing = new CheckpointingOptions
    {
        Enabled = true,
        Provider = "MongoDB", // or other provider
        ConnectionString = "mongodb://localhost:27017",
        DatabaseName = "semantic-kernel-graph",
        CollectionName = "checkpoints"
    };
});
```

**Prevention**:
* Always test database connectivity
* Implement version state validation
* Use robust serialization
* Monitor disk space

### Serialization Problems

**Symptoms**:
* "Cannot serialize type X" error
* Corrupted checkpoints
* Failed to save state

**Probable Causes**:
* Non-serializable types
* Circular references
* Complex types not supported

**Diagnosis**:
```csharp
// Test state serialization
var state = new GraphState();
try
{
    // Test with simple types
    state.SetValue("string_value", "test");
    state.SetValue("int_value", 123);
    state.SetValue("array_value", new[] { 1, 2, 3 });

    // Test serialization using the ISerializableState interface
    var serialized = state.Serialize();
    Console.WriteLine($"State serialization successful. Size: {serialized.Length} bytes");

    // Test with complex types (this might fail)
    try
    {
        state.SetValue("complex_object", new NonSerializableType());
        var complexSerialized = state.Serialize();
        Console.WriteLine("Complex object serialization successful");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Complex object serialization failed (expected): {ex.Message}");
        Console.WriteLine("Solution: Use simple types or implement ISerializableState");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"State serialization failed: {ex.Message}");
}
```

**Solution**:
```csharp
// Implement ISerializableState for complex types
public class MyState : ISerializableState
{
    public string Serialize() => JsonSerializer.Serialize(this);
    public static MyState Deserialize(string data) => JsonSerializer.Deserialize<MyState>(data);
}

// Or use simple types
state.SetValue("simple", "string value");
state.SetValue("number", 42);
state.SetValue("array", new[] { 1, 2, 3 });
```

**Prevention**:
* Use primitive types when possible
* Implement `ISerializableState` for complex types
* Avoid circular references
* Test serialization during development

## Python Node Problems

### Python Execution Errors

**Symptoms**:
* "python not found" error
* Python execution timeouts
* Communication failures between .NET and Python

**Probable Causes**:
* Python is not in PATH
* Incorrect Python version
* Permission issues
* Missing Python dependencies

**Diagnosis**:
```csharp
// Check if Python is available
var pythonNode = new PythonGraphNode("python");
var isAvailable = await pythonNode.CheckAvailabilityAsync();
Console.WriteLine($"Python available: {isAvailable}");
```

**Solution**:
```csharp
// Explicitly configure Python
var pythonOptions = new PythonNodeOptions
{
    PythonPath = @"C:\Python39\python.exe", // Explicit path
    EnvironmentVariables = new Dictionary<string, string>
    {
        ["PYTHONPATH"] = @"C:\my-python-libs",
        ["PYTHONUNBUFFERED"] = "1"
    },
    Timeout = TimeSpan.FromMinutes(5)
};

var pythonNode = new PythonGraphNode("python", pythonOptions);
```

**Prevention**:
* Use absolute paths for Python
* Verify Python dependencies
* Configure environment variables
* Implement fallbacks for Python nodes

## Performance Problems

### Very Slow Execution

**Symptoms**:
* Execution time much longer than expected
* Excessive CPU/memory usage
* Simple graphs take a long time

**Probable Causes**:
* Inefficient nodes
* Lack of parallelism
* Unnecessary blockages
* Suboptimal configurations

**Diagnosis**:
```csharp
// Analyze performance metrics
var serviceProvider = kernel.Services;
var metricsExporter = serviceProvider.GetService<GraphMetricsExporter>();

if (metricsExporter != null)
{
    // Create sample performance metrics for demonstration
    var performanceMetrics = new GraphPerformanceMetrics();
    
    // Export metrics in different formats
    var jsonMetrics = metricsExporter.ExportMetrics(performanceMetrics, MetricsExportFormat.Json);
    Console.WriteLine("Current metrics exported successfully in JSON format");

    // Export for dashboard visualization
    var dashboardMetrics = metricsExporter.ExportForDashboard(performanceMetrics, DashboardType.Grafana);
    Console.WriteLine("Dashboard metrics exported successfully for Grafana");

    // Check for performance anomalies
    if (jsonMetrics.Contains("error") || jsonMetrics.Contains("failure"))
    {
        Console.WriteLine("Performance issues detected in metrics");
        Console.WriteLine("Consider implementing circuit breakers or fallbacks");
    }
}
else
{
    Console.WriteLine("Metrics exporter not available");
}
```

**Solution**:
```csharp
// Enable parallel execution and optimizations
var options = new GraphOptions
{
    EnableMetrics = true,
    EnableLogging = true,
    MaxExecutionSteps = 1000,
    EnablePlanCompilation = true
};

// Configure concurrency
var concurrencyOptions = new GraphConcurrencyOptions
{
    MaxParallelNodes = Environment.ProcessorCount,
    EnableOptimizations = true
};

// Use optimized nodes
var optimizedNode = new ActionGraphNode("optimized-operation", "Optimized Operation", "Fast operation with monitoring");
```

**Prevention**:
* Monitor metrics regularly
* Use profiling to identify bottlenecks
* Implement caching when appropriate
* Optimize critical nodes

## Integration Problems

### Authentication Failures

**Symptoms**:
* 401/403 errors on external APIs
* LLM authentication failures
* Authorization issues

**Probable Causes**:
* Invalid API keys
* Expired tokens
* Incorrect credential configuration
* Permission issues

**Diagnosis**:
```csharp
// Check authentication configuration
var serviceProvider = kernel.Services;
var authService = serviceProvider.GetService<IAuthenticationService>();

if (authService != null)
{
    var isValid = await authService.ValidateCredentialsAsync();
    Console.WriteLine($"Authentication service available: {isValid}");
}
else
{
    Console.WriteLine("Authentication service not available");
}
```

**Solution**:
```csharp
// Correctly configure authentication
builder.AddOpenAIChatCompletion(
    modelId: "gpt-4",
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")
);

// Or use Azure AD
builder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4",
    endpoint: "https://your-endpoint.openai.azure.com/",
    apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")
);
```

**Prevention**:
* Use environment variables for credentials
* Implement automatic token rotation
* Monitor credential expiration
* Use secret managers

## Recovery Strategies

### Automatic Recovery
```csharp
// Configure retry policies
var retryPolicy = new ExponentialBackoffRetryPolicy(
    maxRetries: 3,
    initialDelay: TimeSpan.FromSeconds(1)
);

// Implement circuit breaker
var circuitBreaker = new CircuitBreaker(
    failureThreshold: 5,
    recoveryTimeout: TimeSpan.FromMinutes(1)
);
```

### Fallbacks and Alternatives
```csharp
// Implement fallback nodes
var errorHandlerNode = new ErrorHandlerGraphNode("error-handler", "Error Handler", "Handles errors during execution");
var fallbackNode = new ActionGraphNode("fallback", "Fallback Operation", "Fallback operation executed due to error");

// Configure error handling
errorHandlerNode.ConfigureErrorHandler(GraphErrorType.Validation, ErrorRecoveryAction.Skip);
errorHandlerNode.ConfigureErrorHandler(GraphErrorType.Network, ErrorRecoveryAction.Retry);
errorHandlerNode.AddFallbackNode(GraphErrorType.Unknown, fallbackNode);
```

## Monitoring and Alerts

### Alert Configuration
```csharp
// Configure alerts for critical issues
var alertingService = new GraphAlertingService();
alertingService.AddAlert(new AlertRule
{
    Condition = metrics => metrics.ErrorRate > 0.1,
    Severity = AlertSeverity.Critical,
    Message = "Error rate exceeded threshold"
});
```

### Structured Logging
```csharp
// Configure detailed logging
var logger = new SemanticKernelGraphLogger();
logger.LogExecutionStart(graphId, executionId);
logger.LogNodeExecution(nodeId, executionId, duration);
logger.LogExecutionComplete(graphId, executionId, result);
```

## Complete Working Example

Here's a complete working example that demonstrates troubleshooting techniques:

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Integration;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;

public class TroubleshootingExample
{
    private readonly Kernel _kernel;
    private readonly ILogger<TroubleshootingExample> _logger;

    public TroubleshootingExample(Kernel kernel, ILogger<TroubleshootingExample> logger)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting Troubleshooting Examples");

        try
        {
            // Example 1: Execution Performance Issues
            await DemonstrateExecutionPerformanceTroubleshootingAsync();

            // Example 2: Service Registration Issues
            await DemonstrateServiceRegistrationTroubleshootingAsync();

            // Example 3: State and Checkpoint Problems
            await DemonstrateStateCheckpointTroubleshootingAsync();

            // Example 4: Error Recovery and Resilience
            await DemonstrateErrorRecoveryTroubleshootingAsync();

            // Example 5: Performance Monitoring and Diagnostics
            await DemonstratePerformanceMonitoringTroubleshootingAsync();

            _logger.LogInformation("All troubleshooting examples completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running troubleshooting examples");
            throw;
        }
    }

    private async Task DemonstrateExecutionPerformanceTroubleshootingAsync()
    {
        _logger.LogInformation("=== Execution Performance Troubleshooting ===");

        try
        {
            // Create a graph with potential performance issues
            var graph = new GraphExecutor("performance-test-graph");
            
            // Add nodes to the graph
            var slowNode = new ActionGraphNode("slow-operation", "Slow Operation", "Simulates a slow operation");
            var fastNode = new ActionGraphNode("fast-operation", "Fast Operation", "Simulates a fast operation");
            
            graph.AddNode(slowNode);
            graph.AddNode(fastNode);

            // Set the start node for execution
            graph.SetStartNode(slowNode);

            // Execute with performance monitoring
            var startTime = DateTimeOffset.UtcNow;
            
            // Create arguments for execution
            var arguments = new KernelArguments();
            arguments["input"] = "test input";
            
            var result = await graph.ExecuteAsync(_kernel, arguments, CancellationToken.None);
            var executionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Graph execution completed in {ExecutionTime:F2}ms", executionTime.TotalMilliseconds);

            // Analyze performance metrics if available
            if (result.Metadata != null && result.Metadata.ContainsKey("ExecutionMetrics"))
            {
                _logger.LogInformation("Execution metrics available in result metadata");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in execution performance troubleshooting");
        }
    }

    // ... other methods as shown in the complete example above
}
```

## See Also

* [Error Handling](../how-to/error-handling-and-resilience.md)
* [Performance Tuning](../how-to/performance-tuning.md)
* [Monitoring](../how-to/metrics-and-observability.md)
* [Configuration](../how-to/configuration.md)
* [Examples](../examples/index.md)

## References

* `GraphExecutionOptions`: Execution settings
* `CheckpointingOptions`: Checkpointing settings
* `PythonNodeOptions`: Python node settings
* `RetryPolicy`: Retry policies
* `CircuitBreaker`: Circuit breakers for resilience
* `GraphAlertingService`: Alerting system


