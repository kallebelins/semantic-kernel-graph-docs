# Error Policies

This document covers the comprehensive error handling system in SemanticKernel.Graph, including policy management, retry mechanisms, error handling nodes, and metrics collection. The system provides robust resilience patterns with configurable policies, automatic retry logic, and comprehensive error tracking.

## ErrorPolicyRegistry

The `ErrorPolicyRegistry` provides centralized error handling policies across the graph execution system, supporting retry, circuit breaker, and budget policies with runtime policy resolution.

### Overview

This registry manages error handling policies with versioning, runtime resolution, and integration with the graph execution context. It supports policy rules based on error types, node types, and custom conditions.

### Key Features

- **Centralized Policy Management**: Single source of truth for all error handling policies
- **Runtime Policy Resolution**: Dynamic policy selection based on error context and execution state
- **Policy Versioning**: Support for policy updates and rollbacks
- **Circuit Breaker Integration**: Built-in circuit breaker policies per node
- **Budget Management**: Resource budget policies with automatic enforcement
- **Thread Safety**: All operations are thread-safe for concurrent access

### Policy Registration

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
registry.RegisterNodeCircuitBreakerPolicy("api-node", new CircuitBreakerPolicyConfig
{
    FailureThreshold = 5,
    RecoveryTimeout = TimeSpan.FromMinutes(1),
    MonitoringPeriod = TimeSpan.FromMinutes(5)
});
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
    // Apply retry logic with configured parameters
    var delay = CalculateRetryDelay(policy, errorContext.AttemptNumber);
    await Task.Delay(delay);
}
```

### Policy Rule Configuration

```csharp
public class PolicyRule
{
    public ErrorRecoveryAction RecoveryAction { get; set; }
    public int MaxRetries { get; set; }
    public TimeSpan RetryDelay { get; set; }
    public double BackoffMultiplier { get; set; }
    public TimeSpan MaxRetryDelay { get; set; }
    public int Priority { get; set; }
    public string? NodeTypePattern { get; set; }
    public ErrorSeverity? SeverityThreshold { get; set; }
    public Func<ErrorHandlingContext, GraphExecutionContext?, bool>? CustomCondition { get; set; }
}
```

## RetryPolicyGraphNode

The `RetryPolicyGraphNode` wraps another node with automatic retry capabilities, handling transient failures with configurable retry policies and backoff strategies.

### Overview

This specialized node provides automatic retry logic for wrapped nodes, supporting multiple retry strategies, error type filtering, and comprehensive retry statistics. It enhances graph resilience by automatically handling transient failures.

### Key Features

- **Automatic Retry Logic**: Configurable retry attempts with intelligent backoff
- **Multiple Retry Strategies**: Fixed delay, exponential backoff, linear backoff, and custom strategies
- **Error Type Filtering**: Retry only specific error types or use custom retry conditions
- **Jitter Support**: Random jitter to prevent thundering herd problems
- **Retry Statistics**: Comprehensive tracking of retry attempts and performance
- **Metadata Augmentation**: Adds retry context to kernel arguments and results

### Configuration

```csharp
var retryConfig = new RetryPolicyConfig
{
    MaxRetries = 3,
    BaseDelay = TimeSpan.FromSeconds(1),
    MaxDelay = TimeSpan.FromSeconds(30),
    Strategy = RetryStrategy.ExponentialBackoff,
    BackoffMultiplier = 2.0,
    UseJitter = true,
    RetryableErrorTypes = new HashSet<GraphErrorType>
    {
        GraphErrorType.Network,
        GraphErrorType.ServiceUnavailable,
        GraphErrorType.Timeout
    }
};

var retryNode = new RetryPolicyGraphNode(wrappedNode, retryConfig);
```

### Retry Strategies

```csharp
public enum RetryStrategy
{
    None = 0,                    // No retry attempts
    FixedDelay = 1,             // Fixed delay between attempts
    ExponentialBackoff = 2,     // Exponential increase in delay
    LinearBackoff = 3,          // Linear increase in delay
    RandomJitter = 4,           // Random jitter added to delay
    Custom = 5                   // Custom retry logic
}
```

### Usage Examples

#### Basic Retry Wrapper

```csharp
// Wrap a function node with retry policy
var functionNode = new FunctionGraphNode(kernelFunction, "api-call");
var retryNode = new RetryPolicyGraphNode(functionNode, new RetryPolicyConfig
{
    MaxRetries = 3,
    BaseDelay = TimeSpan.FromSeconds(1),
    Strategy = RetryStrategy.ExponentialBackoff
});

