using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Integration;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.State;

namespace Examples;

/// <summary>
/// Minimal runnable example for the plugin system documentation.
/// This file implements a subset of the documented flows to ensure the
/// documentation code compiles and runs inside the Examples project.
/// </summary>
public static class PluginSystemExample
{
    /// <summary>
    /// Runs the plugin system demonstration used by the documentation.
    /// </summary>
    public static async Task RunExampleAsync()
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = loggerFactory.CreateLogger("PluginSystemExample");

        Console.WriteLine("-- Plugin System Example Starting --");

        // Create a plugin registry and register a simple test plugin
        var registry = new PluginRegistry(new PluginRegistryOptions { MaxPlugins = 100, AllowPluginOverwrite = true }, loggerFactory.CreateLogger<PluginRegistry>());

        // Register a simple test plugin using a factory that returns an IGraphNode
        var metadata = new PluginMetadata
        {
            Id = "test-plugin",
            Name = "Test Plugin",
            Description = "A simple test plugin used by examples",
            Version = new PluginVersion(1, 0, 0),
            Category = PluginCategory.General
        };

        // Factory returns a TestPluginNode instance
        await registry.RegisterPluginAsync(metadata, sp => new TestPluginNode());
        Console.WriteLine("Registered plugin: test-plugin");

        // Create instance via registry to validate factory path
        var serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        serviceCollection.AddSingleton(loggerFactory);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var instance = await registry.CreatePluginInstanceAsync("test-plugin", serviceProvider);
        Console.WriteLine($"Created plugin instance: {instance?.Name ?? "null"}");

        // Execute the plugin if it implements IGraphNode
        if (instance is IGraphNode node)
        {
            var kernel = Kernel.CreateBuilder().Build();
            var args = new KernelArguments { ["input"] = "hello from example" };

            var result = await node.ExecuteAsync(kernel, args);
            Console.WriteLine($"Execution result: {result?.GetValue<object>()}");
        }

        registry.Dispose();

        Console.WriteLine("-- Plugin System Example Completed --");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Minimal test plugin node implementing IGraphNode used only for examples.
    /// </summary>
    private class TestPluginNode : IGraphNode
    {
        public string NodeId { get; } = "test-plugin-node";
        public string Name { get; } = "Test Plugin Node";
        public string Description { get; } = "A test plugin node for examples";
        public IReadOnlyDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
        public bool IsExecutable => true;
        public IReadOnlyList<string> InputParameters => Array.Empty<string>();
        public IReadOnlyList<string> OutputParameters => Array.Empty<string>();

        public async Task<FunctionResult> ExecuteAsync(Kernel kernel, KernelArguments arguments, System.Threading.CancellationToken cancellationToken = default)
        {
            // Produce a simple string result that demonstrates execution flow.
            var output = $"test-plugin executed; input={arguments["input"]?.ToString() ?? string.Empty}";
            var tempFunction = KernelFunctionFactory.CreateFromMethod(() => output);
            var functionResult = new FunctionResult(tempFunction, output);
            await Task.CompletedTask;
            return functionResult;
        }

        public ValidationResult ValidateExecution(KernelArguments arguments) => new ValidationResult();

        public IEnumerable<IGraphNode> GetNextNodes(FunctionResult? executionResult, GraphState graphState) => Array.Empty<IGraphNode>();

        public bool ShouldExecute(GraphState graphState) => true;

        public Task OnBeforeExecuteAsync(Kernel kernel, KernelArguments arguments, System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task OnAfterExecuteAsync(Kernel kernel, KernelArguments arguments, FunctionResult result, System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task OnExecutionFailedAsync(Kernel kernel, KernelArguments arguments, Exception exception, System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}


