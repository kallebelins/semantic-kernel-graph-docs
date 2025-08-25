# Conditional nodes

This guide explains how to use conditional nodes and edges to create dynamic, branching workflows in SemanticKernel.Graph. You'll learn how to implement decision logic, route execution flow, and create complex workflow patterns.

## Overview

Conditional nodes enable you to create dynamic workflows that can:
* **Branch execution** based on state values or conditions
* **Route data** to different processing paths
* **Implement decision trees** for complex business logic
* **Handle multiple scenarios** within a single graph

## Basic Conditional Logic

### Simple Predicate-Based Branching

Use predicates to evaluate conditions and route execution:

```csharp
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Nodes;

// Create a conditional node with a predicate.
// The predicate safely checks for the presence of the "ok" key and
// validates it is a boolean before returning the value.
var conditionalNode = new ConditionalGraphNode(
    predicate: state =>
    {
        if (state.TryGetValue("ok", out var okObj) && okObj is bool ok)
        {
            return ok;
        }

        return false; // default when key is missing or not a boolean
    },
    nodeId: "decision_point"
);

// Add conditional edges based on the result using safe checks.
graph.AddConditionalEdge(
    "decision_point",
    "success_path",
    condition: state => state.TryGetValue("ok", out var okObj) && okObj is bool ok && ok)
.AddConditionalEdge(
    "decision_point",
    "fallback_path",
    condition: state => !(state.TryGetValue("ok", out var okObj2) && okObj2 is bool ok2 && ok2));
```

### Template-Based Conditions

Use template expressions for more complex conditional logic:

```csharp
// Template-based condition example.
// Use typed accessors and local variables for clarity and easier debugging.
var templateCondition = new ConditionalGraphNode(
    predicate: state =>
    {
        var score = state.GetInt("confidence_score", 0);
        var threshold = state.GetInt("threshold", 50);
        return score >= threshold;
    },
    nodeId: "confidence_check"
);

// Route based on confidence level with clear thresholds.
graph.AddConditionalEdge(
        "confidence_check",
        "high_confidence",
        condition: state => state.GetInt("confidence_score", 0) >= 80)
    .AddConditionalEdge(
        "confidence_check",
        "medium_confidence",
        condition: state =>
        {
            var score = state.GetInt("confidence_score", 0);
            return score >= 50 && score < 80;
        })
    .AddConditionalEdge(
        "confidence_check",
        "low_confidence",
        condition: state => state.GetInt("confidence_score", 0) < 50);
```

## Advanced Conditional Patterns

### Multi-Way Branching

Create complex decision trees with multiple paths:

```csharp
// Multi-way branching: ensure predicate returns a boolean and checks for null/empty.
var priorityNode = new ConditionalGraphNode(
    predicate: state => !string.IsNullOrEmpty(state.GetString("priority")),
    nodeId: "priority_router"
);

// Route to different processing paths based on priority value.
graph.AddConditionalEdge(
        "priority_router",
        "urgent_processing",
        condition: state => string.Equals(state.GetString("priority"), "urgent", StringComparison.OrdinalIgnoreCase))
    .AddConditionalEdge(
        "priority_router",
        "normal_processing",
        condition: state => string.Equals(state.GetString("priority"), "normal", StringComparison.OrdinalIgnoreCase))
    .AddConditionalEdge(
        "priority_router",
        "low_priority_processing",
        condition: state => string.Equals(state.GetString("priority"), "low", StringComparison.OrdinalIgnoreCase))
    .AddConditionalEdge(
        "priority_router",
        "default_processing",
        condition: state =>
        {
            var value = state.GetString("priority");
            return string.IsNullOrEmpty(value) || !(new[] { "urgent", "normal", "low" }.Contains(value, StringComparer.OrdinalIgnoreCase));
        });
```

### State-Dependent Routing

Route execution based on multiple state conditions:

```csharp
// State-dependent routing: keep checks explicit and readable.
var analysisNode = new ConditionalGraphNode(
    predicate: state =>
    {
        var hasData = state.ContainsKey("input_data");
        var dataSize = state.GetInt("data_size", 0);
        // complexity defaults to "simple" when missing
        var complexity = state.GetString("complexity", "simple");

        return hasData && dataSize > 0;
    },
    nodeId: "analysis_decision"
);

// Complex routing logic with clear thresholds and explicit comparisons.
graph.AddConditionalEdge(
        "analysis_decision",
        "deep_analysis",
        condition: state =>
        {
            var dataSize = state.GetInt("data_size", 0);
            var complexity = state.GetString("complexity", "simple");
            return dataSize > 1000 && string.Equals(complexity, "complex", StringComparison.OrdinalIgnoreCase);
        })
    .AddConditionalEdge(
        "analysis_decision",
        "standard_analysis",
        condition: state =>
        {
            var dataSize = state.GetInt("data_size", 0);
            var complexity = state.GetString("complexity", "simple");
            return dataSize > 100 && !string.Equals(complexity, "complex", StringComparison.OrdinalIgnoreCase);
        })
    .AddConditionalEdge(
        "analysis_decision",
        "quick_analysis",
        condition: state => state.GetInt("data_size", 0) <= 100);
```

### Dynamic Thresholds

Use state values to determine dynamic thresholds:

```csharp
// Dynamic thresholds: calculate and use a threshold derived from state.
var adaptiveNode = new ConditionalGraphNode(
    predicate: state =>
    {
        var baseThreshold = state.GetInt("base_threshold", 50);
        var multiplier = state.GetFloat("threshold_multiplier", 1.0f);
        var dynamicThreshold = (int)Math.Round(baseThreshold * multiplier);

        var currentValue = state.GetInt("current_value", 0);
        // Return true when current value meets or exceeds the dynamic threshold
        return currentValue >= dynamicThreshold;
    },
    nodeId: "adaptive_threshold"
);

// Store the calculated threshold for later use in the graph state.
graph.AddEdge("adaptive_threshold", "store_threshold");
```

## Conditional Edge Types

### Boolean Conditions

Simple true/false routing:

```csharp
// Boolean condition routing: prefer explicit default values and safe access.
graph.AddConditionalEdge(
    "start",
    "success",
    condition: state => state.GetBool("is_valid", false))
.AddConditionalEdge(
    "start",
    "failure",
    condition: state => !state.GetBool("is_valid", false));
```

### Numeric Comparisons

Route based on numeric values:

```csharp
// Numeric comparisons with clear threshold definitions.
graph.AddConditionalEdge(
    "start",
    "high_priority",
    condition: state => state.GetInt("priority", 0) > 7)
.AddConditionalEdge(
    "start",
    "medium_priority",
    condition: state =>
    {
        var priority = state.GetInt("priority", 0);
        return priority > 3 && priority <= 7;
    })
.AddConditionalEdge(
    "start",
    "low_priority",
    condition: state => state.GetInt("priority", 0) <= 3);
```

### String Matching

Route based on string values and patterns:

```csharp
// String matching: use StringComparison and guard against nulls.
graph.AddConditionalEdge(
    "start",
    "email_processing",
    condition: state => state.GetString("input_type", string.Empty).IndexOf("email", StringComparison.OrdinalIgnoreCase) >= 0)
.AddConditionalEdge(
    "start",
    "document_processing",
    condition: state => state.GetString("input_type", string.Empty).IndexOf("document", StringComparison.OrdinalIgnoreCase) >= 0)
.AddConditionalEdge(
    "start",
    "image_processing",
    condition: state => state.GetString("input_type", string.Empty).IndexOf("image", StringComparison.OrdinalIgnoreCase) >= 0);
```

### Complex Logical Expressions

Combine multiple conditions with logical operators:

```csharp
graph.AddConditionalEdge("start", "premium_processing", 
    condition: state => {
        var isPremium = state.GetBool("is_premium_user", false);
        var hasCredits = state.GetInt("credits", 0) > 0;
        var isBusinessHours = IsBusinessHours(state.GetDateTime("timestamp"));
        
        return isPremium && hasCredits && isBusinessHours;
    })
.AddConditionalEdge("start", "standard_processing", 
    condition: state => {
        var isPremium = state.GetBool("is_premium_user", false);
        var hasCredits = state.GetInt("credits", 0) > 0;
        
        return !isPremium || !hasCredits;
    });
```

## Performance Optimization

### Caching Evaluations

Cache conditional evaluations for better performance:

```csharp
// Caching evaluations: store expensive computation results in the state.
var cachedCondition = new ConditionalGraphNode(
    predicate: state =>
    {
        // Cache the result to avoid repeated calculations
        if (state.TryGetValue("cached_decision", out var cached) && cached is bool cachedBool)
        {
            return cachedBool;
        }

        var result = ExpensiveCalculation(state);
        state["cached_decision"] = result;
        return result;
    },
    nodeId: "cached_decision"
);
```

