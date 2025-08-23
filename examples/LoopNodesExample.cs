// Loop nodes example for documentation - runnable and validated against the examples project
// Comments are in English and intended for readers of all levels.
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;

namespace Examples;

/// <summary>
/// Minimal, self-contained loop nodes example used by the documentation.
/// This file mirrors the documented snippets and is intended to compile and run inside the examples project.
/// </summary>
public static class LoopNodesExample
{
    /// <summary>
    /// Demonstrates a simple while loop that increments a counter until a max value.
    /// </summary>
    public static async Task RunAsync()
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


