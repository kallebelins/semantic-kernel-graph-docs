# Error Handling and Resilience

Error handling and resilience in SemanticKernel.Graph provide robust mechanisms to handle failures, implement retry strategies, and prevent cascading failures through circuit breakers and resource budgets. This guide covers the comprehensive error handling system, including policies, metrics collection, and telemetry.

## What You'll Learn

* How to configure retry policies with exponential backoff and jitter
* Implementing circuit breaker patterns to prevent cascading failures
* Managing resource budgets and preventing resource exhaustion
* Configuring error handling policies through the registry
* Using specialized error handling nodes for complex scenarios
* Collecting and analyzing error metrics and telemetry
* Handling failure and cancellation events in streaming execution

## Concepts and Techniques

**ErrorPolicyRegistry**: Central registry for managing error handling policies across the graph, supporting retry, circuit breaker, and budget policies.

**RetryPolicyGraphNode**: Wraps other nodes with automatic retry capabilities, supporting configurable backoff strategies and error type filtering.

**ErrorHandlerGraphNode**: Specialized node for error categorization, recovery actions, and conditional routing based on error types.

**NodeCircuitBreakerManager**: Manages circuit breaker state per node, integrating with resource governance and error metrics.

**ResourceGovernor**: Provides adaptive rate limiting and resource budget management, preventing resource exhaustion.

**ErrorMetricsCollector**: Collects, aggregates, and analyzes error metrics for trend analysis and anomaly detection.

**ErrorHandlingTypes**: Comprehensive error categorization system with 13 error types and 8 recovery actions.

## Prerequisites

* [First Graph Tutorial](../first-graph-5-minutes.md) completed
* Basic understanding of graph execution concepts
* Familiarity with resilience patterns (retry, circuit breaker)
* Understanding of resource management principles

## Error Types and Recovery Actions

### Error Classification

SemanticKernel.Graph categorizes errors into 13 distinct types for precise handling:

```csharp
// GraphErrorType: classification of errors used by error handling components.
public enum GraphErrorType
{
    Unknown = 0,           // Unspecified error type
    Validation = 1,        // Input or schema validation errors
    NodeExecution = 2,     // Errors thrown during node execution
    Timeout = 3,           // Execution timeouts
    Network = 4,           // Network-related issues (commonly retryable)
    ServiceUnavailable = 5,// External service unavailable
    RateLimit = 6,         // Rate limiting exceeded by upstream services
    Authentication = 7,    // Authentication/authorization failures
    ResourceExhaustion = 8,// Memory/disk or quota exhaustion
    GraphStructure = 9,    // Graph traversal or configuration issues
    Cancellation = 10,     // Operation was cancelled via token
    CircuitBreakerOpen = 11,// Circuit breaker is currently open
    BudgetExhausted = 12   // Resource or budget limits reached
}
```

### Recovery Actions

Eight recovery strategies are available for different error scenarios:

```csharp
// ErrorRecoveryAction: recommended recovery strategies when an error occurs.
public enum ErrorRecoveryAction
{
    Retry = 0,           // Retry the failed operation using configured policy
    Skip = 1,            // Skip the failing node and continue
    Fallback = 2,        // Execute fallback logic or alternative node
    Rollback = 3,        // Rollback state changes performed so far
    Halt = 4,            // Stop the entire execution
    Escalate = 5,        // Escalate to human operator or alerting system
    CircuitBreaker = 6,  // Trigger circuit breaker behavior
    Continue = 7         // Continue despite the error (best-effort)
}
```

## Retry Policies and Backoff Strategies

### Basic Retry Configuration

Configure retry policies with exponential backoff and jitter:

```csharp
using SemanticKernel.Graph.Core;

// Configure a robust retry policy with exponential backoff and jitter.
var retryConfig = new RetryPolicyConfig
{
    MaxRetries = 3,
    BaseDelay = TimeSpan.FromSeconds(1),
    MaxDelay = TimeSpan.FromMinutes(1),
    Strategy = RetryStrategy.ExponentialBackoff,
    BackoffMultiplier = 2.0,
    UseJitter = true,
    // Only attempt retries for error types that are typically transient.
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
// RetryStrategy controls how retry delays are calculated between attempts.
public enum RetryStrategy
{
    NoRetry = 0,              // Do not perform retries
    FixedDelay = 1,           // Constant delay between attempts
    LinearBackoff = 2,        // Delay increases linearly per attempt
    ExponentialBackoff = 3,   // Delay grows exponentially (recommended)
    Custom = 4                // User-provided custom backoff calculation
}
```

### Using RetryPolicyGraphNode

Wrap any node with automatic retry capabilities:

