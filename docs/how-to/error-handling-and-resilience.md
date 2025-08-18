# Error Handling and Resilience

Error handling and resilience in SemanticKernel.Graph provide robust mechanisms to handle failures, implement retry strategies, and prevent cascading failures through circuit breakers and resource budgets. This guide covers the comprehensive error handling system, including policies, metrics collection, and telemetry.

## What You'll Learn

- How to configure retry policies with exponential backoff and jitter
- Implementing circuit breaker patterns to prevent cascading failures
- Managing resource budgets and preventing resource exhaustion
- Configuring error handling policies through the registry
- Using specialized error handling nodes for complex scenarios
- Collecting and analyzing error metrics and telemetry
- Handling failure and cancellation events in streaming execution

## Concepts and Techniques

**ErrorPolicyRegistry**: Central registry for managing error handling policies across the graph, supporting retry, circuit breaker, and budget policies.

**RetryPolicyGraphNode**: Wraps other nodes with automatic retry capabilities, supporting configurable backoff strategies and error type filtering.

**ErrorHandlerGraphNode**: Specialized node for error categorization, recovery actions, and conditional routing based on error types.

**NodeCircuitBreakerManager**: Manages circuit breaker state per node, integrating with resource governance and error metrics.

**ResourceGovernor**: Provides adaptive rate limiting and resource budget management, preventing resource exhaustion.

**ErrorMetricsCollector**: Collects, aggregates, and analyzes error metrics for trend analysis and anomaly detection.

**ErrorHandlingTypes**: Comprehensive error categorization system with 13 error types and 8 recovery actions.

## Prerequisites

- [First Graph Tutorial](../first-graph-5-minutes.md) completed
- Basic understanding of graph execution concepts
- Familiarity with resilience patterns (retry, circuit breaker)
- Understanding of resource management principles

## Error Types and Recovery Actions

### Error Classification

SemanticKernel.Graph categorizes errors into 13 distinct types for precise handling:

```csharp
public enum GraphErrorType
{
    Unknown = 0,           // Unspecified error type
    Validation = 1,        // Validation errors
    NodeExecution = 2,     // Node logic errors
    Timeout = 3,           // Execution timeouts
    Network = 4,           // Network-related (retryable)
    ServiceUnavailable = 5, // External service unavailable
    RateLimit = 6,         // Rate limiting exceeded
    Authentication = 7,     // Auth/authorization failures
    ResourceExhaustion = 8, // Memory/disk exhaustion
    GraphStructure = 9,    // Graph navigation errors
    Cancellation = 10,     // Cancellation requested
    CircuitBreakerOpen = 11, // Circuit breaker state
    BudgetExhausted = 12   // Resource budget limits
}
```

### Recovery Actions

Eight recovery strategies are available for different error scenarios:

```csharp
public enum ErrorRecoveryAction
{
    Retry = 0,           // Retry with backoff
    Skip = 1,            // Skip to next node
    Fallback = 2,        // Execute fallback logic
    Rollback = 3,        // Rollback state changes
    Halt = 4,            // Stop execution
    Escalate = 5,        // Human intervention
    CircuitBreaker = 6,  // Open circuit breaker
    Continue = 7         // Continue execution
}
```

## Retry Policies and Backoff Strategies

### Basic Retry Configuration

Configure retry policies with exponential backoff and jitter:

```csharp
using SemanticKernel.Graph.Core;

var retryConfig = new RetryPolicyConfig
{
    MaxRetries = 3,
    BaseDelay = TimeSpan.FromSeconds(1),
    MaxDelay = TimeSpan.FromMinutes(1),
    Strategy = RetryStrategy.ExponentialBackoff,
    BackoffMultiplier = 2.0,
    UseJitter = true,
    RetryableErrorTypes = new HashSet<GraphErrorType>
    {
        GraphErrorType.Network,
        GraphErrorType.ServiceUnavailable,
        GraphErrorType.Timeout,
        GraphErrorType.RateLimit
    }
};
```

### Retry Strategies

Multiple retry strategies are supported:

```csharp
public enum RetryStrategy
{
    NoRetry = 0,              // No retry attempts
    FixedDelay = 1,           // Constant delay between attempts
    LinearBackoff = 2,        // Linear increase in delay
    ExponentialBackoff = 3,   // Exponential increase (default)
    Custom = 4                // Custom delay calculation
}
```

