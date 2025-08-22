# Executors and Middlewares

This reference covers the specialized executor implementations and middleware system that provide advanced execution capabilities in SemanticKernel.Graph.

## Overview

The SemanticKernel.Graph library implements a layered executor architecture using the decorator pattern, where specialized executors wrap the core `GraphExecutor` to add specific functionality. This design allows for composable execution features while maintaining a clean separation of concerns.

## Executor Architecture

### Core Executor Layer

The base `GraphExecutor` provides the fundamental execution engine, while specialized executors add specific capabilities:

```
┌─────────────────────────────────────────────────────────────┐
│                    Specialized Executors                    │
├─────────────────────────────────────────────────────────────┤
│  CheckpointingGraphExecutor  │  StreamingGraphExecutor     │
│  (State persistence)         │  (Real-time events)         │
├─────────────────────────────────────────────────────────────┤
│                    Core GraphExecutor                       │
│              (Execution engine + middleware)               │
├─────────────────────────────────────────────────────────────┤
│                    IGraphExecutor                           │
│                    (Interface contract)                     │
└─────────────────────────────────────────────────────────────┘
```

## CheckpointingGraphExecutor

A specialized executor that adds automatic checkpointing and state persistence capabilities to graph execution.

### Core Features

```csharp
public class CheckpointingGraphExecutor : IGraphExecutor
{
    // Checkpoint management
    public virtual ICheckpointManager CheckpointManager { get; }
    public CheckpointingOptions Options { get; }
    
    // Recovery integration
    public virtual GraphRecoveryService? RecoveryService { get; set; }
    
    // Execution statistics
    public string? LastExecutionId { get; }
}
```

### Checkpointing Options

```csharp
public sealed class CheckpointingOptions
{
    // Automatic checkpointing
    public bool EnableAutomaticCheckpointing { get; set; } = true;
    public int CheckpointInterval { get; set; } = 10; // Every N nodes
    
    // Manual checkpointing
    public bool EnableManualCheckpointing { get; set; } = true;
    public bool EnableCheckpointMetadata { get; set; } = true;
    
    // Storage options
    public bool EnableCompression { get; set; } = true;
    public bool EnableEncryption { get; set; } = false;
    public TimeSpan? CheckpointRetention { get; set; }
}
```

### Usage Example

```csharp
// Create checkpointing executor
var checkpointManager = new MemoryCheckpointManager();
var executor = new CheckpointingGraphExecutor(
    "my-graph",
    checkpointManager,
    new CheckpointingOptions 
    { 
        CheckpointInterval = 5,
        EnableCompression = true 
    }
);

// Configure automatic recovery
var recoveryService = executor.ConfigureRecovery(new RecoveryOptions
{
    EnableAutomaticRecovery = true,
    MaxRecoveryAttempts = 3
});

// Execute with automatic checkpointing
var result = await executor.ExecuteAsync(kernel, arguments);

// Manual checkpoint creation
var checkpointId = await executor.CheckpointManager.CreateCheckpointAsync(
    "manual-checkpoint",
    new Dictionary<string, object> { ["reason"] = "before_risky_operation" }
);
```

### Checkpointing Lifecycle

The executor automatically creates checkpoints during execution:

1. **Before Execution**: Creates initial checkpoint if enabled
2. **During Execution**: Creates checkpoints at configured intervals
3. **After Node Execution**: Creates checkpoints based on node completion
4. **On Recovery**: Restores from last available checkpoint

## StreamingGraphExecutor

A specialized executor that provides real-time event streaming during graph execution for monitoring and integration purposes.

### Core Features

```csharp
public sealed class StreamingGraphExecutor : IStreamingGraphExecutor, IDisposable
{
    // Streaming capabilities
    public async Task<IGraphExecutionEventStream> ExecuteStreamingAsync(
        Kernel kernel, 
        KernelArguments arguments, 
        CancellationToken cancellationToken = default)
    
    // Event stream management
    public IReadOnlyDictionary<string, GraphExecutionEventStream> ActiveStreams { get; }
    
    // Disposal
    public void Dispose();
}
```

### Event Stream Types

The streaming executor emits various event types during execution:

```csharp
// Execution lifecycle events
public class GraphExecutionStartedEvent : GraphExecutionEvent
public class GraphExecutionCompletedEvent : GraphExecutionEvent
public class GraphExecutionFailedEvent : GraphExecutionEvent

// Node execution events
public class NodeExecutionStartedEvent : GraphExecutionEvent
public class NodeExecutionCompletedEvent : GraphExecutionEvent
public class NodeExecutionFailedEvent : GraphExecutionEvent

// State change events
public class StateChangedEvent : GraphExecutionEvent
public class CheckpointCreatedEvent : GraphExecutionEvent
```

### Usage Example

