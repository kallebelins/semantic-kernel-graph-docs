using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.State;
using SemanticKernel.Graph.Integration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Examples;

/// <summary>
/// Example demonstrating the usage of additional utilities in SemanticKernel.Graph.
/// This example covers utility classes and extension methods that are actually available in the codebase.
/// </summary>
public static class AdditionalUtilitiesExample
{
    private static readonly Kernel _kernel;
    private static readonly GraphExecutor _executor;
    private static readonly GraphState _graphState;

    static AdditionalUtilitiesExample()
    {
        // Initialize Semantic Kernel with basic graph support
        _kernel = Kernel.CreateBuilder()
            .AddGraphSupport()
            .Build();

        // Initialize GraphExecutor
        _executor = new GraphExecutor("additional-utilities-example");

        // Initialize GraphState with some test data
        var arguments = new KernelArguments
        {
            ["userInput"] = "Hello World",
            ["user_role"] = "admin",
            ["priority"] = "high"
        };
        _graphState = new GraphState(arguments);
    }

    /// <summary>
    /// Demonstrates the usage of AdvancedPatternsExtensions
    /// </summary>
    public static void DemonstrateAdvancedPatternsExtensions()
    {
        Console.WriteLine("=== Advanced Patterns Extensions Demo ===");

        try
        {
            // Configure executor with academic patterns
            var advancedExecutor = _executor.WithAcademicPatterns(config => 
            {
                config.EnableCircuitBreaker = true;
                config.EnableBulkhead = true;
                config.EnableCacheAside = true;
            });

            Console.WriteLine("Advanced patterns configured successfully");
            Console.WriteLine("Circuit Breaker: Enabled");
            Console.WriteLine("Bulkhead: Enabled");
            Console.WriteLine("Cache-Aside: Enabled");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Advanced patterns demo error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates the usage of DynamicRoutingExtensions
    /// </summary>
    public static void DemonstrateDynamicRoutingExtensions()
    {
        Console.WriteLine("\n=== Dynamic Routing Extensions Demo ===");

        try
        {
            // Enable dynamic routing
            var routingExecutor = _executor.EnableDynamicRouting();
            Console.WriteLine("Dynamic routing enabled successfully");

            // Get routing metrics (if available)
            var metrics = routingExecutor.GetRoutingMetrics();
            Console.WriteLine($"Routing metrics entries: {metrics.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Dynamic routing demo error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates the usage of GraphPerformanceExtensions
    /// </summary>
    public static void DemonstrateGraphPerformanceExtensions()
    {
        Console.WriteLine("\n=== Graph Performance Extensions Demo ===");

        try
        {
            // Enable performance metrics
            var metricsExecutor = _executor.EnableMetrics();
            Console.WriteLine("Performance metrics enabled");

            // Enable development metrics
            var devMetricsExecutor = _executor.EnableDevelopmentMetrics();
            Console.WriteLine("Development metrics enabled");

            // Enable production metrics
            var prodMetricsExecutor = _executor.EnableProductionMetrics();
            Console.WriteLine("Production metrics enabled");

            // Get performance summary
            var summary = metricsExecutor.GetPerformanceSummary();
            if (summary != null)
            {
                Console.WriteLine($"Total execution time: {summary.TotalExecutionTime}");
                Console.WriteLine($"Success rate: {summary.SuccessRate:P1}");
                Console.WriteLine($"Is healthy: {summary.IsHealthy()}");
            }
            else
            {
                Console.WriteLine("No performance data available yet");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Performance extensions demo error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates the usage of HumanInTheLoopExtensions
    /// </summary>
    public static void DemonstrateHumanInTheLoopExtensions()
    {
        Console.WriteLine("\n=== Human-in-the-Loop Extensions Demo ===");

        try
        {
            // Create a console interaction channel
            var channel = new ConsoleHumanInteractionChannel();

            // Add human approval node
            _executor.AddHumanApproval("approval", "Data Review", "Please review the processed data", channel);
            Console.WriteLine("Human approval node added");

            // Set timeout for human approvals
            _executor.WithHumanApprovalTimeout(TimeSpan.FromMinutes(5));
            Console.WriteLine("Human approval timeout configured");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Human-in-the-loop demo error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates the usage of LoggingExtensions
    /// </summary>
    public static void DemonstrateLoggingExtensions()
    {
        Console.WriteLine("\n=== Logging Extensions Demo ===");

        try
        {
            // Get logger from kernel services
            var logger = _kernel.Services.GetService(typeof(IGraphLogger)) as IGraphLogger;
            if (logger != null)
            {
                var executionId = Guid.NewGuid().ToString();

                // Log graph-level information
                logger.LogGraphInfo(executionId, "Graph execution started", 
                    new Dictionary<string, object> { ["nodeCount"] = 5 });
                Console.WriteLine("Graph info logged");

                // Log node-level information
                logger.LogNodeInfo(executionId, "node1", "Node execution completed", 
                    new Dictionary<string, object> { ["duration"] = "150ms" });
                Console.WriteLine("Node info logged");

                // Log graph debug message
                logger.LogGraphDebug(executionId, "Debug information");
                Console.WriteLine("Graph debug logged");
            }
            else
            {
                Console.WriteLine("Graph logger not available in services");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Logging demo error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates the usage of MultiAgentExtensions
    /// </summary>
    public static void DemonstrateMultiAgentExtensions()
    {
        Console.WriteLine("\n=== Multi-Agent Extensions Demo ===");

        try
        {
            // Create multi-agent coordinator
            var coordinator = _kernel.CreateMultiAgentCoordinator();
            Console.WriteLine("Multi-agent coordinator created");

            // Create a workflow builder
            var workflowBuilder = coordinator.CreateWorkflow("test-workflow", "Test Workflow");
            Console.WriteLine("Multi-agent workflow builder created");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Multi-agent demo error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates the usage of StateExtensions
    /// </summary>
    public static void DemonstrateStateExtensions()
    {
        Console.WriteLine("\n=== State Extensions Demo ===");

        try
        {
            // Clone state for parallel processing
            var clonedState = _graphState.Clone();
            Console.WriteLine("State cloned successfully");

            // Merge states
            var otherArguments = new KernelArguments
            {
                ["otherParam"] = "otherValue",
                ["newData"] = "additional data"
            };
            var otherState = new GraphState(otherArguments);
            var mergedState = _graphState.MergeFrom(otherState);
            Console.WriteLine("States merged successfully");

            // Display merged state parameters
            foreach (var param in mergedState.GetParameterNames())
            {
                Console.WriteLine($"Parameter: {param} = {mergedState.GetValue<object>(param)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"State extensions demo error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates the usage of StreamingExtensions
    /// </summary>
    public static void DemonstrateStreamingExtensions()
    {
        Console.WriteLine("\n=== Streaming Extensions Demo ===");

        try
        {
            // Convert executor to streaming
            var streamingExecutor = _executor.AsStreamingExecutor();
            Console.WriteLine("Executor converted to streaming successfully");

            // Create streaming options
            var options = StreamingExtensions.CreateStreamingOptions();
            Console.WriteLine("Streaming options created");

            // Configure streaming options
            var configuredOptions = options.Configure();
            Console.WriteLine("Streaming options configured with fluent API");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Streaming demo error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates the usage of StateHelpers utility class
    /// </summary>
    public static void DemonstrateStateHelpers()
    {
        Console.WriteLine("\n=== StateHelpers Utility Demo ===");

        try
        {
            // Serialize state with options
            var serialized = StateHelpers.SerializeState(_graphState, indented: true);
            Console.WriteLine($"State serialized: {serialized.Length} characters");

            // Deserialize state
            var restored = StateHelpers.DeserializeState(serialized);
            Console.WriteLine("State deserialized successfully");

            // Get compression statistics
            var compressionStats = StateHelpers.GetCompressionStats(serialized);
            Console.WriteLine($"Compression ratio: {compressionStats.CompressionRatio:P2}");

            // Get adaptive compression threshold
            var threshold = StateHelpers.GetAdaptiveCompressionThreshold();
            Console.WriteLine($"Adaptive compression threshold: {threshold} bytes");

            // Merge states with conflict resolution
            var otherArguments = new KernelArguments { ["conflictParam"] = "otherValue" };
            var otherState = new GraphState(otherArguments);
            var mergeResult = StateHelpers.MergeStates(_graphState, otherState, StateMergeConflictPolicy.PreferFirst);
            Console.WriteLine($"States merged successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"StateHelpers demo error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates the usage of StateValidator utility class
    /// </summary>
    public static void DemonstrateStateValidator()
    {
        Console.WriteLine("\n=== StateValidator Utility Demo ===");

        try
        {
            // Validate complete state
            var validation = StateValidator.ValidateState(_graphState);
            Console.WriteLine($"Complete validation - Is valid: {validation.IsValid}");

            if (!validation.IsValid)
            {
                foreach (var error in validation.Errors)
                {
                    Console.WriteLine($"Validation error: {error.Message}");
                }
            }

            // Validate critical properties
            var criticalValid = StateValidator.ValidateCriticalProperties(_graphState);
            Console.WriteLine($"Critical properties validation: {criticalValid}");

            // Validate serialization capability
            var serializationValidation = StateValidator.ValidateSerializability(_graphState);
            Console.WriteLine($"Serialization validation - Is valid: {serializationValidation.IsValid}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"StateValidator demo error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates the usage of ConditionalExpressionEvaluator utility class
    /// </summary>
    public static void DemonstrateConditionalExpressionEvaluator()
    {
        Console.WriteLine("\n=== ConditionalExpressionEvaluator Utility Demo ===");

        try
        {
            // Simple boolean evaluation
            var simpleResult = ConditionalExpressionEvaluator.Evaluate("true", _graphState);
            Console.WriteLine($"Simple evaluation result: {simpleResult.Value}");

            // Variable-based evaluation
            var variableResult = ConditionalExpressionEvaluator.Evaluate("{{user_role}} == 'admin'", _graphState);
            Console.WriteLine($"Variable evaluation result: {variableResult.Value}");

            // Complex logical evaluation
            var complexResult = ConditionalExpressionEvaluator.Evaluate("{{priority}} == 'high' && {{user_role}} == 'admin'", _graphState);
            Console.WriteLine($"Complex evaluation result: {complexResult.Value}");

            // Clear evaluation cache
            ConditionalExpressionEvaluator.ClearCache();
            Console.WriteLine("Evaluation cache cleared");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ConditionalExpressionEvaluator demo error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates the usage of ChainOfThoughtValidator utility class
    /// </summary>
    public static void DemonstrateChainOfThoughtValidator()
    {
        Console.WriteLine("\n=== ChainOfThoughtValidator Utility Demo ===");

        try
        {
            // Create validator instance
            var validator = new ChainOfThoughtValidator();
            Console.WriteLine("ChainOfThoughtValidator created");

            // Note: The actual validation methods require specific step objects
            // This demo shows the validator can be instantiated
            Console.WriteLine("Validator is ready for Chain-of-Thought step validation");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ChainOfThoughtValidator demo error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates the usage of ModuleActivationExtensions
    /// </summary>
    public static void DemonstrateModuleActivationExtensions()
    {
        Console.WriteLine("\n=== Module Activation Extensions Demo ===");

        try
        {
            // Create kernel with module activation
            var builder = Kernel.CreateBuilder()
                .AddGraphModules(options => 
                {
                    options.EnableStreaming = true;
                    options.EnableCheckpointing = true;
                    options.EnableRecovery = true;
                    options.EnableHumanInTheLoop = true;
                    options.EnableMultiAgent = true;
                });

            Console.WriteLine("Graph modules configured");

            // Create options and apply environment overrides
            var options = new GraphModuleActivationOptions
            {
                EnableStreaming = true,
                EnableCheckpointing = true,
                EnableRecovery = true,
                EnableHumanInTheLoop = true,
                EnableMultiAgent = true
            };

            options.ApplyEnvironmentOverrides();
            Console.WriteLine("Environment overrides applied to module activation options");

            Console.WriteLine($"Streaming enabled: {options.EnableStreaming}");
            Console.WriteLine($"Checkpointing enabled: {options.EnableCheckpointing}");
            Console.WriteLine($"Recovery enabled: {options.EnableRecovery}");
            Console.WriteLine($"HITL enabled: {options.EnableHumanInTheLoop}");
            Console.WriteLine($"Multi-agent enabled: {options.EnableMultiAgent}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Module activation demo error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates basic utility usage patterns from the documentation
    /// </summary>
    public static void DemonstrateBasicUtilityUsage()
    {
        Console.WriteLine("\n=== Basic Utility Usage Demo ===");

        try
        {
            // Use StateHelpers for serialization
            var serialized = StateHelpers.SerializeState(_graphState, indented: true);
            var restored = StateHelpers.DeserializeState(serialized);
            Console.WriteLine("Basic serialization/deserialization completed");

            // Use StateValidator for integrity checks
            var validation = StateValidator.ValidateState(_graphState);
            if (!validation.IsValid)
            {
                foreach (var issue in validation.Errors)
                {
                    Console.WriteLine($"Error: {issue.Message}");
                }
            }
            else
            {
                Console.WriteLine("State validation passed");
            }

            // Use ConditionalExpressionEvaluator
            var result = ConditionalExpressionEvaluator.Evaluate("{{user_role}} == 'admin'", _graphState);
            if (result.Value)
            {
                Console.WriteLine("User is admin");
            }
            else
            {
                Console.WriteLine("User is not admin");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Basic utility usage demo error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates advanced pattern integration from the documentation
    /// </summary>
    public static void DemonstrateAdvancedPatternIntegration()
    {
        Console.WriteLine("\n=== Advanced Pattern Integration Demo ===");

        try
        {
            // Create executor with multiple advanced patterns
            var advancedExecutor = new GraphExecutor("advanced-graph")
                .WithAcademicPatterns(config => 
                {
                    config.EnableCircuitBreaker = true;
                    config.EnableBulkhead = true;
                    config.EnableCacheAside = true;
                })
                .EnableDynamicRouting()
                .EnableMetrics();

            Console.WriteLine("Advanced pattern integration completed");
            Console.WriteLine("- Academic patterns (Circuit Breaker, Bulkhead, Cache-Aside)");
            Console.WriteLine("- Dynamic routing");
            Console.WriteLine("- Performance metrics");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Advanced pattern integration demo error: {ex.Message}");
        }
    }

    /// <summary>
    /// Runs all demonstrations
    /// </summary>
    public static void RunAllDemonstrations()
    {
        Console.WriteLine("Starting Additional Utilities Demonstrations...\n");

        DemonstrateAdvancedPatternsExtensions();
        DemonstrateDynamicRoutingExtensions();
        DemonstrateGraphPerformanceExtensions();
        DemonstrateHumanInTheLoopExtensions();
        DemonstrateLoggingExtensions();
        DemonstrateMultiAgentExtensions();
        DemonstrateStateExtensions();
        DemonstrateStreamingExtensions();
        DemonstrateStateHelpers();
        DemonstrateStateValidator();
        DemonstrateConditionalExpressionEvaluator();
        DemonstrateChainOfThoughtValidator();
        DemonstrateModuleActivationExtensions();
        DemonstrateBasicUtilityUsage();
        DemonstrateAdvancedPatternIntegration();

        Console.WriteLine("\n=== All Additional Utilities Demonstrations Completed ===");
    }
}