# Dynamic Routing Example

This example demonstrates advanced routing patterns using the Semantic Kernel Graph's dynamic routing capabilities. It shows how to implement intelligent routing based on content analysis, user preferences, and real-time conditions.

## Objective

Learn how to implement dynamic routing in graph-based workflows to:
- Route execution based on content analysis and semantic similarity
- Implement intelligent decision-making with multiple routing strategies
- Handle dynamic routing with real-time condition evaluation
- Scale routing logic across complex workflow scenarios
- Optimize routing performance with caching and prediction

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Routing Concepts](../concepts/routing.md)

## Key Components

### Concepts and Techniques

- **Dynamic Routing**: Runtime determination of execution paths based on conditions
- **Content-Based Routing**: Routing decisions based on content analysis and classification
- **Semantic Similarity**: Using embeddings and similarity scores for intelligent routing
- **Multi-Strategy Routing**: Combining multiple routing approaches for optimal decisions
- **Performance Optimization**: Caching and prediction for efficient routing

### Core Classes

- `DynamicRoutingEngine`: Core engine for dynamic routing decisions
- `ConditionalGraphNode`: Nodes that evaluate routing conditions
- `FunctionGraphNode`: Processing nodes for different content types
- `GraphExecutor`: Orchestrates the dynamic routing workflow

## Running the Example

### Getting Started

This example demonstrates dynamic routing and adaptive execution with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Basic Dynamic Routing

This example demonstrates simple dynamic routing based on input content analysis.

```csharp
// Create kernel with mock configuration
var kernel = CreateKernel();

// Create dynamic routing workflow
var dynamicRouter = new GraphExecutor("DynamicRouter", "Basic dynamic routing", logger);

// Content analyzer node
var contentAnalyzer = new FunctionGraphNode(
    "content-analyzer",
    "Analyze input content and determine routing",
    async (context) =>
    {
        var inputContent = context.GetValue<string>("input_content");
        
        // Simple content analysis
        var contentType = AnalyzeContentType(inputContent);
        var priority = CalculatePriority(inputContent);
        var complexity = AssessComplexity(inputContent);
        
        context.SetValue("content_type", contentType);
        context.SetValue("priority_level", priority);
        context.SetValue("complexity_level", complexity);
        context.SetValue("routing_decision", "pending");
        
        return $"Content analyzed: {contentType} (Priority: {priority}, Complexity: {complexity})";
    });

// Dynamic routing decision node
var routingDecision = new ConditionalGraphNode(
    "routing-decision",
    "Make routing decision based on content analysis",
    logger)
{
    ConditionExpression = "content_type == 'technical' && priority_level >= 8",
    TrueNodeId = "expert-processor",
    FalseNodeId = "standard-processor"
};

// Expert processor for high-priority technical content
var expertProcessor = new FunctionGraphNode(
    "expert-processor",
    "Process high-priority technical content",
    async (context) =>
    {
        var content = context.GetValue<string>("input_content");
        var priority = context.GetValue<int>("priority_level");
        
        // Expert-level processing
        var result = await ProcessWithExpertLogic(content, priority);
        context.SetValue("processing_result", result);
        context.SetValue("processing_level", "expert");
        context.SetValue("routing_decision", "expert_processed");
        
        return $"Expert processing completed: {result}";
    });

// Standard processor for other content
var standardProcessor = new FunctionGraphNode(
    "standard-processor",
    "Process standard content",
    async (context) =>
    {
        var content = context.GetValue<string>("input_content");
        var contentType = context.GetValue<string>("content_type");
        
        // Standard processing
        var result = await ProcessWithStandardLogic(content, contentType);
        context.SetValue("processing_result", result);
        context.SetValue("processing_level", "standard");
        context.SetValue("routing_decision", "standard_processed");
        
        return $"Standard processing completed: {result}";
    });

// Add nodes to router
dynamicRouter.AddNode(contentAnalyzer);
dynamicRouter.AddNode(routingDecision);
dynamicRouter.AddNode(expertProcessor);
dynamicRouter.AddNode(standardProcessor);

// Set start node
dynamicRouter.SetStartNode(contentAnalyzer.NodeId);

// Test with different content types
var testContent = new[]
{
    "How do I implement a binary search tree in C#?",
    "What's the weather like today?",
    "Explain quantum computing principles and applications",
    "Can you help me with basic math?"
};

foreach (var content in testContent)
{
    var arguments = new KernelArguments
    {
        ["input_content"] = content
    };

    Console.WriteLine($"🧪 Testing content: {content}");
    var result = await dynamicRouter.ExecuteAsync(kernel, arguments);
    
    var routingDecision = result.GetValue<string>("routing_decision");
    var processingLevel = result.GetValue<string>("processing_level");
    var processingResult = result.GetValue<string>("processing_result");
    
    Console.WriteLine($"   Routing: {routingDecision}");
    Console.WriteLine($"   Level: {processingLevel}");
    Console.WriteLine($"   Result: {processingResult}");
    Console.WriteLine();
}
```