```csharp
// Create streaming executor
var executor = new StreamingGraphExecutor("streaming-graph");

// Execute with streaming
var eventStream = await executor.ExecuteStreamingAsync(kernel, arguments);

// Consume events in real-time
await foreach (var evt in eventStream.WithCancellation(cancellationToken))
{
    switch (evt)
    {
        case NodeExecutionStartedEvent started:
            Console.WriteLine($"Node {started.NodeId} started");
            break;
            
        case NodeExecutionCompletedEvent completed:
            Console.WriteLine($"Node {completed.NodeId} completed in {completed.Duration}");
            break;
            
        case StateChangedEvent stateChange:
            Console.WriteLine($"State changed: {stateChange.ChangedProperties.Count} properties");
            break;
    }
}
```

### Event Stream Configuration

```csharp
public sealed class StreamingExecutionOptions
{
    // Event filtering
    public bool EnableNodeEvents { get; set; } = true;
    public bool EnableStateEvents { get; set; } = true;
    public bool EnableCheckpointEvents { get; set; } = true;
    
    // Performance options
    public int EventBufferSize { get; set; } = 1000;
    public bool EnableEventCompression { get; set; } = false;
    public TimeSpan EventTimeout { get; set; } = TimeSpan.FromMinutes(5);
}
```

## GraphRecoveryService

A service that provides automatic failure detection and recovery management for graph execution.

### Core Features

```csharp
public sealed class GraphRecoveryService : IDisposable
{
    // Recovery management
    public async Task<RecoveryResult> AttemptRecoveryAsync(
        string executionId, 
        FailureContext failureContext, 
        Kernel kernel, 
        CancellationToken cancellationToken)
    
    // Health monitoring
    public event EventHandler<BudgetExhaustedEventArgs>? BudgetExhausted;
    public long BudgetExhaustionCount { get; }
    public DateTimeOffset LastBudgetExhaustion { get; }
    
    // Session management
    public IReadOnlyDictionary<string, RecoverySession> ActiveSessions { get; }
}
```

### Recovery Options

```csharp
public sealed class RecoveryOptions
{
    // Automatic recovery
    public bool EnableAutomaticRecovery { get; set; } = true;
    public int MaxRecoveryAttempts { get; set; } = 3;
    public TimeSpan RecoveryTimeout { get; set; } = TimeSpan.FromMinutes(10);
    
    // Rollback strategies
    public bool EnableAutomaticRollback { get; set; } = true;
    public RollbackStrategy RollbackStrategy { get; set; } = RollbackStrategy.LastCheckpoint;
    
    // Notification
    public bool EnableNotifications { get; set; } = true;
    public TimeSpan NotificationTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
```

### Recovery Strategies

```csharp
public enum RollbackStrategy
{
    LastCheckpoint,      // Rollback to last successful checkpoint
    SpecificCheckpoint,  // Rollback to specified checkpoint
    PartialRollback,     // Rollback only failed nodes
    FullRestart          // Restart entire execution
}

public enum RecoveryStrategy
{
    Automatic,           // Automatic recovery using policies
    Manual,              // Manual intervention required
    Hybrid,              // Automatic with manual approval
    Disabled            // No recovery attempted
}
```

### Usage Example

```csharp
// Configure recovery service
var recoveryService = new GraphRecoveryService(
    checkpointManager,
    executor,
    new RecoveryOptions
    {
        EnableAutomaticRecovery = true,
        MaxRecoveryAttempts = 3,
        RollbackStrategy = RollbackStrategy.LastCheckpoint
    }
);

// Add notification handlers
recoveryService.AddNotificationHandler(new LoggingRecoveryNotificationHandler(logger));
recoveryService.AddNotificationHandler(new EmailRecoveryNotificationHandler(emailService));

// Execute with recovery
try
{
    var result = await executor.ExecuteAsync(kernel, arguments);
}
catch (Exception ex)
{
    // Automatic recovery will be attempted
    var recoveryResult = await recoveryService.AttemptRecoveryAsync(
        executionId, 
        new FailureContext(ex), 
        kernel, 
        cancellationToken);
        
    if (recoveryResult.IsSuccessful)
    {
        Console.WriteLine("Recovery successful!");
    }
}
```

## ResourceGovernor

A lightweight in-process resource governor that provides adaptive rate limiting and cooperative scheduling based on CPU/memory usage and execution priority.

### Core Features

```csharp
public sealed class ResourceGovernor : IDisposable
{
    // Resource acquisition
    public async Task<IResourceLease> AcquireLeaseAsync(
        int cost, 
        ExecutionPriority priority, 
        CancellationToken cancellationToken = default)
    
    // Rate limiting
    public double CurrentPermitsPerSecond { get; }
    public int AvailableBurst { get; }
    
    // Budget monitoring
    public event EventHandler<BudgetExhaustedEventArgs>? BudgetExhausted;
    public long BudgetExhaustionCount { get; }
    public DateTimeOffset LastBudgetExhaustion { get; }
}
```

