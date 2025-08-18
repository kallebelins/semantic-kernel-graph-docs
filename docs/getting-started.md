# Getting Started with SemanticKernel.Graph

Welcome to SemanticKernel.Graph, a powerful .NET library that brings the flexibility and power of graph-based workflows to Microsoft's Semantic Kernel ecosystem. This guide will help you understand what SemanticKernel.Graph is, the problems it solves, and how it relates to both Semantic Kernel and LangGraph.

## What is SemanticKernel.Graph?

SemanticKernel.Graph is a production-ready .NET library that enables you to build complex, stateful AI workflows using directed graphs. It extends Microsoft's Semantic Kernel with advanced orchestration capabilities, allowing you to create sophisticated AI agents, multi-step reasoning workflows, and intelligent automation systems.

### Key Capabilities

- **Graph-Based Workflows**: Design complex AI workflows using nodes and conditional edges
- **Native Semantic Kernel Integration**: Seamlessly use existing SK plugins, memory, and connectors
- **State Management**: Maintain context and state across workflow execution
- **Conditional Logic**: Implement dynamic routing based on AI responses and state
- **Streaming Execution**: Real-time monitoring and event streaming
- **Checkpointing & Recovery**: Resume workflows from any point with state persistence
- **Visualization & Debugging**: Built-in tools for understanding and troubleshooting workflows
- **Multi-Agent Coordination**: Orchestrate multiple AI agents in complex scenarios

## Problems SemanticKernel.Graph Solves

### 1. **Complex AI Workflow Orchestration**
Traditional AI applications often execute functions sequentially without the ability to make decisions or handle complex branching logic. SemanticKernel.Graph enables you to create intelligent workflows that can:
- Make decisions based on AI responses
- Handle multiple execution paths
- Implement retry logic and error recovery
- Coordinate multiple AI agents

### 2. **State Management Across AI Interactions**
Maintaining context across multiple AI function calls is challenging. SemanticKernel.Graph provides:
- Persistent state management with versioning
- Automatic state serialization and checkpointing
- State validation and integrity checks
- Efficient state sharing between workflow steps

### 3. **Dynamic AI Agent Behavior**
Creating AI agents that can adapt their behavior based on context requires sophisticated orchestration. With SemanticKernel.Graph, you can:
- Implement ReAct (Reasoning + Acting) patterns
- Create chain-of-thought reasoning workflows
- Build agents that learn from previous interactions
- Handle human-in-the-loop scenarios

### 4. **Production-Ready AI Workflows**
Building enterprise-grade AI applications requires robust error handling, monitoring, and scalability. SemanticKernel.Graph provides:
- Comprehensive error handling and recovery
- Performance metrics and observability
- Resource governance and rate limiting
- Distributed execution capabilities

## Relationship with Semantic Kernel

SemanticKernel.Graph is built **on top of** Semantic Kernel, not as a replacement. It extends the existing SK ecosystem with graph orchestration capabilities while maintaining full compatibility with:

### **Existing SK Components**
- **Plugins**: All existing SK plugins work as graph nodes
- **Memory**: Vector and semantic memory systems integrate seamlessly
- **Connectors**: OpenAI, Azure OpenAI, and other LLM providers work unchanged
- **Templates**: Handlebars and other template systems remain fully functional
- **Logging**: SK's logging infrastructure is enhanced with graph-specific context

### **Zero-Configuration Setup**
```csharp
var builder = Kernel.CreateBuilder();
// Your existing SK configuration
builder.AddOpenAIChatCompletion("gpt-4", "your-api-key");

// Add graph support with one line
builder.AddGraphSupport();

var kernel = builder.Build();
```

### **Enhanced Capabilities**
SemanticKernel.Graph adds graph-specific features while preserving SK's simplicity:
- **Graph State**: Enhanced `KernelArguments` with versioning and validation
- **Node Types**: Specialized nodes for conditional logic, loops, and error handling
- **Execution Engine**: Advanced orchestration with checkpointing and recovery
- **Observability**: Enhanced logging and metrics for graph execution

## Parity with LangGraph

SemanticKernel.Graph provides feature parity with LangGraph while maintaining .NET-native design patterns and integration with the Microsoft ecosystem.

