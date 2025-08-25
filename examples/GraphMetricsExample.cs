using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

namespace Example;

// Documentation example for Metrics and Observability
// - Comments are written in English and aimed to be clear for developers of any level.
// - This file is intended to be included in the docs folder as an executable example snippet.

public static class GraphMetricsExample
{
    /// <summary>
    /// Demonstrates enabling basic metrics on a GraphExecutor and running it a few times
    /// so that metrics are collected. This code mirrors the examples used in the docs.
    /// </summary>
    public static async Task RunBasicMetricsDemoAsync()
    {
        // Create a minimal kernel (in docs we use a mock or configured model in real examples)
        var kernel = Kernel.CreateBuilder()
            .Build();

        // Create a graph executor and enable development metrics (detailed, frequent sampling)
        var graph = new GraphExecutor("DocsMetricsGraph", "Simple graph used in docs");
        graph.EnableDevelopmentMetrics();

        // Add simple function nodes that simulate work.
        var node1 = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() =>
            {
                // Simulate work
                System.Threading.Thread.Sleep(100);
                return "Hello";
            }, "greeting"),
            "greeting",
            "Greeting");

        var node2 = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() =>
            {
                System.Threading.Thread.Sleep(150);
                return "Processed";
            }, "processing"),
            "processing",
            "Processing");

        // Build graph flow
        graph.AddNode(node1)
             .AddNode(node2)
             .Connect(node1.NodeId, node2.NodeId)
             .SetStartNode(node1.NodeId);

        // Execute the graph multiple times to generate metrics data
        for (int i = 0; i < 5; i++)
        {
            try
            {
                await graph.ExecuteAsync(kernel, new KernelArguments());
                Console.WriteLine($"Run #{i + 1} completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Run #{i + 1} failed: {ex.Message}");
            }
        }

        // Retrieve a simple performance summary and print key values
        var summary = graph.GetPerformanceSummary(TimeSpan.FromMinutes(5));
        if (summary != null)
        {
            Console.WriteLine("\nPERFORMANCE SUMMARY");
            Console.WriteLine($"Total Executions: {summary.TotalExecutions}");
            Console.WriteLine($"Success Rate: {summary.SuccessRate:F1}%");
            Console.WriteLine($"Average Execution Time: {summary.AverageExecutionTime.TotalMilliseconds:F2}ms");
        }
    }
}
