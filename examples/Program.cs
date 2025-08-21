using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Extensions;
using Examples;
using SemanticKernel.Graph.Examples;
using SemanticKernel.Graph.Docs.Examples;

namespace Examples;

/// <summary>
/// Main program entry point for running SemanticKernel.Graph examples.
/// Supports multiple example options based on task names.
/// </summary>
class Program
{
    /// <summary>
    /// Main entry point that runs examples based on command line arguments.
    /// </summary>
    /// <param name="args">Command line arguments - specify example name to run</param>
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("🚀 Starting SemanticKernel.Graph Examples...\n");

            // Define available example options based on task names
            var options = new Dictionary<string, Func<Task>>(StringComparer.OrdinalIgnoreCase)
            {
                ["index"] = async () => await IndexExample.RunAsync(),
                ["first-graph"] = async () => await FirstGraphExample.RunAsync(),
                ["first-graph-5-minutes"] = async () => await FirstGraph5MinutesExample.RunAsync(),
                ["getting-started"] = async () => await GettingStartedExample.RunAsync(),
                ["conditional-nodes-tutorial"] = async () => await ConditionalNodesTutorialExample.RunAllExamples(),
                ["checkpointing-quickstart"] = async () => await CheckpointingQuickstartExample.RunAllExamplesAsync(),
                ["conditional-nodes-quickstart"] = async () => await ConditionalNodesQuickstartExample.RunConditionalWorkflowExample(),
                ["state-quickstart"] = async () => await StateQuickstartExample.RunAsync(),
                ["state-tutorial"] = async () => await StateTutorialExample.RunAsync(),
                ["faq"] = async () => await FaqExample.RunAllExamplesAsync(),
                ["installation"] = async () => await InstallationExample.RunAllExamplesAsync(),
                ["metrics-logging-quickstart"] = async () => await MetricsLoggingQuickstartExample.RunBasicExampleAsync(),
                ["react-cot-quickstart"] = async () => await RunReactCotQuickstartExample(),
                ["all"] = async () => await RunAllAvailableExamples()
            };

            // Determine which example to run
            string exampleToRun = "all"; // Default

            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                exampleToRun = args[0];
            }

            // Check if the requested example exists
            if (options.TryGetValue(exampleToRun, out var exampleFunc))
            {
                Console.WriteLine($"📋 Running example: {exampleToRun}\n");
                await exampleFunc();
                Console.WriteLine($"\n🎉 Example '{exampleToRun}' completed successfully!");
            }
            else
            {
                Console.WriteLine($"❓ Unknown example: {exampleToRun}");
                Console.WriteLine("\n📚 Available examples:");
                foreach (var option in options.Keys)
                {
                    Console.WriteLine($"  - {option}");
                }
                Console.WriteLine("\n💡 Usage: dotnet run [example-name]");
                Console.WriteLine("💡 Example: dotnet run conditional-workflow");
                Environment.Exit(1);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error running example: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Runs the React and Chain of Thought Quickstart example
    /// </summary>
    /// <returns>Task representing the asynchronous operation</returns>
    private static async Task RunReactCotQuickstartExample()
    {
        Console.WriteLine("🎯 Running React and Chain of Thought Quickstart Example...\n");

        try
        {
            // Create a basic kernel for the example
            var kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion("gpt-3.5-turbo", "your-api-key-here")
                .Build();

            // Create and run the example
            var example = new ReactCotQuickstartExample(kernel);
            await example.RunAllExamplesAsync();

            Console.WriteLine("\n✅ React and Chain of Thought Quickstart Example completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in React and Chain of Thought Quickstart Example: {ex.Message}");
            Console.WriteLine("💡 Note: This example requires a valid OpenAI API key to run properly.");
        }
    }

    /// <summary>
    /// Runs all available examples
    /// </summary>
    /// <returns>Task representing the asynchronous operation</returns>
    private static async Task RunAllAvailableExamples()
    {
        Console.WriteLine("🎯 Running all available examples...\n");

        // Run all examples
        await IndexExample.RunAsync();
        await FirstGraphExample.RunAsync();
        await FirstGraph5MinutesExample.RunAsync();
        await ConditionalNodesQuickstartExample.RunConditionalWorkflowExample();
        await ConditionalNodesTutorialExample.RunAllExamples();
        await CheckpointingQuickstartExample.RunAllExamplesAsync();
        await StateQuickstartExample.RunAsync();
        await InstallationExample.RunAllExamplesAsync();
        await MetricsLoggingQuickstartExample.RunBasicExampleAsync();
        await RunReactCotQuickstartExample();

        Console.WriteLine("\n" + "=".PadLeft(50, '='));
        Console.WriteLine("📋 All examples completed!");
    }
}
