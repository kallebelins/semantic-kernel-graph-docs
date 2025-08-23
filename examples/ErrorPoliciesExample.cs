using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

namespace Examples;

/// <summary>
/// Example demonstrating registration and usage of error policies, an error handler node,
/// and the error metrics collector. This example is intentionally minimal and focuses on
/// exercising the documented APIs to ensure they compile and run in the examples project.
/// </summary>
public static class ErrorPoliciesExample
{
    /// <summary>
    /// Runs the error policies demonstration.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Error Policies Example ===\n");

        try
        {
            // Create a kernel with graph support. No external LLM is required for this demo.
            var kernel = Kernel.CreateBuilder()
                .AddGraphSupport()
                .Build();

            // Create registry and register a basic retry policy.
            var registry = new ErrorPolicyRegistry(new ErrorPolicyRegistryOptions());

            // Register a retry policy for network errors (documented example).
            registry.RegisterPolicyRule(new PolicyRule
            {
                ContextId = "Examples",
                ErrorType = GraphErrorType.Network,
                RecoveryAction = ErrorRecoveryAction.Retry,
                MaxRetries = 3,
                RetryDelay = TimeSpan.FromSeconds(1),
                BackoffMultiplier = 2.0,
                Priority = 100,
                Description = "Retry network errors"
            });

            // Register a circuit breaker policy for a hypothetical node "api-node".
            registry.RegisterNodeCircuitBreakerPolicy("api-node", new CircuitBreakerPolicyConfig
            {
                Enabled = true,
                FailureThreshold = 3,
                OpenTimeout = TimeSpan.FromSeconds(30),
                FailureWindow = TimeSpan.FromMinutes(1)
            });

            Console.WriteLine("Registered example policies in ErrorPolicyRegistry.");

            // Create an ErrorHandlerGraphNode and simulate handling an HttpRequestException.
            var errorHandler = new ErrorHandlerGraphNode("error-handler-1", "MainErrorHandler", "Example error handler");

            // Prepare kernel arguments with the last error to trigger the handler.
            var args = new KernelArguments();
            args["LastError"] = new HttpRequestException("Simulated network failure");
            args["AttemptCount"] = 1;

            // Execute the error handler node directly to validate runtime behavior.
            var result = await errorHandler.ExecuteAsync(kernel, args);

            // Output result metadata to verify the handler returned expected keys.
            Console.WriteLine("Error handler executed. Result metadata:");
            if (result?.Metadata != null)
            {
                foreach (var kvp in result.Metadata)
                {
                    Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                }
            }

            // Demonstrate metrics collector usage by recording an example event.
            using var metricsCollector = new ErrorMetricsCollector(new ErrorMetricsOptions());

            // Create an error handling context similar to what the handler would produce.
            var errorContext = new ErrorHandlingContext
            {
                Exception = new HttpRequestException("Simulated network failure for metrics"),
                ErrorType = GraphErrorType.Network,
                Severity = ErrorSeverity.Medium,
                IsTransient = true,
                AttemptNumber = 1,
            };

            // Record an error event to the collector and query the metrics back.
            metricsCollector.RecordError(executionId: "exec-1", nodeId: "api-node", errorContext: errorContext,
                recoveryAction: ErrorRecoveryAction.Retry, recoverySuccess: true);

            var execMetrics = metricsCollector.GetExecutionMetrics("exec-1");
            Console.WriteLine("\nRecorded metrics:");
            if (execMetrics != null)
            {
                Console.WriteLine($"  Total errors: {execMetrics.TotalErrors}");
                Console.WriteLine($"  Recovery success rate: {execMetrics.RecoverySuccessRate:F2}");
                Console.WriteLine($"  Most common error: {execMetrics.MostCommonErrorType}");
            }

            Console.WriteLine("\n✅ Error Policies example completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error running Error Policies Example: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}


