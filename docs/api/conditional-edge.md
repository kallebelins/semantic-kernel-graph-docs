# ConditionalEdge

The `ConditionalEdge` class represents a directional, optionally guarded transition between two graph nodes. It encapsulates navigation rules that determine when execution can flow from a source node to a target node based on runtime conditions.

## Overview

A conditional edge acts as a gatekeeper in your graph workflow, allowing you to create dynamic execution paths that respond to the current state. The edge evaluates a predicate function against either `KernelArguments` or `GraphState` to determine if traversal is allowed.

## Key Concepts

**Conditional Routing**: Edges that only allow traversal when specific conditions are met, enabling dynamic workflow branching.

**Predicate Evaluation**: Functions that examine the current execution context and return true/false to determine routing decisions.

**State-Based Decisions**: Conditions that can access both simple arguments and rich graph state information.

**Merge Configuration**: Settings that control how state is combined when multiple parallel branches converge.

## Core Properties

### Edge Identity
* **`EdgeId`**: Unique, immutable identifier generated at construction time
* **`Name`**: Human-readable name for diagnostics and visualization
* **`SourceNode`**: The origin node from which traversal starts
* **`TargetNode`**: The destination node reached when condition is true

### Condition Evaluation
* **`Condition`**: Predicate function that evaluates against `KernelArguments`
* **`StateCondition`**: Optional predicate function that evaluates against `GraphState`
* **`CreatedAt`**: UTC timestamp when the edge was created

### Execution Metadata
* **`TraversalCount`**: Number of times this edge has been traversed
* **`LastTraversedAt`**: UTC timestamp of the last traversal
* **`HasBeenTraversed`**: Boolean indicating if the edge has been used

### Configuration
* **`Metadata`**: Mutable collection for storing routing weights, visualization hints, or provenance information
* **`MergeConfiguration`**: Settings for state joining during parallel branch convergence

## Constructors

### Basic Conditional Edge

```csharp
public ConditionalEdge(
    IGraphNode sourceNode, 
    IGraphNode targetNode, 
    Func<KernelArguments, bool> condition, 
    string? name = null)
```

Creates an edge with a predicate that evaluates over `KernelArguments`.

**Parameters:**
* `sourceNode`: The origin node
* `targetNode`: The destination node  
* `condition`: Side-effect free predicate function
* `name`: Optional human-readable name (defaults to "Source -> Target")

**Example:**
```csharp
var edge = new ConditionalEdge(
    sourceNode: startNode,
    targetNode: successNode,
    condition: args => args.ContainsKey("success") && (bool)args["success"],
    name: "Success Path"
);
```

### State-Based Conditional Edge

```csharp
public ConditionalEdge(
    IGraphNode sourceNode, 
    IGraphNode targetNode, 
    Func<GraphState, bool> stateCondition, 
    string? name = null)
```

Creates an edge with a predicate that evaluates over `GraphState`.

**Parameters:**
* `sourceNode`: The origin node
* `targetNode`: The destination node
* `stateCondition`: Side-effect free predicate function for graph state
* `name`: Optional human-readable name

**Example:**
```csharp
var edge = new ConditionalEdge(
    sourceNode: decisionNode,
    targetNode: highPriorityNode,
    stateCondition: state => state.GetValue<int>("priority") > 7,
    name: "High Priority Route"
);
```

## Factory Methods

### CreateUnconditional

```csharp
public static ConditionalEdge CreateUnconditional(
    IGraphNode sourceNode, 
    IGraphNode targetNode, 
    string? name = null)
```

Creates an edge that is always traversable (condition always returns true).

**Example:**
```csharp
var alwaysEdge = ConditionalEdge.CreateUnconditional(
    sourceNode: startNode,
    targetNode: nextNode,
    name: "Default Path"
);
```

### CreateParameterEquals

```csharp
public static ConditionalEdge CreateParameterEquals(
    IGraphNode sourceNode, 
    IGraphNode targetNode,
    string parameterName, 
    object expectedValue, 
    string? name = null)
```

Creates an edge that is traversable only when a specific argument equals an expected value.

**Example:**
```csharp
var modeEdge = ConditionalEdge.CreateParameterEquals(
    sourceNode: inputNode,
    targetNode: advancedNode,
    parameterName: "mode",
    expectedValue: "advanced",
    name: "Advanced Mode Route"
);
```

### CreateParameterExists