### Using RetryPolicyGraphNode

Wrap any node with automatic retry capabilities:

```csharp
using SemanticKernel.Graph.Nodes;

// Create a function node
var functionNode = new FunctionGraphNode("api-call", kernelFunction);

// Wrap with retry policy
var retryNode = new RetryPolicyGraphNode(functionNode, retryConfig);

// Add to graph
graph.AddNode(retryNode);
graph.AddEdge(startNode, retryNode);
```

The retry node automatically:
- Tracks attempt counts in `KernelArguments`
- Applies exponential backoff with jitter
- Filters retryable error types
- Records retry statistics in metadata

## Circuit Breaker Patterns

### Circuit Breaker Configuration

Configure circuit breakers to prevent cascading failures:

```csharp
var circuitBreakerConfig = new CircuitBreakerPolicyConfig
{
    Enabled = true,
    FailureThreshold = 5,           // Failures to open circuit
    OpenTimeout = TimeSpan.FromSeconds(30), // Time before half-open
    HalfOpenRetryCount = 3,         // Probe attempts in half-open
    FailureWindow = TimeSpan.FromMinutes(1), // Failure counting window
    TriggerOnBudgetExhaustion = true // Open on resource exhaustion
};
```

### Circuit Breaker States

Circuit breakers operate in three states:

1. **Closed**: Normal operation, failures are counted
2. **Open**: Circuit is open, operations are blocked
3. **Half-Open**: Limited operations allowed to test recovery

### Using NodeCircuitBreakerManager

Manage circuit breakers at the node level:

```csharp
using SemanticKernel.Graph.Core;

var circuitBreakerManager = new NodeCircuitBreakerManager(
    graphLogger,
    errorMetricsCollector,
    eventStream,
    resourceGovernor,
    performanceMetrics);

// Configure circuit breaker for a node
circuitBreakerManager.ConfigureNode("api-node", circuitBreakerConfig);

// Execute through circuit breaker
var result = await circuitBreakerManager.ExecuteAsync<string>(
    "api-node",
    executionId,
    async () => await apiCall(),
    async () => await fallbackCall()); // Optional fallback
```

## Resource Budget Management

### Resource Governance Configuration

Configure resource limits and adaptive rate limiting:

```csharp
var resourceOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 50.0,      // Base execution rate
    MaxBurstSize = 100,               // Maximum burst capacity
    CpuHighWatermarkPercent = 85.0,   // CPU threshold for backpressure
    CpuSoftLimitPercent = 70.0,       // Soft CPU limit
    MinAvailableMemoryMB = 512.0,     // Memory threshold
    DefaultPriority = ExecutionPriority.Normal
};

var resourceGovernor = new ResourceGovernor(resourceOptions);
```

### Execution Priorities

Four priority levels affect resource allocation:

```csharp
public enum ExecutionPriority
{
    Low = 0,      // 1.5x resource cost
    Normal = 1,   // 1.0x resource cost (default)
    High = 2,     // 0.6x resource cost
    Critical = 3  // 0.5x resource cost
}
```

### Resource Leases

Acquire resource permits before execution:

```csharp
// Acquire resource lease
using var lease = await resourceGovernor.AcquireAsync(
    workCostWeight: 2.0,           // Relative cost
    priority: ExecutionPriority.High,
    cancellationToken);

// Execute work
await performWork();

// Lease automatically released on dispose
```

## Error Policy Registry

### Centralized Policy Management

The `ErrorPolicyRegistry` provides centralized error handling policies:

```csharp
var registry = new ErrorPolicyRegistry(new ErrorPolicyRegistryOptions());

// Register retry policy for specific error types
registry.RegisterRetryPolicy(
    GraphErrorType.Network,
    new PolicyRule
    {
        RecoveryAction = ErrorRecoveryAction.Retry,
        MaxRetries = 3,
        RetryDelay = TimeSpan.FromSeconds(1),
        BackoffMultiplier = 2.0,
        Priority = 100
    });

// Register circuit breaker policy for a node
registry.RegisterNodeCircuitBreakerPolicy("api-node", circuitBreakerConfig);
```

### Policy Resolution

Policies are resolved based on error context and node information:

