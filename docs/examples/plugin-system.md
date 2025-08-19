# Plugin System Example

This example demonstrates the advanced plugin system capabilities in Semantic Kernel Graph, including plugin registry, custom nodes, debugging tools, and marketplace functionality.

## Objective

Learn how to implement and manage advanced plugin systems in graph-based workflows to:
- Create and manage a comprehensive plugin registry
- Develop custom plugins with advanced capabilities
- Implement plugin conversion and integration systems
- Enable plugin debugging and profiling tools
- Create plugin marketplace with analytics and discovery
- Support hot-reloading and template systems

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Plugin Integration](../how-to/integration-and-extensions.md)
- Familiarity with [Custom Nodes](../concepts/node-types.md)

## Key Components

### Concepts and Techniques

- **Plugin Registry**: Centralized management of plugins with metadata and lifecycle
- **Custom Plugin Creation**: Development of specialized plugins with custom functionality
- **Plugin Conversion**: Automatic conversion of Semantic Kernel plugins to graph nodes
- **Debugging and Profiling**: Tools for plugin development and performance analysis
- **Marketplace Analytics**: Discovery, rating, and usage analytics for plugins
- **Hot-Reloading**: Dynamic plugin updates without system restart

### Core Classes

- `PluginRegistry`: Central registry for managing plugins and metadata
- `PluginMetadata`: Comprehensive metadata for plugin identification and categorization
- `CustomPluginNode`: Base class for creating custom plugin nodes
- `PluginConverter`: Converts Semantic Kernel plugins to graph-compatible nodes
- `PluginDebugger`: Debugging and profiling tools for plugin development
- `PluginMarketplace`: Marketplace functionality with discovery and analytics

## Running the Example

### Command Line

```bash
# Navigate to examples project
cd semantic-kernel-graph/src/SemanticKernel.Graph.Examples

# Run the Plugin System example
dotnet run -- --example plugin-system
```

### Programmatic Execution

```csharp
// Run the example directly
await PluginSystemExample.RunExampleAsync();

// Or run with custom logging
var loggerFactory = CreateCustomLoggerFactory();
await PluginSystemExample.RunExampleAsync();
```

## Step-by-Step Implementation

### 1. Plugin Registry Setup

The example starts by creating a comprehensive plugin registry.

```csharp
// Create plugin registry
var registry = new PluginRegistry(new PluginRegistryOptions
{
    MaxPlugins = 100,
    AllowPluginOverwrite = true,
    EnablePeriodicCleanup = true
}, loggerFactory.CreateLogger<PluginRegistry>());

// Register several plugins with different metadata
await RegisterSamplePluginsAsync(registry, logger);

// Demonstrate search functionality
await DemonstratePluginSearchAsync(registry, logger);

// Show analytics
var analytics = await registry.GetMarketplaceAnalyticsAsync();
Console.WriteLine($"📊 Marketplace Analytics:");
Console.WriteLine($"   Total plugins: {analytics.TotalPlugins}");
Console.WriteLine($"   Active plugins: {analytics.ActivePlugins}");
Console.WriteLine($"   Categories: {string.Join(", ", analytics.PluginsByCategory.Keys)}");
```

### 2. Plugin Registration

The example demonstrates comprehensive plugin registration with metadata.

```csharp
private static async Task RegisterSamplePluginsAsync(IPluginRegistry registry, ILogger logger)
{
    // Register a semantic search plugin
    var searchMetadata = new PluginMetadata
    {
        Id = "semantic-search-v1",
        Name = "Semantic Search Engine",
        Description = "Advanced semantic search with vector embeddings",
        Version = new PluginVersion(1, 2, 0),
        Author = "Graph Team",
        Category = PluginCategory.Cognitive,
        Tags = { "search", "semantic", "ai", "embeddings" },
        Dependencies = { "vector-db", "embedding-service" },
        License = "MIT",
        Repository = "https://github.com/semantic-kernel-graph/plugins",
        Documentation = "https://docs.semantic-kernel-graph.com/plugins/semantic-search",
        PerformanceMetrics = new PluginPerformanceMetrics
        {
            AverageLatency = TimeSpan.FromMilliseconds(150),
            ThroughputPerSecond = 100,
            MemoryUsageMB = 50,
            CpuUsagePercent = 15
        }
    };

    var searchPlugin = new SemanticSearchPlugin();
    await registry.RegisterPluginAsync(searchMetadata, searchPlugin);
    logger.LogInformation("✅ Registered semantic search plugin");

    // Register a data processing plugin
    var dataMetadata = new PluginMetadata
    {
        Id = "data-processor-v1",
        Name = "Data Processing Pipeline",
        Description = "High-performance data processing with parallel execution",
        Version = new PluginVersion(2, 0, 1),
        Author = "Data Team",
        Category = PluginCategory.DataProcessing,
        Tags = { "data", "processing", "pipeline", "parallel" },
        Dependencies = { "data-source", "processing-engine" },
        License = "Apache-2.0",
        PerformanceMetrics = new PluginPerformanceMetrics
        {
            AverageLatency = TimeSpan.FromMilliseconds(300),
            ThroughputPerSecond = 50,
            MemoryUsageMB = 100,
            CpuUsagePercent = 25
        }
    };

    var dataPlugin = new DataProcessingPlugin();
    await registry.RegisterPluginAsync(dataMetadata, dataPlugin);
    logger.LogInformation("✅ Registered data processing plugin");
}
```

