# Additional Utilities

This reference covers additional utility classes and methods that are publicly exposed in SemanticKernel.Graph, providing helper functionality for common operations, validation, and advanced patterns.

## Overview

The SemanticKernel.Graph library provides a comprehensive set of utility classes and extension methods that simplify common operations, enable advanced patterns, and provide validation and debugging capabilities. These utilities are designed to be non-intrusive and follow functional programming principles where possible.

## Extension Classes

### AdvancedPatternsExtensions

Extension methods that integrate advanced patterns with the graph system, providing fluent helpers for academic resilience patterns, optimizations, and enterprise integration.

#### Academic Patterns

```csharp
public static class AdvancedPatternsExtensions
{
    // Add academic resilience patterns
    public static GraphExecutor WithAcademicPatterns(
        this GraphExecutor executor,
        Action<AcademicPatternsConfiguration>? configureOptions = null)
    
    // Execute with circuit breaker protection
    public static async Task<T> ExecuteWithCircuitBreakerAsync<T>(
        this GraphExecutor executor,
        Func<Task<T>> operation,
        Func<Task<T>>? fallback = null)
    
    // Execute with bulkhead isolation
    public static async Task<T> ExecuteWithBulkheadAsync<T>(
        this GraphExecutor executor,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    
    // Execute with cache-aside pattern
    public static async Task<T> ExecuteWithCacheAsideAsync<T>(
        this GraphExecutor executor,
        string cacheKey,
        Func<Task<T>> operation,
        TimeSpan? expiration = null)
}
```

**Example:**
```csharp
var executor = new GraphExecutor("resilient-graph")
    .WithAcademicPatterns(config => 
    {
        config.EnableCircuitBreaker = true;
        config.EnableBulkhead = true;
        config.EnableCacheAside = true;
    });

// Execute with circuit breaker protection
var result = await executor.ExecuteWithCircuitBreakerAsync(
    async () => await ProcessDataAsync(),
    async () => await GetFallbackDataAsync()
);
```

### CheckpointingExtensions

Extension methods for checkpointing and state persistence operations.

```csharp
public static class CheckpointingExtensions
{
    // Create checkpoint with metadata
    public static async Task<string> CreateCheckpointAsync(
        this GraphState state,
        string checkpointName,
        IDictionary<string, object>? metadata = null)
    
    // Restore checkpoint by ID
    public static async Task<GraphState> RestoreCheckpointAsync(
        this GraphState state,
        string checkpointId)
    
    // List available checkpoints
    public static async Task<IReadOnlyList<CheckpointInfo>> ListCheckpointsAsync(
        this GraphState state)
    
    // Delete checkpoint
    public static async Task DeleteCheckpointAsync(
        this GraphState state,
        string checkpointId)
}
```

**Example:**
```csharp
// Create checkpoint before risky operation
var checkpointId = await graphState.CreateCheckpointAsync("before_processing", 
    new Dictionary<string, object> { ["operation"] = "data_processing" });

try
{
    // Perform risky operation
    await ProcessDataAsync();
}
catch
{
    // Restore to safe state
    graphState = await graphState.RestoreCheckpointAsync(checkpointId);
}
```

### DynamicRoutingExtensions

Extension methods for dynamic routing and conditional execution patterns.

```csharp
public static class DynamicRoutingExtensions
{
    // Add dynamic routing to executor
    public static GraphExecutor WithDynamicRouting(
        this GraphExecutor executor,
        Action<DynamicRoutingConfiguration>? configureOptions = null)
    
    // Configure routing strategies
    public static GraphExecutor ConfigureRoutingStrategies(
        this GraphExecutor executor,
        IDictionary<string, IRoutingStrategy> strategies)
    
    // Add routing middleware
    public static GraphExecutor AddRoutingMiddleware(
        this GraphExecutor executor,
        IRoutingMiddleware middleware)
}
```

**Example:**
```csharp
var executor = new GraphExecutor("dynamic-graph")
    .WithDynamicRouting(config => 
    {
        config.EnableEmbeddingBasedRouting = true;
        config.EnableProbabilisticRouting = true;
        config.DefaultRoutingStrategy = "similarity";
    });
```

### GraphDebuggerExtensions

Extension methods for integrating debugging capabilities with graph execution.

```csharp
public static class GraphDebuggerExtensions
{
    // Execute with debugging enabled
    public static async Task<(FunctionResult Result, IDebugSession DebugSession)> ExecuteWithDebugAsync(
        this GraphExecutor executor,
        Kernel kernel,
        KernelArguments arguments,
        DebugExecutionMode debugMode = DebugExecutionMode.StepOver,
        CancellationToken cancellationToken = default)
    
    // Create debug session
    public static IDebugSession CreateDebugSession(
        this GraphExecutor executor,
        GraphExecutionContext context)
    
    // Create execution replay
    public static ExecutionReplay CreateReplay(this IDebugSession debugSession)
    
    // Enable debug mode
    public static GraphExecutor EnableDebugMode(
        this GraphExecutor executor,
        DebugExecutionMode mode = DebugExecutionMode.StepOver)
}
```

