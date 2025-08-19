# Integration APIs Reference

This reference documents the integration APIs in SemanticKernel.Graph, which provide logging, registry management, data sanitization, and auxiliary services for production-ready graph applications.

## SemanticKernelGraphLogger

Implementation of `IGraphLogger` that integrates with Microsoft.Extensions.Logging and provides structured logging for graph execution events with advanced configuration options.

### Features

- Integration with Microsoft.Extensions.Logging
- Structured logging with correlation IDs
- Configurable data sanitization
- Category and node-specific logging
- Performance timing integration
- State change tracking
- Exception logging with context

### Constructors

```csharp
public SemanticKernelGraphLogger(ILogger? logger, GraphOptions options)
```

### Methods

#### Graph Execution Logging

```csharp
public void LogGraphExecutionStarted(string graphId, string graphName, string executionId, GraphState initialState)
public void LogGraphExecutionCompleted(string graphId, string executionId, GraphState finalState, TimeSpan totalExecutionTime)
public void LogGraphExecutionFailed(string graphId, string executionId, Exception exception, GraphState stateAtFailure)
```

#### Node Execution Logging

```csharp
public void LogNodeExecution(string nodeId, string executionId, NodeExecutionInfo nodeInfo)
public void LogNodeExecutionStarted(string nodeId, string executionId, string nodeName, string nodeType)
public void LogNodeExecutionCompleted(string nodeId, string executionId, object? outputResult, TimeSpan executionTime)
public void LogNodeExecutionFailed(string nodeId, string executionId, Exception exception, TimeSpan executionTime)
```

#### State and Context Logging

```csharp
public void LogStateChange(string executionId, StateChangeInfo stateChange)
public void LogCorrelatedEvent(string executionId, string category, string message, LogLevel level, object? data = null)
public IDisposable BeginExecutionScope(string executionId, string graphId)
```

#### Utility Methods

```csharp
public bool IsEnabled(string category, LogLevel level)
public NodeLoggingConfig GetNodeConfig(string nodeId, string nodeType)
```

### Properties

- `IsDisposed`: Gets whether the logger has been disposed

## IGraphRegistry

Registry interface for managing named `GraphExecutor` instances to enable remote execution and discovery.

### Features

- Graph registration and lifecycle management
- Thread-safe concurrent access
- Remote execution support
- Discovery and metadata access

### Methods

```csharp
Task<bool> RegisterAsync(GraphExecutor executor)
Task<bool> UnregisterAsync(string graphName)
Task<GraphExecutor?> GetAsync(string graphName)
Task<IList<RegisteredGraphInfo>> ListAsync()
Task<bool> ExistsAsync(string graphName)
Task<int> GetRegisteredCountAsync()
```

### GraphRegistry Implementation

Default in-memory implementation of `IGraphRegistry` suitable for single-process hosting scenarios.

#### Constructors

```csharp
public GraphRegistry(ILogger<GraphRegistry>? logger = null)
```

#### Additional Methods

```csharp
public Task<RegisteredGraphInfo?> GetInfoAsync(string graphName)
public Task<IList<RegisteredGraphInfo>> GetInfosAsync()
public Task<bool> ClearAsync()
```

## IToolRegistry

Registry interface for external tools that can be exposed as graph nodes.

### Features

- Tool registration with metadata
- Factory-based node creation
- Search and discovery capabilities
- Lifecycle management

### Methods

```csharp
Task<bool> RegisterAsync(ToolMetadata metadata, Func<IServiceProvider, IGraphNode> factory)
Task<bool> UnregisterAsync(string toolId)
Task<ToolMetadata?> GetAsync(string toolId)
Task<IGraphNode?> CreateNodeAsync(string toolId, IServiceProvider serviceProvider)
Task<IList<ToolMetadata>> SearchAsync(ToolSearchCriteria criteria)
Task<IList<ToolMetadata>> ListAsync()
```

### ToolRegistry Implementation

In-memory implementation of `IToolRegistry` with thread-safe operations.

#### Constructors

```csharp
public ToolRegistry(ILogger<ToolRegistry>? logger = null)
```

## IPluginRegistry

