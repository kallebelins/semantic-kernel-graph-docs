# Advanced Routing

Advanced routing in SemanticKernel.Graph goes beyond simple conditional edges to provide intelligent, context-aware node selection using multiple strategies. This guide covers the sophisticated routing capabilities that enable dynamic, adaptive graph execution.

## Overview

Advanced routing combines multiple strategies to make intelligent decisions about which node to execute next:

- **Semantic Routing**: Uses embeddings to find semantically similar nodes
- **Content Similarity**: Leverages execution history to find similar patterns
- **Probabilistic Routing**: Applies weighted random selection with dynamic weights
- **Contextual Routing**: Considers execution history patterns and transitions
- **Feedback Learning**: Adapts routing decisions based on user feedback

## Core Components

### AdvancedRoutingEngine

The main orchestrator that coordinates all routing strategies:

```csharp
var advancedRoutingEngine = new AdvancedRoutingEngine(
    embeddingService: embeddingService,
    memoryService: memoryService,
    options: new AdvancedRoutingOptions
    {
        EnableSemanticRouting = true,
        EnableSimilarityRouting = true,
        EnableProbabilisticRouting = true,
        EnableContextualRouting = true,
        EnableFeedbackLearning = true
    }
);
```

### DynamicRoutingEngine Integration

The `DynamicRoutingEngine` automatically integrates advanced routing when embedding or memory services are available:

```csharp
var routingEngine = new DynamicRoutingEngine(
    templateEngine: null,
    options: new DynamicRoutingOptions { EnableCaching = true, EnableFallback = true },
    logger: logger,
    embeddingService: embeddingService,
    memoryService: memoryService
);

// Configure graph to use advanced routing
graph.RoutingEngine = routingEngine;
```

## Routing Strategies

### 1. Semantic Routing Strategy

Uses text embeddings to find semantically similar nodes based on context:

```csharp
public sealed class SemanticRoutingStrategy : IRoutingStrategy
{
    public RoutingStrategyType Type => RoutingStrategyType.Semantic;
    public double GetWeight() => 0.3; // High weight for semantic similarity
    
    // Selects node based on embedding similarity
    public async Task<RoutingStrategyResult?> SelectNodeAsync(
        List<IGraphNode> candidates,
        AdvancedRoutingContext context,
        CancellationToken cancellationToken = default)
}
```

**How it works:**
1. Generates embedding for current execution context
2. Retrieves or creates embeddings for candidate nodes
3. Calculates cosine similarity between context and node embeddings
4. Selects node with highest similarity above threshold (default: 0.5)

**Configuration:**
```csharp
var options = new AdvancedRoutingOptions
{
    EnableSemanticRouting = true,
    SemanticSimilarityThreshold = 0.7 // Adjustable threshold
};
```

### 2. Content Similarity Routing Strategy

Leverages execution history to find nodes that performed well in similar situations:

```csharp
public sealed class ContentSimilarityRoutingStrategy : IRoutingStrategy
{
    public RoutingStrategyType Type => RoutingStrategyType.Similarity;
    public double GetWeight() => 0.25;
    
    // Selects node based on historical success in similar contexts
    public async Task<RoutingStrategyResult?> SelectNodeAsync(
        List<IGraphNode> candidates,
        AdvancedRoutingContext context,
        CancellationToken cancellationToken = default)
}
```

**How it works:**
1. Analyzes similar executions from memory service
2. Calculates state similarity between current and historical executions
3. Identifies nodes with high frequency and success rates
4. Selects node with best combination of frequency and success rate

**Requirements:**
- `IGraphMemoryService` implementation
- Similar executions in context (`context.SimilarExecutions`)

### 3. Probabilistic Routing Strategy

Applies weighted random selection with dynamically adjusted weights:

```csharp
public sealed class ProbabilisticRoutingStrategy : IRoutingStrategy
{
    public RoutingStrategyType Type => RoutingStrategyType.Probabilistic;
    public double GetWeight() => 0.2;
    
    // Selects node using weighted random with dynamic weights
    public async Task<RoutingStrategyResult?> SelectNodeAsync(
        List<IGraphNode> candidates,
        AdvancedRoutingContext context,
        CancellationToken cancellationToken = default)
}
```

