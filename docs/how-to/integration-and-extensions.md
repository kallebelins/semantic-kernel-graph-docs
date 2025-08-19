# Integration and Extensions

Integration and extensions in SemanticKernel.Graph provide seamless integration with Semantic Kernel and comprehensive configuration options for production deployments. This guide covers kernel builder extensions, configuration options, data sanitization, structured logging, and policy management.

## What You'll Learn

- How to integrate graph functionality with existing Semantic Kernel instances
- Configuring comprehensive graph options for different environments
- Implementing data sanitization and security policies
- Setting up structured logging with correlation and context
- Managing policies for cost, timeout, and error handling
- Best practices for production integration and deployment

## Concepts and Techniques

**KernelBuilderExtensions**: Fluent API extensions that enable zero-configuration graph setup and integrate with existing Semantic Kernel instances.

**GraphOptions**: Comprehensive configuration options for graph functionality including logging, metrics, validation, and execution bounds.

**SensitiveDataSanitizer**: Utility for automatically redacting sensitive information from logs, exports, and debugging output.

**Structured Logging**: Enhanced logging with correlation IDs, execution context, and structured data for better observability.

**Policy System**: Pluggable policies for cost management, timeout handling, error recovery, and business logic integration.

## Prerequisites

- [First Graph Tutorial](../first-graph-5-minutes.md) completed
- Basic understanding of Semantic Kernel concepts
- Familiarity with dependency injection and configuration
- Understanding of logging and security best practices

## Kernel Builder Integration

### Basic Graph Support Integration

Add graph functionality to existing Semantic Kernel instances:

```csharp
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Extensions;

// Create kernel with graph support
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddGraphSupport()  // Enable with default configuration
    .Build();
```

### Customized Graph Integration

Configure graph options during integration:

```csharp
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddGraphSupport(options =>
    {
        options.EnableLogging = true;
        options.EnableMetrics = true;
        options.MaxExecutionSteps = 500;
        options.ValidateGraphIntegrity = true;
        options.ExecutionTimeout = TimeSpan.FromMinutes(5);
        
        // Configure logging options
        options.Logging.EnableStructuredLogging = true;
        options.Logging.EnableCorrelationIds = true;
        options.Logging.IncludeTimings = true;
        options.Logging.LogSensitiveData = false;
    })
    .Build();
```

### Environment-Specific Integration

Use preset configurations for different environments:

```csharp
// Development environment
var devKernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddGraphSupportForDebugging()  // Enhanced logging, detailed metrics
    .Build();

// Production environment
var prodKernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddGraphSupportForProduction()  // Optimized logging, production metrics
    .Build();

// High-performance environment
var perfKernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddGraphSupportForPerformance()  // Minimal logging, performance focus
    .Build();
```

### Complete Integration with All Features

Enable comprehensive graph functionality:

```csharp
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddCompleteGraphSupport(options =>
    {
        options.EnableLogging = true;
        options.EnableMetrics = true;
        options.EnableMemory = true;
        options.EnableTemplates = true;
        options.EnableVectorSearch = true;
        options.EnableSemanticSearch = true;
        options.MaxExecutionSteps = 1000;
    })
    .Build();
```

## Graph Options Configuration

### Core Graph Options

Configure fundamental graph behavior:

```csharp
var graphOptions = new GraphOptions
{
    // Core functionality
    EnableLogging = true,
    EnableMetrics = true,
    EnablePlanCompilation = true,
    
    // Execution limits
    MaxExecutionSteps = 1000,
    ExecutionTimeout = TimeSpan.FromMinutes(10),
    
    // Validation
    ValidateGraphIntegrity = true
};

// Apply to kernel
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddGraphSupport(options =>
    {
        options.EnableLogging = graphOptions.EnableLogging;
        options.EnableMetrics = graphOptions.EnableMetrics;
        options.MaxExecutionSteps = graphOptions.MaxExecutionSteps;
        options.ExecutionTimeout = graphOptions.ExecutionTimeout;
        options.ValidateGraphIntegrity = graphOptions.ValidateGraphIntegrity;
    })
    .Build();
```

