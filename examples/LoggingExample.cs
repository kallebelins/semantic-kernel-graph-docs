using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Nodes;

namespace Examples;

/// <summary>
/// Runnable documentation example for logging features.
/// This file is used by the docs and must compile and run independently.
/// </summary>
public static class LoggingExample
{
    /// <summary>
    /// Runs all logging demonstrations (basic, advanced, error, export).
    /// </summary>
    public static async Task RunAllAsync()
    {
        // Use a kernel with no external AI calls for deterministic local demos
        var kernel = Kernel.CreateBuilder().Build();

        // Basic logging demo
        await RunBasicLoggingDemo(kernel);

        // Advanced logging demo
        await RunAdvancedLoggingDemo(kernel);

        // Error logging demo
        await RunErrorLoggingDemo(kernel);

        // Export logs demo (writes files under ./export-logs)
        await RunLogExportDemo(kernel);
    }

    /// <summary>
    /// Basic logging configuration and sample execution.
    /// </summary>
    private static async Task RunBasicLoggingDemo(Kernel kernel)
    {
        Console.WriteLine("=== Basic Logging Demo ===\n");

        var graph = new GraphExecutor("LoggingGraph", "Basic logging demo");

        var processor = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() => "ok", "noop"),
            "logging-processor",
            "Logging Processor");

        var aggregator = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() => "aggregated", "agg"),
            "log-aggregator",
            "Log Aggregator");

        graph.AddNode(processor).AddNode(aggregator).SetStartNode(processor.NodeId);

        // Execute a few times
        for (int i = 0; i < 3; i++)
        {
            var result = await graph.ExecuteAsync(kernel, new KernelArguments { ["input_data"] = $"sample {i + 1}" });
            Console.WriteLine($"Executed iteration {i + 1}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Advanced structured logging demo using heavier computation to produce metrics.
    /// </summary>
    private static async Task RunAdvancedLoggingDemo(Kernel kernel)
    {
        Console.WriteLine("=== Advanced Logging Demo ===\n");

        var graph = new GraphExecutor("AdvancedLoggingGraph", "Advanced logging demo");

        var advanced = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() =>
            {
                // Simulate CPU work
                Thread.Sleep(50);
                return "advanced-result";
            }, "advanced"),
            "advanced-processor",
            "Advanced Processor");

        graph.AddNode(advanced).SetStartNode(advanced.NodeId);

        var result = await graph.ExecuteAsync(kernel, new KernelArguments { ["input_data"] = "advanced-sample" });
        Console.WriteLine($"Advanced run completed.\n");
    }

    /// <summary>
    /// Demonstrates error logging by throwing in some runs and observing behavior.
    /// </summary>
    private static async Task RunErrorLoggingDemo(Kernel kernel)
    {
        Console.WriteLine("=== Error Logging Demo ===\n");

        var graph = new GraphExecutor("ErrorLoggingGraph", "Error logging demo");

        var errorNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() =>
            {
                // Intentionally throw on demand
                if (Random.Shared.NextDouble() < 0.5)
                    throw new InvalidOperationException("simulated failure");
                return "success";
            }, "maybe-fail"),
            "error-prone-processor",
            "Error Prone Processor");

        var monitor = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() => "monitor", "monitor"),
            "error-monitor",
            "Error Monitor");

        graph.AddNode(errorNode).AddNode(monitor).SetStartNode(errorNode.NodeId);

        for (int i = 0; i < 3; i++)
        {
            try
            {
                var result = await graph.ExecuteAsync(kernel, new KernelArguments { ["input_data"] = $"run-{i}" });
                Console.WriteLine($"Run {i}: succeeded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Run {i}: failed with {ex.GetType().Name}");
            }
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Writes sample export files demonstrating the log export helpers in the docs.
    /// </summary>
    private static async Task RunLogExportDemo(Kernel kernel)
    {
        Console.WriteLine("=== Log Export Demo ===\n");

        var exportDir = Path.Combine(Directory.GetCurrentDirectory(), "export-logs");
        Directory.CreateDirectory(exportDir);

        var logs = new List<Dictionary<string, object>>();
        for (int i = 0; i < 5; i++)
        {
            logs.Add(new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.UtcNow,
                ["level"] = i % 2 == 0 ? "Information" : "Warning",
                ["message"] = $"sample log {i}",
                ["node_id"] = "log-generator",
                ["execution_id"] = Guid.NewGuid().ToString()
            });
        }

        var jsonFile = Path.Combine(exportDir, $"logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
        await File.WriteAllTextAsync(jsonFile, System.Text.Json.JsonSerializer.Serialize(logs, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        Console.WriteLine($"Exported logs to: {jsonFile}");
        Console.WriteLine();
    }
}