```csharp
using SemanticKernel.Graph.Nodes;

// Example: create a simple function node and wrap it with retry behavior.
// 'kernelFunction' represents an existing KernelFunction instance.
var functionNode = new FunctionGraphNode("api-call", kernelFunction);

// Wrap with configured retry policy to make the call resilient.
var retryNode = new RetryPolicyGraphNode(functionNode, retryConfig);

// Add the retry node into the graph and connect it from the start node.
graph.AddNode(retryNode);
graph.AddEdge(startNode, retryNode);
```

The retry node automatically:
* Tracks attempt counts in `KernelArguments`
* Applies exponential backoff with jitter
* Filters retryable error types
* Records retry statistics in metadata

## Circuit Breaker Patterns

### Circuit Breaker Configuration

Configure circuit breakers to prevent cascading failures:

```csharp
// Configure a circuit breaker to prevent cascading failures.
var circuitBreakerConfig = new CircuitBreakerPolicyConfig
{
    Enabled = true,
    FailureThreshold = 5,               // Number of failures to open the circuit
    OpenTimeout = TimeSpan.FromSeconds(30), // Time to wait before moving to half-open
    HalfOpenRetryCount = 3,             // Number of probe attempts in half-open state
    FailureWindow = TimeSpan.FromMinutes(1), // Window for failure counting
    TriggerOnBudgetExhaustion = true    // Open when resource budgets are exhausted
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

// Create a manager responsible for per-node circuit breaker state.
var circuitBreakerManager = new NodeCircuitBreakerManager(
    graphLogger,
    errorMetricsCollector,
    eventStream,
    resourceGovernor,
    performanceMetrics);

// Apply the circuit breaker policy to a specific node id.
circuitBreakerManager.ConfigureNode("api-node", circuitBreakerConfig);

// Execute a protected operation via the circuit breaker manager. Provide
// an optional fallback to run when the circuit is open or the operation fails.
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
// Configure resource governance to control concurrency and resource usage.
var resourceOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 50.0,      // Base execution rate per second
    MaxBurstSize = 100,               // Allowed burst capacity
    CpuHighWatermarkPercent = 85.0,   // CPU usage threshold for strong backpressure
    CpuSoftLimitPercent = 70.0,       // Soft CPU threshold before throttling
    MinAvailableMemoryMB = 512.0,     // Minimum free memory for allocations
    DefaultPriority = ExecutionPriority.Normal
};

// Create the governor that enforces the configured resource limits.
var resourceGovernor = new ResourceGovernor(resourceOptions);
```

### Execution Priorities

Four priority levels affect resource allocation:

```csharp
// ExecutionPriority influences how the resource governor allocates permits.
public enum ExecutionPriority
{
    Low = 0,      // Low priority (higher relative cost)
    Normal = 1,   // Default priority
    High = 2,     // High priority for latency-sensitive work
    Critical = 3  // Highest priority for essential operations
}
```

### Resource Leases

Acquire resource permits before execution:

```csharp
// Acquire a resource lease before performing work. The lease is released when disposed.
using var lease = await resourceGovernor.AcquireAsync(
    workCostWeight: 2.0,                  // Cost weight for this unit of work
    priority: ExecutionPriority.High,
    cancellationToken);

// Perform the protected work while the lease is held.
await performWork();

// Lease is automatically released when 'lease' is disposed (end of using block).
```

## Error Policy Registry

### Centralized Policy Management

The `ErrorPolicyRegistry` provides centralized error handling policies:

```csharp
// Create a central registry to hold error handling rules used by the registry-backed policy.
var registry = new ErrorPolicyRegistry(new ErrorPolicyRegistryOptions());

// Register a retry rule for network errors. The registry will be consulted by
// the error handling policy during execution to determine recovery actions.
registry.RegisterPolicyRule(new PolicyRule
{
    ContextId = "Examples",
    ErrorType = GraphErrorType.Network,
    RecoveryAction = ErrorRecoveryAction.Retry,
    MaxRetries = 3,
    RetryDelay = TimeSpan.FromSeconds(1),
    BackoffMultiplier = 2.0,
    Priority = 100,
    Description = "Retry network errors"
});

// Register node-level circuit breaker configuration for 'api-node'.
registry.RegisterNodeCircuitBreakerPolicy("api-node", circuitBreakerConfig);
```

### Policy Resolution

Policies are resolved based on error context and node information:

```csharp
// Construct an error handling context that represents a captured exception.
var errorContext = new ErrorHandlingContext
{
    Exception = exception,
    ErrorType = GraphErrorType.Network,
    Severity = ErrorSeverity.Medium,
    AttemptNumber = 1,
    IsTransient = true
};

// Resolve the appropriate policy for this error and execution context.
var policy = registry.ResolvePolicy(errorContext, executionContext);
if (policy?.RecoveryAction == ErrorRecoveryAction.Retry)
{
    // The resolved policy indicates this error should be retried.
    // Retry orchestration should respect policy.MaxRetries and timing values.
}
```

