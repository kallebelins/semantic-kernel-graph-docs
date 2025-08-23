using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

namespace Examples;

/// <summary>
/// Demonstrates the main node types: FunctionGraphNode, ConditionalGraphNode,
/// ReasoningGraphNode, ReActLoopGraphNode and ObservationGraphNode.
/// This example is compiled and executed by the examples runner to ensure
/// documented code is valid and runnable.
/// </summary>
public static class MainNodeTypesExample
{
    /// <summary>
    /// Runs the example demonstrating creation and simple execution of main node types.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("▶ Running Main Node Types example...");

        // Create a minimal kernel with graph support enabled. No external LLM is required
        // for this example because we use simple in-memory functions.
        var kernel = Kernel.CreateBuilder()
            .AddGraphSupport()
            .Build();

        // Create a simple kernel function that processes an input string.
        var processFunction = kernel.CreateFunctionFromMethod(
            (string input) => $"Processed: {input}",
            functionName: "process_input"
        );

        // Wrap the kernel function in a FunctionGraphNode and store the result in state.
        var functionNode = new FunctionGraphNode(processFunction, "process_node")
            .StoreResultAs("processed_result");

        // Create a conditional node that checks if the processed result exists and is non-empty.
        var conditionalNode = new ConditionalGraphNode(
            condition: state => state.ContainsValue("processed_result") &&
                                !string.IsNullOrEmpty(state.GetValue<string>("processed_result")),
            name: "quality_check"
        );

        // Create a simple reasoning node (uses templates/prompts in real scenarios)
        var reasoningNode = new ReasoningGraphNode(
            reasoningPrompt: "Analyze the processed result and decide next steps.",
            name: "reasoning_node"
        );

        // Create an observation node that would inspect action results.
        var observationNode = new ObservationGraphNode(
            observationPrompt: "Analyze the action result and say if the goal was achieved.",
            name: "observation_node"
        );

        // Create an ActionGraphNode to represent executable actions discovered from the kernel.
        var actionNode = ActionGraphNode.CreateWithActions(
            kernel,
            new ActionSelectionCriteria { MinRequiredParameters = 0, MaxRequiredParameters = 5 },
            "action_node"
        );

        // Create a ReAct loop node that composes reasoning, action and observation
        // into an iterative loop. Use the ActionGraphNode as the action component.
        var reactLoopNode = new ReActLoopGraphNode(nodeId: "react_loop", name: "react_loop");
        reactLoopNode.ConfigureNodes(reasoningNode, actionNode, observationNode);

        // Build a graph executor, register nodes and set the start node.
        var graph = new GraphExecutor("MainNodeTypesGraph", "Demo of main node types");
        graph.AddNode(functionNode);
        graph.AddNode(conditionalNode);
        graph.AddNode(reasoningNode);
        graph.AddNode(observationNode);
        graph.AddNode(reactLoopNode);

        // Start execution at the function node
        graph.SetStartNode(functionNode.NodeId);

        // Prepare initial kernel arguments
        var initialState = new KernelArguments { ["input"] = "example input" };

        // Execute the graph. The example focuses on structural validation rather
        // than complex LLM-driven behavior so this should complete without external APIs.
        await graph.ExecuteAsync(kernel, initialState);

        Console.WriteLine("▶ Execution completed. Final state:");
        foreach (var kvp in initialState)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("✅ Main Node Types example finished successfully.");
    }
}