// Connect retry node in graph
graph.AddNode(retryNode);
graph.AddEdge(previousNode, retryNode);
```

#### Custom Retry Condition

```csharp
var retryNode = new RetryPolicyGraphNode(wrappedNode, new RetryPolicyConfig
{
    MaxRetries = 5,
    BaseDelay = TimeSpan.FromSeconds(2),
    CustomRetryCondition = (exception, attemptNumber) =>
    {
        // Only retry on specific exceptions
        if (exception is HttpRequestException httpEx)
        {
            return httpEx.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                   httpEx.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable;
        }
        return false;
    }
});
```

#### Retry Outcome Routing

```csharp
// Add edge that only executes after retry attempts
retryNode.AddEdgeForRetryOutcome(
    targetNode: fallbackNode,
    onlyOnRetrySuccess: false,
    minRetryAttempts: 2
);

// Add edge for successful retry scenarios
retryNode.AddEdgeForRetryOutcome(
    targetNode: successNode,
    onlyOnRetrySuccess: true,
    minRetryAttempts: 1
);
```

### Retry Statistics

The node provides comprehensive retry statistics:

```csharp
var statistics = retryNode.RetryStatistics;
Console.WriteLine($"Total retry attempts: {statistics.TotalRetryAttempts}");
Console.WriteLine($"Successful retries: {statistics.SuccessfulRetries}");
Console.WriteLine($"Average retry delay: {statistics.AverageRetryDelay}");
Console.WriteLine($"Last retry error: {statistics.LastRetryError?.Message}");
```

## ErrorHandlerGraphNode

The `ErrorHandlerGraphNode` is a specialized node for handling errors during graph execution, providing error categorization, recovery actions, and conditional routing based on error types.

### Overview

This node implements sophisticated error handling logic with automatic error categorization, configurable recovery actions, and intelligent routing decisions. It serves as a central error handling hub in complex workflows.

### Key Features

- **Automatic Error Categorization**: Maps exceptions to `GraphErrorType` enum values
- **Configurable Recovery Actions**: Retry, Skip, Fallback, Rollback, Halt, Escalate, Continue
- **Conditional Routing**: Dynamic edge selection based on error handling outcomes
- **Fallback Node Support**: Alternative execution paths for different error scenarios
- **Comprehensive Telemetry**: Detailed error tracking and recovery metrics
- **Default Error Handlers**: Pre-configured handling strategies for common error types

### Error Categorization

The node automatically categorizes exceptions into error types:

```csharp
// Automatic categorization based on exception type
ArgumentException → GraphErrorType.Validation
TimeoutException → GraphErrorType.Timeout
OperationCanceledException → GraphErrorType.Cancellation
HttpRequestException → GraphErrorType.Network
UnauthorizedAccessException → GraphErrorType.Authentication
OutOfMemoryException → GraphErrorType.ResourceExhaustion
```

### Default Error Handlers

```csharp
// Transient errors - retry automatically
_errorHandlers[GraphErrorType.Network] = ErrorRecoveryAction.Retry;
_errorHandlers[GraphErrorType.ServiceUnavailable] = ErrorRecoveryAction.Retry;
_errorHandlers[GraphErrorType.Timeout] = ErrorRecoveryAction.Retry;
_errorHandlers[GraphErrorType.RateLimit] = ErrorRecoveryAction.Retry;

// Authentication errors - halt execution
_errorHandlers[GraphErrorType.Authentication] = ErrorRecoveryAction.Halt;

// Validation errors - skip to next node
_errorHandlers[GraphErrorType.Validation] = ErrorRecoveryAction.Skip;

// Critical system errors - halt execution
_errorHandlers[GraphErrorType.ResourceExhaustion] = ErrorRecoveryAction.Halt;
_errorHandlers[GraphErrorType.GraphStructure] = ErrorRecoveryAction.Halt;
```

### Usage Examples

#### Basic Error Handler

```csharp
var errorHandler = new ErrorHandlerGraphNode(
    nodeId: "error-handler-1",
    name: "MainErrorHandler",
    description: "Handles errors in the main workflow",
    logger: graphLogger
);

// Add to graph
graph.AddNode(errorHandler);
graph.AddEdge(failingNode, errorHandler);
```

#### Custom Error Handling

```csharp
var errorHandler = new ErrorHandlerGraphNode("custom-error", "CustomErrorHandler");

// Configure custom error handling for specific error types
errorHandler.ConfigureErrorHandler(GraphErrorType.Network, ErrorRecoveryAction.Retry);
errorHandler.ConfigureErrorHandler(GraphErrorType.Authentication, ErrorRecoveryAction.Escalate);

// Set fallback nodes for specific error types
errorHandler.SetFallbackNode(GraphErrorType.ServiceUnavailable, alternativeServiceNode);
errorHandler.SetFallbackNode(GraphErrorType.Validation, validationHelperNode);
```

#### Conditional Routing Based on Error Outcomes

```csharp
// Route to different nodes based on recovery action
errorHandler.AddEdgeForRecoveryAction(
    targetNode: retryNode,
    recoveryAction: ErrorRecoveryAction.Retry
);

