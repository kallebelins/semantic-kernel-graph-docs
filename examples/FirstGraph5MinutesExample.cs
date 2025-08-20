using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

namespace Examples;

/// <summary>
/// Example demonstrating how to create your first graph in 5 minutes with SemanticKernel.Graph.
/// </summary>
public class FirstGraph5MinutesExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== My First Graph ===\n");

        try
        {
            // 1. Create kernel and enable graph support
            var builder = Kernel.CreateBuilder();
            builder.AddGraphSupport();
            var kernel = builder.Build();

            // 2. Create simple function nodes
            var greetingNode = CreateGreetingNode(kernel);
            var followUpNode = CreateFollowUpNode(kernel);

            // 3. Build and configure the graph
            var graph = new GraphExecutor("MyFirstGraph", "A simple greeting workflow");
            
            graph.AddNode(greetingNode);
            graph.AddNode(followUpNode);
            
            // Connect nodes in sequence
            graph.Connect(greetingNode.NodeId, followUpNode.NodeId);
            
            // Set the starting point
            graph.SetStartNode(greetingNode.NodeId);

            // 4. Execute the graph
            var initialState = new KernelArguments { ["name"] = "Developer" };
            
            Console.WriteLine("Executing graph...");
            var result = await graph.ExecuteAsync(kernel, initialState);
            
            Console.WriteLine("\n=== Results ===");
            
            // Get results from the graph state
            var graphState = initialState.GetOrCreateGraphState();
            var greeting = graphState.GetValue<string>("greeting") ?? "No greeting";
            var followup = graphState.GetValue<string>("followup") ?? "No follow-up";
            
            Console.WriteLine($"Greeting: {greeting}");
            Console.WriteLine($"Follow-up: {followup}");
            
            Console.WriteLine("\n✅ Graph executed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error executing graph: {ex.Message}");
        }
    }

    private static FunctionGraphNode CreateGreetingNode(Kernel kernel)
    {
        var greetingFunction = kernel.CreateFunctionFromMethod(
            (string name) => $"Hello {name}! Welcome to SemanticKernel.Graph.",
            functionName: "GenerateGreeting",
            description: "Creates a personalized greeting"
        );

        var node = new FunctionGraphNode(greetingFunction, "greeting_node")
            .StoreResultAs("greeting");

        return node;
    }

    private static FunctionGraphNode CreateFollowUpNode(Kernel kernel)
    {
        var followUpFunction = kernel.CreateFunctionFromMethod(
            (string greeting) => $"Based on: '{greeting}', here's a follow-up question: How can I help you today?",
            functionName: "GenerateFollowUp",
            description: "Creates a follow-up question"
        );

        var node = new FunctionGraphNode(followUpFunction, "followup_node")
            .StoreResultAs("followup");

        return node;
    }
}
