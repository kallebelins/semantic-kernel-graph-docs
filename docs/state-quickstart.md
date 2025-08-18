# State Quickstart

This quick tutorial will teach you how to work with state in SemanticKernel.Graph. You'll learn how to use `KernelArguments` and `GraphState` with extension methods to manage data flow between nodes.

## What You'll Learn

- Creating and managing state with `KernelArguments`
- Using `GraphState` for enhanced state management
- Reading and writing variables with extension methods
- State flow between nodes
- Basic state validation and tracking

## Concepts and Techniques

**KernelArguments**: A dictionary-like container that holds key-value pairs representing the state and context of your graph execution.

**GraphState**: An enhanced wrapper around `KernelArguments` that provides additional metadata, validation, and serialization capabilities.

**State Flow**: The movement of data between nodes as the graph executes, where each node can read from and write to the shared state.

**Extension Methods**: Utility methods that extend `KernelArguments` with graph-specific functionality for easier state management.

## Prerequisites

- [First Graph Tutorial](first-graph-5-minutes.md) completed
- Basic understanding of SemanticKernel.Graph concepts
- .NET 8.0+ runtime

## Step 1: Basic State Creation

### Simple State with KernelArguments

```csharp
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Extensions;

// Create basic state
var state = new KernelArguments
{
    ["userName"] = "Alice",
    ["userAge"] = 30,
    ["preferences"] = new[] { "AI", "Machine Learning", "Graphs" }
};

// Add more values
state["timestamp"] = DateTimeOffset.UtcNow;
state["requestId"] = Guid.NewGuid().ToString();
```

### Complex Objects in State

```csharp
// Complex object
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
```

## Step 2: Reading State Values

### Basic Reading

```csharp
// Read simple values
var name = state["userName"]; // Returns object
var age = state.GetValue<int>("userAge"); // Returns typed value
var preferences = state.GetValue<string[]>("preferences");

// Safe reading with defaults
var department = state.GetValueOrDefault("department", "Unknown");
var score = state.GetValueOrDefault("score", 0);
```

### Reading Complex Objects

```csharp
// Read complex objects
var profile = state.GetValue<object>("userProfile");
var metadata = state.GetValue<Dictionary<string, object>>("metadata");

// Check if key exists
if (state.ContainsKey("userProfile"))
{
    var userName = state["userProfile"];
    // Process user profile
}
```

## Step 3: Writing to State

### Basic Writing

```csharp
// Set values
state["processed"] = true;
state["result"] = "Success";
state["score"] = 95.5;

// Update existing values
state["userAge"] = 31; // Overwrites existing value
```

### Using Extension Methods

```csharp
// Convert to GraphState for enhanced features
var graphState = state.ToGraphState();

// Add execution tracking
state.StartExecutionStep("analysis_node", "AnalyzeUser");
state.SetCurrentNode("analysis_node");

// Complete execution step
state.CompleteExecutionStep("Analysis completed successfully");
```

## Step 4: Enhanced State with GraphState

### Creating GraphState

```csharp
using SemanticKernel.Graph.State;

// Create GraphState from KernelArguments
var graphState = new GraphState(state);

// Or use extension method
var enhancedState = state.ToGraphState();

// Access enhanced features
var stateId = enhancedState.StateId;
var version = enhancedState.Version;
var createdAt = enhancedState.CreatedAt;
```

### State Validation and Metadata

```csharp
// Add metadata
enhancedState.SetMetadata("source", "user_input");
enhancedState.SetMetadata("priority", "high");

// Validate state
var isValid = enhancedState.ValidateState();
var errors = enhancedState.GetValidationErrors();

// Get execution history
var history = enhancedState.ExecutionHistory;
var stepCount = enhancedState.ExecutionStepCount;
```

## Step 5: State Flow Between Nodes

### Building a State-Aware Graph

```csharp
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-3.5-turbo", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        builder.AddGraphSupport();
        var kernel = builder.Build();

        // Node 1: Input processing
        var inputNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    var input = args.GetValue<string>("input");
                    var processed = $"Processed: {input.ToUpper()}";
                    
                    // Write to state
                    args["processedInput"] = processed;
                    args["wordCount"] = input.Split(' ').Length;
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
                    var processedInput = args.GetValue<string>("processedInput");
                    var wordCount = args.GetValue<int>("wordCount");
                    
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
                    var input = args.GetValue<string>("input");
                    var processed = args.GetValue<string>("processedInput");
                    var wordCount = args.GetValue<int>("wordCount");
                    var sentiment = args.GetValue<string>("sentiment");
                    var complexity = args.GetValue<string>("complexity");
                    
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

        // Build and configure the graph
        var graph = new GraphExecutor("StateFlowExample", "Demonstrates state flow between nodes");
        
        graph.AddNode(inputNode);
        graph.AddNode(analysisNode);
        graph.AddNode(summaryNode);
        
        // Connect nodes in sequence
        graph.Connect(inputNode, analysisNode);
        graph.Connect(analysisNode, summaryNode);
        
        graph.SetStartNode(inputNode);

        // Execute with initial state
        var initialState = new KernelArguments
        {
            ["input"] = "Hello world from SemanticKernel.Graph"
        };
        
        Console.WriteLine("=== State Flow Example ===\n");
        Console.WriteLine("Executing graph...");
        
        var result = await graph.ExecuteAsync(initialState);
        
        Console.WriteLine("\n=== Final State ===");
        Console.WriteLine($"Input: {result.GetValue<string>("input")}");
        Console.WriteLine($"Processed: {result.GetValue<string>("processedInput")}");
        Console.WriteLine($"Word Count: {result.GetValue<int>("wordCount")}");
        Console.WriteLine($"Sentiment: {result.GetValue<string>("sentiment")}");
        Console.WriteLine($"Complexity: {result.GetValue<string>("complexity")}");
        Console.WriteLine($"Summary: {result.GetValue<string>("finalSummary")}");
        
        Console.WriteLine("\n✅ State flow completed successfully!");
    }
}
```

