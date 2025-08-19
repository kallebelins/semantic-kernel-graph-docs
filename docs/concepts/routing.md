# Routing

Routing determines which node will be executed next using conditional edges or dynamic strategies.

## Concepts and Techniques

**Routing**: Process of determining the next node to execute based on conditions, state or dynamic strategies.

**Conditional Edge**: Connection between nodes that only allows passage when a specific condition is met.

**Routing Strategy**: Algorithm or logic that decides the execution path based on predefined criteria.

## Routing Types

### Simple Predicate Routing
- **State Conditions**: Direct evaluation of `GraphState` properties
- **Boolean Expressions**: Simple conditions like `state.Value > 10`
- **Comparisons**: Equality, inequality and range operators

### Template-Based Routing
- **SK Evaluation**: Use of Semantic Kernel functions for complex decisions
- **Prompt-based Routing**: Decisions based on text or context analysis
- **Semantic Matching**: Routing by semantic similarity

### Advanced Routing
- **Semantic Similarity**: Use of embeddings to find the best path
- **Probabilistic Routing**: Decisions with weights and probabilities
- **Learning from Feedback**: Adaptation based on previous results

## Main Components

### ConditionalEdge
```csharp
var edge = new ConditionalEdge(
    sourceNode: nodeA,
    targetNode: nodeB,
    condition: state => state.GetValue<int>("counter") > 5
);
```

### DynamicRoutingEngine
```csharp
var routingEngine = new DynamicRoutingEngine(
    strategies: new[] { new SemanticRoutingStrategy() },
    fallbackStrategy: new DefaultRoutingStrategy()
);
```

### RoutingStrategies
- **SemanticRoutingStrategy**: Routing by semantic similarity
- **ProbabilisticRoutingStrategy**: Routing with probabilistic weights
- **ContextualRoutingStrategy**: Routing based on execution history

## Usage Examples

### Simple Conditional Routing
```csharp
// Routing based on numeric value
var edge = new ConditionalEdge(
    sourceNode: processNode,
    targetNode: successNode,
    condition: state => state.GetValue<int>("status") == 200
);

var failureEdge = new ConditionalEdge(
    sourceNode: processNode,
    targetNode: failureNode,
    condition: state => state.GetValue<int>("status") != 200
);
```

### Template-Based Routing
```csharp
// Routing based on text analysis
var routingNode = new ConditionalGraphNode(
    condition: async (state) => {
        var text = state.GetValue<string>("input");
        var result = await kernel.InvokeAsync("analyze_sentiment", text);
        return result.GetValue<string>("sentiment") == "positive";
    }
);
```

### Dynamic Routing
```csharp
// Adaptive routing based on metrics
var dynamicRouter = new DynamicRoutingEngine(
    strategies: new[] {
        new PerformanceBasedRoutingStrategy(),
        new LoadBalancingRoutingStrategy()
    }
);
```

## Configuration and Options

### GraphRoutingOptions
```csharp
var options = new GraphRoutingOptions
{
    EnableDynamicRouting = true,
    MaxRoutingAttempts = 3,
    RoutingTimeout = TimeSpan.FromSeconds(30),
    FallbackStrategy = RoutingFallbackStrategy.Random
};
```

### Routing Policies
- **Retry Policy**: Multiple retries in case of failure
- **Circuit Breaker**: Temporary interruption in case of problems
- **Load Balancing**: Balanced load distribution

## Monitoring and Debugging

### Routing Metrics
- **Decision Time**: Latency to determine the next node
- **Success Rate**: Percentage of successful routings
- **Path Distribution**: Frequency of use of each route

### Routing Debugging
```csharp
var debugger = new ConditionalDebugger();
debugger.EnableRoutingLogging = true;
debugger.LogRoutingDecisions = true;
```

## See Also

- [Conditional Nodes](../how-to/conditional-nodes.md)
- [Advanced Routing](../how-to/advanced-routing.md)
- [Routing Examples](../examples/dynamic-routing.md)
- [Advanced Routing Examples](../examples/advanced-routing.md)
- [Nodes](../concepts/node-types.md)
- [Execution](../concepts/execution-model.md)

## References

- `ConditionalEdge`: Class to create edges with conditions
- `DynamicRoutingEngine`: Adaptive routing engine
- `RoutingStrategies`: Predefined routing strategies
- `GraphRoutingOptions`: Routing configurations
- `ConditionalDebugger`: Debugging tools for routing
