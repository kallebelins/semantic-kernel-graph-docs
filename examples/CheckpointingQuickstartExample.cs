using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;
using System.ComponentModel;

namespace Examples;

/// <summary>
/// Comprehensive checkpointing quickstart example demonstrating all major features.
/// This example shows how to configure, use, and manage the checkpointing system.
/// </summary>
public static class CheckpointingQuickstartExample
{
    private static string? openAiApiKey;
    private static string? openAiModel;

    static CheckpointingQuickstartExample()
    {
        // Load configuration from appsettings.json (optional)
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
            .Build();

        openAiApiKey = configuration["OpenAI:ApiKey"];
        openAiModel = configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";
    }

    /// <summary>
    /// Main entry point demonstrating all checkpointing features.
    /// </summary>
    public static async Task RunAllExamplesAsync()
    {
        Console.WriteLine("=== Checkpointing Quickstart Examples ===\n");

        // Run basic checkpointing example
        await RunBasicCheckpointingExampleAsync();

        // Run manual checkpoint management example
        await RunManualCheckpointManagementExampleAsync();

        // Run advanced configuration example
        await RunAdvancedConfigurationExampleAsync();

        // Run recovery example
        await RunRecoveryExampleAsync();

        // Run monitoring example
        await RunMonitoringExampleAsync();

        Console.WriteLine("\n=== All examples completed ===");
    }

