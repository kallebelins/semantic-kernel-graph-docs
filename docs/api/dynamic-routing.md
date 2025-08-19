# Dynamic Routing Engine

The `DynamicRoutingEngine` provides advanced node selection capabilities that go beyond simple conditional edges. It combines multiple routing strategies, template-based decisions, caching, and fallback mechanisms to make intelligent routing choices based on execution context and state content.

## Overview

The dynamic routing system consists of several key components:

- **`DynamicRoutingEngine`**: Main orchestrator that coordinates routing strategies
- **`AdvancedRoutingEngine`**: Handles sophisticated routing using embeddings and memory
- **Routing Strategies**: Multiple approaches to node selection
- **Template Engine Integration**: Handlebars-based routing decisions
- **Caching and Fallback**: Performance optimization and reliability

## Core Classes

### DynamicRoutingEngine

The primary routing engine that provides dynamic node selection based on state content, template-based routing, caching, and fallback mechanisms.

```csharp
public sealed class DynamicRoutingEngine : IAsyncDisposable
{
    public DynamicRoutingEngine(
        IGraphTemplateEngine? templateEngine = null,
        DynamicRoutingOptions? options = null, 
        ILogger<DynamicRoutingEngine>? logger = null,
        ITextEmbeddingGenerationService? embeddingService = null, 
        IGraphMemoryService? memoryService = null);
}
```

**Key Features:**
- **Content-based routing**: Analyzes state content and execution results
- **Template-based routing**: Uses Handlebars templates for dynamic decisions
- **Caching**: Stores routing decisions for performance optimization
- **Fallback mechanisms**: Ensures routing always succeeds
- **Advanced routing integration**: Automatically enables when services are available

**Constructor Parameters:**
- `templateEngine`: Optional template engine for routing decisions
- `options`: Configuration options for caching and fallback behavior
- `logger`: Optional logger for debugging and monitoring
- `embeddingService`: Optional service for semantic routing
- `memoryService`: Optional service for contextual routing

### DynamicRoutingOptions

Configuration options for the dynamic routing engine:

```csharp
public sealed class DynamicRoutingOptions
{
    public bool EnableCaching { get; set; } = true;
    public bool EnableFallback { get; set; } = true;
    public int MaxCacheSize { get; set; } = 1000;
    public int CacheExpirationMinutes { get; set; } = 30;
}
```

**Properties:**
- `EnableCaching`: Whether to cache routing decisions for performance
- `EnableFallback`: Whether to use fallback mechanisms when routing fails
- `MaxCacheSize`: Maximum number of cached decisions to store
- `CacheExpirationMinutes`: How long cached decisions remain valid

## Advanced Routing Integration

### AdvancedRoutingEngine

When embedding or memory services are available, the `DynamicRoutingEngine` automatically initializes an `AdvancedRoutingEngine` with multiple routing strategies:

```csharp
// Automatically initialized when services are provided
var routingEngine = new DynamicRoutingEngine(
    templateEngine: null,
    options: new DynamicRoutingOptions { EnableCaching = true, EnableFallback = true },
    logger: logger,
    embeddingService: embeddingService,  // Enables semantic routing
    memoryService: memoryService         // Enables contextual routing
);
```

### AdvancedRoutingOptions

Configuration for advanced routing capabilities:

```csharp
public sealed class AdvancedRoutingOptions
{
    public bool EnableSemanticRouting { get; set; } = true;
    public bool EnableSimilarityRouting { get; set; } = true;
    public bool EnableProbabilisticRouting { get; set; } = true;
    public bool EnableContextualRouting { get; set; } = true;
    public bool EnableFeedbackLearning { get; set; } = true;
    public double SemanticSimilarityThreshold { get; set; } = 0.7;
    public int HistoryLookbackLimit { get; set; } = 10;
    public double FeedbackLearningRate { get; set; } = 0.1;
    public double ProbabilisticDecayFactor { get; set; } = 0.95;
    public double MinimumConfidenceThreshold { get; set; } = 0.3;
}
```

