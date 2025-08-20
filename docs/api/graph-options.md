# Graph Options and Configuration

SemanticKernel.Graph provides a comprehensive configuration system with modular options for different subsystems. This reference covers the complete options hierarchy including core options, module-specific configurations, and immutability guarantees during execution.

## GraphOptions

The main configuration class for core graph functionality that controls logging, metrics, validation, and execution bounds.

### Constructor

```csharp
public sealed class GraphOptions
{
    // Default constructor with sensible defaults
    public GraphOptions()
}
```

### Core Properties

```csharp
public sealed class GraphOptions
{
    /// <summary>
    /// Gets or sets whether logging is enabled for graph execution.
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets whether metrics collection is enabled.
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of execution steps before termination.
    /// </summary>
    public int MaxExecutionSteps { get; set; } = 1000;

    /// <summary>
    /// Gets or sets whether to validate graph integrity before execution.
    /// </summary>
    public bool ValidateGraphIntegrity { get; set; } = true;

    /// <summary>
    /// Gets or sets the execution timeout.
    /// </summary>
    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Enables compilation and caching of structural execution plans by graph signature.
    /// </summary>
    public bool EnablePlanCompilation { get; set; } = true;

    /// <summary>
    /// Gets or sets the logging configuration for different categories and nodes.
    /// </summary>
    public GraphLoggingOptions Logging { get; set; } = new();

    /// <summary>
    /// Gets or sets interoperability-related options (import/export, bridges, federation).
    /// </summary>
    public GraphInteropOptions Interop { get; set; } = new();
}
```

### Usage Example

```csharp
var options = new GraphOptions
{
    EnableLogging = true,
    EnableMetrics = true,
    MaxExecutionSteps = 500,
    ValidateGraphIntegrity = true,
    ExecutionTimeout = TimeSpan.FromMinutes(5),
    EnablePlanCompilation = true
};

// Configure logging options
options.Logging.MinimumLevel = LogLevel.Debug;
options.Logging.EnableStructuredLogging = true;
options.Logging.EnableCorrelationIds = true;

// Configure interop options
options.Interop.EnableImporters = true;
options.Interop.EnableExporters = true;
options.Interop.EnablePythonBridge = false;
```

## Module Activation Options

### GraphModuleActivationOptions

Configuration flags for conditionally activating optional graph modules via dependency injection.

```csharp
public sealed class GraphModuleActivationOptions
{
    /// <summary>
    /// Enables streaming components (event stream connection pool, reconnection manager).
    /// </summary>
    public bool EnableStreaming { get; set; }

    /// <summary>
    /// Enables checkpointing services and factories.
    /// </summary>
    public bool EnableCheckpointing { get; set; }

    /// <summary>
    /// Enables recovery integration. Effective only when checkpointing is enabled.
    /// </summary>
    public bool EnableRecovery { get; set; }

    /// <summary>
    /// Enables Human-in-the-Loop (registers in-memory store and a Web API backed channel by default).
    /// </summary>
    public bool EnableHumanInTheLoop { get; set; }

    /// <summary>
    /// Enables multi-agent infrastructure (connection pool and options).
    /// </summary>
    public bool EnableMultiAgent { get; set; }

    /// <summary>
    /// Applies environment variable overrides for all flags.
    /// </summary>
    public void ApplyEnvironmentOverrides()
}
```

### Environment Variable Support

The module activation options support environment variable overrides:

```csharp
// Environment variables can override these settings:
// SKG_ENABLE_STREAMING=true
// SKG_ENABLE_CHECKPOINTING=true
// SKG_ENABLE_RECOVERY=true
// SKG_ENABLE_HITL=true
// SKG_ENABLE_MULTIAGENT=true

var options = new GraphModuleActivationOptions();
options.ApplyEnvironmentOverrides();

// Add modules to kernel builder
var builder = Kernel.CreateBuilder()
    .AddGraphModules(options);
```

## Streaming Options

### StreamingExecutionOptions

Configuration options for streaming execution behavior and event handling.