```csharp
var errorContext = new ErrorHandlingContext
{
    Exception = exception,
    ErrorType = GraphErrorType.Network,
    Severity = ErrorSeverity.Medium,
    AttemptNumber = 1,
    IsTransient = true
};

var policy = registry.ResolvePolicy(errorContext, executionContext);
if (policy?.RecoveryAction == ErrorRecoveryAction.Retry)
{
    // Apply retry logic
}
```

## Error Handling Nodes

### ErrorHandlerGraphNode

Specialized node for complex error handling scenarios:

```csharp
var errorHandler = new ErrorHandlerGraphNode(
    "error-handler",
    "ErrorHandler",
    "Handles errors and routes execution");

// Configure error handlers
errorHandler.ConfigureErrorHandler(GraphErrorType.Network, ErrorRecoveryAction.Retry);
errorHandler.ConfigureErrorHandler(GraphErrorType.Authentication, ErrorRecoveryAction.Escalate);
errorHandler.ConfigureErrorHandler(GraphErrorType.BudgetExhausted, ErrorRecoveryAction.CircuitBreaker);

// Add fallback nodes
errorHandler.AddFallbackNode(GraphErrorType.Network, fallbackNode);
errorHandler.AddFallbackNode(GraphErrorType.Authentication, escalationNode);

// Add to graph with conditional routing
graph.AddNode(errorHandler);
graph.AddConditionalEdge(startNode, errorHandler, 
    edge => edge.Condition = "HasError");
```

### Conditional Error Routing

Route execution based on error types and recovery actions:

```csharp
// Route based on error type
graph.AddConditionalEdge(errorHandler, retryNode,
    edge => edge.Condition = "ErrorType == 'Network'");

// Route based on recovery action
graph.AddConditionalEdge(errorHandler, fallbackNode,
    edge => edge.Condition = "RecoveryAction == 'Fallback'");

// Route based on severity
graph.AddConditionalEdge(errorHandler, escalationNode,
    edge => edge.Condition = "ErrorSeverity >= 'High'");
```

## Error Metrics and Telemetry

### Error Metrics Collection

Collect comprehensive error metrics for analysis:

```csharp
var errorMetricsOptions = new ErrorMetricsOptions
{
    AggregationInterval = TimeSpan.FromMinutes(1),
    MaxEventQueueSize = 10000,
    EnableMetricsCleanup = true,
    MetricsRetentionPeriod = TimeSpan.FromDays(7)
};

var errorMetricsCollector = new ErrorMetricsCollector(errorMetricsOptions, graphLogger);

// Record error events
errorMetricsCollector.RecordError(
    executionId,
    nodeId,
    errorContext,
    recoveryAction: ErrorRecoveryAction.Retry,
    recoverySuccess: true);
```

### Error Event Structure

Error events capture comprehensive information:

```csharp
public sealed class ErrorEvent
{
    public string EventId { get; set; }           // Unique identifier
    public string ExecutionId { get; set; }       // Execution context
    public string NodeId { get; set; }            // Node where error occurred
    public GraphErrorType ErrorType { get; set; } // Error classification
    public ErrorSeverity Severity { get; set; }   // Error severity
    public bool IsTransient { get; set; }         // Transient vs permanent
    public int AttemptNumber { get; set; }        // Retry attempt
    public DateTimeOffset Timestamp { get; set; } // When error occurred
    public string ExceptionType { get; set; }     // Exception type name
    public string ErrorMessage { get; set; }      // Error message
    public ErrorRecoveryAction? RecoveryAction { get; set; } // Action taken
    public bool? RecoverySuccess { get; set; }   // Recovery outcome
    public TimeSpan Duration { get; set; }        // Execution duration
}
```

### Metrics Queries

Query error metrics for analysis and monitoring:

```csharp
// Get execution-specific metrics
var executionMetrics = errorMetricsCollector.GetExecutionMetrics(executionId);
if (executionMetrics != null)
{
    Console.WriteLine($"Total Errors: {executionMetrics.TotalErrors}");
    Console.WriteLine($"Recovery Success Rate: {executionMetrics.RecoverySuccessRate:F1}%");
    Console.WriteLine($"Most Common Error: {executionMetrics.MostCommonErrorType}");
}

// Get node-specific metrics
var nodeMetrics = errorMetricsCollector.GetNodeMetrics(nodeId);
if (nodeMetrics != null)
{
    Console.WriteLine($"Node Error Rate: {nodeMetrics.ErrorRate:F2} errors/min");
    Console.WriteLine($"Recovery Success Rate: {nodeMetrics.RecoverySuccessRate:F1}%");
}

// Get overall statistics
var overallStats = errorMetricsCollector.OverallStatistics;
Console.WriteLine($"Current Error Rate: {overallStats.CurrentErrorRate:F2} errors/min");
Console.WriteLine($"Total Errors Recorded: {overallStats.TotalErrorsRecorded}");
```

