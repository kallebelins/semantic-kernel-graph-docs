# Chatbot with Memory Example

This example demonstrates a basic chatbot with memory using graphs. It shows conversation management, context persistence, and intelligent routing for building conversational AI systems with persistent memory.

## Objective

Learn how to implement a conversational AI system that:
- Maintains conversation context across multiple turns
- Integrates short-term and long-term memory
- Routes conversations intelligently based on user intent
- Provides personalized responses based on conversation history
- Scales from simple to advanced conversation patterns

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [State Management](../concepts/state.md)

## Key Components

### Concepts and Techniques

- **Conversation Memory**: Persistent storage of conversation context and user interactions
- **Intent Recognition**: Understanding user intent and routing conversations appropriately
- **Context Management**: Maintaining relevant information across conversation turns
- **Memory Integration**: Combining short-term and long-term memory for better responses

### Core Classes

- `GraphMemoryService`: Manages conversation memory and context
- `FunctionGraphNode`: Executes conversation logic and memory operations
- `GraphMemoryOptions`: Configures memory behavior and storage
- `KernelArguments`: Carries conversation state and context between turns

## Running the Example

### Getting Started

This example demonstrates how to build a chatbot with memory using the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Basic Chatbot with Short-term Memory

This example demonstrates a simple chatbot with conversation context.

```csharp
// Create kernel with mock configuration
var kernel = CreateKernel();

// Create memory service
var memoryOptions = new GraphMemoryOptions
{
    EnableVectorSearch = true,
    EnableSemanticSearch = true,
    DefaultCollectionName = "chatbot-memory"
};
var memoryService = new GraphMemoryService(memoryOptions);

// Create chatbot graph
var chatbot = await CreateBasicChatbotGraphAsync(kernel, memoryService);

// Simulate conversation
var conversations = new[]
{
    "Olá, qual é o seu nome?",
    "Meu nome é João. E o seu?",
    "João, você pode me ajudar com matemática?",
    "Qual é a capital do Brasil?",
    "Obrigado pela ajuda!"
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
```

### 2. Advanced Chatbot with Long-term Memory

Demonstrates persistent memory across multiple conversations.

```csharp
// Create advanced chatbot with long-term memory
var advancedChatbot = await CreateAdvancedChatbotGraphAsync(kernel, memoryService);

// Simulate multiple conversations with the same user
var conversation1 = new[]
{
    "Hi, I'm Alex. I'm interested in machine learning.",
    "What are the best resources to start with?",
    "I have some experience with Python."
};

var conversation2 = new[]
{
    "Hi again! Remember our discussion about machine learning?",
    "I've been studying the resources you recommended.",
    "Can you suggest some advanced topics?"
};

// First conversation
Console.WriteLine("=== First Conversation ===");
foreach (var message in conversation1)
{
    var arguments = new KernelArguments
    {
        ["user_message"] = message,
        ["conversation_id"] = "conv_alex_001",
        ["user_id"] = "alex",
        ["session_type"] = "learning_consultation"
    };

    var result = await advancedChatbot.ExecuteAsync(kernel, arguments);
    var response = result.GetValue<string>();
    Console.WriteLine($"User: {message}");
    Console.WriteLine($"Bot: {response}\n");
}

// Second conversation (with memory)
Console.WriteLine("=== Second Conversation (with Memory) ===");
foreach (var message in conversation2)
{
    var arguments = new KernelArguments
    {
        ["user_message"] = message,
        ["conversation_id"] = "conv_alex_002",
        ["user_id"] = "alex",
        ["session_type"] = "follow_up"
    };

    var result = await advancedChatbot.ExecuteAsync(kernel, arguments);
    var response = result.GetValue<string>();
    Console.WriteLine($"User: {message}");
    Console.WriteLine($"Bot: {response}\n");
}
```

### 3. Multi-turn Conversation with Context

Shows how to maintain context across complex conversation flows.