### 3. Plugin Search and Discovery

The registry provides advanced search and discovery capabilities.

```csharp
private static async Task DemonstratePluginSearchAsync(IPluginRegistry registry, ILogger logger)
{
    Console.WriteLine("\n🔍 Plugin Search Demonstration:");

    // Search by category
    var cognitivePlugins = await registry.SearchPluginsAsync(new PluginSearchCriteria
    {
        Category = PluginCategory.Cognitive,
        MinVersion = new PluginVersion(1, 0, 0)
    });

    Console.WriteLine($"  Cognitive plugins: {cognitivePlugins.Count}");
    foreach (var plugin in cognitivePlugins.Take(3))
    {
        Console.WriteLine($"    - {plugin.Metadata.Name} v{plugin.Metadata.Version}");
    }

    // Search by tags
    var searchPlugins = await registry.SearchPluginsAsync(new PluginSearchCriteria
    {
        Tags = { "search", "semantic" },
        MaxLatency = TimeSpan.FromMilliseconds(200)
    });

    Console.WriteLine($"  Fast search plugins: {searchPlugins.Count}");
    foreach (var plugin in searchPlugins)
    {
        Console.WriteLine($"    - {plugin.Metadata.Name} ({plugin.Metadata.PerformanceMetrics.AverageLatency.TotalMilliseconds}ms)");
    }

    // Search by performance criteria
    var highPerformancePlugins = await registry.SearchPluginsAsync(new PluginSearchCriteria
    {
        MinThroughput = 50,
        MaxMemoryUsageMB = 100
    });

    Console.WriteLine($"  High-performance plugins: {highPerformancePlugins.Count}");
}
```

### 4. Custom Plugin Creation

The example demonstrates creating custom plugins with advanced capabilities.

```csharp
private static async Task DemonstrateCustomPluginCreationAsync(ILogger logger, ILoggerFactory loggerFactory)
{
    Console.WriteLine("\n🔧 2. Custom Plugin Creation");
    Console.WriteLine("-------------------------------");

    // Create a custom analytics plugin
    var analyticsPlugin = new CustomAnalyticsPlugin();
    var analyticsMetadata = new PluginMetadata
    {
        Id = "custom-analytics-v1",
        Name = "Custom Analytics Engine",
        Description = "Advanced analytics with custom algorithms",
        Version = new PluginVersion(1, 0, 0),
        Author = "Analytics Team",
        Category = PluginCategory.Analytics,
        Tags = { "analytics", "custom", "algorithms" }
    };

    // Register the custom plugin
    var registry = new PluginRegistry(new PluginRegistryOptions(), loggerFactory.CreateLogger<PluginRegistry>());
    await registry.RegisterPluginAsync(analyticsMetadata, analyticsPlugin);

    // Test the custom plugin
    var result = await analyticsPlugin.ExecuteAsync(new KernelArguments
    {
        ["data"] = "sample data for analysis",
        ["algorithm"] = "custom_algorithm_v1"
    });

    Console.WriteLine($"  Custom plugin result: {result}");
    registry.Dispose();
}

// Custom plugin implementation
public class CustomAnalyticsPlugin : CustomPluginNode
{
    public override string Name => "Custom Analytics";
    public override string Description => "Advanced analytics with custom algorithms";

    public override async Task<object> ExecuteAsync(KernelArguments arguments)
    {
        var data = arguments.TryGetValue("data", out var d) ? d?.ToString() ?? string.Empty : string.Empty;
        var algorithm = arguments.TryGetValue("algorithm", out var a) ? a?.ToString() ?? string.Empty : string.Empty;

        // Simulate custom analytics processing
        await Task.Delay(100); // Simulate processing time

        var result = new
        {
            InputData = data,
            Algorithm = algorithm,
            ProcessedAt = DateTime.UtcNow,
            Insights = new[] { "Custom insight 1", "Custom insight 2" },
            Confidence = 0.95
        };

        return result;
    }
}
```

