using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Execution;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Integration;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;

namespace Examples;

/// <summary>
/// Demonstrates troubleshooting techniques and error recovery strategies
/// for common problems in SemanticKernel.Graph applications.
/// </summary>
public static class TroubleshootingExample
{
    private static Kernel? _kernel;
    private static ILogger? _logger;

    private static void Initialize(Kernel kernel, ILogger logger)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs the troubleshooting examples demonstrating various error scenarios and solutions.
    /// </summary>
    public static async Task RunAsync()
    {
        // Create a basic kernel for the example
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-3.5-turbo", Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "your-api-key-here")
            .AddGraphSupport()
            .Build();

        // Create a logger
        var logger = ConsoleLogger.Instance;

        // Initialize the example
        Initialize(kernel, logger);

        logger.LogInformation("Starting Troubleshooting Examples");

        try
        {
            // Example 1: Execution Performance Issues
            await DemonstrateExecutionPerformanceTroubleshootingAsync();

            // Example 2: Service Registration Issues
            await DemonstrateServiceRegistrationTroubleshootingAsync();

            // Example 3: State and Checkpoint Problems
            await DemonstrateStateCheckpointTroubleshootingAsync();

            // Example 4: Error Recovery and Resilience
            await DemonstrateErrorRecoveryTroubleshootingAsync();

            // Example 5: Performance Monitoring and Diagnostics
            await DemonstratePerformanceMonitoringTroubleshootingAsync();

            logger.LogInformation("All troubleshooting examples completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running troubleshooting examples");
            throw;
        }
    }

