# Advanced Routing Example

This example demonstrates advanced routing capabilities in Semantic Kernel Graph, including semantic routing, content similarity, probabilistic routing, contextual routing, and feedback learning.

## Objective

Learn how to implement advanced routing strategies in graph-based workflows to:
- Use semantic routing with embeddings for content-aware decisions
- Implement similarity-based routing using historical execution patterns
- Configure probabilistic routing with dynamic weights
- Enable contextual routing based on execution history and state
- Implement feedback learning to improve routing decisions over time

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- **Text embedding service** configured (OpenAI, Azure OpenAI, or local)
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Routing](../concepts/routing.md)
- Familiarity with [Dynamic Routing](../how-to/advanced-routing.md)

## Key Components

### Concepts and Techniques

- **Semantic Routing**: Content-aware routing using text embeddings and similarity
- **Similarity Routing**: Routing based on historical execution patterns and outcomes
- **Probabilistic Routing**: Dynamic routing with weighted probabilities and learning
- **Contextual Routing**: Routing decisions based on execution context and state
- **Feedback Learning**: Continuous improvement of routing decisions through feedback

### Core Classes

- `DynamicRoutingEngine`: Advanced routing engine with multiple strategies
- `ITextEmbeddingGenerationService`: Service for generating text embeddings
- `IGraphMemoryService`: Service for storing and retrieving routing history
- `GraphExecutor`: Enhanced executor with advanced routing capabilities
- `FunctionGraphNode`: Nodes that can be routed using advanced strategies

## Running the Example

### Getting Started

This example demonstrates advanced routing and decision-making with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Creating Advanced Routing Graph

The example starts by creating a graph optimized for demonstrating advanced routing scenarios.

```csharp
var graph = new GraphExecutor(kernel, logger: null);

// Create nodes that simulate different types of decision points
var startNode = new FunctionGraphNode(
    CreateAnalysisFunction(kernel, "Analyze the input and determine the best processing approach"),
    nodeId: "start",
    description: "Analyzes input and determines processing path");

var semanticNode = new FunctionGraphNode(
    CreateProcessingFunction(kernel, "Apply semantic processing and understanding"),
    nodeId: "semantic",
    description: "Processes content using semantic understanding and natural language analysis");

var statisticalNode = new FunctionGraphNode(
    CreateProcessingFunction(kernel, "Apply statistical processing and analysis"),
    nodeId: "statistical",
    description: "Processes content using statistical methods and numerical analysis");

var hybridNode = new FunctionGraphNode(
    CreateProcessingFunction(kernel, "Apply hybrid semantic and statistical processing"),
    nodeId: "hybrid",
    description: "Combines semantic and statistical approaches for comprehensive analysis");

var errorHandlerNode = new FunctionGraphNode(
    CreateErrorHandlerFunction(kernel),
    nodeId: "error",
    description: "Handles errors and provides fallback processing");

var summaryNode = new FunctionGraphNode(
    CreateSummaryFunction(kernel),
    nodeId: "summary",
    description: "Generates final summary and results");

// Add nodes to graph
graph.AddNode(startNode);
graph.AddNode(semanticNode);
graph.AddNode(statisticalNode);
graph.AddNode(hybridNode);
graph.AddNode(errorHandlerNode);
graph.AddNode(summaryNode);
graph.SetStartNode(startNode.NodeId);

// Create edges that will benefit from advanced routing
graph.ConnectWhen(startNode.NodeId, semanticNode.NodeId, state => ContainsTextContent(new GraphState(state)));
graph.ConnectWhen(startNode.NodeId, statisticalNode.NodeId, state => ContainsNumericalContent(new GraphState(state)));
graph.ConnectWhen(startNode.NodeId, hybridNode.NodeId, state => ContainsComplexContent(new GraphState(state)));
graph.ConnectWhen(startNode.NodeId, errorHandlerNode.NodeId, state => HasErrors(new GraphState(state)));

// All processing nodes can go to summary
graph.Connect(semanticNode.NodeId, summaryNode.NodeId);
graph.Connect(statisticalNode.NodeId, summaryNode.NodeId);
graph.Connect(hybridNode.NodeId, summaryNode.NodeId);
graph.Connect(errorHandlerNode.NodeId, summaryNode.NodeId);
```

### 2. Configuring Advanced Routing Engine

```csharp
// Create advanced routing engine with all capabilities
var typedLogger = kernel.Services.GetService<ILogger<DynamicRoutingEngine>>();
var routingEngine = new DynamicRoutingEngine(
    templateEngine: null,
    options: new DynamicRoutingOptions { EnableCaching = true, EnableFallback = true },
    logger: typedLogger,
    embeddingService: embeddingService,
    memoryService: memoryService);

// Configure the graph to use advanced routing
graph.RoutingEngine = routingEngine;

logger.LogInformation("Advanced routing enabled: {IsEnabled}", routingEngine.IsAdvancedRoutingEnabled);
```

### 3. Semantic Routing Demonstration

Semantic routing uses embeddings to make content-aware routing decisions.

```csharp
var semanticQueries = new[]
{
    "Analyze the emotional sentiment of this customer feedback: 'I love this product!'",
    "Calculate the mean and standard deviation of this dataset: [1, 2, 3, 4, 5]",
    "Process this complex research paper that combines qualitative interviews with quantitative surveys",
    "Handle this error: connection timeout occurred"
};

foreach (var query in semanticQueries)
{
    logger.LogInformation("Processing query: {Query}", query);

    var args = new KernelArguments { ["input"] = query };
    var result = await graph.ExecuteAsync(kernel, args);

    logger.LogInformation("Result: {Result}", result);
}
```