```csharp
public sealed class StreamingExecutionOptions
{
    /// <summary>
    /// Gets or sets the buffer size for event streams.
    /// Default: 100 events.
    /// </summary>
    public int BufferSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the maximum buffer size before backpressure is applied.
    /// Default: 1000 events.
    /// </summary>
    public int MaxBufferSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets whether to enable automatic reconnection on stream interruption.
    /// Default: true.
    /// </summary>
    public bool EnableAutoReconnect { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of reconnection attempts.
    /// Default: 3 attempts.
    /// </summary>
    public int MaxReconnectAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial reconnection delay.
    /// Default: 1 second.
    /// </summary>
    public TimeSpan InitialReconnectDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the maximum reconnection delay (for exponential backoff).
    /// Default: 30 seconds.
    /// </summary>
    public TimeSpan MaxReconnectDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets whether to include intermediate state snapshots in events.
    /// Default: false (to reduce event size).
    /// </summary>
    public bool IncludeStateSnapshots { get; set; } = false;

    /// <summary>
    /// Gets or sets the types of events to emit.
    /// Default: all event types.
    /// </summary>
    public GraphExecutionEventType[]? EventTypesToEmit { get; set; }

    /// <summary>
    /// Gets or sets custom event handlers to attach to the stream.
    /// </summary>
    public List<IGraphExecutionEventHandler> CustomEventHandlers { get; set; } = new();
}
```

### Streaming Configuration Examples

```csharp
// Basic streaming configuration
var basicOptions = new StreamingExecutionOptions
{
    BufferSize = 50,
    MaxBufferSize = 500,
    EnableAutoReconnect = true
};

// High-performance configuration
var performanceOptions = new StreamingExecutionOptions
{
    BufferSize = 1000,
    MaxBufferSize = 10000,
    IncludeStateSnapshots = false,
    EventTypesToEmit = new[] 
    { 
        GraphExecutionEventType.ExecutionStarted,
        GraphExecutionEventType.NodeCompleted,
        GraphExecutionEventType.ExecutionCompleted 
    }
};

// Monitoring configuration
var monitoringOptions = new StreamingExecutionOptions
{
    BufferSize = 100,
    IncludeStateSnapshots = true,
    EventTypesToEmit = new[] 
    { 
        GraphExecutionEventType.NodeStarted,
        GraphExecutionEventType.NodeCompleted,
        GraphExecutionEventType.NodeFailed,
        GraphExecutionEventType.ExecutionCompleted 
    }
};
```

## Checkpointing Options

### CheckpointingOptions

Configuration options for automatic checkpointing behavior and state persistence.

```csharp
public sealed class CheckpointingOptions
{
    /// <summary>
    /// Gets or sets the interval (in number of executed nodes) for creating checkpoints.
    /// </summary>
    public int CheckpointInterval { get; set; } = 10;

    /// <summary>
    /// Gets or sets the optional time interval for creating checkpoints.
    /// </summary>
    public TimeSpan? CheckpointTimeInterval { get; set; }

    /// <summary>
    /// Gets or sets whether to create an initial checkpoint before execution starts.
    /// </summary>
    public bool CreateInitialCheckpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to create a final checkpoint after execution completes.
    /// </summary>
    public bool CreateFinalCheckpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to create checkpoints when errors occur.
    /// </summary>
    public bool CreateErrorCheckpoints { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of critical node IDs that should always trigger checkpoint creation.
    /// </summary>
    public ISet<string> CriticalNodes { get; set; } = new HashSet<string>();

    /// <summary>
    /// Gets or sets whether to enable automatic cleanup of old checkpoints.
    /// </summary>
    public bool EnableAutoCleanup { get; set; } = true;

    /// <summary>
    /// Gets or sets the retention policy for automatic cleanup.
    /// </summary>
    public CheckpointRetentionPolicy RetentionPolicy { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to enable distributed backup of critical checkpoints.
    /// </summary>
    public bool EnableDistributedBackup { get; set; } = false;

    /// <summary>
    /// Gets or sets the backup options for distributed storage.
    /// </summary>
    public DistributedBackupOptions? DistributedBackupOptions { get; set; }
}
```

### Checkpointing Configuration Examples

```csharp
// Frequent checkpointing for critical workflows
var criticalOptions = new CheckpointingOptions
{
    CheckpointInterval = 5,
    CreateInitialCheckpoint = true,
    CreateFinalCheckpoint = true,
    CreateErrorCheckpoints = true,
    CriticalNodes = new HashSet<string> { "decision_node", "approval_node" },
    EnableAutoCleanup = true
};

// Time-based checkpointing
var timeBasedOptions = new CheckpointingOptions
{
    CheckpointTimeInterval = TimeSpan.FromMinutes(5),
    CheckpointInterval = 100, // Fallback to node-based
    CreateInitialCheckpoint = true,
    CreateFinalCheckpoint = true,
    EnableAutoCleanup = true
};

// Minimal checkpointing for performance
var minimalOptions = new CheckpointingOptions
{
    CheckpointInterval = 50,
    CreateInitialCheckpoint = false,
    CreateFinalCheckpoint = true,
    CreateErrorCheckpoints = true,
    EnableAutoCleanup = true
};
```

