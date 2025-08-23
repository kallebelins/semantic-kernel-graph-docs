using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;

namespace Examples;

/// <summary>
/// Example demonstrating state management in SemanticKernel.Graph.
/// This example shows how to work with KernelArguments and GraphState,
/// including state creation, reading, writing, and flow between nodes.
/// </summary>
public class StateQuickstartExample
{
    /// <summary>
    /// Runs the comprehensive state management example demonstrating all core concepts.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== State Management Quickstart Example ===\n");

        try
        {
            // Step 1: Demonstrate basic state creation and management
            DemonstrateBasicStateManagement();

            // Step 2: Show enhanced state with GraphState
            DemonstrateEnhancedState();

            // Step 3: Execute a complete graph with state flow
            await DemonstrateStateFlowBetweenNodesAsync();

            Console.WriteLine("\n✅ State management examples completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in state management examples: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Demonstrates basic state creation and management with KernelArguments.
    /// Shows simple state creation, reading, and writing operations.
    /// </summary>
    private static void DemonstrateBasicStateManagement()
    {
        Console.WriteLine("Step 1: Basic State Creation and Management\n");

        // Create basic state with KernelArguments
        var state = new KernelArguments
        {
            ["userName"] = "Alice",
            ["userAge"] = 30,
            ["preferences"] = new[] { "AI", "Machine Learning", "Graphs" }
        };

        // Add more values to demonstrate dynamic state building
        state["timestamp"] = DateTimeOffset.UtcNow;
        state["requestId"] = Guid.NewGuid().ToString();

        Console.WriteLine("✅ Basic state created with KernelArguments");
        Console.WriteLine($"   - User: {state["userName"]}");
        Console.WriteLine($"   - Age: {state["userAge"]}");
        Console.WriteLine($"   - Preferences: {string.Join(", ", (string[])state["preferences"])}");
        Console.WriteLine($"   - Timestamp: {state["timestamp"]}");
        Console.WriteLine($"   - Request ID: {state["requestId"]}\n");

        // Demonstrate complex objects in state
        var userProfile = new
        {
            Name = "Bob",
            Department = "Engineering",
            Skills = new[] { "C#", ".NET", "AI" },
            Experience = 5
        };

        var stateWithObjects = new KernelArguments
        {
            ["userProfile"] = userProfile,
            ["metadata"] = new Dictionary<string, object>
            {
                ["source"] = "tutorial",
                ["version"] = "1.0"
            }
        };

        Console.WriteLine("✅ Complex objects added to state");
        Console.WriteLine($"   - User Profile: {userProfile.Name} from {userProfile.Department}");
        Console.WriteLine($"   - Skills: {string.Join(", ", userProfile.Skills)}");
        Console.WriteLine($"   - Metadata: {stateWithObjects["metadata"]}\n");

        // Demonstrate reading state values
        Console.WriteLine("Step 1.1: Reading State Values\n");

        // Read simple values with type safety using TryGetValue
        var name = state.TryGetValue("userName", out var nameValue) ? nameValue : "Unknown";
        var age = state.TryGetValue("userAge", out var ageValue) ? ageValue : 0;
        var preferences = state.TryGetValue("preferences", out var prefValue) ? prefValue : new string[0];

        Console.WriteLine($"✅ Type-safe reading:");
        Console.WriteLine($"   - Name: {name} (Type: {name.GetType().Name})");
        Console.WriteLine($"   - Age: {age} (Type: {age.GetType().Name})");
        Console.WriteLine($"   - Preferences: {string.Join(", ", (string[])preferences)} (Type: {preferences.GetType().Name})\n");

        // Safe reading with defaults
        var department = state.TryGetValue("department", out var deptValue) ? deptValue : "Unknown";
        var score = state.TryGetValue("score", out var scoreValue) ? scoreValue : 0;

        Console.WriteLine($"✅ Safe reading with defaults:");
        Console.WriteLine($"   - Department: {department} (default used)");
        Console.WriteLine($"   - Score: {score} (default used)\n");

        // Demonstrate writing to state
        Console.WriteLine("Step 1.2: Writing to State\n");

        // Set new values
        state["processed"] = true;
        state["result"] = "Success";
        state["score"] = 95.5;

        // Update existing values
        state["userAge"] = 31;

        Console.WriteLine($"✅ State updated:");
        Console.WriteLine($"   - Processed: {state["processed"]}");
        Console.WriteLine($"   - Result: {state["result"]}");
        Console.WriteLine($"   - Score: {state["score"]}");
        Console.WriteLine($"   - Updated Age: {state["userAge"]}\n");

        // Demonstrate extension methods
        Console.WriteLine("Step 1.3: Using Extension Methods\n");

        // Convert to GraphState for enhanced features
        var graphState = state.ToGraphState();

        // Add execution tracking
        state.StartExecutionStep("basic_demo", "BasicStateDemo");
        state.SetCurrentNode("basic_demo");

        // Complete execution step
        state.CompleteExecutionStep("Basic state management completed successfully");

        Console.WriteLine($"✅ Extension methods applied:");
        Console.WriteLine($"   - GraphState created: {graphState.StateId}");
        Console.WriteLine($"   - Execution step started and completed\n");
    }

    /// <summary>
    /// Demonstrates enhanced state features with GraphState.
    /// Shows metadata, validation, and execution tracking capabilities.
    /// </summary>
    private static void DemonstrateEnhancedState()
    {
        Console.WriteLine("Step 2: Enhanced State with GraphState\n");

        // Create a base state
        var baseState = new KernelArguments
        {
            ["input"] = "Sample input data",
            ["priority"] = "high",
            ["category"] = "tutorial"
        };

        // Create GraphState from KernelArguments
        var graphState = new GraphState(baseState);

        Console.WriteLine($"✅ GraphState created:");
        Console.WriteLine($"   - State ID: {graphState.StateId}");
        Console.WriteLine($"   - Version: {graphState.Version}");
        Console.WriteLine($"   - Created At: {graphState.CreatedAt}\n");

        // Add metadata
        graphState.SetMetadata("source", "user_input");
        graphState.SetMetadata("priority", "high");
        graphState.SetMetadata("environment", "development");

        Console.WriteLine($"✅ Metadata added:");
        Console.WriteLine($"   - Source: {graphState.GetMetadata<string>("source")}");
        Console.WriteLine($"   - Priority: {graphState.GetMetadata<string>("priority")}");
        Console.WriteLine($"   - Environment: {graphState.GetMetadata<string>("environment")}\n");

        // Get execution history
        var history = graphState.ExecutionHistory;
        var stepCount = graphState.ExecutionStepCount;

        Console.WriteLine($"✅ Execution tracking:");
        Console.WriteLine($"   - Step Count: {stepCount}");
        Console.WriteLine($"   - History Entries: {history.Count}\n");
    }

    /// <summary>
    /// Demonstrates state flow between nodes in a complete graph.
    /// Shows how data flows through the graph with each node reading and writing state.
    /// </summary>
    private static async Task DemonstrateStateFlowBetweenNodesAsync()
    {
        Console.WriteLine("Step 3: State Flow Between Nodes\n");

        try
        {
            // Create kernel with graph support
            var kernel = CreateKernelWithGraphSupport();

            // Create nodes that demonstrate state flow
            var (inputNode, analysisNode, summaryNode) = CreateStateFlowNodes();

            // Build and configure the graph
            var graph = BuildStateFlowGraph(inputNode, analysisNode, summaryNode);

            // Execute with initial state
            var initialState = new KernelArguments
            {
                ["input"] = "Hello world from SemanticKernel.Graph state management"
            };

            Console.WriteLine("🚀 Executing graph with state flow...\n");

            var result = await graph.ExecuteAsync(kernel, initialState);

            // Display final state
            Console.WriteLine("=== Final State ===");
            var input = initialState.TryGetValue("input", out var inputValue) ? inputValue : "Not found";
            var processed = initialState.TryGetValue("processedInput", out var processedValue) ? processedValue : "Not found";
            var wordCount = initialState.TryGetValue("wordCount", out var wordCountValue) ? wordCountValue : "Not found";
            var sentiment = initialState.TryGetValue("sentiment", out var sentimentValue) ? sentimentValue : "Not found";
            var complexity = initialState.TryGetValue("complexity", out var complexityValue) ? complexityValue : "Not found";
            var summary = initialState.TryGetValue("finalSummary", out var summaryValue) ? summaryValue : "Not found";

            Console.WriteLine($"Input: {input}");
            Console.WriteLine($"Processed: {processed}");
            Console.WriteLine($"Word Count: {wordCount}");
            Console.WriteLine($"Sentiment: {sentiment}");
            Console.WriteLine($"Complexity: {complexity}");
            Console.WriteLine($"Summary: {summary}");

            Console.WriteLine($"\nFinal Result: {result.GetValueAsString()}");

            Console.WriteLine("\n✅ State flow completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in state flow demonstration: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates and configures a kernel with graph support enabled.
    /// </summary>
    /// <returns>A configured kernel instance with graph support</returns>
    private static Kernel CreateKernelWithGraphSupport()
    {
        var builder = Kernel.CreateBuilder();

        // Add OpenAI chat completion service (optional for this example)
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrEmpty(apiKey))
        {
            builder.AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey);
        }
        else
        {
            Console.WriteLine("⚠️  OPENAI_API_KEY not found. Using mock functions for demonstration.");
        }

        // Enable graph functionality
        builder.AddGraphSupport();

        return builder.Build();
    }

    /// <summary>
    /// Creates nodes that demonstrate state flow between execution steps.
    /// </summary>
    /// <returns>A tuple containing all created nodes</returns>
    private static (FunctionGraphNode inputNode, FunctionGraphNode analysisNode, FunctionGraphNode summaryNode)
        CreateStateFlowNodes()
    {
        // Node 1: Input processing
        var inputNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    var input = args.TryGetValue("input", out var inputValue) ? inputValue?.ToString() : "No input";
                    var processed = $"Processed: {input?.ToUpper()}";

                    // Write to state
                    args["processedInput"] = processed;
                    args["wordCount"] = input?.Split(' ')?.Length ?? 0;
                    args["timestamp"] = DateTimeOffset.UtcNow;

                    return processed;
                },
                "ProcessInput",
                "Processes and analyzes input text"
            ),
            "input_node"
        ).StoreResultAs("inputResult");

        // Node 2: Analysis
        var analysisNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    var processedInput = args.TryGetValue("processedInput", out var processedValue) ? processedValue?.ToString() : "";
                    var wordCount = args.TryGetValue("wordCount", out var wordCountValue) ? Convert.ToInt32(wordCountValue) : 0;

                    // Perform analysis
                    var sentiment = wordCount > 5 ? "Detailed" : "Brief";
                    var complexity = wordCount > 10 ? "High" : "Low";

                    // Write analysis to state
                    args["sentiment"] = sentiment;
                    args["complexity"] = complexity;
                    args["analysisComplete"] = true;

                    return $"Analysis: {sentiment} content with {complexity} complexity";
                },
                "AnalyzeContent",
                "Analyzes content characteristics"
            ),
            "analysis_node"
        ).StoreResultAs("analysisResult");

        // Node 3: Summary
        var summaryNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    // Read all state values
                    var input = args.TryGetValue("input", out var inputValue) ? inputValue?.ToString() : "No input";
                    var processed = args.TryGetValue("processedInput", out var processedValue) ? processedValue?.ToString() : "No processed input";
                    var wordCount = args.TryGetValue("wordCount", out var wordCountValue) ? Convert.ToInt32(wordCountValue) : 0;
                    var sentiment = args.TryGetValue("sentiment", out var sentimentValue) ? sentimentValue?.ToString() : "No sentiment";
                    var complexity = args.TryGetValue("complexity", out var complexityValue) ? complexityValue?.ToString() : "No complexity";

                    var summary = $"Input: '{input}' -> Processed: '{processed}' -> " +
                                $"Word Count: {wordCount}, Sentiment: {sentiment}, Complexity: {complexity}";

                    args["finalSummary"] = summary;
                    return summary;
                },
                "CreateSummary",
                "Creates final summary from all state data"
            ),
            "summary_node"
        ).StoreResultAs("summaryResult");

        return (inputNode, analysisNode, summaryNode);
    }

    /// <summary>
    /// Builds and configures the state flow graph with all nodes connected.
    /// </summary>
    /// <param name="inputNode">The input processing node</param>
    /// <param name="analysisNode">The analysis node</param>
    /// <param name="summaryNode">The summary node</param>
    /// <returns>A configured graph executor</returns>
    private static GraphExecutor BuildStateFlowGraph(
        FunctionGraphNode inputNode,
        FunctionGraphNode analysisNode,
        FunctionGraphNode summaryNode)
    {
        var graph = new GraphExecutor("StateFlowExample", "Demonstrates state flow between nodes");

        // Add all nodes to the graph
        graph.AddNode(inputNode);
        graph.AddNode(analysisNode);
        graph.AddNode(summaryNode);

        // Connect nodes in sequence using node names
        graph.Connect("input_node", "analysis_node");
        graph.Connect("analysis_node", "summary_node");

        // Set the starting node
        graph.SetStartNode("input_node");

        return graph;
    }
}