**Example:**
```csharp
// Execute with step-by-step debugging
var (result, debugSession) = await executor.ExecuteWithDebugAsync(
    kernel, 
    arguments, 
    DebugExecutionMode.StepInto
);

// Create replay for analysis
var replay = debugSession.CreateReplay();
foreach (var step in replay.GetExecutionSteps())
{
    Console.WriteLine($"Step {step.StepNumber}: {step.NodeName}");
}
```

### GraphPerformanceExtensions

Extension methods for performance monitoring and optimization.

```csharp
public static class GraphPerformanceExtensions
{
    // Enable performance metrics
    public static GraphExecutor WithPerformanceMetrics(
        this GraphExecutor executor,
        Action<GraphPerformanceOptions>? configureOptions = null)
    
    // Get performance summary
    public static GraphPerformanceSummary GetPerformanceSummary(
        this GraphExecutor executor)
    
    // Export metrics
    public static async Task ExportMetricsAsync(
        this GraphExecutor executor,
        IMetricsExporter exporter)
}
```

**Example:**
```csharp
var executor = new GraphExecutor("performance-graph")
    .WithPerformanceMetrics(config => 
    {
        config.EnableDetailedMetrics = true;
        config.EnableExport = true;
        config.ExportInterval = TimeSpan.FromMinutes(5);
    });

// Get performance summary after execution
var summary = executor.GetPerformanceSummary();
Console.WriteLine($"Total execution time: {summary.TotalExecutionTime}");
Console.WriteLine($"Average node time: {summary.AverageNodeExecutionTime}");
```

### HumanInTheLoopExtensions

Extension methods for Human-in-the-Loop functionality.

```csharp
public static class HumanInTheLoopExtensions
{
    // Add human approval node
    public static GraphExecutor AddHumanApproval(
        this GraphExecutor executor,
        string nodeId,
        string title,
        string message,
        IHumanInteractionChannel channel)
    
    // Add confidence gate
    public static GraphExecutor AddConfidenceGate(
        this GraphExecutor executor,
        string nodeId,
        double confidenceThreshold,
        IConfidenceSource confidenceSource)
    
    // Configure HITL options
    public static GraphExecutor ConfigureHumanInTheLoop(
        this GraphExecutor executor,
        Action<HumanInTheLoopOptions> configureOptions)
}
```

**Example:**
```csharp
var executor = new GraphExecutor("hitl-graph")
    .AddHumanApproval("approval", "Data Review", "Please review the processed data", channel)
    .AddConfidenceGate("confidence", 0.8, confidenceSource)
    .ConfigureHumanInTheLoop(config => 
    {
        config.DefaultTimeout = TimeSpan.FromHours(24);
        config.EnableBatching = true;
    });
```

### LoggingExtensions

Extension methods for enhanced graph logging functionality.

```csharp
public static class LoggingExtensions
{
    // Log graph-level information
    public static void LogGraphInfo(
        this IGraphLogger logger,
        string executionId,
        string message,
        IDictionary<string, object>? properties = null)
    
    // Log node-level information
    public static void LogNodeInfo(
        this IGraphLogger logger,
        string executionId,
        string nodeId,
        string message,
        IDictionary<string, object>? properties = null)
    
    // Log execution metrics
    public static void LogExecutionMetrics(
        this IGraphLogger logger,
        string executionId,
        GraphPerformanceMetrics metrics)
}
```

**Example:**
```csharp
logger.LogGraphInfo(executionId, "Graph execution started", 
    new Dictionary<string, object> { ["nodeCount"] = 5 });

logger.LogNodeInfo(executionId, "node1", "Node execution completed", 
    new Dictionary<string, object> { ["duration"] = "150ms" });
```

### MultiAgentExtensions

Extension methods for multi-agent coordination and communication.

```csharp
public static class MultiAgentExtensions
{
    // Add multi-agent support
    public static GraphExecutor WithMultiAgentSupport(
        this GraphExecutor executor,
        Action<MultiAgentOptions>? configureOptions = null)
    
    // Configure agent coordination
    public static GraphExecutor ConfigureAgentCoordination(
        this GraphExecutor executor,
        IAgentCoordinationStrategy strategy)
    
    // Add agent communication
    public static GraphExecutor AddAgentCommunication(
        this GraphExecutor executor,
        IAgentCommunicationChannel channel)
}
```

**Example:**
```csharp
var executor = new GraphExecutor("multi-agent-graph")
    .WithMultiAgentSupport(config => 
    {
        config.MaxConcurrentAgents = 10;
        config.EnableLoadBalancing = true;
        config.DefaultAgentTimeout = TimeSpan.FromMinutes(5);
    });
```

