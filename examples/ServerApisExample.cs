using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Integration;

namespace Examples
{
    /// <summary>
    /// Example demonstrating minimal usage of GraphRestApi and GraphRestApiOptions.
    /// This example builds a minimal in-memory graph registry and calls the API wrapper
    /// to execute a synthetic graph request. It is intended for documentation validation
    /// and does not require network or LLM calls.
    /// </summary>
    public static class ServerApisExample
    {
        /// <summary>
        /// Runs the Server APIs example.
        /// </summary>
        public static async Task RunAsync()
        {
            Console.WriteLine("🌐 Running ServerApisExample - validates GraphRestApi usage");

            try
            {
                // Prepare a minimal fake registry that implements IGraphRegistry.
                // For documentation purposes we provide a lightweight in-memory stub.
                var registry = new InMemoryGraphRegistry();

                // Create a simple service provider and logger required by GraphRestApi.
                // The service provider exposes a minimal Kernel instance used by GraphRestApi to execute graphs.
                var services = new SimpleServiceProvider(Kernel.CreateBuilder().Build());
                var logger = new SimpleLogger<GraphRestApi>();

                // Configure API options mirroring documentation defaults.
                var apiOptions = new GraphRestApiOptions
                {
                    ApiKey = "doc-sample-key",
                    RequireAuthentication = false,
                    EnableRateLimiting = false,
                    EnableExecutionQueue = false,
                    MaxConcurrentExecutions = 4
                };

                // Create the API wrapper instance
                var graphApi = new GraphRestApi(registry, services, apiOptions, logger, null);

                // Execute a synthetic request to validate that the methods are callable.
                var request = new ExecuteGraphRequest
                {
                    GraphName = "__doc-synthetic__",
                    Variables = new Dictionary<string, object>
                    {
                        ["input"] = "test"
                    }
                };

                // Register a minimal GraphExecutor so ExecuteAsync can find and run it.
                var executor = new SemanticKernel.Graph.Core.GraphExecutor("__doc-synthetic__", "Synthetic doc graph");
                var startNode = new SimpleNodeExample();
                executor.AddNode(startNode).SetStartNode(startNode.NodeId);
                await registry.RegisterAsync(executor);

                // Call ExecuteAsync. The registered executor will run the simple node and return success.
                var response = await graphApi.ExecuteAsync(request, apiOptions.ApiKey);

                // Print basic response info for verification.
                Console.WriteLine($"ExecutionId: {response?.ExecutionId ?? "(null)"}");
                Console.WriteLine($"Success: {response?.Success}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ServerApisExample failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine();
        }
    }

    // Minimal in-memory registry stub for documentation/testing.
    internal class InMemoryGraphRegistry : IGraphRegistry
    {
        private readonly ConcurrentDictionary<string, SemanticKernel.Graph.Core.GraphExecutor> _executors = new();

        /// <summary>
        /// Registers a graph executor. Returns false when a graph with the same name already exists.
        /// </summary>
        public Task<bool> RegisterAsync(SemanticKernel.Graph.Core.GraphExecutor executor)
        {
            if (executor == null) throw new ArgumentNullException(nameof(executor));
            var added = _executors.TryAdd(executor.Name, executor);
            return Task.FromResult(added);
        }

        /// <summary>
        /// Unregisters a graph by name.
        /// </summary>
        public Task<bool> UnregisterAsync(string graphName)
        {
            if (string.IsNullOrWhiteSpace(graphName)) throw new ArgumentException("graphName is required", nameof(graphName));
            var removed = _executors.TryRemove(graphName, out _);
            return Task.FromResult(removed);
        }

        /// <summary>
        /// Gets a registered GraphExecutor instance or null when not found.
        /// </summary>
        public Task<SemanticKernel.Graph.Core.GraphExecutor?> GetAsync(string graphName)
        {
            if (string.IsNullOrWhiteSpace(graphName)) throw new ArgumentException("graphName is required", nameof(graphName));
            _executors.TryGetValue(graphName, out var executor);
            return Task.FromResult(executor);
        }

        /// <summary>
        /// Lists registered graphs as brief information snapshots.
        /// </summary>
        public Task<IReadOnlyList<RegisteredGraphInfo>> ListAsync()
        {
            var list = new List<RegisteredGraphInfo>();
            foreach (var kv in _executors)
            {
                var exec = kv.Value;
                list.Add(new RegisteredGraphInfo
                {
                    Name = exec.Name,
                    GraphId = exec.GraphId ?? string.Empty,
                    Description = exec.Description ?? string.Empty,
                    NodeCount = exec.NodeCount
                });
            }

            return Task.FromResult<IReadOnlyList<RegisteredGraphInfo>>(list);
        }
    }

    // Minimal service provider stub used only to satisfy GraphRestApi constructor.
    internal class SimpleServiceProvider : IServiceProvider
    {
        private readonly Kernel _kernel;

        public SimpleServiceProvider(Kernel kernel)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        public object? GetService(Type serviceType)
        {
            // Provide Kernel when requested; other services are not required for this example.
            if (serviceType == typeof(Kernel)) return _kernel;
            return null;
        }
    }

    // Minimal logger implementation for examples.
    internal sealed class SimpleLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
            if (exception != null) Console.WriteLine(exception);
        }
    }
}


