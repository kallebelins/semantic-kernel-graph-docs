using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;
using SemanticKernel.Graph.Execution;
using SemanticKernel.Graph.Integration.Policies;
using SemanticKernel.Graph.Integration;

namespace Examples;

/// <summary>
/// Comprehensive example demonstrating the Core API types and functionality.
/// This example showcases the key types mentioned in the Core API documentation:
/// - GraphExecutor
/// - GraphExecutionContext
/// - IGraphNode
/// - ConditionalEdge
/// - GraphState
/// - ErrorPolicyRegistry
/// - RetryPolicyGraphNode
/// - ErrorHandlerGraphNode
/// - ErrorMetricsCollector
/// - MultiAgentCoordinator
/// - ResultAggregator
/// - AgentConnectionPool
/// - WorkDistributor
/// - WorkflowValidator
/// - GraphTypeInferenceEngine
/// - StateValidator
/// - StateMergeConflictPolicy
/// </summary>
public class CoreApiExample
{
    /// <summary>
    /// Runs the comprehensive Core API example demonstrating all core types and functionality.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Core API Example ===\n");

        try
        {
            // Step 1: Create and configure kernel with graph support
            var kernel = CreateKernelWithGraphSupport();

            // Step 2: Demonstrate core types and their usage
            await DemonstrateCoreTypesAsync(kernel);

            // Step 3: Show advanced features and patterns
            await DemonstrateAdvancedFeaturesAsync(kernel);

            Console.WriteLine("\n✅ Core API example completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in Core API example: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Creates and configures a kernel with graph support enabled.
    /// </summary>
    /// <returns>A configured kernel instance with graph support</returns>
    private static Kernel CreateKernelWithGraphSupport()
    {
        Console.WriteLine("Step 1: Creating kernel with graph support...");

        var builder = Kernel.CreateBuilder();

        // Add OpenAI service if available
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrEmpty(apiKey))
        {
            builder.AddOpenAIChatCompletion("gpt-4", apiKey);
            Console.WriteLine("✅ OpenAI service configured");
        }
        else
        {
            Console.WriteLine("⚠️  OPENAI_API_KEY not found. Using mock functions for demonstration.");
        }

        // Enable graph functionality
        builder.AddGraphSupport();

        var kernel = builder.Build();
        Console.WriteLine("✅ Kernel created successfully with graph support enabled");
        return kernel;
    }

    /// <summary>
    /// Demonstrates the core types and their basic usage patterns.
    /// </summary>
    /// <param name="kernel">The configured kernel instance</param>
    private static async Task DemonstrateCoreTypesAsync(Kernel kernel)
    {
        Console.WriteLine("\nStep 2: Demonstrating Core Types...");

        // Demonstrate GraphExecutor
        await DemonstrateGraphExecutorAsync(kernel);

        // Demonstrate GraphState and state management
        await DemonstrateGraphStateAsync(kernel);

        // Demonstrate ConditionalEdge
        await DemonstrateConditionalEdgeAsync(kernel);

        // Demonstrate Error handling types
        await DemonstrateErrorHandlingTypesAsync(kernel);
    }

    /// <summary>
    /// Demonstrates GraphExecutor creation and basic configuration.
    /// </summary>
    /// <param name="kernel">The configured kernel instance</param>
    private static async Task DemonstrateGraphExecutorAsync(Kernel kernel)
    {
        Console.WriteLine("\n--- GraphExecutor Demonstration ---");

        // Create a new graph executor
        var executor = new GraphExecutor("CoreApiDemo", "Demonstration of Core API functionality");

        // Configure metrics for performance monitoring
        executor.ConfigureMetrics(new GraphMetricsOptions
        {
            EnableRealTimeMetrics = true,
            MetricsRetentionPeriod = TimeSpan.FromHours(1),
            EnablePercentileCalculations = true
        });

        // Configure concurrency options
        executor.ConfigureConcurrency(new GraphConcurrencyOptions
        {
            MaxDegreeOfParallelism = 4,
            EnableParallelExecution = true
        });

        Console.WriteLine($"✅ GraphExecutor created: {executor.Name}");
        Console.WriteLine($"   Graph ID: {executor.GraphId}");
        Console.WriteLine($"   Description: {executor.Description}");
        Console.WriteLine($"   Node Count: {executor.NodeCount}");
        Console.WriteLine($"   Edge Count: {executor.EdgeCount}");
        Console.WriteLine($"   Ready for execution: {executor.IsReadyForExecution}");
    }

    /// <summary>
    /// Demonstrates GraphState creation and state management.
    /// </summary>
    /// <param name="kernel">The configured kernel instance</param>
    private static async Task DemonstrateGraphStateAsync(Kernel kernel)
    {
        Console.WriteLine("\n--- GraphState Demonstration ---");

        // Create a new graph state
        var graphState = new GraphState();

        // Add some sample data to the state
        graphState.KernelArguments["userName"] = "John Doe";
        graphState.KernelArguments["currentStep"] = 1;
        graphState.KernelArguments["timestamp"] = DateTimeOffset.UtcNow;
        graphState.KernelArguments["isActive"] = true;

        // Create kernel arguments with the graph state
        var arguments = new KernelArguments();
        arguments.SetGraphState(graphState);

        Console.WriteLine($"✅ GraphState created");
        Console.WriteLine($"   State ID: {graphState.StateId}");
        Console.WriteLine($"   Values count: {graphState.KernelArguments.Count}");
        Console.WriteLine($"   User name: {graphState.KernelArguments["userName"]}");
        Console.WriteLine($"   Current step: {graphState.KernelArguments["currentStep"]}");
        Console.WriteLine($"   Is active: {graphState.KernelArguments["isActive"]}");
    }

    /// <summary>
    /// Demonstrates ConditionalEdge creation and usage.
    /// </summary>
    /// <param name="kernel">The configured kernel instance</param>
    private static async Task DemonstrateConditionalEdgeAsync(Kernel kernel)
    {
        Console.WriteLine("\n--- ConditionalEdge Demonstration ---");

        // Create sample nodes for demonstration
        var sourceNode = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod(
                (string input) => $"Processed: {input}",
                functionName: "ProcessInput",
                description: "Processes input and returns processed result"
            ),
            "sourceNode",
            "Source Node"
        );

        var targetNode = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod(
                (string input) => $"Final: {input}",
                functionName: "Finalize",
                description: "Finalizes the processing"
            ),
            "targetNode",
            "Target Node"
        );