Interface for plugin registry operations providing discovery, registration, and lifecycle management.

### Features

- Plugin registration with metadata
- Factory-based instantiation
- Search and filtering capabilities
- Statistics and monitoring

### Methods

```csharp
Task<PluginRegistrationResult> RegisterPluginAsync(PluginMetadata metadata, Func<IServiceProvider, IGraphNode> factory)
Task<bool> UnregisterPluginAsync(string pluginId)
Task<PluginMetadata?> GetPluginMetadataAsync(string pluginId)
Task<IGraphNode?> CreatePluginInstanceAsync(string pluginId, IServiceProvider serviceProvider)
Task<IList<PluginMetadata>> SearchPluginsAsync(PluginSearchCriteria criteria)
Task<IList<PluginMetadata>> GetAllPluginsAsync()
Task<PluginStatistics?> GetPluginStatisticsAsync(string pluginId)
```

### PluginRegistry Implementation

Thread-safe in-memory implementation with periodic cleanup and statistics tracking.

#### Constructors

```csharp
public PluginRegistry(PluginRegistryOptions? options = null, ILogger<PluginRegistry>? logger = null)
```

## SensitiveDataSanitizer

Utility for sanitizing sensitive data in logs, events, and exports with configurable policies.

### Features

- Automatic sensitive key detection
- Configurable sanitization levels
- JSON element handling
- Authorization token masking
- Custom redaction text

### Constructors

```csharp
public SensitiveDataSanitizer(SensitiveDataPolicy policy)
```

### Methods

#### Data Sanitization

```csharp
public object? Sanitize(object? value, string? keyHint = null)
public IDictionary<string, object?> Sanitize(IDictionary<string, object?> data)
public IDictionary<string, object?> Sanitize(IReadOnlyDictionary<string, object?> data)
```

#### Utility Methods

```csharp
public bool IsSensitiveKey(string key)
public string GetRedactionText()
```

## SensitiveDataPolicy

Configuration policy for sensitive data sanitization in logs and exports.

### Properties

```csharp
public bool Enabled { get; set; } = true                                    // Enable sanitization
public SanitizationLevel Level { get; set; } = SanitizationLevel.Basic      // Sanitization aggressiveness
public string RedactionText { get; set; } = "***REDACTED***"               // Replacement text
public string[] KeySubstrings { get; set; } = DefaultKeySubstrings         // Sensitive key patterns
public bool MaskAuthorizationBearerToken { get; set; } = true               // Mask auth tokens
public bool PreserveKeyNames { get; set; } = true                          // Keep key names visible
```

### Static Properties

```csharp
public static string[] DefaultKeySubstrings { get; }                       // Default sensitive patterns
public static SensitiveDataPolicy Default { get; }                         // Default policy instance
```

## SanitizationLevel Enum

Enumeration controlling sanitization aggressiveness.

```csharp
public enum SanitizationLevel
{
    None = 0,      // No sanitization applied
    Basic = 1,     // Redact only when key suggests sensitivity
    Strict = 2     // Redact all string values regardless of key
}
```

## GraphLoggingOptions

Advanced logging configuration for graph execution with granular control over behavior and structured data.

### Properties

#### Basic Configuration

```csharp
public LogLevel MinimumLevel { get; set; } = LogLevel.Information          // Minimum log level
public bool EnableStructuredLogging { get; set; } = true                   // Enable structured logging
public bool EnableCorrelationIds { get; set; } = true                      // Enable correlation IDs
public bool IncludeTimings { get; set; } = true                            // Include execution timing
public bool IncludeNodeMetadata { get; set; } = true                       // Include node metadata
public bool IncludeStateSnapshots { get; set; } = false                    // Include state snapshots
public int MaxStateDataSize { get; set; } = 2000                           // Max state data size
```

#### Data Handling

```csharp
public bool LogSensitiveData { get; set; } = false                         // Log sensitive data
public SensitiveDataPolicy Sanitization { get; set; } = Default            // Sanitization policy
public string? CorrelationIdPrefix { get; set; }                           // Correlation ID prefix
public string TimestampFormat { get; set; } = "yyyy-MM-dd HH:mm:ss.fff"    // Timestamp format
```