## Recovery Options

### RecoveryOptions

Configuration options for automatic recovery and replay capabilities.

```csharp
public sealed class RecoveryOptions
{
    /// <summary>
    /// Gets or sets whether to enable automatic recovery on execution failure.
    /// </summary>
    public bool EnableAutomaticRecovery { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of recovery attempts.
    /// </summary>
    public int MaxRecoveryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the recovery strategy to use.
    /// </summary>
    public RecoveryStrategy Strategy { get; set; } = RecoveryStrategy.LastSuccessfulCheckpoint;

    /// <summary>
    /// Gets or sets whether to enable conditional replay for "what-if" scenarios.
    /// </summary>
    public bool EnableConditionalReplay { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum replay depth for conditional scenarios.
    /// </summary>
    public int MaxReplayDepth { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether to preserve execution history during recovery.
    /// </summary>
    public bool PreserveExecutionHistory { get; set; } = true;

    /// <summary>
    /// Gets or sets the recovery timeout.
    /// </summary>
    public TimeSpan RecoveryTimeout { get; set; } = TimeSpan.FromMinutes(5);
}
```

### Recovery Configuration Examples

```csharp
// Aggressive recovery for production systems
var aggressiveRecovery = new RecoveryOptions
{
    EnableAutomaticRecovery = true,
    MaxRecoveryAttempts = 5,
    Strategy = RecoveryStrategy.LastSuccessfulCheckpoint,
    EnableConditionalReplay = false,
    PreserveExecutionHistory = true,
    RecoveryTimeout = TimeSpan.FromMinutes(10)
};

// Conservative recovery for development
var conservativeRecovery = new RecoveryOptions
{
    EnableAutomaticRecovery = false,
    MaxRecoveryAttempts = 1,
    Strategy = RecoveryStrategy.Manual,
    EnableConditionalReplay = true,
    MaxReplayDepth = 5,
    PreserveExecutionHistory = false
};
```

## Human-in-the-Loop (HITL) Options

### HumanApprovalOptions

Configuration options for human approval nodes and interaction behavior.

```csharp
public sealed class HumanApprovalOptions
{
    /// <summary>
    /// Gets or sets the title for the approval request.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description for the approval request.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the instructions for the approver.
    /// </summary>
    public string Instructions { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the required fields for the approval.
    /// </summary>
    public string[] RequiredFields { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the optional fields for the approval.
    /// </summary>
    public string[] OptionalFields { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the number of approvals required.
    /// </summary>
    public int ApprovalThreshold { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of rejections to fail the request.
    /// </summary>
    public int RejectionThreshold { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether to allow partial approval.
    /// </summary>
    public bool AllowPartialApproval { get; set; } = false;
}
```

### WebApiChannelOptions

Configuration options for web-based human interaction channels.

```csharp
public sealed class WebApiChannelOptions
{
    /// <summary>
    /// Gets or sets the endpoint path for the web API.
    /// </summary>
    public string EndpointPath { get; set; } = "/api/approvals";

    /// <summary>
    /// Gets or sets the request timeout for HTTP operations.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the retry policy for failed requests.
    /// </summary>
    public RetryPolicy RetryPolicy { get; set; } = RetryPolicy.ExponentialBackoff(3, TimeSpan.FromSeconds(1));

    /// <summary>
    /// Gets or sets the authentication configuration.
    /// </summary>
    public IAuthenticationConfig? Authentication { get; set; }

    /// <summary>
    /// Gets or sets custom headers to include in requests.
    /// </summary>
    public Dictionary<string, string> CustomHeaders { get; set; } = new();
}
```

### HITL Configuration Examples

