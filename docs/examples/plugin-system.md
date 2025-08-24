# Plugin System Example

This example demonstrates the advanced plugin system capabilities in Semantic Kernel Graph, including plugin registry, custom nodes, debugging tools, and marketplace functionality.

## Objective

Learn how to implement and manage advanced plugin systems in graph-based workflows to:
* Create and manage a comprehensive plugin registry
* Develop custom plugins with advanced capabilities
* Implement plugin conversion and integration systems
* Enable plugin debugging and profiling tools
* Create plugin marketplace with analytics and discovery
* Support hot-reloading and template systems

## Prerequisites

* **.NET 8.0** or later
* **OpenAI API Key** configured in `appsettings.json`
* **Semantic Kernel Graph package** installed
* Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Plugin Integration](../how-to/integration-and-extensions.md)
* Familiarity with [Custom Nodes](../concepts/node-types.md)

## Key Components

### Concepts and Techniques

* **Plugin Registry**: Centralized management of plugins with metadata and lifecycle
* **Custom Plugin Creation**: Development of specialized plugins with custom functionality
* **Plugin Conversion**: Automatic conversion of Semantic Kernel plugins to graph nodes
* **Debugging and Profiling**: Tools for plugin development and performance analysis
* **Marketplace Analytics**: Discovery, rating, and usage analytics for plugins
* **Hot-Reloading**: Dynamic plugin updates without system restart

### Core Classes

* `PluginRegistry`: Central registry for managing plugins and metadata
* `PluginMetadata`: Comprehensive metadata for plugin identification and categorization
* `CustomPluginNode`: Base class for creating custom plugin nodes
* `PluginConverter`: Converts Semantic Kernel plugins to graph-compatible nodes
* `PluginDebugger`: Debugging and profiling tools for plugin development
* `PluginMarketplace`: Marketplace functionality with discovery and analytics

## Running the Example

### Getting Started

This example demonstrates the plugin system and dynamic loading with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Plugin Registry Setup

This minimal snippet shows the registry creation used by the runnable example `PluginSystemExample`.

```csharp
// Create a logger factory for examples
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

// Create plugin registry with conservative limits and a typed logger
var registry = new PluginRegistry(new PluginRegistryOptions
{
    MaxPlugins = 100,
    AllowPluginOverwrite = true,
    EnablePeriodicCleanup = true
}, loggerFactory.CreateLogger<PluginRegistry>());

// Basic marketplace analytics snapshot (async call)
var analytics = await registry.GetMarketplaceAnalyticsAsync();
Console.WriteLine($"Marketplace total plugins: {analytics.TotalPlugins}");
```

### 2. Plugin Registration

Register plugins with minimal metadata and a factory that produces the runtime node.

```csharp
// Minimal plugin metadata
var metadata = new PluginMetadata
{
    Id = "test-plugin",
    Name = "Test Plugin",
    Description = "A simple test plugin used by examples",
    Version = new PluginVersion(1, 0, 0),
    Category = PluginCategory.General
};

// Register with a factory that creates the graph node instance when requested
var result = await registry.RegisterPluginAsync(metadata, serviceProvider => new TestPluginNode());
if (!result.IsSuccess)
{
    Console.WriteLine($"Failed to register plugin: {result.ErrorMessage}");
}
```

### 3. Plugin Search and Discovery

Use the registry search API to find plugins matching simple criteria.

```csharp
// Find plugins in a specific category
var found = await registry.SearchPluginsAsync(new PluginSearchCriteria
{
    Category = PluginCategory.General
});

Console.WriteLine($"Found plugins: {found.Count}");
foreach (var p in found.Take(10))
{
    Console.WriteLine($" - {p.Name} ({p.Id}) v{p.Version}");
}
```

### 4. Custom Plugin Creation

Create a small custom plugin node that implements `IGraphNode`. The example project includes `TestPluginNode` used by the `PluginSystemExample`.

The runnable example demonstrates registering a plugin with a factory and creating an instance via the registry. Prefer creating reusable node classes in library code and keeping examples small and self-contained.

```csharp
// Example node factory used above: serviceProvider => new TestPluginNode()
// TestPluginNode implements IGraphNode and returns a simple FunctionResult.
```

### 5. Advanced Plugin Conversion

