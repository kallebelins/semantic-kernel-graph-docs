# Graph Concepts

This guide explains the fundamental concepts of SemanticKernel.Graph, including nodes, conditional edges, execution paths, and controlled cycles.

## Overview

SemanticKernel.Graph extends the Semantic Kernel with a graph-based execution model that allows you to create complex workflows by connecting nodes with conditional logic. Unlike traditional linear execution, graphs enable branching, looping, and dynamic routing based on execution state.

## Core Components

### Nodes

Nodes are the fundamental building blocks of a graph. Each node represents a unit of work that can be executed, and they communicate through shared state.

**Key Properties:**
- **NodeId**: Unique identifier for the node
- **Name**: Human-readable name for debugging and visualization
- **Description**: What the node does
- **IsExecutable**: Whether the node can be executed
- **InputParameters**: Expected input parameters
- **OutputParameters**: Produced output parameters

**Lifecycle Methods:**
- `OnBeforeExecuteAsync`: Setup and validation before execution
- `ExecuteAsync`: Main execution logic
- `OnAfterExecuteAsync`: Cleanup and post-processing
- `OnExecutionFailedAsync`: Error handling and recovery

### Conditional Edges

Edges define the connections between nodes and control the flow of execution. They can be unconditional (always taken) or conditional (taken only when specific conditions are met).

**Edge Types:**
- **Unconditional**: Always traversed after the source node executes
- **Parameter-based**: Evaluates against specific argument values
- **State-based**: Evaluates against the entire graph state
- **Template-based**: Uses Handlebars-like templates for complex conditions

**Edge Properties:**
- **SourceNode**: The origin node
- **TargetNode**: The destination node
- **Condition**: Predicate function that determines traversal
- **Metadata**: Additional information for routing and visualization
- **MergeConfiguration**: How to handle state when multiple paths converge

## Execution Model

### Execution Flow

The graph executor follows a systematic approach to navigate and execute nodes:

1. **Start Node**: Execution begins at the designated start node
2. **Node Execution**: The current node executes with access to shared state
3. **Next Node Selection**: The executor determines which nodes to execute next
4. **State Propagation**: Results and state changes flow to subsequent nodes
5. **Termination**: Execution stops when no more nodes are available

### Navigation Logic

Each node implements `GetNextNodes()` to specify which nodes should execute next:

```csharp
public IEnumerable<IGraphNode> GetNextNodes(FunctionResult? executionResult, GraphState graphState)
{
    // Return the next nodes based on execution result and current state
    if (executionResult?.GetValue<bool>("should_continue") == true)
    {
        return new[] { nextNode };
    }
    return Enumerable.Empty<IGraphNode>(); // Terminate execution
}
```

### Conditional Routing

Conditional edges enable dynamic routing based on execution state:

```csharp
// Create an edge that's only taken when a condition is met
var edge = new ConditionalEdge(
    sourceNode, 
    targetNode, 
    state => state.GetValue<int>("score") >= 80
);

// Or use factory methods for common patterns
var edge = ConditionalEdge.CreateParameterEquals(
    sourceNode, 
    targetNode, 
    "status", 
    "success"
);
```

## Execution Paths

### Linear Paths

Simple workflows follow a linear sequence:

```
Node A → Node B → Node C
```

### Branching Paths

Conditional logic creates branching execution:

```
        ┌─ Condition True ─→ Node B
Node A ─┤
        └─ Condition False ─→ Node C
```

### Parallel Paths

Multiple paths can execute concurrently and then converge:

```
Node A ─┬─→ Node B ─┐
        │            │
        └─→ Node C ─┼─→ Node D
                     │
        └─→ Node E ─┘
```

### Loop Paths

Nodes can create loops for iterative processing:

```
Node A → Node B → Condition → Node A (if condition met)
```

## Controlled Cycles

### Loop Prevention

The graph executor includes built-in safeguards to prevent infinite loops:

- **Maximum Iterations**: Configurable limits on loop execution
- **Execution Depth**: Tracking of execution depth to detect excessive nesting
- **Timeout Controls**: Configurable timeouts for long-running operations
- **Circuit Breakers**: Automatic termination of problematic execution paths

### Loop Control

Nodes can implement loop control logic:

```csharp
public bool ShouldExecute(GraphState graphState)
{
    var iterationCount = graphState.GetValue<int>("iteration_count", 0);
    var maxIterations = graphState.GetValue<int>("max_iterations", 10);
    
    return iterationCount < maxIterations;
}
```

### State Management in Loops

Loop nodes maintain state across iterations:

```csharp
// Store iteration count
graphState.SetValue("iteration_count", 
    graphState.GetValue<int>("iteration_count", 0) + 1);

// Check for termination conditions
if (goalAchieved || maxIterationsReached)
{
    graphState.SetValue("loop_terminated", true);
}
```

## State Management

### Shared State

All nodes share a common `GraphState` that wraps `KernelArguments`:

```csharp
// Set values in state
graphState.SetValue("user_input", "Hello, world!");
graphState.SetValue("processing_step", 1);

// Retrieve values from state
var input = graphState.GetValue<string>("user_input");
var step = graphState.GetValue<int>("processing_step");
```

### State Persistence

Graph state can be serialized and persisted:

```csharp
// Save state to checkpoint
var checkpoint = await stateHelpers.SaveCheckpointAsync(graphState);

// Restore state from checkpoint
var restoredState = await stateHelpers.RestoreCheckpointAsync(checkpoint);
```

### State Validation

Nodes can validate state before execution:

```csharp
public ValidationResult ValidateExecution(KernelArguments arguments)
{
    var result = new ValidationResult();
    
    if (!arguments.ContainsName("required_parameter"))
    {
        result.AddError("Required parameter 'required_parameter' is missing");
    }
    
    return result;
}
```

## Advanced Patterns

### ReAct Pattern

The ReAct (Reasoning + Acting) pattern implements iterative problem-solving:

```csharp
var reactNode = new ReActLoopGraphNode()
    .ConfigureNodes(
        reasoningNode,    // Analyze and plan
        actionNode,       // Execute actions
        observationNode   // Observe results
    );
```

### Multi-Agent Coordination

Multiple agents can work together with shared state:

```csharp
var coordinator = new MultiAgentCoordinator();
coordinator.AddAgent("researcher", researchAgent);
coordinator.AddAgent("analyzer", analysisAgent);
coordinator.AddAgent("summarizer", summaryAgent);
```

### Human-in-the-Loop

Graphs can pause execution for human approval:

```csharp
var approvalNode = new HumanApprovalGraphNode(
    "approval_required",
    "Requires human approval to proceed"
);
```

## Best Practices

### Node Design

- **Single Responsibility**: Each node should have one clear purpose
- **State Validation**: Always validate inputs before execution
- **Error Handling**: Implement robust error handling and recovery
- **Metadata**: Use metadata to provide context for debugging

### Edge Design

- **Clear Conditions**: Make conditional logic explicit and readable
- **Performance**: Keep condition evaluation fast and side-effect free
- **Documentation**: Document complex routing logic

### State Management

- **Immutable Updates**: Avoid modifying existing state directly
- **Validation**: Validate state changes and provide meaningful error messages
- **Serialization**: Consider serialization requirements when designing state

### Performance

- **Lazy Evaluation**: Only evaluate conditions when necessary
- **Caching**: Cache expensive computations when possible
- **Parallelization**: Use parallel execution for independent paths

## See Also

- [Execution Model](execution.md) - Detailed execution lifecycle
- [Node Types](nodes.md) - Available node implementations
- [State Management](state.md) - Advanced state handling
- [Routing Strategies](routing.md) - Dynamic routing techniques
- [Examples](../examples/) - Practical examples and use cases
