# Your First Graph in 5 Minutes

This tutorial will guide you through creating your first graph workflow with SemanticKernel.Graph. You'll learn how to create a kernel, enable graph support, build nodes, connect them, and execute your first graph.

## What You'll Build

You'll create a simple "Hello World" graph that demonstrates the basic concepts:
* A function node that processes input
* A conditional node that makes decisions
* Basic state management
* Graph execution

## Prerequisites

Before starting, ensure you have:
* [SemanticKernel.Graph installed](installation.md) in your project
* A configured LLM provider (OpenAI, Azure OpenAI, etc.)
* Your API keys set up in environment variables

## Step 1: Set Up Your Project

### Create a New Console Application

```bash
dotnet new console -n MyFirstGraph
cd MyFirstGraph
```

### Add Required Packages

```bash
dotnet add package Microsoft.SemanticKernel
dotnet add package SemanticKernel.Graph
```

### Set Up Environment Variables

```bash
# Windows
setx OPENAI_API_KEY "your-api-key-here"

# macOS/Linux
export OPENAI_API_KEY="your-api-key-here"
```

## Step 2: Create Your First Graph

Replace the contents of `Program.cs` with this code:

```csharp
using Microsoft.SemanticKernel;
using SemanticKernel.Graph;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== My First Graph ===\n");

        // Step 1: Create and configure your kernel
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-4", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        
        // Enable graph functionality with one line
        builder.AddGraphSupport();
        
        var kernel = builder.Build();

        // Step 2: Create your first function node
        var greetingNode = new FunctionGraphNode(
            kernel.CreateFunctionFromPrompt("Generate a friendly greeting for {{$name}}. Keep it short and warm.")
        );

        // Step 3: Create a conditional node for decision making
        var decisionNode = new ConditionalGraphNode("ShouldContinue", 
            (state) => state.ContainsKey("greeting") && state["greeting"]?.ToString()?.Length > 20);

        // Step 4: Create a follow-up node
        var followUpNode = new FunctionGraphNode(
            kernel.CreateFunctionFromPrompt("Based on this greeting: '{{$greeting}}', suggest a follow-up question to continue the conversation.")
        );

        // Step 5: Create and configure your graph
        var graph = new GraphExecutor("MyFirstGraph", "A simple greeting workflow");

        // Add all nodes to the graph
        graph.AddNode(greetingNode);
        graph.AddNode(decisionNode);
        graph.AddNode(followUpNode);

        // Connect the nodes
        graph.Connect(greetingNode, decisionNode);
        graph.Connect(decisionNode, followUpNode, 
            edge => edge.When(state => state.ContainsKey("greeting") && state["greeting"]?.ToString()?.Length > 20));
        graph.Connect(decisionNode, null, 
            edge => edge.When(state => !(state.ContainsKey("greeting") && state["greeting"]?.ToString()?.Length > 20)));

        // Set the starting point
        graph.SetStartNode(greetingNode);

        // Step 6: Execute your graph
        var state = new KernelArguments { ["name"] = "Alice" };
        
        Console.WriteLine("Executing graph...");
        var result = await graph.ExecuteAsync(state);
        
        Console.WriteLine("\n=== Results ===");
        Console.WriteLine($"Greeting: {result.GetValueOrDefault("greeting", "No greeting generated")}");
        
        if (result.ContainsKey("output"))
        {
            Console.WriteLine($"Follow-up: {result["output"]}");
        }
        
        Console.WriteLine("\nGraph execution completed successfully!");
    }
}
```

## Step 3: Run Your Graph

Execute your application:

```bash
dotnet run
```

You should see output similar to:

```
=== My First Graph ===

Executing graph...

=== Results ===
Greeting: Hello Alice! It's wonderful to meet you today. I hope you're having a fantastic day filled with joy and positivity.
Follow-up: That's such a warm and enthusiastic greeting! What's something that's bringing you joy today, or is there a particular topic you'd like to explore together?

Graph execution completed successfully!
```

## Understanding What Happened

Let's break down what your graph accomplished:

### 1. **Kernel Creation and Graph Support**
```csharp
var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion("gpt-4", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
builder.AddGraphSupport(); // This enables all graph functionality
```

The `AddGraphSupport()` extension method registers all the necessary services for graph execution, including:
* Graph executor factory
* Node converters
* State management
* Error handling policies

### 2. **Function Graph Node**
```csharp
var greetingNode = new FunctionGraphNode(
    kernel.CreateFunctionFromPrompt("Generate a friendly greeting for {{$name}}. Keep it short and warm.")
);
```