    /// <summary>
    /// Demonstrates basic checkpointing setup and execution.
    /// </summary>
    public static async Task RunBasicCheckpointingExampleAsync()
    {
        Console.WriteLine("1. Basic Checkpointing Setup and Execution");
        Console.WriteLine("==========================================");

        if (string.IsNullOrEmpty(openAiApiKey))
        {
            Console.WriteLine("⚠️  OpenAI API Key not found in appsettings.json. Using mock key for demonstration.");
            openAiApiKey = "mock-api-key";
        }

        // Step 1: Enable checkpoint support with memory integration
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(openAiModel!, openAiApiKey)
            .AddGraphMemory()  // Required for checkpointing
            .AddCheckpointSupport(options =>
            {
                options.EnableCompression = true;
                options.MaxCacheSize = 100;
            })
            .Build();

        // Step 2: Create checkpointing graph executor
        var executorFactory = kernel.Services.GetRequiredService<ICheckpointingGraphExecutorFactory>();
        var executor = executorFactory.CreateExecutor("basic-graph", new CheckpointingOptions
        {
            CheckpointInterval = 2,  // Create checkpoint every 2 nodes
            CreateInitialCheckpoint = true,
            CreateFinalCheckpoint = true,
            EnableAutoCleanup = true,
            CriticalNodes = new HashSet<string> { "process", "validate" }
        });

        // Step 3: Build and execute graph with automatic checkpointing
        await BuildBasicGraph(executor);

        var arguments = new KernelArguments();
        arguments["input"] = "Process this data";
        arguments["counter"] = 0;

        try
        {
            var result = await executor.ExecuteAsync(kernel, arguments);
            Console.WriteLine($"✅ Execution completed: {result.GetValue<object>()}");
            Console.WriteLine($"✅ ExecutionId: {executor.LastExecutionId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Execution failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates manual checkpoint creation and restoration.
    /// </summary>
    public static async Task RunManualCheckpointManagementExampleAsync()
    {
        Console.WriteLine("2. Manual Checkpoint Management");
        Console.WriteLine("===============================");

        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(openAiModel!, openAiApiKey!)
            .AddGraphMemory()
            .AddCheckpointSupport()
            .Build();

        var executorFactory = kernel.Services.GetRequiredService<ICheckpointingGraphExecutorFactory>();
        var executor = executorFactory.CreateExecutor("manual-graph", new CheckpointingOptions
        {
            CheckpointInterval = 1,
            CreateInitialCheckpoint = true
        });

        await BuildBasicGraph(executor);

        var arguments = new KernelArguments();
        arguments["input"] = "Manual checkpoint test";
        arguments["counter"] = 0;

        // Get the current graph state
        var graphState = arguments.GetOrCreateGraphState();

        // Create a manual checkpoint with custom name
        var checkpointId = StateHelpers.CreateCheckpoint(graphState, "manual-checkpoint");
        Console.WriteLine($"✅ Created manual checkpoint: {checkpointId}");

        // The checkpoint is now stored in the state metadata
        var checkpoint = graphState.GetMetadata<object>($"checkpoint_{checkpointId}");
        if (checkpoint != null)
        {
            Console.WriteLine($"✅ Checkpoint created successfully");
        }

        // Execute the graph
        try
        {
            var result = await executor.ExecuteAsync(kernel, arguments);
            Console.WriteLine($"✅ Execution completed: {result.GetValue<object>()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Execution failed: {ex.Message}");
        }

        // Demonstrate restoration from checkpoint
        try
        {
            // Restore state from the manual checkpoint
            var restoredState = StateHelpers.RestoreCheckpoint(graphState, checkpointId);

            // Update arguments with the restored state
            // Note: UpdateFromGraphState method doesn't exist, so we'll just log the restoration
            Console.WriteLine("✅ State restored successfully from manual checkpoint");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"❌ Failed to restore checkpoint: {ex.Message}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates advanced checkpoint configuration options.
    /// </summary>
    public static async Task RunAdvancedConfigurationExampleAsync()
    {
        Console.WriteLine("3. Advanced Checkpoint Configuration");
        Console.WriteLine("===================================");

        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(openAiModel!, openAiApiKey!)
            .AddGraphMemory()
            .AddCheckpointSupport()
            .Build();

        // Configure detailed checkpoint behavior
        var checkpointingOptions = new CheckpointingOptions
        {
            CheckpointInterval = 3,  // Every 3 nodes
            CheckpointTimeInterval = TimeSpan.FromMinutes(5),  // Or every 5 minutes
            CreateInitialCheckpoint = true,
            CreateFinalCheckpoint = true,
            CreateErrorCheckpoints = true,  // Save state on errors
            EnableAutoCleanup = true,
            FailOnCheckpointError = false,  // Continue execution even if checkpointing fails

            // Define critical nodes that always trigger checkpoints
            CriticalNodes = new HashSet<string> { "process", "validate", "output" },

            // Configure retention policy
            RetentionPolicy = new CheckpointRetentionPolicy
            {
                MaxAge = TimeSpan.FromHours(24),
                MaxCheckpointsPerExecution = 50,
                MaxTotalStorageBytes = 100 * 1024 * 1024  // 100MB
            }
        };

        var executorFactory = kernel.Services.GetRequiredService<ICheckpointingGraphExecutorFactory>();
        var executor = executorFactory.CreateExecutor("advanced-graph", checkpointingOptions);

        await BuildBasicGraph(executor);

        var arguments = new KernelArguments();
        arguments["input"] = "Advanced configuration test";
        arguments["counter"] = 0;

        try
        {
            var result = await executor.ExecuteAsync(kernel, arguments);
            Console.WriteLine($"✅ Advanced execution completed: {result.GetValue<object>()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Advanced execution failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates fault tolerance with automatic checkpoint recovery.
    /// </summary>
    public static async Task RunRecoveryExampleAsync()
    {
        Console.WriteLine("4. Fault Tolerance and Recovery");
        Console.WriteLine("================================");

        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(openAiModel!, openAiApiKey!)
            .AddGraphMemory()
            .AddCheckpointSupport()
            .Build();

        var executorFactory = kernel.Services.GetRequiredService<ICheckpointingGraphExecutorFactory>();
        var executor = executorFactory.CreateExecutor("recovery-graph", new CheckpointingOptions
        {
            CheckpointInterval = 1,
            CreateErrorCheckpoints = true,
            FailOnCheckpointError = false
        });

        await BuildFailingGraph(executor);

        var arguments = new KernelArguments();
        arguments["input"] = "Recovery test data";
        arguments["fail_at_step"] = 3; // Simulate failure at step 3

        string? lastCheckpointId = null;

        try
        {
            await executor.ExecuteAsync(kernel, arguments);
            Console.WriteLine("✅ Execution completed without failures");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Execution failed as expected: {ex.Message}");

            // Find the latest checkpoint for recovery
            var executionId = executor.LastExecutionId ?? arguments.GetOrCreateGraphState().StateId;
            var checkpoints = await executor.GetExecutionCheckpointsAsync(executionId);

            if (checkpoints.Count > 0)
            {
                lastCheckpointId = checkpoints.First().CheckpointId;
                Console.WriteLine($"✅ Found checkpoint for recovery: {lastCheckpointId}");
            }
        }

        // Recover from checkpoint if available
        if (!string.IsNullOrEmpty(lastCheckpointId))
        {
            try
            {
                Console.WriteLine("🔄 Attempting recovery from checkpoint...");

                // Modify parameters to avoid the failure
                arguments["fail_at_step"] = -1; // Disable failure

                // Resume from checkpoint
                var recoveredResult = await executor.ResumeFromCheckpointAsync(lastCheckpointId, kernel);

                Console.WriteLine($"✅ Recovery successful: {recoveredResult.GetValue<object>()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Recovery failed: {ex.Message}");
            }
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates checkpoint monitoring and management.
    /// </summary>
    public static async Task RunMonitoringExampleAsync()
    {
        Console.WriteLine("5. Checkpoint Monitoring and Management");
        Console.WriteLine("======================================");

        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(openAiModel!, openAiApiKey!)
            .AddGraphMemory()
            .AddCheckpointSupport()
            .Build();

        var executorFactory = kernel.Services.GetRequiredService<ICheckpointingGraphExecutorFactory>();
        var executor = executorFactory.CreateExecutor("monitoring-graph", new CheckpointingOptions
        {
            CheckpointInterval = 2,
            CreateInitialCheckpoint = true,
            CreateFinalCheckpoint = true
        });

        await BuildBasicGraph(executor);

        // Run multiple executions to demonstrate monitoring
        var executionIds = new List<string>();

        for (int i = 0; i < 2; i++)
        {
            var arguments = new KernelArguments();
            arguments["input"] = $"Execution {i + 1} data";
            arguments["execution_number"] = i + 1;
            arguments["counter"] = 0;

            try
            {
                await executor.ExecuteAsync(kernel, arguments);
                executionIds.Add(executor.LastExecutionId ?? arguments.GetOrCreateGraphState().StateId);
                Console.WriteLine($"✅ Completed execution {i + 1}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Execution {i + 1} failed: {ex.Message}");
            }
        }

        // Display monitoring information
        foreach (var executionId in executionIds)
        {
            var checkpoints = await executor.GetExecutionCheckpointsAsync(executionId);

            Console.WriteLine($"\n📊 Execution {executionId}:");
            Console.WriteLine($"  Checkpoints: {checkpoints.Count}");

            foreach (var checkpoint in checkpoints.OrderBy(c => c.SequenceNumber))
            {
                Console.WriteLine($"    {checkpoint.CheckpointId}: " +
                               $"Node {checkpoint.NodeId}, " +
                               $"Size: {checkpoint.SizeInBytes / 1024:F1} KB, " +
                               $"Created: {checkpoint.CreatedAt:HH:mm:ss}");
            }
        }

        // Manual cleanup demonstration
        Console.WriteLine("\n🧹 Performing manual cleanup...");
        var checkpointManager = kernel.Services.GetRequiredService<ICheckpointManager>();

        var cleanupCount = await checkpointManager.CleanupCheckpointsAsync(
            retentionPolicy: new CheckpointRetentionPolicy
            {
                MaxAge = TimeSpan.FromMinutes(5),
                MaxCheckpointsPerExecution = 10
            });

        Console.WriteLine($"✅ Cleaned up {cleanupCount} old checkpoints");
        Console.WriteLine();
    }

    #region Helper Methods

    /// <summary>
    /// Builds a basic graph for demonstration purposes.
    /// </summary>
    private static async Task BuildBasicGraph(CheckpointingGraphExecutor executor)
    {
        // Node 1: Data input
        var inputNode = new FunctionGraphNode(CreateDataInputFunction(), "input", "DataInput")
            .StoreResultAs("data");
        executor.AddNode(inputNode);
        executor.SetStartNode("input");

        // Node 2: Data processing
        var processNode = new FunctionGraphNode(CreateDataProcessFunction(), "process", "DataProcess");
        executor.AddNode(processNode);
        executor.Connect("input", "process");

        // Node 3: Data transformation
        var transformNode = new FunctionGraphNode(CreateDataTransformFunction(), "transform", "DataTransform")
            .StoreResultAs("data");
        executor.AddNode(transformNode);
        executor.Connect("process", "transform");

        // Node 4: Data validation
        var validateNode = new FunctionGraphNode(CreateDataValidateFunction(), "validate", "DataValidate");
        executor.AddNode(validateNode);
        executor.Connect("transform", "validate");

        // Node 5: Data output
        var outputNode = new FunctionGraphNode(CreateDataOutputFunction(), "output", "DataOutput");
        executor.AddNode(outputNode);
        executor.Connect("validate", "output");

        await Task.CompletedTask;
    }

    /// <summary>
    /// Builds a graph with potential failures for recovery testing.
    /// </summary>
    private static async Task BuildFailingGraph(CheckpointingGraphExecutor executor)
    {
        // Add nodes that can fail for recovery testing
        var nodes = new[]
        {
            ("step1", "Step1", CreateStepFunction(1)),
            ("step2", "Step2", CreateStepFunction(2)),
            ("step3", "Step3", CreateFailingStepFunction(3)),
            ("step4", "Step4", CreateStepFunction(4)),
            ("step5", "Step5", CreateStepFunction(5))
        };

        IGraphNode? previousNode = null;
        foreach (var (id, name, func) in nodes)
        {
            var node = new FunctionGraphNode(func, id, name);
            executor.AddNode(node);

            if (previousNode == null)
            {
                executor.SetStartNode(id);
            }
            else
            {
                executor.Connect(previousNode.NodeId, id);
            }

            previousNode = node;
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Creates a data input function.
    /// </summary>
    private static KernelFunction CreateDataInputFunction()
    {
        return KernelFunctionFactory.CreateFromMethod(
            ([Description("Input data")] string input, [Description("Counter")] int counter) =>
            {
                return $"Input processed: {input} (counter: {counter + 1})";
            },
            "DataInput",
            "Processes input data");
    }

    /// <summary>
    /// Creates a data processing function.
    /// </summary>
    private static KernelFunction CreateDataProcessFunction()
    {
        return KernelFunctionFactory.CreateFromMethod(
            ([Description("Data to process")] string data) =>
            {
                return $"Processed: {data.ToUpperInvariant()}";
            },
            "DataProcess",
            "Processes data");
    }

    /// <summary>
    /// Creates a data transformation function.
    /// </summary>
    private static KernelFunction CreateDataTransformFunction()
    {
        return KernelFunctionFactory.CreateFromMethod(
            ([Description("Data to transform")] string data) =>
            {
                return $"Transformed: [{data}]";
            },
            "DataTransform",
            "Transforms data");
    }

    /// <summary>
    /// Creates a data validation function.
    /// </summary>
    private static KernelFunction CreateDataValidateFunction()
    {
        return KernelFunctionFactory.CreateFromMethod(
            ([Description("Data to validate")] string data) =>
            {
                return $"Validated: {data} ✓";
            },
            "DataValidate",
            "Validates data");
    }

    /// <summary>
    /// Creates a data output function.
    /// </summary>
    private static KernelFunction CreateDataOutputFunction()
    {
        return KernelFunctionFactory.CreateFromMethod(
            ([Description("Data to output")] string data) =>
            {
                return $"Final output: {data}";
            },
            "DataOutput",
            "Outputs final data");
    }

    /// <summary>
    /// Creates a step function for recovery testing.
    /// </summary>
    private static KernelFunction CreateStepFunction(int stepNumber)
    {
        return KernelFunctionFactory.CreateFromMethod(
            ([Description("Input data")] string input, [Description("Step counter")] int counter = 0) =>
            {
                return $"Step {stepNumber} completed: {input} (total steps: {counter + 1})";
            },
            $"Step{stepNumber}",
            $"Executes step {stepNumber}");
    }

    /// <summary>
    /// Creates a failing step function for recovery testing.
    /// </summary>
    private static KernelFunction CreateFailingStepFunction(int stepNumber)
    {
        return KernelFunctionFactory.CreateFromMethod(
            ([Description("Input data")] string input, [Description("Fail at step")] int fail_at_step = -1) =>
            {
                if (fail_at_step == stepNumber)
                {
                    throw new InvalidOperationException($"Simulated failure at step {stepNumber}");
                }
                return $"Step {stepNumber} completed: {input}";
            },
            $"Step{stepNumber}",
            $"Executes step {stepNumber} with potential failure");
    }

    #endregion
}
