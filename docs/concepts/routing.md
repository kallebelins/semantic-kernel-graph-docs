# Routing

Routing determines which node will be executed next using conditional edges or dynamic strategies.

## Concepts and Techniques

**Routing**: Process of determining the next node to execute based on conditions, state or dynamic strategies.

**Conditional Edge**: Connection between nodes that only allows passage when a specific condition is met.

**Routing Strategy**: Algorithm or logic that decides the execution path based on predefined criteria.

## Routing Types

### Simple Predicate Routing
* **State Conditions**: Direct evaluation of `GraphState` properties
* **Boolean Expressions**: Simple conditions like `state.Value > 10`
* **Comparisons**: Equality, inequality and range operators

### Template-Based Routing
* **SK Evaluation**: Use of Semantic Kernel functions for complex decisions
* **Prompt-based Routing**: Decisions based on text or context analysis
* **Semantic Matching**: Routing by semantic similarity

### Advanced Routing
* **Semantic Similarity**: Use of embeddings to find the best path
* **Probabilistic Routing**: Decisions with weights and probabilities
* **Learning from Feedback**: Adaptation based on previous results

## Main Components

### ConditionalEdge / Conditional Routing Node
```csharp
// Example using the project's conditional routing node implementation
var conditionalNode = new ConditionalNodeExample(
    nodeId: "checkStatus",
    name: "CheckStatus",
    condition: args => args.Get<int>("status") == 200
);

// Add next nodes that should be selected when the condition is true
var successNode = new FunctionGraphNode(
    kernel.CreateFunctionFromMethod(() => "Success", functionName: "SuccessHandler"),
    "success"
);
conditionalNode.AddNextNode(successNode);
```

### DynamicRoutingEngine
```csharp
// Create a template engine (optional) and a dynamic routing engine with options
var templateEngine = new HandlebarsGraphTemplateEngine(new GraphTemplateOptions
{
    EnableHandlebars = true,
    EnableCustomHelpers = true,
    TemplateCacheSize = 100
});

var routingEngine = new DynamicRoutingEngine(
    templateEngine: templateEngine,
    options: new DynamicRoutingOptions
    {
        EnableCaching = true,
        EnableFallback = true,
        MaxCacheSize = 500,
        CacheExpirationMinutes = 60
    },
    logger: null,
    embeddingService: null,
    memoryService: null
);
```

### RoutingStrategies
* **SemanticRoutingStrategy**: Routing by semantic similarity
* **ProbabilisticRoutingStrategy**: Routing with probabilistic weights
* **ContextualRoutingStrategy**: Routing based on execution history

## Usage Examples

### Simple Conditional Routing
```csharp
// Routing based on a numeric value: use a conditional routing node together
// with normal FunctionGraphNode instances. This mirrors the examples.
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddGraphSupport();
var kernel = kernelBuilder.Build();

var startNode = new FunctionGraphNode(
    kernel.CreateFunctionFromMethod((string input) => $"Processed: {input}", functionName: "ProcessInput"),
    "start"
).StoreResultAs("processed_input");

var conditional = new ConditionalNodeExample("cond", "IsSuccess", args => args.Get<int>("status") == 200);
var successNode = new FunctionGraphNode(
    kernel.CreateFunctionFromMethod(() => "OK", functionName: "Success"),
    "success"
);

conditional.AddNextNode(successNode);
// Add nodes to a GraphExecutor and set the start node to wire this routing.
```

### Template-Based Routing
```csharp
// Template-based routing using the project's template engine (Handlebars example)
var templateEngine = new HandlebarsGraphTemplateEngine(new GraphTemplateOptions { EnableHandlebars = true });
var routingEngine = new DynamicRoutingEngine(templateEngine: templateEngine, options: new DynamicRoutingOptions { EnableCaching = true });

var template = "{{#if (eq priority 'high')}}high{{else}}low{{/if}}";
var context = new KernelArguments { ["priority"] = "high" };
var rendered = await templateEngine.RenderWithArgumentsAsync(template, context, CancellationToken.None);
// Use the rendered value to decide which node to route to.
```

### Dynamic Routing
```csharp
// Adaptive routing using the DynamicRoutingEngine. In practice you may
// combine multiple strategies (performance, load, semantic similarity).
var dynamicRouter = new DynamicRoutingEngine(
    options: new DynamicRoutingOptions { EnableCaching = true }
);

// Real deployments would add or configure strategies such as
// PerformanceBasedRoutingStrategy or LoadBalancingRoutingStrategy.
```

## Configuration and Options

### Routing Options
```csharp
// Use the project's dynamic routing options for configuration
var options = new DynamicRoutingOptions
{
    EnableCaching = true,
    EnableFallback = true,
    MaxCacheSize = 500,
    CacheExpirationMinutes = 60
};
```

### Routing Policies
* **Retry Policy**: Multiple retries in case of failure
* **Circuit Breaker**: Temporary interruption in case of problems
* **Load Balancing**: Balanced load distribution

## Monitoring and Debugging

### Routing Metrics
* **Decision Time**: Latency to determine the next node
* **Success Rate**: Percentage of successful routings
* **Path Distribution**: Frequency of use of each route

### Routing Debugging
```csharp
// Enable logging/diagnostics on your routing engine or use the project's
// debug helpers (if available) to trace decisions.
// Example: enable verbose logs from the routing engine via your logger
// or inspect routingEngine internals in tests.
```

## See Also

* [Conditional Nodes](../how-to/conditional-nodes.md)
* [Advanced Routing](../how-to/advanced-routing.md)
* [Routing Examples](../examples/dynamic-routing.md)
* [Advanced Routing Examples](../examples/advanced-routing.md)
* [Nodes](../concepts/node-types.md)
* [Execution](../concepts/execution-model.md)

## References

* `ConditionalEdge`: Class to create edges with conditions
* `DynamicRoutingEngine`: Adaptive routing engine
* `RoutingStrategies`: Predefined routing strategies
* `GraphRoutingOptions`: Routing configurations
* `ConditionalDebugger`: Debugging tools for routing
