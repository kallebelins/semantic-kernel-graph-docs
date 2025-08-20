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
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== My First Graph ===\n");

        try
        {
            // Step 1: Create and configure your kernel with graph support
            var kernel = CreateKernelWithGraphSupport();

            // Step 2: Create all the nodes for our graph workflow
            var (greetingNode, decisionNode, followUpNode) = CreateGraphNodes(kernel);

            // Step 3: Build and configure the complete graph
            var graph = BuildAndConfigureGraph(greetingNode, decisionNode, followUpNode);

            // Step 4: Execute the graph with sample input
            await ExecuteGraphWithSampleInputAsync(graph, kernel);

            Console.WriteLine("\n✅ Your first graph executed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error executing first graph: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates and configures a kernel with graph support enabled.
    /// This demonstrates the basic setup required for graph-based workflows.
    /// </summary>
    /// <returns>A configured kernel instance with graph support</returns>
    private static Kernel CreateKernelWithGraphSupport()
    {
        Console.WriteLine("Step 1: Creating kernel with graph support...");

        // Create a new kernel builder instance
        var builder = Kernel.CreateBuilder();

        // Add OpenAI chat completion service (you can replace with your preferred LLM)
        // Note: In a real application, you would use your actual API key
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrEmpty(apiKey))
        {
            builder.AddOpenAIChatCompletion("gpt-4", apiKey);
        }
        else
        {
            // Fallback to a mock function for demonstration purposes
            Console.WriteLine("⚠️  OPENAI_API_KEY not found. Using mock functions for demonstration.");
        }

        // Enable graph functionality with a single line - this registers all necessary services
        builder.AddGraphSupport();

        // Build the kernel with all configured services
        var kernel = builder.Build();

        Console.WriteLine("✅ Kernel created successfully with graph support enabled");
        return kernel;
    }

    /// <summary>
    /// Creates all the nodes needed for the graph workflow.
    /// Demonstrates different node types: function nodes and conditional nodes.
    /// </summary>
    /// <param name="kernel">The configured kernel instance</param>
    /// <returns>A tuple containing all created nodes</returns>
    private static (FunctionGraphNode greetingNode, ConditionalGraphNode decisionNode, FunctionGraphNode followUpNode) 
        CreateGraphNodes(Kernel kernel)
    {
        Console.WriteLine("Step 2: Creating graph nodes...");

        // Create a function node that generates personalized greetings
        // This node will process the input name and generate a friendly greeting
        var greetingNode = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod(
                (string name) => $"Hello {name}! It's wonderful to meet you today. I hope you're having a fantastic day filled with joy and positivity.",
                functionName: "GenerateGreeting",
                description: "Creates a personalized greeting message"
            ),
            "greeting_node"
        ).StoreResultAs("greeting");

        // Create a conditional node for decision making
        // This node evaluates whether the greeting is substantial enough to continue
        // Note: The condition function receives GraphState, not KernelArguments
        var decisionNode = new ConditionalGraphNode(
            (state) => state.ContainsValue("greeting") && 
                      state.GetValue<string>("greeting")?.Length > 20,
            "decision_node"
        );

        // Create a follow-up node that generates conversation continuations
        // This node only executes when the decision node allows it
        var followUpNode = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod(
                (string greeting) => $"Based on this greeting: '{greeting}', here's a follow-up question: What's something that's bringing you joy today, or is there a particular topic you'd like to explore together?",
                functionName: "GenerateFollowUp",
                description: "Creates engaging follow-up questions"
            ),
            "followup_node"
        ).StoreResultAs("output");

        Console.WriteLine("✅ All graph nodes created successfully");
        return (greetingNode, decisionNode, followUpNode);
    }

    /// <summary>
    /// Builds and configures the complete graph structure.
    /// This method demonstrates how to assemble nodes and define execution flow.
    /// </summary>
    /// <param name="greetingNode">The greeting generation node</param>
    /// <param name="decisionNode">The conditional decision node</param>
    /// <param name="followUpNode">The follow-up generation node</param>
    /// <returns>A fully configured graph executor</returns>
    private static GraphExecutor BuildAndConfigureGraph(
        FunctionGraphNode greetingNode,
        ConditionalGraphNode decisionNode,
        FunctionGraphNode followUpNode)
    {
        Console.WriteLine("Step 3: Building and configuring the graph...");

        // Create a new graph executor with a descriptive name and description
        var graph = new GraphExecutor(
            "MyFirstGraph",
            "A simple greeting workflow that demonstrates basic graph concepts"
        );

        // Add all nodes to the graph
        // Nodes must be added before they can be connected
        graph.AddNode(greetingNode);
        graph.AddNode(decisionNode);
        graph.AddNode(followUpNode);

        // Connect the nodes to define the execution flow
        // Start with the greeting node flowing to the decision node
        graph.Connect(greetingNode.NodeId, decisionNode.NodeId);

        // Connect decision node to follow-up node when condition is met
        // This creates a conditional edge that only executes when the greeting is substantial
        // Note: The condition function receives KernelArguments, not GraphState
        graph.ConnectWhen(decisionNode.NodeId, followUpNode.NodeId,
            args => args.ContainsKey("greeting") && 
                    args["greeting"]?.ToString()?.Length > 20);

        // Connect decision node to end when condition is not met
        // This creates an exit path for short greetings
        // Note: We don't need to explicitly connect to null - the graph will naturally end
        // when no more edges are available

        // Set the starting point of the graph
        // Execution always begins at this node
        graph.SetStartNode(greetingNode.NodeId);

        Console.WriteLine("✅ Graph built and configured successfully");
        return graph;
    }

    /// <summary>
    /// Executes the graph with sample input data.
    /// Demonstrates how to provide input and retrieve results from graph execution.
    /// </summary>
    /// <param name="graph">The configured graph executor</param>
    /// <param name="kernel">The kernel instance for execution</param>
    private static async Task ExecuteGraphWithSampleInputAsync(GraphExecutor graph, Kernel kernel)
    {
        Console.WriteLine("Step 4: Executing the graph...");

        // Create initial state with input data
        // This demonstrates how to pass data into your graph workflow
        var initialState = new KernelArguments { ["name"] = "Alice" };

        Console.WriteLine($"Input state: {{ \"name\": \"{initialState["name"]}\" }}");

        // Execute the graph with the initial state
        // The graph executor will traverse all nodes according to the defined flow
        var result = await graph.ExecuteAsync(kernel, initialState);

        // Display the execution results
        Console.WriteLine("\n=== Execution Results ===");
        
        // Extract and display the greeting result from the final state
        // The result is a FunctionResult, but the actual data is stored in the arguments
        var greeting = initialState.GetValueOrDefault("greeting", "No greeting generated");
        Console.WriteLine($"Greeting: {greeting}");

        // Check if follow-up was generated (depends on conditional execution)
        if (initialState.ContainsKey("output"))
        {
            Console.WriteLine($"Follow-up: {initialState["output"]}");
        }
        else
        {
            Console.WriteLine("Follow-up: Not generated (greeting was too short)");
        }

        // Display the complete final state for analysis
        Console.WriteLine("\n=== Complete Final State ===");
        foreach (var kvp in initialState)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }
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