### 5. Advanced Plugin Conversion

The system can automatically convert Semantic Kernel plugins to graph nodes.

```csharp
private static async Task DemonstrateAdvancedPluginConversionAsync(ILogger logger, ILoggerFactory loggerFactory)
{
    Console.WriteLine("\n🔄 3. Advanced Plugin Conversion");
    Console.WriteLine("----------------------------------");

    var converter = new PluginConverter(loggerFactory.CreateLogger<PluginConverter>());

    // Convert a Semantic Kernel plugin to a graph node
    var kernel = Kernel.CreateBuilder().Build();
    var semanticPlugin = kernel.ImportPluginFromObject(new SemanticKernelPlugin());

    var convertedNode = await converter.ConvertPluginToNodeAsync(semanticPlugin, "converted-semantic");
    
    Console.WriteLine($"  Converted plugin: {convertedNode.NodeId}");
    Console.WriteLine($"  Node type: {convertedNode.GetType().Name}");

    // Test the converted node
    var executor = new GraphExecutor("Conversion Test", "Testing converted plugin");
    executor.AddNode(convertedNode);
    executor.SetStartNode(convertedNode.NodeId);

    var result = await executor.ExecuteAsync(kernel, new KernelArguments
    {
        ["input"] = "test input for converted plugin"
    });

    Console.WriteLine($"  Conversion test result: {result}");
}

// Sample Semantic Kernel plugin for conversion
public class SemanticKernelPlugin
{
    [KernelFunction, Description("Process input with semantic understanding")]
    public string ProcessInput([Description("Input text to process")] string input)
    {
        return $"Processed: {input.ToUpperInvariant()}";
    }
}
```

### 6. Plugin Debugging and Profiling

The system provides comprehensive debugging and profiling tools.

```csharp
private static async Task DemonstratePluginDebuggingAsync(ILogger logger, ILoggerFactory loggerFactory)
{
    Console.WriteLine("\n🐛 4. Plugin Debugging and Profiling");
    Console.WriteLine("-------------------------------------");

    var debugger = new PluginDebugger(loggerFactory.CreateLogger<PluginDebugger>());
    var registry = new PluginRegistry(new PluginRegistryOptions(), loggerFactory.CreateLogger<PluginRegistry>());

    // Register a plugin for debugging
    var testPlugin = new TestPlugin();
    var testMetadata = new PluginMetadata
    {
        Id = "test-plugin-v1",
        Name = "Test Plugin",
        Description = "Plugin for debugging demonstration",
        Version = new PluginVersion(1, 0, 0)
    };

    await registry.RegisterPluginAsync(testMetadata, testPlugin);

    // Enable debugging for the plugin
    debugger.EnableDebugging(testPlugin.Id);

    // Run with debugging enabled
    var debugResult = await debugger.ExecuteWithDebuggingAsync(testPlugin, new KernelArguments
    {
        ["input"] = "debug test input"
    });

    Console.WriteLine($"  Debug execution result: {debugResult.Result}");
    Console.WriteLine($"  Debug info: {debugResult.DebugInfo.Count} debug points");

    // Get profiling information
    var profiling = await debugger.GetProfilingInfoAsync(testPlugin.Id);
    Console.WriteLine($"  Profiling: {profiling.ExecutionCount} executions, " +
                     $"avg time: {profiling.AverageExecutionTime.TotalMilliseconds:F2}ms");

    registry.Dispose();
}

// Test plugin for debugging
public class TestPlugin : CustomPluginNode
{
    public override string Name => "Test Plugin";
    public override string Description => "Plugin for debugging demonstration";

    public override async Task<object> ExecuteAsync(KernelArguments arguments)
    {
        var input = arguments.TryGetValue("input", out var i) ? i?.ToString() ?? string.Empty : string.Empty;
        
        // Simulate some processing
        await Task.Delay(50);
        
        return $"Test result: {input} processed at {DateTime.UtcNow:HH:mm:ss}";
    }
}
```

### 7. Plugin Marketplace Analytics

The marketplace provides comprehensive analytics and discovery features.