## Error Handling Nodes

### ErrorHandlerGraphNode

Specialized node for complex error handling scenarios:

```csharp
// Create a specialized node that inspects errors and routes execution accordingly.
var errorHandler = new ErrorHandlerGraphNode(
    "error-handler",
    "ErrorHandler",
    "Handles errors and routes execution");

// Map specific error types to recovery actions.
errorHandler.ConfigureErrorHandler(GraphErrorType.Network, ErrorRecoveryAction.Retry);
errorHandler.ConfigureErrorHandler(GraphErrorType.Authentication, ErrorRecoveryAction.Escalate);
errorHandler.ConfigureErrorHandler(GraphErrorType.BudgetExhausted, ErrorRecoveryAction.CircuitBreaker);

// Define fallback nodes to execute when specific recovery actions are selected.
errorHandler.AddFallbackNode(GraphErrorType.Network, fallbackNode);
errorHandler.AddFallbackNode(GraphErrorType.Authentication, escalationNode);

// Add the error handler into the graph and wire conditional routing based on an error flag.
graph.AddNode(errorHandler);
graph.AddConditionalEdge(startNode, errorHandler,
    edge => edge.Condition = "HasError");
```

### Conditional Error Routing

Route execution based on error types and recovery actions:

```csharp
// Examples of conditional routing from the error handler based on resolved conditions.
// Route to retry node for network errors.
graph.AddConditionalEdge(errorHandler, retryNode,
    edge => edge.Condition = "ErrorType == 'Network'");

// Route to fallback node when recovery action indicates a fallback should run.
graph.AddConditionalEdge(errorHandler, fallbackNode,
    edge => edge.Condition = "RecoveryAction == 'Fallback'");

// Route to escalation flow for high severity issues.
graph.AddConditionalEdge(errorHandler, escalationNode,
    edge => edge.Condition = "ErrorSeverity >= 'High'");
```

## Error Metrics and Telemetry

### Error Metrics Collection

Collect comprehensive error metrics for analysis:

```csharp
// Configure error metrics collection and initialize the collector.
var errorMetricsOptions = new ErrorMetricsOptions
{
    AggregationInterval = TimeSpan.FromMinutes(1),
    MaxEventQueueSize = 10000,
    EnableMetricsCleanup = true,
    MetricsRetentionPeriod = TimeSpan.FromDays(7)
};

var errorMetricsCollector = new ErrorMetricsCollector(errorMetricsOptions, graphLogger);

// Record a sample error event to validate metrics plumbing.
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
// ErrorEvent: immutable-like DTO describing a single captured error occurrence.
public sealed class ErrorEvent
{
    public string EventId { get; set; }           // Unique identifier for the event
    public string ExecutionId { get; set; }       // Execution context id
    public string NodeId { get; set; }            // Node where the error occurred
    public GraphErrorType ErrorType { get; set; } // Classified error type
    public ErrorSeverity Severity { get; set; }   // Severity level for alerting/escalation
    public bool IsTransient { get; set; }         // Indicates if the error is transient
    public int AttemptNumber { get; set; }        // The attempt number when the error happened
    public DateTimeOffset Timestamp { get; set; } // When the error was recorded
    public string ExceptionType { get; set; }     // CLR exception type name
    public string ErrorMessage { get; set; }      // The exception message or error description
    public ErrorRecoveryAction? RecoveryAction { get; set; } // Suggested recovery action
    public bool? RecoverySuccess { get; set; }    // Whether the recovery succeeded
    public TimeSpan Duration { get; set; }        // Duration of the failing operation
}
```

### Metrics Queries

Query error metrics for analysis and monitoring:

```csharp
// Query and display execution-specific metrics from the collector.
var executionMetrics = errorMetricsCollector.GetExecutionMetrics(executionId);
if (executionMetrics != null)
{
    Console.WriteLine($"Total Errors: {executionMetrics.TotalErrors}");
    Console.WriteLine($"Recovery Success Rate: {executionMetrics.RecoverySuccessRate:F1}%");
    Console.WriteLine($"Most Common Error: {executionMetrics.MostCommonErrorType}");
}

// Query node-specific metrics for targeted troubleshooting.
var nodeMetrics = errorMetricsCollector.GetNodeMetrics(nodeId);
if (nodeMetrics != null)
{
    Console.WriteLine($"Node Error Rate: {nodeMetrics.ErrorRate:F2} errors/min");
    Console.WriteLine($"Recovery Success Rate: {nodeMetrics.RecoverySuccessRate:F1}%");
}

// Read overall aggregated statistics exposed by the collector.
var overallStats = errorMetricsCollector.OverallStatistics;
Console.WriteLine($"Current Error Rate: {overallStats.CurrentErrorRate:F2} errors/min");
Console.WriteLine($"Total Errors Recorded: {overallStats.TotalErrorsRecorded}");
```

