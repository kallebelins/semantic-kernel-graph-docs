using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

namespace Examples;

/// <summary>
/// Demonstration of subgraph composition for documentation purposes.
/// This file mirrors the examples shown in `docs/examples/subgraph-examples.md` but
/// includes English comments and simple console outputs so it can be executed
/// as a standalone demo (when included in the docs project that references
/// the main library).
/// </summary>
public static class SubgraphExamples
{
	public static async Task RunAsync()
	{
		Console.WriteLine("🎯 Running ReAct Agent Example (documentation runnable)\n");

		// Create a minimal kernel with graph support. No LLM provider is configured
		// so the example only uses method-based functions (deterministic and offline).
		var kernel = Kernel.CreateBuilder()
			.AddGraphSupport()
			.Build();

		await RunIsolatedCloneAsync(kernel);
		await RunScopedPrefixAsync(kernel);
    }

    /// <summary>
    /// Runs an isolated clone subgraph example: parent provides `a` and `b`,
    /// they are mapped to `x` and `y` inside the child subgraph, the child
    /// computes `sum` and maps it back to `total` in the parent.
    /// </summary>
    public static async Task RunIsolatedCloneAsync(Kernel kernel = null)
	{
		// Create kernel with graph support
		kernel ??= Kernel.CreateBuilder().AddGraphSupport().Build();

		// Create child graph that computes sum
		var child = new GraphExecutor("Subgraph_Sum", "Child that computes sum of two numbers");

		var sumFunction = KernelFunctionFactory.CreateFromMethod(
			(KernelArguments args) =>
			{
				// Read x and y, convert to double safely
				var x = args.TryGetValue("x", out var xv) && xv is IConvertible ? Convert.ToDouble(xv) : 0.0;
				var y = args.TryGetValue("y", out var yv) && yv is IConvertible ? Convert.ToDouble(yv) : 0.0;
				var sum = x + y;
				// Store computed value in child state
				args["sum"] = sum;
				return sum.ToString("F2");
			},
			functionName: "compute_sum",
			description: "Compute sum of x and y and store in 'sum'"
		);

		var sumNode = new FunctionGraphNode(sumFunction, nodeId: "sum_node", description: "Compute sum node");
		child.AddNode(sumNode).SetStartNode(sumNode.NodeId);

		// Configure subgraph node in parent with explicit mappings
		var config = new SubgraphConfiguration
		{
			IsolationMode = SubgraphIsolationMode.IsolatedClone,
			MergeConflictPolicy = SemanticKernel.Graph.State.StateMergeConflictPolicy.PreferSecond,
			InputMappings =
			{
				["a"] = "x",
				["b"] = "y"
			},
			OutputMappings =
			{
				["sum"] = "total"
			}
		};

		var parent = new GraphExecutor("Parent_IsolatedClone", "Parent that invokes sum subgraph");
		var subgraphNode = new SubgraphGraphNode(child, name: "Subgraph(Sum)", description: "Executes sum subgraph", config: config);

		var finalizeFunction = KernelFunctionFactory.CreateFromMethod(
			(KernelArguments args) =>
			{
				// Read total mapped from subgraph
				var total = args.TryGetValue("total", out var tv) ? tv : 0;
				return $"Total (from subgraph): {total}";
			},
			functionName: "finalize",
			description: "Return total"
		);

		var finalizeNode = new FunctionGraphNode(finalizeFunction, nodeId: "finalize_node", description: "Finalize node");

		parent.AddNode(subgraphNode)
			.AddNode(finalizeNode)
			.SetStartNode(subgraphNode.NodeId)
			.Connect(subgraphNode.NodeId, finalizeNode.NodeId);

		// Prepare kernel arguments and execute
		var args = new KernelArguments
		{
			["a"] = 3,
			["b"] = 7
		};

		var result = await parent.ExecuteAsync(kernel, args, CancellationToken.None);

		Console.WriteLine("[IsolatedClone] expected total = 10");
		var totalOk = args.TryGetValue("total", out var totalVal);
		Console.WriteLine($"[IsolatedClone] obtained total = {(totalOk ? totalVal : "(not mapped)")}");
		Console.WriteLine($"[IsolatedClone] final message = {result.GetValue<object>()}");
	}

	/// <summary>
	/// Runs a scoped prefix subgraph example. Parent state uses a prefix (e.g. "invoice.")
	/// and the child operates on unprefixed keys; after execution the results are written
	/// back under the configured prefix.
	/// </summary>
	public static async Task RunScopedPrefixAsync(Kernel kernel = null)
	{
		// Create kernel with graph support
		kernel ??= Kernel.CreateBuilder().AddGraphSupport().Build();

		var child = new GraphExecutor("Subgraph_Discount", "Child that applies a discount to a total");

		var applyDiscount = KernelFunctionFactory.CreateFromMethod(
			(KernelArguments args) =>
			{
				var total = args.TryGetValue("total", out var tv) && tv is IConvertible ? Convert.ToDouble(tv) : 0.0;
				var discount = args.TryGetValue("discount", out var dv) && dv is IConvertible ? Convert.ToDouble(dv) : 0.0;
				var final = Math.Max(0.0, total - discount);
				args["final"] = final;
				return final.ToString("F2");
			},
			functionName: "apply_discount",
			description: "Apply discount and store in 'final'"
		);

		var discountNode = new FunctionGraphNode(applyDiscount, nodeId: "discount_node", description: "Apply discount node");
		discountNode.SetMetadata("StoreResultAs", "final");
		child.AddNode(discountNode).SetStartNode(discountNode.NodeId);

		var config = new SubgraphConfiguration
		{
			IsolationMode = SubgraphIsolationMode.ScopedPrefix,
			ScopedPrefix = "invoice."
		};

		var parent = new GraphExecutor("Parent_ScopedPrefix", "Parent that invokes discount subgraph");
		var subgraphNode = new SubgraphGraphNode(child, name: "Subgraph(Discount)", description: "Executes discount subgraph", config: config);

		var echoFunction = KernelFunctionFactory.CreateFromMethod(
			(KernelArguments args) =>
			{
				var total = args.TryGetValue("invoice.total", out var t) ? t : 0;
				var discount = args.TryGetValue("invoice.discount", out var d) ? d : 0;
				var final = args.TryGetValue("invoice.final", out var f) ? f : 0;
				return $"Total: {total} | Discount: {discount} | Final: {final}";
			},
			functionName: "echo_invoice",
			description: "Echo invoice values"
		);

		var echoNode = new FunctionGraphNode(echoFunction, nodeId: "echo_node", description: "Echo node");

		parent.AddNode(subgraphNode)
			.AddNode(echoNode)
			.SetStartNode(subgraphNode.NodeId)
			.Connect(subgraphNode.NodeId, echoNode.NodeId);

		var args = new KernelArguments
		{
			["invoice.total"] = 125.0,
			["invoice.discount"] = 20.0
		};

		var result = await parent.ExecuteAsync(kernel, args, CancellationToken.None);

		Console.WriteLine("[ScopedPrefix] final expected = 105.00");
		var finalOk = args.TryGetValue("invoice.final", out var finalVal);
		Console.WriteLine($"[ScopedPrefix] invoice.final = {(finalOk ? finalVal : "(not mapped)")}");
		Console.WriteLine($"[ScopedPrefix] final message = {result.GetValue<object>()}");
	}
}