```csharp
// Create contextual chatbot
var contextualChatbot = await CreateContextualChatbotGraphAsync(kernel, memoryService);

// Simulate a complex multi-turn conversation
var complexConversation = new[]
{
    "I need help planning a vacation to Europe.",
    "I'm interested in history and culture.",
    "My budget is around $5000 for two weeks.",
    "I prefer smaller cities over tourist traps.",
    "What about transportation between cities?",
    "Can you suggest a specific itinerary?",
    "What about accommodation recommendations?",
    "Thank you, this is very helpful!"
};

Console.WriteLine("🌍 Starting vacation planning conversation...\n");

var context = new Dictionary<string, object>
{
    ["topic"] = "vacation_planning",
    ["destination"] = "Europe",
    ["interests"] = new[] { "history", "culture" },
    ["budget"] = 5000,
    ["duration"] = "2 weeks",
    ["preferences"] = new[] { "smaller cities", "authentic experience" }
};

foreach (var message in complexConversation)
{
    var arguments = new KernelArguments
    {
        ["user_message"] = message,
        ["conversation_id"] = "conv_vacation_001",
        ["user_id"] = "vacation_planner",
        ["context"] = context
    };

    var result = await contextualChatbot.ExecuteAsync(kernel, arguments);
    var response = result.GetValue<string>();
    
    // Update context based on conversation
    var updatedContext = result.GetValue<Dictionary<string, object>>("updated_context");
    if (updatedContext != null)
    {
        context = updatedContext;
    }

    Console.WriteLine($"User: {message}");
    Console.WriteLine($"Bot: {response}\n");
}
```

## Expected Output

### Basic Chatbot Example

```
🤖 Starting conversation simulation...

👤 User: Olá, qual é o seu nome?
🤖 Bot: Olá! Meu nome é Assistant, um chatbot criado para ajudar você. Como posso ser útil hoje?

👤 User: Meu nome é João. E o seu?
🤖 Bot: Olá João! É um prazer conhecê-lo. Como mencionei, sou o Assistant. Como posso ajudá-lo hoje?

👤 User: João, você pode me ajudar com matemática?
🤖 Bot: Claro, João! Ficarei feliz em ajudá-lo com matemática. Que tipo de problema ou conceito você gostaria de revisar?

👤 User: Qual é a capital do Brasil?
🤖 Bot: A capital do Brasil é Brasília. Foi inaugurada em 21 de abril de 1960, substituindo o Rio de Janeiro como capital federal.

👤 User: Obrigado pela ajuda!
🤖 Bot: De nada, João! Foi um prazer ajudá-lo. Se precisar de mais alguma coisa, especialmente com matemática ou outras questões, estarei aqui para ajudar!

✅ Basic chatbot example completed successfully!
```

### Advanced Chatbot with Memory

```
=== First Conversation ===
User: Hi, I'm Alex. I'm interested in machine learning.
Bot: Hello Alex! It's great to meet you. I'm excited to help you with machine learning. What specific areas interest you most?

User: What are the best resources to start with?
Bot: For beginners in machine learning, I recommend starting with:
1. "Hands-On Machine Learning" by Aurélien Géron
2. Online courses on Coursera or edX
3. Practical projects on Kaggle

User: I have some experience with Python.
Bot: Excellent! Since you know Python, you're already ahead. I'd suggest diving into:
- Scikit-learn for traditional ML
- TensorFlow or PyTorch for deep learning
- Jupyter notebooks for experimentation

=== Second Conversation (with Memory) ===
User: Hi again! Remember our discussion about machine learning?
Bot: Of course, Alex! Welcome back! I remember we discussed your interest in machine learning and I recommended some great resources including "Hands-On Machine Learning" and practical projects on Kaggle. How has your learning journey been going?

User: I've been studying the resources you recommended.
Bot: That's fantastic, Alex! I'm glad you've been following up on those recommendations. How are you finding the "Hands-On Machine Learning" book? And have you had a chance to explore any Kaggle competitions yet?

User: Can you suggest some advanced topics?
Bot: Absolutely! Based on your foundation, here are some advanced topics to explore:
- Deep Learning architectures (CNNs, RNNs, Transformers)
- Advanced optimization techniques
- Model interpretability and explainability
- MLOps and model deployment
- Reinforcement learning fundamentals

Would you like me to elaborate on any of these areas?
```

## Configuration Options

### Memory Service Configuration

```csharp
var memoryOptions = new GraphMemoryOptions
{
    EnableVectorSearch = true,           // Enable semantic search
    EnableSemanticSearch = true,        // Enable similarity-based search
    DefaultCollectionName = "chatbot-memory", // Memory collection name
    SimilarityThreshold = 0.8,          // Similarity threshold for search
    MaxMemoryItems = 1000,              // Maximum memory items per user
    MemoryExpiration = TimeSpan.FromDays(30), // Memory retention period
    EnableCompression = true,           // Compress memory for storage
    EnableIndexing = true               // Enable fast search indexing
};
```

### Chatbot Graph Configuration