```csharp
// Basic approval configuration
var approvalOptions = new HumanApprovalOptions
{
    Title = "Document Review Required",
    Description = "Please review the generated document for accuracy",
    Instructions = "Check grammar, content, and formatting",
    RequiredFields = new[] { "reviewer_name", "approval_notes" },
    ApprovalThreshold = 1,
    RejectionThreshold = 1,
    AllowPartialApproval = false
};

// Web API channel configuration
var webChannelOptions = new WebApiChannelOptions
{
    EndpointPath = "/api/approvals",
    RequestTimeout = TimeSpan.FromSeconds(30),
    RetryPolicy = RetryPolicy.ExponentialBackoff(3, TimeSpan.FromSeconds(1)),
    CustomHeaders = new Dictionary<string, string>
    {
        ["X-Client-Version"] = "1.0.0",
        ["X-Environment"] = "production"
    }
};
```

## Multi-Agent Options

### MultiAgentOptions

Configuration options for multi-agent coordination and workflow management.

```csharp
public sealed class MultiAgentOptions
{
    /// <summary>
    /// Gets or sets the maximum number of concurrent agents.
    /// </summary>
    public int MaxConcurrentAgents { get; set; } = 10;

    /// <summary>
    /// Gets or sets the shared state management options.
    /// </summary>
    public SharedStateOptions SharedStateOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the work distribution options.
    /// </summary>
    public WorkDistributionOptions WorkDistributionOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the result aggregation options.
    /// </summary>
    public ResultAggregationOptions ResultAggregationOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the health monitoring options.
    /// </summary>
    public HealthMonitoringOptions HealthMonitoringOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the coordination timeout.
    /// </summary>
    public TimeSpan CoordinationTimeout { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets or sets whether to enable automatic cleanup of completed workflows.
    /// </summary>
    public bool EnableAutomaticCleanup { get; set; } = true;

    /// <summary>
    /// Gets or sets the workflow retention period.
    /// </summary>
    public TimeSpan WorkflowRetentionPeriod { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Gets or sets whether to enable distributed tracing (ActivitySource) for multi-agent workflows.
    /// </summary>
    public bool EnableDistributedTracing { get; set; } = true;
}
```

### AgentConnectionPoolOptions

Configuration options for managing agent connection pools.

```csharp
public sealed class AgentConnectionPoolOptions
{
    /// <summary>
    /// Gets or sets the maximum number of connections in the pool.
    /// </summary>
    public int MaxConnections { get; set; } = 100;

    /// <summary>
    /// Gets or sets the minimum number of connections to maintain.
    /// </summary>
    public int MinConnections { get; set; } = 10;

    /// <summary>
    /// Gets or sets the connection timeout.
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the connection lifetime.
    /// </summary>
    public TimeSpan ConnectionLifetime { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets whether to enable connection health checks.
    /// </summary>
    public bool EnableHealthChecks { get; set; } = true;

    /// <summary>
    /// Gets or sets the health check interval.
    /// </summary>
    public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromSeconds(30);
}
```

### Multi-Agent Configuration Examples

```csharp
// High-performance multi-agent configuration
var highPerfOptions = new MultiAgentOptions
{
    MaxConcurrentAgents = 50,
    CoordinationTimeout = TimeSpan.FromMinutes(15),
    EnableAutomaticCleanup = true,
    WorkflowRetentionPeriod = TimeSpan.FromHours(12),
    EnableDistributedTracing = true
};

// Conservative multi-agent configuration
var conservativeOptions = new MultiAgentOptions
{
    MaxConcurrentAgents = 5,
    CoordinationTimeout = TimeSpan.FromMinutes(60),
    EnableAutomaticCleanup = false,
    WorkflowRetentionPeriod = TimeSpan.FromDays(7),
    EnableDistributedTracing = false
};

// Connection pool configuration
var poolOptions = new AgentConnectionPoolOptions
{
    MaxConnections = 200,
    MinConnections = 20,
    ConnectionTimeout = TimeSpan.FromSeconds(15),
    ConnectionLifetime = TimeSpan.FromMinutes(10),
    EnableHealthChecks = true,
    HealthCheckInterval = TimeSpan.FromSeconds(15)
};
```

## Logging Options

### GraphLoggingOptions

Advanced logging configuration options for graph execution.

