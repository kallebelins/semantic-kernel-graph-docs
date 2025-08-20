# First Graph in 5 Minutes

This quick tutorial will get you up and running with SemanticKernel.Graph in just 5 minutes. You'll create a simple graph, connect nodes, and execute your first workflow.

## What You'll Build

A simple "Hello World" graph that demonstrates the core concepts:
* Creating a kernel with graph support
* Building function nodes
* Connecting nodes to form a workflow
* Executing the graph and getting results

## Concepts and Techniques

**Graph**: A directed graph structure composed of nodes (execution units) and edges (connections) that defines the flow of data and control in your application.

**FunctionGraphNode**: A node type that wraps `KernelFunction` instances, allowing you to execute semantic functions as part of your graph workflow.

**GraphExecutor**: The main execution engine that navigates through the graph, executes nodes in the correct order, and manages the flow of data between nodes.

**KernelBuilderExtensions**: Extension methods that add graph support to the Semantic Kernel builder, enabling graph-based workflows.

## Prerequisites

* [SemanticKernel.Graph installed](installation.md)
* .NET 8.0+ runtime
* LLM provider configured (OpenAI, Azure OpenAI, etc.)

## Step 1: Create Your Project

```bash
dotnet new console -n MyFirstGraph
cd MyFirstGraph
dotnet add package Microsoft.SemanticKernel
dotnet add package SemanticKernel.Graph
```

## Step 2: Build Your First Graph

Replace `Program.cs` with this code:

```csharp
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== My First Graph ===\n");

        // 1. Create kernel and enable graph support
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-3.5-turbo", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        builder.AddGraphSupport();
        var kernel = builder.Build();

        // 2. Create a simple function node
        var greetingNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (string name) => $"Hello {name}! Welcome to SemanticKernel.Graph.",
                "GenerateGreeting",
                "Creates a personalized greeting"
            ),
            "greeting_node"
        );

        // 3. Create a follow-up node
        var followUpNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (string greeting) => $"Based on: '{greeting}', here's a follow-up question: How can I help you today?",
                "GenerateFollowUp",
                "Creates a follow-up question"
            ),
            "followup_node"
        );

        // 4. Build and configure the graph
        var graph = new GraphExecutor("MyFirstGraph", "A simple greeting workflow");
        
        graph.AddNode(greetingNode);
        graph.AddNode(followUpNode);
        
        // Connect nodes in sequence
        graph.Connect(greetingNode, followUpNode);
        
        // Set the starting point
        graph.SetStartNode(greetingNode);

        // 5. Execute the graph
        var initialState = new KernelArguments { ["name"] = "Developer" };
        
        Console.WriteLine("Executing graph...");
        var result = await graph.ExecuteAsync(initialState);
        
        Console.WriteLine("\n=== Results ===");
        Console.WriteLine($"Greeting: {result.GetValueOrDefault("greeting", "No greeting")}");
        Console.WriteLine($"Follow-up: {result.GetValueOrDefault("followup", "No follow-up")}");
        
        Console.WriteLine("\n✅ Graph executed successfully!");
    }
}
```

## Step 3: Set Up Environment Variables

```bash
# Windows
setx OPENAI_API_KEY "your-api-key-here"

# macOS/Linux
export OPENAI_API_KEY="your-api-key-here"
```

## Step 4: Run Your Graph

```bash
dotnet run
```

You should see output like:

```
=== My First Graph ===

Executing graph...

=== Results ===
Greeting: Hello Developer! Welcome to SemanticKernel.Graph.
Follow-up: Based on: 'Hello Developer! Welcome to SemanticKernel.Graph.', here's a follow-up question: How can I help you today?

✅ Graph executed successfully!
```

## What Just Happened?

### 1. **Kernel Setup with Graph Support**
```csharp
builder.AddGraphSupport();
```
This single line enables all graph functionality in your Semantic Kernel instance.

### 2. **Function Graph Node**
```csharp
var greetingNode = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod(...),
    "greeting_node"
);
```
Creates a node that wraps a simple function and can be connected to other nodes.

### 3. **Graph Assembly**
```csharp
graph.AddNode(greetingNode);
graph.AddNode(followUpNode);
graph.Connect(greetingNode, followUpNode);
```
Adds nodes to the graph and connects them to define execution flow.

### 4. **Execution**
```csharp
var result = await graph.ExecuteAsync(initialState);
```
The graph executor traverses from the start node, executing each connected node in sequence.

## Key Concepts

* **Graph**: A directed structure of connected nodes that defines workflow execution
* **Node**: Individual units of work (functions, decisions, operations)
* **Edge**: Connections between nodes that define execution flow
* **State**: Data that flows through the graph via `KernelArguments`
* **Executor**: Orchestrates the entire workflow execution

## Next Steps

* **[State Management](state-tutorial.md)**: Learn to work with data flow between nodes
* **[Conditional Logic](conditional-nodes-tutorial.md)**: Add decision-making to your workflows
* **[Core Concepts](concepts/index.md)**: Understand the fundamental building blocks
* **[Examples](examples/index.md)**: See more complex real-world patterns

## Troubleshooting

### **API Key Not Found**
```
System.InvalidOperationException: OPENAI_API_KEY not found
```
**Solution**: Set your environment variable and restart your terminal.

### **No Start Node Configured**
```
System.InvalidOperationException: No start node configured
```
**Solution**: Call `graph.SetStartNode()` with a valid node.

### **Nodes Not Connected**
```
System.InvalidOperationException: No next nodes found
```
**Solution**: Use `graph.Connect()` to link your nodes.

## Concepts and Techniques

This tutorial introduces several key concepts:

* **Graph**: A directed structure of nodes and edges that defines workflow execution
* **Node**: Individual units of work that can execute functions, make decisions, or perform operations
* **Edge**: Connections between nodes that define the sequence of execution
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

## Reference APIs

* **[IGraphExecutor](../api/core.md#igraph-executor)**: Core execution interface
* **[FunctionGraphNode](../api/nodes.md#function-graph-node)**: Function wrapper node
* **[GraphExecutor](../api/core.md#graph-executor)**: Main execution engine
* **[KernelBuilderExtensions](../api/extensions.md#kernel-builder-extensions)**: Graph support extensions
