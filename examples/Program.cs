using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.State;


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
                ["conditional-edge"] = async () => await ConditionalEdgeExample.RunAllExamplesAsync(),
                ["checkpointing-quickstart"] = async () => await CheckpointingQuickstartExample.RunAllExamplesAsync(),
                ["conditional-nodes-quickstart"] = async () => await ConditionalNodesQuickstartExample.RunConditionalWorkflowExample(),
                ["state-quickstart"] = async () => await StateQuickstartExample.RunAsync(),
                ["state-tutorial"] = async () => await StateTutorialExample.RunAsync(),
                ["state-and-serialization"] = async () => await StateAndSerializationExample.RunAsync(),
                ["faq"] = async () => await FaqExample.RunAllExamplesAsync(),
                ["installation"] = async () => await InstallationExample.RunAllExamplesAsync(),
                ["metrics-logging-quickstart"] = async () => await MetricsLoggingQuickstartExample.RunBasicExampleAsync(),
                ["metrics"] = async () => await MetricsExample.RunAsync(),
                ["react-cot-quickstart"] = async () => await ReactCotQuickstartExample.RunAllExamplesAsync(),
                ["streaming-quickstart"] = async () => await RunStreamingQuickstartExample(),
                ["troubleshooting"] = async () => await TroubleshootingExample.RunAsync(),
                ["core-api"] = async () => await CoreApiExample.RunAsync(),
                ["main-node-types"] = async () => await MainNodeTypesExample.RunAsync(),
                ["additional-utilities"] = () => { AdditionalUtilitiesExample.RunAllDemonstrations(); return Task.CompletedTask; },
                ["rest-tools"] = async () => await RestToolsExample.RunAsync(),
                ["server-apis"] = async () => await ServerApisExample.RunAsync(),
                ["dynamic-routing"] = async () => await DynamicRoutingExample.RunAsync(),
                ["advanced-routing"] = async () => await AdvancedRoutingExample.RunAsync(),
                ["error-policies"] = async () => await ErrorPoliciesExample.RunAsync(),
                ["execution-context"] = async () => await ExecutionContextExample.RunAsync(),
                ["executors-and-middlewares"] = async () => await ExecutorsAndMiddlewaresExample.RunAsync(),
                ["extensions-and-options"] = async () => await ExtensionsAndOptionsExample.RunAsync(),
                ["graph-executor"] = async () => await GraphExecutorExample.RunAsync(),
                ["inspection-visualization"] = async () => await GraphVisualizationExample.RunAsync(),
                ["graph-options"] = async () => await GraphOptionsExample.RunAsync(),
                ["human-in-the-loop"] = async () => await HumanInTheLoopExample.RunAsync(),
                ["integration"] = async () => await IntegrationExample.RunAsync(),
                ["document-analysis-pipeline"] = async () => await DocumentAnalysisPipelineDocExample.DemoAsync(),
                ["all"] = async () => await RunAllAvailableExamples(),
                ["simple-node"] = async () => await ExampleRunners.RunSimpleNodeExample(),
                ["conditional-node"] = async () => await ExampleRunners.RunConditionalNodeExample(),
                ["validation-compilation"] = async () => await ValidationCompilationExample.RunAsync(),
                ["multi-agent"] = async () => await MultiAgentExample.RunAsync(),
                ["execution-model"] = async () => await ExecutionModelExample.RunAsync(),
                ["graph-concepts"] = async () => await GraphConceptsExample.RunAsync(),
                ["execution"] = async () => await ExecutionConceptsExample.RunAsync(),
                ["checkpointing-concepts"] = async () => await CheckpointingConceptsExample.RunAsync(),
                ["advanced-patterns"] = async () => await AdvancedPatternsExample.RunAsync(),
                ["assert-and-suggest"] = async () => await AssertAndSuggestExample.RunAsync(),
                ["chain-of-thought"] = async () => await ChainOfThoughtExample.RunAsync(),
                ["chatbot"] = async () => await ChatbotExample.RunAsync(),
                ["execution-guide"] = async () => await ExecutionGuideExample.RunAsync(),
                ["logging"] = async () => await LoggingExample.RunAllAsync(),
                ["loop-nodes"] = async () => await LoopNodesExample.RunAsync(),
                ["memory-agent"] = async () => await MemoryAgentExample.RunAsync(),
                ["multi-hop-rag-retry"] = async () => await MultiHopRagRetryExample.RunAsync(),
                ["optimizers-and-few-shot"] = async () => await OptimizersAndFewShotExample.RunAsync(),
                ["plugin-system"] = async () => await PluginSystemExample.RunExampleAsync()
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
    /// Runs the Streaming Quickstart example demonstrating real-time graph execution monitoring
    /// </summary>
    /// <returns>Task representing the asynchronous operation</returns>
    private static async Task RunStreamingQuickstartExample()
    {
        Console.WriteLine("🎯 Running Streaming Quickstart Example...\n");

        try
        {
            // Create a basic kernel for the example
            var kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion("gpt-3.5-turbo", Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "your-api-key-here")
                .AddGraphSupport()
                .Build();

            // Run the streaming example
            await StreamingQuickstartExample.RunAsync(kernel);

            Console.WriteLine("\n✅ Streaming Quickstart Example completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in Streaming Quickstart Example: {ex.Message}");
            Console.WriteLine("💡 Note: This example requires a valid OpenAI API key to run properly.");
            Console.WriteLine("💡 Set the OPENAI_API_KEY environment variable or update the code with your key.");
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
        await ConditionalEdgeExample.RunAllExamplesAsync();
        await CheckpointingQuickstartExample.RunAllExamplesAsync();
        await StateQuickstartExample.RunAsync();
        await StateAndSerializationExample.RunAsync();
        await ValidationCompilationExample.RunAsync();
        await InstallationExample.RunAllExamplesAsync();
        await MetricsLoggingQuickstartExample.RunBasicExampleAsync();
        await ReactCotQuickstartExample.RunAllExamplesAsync();
        await RunStreamingQuickstartExample();
        await TroubleshootingExample.RunAsync();
        await DynamicRoutingExample.RunAsync();
        await ExecutionModelExample.RunAsync();
        await ExecutionConceptsExample.RunAsync();
        AdditionalUtilitiesExample.RunAllDemonstrations();

        Console.WriteLine("\n" + "=".PadLeft(50, '='));
        Console.WriteLine("📋 All examples completed!");
    }
}