```csharp
public sealed class GraphLoggingOptions
{
    /// <summary>
    /// Gets or sets the minimum log level for graph execution.
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets whether to enable structured logging.
    /// </summary>
    public bool EnableStructuredLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable correlation IDs.
    /// </summary>
    public bool EnableCorrelationIds { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum size of state data to log.
    /// </summary>
    public int MaxStateDataSize { get; set; } = 2000;

    /// <summary>
    /// Gets or sets category-specific logging configurations.
    /// Categories include: "Graph", "Node", "Routing", "Error", "Performance", "State", "Validation".
    /// </summary>
    public Dictionary<string, LogCategoryConfig> CategoryConfigs { get; set; } = new();

    /// <summary>
    /// Gets or sets node-specific logging configurations.
    /// Key is the node ID or node type name.
    /// </summary>
    public Dictionary<string, NodeLoggingConfig> NodeConfigs { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to log sensitive data (parameters, state values).
    /// When false, only parameter names and counts are logged, not values.
    /// </summary>
    public bool LogSensitiveData { get; set; } = false;

    /// <summary>
    /// Gets or sets the sanitization policy for sensitive data.
    /// </summary>
    public SensitiveDataPolicy Sanitization { get; set; } = SensitiveDataPolicy.Default;

    /// <summary>
    /// Gets or sets custom correlation ID prefix for this graph instance.
    /// </summary>
    public string? CorrelationIdPrefix { get; set; }

    /// <summary>
    /// Gets or sets the format for timestamps in log entries.
    /// </summary>
    public string TimestampFormat { get; set; } = "yyyy-MM-dd HH:mm:ss.fff";
}
```

### Logging Configuration Examples

```csharp
// Verbose logging for development
var verboseLogging = new GraphLoggingOptions
{
    MinimumLevel = LogLevel.Debug,
    EnableStructuredLogging = true,
    EnableCorrelationIds = true,
    LogSensitiveData = true,
    MaxStateDataSize = 5000
};

// Production logging with sanitization
var productionLogging = new GraphLoggingOptions
{
    MinimumLevel = LogLevel.Information,
    EnableStructuredLogging = true,
    EnableCorrelationIds = true,
    LogSensitiveData = false,
    Sanitization = SensitiveDataPolicy.Strict,
    MaxStateDataSize = 1000
};

// Category-specific logging
var categoryLogging = new GraphLoggingOptions();
categoryLogging.CategoryConfigs["Performance"] = new LogCategoryConfig 
{ 
    Level = LogLevel.Debug, 
    Enabled = true 
};
categoryLogging.CategoryConfigs["State"] = new LogCategoryConfig 
{ 
    Level = LogLevel.Warning, 
    Enabled = false 
};
```

## Interoperability Options

### GraphInteropOptions

Configuration options for cross-ecosystem integrations and external system bridges.

```csharp
public sealed class GraphInteropOptions
{
    /// <summary>
    /// Enables importers that convert external graph definitions (e.g., LangGraph/LangChain JSON) into GraphExecutor instances.
    /// </summary>
    public bool EnableImporters { get; set; } = true;

    /// <summary>
    /// Enables exporters for industry formats (e.g., BPMN XML).
    /// </summary>
    public bool EnableExporters { get; set; } = true;

    /// <summary>
    /// Enables Python execution bridge nodes.
    /// </summary>
    public bool EnablePythonBridge { get; set; } = false;

    /// <summary>
    /// Enables federation with external graph engines via HTTP.
    /// </summary>
    public bool EnableFederation { get; set; } = true;

    /// <summary>
    /// Optional path to Python executable for the Python bridge. If null or empty, "python" will be used from PATH.
    /// </summary>
    public string? PythonExecutablePath { get; set; }

    /// <summary>
    /// Optional default base address for federated graph calls (e.g., an upstream LangGraph server).
    /// </summary>
    public string? FederationBaseAddress { get; set; }

    /// <summary>
    /// Options for replay/export security such as hashing and encryption.
    /// </summary>
    public ReplaySecurityOptions ReplaySecurity { get; set; } = new();
}
```

## Complete Configuration

### CompleteGraphOptions

Convenience aggregate configuration for all graph features.

```csharp
public sealed class CompleteGraphOptions
{
    /// <summary>
    /// Gets or sets whether logging is enabled.
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets whether memory integration is enabled.
    /// </summary>
    public bool EnableMemory { get; set; } = true;

    /// <summary>
    /// Gets or sets whether template support is enabled.
    /// </summary>
    public bool EnableTemplates { get; set; } = true;

    /// <summary>
    /// Gets or sets whether vector search is enabled.
    /// </summary>
    public bool EnableVectorSearch { get; set; } = true;

    /// <summary>
    /// Gets or sets whether semantic search is enabled.
    /// </summary>
    public bool EnableSemanticSearch { get; set; } = true;

    /// <summary>
    /// Gets or sets whether custom template helpers are enabled.
    /// </summary>
    public bool EnableCustomHelpers { get; set; } = true;

    /// <summary>
    /// Gets or sets whether metrics collection is enabled.
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum execution steps.
    /// </summary>
    public int MaxExecutionSteps { get; set; } = 1000;

    /// <summary>
    /// Gets or sets interop (import/export) options.
    /// </summary>
    public GraphInteropOptions Interop { get; set; } = new();
}
```