**Strategy Controls:**
- `EnableSemanticRouting`: Uses text embeddings for similarity-based routing
- `EnableSimilarityRouting`: Leverages execution history for pattern matching
- `EnableProbabilisticRouting`: Applies weighted random selection with dynamic weights
- `EnableContextualRouting`: Considers execution history patterns and transitions
- `EnableFeedbackLearning`: Adapts routing decisions based on user feedback

**Thresholds and Limits:**
- `SemanticSimilarityThreshold`: Minimum similarity score for semantic routing (0.0 to 1.0)
- `HistoryLookbackLimit`: Number of similar executions to consider
- `FeedbackLearningRate`: How quickly feedback affects future decisions (0.0 to 1.0)
- `ProbabilisticDecayFactor`: Weight decay factor over time (0.0 to 1.0)
- `MinimumConfidenceThreshold`: Minimum confidence required for routing decisions

## Routing Strategies

The advanced routing system implements multiple strategies that work together:

### 1. Semantic Routing Strategy

Uses text embeddings to find semantically similar nodes based on context:

```csharp
public sealed class SemanticRoutingStrategy : IRoutingStrategy
{
    public RoutingStrategyType Type => RoutingStrategyType.Semantic;
    public double GetWeight() => 0.3; // High weight for semantic similarity
}
```

**How it works:**
1. Generates embeddings for current execution context
2. Compares with stored node embeddings
3. Selects node with highest semantic similarity
4. Applies similarity threshold filtering

**Requirements:**
- `ITextEmbeddingGenerationService` implementation
- Node embeddings stored in `_nodeEmbeddings` cache

### 2. Content Similarity Routing Strategy

Leverages execution history to find similar patterns:

```csharp
public sealed class ContentSimilarityRoutingStrategy : IRoutingStrategy
{
    public RoutingStrategyType Type => RoutingStrategyType.Similarity;
    public double GetWeight() => 0.25;
}
```

**How it works:**
1. Analyzes current state parameters and execution context
2. Searches memory for similar execution patterns
3. Calculates similarity scores using Jaccard index
4. Selects node based on historical success patterns

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
}
```

**How it works:**
1. Analyzes routing history for current node
2. Calculates transition probabilities to candidate nodes
3. Considers success rates of historical transitions
4. Selects node with best historical performance pattern

**Features:**
- Transition probability analysis
- Success rate weighting
- Historical pattern recognition
- Context-aware decision making

### 5. Feedback Learning Routing Strategy

Adapts routing decisions based on user feedback:

```csharp
public sealed class FeedbackLearningRoutingStrategy : IRoutingStrategy
{
    public RoutingStrategyType Type => RoutingStrategyType.FeedbackLearning;
    public double GetWeight() => 0.1;
}
```

**How it works:**
1. Collects feedback on routing decisions
2. Adjusts strategy weights based on feedback
3. Learns from successful and unsuccessful routes
4. Improves decision quality over time

**Configuration:**
```csharp
var options = new AdvancedRoutingOptions
{
    EnableFeedbackLearning = true,
    FeedbackLearningRate = 0.1, // How quickly to adapt to feedback
    MinimumConfidenceThreshold = 0.3
};
```

## Template Engine Integration

### IGraphTemplateEngine

The routing system integrates with template engines for dynamic routing decisions:

```csharp
public interface IGraphTemplateEngine
{
    Task<string> RenderAsync(string template, object context, CancellationToken cancellationToken = default);
    Task<string> RenderWithArgumentsAsync(string template, KernelArguments arguments, CancellationToken cancellationToken = default);
}
```

**Available Implementations:**
- `HandlebarsGraphTemplateEngine`: Basic Handlebars-like templating
- `ChainOfThoughtTemplateEngine`: Specialized for reasoning patterns
- `ReActTemplateEngine`: Optimized for ReAct pattern prompts

### Template-Based Routing

Templates can be used to make routing decisions based on state content:

```csharp
// Template-based routing example
var routingTemplate = "{{#if error}}ErrorHandler{{else}}{{#if complete}}CompleteNode{{else}}DefaultNode{{/if}}{{/if}}";

// The template engine renders this with current state
var routingDecision = await templateEngine.RenderWithArgumentsAsync(
    routingTemplate, 
    context.Parameters, 
    cancellationToken);
