using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Integration;
using SemanticKernel.Graph.Nodes;

namespace SemanticKernel.Graph.Docs.Examples;

/// <summary>
/// Example demonstrating metrics and logging quickstart functionality.
/// Shows how to enable comprehensive metrics collection and structured logging.
/// </summary>
public static class MetricsLoggingQuickstartExample
{
    private static string? openAiApiKey;
    private static string openAiModel = "gpt-3.5-turbo";

    static MetricsLoggingQuickstartExample()
    {
        // Load configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        openAiApiKey = configuration["OpenAI:ApiKey"];
        openAiModel = configuration["OpenAI:Model"] ?? openAiModel;
    }

    /// <summary>
    /// Demonstrates basic metrics and logging setup.
    /// </summary>
    public static async Task RunBasicExampleAsync()
    {
        Console.WriteLine("=== Basic Metrics and Logging Example ===\n");

        if (string.IsNullOrEmpty(openAiApiKey))
        {
            Console.WriteLine("⚠️  OpenAI API Key not found in appsettings.json. Using mock key for demonstration.");
            openAiApiKey = "mock-api-key";
        }

        // 1. Enable Metrics and Logging
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(openAiModel, openAiApiKey)
            .AddGraphSupport(options =>
            {
                options.EnableLogging = true;
                options.EnableMetrics = true;
            })
            .Build();

        // 2. Create a Graph with Metrics
        var graph = new GraphExecutor("MyGraph", "Example graph with metrics");

        // Enable development metrics (detailed tracking, frequent sampling)
        graph.EnableDevelopmentMetrics();

        // Or use production metrics (optimized for performance)
        // graph.EnableProductionMetrics();

        // 3. Add Nodes and Execute
        var node1 = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() => "Hello!", "greeting"),
            "greeting",
            "Greeting");

        var node2 = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() => "Processing...", "processing"),
            "processing",
            "Processing");

        graph.AddNode(node1)
             .AddNode(node2)
             .Connect(node1.NodeId, node2.NodeId)
             .SetStartNode(node1.NodeId);

        // Execute multiple times to generate meaningful metrics
        Console.WriteLine("Executing graph multiple times to collect metrics...");
        for (int i = 0; i < 10; i++)
        {
            try
            {
                var result = await graph.ExecuteAsync(kernel, new KernelArguments());
                Console.Write(".");
            }
            catch (Exception)
            {
                Console.Write("X"); // Failed execution
            }
        }
        Console.WriteLine("\n");

        // Display performance data
        await DisplayPerformanceDataAsync(graph);
    }

    /// <summary>
    /// Demonstrates advanced metrics configuration.
    /// </summary>
    public static async Task RunAdvancedMetricsExampleAsync()
    {
        Console.WriteLine("=== Advanced Metrics Configuration Example ===\n");

        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(openAiModel, openAiApiKey)
            .Build();

        var graph = new GraphExecutor("AdvancedMetricsGraph", "Advanced metrics configuration");

        // Custom Metrics Options
        var metricsOptions = GraphMetricsOptions.CreateProductionOptions();
        metricsOptions.EnableResourceMonitoring = true;  // Monitor CPU and memory
        metricsOptions.ResourceSamplingInterval = TimeSpan.FromSeconds(10);
        metricsOptions.MaxSampleHistory = 10000;
        metricsOptions.EnableDetailedPathTracking = true;

        graph.ConfigureMetrics(metricsOptions);

        // Add test nodes
        var workNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() =>
            {
                Thread.Sleep(Random.Shared.Next(100, 300)); // Simulate work
                return "Work completed";
            }, "work"),
            "work",
            "Work");

        graph.AddNode(workNode).SetStartNode(workNode.NodeId);

        // Execute to collect metrics
        Console.WriteLine("Executing graph with advanced metrics...");
        for (int i = 0; i < 5; i++)
        {
            await graph.ExecuteAsync(kernel, new KernelArguments());
            Console.Write(".");
        }
        Console.WriteLine("\n");

        // Real-Time Monitoring
        var dashboard = new MetricsDashboard(graph.PerformanceMetrics!);

        // Generate real-time metrics
        var realtimeMetrics = dashboard.GenerateRealTimeMetrics();
        Console.WriteLine("Real-time Metrics:");
        Console.WriteLine(realtimeMetrics);

        // Or get a comprehensive dashboard report
        var dashboardReport = dashboard.GenerateDashboard(
            timeWindow: TimeSpan.FromMinutes(10),
            includeNodeDetails: true,
            includePathAnalysis: true);
        Console.WriteLine("\nComprehensive Dashboard Report:");
        Console.WriteLine(dashboardReport);
    }

    /// <summary>
    /// Demonstrates logging configuration.
    /// </summary>
    public static Task RunLoggingExampleAsync()
    {
        Console.WriteLine("=== Logging Configuration Example ===\n");

        // Structured Logging Setup
        var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        var graphLogger = new SemanticKernelGraphLogger(
            loggerFactory.CreateLogger("MyGraph"), 
            new GraphOptions { EnableLogging = true });

        // The logger automatically tracks execution context and correlation
        Console.WriteLine("Logger configured successfully!");

        // Logging Extensions
        var executionId = Guid.NewGuid().ToString();
        var nodeId = "test-node";

        // Log graph-level information
        graphLogger.LogGraphInfo(executionId, "Graph execution started");

        // Log node-level details
        graphLogger.LogNodeInfo(executionId, nodeId, "Node processing started");

        // Log performance metrics
        graphLogger.LogPerformance(executionId, "execution_time", 150.5, "ms", 
            new Dictionary<string, string> { ["node_type"] = "function" });

        Console.WriteLine("Logging examples completed. Check console output for log messages.");
        
        return Task.CompletedTask;
    }

    #region Helper Methods

    private static Task DisplayPerformanceDataAsync(GraphExecutor graph)
    {
        // Basic Performance Summary
        var summary = graph.GetPerformanceSummary(TimeSpan.FromMinutes(5));
        if (summary != null)
        {
            Console.WriteLine("📊 PERFORMANCE SUMMARY");
            Console.WriteLine("".PadRight(50, '-'));
            Console.WriteLine($"Total Executions: {summary.TotalExecutions}");
            Console.WriteLine($"Success Rate: {summary.SuccessRate:F1}%");
            Console.WriteLine($"Average Execution Time: {summary.AverageExecutionTime.TotalMilliseconds:F2}ms");
            Console.WriteLine($"Throughput: {summary.Throughput:F2} executions/second");
            
            // Check system health
            var isHealthy = summary.IsHealthy();
            Console.WriteLine($"System Health: {(isHealthy ? "🟢 HEALTHY" : "🔴 NEEDS ATTENTION")}");
            Console.WriteLine();
        }

        // Node-Level Metrics
        var nodeMetrics = graph.GetAllNodeMetrics();
        if (nodeMetrics.Count > 0)
        {
            Console.WriteLine("🔧 NODE PERFORMANCE");
            Console.WriteLine("".PadRight(50, '-'));
            Console.WriteLine($"{"Node",-15} {"Executions",-12} {"Avg Time",-12} {"Success %",-10} {"Rating",-12}");
            Console.WriteLine("".PadRight(70, '-'));

            foreach (var kvp in nodeMetrics.OrderByDescending(x => x.Value.TotalExecutionTime))
            {
                var node = kvp.Value;
                var rating = node.GetPerformanceClassification();
                
                Console.WriteLine($"{node.NodeName.Substring(0, Math.Min(14, node.NodeName.Length)),-15} " +
                                 $"{node.TotalExecutions,-12} " +
                                 $"{node.AverageExecutionTime.TotalMilliseconds,-12:F2}ms " +
                                 $"{node.SuccessRate,-10:F1}% " +
                                 $"{rating,-12}");
            }
            Console.WriteLine();
        }

        // Execution Path Analysis
        var pathMetrics = graph.GetPathMetrics();
        if (pathMetrics.Count > 0)
        {
            Console.WriteLine("🛣️ EXECUTION PATHS");
            Console.WriteLine("".PadRight(50, '-'));
            foreach (var kvp in pathMetrics.OrderByDescending(x => x.Value.ExecutionCount))
            {
                var path = kvp.Value;
                Console.WriteLine($"Path: {path.PathKey}");
                Console.WriteLine($"  Executions: {path.ExecutionCount} | " +
                                 $"Avg Time: {path.AverageExecutionTime.TotalMilliseconds:F2}ms | " +
                                 $"Success: {path.SuccessRate:F1}%");
            }
            Console.WriteLine();
        }
        
        return Task.CompletedTask;
    }

    #endregion
}