### 2. Advanced Semantic Routing

Demonstrates routing based on semantic similarity and content classification.

```csharp
// Create advanced semantic routing workflow
var semanticRouter = new GraphExecutor("SemanticRouter", "Semantic-based routing", logger);

// Semantic content analyzer
var semanticAnalyzer = new FunctionGraphNode(
    "semantic-analyzer",
    "Perform semantic analysis of content",
    async (context) =>
    {
        var inputContent = context.GetValue<string>("input_content");
        
        // Semantic analysis
        var topics = ExtractTopics(inputContent);
        var sentiment = AnalyzeSentiment(inputContent);
        var domain = ClassifyDomain(inputContent);
        var intent = DetectIntent(inputContent);
        
        context.SetValue("semantic_topics", topics);
        context.SetValue("sentiment_score", sentiment);
        context.SetValue("domain_classification", domain);
        context.SetValue("user_intent", intent);
        context.SetValue("semantic_analysis_complete", true);
        
        return $"Semantic analysis: {domain} domain, {intent} intent";
    });

// Multi-dimensional routing decision
var semanticRouter = new ConditionalGraphNode(
    "semantic-router",
    "Route based on semantic analysis",
    logger)
{
    ConditionExpression = "domain_classification == 'technical' && sentiment_score > 0.7",
    TrueNodeId = "technical-expert",
    FalseNodeId = "domain-router"
};

// Domain-specific routing
var domainRouter = new ConditionalGraphNode(
    "domain-router",
    "Route to domain-specific processors",
    logger)
{
    ConditionExpression = "domain_classification == 'business'",
    TrueNodeId = "business-processor",
    FalseNodeId = "general-processor"
};

// Technical expert processor
var technicalExpert = new FunctionGraphNode(
    "technical-expert",
    "Process technical content with expert knowledge",
    async (context) =>
    {
        var content = context.GetValue<string>("input_content");
        var topics = context.GetValue<string[]>("semantic_topics");
        var intent = context.GetValue<string>("user_intent");
        
        // Expert technical processing
        var result = await ProcessTechnicalContent(content, topics, intent);
        context.SetValue("processing_result", result);
        context.SetValue("processing_specialization", "technical_expert");
        
        return $"Technical expert processing: {result}";
    });

// Business processor
var businessProcessor = new FunctionGraphNode(
    "business-processor",
    "Process business-related content",
    async (context) =>
    {
        var content = context.GetValue<string>("input_content");
        var topics = context.GetValue<string[]>("semantic_topics");
        
        // Business processing
        var result = await ProcessBusinessContent(content, topics);
        context.SetValue("processing_result", result);
        context.SetValue("processing_specialization", "business");
        
        return $"Business processing: {result}";
    });

// General processor
var generalProcessor = new FunctionGraphNode(
    "general-processor",
    "Process general content",
    async (context) =>
    {
        var content = context.GetValue<string>("input_content");
        var domain = context.GetValue<string>("domain_classification");
        
        // General processing
        var result = await ProcessGeneralContent(content, domain);
        context.SetValue("processing_result", result);
        context.SetValue("processing_specialization", "general");
        
        return $"General processing: {result}";
    });

// Add nodes to semantic router
semanticRouter.AddNode(semanticAnalyzer);
semanticRouter.AddNode(semanticRouter);
semanticRouter.AddNode(domainRouter);
semanticRouter.AddNode(technicalExpert);
semanticRouter.AddNode(businessProcessor);
semanticRouter.AddNode(generalProcessor);

// Set start node
semanticRouter.SetStartNode(semanticAnalyzer.NodeId);

// Test semantic routing
var semanticTestContent = new[]
{
    "I need help with implementing a microservices architecture",
    "Can you analyze this quarterly financial report?",
    "What are the best practices for team collaboration?",
    "How do I optimize database performance for large datasets?"
};

foreach (var content in semanticTestContent)
{
    var arguments = new KernelArguments
    {
        ["input_content"] = content
    };

    Console.WriteLine($"🧠 Testing semantic routing: {content}");
    var result = await semanticRouter.ExecuteAsync(kernel, arguments);
    
    var specialization = result.GetValue<string>("processing_specialization");
    var processingResult = result.GetValue<string>("processing_result");
    
    Console.WriteLine($"   Specialization: {specialization}");
    Console.WriteLine($"   Result: {processingResult}");
    Console.WriteLine();
}
```