### Resource Options

```csharp
public sealed class GraphResourceOptions
{
    // Resource governance
    public bool EnableResourceGovernance { get; set; } = false;
    
    // Rate limiting
    public double BasePermitsPerSecond { get; set; } = 100.0;
    public int MaxBurstSize { get; set; } = 50;
    
    // Priority scheduling
    public bool EnablePriorityScheduling { get; set; } = true;
    public TimeSpan PriorityTimeout { get; set; } = TimeSpan.FromMinutes(5);
    
    // Adaptive throttling
    public bool EnableAdaptiveThrottling { get; set; } = true;
    public double ThrottlingThreshold { get; set; } = 0.8; // 80% resource usage
}
```

### Execution Priorities

```csharp
public enum ExecutionPriority
{
    Critical = 0,    // Highest priority, immediate execution
    High = 1,        // High priority, minimal delay
    Normal = 2,      // Normal priority, standard scheduling
    Low = 3,         // Low priority, may be delayed
    Background = 4   // Background priority, lowest priority
}
```

### Usage Example

```csharp
// Configure resource governance
var resourceOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 50.0,
    MaxBurstSize = 25,
    EnablePriorityScheduling = true
};

var executor = new GraphExecutor("governed-graph")
    .ConfigureResources(resourceOptions);

// Execute with resource constraints
var result = await executor.ExecuteAsync(kernel, arguments);

// The ResourceGovernor will automatically:
// - Limit concurrent executions based on permits
// - Schedule work based on priority
// - Adapt throttling based on system load
// - Emit events when budget is exhausted
```

## Middleware Pipeline

The executor system supports a configurable middleware pipeline that allows custom logic to be injected at various points during execution.

### Middleware Interface

```csharp
public interface IGraphExecutionMiddleware
{
    // Execution order (lower values run earlier)
    int Order { get; }
    
    // Lifecycle hooks
    Task OnBeforeNodeAsync(GraphExecutionContext context, IGraphNode node, CancellationToken cancellationToken);
    Task OnAfterNodeAsync(GraphExecutionContext context, IGraphNode node, FunctionResult result, CancellationToken cancellationToken);
    Task OnNodeFailedAsync(GraphExecutionContext context, IGraphNode node, Exception exception, CancellationToken cancellationToken);
}
```

### Middleware Execution Order

The middleware pipeline executes in the following order:

1. **Before Node Execution**: Middlewares execute in ascending `Order` value
2. **Node Execution**: The actual node executes
3. **After Node Execution**: Middlewares execute in descending `Order` value
4. **On Failure**: Middlewares execute in descending `Order` value

### Built-in Middlewares

```csharp
// Performance monitoring middleware
public class PerformanceMonitoringMiddleware : IGraphExecutionMiddleware
{
    public int Order => 100;

    public Task OnBeforeNodeAsync(GraphExecutionContext context, IGraphNode node, CancellationToken cancellationToken)
    {
        // Record start timestamp in the execution context properties
        context.SetProperty($"node:{node.NodeId}:start", DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }

    public Task OnAfterNodeAsync(GraphExecutionContext context, IGraphNode node, FunctionResult result, CancellationToken cancellationToken)
    {
        // Compute elapsed time using the stored start timestamp
        var startObj = context.GetProperty<object>($"node:{node.NodeId}:start");
        if (startObj is DateTimeOffset start)
        {
            var elapsed = DateTimeOffset.UtcNow - start;
            Console.WriteLine($"[PERF] Node {node.NodeId} completed in {elapsed.TotalMilliseconds}ms");
        }
        return Task.CompletedTask;
    }
}

// Logging middleware
public class LoggingMiddleware : IGraphExecutionMiddleware
{
    public int Order => 200;

    public Task OnBeforeNodeAsync(GraphExecutionContext context, IGraphNode node, CancellationToken cancellationToken)
    {
        // Simple console logging for examples and documentation
        Console.WriteLine($"[LOG] Starting node {node.NodeId}");
        return Task.CompletedTask;
    }

    public Task OnAfterNodeAsync(GraphExecutionContext context, IGraphNode node, FunctionResult result, CancellationToken cancellationToken)
    {
        // Print a concise completion message and include the result value when available
        var value = "<unavailable>";
        try { value = result.GetValue<object>()?.ToString() ?? "null"; } catch { }
        Console.WriteLine($"[LOG] Completed node {node.NodeId} with result: {value}");
        return Task.CompletedTask;
    }
}
```

### Custom Middleware Example

