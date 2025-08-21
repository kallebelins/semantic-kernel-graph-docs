# State Management Tutorial

This tutorial will teach you how to work with state in SemanticKernel.Graph. You'll learn how to use `KernelArguments` and `GraphState`, manage data flow between nodes, and leverage the powerful state management features.

## What You'll Learn

* How state flows through your graph
* Using `KernelArguments` for input and output
* Working with `GraphState` for enhanced state management
* State validation and versioning
* Best practices for state design

## Prerequisites

Before starting, ensure you have:
* Completed the [First Graph Tutorial](first-graph.md)
* Basic understanding of SemanticKernel.Graph concepts
* A configured LLM provider

## Understanding State in Graphs

### What is State?

State in SemanticKernel.Graph represents the data that flows through your workflow. It's like a backpack that gets passed from node to node, collecting and carrying information throughout the execution.

### State Components

* **Input State**: Initial data when the graph starts
* **Intermediate State**: Data modified by each node during execution
* **Output State**: Final state containing all results and intermediate data

## Basic State Management

### Creating Input State

```csharp
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Extensions;

// Basic state with simple values
var state = new KernelArguments
{
    ["userName"] = "Alice",
    ["userAge"] = 30,
    ["preferences"] = new[] { "AI", "Machine Learning", "Graphs" }
};

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
```

### Reading State in Nodes

```csharp
var analysisNode = new FunctionGraphNode(
    kernel.CreateFunctionFromPrompt(@"
        Analyze this user profile:
        Name: {{$userProfile.Name}}
        Department: {{$userProfile.Department}}
        Skills: {{$userProfile.Skills}}
        
        Provide insights about their technical background and suggest learning paths.
    ")
);
```

### Writing to State

```csharp
var processingNode = new FunctionGraphNode(
    kernel.CreateFunctionFromPrompt(@"
        Process the user profile and provide:
        1. Technical assessment
        2. Learning recommendations
        3. Career suggestions
        
        Format as JSON with keys: assessment, recommendations, career
    ")
);

// Store the result in a specific key
processingNode.StoreResultAs("analysis");
```

## Enhanced State with GraphState

### What is GraphState?

`GraphState` is an enhanced wrapper around `KernelArguments` that provides additional features like:
* State versioning and validation
* Execution history tracking
* Metadata management
* Serialization capabilities

### Using GraphState

```csharp
using SemanticKernel.Graph.State;

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

// Access state with enhanced features
var input = graphState.GetValue<string>("input");
var metadata = graphState.GetValue<Dictionary<string, object>>("metadata");

        // GraphState automatically tracks execution metadata
        Console.WriteLine($"State ID: {graphState.StateId}");
        Console.WriteLine($"Created At: {graphState.CreatedAt}");
```

## State Flow Between Nodes

### Building a State-Aware Graph

```csharp
using Microsoft.SemanticKernel;
using SemanticKernel.Graph;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-4", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        builder.AddGraphSupport();
        var kernel = builder.Build();

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
            
            // Access results from the state after execution
            var analysis = state.TryGetValue("analysis", out var analysisValue) ? analysisValue?.ToString() : "No analysis";
            var response = state.TryGetValue("response", out var responseValue) ? responseValue?.ToString() : "No response";
            var summary = state.TryGetValue("summary", out var summaryValue) ? summaryValue?.ToString() : "No summary";
            
            Console.WriteLine($"Sentiment: {analysis}");
            Console.WriteLine($"Response: {response}");
            Console.WriteLine($"Summary: {summary}");
        }
    }
}
```

## State Validation and Type Safety

### Type-Safe State Access

```csharp
// Use extension methods for type-safe access
var state = new KernelArguments
{
    ["count"] = 42,
    ["name"] = "Alice",
    ["isActive"] = true
};

// Type-safe retrieval with defaults using TryGetValue
var count = state.TryGetValue("count", out var countValue) && countValue is int countInt ? countInt : 0;
var name = state.TryGetValue("name", out var nameValue) && nameValue is string nameString ? nameString : "Unknown";
var isActive = state.TryGetValue("isActive", out var isActiveValue) && isActiveValue is bool isActiveBool ? isActiveBool : false;

// Try to get values with type checking
if (state.TryGetValue("count", out var countValue) && countValue is int safeCount)
{
    Console.WriteLine($"Count is: {safeCount}");
}
```

### State Validation

```csharp
var validationNode = new FunctionGraphNode(
    kernel.CreateFunctionFromPrompt(@"
        Validate this user data:
        Name: {{$userName}}
        Age: {{$userAge}}
        Email: {{$userEmail}}
        
        Return JSON with validation results:
        {
            "isValid": boolean,
            "errors": [string],
            "warnings": [string]
        }
    ")
).StoreResultAs("validation");

// Add validation logic
var decisionNode = new ConditionalGraphNode("IsValidUser",
    state => 
    {
        var validation = state.TryGetValue("validation", out var validationValue) ? validationValue?.ToString() : "";
        return validation.Contains("\"isValid\": true");
    });
```

## Advanced State Patterns

### State Accumulation

```csharp
var accumulationNode = new FunctionGraphNode(
    kernel.CreateFunctionFromPrompt(@"
        Based on the current conversation history: {{$conversationHistory}}
        And the new input: {{$newInput}}
        
        Update the conversation with the new interaction.
        Return the updated conversation history.
    ")
).StoreResultAs("conversationHistory");

// Initialize with empty history
var state = new KernelArguments
{
    ["conversationHistory"] = new List<string>(),
    ["newInput"] = "Hello, I need help with AI"
};
```