### 3. Real-Time Adaptive Routing

Shows how to implement routing that adapts based on real-time conditions and performance metrics.

```csharp
// Create adaptive routing workflow
var adaptiveRouter = new GraphExecutor("AdaptiveRouter", "Real-time adaptive routing", logger);

// Performance monitor
var performanceMonitor = new FunctionGraphNode(
    "performance-monitor",
    "Monitor system performance and routing conditions",
    async (context) =>
    {
        // Get real-time metrics
        var cpuUsage = GetCurrentCpuUsage();
        var memoryUsage = GetCurrentMemoryUsage();
        var queueLength = GetCurrentQueueLength();
        var responseTime = GetAverageResponseTime();
        
        context.SetValue("cpu_usage", cpuUsage);
        context.SetValue("memory_usage", memoryUsage);
        context.SetValue("queue_length", queueLength);
        context.SetValue("response_time", responseTime);
        context.SetValue("system_health", "monitored");
        
        return $"System health: CPU {cpuUsage}%, Memory {memoryUsage}%, Queue {queueLength}";
    });

// Adaptive routing decision
var adaptiveRouter = new ConditionalGraphNode(
    "adaptive-router",
    "Make adaptive routing decision",
    logger)
{
    ConditionExpression = "cpu_usage < 70 && memory_usage < 80 && queue_length < 10",
    TrueNodeId = "high-performance-processor",
    FalseNodeId = "load-balanced-processor"
};

// High-performance processor for healthy systems
var highPerformanceProcessor = new FunctionGraphNode(
    "high-performance-processor",
    "Process with high-performance resources",
    async (context) =>
    {
        var content = context.GetValue<string>("input_content");
        
        // Use high-performance processing
        var result = await ProcessWithHighPerformance(content);
        context.SetValue("processing_result", result);
        context.SetValue("processing_mode", "high_performance");
        context.SetValue("resource_usage", "optimized");
        
        return $"High-performance processing: {result}";
    });

// Load-balanced processor for stressed systems
var loadBalancedProcessor = new FunctionGraphNode(
    "load-balanced-processor",
    "Process with load balancing",
    async (context) =>
    {
        var content = context.GetValue<string>("input_content");
        var cpuUsage = context.GetValue<double>("cpu_usage");
        var queueLength = context.GetValue<int>("queue_length");
        
        // Adaptive load balancing
        var result = await ProcessWithLoadBalancing(content, cpuUsage, queueLength);
        context.SetValue("processing_result", result);
        context.SetValue("processing_mode", "load_balanced");
        context.SetValue("resource_usage", "conservative");
        
        return $"Load-balanced processing: {result}";
    });

// Add nodes to adaptive router
adaptiveRouter.AddNode(performanceMonitor);
adaptiveRouter.AddNode(adaptiveRouter);
adaptiveRouter.AddNode(highPerformanceProcessor);
adaptiveRouter.AddNode(loadBalancedProcessor);

// Set start node
adaptiveRouter.SetStartNode(performanceMonitor.NodeId);

// Test adaptive routing with simulated conditions
var adaptiveTestScenarios = new[]
{
    new { Content = "Process this urgent request", CpuSimulation = 50.0, MemorySimulation = 60.0, QueueSimulation = 5 },
    new { Content = "Handle this batch job", CpuSimulation = 85.0, MemorySimulation = 90.0, QueueSimulation = 25 },
    new { Content = "Process user query", CpuSimulation = 65.0, MemorySimulation = 75.0, QueueSimulation = 8 }
};

foreach (var scenario in adaptiveTestScenarios)
{
    // Simulate system conditions
    SimulateSystemConditions(scenario.CpuSimulation, scenario.MemorySimulation, scenario.QueueSimulation);
    
    var arguments = new KernelArguments
    {
        ["input_content"] = scenario.Content
    };

    Console.WriteLine($"⚡ Testing adaptive routing: {scenario.Content}");
    Console.WriteLine($"   Simulated: CPU {scenario.CpuSimulation}%, Memory {scenario.MemorySimulation}%, Queue {scenario.QueueSimulation}");
    
    var result = await adaptiveRouter.ExecuteAsync(kernel, arguments);
    
    var processingMode = result.GetValue<string>("processing_mode");
    var resourceUsage = result.GetValue<string>("resource_usage");
    var processingResult = result.GetValue<string>("processing_result");
    
    Console.WriteLine($"   Mode: {processingMode}");
    Console.WriteLine($"   Resources: {resourceUsage}");
    Console.WriteLine($"   Result: {processingResult}");
    Console.WriteLine();
}
```

