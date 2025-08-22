using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Integration;
using SemanticKernel.Graph.Nodes;
using System.Text;

namespace Examples;

/// <summary>
/// Example demonstrating a basic chatbot with memory using graphs.
/// Shows conversation management, context persistence, and simple routing.
/// </summary>
public static class ChatbotExample
{
    /// <summary>
    /// Entry point to run the chatbot examples.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Chatbot with Memory Example ===\n");

        // Run basic, advanced and contextual variants sequentially
        await RunBasicChatbotExampleAsync();
        await RunAdvancedChatbotExampleAsync();
        await RunContextualChatbotExampleAsync();

        Console.WriteLine("=== All Chatbot examples completed! ===");
    }

    /// <summary>
    /// Basic chatbot example using short-term memory.
    /// </summary>
    private static async Task RunBasicChatbotExampleAsync()
    {
        Console.WriteLine("--- Example 1: Basic Chatbot with Short-term Memory ---");

        try
        {
            // Create a kernel instance with default/mock configuration
            var kernel = CreateKernel();

            // Configure memory service options
            var memoryOptions = new GraphMemoryOptions
            {
                EnableVectorSearch = true,
                EnableSemanticSearch = true,
                DefaultCollectionName = "chatbot-memory"
            };
            var memoryService = new GraphMemoryService(memoryOptions);

            // Build the chatbot graph executor
            var chatbot = await CreateBasicChatbotGraphAsync(kernel, memoryService);

            // Simulated conversation turns
            var conversations = new[]
            {
                "Hello, what's your name?",
                "My name is Joao. And yours?",
                "Joao, can you help me with math?",
                "What's the capital of Brazil?",
                "Thanks for the help!"
            };

            Console.WriteLine("🤖 Starting conversation simulation...\n");

            var turnNumber = 1;
            foreach (var userMessage in conversations)
            {
                Console.WriteLine($"👤 User: {userMessage}");

                var arguments = new KernelArguments
                {
                    ["user_message"] = userMessage,
                    ["conversation_id"] = "conv_001",
                    ["user_id"] = "user_001",
                    ["turn_number"] = turnNumber
                };

                var result = await chatbot.ExecuteAsync(kernel, arguments);
                var botResponse = result.GetValue<string>() ?? "I'm sorry, I couldn't process that.";

                Console.WriteLine($"🤖 Bot: {botResponse}");
                Console.WriteLine();

                // Small delay to simulate real conversation
                await Task.Delay(500);
                turnNumber++;
            }

            Console.WriteLine("✅ Basic chatbot example completed successfully!\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in basic chatbot example: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Create a basic chatbot graph with three nodes: load, generate, save.
    /// </summary>
    private static async Task<GraphExecutor> CreateBasicChatbotGraphAsync(
        Kernel kernel,
        IGraphMemoryService memoryService)
    {
        var executor = new GraphExecutor("BasicChatbot", "Simple chatbot with memory");

        // Node: Load context from memory
        var loadContextNode = new FunctionGraphNode(
            CreateLoadContextFunction(kernel, memoryService),
            "load_context",
            "Load Context"
        );

        // Node: Generate a response
        var generateResponseNode = new FunctionGraphNode(
            CreateGenerateResponseFunction(kernel),
            "generate_response",
            "Generate Response"
        );

        // Node: Save context to memory
        var saveContextNode = new FunctionGraphNode(
            CreateSaveContextFunction(kernel, memoryService),
            "save_context",
            "Save Context"
        );

        executor.AddNode(loadContextNode);
        executor.AddNode(generateResponseNode);
        executor.AddNode(saveContextNode);

        // Store results and persist memory after execution of nodes
        loadContextNode.SetMetadata("StoreResultAs", "context");
        loadContextNode.SetMetadata("AfterExecute",
            new Func<Kernel, KernelArguments, FunctionResult, CancellationToken, Task>(async (k, args, result, ct) =>
            {
                var execContext = args.TryGetValue("conversation_id", out var conv) ? conv?.ToString() ?? "unknown" : "unknown";
                await memoryService.StoreNodeExecutionAsync(
                    nodeId: "load_context",
                    input: args,
                    output: result,
                    executionContext: execContext,
                    cancellationToken: ct);
            }));

        generateResponseNode.SetMetadata("StoreResultAs", "bot_response");
        generateResponseNode.SetMetadata("AfterExecute",
            new Func<Kernel, KernelArguments, FunctionResult, CancellationToken, Task>(async (k, args, result, ct) =>
            {
                var execContext = args.TryGetValue("conversation_id", out var conv) ? conv?.ToString() ?? "unknown" : "unknown";
                await memoryService.StoreNodeExecutionAsync(
                    nodeId: "generate_response",
                    input: args,
                    output: result,
                    executionContext: execContext,
                    cancellationToken: ct);
            }));

        saveContextNode.SetMetadata("StoreResultAs", "bot_response");
        saveContextNode.SetMetadata("AfterExecute",
            new Func<Kernel, KernelArguments, FunctionResult, CancellationToken, Task>(async (k, args, result, ct) =>
            {
                var graphState = args.GetOrCreateGraphState();
                var executionId = BuildExecutionId(args);
                var metadata = new Dictionary<string, object>
                {
                    ["conversation_id"] = args.TryGetValue("conversation_id", out var conv) ? conv?.ToString() ?? string.Empty : string.Empty,
                    ["user_id"] = args.TryGetValue("user_id", out var uid) ? uid?.ToString() ?? string.Empty : string.Empty,
                    ["turn_number"] = args.TryGetValue("turn_number", out var turn) ? turn?.ToString() ?? string.Empty : string.Empty,
                };

                await memoryService.StoreExecutionContextAsync(executionId, graphState, metadata, ct);

                var execContext = args.TryGetValue("conversation_id", out var ctx) ? ctx?.ToString() ?? executionId : executionId;
                await memoryService.StoreNodeExecutionAsync(
                    nodeId: "save_context",
                    input: args,
                    output: result,
                    executionContext: execContext,
                    cancellationToken: ct);
            }));

        // Setup the flow: load -> generate -> save
        executor.SetStartNode(loadContextNode.NodeId);
        executor.AddEdge(ConditionalEdge.CreateUnconditional(loadContextNode, generateResponseNode));
        executor.AddEdge(ConditionalEdge.CreateUnconditional(generateResponseNode, saveContextNode));

        return executor;
    }

    /// <summary>
    /// Create function that loads relevant context from memory.
    /// </summary>
    private static KernelFunction CreateLoadContextFunction(Kernel kernel, IGraphMemoryService memoryService)
    {
        return kernel.CreateFunctionFromMethod(
            async (KernelArguments args) =>
            {
                var userMessage = args["user_message"]?.ToString() ?? string.Empty;
                var conversationId = args["conversation_id"]?.ToString() ?? "conv_default";

                var graphState = args.GetOrCreateGraphState();
                var similar = await memoryService.FindSimilarExecutionsAsync(graphState, limit: 3, minSimilarity: 0.2);
                var relevant = await memoryService.SearchRelevantMemoryAsync("save_context", string.IsNullOrWhiteSpace(userMessage) ? conversationId : userMessage, limit: 5);

                var sb = new StringBuilder();
                if (similar.Count > 0)
                {
                    sb.AppendLine($"Found {similar.Count} similar turns for conversation '{conversationId}':");
                    foreach (var s in similar.OrderByDescending(s => s.Timestamp))
                    {
                        sb.AppendLine($"- Similarity {s.SimilarityScore:F2} at {s.Timestamp:HH:mm:ss}");
                    }
                }
                if (relevant.Count > 0)
                {
                    sb.AppendLine("Relevant memory snippets:");
                    foreach (var r in relevant.Take(3))
                    {
                        sb.AppendLine($"- {r.Content}");
                    }
                }

                var context = sb.Length > 0 ? sb.ToString().Trim() : "No previous context";
                return context;
            },
            "load_context",
            "Loads previous conversation context from memory"
        );
    }

    /// <summary>
    /// Create function that generates a simple, deterministic response.
    /// </summary>
    private static KernelFunction CreateGenerateResponseFunction(Kernel kernel)
    {
        return kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var userMessage = args["user_message"]?.ToString() ?? "Hello";
                var context = args["context"]?.ToString() ?? "No previous context";

                return $"Bot response to '{userMessage}': I understand you said '{userMessage}'. Based on context: {context}";
            },
            "generate_response",
            "Generates a natural chatbot response based on context and user input"
        );
    }

    /// <summary>
    /// Create function that saves the current conversation turn to memory and returns the bot reply.
    /// </summary>
    private static KernelFunction CreateSaveContextFunction(Kernel kernel, IGraphMemoryService memoryService)
    {
        return kernel.CreateFunctionFromMethod(
            async (KernelArguments args) =>
            {
                var userMessage = args["user_message"]?.ToString() ?? string.Empty;
                var botResponse = args["bot_response"]?.ToString() ?? string.Empty;
                var conversationId = args["conversation_id"]?.ToString() ?? "conv_default";
                var turnNumber = args["turn_number"]?.ToString() ?? "1";

                var summary = $"conversation:{conversationId} turn:{turnNumber} user:'{userMessage}' bot:'{botResponse}'";
                var graphState = args.GetOrCreateGraphState();
                await memoryService.StoreExecutionContextAsync(
                    executionId: BuildExecutionId(args),
                    graphState: graphState,
                    metadata: new Dictionary<string, object> { ["summary"] = summary });

                // Return the bot response so the graph final output is the assistant reply
                return botResponse;
            },
            "save_context",
            "Saves current conversation turn to memory for future reference"
        );
    }

    /// <summary>
    /// Advanced chatbot example that demonstrates long-term memory and personality.
    /// Kept concise for documentation; it delegates to a graph with a few nodes.
    /// </summary>
    private static async Task RunAdvancedChatbotExampleAsync()
    {
        Console.WriteLine("--- Example 2: Advanced Chatbot with Long-term Memory ---");

        try
        {
            var kernel = CreateKernel();
            var memoryOptions = new GraphMemoryOptions
            {
                EnableVectorSearch = true,
                EnableSemanticSearch = true,
                DefaultCollectionName = "advanced-chatbot-memory"
            };
            var memoryService = new GraphMemoryService(memoryOptions);

            var chatbot = await CreateAdvancedChatbotGraphAsync(kernel, memoryService);

            var conversations = new[]
            {
                "Hello! Do you remember me?",
                "I'm working on an AI project. Do you have tips?",
                "What's the difference between machine learning and deep learning?",
                "Can you recommend some books?"
            };

            Console.WriteLine("🤖 Starting advanced conversation simulation...\n");

            foreach (var userMessage in conversations)
            {
                Console.WriteLine($"👤 User: {userMessage}");

                var arguments = new KernelArguments
                {
                    ["user_message"] = userMessage,
                    ["conversation_id"] = "conv_advanced_001",
                    ["user_id"] = "user_advanced_001",
                    ["personality"] = "helpful_expert",
                    // Defaults required by some validation logic
                    ["context"] = "",
                    ["interaction_history"] = ""
                };

                var result = await chatbot.ExecuteAsync(kernel, arguments);
                var botResponse = result.GetValue<string>() ?? "I need more information to help you.";

                Console.WriteLine($"🤖 Bot: {botResponse}");
                Console.WriteLine();

                await Task.Delay(700);
            }

            Console.WriteLine("✅ Advanced chatbot example completed successfully!\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in advanced chatbot example: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Build an advanced chatbot graph with a few nodes. Implementation uses prompt-based functions
    /// to keep the example compact for documentation.
    /// </summary>
    private static async Task<GraphExecutor> CreateAdvancedChatbotGraphAsync(
        Kernel kernel,
        IGraphMemoryService memoryService)
    {
        var executor = new GraphExecutor("AdvancedChatbot", "Advanced chatbot with personality and long-term memory");

        var contextNode = new FunctionGraphNode(
            CreateAdvancedContextFunction(kernel, memoryService),
            "advanced_context",
            "Advanced Context Loading"
        );

        var intentNode = new ConditionalGraphNode(
            state => true,
            "intent_analysis",
            "Intent Analysis",
            "Analyzes user intent and routes to appropriate handler"
        );

        var responseNode = new FunctionGraphNode(
            CreateAdvancedResponseFunction(kernel),
            "advanced_response",
            "Advanced Response Generation"
        );

        var persistNode = new FunctionGraphNode(
            CreateAdvancedPersistFunction(kernel, memoryService),
            "advanced_persist",
            "Advanced Context Persistence"
        );

        executor.AddNode(contextNode);
        executor.AddNode(intentNode);
        executor.AddNode(responseNode);
        executor.AddNode(persistNode);

        contextNode.SetMetadata("StoreResultAs", "context");
        contextNode.SetMetadata("AfterExecute",
            new Func<Kernel, KernelArguments, FunctionResult, CancellationToken, Task>((k, args, result, ct) =>
            {
                if (!args.TryGetValue("interaction_history", out var historyObj) || string.IsNullOrWhiteSpace(historyObj?.ToString()))
                {
                    args["interaction_history"] = "";
                }
                return Task.CompletedTask;
            }));

        responseNode.SetMetadata("StoreResultAs", "bot_response");

        executor.SetStartNode(contextNode.NodeId);
        executor.AddEdge(ConditionalEdge.CreateUnconditional(contextNode, intentNode));
        executor.AddEdge(ConditionalEdge.CreateUnconditional(intentNode, responseNode));
        executor.AddEdge(ConditionalEdge.CreateUnconditional(responseNode, persistNode));

        return executor;
    }

    /// <summary>
    /// Creates a contextual chatbot graph with sophisticated conversation management.
    /// </summary>
    private static async Task<GraphExecutor> CreateContextualChatbotGraphAsync(
        Kernel kernel,
        IGraphMemoryService memoryService)
    {
        var executor = new GraphExecutor("ContextualChatbot", "Sophisticated contextual conversation management");

        // Advanced conversation flow nodes
        var sessionNode = new FunctionGraphNode(
            CreateSessionManagementFunction(kernel, memoryService),
            "session_management",
            "Session Management"
        );

        var contextAnalysisNode = new FunctionGraphNode(
            CreateContextAnalysisFunction(kernel),
            "context_analysis",
            "Context Analysis"
        );

        var responseGenerationNode = new FunctionGraphNode(
            CreateContextualResponseFunction(kernel),
            "contextual_response",
            "Contextual Response Generation"
        );

        var memoryUpdateNode = new FunctionGraphNode(
            CreateMemoryUpdateFunction(kernel, memoryService),
            "memory_update",
            "Memory Update"
        );

        executor.AddNode(sessionNode);
        executor.AddNode(contextAnalysisNode);
        executor.AddNode(responseGenerationNode);
        executor.AddNode(memoryUpdateNode);

        executor.SetStartNode(sessionNode.NodeId);
        executor.AddEdge(ConditionalEdge.CreateUnconditional(sessionNode, contextAnalysisNode));
        executor.AddEdge(ConditionalEdge.CreateUnconditional(contextAnalysisNode, responseGenerationNode));
        executor.AddEdge(ConditionalEdge.CreateUnconditional(responseGenerationNode, memoryUpdateNode));

        return executor;
    }

    // --- Contextual chatbot (third example) ---

    private static async Task RunContextualChatbotExampleAsync()
    {
        Console.WriteLine("--- Example 3: Multi-turn Conversation with Context ---");

        try
        {
            var kernel = CreateKernel();
            var memoryOptions = new GraphMemoryOptions
            {
                EnableVectorSearch = true,
                EnableSemanticSearch = true,
                DefaultCollectionName = "contextual-chatbot-memory"
            };
            var memoryService = new GraphMemoryService(memoryOptions);

            var chatbot = await CreateContextualChatbotGraphAsync(kernel, memoryService);

            var conversationScenario = new[]
            {
                "I need help planning a trip to Japan",
                "It would be in March, for 10 days",
                "I'm interested in traditional culture and technology",
                "What would be a rough budget?",
                "Do I need a visa?",
                "Thanks for the info!"
            };

            Console.WriteLine("🤖 Starting contextual conversation simulation...\n");

            var conversationState = new KernelArguments
            {
                ["conversation_id"] = "conv_contextual_001",
                ["user_id"] = "user_contextual_001",
                ["conversation_topic"] = "travel_planning",
                ["context_depth"] = "deep",
                ["turn_number"] = 0,
                ["full_context"] = "",
                ["key_entities"] = ""
            };

            foreach (var userMessage in conversationScenario)
            {
                Console.WriteLine($"👤 User: {userMessage}");

                conversationState["user_message"] = userMessage;
                conversationState["turn_number"] = (int)(conversationState["turn_number"] ?? 0) + 1;

                var result = await chatbot.ExecuteAsync(kernel, conversationState);
                var botResponse = result.GetValue<string>() ?? "Let me think about that...";

                Console.WriteLine($"🤖 Bot: {botResponse}");
                Console.WriteLine();

                await Task.Delay(800);
            }

            Console.WriteLine("✅ Contextual chatbot example completed successfully!\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in contextual chatbot example: {ex.Message}\n");
        }
    }

    // --- Helper factory functions used by examples ---

    private static KernelFunction CreateAdvancedContextFunction(Kernel kernel, IGraphMemoryService memoryService)
    {
        return kernel.CreateFunctionFromPrompt(
            "Loading advanced conversation context with personality integration...",
            functionName: "advanced_context",
            description: "Loads conversation context with personality and preference data"
        );
    }

    private static KernelFunction CreateAdvancedResponseFunction(Kernel kernel)
    {
        var prompt = @"
You are an advanced AI assistant with expertise in multiple domains.

Conversation Context: {{$context}}
User Message: {{$user_message}}
User Personality: {{$personality}}
Previous Interactions: {{$interaction_history}}

Guidelines:
- Adapt your communication style to the user's preferences
- Provide detailed, accurate information when requested
- Reference previous conversations naturally
- Offer proactive suggestions when appropriate
- Maintain consistency with your established personality

Generate a thoughtful, personalized response:";

        return kernel.CreateFunctionFromPrompt(
            prompt,
            functionName: "advanced_response",
            description: "Generates personalized responses based on user history and preferences"
        );
    }

    private static KernelFunction CreateAdvancedPersistFunction(Kernel kernel, IGraphMemoryService memoryService)
    {
        return kernel.CreateFunctionFromPrompt(
            "Persisting advanced conversation data with relationship mapping...",
            functionName: "advanced_persist",
            description: "Saves conversation with relationship and preference data"
        );
    }

    private static KernelFunction CreateSessionManagementFunction(Kernel kernel, IGraphMemoryService memoryService)
    {
        return kernel.CreateFunctionFromPrompt(
            "Managing conversation session and user state...",
            functionName: "session_management",
            description: "Manages conversation sessions and user state persistence"
        );
    }

    private static KernelFunction CreateContextAnalysisFunction(Kernel kernel)
    {
        return kernel.CreateFunctionFromPrompt(
            "Analyzing conversation context and extracting key information...",
            functionName: "context_analysis",
            description: "Analyzes conversation context for key entities and relationships"
        );
    }

    private static KernelFunction CreateContextualResponseFunction(Kernel kernel)
    {
        var prompt = @"
You are having a contextual conversation. Analyze the full conversation history and generate an appropriate response.

Conversation Topic: {{$conversation_topic}}
Turn Number: {{$turn_number}}
Full Context: {{$full_context}}
Current Message: {{$user_message}}

Key Context Elements:
{{$key_entities}}

Generate a contextually aware response that:
- References relevant previous information naturally
- Builds upon the established conversation thread
- Provides helpful and specific information
- Maintains conversation flow

Response:";

        return kernel.CreateFunctionFromPrompt(
            prompt,
            functionName: "contextual_response",
            description: "Generates contextually aware responses using full conversation history"
        );
    }

    private static KernelFunction CreateMemoryUpdateFunction(Kernel kernel, IGraphMemoryService memoryService)
    {
        return kernel.CreateFunctionFromPrompt(
            "Updating long-term memory with conversation insights...",
            functionName: "memory_update",
            description: "Updates long-term memory with extracted insights and relationships"
        );
    }

    /// <summary>
    /// Create a Kernel instance using appsettings.json if available. Falls back to a mock key.
    /// </summary>
    private static Kernel CreateKernel()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var openAiApiKey = configuration["OpenAI:ApiKey"];
        var openAiModel = configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";

        if (string.IsNullOrEmpty(openAiApiKey))
        {
            Console.WriteLine("⚠️  OpenAI API Key not found in appsettings.json. Using mock key for demo.");
            openAiApiKey = "mock-api-key";
        }

        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(openAiModel, openAiApiKey)
            .Build();

        // Add simple mock plugin functions to ensure examples run even without external models
        kernel.ImportPluginFromFunctions("mock", "Mock functions for demonstration", new[]
        {
            kernel.CreateFunctionFromMethod(
                (string input) => $"Mock response to: {input}",
                "mock_response",
                "Generates a mock response"
            ),
            kernel.CreateFunctionFromMethod(
                (string context) => $"Loaded context: {context}",
                "mock_load_context",
                "Mock context loading"
            ),
            kernel.CreateFunctionFromMethod(
                (string context) => $"Saved context: {context}",
                "mock_save_context",
                "Mock context saving"
            )
        });

        return kernel;
    }

    /// <summary>
    /// Helper to build a stable execution id from kernel arguments.
    /// </summary>
    private static string BuildExecutionId(KernelArguments args)
    {
        var conv = args.TryGetValue("conversation_id", out var c) ? c?.ToString() ?? "conv" : "conv";
        var turn = args.TryGetValue("turn_number", out var t) ? t?.ToString() ?? "0" : "0";
        return $"{conv}:{turn}";
    }
}