```csharp
public static ConditionalEdge CreateParameterExists(
    IGraphNode sourceNode, 
    IGraphNode targetNode,
    string parameterName, 
    string? name = null)
```

Creates an edge that is traversable only when a specific argument exists.

**Example:**
```csharp
var authEdge = ConditionalEdge.CreateParameterExists(
    sourceNode: loginNode,
    targetNode: protectedNode,
    parameterName: "authToken",
    name: "Authenticated Route"
);
```

## Condition Evaluation

### EvaluateCondition (KernelArguments)

```csharp
public bool EvaluateCondition(KernelArguments arguments)
```

Evaluates the condition against the provided arguments.

**Parameters:**
* `arguments`: The argument bag to evaluate

**Returns:** `true` if the condition is met, `false` otherwise

**Exceptions:**
* `ArgumentNullException`: When arguments is null
* `InvalidOperationException`: When the underlying predicate throws

**Example:**
```csharp
var args = new KernelArguments { ["status"] = "approved" };
if (edge.EvaluateCondition(args))
{
    // Traverse to target node
}
```

### EvaluateCondition (GraphState)

```csharp
public bool EvaluateCondition(GraphState graphState)
```

Evaluates the condition using `GraphState` when available, otherwise falls back to the `KernelArguments` predicate.

**Parameters:**
* `graphState`: The graph state to evaluate

**Returns:** `true` if the condition is met, `false` otherwise

**Example:**
```csharp
var state = new GraphState(new KernelArguments { ["score"] = 85 });
if (edge.EvaluateCondition(state))
{
    // Traverse to target node
}
```

## Merge Configuration

### WithMergePolicy

```csharp
public ConditionalEdge WithMergePolicy(StateMergeConflictPolicy defaultPolicy)
```

Configures the edge to use a specific merge policy for all parameters.

**Example:**
```csharp
var edge = ConditionalEdge.CreateUnconditional(source, target)
    .WithMergePolicy(StateMergeConflictPolicy.PreferFirst);
```

### WithMergeConfiguration

```csharp
public ConditionalEdge WithMergeConfiguration(StateMergeConfiguration configuration)
```

Configures the edge with detailed merge settings for parallel branch joins.

**Example:**
```csharp
var config = new StateMergeConfiguration 
{ 
    DefaultPolicy = StateMergeConflictPolicy.Reduce 
};
var edge = ConditionalEdge.CreateUnconditional(source, target)
    .WithMergeConfiguration(config);
```

### WithKeyMergePolicy

```csharp
public ConditionalEdge WithKeyMergePolicy(string key, StateMergeConflictPolicy policy)
```

Configures a merge policy for a specific parameter key.

**Example:**
```csharp
var edge = ConditionalEdge.CreateUnconditional(source, target)
    .WithKeyMergePolicy("userData", StateMergeConflictPolicy.PreferSecond);
```

### WithTypeMergePolicy

```csharp
public ConditionalEdge WithTypeMergePolicy(Type type, StateMergeConflictPolicy policy)
```

Configures a merge policy for a specific .NET type.

**Example:**
```csharp
var edge = ConditionalEdge.CreateUnconditional(source, target)
    .WithTypeMergePolicy(typeof(List<string>), StateMergeConflictPolicy.Reduce);
```

### WithCustomKeyMerger

```csharp
public ConditionalEdge WithCustomKeyMerger(string key, Func<object?, object?, object?> merger)
```

Configures a custom merge function for a specific parameter key.

**Example:**
```csharp
var edge = ConditionalEdge.CreateUnconditional(source, target)
    .WithCustomKeyMerger("counters", (baseVal, overlayVal) => 
    {
        var baseCount = baseVal as int? ?? 0;
        var overlayCount = overlayVal as int? ?? 0;
        return baseCount + overlayCount;
    });
```

### WithReduceSemantics

```csharp
public ConditionalEdge WithReduceSemantics()
```

Configures the edge to use reduce semantics with default reducers for common types.

**Example:**
```csharp
var edge = ConditionalEdge.CreateUnconditional(source, target)
    .WithReduceSemantics();
```

## Validation and Integrity

### ValidateIntegrity

```csharp
public ValidationResult ValidateIntegrity()
```

Validates the edge for potential issues like self-loops and invalid condition functions.

**Returns:** A `ValidationResult` containing warnings and errors

**Example:**
```csharp
var validation = edge.ValidateIntegrity();
if (validation.HasErrors)
{
    foreach (var error in validation.Errors)
    {
        Console.WriteLine($"Edge validation error: {error}");
    }
}
```