        // Create a conditional edge with a simple condition
        var conditionalEdge = new ConditionalEdge(
            sourceNode,
            targetNode,
            (args) => args.GetValue<string>("input")?.Length > 5,
            "ProcessedToFinal"
        );

        Console.WriteLine($"✅ ConditionalEdge created: {conditionalEdge.EdgeId}");
        Console.WriteLine($"   Source: {conditionalEdge.SourceNode.NodeId}");
        Console.WriteLine($"   Target: {conditionalEdge.TargetNode.NodeId}");
        Console.WriteLine($"   Name: {conditionalEdge.Name}");
        Console.WriteLine($"   Condition: Input length > 5");
    }

    /// <summary>
    /// Demonstrates error handling types and their usage.
    /// </summary>
    /// <param name="kernel">The configured kernel instance</param>
    private static async Task DemonstrateErrorHandlingTypesAsync(Kernel kernel)
    {
        Console.WriteLine("\n--- Error Handling Types Demonstration ---");

        // Create a simple function node to wrap with retry policy
        var functionNode = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod(
                (string input) => $"Processed: {input}",
                functionName: "ProcessFunction",
                description: "Function to be wrapped with retry policy"
            ),
            "processFunction",
            "Process Function"
        );

        // Create a retry policy configuration
        var retryConfig = new RetryPolicyConfig
        {
            MaxRetries = 3,
            BaseDelay = TimeSpan.FromSeconds(1),
            MaxDelay = TimeSpan.FromSeconds(30),
            Strategy = RetryStrategy.ExponentialBackoff,
            BackoffMultiplier = 2.0,
            UseJitter = true
        };

        // Create a retry policy node
        var retryNode = new RetryPolicyGraphNode(functionNode, retryConfig);

        Console.WriteLine($"✅ RetryPolicyGraphNode created");
        Console.WriteLine($"   Wrapped node: {retryNode.WrappedNode.NodeId}");
        Console.WriteLine($"   Max retries: {retryNode.RetryConfig.MaxRetries}");
        Console.WriteLine($"   Strategy: {retryNode.RetryConfig.Strategy}");
        Console.WriteLine($"   Base delay: {retryNode.RetryConfig.BaseDelay}");
    }

    /// <summary>
    /// Demonstrates advanced features and patterns using the Core API.
    /// </summary>
    /// <param name="kernel">The configured kernel instance</param>
    private static async Task DemonstrateAdvancedFeaturesAsync(Kernel kernel)
    {
        Console.WriteLine("\nStep 3: Demonstrating Advanced Features...");

        // Demonstrate multi-agent coordination
        await DemonstrateMultiAgentCoordinationAsync(kernel);

        // Demonstrate workflow validation
        await DemonstrateWorkflowValidationAsync(kernel);

        // Demonstrate state validation and merge conflict resolution
        await DemonstrateStateValidationAsync(kernel);
    }

    /// <summary>
    /// Demonstrates multi-agent coordination using the Core API.
    /// </summary>
    /// <param name="kernel">The configured kernel instance</param>
    private static async Task DemonstrateMultiAgentCoordinationAsync(Kernel kernel)
    {
        Console.WriteLine("\n--- Multi-Agent Coordination Demonstration ---");

        // Create multi-agent options
        var options = new MultiAgentOptions
        {
            MaxConcurrentAgents = 5,
            CoordinationTimeout = TimeSpan.FromMinutes(10),
            SharedStateOptions = new SharedStateOptions
            {
                ConflictResolutionStrategy = ConflictResolutionStrategy.Merge,
                AllowOverwrite = true
            },
            WorkDistributionOptions = new WorkDistributionOptions
            {
                DistributionStrategy = WorkDistributionStrategy.RoleBased,
                EnablePrioritization = true
            },
            ResultAggregationOptions = new ResultAggregationOptions
            {
                DefaultAggregationStrategy = AggregationStrategy.Consensus,
                ConsensusThreshold = 0.6
            }
        };

        // Create a multi-agent coordinator
        var coordinator = new MultiAgentCoordinator(options);

        Console.WriteLine($"✅ MultiAgentCoordinator created");
        Console.WriteLine($"   Max concurrent agents: {coordinator.Options.MaxConcurrentAgents}");
        Console.WriteLine($"   Coordination timeout: {coordinator.Options.CoordinationTimeout}");
        Console.WriteLine($"   Conflict resolution: {coordinator.Options.SharedStateOptions.ConflictResolutionStrategy}");
        Console.WriteLine($"   Work distribution: {coordinator.Options.WorkDistributionOptions.DistributionStrategy}");
        Console.WriteLine($"   Result aggregation: {coordinator.Options.ResultAggregationOptions.DefaultAggregationStrategy}");
    }

    /// <summary>
    /// Demonstrates workflow validation using the Core API.
    /// </summary>
    /// <param name="kernel">The configured kernel instance</param>
    private static async Task DemonstrateWorkflowValidationAsync(Kernel kernel)
    {
        Console.WriteLine("\n--- Workflow Validation Demonstration ---");

        // Create a sample workflow for validation
        var workflow = new GraphExecutor("DemoWorkflow", "Sample workflow for validation demonstration");

        // Add some nodes to the workflow
        var startNode = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod(
                (string input) => $"Started: {input}",
                functionName: "StartProcess",
                description: "Starts the workflow"
            ),
            "startNode",
            "Workflow start point"
        );

        var endNode = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod(
                (string input) => $"Completed: {input}",
                functionName: "EndProcess",
                description: "Ends the workflow"
            ),
            "endNode",
            "Workflow end point"
        );

        workflow.AddNode(startNode);
        workflow.AddNode(endNode);
        workflow.SetStartNode("startNode");

        Console.WriteLine($"✅ Workflow created: {workflow.Name}");
        Console.WriteLine($"   Description: {workflow.Description}");
        Console.WriteLine($"   Node count: {workflow.NodeCount}");
        Console.WriteLine($"   Edge count: {workflow.EdgeCount}");
        Console.WriteLine($"   Ready for execution: {workflow.IsReadyForExecution}");
    }

    /// <summary>
    /// Demonstrates state validation and merge conflict resolution.
    /// </summary>
    /// <param name="kernel">The configured kernel instance</param>
    private static async Task DemonstrateStateValidationAsync(Kernel kernel)
    {
        Console.WriteLine("\n--- State Validation and Merge Conflict Resolution ---");

        // Create sample states for validation
        var state1 = new GraphState();
        state1.KernelArguments["key1"] = "value1";
        state1.KernelArguments["key2"] = "value2";

        var state2 = new GraphState();
        state2.KernelArguments["key2"] = "value2_updated";
        state2.KernelArguments["key3"] = "value3";

        // Demonstrate state merge conflict policy
        var mergePolicy = StateMergeConflictPolicy.PreferSecond;

        Console.WriteLine($"✅ States created for validation");
        Console.WriteLine($"   State 1 values: {state1.KernelArguments.Count}");
        Console.WriteLine($"   State 2 values: {state2.KernelArguments.Count}");
        Console.WriteLine($"   Merge conflict policy: {mergePolicy}");
        Console.WriteLine($"   State 1 ID: {state1.StateId}");
        Console.WriteLine($"   State 2 ID: {state2.StateId}");
    }
}