## Integration with Graph Execution

### Error Handling Middleware

Integrate error handling with graph execution:

```csharp
// Use a registry-backed error handling policy to centralize decisions.
var errorHandlingPolicy = new RegistryBackedErrorHandlingPolicy(errorPolicyRegistry);

// Create a graph executor configured with error handling and resource governance.
var executor = new GraphExecutor("ResilientGraph", "Graph with error handling")
    .ConfigureErrorHandling(errorHandlingPolicy)
    .ConfigureResources(resourceOptions);

// Attach the error metrics collector to the executor for telemetry.
executor.ConfigureMetrics(new GraphMetricsOptions
{
    EnableErrorMetrics = true,
    ErrorMetricsCollector = errorMetricsCollector
});
```

### Streaming Error Events

Error events are emitted to the execution stream:

```csharp
// Create a streaming executor and subscribe to runtime events, including error events.
using var eventStream = executor.CreateStreamingExecutor()
    .CreateEventStream();

// Subscribe to execution events and handle node error events for observability.
eventStream.SubscribeToEvents<GraphExecutionEvent>(evt =>
{
    if (evt.EventType == GraphExecutionEventType.NodeError)
    {
        var errorEvent = evt as NodeErrorEvent;
        Console.WriteLine($"Error in {errorEvent.NodeId}: {errorEvent.ErrorType}");
        Console.WriteLine($"Recovery Action: {errorEvent.RecoveryAction}");
    }
});

// Start execution with an attached event stream for real-time inspection.
await executor.ExecuteAsync(arguments, eventStream);
```

## Best Practices

### Error Classification

* **Use specific error types** rather than generic `Unknown`
* **Mark transient errors** appropriately for retry logic
* **Set appropriate severity levels** for escalation decisions

### Retry Configuration

* **Start with exponential backoff** for most scenarios
* **Add jitter** to prevent thundering herd problems
* **Limit retry attempts** to prevent infinite loops
* **Use error type filtering** to avoid retrying permanent failures

### Circuit Breaker Tuning

* **Set appropriate failure thresholds** based on expected failure rates
* **Configure timeouts** that allow for recovery
* **Monitor circuit breaker state changes** for operational insights
* **Use fallback operations** when circuits are open

### Resource Management

* **Set realistic resource limits** based on system capacity
* **Use execution priorities** for critical operations
* **Monitor resource exhaustion** events for capacity planning
* **Implement graceful degradation** when budgets are exceeded

### Metrics and Monitoring

* **Collect error metrics** in production environments
* **Set up alerts** for high error rates or circuit breaker openings
* **Analyze error patterns** for system improvements
* **Track recovery success rates** to validate error handling

## Troubleshooting

### Common Issues

**Retry loops not working**: Check that error types are marked as retryable and `MaxRetries` is greater than 0.

**Circuit breaker not opening**: Verify `FailureThreshold` is appropriate and `TriggerErrorTypes` includes relevant error types.

**Resource budget exhaustion**: Check `BasePermitsPerSecond` and `MaxBurstSize` settings, and monitor system resource usage.

**Error metrics not collected**: Ensure `ErrorMetricsCollector` is properly configured and integrated with the graph executor.

### Debugging Error Handling

Enable debug logging to trace error handling decisions:

```csharp
// Configure detailed graph logging to trace error handling decisions.
var graphOptions = new GraphOptions
{
    LogLevel = LogLevel.Debug,
    EnableErrorHandlingLogging = true
};

// Create a logger adapter used across graph components.
var graphLogger = new SemanticKernelGraphLogger(logger, graphOptions);
```

### Performance Considerations

* **Error handling adds overhead** - use judiciously in performance-critical paths
* **Metrics collection** can impact performance at high error rates
* **Circuit breaker state changes** are logged and can generate noise
* **Resource budget checks** add latency to node execution

## See Also

* [Resource Governance and Concurrency](resource-governance-and-concurrency.md) - Managing resource limits and priorities
* [Metrics and Observability](metrics-logging-quickstart.md) - Comprehensive monitoring and telemetry
* [Streaming Execution](streaming-quickstart.md) - Real-time error event streaming
* [State Management](state-quickstart.md) - Error state persistence and recovery
* [Graph Execution](execution.md) - Understanding the execution lifecycle