### RecoveryExtensions

Extension methods for recovery and replay functionality.

```csharp
public static class RecoveryExtensions
{
    // Enable recovery support
    public static GraphExecutor WithRecoverySupport(
        this GraphExecutor executor,
        Action<RecoveryOptions>? configureOptions = null)
    
    // Configure recovery strategies
    public static GraphExecutor ConfigureRecoveryStrategies(
        this GraphExecutor executor,
        IDictionary<string, IRecoveryStrategy> strategies)
    
    // Enable replay mode
    public static GraphExecutor EnableReplayMode(
        this GraphExecutor executor,
        ReplayOptions options)
}
```

**Example:**
```csharp
var executor = new GraphExecutor("recovery-graph")
    .WithRecoverySupport(config => 
    {
        config.EnableAutomaticRecovery = true;
        config.MaxRecoveryAttempts = 3;
        config.RecoveryTimeout = TimeSpan.FromMinutes(10);
    });
```

### StateExtensions

Extension methods for state management and cloning operations.

```csharp
public static class StateExtensions
{
    // Clone GraphState
    public static GraphState Clone(this GraphState state)
    
    // Merge states
    public static GraphState MergeFrom(this GraphState state, GraphState other)
    
    // Get string representation
    public static string GetStringValue(this GraphState state, string parameterName)
}
```

**Example:**
```csharp
// Clone state for parallel processing
var clonedState = graphState.Clone();

// Merge states
var mergedState = state1.MergeFrom(state2);

// Get string value with fallback
var value = graphState.GetStringValue("userInput") ?? "default";
```

### StreamingExtensions

Extension methods for streaming execution and event handling.

```csharp
public static class StreamingExtensions
{
    // Enable streaming execution
    public static GraphExecutor WithStreamingSupport(
        this GraphExecutor executor,
        Action<StreamingOptions>? configureOptions = null)
    
    // Configure event stream
    public static GraphExecutor ConfigureEventStream(
        this GraphExecutor executor,
        IGraphExecutionEventStream eventStream)
    
    // Add streaming middleware
    public static GraphExecutor AddStreamingMiddleware(
        this GraphExecutor executor,
        IStreamingMiddleware middleware)
}
```

**Example:**
```csharp
var executor = new GraphExecutor("streaming-graph")
    .WithStreamingSupport(config => 
    {
        config.EnableRealTimeEvents = true;
        config.EventBufferSize = 1000;
        config.EnableCompression = true;
    });
```

## Utility Classes

### StateHelpers

Static utility methods for common state operations including serialization, merging, validation, and checkpointing.

#### Serialization Methods

```csharp
public static class StateHelpers
{
    // Serialize state with options
    public static string SerializeState(
        GraphState state, 
        bool indented = false, 
        bool enableCompression = true, 
        bool useCache = true)
    
    // Serialize with metrics
    public static string SerializeState(
        GraphState state, 
        bool indented, 
        bool enableCompression, 
        bool useCache, 
        out SerializationMetrics metrics)
    
    // Deserialize state
    public static GraphState DeserializeState(string serializedData)
    
    // Create state snapshot
    public static GraphState CreateSnapshot(GraphState state)
}
```

#### State Management Methods

```csharp
// Merge states with conflict resolution
public static StateMergeResult MergeStates(
    GraphState baseState, 
    GraphState otherState, 
    StateMergeConflictPolicy policy)

// Validate state integrity
public static ValidationResult ValidateState(GraphState state)

// Create checkpoint
public static string CreateCheckpoint(GraphState state, string checkpointName)

// Restore checkpoint
public static GraphState RestoreCheckpoint(GraphState state, string checkpointId)

// Rollback transaction
public static GraphState RollbackTransaction(GraphState state, string transactionId)
```

#### Compression Methods

```csharp
// Get compression statistics
public static CompressionStats GetCompressionStats(string data)

// Get adaptive compression threshold
public static int GetAdaptiveCompressionThreshold()

// Reset adaptive compression
public static void ResetAdaptiveCompression()

// Get adaptive compression state
public static AdaptiveCompressionState GetAdaptiveCompressionState()
```

### StateValidator

Static utility methods for validating graph state integrity and consistency.

```csharp
public static class StateValidator
{
    // Validate complete state
    public static ValidationResult ValidateState(GraphState state)
    
    // Validate critical properties
    public static bool ValidateCriticalProperties(GraphState state)
    
    // Validate parameter names
    public static IList<string> ValidateParameterNames(GraphState state)
    
    // Validate execution history
    public static IList<string> ValidateExecutionHistory(GraphState state)
}
```

### ConditionalExpressionEvaluator

Utility class for evaluating conditional expressions using Semantic Kernel templates and custom logic.

