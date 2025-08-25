using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.Extensions;

namespace Examples;

/// <summary>
/// Simple fork/join example for documentation. This mirrors the snippet in
/// `docs/how-to/parallelism-and-fork-join.md` and is meant to be copy-paste
/// runnable inside the examples project after minor namespace adjustments.
/// </summary>
public static class SimpleForkJoinExample
{
    /// <summary>
    /// Runs a simple fork/join graph: start -> {A,B} -> join.
    /// Each parallel branch writes a different key into the kernel arguments
    /// and the join node continues after both complete.
    /// </summary>
    public static async Task RunAsync()
    {
        // Use an in-memory kernel for examples (no external services required)
        var kernel = Kernel.CreateBuilder().Build();
        var args = new KernelArguments();

        // Create nodes
        var start = new FunctionGraphNode(KernelFunctionFactory.CreateFromMethod(() => "start", "Start"), nodeId: "start");
        var a = new FunctionGraphNode(KernelFunctionFactory.CreateFromMethod(() => "A", "A"), nodeId: "A");
        var b = new FunctionGraphNode(KernelFunctionFactory.CreateFromMethod(() => "B", "B"), nodeId: "B");
        var join = new FunctionGraphNode(KernelFunctionFactory.CreateFromMethod(() => "join", "Join"), nodeId: "join");

        // Each branch records a result into the kernel arguments after executing.
        a.SetMetadata("AfterExecute", (Action<Kernel, KernelArguments, FunctionResult>)((k, ka, r) => ka["result_a"] = "Result from A"));
        b.SetMetadata("AfterExecute", (Action<Kernel, KernelArguments, FunctionResult>)((k, ka, r) => ka["result_b"] = "Result from B"));

        // Wire the graph: start -> A,B and A,B -> join
        start.ConnectTo(a);
        start.ConnectTo(b);
        a.ConnectTo(join);
        b.ConnectTo(join);

        // Build executor with parallel execution enabled
        var executor = new GraphExecutor("SimpleForkJoin");
        executor.AddNode(start).AddNode(a).AddNode(b).AddNode(join);
        executor.SetStartNode("start");
        executor.ConfigureConcurrency(new GraphConcurrencyOptions
        {
            EnableParallelExecution = true,
            MaxDegreeOfParallelism = 2
        });

        // Execute the graph
        var result = await executor.ExecuteAsync(kernel, args);

        // Output the results
        Console.WriteLine($"Result node: {result.GetValueAsString()}");
        Console.WriteLine($"result_a: {args.GetValueOrDefault("result_a")} ");
        Console.WriteLine($"result_b: {args.GetValueOrDefault("result_b")} ");
    }
}


