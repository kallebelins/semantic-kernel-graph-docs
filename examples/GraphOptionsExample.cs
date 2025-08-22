using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Execution;
using SemanticKernel.Graph.Extensions;
using System;
using System.Threading;

/// <summary>
/// Example demonstrating how to configure and use GraphOptions in a GraphExecutor.
/// This file is created to match the documentation examples and is runnable from the examples runner.
/// </summary>
public static class GraphOptionsExample
{
    /// <summary>
    /// Entry point called by the examples runner.
    /// </summary>
    public static async System.Threading.Tasks.Task RunAsync()
    {
        // Create GraphOptions and register them in the kernel's DI using AddGraphSupport.
        // This matches how the real library wires options into executions.
        var kernel = Kernel.CreateBuilder()
            .AddGraphSupport(opts =>
            {
                // Enable or disable logging for executions
                opts.EnableLogging = true;
                // Enable metrics collection
                opts.EnableMetrics = true;
                // Limit execution steps to avoid runaway graphs
                opts.MaxExecutionSteps = 500;
                // Validate graph structure before execution
                opts.ValidateGraphIntegrity = true;
                // Overall timeout for execution
                opts.ExecutionTimeout = TimeSpan.FromMinutes(5);
                // Enable plan compilation cache
                opts.EnablePlanCompilation = true;
            })
            .Build();

        // Create an executor associated with the kernel so it can pick up GraphOptions from DI
        var executor = new GraphExecutor(kernel);

        // Prepare a trivial kernel function and wrap it into a FunctionGraphNode so execution can run.
        var okFunction = KernelFunctionFactory.CreateFromMethod((KernelArguments args) =>
        {
            // Return a fixed value to demonstrate execution
            return "ok";
        }, "OkFunction");

        var node = new SemanticKernel.Graph.Nodes.FunctionGraphNode(okFunction, "start", "Start node");

        executor.AddNode(node);
        executor.SetStartNode("start");

        // Execute the graph with a cancellation token and print the result
        var result = await executor.ExecuteAsync(kernel, new KernelArguments(), CancellationToken.None);

        Console.WriteLine("GraphOptions example executed. Result: " + result?.GetValue<string>());
    }
}