### Advanced Logging Configuration

Configure detailed logging behavior:

```csharp
var loggingOptions = new GraphLoggingOptions
{
    // Basic logging
    MinimumLevel = LogLevel.Information,
    EnableStructuredLogging = true,
    EnableCorrelationIds = true,
    IncludeTimings = true,
    
    // Context and metadata
    IncludeNodeMetadata = true,
    IncludeStateSnapshots = false,
    MaxStateDataSize = 2000,
    
    // Sensitive data handling
    LogSensitiveData = false,
    Sanitization = SensitiveDataPolicy.Default,
    
    // Customization
    CorrelationIdPrefix = "myapp",
    TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff"
};

// Configure category-specific logging
loggingOptions.CategoryConfigs["Graph"] = new LogCategoryConfig 
{ 
    Level = LogLevel.Information, 
    Enabled = true 
};
loggingOptions.CategoryConfigs["Node"] = new LogCategoryConfig 
{ 
    Level = LogLevel.Debug, 
    Enabled = true 
};
loggingOptions.CategoryConfigs["Performance"] = new LogCategoryConfig 
{ 
    Level = LogLevel.Information, 
    Enabled = true 
};

// Apply to kernel
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddGraphSupport(options =>
    {
        options.Logging = loggingOptions;
    })
    .Build();
```

### Metrics Configuration

Configure performance metrics collection:

```csharp
var metricsOptions = GraphMetricsOptions.CreateProductionOptions();
metricsOptions.EnableResourceMonitoring = true;
metricsOptions.ResourceSamplingInterval = TimeSpan.FromSeconds(10);
metricsOptions.MaxSampleHistory = 10000;
metricsOptions.EnableDetailedPathTracking = true;

// Apply to graph executor
var graph = new GraphExecutor("ConfiguredGraph", "Graph with custom metrics");
graph.ConfigureMetrics(metricsOptions);
```

## Data Sanitization and Security

### Basic Data Sanitization

Implement automatic sensitive data redaction:

```csharp
using SemanticKernel.Graph.Integration;

// Create sanitizer with default policy
var sanitizer = new SensitiveDataSanitizer();

// Sanitize dictionary data
var sensitiveData = new Dictionary<string, object?>
{
    ["username"] = "john_doe",
    ["password"] = "secret123",
    ["api_key"] = "sk-1234567890abcdef",
    ["authorization"] = "Bearer sk-1234567890abcdef",
    ["connection_string"] = "Server=localhost;Database=test;User Id=admin;Password=secret;",
    ["normal_data"] = "This is not sensitive"
};

var sanitizedData = sanitizer.Sanitize(sensitiveData);

// Output: sensitive values are redacted
foreach (var kvp in sanitizedData)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}
// username: john_doe
// password: ***REDACTED***
// api_key: ***REDACTED***
// authorization: Bearer ***REDACTED***
// connection_string: ***REDACTED***
// normal_data: This is not sensitive
```

### Custom Sanitization Policies

Configure sanitization behavior for your domain:

```csharp
var customPolicy = new SensitiveDataPolicy
{
    Enabled = true,
    Level = SanitizationLevel.Basic,
    RedactionText = "[REDACTED]",
    SensitiveKeySubstrings = new[]
    {
        "password", "secret", "token", "key",
        "credential", "auth", "private",
        "ssn", "credit_card", "bank_account"
    },
    MaskAuthorizationBearerToken = true
};

var sanitizer = new SensitiveDataSanitizer(customPolicy);

// Test custom policy
var testData = new Dictionary<string, object?>
{
    ["user_ssn"] = "123-45-6789",
    ["credit_card_number"] = "4111-1111-1111-1111",
    ["private_note"] = "Confidential information",
    ["public_info"] = "This is public"
};

var sanitized = sanitizer.Sanitize(testData);
```

### Integration with Logging

Apply sanitization to logging output:

```csharp
// Configure logging with sanitization
var loggingOptions = new GraphLoggingOptions
{
    LogSensitiveData = false,
    Sanitization = new SensitiveDataPolicy
    {
        Enabled = true,
        Level = SanitizationLevel.Basic,
        RedactionText = "[SENSITIVE]"
    }
};

// Apply to kernel
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddGraphSupport(options =>
    {
        options.Logging = loggingOptions;
    })
    .Build();
```

## Structured Logging Setup

### Basic Structured Logging

Enable structured logging with correlation:

```csharp
using Microsoft.Extensions.Logging;
using SemanticKernel.Graph.Integration;

// Configure logging factory
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole()
           .AddJsonConsole()  // Enable structured logging
           .SetMinimumLevel(LogLevel.Information);
});

// Create graph logger with structured logging
var graphLogger = new SemanticKernelGraphLogger(
    loggerFactory.CreateLogger<SemanticKernelGraphLogger>(),
    new GraphLoggingOptions
    {
        EnableStructuredLogging = true,
        EnableCorrelationIds = true,
        IncludeTimings = true,
        IncludeNodeMetadata = true
    }
);

// Use in graph executor
var graph = new GraphExecutor("LoggedGraph", "Graph with structured logging");
graph.SetLogger(graphLogger);
```

### Advanced Logging Configuration

Configure comprehensive logging behavior:

```csharp
var advancedLoggingOptions = new GraphLoggingOptions
{
    // Basic settings
    MinimumLevel = LogLevel.Debug,
    EnableStructuredLogging = true,
    EnableCorrelationIds = true,
    IncludeTimings = true,
    
    // Context and metadata
    IncludeNodeMetadata = true,
    IncludeStateSnapshots = true,
    MaxStateDataSize = 5000,
    
    // Category-specific configuration
    CategoryConfigs = new Dictionary<string, LogCategoryConfig>
    {
        ["Graph"] = new LogCategoryConfig 
        { 
            Level = LogLevel.Information, 
            Enabled = true,
            CustomProperties = { ["component"] = "graph-engine" }
        },
        ["Node"] = new LogCategoryConfig 
        { 
            Level = LogLevel.Debug, 
            Enabled = true,
            CustomProperties = { ["component"] = "node-executor" }
        },
        ["Performance"] = new LogCategoryConfig 
        { 
            Level = LogLevel.Information, 
            Enabled = true,
            CustomProperties = { ["component"] = "metrics-collector" }
        },
        ["Error"] = new LogCategoryConfig 
        { 
            Level = LogLevel.Error, 
            Enabled = true,
            CustomProperties = { ["component"] = "error-handler" }
        }
    },
    
    // Node-specific logging
    NodeConfigs = new Dictionary<string, NodeLoggingConfig>
    {
        ["api_call"] = new NodeLoggingConfig
        {
            Level = LogLevel.Debug,
            LogInputs = true,
            LogOutputs = false,  // Don't log API responses
            LogExecutionTime = true
        },
        ["data_processing"] = new NodeLoggingConfig
        {
            Level = LogLevel.Information,
            LogInputs = false,   // Don't log large data inputs
            LogOutputs = false,  // Don't log processed data
            LogExecutionTime = true
        }
    }
};

// Apply to kernel
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddGraphSupport(options =>
    {
        options.Logging = advancedLoggingOptions;
    })
    .Build();
```

### Logging with Correlation

Implement correlation-based logging for distributed tracing:

```csharp
// Configure correlation ID generation
var correlationOptions = new GraphLoggingOptions
{
    EnableCorrelationIds = true,
    CorrelationIdPrefix = "graph-exec",
    EnableStructuredLogging = true,
    IncludeTimings = true
};

// Create logger with correlation
var logger = new SemanticKernelGraphLogger(
    loggerFactory.CreateLogger<SemanticKernelGraphLogger>(),
    correlationOptions
);

// Execute graph with correlation
var graph = new GraphExecutor("CorrelatedGraph", "Graph with correlation IDs");
graph.SetLogger(logger);

var arguments = new KernelArguments();
arguments.Set("request_id", Guid.NewGuid().ToString());

var result = await graph.ExecuteAsync(kernel, arguments);

// Logs will include correlation IDs for tracing
// [2025-08-15 10:30:45.123] [INFO] [graph-exec-abc123] Graph execution started
// [2025-08-15 10:30:45.125] [DEBUG] [graph-exec-abc123] Node 'start' execution started
// [2025-08-15 10:30:45.130] [DEBUG] [graph-exec-abc123] Node 'start' execution completed in 5ms
```

