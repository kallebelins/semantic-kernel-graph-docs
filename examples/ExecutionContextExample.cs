using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.Streaming;

namespace Examples;

/// <summary>
/// Example demonstrating execution context and basic event emission as
/// documented in `execution-context.md`.
/// This example builds a small graph, executes it, and subscribes to
/// execution events to print lifecycle information to the console.
/// </summary>
public static class ExecutionContextExample
{
    /// <summary>
    /// Runs the execution context example.
    /// </summary>
    /// <returns>Task representing the asynchronous operation</returns>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Execution Context Example ===\n");

        // Create kernel and enable graph support
        var kernel = Kernel.CreateBuilder()
            .AddGraphSupport()
            .Build();

        // Build a simple graph executor
        var executor = new GraphExecutor("ExecutionContextDemo");

        // Create nodes that simulate work and record state
        var startNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() => "start", "start_fn", "Start node"),
            "start", "Start Node");

        var workNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) =>
            {
                // Demonstrate property manipulation on the execution arguments
                args["processedAt"] = DateTimeOffset.UtcNow;
                return "work-done";
            }, "work_fn", "Work node"),
            "work", "Work Node").StoreResultAs("workResult");

        var endNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() => "end", "end_fn", "End node"),
            "end", "End Node");

        // Add nodes and connect them
        executor.AddNode(startNode);
        executor.AddNode(workNode);
        executor.AddNode(endNode);
        executor.Connect("start", "work");
        executor.Connect("work", "end");
        executor.SetStartNode("start");

        // Subscribe to a simple event stream for execution events
        var streamingExecutor = new StreamingGraphExecutor(executor.Name, "Execution context demo wrapper");
        // Import the same nodes into streaming executor for demo purposes
        streamingExecutor.AddNode(startNode);
        streamingExecutor.AddNode(workNode);
        streamingExecutor.AddNode(endNode);
        streamingExecutor.Connect("start", "work");
        streamingExecutor.Connect("work", "end");
        streamingExecutor.SetStartNode("start");

        var args = new KernelArguments { ["input"] = "sample-input" };

        // Execute and print events
        await foreach (var @event in streamingExecutor.ExecuteStreamAsync(kernel, args))
        {
            // Print basic event info for learning purposes
            Console.WriteLine($"Event: {@event.EventType} - {@event.Timestamp:HH:mm:ss.fff}");
        }

        // Inspect final state
        Console.WriteLine("\n=== Final State ===");
        foreach (var kvp in args)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n✅ Execution context example completed!");
    }
}