Step 1: Creating kernel with graph support...
⚠️  OPENAI_API_KEY not found. Using mock functions for demonstration.
✅ Kernel created successfully with graph support enabled
Step 2: Creating graph nodes...
✅ All graph nodes created successfully
Step 3: Building and configuring the graph...
✅ Graph built and configured successfully
Step 4: Executing the graph...
Input state: { "name": "Alice" }

=== Execution Results ===
Greeting: Hello Alice! It's wonderful to meet you today. I hope you're having a fantastic day filled with joy and positivity.

Follow-up: Based on this greeting: 'Hello Alice! It's wonderful to meet you today. I hope you're having a fantastic day filled with joy and positivity.', here's a follow-up question: What's something that's bringing you joy today, or is there a particular topic you'd like to explore together?

=== Complete Final State ===
  name: Alice
  greeting: Hello Alice! It's wonderful to meet you today. I hope you're having a fantastic day filled with joy and positivity.
  output: Based on this greeting: 'Hello Alice! It's wonderful to meet you today. I hope you're having a fantastic day filled with joy and positivity.', here's a follow-up question: What's something that's bringing you joy today, or is there a particular topic you'd like to explore together?

✅ Your first graph executed successfully!
```

## Understanding What Happened

Let's break down what your graph accomplished:

### 1. **Modular Code Structure**
The code is now organized into clear, focused methods:
- `CreateKernelWithGraphSupport()`: Sets up the kernel with graph capabilities
- `CreateGraphNodes()`: Creates all the nodes needed for the workflow
- `BuildAndConfigureGraph()`: Assembles the graph structure and connections
- `ExecuteGraphWithSampleInputAsync()`: Runs the graph and displays results

### 2. **Kernel Creation and Graph Support**
```csharp
var builder = Kernel.CreateBuilder();
builder.AddGraphSupport(); // This enables all graph functionality
```

The `AddGraphSupport()` extension method registers all the necessary services for graph execution, including:
* Graph executor factory
* Node converters
* State management
* Error handling policies

### 3. **Function Graph Node with Result Storage**
```csharp
var greetingNode = new FunctionGraphNode(
    kernel.CreateFunctionFromMethod(
        (string name) => $"Hello {name}! It's wonderful to meet you today...",
        functionName: "GenerateGreeting",
        description: "Creates a personalized greeting message"
    ),
    "greeting_node"
).StoreResultAs("greeting");
```

This creates a node that:
* Wraps a Semantic Kernel function (using `CreateFunctionFromMethod` for reliability)
* Can be connected to other nodes
* Automatically stores its result in the graph state using `StoreResultAs("greeting")`
* Has a descriptive name and description for better debugging

### 4. **Conditional Node with GraphState**
```csharp
var decisionNode = new ConditionalGraphNode(
    (state) => state.ContainsValue("greeting") && 
              state.GetValue<string>("greeting")?.Length > 20,
    "decision_node"
);
```

This node:
* Evaluates a condition based on the current `GraphState`
* Uses `ContainsValue()` and `GetValue<T>()` methods for type-safe state access
* Routes execution to different paths based on the result
* Enables dynamic workflow behavior

### 5. **Graph Assembly with Clear Connections**
```csharp
graph.AddNode(greetingNode);
graph.AddNode(decisionNode);
graph.AddNode(followUpNode);