```csharp
// Custom validation middleware
public class ValidationMiddleware : IGraphExecutionMiddleware
{
    public int Order => 50; // Run early in the pipeline

    public Task OnBeforeNodeAsync(GraphExecutionContext context, IGraphNode node, CancellationToken cancellationToken)
    {
        // Use the node's built-in validation against KernelArguments
        var validationResult = node.ValidateExecution(context.GraphState.KernelArguments);
        if (!validationResult.IsValid)
        {
            throw new ValidationException($"Node {node.NodeId} validation failed: {validationResult.CreateSummary()}");
        }
        return Task.CompletedTask;
    }

    public Task OnAfterNodeAsync(GraphExecutionContext context, IGraphNode node, FunctionResult result, CancellationToken cancellationToken)
    {
        // Basic output sanity check: prefer node-specific validation in real code
        try
        {
            var value = result.GetValue<object>();
            if (value == null)
            {
                Console.WriteLine($"[WARN] Node {node.NodeId} produced null result");
            }
        }
        catch
        {
            Console.WriteLine($"[WARN] Unable to inspect result for node {node.NodeId}");
        }
        return Task.CompletedTask;
    }

    public Task OnNodeFailedAsync(GraphExecutionContext context, IGraphNode node, Exception exception, CancellationToken cancellationToken)
    {
        // Log validation-related failures to console for examples
        if (exception is ValidationException)
        {
            Console.WriteLine($"[ERROR] Validation failure in node {node.NodeId}: {exception.Message}");
        }
        return Task.CompletedTask;
    }
}

// Add middleware to executor
var executor = new GraphExecutor("validated-graph")
    .UseMiddleware(new ValidationMiddleware())
    .UseMiddleware(new PerformanceMonitoringMiddleware())
    .UseMiddleware(new LoggingMiddleware());
```

## Integration Patterns

### Combining Multiple Executors

```csharp
// Create a checkpointing executor with streaming capabilities
var baseExecutor = new GraphExecutor("base-graph");
var checkpointingExecutor = new CheckpointingGraphExecutor("checkpointing-graph", checkpointManager);
var streamingExecutor = new StreamingGraphExecutor("streaming-graph");

// Configure recovery service
var recoveryService = checkpointingExecutor.ConfigureRecovery(new RecoveryOptions
{
    EnableAutomaticRecovery = true,
    MaxRecoveryAttempts = 3
});

// Add middleware for cross-cutting concerns
baseExecutor.UseMiddleware(new PerformanceMonitoringMiddleware())
    .UseMiddleware(new LoggingMiddleware())
    .UseMiddleware(new ValidationMiddleware());

// Execute with full capabilities
var result = await checkpointingExecutor.ExecuteAsync(kernel, arguments);
```

### Resource Governance Integration

```csharp
// Configure resource governance across all executors
var resourceOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 100.0,
    MaxBurstSize = 50
};

var baseExecutor = new GraphExecutor("governed-graph")
    .ConfigureResources(resourceOptions);

var checkpointingExecutor = new CheckpointingGraphExecutor("checkpointing-graph", checkpointManager)
    .ConfigureRecovery(new RecoveryOptions { EnableAutomaticRecovery = true });

// Resource governance will be applied to all executions
var result = await checkpointingExecutor.ExecuteAsync(kernel, arguments);
```

## Configuration and Options

### Environment-Based Configuration

```csharp
// Environment variables can control executor behavior
// SKG_ENABLE_CHECKPOINTING=true
// SKG_ENABLE_STREAMING=true
// SKG_ENABLE_RECOVERY=true
// SKG_ENABLE_RESOURCE_GOVERNANCE=true

var builder = Kernel.CreateBuilder()
    .AddGraphModules(options => 
    {
        options.EnableCheckpointing = true;
        options.EnableStreaming = true;
        options.EnableRecovery = true;
        options.EnableMultiAgent = false;
    });
```

### Dependency Injection Integration

```csharp
// Register executors and services
services.AddSingleton<ICheckpointManager, FileCheckpointManager>();
services.AddSingleton<GraphRecoveryService>();
services.AddSingleton<ResourceGovernor>();

// Register middleware
services.AddTransient<ValidationMiddleware>();
services.AddTransient<PerformanceMonitoringMiddleware>();
services.AddTransient<LoggingMiddleware>();
```

## See Also

* [GraphExecutor API](./graph-executor.md) - Core executor interface and implementation
* [Execution Context](./execution-context.md) - Execution context and event utilities
* [State and Serialization](./state-and-serialization.md) - State management and checkpointing
* [Streaming Execution](../concepts/streaming.md) - Streaming execution concepts
* [Error Handling and Resilience](../how-to/error-handling-and-resilience.md) - Error handling patterns
* [Resource Governance and Concurrency](../how-to/resource-governance-and-concurrency.md) - Resource management guides
