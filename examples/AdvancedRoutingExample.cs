using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

namespace Examples;

/// <summary>
/// Advanced Routing example adapted from documentation to be runnable.
/// All public methods have English comments and the example focuses on
/// validating code snippets shown in the docs/advanced-routing.md file.
/// </summary>
public static class AdvancedRoutingExample
{
    /// <summary>
    /// Run the advanced routing demonstration used by docs.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("🔀 Advanced Routing Example (docs)");

        // Create a minimal kernel with graph support for the example
        var kernel = Kernel.CreateBuilder()
            .AddGraphSupport()
            .Build();

        // Create a graph executor instance used to compose the demo graph
        var graph = new GraphExecutor(kernel, logger: null);

        // Create demo nodes based on the documented snippets
        var startNode = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod((string input) => $"Analyzed: {input}", functionName: "Analyze", description: "Analyze input"),
            nodeId: "start",
            description: "Start node that analyzes input")
            .StoreResultAs("analysis");

        var semanticNode = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod((string input) => $"Semantic processed: {input}", functionName: "SemanticProcess", description: "Semantic processing"),
            nodeId: "semantic",
            description: "Semantic node")
            .StoreResultAs("semantic_out");

        var statisticalNode = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod((string input) => $"Stat result: {input}", functionName: "StatProcess", description: "Statistical processing"),
            nodeId: "statistical",
            description: "Statistical node")
            .StoreResultAs("stat_out");

        var hybridNode = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod((string input) => $"Hybrid processed: {input}", functionName: "HybridProcess", description: "Hybrid processing"),
            nodeId: "hybrid",
            description: "Hybrid node")
            .StoreResultAs("hybrid_out");

        var errorNode = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod(() => "Handled error", functionName: "HandleError", description: "Error handler"),
            nodeId: "error",
            description: "Error handler node")
            .StoreResultAs("error_out");

        var summaryNode = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod((string input) => $"Summary: {input}", functionName: "Summary", description: "Summarize results"),
            nodeId: "summary",
            description: "Summary node")
            .StoreResultAs("summary");

        // Add nodes and configure start node
        graph.AddNode(startNode);
        graph.AddNode(semanticNode);
        graph.AddNode(statisticalNode);
        graph.AddNode(hybridNode);
        graph.AddNode(errorNode);
        graph.AddNode(summaryNode);
        graph.SetStartNode(startNode.NodeId);

        // Connect edges using simple predicates that mirror the docs examples
        // The ConnectWhen condition expects KernelArguments (not GraphState) so use KernelArguments directly
        graph.ConnectWhen(startNode.NodeId, semanticNode.NodeId, ka => ka.ContainsKey("input") && ka["input"]?.ToString()?.Contains("semantic", StringComparison.OrdinalIgnoreCase) == true);
        graph.ConnectWhen(startNode.NodeId, statisticalNode.NodeId, ka => ka.ContainsKey("input") && ka["input"]?.ToString()?.Contains("stat", StringComparison.OrdinalIgnoreCase) == true);
        graph.ConnectWhen(startNode.NodeId, hybridNode.NodeId, ka => ka.ContainsKey("input") && ka["input"]?.ToString()?.Contains("hybrid", StringComparison.OrdinalIgnoreCase) == true);
        graph.ConnectWhen(startNode.NodeId, errorNode.NodeId, ka => ka.ContainsKey("error") && ka["error"]?.ToString() == "true");

        // All processing nodes lead to summary
        graph.Connect(semanticNode.NodeId, summaryNode.NodeId);
        graph.Connect(statisticalNode.NodeId, summaryNode.NodeId);
        graph.Connect(hybridNode.NodeId, summaryNode.NodeId);
        graph.Connect(errorNode.NodeId, summaryNode.NodeId);

        // Run a few demo inputs described in the docs
        var demoInputs = new[]
        {
            "This is a semantic request",
            "Perform stat analysis",
            "Use hybrid approach",
            "error"
        };

        foreach (var input in demoInputs)
        {
            var args = new KernelArguments { ["input"] = input, ["error"] = input == "error" ? "true" : "false" };
            var result = await graph.ExecuteAsync(kernel, args);
            // FunctionResult does not expose routing metadata directly; print output value and basic diagnostic info
            var outVal = result?.GetValue<object>()?.ToString() ?? string.Empty;
            Console.WriteLine($"Input: {input} -> Output: {outVal}");
        }

        Console.WriteLine("✅ Advanced routing docs example completed.");
    }
}


