# GraphExecutor

The `GraphExecutor` class is the main orchestrator for graph execution, implementing the `IGraphExecutor` interface. It manages the complete execution flow, navigation, and coordination of graph nodes with comprehensive configuration options for performance, concurrency, and resource governance.

## Overview

`GraphExecutor` provides a robust, thread-safe execution engine with built-in safeguards against infinite loops, configurable performance monitoring, and advanced features like parallel execution and resource governance. It's designed for both simple workflows and complex enterprise scenarios.

## Properties

### Core Properties

- **GraphId**: Unique identifier for this graph instance
- **Name**: Human-readable name of the graph
- **Description**: Detailed description of what the graph does
- **CreatedAt**: Timestamp when the graph was created
- **StartNode**: The configured starting node for execution (nullable)
- **Nodes**: Read-only collection of all nodes in the graph
- **Edges**: Read-only collection of all conditional edges
- **NodeCount**: Total number of nodes in the graph
- **EdgeCount**: Total number of edges in the graph

### Execution State

- **IsReadyForExecution**: Indicates whether the graph is ready to execute (has nodes and a start node)

## Configuration Methods

### Metrics Configuration

Configure performance metrics collection for the graph:

```csharp
GraphExecutor ConfigureMetrics(GraphMetricsOptions? options = null)
```

**Parameters:**
- `options`: Metrics collection options (null to disable metrics)

**Returns:** This executor for method chaining

**Example:**
```csharp
var executor = new GraphExecutor("MyGraph");
executor.ConfigureMetrics(new GraphMetricsOptions
{
    EnableRealTimeMetrics = true,
    MetricsRetentionPeriod = TimeSpan.FromHours(24),
    EnablePercentileCalculations = true
});
```

### Concurrency Configuration

Configure parallel execution behavior:

```csharp
GraphExecutor ConfigureConcurrency(GraphConcurrencyOptions? options)
```

**Parameters:**
- `options`: Concurrency options (null disables parallel execution)

**Returns:** This executor for method chaining

**Example:**
```csharp
executor.ConfigureConcurrency(new GraphConcurrencyOptions
{
    EnableParallelExecution = true,
    MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
    MergeConflictPolicy = StateMergeConflictPolicy.PreferSecond
});
```

### Resource Governance Configuration

Configure resource limits and QoS behavior:

```csharp
GraphExecutor ConfigureResources(GraphResourceOptions? options)
```

**Parameters:**
- `options`: Resource governance options (null disables resource governance)

**Returns:** This executor for method chaining

**Example:**
```csharp
executor.ConfigureResources(new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 100.0,
    MaxBurstSize = 200,
    CpuHighWatermarkPercent = 85.0,
    MinAvailableMemoryMB = 1024.0
});
```

### Self-Healing Configuration

Configure automatic recovery from node failures:

```csharp
GraphExecutor ConfigureSelfHealing(SelfHealingOptions options)
```

**Parameters:**
- `options`: Self-healing configuration options

**Returns:** This executor for method chaining

## Execution Methods

### Primary Execution

Execute the graph from the configured start node:

```csharp
Task<FunctionResult> ExecuteAsync(
    Kernel kernel,
    KernelArguments arguments,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `kernel`: Semantic Kernel instance for function resolution
- `arguments`: Execution state and inputs
- `cancellationToken`: Cancellation token

**Returns:** Final execution result

**Exceptions:**
- `ArgumentNullException`: When kernel or arguments are null
- `InvalidOperationException`: When graph is not ready for execution
- `OperationCanceledException`: When execution is cancelled

### Execution from Specific Node

Execute starting from a specific node instance:

```csharp
Task<FunctionResult> ExecuteFromNodeAsync(
    IGraphNode startNode,
    Kernel kernel,
    KernelArguments arguments,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `startNode`: Node instance to begin execution from
- `kernel`: Semantic Kernel instance
- `arguments`: Execution state and inputs
- `cancellationToken`: Cancellation token

**Returns:** Final execution result

**Exceptions:**
- `ArgumentNullException`: When any parameter is null
- `InvalidOperationException`: When startNode is not part of the graph

### Execution from Node ID

Execute starting from a node identified by ID:

```csharp
Task<FunctionResult> ExecuteFromAsync(
    Kernel kernel,
    KernelArguments arguments,
    string startNodeId,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `kernel`: Semantic Kernel instance
- `arguments`: Execution state and inputs
- `startNodeId`: ID of the node to start from
- `cancellationToken`: Cancellation token

**Returns:** Final execution result

**Exceptions:**
- `ArgumentNullException`: When kernel or arguments are null
- `ArgumentException`: When startNodeId is invalid
- `InvalidOperationException`: When the start node is not found

### Node Execution

Execute a single node in isolation:

```csharp
Task<FunctionResult> ExecuteNodeAsync(
    IGraphNode node,
    Kernel kernel,
    KernelArguments arguments,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `node`: The node to execute
- `kernel`: Semantic Kernel instance
- `arguments`: Execution state and inputs
- `cancellationToken`: Cancellation token

**Returns:** Node execution result

### Graph Execution

Execute a custom sequence of nodes:

```csharp
Task<FunctionResult> ExecuteGraphAsync(
    IEnumerable<IGraphNode> nodes,
    Kernel kernel,
    KernelArguments arguments,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `nodes`: Ordered sequence of nodes to execute
- `kernel`: Semantic Kernel instance
- `arguments`: Execution state and inputs
- `cancellationToken`: Cancellation token

**Returns:** Final execution result

## Validation and Integrity

### Graph Validation

Validate the structural integrity of the graph:

```csharp
ValidationResult ValidateGraphIntegrity()
```

**Returns:** Validation result with errors and warnings

**Validation Checks:**
- Graph contains at least one node
- Start node is configured
- All nodes are valid
- All edges are valid
- Schema compatibility across edges
- No unreachable nodes (when strict mode enabled)

**Example:**
```csharp
var validationResult = executor.ValidateGraphIntegrity();
if (!validationResult.IsValid)
{
    Console.WriteLine($"Graph validation failed: {validationResult.CreateSummary()}");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"Error: {error}");
    }
}
```

## Graph Construction

### Adding Nodes

Add nodes to the graph:

```csharp
GraphExecutor AddNode(IGraphNode node)
```

**Parameters:**
- `node`: The node to add

**Returns:** This executor for method chaining

**Example:**
```csharp
executor.AddNode(new FunctionGraphNode(myFunction, "processData"))
       .AddNode(new ConditionalGraphNode("validate", "Validate input"))
       .AddNode(new FunctionGraphNode(outputFunction, "generateOutput"));
```

### Connecting Nodes

Create conditional edges between nodes:

```csharp
GraphExecutor Connect(string sourceNodeId, string targetNodeId, string? edgeName = null)
```

**Parameters:**
- `sourceNodeId`: Source node identifier
- `targetNodeId`: Target node identifier
- `edgeName`: Optional name for the edge

**Returns:** This executor for method chaining

**Example:**
```csharp
executor.Connect("start", "processData")
       .Connect("processData", "validate")
       .Connect("validate", "generateOutput", "success")
       .Connect("validate", "errorHandler", "failure");
```

### Setting Start Node

Configure the execution starting point:

```csharp
GraphExecutor SetStartNode(string nodeId)
```

**Parameters:**
- `nodeId`: ID of the node to start execution from

**Returns:** This executor for method chaining

**Exceptions:**
- `ArgumentException`: When nodeId is null or empty
- `InvalidOperationException`: When the node is not found

## Middleware Support

### Adding Middleware

Extend execution behavior with custom middleware:

```csharp
GraphExecutor UseMiddleware(IGraphExecutionMiddleware middleware)
```

**Parameters:**
- `middleware`: Middleware instance to add

**Returns:** This executor for method chaining

**Example:**
```csharp
executor.UseMiddleware(new LoggingMiddleware())
       .UseMiddleware(new MetricsMiddleware())
       .UseMiddleware(new CustomBusinessLogicMiddleware());
