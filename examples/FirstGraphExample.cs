using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

namespace Examples;

/// <summary>
/// Example demonstrating how to create your first graph workflow with SemanticKernel.Graph.
/// This example shows the complete workflow from kernel creation to graph execution,
/// including conditional nodes and state management.
/// </summary>
public class FirstGraphExample
{
    /// <summary>
    /// Runs the comprehensive first graph example demonstrating all core concepts.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Your First Graph in 5 Minutes ===\n");

        try
        {
            // Step 1: Create and configure your kernel with graph support
            var kernel = CreateKernelWithGraphSupport();

            // Step 2: Create all the nodes for our graph workflow
            var (greetingNode, decisionNode, followUpNode) = CreateGraphNodes(kernel);

            // Step 3: Build and configure the complete graph
            var graph = BuildAndConfigureGraph(greetingNode, decisionNode, followUpNode);

            // Step 4: Execute the graph with sample input
            await ExecuteGraphWithSampleInputAsync(graph, kernel);

            // Step 5: Demonstrate experimentation with different inputs
            await RunExperimentationExamplesAsync();

            Console.WriteLine("\n✅ Your first graph executed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error executing first graph: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Creates and configures a kernel with graph support enabled.
    /// This demonstrates the basic setup required for graph-based workflows.
    /// </summary>
    /// <returns>A configured kernel instance with graph support</returns>
    private static Kernel CreateKernelWithGraphSupport()
    {
        Console.WriteLine("Step 1: Creating kernel with graph support...");

        // Create a new kernel builder instance
        var builder = Kernel.CreateBuilder();

        // Add OpenAI chat completion service (you can replace with your preferred LLM)
        // Note: In a real application, you would use your actual API key
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrEmpty(apiKey))
        {
            builder.AddOpenAIChatCompletion("gpt-4", apiKey);
        }
        else
        {
            // Fallback to a mock function for demonstration purposes
            Console.WriteLine("⚠️  OPENAI_API_KEY not found. Using mock functions for demonstration.");
        }

        // Enable graph functionality with a single line - this registers all necessary services
        builder.AddGraphSupport();

        // Build the kernel with all configured services
        var kernel = builder.Build();

        Console.WriteLine("✅ Kernel created successfully with graph support enabled");
        return kernel;
    }

    /// <summary>
    /// Creates all the nodes needed for the graph workflow.
    /// Demonstrates different node types: function nodes and conditional nodes.
    /// </summary>
    /// <param name="kernel">The configured kernel instance</param>
    /// <returns>A tuple containing all created nodes</returns>
    private static (FunctionGraphNode greetingNode, ConditionalGraphNode decisionNode, FunctionGraphNode followUpNode)
        CreateGraphNodes(Kernel kernel)
    {
        Console.WriteLine("Step 2: Creating graph nodes...");

        // Create a function node that generates personalized greetings
        // This node will process the input name and generate a friendly greeting
        var greetingNode = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod(
                (string name) => $"Hello {name}! It's wonderful to meet you today. I hope you're having a fantastic day filled with joy and positivity.",
                functionName: "GenerateGreeting",
                description: "Creates a personalized greeting message"
            ),
            "greeting_node"
        ).StoreResultAs("greeting");

        // Create a conditional node for decision making
        // This node evaluates whether the greeting is substantial enough to continue
        // Note: The condition function receives GraphState, not KernelArguments
        var decisionNode = new ConditionalGraphNode(
            (state) => state.ContainsValue("greeting") &&
                      state.GetValue<string>("greeting")?.Length > 20,
            "decision_node"
        );

        // Create a follow-up node that generates conversation continuations
        // This node only executes when the decision node allows it
        var followUpNode = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod(
                (string greeting) => $"Based on this greeting: '{greeting}', here's a follow-up question: What's something that's bringing you joy today, or is there a particular topic you'd like to explore together?",
                functionName: "GenerateFollowUp",
                description: "Creates engaging follow-up questions"
            ),
            "followup_node"
        ).StoreResultAs("output");

