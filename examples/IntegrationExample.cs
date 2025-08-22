using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Integration;
using SemanticKernel.Graph;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.State;
using Microsoft.Extensions.DependencyInjection;

namespace Examples
{
    /// <summary>
    /// Example demonstrating integration APIs: logger, registries, and sanitizer.
    /// This file mirrors the documentation at docs/api/integration.md and provides
    /// an executable example suitable for inclusion in the Examples project.
    /// </summary>
    public static class IntegrationExample
    {
        /// <summary>
        /// Runs the integration example showcasing logger, graph registry, tool registry,
        /// plugin registry usage and data sanitization.
        /// </summary>
        public static async Task RunAsync()
        {
            // Create a simple console logger using Microsoft.Extensions.Logging
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            var logger = loggerFactory.CreateLogger("IntegrationExample");

            // Create SemanticKernelGraphLogger with basic GraphOptions
            var graphLogger = new SemanticKernelGraphLogger(logger, new GraphOptions
            {
                EnableLogging = true,
                Logging = new GraphLoggingOptions
                {
                    MinimumLevel = LogLevel.Information,
                    EnableStructuredLogging = true,
                    EnableCorrelationIds = true,
                    IncludeTimings = true
                }
            });

            Console.WriteLine("-- Integration Example Starting --");

            // Demonstrate registry: register a simple in-memory GraphExecutor
            var registry = new GraphRegistry(loggerFactory.CreateLogger<GraphRegistry>());

            // Create graph executor and pass the graph logger to the constructor
            var graphExecutor = new GraphExecutor("demo-graph", "Demo graph for integration example", graphLogger);

            var registered = await registry.RegisterAsync(graphExecutor);
            Console.WriteLine($"Graph registered: {registered}");

            // List registered graphs
            var graphs = await registry.ListAsync();
            foreach (var g in graphs)
            {
                Console.WriteLine($"Registered graph: {g.Name} (nodes: {g.NodeCount})");
            }

            // Demonstrate tool registry with a dummy tool node factory
            var toolRegistry = new ToolRegistry(loggerFactory.CreateLogger<ToolRegistry>());

            var toolMetadata = new ToolMetadata
            {
                Id = "dummy_tool",
                Name = "Dummy Tool",
                Description = "A dummy tool used for examples",
                Type = ToolType.Local
            };

            // Build a minimal service provider to satisfy registry/create calls that expect non-null
            var services = new ServiceCollection();
            services.AddSingleton(loggerFactory);
            services.AddSingleton(logger);
            var serviceProvider = services.BuildServiceProvider();

            await toolRegistry.RegisterAsync(toolMetadata, sp => new DummyToolNode());

            var toolNode = await toolRegistry.CreateNodeAsync("dummy_tool", serviceProvider: serviceProvider);
            Console.WriteLine($"Tool node created: {toolNode?.NodeId ?? "null"}");

            // Demonstrate data sanitization
            var policy = new SensitiveDataPolicy
            {
                Enabled = true,
                Level = SanitizationLevel.Basic,
                RedactionText = "***REDACTED***",
                MaskAuthorizationBearerToken = true,
                SensitiveKeySubstrings = new[] { "password", "secret", "token", "api_key" }
            };

            var sanitizer = new SensitiveDataSanitizer(policy);

            var sensitiveData = new Dictionary<string, object?>
            {
                ["username"] = "alice",
                ["password"] = "p@ssword",
                ["api_key"] = "sk-FAKE-KEY"
            };

            // Cast to IDictionary to disambiguate overload between IDictionary and IReadOnlyDictionary
            var sanitized = sanitizer.Sanitize((IDictionary<string, object?>)sensitiveData);
            Console.WriteLine($"Sanitized contains password: {sanitized.ContainsKey("password")}");

            Console.WriteLine("-- Integration Example Completed --");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Minimal dummy tool node used solely for the example. Implements IGraphNode.
        /// This implementation follows the project's IGraphNode contract and returns a
        /// simple FunctionResult to ensure compatibility with the examples runner.
        /// </summary>
        private class DummyToolNode : IGraphNode
        {
            public string NodeId { get; } = "dummy_tool";

            public string Name { get; } = "Dummy Tool Node";

            public string Description { get; } = "A dummy tool node used for examples";

            public IReadOnlyDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();

            public bool IsExecutable => true;

            public IReadOnlyList<string> InputParameters => Array.Empty<string>();

            public IReadOnlyList<string> OutputParameters => Array.Empty<string>();

            public async Task<FunctionResult> ExecuteAsync(Kernel kernel, KernelArguments arguments, System.Threading.CancellationToken cancellationToken = default)
            {
                // Simple execution that returns a textual confirmation as the function result.
                var output = "dummy-executed";

                // Create a lightweight kernel function to back the FunctionResult.
                var tempFunction = KernelFunctionFactory.CreateFromMethod(() => output);
                var functionResult = new FunctionResult(tempFunction, output);

                await Task.CompletedTask;
                return functionResult;
            }

            public ValidationResult ValidateExecution(KernelArguments arguments)
            {
                // This dummy node always validates successfully in the example.
                return new ValidationResult();
            }

            public IEnumerable<IGraphNode> GetNextNodes(FunctionResult? executionResult, GraphState graphState)
            {
                return Array.Empty<IGraphNode>();
            }

            public bool ShouldExecute(GraphState graphState) => true;

            public Task OnBeforeExecuteAsync(Kernel kernel, KernelArguments arguments, System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task OnAfterExecuteAsync(Kernel kernel, KernelArguments arguments, FunctionResult result, System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task OnExecutionFailedAsync(Kernel kernel, KernelArguments arguments, Exception exception, System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
        }
    }
}