// Add demo runners for the new examples.
static class ExampleRunners
{
    /// <summary>
    /// Runs a minimal demo of SimpleNodeExample to validate compilation and basic behavior.
    /// </summary>
    public static async Task RunSimpleNodeExample()
    {
        Console.WriteLine("🔧 Running SimpleNodeExample demo...");

        // Create a minimal kernel instance used only to satisfy method signatures.
        var kernel = Kernel.CreateBuilder().Build();

        // Create kernel arguments and set 'input'
        var args = new KernelArguments();
        args["input"] = "hello world";

        var node = new SimpleNodeExample();

        // Validate execution
        var validation = node.ValidateExecution(args);
        if (!validation.IsValid)
        {
            Console.WriteLine($"Validation failed: {string.Join(", ", validation.Errors)}");
            return;
        }

        // Execute node
        var result = await node.ExecuteAsync(kernel, args);
        var resultStr = result == null ? string.Empty : result.GetValue<object>()?.ToString() ?? string.Empty;
        Console.WriteLine($"Execution result: {resultStr}");

        Console.WriteLine("✅ SimpleNodeExample demo completed.");
    }

    /// <summary>
    /// Runs a minimal demo of ConditionalNodeExample to validate routing logic.
    /// </summary>
    public static async Task RunConditionalNodeExample()
    {
        Console.WriteLine("🔧 Running ConditionalNodeExample demo...");

        // Predicate: return true when 'route' argument equals 'yes'
        Func<KernelArguments, bool> predicate = (ka) => ka.ContainsKey("route") && string.Equals(ka["route"]?.ToString(), "yes", StringComparison.OrdinalIgnoreCase);

        var node = new ConditionalNodeExample("node-1", "conditional-demo", predicate);

        // Add a simple next node (reusing SimpleNodeExample)
        var next = new SimpleNodeExample();
        node.AddNextNode(next);

        // Prepare a graph state with KernelArguments
        var graphState = new GraphState(new KernelArguments { ["route"] = "yes", ["input"] = "conditional input" });

        var nextNodes = node.GetNextNodes(null, graphState);
        Console.WriteLine($"Next nodes count: {nextNodes.Count()}");

        Console.WriteLine("✅ ConditionalNodeExample demo completed.");
        await Task.CompletedTask;
    }
}

/// <summary>
/// Simple console logger implementation for examples
/// </summary>
public class ConsoleLogger : ILogger
{
    public static ConsoleLogger Instance { get; } = new ConsoleLogger();

    private ConsoleLogger() { }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        var logLevelString = logLevel switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "CRITICAL",
            _ => "UNKNOWN"
        };

        Console.WriteLine($"[{logLevelString}] {message}");
        if (exception != null)
        {
            Console.WriteLine($"Exception: {exception.Message}");
        }
    }
}