        Console.WriteLine("✅ All graph nodes created successfully");
        return (greetingNode, decisionNode, followUpNode);
    }

    /// <summary>
    /// Builds and configures the complete graph structure.
    /// This method demonstrates how to assemble nodes and define execution flow.
    /// </summary>
    /// <param name="greetingNode">The greeting generation node</param>
    /// <param name="decisionNode">The conditional decision node</param>
    /// <param name="followUpNode">The follow-up generation node</param>
    /// <returns>A fully configured graph executor</returns>
    private static GraphExecutor BuildAndConfigureGraph(
        FunctionGraphNode greetingNode,
        ConditionalGraphNode decisionNode,
        FunctionGraphNode followUpNode)
    {
        Console.WriteLine("Step 3: Building and configuring the graph...");

        // Create a new graph executor with a descriptive name and description
        var graph = new GraphExecutor(
            "MyFirstGraph",
            "A simple greeting workflow that demonstrates basic graph concepts"
        );

        // Add all nodes to the graph
        // Nodes must be added before they can be connected
        graph.AddNode(greetingNode);
        graph.AddNode(decisionNode);
        graph.AddNode(followUpNode);

        // Connect the nodes to define the execution flow
        // Start with the greeting node flowing to the decision node
        graph.Connect(greetingNode.NodeId, decisionNode.NodeId);

        // Connect decision node to follow-up node when condition is met
        // This creates a conditional edge that only executes when the greeting is substantial
        // Note: The condition function receives KernelArguments, not GraphState
        graph.ConnectWhen(decisionNode.NodeId, followUpNode.NodeId,
            args => args.ContainsKey("greeting") &&
                    args["greeting"]?.ToString()?.Length > 20);

        // Connect decision node to end when condition is not met
        // This creates an exit path for short greetings
        // Note: We don't need to explicitly connect to null - the graph will naturally end
        // when no more edges are available

        // Set the starting point of the graph
        // Execution always begins at this node
        graph.SetStartNode(greetingNode.NodeId);

        Console.WriteLine("✅ Graph built and configured successfully");
        return graph;
    }

    /// <summary>
    /// Executes the graph with sample input data.
    /// Demonstrates how to provide input and retrieve results from graph execution.
    /// </summary>
    /// <param name="graph">The configured graph executor</param>
    /// <param name="kernel">The kernel instance for execution</param>
    private static async Task ExecuteGraphWithSampleInputAsync(GraphExecutor graph, Kernel kernel)
    {
        Console.WriteLine("Step 4: Executing the graph...");

        // Create initial state with input data
        // This demonstrates how to pass data into your graph workflow
        var initialState = new KernelArguments { ["name"] = "Alice" };

        Console.WriteLine($"Input state: {{ \"name\": \"{initialState["name"]}\" }}");

        // Execute the graph with the initial state
        // The graph executor will traverse all nodes according to the defined flow
        var result = await graph.ExecuteAsync(kernel, initialState);

        // Display the execution results
        Console.WriteLine("\n=== Execution Results ===");

        // Extract and display the greeting result from the final state
        // The result is a FunctionResult, but the actual data is stored in the arguments
        var greeting = initialState.GetValueOrDefault("greeting", "No greeting generated");
        Console.WriteLine($"Greeting: {greeting}");

        // Check if follow-up was generated (depends on conditional execution)
        if (initialState.ContainsKey("output"))
        {
            Console.WriteLine($"Follow-up: {initialState["output"]}");
        }
        else
        {
            Console.WriteLine("Follow-up: Not generated (greeting was too short)");
        }

        // Display the complete final state for analysis
        Console.WriteLine("\n=== Complete Final State ===");
        foreach (var kvp in initialState)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }
    }

    /// <summary>
    /// Demonstrates how to experiment with different inputs and conditions.
    /// This method shows the flexibility of graph-based workflows.
    /// </summary>
    public static async Task RunExperimentationExamplesAsync()
    {
        Console.WriteLine("\n=== Experimentation Examples ===\n");

        try
        {
            var kernel = CreateKernelWithGraphSupport();
            var (greetingNode, decisionNode, followUpNode) = CreateGraphNodes(kernel);
            var graph = BuildAndConfigureGraph(greetingNode, decisionNode, followUpNode);

            // Example 1: Test with a different name
            Console.WriteLine("--- Testing with name 'Bob' ---");
            var state1 = new KernelArguments { ["name"] = "Bob" };
            var result1 = await graph.ExecuteAsync(kernel, state1);
            Console.WriteLine($"Greeting: {state1.GetValueOrDefault("greeting", "No greeting")}");

            // Example 2: Test with a very short name (should trigger early exit)
            Console.WriteLine("\n--- Testing with very short name 'A' ---");
            var state2 = new KernelArguments { ["name"] = "A" };
            var result2 = await graph.ExecuteAsync(kernel, state2);
            Console.WriteLine($"Greeting: {state2.GetValueOrDefault("greeting", "No greeting")}");
            Console.WriteLine($"Follow-up generated: {state2.ContainsKey("output")}");

            Console.WriteLine("\n✅ Experimentation examples completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in experimentation examples: {ex.Message}");
        }
    }
}
