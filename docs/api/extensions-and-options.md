# Extensions and Options

The SemanticKernel.Graph library provides a comprehensive set of extension methods and configuration options that enable seamless integration with existing Semantic Kernel instances. This reference covers the core extension classes, configuration options, and builder patterns for setting up graph functionality.

## Overview

Extensions and options provide a fluent, configuration-driven approach to enabling graph functionality in Semantic Kernel applications. The system offers multiple levels of configuration granularity, from simple one-line setup to advanced, fine-tuned configurations for production environments.

## Key Concepts

**KernelArgumentsExtensions**: Extension methods that add graph-specific functionality to `KernelArguments`, including state management, execution tracking, and debugging capabilities.

**GraphOptions**: Configuration options for core graph functionality including logging, metrics, validation, and execution bounds.

**KernelBuilderExtensions**: Extension methods for `IKernelBuilder` that enable zero-configuration graph setup with fluent configuration APIs.

## Core Extension Classes

### KernelArgumentsExtensions

Extension methods that enhance `KernelArguments` with graph-specific functionality for state management, execution tracking, and debugging.

#### Graph State Methods

```csharp
// Convert to GraphState
public static GraphState ToGraphState(this KernelArguments arguments)

// Get or create GraphState
public static GraphState GetOrCreateGraphState(this KernelArguments arguments)

// Check if contains GraphState
public static bool HasGraphState(this KernelArguments arguments)

// Set GraphState
public static void SetGraphState(this KernelArguments arguments, GraphState graphState)
```

**Example:**
```csharp
var arguments = new KernelArguments
{
    ["input"] = "Hello World",
    ["user"] = "Alice"
};

// Convert to GraphState
var graphState = arguments.ToGraphState();

// Check if already has GraphState
if (arguments.HasGraphState())
{
    var existingState = arguments.GetOrCreateGraphState();
    Console.WriteLine($"State ID: {existingState.StateId}");
}
```

#### Execution Tracking Methods

```csharp
// Start execution step
public static void StartExecutionStep(this KernelArguments arguments, string nodeId, 
    string? description = null)

// Complete execution step
public static void CompleteExecutionStep(this KernelArguments arguments, string nodeId, 
    object? result = null, TimeSpan? duration = null)

// Set execution context
public static void SetExecutionContext(this KernelArguments arguments, 
    GraphExecutionContext context)

// Get execution context
public static GraphExecutionContext? GetExecutionContext(this KernelArguments arguments)

// Set execution priority
public static void SetExecutionPriority(this KernelArguments arguments, 
    ExecutionPriority priority)

// Get execution priority
public static ExecutionPriority? GetExecutionPriority(this KernelArguments arguments)
```

**Example:**
```csharp
// Start tracking execution
arguments.StartExecutionStep("process_input", "Processing user input");

// Simulate work
await Task.Delay(100);

// Complete step with result
arguments.CompleteExecutionStep("process_input", "processed", TimeSpan.FromMilliseconds(100));

// Set execution priority
arguments.SetExecutionPriority(ExecutionPriority.High);

// Get execution context
var context = arguments.GetExecutionContext();
if (context != null)
{
    Console.WriteLine($"Execution ID: {context.ExecutionId}");
}
```

#### Execution ID Management

```csharp
// Set execution ID
public static void SetExecutionId(this KernelArguments arguments, string executionId)

// Get execution ID
public static string? GetExecutionId(this KernelArguments arguments)

// Generate execution ID
public static string GenerateExecutionId(this KernelArguments arguments)

// Generate deterministic execution ID
public static string GenerateDeterministicExecutionId(this KernelArguments arguments, int seed)
```

**Example:**
```csharp
// Generate unique execution ID
var executionId = arguments.GenerateExecutionId();
Console.WriteLine($"Generated ID: {executionId}");

// Generate deterministic ID for testing
var deterministicId = arguments.GenerateDeterministicExecutionId(42);
Console.WriteLine($"Deterministic ID: {deterministicId}");

// Set custom execution ID
arguments.SetExecutionId("custom-execution-123");

// Retrieve execution ID
var currentId = arguments.GetExecutionId();
```

#### Debug and Inspection Methods