### Lazy Evaluation

Use lazy evaluation for expensive conditions:

```csharp
// Lazy evaluation: perform expensive checks only when necessary and cache the result.
var lazyCondition = new ConditionalGraphNode(
    predicate: state =>
    {
        // Only evaluate if we haven't already decided
        if (state.ContainsKey("route_decided") && state.GetBool("route_decided", false))
        {
            return state.GetBool("route_decided", false);
        }

        // Perform expensive evaluation and cache the result
        var result = EvaluateComplexCondition(state);
        state["route_decided"] = result;
        return result;
    },
    nodeId: "lazy_evaluation"
);
```

## Debug and Inspection

### Conditional Debugging

Add debug information to conditional nodes:

```csharp
// Conditional debugging: store structured debug info in graph state for inspection.
var debugCondition = new ConditionalGraphNode(
    predicate: state =>
    {
        var condition = state.GetString("condition", string.Empty);
        var value = state.GetInt("value", 0);
        var threshold = state.GetInt("threshold", 50);

        var result = value >= threshold;

        // Log the decision for debugging in a structured object
        state["debug_info"] = new
        {
            Condition = condition,
            Value = value,
            Threshold = threshold,
            Result = result,
            Timestamp = DateTimeOffset.UtcNow
        };

        return result;
    },
    nodeId: "debug_condition"
);
```

### Decision Tracing

Track decision paths through your workflow:

```csharp
// Decision tracing: append decision entries to a history list stored in state.
var traceCondition = new ConditionalGraphNode(
    predicate: state =>
    {
        var decision = EvaluateCondition(state);

        // Add to decision history (initialize list if missing)
        var history = state.GetValue<List<string>>("decision_history") ?? new List<string>();
        history.Add($"Node: trace_condition, Decision: {decision}, Timestamp: {DateTimeOffset.UtcNow}");
        state["decision_history"] = history;

        return decision;
    },
    nodeId: "trace_condition"
);
```

## Best Practices

### Condition Design

1. **Keep conditions simple** - Complex logic should be in separate functions
2. **Use meaningful names** - Name conditions to reflect their purpose
3. **Handle edge cases** - Always provide fallback paths
4. **Validate inputs** - Check for required state values before evaluation

### Performance Considerations

1. **Cache expensive evaluations** - Store results to avoid recalculation
2. **Use lazy evaluation** - Only evaluate when necessary
3. **Optimize state access** - Minimize state lookups in conditions
4. **Batch decisions** - Group related conditions when possible

### Error Handling

1. **Provide fallbacks** - Always have a default path
2. **Validate state** - Check for required values before evaluation
3. **Log decisions** - Track decision paths for debugging
4. **Handle exceptions** - Wrap conditions in try-catch blocks

## Troubleshooting

### Common Issues

**Conditions not evaluating**: Check that state contains required values and types match

**Unexpected routing**: Verify condition logic and add debug logging

**Performance problems**: Cache expensive evaluations and optimize state access

**Infinite loops**: Ensure conditions eventually resolve to a terminal state

### Debugging Tips

1. **Enable debug logging** to trace condition evaluation
2. **Add decision tracing** to track execution paths
3. **Use breakpoints** in conditional logic
4. **Inspect state values** at decision points

## Concepts and Techniques

**ConditionalGraphNode**: A specialized graph node that evaluates predicates to determine execution flow. It enables dynamic routing based on the current state of the graph.

**ConditionalEdge**: A connection between nodes that includes a condition for execution. It allows for complex branching logic and dynamic workflow paths based on state evaluation.

**Predicate**: A function that evaluates the current graph state and returns a boolean value. Predicates determine which execution path to take in conditional routing.

**Template Expression**: A more complex conditional expression that can evaluate multiple state values and perform calculations to determine routing decisions.

**State-Dependent Routing**: A pattern where execution paths are determined dynamically based on the current values in the graph state, enabling adaptive workflows.

## See Also

* [Build a Graph](build-a-graph.md) - Learn the fundamentals of graph construction
* [Loops](loops.md) - Implement iterative workflows with loop nodes
* [Advanced Routing](advanced-routing.md) - Explore complex routing patterns and strategies
* [State Management](../state-quickstart.md) - Understand how to manage data flow between nodes
* [Debug and Inspection](debug-and-inspection.md) - Learn how to debug conditional logic
* [Examples: Conditional Workflows](../examples/conditional-nodes-example.md) - Complete working examples of conditional routing