### 4. Multi-Strategy Routing Orchestration

Demonstrates orchestrating multiple routing strategies for complex decision-making.

```csharp
// Create multi-strategy routing workflow
var multiStrategyRouter = new GraphExecutor("MultiStrategyRouter", "Multi-strategy routing orchestration", logger);

// Strategy coordinator
var strategyCoordinator = new FunctionGraphNode(
    "strategy-coordinator",
    "Coordinate multiple routing strategies",
    async (context) =>
    {
        var inputContent = context.GetValue<string>("input_content");
        var userContext = context.GetValue<Dictionary<string, object>>("user_context");
        
        // Evaluate multiple strategies
        var contentStrategy = EvaluateContentStrategy(inputContent);
        var userStrategy = EvaluateUserStrategy(userContext);
        var performanceStrategy = EvaluatePerformanceStrategy();
        var businessStrategy = EvaluateBusinessStrategy(inputContent);
        
        context.SetValue("content_strategy", contentStrategy);
        context.SetValue("user_strategy", userStrategy);
        context.SetValue("performance_strategy", performanceStrategy);
        context.SetValue("business_strategy", businessStrategy);
        context.SetValue("strategies_evaluated", true);
        
        return $"Strategies evaluated: {contentStrategy}, {userStrategy}, {performanceStrategy}, {businessStrategy}";
    });

// Strategy conflict resolver
var conflictResolver = new ConditionalGraphNode(
    "conflict-resolver",
    "Resolve conflicts between routing strategies",
    logger)
{
    ConditionExpression = "strategies_evaluated == true && (content_strategy != user_strategy || performance_strategy != business_strategy)",
    TrueNodeId = "conflict-resolution",
    FalseNodeId = "direct-routing"
};

// Conflict resolution processor
var conflictResolution = new FunctionGraphNode(
    "conflict-resolution",
    "Resolve routing strategy conflicts",
    async (context) =>
    {
        var contentStrategy = context.GetValue<string>("content_strategy");
        var userStrategy = context.GetValue<string>("user_strategy");
        var performanceStrategy = context.GetValue<string>("performance_strategy");
        var businessStrategy = context.GetValue<string>("business_strategy");
        
        // Apply conflict resolution logic
        var resolvedStrategy = ResolveStrategyConflicts(
            contentStrategy, userStrategy, performanceStrategy, businessStrategy);
        
        context.SetValue("resolved_strategy", resolvedStrategy);
        context.SetValue("conflict_resolved", true);
        
        return $"Conflict resolved: {resolvedStrategy}";
    });

// Direct routing for no conflicts
var directRouting = new FunctionGraphNode(
    "direct-routing",
    "Route directly based on primary strategy",
    async (context) =>
    {
        var contentStrategy = context.GetValue<string>("content_strategy");
        var userStrategy = context.GetValue<string>("user_strategy");
        
        // Use primary strategy
        var primaryStrategy = DeterminePrimaryStrategy(contentStrategy, userStrategy);
        context.SetValue("resolved_strategy", primaryStrategy);
        context.SetValue("conflict_resolved", false);
        
        return $"Direct routing: {primaryStrategy}";
    });

// Strategy execution router
var strategyExecutor = new ConditionalGraphNode(
    "strategy-executor",
    "Execute the resolved routing strategy",
    logger)
{
    ConditionExpression = "resolved_strategy == 'content_based'",
    TrueNodeId = "content-based-executor",
    FalseNodeId = "hybrid-executor"
};

// Content-based executor
var contentBasedExecutor = new FunctionGraphNode(
    "content-based-executor",
    "Execute content-based routing",
    async (context) =>
    {
        var content = context.GetValue<string>("input_content");
        var strategy = context.GetValue<string>("resolved_strategy");
        
        var result = await ExecuteContentBasedRouting(content, strategy);
        context.SetValue("execution_result", result);
        context.SetValue("execution_strategy", strategy);
        
        return $"Content-based execution: {result}";
    });

// Hybrid executor
var hybridExecutor = new FunctionGraphNode(
    "hybrid-executor",
    "Execute hybrid routing strategy",
    async (context) =>
    {
        var content = context.GetValue<string>("input_content");
        var strategy = context.GetValue<string>("resolved_strategy");
        var userContext = context.GetValue<Dictionary<string, object>>("user_context");
        
        var result = await ExecuteHybridRouting(content, strategy, userContext);
        context.SetValue("execution_result", result);
        context.SetValue("execution_strategy", strategy);
        
        return $"Hybrid execution: {result}";
    });

// Add nodes to multi-strategy router
multiStrategyRouter.AddNode(strategyCoordinator);
multiStrategyRouter.AddNode(conflictResolver);
multiStrategyRouter.AddNode(conflictResolution);
multiStrategyRouter.AddNode(directRouting);
multiStrategyRouter.AddNode(strategyExecutor);
multiStrategyRouter.AddNode(contentBasedExecutor);
multiStrategyRouter.AddNode(hybridExecutor);

// Set start node
multiStrategyRouter.SetStartNode(strategyCoordinator.NodeId);

// Test multi-strategy routing
var multiStrategyTestScenarios = new[]
{
    new { 
        Content = "Analyze this technical document", 
        UserContext = new Dictionary<string, object> { ["expertise"] = "beginner", ["preference"] = "detailed" }
    },
    new { 
        Content = "Quick summary needed", 
        UserContext = new Dictionary<string, object> { ["expertise"] = "expert", ["preference"] = "concise" }
    }
};

foreach (var scenario in multiStrategyTestScenarios)
{
    var arguments = new KernelArguments
    {
        ["input_content"] = scenario.Content,
        ["user_context"] = scenario.UserContext
    };

    Console.WriteLine($"🎯 Testing multi-strategy routing: {scenario.Content}");
    Console.WriteLine($"   User Context: {string.Join(", ", scenario.UserContext.Select(kv => $"{kv.Key}={kv.Value}"))}");
    
    var result = await multiStrategyRouter.ExecuteAsync(kernel, arguments);
    
    var resolvedStrategy = result.GetValue<string>("resolved_strategy");
    var conflictResolved = result.GetValue<bool>("conflict_resolved");
    var executionStrategy = result.GetValue<string>("execution_strategy");
    var executionResult = result.GetValue<string>("execution_result");
    
    Console.WriteLine($"   Resolved Strategy: {resolvedStrategy}");
    Console.WriteLine($"   Conflict Resolved: {conflictResolved}");
    Console.WriteLine($"   Execution Strategy: {executionStrategy}");
    Console.WriteLine($"   Result: {executionResult}");
    Console.WriteLine();
}
```