## Policy System Integration

### Cost Policy Implementation

Implement custom cost calculation policies:

```csharp
using SemanticKernel.Graph.Integration.Policies;

public class BusinessCostPolicy : ICostPolicy
{
    public double? GetNodeCostWeight(IGraphNode node, GraphState state)
    {
        // Calculate cost based on business value
        if (state.KernelArguments.TryGetValue("business_value", out var valueObj))
        {
            var businessValue = Convert.ToDouble(valueObj);
            return Math.Max(1.0, businessValue / 100.0);
        }
        
        // Calculate cost based on data size
        if (state.KernelArguments.TryGetValue("data_size_mb", out var sizeObj))
        {
            var sizeMB = Convert.ToDouble(sizeObj);
            if (sizeMB > 100) return 5.0;
            if (sizeMB > 10) return 2.0;
            return 0.5;
        }
        
        return null; // Use default
    }

    public ExecutionPriority? GetNodePriority(IGraphNode node, GraphState state)
    {
        // Determine priority based on customer tier
        if (state.KernelArguments.TryGetValue("customer_tier", out var tierObj))
        {
            return tierObj.ToString() switch
            {
                "premium" => ExecutionPriority.Critical,
                "gold" => ExecutionPriority.High,
                "silver" => ExecutionPriority.Normal,
                _ => ExecutionPriority.Low
            };
        }
        
        return null; // Use default
    }
}

// Register policy with graph
var graph = new GraphExecutor("PolicyGraph", "Graph with custom policies");
graph.AddMetadata(nameof(ICostPolicy), new BusinessCostPolicy());
```

### Timeout Policy Implementation

Implement custom timeout policies:

```csharp
public class AdaptiveTimeoutPolicy : ITimeoutPolicy
{
    public TimeSpan? GetNodeTimeout(IGraphNode node, GraphState state)
    {
        // Set timeout based on node type
        if (node.NodeId.Contains("api_call"))
        {
            return TimeSpan.FromSeconds(30); // API calls get 30s timeout
        }
        
        if (node.NodeId.Contains("data_processing"))
        {
            // Timeout based on data size
            if (state.KernelArguments.TryGetValue("data_size_mb", out var sizeObj))
            {
                var sizeMB = Convert.ToDouble(sizeObj);
                if (sizeMB > 100) return TimeSpan.FromMinutes(5);
                if (sizeMB > 10) return TimeSpan.FromMinutes(2);
                return TimeSpan.FromSeconds(30);
            }
        }
        
        if (node.NodeId.Contains("ml_inference"))
        {
            return TimeSpan.FromMinutes(10); // ML operations get 10m timeout
        }
        
        return null; // Use default timeout
    }
}

// Register timeout policy
graph.AddMetadata(nameof(ITimeoutPolicy), new AdaptiveTimeoutPolicy());
```

### Error Handling Policy Implementation

Implement custom error recovery policies:

```csharp
public class RetryPolicy : IErrorHandlingPolicy
{
    public bool ShouldRetry(IGraphNode node, Exception exception, GraphExecutionContext context, out TimeSpan? delay)
    {
        delay = null;
        
        // Retry transient exceptions
        if (exception is HttpRequestException || 
            exception is TaskCanceledException ||
            exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        {
            // Exponential backoff: 1s, 2s, 4s, 8s
            var retryCount = context.GraphState.GetRetryCount(node.NodeId);
            if (retryCount < 3)
            {
                delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                return true;
            }
        }
        
        return false;
    }

    public bool ShouldSkip(IGraphNode node, Exception exception, GraphExecutionContext context)
    {
        // Skip nodes that fail with non-critical errors
        if (exception is UnauthorizedAccessException ||
            exception.Message.Contains("permission denied", StringComparison.OrdinalIgnoreCase))
        {
            return true; // Skip unauthorized operations
        }
        
        return false;
    }
}

// Register error handling policy
graph.AddMetadata(nameof(IErrorHandlingPolicy), new RetryPolicy());
```