```csharp
// Set debug session
public static void SetDebugSession(this KernelArguments arguments, IDebugSession debugSession)

// Get debug session
public static IDebugSession? GetDebugSession(this KernelArguments arguments)

// Set debug mode
public static void SetDebugMode(this KernelArguments arguments, DebugExecutionMode mode)

// Get debug mode
public static DebugExecutionMode GetDebugMode(this KernelArguments arguments)

// Check if debug enabled
public static bool IsDebugEnabled(this KernelArguments arguments)

// Clear debug info
public static void ClearDebugInfo(this KernelArguments arguments)
```

**Example:**
```csharp
// Enable debug mode
arguments.SetDebugMode(DebugExecutionMode.Verbose);

// Check debug status
if (arguments.IsDebugEnabled())
{
    Console.WriteLine("Debug mode is enabled");
    
    // Set debug session
    var debugSession = new DebugSession("debug-123");
    arguments.SetDebugSession(debugSession);
}

// Clear debug information
arguments.ClearDebugInfo();
```

#### Utility Methods

```csharp
// Clone arguments
public static KernelArguments Clone(this KernelArguments arguments)

// Set node cost weight
public static void SetNodeCostWeight(this KernelArguments arguments, double weight)

// Get node cost weight
public static double GetNodeCostWeight(this KernelArguments arguments)
```

**Example:**
```csharp
// Clone arguments for parallel execution
var clonedArgs = arguments.Clone();

// Set cost weight for resource governance
arguments.SetNodeCostWeight(2.5);

// Get cost weight
var weight = arguments.GetNodeCostWeight();
Console.WriteLine($"Node cost weight: {weight}");
```

### GraphOptions

Configuration options for core graph functionality that control logging, metrics, validation, and execution bounds.

#### Core Properties

```csharp
public sealed class GraphOptions
{
    // Enable/disable logging
    public bool EnableLogging { get; set; } = true;
    
    // Enable/disable metrics collection
    public bool EnableMetrics { get; set; } = true;
    
    // Maximum execution steps
    public int MaxExecutionSteps { get; set; } = 1000;
    
    // Validate graph integrity
    public bool ValidateGraphIntegrity { get; set; } = true;
    
    // Execution timeout
    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromMinutes(10);
    
    // Enable plan compilation
    public bool EnablePlanCompilation { get; set; } = true;
    
    // Logging configuration
    public GraphLoggingOptions Logging { get; set; } = new();
    
    // Interoperability options
    public GraphInteropOptions Interop { get; set; } = new();
}
```

**Example:**
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

#### GraphLoggingOptions

Advanced logging configuration for graph execution with granular control over behavior and structured data.

```csharp
public sealed class GraphLoggingOptions
{
    // Minimum log level
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
    
    // Enable structured logging
    public bool EnableStructuredLogging { get; set; } = true;
    
    // Enable correlation IDs
    public bool EnableCorrelationIds { get; set; } = true;
    
    // Include execution timing
    public bool IncludeTimings { get; set; } = true;
    
    // Include node metadata
    public bool IncludeNodeMetadata { get; set; } = true;
    
    // Include state snapshots in debug logs
    public bool IncludeStateSnapshots { get; set; } = false;
    
    // Maximum state data size in logs
    public int MaxStateDataSize { get; set; } = 2000;
    
    // Category-specific configurations
    public Dictionary<string, NodeLoggingOptions> CategoryConfigurations { get; set; } = new();
}
```

**Example:**
```csharp
var loggingOptions = new GraphLoggingOptions
{
    MinimumLevel = LogLevel.Debug,
    EnableStructuredLogging = true,
    EnableCorrelationIds = true,
    IncludeTimings = true,
    IncludeNodeMetadata = true,
    MaxStateDataSize = 1000
};

// Configure specific node categories
loggingOptions.CategoryConfigurations["ai_nodes"] = new NodeLoggingOptions
{
    LogInputs = true,
    LogOutputs = true,
    LogTiming = true,
    LogStateChanges = true,
    MaxDataSize = 500
};

loggingOptions.CategoryConfigurations["utility_nodes"] = new NodeLoggingOptions
{
    LogInputs = false,
    LogOutputs = false,
    LogTiming = true,
    LogStateChanges = false,
    MaxDataSize = 100
};
```

