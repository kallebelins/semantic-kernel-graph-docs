using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;
using System.Threading;

// Minimal runnable example demonstrating how to construct a GraphExecutor,
// add function nodes, connect them, set a start node and execute the graph.
// This mirrors the example added to the examples project.
public static class GraphExecutorExample
{
    /// <summary>
    /// Entry point for the docs example. Call from example runner.
    /// </summary>
    public static async Task RunAsync()
    {
        // Create a minimal kernel for function invocation.
        var kernel = Kernel.CreateBuilder().Build();

        // Create simple kernel functions as lightweight delegates to be used by nodes.
        var fnA = KernelFunctionFactory.CreateFromMethod((KernelArguments args) =>
        {
            // Simple function: returns a greeting string
            return "Hello from A";
        }, "FnA");

        var fnB = KernelFunctionFactory.CreateFromMethod((KernelArguments args) =>
        {
            // Echo function: appends text to previous state
            var prev = args.ContainsName("message") ? args["message"]?.ToString() : string.Empty;
            return $"B received: {prev}";
        }, "FnB");

        // Wrap functions into graph nodes
        var nodeA = new FunctionGraphNode(fnA, "nodeA", "Start node A");
        var nodeB = new FunctionGraphNode(fnB, "nodeB", "Receiver node B");

        // Create executor and add nodes
        var executor = new GraphExecutor("ExampleGraph", "A tiny demo graph for docs");
        executor.AddNode(nodeA).AddNode(nodeB);

        // Connect A -> B and set start node
        executor.Connect("nodeA", "nodeB");
        executor.SetStartNode("nodeA");

        // Prepare initial kernel arguments / graph state
        var args = new KernelArguments();
        args["message"] = "Initial message";

        // Execute graph
        var result = await executor.ExecuteAsync(kernel, args, CancellationToken.None);

        // Print final result
        Console.WriteLine("Graph execution completed.");
        Console.WriteLine($"Final result: {result.GetValue<string>()}");
    }
}


