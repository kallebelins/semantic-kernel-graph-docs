# Examples Index

This section provides a comprehensive overview of all available examples in the Semantic Kernel Graph package. Each example demonstrates specific capabilities and patterns that you can use as a reference for building your own graph-based workflows.

## Quick Start

The examples in this documentation provide comprehensive demonstrations of the Semantic Kernel Graph package capabilities. Each example includes:

- **Complete code snippets** that you can copy and adapt
- **Step-by-step explanations** of how the code works
- **Configuration examples** for different scenarios
- **Best practices** and usage patterns

Browse the examples by category below to find the right starting point for your use case.

## Example Categories

### 🔧 Core Graph Patterns

Examples demonstrating fundamental graph concepts and execution patterns.

| Example | Description | Key Features |
|---------|-------------|--------------|
| [Chain of Thought](./chain-of-thought.md) | Step-by-step reasoning with validation and backtracking | `ChainOfThoughtGraphNode`, reasoning types, confidence scoring |
| [Conditional Nodes](./conditional-nodes.md) | Dynamic routing based on conditions and state | `ConditionalGraphNode`, `ConditionalEdge`, template-based routing |
| [Loop Nodes](./loop-nodes.md) | Controlled iteration with exit conditions | Loop control, state management, iteration limits |
| [Subgraphs](./subgraphs.md) | Modular graph composition and isolation | Isolated execution, scoped state, graph composition |

### 🤖 Agent Patterns

Examples showing different agent architectures and coordination strategies.

| Example | Description | Key Features |
|---------|-------------|--------------|
| [ReAct Agent](./react-agent.md) | Reasoning and action loops with tool selection | `ReActLoopGraphNode`, tool integration, action validation |
| [ReAct Problem Solving](./react-problem-solving.md) | Complex problem decomposition and solution | Multi-step reasoning, tool orchestration, solution validation |
| [Memory Agent](./memory-agent.md) | Persistent memory across conversations | Memory integration, context persistence, conversation history |
| [Retrieval Agent](./retrieval-agent.md) | Information retrieval and synthesis | RAG patterns, document processing, knowledge synthesis |
| [Multi-Agent](./multi-agent.md) | Coordinated multi-agent workflows | `MultiAgentCoordinator`, parallel execution, result aggregation |

### 🔄 Advanced Workflows

Examples demonstrating sophisticated workflow patterns and integrations.

| Example | Description | Key Features |
|---------|-------------|--------------|
| [Advanced Patterns](./advanced-patterns.md) | Complex workflow compositions | Pattern combinations, advanced routing, workflow orchestration |
| [Advanced Routing](./advanced-routing.md) | Dynamic routing with semantic similarity | Embedding-based routing, probabilistic decisions, context-aware routing |
| [Dynamic Routing](./dynamic-routing.md) | Runtime routing decisions | Adaptive routing, performance-based decisions, fallback strategies |
| [Document Analysis Pipeline](./document-analysis-pipeline.md) | Multi-stage document processing | Pipeline orchestration, content extraction, analysis workflows |
| [Multi-Hop RAG with Retry](./multi-hop-rag-retry.md) | Resilient information retrieval | Retry policies, circuit breakers, checkpoint recovery |

### 💾 State and Persistence

Examples focusing on state management, checkpointing, and data persistence.

| Example | Description | Key Features |
|---------|-------------|--------------|
| [State Management](./state-management.md) | Graph state and argument handling | `GraphState`, `KernelArguments`, state validation |
| [Checkpointing](./checkpointing.md) | Execution state persistence and recovery | `CheckpointManager`, state serialization, recovery workflows |
| [Streaming Execution](./streaming-execution.md) | Real-time execution monitoring | `StreamingGraphExecutor`, event streams, real-time updates |

### 📊 Observability and Debugging

Examples demonstrating monitoring, metrics, and debugging capabilities.

| Example | Description | Key Features |
|---------|-------------|--------------|
| [Graph Metrics](./graph-metrics.md) | Performance monitoring and metrics collection | `GraphPerformanceMetrics`, execution timing, performance analysis |
| [Graph Visualization](./graph-visualization.md) | Graph structure visualization and export | DOT/JSON/Mermaid export, real-time highlighting, graph inspection |
| [Logging](./logging-example.md) | Comprehensive logging and tracing | Structured logging, execution tracing, debug information |