#### GraphInteropOptions

Interoperability options for cross-ecosystem integrations and external tool support.

```csharp
public sealed class GraphInteropOptions
{
    // Enable importers for external formats
    public bool EnableImporters { get; set; } = true;
    
    // Enable exporters for industry formats
    public bool EnableExporters { get; set; } = true;
    
    // Enable Python execution bridge
    public bool EnablePythonBridge { get; set; } = false;
    
    // Enable federation with external engines
    public bool EnableFederation { get; set; } = true;
    
    // Python executable path
    public string? PythonExecutablePath { get; set; }
    
    // Federation base address
    public string? FederationBaseAddress { get; set; }
    
    // Security options for replay/export
    public GraphSecurityOptions Security { get; set; } = new();
}
```

**Example:**
```csharp
var interopOptions = new GraphInteropOptions
{
    EnableImporters = true,
    EnableExporters = true,
    EnablePythonBridge = true,
    EnableFederation = false,
    PythonExecutablePath = "/usr/bin/python3"
};

// Configure security options
interopOptions.Security.EnableHashing = true;
interopOptions.Security.EnableEncryption = false;
interopOptions.Security.HashAlgorithm = "SHA256";
```

### KernelBuilderExtensions

Extension methods for `IKernelBuilder` that enable zero-configuration graph setup with fluent configuration APIs.

#### Basic Graph Support

```csharp
// Add graph support with default configuration
public static IKernelBuilder AddGraphSupport(this IKernelBuilder builder)

// Add graph support with custom configuration
public static IKernelBuilder AddGraphSupport(this IKernelBuilder builder, 
    Action<GraphOptions> configure)

// Add graph support with logging configuration
public static IKernelBuilder AddGraphSupportWithLogging(this IKernelBuilder builder, 
    Action<GraphLoggingOptions> configure)
```

**Example:**
```csharp
var builder = Kernel.CreateBuilder();

// Basic setup
builder.AddGraphSupport();

// Custom configuration
builder.AddGraphSupport(options =>
{
    options.EnableLogging = true;
    options.EnableMetrics = true;
    options.MaxExecutionSteps = 500;
    options.ExecutionTimeout = TimeSpan.FromMinutes(5);
    
    options.Logging.MinimumLevel = LogLevel.Debug;
    options.Logging.EnableStructuredLogging = true;
    
    options.Interop.EnableImporters = true;
    options.Interop.EnableExporters = true;
});

// With logging configuration
builder.AddGraphSupportWithLogging(logging =>
{
    logging.MinimumLevel = LogLevel.Information;
    logging.EnableCorrelationIds = true;
    logging.IncludeTimings = true;
});
```

#### Environment-Specific Configurations

```csharp
// Debug configuration
public static IKernelBuilder AddGraphSupportForDebugging(this IKernelBuilder builder)

// Production configuration
public static IKernelBuilder AddGraphSupportForProduction(this IKernelBuilder builder)

// High performance configuration
public static IKernelBuilder AddGraphSupportForPerformance(this IKernelBuilder builder)
```

**Example:**
```csharp
var builder = Kernel.CreateBuilder();

// Environment-specific setup
if (environment.IsDevelopment())
{
    builder.AddGraphSupportForDebugging();
}
else if (environment.IsProduction())
{
    builder.AddGraphSupportForProduction();
}
else
{
    builder.AddGraphSupportForPerformance();
}
```

#### Complete Graph Support

```csharp
// Add complete graph support with all integrations
public static IKernelBuilder AddCompleteGraphSupport(this IKernelBuilder builder, 
    Action<CompleteGraphOptions>? configure = null)
```

**Example:**
```csharp
var builder = Kernel.CreateBuilder();

// Complete setup with all features
builder.AddCompleteGraphSupport(options =>
{
    options.EnableLogging = true;
    options.EnableMemory = true;
    options.EnableTemplates = true;
    options.EnableVectorSearch = true;
    options.EnableSemanticSearch = true;
    options.EnableCustomHelpers = true;
    options.EnableMetrics = true;
    options.MaxExecutionSteps = 1000;
});
```

#### Module-Specific Extensions

