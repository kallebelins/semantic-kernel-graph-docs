using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

namespace Examples;

/// <summary>
/// Demonstrates configuring resource governance and executing a small graph
/// that respects permit-based rate limiting. This example mirrors the
/// documentation snippets and is validated to compile and run in the
/// examples project.
/// </summary>
public static class ResourceGovernanceExample
{
    /// <summary>
    /// Runs a demo showing basic resource governance configuration and
    /// a short execution that consumes permits.
    /// </summary>
    public static async Task RunAsync()
    {
        // Create an in-memory kernel (no external AI required for this example)
        var kernel = Kernel.CreateBuilder().Build();

        // Prepare arguments container
        var args = new KernelArguments();

        // Create simple function nodes that simulate work
        var start = new FunctionGraphNode(KernelFunctionFactory.CreateFromMethod(() => "start", "Start"), nodeId: "start");
        var workA = new FunctionGraphNode(KernelFunctionFactory.CreateFromMethod(() => "work-a", "WorkA"), nodeId: "workA");
        var workB = new FunctionGraphNode(KernelFunctionFactory.CreateFromMethod(() => "work-b", "WorkB"), nodeId: "workB");
        var join = new FunctionGraphNode(KernelFunctionFactory.CreateFromMethod(() => "join", "Join"), nodeId: "join");

        // Wire graph: start -> workA, workB -> join
        start.ConnectTo(workA);
        start.ConnectTo(workB);
        workA.ConnectTo(join);
        workB.ConnectTo(join);

        // Set metadata hooks to record that branches executed
        workA.SetMetadata("AfterExecute", (Action<Kernel, KernelArguments, FunctionResult>)((k, ka, r) => ka["a"] = "done"));
        workB.SetMetadata("AfterExecute", (Action<Kernel, KernelArguments, FunctionResult>)((k, ka, r) => ka["b"] = "done"));

        // Build executor and configure resource governance
        var executor = new GraphExecutor("ResourceGovernanceDemo")
            .AddNode(start)
            .AddNode(workA)
            .AddNode(workB)
            .AddNode(join)
            .SetStartNode("start")
            .ConfigureConcurrency(new GraphConcurrencyOptions
            {
                EnableParallelExecution = true,
                MaxDegreeOfParallelism = 2
            })
            .ConfigureResources(new GraphResourceOptions
            {
                EnableResourceGovernance = true,
                BasePermitsPerSecond = 2.0, // low rate for demo purposes
                MaxBurstSize = 2,
                CpuHighWatermarkPercent = 90.0,
                CpuSoftLimitPercent = 70.0,
                MinAvailableMemoryMB = 128.0,
                DefaultPriority = ExecutionPriority.Normal
            });

        // Execute the graph; resource governor will enforce permits
        var result = await executor.ExecuteAsync(kernel, args);

        // Print results to verify both branches ran
        Console.WriteLine($"Execution finished. Join node value: {result.GetValueAsString()}");
        Console.WriteLine($"a: {args.GetValueOrDefault("a")} b: {args.GetValueOrDefault("b")} ");
    }
}


