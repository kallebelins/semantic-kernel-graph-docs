using Microsoft.SemanticKernel;
using SemanticKernel.Graph;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;
using SemanticKernel.Graph.Extensions;
using System.Text.Json;

namespace Examples;

/// <summary>
/// Comprehensive example demonstrating all ConditionalEdge functionality as documented in conditional-edge.md
/// This example covers all constructors, factory methods, merge configuration, and usage patterns.
/// </summary>
public static class ConditionalEdgeExample
{
    /// <summary>
    /// Extension method to safely get values from KernelArguments
    /// </summary>
    private static T GetValue<T>(this KernelArguments args, string key, T defaultValue = default!)
    {
        if (args.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    /// <summary>
    /// Runs all ConditionalEdge examples to demonstrate the complete API
    /// </summary>
    public static async Task RunAllExamplesAsync()
    {
        Console.WriteLine("🔗 ConditionalEdge Comprehensive Examples");
        Console.WriteLine("=".PadLeft(50, '='));

        // Run all examples
        await BasicConstructorsExample();
        await FactoryMethodsExample();
        await MergeConfigurationExample();
        await AdvancedUsagePatternsExample();
        await ValidationAndIntegrityExample();
        await IntegrationWithGraphExecutorExample();

        Console.WriteLine("\n✅ All ConditionalEdge examples completed successfully!");
    }

    /// <summary>
    /// Demonstrates basic ConditionalEdge constructors
    /// </summary>
    private static async Task BasicConstructorsExample()
    {
        Console.WriteLine("\n📚 Basic Constructors Example");
        Console.WriteLine("-".PadLeft(30, '-'));

        // Create simple nodes for testing
        var startNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args.Add("status", "approved");
                    args.Add("priority", 8);
                    args.Add("isUrgent", true);
                    args.Add("userRole", "admin");
                    return "Start node executed";
                },
                "StartNode",
                "Adds initial data to arguments"
            ),
            "start"
        );

