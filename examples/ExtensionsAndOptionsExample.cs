using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Extensions;

namespace Examples;

/// <summary>
/// Example that demonstrates configuring GraphOptions and using KernelBuilderExtensions
/// from the `extensions-and-options.md` documentation. This example is executable and
/// follows C# best practices with English comments.
/// </summary>
public static class ExtensionsAndOptionsExample
{
    /// <summary>
    /// Runs the example showing how to configure graph options and builder extensions.
    /// </summary>
    public static Task RunAsync()
    {
        // Create a Kernel builder and demonstrate simple graph configuration
        var builder = Kernel.CreateBuilder();

        // Configure GraphOptions using the fluent Action pattern
        builder.AddGraphSupport(options =>
        {
            // Enable logging and metrics
            options.EnableLogging = true;
            options.EnableMetrics = true;

            // Tune execution bounds for the example
            options.MaxExecutionSteps = 500;
            options.ExecutionTimeout = TimeSpan.FromMinutes(5);

            // Configure logging internals
            options.Logging.MinimumLevel = LogLevel.Debug;
            options.Logging.EnableStructuredLogging = true;
            options.Logging.EnableCorrelationIds = true;
        });

        // Optionally add memory and templates to illustrate AddCompleteGraphSupport
        builder.AddGraphMemory(memory =>
        {
            memory.EnableVectorSearch = true;
            memory.EnableSemanticSearch = false;
            memory.DefaultCollectionName = "docs-example-memory";
        });

        builder.AddGraphTemplates(templateOptions =>
        {
            templateOptions.EnableHandlebars = true;
            templateOptions.TemplateCacheSize = 50;
        });

        // Build the kernel (no external LLM configured here)
        var kernel = builder.Build();

        Console.WriteLine("Extensions and Options example configured successfully.");

        // Show how to read back configured options from DI
        var optionsFromServices = kernel.Services.GetService(typeof(GraphOptions)) as GraphOptions;
        if (optionsFromServices != null)
        {
            Console.WriteLine($"EnableLogging: {optionsFromServices.EnableLogging}");
            Console.WriteLine($"MaxExecutionSteps: {optionsFromServices.MaxExecutionSteps}");
        }

        // Demonstrate KernelBuilderExtensions convenience method for creating a simple graph
        var graphExecutor = builder.CreateGraphWithTemplates("example-graph", "Example graph created from documentation demo");
        Console.WriteLine($"Created graph executor: {graphExecutor.Name}");

        return Task.CompletedTask;
    }
}