### State Transformation

```csharp
var transformNode = new FunctionGraphNode(
    kernel.CreateFunctionFromPrompt(@"
        Transform this data structure: {{$rawData}}
        
        Extract and structure the following information:
        - Key insights
        - Action items
        - Priority levels
        
        Return as structured JSON.
    ")
).StoreResultAs("structuredData");
```

## State Persistence and Checkpointing

### Basic Checkpointing

```csharp
// Enable checkpointing in your graph options
builder.AddGraphSupport(options =>
{
    options.Checkpointing.Enabled = true;
    options.Checkpointing.Interval = 3; // Checkpoint every 3 nodes
});

// State is automatically saved and can be restored
var checkpointManager = kernel.GetRequiredService<ICheckpointManager>();
var savedState = await checkpointManager.SaveCheckpointAsync(graphId, executionId, state);
```

### State Serialization

```csharp
// State can be serialized for external storage
var serializedState = JsonSerializer.Serialize(state, new JsonSerializerOptions
{
    WriteIndented = true
});

// And deserialized back
var restoredState = JsonSerializer.Deserialize<KernelArguments>(serializedState);
```

## Best Practices

### 1. **Use Descriptive Keys**
```csharp
// Good
state["userProfile"] = userData;
state["analysisResults"] = analysis;

// Avoid
state["data"] = userData;
state["result"] = analysis;
```

### 2. **Validate State at Key Points**
```csharp
var validationNode = new ConditionalGraphNode("ValidateState",
    state => 
    {
        // Check required fields exist
        var required = new[] { "userProfile", "analysisResults" };
        return required.All(key => state.ContainsKey(key));
    });
```

### 3. **Use Type-Safe Access**
```csharp
// Use TryGetValue for safety
var count = state.TryGetValue("count", out var countValue) && countValue is int countInt ? countInt : 0;
var name = state.TryGetValue("name", out var nameValue) && nameValue is string nameString ? nameString : "Unknown";
```

### 4. **Structure Complex State**
```csharp
// Group related data
state["user"] = new
{
    Profile = userProfile,
    Preferences = userPreferences,
    History = userHistory
};
```

### 5. **Handle State Errors Gracefully**
```csharp
var errorHandlerNode = new FunctionGraphNode(
    kernel.CreateFunctionFromPrompt(@"
        Handle this error gracefully: {{$error}}
        Provide a user-friendly message and recovery suggestions.
    ")
).StoreResultAs("errorResponse");
```

## Common State Patterns

### **Configuration State**
```csharp
var configState = new KernelArguments
{
    ["maxRetries"] = 3,
    ["timeout"] = TimeSpan.FromSeconds(30),
    ["logLevel"] = "Information"
};
```

### **Session State**
```csharp
var sessionState = new KernelArguments
{
    ["sessionId"] = Guid.NewGuid().ToString(),
    ["startTime"] = DateTimeOffset.UtcNow,
    ["userContext"] = userContext
};
```

### **Workflow State**
```csharp
var workflowState = new KernelArguments
{
    ["currentStep"] = "analysis",
    ["completedSteps"] = new[] { "input", "validation" },
    ["nextSteps"] = new[] { "processing", "output" }
};
```

## Troubleshooting State Issues

### **Common Problems**

#### State Not Persisting Between Nodes
```
System.Collections.Generic.KeyNotFoundException: The given key 'result' was not present in the dictionary.
```
**Solution**: Ensure nodes are properly storing results using `StoreResultAs()` or manual state assignment.

#### Type Conversion Errors
```
System.InvalidCastException: Unable to cast object of type 'System.String' to type 'System.Int32'.
```
**Solution**: Use type-safe access methods like `TryGetValue` with type checking or validate types before casting.

#### State Corruption
```
System.Text.Json.JsonException: The JSON value could not be converted to KernelArguments.
```
**Solution**: Check state serialization/deserialization logic and ensure all objects are serializable.

## Next Steps

Now that you understand state management, explore these advanced topics:

* **[Conditional Nodes Guide](how-to/conditional-nodes.md)**: Master conditional logic and routing
* **[Checkpointing Tutorial](checkpointing-tutorial.md)**: Learn about state persistence and recovery
* **[Error Handling Guide](how-to/error-handling.md)**: Handle state errors and exceptions
* **[Advanced Patterns](patterns/index.md)**: Discover complex state management patterns

## Concepts and Techniques

This tutorial covers several key concepts:

* **State**: Data that flows through the graph, maintaining context across execution steps
* **KernelArguments**: The base state container that holds key-value pairs
* **GraphState**: Enhanced state wrapper with versioning, validation, and metadata
* **State Flow**: How data moves between nodes and transforms during execution
* **State Validation**: Ensuring data integrity and type safety throughout the workflow

## Prerequisites and Minimum Configuration

To complete this tutorial, you need:
* **SemanticKernel.Graph** package installed and configured
* **LLM Provider** with valid API keys
* **Basic understanding** of graph concepts from the first tutorial
* **.NET 8.0+** runtime environment

## See Also

* **[First Graph Tutorial](first-graph.md)**: Build your first complete graph workflow
* **[Core Concepts](concepts/index.md)**: Understanding graphs, nodes, and execution
* **[API Reference](api/core.md)**: Complete API documentation
* **[Examples](examples/index.md)**: Real-world usage patterns and implementations
