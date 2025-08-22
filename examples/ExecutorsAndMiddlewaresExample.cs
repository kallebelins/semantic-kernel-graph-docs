using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Execution;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.Extensions;

namespace Examples;

/// <summary>
/// Example demonstrating how to create and use execution middlewares and executors
/// as described in `executors-and-middlewares.md`.
/// </summary>
public static class ExecutorsAndMiddlewaresExample
{
	/// <summary>
	/// Runs the example showing middleware lifecycle hooks and executor composition.
	/// </summary>
	public static async Task RunAsync()
	{
		Console.WriteLine("=== Executors and Middlewares Example ===\n");

		// Create a simple kernel and enable graph support (no external LLMs required)
		var kernel = Kernel.CreateBuilder()
			.AddGraphSupport()
			.Build();

		// Build a basic graph executor
		var executor = new GraphExecutor("MiddlewareDemoGraph");

		// Create three simple function nodes: start -> work -> end
		var startNode = new FunctionGraphNode(
			KernelFunctionFactory.CreateFromMethod(() => "start", "start_fn", "Start node"),
			"start", "Start Node");

		var workNode = new FunctionGraphNode(
			KernelFunctionFactory.CreateFromMethod((KernelArguments args) =>
			{
				// Mark when work was executed in the shared arguments
				args["workedAt"] = DateTimeOffset.UtcNow.ToString("o");
				return "work-done";
			}, "work_fn", "Work node"),
			"work", "Work Node").StoreResultAs("workResult");

		var endNode = new FunctionGraphNode(
			KernelFunctionFactory.CreateFromMethod(() => "end", "end_fn", "End node"),
			"end", "End Node");

		// Register nodes in the executor and wire up edges
		executor.AddNode(startNode);
		executor.AddNode(workNode);
		executor.AddNode(endNode);
		executor.Connect("start", "work");
		executor.Connect("work", "end");
		executor.SetStartNode("start");

		// Add middleware implementations for validation, performance and logging
		executor.UseMiddleware(new ValidationMiddleware());
		executor.UseMiddleware(new PerformanceMonitoringMiddleware());
		executor.UseMiddleware(new LoggingMiddleware());

		// Execute the graph and inspect final arguments/state
		var args = new KernelArguments { ["input"] = "demo-input" };

		var result = await executor.ExecuteAsync(kernel, args);

		Console.WriteLine("\n=== Execution Result ===");
		Console.WriteLine($"Final result: {result.GetValue<object>()}");
		Console.WriteLine("Final arguments/state:");
		foreach (var kv in args)
		{
			Console.WriteLine($"  {kv.Key}: {kv.Value}");
		}

		Console.WriteLine("\n✅ Executors and Middlewares example completed!");
	}
}

/// <summary>
/// Simple validation middleware that ensures required inputs are present before node execution.
/// </summary>
public sealed class ValidationMiddleware : IGraphExecutionMiddleware
{
	public int Order => 50; // Run early in the pipeline

	public Task OnBeforeNodeAsync(GraphExecutionContext context, IGraphNode node, CancellationToken cancellationToken)
	{
		// If a node expects an "input" parameter, ensure it exists in the arguments.
		if (node.InputParameters.Contains("input") && !context.GraphState.KernelArguments.ContainsKey("input"))
		{
			throw new InvalidOperationException($"Node {node.NodeId} requires 'input' parameter but it was not provided.");
		}
		return Task.CompletedTask;
	}

	public Task OnAfterNodeAsync(GraphExecutionContext context, IGraphNode node, FunctionResult result, CancellationToken cancellationToken)
	{
		// No-op for after node in this simple example
		return Task.CompletedTask;
	}

	public Task OnNodeFailedAsync(GraphExecutionContext context, IGraphNode node, Exception exception, CancellationToken cancellationToken)
	{
		// Log validation-related failures to the console
		if (exception is InvalidOperationException)
		{
			Console.WriteLine($"[VALIDATION] Node {node.NodeId} failed validation: {exception.Message}");
		}
		return Task.CompletedTask;
	}
}

/// <summary>
/// Simple performance monitoring middleware that records node start times in the context metadata.
/// </summary>
public sealed class PerformanceMonitoringMiddleware : IGraphExecutionMiddleware
{
	public int Order => 100;

	public Task OnBeforeNodeAsync(GraphExecutionContext context, IGraphNode node, CancellationToken cancellationToken)
	{
		// Save start timestamp for the node in execution properties
		context.SetProperty($"node:{node.NodeId}:start", DateTimeOffset.UtcNow);
		return Task.CompletedTask;
	}

	public Task OnAfterNodeAsync(GraphExecutionContext context, IGraphNode node, FunctionResult result, CancellationToken cancellationToken)
	{
		// Compute elapsed time and print a simple metric
		var startObj = context.GetProperty<object>($"node:{node.NodeId}:start");
		if (startObj is DateTimeOffset start)
		{
			var elapsed = DateTimeOffset.UtcNow - start;
			Console.WriteLine($"[PERF] Node {node.NodeId} completed in {elapsed.TotalMilliseconds}ms");
		}
		return Task.CompletedTask;
	}

	public Task OnNodeFailedAsync(GraphExecutionContext context, IGraphNode node, Exception exception, CancellationToken cancellationToken)
	{
		Console.WriteLine($"[PERF] Node {node.NodeId} failed: {exception.Message}");
		return Task.CompletedTask;
	}
}

/// <summary>
/// Simple logging middleware that prints node lifecycle events to the console.
/// </summary>
public sealed class LoggingMiddleware : IGraphExecutionMiddleware
{
	public int Order => 200;

	public Task OnBeforeNodeAsync(GraphExecutionContext context, IGraphNode node, CancellationToken cancellationToken)
	{
		Console.WriteLine($"[LOG] Starting node {node.NodeId} ({node.Name})");
		return Task.CompletedTask;
	}

	public Task OnAfterNodeAsync(GraphExecutionContext context, IGraphNode node, FunctionResult result, CancellationToken cancellationToken)
	{
		// FunctionResult does not expose a 'Result' property in this runtime wrapper.
		// Use GetValue to access the underlying value if available.
		var value = result.GetValue<object>();
		Console.WriteLine($"[LOG] Completed node {node.NodeId} with result: {value}");
		return Task.CompletedTask;
	}

	public Task OnNodeFailedAsync(GraphExecutionContext context, IGraphNode node, Exception exception, CancellationToken cancellationToken)
	{
		Console.WriteLine($"[LOG] Node {node.NodeId} failed with exception: {exception.Message}");
		return Task.CompletedTask;
	}
}