### 4. Similarity Routing Demonstration

Similarity routing uses historical execution patterns to make routing decisions.

```csharp
// Execute similar patterns to build history
var similarPatterns = new[]
{
    ("customer_feedback", "positive"),
    ("customer_feedback", "negative"),
    ("customer_feedback", "neutral"),
    ("data_analysis", "statistical"),
    ("data_analysis", "visualization")
};

foreach (var (category, type) in similarPatterns)
{
    var args = new KernelArguments
    {
        ["category"] = category,
        ["type"] = type,
        ["input"] = $"Process {category} of {type} nature"
    };

    logger.LogInformation("Executing pattern: {Category} - {Type}", category, type);
    await graph.ExecuteAsync(kernel, args);
}

logger.LogInformation("Similarity patterns established for future routing decisions");
```

### 5. Probabilistic Routing Demonstration

Probabilistic routing uses dynamic weights and learning to make routing decisions.

```csharp
// Execute multiple similar scenarios to show probabilistic selection
for (int i = 0; i < 10; i++)
{
    var args = new KernelArguments
    {
        ["input"] = $"Process customer feedback iteration {i}",
        ["priority"] = i % 3 == 0 ? "high" : "normal",
        ["category"] = "customer_feedback"
    };

    logger.LogInformation("Executing probabilistic routing iteration {Iteration}", i);
    var result = await graph.ExecuteAsync(kernel, args);
    
    // Simulate feedback for learning
    var feedback = new RoutingFeedback
    {
        ExecutionId = result.ExecutionId,
        RouteSelected = result.RouteTaken,
        Success = Random.Shared.Next(100) < 85, // 85% success rate
        Performance = TimeSpan.FromMilliseconds(Random.Shared.Next(100, 500))
    };
    
    await routingEngine.ProvideFeedbackAsync(feedback);
}
```

### 6. Contextual Routing Demonstration

Contextual routing considers execution history and current state for routing decisions.

```csharp
// Execute with different contexts to show contextual routing
var contexts = new[]
{
    new { TimeOfDay = "morning", Load = "low", Priority = "normal" },
    new { TimeOfDay = "afternoon", Load = "high", Priority = "urgent" },
    new { TimeOfDay = "evening", Load = "medium", Priority = "high" }
};

foreach (var context in contexts)
{
    var args = new KernelArguments
    {
        ["input"] = "Process customer request",
        ["time_of_day"] = context.TimeOfDay,
        ["system_load"] = context.Load,
        ["priority"] = context.Priority
    };

    logger.LogInformation("Executing with context: {Context}", context);
    var result = await graph.ExecuteAsync(kernel, args);
    
    // Show how context influenced routing
    logger.LogInformation("Route taken: {Route} based on context {Context}", 
        result.RouteTaken, context);
}
```

### 7. Feedback Learning Demonstration

Feedback learning continuously improves routing decisions based on execution outcomes.

```csharp
// Simulate feedback collection and learning
var feedbackBatch = new List<RoutingFeedback>();

for (int i = 0; i < 20; i++)
{
    var args = new KernelArguments
    {
        ["input"] = $"Learning iteration {i}",
        ["category"] = i % 4 == 0 ? "urgent" : "normal",
        ["complexity"] = i % 3 == 0 ? "high" : "low"
    };

    var result = await graph.ExecuteAsync(kernel, args);
    
    // Collect feedback
    var feedback = new RoutingFeedback
    {
        ExecutionId = result.ExecutionId,
        RouteSelected = result.RouteTaken,
        Success = Random.Shared.Next(100) < 90, // 90% success rate
        Performance = TimeSpan.FromMilliseconds(Random.Shared.Next(50, 300)),
        UserSatisfaction = Random.Shared.Next(1, 6) // 1-5 scale
    };
    
    feedbackBatch.Add(feedback);
}

// Provide batch feedback for learning
await routingEngine.ProvideBatchFeedbackAsync(feedbackBatch);
logger.LogInformation("Provided feedback for {Count} executions", feedbackBatch.Count);
```

### 8. Routing Analytics and Insights

The example concludes by displaying comprehensive routing analytics.

```csharp
// Show analytics
await DisplayRoutingAnalyticsAsync(routingEngine, logger);

// Cleanup
await routingEngine.DisposeAsync();
logger.LogInformation("=== Advanced Routing Demonstration Complete ===");
```

## Expected Output

The example produces comprehensive output showing:

- ✅ Advanced routing graph creation with multiple node types
- 🔀 Semantic routing decisions based on content analysis
- 📊 Similarity routing using historical patterns
- 🎲 Probabilistic routing with dynamic weights
- 🧠 Contextual routing based on execution context
- 📈 Feedback learning and continuous improvement
- 📋 Comprehensive routing analytics and insights

## Troubleshooting

### Common Issues

1. **Embedding Service Errors**: Ensure text embedding service is properly configured
2. **Memory Service Failures**: Check memory service configuration and connectivity
3. **Routing Decision Failures**: Verify routing conditions and edge configurations
4. **Performance Issues**: Monitor routing decision timing and optimize thresholds

### Debugging Tips

- Enable detailed logging to trace routing decisions
- Monitor similarity scores and confidence levels
- Check feedback collection and learning progress
- Verify contextual routing conditions and state

## See Also

- [Advanced Routing](../how-to/advanced-routing.md)
- [Dynamic Routing](../how-to/dynamic-routing.md)
- [Graph Concepts](../concepts/graph-concepts.md)
- [Routing](../concepts/routing.md)
- [State Management](../concepts/state.md)