#### Category and Node Configuration

```csharp
public Dictionary<string, LogCategoryConfig> CategoryConfigs { get; set; }  // Category configs
public Dictionary<string, NodeLoggingConfig> NodeConfigs { get; set; }      // Node configs
```

## LogCategoryConfig

Configuration for logging a specific category of events.

### Properties

```csharp
public bool Enabled { get; set; } = true                                   // Enable category logging
public LogLevel Level { get; set; } = LogLevel.Information                 // Minimum log level
public Dictionary<string, object> CustomProperties { get; set; } = new()    // Custom properties
```

## NodeLoggingConfig

Configuration for logging a specific node, extending `LogCategoryConfig`.

### Properties

```csharp
public bool LogInputs { get; set; } = true                                 // Log input parameters
public bool LogOutputs { get; set; } = true                                // Log output results
public bool LogTiming { get; set; } = true                                 // Log execution timing
public bool LogStateChanges { get; set; } = true                           // Log state changes
public int MaxDataSize { get; set; } = 1000                                // Max data size to log
```

## HitlAuditService

Service that subscribes to human interaction events and records audit entries for compliance and inspection.

### Features

- Automatic audit trail generation
- Sensitive data sanitization
- Memory service integration
- Recent audit caching

### Constructors

```csharp
public HitlAuditService(
    IHumanInteractionStore store,
    IGraphMemoryService? memory,
    ILogger<HitlAuditService>? logger = null)
```

### Methods

```csharp
public Task RecordAuditAsync(string action, string? requestId = null, string? executionId = null, 
    string? nodeId = null, ApprovalStatus? status = null, string? userId = null, string? comments = null)
public Task<IList<AuditRecord>> GetRecentAuditsAsync(int count = 100)
public Task<IList<AuditRecord>> SearchAuditsAsync(AuditSearchCriteria criteria)
```

## Usage Examples

### Basic Logger Setup

```csharp
using Microsoft.Extensions.Logging;
using SemanticKernel.Graph.Integration;

// Create logger factory
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole()
           .AddJsonConsole()
           .SetMinimumLevel(LogLevel.Information);
});

// Create graph logger
var graphLogger = new SemanticKernelGraphLogger(
    loggerFactory.CreateLogger<SemanticKernelGraphLogger>(),
    new GraphOptions
    {
        EnableLogging = true,
        Logging = new GraphLoggingOptions
        {
            MinimumLevel = LogLevel.Information,
            EnableStructuredLogging = true,
            EnableCorrelationIds = true,
            IncludeTimings = true
        }
    }
);

// Use in graph executor
var graph = new GraphExecutor("LoggedGraph", "Graph with structured logging");
graph.SetLogger(graphLogger);
```

### Advanced Logging Configuration

```csharp
var loggingOptions = new GraphLoggingOptions
{
    MinimumLevel = LogLevel.Debug,
    EnableStructuredLogging = true,
    EnableCorrelationIds = true,
    IncludeTimings = true,
    IncludeNodeMetadata = true,
    MaxStateDataSize = 1000,
    LogSensitiveData = false,
    Sanitization = new SensitiveDataPolicy
    {
        Enabled = true,
        Level = SanitizationLevel.Basic,
        RedactionText = "[SENSITIVE]",
        MaskAuthorizationBearerToken = true
    }
};

// Configure category-specific logging
loggingOptions.CategoryConfigs["Graph"] = new LogCategoryConfig 
{ 
    Level = LogLevel.Information, 
    Enabled = true,
    CustomProperties = { ["component"] = "graph-engine" }
};

loggingOptions.CategoryConfigs["Node"] = new LogCategoryConfig 
{ 
    Level = LogLevel.Debug, 
    Enabled = true,
    CustomProperties = { ["component"] = "node-executor" }
};

// Configure node-specific logging
loggingOptions.NodeConfigs["api_call"] = new NodeLoggingConfig
{
    Level = LogLevel.Debug,
    LogInputs = true,
    LogOutputs = false,  // Don't log API responses
    LogTiming = true,
    MaxDataSize = 500
};
```

### Graph Registry Usage