## Integration with Graph Execution

### Error Handling Middleware

Integrate error handling with graph execution:

```csharp
// Configure error handling policy
var errorHandlingPolicy = new RegistryBackedErrorHandlingPolicy(errorPolicyRegistry);

// Configure graph executor with error handling
var executor = new GraphExecutor("ResilientGraph", "Graph with error handling")
    .ConfigureErrorHandling(errorHandlingPolicy)
    .ConfigureResources(resourceOptions);

// Add error metrics collection
executor.ConfigureMetrics(new GraphMetricsOptions
{
    EnableErrorMetrics = true,
    ErrorMetricsCollector = errorMetricsCollector
});
```

### Streaming Error Events

Error events are emitted to the execution stream:

```csharp
using var eventStream = executor.CreateStreamingExecutor()
    .CreateEventStream();

// Subscribe to error events
eventStream.SubscribeToEvents<GraphExecutionEvent>(event =>
{
    if (event.EventType == GraphExecutionEventType.NodeError)
    {
        var errorEvent = event as NodeErrorEvent;
        Console.WriteLine($"Error in {errorEvent.NodeId}: {errorEvent.ErrorType}");
        Console.WriteLine($"Recovery Action: {errorEvent.RecoveryAction}");
    }
});

// Execute with streaming
await executor.ExecuteAsync(arguments, eventStream);
```

## Best Practices

### Error Classification

- **Use specific error types** rather than generic `Unknown`
- **Mark transient errors** appropriately for retry logic
- **Set appropriate severity levels** for escalation decisions

### Retry Configuration

- **Start with exponential backoff** for most scenarios
- **Add jitter** to prevent thundering herd problems
- **Limit retry attempts** to prevent infinite loops
- **Use error type filtering** to avoid retrying permanent failures

### Circuit Breaker Tuning

- **Set appropriate failure thresholds** based on expected failure rates
- **Configure timeouts** that allow for recovery
- **Monitor circuit breaker state changes** for operational insights
- **Use fallback operations** when circuits are open

### Resource Management

- **Set realistic resource limits** based on system capacity
- **Use execution priorities** for critical operations
- **Monitor resource exhaustion** events for capacity planning
- **Implement graceful degradation** when budgets are exceeded

### Metrics and Monitoring

- **Collect error metrics** in production environments
- **Set up alerts** for high error rates or circuit breaker openings
- **Analyze error patterns** for system improvements
- **Track recovery success rates** to validate error handling

## Troubleshooting

### Common Issues

**Retry loops not working**: Check that error types are marked as retryable and `MaxRetries` is greater than 0.

**Circuit breaker not opening**: Verify `FailureThreshold` is appropriate and `TriggerErrorTypes` includes relevant error types.

**Resource budget exhaustion**: Check `BasePermitsPerSecond` and `MaxBurstSize` settings, and monitor system resource usage.

**Error metrics not collected**: Ensure `ErrorMetricsCollector` is properly configured and integrated with the graph executor.

### Debugging Error Handling

Enable debug logging to trace error handling decisions:

```csharp
// Configure detailed logging
var graphOptions = new GraphOptions
{
    LogLevel = LogLevel.Debug,
    EnableErrorHandlingLogging = true
};

var graphLogger = new SemanticKernelGraphLogger(logger, graphOptions);
```

### Performance Considerations

- **Error handling adds overhead** - use judiciously in performance-critical paths
- **Metrics collection** can impact performance at high error rates
- **Circuit breaker state changes** are logged and can generate noise
- **Resource budget checks** add latency to node execution

## See Also

- [Resource Governance and Concurrency](resource-governance-and-concurrency.md) - Managing resource limits and priorities
- [Metrics and Observability](metrics-logging-quickstart.md) - Comprehensive monitoring and telemetry
- [Streaming Execution](streaming-quickstart.md) - Real-time error event streaming
- [State Management](state-quickstart.md) - Error state persistence and recovery
- [Graph Execution](execution.md) - Understanding the execution lifecycle