This creates a node that:
* Wraps a Semantic Kernel function
* Can be connected to other nodes
* Automatically handles input/output state management

### 3. **Conditional Node**
```csharp
var decisionNode = new ConditionalGraphNode("ShouldContinue", 
    (state) => state.ContainsKey("greeting") && state["greeting"]?.ToString()?.Length > 20);
```

This node:
* Evaluates a condition based on the current state
* Routes execution to different paths based on the result
* Enables dynamic workflow behavior

### 4. **Graph Assembly**
```csharp
graph.AddNode(greetingNode);
graph.AddNode(decisionNode);
graph.AddNode(followUpNode);
```

You add nodes to the graph, then connect them to define the flow.

### 5. **Node Connections**
```csharp
graph.Connect(greetingNode, decisionNode);
graph.Connect(decisionNode, followUpNode, 
    edge => edge.When(state => state.ContainsKey("greeting") && state["greeting"]?.ToString()?.Length > 20));
```

Connections define:
* The sequence of execution
* Conditional routing based on state
* Multiple possible execution paths

### 6. **Execution**
```csharp
var result = await graph.ExecuteAsync(state);
```

The graph executor:
* Traverses the graph from the start node
* Executes each node in sequence
* Manages state transitions between nodes
* Returns the final state with all results

## Key Concepts Demonstrated

### **State Management**
* Input state: `{ "name": "Alice" }`
* Intermediate state: `{ "name": "Alice", "greeting": "Hello Alice!..." }`
* Final state: Contains both input and generated content

### **Conditional Execution**
* The decision node evaluates the greeting length
* Only executes the follow-up if the greeting is substantial
* Demonstrates dynamic workflow behavior

### **Node Types**
* **FunctionGraphNode**: Executes AI functions
* **ConditionalGraphNode**: Makes routing decisions
* **GraphExecutor**: Orchestrates the entire workflow

## Experiment and Customize

Try these modifications to learn more:

### **Change the Input**
```csharp
var state = new KernelArguments { ["name"] = "Bob" };
```

### **Modify the Condition**
```csharp
var decisionNode = new ConditionalGraphNode("ShouldContinue", 
    (state) => state.ContainsKey("greeting") && state["greeting"]?.ToString()?.Contains("wonderful"));
```

### **Add More Nodes**
```csharp
var summaryNode = new FunctionGraphNode(
    kernel.CreateFunctionFromPrompt("Summarize this conversation: {{$greeting}} {{$output}}")
);
graph.AddNode(summaryNode);
graph.Connect(followUpNode, summaryNode);
```

## Common Issues and Solutions

### **API Key Not Found**
```
System.InvalidOperationException: OPENAI_API_KEY not found
```
**Solution**: Ensure your environment variable is set correctly and restart your terminal.

### **Graph Execution Fails**
```
System.InvalidOperationException: No start node configured
```
**Solution**: Make sure you've called `graph.SetStartNode()` with a valid node.

### **Nodes Not Connected**
```
System.InvalidOperationException: No next nodes found for node 'NodeName'
```
**Solution**: Verify all nodes are properly connected using `graph.Connect()`.

## Next Steps

Congratulations! You've successfully created your first graph. Here's what to explore next:

* **[State Management Tutorial](state-tutorial.md)**: Learn how to work with graph state and data flow
* **[Conditional Nodes Guide](how-to/conditional-nodes.md)**: Master conditional logic and routing
* **[Core Concepts](concepts/index.md)**: Understand the fundamental building blocks
* **[Examples](examples/index.md)**: See more complex real-world patterns

## Concepts and Techniques

This tutorial introduces several key concepts:

* **Graph**: A directed structure of nodes and edges that defines workflow execution
* **Node**: Individual units of work that can execute functions, make decisions, or perform operations
* **Edge**: Connections between nodes that can include conditional logic for dynamic routing
* **State**: Data that flows through the graph, maintaining context across execution steps
* **Execution**: The process of traversing the graph, executing nodes, and managing state transitions

## Prerequisites and Minimum Configuration

To complete this tutorial, you need:
* **.NET 8.0+** runtime and SDK
* **SemanticKernel.Graph** package installed
* **LLM Provider** configured with valid API keys
* **Environment Variables** set up for your API credentials

## See Also

* **[Installation Guide](installation.md)**: Set up SemanticKernel.Graph in your project
* **[Core Concepts](concepts/index.md)**: Understanding graphs, nodes, and execution
* **[How-to Guides](how-to/build-a-graph.md)**: Building more complex graph workflows
* **[API Reference](api/core.md)**: Complete API documentation