```csharp
using SemanticKernel.Graph.Integration;

// Create registry
var registry = new GraphRegistry(logger);

// Register graphs
await registry.RegisterAsync(graphExecutor1);
await registry.RegisterAsync(graphExecutor2);

// List registered graphs
var graphs = await registry.ListAsync();
foreach (var graphInfo in graphs)
{
    Console.WriteLine($"Graph: {graphInfo.Name} ({graphInfo.NodeCount} nodes)");
}

// Get specific graph
var graph = await registry.GetAsync("MyGraph");
if (graph != null)
{
    // Execute the graph
    var result = await graph.ExecuteAsync(kernel, arguments);
}
```

### Tool Registry Integration

```csharp
using SemanticKernel.Graph.Integration;

// Create tool registry
var toolRegistry = new ToolRegistry(logger);

// Register external tool
var toolMetadata = new ToolMetadata
{
    Id = "weather_api",
    Name = "Weather API",
    Description = "Get current weather information",
    Type = "REST_API",
    Version = "1.0.0"
};

await toolRegistry.RegisterAsync(toolMetadata, serviceProvider =>
{
    return new RestToolGraphNode("weather_api", "https://api.weather.com/current");
});

// Create tool node
var toolNode = await toolRegistry.CreateNodeAsync("weather_api", serviceProvider);
if (toolNode != null)
{
    // Use the tool node in graph
    graph.AddNode(toolNode);
}
```

### Data Sanitization

```csharp
using SemanticKernel.Graph.Integration;

// Create sanitizer with custom policy
var policy = new SensitiveDataPolicy
{
    Enabled = true,
    Level = SanitizationLevel.Basic,
    RedactionText = "***REDACTED***",
    MaskAuthorizationBearerToken = true,
    KeySubstrings = new[]
    {
        "password", "secret", "token", "api_key",
        "credit_card", "ssn", "social_security"
    }
};

var sanitizer = new SensitiveDataSanitizer(policy);

// Sanitize sensitive data
var sensitiveData = new Dictionary<string, object>
{
    ["username"] = "john_doe",
    ["password"] = "secret123",
    ["api_key"] = "sk-1234567890abcdef",
    ["authorization"] = "Bearer sk-1234567890abcdef"
};

var sanitized = sanitizer.Sanitize(sensitiveData);
// Result: password and api_key values are redacted, authorization token is masked
```

### Plugin Registry Management

```csharp
using SemanticKernel.Graph.Integration;

// Create plugin registry
var pluginRegistry = new PluginRegistry(new PluginRegistryOptions
{
    MaxPlugins = 1000,
    EnablePeriodicCleanup = true,
    CleanupInterval = TimeSpan.FromHours(24)
}, logger);

// Register plugin
var pluginMetadata = new PluginMetadata
{
    Id = "data_processor",
    Name = "Data Processing Plugin",
    Description = "Process and transform data",
    Version = "2.1.0",
    Author = "Data Team",
    Tags = new[] { "data", "processing", "etl" }
};

var result = await pluginRegistry.RegisterPluginAsync(pluginMetadata, serviceProvider =>
{
    return new DataProcessingGraphNode();
});

if (result.IsSuccess)
{
    Console.WriteLine($"Plugin registered: {result.PluginId}");
}
else
{
    Console.WriteLine($"Registration failed: {result.ErrorMessage}");
}

// Search plugins
var searchCriteria = new PluginSearchCriteria
{
    Tags = new[] { "data" },
    MinVersion = "2.0.0"
};

var matchingPlugins = await pluginRegistry.SearchPluginsAsync(searchCriteria);
```

## See Also

- [Integration and Extensions Guide](../how-to/integration-and-extensions.md) - How to configure and use integration features
- [Security and Data Guide](../how-to/security-and-data.md) - Data sanitization and security best practices
- [Metrics and Logging Guide](../how-to/metrics-and-observability.md) - Logging configuration and observability
- [Extensions and Options Reference](./extensions-and-options.md) - Graph options and configuration
- [Streaming APIs Reference](./streaming.md) - Real-time execution monitoring
- [Inspection and Visualization Reference](./inspection-visualization.md) - Debug and inspection capabilities