## Step 6: Run Your State Example

### Set Up Environment Variables

```bash
# Windows
setx OPENAI_API_KEY "your-api-key-here"

# macOS/Linux
export OPENAI_API_KEY="your-api-key-here"
```

### Execute the Graph

```bash
dotnet run
```

You should see output like:

```
=== State Flow Example ===

Executing graph...

=== Final State ===
Input: Hello world from SemanticKernel.Graph
Processed: Processed: HELLO WORLD FROM SEMANTICKERNEL.GRAPH
Word Count: 5
Sentiment: Brief
Complexity: Low
Summary: Input: 'Hello world from SemanticKernel.Graph' -> Processed: 'Processed: HELLO WORLD FROM SEMANTICKERNEL.GRAPH' -> Word Count: 5, Sentiment: Brief, Complexity: Low

✅ State flow completed successfully!
```

## What Just Happened?

### 1. **State Creation and Management**
```csharp
var state = new KernelArguments { ["userName"] = "Alice" };
```
`KernelArguments` provides a flexible dictionary-like structure for storing state data.

### 2. **State Reading with Type Safety**
```csharp
var age = state.GetValue<int>("userAge");
var name = state.GetValueOrDefault("name", "Unknown");
```
Extension methods provide type-safe reading with fallback values.

### 3. **State Writing and Updates**
```csharp
state["processed"] = true;
state["result"] = "Success";
```
Simple key-value assignment for writing to state.

### 4. **Enhanced State with GraphState**
```csharp
var graphState = state.ToGraphState();
```
`GraphState` adds execution tracking, metadata, and validation capabilities.

### 5. **State Flow Between Nodes**
Each node reads from and writes to the shared state, creating a data pipeline that flows through the graph.

## Key Concepts

- **State**: Data that flows through your graph, maintained in `KernelArguments`
- **GraphState**: Enhanced wrapper that adds execution tracking and metadata
- **State Flow**: Data moves from node to node, with each node reading input and writing output
- **Extension Methods**: Helper methods that add graph-specific functionality to `KernelArguments`
- **Type Safety**: Use `GetValue<T>()` for type-safe reading of state values

## Common Patterns

### State Initialization
```csharp
var state = new KernelArguments
{
    ["input"] = userInput,
    ["metadata"] = new Dictionary<string, object>(),
    ["timestamp"] = DateTimeOffset.UtcNow
};
```

### State Reading with Validation
```csharp
if (state.TryGetValue("requiredField", out var value))
{
    var typedValue = value as string;
    if (!string.IsNullOrEmpty(typedValue))
    {
        // Process the value
    }
}
```

### State Writing with Metadata
```csharp
state["result"] = processedData;
state["processingTime"] = stopwatch.ElapsedMilliseconds;
state["nodeId"] = "processor_node";
```

## Troubleshooting

### **State Key Not Found**
```
System.Collections.Generic.KeyNotFoundException: The given key 'missingKey' was not present
```
**Solution**: Use `GetValueOrDefault()` or check with `ContainsKey()` before reading.

### **Type Casting Errors**
```
System.InvalidCastException: Unable to cast object of type 'System.Int32' to type 'System.String'
```
**Solution**: Use `GetValue<T>()` for type-safe reading or verify the expected type.

### **State Not Persisting Between Nodes**
```
State values are missing in subsequent nodes
```
**Solution**: Ensure nodes are properly connected and the graph executor is configured correctly.

## Next Steps

- **[Conditional Logic](conditional-nodes-tutorial.md)**: Add decision-making based on state values
- **[State Management Tutorial](state-tutorial.md)**: Advanced state features and patterns
- **[Checkpointing](how-to/checkpointing.md)**: Save and restore graph state
- **[Core Concepts](concepts/index.md)**: Understanding the fundamental building blocks

## Concepts and Techniques

This tutorial introduces several key concepts:

- **State**: Data that flows through the graph, maintaining context across execution steps
- **KernelArguments**: The primary container for state data in Semantic Kernel
- **GraphState**: Enhanced state wrapper with execution tracking and metadata
- **Extension Methods**: Helper methods that add graph-specific functionality to existing types
- **State Flow**: The pattern of data moving from node to node through the graph

## Prerequisites and Minimum Configuration

To complete this tutorial, you need:
- **.NET 8.0+** runtime and SDK
- **SemanticKernel.Graph** package installed
- **LLM Provider** configured with valid API keys
- **Environment Variables** set up for your API credentials

## See Also

- **[First Graph Tutorial](first-graph-5-minutes.md)**: Create your first graph workflow
- **[State Management Tutorial](state-tutorial.md)**: Advanced state management concepts
- **[Core Concepts](concepts/index.md)**: Understanding graphs, nodes, and execution
- **[API Reference](api/state.md)**: Complete state management API documentation

## Reference APIs

- **[KernelArguments](../api/core.md#kernel-arguments)**: Core state container
- **[GraphState](../api/state.md#graph-state)**: Enhanced state wrapper
- **[KernelArgumentsExtensions](../api/extensions.md#kernel-arguments-extensions)**: State extension methods
- **[ISerializableState](../api/state.md#iserializable-state)**: State serialization interface