```csharp
private static async Task DemonstratePluginMarketplaceAsync(ILogger logger, ILoggerFactory loggerFactory)
{
    Console.WriteLine("\n🏪 5. Plugin Marketplace Analytics");
    Console.WriteLine("-----------------------------------");

    var marketplace = new PluginMarketplace(loggerFactory.CreateLogger<PluginMarketplace>());
    var registry = new PluginRegistry(new PluginRegistryOptions(), loggerFactory.CreateLogger<PluginRegistry>());

    // Register multiple plugins for marketplace analysis
    await RegisterMarketplacePluginsAsync(registry, logger);

    // Get marketplace analytics
    var analytics = await marketplace.GetAnalyticsAsync();
    
    Console.WriteLine($"📊 Marketplace Overview:");
    Console.WriteLine($"   Total plugins: {analytics.TotalPlugins}");
    Console.WriteLine($"   Active plugins: {analytics.ActivePlugins}");
    Console.WriteLine($"   Total downloads: {analytics.TotalDownloads}");
    Console.WriteLine($"   Average rating: {analytics.AverageRating:F2}/5.0");

    // Category breakdown
    Console.WriteLine($"\n📂 Category Breakdown:");
    foreach (var category in analytics.PluginsByCategory)
    {
        Console.WriteLine($"   {category.Key}: {category.Value.Count} plugins");
    }

    // Top plugins by rating
    var topPlugins = await marketplace.GetTopPluginsAsync(5, SortBy.Rating);
    Console.WriteLine($"\n⭐ Top Rated Plugins:");
    foreach (var plugin in topPlugins)
    {
        Console.WriteLine($"   {plugin.Metadata.Name}: {plugin.Rating:F1}/5.0 ({plugin.DownloadCount} downloads)");
    }

    registry.Dispose();
}
```

### 8. Hot-Reloading and Template System

The system supports dynamic plugin updates and template-based development.

```csharp
private static async Task DemonstrateHotReloadingAsync(ILogger logger, ILoggerFactory loggerFactory)
{
    Console.WriteLine("\n🔥 6. Hot-Reloading and Template System");
    Console.WriteLine("----------------------------------------");

    var hotReloader = new PluginHotReloader(loggerFactory.CreateLogger<PluginHotReloader>());
    var templateEngine = new PluginTemplateEngine(loggerFactory.CreateLogger<PluginTemplateEngine>());

    // Create a plugin from template
    var template = await templateEngine.GetTemplateAsync("basic-analytics");
    var pluginCode = await template.GenerateCodeAsync(new Dictionary<string, object>
    {
        ["pluginName"] = "Generated Analytics",
        ["description"] = "Auto-generated analytics plugin",
        ["category"] = "Analytics"
    });

    Console.WriteLine($"  Generated plugin code: {pluginCode.Length} characters");

    // Compile and load the plugin
    var compiledPlugin = await hotReloader.CompileAndLoadAsync(pluginCode);
    Console.WriteLine($"  Plugin compiled and loaded: {compiledPlugin.GetType().Name}");

    // Test the hot-reloaded plugin
    var result = await compiledPlugin.ExecuteAsync(new KernelArguments
    {
        ["data"] = "test data for hot-reloaded plugin"
    });

    Console.WriteLine($"  Hot-reload test result: {result}");

    // Demonstrate template system
    var availableTemplates = await templateEngine.GetAvailableTemplatesAsync();
    Console.WriteLine($"\n📋 Available Templates:");
    foreach (var templateInfo in availableTemplates)
    {
        Console.WriteLine($"   - {templateInfo.Name}: {templateInfo.Description}");
    }
}
```

## Expected Output

The example produces comprehensive output showing:

- 📚 Plugin registry setup and management
- 🔧 Custom plugin creation and registration
- 🔄 Advanced plugin conversion from Semantic Kernel
- 🐛 Plugin debugging and profiling capabilities
- 🏪 Plugin marketplace analytics and discovery
- 🔥 Hot-reloading and template system functionality
- ✅ Complete plugin system workflow execution

## Troubleshooting

### Common Issues

1. **Plugin Registration Failures**: Ensure plugin metadata is complete and valid
2. **Conversion Errors**: Check Semantic Kernel plugin compatibility and dependencies
3. **Debugging Failures**: Verify plugin debugging is enabled and logging is configured
4. **Hot-Reload Issues**: Ensure plugin code compilation and loading permissions

### Debugging Tips

- Enable detailed logging for plugin registry operations
- Use plugin debugging tools to trace execution flow
- Monitor plugin performance metrics and resource usage
- Verify template generation and compilation processes

## See Also

- [Plugin Integration](../how-to/integration-and-extensions.md)
- [Custom Nodes](../concepts/node-types.md)
- [Plugin Development](../how-to/plugin-development.md)
- [Debugging and Inspection](../how-to/debug-and-inspection.md)
- [Template System](../concepts/templates.md)