## Advanced Integration Patterns

### Module Activation Extensions

Conditionally enable graph modules:

```csharp
using SemanticKernel.Graph.Extensions;

var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddGraphModules(options =>
    {
        // Enable modules based on environment
        options.EnableStreaming = true;
        options.EnableCheckpointing = true;
        options.EnableRecovery = true;
        options.EnableHumanInTheLoop = false; // Disable in production
        options.EnableMultiAgent = true;
    })
    .Build();
```

### Checkpoint Support Integration

Add checkpointing capabilities:

```csharp
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddGraphSupport()
    .AddCheckpointSupport(options =>
    {
        options.EnableCompression = true;
        options.MaxCacheSize = 1000;
        options.EnableAutoCleanup = true;
        options.AutoCleanupInterval = TimeSpan.FromHours(1);
        options.CompressionLevel = System.IO.Compression.CompressionLevel.Optimal;
    })
    .Build();
```

### Memory Integration

Add memory capabilities for state persistence:

```csharp
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey)
    .AddGraphSupport()
    .AddGraphMemory(options =>
    {
        options.EnableVectorSearch = true;
        options.EnableSemanticSearch = true;
        options.MaxMemorySize = 1024 * 1024 * 100; // 100MB
        options.EnablePersistence = true;
        options.PersistencePath = "./graph-memory";
    })
    .Build();
```

## Best Practices

### Integration Configuration

- **Start Simple**: Begin with basic `AddGraphSupport()` and add features incrementally
- **Environment-Specific**: Use preset configurations for different deployment environments
- **Dependency Management**: Ensure all required services are registered before building the kernel
- **Configuration Validation**: Validate configuration options early in the startup process

### Security and Data Handling

- **Always Sanitize**: Enable data sanitization for all production deployments
- **Custom Policies**: Implement domain-specific sanitization policies
- **Audit Logging**: Log all policy decisions and data access for compliance
- **Regular Reviews**: Periodically review and update security policies

### Logging and Observability

- **Structured Logging**: Use structured logging for better searchability and analysis
- **Correlation IDs**: Enable correlation IDs for distributed tracing
- **Performance Monitoring**: Include execution timing in logs for performance analysis
- **Log Levels**: Configure appropriate log levels for different environments

### Policy Management

- **Business Logic**: Implement policies that reflect your business requirements
- **Performance Impact**: Consider the performance impact of complex policy logic
- **Testing**: Thoroughly test policies with various scenarios and edge cases
- **Documentation**: Document policy behavior and configuration options

## Troubleshooting

### Common Integration Issues

**Service registration failures**: Ensure all required services are registered before building the kernel.

**Configuration validation errors**: Check that all configuration options have valid values.

**Policy execution errors**: Verify that policy implementations handle all edge cases gracefully.

**Logging configuration issues**: Ensure logging providers are properly configured and accessible.

### Performance Optimization

```csharp
// Optimize for high-throughput scenarios
var optimizedOptions = new GraphOptions
{
    EnableLogging = true,
    EnableMetrics = true,
    EnablePlanCompilation = true,
    ValidateGraphIntegrity = false, // Disable for performance
    MaxExecutionSteps = 5000
};

// Use performance-optimized logging
var perfLoggingOptions = new GraphLoggingOptions
{
    MinimumLevel = LogLevel.Warning, // Reduce log volume
    IncludeStateSnapshots = false,   // Disable expensive operations
    MaxStateDataSize = 500,          // Limit data size
    EnableStructuredLogging = true   // Keep structured logging
};
```

## See Also

- [Resource Governance and Concurrency](resource-governance-and-concurrency.md) - Managing resource allocation and execution policies
- [Metrics and Observability](metrics-and-observability.md) - Monitoring and performance analysis
- [Debug and Inspection](debug-and-inspection.md) - Debugging and inspection capabilities
- [Examples](../../examples/) - Practical examples of integration and extensions