errorHandler.AddEdgeForRecoveryAction(
    targetNode: fallbackNode,
    recoveryAction: ErrorRecoveryAction.Fallback
);

errorHandler.AddEdgeForRecoveryAction(
    targetNode: escalationNode,
    recoveryAction: ErrorRecoveryAction.Escalate
);
```

### Error Context and Recovery

The node processes error context from kernel arguments:

```csharp
// Input parameters expected by the error handler
public IReadOnlyList<string> InputParameters { get; } = new[]
{
    "LastError",           // Exception that occurred
    "ErrorContext",        // Additional error context
    "ErrorType",           // Categorized error type
    "ErrorSeverity",       // Error severity level
    "AttemptCount"         // Current attempt number
}.AsReadOnly();

// Output parameters provided by the error handler
public IReadOnlyList<string> OutputParameters { get; } = new[]
{
    "ErrorHandled",        // Whether error was handled
    "RecoveryAction",      // Action taken to recover
    "ShouldRetry",         // Whether retry is recommended
    "RetryDelay",          // Suggested retry delay
    "FallbackExecuted",    // Whether fallback was used
    "EscalationRequired"   // Whether escalation is needed
}.AsReadOnly();
```

## ErrorMetricsCollector

The `ErrorMetricsCollector` collects, aggregates, and analyzes error metrics across graph executions, providing trend analysis, performance insights, and anomaly detection.

### Overview

This component provides comprehensive error tracking with real-time metrics, historical analysis, and performance insights. It integrates with the error handling system to provide actionable intelligence for system reliability.

### Key Features

- **Real-time Metrics Collection**: Immediate error event processing and aggregation
- **Multi-dimensional Analysis**: Metrics by execution, node, error type, and time
- **Performance Insights**: Error rates, recovery success rates, and trend analysis
- **Anomaly Detection**: Automatic identification of unusual error patterns
- **Configurable Retention**: Adjustable data retention and cleanup policies
- **Integration Ready**: Easy integration with monitoring and alerting systems

### Metrics Structure

```csharp
// Execution-level metrics
public class ExecutionErrorMetrics
{
    public string ExecutionId { get; set; }
    public int TotalErrors { get; set; }
    public List<GraphErrorType> ErrorTypes { get; set; }
    public double RecoverySuccessRate { get; set; }
    public double AverageErrorSeverity { get; set; }
    public DateTimeOffset FirstError { get; set; }
    public DateTimeOffset LastError { get; set; }
    public double ErrorRate { get; set; }
    public GraphErrorType MostCommonErrorType { get; set; }
}

// Node-level metrics
public class NodeErrorMetrics
{
    public string NodeId { get; set; }
    public int TotalErrors { get; set; }
    public double ErrorRate { get; set; }
    public double AverageErrorSeverity { get; set; }
    public double RecoverySuccessRate { get; set; }
    public DateTimeOffset LastErrorTime { get; set; }
    public GraphErrorType MostCommonErrorType { get; set; }
    public List<GraphErrorType> ErrorTypes { get; set; }
    public int RecoveryAttempts { get; set; }
    public int SuccessfulRecoveries { get; set; }
}
```

### Usage Examples

#### Basic Metrics Collection

```csharp
var metricsCollector = new ErrorMetricsCollector(new ErrorMetricsOptions
{
    AggregationInterval = TimeSpan.FromMinutes(1),
    MaxEventQueueSize = 10000,
    EnableMetricsCleanup = true,
    MetricsRetentionPeriod = TimeSpan.FromDays(7)
});

// Record an error event
metricsCollector.RecordError(
    executionId: "exec-123",
    nodeId: "api-node",
    errorContext: errorContext,
    recoveryAction: ErrorRecoveryAction.Retry,
    recoverySuccess: true
);
```

#### Metrics Queries

```csharp
// Get execution-specific metrics
var executionMetrics = metricsCollector.GetExecutionMetrics("exec-123");
if (executionMetrics != null)
{
    Console.WriteLine($"Total errors: {executionMetrics.TotalErrors}");
    Console.WriteLine($"Recovery success rate: {executionMetrics.RecoverySuccessRate:F2}%");
    Console.WriteLine($"Most common error: {executionMetrics.MostCommonErrorType}");
}

// Get node-specific metrics
var nodeMetrics = metricsCollector.GetNodeMetrics("api-node");
if (nodeMetrics != null)
{
    Console.WriteLine($"Node error rate: {nodeMetrics.ErrorRate:F2} errors/min");
    Console.WriteLine($"Recovery success rate: {nodeMetrics.RecoverySuccessRate:F2}%");
}

