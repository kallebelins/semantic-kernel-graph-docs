# Core API

The Core API provides the fundamental building blocks for SemanticKernel.Graph, including the main execution engine, state management, and core types that enable graph-based workflows.

## Key Types

### Core Execution
* **`GraphExecutor`** - Main orchestrator for graph execution, manages execution flow and coordination
* **`GraphExecutionContext`** - Execution context for a single graph run, tracks progress and manages resources
* **`IGraphNode`** - Base contract for all graph nodes, defines essential structure and behavior

### State Management
* **`GraphState`** - Typed wrapper for KernelArguments that serves as graph state foundation
* **`ConditionalEdge`** - Directional, optionally guarded transitions between graph nodes
* **`StateMergeConflictPolicy`** - Conflict resolution strategies for state merges during parallel execution

### Error Handling and Resilience
* **`ErrorPolicyRegistry`** - Registry for error handling policies and strategies
* **`RetryPolicyGraphNode`** - Node wrapper with automatic retry capabilities and configurable policies
* **`ErrorHandlerGraphNode`** - Specialized node for handling and recovering from errors
* **`ErrorMetricsCollector`** - Collects and tracks error metrics for monitoring and analysis

### Multi-Agent Coordination
* **`MultiAgentCoordinator`** - Coordinates multiple graph executor instances for multi-agent execution
* **`ResultAggregator`** - Aggregates results from multiple agents using configurable strategies
* **`AgentConnectionPool`** - Manages connections and reuse for remote agent communication
* **`WorkDistributor`** - Distributes work across multiple agents using various strategies

### Validation and Compilation
* **`WorkflowValidator`** - Validates workflow integrity and structural correctness
* **`GraphTypeInferenceEngine`** - Infers types and validates schema compatibility across edges
* **`StateValidator`** - Validates state integrity and resolves conflicts
* **`StateMergeConflictPolicy`** - Defines how conflicting state values are resolved during merges

## Performance and Metrics

* **`GraphPerformanceMetrics`** - Comprehensive performance metrics collector
* **`NodeExecutionMetrics`** - Node-level execution statistics
* **`GraphMetricsOptions`** - Metrics configuration options
* **`GraphMetricsExporter`** - Metrics export and visualization

Refer to [Metrics APIs Reference](./metrics.md) for detailed metrics documentation.

## Example Usage

Here's a comprehensive example demonstrating the Core API types:

```csharp
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;
using SemanticKernel.Graph.Execution;
using SemanticKernel.Graph.Integration.Policies;
using SemanticKernel.Graph.Integration;

// Create and configure kernel with graph support
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-4", "your-api-key")
    .AddGraphSupport()
    .Build();

// Create a graph executor with configuration
var executor = new GraphExecutor("MyWorkflow", "Sample workflow demonstration");

// Configure performance monitoring
executor.ConfigureMetrics(new GraphMetricsOptions
{
    EnableRealTimeMetrics = true,
    MetricsRetentionPeriod = TimeSpan.FromHours(1),
    EnablePercentileCalculations = true
});

// Configure concurrency options
executor.ConfigureConcurrency(new GraphConcurrencyOptions
{
    MaxDegreeOfParallelism = 4,
    EnableParallelExecution = true
});

// Create and manage graph state
var graphState = new GraphState();
graphState.KernelArguments["userName"] = "John Doe";
graphState.KernelArguments["currentStep"] = 1;

// Create function nodes
var startNode = new FunctionGraphNode(
    kernel.CreateFunctionFromMethod(
        (string input) => $"Started: {input}",
        functionName: "StartProcess",
        description: "Starts the workflow"
    ),
    "startNode",
    "Workflow start point"
);

var processNode = new FunctionGraphNode(
    kernel.CreateFunctionFromMethod(
        (string input) => $"Processed: {input}",
        functionName: "ProcessData",
        description: "Processes the input data"
    ),
    "processNode",
    "Data processing node"
);

// Create conditional edge
var edge = new ConditionalEdge(
    startNode,
    processNode,
    (args) => args.GetValue<string>("input")?.Length > 0,
    "StartToProcess"
);

// Wrap node with retry policy
var retryConfig = new RetryPolicyConfig
{
    MaxRetries = 3,
    BaseDelay = TimeSpan.FromSeconds(1),
    Strategy = RetryStrategy.ExponentialBackoff,
    UseJitter = true
};

var retryNode = new RetryPolicyGraphNode(processNode, retryConfig);

// Create multi-agent coordinator
var multiAgentOptions = new MultiAgentOptions
{
    MaxConcurrentAgents = 5,
    CoordinationTimeout = TimeSpan.FromMinutes(10),
    SharedStateOptions = new SharedStateOptions
    {
        ConflictResolutionStrategy = ConflictResolutionStrategy.Merge,
        AllowOverwrite = true
    }
};

var coordinator = new MultiAgentCoordinator(multiAgentOptions);

// Add nodes to executor and execute
executor.AddNode(startNode)
        .AddNode(retryNode)
        .SetStartNode("startNode");

var arguments = new KernelArguments();
arguments.SetGraphState(graphState);
arguments["input"] = "Hello World";

var result = await executor.ExecuteAsync(kernel, arguments);
Console.WriteLine($"Execution result: {result.GetValue<string>()}");
```

## Core Concepts

### Graph Execution Flow
1. **Initialization**: Create `GraphExecutor` and configure options
2. **Node Creation**: Create `FunctionGraphNode`, `ConditionalGraphNode`, or custom nodes
3. **Edge Definition**: Define `ConditionalEdge` instances for navigation logic
4. **State Management**: Use `GraphState` for persistent execution state
5. **Execution**: Call `ExecuteAsync()` to run the complete workflow

### State Management
- `GraphState` wraps `KernelArguments` for graph-specific functionality
- State is shared across all nodes during execution
- Use `SetValue()` and `GetValue<T>()` for type-safe state access
- State merge conflicts are resolved using `StateMergeConflictPolicy`

### Error Handling
- Wrap nodes with `RetryPolicyGraphNode` for automatic retry
- Configure retry strategies: fixed delay, exponential backoff, custom logic
- Use `ErrorHandlerGraphNode` for specialized error recovery
- Monitor errors with `ErrorMetricsCollector`

### Multi-Agent Coordination
- `MultiAgentCoordinator` manages multiple executor instances
- Configure work distribution strategies: role-based, load-based, priority-based
- Handle state conflicts with configurable resolution strategies
- Aggregate results using consensus, merge, or custom strategies

## See Also

* [Graph Concepts](../concepts/graph-concepts.md) - Core graph concepts and terminology
* [Execution Model](../concepts/execution-model.md) - How graph execution works
* [Node Types](../concepts/node-types.md) - Available node types and their capabilities
* [Build a Graph](../how-to/build-a-graph.md) - Step-by-step guide to creating graphs
* [Error Handling and Resilience](../how-to/error-handling-and-resilience.md) - Error policies and recovery
* [Multi-Agent and Shared State](../how-to/multi-agent-and-shared-state.md) - Multi-agent coordination
* [Integration and Extensions](../how-to/integration-and-extensions.md) - Extending the framework
* [Metrics and Observability](../how-to/metrics-and-observability.md) - Performance monitoring

Refer to XML docs in source for full signatures and additional examples.
