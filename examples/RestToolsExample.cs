using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Integration;
using SemanticKernel.Graph.Nodes;

namespace Examples;

/// <summary>
/// Demonstrates how to configure and execute a simple REST tool node using
/// <see cref="RestToolSchema"/> and <see cref="RestToolGraphNode"/>.
/// The example performs a GET request against a public echo service to
/// validate request mapping and response handling.
/// </summary>
public static class RestToolsExample
{
    /// <summary>
    /// Runs the REST tools example.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("🔗 Running RestToolsExample - simple GET against postman-echo.com");

        try
        {
            // 1) Define a minimal REST tool schema that maps query parameter 'foo' from state
            var schema = new RestToolSchema
            {
                Id = "postman.echo.get",
                Name = "Postman Echo GET",
                Description = "Simple GET to Postman Echo that returns provided query parameters",
                BaseUri = new Uri("https://postman-echo.com"),
                Path = "/get",
                Method = HttpMethod.Get,
                // Map server query parameter 'foo' to kernel argument key 'foo'
                QueryParameters =
                {
                    ["foo"] = "foo"
                },
                Headers =
                {
                    ["User-Agent"] = ":Examples/1.0"
                },
                CacheEnabled = false,
                TimeoutSeconds = 10
            };

            // 2) Create an HttpClient and the REST node
            using var httpClient = new HttpClient();
            var restNode = new RestToolGraphNode(schema, httpClient);

            // 3) Build a graph and register the node as start
            var graph = new GraphExecutor("rest-tools-graph", "Demo graph for REST tools");
            graph.AddNode(restNode).SetStartNode(schema.Id);

            // 4) Minimal kernel for execution (no LLM required for this demo)
            var kernel = Kernel.CreateBuilder().Build();

            // 5) Prepare arguments that map to the schema (this will be sent as ?foo=bar)
            var args = new KernelArguments { ["foo"] = "bar-value" };

            // 6) Execute the graph and inspect results
            var result = await graph.ExecuteAsync(kernel, args);

            // The REST node stores HTTP metadata in the FunctionResult.Metadata dictionary.
            // Retrieve status code and response body from metadata to avoid using non-existent overloads.
            var status = result.Metadata.TryGetValue("status_code", out var statusObj) && statusObj is int s ? s : -1;
            var body = result.Metadata.TryGetValue("response_body", out var bodyObj) ? bodyObj?.ToString() ?? string.Empty : string.Empty;

            Console.WriteLine($"HTTP status: {status}");
            Console.WriteLine("Response body (truncated):");
            Console.WriteLine(body?.Substring(0, Math.Min(800, body.Length)) ?? string.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ RestToolsExample failed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }

        Console.WriteLine();
    }
}


