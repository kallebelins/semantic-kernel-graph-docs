# IGraphNode

The `IGraphNode` interface defines the base contract for all graph nodes in SemanticKernel.Graph. It establishes the essential structure and behavior that every node must implement, providing a consistent foundation for building complex workflows.

## Overview

`IGraphNode` serves as the fundamental building block for graph-based workflows. It defines the contract that enables nodes to participate in execution, navigation, and lifecycle management while maintaining side-effect awareness and operating on shared state.

## Core Principles

Nodes implementing this interface are expected to:

- **Be side-effect aware**: Operate on shared `KernelArguments` contained in `GraphState`
- **Validate inputs**: Use `ValidateExecution` for lightweight, non-mutating validation
- **Use lifecycle hooks**: Prefer `OnBeforeExecuteAsync` and `OnAfterExecuteAsync` over constructors
- **Honor cancellation**: Support cooperative cancellation via `CancellationToken`

## Properties

### Identity and Metadata

- **NodeId**: Unique identifier for this node instance
- **Name**: Human-readable name for display and logging
- **Description**: Detailed description of the node's purpose and behavior
- **Metadata**: Read-only dictionary of custom metadata and configuration
- **IsExecutable**: Indicates whether the node can be executed

### Input/Output Contracts

- **InputParameters**: List of parameter names this node expects (best-effort hint for wiring)
- **OutputParameters**: List of parameter names this node produces (should be stable across versions)

## Execution Methods

### Core Execution

Execute the node's primary logic:

```csharp
Task<FunctionResult> ExecuteAsync(
    Kernel kernel,
    KernelArguments arguments,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `kernel`: Semantic Kernel instance for function resolution and services
- `arguments`: Execution arguments containing the graph state
- `cancellationToken`: Token for cooperative cancellation

**Returns:** Result of the node execution

**Exceptions:**
- `ArgumentNullException`: When kernel or arguments are null
- `InvalidOperationException`: When the node cannot be executed

**Guidelines:**
- Avoid throwing for expected domain errors; encode failures in the result instead
- Reserve exceptions for truly exceptional conditions
- Honor the cancellation token for all async operations

### Execution Validation

Validate that the node can execute with provided arguments:

```csharp
ValidationResult ValidateExecution(KernelArguments arguments)
```

**Parameters:**
- `arguments`: Arguments to validate

**Returns:** Validation result indicating execution feasibility

**Exceptions:**
- `ArgumentNullException`: When arguments are null

**Guidelines:**
- Perform lightweight checks only; avoid expensive operations
- Do not mutate state during validation
- Return actionable validation messages

## Navigation Methods

### Next Node Discovery

Determine possible next nodes after execution:

```csharp
IEnumerable<IGraphNode> GetNextNodes(
    FunctionResult? executionResult,
    GraphState graphState)
