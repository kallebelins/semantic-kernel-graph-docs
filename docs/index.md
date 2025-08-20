# Semantic Kernel Graph

Welcome to the SemanticKernel.Graph documentation. This site mirrors the LangGraph documentation structure, focusing on a lean and pragmatic .NET implementation that's fully integrated with Semantic Kernel.

## Concepts and Techniques

**SemanticKernel.Graph**: Extension of Semantic Kernel that adds computational graph execution capabilities, allowing you to create complex workflows with intelligent orchestration.

**Computational Graphs**: Structures that represent workflows through nodes connected by edges, with controlled execution and conditional routing.

**Native Integration**: Works as an extension of the existing Semantic Kernel, maintaining full compatibility and leveraging existing plugins and services.

## What SemanticKernel.Graph Solves

### Orchestration Problems
- **Complex Workflows**: Creation of AI pipelines with multiple steps
- **Intelligent Routing**: Decisions based on state and context
- **Flow Control**: Loops, conditionals and controlled iterations
- **Composition**: Reuse of components and subgraphs

### Production Challenges
- **Scalability**: Parallel and distributed execution
- **Resilience**: Checkpointing, retry and circuit breakers
- **Observability**: Metrics, logging and real-time visualization
- **Maintainability**: Debug, inspection and automatic documentation

## Core Features

### **Graph Execution**
- Function, conditional, reasoning and loop nodes
- Edges with conditions and dynamic routing
- Sequential, parallel and distributed execution
- Deterministic scheduler for reproducibility

### **Streaming and Events**
- Streaming execution with real-time events
- Automatic reconnection and backpressure control
- Asynchronous consumption of execution events
- Integration with messaging systems

### **State and Persistence**
- Typed and validated state system
- Automatic and manual checkpointing
- State serialization and compression
- Execution recovery and replay

### **Intelligent Routing**
- State-based conditional routing
- Dynamic and adaptive strategies
- Semantic similarity for decisions
- Learning from feedback

### **Human-in-the-Loop**
- Human approval nodes
- Multiple channels (console, web, email)
- Timeouts and SLA policies
- Audit and decision tracking

### **Integration and Extensibility**
- Integrated REST tools
- Extensible plugin system
- Integration with external services
- Templates for common workflows

## Get Started in Minutes

### 1. **Quick Installation**
```bash
dotnet add package SemanticKernel.Graph
```

### 2. **First Graph**
```csharp
builder.AddGraphSupport();
var kernel = builder.Build();

var graph = new Graph
{
    StartNode = startNode,
    Nodes = new[] { startNode, processNode, endNode }
};

var result = await kernel.GetRequiredService<IGraphExecutor>()
    .ExecuteAsync(graph, arguments);
```

## Documentation Structure

### **Get Started**
- [Installation](./installation.md) - Setup and requirements
- [First Graph](./first-graph-5-minutes.md) - Hello World in 5 minutes
- [Quickstarts](./index.md#quickstarts) - Quick guides by functionality

### **Concepts**
- [Graphs](./concepts/graphs.md) - Structure and components
- [Nodes](./concepts/nodes.md) - Types and lifecycle
- [Execution](./concepts/execution.md) - Modes and control
- [Routing](./concepts/routing.md) - Strategies and conditions
- [State](./concepts/state.md) - Management and persistence

### **How-To Guides**
- [Building Graphs](./how-to/build-a-graph.md) - Creation and validation
- [Conditional Nodes](./how-to/conditional-nodes.md) - Dynamic routing
- [Checkpointing](./how-to/checkpointing.md) - Persistence and recovery
- [Streaming](./how-to/streaming.md) - Real-time execution
- [Metrics](./how-to/metrics-and-observability.md) - Monitoring

### **Reference**
- [APIs](./api/index.md) - Complete API documentation
- [Configuration](./api/configuration.md) - Options and parameters
- [Types](./api/types.md) - Data structures
- [Extensions](./api/extensions.md) - Extension methods

### **Examples**
- [Index](./examples/index.md) - All available examples
- [Chatbot](./examples/chatbot.md) - Conversation with memory
- [ReAct](./examples/react-agent.md) - Reasoning and action
- [Multi-Agent](./examples/multi-agent.md) - Agent coordination
- [Documents](./examples/document-analysis-pipeline.md) - Document analysis

## Use Cases

### **AI Agents**
- Chatbots with memory and context
- Reasoning agents (ReAct, Chain of Thought)
- Coordination of multiple agents
- Automated decision workflows

### **Document Processing**
- Automatic analysis and classification
- Structured information extraction
- Validation and approval pipelines
- Batch processing with checkpoints

### **Recommendation Systems**
- Similarity-based routing
- Learning from user feedback
- Conditional filters and personalization
- Continuous result optimization

### **Microservice Orchestration**
- API call coordination
- Circuit breakers and retry policies
- Intelligent load balancing
- Monitoring and observability

## Comparison with Alternatives

| Feature | SemanticKernel.Graph | LangGraph | Temporal | Durable Functions |
|---------|----------------------|-----------|----------|-------------------|
| **SK Integration** | ✅ Native | ❌ Python | ❌ Java/Go | ❌ Azure |
| **Performance** | ✅ Native .NET | ⚠️ Python | ✅ JVM | ✅ Azure Runtime |
| **Checkpointing** | ✅ Advanced | ✅ Basic | ✅ Robust | ✅ Native |
| **Streaming** | ✅ Events | ✅ Streaming | ❌ | ⚠️ Limited |
| **Visualization** | ✅ Real Time | ✅ Static | ❌ | ❌ |
| **HITL** | ✅ Multiple Channels | ⚠️ Basic | ❌ | ❌ |

## Community and Support

### **Contribute**
- [GitHub Repository](https://github.com/kallebelins/semantic-kernel-graph-docs)
- [Issues](https://github.com/kallebelins/semantic-kernel-graph-docs/issues)
- [Discussions](https://github.com/kallebelins/semantic-kernel-graph-docs/discussions)
- [Contributing Guide](https://github.com/kallebelins/semantic-kernel-graph-docs/CONTRIBUTING.md)

### **Additional Resources**
- [LinkedIn](https://www.linkedin.com/company/skgraph-dev)

### **Need Help?**
- [FAQ](./faq.md) - Frequently asked questions
- [Troubleshooting](./troubleshooting.md) - Problem resolution
- [Examples](./examples/index.md) - Practical examples
- [API Reference](./api/index.md) - Technical documentation

## Quickstarts

### **5 Minutes**
- [First Graph](./first-graph-5-minutes.md) - Basic Hello World
- [State](./state-quickstart.md) - Variable management
- [Conditionals](./conditional-nodes-quickstart.md) - Simple routing
- [Streaming](./streaming-quickstart.md) - Real-time events

### **15 Minutes**
- [Checkpointing](./checkpointing-quickstart.md) - State persistence
- [Metrics](./metrics-logging-quickstart.md) - Basic monitoring
- [ReAct/CoT](./react-cot-quickstart.md) - Reasoning patterns

### **30 Minutes**
- [Conditionals Tutorial](./conditional-nodes-tutorial.md) - Advanced routing
- [State Tutorial](./state-tutorial.md) - Complex management
- [Multi-Agent](./examples/multi-agent.md) - Agent coordination

---

> **Tip**: This documentation uses Material for MkDocs. Use the left navigation and search bar to quickly find topics.

> **Ready to get started?** Go to [Installation](./installation.md) or [First Graph](./first-graph-5-minutes.md) to begin in minutes!