```csharp
var chatbotOptions = new ChatbotOptions
{
    EnableContextMemory = true,         // Remember conversation context
    EnableUserProfiles = true,          // Maintain user profiles
    EnableIntentRecognition = true,     // Recognize user intent
    EnableSentimentAnalysis = true,     // Analyze user sentiment
    MaxContextTurns = 10,               // Maximum context turns to remember
    ResponseTimeout = TimeSpan.FromSeconds(30), // Response generation timeout
    EnableFallbackResponses = true,     // Provide fallback when uncertain
    EnableConversationAnalytics = true  // Track conversation metrics
};
```

## Troubleshooting

### Common Issues

#### Memory Not Persisting
```bash
# Problem: Conversation context is lost between turns
# Solution: Ensure memory service is properly configured
EnableContextMemory = true;
MaxContextTurns = 10;
```

#### Slow Response Generation
```bash
# Problem: Bot responses are slow
# Solution: Optimize memory search and enable caching
EnableIndexing = true;
EnableCompression = true;
ResponseTimeout = TimeSpan.FromSeconds(60);
```

#### Context Confusion
```bash
# Problem: Bot gets confused about conversation context
# Solution: Improve context management and memory cleanup
MaxContextTurns = 5; // Reduce context window
EnableMemoryCleanup = true;
```

### Debug Mode

Enable detailed logging for troubleshooting:

```csharp
// Enable debug logging
var logger = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
}).CreateLogger<ChatbotExample>();

// Configure chatbot with debug logging
var debugChatbot = await CreateBasicChatbotGraphAsync(kernel, memoryService);
debugChatbot.EnableDebugMode = true;
debugChatbot.LogMemoryOperations = true;
debugChatbot.LogContextChanges = true;
```

## Advanced Patterns

### Intent-Based Routing

```csharp
// Implement intent recognition for better conversation flow
var intentRouter = new IntentRouter
{
    IntentPatterns = new Dictionary<string, string>
    {
        ["greeting"] = @"^(hi|hello|hey|olá|oi)",
        ["question"] = @"^(what|how|why|when|where|can you|do you)",
        ["request"] = @"^(help|assist|support|need|want)",
        ["gratitude"] = @"^(thank|thanks|obrigado|obrigada|appreciate)"
    },
    IntentHandlers = new Dictionary<string, Func<string, Task<string>>>
    {
        ["greeting"] = async (message) => await HandleGreeting(message),
        ["question"] = async (message) => await HandleQuestion(message),
        ["request"] = async (message) => await HandleRequest(message),
        ["gratitude"] = async (message) => await HandleGratitude(message)
    }
};

chatbot.IntentRouter = intentRouter;
```

### Dynamic Memory Management

```csharp
// Implement adaptive memory management
var adaptiveMemory = new AdaptiveMemoryManager
{
    MemoryPrioritization = (context) =>
    {
        // Prioritize recent and relevant memories
        var relevance = CalculateRelevance(context);
        var recency = CalculateRecency(context);
        return relevance * 0.7 + recency * 0.3;
    },
    MemoryCleanup = (memories) =>
    {
        // Remove low-priority memories when storage is full
        return memories.OrderByDescending(m => m.Priority)
                      .Take(1000);
    }
};

memoryService.MemoryManager = adaptiveMemory;
```

### Multi-Modal Conversations

```csharp
// Support for different conversation modalities
var multiModalChatbot = new MultiModalChatbot
{
    ModalityHandlers = new Dictionary<string, IModalityHandler>
    {
        ["text"] = new TextModalityHandler(),
        ["voice"] = new VoiceModalityHandler(),
        ["image"] = new ImageModalityHandler(),
        ["gesture"] = new GestureModalityHandler()
    },
    ModalityRouter = (input) =>
    {
        // Route to appropriate modality handler
        return input.Type switch
        {
            InputType.Text => "text",
            InputType.Voice => "voice",
            InputType.Image => "image",
            InputType.Gesture => "gesture",
            _ => "text"
        };
    }
};
```

## Related Examples

- [Memory Agent](./memory-agent.md): Persistent memory across conversations
- [Retrieval Agent](./retrieval-agent.md): Information retrieval and synthesis
- [Multi-Agent](./multi-agent.md): Coordinated multi-agent workflows
- [State Management](./state-management.md): Graph state and argument handling

## See Also

- [Memory and State](../concepts/state.md): Understanding conversation persistence
- [Graph Concepts](../concepts/graph-concepts.md): Graph-based workflow fundamentals
- [Conversation Patterns](../patterns/chatbot.md): Building conversational AI
- [API Reference](../api/): Complete API documentation