// Get overall statistics
var overallStats = metricsCollector.OverallStatistics;
Console.WriteLine($"Total errors recorded: {overallStats.TotalErrors}");
Console.WriteLine($"Current error rate: {overallStats.CurrentErrorRate:F2} errors/min");
Console.WriteLine($"Overall recovery success rate: {overallStats.RecoverySuccessRate:F2}%");
```

#### Batch Processing

```csharp
// Record multiple error events at once
var errorEvents = new List<ErrorEvent>
{
    new ErrorEvent
    {
        ExecutionId = "exec-123",
        NodeId = "node-1",
        ErrorType = GraphErrorType.Network,
        Severity = ErrorSeverity.Medium,
        IsTransient = true,
        Timestamp = DateTimeOffset.UtcNow
    },
    new ErrorEvent
    {
        ExecutionId = "exec-123",
        NodeId = "node-2",
        ErrorType = GraphErrorType.Timeout,
        Severity = ErrorSeverity.High,
        IsTransient = true,
        Timestamp = DateTimeOffset.UtcNow
    }
};

metricsCollector.RecordErrorBatch(errorEvents);
```

### Configuration Options

```csharp
public class ErrorMetricsOptions
{
    public TimeSpan AggregationInterval { get; set; } = TimeSpan.FromMinutes(1);
    public int MaxEventQueueSize { get; set; } = 10000;
    public bool EnableMetricsCleanup { get; set; } = true;
    public TimeSpan MetricsRetentionPeriod { get; set; } = TimeSpan.FromDays(7);
}
```

### Integration with Error Handling

The metrics collector integrates seamlessly with the error handling system:

```csharp
// In ErrorHandlerGraphNode
public async Task<FunctionResult> ExecuteAsync(Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken = default)
{
    try
    {
        // ... error handling logic ...
        
        // Record metrics for successful error handling
        _metricsCollector?.RecordError(
            executionId: arguments.GetExecutionId(),
            nodeId: NodeId,
            errorContext: errorContext,
            recoveryAction: recoveryAction,
            recoverySuccess: true
        );
        
        return result;
    }
    catch (Exception ex)
    {
        // Record metrics for failed error handling
        _metricsCollector?.RecordError(
            executionId: arguments.GetExecutionId(),
            nodeId: NodeId,
            errorContext: new ErrorHandlingContext { Exception = ex },
            recoveryAction: null,
            recoverySuccess: false
        );
        
        throw;
    }
}
```

## Error Types and Recovery Actions

### GraphErrorType Enum

```csharp
public enum GraphErrorType
{
    Unknown = 0,           // Unknown or unspecified error type
    Validation = 1,        // Validation error before or during execution
    NodeExecution = 2,     // Node execution failed due to internal logic error
    Timeout = 3,           // Timeout occurred during node or graph execution
    Network = 4,           // Network-related error (transient, retryable)
    ServiceUnavailable = 5, // External service unavailable (potentially transient)
    RateLimit = 6,         // Rate limiting exceeded (potentially transient)
    Authentication = 7,     // Authentication or authorization failure
    ResourceExhaustion = 8, // Resource exhaustion (memory, disk, etc.)
    GraphStructure = 9,    // Graph structure or navigation error
    Cancellation = 10,     // Cancellation was requested
    CircuitBreakerOpen = 11, // Circuit breaker is open (operation short-circuited)
    BudgetExhausted = 12   // Resource budget exhausted (CPU, memory, or cost limits)
}
```

### ErrorRecoveryAction Enum

```csharp
public enum ErrorRecoveryAction
{
    Continue = 0,          // Continue execution without recovery
    Retry = 1,             // Retry the failed operation
    Skip = 2,              // Skip the failed node and continue
    Fallback = 3,          // Execute fallback logic or alternative path
    Rollback = 4,          // Rollback to previous known good state
    Halt = 5,              // Halt execution and propagate error
    Escalate = 6,          // Escalate to human intervention
    CircuitBreaker = 7     // Open circuit breaker for the node
}
```

### ErrorSeverity Enum

```csharp
public enum ErrorSeverity
{
    Low = 0,               // Low severity - continue execution with logging
    Medium = 1,            // Medium severity - attempt recovery or retry
    High = 2,              // High severity - halt current branch, try alternatives
    Critical = 3           // Critical severity - halt entire graph execution
}
```

## See Also

- [Error Handling and Resilience Guide](../how-to/error-handling-and-resilience.md) - Comprehensive guide to implementing error handling patterns
- [Graph Executor Reference](graph-executor.md) - Core execution engine that integrates with error policies
- [Main Node Types Reference](main-node-types.md) - Other specialized node types for workflow construction
- [State and Serialization Reference](state-and-serialization.md) - State management for error recovery and rollback
- [Integration Reference](integration.md) - Error handling integration with external systems
- [Error Handling Examples](../examples/error-handling-examples.md) - Practical examples of error handling implementations