```csharp
// Add checkpoint support
public static IKernelBuilder AddCheckpointSupport(this IKernelBuilder builder, 
    Action<CheckpointOptions> configure)

// Add streaming support
public static IKernelBuilder AddGraphStreamingPool(this IKernelBuilder builder, 
    Action<StreamingOptions>? configure = null)

// Add memory integration
public static IKernelBuilder AddGraphMemory(this IKernelBuilder builder, 
    Action<GraphMemoryOptions>? configure = null)

// Add template support
public static IKernelBuilder AddGraphTemplates(this IKernelBuilder builder, 
    Action<GraphTemplateOptions>? configure = null)
```

**Example:**
```csharp
var builder = Kernel.CreateBuilder();

// Add core graph support
builder.AddGraphSupport();

// Add checkpointing
builder.AddCheckpointSupport(options =>
{
    options.EnableCompression = true;
    options.MaxCacheSize = 500;
    options.EnableAutoCleanup = true;
    options.AutoCleanupInterval = TimeSpan.FromHours(2);
});

// Add streaming
builder.AddGraphStreamingPool(options =>
{
    options.BufferSize = 100;
    options.MaxBufferSize = 1000;
    options.EnableAutoReconnect = true;
    options.MaxReconnectAttempts = 3;
});

// Add memory integration
builder.AddGraphMemory(options =>
{
    options.EnableVectorSearch = true;
    options.EnableSemanticSearch = true;
    options.DefaultCollectionName = "my-graph-memory";
    options.SimilarityThreshold = 0.8;
});

// Add templates
builder.AddGraphTemplates(options =>
{
    options.EnableHandlebars = true;
    options.EnableCustomHelpers = true;
    options.TemplateCacheSize = 200;
});
```

#### Simple Graph Creation

```csharp
// Create simple sequential graph
public static IKernelBuilder AddSimpleSequentialGraph(this IKernelBuilder builder, 
    string graphName, params string[] functionNames)

// Create graph from template
public static IKernelBuilder AddGraphFromTemplate(this IKernelBuilder builder, 
    string graphName, string templatePath, object? templateData = null)
```

**Example:**
```csharp
var builder = Kernel.CreateBuilder();

// Create simple sequential graph
builder.AddSimpleSequentialGraph("workflow", 
    "validate_input", 
    "process_data", 
    "generate_output");

// Create graph from template
builder.AddGraphFromTemplate("chatbot", "templates/chatbot.json", new
{
    welcomeMessage = "Hello! How can I help you?",
    maxTurns = 10
});
```

## Sub-Options Classes

### CheckpointOptions

Configuration for checkpointing behavior and persistence.

```csharp
public sealed class CheckpointOptions
{
    // Enable compression
    public bool EnableCompression { get; set; } = true;
    
    // Maximum cache size
    public int MaxCacheSize { get; set; } = 1000;
    
    // Default retention policy
    public CheckpointRetentionPolicy DefaultRetentionPolicy { get; set; } = new();
    
    // Enable auto cleanup
    public bool EnableAutoCleanup { get; set; } = true;
    
    // Auto cleanup interval
    public TimeSpan AutoCleanupInterval { get; set; } = TimeSpan.FromHours(1);
    
    // Enable distributed backup
    public bool EnableDistributedBackup { get; set; } = false;
    
    // Default backup options
    public CheckpointBackupOptions DefaultBackupOptions { get; set; } = new();
}
```

### StreamingOptions

Configuration for streaming execution and event handling.

```csharp
public sealed class StreamingOptions
{
    // Buffer size
    public int BufferSize { get; set; } = 100;
    
    // Maximum buffer size
    public int MaxBufferSize { get; set; } = 1000;
    
    // Enable auto reconnect
    public bool EnableAutoReconnect { get; set; } = true;
    
    // Maximum reconnect attempts
    public int MaxReconnectAttempts { get; set; } = 3;
    
    // Initial reconnect delay
    public TimeSpan InitialReconnectDelay { get; set; } = TimeSpan.FromSeconds(1);
    
    // Maximum reconnect delay
    public TimeSpan MaxReconnectDelay { get; set; } = TimeSpan.FromSeconds(30);
    
    // Include state snapshots
    public bool IncludeStateSnapshots { get; set; } = false;
    
    // Event types to emit
    public GraphExecutionEventType[]? EventTypesToEmit { get; set; }
}
```