## Expected Output

### Basic Dynamic Routing Example

```
🧪 Testing content: How do I implement a binary search tree in C#?
   Routing: expert_processed
   Level: expert
   Result: Expert technical implementation guide provided

🧪 Testing content: What's the weather like today?
   Routing: standard_processed
   Level: standard
   Result: Weather information retrieved and formatted
```

### Advanced Semantic Routing Example

```
🧠 Testing semantic routing: I need help with implementing a microservices architecture
   Specialization: technical_expert
   Result: Comprehensive microservices implementation guide

🧠 Testing semantic routing: Can you analyze this quarterly financial report?
   Specialization: business
   Result: Financial analysis with key metrics and insights
```

### Real-Time Adaptive Routing Example

```
⚡ Testing adaptive routing: Process this urgent request
   Simulated: CPU 50%, Memory 60%, Queue 5
   Mode: high_performance
   Resources: optimized
   Result: High-performance processing completed

⚡ Testing adaptive routing: Handle this batch job
   Simulated: CPU 85%, Memory 90%, Queue 25
   Mode: load_balanced
   Resources: conservative
   Result: Load-balanced processing completed
```

### Multi-Strategy Routing Example

```
🎯 Testing multi-strategy routing: Analyze this technical document
   User Context: expertise=beginner, preference=detailed
   Resolved Strategy: content_based
   Conflict Resolved: False
   Execution Strategy: content_based
   Result: Detailed technical analysis with explanations

🎯 Testing multi-strategy routing: Quick summary needed
   User Context: expertise=expert, preference=concise
   Resolved Strategy: hybrid
   Conflict Resolved: True
   Execution Strategy: hybrid
   Result: Concise summary with expert-level insights
```