The codebase may include a `PluginConverter` implementation; if not, convert kernel plugins to graph nodes by creating metadata via `PluginMetadata.FromKernelPlugin` and implementing a node wrapper. The example project focuses on registry and execution; conversion utilities are optional and should be implemented in library code when required.

```csharp
// Example: Create metadata from a kernel plugin
var kernel = Kernel.CreateBuilder().Build();
// var kernelPlugin = kernel.ImportPluginFromObject(new SomeKernelPlugin());
// var metadata = PluginMetadata.FromKernelPlugin(kernelPlugin);
// registry.RegisterPluginAsync(metadata, sp => new ConvertedKernelNode(kernelPlugin));
```

### 6. Plugin Debugging and Profiling

The library provides a `PluginDebugger` that integrates with the `IPluginRegistry` to collect execution traces, generate reports and run lightweight profiling. The example below uses the public APIs available in the codebase (`PluginDebugger`, `IPluginDebugSession`) and keeps the flow minimal and reproducible.

```csharp
// Create debugger and registry (use existing loggerFactory from examples)
var registry = new PluginRegistry(new PluginRegistryOptions(), loggerFactory.CreateLogger<PluginRegistry>());
var debugger = new PluginDebugger(registry, null, loggerFactory.CreateLogger<PluginDebugger>());

// Register or ensure a plugin with id 'test-plugin' exists in the registry before debugging
// registry.RegisterPluginAsync(metadata, sp => new TestPluginNode());

// Start a debug session for the plugin
var session = await debugger.StartDebugSessionAsync("test-plugin", new PluginDebugConfiguration
{
    EnableTracing = true,
    EnableProfiling = false,
    LogExecutionSteps = true
});

// Capture a lightweight execution trace using the session
var trace = await session.TraceExecutionAsync(new KernelArguments { ["input"] = "debug test input" });
Console.WriteLine($"Trace captured: {trace.Steps.Count} steps for plugin {trace.PluginId}");

// Generate a debug report (includes session summaries and optional execution history)
var report = await debugger.GenerateDebugReportAsync("test-plugin");
Console.WriteLine($"Debug report generated for {report.PluginName} at {report.GeneratedAt}");

// Optionally profile resource usage for the plugin (simulated profile duration)
var profile = await debugger.ProfilePluginResourceUsageAsync("test-plugin", new PluginProfilingOptions { Duration = TimeSpan.FromSeconds(1) });
Console.WriteLine($"Profile: peak memory {profile.PeakMemoryUsage} MB, peak CPU {profile.PeakCpuUsage}%");

// Dispose session when finished
session.Dispose();
```

### 7. Plugin Marketplace Analytics

The `PluginRegistry` provides a simple analytics snapshot that is suitable for documentation examples. For richer marketplace features implement a separate service that aggregates registry statistics and marketplace metadata.

```csharp
// Use the registry analytics helper to get a quick overview
var analytics = await registry.GetMarketplaceAnalyticsAsync();
Console.WriteLine($"Total plugins: {analytics.TotalPlugins}");
foreach (var kv in analytics.PluginsByCategory)
{
    Console.WriteLine($"  {kv.Key}: {kv.Value}");
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

* 📚 Plugin registry setup and management
* 🔧 Custom plugin creation and registration
* 🔄 Advanced plugin conversion from Semantic Kernel
* 🐛 Plugin debugging and profiling capabilities
* 🏪 Plugin marketplace analytics and discovery
* 🔥 Hot-reloading and template system functionality
* ✅ Complete plugin system workflow execution

## Troubleshooting

### Common Issues

1. **Plugin Registration Failures**: Ensure plugin metadata is complete and valid
2. **Conversion Errors**: Check Semantic Kernel plugin compatibility and dependencies
3. **Debugging Failures**: Verify plugin debugging is enabled and logging is configured
4. **Hot-Reload Issues**: Ensure plugin code compilation and loading permissions

### Debugging Tips

* Enable detailed logging for plugin registry operations
* Use plugin debugging tools to trace execution flow
* Monitor plugin performance metrics and resource usage
* Verify template generation and compilation processes

## See Also

* [Plugin Integration](../how-to/integration-and-extensions.md)
* [Custom Nodes](../concepts/node-types.md)
* [Plugin Development](../how-to/plugin-development.md)
* [Debugging and Inspection](../how-to/debug-and-inspection.md)
* [Template System](../concepts/templates.md)