### GraphMemoryOptions

Configuration for memory integration and vector search.

```csharp
public sealed class GraphMemoryOptions
{
    // Enable vector search
    public bool EnableVectorSearch { get; set; } = true;
    
    // Enable semantic search
    public bool EnableSemanticSearch { get; set; } = true;
    
    // Default collection name
    public string DefaultCollectionName { get; set; } = "graph-memory";
    
    // Similarity threshold
    public double SimilarityThreshold { get; set; } = 0.7;
}
```

### GraphTemplateOptions

Configuration for template engines and custom helpers.

```csharp
public sealed class GraphTemplateOptions
{
    // Enable Handlebars templates
    public bool EnableHandlebars { get; set; } = true;
    
    // Enable custom template helpers
    public bool EnableCustomHelpers { get; set; } = true;
    
    // Template cache size
    public int TemplateCacheSize { get; set; } = 100;
}
```

## Usage Patterns

### Basic Setup Pattern

```csharp
var builder = Kernel.CreateBuilder();

// Add basic graph support
builder.AddGraphSupport(options =>
{
    options.EnableLogging = true;
    options.EnableMetrics = true;
    options.MaxExecutionSteps = 1000;
    options.ExecutionTimeout = TimeSpan.FromMinutes(10);
});

var kernel = builder.Build();
```

### Production Setup Pattern

```csharp
var builder = Kernel.CreateBuilder();

// Production-optimized configuration
builder.AddGraphSupportForProduction()
    .AddCheckpointSupport(options =>
    {
        options.EnableCompression = true;
        options.MaxCacheSize = 2000;
        options.EnableAutoCleanup = true;
    })
    .AddGraphStreamingPool(options =>
    {
        options.BufferSize = 500;
        options.MaxBufferSize = 5000;
        options.EnableAutoReconnect = true;
    });

var kernel = builder.Build();
```

### Debug Setup Pattern

```csharp
var builder = Kernel.CreateBuilder();

// Debug-optimized configuration
builder.AddGraphSupportForDebugging()
    .AddGraphStreamingPool(options =>
    {
        options.IncludeStateSnapshots = true;
        options.EventTypesToEmit = new[]
        {
            GraphExecutionEventType.ExecutionStarted,
            GraphExecutionEventType.NodeStarted,
            GraphExecutionEventType.NodeCompleted,
            GraphExecutionEventType.ExecutionCompleted
        };
    });

var kernel = builder.Build();
```

### Complete Integration Pattern

```csharp
var builder = Kernel.CreateBuilder();

// Complete setup with all features
builder.AddCompleteGraphSupport(options =>
{
    options.EnableLogging = true;
    options.EnableMemory = true;
    options.EnableTemplates = true;
    options.EnableVectorSearch = true;
    options.EnableSemanticSearch = true;
    options.EnableCustomHelpers = true;
    options.EnableMetrics = true;
    options.MaxExecutionSteps = 2000;
});

var kernel = builder.Build();
```

## Performance Considerations

- **Logging**: Disable detailed logging in production to improve performance
- **Metrics**: Use sampling for high-frequency operations
- **Checkpointing**: Balance checkpoint frequency with storage costs
- **Streaming**: Configure buffer sizes based on memory constraints
- **Templates**: Use caching for frequently accessed templates

## Thread Safety

- **GraphOptions**: Thread-safe for configuration; immutable during execution
- **KernelArgumentsExtensions**: Thread-safe for concurrent access
- **KernelBuilderExtensions**: Thread-safe during builder configuration

## Error Handling

- **Configuration Validation**: Options are validated during builder configuration
- **Graceful Degradation**: Optional features fail gracefully if dependencies are missing
- **Environment Detection**: Automatic configuration based on environment variables

## See Also

- [Graph Executor](graph-executor.md)
- [Graph State](state-and-serialization.md)
- [Streaming Execution](../concepts/streaming.md)
- [Checkpointing](../concepts/checkpointing.md)
- [Memory Integration](../concepts/memory.md)
- [Template Engine](../concepts/templates.md)
- [Integration Guide](../how-to/integration-and-extensions.md)