```

**Template Features:**
- Variable substitution: `{{variable}}`
- Conditional statements: `{{#if condition}}...{{else}}...{{/if}}`
- Helper functions: `{{helper arg1 arg2}}`
- State-aware rendering
- Caching for performance

## Routing Context and Results

### AdvancedRoutingContext

Rich context information for routing decisions:

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

### AdvancedRoutingResult

Comprehensive result of routing decisions:

```csharp
public sealed class AdvancedRoutingResult
{
    public required IGraphNode SelectedNode { get; init; }
    public required double FinalConfidence { get; init; }
    public required List<RoutingStrategyType> UsedStrategies { get; init; }
    public required Dictionary<RoutingStrategyType, object> StrategyDetails { get; init; }
    public required string DecisionId { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}
```

**Properties:**
- `SelectedNode`: The node chosen by the routing engine
- `FinalConfidence`: Overall confidence score (0.0 to 1.0)
- `UsedStrategies`: List of strategies that contributed to the decision
- `StrategyDetails`: Detailed information from each strategy
- `DecisionId`: Unique identifier for the routing decision
- `Timestamp`: When the decision was made

## Metrics and Monitoring

### RoutingMetrics

Performance metrics for routing decisions:

```csharp
public sealed class RoutingMetrics
{
    public required string NodeId { get; init; }
    public required string NodeName { get; init; }
    public int TotalDecisions { get; set; }
    public int CachedDecisions { get; set; }
    public int FailedDecisions { get; set; }
    public double AverageDecisionTime { get; set; }
    public DateTimeOffset? LastDecisionAt { get; set; }
    public ConcurrentDictionary<string, string> SelectedNodes { get; } = new();
    
    public double CacheHitRatio => TotalDecisions > 0 ? (CachedDecisions / (double)TotalDecisions) * 100 : 0;
    public double SuccessRatio => TotalDecisions > 0 ? ((TotalDecisions - FailedDecisions) / (double)TotalDecisions) * 100 : 0;
}
```

**Key Metrics:**
- `TotalDecisions`: Total number of routing decisions made
- `CachedDecisions`: Number of decisions served from cache
- `FailedDecisions`: Number of failed routing attempts
- `AverageDecisionTime`: Average time to make routing decisions
- `CacheHitRatio`: Percentage of decisions served from cache
- `SuccessRatio`: Percentage of successful routing decisions

### RoutingAnalytics

Aggregated analytics across all routing strategies:

```csharp
public sealed class RoutingAnalytics
{
    public int TotalDecisions { get; init; }
    public int SemanticRoutingUsage { get; init; }
    public int SimilarityRoutingUsage { get; init; }
    public int ProbabilisticRoutingUsage { get; init; }
    public int ContextualRoutingUsage { get; init; }
    public int FeedbackLearningUsage { get; init; }
    public double AverageConfidence { get; init; }
    public int FeedbackReceived { get; init; }
    public int PositiveFeedback { get; init; }
    public DateTimeOffset? LastDecisionAt { get; init; }
}
```

## Usage Examples

### Basic Dynamic Routing Setup

```csharp
// Create kernel with dynamic routing
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.Services.AddLogging(builder => builder.AddConsole());

var kernel = kernelBuilder.Build();

// Create graph with dynamic routing enabled
var graph = kernelBuilder.CreateGraphWithDynamicRouting("DynamicDemo", "Dynamic routing example");

// Add nodes and connections
var startNode = new FunctionGraphNode(/* ... */);
var processNode = new FunctionGraphNode(/* ... */);
var errorNode = new FunctionGraphNode(/* ... */);

graph.AddNode(startNode)
     .AddNode(processNode)
     .AddNode(errorNode)
     .SetStartNode("start");

// Execute with dynamic routing
var result = await graph.ExecuteAsync(kernel, new KernelArguments { ["input"] = "test" });
```

### Advanced Routing with Services

```csharp
// Create advanced routing with embedding and memory services
var routingEngine = new DynamicRoutingEngine(
    templateEngine: new HandlebarsGraphTemplateEngine(),
    options: new DynamicRoutingOptions 
    { 
        EnableCaching = true, 
        EnableFallback = true,
        MaxCacheSize = 500,
        CacheExpirationMinutes = 60
    },
    logger: logger,
    embeddingService: embeddingService,
    memoryService: memoryService
);

// Configure graph to use advanced routing
graph.RoutingEngine = routingEngine;

// Execute with advanced routing capabilities
var result = await graph.ExecuteAsync(kernel, arguments);
```

### Template-Based Routing

```csharp
// Create template engine
var templateEngine = new HandlebarsGraphTemplateEngine(new GraphTemplateOptions
{
    EnableHandlebars = true,
    EnableCustomHelpers = true,
    TemplateCacheSize = 100
});

// Create routing engine with template support
var routingEngine = new DynamicRoutingEngine(
    templateEngine: templateEngine,
    options: new DynamicRoutingOptions { EnableCaching = true, EnableFallback = true }
);

// Use templates for routing decisions
graph.ConnectWithTemplate("start", "process", 
    "{{#if (eq priority 'high')}}true{{else}}false{{/if}}", 
    templateEngine, "HighPriorityRoute");
```

## Extension Methods

### DynamicRoutingExtensions

Helper methods for configuring dynamic routing:

```csharp
public static class DynamicRoutingExtensions
{
    // Enable dynamic routing with default options
    public static GraphExecutor EnableDynamicRouting(this GraphExecutor executor,
        IGraphTemplateEngine? templateEngine = null, 
        ILogger<DynamicRoutingEngine>? logger = null);
    
    // Enable dynamic routing with custom options
    public static GraphExecutor EnableDynamicRouting(this GraphExecutor executor,
        DynamicRoutingOptions options, 
        IGraphTemplateEngine? templateEngine = null,
        ILogger<DynamicRoutingEngine>? logger = null);
    
    // Disable dynamic routing
    public static GraphExecutor DisableDynamicRouting(this GraphExecutor executor);
    
    // Get routing metrics
    public static IReadOnlyDictionary<string, RoutingMetrics> GetRoutingMetrics(
        this GraphExecutor executor, string? nodeId = null);
}
```

## Performance Considerations

### Caching Strategy

The routing engine implements intelligent caching:

- **Decision caching**: Stores routing decisions to avoid recomputation
- **Template caching**: Compiles and caches templates for faster rendering
- **Embedding caching**: Stores node embeddings to avoid regeneration
- **Configurable expiration**: Cache entries expire based on time and usage

### Fallback Mechanisms

Multiple fallback strategies ensure routing always succeeds:

1. **Advanced routing strategies** (semantic, similarity, probabilistic, contextual, feedback)
2. **Content-based selection** (analyzing state and execution results)
3. **Template-based routing** (using Handlebars templates)
4. **Fallback selection** (random or round-robin when all else fails)

### Resource Management

The engine implements proper resource management:

- **Async disposal**: Implements `IAsyncDisposable` for cleanup
- **Memory management**: Configurable cache sizes and expiration
- **Concurrent access**: Thread-safe operations using concurrent collections
- **Performance monitoring**: Built-in metrics and logging

## Error Handling

### Routing Failures

When routing fails, the engine provides detailed error information:

- **Exception details**: Specific error messages and stack traces
- **Fallback behavior**: Automatic fallback when enabled
- **Logging**: Comprehensive logging of routing decisions and failures
- **Metrics tracking**: Failed decisions are tracked for analysis

### Recovery Strategies

The engine implements several recovery strategies:

- **Automatic fallback**: Uses alternative routing methods
- **Cache invalidation**: Removes invalid cached decisions
- **Strategy adjustment**: Adapts strategy weights based on failures
- **Feedback learning**: Learns from routing failures to improve future decisions

## See Also

- [Advanced Routing Guide](../how-to/advanced-routing.md) - Comprehensive guide to advanced routing concepts and techniques
- [Templates and Memory](../how-to/templates-and-memory.md) - Template engine system and memory integration
- [Graph Executor](./graph-executor.md) - Core execution engine that uses routing
- [Graph State](./graph-state.md) - State management for routing decisions
- [Dynamic Routing Example](../../examples/dynamic-routing-example.md) - Complete example demonstrating dynamic routing capabilities