## Configuration Options

### Dynamic Routing Configuration

```csharp
var routingOptions = new DynamicRoutingOptions
{
    EnableSemanticRouting = true,                    // Enable semantic similarity routing
    EnablePerformanceRouting = true,                 // Enable performance-based routing
    EnableUserContextRouting = true,                 // Enable user context routing
    MaxRoutingStrategies = 5,                       // Maximum strategies to evaluate
    RoutingTimeout = TimeSpan.FromSeconds(10),       // Routing decision timeout
    EnableStrategyCaching = true,                    // Cache routing strategy decisions
    EnableConflictResolution = true,                 // Enable automatic conflict resolution
    DefaultRoutingStrategy = "content_based",        // Default strategy when conflicts occur
    PerformanceThresholds = new PerformanceThresholds
    {
        CpuUsageThreshold = 80.0,                   // CPU usage threshold for routing
        MemoryUsageThreshold = 85.0,                // Memory usage threshold for routing
        QueueLengthThreshold = 15,                   // Queue length threshold for routing
        ResponseTimeThreshold = TimeSpan.FromSeconds(2) // Response time threshold
    }
};
```

### Semantic Routing Configuration

```csharp
var semanticOptions = new SemanticRoutingOptions
{
    EnableTopicExtraction = true,                    // Extract topics for routing
    EnableSentimentAnalysis = true,                  // Analyze sentiment for routing
    EnableDomainClassification = true,               // Classify content domains
    EnableIntentDetection = true,                    // Detect user intent
    SimilarityThreshold = 0.7,                      // Minimum similarity score
    MaxTopics = 10,                                 // Maximum topics to extract
    EnableEmbeddingCaching = true,                   // Cache embeddings for performance
    DomainMappings = new Dictionary<string, string>  // Domain to processor mappings
    {
        ["technical"] = "technical_processor",
        ["business"] = "business_processor",
        ["general"] = "general_processor"
    }
};
```

## Troubleshooting

### Common Issues

#### Routing Decisions Not Working
```bash
# Problem: Routing decisions not following expected logic
# Solution: Check condition expressions and node IDs
ConditionExpression = "simple_condition == true";
TrueNodeId = "correct-node-id";
FalseNodeId = "correct-node-id";
```