```

**Parameters:**
- `executionResult`: Result of executing this node (may be null)
- `graphState`: Current graph state

**Returns:** Collection of possible next nodes

**Exceptions:**
- `ArgumentNullException`: When graphState is null

**Guidelines:**
- Use either `executionResult` and/or `graphState` to decide transitions
- Return an empty collection to terminate execution when appropriate
- Consider conditional routing based on execution results

### Execution Decision

Determine if the node should execute based on current state:

```csharp
bool ShouldExecute(GraphState graphState)
```

**Parameters:**
- `graphState`: Current graph state

**Returns:** True if the node should execute

**Exceptions:**
- `ArgumentNullException`: When graphState is null

**Guidelines:**
- This method should be deterministic and side-effect free
- Base decisions on state conditions, not external factors
- Consider required parameters, business rules, or conditional logic

## Lifecycle Methods

### Pre-Execution Setup

Called before the node is executed:

```csharp
Task OnBeforeExecuteAsync(
    Kernel kernel,
    KernelArguments arguments,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `kernel`: Semantic Kernel instance
- `arguments`: Execution arguments
- `cancellationToken`: Cancellation token

**Returns:** Task representing the setup operation

**Guidelines:**
- Perform only idempotent setup operations
- Avoid expensive I/O operations; prefer doing work in `ExecuteAsync`
- Use for initialization, validation, and resource preparation

### Post-Execution Cleanup

Called after successful execution:

```csharp
Task OnAfterExecuteAsync(
    Kernel kernel,
    KernelArguments arguments,
    FunctionResult result,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `kernel`: Semantic Kernel instance
- `arguments`: Execution arguments
- `result`: Execution result
- `cancellationToken`: Cancellation token

**Returns:** Task representing the cleanup operation

**Guidelines:**
- Avoid throwing from this method
- Prefer logging and compensating actions over exceptions
- Use for cleanup, result processing, and state updates

### Error Handling

Called when execution fails:

```csharp
Task OnExecutionFailedAsync(
    Kernel kernel,
    KernelArguments arguments,
    Exception exception,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `kernel`: Semantic Kernel instance
- `arguments`: Execution arguments
- `exception`: Exception that occurred
- `cancellationToken`: Cancellation token

**Returns:** Task representing the error handling operation

**Guidelines:**
- Perform cleanup, emit telemetry, or request retries via shared state
- Must not throw unless recovery cannot proceed
- Consider implementing retry logic or fallback strategies

## Implementation Guidelines

### State Management

- **Shared State**: Operate on `KernelArguments` contained in `GraphState`
- **Immutability**: Avoid modifying state during validation
- **Consistency**: Ensure state updates are atomic and consistent

### Error Handling

- **Graceful Degradation**: Handle errors gracefully when possible
- **Logging**: Provide detailed logging for debugging and monitoring
- **Recovery**: Implement recovery strategies where appropriate

### Performance Considerations

- **Lightweight Validation**: Keep validation operations fast and efficient
- **Async Operations**: Use async/await patterns for I/O operations
- **Resource Management**: Properly dispose of resources in lifecycle methods

## Usage Examples

### Basic Node Implementation

```csharp
public class SimpleNode : IGraphNode
{
    public string NodeId { get; }
    public string Name { get; }
    public string Description { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
    public bool IsExecutable => true;
    
    public IReadOnlyList<string> InputParameters => new[] { "input" };
    public IReadOnlyList<string> OutputParameters => new[] { "output" };

    public async Task<FunctionResult> ExecuteAsync(
        Kernel kernel, 
        KernelArguments arguments, 
        CancellationToken cancellationToken = default)
    {
        var input = arguments.GetValue<string>("input");
        var output = $"Processed: {input}";
        
        arguments["output"] = output;
        return new FunctionResult(KernelFunctionFactory.CreateFromMethod(() => output), output);
    }

    public ValidationResult ValidateExecution(KernelArguments arguments)
    {
        var result = new ValidationResult();
        
        if (!arguments.ContainsKey("input"))
        {
            result.AddError("Required parameter 'input' is missing");
        }
        
        return result;
    }

    public IEnumerable<IGraphNode> GetNextNodes(
        FunctionResult? executionResult, 
        GraphState graphState)
    {
        // Return next nodes based on execution result or state
        return Enumerable.Empty<IGraphNode>();
    }

    public bool ShouldExecute(GraphState graphState)
    {
        return graphState.KernelArguments.ContainsKey("input") &&
               !string.IsNullOrEmpty(graphState.KernelArguments["input"]?.ToString());
    }

    public Task OnBeforeExecuteAsync(
        Kernel kernel, 
        KernelArguments arguments, 
        CancellationToken cancellationToken = default)
    {
        // Setup logic here
        return Task.CompletedTask;
    }

    public Task OnAfterExecuteAsync(
        Kernel kernel, 
        KernelArguments arguments, 
        FunctionResult result, 
        CancellationToken cancellationToken = default)
    {
        // Cleanup logic here
        return Task.CompletedTask;
    }

    public Task OnExecutionFailedAsync(
        Kernel kernel, 
        KernelArguments arguments, 
        Exception exception, 
        CancellationToken cancellationToken = default)
    {
        // Error handling logic here
        return Task.CompletedTask;
    }
}
```

### Advanced Node with Conditional Logic

```csharp
public class ConditionalNode : IGraphNode
{
    private readonly List<IGraphNode> _nextNodes;
    private readonly Func<KernelArguments, bool> _condition;

    public ConditionalNode(
        string nodeId, 
        string name, 
        Func<KernelArguments, bool> condition)
    {
        NodeId = nodeId;
        Name = name;
        Description = "Conditional routing node";
        _condition = condition;
        _nextNodes = new List<IGraphNode>();
        Metadata = new Dictionary<string, object>();
        IsExecutable = false; // This node doesn't execute, it only routes
    }

    public string NodeId { get; }
    public string Name { get; }
    public string Description { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
    public bool IsExecutable { get; }
    
    public IReadOnlyList<string> InputParameters => Array.Empty<string>();
    public IReadOnlyList<string> OutputParameters => Array.Empty<string>();

    public Task<FunctionResult> ExecuteAsync(
        Kernel kernel, 
        KernelArguments arguments, 
        CancellationToken cancellationToken = default)
    {
        // This node doesn't execute
        return Task.FromResult(new FunctionResult(
            KernelFunctionFactory.CreateFromMethod(() => "No execution"), 
            "No execution"));
    }

    public ValidationResult ValidateExecution(KernelArguments arguments)
    {
        return new ValidationResult(); // Always valid
    }

    public IEnumerable<IGraphNode> GetNextNodes(
        FunctionResult? executionResult, 
        GraphState graphState)
    {
        if (_condition(graphState.KernelArguments))
        {
            return _nextNodes;
        }
        
        return Enumerable.Empty<IGraphNode>();
    }

    public bool ShouldExecute(GraphState graphState)
    {
        return false; // Never execute
    }

    public void AddNextNode(IGraphNode node)
    {
        _nextNodes.Add(node);
    }

    // Lifecycle methods with minimal implementation
    public Task OnBeforeExecuteAsync(Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task OnAfterExecuteAsync(Kernel kernel, KernelArguments arguments, FunctionResult result, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task OnExecutionFailedAsync(Kernel kernel, KernelArguments arguments, Exception exception, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
```

## Related Types

- **FunctionGraphNode**: Implementation that wraps Semantic Kernel functions
- **ConditionalGraphNode**: Implementation for conditional routing
- **GraphState**: Wrapper around KernelArguments with execution metadata
- **ValidationResult**: Result of validation operations
- **FunctionResult**: Result of node execution

## See Also

- [Node Types](../concepts/node-types.md) - Available node implementations
- [Execution Model](../concepts/execution-model.md) - How nodes participate in execution
- [Graph Concepts](../concepts/graph-concepts.md) - Understanding graph structure
- [FunctionGraphNode](function-graph-node.md) - Function wrapper implementation
- [Getting Started](../getting-started.md) - Building your first graph