// Connect nodes using their IDs
graph.Connect(greetingNode.NodeId, decisionNode.NodeId);
graph.ConnectWhen(decisionNode.NodeId, followUpNode.NodeId,
    args => args.ContainsKey("greeting") && 
            args["greeting"]?.ToString()?.Length > 20);
```

You add nodes to the graph, then connect them using their `NodeId` properties to define the flow.

### 6. **Conditional Routing with ConnectWhen**
```csharp
graph.ConnectWhen(decisionNode.NodeId, followUpNode.NodeId,
    args => args.ContainsKey("greeting") && 
            args["greeting"]?.ToString()?.Length > 20);
```

The `ConnectWhen` method creates conditional edges that:
* Only execute when the condition evaluates to `true`
* Receive `KernelArguments` for condition evaluation
* Enable dynamic workflow behavior based on runtime state

### 7. **Execution and State Management**
```csharp
var result = await graph.ExecuteAsync(kernel, initialState);
var greeting = initialState.GetValueOrDefault("greeting", "No greeting generated");
```

The graph executor:
* Traverses the graph from the start node
* Executes each node in sequence
* Manages state transitions between nodes
* Stores results in the `KernelArguments` for easy access
* Returns a `FunctionResult` representing the final execution outcome

## Key Concepts Demonstrated

### **Modular Code Organization**
* **Separation of Concerns**: Each method has a single, clear responsibility
* **Reusability**: Methods can be easily modified or extended
* **Maintainability**: Code is easier to read, test, and debug
* **Best Practices**: Follows C# coding standards and patterns

### **State Management with GraphState**
* **Input state**: `{ "name": "Alice" }`
* **Intermediate state**: `{ "name": "Alice", "greeting": "Hello Alice!..." }`
* **Final state**: Contains both input and generated content with proper metadata
* **Type Safety**: Using `GetValue<T>()` and `ContainsValue()` methods

### **Conditional Execution with GraphState**
* The decision node evaluates the greeting length using `GraphState` methods
* Only executes the follow-up if the greeting is substantial (> 20 characters)
* Demonstrates dynamic workflow behavior based on runtime conditions
* Shows the difference between `GraphState` (for node conditions) and `KernelArguments` (for edge conditions)

### **Result Storage and Retrieval**
* **StoreResultAs**: Automatically stores node results in the graph state
* **State Access**: Results are accessible through the `KernelArguments` after execution
* **Metadata Tracking**: Execution context and timing information is automatically captured
* **Debugging Support**: Rich state information for troubleshooting

### **Node Types and Their Roles**
* **FunctionGraphNode**: Executes functions and stores results
* **ConditionalGraphNode**: Makes routing decisions based on state
* **GraphExecutor**: Orchestrates the entire workflow and manages execution flow

## Experiment and Customize

Try these modifications to learn more:

### **Change the Input**
```csharp
var state = new KernelArguments { ["name"] = "Bob" };
```

### **Modify the Condition**
```csharp
var decisionNode = new ConditionalGraphNode(
    (state) => state.ContainsValue("greeting") && 
              state.GetValue<string>("greeting")?.Contains("wonderful"),
    "decision_node"
);
```

### **Add More Nodes**
```csharp
var summaryNode = new FunctionGraphNode(
    kernel.CreateFunctionFromMethod(
        (string greeting, string followup) => 
            $"Summary: Greeting was '{greeting}' and follow-up was '{followup}'",
        functionName: "GenerateSummary",
        description: "Creates a conversation summary"
    ),
    "summary_node"
).StoreResultAs("summary");

