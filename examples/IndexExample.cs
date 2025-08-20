using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

namespace SemanticKernel.Graph.Examples;

/// <summary>
/// Example demonstrating the correct way to create and execute graphs
/// using the SemanticKernel.Graph API.
/// </summary>
public static class IndexExample
{
    /// <summary>
    /// Runs the index example showing correct graph creation and execution.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Index Example: Correct Graph Creation ===\n");

        try
        {
            // Create kernel with graph support
            var builder = Kernel.CreateBuilder();
            builder.AddGraphSupport();
            var kernel = builder.Build();

            // Create a simple graph executor
            var graphExecutor = CreateSimpleGraphAsync(kernel);

            // Execute the graph
            var arguments = new KernelArguments
            {
                ["input"] = "Hello from SemanticKernel.Graph!",
                ["user_name"] = "Developer"
            };

            Console.WriteLine("🚀 Executing graph...");
            var result = await graphExecutor.ExecuteAsync(kernel, arguments);
            
            var output = result.GetValue<string>() ?? "No output received";
            Console.WriteLine($"✅ Graph execution completed!");
            Console.WriteLine($"📤 Output: {output}\n");

            Console.WriteLine("=== Index example completed successfully! ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in index example: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Creates a simple graph with three nodes: start, process, and end.
    /// </summary>
    private static GraphExecutor CreateSimpleGraphAsync(Kernel kernel)
    {
        // Create the graph executor (this is the correct way, not a Graph class)
        var executor = new GraphExecutor("SimpleGraph", "A simple example graph");

        // Create nodes
        var startNode = new FunctionGraphNode(
            CreateStartFunction(kernel),
            "start",
            "Start Node"
        );

        var processNode = new FunctionGraphNode(
            CreateProcessFunction(kernel),
            "process",
            "Process Node"
        );

        var endNode = new FunctionGraphNode(
            CreateEndFunction(kernel),
            "end",
            "End Node"
        );

        // Add nodes to the executor
        executor.AddNode(startNode);
        executor.AddNode(processNode);
        executor.AddNode(endNode);

        // Set the start node
        executor.SetStartNode(startNode.NodeId);

        // Add edges to define execution flow
        executor.AddEdge(ConditionalEdge.CreateUnconditional(startNode, processNode));
        executor.AddEdge(ConditionalEdge.CreateUnconditional(processNode, endNode));

        return executor;
    }

    /// <summary>
    /// Creates a function that initializes the process.
    /// </summary>
    private static KernelFunction CreateStartFunction(Kernel kernel)
    {
        return KernelFunctionFactory.CreateFromMethod(
            (string input, string user_name) =>
            {
                Console.WriteLine($"🎯 Start Node: Processing input '{input}' for user '{user_name}'");
                return $"Started processing: {input}";
            },
            "start_function",
            "Initializes the processing workflow"
        );
    }

    /// <summary>
    /// Creates a function that processes the input.
    /// </summary>
    private static KernelFunction CreateProcessFunction(Kernel kernel)
    {
        return KernelFunctionFactory.CreateFromMethod(
            (string input) =>
            {
                Console.WriteLine($"⚙️  Process Node: Processing input '{input}'");
                var processedMessage = $"Processed: {input} - Hello!";
                return processedMessage;
            },
            "process_function",
            "Processes the input data"
        );
    }

    /// <summary>
    /// Creates a function that finalizes the process.
    /// </summary>
    private static KernelFunction CreateEndFunction(Kernel kernel)
    {
        return KernelFunctionFactory.CreateFromMethod(
            (string input) =>
            {
                Console.WriteLine($"🏁 End Node: Finalizing process with input '{input}'");
                var finalOutput = $"Final result: {input} - Completed at {DateTime.Now:HH:mm:ss}";
                return finalOutput;
            },
            "end_function",
            "Finalizes the processing workflow"
        );
    }
}