    /// <summary>
    /// Demonstrates troubleshooting for execution performance issues.
    /// </summary>
    private static async Task DemonstrateExecutionPerformanceTroubleshootingAsync()
    {
        _logger.LogInformation("=== Execution Performance Troubleshooting ===");

        try
        {
            // Create a graph with potential performance issues
            var graph = new GraphExecutor("performance-test-graph");

            // Add nodes to the graph
            var slowNode = new ActionGraphNode("slow-operation", "Slow Operation", "Simulates a slow operation");
            var fastNode = new ActionGraphNode("fast-operation", "Fast Operation", "Simulates a fast operation");

            graph.AddNode(slowNode);
            graph.AddNode(fastNode);

            // Configure execution options with performance monitoring
            var executionOptions = GraphExecutionOptions.CreateDefault();

            // Execute with performance monitoring
            var startTime = DateTimeOffset.UtcNow;

            // Create arguments for execution
            var arguments = new KernelArguments();
            arguments["input"] = "test input";

            var result = await graph.ExecuteAsync(_kernel, arguments, CancellationToken.None);
            var executionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Graph execution completed in {ExecutionTime:F2}ms", executionTime.TotalMilliseconds);

            // Analyze performance metrics if available
            if (result.Metadata != null && result.Metadata.ContainsKey("ExecutionMetrics"))
            {
                _logger.LogInformation("Execution metrics available in result metadata");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in execution performance troubleshooting");
        }
    }

    /// <summary>
    /// Demonstrates troubleshooting for service registration issues.
    /// </summary>
    private static async Task DemonstrateServiceRegistrationTroubleshootingAsync()
    {
        _logger.LogInformation("=== Service Registration Troubleshooting ===");

        try
        {
            // Check if graph support is properly configured
            var serviceProvider = _kernel.Services;
            var graphExecutorFactory = serviceProvider.GetService<IGraphExecutorFactory>();

            if (graphExecutorFactory == null)
            {
                _logger.LogWarning("Graph support not enabled! This will cause errors.");

                // Demonstrate the correct way to configure services
                _logger.LogInformation("Correct configuration should include:");
                _logger.LogInformation("builder.AddGraphSupport(options => {{");
                _logger.LogInformation("    options.EnableMetrics = true;");
                _logger.LogInformation("    options.EnableCheckpointing = true;");
                _logger.LogInformation("}});");
            }
            else
            {
                _logger.LogInformation("Graph support is properly configured");
            }

            // Check for other essential services
            var checkpointManager = serviceProvider.GetService<ICheckpointManager>();
            var errorRecoveryEngine = serviceProvider.GetService<ErrorRecoveryEngine>();
            var metricsExporter = serviceProvider.GetService<GraphMetricsExporter>();

            _logger.LogInformation("Service availability check:");
            _logger.LogInformation("- GraphExecutorFactory: {GraphExecutorFactory}", graphExecutorFactory != null ? "Available" : "Missing");
            _logger.LogInformation("- CheckpointManager: {CheckpointManager}", checkpointManager != null ? "Available" : "Missing");
            _logger.LogInformation("- ErrorRecoveryEngine: {ErrorRecoveryEngine}", errorRecoveryEngine != null ? "Available" : "Missing");
            _logger.LogInformation("- MetricsExporter: {MetricsExporter}", metricsExporter != null ? "Available" : "Missing");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking service registration");
        }
    }

    /// <summary>
    /// Demonstrates troubleshooting for state and checkpoint problems.
    /// </summary>
    private static async Task DemonstrateStateCheckpointTroubleshootingAsync()
    {
        _logger.LogInformation("=== State and Checkpoint Troubleshooting ===");

        try
        {
            // Test checkpointing functionality
            var serviceProvider = _kernel.Services;
            var checkpointManager = serviceProvider.GetService<ICheckpointManager>();

            if (checkpointManager != null)
            {
                // Test checkpoint creation
                var testState = new GraphState();
                testState.SetValue("test_key", "test_value");
                testState.SetValue("test_number", 42);

                var checkpoint = await checkpointManager.CreateCheckpointAsync(
                    "test-execution",
                    testState,
                    "test-node",
                    null,
                    CancellationToken.None);

                _logger.LogInformation("Checkpoint created successfully: {CheckpointId}", checkpoint.CheckpointId);

                // Test checkpoint restoration
                var restoredState = await checkpointManager.RestoreFromCheckpointAsync(
                    checkpoint.CheckpointId,
                    CancellationToken.None);

                if (restoredState != null)
                {
                    var restoredValue = restoredState.GetValue<string>("test_key");
                    _logger.LogInformation("Checkpoint restored successfully. Value: {Value}", restoredValue);
                }
                else
                {
                    _logger.LogWarning("Failed to restore checkpoint");
                }
            }
            else
            {
                _logger.LogWarning("Checkpointing service not available");
            }

            // Test state serialization
            var state = new GraphState();
            try
            {
                // Test with simple types
                state.SetValue("string_value", "test");
                state.SetValue("int_value", 123);
                state.SetValue("array_value", new[] { 1, 2, 3 });

                // Test serialization using the ISerializableState interface
                var serialized = state.Serialize();
                _logger.LogInformation("State serialization successful. Size: {Size} bytes", serialized.Length);

                // Test with complex types (this might fail)
                try
                {
                    state.SetValue("complex_object", new NonSerializableType());
                    var complexSerialized = state.Serialize();
                    _logger.LogInformation("Complex object serialization successful");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Complex object serialization failed (expected): {Error}", ex.Message);
                    _logger.LogInformation("Solution: Use simple types or implement ISerializableState");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "State serialization failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in state and checkpoint troubleshooting");
        }
    }

    /// <summary>
    /// Demonstrates troubleshooting for error recovery and resilience.
    /// </summary>
    private static async Task DemonstrateErrorRecoveryTroubleshootingAsync()
    {
        _logger.LogInformation("=== Error Recovery and Resilience Troubleshooting ===");

        try
        {
            // Create a graph with error handling
            var errorHandlerNode = new ErrorHandlerGraphNode("error-handler", "Error Handler", "Handles errors during execution");
            var fallbackNode = new ActionGraphNode("fallback", "Fallback Operation", "Fallback operation executed due to error");

            var graph = new GraphExecutor("error-recovery-test-graph");

            // Add nodes to the graph
            var riskyNode = new ActionGraphNode("risky-operation", "Risky Operation", "Simulates a risky operation that might fail");
            graph.AddNode(riskyNode);
            graph.AddNode(errorHandlerNode);
            graph.AddNode(fallbackNode);

            // Configure error handling
            errorHandlerNode.ConfigureErrorHandler(GraphErrorType.Validation, ErrorRecoveryAction.Skip);
            errorHandlerNode.ConfigureErrorHandler(GraphErrorType.Network, ErrorRecoveryAction.Retry);
            errorHandlerNode.AddFallbackNode(GraphErrorType.Unknown, fallbackNode);

            // Execute with error handling
            var arguments = new KernelArguments();
            arguments["input"] = "test input";

            var result = await graph.ExecuteAsync(_kernel, arguments, CancellationToken.None);

            if (result != null)
            {
                _logger.LogInformation("Graph executed successfully with error handling");
            }
            else
            {
                _logger.LogWarning("Graph execution encountered errors but was handled");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in error recovery troubleshooting");
        }
    }

    /// <summary>
    /// Demonstrates troubleshooting for performance monitoring and diagnostics.
    /// </summary>
    private static async Task DemonstratePerformanceMonitoringTroubleshootingAsync()
    {
        _logger.LogInformation("=== Performance Monitoring and Diagnostics Troubleshooting ===");

        try
        {
            // Configure comprehensive monitoring
            var serviceProvider = _kernel.Services;
            var metricsExporter = serviceProvider.GetService<GraphMetricsExporter>();

            if (metricsExporter != null)
            {
                // Create sample performance metrics for demonstration
                var performanceMetrics = new GraphPerformanceMetrics();

                // Export metrics in different formats
                var jsonMetrics = metricsExporter.ExportMetrics(performanceMetrics, MetricsExportFormat.Json);
                _logger.LogInformation("Current metrics exported successfully in JSON format");

                // Export for dashboard visualization
                var dashboardMetrics = metricsExporter.ExportForDashboard(performanceMetrics, DashboardType.Grafana);
                _logger.LogInformation("Dashboard metrics exported successfully for Grafana");

                // Check for performance anomalies
                if (jsonMetrics.Contains("error") || jsonMetrics.Contains("failure"))
                {
                    _logger.LogWarning("Performance issues detected in metrics");
                    _logger.LogInformation("Consider implementing circuit breakers or fallbacks");
                }
            }
            else
            {
                _logger.LogWarning("Metrics exporter not available");
            }

            // Test logging configuration
            var logger = serviceProvider.GetService<ILogger<GraphExecutor>>();
            if (logger != null)
            {
                logger.LogInformation("Graph execution logging is properly configured");
            }
            else
            {
                _logger.LogWarning("Graph execution logger not available");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in performance monitoring troubleshooting");
        }
    }

    /// <summary>
    /// Demonstrates common troubleshooting patterns and solutions.
    /// </summary>
    private static async Task DemonstrateCommonTroubleshootingPatternsAsync()
    {
        _logger.LogInformation("=== Common Troubleshooting Patterns ===");

        // Pattern 1: Circuit Breaker Implementation
        _logger.LogInformation("Pattern 1: Circuit Breaker for External Services");
        var circuitBreaker = new CircuitBreaker(
            failureThreshold: 5,
            recoveryTimeout: TimeSpan.FromMinutes(1));

        // Pattern 2: Retry with Exponential Backoff
        _logger.LogInformation("Pattern 2: Retry with Exponential Backoff");
        var retryPolicy = new ExponentialBackoffRetryPolicy(
            maxRetries: 3,
            initialDelay: TimeSpan.FromSeconds(1));

        // Pattern 3: Fallback Strategy
        _logger.LogInformation("Pattern 3: Fallback Strategy Implementation");
        _logger.LogInformation("Primary operation failed, executing fallback...");

        // Pattern 4: Health Check
        _logger.LogInformation("Pattern 4: Health Check Implementation");
        var healthStatus = await CheckSystemHealthAsync();
        _logger.LogInformation("System health status: {Status}", healthStatus);

        // Pattern 5: Resource Monitoring
        _logger.LogInformation("Pattern 5: Resource Monitoring");
        var memoryUsage = GC.GetTotalMemory(false);
        var processorCount = Environment.ProcessorCount;
        _logger.LogInformation("Memory usage: {Memory} bytes, CPU cores: {Cores}",
            memoryUsage, processorCount);
    }

    /// <summary>
    /// Performs a basic system health check.
    /// </summary>
    private static async Task<string> CheckSystemHealthAsync()
    {
        try
        {
            // Check essential services
            var serviceProvider = _kernel.Services;
            var graphExecutorFactory = serviceProvider.GetService<IGraphExecutorFactory>();
            var checkpointManager = serviceProvider.GetService<ICheckpointManager>();

            var healthChecks = new List<bool>
            {
                graphExecutorFactory != null,
                checkpointManager != null,
                Environment.ProcessorCount > 0,
                GC.GetTotalMemory(false) < 1024 * 1024 * 1024 // Less than 1GB
            };

            var healthyChecks = healthChecks.Count(h => h);
            var totalChecks = healthChecks.Count;
            var healthPercentage = (double)healthyChecks / totalChecks;

            return healthPercentage switch
            {
                >= 0.8 => "Healthy",
                >= 0.6 => "Degraded",
                >= 0.4 => "Unhealthy",
                _ => "Critical"
            };
        }
        catch
        {
            return "Unknown";
        }
    }
}

/// <summary>
/// Example of a non-serializable type that would cause serialization issues.
/// </summary>
public class NonSerializableType
{
    public string Name { get; set; } = "Test";
    public DateTime Created { get; set; } = DateTime.Now;

    // This type is not serializable and would cause issues
    // Solution: Implement ISerializableState or use simple types
}

/// <summary>
/// Simple circuit breaker implementation for demonstration.
/// </summary>
public class CircuitBreaker
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _recoveryTimeout;
    private int _failureCount;
    private DateTime _lastFailureTime;
    private CircuitBreakerState _state;

    public CircuitBreaker(int failureThreshold, TimeSpan recoveryTimeout)
    {
        _failureThreshold = failureThreshold;
        _recoveryTimeout = recoveryTimeout;
        _state = CircuitBreakerState.Closed;
    }

    public bool CanExecute()
    {
        return _state switch
        {
            CircuitBreakerState.Closed => true,
            CircuitBreakerState.Open => DateTime.Now - _lastFailureTime > _recoveryTimeout,
            CircuitBreakerState.HalfOpen => true,
            _ => false
        };
    }

    public void RecordSuccess()
    {
        _failureCount = 0;
        _state = CircuitBreakerState.Closed;
    }

    public void RecordFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.Now;

        if (_failureCount >= _failureThreshold)
        {
            _state = CircuitBreakerState.Open;
        }
    }

    private enum CircuitBreakerState
    {
        Closed,
        Open,
        HalfOpen
    }
}

/// <summary>
/// Simple retry policy implementation for demonstration.
/// </summary>
public class ExponentialBackoffRetryPolicy
{
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;

    public ExponentialBackoffRetryPolicy(int maxRetries, TimeSpan initialDelay)
    {
        _maxRetries = maxRetries;
        _initialDelay = initialDelay;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        var attempt = 0;
        var delay = _initialDelay;

        while (attempt < _maxRetries)
        {
            try
            {
                return await operation();
            }
            catch (Exception) when (attempt < _maxRetries - 1)
            {
                attempt++;
                await Task.Delay(delay);
                delay = TimeSpan.FromTicks(delay.Ticks * 2); // Exponential backoff
            }
        }

        // Last attempt
        return await operation();
    }
}