## Immutability and Execution

### Immutability Guarantees

All configuration options are immutable once execution begins. This ensures:

* **Consistent Behavior**: Configuration cannot change during execution
* **Thread Safety**: Multiple executions can run with different configurations
* **Predictable Performance**: No runtime configuration overhead
* **Debugging Clarity**: Configuration state is frozen for analysis

### Configuration Per Execution

```csharp
// Each execution gets its own configuration snapshot
var executor1 = new GraphExecutor("graph1", new GraphOptions 
{ 
    MaxExecutionSteps = 100,
    EnableLogging = true 
});

var executor2 = new GraphExecutor("graph2", new GraphOptions 
{ 
    MaxExecutionSteps = 1000,
    EnableLogging = false 
});

// Executions run independently with their configurations
var result1 = await executor1.ExecuteAsync(kernel, arguments1);
var result2 = await executor2.ExecuteAsync(kernel, arguments2);
```

### Runtime Configuration Validation

```csharp
// Configuration is validated at execution start
var options = new GraphOptions
{
    MaxExecutionSteps = -1, // Invalid
    ExecutionTimeout = TimeSpan.Zero // Invalid
};

try
{
    var executor = new GraphExecutor("graph", options);
    var result = await executor.ExecuteAsync(kernel, arguments);
}
catch (ArgumentException ex)
{
    // Configuration validation failed
    Console.WriteLine($"Configuration error: {ex.Message}");
}
```

## Usage Patterns

### Builder Pattern Configuration

```csharp
var options = new GraphOptions()
    .WithLogging(LogLevel.Debug, enableStructured: true)
    .WithMetrics(enabled: true)
    .WithExecutionLimits(maxSteps: 500, timeout: TimeSpan.FromMinutes(5))
    .WithValidation(validateIntegrity: true, enablePlanCompilation: true)
    .WithInterop(enableImporters: true, enableExporters: true);
```

### Environment-Based Configuration

```csharp
// Load configuration from environment variables
var options = new GraphOptions();
options.LoadFromEnvironment();

// Environment variables:
// SKG_ENABLE_LOGGING=true
// SKG_MAX_EXECUTION_STEPS=1000
// SKG_EXECUTION_TIMEOUT=00:10:00
// SKG_ENABLE_METRICS=true
```

### Configuration Inheritance

```csharp
// Base configuration
var baseOptions = new GraphOptions
{
    EnableLogging = true,
    EnableMetrics = true,
    MaxExecutionSteps = 1000
};

// Specialized configuration
var specializedOptions = new GraphOptions
{
    EnableLogging = baseOptions.EnableLogging,
    EnableMetrics = baseOptions.EnableMetrics,
    MaxExecutionSteps = 500, // Override
    ExecutionTimeout = TimeSpan.FromMinutes(2) // Add
};
```

## Performance Considerations

* **Configuration Validation**: Happens once at executor creation, not during execution
* **Option Access**: Direct property access with no indirection overhead
* **Memory Usage**: Minimal memory footprint for configuration objects
* **Serialization**: Options can be serialized for configuration persistence
* **Caching**: Frequently used configurations can be cached and reused

## Security Considerations

* **Sensitive Data**: Use `LogSensitiveData = false` in production
* **Sanitization**: Configure appropriate sanitization policies
* **Validation**: Always validate configuration before use
* **Environment Variables**: Secure environment variable access
* **Configuration Files**: Protect configuration files with appropriate permissions

## See Also

* [Core API Reference](core.md) - Core graph execution APIs
* [Extensions and Options](extensions-and-options.md) - Additional configuration options
* [Module Activation](../how-to/module-activation.md) - How to enable optional modules
* [Configuration Best Practices](../how-to/configuration-best-practices.md) - Configuration guidelines
* [Performance Tuning](../how-to/performance-tuning.md) - Performance optimization
