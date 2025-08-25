using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Integration;

namespace Examples;

/// <summary>
/// Minimal REST API example adapted from documentation `rest-api.md`.
/// This example validates `GraphRestApi` usage without starting an HTTP server.
/// It registers a synthetic graph and calls `GraphRestApi.ExecuteAsync` directly.
/// </summary>
public static class RestApiExample
{
    /// <summary>
    /// Runs the REST API example.
    /// </summary>
    public static async Task RunAsync(string[] args)
    {
        Console.WriteLine("🌐 Running RestApiExample - validates REST API documentation sample");

        // Build a minimal kernel instance used by GraphRestApi to execute graphs.
        var kernel = Kernel.CreateBuilder().Build();

        // Minimal service provider exposing Kernel instance.
        var services = new SimpleServiceProvider(kernel);

        // Minimal in-memory registry and logger (reuse existing doc stubs if present)
        var registry = new InMemoryGraphRegistry();
        var logger = new SimpleLogger<GraphRestApi>();

        var apiOptions = new GraphRestApiOptions
        {
            ApiKey = "doc-sample-key",
            RequireAuthentication = false,
            EnableRateLimiting = false,
            EnableExecutionQueue = false,
            MaxConcurrentExecutions = 4
        };

        var api = new GraphRestApi(registry, services, apiOptions, logger, null);

        // Create and register a synthetic executor
        var executor = new SemanticKernel.Graph.Core.GraphExecutor("sample-graph", "Synthetic sample for docs");
        var startNode = new SimpleNodeExample();
        executor.AddNode(startNode).SetStartNode(startNode.NodeId);
        await registry.RegisterAsync(executor);

        var request = new ExecuteGraphRequest
        {
            GraphName = "sample-graph",
            Variables = new Dictionary<string, object>
            {
                ["input"] = "Hello from REST docs example"
            }
        };

        var resp = await api.ExecuteAsync(request, apiOptions.ApiKey);

        Console.WriteLine($"Execution success: {resp.Success}, ExecutionId: {resp.ExecutionId}");
        Console.WriteLine();
    }
}


