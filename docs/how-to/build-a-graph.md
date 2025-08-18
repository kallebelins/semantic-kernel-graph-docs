# How to build a graph

This guide explains the fundamental steps for creating and configuring graphs in SemanticKernel.Graph. You'll learn how to define nodes, connect them with conditional edges, and execute the resulting workflow.

## Overview

Building a graph involves several key steps:

1. **Create and configure a `Kernel`** with the necessary plugins and functions
2. **Define nodes** (functions, conditionals, loops) that represent your workflow steps
3. **Connect nodes** with conditional edges that control execution flow
4. **Execute** with `GraphExecutor` to run the complete workflow

## Step-by-Step Process

### 1. Create and Configure Kernel

Start by creating a Semantic Kernel instance and adding the required plugins:

```csharp
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;

var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion("gpt-4", "your-api-key");
builder.AddPluginFromObject(new PlannerPlugin(), "Planner");
builder.AddPluginFromObject(new AnalyzerPlugin(), "Analyzer");
builder.AddPluginFromObject(new ExecutorPlugin(), "Executor");

var kernel = builder.Build();
```

### 2. Define Graph Nodes

Create nodes that represent different steps in your workflow:

```csharp
// Function nodes that execute kernel functions
var planNode = new FunctionGraphNode(
    kernel.GetFunction("Planner", "Plan"), 
    nodeId: "plan"
);

var analyzeNode = new FunctionGraphNode(
    kernel.GetFunction("Analyzer", "Analyze"), 
    nodeId: "analyze"
);

var actNode = new FunctionGraphNode(
    kernel.GetFunction("Executor", "Act"), 
    nodeId: "act"
);

// Conditional node for branching logic
var decisionNode = new ConditionalGraphNode(
    predicate: state => state.GetString("needs_analysis") == "yes",
    nodeId: "decision"
);
```

### 3. Connect Nodes with Edges

Define the flow between nodes using conditional edges:

```csharp
var graph = new GraphExecutor("workflow-graph", "Complete workflow with analysis");

// Add nodes to the graph
graph.AddNode(planNode)
     .AddNode(decisionNode)
     .AddNode(analyzeNode)
     .AddNode(actNode);

// Connect nodes with conditional logic
graph.AddEdge(planNode, decisionNode)
     .AddConditionalEdge(decisionNode, analyzeNode, 
         condition: state => state.GetString("needs_analysis") == "yes")
     .AddConditionalEdge(decisionNode, actNode, 
         condition: state => state.GetString("needs_analysis") != "yes")
     .AddEdge(analyzeNode, actNode);

// Set the starting point
graph.SetStartNode("plan");
```

### 4. Execute the Graph

Run the complete workflow:

```csharp
// Prepare execution arguments
var arguments = new KernelArguments
{
    ["input"] = "Process this data",
    ["needs_analysis"] = "yes"
};

// Execute the graph
var executor = new GraphExecutor(graph);
var result = await executor.ExecuteAsync(kernel, arguments);

Console.WriteLine($"Result: {result.GetValue<string>()}");
```

## Alternative Builder Pattern

For simpler graphs, you can use the fluent builder pattern:

```csharp
var graph = GraphBuilder.Create()
    .AddFunctionNode("plan", kernel, "Planner", "Plan")
    .When(state => state.GetString("needs_analysis") == "yes")
        .AddFunctionNode("analyze", kernel, "Analyzer", "Analyze")
    .AddFunctionNode("act", kernel, "Executor", "Act")
    .Build();
```

## Advanced Patterns

### Conditional Execution

Use conditional edges to create complex branching logic:

```csharp
graph.AddConditionalEdge("start", "branch_a", 
    condition: state => state.GetInt("priority") > 5)
.AddConditionalEdge("start", "branch_b", 
    condition: state => state.GetInt("priority") <= 5);
```

### Loop Control

Implement loops with iteration limits:

```csharp
var loopNode = new WhileGraphNode(
    condition: state => state.GetInt("attempt") < 3,
    maxIterations: 5,
    nodeId: "retry_loop"
);

graph.AddNode(loopNode)
     .AddEdge("start", "retry_loop")
     .AddEdge("retry_loop", "process");
```

### Error Handling

Add error handling nodes to your workflow:

```csharp
var errorHandler = new ErrorHandlerGraphNode(
    errorTypes: new[] { ErrorType.Transient, ErrorType.External },
    recoveryAction: RecoveryAction.Retry,
    maxRetries: 3,
    nodeId: "error_handler"
);

graph.AddNode(errorHandler)
     .AddEdge("process", "error_handler")
     .AddEdge("error_handler", "fallback");
```

## Best Practices

### Node Design

1. **Single Responsibility**: Each node should have one clear purpose
2. **Meaningful Names**: Use descriptive node IDs that explain their function
3. **State Management**: Design nodes to work with the graph state effectively
4. **Error Handling**: Include error handling nodes for robust workflows

### Graph Structure

1. **Logical Flow**: Organize nodes in a logical sequence
2. **Conditional Logic**: Use conditional edges for dynamic routing
3. **Loop Prevention**: Set appropriate iteration limits to prevent infinite loops
4. **Start Node**: Always define a clear starting point

### Performance Considerations

1. **Node Efficiency**: Optimize individual node performance
2. **State Size**: Keep graph state manageable for memory efficiency
3. **Parallel Execution**: Use parallel nodes where possible for better performance
4. **Caching**: Implement caching for expensive operations

## Troubleshooting

### Common Issues

**Graph not executing**: Ensure you've set a start node with `SetStartNode()`

**Nodes not connected**: Verify all edges are properly defined with `AddEdge()` or `AddConditionalEdge()`

**Infinite loops**: Check loop conditions and set appropriate `maxIterations`

**State not persisting**: Use `GraphState` for persistent state across nodes

### Debugging Tips

1. **Enable logging** to trace execution flow
2. **Use breakpoints** in conditional logic
3. **Inspect state** at each node execution
4. **Validate graph integrity** before execution

## Concepts and Techniques

**GraphExecutor**: The main class responsible for executing graph workflows. It manages node execution order, state transitions, and error handling throughout the workflow lifecycle.

**FunctionGraphNode**: A graph node that wraps and executes Semantic Kernel functions. It handles input/output mapping between the graph state and the underlying kernel function.

**ConditionalGraphNode**: A node that evaluates predicates to determine execution flow. It enables dynamic routing based on the current state of the graph.

**ConditionalEdge**: A connection between nodes that includes a condition for execution. It allows for complex branching logic and dynamic workflow paths.

**GraphState**: A wrapper around KernelArguments that provides additional metadata, execution history, and validation capabilities for graph workflows.

## See Also

- [First Graph in 5 Minutes](../first-graph-5-minutes.md) - Quick start guide for building your first graph
- [Conditional Nodes](conditional-nodes.md) - Learn about branching and conditional execution
- [Loops](loops.md) - Implement iterative workflows with loop nodes
- [State Management](../state-quickstart.md) - Understand how to manage data flow between nodes
- [Graph Execution](../concepts/execution.md) - Learn about the execution lifecycle and flow control
- [Examples: Basic Graph Building](../examples/basic-graph-example.md) - Complete working examples of graph construction