```csharp
public sealed class ConditionalExpressionEvaluator
{
    // Evaluate conditional expression
    public static ConditionalEvaluationResult Evaluate(
        string expression, 
        GraphState state, 
        Kernel kernel)
    
    // Evaluate with custom context
    public static ConditionalEvaluationResult Evaluate(
        string expression, 
        GraphState state, 
        Kernel kernel, 
        IDictionary<string, object> customContext)
    
    // Get evaluation statistics
    public static ConditionalEvaluationStats GetStatistics()
    
    // Clear evaluation cache
    public static void ClearCache()
}
```

### ChainOfThoughtValidator

Utility class for validating Chain-of-Thought reasoning steps.

```csharp
public sealed class ChainOfThoughtValidator
{
    // Validate reasoning step
    public async Task<ChainOfThoughtValidationResult> ValidateStepAsync(
        ChainOfThoughtStep step,
        ChainOfThoughtContext context,
        ChainOfThoughtResult previousResult,
        CancellationToken cancellationToken = default)
    
    // Add custom validation rule
    public void AddCustomRule(IChainOfThoughtValidationRule rule)
    
    // Configure validation thresholds
    public void ConfigureThresholds(IDictionary<string, double> thresholds)
    
    // Get validation statistics
    public ChainOfThoughtValidationStats GetStatistics()
}
```

## Module Activation

### ModuleActivationExtensions

Extensions for conditionally activating optional graph modules via dependency injection.

```csharp
public static class ModuleActivationExtensions
{
    // Add optional graph modules
    public static IKernelBuilder AddGraphModules(
        this IKernelBuilder builder, 
        Action<GraphModuleActivationOptions>? configure = null)
}

public sealed class GraphModuleActivationOptions
{
    public bool EnableStreaming { get; set; }
    public bool EnableCheckpointing { get; set; }
    public bool EnableRecovery { get; set; }
    public bool EnableHumanInTheLoop { get; set; }
    public bool EnableMultiAgent { get; set; }
    
    // Apply environment overrides
    public void ApplyEnvironmentOverrides()
}
```

**Example:**
```csharp
var builder = Kernel.CreateBuilder()
    .AddGraphModules(options => 
    {
        options.EnableStreaming = true;
        options.EnableCheckpointing = true;
        options.EnableRecovery = true;
        options.EnableHumanInTheLoop = true;
        options.EnableMultiAgent = true;
    });

// Environment variables can override these settings:
// SKG_ENABLE_STREAMING=true
// SKG_ENABLE_CHECKPOINTING=true
// SKG_ENABLE_RECOVERY=true
// SKG_ENABLE_HITL=true
// SKG_ENABLE_MULTIAGENT=true
```

## Usage Examples

### Basic Utility Usage

```csharp
// Use StateHelpers for serialization
var serialized = StateHelpers.SerializeState(graphState, indented: true);
var restored = StateHelpers.DeserializeState(serialized);

// Use StateValidator for integrity checks
var validation = StateValidator.ValidateState(graphState);
if (!validation.IsValid)
{
    foreach (var issue in validation.Errors)
    {
        Console.WriteLine($"Error: {issue.Message}");
    }
}

// Use ConditionalExpressionEvaluator
var evaluator = new ConditionalExpressionEvaluator();
var result = evaluator.Evaluate("{{user.role}} == 'admin'", graphState, kernel);
if (result.IsTrue)
{
    Console.WriteLine("User is admin");
}
```

### Advanced Pattern Integration

```csharp
var executor = new GraphExecutor("advanced-graph")
    .WithAcademicPatterns(config => 
    {
        config.EnableCircuitBreaker = true;
        config.EnableBulkhead = true;
        config.EnableCacheAside = true;
    })
    .WithDynamicRouting(config => 
    {
        config.EnableEmbeddingBasedRouting = true;
    })
    .WithPerformanceMetrics(config => 
    {
        config.EnableDetailedMetrics = true;
    })
    .WithRecoverySupport(config => 
    {
        config.EnableAutomaticRecovery = true;
    });
```

### Module Configuration

```csharp
var builder = Kernel.CreateBuilder()
    .AddGraphSupport()
    .AddGraphModules(options => 
    {
        options.EnableStreaming = true;
        options.EnableCheckpointing = true;
        options.EnableRecovery = true;
        options.EnableHumanInTheLoop = true;
        options.EnableMultiAgent = true;
    });

var kernel = builder.Build();
```

## See Also

* [Extensions and Options](./extensions-and-options.md) - Core extension classes and configuration options
* [State and Serialization](./state-and-serialization.md) - State management and serialization utilities
* [Execution Context](./execution-context.md) - Execution context and event utilities
* [GraphExecutor API](./graph-executor.md) - Main executor interface
* [Advanced Patterns](../how-to/advanced-patterns.md) - Advanced pattern usage guides
