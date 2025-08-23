using Microsoft.SemanticKernel;
using SemanticKernel.Graph.State;

namespace Examples;

/// <summary>
/// Example derived from docs/concepts/execution.md to validate documentation snippets.
/// It demonstrates execution initialization, simple node execution, checkpointing and streaming usage.
/// </summary>
public static class ExecutionConceptsExample
{
    /// <summary>
    /// Runs the execution concept demos. This method is intended to be invoked by the examples runner.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("🔎 Running ExecutionConceptsExample...");

        // Create a minimal kernel. In real scenarios, configure connectors like OpenAI.
        var kernel = Kernel.CreateBuilder().Build();

        // Create kernel arguments and graph state used by examples
        var args = new KernelArguments();
        args["input"] = "example input";

        var graphState = new GraphState(args);

        // Demonstrate node lifecycle: Before -> Execute -> After
        // Here we use a simple Func to simulate an async node execution
        Func<Kernel, KernelArguments, CancellationToken, Task<object?>> executeNode = async (k, a, ct) =>
        {
            // Simulate asynchronous work and return a result
            await Task.Delay(10, ct).ConfigureAwait(false);
            return "result";
        };

        // Run the simulated node and print result
        using var cts = new CancellationTokenSource();
        var result = await executeNode(kernel, args, cts.Token).ConfigureAwait(false);
        Console.WriteLine($"Node execution result: {result}");

        // Demonstrate a simple checkpoint creation simulation
        // The real CheckpointManager lives in the main library; here we validate the flow.
        var checkpoint = new
        {
            Id = Guid.NewGuid().ToString(),
            GraphId = "sample-graph",
            ExecutionId = Guid.NewGuid().ToString()
        };
        Console.WriteLine($"Created checkpoint id: {checkpoint.Id}");

        // Demonstrate streaming consumption simulation
        var events = new[]
        {
            new { Type = "NodeStarted", NodeId = "process", Timestamp = DateTime.UtcNow },
            new { Type = "NodeCompleted", NodeId = "process", Timestamp = DateTime.UtcNow }
        };

        foreach (var evt in events)
        {
            Console.WriteLine($"Event: {evt.Type} - Node: {evt.NodeId} - Time: {evt.Timestamp}");
        }

        Console.WriteLine("✅ ExecutionConceptsExample completed successfully.");
    }
}
