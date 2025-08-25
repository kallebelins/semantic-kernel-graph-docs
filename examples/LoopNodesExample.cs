using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;
using SemanticKernel.Graph.Extensions;
using System;
using System.Threading.Tasks;

namespace Examples;

/// <summary>
/// Minimal runnable loops example for documentation.
/// This example demonstrates a simple while-loop that increments a counter
/// using the real loop node implementation. All comments are in English
/// and the code is designed to compile inside the docs examples project.
/// </summary>
public static class LoopNodesExample
{
    /// <summary>
    /// Runs a minimal while-loop example that increments a counter until it
    /// reaches a configured maximum value.
    /// </summary>
    public static async Task RunAsync()
    {
        // Build a minimal kernel instance. No external LLM is required for this example.
        var kernel = Kernel.CreateBuilder()
            .AddGraphSupport()
            .Build();

        // Create a lightweight graph state and initialize loop variables.
        var state = new GraphState();
        state.SetValue("counter", 0);
        state.SetValue("max_count", 3); // Stop after reaching this value

        // Create a while loop node that runs while 'counter' < 'max_count'.
        var whileLoop = new WhileLoopGraphNode(
            condition: s => s.GetValue<int>("counter") < s.GetValue<int>("max_count"),
            maxIterations: 100,
            nodeId: "doc_while_loop",
            name: "doc_counter_loop",
            description: "Increments counter until max_count"
        );

        // Create a function that increments the counter in the graph state.
        var incrementFn = KernelFunctionFactory.CreateFromMethod((KernelArguments args) =>
        {
            // Retrieve the graph state associated with the current execution.
            var graphState = args.GetOrCreateGraphState();
            var v = graphState.GetValue<int>("counter");
            graphState.SetValue("counter", v + 1);
            return $"counter={v + 1}"; // Return a human-readable result
        }, "doc_increment", "Increment counter");

        // Wrap the function in a FunctionGraphNode and add it to the loop.
        var incrementNode = new FunctionGraphNode(incrementFn, "doc_increment_node");
        whileLoop.AddLoopNode(incrementNode);

        // Execute the loop and report results.
        Console.WriteLine("[doc] Starting while loop example...");
        var iterationsResult = await whileLoop.ExecuteAsync(kernel, state.KernelArguments);

        // iterationsResult contains a FunctionResult — convert to string/int as needed.
        var iterations = iterationsResult == null ? 0 : int.TryParse(iterationsResult.ToString(), out var it) ? it : 0;

        Console.WriteLine($"[doc] Loop finished after {iterations} iterations");
        Console.WriteLine($"[doc] Final counter value: {state.GetValue<int>("counter")}");
    }

    /// <summary>
    /// Demonstrates a simple while loop that increments a counter until a max value.
    /// </summary>
    public static async Task Run2Async()
    {
        // Create a local kernel (no external AI calls required for this basic demo)
        var kernel = Kernel.CreateBuilder().Build();

        // Create graph state and initialize values
        var state = new GraphState();
        state.SetValue("counter", 0);
        state.SetValue("max_count", 3);

        // Create a WhileLoopGraphNode using a simple condition function
        var whileLoop = new WhileLoopGraphNode(
            condition: s => s.GetValue<int>("counter") < s.GetValue<int>("max_count"),
            maxIterations: 10,
            nodeId: "doc_while_loop",
            name: "doc_counter_loop",
            description: "Increments counter until max_count"
        );

        // Create a function node that increments the counter
        // Create a kernel function that captures the outer `state` instance (avoids needing Graph extensions)
        var incrementFn = KernelFunctionFactory.CreateFromMethod((KernelArguments args) =>
        {
            // Use the captured GraphState to update the counter
            var v = state.GetValue<int>("counter");
            state.SetValue("counter", v + 1);
            return $"counter={v + 1}";
        }, "doc_increment", "Increment counter");

        var incrementNode = new FunctionGraphNode(incrementFn, "doc_increment_node");

        // Add the increment node to the loop and execute
        whileLoop.AddLoopNode(incrementNode);

        Console.WriteLine("[doc] Starting while loop example...");
        var iterations = await whileLoop.ExecuteAsync(kernel, state.KernelArguments);
        Console.WriteLine($"[doc] Loop finished after {iterations} iterations");
        Console.WriteLine($"[doc] Final counter value: {state.GetValue<int>("counter")}");
    }
}