**How it works:**
1. Calculates dynamic weights based on historical performance
2. Applies decay factor to prevent over-reliance on old data
3. Normalizes weights and performs weighted random selection
4. Adapts weights based on execution confidence and recency

**Configuration:**
```csharp
var options = new AdvancedRoutingOptions
{
    EnableProbabilisticRouting = true,
    ProbabilisticDecayFactor = 0.95, // Weight decay over time
    MinimumConfidenceThreshold = 0.3  // Minimum confidence for selection
};
```

### 4. Contextual Routing Strategy

Considers execution history patterns and transition probabilities:

```csharp
public sealed class ContextualRoutingStrategy : IRoutingStrategy
{
    public RoutingStrategyType Type => RoutingStrategyType.Contextual;
    public double GetWeight() => 0.15;
    
    // Selects node based on historical transition patterns
    public async Task<RoutingStrategyResult?> SelectNodeAsync(
        List<IGraphNode> candidates,
        AdvancedRoutingContext context,
        CancellationToken cancellationToken = default)
}
```

**How it works:**
1. Analyzes routing history for current node
2. Calculates transition probabilities to candidate nodes
3. Considers success rates of historical transitions
4. Selects node with best historical performance pattern

**Features:**
- Looks back at recent history (configurable limit)
- Considers both frequency and success rate
- Adapts to changing execution patterns

### 5. Feedback Learning Routing Strategy

Adapts routing decisions based on user feedback:

```csharp
public sealed class FeedbackLearningRoutingStrategy : IRoutingStrategy
{
    public RoutingStrategyType Type => RoutingStrategyType.FeedbackLearning;
    public double GetWeight() => 0.1;
    
    // Selects node based on feedback-adjusted scores
    public async Task<RoutingStrategyResult?> SelectNodeAsync(
        List<IGraphNode> candidates,
        AdvancedRoutingContext context,
        CancellationToken cancellationToken = default)
    }
    
    // Provides feedback to improve future routing
    public async Task ProvideFeedbackAsync(string routingDecisionId, 
        RoutingFeedbackInfo feedbackInfo, CancellationToken cancellationToken = default)
}
```

**How it works:**
1. Calculates feedback-adjusted scores for candidates
2. Weights recent feedback more heavily
3. Selects node with highest feedback score
4. Continuously learns from user feedback

**Feedback Types:**
```csharp
public enum RoutingFeedbackType
{
    Positive,    // Good routing decision
    Negative,    // Poor routing decision
    Neutral,     // Acceptable but not optimal
    Correction   // Explicit correction
}
```

## Configuration and Options

### AdvancedRoutingOptions

Comprehensive configuration for all routing strategies:

```csharp
public sealed class AdvancedRoutingOptions
{
    // Enable/disable individual strategies
    public bool EnableSemanticRouting { get; set; } = true;
    public bool EnableSimilarityRouting { get; set; } = true;
    public bool EnableProbabilisticRouting { get; set; } = true;
    public bool EnableContextualRouting { get; set; } = true;
    public bool EnableFeedbackLearning { get; set; } = true;
    
    // Thresholds and limits
    public double SemanticSimilarityThreshold { get; set; } = 0.7;
    public int HistoryLookbackLimit { get; set; } = 10;
    public double MinimumConfidenceThreshold { get; set; } = 0.3;
    
    // Learning and adaptation parameters
    public double FeedbackLearningRate { get; set; } = 0.1;
    public double ProbabilisticDecayFactor { get; set; } = 0.95;
}
```

### Strategy Weights

Each strategy contributes to the final decision with configurable weights:

- **Semantic**: 0.3 (30%) - High weight for semantic understanding
- **Similarity**: 0.25 (25%) - Good weight for historical patterns
- **Probabilistic**: 0.2 (20%) - Balanced weight for exploration
- **Contextual**: 0.15 (15%) - Moderate weight for patterns
- **Feedback**: 0.1 (10%) - Lower weight for learning

