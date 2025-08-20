using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Extensions;

namespace SemanticKernel.Graph.Examples;

/// <summary>
/// Main program entry point for running conditional nodes quickstart examples.
/// </summary>
class Program
{
    /// <summary>
    /// Main entry point that runs the conditional nodes example.
    /// </summary>
    /// <param name="args">Command line arguments (not used)</param>
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("🚀 Starting SemanticKernel.Graph Conditional Nodes Quickstart Example...\n");
            
            // Create kernel with basic configuration
            var builder = Kernel.CreateBuilder();
            // Note: In a real scenario, you would add your LLM provider here
            // builder.AddOpenAIChatCompletion("gpt-3.5-turbo", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
            builder.AddGraphSupport();
            var kernel = builder.Build();

            // Run the conditional nodes example
            await ConditionalNodesQuickstartExample.ConditionalNodesQuickstartExample.RunConditionalWorkflowExample(kernel);
            
            Console.WriteLine("🎉 Conditional nodes example completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error running example: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }
}