```

## Thread Safety

The `GraphExecutor` is designed for thread safety:

- **Node collections**: Protected by `ConcurrentDictionary<TKey, TValue>`
- **Edge mutations**: Guarded by private locks for consistency
- **Public methods**: Validate inputs and throw appropriate exceptions
- **Execution**: Honors `CancellationToken` and propagates cancellation
- **Reuse**: Instances are safe to reuse across executions

## Performance and Observability

### Built-in Tracing

- **ActivitySource**: Automatic distributed tracing with `ActivitySource`
- **Execution tags**: Rich metadata for correlation and debugging
- **Performance metrics**: Optional detailed performance tracking

### Event System

Graph mutation events for monitoring and integration:

- **NodeAdded**: Raised after successful node addition
- **NodeRemoved**: Raised after successful node removal
- **NodeReplaced**: Raised when a node is replaced
- **EdgeAdded**: Raised after successful edge addition
- **EdgeRemoved**: Raised after successful edge removal

## Usage Examples

### Basic Graph Construction

```csharp
var executor = new GraphExecutor("DataProcessingGraph", "Process and validate data");

// Add nodes
executor.AddNode(new FunctionGraphNode(LoadDataFunction, "loadData"))
       .AddNode(new FunctionGraphNode(ProcessDataFunction, "processData"))
       .AddNode(new ConditionalGraphNode("validate", "Validate processed data"))
       .AddNode(new FunctionGraphNode(SaveDataFunction, "saveData"));

// Connect nodes
executor.Connect("loadData", "processData")
       .Connect("processData", "validate")
       .Connect("validate", "saveData", "valid")
       .Connect("validate", "errorHandler", "invalid");

// Set start node
executor.SetStartNode("loadData");

// Configure execution
executor.ConfigureMetrics()
       .ConfigureConcurrency(new GraphConcurrencyOptions { EnableParallelExecution = true });

// Execute
var result = await executor.ExecuteAsync(kernel, arguments);
```

### Advanced Configuration

```csharp
var executor = new GraphExecutor("EnterpriseGraph", "High-performance enterprise workflow");

// Configure comprehensive metrics
executor.ConfigureMetrics(new GraphMetricsOptions
{
    EnableRealTimeMetrics = true,
    MetricsRetentionPeriod = TimeSpan.FromDays(7),
    EnablePercentileCalculations = true,
    ResourceSamplingInterval = TimeSpan.FromSeconds(5)
});

// Configure parallel execution
executor.ConfigureConcurrency(new GraphConcurrencyOptions
{
    EnableParallelExecution = true,
    MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
    MergeConflictPolicy = StateMergeConflictPolicy.PreferSecond,
    FallbackToSequentialOnCycles = true
});

// Configure resource governance
executor.ConfigureResources(new GraphResourceOptions
{
    EnableResourceGovernance = true,
    BasePermitsPerSecond = 200.0,
    MaxBurstSize = 500,
    CpuHighWatermarkPercent = 80.0,
    MinAvailableMemoryMB = 2048.0,
    EnableCooperativePreemption = true
});

// Configure self-healing
executor.ConfigureSelfHealing(new SelfHealingOptions
{
    EnableAutomaticRecovery = true,
    MaxRetryAttempts = 3,
    QuarantineFailedNodes = true
});
```

## Related Types

- **IGraphExecutor**: Interface contract
- **GraphExecutionContext**: Execution state and coordination
- **GraphExecutionOptions**: Immutable execution configuration
- **GraphMetricsOptions**: Performance metrics configuration
- **GraphConcurrencyOptions**: Parallel execution configuration
- **GraphResourceOptions**: Resource governance configuration
- **ValidationResult**: Graph validation results

## See Also

- [IGraphExecutor](igraph-executor.md) - Interface contract and semantics
- [Execution Model](../concepts/execution-model.md) - How execution flows through graphs
- [Resource Governance and Concurrency](../how-to/resource-governance-and-concurrency.md) - Advanced configuration
- [Parallelism and Fork/Join](../how-to/parallelism-and-fork-join.md) - Parallel execution patterns
- [Getting Started](../getting-started.md) - Building your first graph