## Usage Examples

### Basic Setup

```csharp
// 1. Create kernel with embedding service
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddTextEmbeddingGeneration("text-embedding-ada-002", "your-api-key");
var kernel = kernelBuilder.Build();

// 2. Create memory service (if available)
var memoryService = new GraphMemoryService(); // Your implementation

// 3. Create advanced routing engine
var advancedRoutingEngine = new AdvancedRoutingEngine(
    embeddingService: kernel.GetRequiredService<ITextEmbeddingGenerationService>(),
    memoryService: memoryService,
    options: new AdvancedRoutingOptions
    {
        EnableSemanticRouting = true,
        EnableSimilarityRouting = true,
        EnableProbabilisticRouting = true,
        EnableContextualRouting = true,
        EnableFeedbackLearning = true
    }
);

// 4. Configure graph with advanced routing
var graph = new GraphExecutor(kernel);
graph.RoutingEngine = new DynamicRoutingEngine(
    embeddingService: kernel.GetRequiredService<ITextEmbeddingGenerationService>(),
    memoryService: memoryService
);
```

### Providing Feedback

```csharp
// Get feedback strategy from routing engine
var feedbackStrategy = routingEngine.GetFeedbackStrategy();

// Provide feedback for a routing decision
await feedbackStrategy.ProvideFeedbackAsync(
    routingDecisionId: "decision-123",
    new RoutingFeedbackInfo
    {
        NodeId = "selected-node",
        FeedbackType = RoutingFeedbackType.Positive,
        Score = 0.9,
        Comments = "Excellent routing decision",
        Timestamp = DateTimeOffset.UtcNow
    }
);
```

### Custom Strategy Weights

```csharp
// Override default strategy weights
var customOptions = new AdvancedRoutingOptions
{
    EnableSemanticRouting = true,
    EnableSimilarityRouting = true,
    EnableProbabilisticRouting = false, // Disable probabilistic
    EnableContextualRouting = true,
    EnableFeedbackLearning = true,
    
    // Adjust thresholds
    SemanticSimilarityThreshold = 0.8,  // Higher threshold
    HistoryLookbackLimit = 20,           // More history
    MinimumConfidenceThreshold = 0.5     // Higher confidence required
};
```

## Advanced Features

### Routing Context

The `AdvancedRoutingContext` provides rich information for routing decisions:

```csharp
public sealed class AdvancedRoutingContext
{
    public required string CurrentNodeId { get; init; }
    public required string CurrentNodeName { get; init; }
    public required GraphState GraphState { get; init; }
    public FunctionResult? ExecutionResult { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required string ExecutionId { get; init; }
    public required int ExecutionStep { get; init; }
    public List<GraphExecutionMemory> SimilarExecutions { get; set; } = new();
    public Random Random { get; init; } = new();
}
```

### Strategy Aggregation

Multiple strategies contribute to the final decision:

```csharp
// Results from different strategies
var routingResults = new List<RoutingStrategyResult>();

// Add results from enabled strategies
if (semanticResult != null) routingResults.Add(semanticResult);
if (similarityResult != null) routingResults.Add(similarityResult);
if (probabilisticResult != null) routingResults.Add(probabilisticResult);
if (contextualResult != null) routingResults.Add(contextualResult);
if (feedbackResult != null) routingResults.Add(feedbackResult);

// Aggregate and select final node
var finalResult = AggregateRoutingResults(routingResults, routingContext);
```

### Routing History

Track and analyze routing decisions:

```csharp
public sealed class RoutingHistory
{
    public required string DecisionId { get; init; }
    public required string CurrentNodeId { get; init; }
    public required string SelectedNodeId { get; init; }
    public required List<RoutingStrategyType> UsedStrategies { get; init; }
    public required double FinalConfidence { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required string ContextSnapshot { get; init; }
}
```

## Performance Considerations

### Caching

The `DynamicRoutingEngine` includes built-in caching:

```csharp
var routingOptions = new DynamicRoutingOptions
{
    EnableCaching = true,
    MaxCacheSize = 1000,
    CacheExpiration = TimeSpan.FromMinutes(30)
};
```

