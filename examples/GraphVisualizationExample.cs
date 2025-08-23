using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Debug;
using SemanticKernel.Graph.Nodes;

namespace Examples;

/// <summary>
/// Example demonstrating the GraphVisualizationEngine and GraphInspectionApi usage.
/// This example builds a tiny graph, generates visualization data and prints
/// DOT, Mermaid and JSON outputs to validate the documentation samples.
/// </summary>
public static class GraphVisualizationExample
{
    /// <summary>
    /// Runs the visualization example.
    /// </summary>
    public static async System.Threading.Tasks.Task RunAsync()
    {
        // Create a minimal kernel instance for APIs that require it.
        var kernel = Kernel.CreateBuilder().Build();

        // Create two simple function nodes (reusing lightweight FunctionGraphNode)
        var fn1 = KernelFunctionFactory.CreateFromMethod(() => "node1-output", "Fn1");
        var fn2 = KernelFunctionFactory.CreateFromMethod(() => "node2-output", "Fn2");

        var node1 = new FunctionGraphNode(fn1, "node1", "Node 1");
        var node2 = new FunctionGraphNode(fn2, "node2", "Node 2");

        // Build visualization data manually for the purpose of the example
        var nodes = new List<IGraphNode> { node1, node2 };
        var edges = new List<GraphEdgeInfo> { new GraphEdgeInfo("node1", "node2", "to-node2") };

        var visualizationData = new GraphVisualizationData(nodes, edges, currentNode: node2, executionPath: nodes);

        // Create engine and produce outputs
        using var engine = new GraphVisualizationEngine();

        // Generate DOT
        var dot = engine.SerializeToDot(visualizationData, new DotSerializationOptions { GraphName = "VizExample" });
        Console.WriteLine("--- DOT Output ---");
        Console.WriteLine(dot);

        // Generate Mermaid
        var mermaid = engine.GenerateEnhancedMermaidDiagram(visualizationData, new MermaidGenerationOptions { Direction = "TD" });
        Console.WriteLine("--- Mermaid Output ---");
        Console.WriteLine(mermaid);

        // Generate JSON
        var json = engine.SerializeToJson(visualizationData, new JsonSerializationOptions { Indented = true });
        Console.WriteLine("--- JSON Output ---");
        Console.WriteLine(json);

        await System.Threading.Tasks.Task.CompletedTask;
    }
}


