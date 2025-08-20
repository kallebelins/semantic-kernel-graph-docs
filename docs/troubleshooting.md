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
// Enable detailed metrics
var options = new GraphExecutionOptions
{
    EnableMetrics = true,
    EnableLogging = true,
    MaxExecutionTime = TimeSpan.FromMinutes(5)
};

// Check execution logs
var logger = kernel.GetRequiredService<ILogger<GraphExecutor>>();
```

**Solution**:
```csharp
// Set iteration limits
var loopNode = new ReActLoopGraphNode(
    maxIterations: 10,  // Explicit limit
    timeout: TimeSpan.FromMinutes(2)
);

// Add timeouts to nodes
var nodeOptions = new GraphNodeOptions
{
    MaxExecutionTime = TimeSpan.FromSeconds(30)
};
```

**Prevention**:
* Always set `MaxIterations` for loop nodes
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
// Check if graph support was added
var graphExecutor = kernel.GetService<IGraphExecutor>();
if (graphExecutor == null)
{
    Console.WriteLine("Graph support not enabled!");
}
```

**Solution**:
```csharp
// Correct configuration
var builder = Kernel.CreateBuilder();

// Add graph support BEFORE other services
builder.AddGraphSupport(options => {
    options.EnableMetrics = true;
    options.EnableCheckpointing = true;
});

// Add other services
```

**Prevention**:
* Always set `MaxIterations` for loop nodes
* Configure appropriate timeouts
* Use metrics to monitor performance
* Implement circuit breakers for external resources

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
// Check telemetry of dependencies
var telemetry = kernel.GetRequiredService<ITelemetryService>();
var httpMetrics = telemetry.GetHttpMetrics();

// Check error logs
var logger = kernel.GetRequiredService<ILogger<RestToolGraphNode>>();
```

**Solution**:
```csharp
// Configure appropriate timeouts
var restToolOptions = new RestToolOptions
{
    Timeout = TimeSpan.FromSeconds(30),
    RetryPolicy = new ExponentialBackoffRetryPolicy(maxRetries: 3),
    CircuitBreaker = new CircuitBreakerOptions
    {
        FailureThreshold = 5,
        RecoveryTimeout = TimeSpan.FromMinutes(1)
    }
};

// Validate schemas
var schema = new RestToolSchema
{
    InputValidation = true,
    OutputValidation = true
};
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
// Check checkpointing configuration
var checkpointManager = kernel.GetService<ICheckpointManager>();
if (checkpointManager == null)
{
    Console.WriteLine("Checkpointing not enabled!");
}

// Check database connectivity
var connection = await checkpointManager.TestConnectionAsync();
```

**Solution**:
```csharp
// Configure checkpointing correctly
builder.AddGraphSupport(options => {
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
// Check if type is serializable
var state = new GraphState();
try
{
    state.SetValue("test", new NonSerializableType());
    var serialized = await state.SerializeAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Serialization error: {ex.Message}");
}
```

**Solution**:
```csharp
// Implement ISerializableState
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
var metrics = await executor.GetPerformanceMetricsAsync();
foreach (var nodeMetric in metrics.NodeMetrics)
{
    Console.WriteLine($"Node {nodeMetric.NodeId}: {nodeMetric.AverageExecutionTime}");
}
```

**Solution**:
```csharp
// Enable parallel execution
var options = new GraphExecutionOptions
{
    MaxParallelNodes = Environment.ProcessorCount,
    EnableOptimizations = true
};

// Use optimized nodes
var optimizedNode = new OptimizedFunctionGraphNode(
    function: kernelFunction,
    options: new NodeOptimizationOptions
    {
        EnableCaching = true,
        EnableBatching = true
    }
);
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
var authService = kernel.GetService<IAuthenticationService>();
var isValid = await authService.ValidateCredentialsAsync();
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
var fallbackNode = new FallbackGraphNode(
    primaryNode: primaryNode,
    fallbackNode: backupNode,
    condition: state => state.GetValue<bool>("use_fallback")
);
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