### Memory Management

- Node embeddings are cached with TTL (24 hours default)
- Routing history uses concurrent dictionaries for thread safety
- Similar executions are limited by `HistoryLookbackLimit`

### Fallback Mechanisms

When advanced routing fails, fallback options ensure execution continues:

```csharp
var routingOptions = new DynamicRoutingOptions
{
    EnableFallback = true,  // Always select a node
    EnableCaching = true,   // Cache decisions for performance
    MaxCacheSize = 100      // Limit memory usage
};
```

## Monitoring and Debugging

### Routing Metrics

Track routing performance and decisions:

```csharp
// Get routing metrics for specific node
var nodeMetrics = routingEngine.GetRoutingMetrics("node-id");

// Get all routing metrics
var allMetrics = routingEngine.GetRoutingMetrics();
```

### Logging

Comprehensive logging for debugging routing decisions:

```csharp
// Enable debug logging for routing
var logger = LoggerFactory.Create(builder => 
    builder.SetMinimumLevel(LogLevel.Debug)
).CreateLogger<DynamicRoutingEngine>();

var routingEngine = new DynamicRoutingEngine(
    logger: logger,
    embeddingService: embeddingService,
    memoryService: memoryService
);
```

### Strategy Details

Each routing result includes detailed information about the decision:

```csharp
var result = await routingEngine.SelectNextNodeAsync(candidates, currentNode, state);

// Access strategy details
foreach (var strategyResult in result.UsedStrategies)
{
    var details = strategyResult.StrategyDetails;
    // Process strategy-specific details
}
```

## Best Practices

### 1. Service Availability

- Always check if required services are available before enabling strategies
- Provide fallback mechanisms when services are unavailable
- Use dependency injection for service management

### 2. Threshold Tuning

- Start with default thresholds and adjust based on your use case
- Monitor routing confidence scores to identify optimal thresholds
- Use feedback learning to automatically tune parameters

### 3. Memory Management

- Set appropriate limits for history lookback and cache sizes
- Monitor memory usage in production environments
- Implement cleanup policies for old routing data

### 4. Performance Monitoring

- Track routing decision latency
- Monitor cache hit rates
- Analyze strategy contribution patterns

### 5. Feedback Quality

- Provide consistent and meaningful feedback
- Use appropriate feedback types for different scenarios
- Regularly review and clean up feedback data

## Troubleshooting

### Common Issues

**Low routing confidence:**
- Check if embedding service is working correctly
- Verify memory service has relevant historical data
- Adjust similarity thresholds

**Slow routing decisions:**
- Enable caching for frequently accessed routes
- Reduce history lookback limits
- Optimize embedding generation

**Inconsistent routing:**
- Check strategy weights and thresholds
- Verify feedback data quality
- Monitor routing history patterns

### Debug Information

Enable detailed logging to diagnose routing issues:

```csharp
// Enable debug logging
var logger = LoggerFactory.Create(builder => 
    builder.SetMinimumLevel(LogLevel.Debug)
).CreateLogger<AdvancedRoutingEngine>();

// Check strategy results
var result = await routingEngine.SelectNextNodeAsync(candidates, currentNode, state);
Console.WriteLine($"Used strategies: {string.Join(", ", result.UsedStrategies)}");
Console.WriteLine($"Final confidence: {result.FinalConfidence:F3}");
```

## See Also

- [Conditional Nodes](./conditional-nodes.md) - Basic conditional routing
- [Dynamic Routing](./dynamic-routing.md) - Template-based routing
- [State Management](./state.md) - Graph state and context
- [Memory and Templates](./templates-and-memory.md) - Memory services and templates
- [Performance Metrics](./metrics-and-observability.md) - Monitoring routing performance

## Examples

- [Advanced Routing Example](../../examples/advanced-routing.md) - Complete demonstration
- [Dynamic Routing Example](../../examples/dynamic-routing.md) - Template-based routing
- [Multi-Agent Workflow](../../tutorials/multi-agent-workflow.md) - Complex routing scenarios