        var successNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args.Add("result", "Success path taken");
                    return "Success path executed";
                },
                "SuccessNode",
                "Handles success path"
            ),
            "success"
        );

        var failureNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args.Add("result", "Failure path taken");
                    return "Failure path executed";
                },
                "FailureNode",
                "Handles failure path"
            ),
            "failure"
        );

        var highPriorityNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args.Add("result", "High priority route taken");
                    return "High priority route executed";
                },
                "HighPriorityNode",
                "Handles high priority route"
            ),
            "highPriority"
        );

        // Example 1: Basic Conditional Edge with KernelArguments condition
        var successEdge = new ConditionalEdge(
            sourceNode: startNode,
            targetNode: successNode,
            condition: args => args.ContainsKey("status") && (string)args["status"] == "approved",
            name: "Success Path"
        );

        Console.WriteLine($"✅ Created edge: {successEdge.Name}");
        Console.WriteLine($"   Edge ID: {successEdge.EdgeId}");
        Console.WriteLine($"   Source: {successEdge.SourceNode.Name} -> Target: {successEdge.TargetNode.Name}");
        Console.WriteLine($"   Created at: {successEdge.CreatedAt}");

        // Example 2: State-Based Conditional Edge
        var highPriorityEdge = new ConditionalEdge(
            sourceNode: startNode,
            targetNode: highPriorityNode,
            stateCondition: state =>
            {
                var priority = state.GetValue<int>("priority");
                var isUrgent = state.GetValue<bool>("isUrgent");
                var hasPermission = state.GetValue<string>("userRole") == "admin";
                
                return priority > 7 || (isUrgent && hasPermission);
            },
            name: "High Priority or Urgent Admin Route"
        );

        Console.WriteLine($"✅ Created state-based edge: {highPriorityEdge.Name}");
        Console.WriteLine($"   Has StateCondition: {highPriorityEdge.StateCondition != null}");

        // Test the edges
        var testArgs = new KernelArguments();
        testArgs.Add("status", "approved");
        testArgs.Add("priority", 8);
        testArgs.Add("isUrgent", true);
        testArgs.Add("userRole", "admin");

        var testState = new GraphState(testArgs);

        Console.WriteLine($"\n🧪 Testing edge conditions:");
        Console.WriteLine($"   Success edge condition: {successEdge.EvaluateCondition(testArgs)}");
        Console.WriteLine($"   High priority edge condition: {highPriorityEdge.EvaluateCondition(testState)}");
    }

    /// <summary>
    /// Demonstrates ConditionalEdge factory methods
    /// </summary>
    private static async Task FactoryMethodsExample()
    {
        Console.WriteLine("\n🏭 Factory Methods Example");
        Console.WriteLine("-".PadLeft(30, '-'));

        // Create nodes for testing
        var sourceNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) => "Source executed", "SourceNode", "Source node"),
            "source"
        );
        var nextNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) => "Next executed", "NextNode", "Next node"),
            "next"
        );
        var advancedNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) => "Advanced executed", "AdvancedNode", "Advanced node"),
            "advanced"
        );
        var dashboardNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) => "Dashboard executed", "DashboardNode", "Dashboard node"),
            "dashboard"
        );
        var premiumNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) => "Premium executed", "PremiumNode", "Premium node"),
            "premium"
        );
        var fallbackNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) => "Fallback executed", "FallbackNode", "Fallback node"),
            "fallback"
        );
        var endNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) => "End executed", "EndNode", "End node"),
            "end"
        );

        // Example 1: CreateUnconditional
        var alwaysEdge = ConditionalEdge.CreateUnconditional(
            sourceNode: sourceNode,
            targetNode: nextNode,
            name: "Default Path"
        );

        Console.WriteLine($"✅ Created unconditional edge: {alwaysEdge.Name}");
        Console.WriteLine($"   Always traversable: {alwaysEdge.EvaluateCondition(new KernelArguments())}");

        // Example 2: CreateParameterEquals
        var modeEdge = ConditionalEdge.CreateParameterEquals(
            sourceNode: sourceNode,
            targetNode: advancedNode,
            parameterName: "mode",
            expectedValue: "advanced",
            name: "Advanced Mode Route"
        );

        Console.WriteLine($"✅ Created parameter equals edge: {modeEdge.Name}");

        // Example 3: CreateParameterExists
        var authEdge = ConditionalEdge.CreateParameterExists(
            sourceNode: sourceNode,
            targetNode: dashboardNode,
            parameterName: "authToken",
            name: "Authenticated Route"
        );

        Console.WriteLine($"✅ Created parameter exists edge: {authEdge.Name}");

        // Test the factory-created edges
        var testArgs = new KernelArguments();
        testArgs.Add("mode", "advanced");
        testArgs.Add("authToken", "valid-token-123");

        Console.WriteLine($"\n🧪 Testing factory-created edges:");
        Console.WriteLine($"   Mode edge (mode=advanced): {modeEdge.EvaluateCondition(testArgs)}");
        Console.WriteLine($"   Auth edge (authToken exists): {authEdge.EvaluateCondition(testArgs)}");

        // Test with missing parameters
        var missingArgs = new KernelArguments();
        missingArgs.Add("otherParam", "value");

        Console.WriteLine($"   Mode edge (no mode): {modeEdge.EvaluateCondition(missingArgs)}");
        Console.WriteLine($"   Auth edge (no authToken): {authEdge.EvaluateCondition(missingArgs)}");
    }

    /// <summary>
    /// Demonstrates merge configuration capabilities
    /// </summary>
    private static async Task MergeConfigurationExample()
    {
        Console.WriteLine("\n🔀 Merge Configuration Example");
        Console.WriteLine("-".PadLeft(30, '-'));

        // Create nodes for testing
        var sourceNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) => "Source executed", "SourceNode", "Source node"),
            "source"
        );
        var targetNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) => "Target executed", "TargetNode", "Target node"),
            "target"
        );

        // Example 1: Basic merge policy
        var mergeEdge = ConditionalEdge.CreateUnconditional(sourceNode, targetNode)
            .WithMergePolicy(StateMergeConflictPolicy.Reduce);

        Console.WriteLine($"✅ Created edge with merge policy: {mergeEdge.Name}");
        Console.WriteLine($"   Merge configuration: {mergeEdge.MergeConfiguration?.DefaultPolicy}");

        // Example 2: Detailed merge configuration
        var detailedConfig = new StateMergeConfiguration 
        { 
            DefaultPolicy = StateMergeConflictPolicy.Reduce 
        };
        
        var detailedEdge = ConditionalEdge.CreateUnconditional(sourceNode, targetNode)
            .WithMergeConfiguration(detailedConfig);

        Console.WriteLine($"✅ Created edge with detailed merge configuration");

        // Example 3: Key-specific merge policy
        var keyPolicyEdge = ConditionalEdge.CreateUnconditional(sourceNode, targetNode)
            .WithKeyMergePolicy("userData", StateMergeConflictPolicy.PreferSecond);

        Console.WriteLine($"✅ Created edge with key-specific merge policy");

        // Example 4: Type-specific merge policy
        var typePolicyEdge = ConditionalEdge.CreateUnconditional(sourceNode, targetNode)
            .WithTypeMergePolicy(typeof(List<string>), StateMergeConflictPolicy.Reduce);

        Console.WriteLine($"✅ Created edge with type-specific merge policy");

        // Example 5: Custom key merger
        var customMergerEdge = ConditionalEdge.CreateUnconditional(sourceNode, targetNode)
            .WithCustomKeyMerger("counters", (baseVal, overlayVal) => 
            {
                var baseCount = baseVal as int? ?? 0;
                var overlayCount = overlayVal as int? ?? 0;
                return baseCount + overlayCount;
            });

        Console.WriteLine($"✅ Created edge with custom key merger");

        // Example 6: Reduce semantics
        var reduceEdge = ConditionalEdge.CreateUnconditional(sourceNode, targetNode)
            .WithReduceSemantics();

        Console.WriteLine($"✅ Created edge with reduce semantics");

        // Example 7: Chaining multiple merge configurations
        var chainedEdge = ConditionalEdge.CreateUnconditional(sourceNode, targetNode)
            .WithMergePolicy(StateMergeConflictPolicy.Reduce)
            .WithKeyMergePolicy("counters", StateMergeConflictPolicy.Reduce)
            .WithTypeMergePolicy(typeof(List<string>), StateMergeConflictPolicy.Reduce)
            .WithCustomKeyMerger("userData", (baseVal, overlayVal) => 
            {
                // Custom merge logic for user data
                if (baseVal is Dictionary<string, object> baseDict && 
                    overlayVal is Dictionary<string, object> overlayDict)
                {
                    var merged = new Dictionary<string, object>(baseDict);
                    foreach (var kvp in overlayDict)
                    {
                        merged[kvp.Key] = kvp.Value;
                    }
                    return merged;
                }
                return overlayVal;
            });

        Console.WriteLine($"✅ Created edge with chained merge configurations");

        // Display merge configuration details
        Console.WriteLine($"\n📋 Merge Configuration Details:");
        Console.WriteLine($"   Chained edge merge config: {chainedEdge.MergeConfiguration != null}");
        if (chainedEdge.MergeConfiguration != null)
        {
            Console.WriteLine($"   Default policy: {chainedEdge.MergeConfiguration.DefaultPolicy}");
        }
    }

    /// <summary>
    /// Demonstrates advanced usage patterns
    /// </summary>
    private static async Task AdvancedUsagePatternsExample()
    {
        Console.WriteLine("\n🚀 Advanced Usage Patterns Example");
        Console.WriteLine("-".PadLeft(30, '-'));

        // Create nodes for complex workflow
        var startNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args.Add("input", "test data");
                    args.Add("priority", 5);
                    return "Start executed";
                },
                "StartNode",
                "Adds initial data"
            ),
            "start"
        );

        var decisionNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args.Add("decision", "proceed");
                    args.Add("confidence", 0.85);
                    return "Decision executed";
                },
                "DecisionNode",
                "Makes decision"
            ),
            "decision"
        );

        var actionNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args.Add("action", "executed");
                    return "Action executed";
                },
                "ActionNode",
                "Executes action"
            ),
            "action"
        );

        var endNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args.Add("result", "completed");
                    return "End executed";
                },
                "EndNode",
                "Completes workflow"
            ),
            "end"
        );

        // Example 1: Complex state-based conditions
        var decisionEdge = new ConditionalEdge(
            decisionNode,
            actionNode,
            state => 
            {
                var priority = state.GetValue<int>("priority", 0);
                var isUrgent = state.GetValue<bool>("isUrgent", false);
                var hasPermission = state.GetValue<string>("userRole") == "admin";
                
                return priority > 7 || (isUrgent && hasPermission);
            },
            name: "High Priority or Urgent Admin Route"
        );

        Console.WriteLine($"✅ Created complex decision edge: {decisionEdge.Name}");

        // Example 2: Multiple conditional paths
        var highPriorityEdge = new ConditionalEdge(
            startNode,
            actionNode,
            args => args.ContainsKey("priority") && (int)args["priority"] > 7,
            "High Priority Route"
        );

        var normalPriorityEdge = new ConditionalEdge(
            startNode,
            decisionNode,
            args => args.ContainsKey("priority") && (int)args["priority"] <= 7,
            "Normal Priority Route"
        );

        var fallbackEdge = new ConditionalEdge(
            decisionNode,
            endNode,
            (Func<KernelArguments, bool>)(args => true), // Always traversable
            "Fallback Route"
        );

        Console.WriteLine($"✅ Created multiple conditional paths");

        // Test the workflow
        var testArgs = new KernelArguments();
        testArgs.Add("priority", 8);
        testArgs.Add("isUrgent", true);
        testArgs.Add("userRole", "admin");

        var testState = new GraphState(testArgs);

        Console.WriteLine($"\n🧪 Testing complex workflow:");
        Console.WriteLine($"   High priority edge: {highPriorityEdge.EvaluateCondition(testArgs)}");
        Console.WriteLine($"   Normal priority edge: {normalPriorityEdge.EvaluateCondition(testArgs)}");
        Console.WriteLine($"   Decision edge: {decisionEdge.EvaluateCondition(testState)}");
        Console.WriteLine($"   Fallback edge: {fallbackEdge.EvaluateCondition(testArgs)}");
    }

    /// <summary>
    /// Demonstrates validation and integrity checking
    /// </summary>
    private static async Task ValidationAndIntegrityExample()
    {
        Console.WriteLine("\n🔍 Validation and Integrity Example");
        Console.WriteLine("-".PadLeft(30, '-'));

        // Create nodes for testing
        var node1 = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) => "Node1 executed", "Node1", "First node"),
            "node1"
        );
        var node2 = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) => "Node2 executed", "Node2", "Second node"),
            "node2"
        );

        // Example 1: Valid edge
        var validEdge = new ConditionalEdge(
            sourceNode: node1,
            targetNode: node2,
            condition: args => args.ContainsKey("valid"),
            name: "Valid Edge"
        );

        var validation = validEdge.ValidateIntegrity();
        Console.WriteLine($"✅ Valid edge validation:");
        Console.WriteLine($"   Has errors: {validation.HasErrors}");
        Console.WriteLine($"   Has warnings: {validation.HasWarnings}");
        Console.WriteLine($"   Error count: {validation.ErrorCount}");
        Console.WriteLine($"   Warning count: {validation.WarningCount}");

        // Example 2: Self-loop edge (should generate warning)
        var selfLoopEdge = new ConditionalEdge(
            sourceNode: node1,
            targetNode: node1, // Same node
            condition: args => true,
            name: "Self Loop Edge"
        );

        var selfLoopValidation = selfLoopEdge.ValidateIntegrity();
        Console.WriteLine($"\n⚠️ Self-loop edge validation:");
        Console.WriteLine($"   Has errors: {selfLoopValidation.HasErrors}");
        Console.WriteLine($"   Has warnings: {selfLoopValidation.HasWarnings}");
        if (selfLoopValidation.HasWarnings)
        {
            foreach (var warning in selfLoopValidation.Warnings)
            {
                Console.WriteLine($"   Warning: {warning}");
            }
        }

        // Example 3: Edge with problematic condition
        var problematicEdge = new ConditionalEdge(
            sourceNode: node1,
            targetNode: node2,
            condition: args => throw new InvalidOperationException("Simulated error"),
            name: "Problematic Edge"
        );

        var problematicValidation = problematicEdge.ValidateIntegrity();
        Console.WriteLine($"\n❌ Problematic edge validation:");
        Console.WriteLine($"   Has errors: {problematicValidation.HasErrors}");
        Console.WriteLine($"   Has warnings: {problematicValidation.HasWarnings}");
        if (problematicValidation.HasErrors)
        {
            foreach (var error in problematicValidation.Errors)
            {
                Console.WriteLine($"   Error: {error}");
            }
        }
    }

    /// <summary>
    /// Demonstrates integration with GraphExecutor
    /// </summary>
    private static async Task IntegrationWithGraphExecutorExample()
    {
        Console.WriteLine("\n🔗 GraphExecutor Integration Example");
        Console.WriteLine("-".PadLeft(30, '-'));

        // Create a simple graph executor
        var executor = new GraphExecutor("SimpleExample", "Simple example executor");

        // Create nodes
        var startNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args.Add("input", "test");
                    args.Add("mode", "advanced");
                    return "Start executed";
                },
                "StartNode",
                "Adds input data"
            ),
            "start"
        );

        var processNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args.Add("processed", true);
                    return "Process executed";
                },
                "ProcessNode",
                "Processes data"
            ),
            "process"
        );

        var endNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args.Add("result", "success");
                    return "End executed";
                },
                "EndNode",
                "Completes execution"
            ),
            "end"
        );

        // Add nodes to executor
        executor.AddNode(startNode);
        executor.AddNode(processNode);
        executor.AddNode(endNode);

        // Example 1: Direct edge addition
        var directEdge = new ConditionalEdge(
            sourceNode: startNode,
            targetNode: processNode,
            condition: args => args.ContainsKey("input"),
            name: "Direct Edge"
        );

        executor.AddEdge(directEdge);
        Console.WriteLine($"✅ Added direct edge: {directEdge.Name}");

        // Example 2: Using ConnectWhen extension
        executor.ConnectWhen("process", "end", 
            args => args.ContainsKey("processed") && (bool)args["processed"],
            "Process Complete Route");

        Console.WriteLine($"✅ Connected nodes using ConnectWhen extension");

        // Example 3: Create a complete workflow
        var workflowExecutor = new GraphExecutor("WorkflowExample", "Complete workflow example");

        var inputNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args.Add("data", "sample data");
                    args.Add("priority", 8);
                    return "Input processed";
                },
                "InputNode",
                "Processes input"
            ),
            "input"
        );

        var decisionNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    var priority = args.GetValue<int>("priority");
                    args.Add("route", priority > 7 ? "high" : "normal");
                    return "Decision made";
                },
                "DecisionNode",
                "Makes routing decision"
            ),
            "decision"
        );

        var highPriorityNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args.Add("handled", "high priority");
                    return "High priority handled";
                },
                "HighPriorityNode",
                "Handles high priority"
            ),
            "highPriority"
        );

        var normalPriorityNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args.Add("handled", "normal priority");
                    return "Normal priority handled";
                },
                "NormalPriorityNode",
                "Handles normal priority"
            ),
            "normalPriority"
        );

        var outputNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args.Add("final", "completed");
                    return "Workflow completed";
                },
                "OutputNode",
                "Completes workflow"
            ),
            "output"
        );

        // Add nodes
        workflowExecutor.AddNode(inputNode);
        workflowExecutor.AddNode(decisionNode);
        workflowExecutor.AddNode(highPriorityNode);
        workflowExecutor.AddNode(normalPriorityNode);
        workflowExecutor.AddNode(outputNode);

        // Create conditional routing
        var highPriorityEdge = new ConditionalEdge(
            decisionNode,
            highPriorityNode,
            args => args.ContainsKey("route") && (string)args["route"] == "high",
            "High Priority Route"
        );

        var normalPriorityEdge = new ConditionalEdge(
            decisionNode,
            normalPriorityNode,
            args => args.ContainsKey("route") && (string)args["route"] == "normal",
            "Normal Priority Route"
        );

        var highToOutputEdge = ConditionalEdge.CreateUnconditional(highPriorityNode, outputNode, "High to Output");
        var normalToOutputEdge = ConditionalEdge.CreateUnconditional(normalPriorityNode, outputNode, "Normal to Output");

        // Add edges
        workflowExecutor.AddEdge(highPriorityEdge);
        workflowExecutor.AddEdge(normalPriorityEdge);
        workflowExecutor.AddEdge(highToOutputEdge);
        workflowExecutor.AddEdge(normalToOutputEdge);

        Console.WriteLine($"✅ Created complete workflow with conditional routing");

        // Test the workflow
        var testArgs = new KernelArguments();
        testArgs.Add("data", "test data");
        testArgs.Add("priority", 9);

        Console.WriteLine($"\n🧪 Testing workflow execution:");
        Console.WriteLine($"   Input priority: {testArgs.GetValue<int>("priority")}");
        
        try
        {
            // Create a mock kernel for testing
            var kernel = Kernel.CreateBuilder().Build();
            var result = await workflowExecutor.ExecuteAsync(kernel, testArgs);
            Console.WriteLine($"   Workflow completed successfully");
            Console.WriteLine($"   Final result: {result.GetValueAsString()}");
            Console.WriteLine($"   Route taken: {testArgs.GetValue<string>("route") ?? "unknown"}");
            Console.WriteLine($"   Handling: {testArgs.GetValue<string>("handled") ?? "unknown"}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Workflow execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates edge traversal tracking and metadata
    /// </summary>
    private static async Task EdgeTraversalExample()
    {
        Console.WriteLine("\n📊 Edge Traversal Tracking Example");
        Console.WriteLine("-".PadLeft(30, '-'));

        // Create nodes
        var startNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) => "Start executed", "StartNode", "Start node"),
            "start"
        );
        var endNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) => "End executed", "EndNode", "End node"),
            "end"
        );

        // Create edge with metadata
        var edge = new ConditionalEdge(
            sourceNode: startNode,
            targetNode: endNode,
            condition: args => true,
            name: "Tracked Edge"
        );

        // Add metadata
        edge.Metadata["weight"] = 1.0;
        edge.Metadata["description"] = "This edge tracks traversal metrics";
        edge.Metadata["createdBy"] = "ConditionalEdgeExample";

        Console.WriteLine($"✅ Created edge with metadata:");
        Console.WriteLine($"   Edge: {edge.Name}");
        Console.WriteLine($"   Initial traversal count: {edge.TraversalCount}");
        Console.WriteLine($"   Has been traversed: {edge.HasBeenTraversed}");
        Console.WriteLine($"   Last traversed: {edge.LastTraversedAt}");
        Console.WriteLine($"   Metadata count: {edge.Metadata.Count}");

        // Display metadata
        Console.WriteLine($"\n📋 Edge Metadata:");
        foreach (var kvp in edge.Metadata)
        {
            Console.WriteLine($"   {kvp.Key}: {kvp.Value}");
        }

        // Simulate edge traversal (in real usage, this is done by the executor)
        // Note: RecordTraversal is internal, so we can't call it directly in this example
        Console.WriteLine($"\n💡 Note: In real usage, edge traversal is recorded automatically by the GraphExecutor");
        Console.WriteLine($"   The TraversalCount and LastTraversedAt properties are updated during execution");
    }
}
