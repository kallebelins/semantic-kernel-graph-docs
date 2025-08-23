using SemanticKernel.Graph.Core;

namespace Examples;

/// <summary>
/// Demonstrates basic usage of GraphPerformanceMetrics and GraphMetricsExporter.
/// This example constructs a metrics collector, simulates a few node executions,
/// and prints exported JSON and Prometheus outputs to validate behavior.
/// </summary>
public static class MetricsExample
{
    /// <summary>
    /// Runs the metrics example.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("▶ Running Metrics example...");

        // Create options with short sampling interval for demonstration
        var options = GraphMetricsOptions.CreateDevelopmentOptions();
        options.EnableResourceMonitoring = false; // Keep example deterministic

        using var metrics = new GraphPerformanceMetrics(options);

        // Simulate few node executions
        for (int i = 0; i < 5; i++)
        {
            var execId = $"exec-{i}";
            var tracker = metrics.StartNodeTracking($"node-{i}", $"node-name-{i}", execId);
            await Task.Delay(10 + i * 5); // Simulate execution time
            metrics.CompleteNodeTracking(tracker, success: true, result: null);
            metrics.RecordExecutionPath(execId, new[] { tracker.NodeId }, TimeSpan.FromMilliseconds(10 + i * 5), success: true);
        }

        // Export metrics to JSON and Prometheus formats to validate exporter
        using var exporter = new GraphMetricsExporter();

        var json = exporter.ExportMetrics(metrics, MetricsExportFormat.Json, TimeSpan.FromMinutes(10));
        Console.WriteLine("\n--- JSON Export Preview ---\n");
        Console.WriteLine(json.Substring(0, Math.Min(800, json.Length)));

        var prometheus = exporter.ExportMetrics(metrics, MetricsExportFormat.Prometheus, TimeSpan.FromMinutes(10));
        Console.WriteLine("\n--- Prometheus Export Preview ---\n");
        Console.WriteLine(prometheus.Substring(0, Math.Min(800, prometheus.Length)));

        Console.WriteLine("\n✅ Metrics example finished.");
    }
}


