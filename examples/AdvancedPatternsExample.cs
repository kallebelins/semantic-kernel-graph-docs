using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Integration;

namespace Examples;

/// <summary>
/// Advanced Patterns example adapted for documentation runnable sample.
/// All comments are in English and the code compiles against the examples project.
/// </summary>
public static class AdvancedPatternsExample
{
    /// <summary>
    /// Entry point used by the docs examples runner.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("Running Advanced Patterns Example from docs/examples...");

        // Create a minimal kernel with graph support for the example
        var kernel = Kernel.CreateBuilder()
            .AddGraphSupport()
            .Build();

        // Create a simple console logger factory
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        var graphLogger = new SemanticKernelGraphLogger(loggerFactory.CreateLogger("AdvancedPatternsDocs"), new GraphOptions());

        // Reuse the implementation from the library examples where possible.
        var executor = await CreateAdvancedGraphExecutorAsync(kernel, graphLogger);

        // Run a subset of the full example to validate compilation and runtime.
        await DemonstrateAcademicPatternsAsync(executor);
        await DemonstrateAdvancedOptimizationsAsync(executor, graphLogger);

        Console.WriteLine("Advanced Patterns docs example completed.");
    }

    // The methods below are adapted and simplified from the library examples to ensure
    // the sample is runnable and focused on validating the documented code snippets.

    private static async Task<GraphExecutor> CreateAdvancedGraphExecutorAsync(Kernel kernel, IGraphLogger graphLogger)
    {
        var executor = new GraphExecutor(kernel, graphLogger);

        executor.WithAllAdvancedPatterns(config =>
        {
            config.EnableAcademicPatterns = true;
            config.Academic.EnableCircuitBreaker = true;
            config.Academic.EnableBulkhead = true;
            config.Academic.EnableCacheAside = true;

            config.Academic.CircuitBreakerOptions.FailureThreshold = 3;
            config.Academic.CircuitBreakerOptions.OpenTimeout = TimeSpan.FromSeconds(10);

            config.Academic.BulkheadOptions.MaxConcurrency = 5;
            config.Academic.BulkheadOptions.AcquisitionTimeout = TimeSpan.FromSeconds(15);

            config.Academic.CacheAsideOptions.MaxCacheSize = 1000;
            config.Academic.CacheAsideOptions.DefaultTtl = TimeSpan.FromMinutes(10);
        });

        await Task.CompletedTask;
        return executor;
    }

    private static async Task DemonstrateAcademicPatternsAsync(GraphExecutor executor)
    {
        // Circuit breaker: run a successful operation with a fallback defined.
        var circuitBreakerTest = await executor.ExecuteWithCircuitBreakerAsync(
            operation: async () =>
            {
                await Task.Delay(10);
                Console.WriteLine("Operation executed successfully");
                return "Success";
            },
            fallback: async () =>
            {
                Console.WriteLine("Fallback operation executed");
                return "Fallback";
            });

        Console.WriteLine($"Circuit breaker result: {circuitBreakerTest}");

        // Bulkhead: execute a few concurrent operations to validate bulkhead plumbing.
        var tasks = Enumerable.Range(1, 3).Select(async i =>
        {
            return await executor.ExecuteWithBulkheadAsync(async ct =>
            {
                await Task.Delay(20, ct);
                return $"Result-{i}";
            });
        });

        var results = await Task.WhenAll(tasks);
        Console.WriteLine($"Bulkhead results: {string.Join(',', results)}");

        // Cache-aside: ensure loader is invoked on miss.
        var r1 = await executor.GetOrSetCacheAsync("key-docs-1", async () =>
        {
            await Task.Delay(10);
            return new { Id = 1, Name = "DocUser" };
        });

        var r2 = await executor.GetOrSetCacheAsync("key-docs-1", async () =>
        {
            // This loader should not be called if caching works.
            throw new InvalidOperationException("Cache hit expected");
        });

        Console.WriteLine("Academic patterns validated in docs example.");
    }

    private static async Task DemonstrateAdvancedOptimizationsAsync(GraphExecutor executor, IGraphLogger graphLogger)
    {
        var metrics = new GraphPerformanceMetrics(new GraphMetricsOptions(), graphLogger);

        for (int i = 0; i < 5; i++)
        {
            var tracker = metrics.StartNodeTracking($"node_{i % 2}", $"TestNode{i % 2}", $"exec_{i}");
            await Task.Delay(10);
            metrics.CompleteNodeTracking(tracker, success: true);
        }

        var optimizationResult = await executor.OptimizeAsync(metrics);
        Console.WriteLine($"Optimization run completed: {optimizationResult.TotalOptimizations} optimizations identified (docs sample)");
    }
}