### **Core Concepts Alignment**
| LangGraph Concept | SemanticKernel.Graph Equivalent | Status |
|------------------|----------------------------------|---------|
| State Management | `GraphState` + `KernelArguments` | ✅ Full |
| Conditional Edges | `ConditionalEdge` + `ConditionalGraphNode` | ✅ Full |
| Function Nodes | `FunctionGraphNode` | ✅ Full |
| Loops | `LoopGraphNode` + `ReActLoopGraphNode` | ✅ Full |
| Human-in-the-Loop | `HumanApprovalGraphNode` | ✅ Full |
| Streaming | `IStreamingGraphExecutor` | ✅ Full |
| Checkpointing | `CheckpointManager` + `StateHelpers` | ✅ Full |
| Multi-Agent | `MultiAgentCoordinator` | ✅ Full |

### **Advanced Features**
- **ReAct Pattern**: Full implementation with reasoning and acting cycles
- **Chain-of-Thought**: Structured reasoning workflows with validation
- **Dynamic Routing**: Context-aware node selection
- **Error Recovery**: Comprehensive error handling and retry mechanisms
- **Performance Optimization**: Built-in profiling and optimization tools

### **.NET-Specific Advantages**
- **Native Performance**: Optimized for .NET runtime and memory management
- **Enterprise Integration**: Built-in support for Azure services and enterprise patterns
- **Type Safety**: Strong typing throughout the graph execution pipeline
- **Async/Await**: Native support for asynchronous operations and streaming
- **Dependency Injection**: Seamless integration with .NET DI containers

## When to Use SemanticKernel.Graph

### **Perfect For**
- **AI Agents**: Building intelligent agents that can reason and act
- **Workflow Automation**: Complex multi-step AI processes
- **Decision Systems**: AI-powered decision trees and routing logic
- **Multi-Agent Coordination**: Orchestrating multiple AI agents
- **Human-AI Collaboration**: Workflows requiring human approval or input
- **Production AI Applications**: Enterprise-grade AI systems with monitoring and recovery

### **Consider Alternatives When**
- **Simple Function Calls**: Basic SK function execution is sufficient
- **Static Workflows**: No conditional logic or dynamic behavior needed
- **Minimal State Management**: Simple stateless AI interactions
- **Resource Constraints**: Very limited memory or processing resources

## Quick Start

Ready to get started? Here's how to create your first graph in under 5 minutes:

```csharp
using Microsoft.SemanticKernel;
using SemanticKernel.Graph;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

// 1. Create and configure your kernel
var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion("gpt-4", "your-api-key");
builder.AddGraphSupport(); // Enable graph functionality
var kernel = builder.Build();

// 2. Create a simple function node
var echoNode = new FunctionGraphNode(
    kernel.CreateFunctionFromPrompt("Echo: {{$input}}")
);

// 3. Create and execute a graph
var graph = new GraphExecutor("MyFirstGraph");
graph.AddNode(echoNode);
graph.SetStartNode(echoNode);

var state = new KernelArguments { ["input"] = "Hello, World!" };
var result = await graph.ExecuteAsync(state);

Console.WriteLine(result["output"]); // Output: Echo: Hello, World!
```

## Next Steps

Now that you understand what SemanticKernel.Graph is and how it fits into your AI development workflow, explore these resources:

- **[Installation Guide](installation.md)**: Set up SemanticKernel.Graph in your project
- **[First Graph Tutorial](first-graph.md)**: Build your first complete graph workflow
- **[Core Concepts](concepts/index.md)**: Understand graphs, nodes, state, and execution
- **[Examples](examples/index.md)**: See real-world usage patterns and implementations
- **[API Reference](api/core.md)**: Explore the complete API surface

## Concepts and Techniques

This overview introduces several key concepts that you'll use throughout SemanticKernel.Graph:

- **Graph**: A directed graph structure composed of nodes and edges that defines the flow of execution
- **Node**: Individual units of work that can execute AI functions, make decisions, or perform other operations
- **Edge**: Connections between nodes that can include conditional logic for dynamic routing
- **State**: Persistent data that flows through the graph, maintaining context across execution steps
- **Execution**: The process of traversing the graph, executing nodes, and managing state transitions

## Prerequisites and Minimum Configuration

To use SemanticKernel.Graph, you need:
- **.NET 8.0** or later
- **Semantic Kernel** package (latest stable version)
- **LLM Provider** (OpenAI, Azure OpenAI, or other supported providers)
- **API Keys** for your chosen LLM provider

## Troubleshooting Quick

**Common Issues:**
- **Package not found**: Ensure you're using the correct NuGet package name
- **Kernel configuration errors**: Verify your LLM provider is properly configured
- **Graph execution failures**: Check that all nodes are properly connected and configured

For more detailed troubleshooting, see the [Troubleshooting Guide](troubleshooting.md).
