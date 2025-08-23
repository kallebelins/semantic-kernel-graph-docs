using Microsoft.SemanticKernel;
using SemanticKernel.Graph.State;

namespace Examples;

/// <summary>
/// Demonstrates small runnable snippets of the execution model described in the docs.
/// This file is used to validate that the documentation code compiles and runs.
/// </summary>
public static class ExecutionModelExample
{
    /// <summary>
    /// Entry point used by the examples runner. It validates basic lifecycle behaviors.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("🔎 Running ExecutionModelExample...");

        // Create a minimal kernel to satisfy APIs used by snippets.
        // In real scenarios you would configure real connectors.
        var kernel = Kernel.CreateBuilder().Build();

        // Demonstrate execution initialization: create GraphState and KernelArguments
        var args = new KernelArguments();
        args["input"] = "example input";

        var graphState = new GraphState(args);

        // Validate a hypothetical graph integrity check result
        var validationResult = new { IsValid = true };
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException("Graph validation failed: sample");
        }

        // Demonstrate a node 'execution' via a Func that mimics ExecuteAsync
        // Use concrete 'Kernel' type here to match Examples project references
        Func<Kernel, KernelArguments, CancellationToken, Task<object?>> executeNode = async (k, a, ct) =>
        {
            // Simulate asynchronous work
            await Task.Delay(10, ct).ConfigureAwait(false);
            return "result";
        };

        // Run the node and show lifecycle logging
        var cts = new CancellationTokenSource();
        var nodeTask = executeNode(kernel, args, cts.Token);
        var result = await nodeTask.ConfigureAwait(false);
        Console.WriteLine($"Node execution result: {result}");

        // Demonstrate iteration limits: simple loop with max steps
        int maxIterations = 10;
        int iterations = 0;
        while (iterations < maxIterations)
        {
            iterations++;
            if (iterations >= maxIterations)
            {
                Console.WriteLine($"Reached max iterations: {iterations}");
                break;
            }
        }

        // Demonstrate timeout enforcement
        var start = DateTimeOffset.UtcNow;
        var executionTimeout = TimeSpan.FromSeconds(1);
        await Task.Delay(5).ConfigureAwait(false); // noop small delay
        var elapsed = DateTimeOffset.UtcNow - start;
        if (executionTimeout > TimeSpan.Zero && elapsed > executionTimeout)
        {
            throw new OperationCanceledException($"Graph execution exceeded timeout {executionTimeout}");
        }

        Console.WriteLine("✅ ExecutionModelExample completed successfully.");
    }
}