## Usage Patterns

### Basic Conditional Routing

```csharp
// Create nodes
var startNode = new FunctionGraphNode(startFunction, "start");
var successNode = new FunctionGraphNode(successFunction, "success");
var failureNode = new FunctionGraphNode(failureFunction, "failure");

// Create conditional edges
var successEdge = new ConditionalEdge(
    startNode, 
    successNode,
    args => args.ContainsKey("result") && (bool)args["result"]
);

var failureEdge = new ConditionalEdge(
    startNode, 
    failureNode,
    args => !args.ContainsKey("result") || !(bool)args["result"]
);

// Add to graph
graph.AddEdge(successEdge);
graph.AddEdge(failureEdge);
```

### Complex State-Based Conditions

```csharp
var decisionEdge = new ConditionalEdge(
    decisionNode,
    actionNode,
    state => 
    {
        var priority = state.GetValue<int>("priority", 0);
        var isUrgent = state.GetValue<bool>("isUrgent", false);
        var hasPermission = state.GetValue<string>("userRole") == "admin";
        
        return priority > 7 || (isUrgent && hasPermission);
    },
    name: "High Priority or Urgent Admin Route"
);
```

### Using Factory Methods

```csharp
// Simple parameter check
var authEdge = ConditionalEdge.CreateParameterExists(
    loginNode, 
    dashboardNode, 
    "authToken"
);

// Value comparison
var premiumEdge = ConditionalEdge.CreateParameterEquals(
    userNode, 
    premiumNode, 
    "subscription", 
    "premium"
);

// Always traversable
var defaultEdge = ConditionalEdge.CreateUnconditional(
    fallbackNode, 
    endNode
);
```

### Merge Configuration for Parallel Branches

```csharp
var mergeEdge = ConditionalEdge.CreateUnconditional(source, target)
    .WithMergePolicy(StateMergeConflictPolicy.Reduce)
    .WithKeyMergePolicy("counters", StateMergeConflictPolicy.Reduce)
    .WithTypeMergePolicy(typeof(List<string>), StateMergeConflictPolicy.Reduce)
    .WithCustomKeyMerger("userData", (baseVal, overlayVal) => 
    {
        // Custom merge logic for user data
        if (baseVal is Dictionary<string, object> baseDict && 
            overlayVal is Dictionary<string, object> overlayDict)
        {
            var merged = new Dictionary<string, object>(baseDict);
            foreach (var kvp in overlayDict)
            {
                merged[kvp.Key] = kvp.Value;
            }
            return merged;
        }
        return overlayVal;
    });
```

## Integration with Graph Executor

### Direct Edge Addition

```csharp
var edge = new ConditionalEdge(sourceNode, targetNode, condition);
graph.AddEdge(edge);
```

### Using ConnectWhen Extension

```csharp
graph.ConnectWhen("sourceNode", "targetNode", 
    args => args.ContainsKey("condition") && (bool)args["condition"],
    "Conditional Route");
```

### Template-Based Routing

```csharp
graph.ConnectWithTemplate("sourceNode", "targetNode", 
    "{{priority}} > 7 && {{isUrgent}} == true",
    templateEngine,
    "High Priority Urgent Route");
```

## Performance Considerations

* **Condition Functions**: Keep predicates fast and side-effect free
* **State Access**: Use `GraphState` methods like `GetValue<T>()` for type-safe access
* **Caching**: Consider caching expensive condition evaluations
* **Validation**: Use `ValidateIntegrity()` during development to catch issues early

## Thread Safety

* Instances are safe for concurrent reads
* The `Metadata` bag and traversal counters are not synchronized
* External synchronization required when multiple threads may mutate concurrently
* Use `RecordTraversal()` for atomic updates when exact counts are important

## Error Handling

* Conditions that throw are wrapped in `InvalidOperationException`
* Use try-catch blocks around condition evaluation in production code
* Validate edge integrity before adding to graphs
* Monitor traversal metrics for unexpected behavior

## See Also

* [Conditional Nodes Guide](../how-to/conditional-nodes.md)
* [State Management](../concepts/state.md)
* [Graph Execution Model](../concepts/execution-model.md)
* [Routing Concepts](../concepts/routing.md)
* [StateMergeConfiguration](state-merge-configuration.md)
* [StateMergeConflictPolicy](state-merge-conflict-policy.md)