### 🔌 Integration and Extensions

Examples showing integration with external systems and extension patterns.

| Example | Description | Key Features |
|---------|-------------|--------------|
| [Plugin System](./plugin-system.md) | Dynamic plugin loading and execution | Plugin discovery, dynamic loading, plugin orchestration |
| [REST API Integration](./rest-api.md) | External API integration via REST tools | `RestToolGraphNode`, API orchestration, external service integration |
| [Assert and Suggest](./assert-suggest.md) | Validation and suggestion patterns | Input validation, suggestion generation, quality gates |

### 🧠 AI and Optimization

Examples demonstrating AI-specific patterns and optimization strategies.

| Example | Description | Key Features |
|---------|-------------|--------------|
| [Optimizers and Few-Shot](./optimizers-fewshot.md) | Prompt optimization and few-shot learning | Prompt optimization, few-shot examples, performance tuning |
| [Chatbot with Memory](./chatbot.md) | Conversational AI with persistent context | Conversation management, memory integration, context awareness |

## Example Complexity Levels

### 🟢 Beginner
- **Chain of Thought**: Basic reasoning patterns
- **Conditional Nodes**: Simple routing logic
- **State Management**: Basic state handling
- **Logging**: Basic observability

### 🟡 Intermediate
- **ReAct Agent**: Tool integration and reasoning
- **Checkpointing**: State persistence
- **Graph Metrics**: Performance monitoring
- **Plugin System**: Dynamic loading

### 🔴 Advanced
- **Multi-Agent**: Distributed coordination
- **Advanced Patterns**: Complex workflow composition
- **Document Analysis**: Multi-stage pipelines
- **Dynamic Routing**: Adaptive execution

## Running Examples

### Prerequisites

1. **.NET 8.0** or later
2. **OpenAI API Key** (configured in `appsettings.json`)
3. **Semantic Kernel Graph package** installed

### Configuration

Create or update `appsettings.json` in the examples project:

```json
{
  "OpenAI": {
    "ApiKey": "your-api-key-here",
    "Model": "gpt-3.5-turbo",
    "MaxTokens": 4000,
    "Temperature": 0.7
  }
}
```

### Example Execution Flow

1. **Setup**: Kernel configuration with graph support
2. **Graph Creation**: Build graph structure with nodes and edges
3. **Execution**: Run graph with input arguments
4. **Monitoring**: Observe execution flow and collect metrics
5. **Results**: Process and display execution results

## Learning Path

### Start Here
1. **Chain of Thought**: Understand basic reasoning patterns
2. **Conditional Nodes**: Learn dynamic routing
3. **State Management**: Master state handling

### Build Complexity
1. **ReAct Agent**: Add tool integration
2. **Checkpointing**: Implement persistence
3. **Multi-Agent**: Explore distributed workflows

### Advanced Patterns
1. **Advanced Patterns**: Complex compositions
2. **Document Analysis**: Multi-stage pipelines
3. **Dynamic Routing**: Adaptive execution

## Contributing Examples

When contributing new examples:

1. **Follow the pattern**: Use existing examples as templates
2. **Include documentation**: Add comprehensive comments and README
3. **Add to Program.cs**: Register in the examples dictionary
4. **Test thoroughly**: Ensure examples run without errors
5. **Update this index**: Add new examples to appropriate categories

## Troubleshooting

### Common Issues

- **API Key Missing**: Check `appsettings.json` configuration
- **Package Dependencies**: Ensure all required packages are installed
- **Memory Issues**: Large graphs may require increased memory limits
- **Timeout Errors**: Adjust execution timeouts for complex workflows

### Getting Help

- Check the [Troubleshooting Guide](../troubleshooting.md)
- Review [API Reference](../api/) for detailed class documentation
- Open issues on the project repository for specific problems

## Related Documentation

- [Getting Started](../getting-started.md): Quick start guide
- [Concepts](../concepts/): Core concepts and terminology
- [How-to Guides](../how-to/): Step-by-step tutorials
- [API Reference](../api/): Complete API documentation
- [Architecture](../architecture/): System design and decisions
