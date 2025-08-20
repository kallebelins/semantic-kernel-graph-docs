using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

namespace Examples;

/// <summary>
/// Example demonstrating the Getting Started guide from the documentation.
/// This example shows the basic setup and first graph creation as described
/// in the getting-started.md documentation.
/// </summary>
public class GettingStartedExample
{
    /// <summary>
    /// Runs the getting started example that demonstrates basic kernel setup
    /// and graph creation as shown in the documentation.
    /// </summary>
    /// <returns>Task representing the asynchronous operation</returns>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Getting Started with SemanticKernel.Graph ===\n");

        try
        {
            // Step 1: Create and configure your kernel
            var kernel = CreateKernelWithGraphSupport();

            // Step 2: Create a simple function node
            var echoNode = CreateEchoNode(kernel);

            // Step 3: Create and execute a graph
            var graph = CreateAndExecuteGraph(echoNode);

            // Step 4: Execute with sample input
            await ExecuteGraphWithSampleInputAsync(graph, kernel);

            Console.WriteLine("\n✅ Getting started example completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in getting started example: {ex.Message}");
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
            Console.WriteLine("✅ OpenAI service configured with API key");
        }
        else
        {
            // Fallback to a mock function for demonstration purposes
            Console.WriteLine("⚠️  OPENAI_API_KEY not found. Using mock functions for demonstration.");
        }

        // Enable graph functionality with one line - this registers all necessary services
        builder.AddGraphSupport();

        // Build the kernel with all configured services
        var kernel = builder.Build();

        Console.WriteLine("✅ Kernel created successfully with graph support enabled");
        return kernel;
    }

    /// <summary>
    /// Creates a simple echo function node that demonstrates basic node creation.
    /// This node will process input and generate an echo response.
    /// </summary>
    /// <param name="kernel">The configured kernel instance</param>
    /// <returns>A configured function graph node</returns>
    private static FunctionGraphNode CreateEchoNode(Kernel kernel)
    {
        Console.WriteLine("Step 2: Creating echo function node...");

        // Create a function node that echoes the input with a prefix
        // This demonstrates how to create a simple function node
        var echoNode = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod(
                (string input) => $"Echo: {input}",
                functionName: "EchoFunction",
                description: "Echoes the input with a prefix"
            ),
            "echo_node"
        ).StoreResultAs("output");

        Console.WriteLine("✅ Echo function node created successfully");
        return echoNode;
    }

    /// <summary>
    /// Creates and configures the graph structure with the echo node.
    /// This demonstrates basic graph assembly and configuration.
    /// </summary>
    /// <param name="echoNode">The echo function node to add to the graph</param>
    /// <returns>A configured graph executor</returns>
    private static GraphExecutor CreateAndExecuteGraph(FunctionGraphNode echoNode)
    {
        Console.WriteLine("Step 3: Creating and configuring the graph...");

        // Create a new graph executor with a descriptive name
        var graph = new GraphExecutor("MyFirstGraph");

        // Add the echo node to the graph
        graph.AddNode(echoNode);

        // Set the echo node as the starting point of the graph
        graph.SetStartNode(echoNode.NodeId);

        Console.WriteLine("✅ Graph created and configured successfully");
        return graph;
    }

    /// <summary>
    /// Executes the graph with sample input data to demonstrate the workflow.
    /// This shows how to provide input and retrieve results from graph execution.
    /// </summary>
    /// <param name="graph">The configured graph executor</param>
    /// <param name="kernel">The kernel instance for execution</param>
    /// <returns>Task representing the asynchronous operation</returns>
    private static async Task ExecuteGraphWithSampleInputAsync(GraphExecutor graph, Kernel kernel)
    {
        Console.WriteLine("Step 4: Executing the graph with sample input...");

        // Create initial state with sample input data
        // This demonstrates how to pass data into your graph workflow
        var state = new KernelArguments { ["input"] = "Hello, World!" };

        Console.WriteLine($"Input state: {{ \"input\": \"{state["input"]}\" }}");

        // Execute the graph with the initial state
        // The graph executor will traverse all nodes according to the defined flow
        var result = await graph.ExecuteAsync(kernel, state);

        // Display the execution results
        Console.WriteLine("\n=== Execution Results ===");
        
        // Extract and display the output result from the final state
        if (state.ContainsKey("output"))
        {
            Console.WriteLine($"Output: {state["output"]}");
        }
        else
        {
            Console.WriteLine("Output: No output generated");
        }

        // Display the complete final state for analysis
        Console.WriteLine("\n=== Complete Final State ===");
        foreach (var kvp in state)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n✅ Graph execution completed successfully!");
    }
}
