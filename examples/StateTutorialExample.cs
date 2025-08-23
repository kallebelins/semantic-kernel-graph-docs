using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;
using System.Text.Json;

namespace Examples;

/// <summary>
/// Comprehensive tutorial example demonstrating state management in SemanticKernel.Graph.
/// This example covers all aspects of state management including:
/// - Basic state creation and management with KernelArguments
/// - Enhanced state with GraphState
/// - State flow between nodes with conditional routing
/// - State validation and type safety
/// - Advanced state patterns and best practices
/// </summary>
public class StateTutorialExample
{
    private static string? openAiApiKey;
    private static string? openAiModel;

    static StateTutorialExample()
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
    /// Runs the complete state management tutorial demonstrating all concepts.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== State Management Tutorial Example ===\n");

        try
        {
            // Step 1: Demonstrate basic state creation and management
            await DemonstrateBasicStateManagementAsync();

            // Step 2: Show enhanced state with GraphState
            await DemonstrateEnhancedStateAsync();

            // Step 3: Execute a complete graph with state flow
            await DemonstrateStateFlowBetweenNodesAsync();

            // Step 4: Demonstrate state validation and type safety
            await DemonstrateStateValidationAndTypeSafetyAsync();

            // Step 5: Show advanced state patterns
            await DemonstrateAdvancedStatePatternsAsync();

            Console.WriteLine("\n✅ State management tutorial completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in state management tutorial: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Demonstrates basic state creation and management with KernelArguments.
    /// Shows simple state creation, reading, and writing operations.
    /// </summary>
    private static async Task DemonstrateBasicStateManagementAsync()
    {
        Console.WriteLine("Step 1: Basic State Creation and Management\n");

        // Create basic state with simple values
        var state = new KernelArguments
        {
            ["userName"] = "Alice",
            ["userAge"] = 30,
            ["preferences"] = new[] { "AI", "Machine Learning", "Graphs" }
        };

        Console.WriteLine("✅ Basic state created with KernelArguments");
        Console.WriteLine($"   - User: {state["userName"]}");
        Console.WriteLine($"   - Age: {state["userAge"]}");
        Console.WriteLine($"   - Preferences: {string.Join(", ", (string[])state["preferences"])}\n");

        // State with complex objects
        var userProfile = new
        {
            Name = "Bob",
            Department = "Engineering",
            Skills = new[] { "C#", ".NET", "AI" }
        };

        var stateWithObjects = new KernelArguments
        {
            ["userProfile"] = userProfile,
            ["requestId"] = Guid.NewGuid().ToString(),
            ["timestamp"] = DateTimeOffset.UtcNow
        };

        Console.WriteLine("✅ Complex objects added to state");
        Console.WriteLine($"   - User Profile: {userProfile.Name} from {userProfile.Department}");
        Console.WriteLine($"   - Skills: {string.Join(", ", userProfile.Skills)}");
        Console.WriteLine($"   - Request ID: {stateWithObjects["requestId"]}\n");

        // Demonstrate reading state values
        Console.WriteLine("Step 1.1: Reading State Values\n");

        // Read simple values with type safety using TryGetValue
        var name = state.TryGetValue("userName", out var nameValue) ? nameValue : "Unknown";
        var age = state.TryGetValue("userAge", out var ageValue) ? ageValue : 0;
        var preferences = state.TryGetValue("preferences", out var prefValue) ? prefValue : new string[0];

        Console.WriteLine($"✅ Retrieved values: Name={name}, Age={age}");
        Console.WriteLine($"   Preferences: {string.Join(", ", (string[])preferences)}\n");

        // Demonstrate writing to state
        Console.WriteLine("Step 1.2: Writing to State\n");

        // Add new values to existing state
        state["lastLogin"] = DateTimeOffset.UtcNow;
        state["isActive"] = true;
        state["score"] = 95.5;

        Console.WriteLine("✅ New values added to state");
        Console.WriteLine($"   - Last Login: {state["lastLogin"]}");
        Console.WriteLine($"   - Is Active: {state["isActive"]}");
        Console.WriteLine($"   - Score: {state["score"]}\n");
    }

    /// <summary>
    /// Demonstrates enhanced state management using GraphState.
    /// Shows advanced features like versioning, validation, and metadata.
    /// </summary>
    private static async Task DemonstrateEnhancedStateAsync()
    {
        Console.WriteLine("Step 2: Enhanced State with GraphState\n");

        // Create enhanced state
        var graphState = new GraphState(new KernelArguments
        {
            ["input"] = "Hello World",
            ["metadata"] = new Dictionary<string, object>
            {
                ["source"] = "tutorial",
                ["version"] = "1.0"
            }
        });

        Console.WriteLine("✅ Enhanced state created with GraphState");
        Console.WriteLine($"   - Input: {graphState.GetValue<string>("input")}");
        Console.WriteLine($"   - Source: {graphState.GetValue<Dictionary<string, object>>("metadata")["source"]}");
        Console.WriteLine($"   - Version: {graphState.GetValue<Dictionary<string, object>>("metadata")["version"]}\n");

        // Add execution metadata (GraphState tracks execution history automatically)
        Console.WriteLine("✅ GraphState provides execution tracking");
        Console.WriteLine($"   - State ID: {graphState.StateId}");
        Console.WriteLine($"   - Created At: {graphState.CreatedAt}");
        Console.WriteLine($"   - Is Modified: {graphState.IsModified}");
        Console.WriteLine($"   - Execution Steps: {graphState.ExecutionStepCount}\n");

        // Demonstrate state serialization
        var serializedState = JsonSerializer.Serialize(graphState, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        Console.WriteLine("✅ State serialization demonstrated");
        Console.WriteLine($"   - Serialized length: {serializedState.Length} characters\n");
    }

    /// <summary>
    /// Demonstrates state flow between nodes in a complete graph workflow.
    /// Shows how state is passed, modified, and used for conditional routing.
    /// </summary>
    private static async Task DemonstrateStateFlowBetweenNodesAsync()
    {
        Console.WriteLine("Step 3: State Flow Between Nodes\n");

        // Create kernel with graph support
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(openAiModel!, openAiApiKey!);
        builder.AddGraphSupport();
        var kernel = builder.Build();

        Console.WriteLine("✅ Kernel created with graph support\n");

        // Create nodes that work with state
        var inputNode = new FunctionGraphNode(
            kernel.CreateFunctionFromPrompt(@"
                Analyze this text: {{$input}}
                Provide:
                1. Sentiment (positive/negative/neutral)
                2. Key topics
                3. Word count
                
                Format as JSON with keys: sentiment, topics, wordCount
            ")
        ).StoreResultAs("analysis");

        var decisionNode = new ConditionalGraphNode(
            state => state.GetValue<string>("analysis")?.Contains("positive") == true,
            name: "RouteBySentiment");

        var positiveNode = new FunctionGraphNode(
            kernel.CreateFunctionFromPrompt(@"
                The analysis shows positive sentiment: {{$analysis}}
                Generate an encouraging response with suggestions for next steps.
            ")
        ).StoreResultAs("response");

        var negativeNode = new FunctionGraphNode(
            kernel.CreateFunctionFromPrompt(@"
                The analysis shows negative sentiment: {{$analysis}}
                Provide empathetic support and constructive feedback.
            ")
        ).StoreResultAs("response");

        var summaryNode = new FunctionGraphNode(
            kernel.CreateFunctionFromPrompt(@"
                Create a summary of this interaction:
                Input: {{$input}}
                Analysis: {{$analysis}}
                Response: {{$response}}
                
                Format as a concise summary paragraph.
            ")
        ).StoreResultAs("summary");

        Console.WriteLine("✅ All nodes created with state-aware prompts\n");

        // Build the graph
        var graph = new GraphExecutor("StateManagementExample");

        graph.AddNode(inputNode);
        graph.AddNode(decisionNode);
        graph.AddNode(positiveNode);
        graph.AddNode(negativeNode);
        graph.AddNode(summaryNode);

        // Connect with conditional routing
        graph.Connect(inputNode.NodeId, decisionNode.NodeId);
        graph.ConnectWhen(decisionNode.NodeId, positiveNode.NodeId,
            args => args.ToGraphState().GetValue<string>("analysis")?.Contains("positive") == true);
        graph.ConnectWhen(decisionNode.NodeId, negativeNode.NodeId,
            args => args.ToGraphState().GetValue<string>("analysis")?.Contains("positive") != true);
        graph.Connect(positiveNode.NodeId, summaryNode.NodeId);
        graph.Connect(negativeNode.NodeId, summaryNode.NodeId);

        graph.SetStartNode(inputNode.NodeId);

        Console.WriteLine("✅ Graph built with conditional routing\n");

        // Execute with different inputs
        var testInputs = new[]
        {
            "I love working with AI and machine learning!",
            "I'm struggling with this programming problem.",
            "The weather is beautiful today and I feel great!"
        };

        foreach (var input in testInputs)
        {
            Console.WriteLine($"\n=== Testing with: {input} ===");

            var state = new KernelArguments { ["input"] = input };
            var result = await graph.ExecuteAsync(kernel, state);

            // Access results using safe methods from the state
            var analysis = state.TryGetValue("analysis", out var analysisValue) ? analysisValue?.ToString() : "No analysis";
            var response = state.TryGetValue("response", out var responseValue) ? responseValue?.ToString() : "No response";
            var summary = state.TryGetValue("summary", out var summaryValue) ? summaryValue?.ToString() : "No summary";

            Console.WriteLine($"✅ Sentiment: {analysis}");
            Console.WriteLine($"✅ Response: {response}");
            Console.WriteLine($"✅ Summary: {summary}");
        }

        Console.WriteLine("\n✅ State flow demonstration completed\n");
    }

    /// <summary>
    /// Demonstrates state validation and type safety features.
    /// Shows how to safely access state values and validate data integrity.
    /// </summary>
    private static async Task DemonstrateStateValidationAndTypeSafetyAsync()
    {
        Console.WriteLine("Step 4: State Validation and Type Safety\n");

        // Create state with various types
        var state = new KernelArguments
        {
            ["count"] = 42,
            ["name"] = "Alice",
            ["isActive"] = true,
            ["score"] = 95.5,
            ["tags"] = new[] { "AI", "ML", "Graphs" }
        };

        Console.WriteLine("✅ State created with various data types\n");

        // Type-safe retrieval with defaults using TryGetValue
        var count = state.TryGetValue("count", out var countValue) && countValue is int countInt ? countInt : 0;
        var name = state.TryGetValue("name", out var nameValue) && nameValue is string nameString ? nameString : "Unknown";
        var isActive = state.TryGetValue("isActive", out var isActiveValue) && isActiveValue is bool isActiveBool ? isActiveBool : false;
        var score = state.TryGetValue("score", out var scoreValue) && scoreValue is double scoreDouble ? scoreDouble : 0.0;
        var tags = state.TryGetValue("tags", out var tagsValue) && tagsValue is string[] tagsArray ? tagsArray : new string[0];

        Console.WriteLine("✅ Type-safe retrieval demonstrated");
        Console.WriteLine($"   - Count: {count} (type: {count.GetType().Name})");
        Console.WriteLine($"   - Name: {name} (type: {name.GetType().Name})");
        Console.WriteLine($"   - Is Active: {isActive} (type: {isActive.GetType().Name})");
        Console.WriteLine($"   - Score: {score} (type: {score.GetType().Name})");
        Console.WriteLine($"   - Tags: {string.Join(", ", tags)} (type: {tags.GetType().Name})\n");

        // Try to get values with type checking using standard TryGetValue
        if (state.TryGetValue("count", out var safeCountObj) && safeCountObj is int safeCount)
        {
            Console.WriteLine($"✅ Safe count retrieval: {safeCount}");
        }

        if (state.TryGetValue("name", out var safeNameObj) && safeNameObj is string safeName)
        {
            Console.WriteLine($"✅ Safe name retrieval: {safeName}");
        }

        // Demonstrate validation logic using GraphState
        var validationNode = new ConditionalGraphNode(
            state =>
            {
                // Check required fields exist and have valid types
                var hasValidName = state.TryGetValue<string>("name", out var userName) &&
                                 !string.IsNullOrWhiteSpace(userName);
                var hasValidAge = state.TryGetValue<int>("age", out var userAge) &&
                                userAge > 0 && userAge < 150;
                var hasValidScore = state.TryGetValue<double>("score", out var userScore) &&
                                  userScore >= 0.0 && userScore <= 100.0;

                return hasValidName && hasValidAge && hasValidScore;
            },
            name: "IsValidUser");

        Console.WriteLine("\n✅ Validation logic demonstrated");
        Console.WriteLine($"   - Validation node created with type-safe checks\n");
    }

    /// <summary>
    /// Demonstrates advanced state patterns including accumulation and transformation.
    /// Shows complex state management scenarios and best practices.
    /// </summary>
    private static async Task DemonstrateAdvancedStatePatternsAsync()
    {
        Console.WriteLine("Step 5: Advanced State Patterns\n");

        // State accumulation pattern
        var conversationState = new KernelArguments
        {
            ["conversationHistory"] = new List<string>(),
            ["userPreferences"] = new Dictionary<string, object>(),
            ["sessionMetrics"] = new
            {
                StartTime = DateTimeOffset.UtcNow,
                MessageCount = 0,
                AverageResponseTime = TimeSpan.Zero
            }
        };

        Console.WriteLine("✅ Conversation state initialized with accumulation structure\n");

        // State transformation pattern
        var rawData = new
        {
            Text = "Sample text for analysis",
            Timestamp = DateTimeOffset.UtcNow,
            Source = "tutorial"
        };

        var transformState = new KernelArguments
        {
            ["rawData"] = rawData,
            ["transformationRules"] = new Dictionary<string, string>
            {
                ["text"] = "extract_keywords",
                ["timestamp"] = "format_datetime",
                ["source"] = "validate_source"
            }
        };

        Console.WriteLine("✅ Transformation state created with rules\n");

        // Configuration state pattern
        var configState = new KernelArguments
        {
            ["maxRetries"] = 3,
            ["timeout"] = TimeSpan.FromSeconds(30),
            ["logLevel"] = "Information",
            ["cacheEnabled"] = true,
            ["cacheExpiration"] = TimeSpan.FromMinutes(15)
        };

        Console.WriteLine("✅ Configuration state created");
        Console.WriteLine($"   - Max Retries: {configState["maxRetries"]}");
        Console.WriteLine($"   - Timeout: {configState["timeout"]}");
        Console.WriteLine($"   - Log Level: {configState["logLevel"]}");
        Console.WriteLine($"   - Cache Enabled: {configState["cacheEnabled"]}\n");

        // Workflow state pattern
        var workflowState = new KernelArguments
        {
            ["currentStep"] = "initialization",
            ["completedSteps"] = new[] { "setup", "validation" },
            ["nextSteps"] = new[] { "processing", "output", "cleanup" },
            ["stepResults"] = new Dictionary<string, object>(),
            ["errors"] = new List<string>(),
            ["warnings"] = new List<string>()
        };

        Console.WriteLine("✅ Workflow state created with step tracking");
        Console.WriteLine($"   - Current Step: {workflowState["currentStep"]}");
        Console.WriteLine($"   - Completed Steps: {string.Join(", ", (string[])workflowState["completedSteps"])}");
        Console.WriteLine($"   - Next Steps: {string.Join(", ", (string[])workflowState["nextSteps"])}\n");

        // Demonstrate state persistence
        var serializedWorkflowState = JsonSerializer.Serialize(workflowState, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        Console.WriteLine("✅ State persistence demonstrated");
        Console.WriteLine($"   - Workflow state serialized: {serializedWorkflowState.Length} characters\n");

        Console.WriteLine("✅ Advanced state patterns demonstration completed\n");
    }

    /// <summary>
    /// Runs all examples in sequence for comprehensive demonstration.
    /// </summary>
    public static async Task RunAllExamplesAsync()
    {
        Console.WriteLine("🚀 Running Complete State Management Tutorial...\n");
        await RunAsync();
    }
}
