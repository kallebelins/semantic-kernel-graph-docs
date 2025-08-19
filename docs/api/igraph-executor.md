# IGraphExecutor

The `IGraphExecutor` interface defines the public contract for executing graphs built on top of Semantic Kernel. It abstracts the execution engine to enable mocking, dependency inversion, and consistent behavior across different implementations.

## Overview

`IGraphExecutor` serves as the core execution contract that orchestrates graph workflows. It manages the complete execution lifecycle, from initialization to completion, while providing safeguards against infinite loops and ensuring predictable behavior.

## Contract and Semantics

### Core Execution Method

The primary method `ExecuteAsync` orchestrates graph execution from a configured start node until completion or cancellation:

```csharp
Task<FunctionResult> ExecuteAsync(
    Kernel kernel,
    KernelArguments arguments,
    CancellationToken cancellationToken = default);
```

**Parameters:**
- `kernel`: The Semantic Kernel instance used to resolve functions, prompts, memory, and other services
- `arguments`: The execution state and inputs as `KernelArguments`; updated values may be written during execution
- `cancellationToken`: A token for cooperative cancellation

**Returns:** A `FunctionResult` representing the terminal result of the graph execution

**Exceptions:**
- `ArgumentNullException`: Thrown if `kernel` or `arguments` is null
- `OperationCanceledException`: Thrown if the operation is canceled via `cancellationToken`

### Node Execution Methods

#### ExecuteNodeAsync
Executes a single graph node in isolation:

```csharp
Task<FunctionResult> ExecuteNodeAsync(
    IGraphNode node,
    Kernel kernel,
    KernelArguments arguments,
    CancellationToken cancellationToken = default);
```

This method is useful for testing individual nodes or implementing custom execution strategies.

#### ExecuteGraphAsync
Executes a graph composed of provided nodes:

```csharp
Task<FunctionResult> ExecuteGraphAsync(
    IEnumerable<IGraphNode> nodes,
    Kernel kernel,
    KernelArguments arguments,
    CancellationToken cancellationToken = default);
```

This method supports conditional routing, branching, or early termination depending on node types and runtime conditions.

### Properties

- `Name`: A human-readable logical name for the executor instance, useful for logging, diagnostics, or multi-executor scenarios

## Execution Semantics

### Lifecycle Management

The executor follows a structured execution lifecycle:

1. **Initialization**: Creates execution context with immutable options snapshot
2. **Validation**: Optionally validates graph integrity before execution
3. **Plan Compilation**: May compile and cache structural execution plans
4. **Node Execution**: Processes nodes sequentially with lifecycle hooks
5. **Completion**: Returns final result or propagates exceptions

### State Management

- **KernelArguments**: Treated as the authoritative execution state
- **GraphState**: Wrapper around KernelArguments with additional metadata
- **Execution Context**: Captures immutable options and execution metadata
- **State Propagation**: Updates are consistently propagated across nodes

### Safety Features

- **Loop Prevention**: Built-in safeguards against infinite loops
- **Timeout Support**: Configurable execution timeouts
- **Cancellation**: Cooperative cancellation via CancellationToken
- **Resource Limits**: Configurable maximum execution steps
- **Integrity Validation**: Optional graph structure validation

### Thread Safety

Instances are generally safe to reuse across executions if no mutable per-run state is kept on the instance itself. The interface does not guarantee thread safety for concurrent executions.

## Implementation Requirements

Implementations are expected to:

- Validate inputs and honor the provided `Kernel` configuration and policies
- Treat `KernelArguments` as the authoritative execution state
- Support cooperative cancellation via `CancellationToken`
- Emit rich logging/telemetry as configured elsewhere in the system
- Handle exceptions appropriately and propagate cancellation

## Usage Examples

### Basic Execution

```csharp
Kernel kernel = BuildKernel();
KernelArguments args = new() { ["input"] = "Hello" };
IGraphExecutor executor = GetExecutor();
FunctionResult result = await executor.ExecuteAsync(kernel, args, CancellationToken.None);
```

### With Cancellation

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
var result = await executor.ExecuteAsync(kernel, arguments, cts.Token);
```

### Single Node Execution

```csharp
var node = executor.GetNode("specificNode");
var result = await executor.ExecuteNodeAsync(node, kernel, arguments, cancellationToken);
```

### Custom Node Sequence

```csharp
var customNodes = new[] { node1, node2, node3 };
var result = await executor.ExecuteGraphAsync(customNodes, kernel, arguments, cancellationToken);
```

## Related Types

- **GraphExecutor**: The main implementation of this interface
- **GraphExecutionContext**: Maintains execution state and coordination
- **GraphExecutionOptions**: Immutable execution configuration snapshot
- **GraphState**: Wrapper around KernelArguments with execution metadata

## See Also

- [GraphExecutor](graph-executor.md) - Main implementation details
- [Execution Model](../concepts/execution-model.md) - How execution flows through graphs
- [Graph Concepts](../concepts/graph-concepts.md) - Understanding graph structure
- [Getting Started](../getting-started.md) - Building your first graph