graph.AddNode(summaryNode);
graph.Connect(followUpNode.NodeId, summaryNode.NodeId);
```

### **Test Different Scenarios**
The example includes a built-in experimentation method that you can call:

```csharp
// Test with different names and see how the conditional logic behaves
await RunExperimentationExamplesAsync();
```

This will test the graph with various inputs and demonstrate how the conditional routing works in different scenarios.

## Common Issues and Solutions

### **API Key Not Found**
```
⚠️  OPENAI_API_KEY not found. Using mock functions for demonstration.
```
**Solution**: The example will work with mock functions, but for real LLM functionality, ensure your environment variable is set correctly and restart your terminal.

### **Graph Execution Fails**
```
System.InvalidOperationException: No start node configured
```
**Solution**: Make sure you've called `graph.SetStartNode(nodeId)` with a valid node ID, not the node object itself.

### **Nodes Not Connected**
```
System.InvalidOperationException: No next nodes found for node 'NodeName'
```
**Solution**: Verify all nodes are properly connected using `graph.Connect(sourceNodeId, targetNodeId)` and `graph.ConnectWhen(sourceNodeId, targetNodeId, condition)`.

### **Results Not Stored in State**
```
Greeting: No greeting generated
```
**Solution**: Ensure you've called `.StoreResultAs("keyName")` on your FunctionGraphNode instances to specify where results should be stored.

### **Conditional Node Condition Fails**
```
System.InvalidOperationException: Condition evaluation failed
```
**Solution**: Make sure your conditional node's condition function receives `GraphState` and uses methods like `ContainsValue()` and `GetValue<T>()` instead of dictionary-style access.

### **Type Conversion Errors**
```
System.InvalidCastException: Unable to cast object of type 'System.String' to type 'System.Int32'
```
**Solution**: Use the generic `GetValue<T>()` method for type-safe state access: `state.GetValue<string>("keyName")` instead of `(string)state["keyName"]`.

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
* **Modular Design**: Code organization that separates concerns and improves maintainability
* **Result Storage**: Automatic storage of node execution results in the graph state
* **Type Safety**: Safe access to state values using generic methods and proper type checking
* **Conditional Routing**: Dynamic workflow paths based on runtime state evaluation
* **Error Handling**: Comprehensive error handling with try-catch blocks and user-friendly messages

## Prerequisites and Minimum Configuration

To complete this tutorial, you need:
* **.NET 8.0+** runtime and SDK
* **SemanticKernel.Graph** package installed
* **LLM Provider** configured with valid API keys (optional - example works with mock functions)
* **Environment Variables** set up for your API credentials (optional)

### **Running the Example**

The complete working example is available in the `examples` folder:

```bash
# Navigate to the examples directory
cd semantic-kernel-graph-docs/examples

# Run the first graph example
dotnet run -- first-graph

# Run all examples
dotnet run -- all
```

### **Example Features**

The example demonstrates:
* **Modular Code Structure**: Clear separation of concerns with focused methods
* **Mock Function Support**: Works without LLM API keys for demonstration
* **Comprehensive Error Handling**: Try-catch blocks with informative error messages
* **Step-by-Step Progress**: Visual feedback during execution
* **Experimentation Mode**: Built-in testing with different inputs
* **State Visualization**: Complete view of the final execution state

## See Also

* **[Installation Guide](installation.md)**: Set up SemanticKernel.Graph in your project
* **[Core Concepts](concepts/index.md)**: Understanding graphs, nodes, and execution
* **[How-to Guides](how-to/build-a-graph.md)**: Building more complex graph workflows
* **[API Reference](api/core.md)**: Complete API documentation
* **[Examples](examples/index.md)**: More real-world graph patterns and use cases
* **[State Management](concepts/state.md)**: Working with graph state and data flow
* **[Conditional Nodes](how-to/conditional-nodes.md)**: Advanced conditional logic and routing
* **[Running Examples](running-examples.md)**: How to execute and test the provided examples