#### Performance Issues
```bash
# Problem: Dynamic routing is slow
# Solution: Enable caching and optimize strategy evaluation
EnableStrategyCaching = true;
EnableEmbeddingCaching = true;
MaxRoutingStrategies = 3;
```

#### Strategy Conflicts
```bash
# Problem: Multiple strategies conflict
# Solution: Implement conflict resolution and set priorities
EnableConflictResolution = true;
DefaultRoutingStrategy = "fallback_strategy";
```

### Debug Mode

Enable detailed logging for troubleshooting:

```csharp
// Enable debug logging
var logger = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
}).CreateLogger<DynamicRoutingExample>();

// Configure dynamic routing with debug logging
var debugRouter = new GraphExecutor("DebugRouter", "Debug dynamic routing", logger);
debugRouter.EnableDebugMode = true;
debugRouter.LogRoutingDecisions = true;
debugRouter.LogStrategyEvaluation = true;
```

## Advanced Patterns

### Custom Routing Strategies

```csharp
// Implement custom routing strategies
public class CustomRoutingStrategy : IRoutingStrategy
{
    public async Task<RoutingDecision> EvaluateAsync(RoutingContext context)
    {
        var content = context.Content;
        var userContext = context.UserContext;
        var systemMetrics = context.SystemMetrics;
        
        // Custom evaluation logic
        var score = await CalculateCustomScore(content, userContext, systemMetrics);
        
        return new RoutingDecision
        {
            Strategy = "custom",
            Confidence = score,
            TargetNode = score > 0.8 ? "high_priority_processor" : "standard_processor",
            Metadata = new Dictionary<string, object>
            {
                ["custom_score"] = score,
                ["evaluation_timestamp"] = DateTime.UtcNow
            }
        };
    }
}
```

### Machine Learning Routing

```csharp
// Implement ML-based routing
public class MLRoutingEngine : IRoutingEngine
{
    private readonly IMLModel _routingModel;
    
    public async Task<RoutingDecision> PredictRouteAsync(RoutingContext context)
    {
        // Prepare features for ML model
        var features = await ExtractFeatures(context);
        
        // Get ML prediction
        var prediction = await _routingModel.PredictAsync(features);
        
        return new RoutingDecision
        {
            Strategy = "ml_prediction",
            Confidence = prediction.Confidence,
            TargetNode = prediction.TargetNode,
            Metadata = new Dictionary<string, object>
            {
                ["ml_model_version"] = prediction.ModelVersion,
                ["prediction_features"] = features
            }
        };
    }
}
```

### A/B Testing Routing

```csharp
// Implement A/B testing for routing strategies
public class ABTestingRouter : IRouter
{
    private readonly IRandom _random;
    private readonly Dictionary<string, double> _strategyWeights;
    
    public async Task<string> RouteAsync(RoutingContext context)
    {
        // Determine A/B test group
        var testGroup = DetermineTestGroup(context.UserId);
        
        // Apply A/B testing weights
        var strategy = await SelectStrategyWithABTesting(context, testGroup);
        
        return strategy;
    }
    
    private async Task<string> SelectStrategyWithABTesting(RoutingContext context, string testGroup)
    {
        var random = _random.NextDouble();
        var cumulativeWeight = 0.0;
        
        foreach (var strategy in _strategyWeights)
        {
            cumulativeWeight += strategy.Value;
            if (random <= cumulativeWeight)
            {
                return strategy.Key;
            }
        }
        
        return _strategyWeights.Keys.First();
    }
}
```

## Related Examples

- [Conditional Nodes](./conditional-nodes.md): Basic conditional routing
- [Advanced Patterns](./advanced-patterns.md): Complex routing patterns
- [Multi-Agent](./multi-agent.md): Coordinated routing decisions
- [Performance Optimization](./performance-optimization.md): Routing performance tuning

## See Also

- [Dynamic Routing Concepts](../concepts/dynamic-routing.md): Understanding dynamic routing
- [Routing Strategies](../patterns/routing-strategies.md): Routing pattern implementations
- [Performance Optimization](../how-to/performance-optimization.md): Optimizing routing performance
- [API Reference](../api/): Complete API documentation
