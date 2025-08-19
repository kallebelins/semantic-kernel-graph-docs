# Optimizers and Few-Shot Example

This example demonstrates how to combine advanced optimizers with few-shot prompting patterns in Semantic Kernel Graph workflows.

## Objective

Learn how to implement and optimize few-shot prompting workflows in graph-based systems to:
- Create few-shot classification and response generation workflows
- Enable advanced optimization engines for performance improvement
- Implement machine learning optimization with incremental learning
- Combine few-shot patterns with performance prediction
- Demonstrate lightweight optimization in simple graph structures

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Optimization](../how-to/advanced-optimizations.md)
- Familiarity with [Few-Shot Learning](../concepts/few-shot-learning.md)

## Key Components

### Concepts and Techniques

- **Few-Shot Prompting**: Using examples to guide AI model responses
- **Advanced Optimizations**: Performance optimization based on execution metrics
- **Machine Learning Optimization**: ML-based performance prediction and improvement
- **Performance Metrics**: Tracking and analyzing execution performance
- **Incremental Learning**: Continuous model improvement based on new data

### Core Classes

- `GraphExecutor`: Executor with optimization capabilities
- `FunctionGraphNode`: Nodes for few-shot classification and response generation
- `GraphPerformanceMetrics`: Performance tracking and analysis
- `ConditionalEdge`: Graph edges for workflow control
- `GraphConfiguration`: Configuration for performance prediction

## Running the Example

### Command Line

```bash
# Navigate to examples project
cd semantic-kernel-graph/src/SemanticKernel.Graph.Examples

# Run the Optimizers and Few-Shot example
dotnet run -- --example optimizers-and-few-shot
```

### Programmatic Execution

```csharp
// Run the example with custom kernel
var kernel = CreateCustomKernel();

await OptimizersAndFewShotExample.RunAsync(kernel);
```

## Step-by-Step Implementation

### 1. Creating the Few-Shot Graph

The example starts by building a minimal graph with few-shot prompt functions.

```csharp
// Build a minimal graph with few-shot prompt functions
var executor = new GraphExecutor("FewShotWithOptimizers", "Few-shot prompting with optimization engines");

var classify = new FunctionGraphNode(
    CreateFewShotClassifierFunction(kernel),
    "fewshot_classifier",
    "Classify the user request into a category using few-shot examples"
).StoreResultAs("category");

var respond = new FunctionGraphNode(
    CreateFewShotAnswerFunction(kernel),
    "fewshot_answer",
    "Generate a concise, high-quality answer using few-shot guidance"
).StoreResultAs("final_answer");

executor.AddNode(classify);
executor.AddNode(respond);
executor.SetStartNode(classify.NodeId);
executor.AddEdge(ConditionalEdge.CreateUnconditional(classify, respond));
```

### 2. Enabling Advanced Optimizations

The executor is configured with advanced optimization and machine learning capabilities.

```csharp
// Enable optimizers (advanced + ML) with lightweight defaults
executor.WithAdvancedOptimizations();
executor.WithMachineLearningOptimization(options =>
{
    options.EnableIncrementalLearning = true;
});
```

### 3. Few-Shot Classification Function

The classification function uses few-shot examples to categorize user requests.

```csharp
private static KernelFunction CreateFewShotClassifierFunction(Kernel kernel)
{
    return KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var input = args.TryGetValue("input", out var i) ? i?.ToString() ?? string.Empty : string.Empty;
            
            // Few-shot examples for classification
            var examples = @"
Examples:
Input: 'Summarize this article about distributed systems in simple terms.'
Category: summarization

Input: 'Translate the following text to Portuguese: The system achieved 99.9% uptime.'
Category: translation

Input: 'Classify the sentiment of: I love how responsive this app is!'
Category: sentiment_analysis

Input: 'Explain quantum computing in simple terms.'
Category: explanation

Input: 'Generate a creative story about a robot.'
Category: creative_writing

Now classify this input: " + input;

            // Simple classification logic based on keywords
            var category = input.ToLowerInvariant() switch
            {
                var s when s.Contains("summarize") || s.Contains("summary") => "summarization",
                var s when s.Contains("translate") || s.Contains("portuguese") => "translation",
                var s when s.Contains("sentiment") || s.Contains("love") || s.Contains("hate") => "sentiment_analysis",
                var s when s.Contains("explain") || s.Contains("explanation") => "explanation",
                var s when s.Contains("story") || s.Contains("creative") => "creative_writing",
                _ => "general_query"
            };

            args["category"] = category;
            return $"Classified as: {category}";
        },
        functionName: "fewshot_classifier",
        description: "Classifies user requests using few-shot examples"
    );
}
```

### 4. Few-Shot Answer Function

The answer function generates responses using few-shot guidance patterns.

```csharp
private static KernelFunction CreateFewShotAnswerFunction(Kernel kernel)
{
    return KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var input = args.TryGetValue("input", out var i) ? i?.ToString() ?? string.Empty : string.Empty;
            var category = args.TryGetValue("category", out var c) ? c?.ToString() ?? string.Empty : string.Empty;
            
            // Few-shot examples for response generation
            var examples = @"
Examples:
Input: 'Summarize this article about distributed systems in simple terms.'
Category: summarization
Response: 'Distributed systems are networks of computers that work together to solve problems. They're like a team where each member has a specific job, making the whole system more reliable and faster than a single computer.'

Input: 'Translate the following text to Portuguese: The system achieved 99.9% uptime.'
Category: translation
Response: 'O sistema alcançou 99,9% de tempo de atividade.'

Input: 'Classify the sentiment of: I love how responsive this app is!'
Category: sentiment_analysis
Response: 'Positive sentiment. The user expresses strong satisfaction with the app's responsiveness using enthusiastic language like "love" and an exclamation mark.'

Now generate a response for:
Input: " + input + @"
Category: " + category;

            // Generate response based on category
            var response = category switch
            {
                "summarization" => $"Here's a simple summary: {input.Replace("summarize", "").Replace("in simple terms", "").Trim()} can be explained as a fundamental concept that helps us understand complex topics more easily.",
                "translation" => $"Translation: {input.Replace("Translate the following text to Portuguese:", "").Trim()} would be translated to Portuguese as requested.",
                "sentiment_analysis" => $"Sentiment Analysis: {input.Replace("Classify the sentiment of:", "").Trim()} shows positive sentiment with enthusiastic language.",
                "explanation" => $"Explanation: {input.Replace("Explain", "").Replace("in simple terms", "").Trim()} is a concept that can be broken down into simple, understandable parts.",
                "creative_writing" => $"Creative Response: Here's a creative take on {input.Replace("Generate a creative story about", "").Trim()}: A fascinating tale unfolds...",
                _ => $"General Response: {input} - This is a general query that can be addressed with standard information and guidance."
            };

            args["final_answer"] = response;
            return response;
        },
        functionName: "fewshot_answer",
        description: "Generates responses using few-shot guidance patterns"
    );
}
```

### 5. Sample Input Processing

The example processes multiple sample inputs to demonstrate the workflow.

```csharp
// Run a few sample inputs
var samples = new[]
{
    "Summarize this article about distributed systems in simple terms.",
    "Translate the following text to Portuguese: 'The system achieved 99.9% uptime.'",
    "Classify the sentiment of: 'I love how responsive this app is!'"
};

foreach (var input in samples)
{
    Console.WriteLine($"🧑‍💻 User: {input}");
    var args = new KernelArguments
    {
        ["input"] = input
    };

    var result = await executor.ExecuteAsync(kernel, args);
    var state = args.GetOrCreateGraphState();
    var category = state.GetValue<string>("category") ?? "(unknown)";
    var answer = state.GetValue<string>("final_answer") ?? result.GetValue<string>() ?? "No answer produced";
    Console.WriteLine($"📂 Category: {category}");
    Console.WriteLine($"🤖 Answer: {answer}\n");
    await Task.Delay(150);
}
```

### 6. Performance Metrics and Optimization

The example demonstrates optimization usage with performance metrics.

```csharp
// Demonstrate optimizers usage briefly
var metrics = new GraphPerformanceMetrics(new GraphMetricsOptions(), executor.GetService<IGraphLogger>());

// Simulate a few node runs to generate basic metrics
for (int i = 0; i < 6; i++)
{
    var tracker = metrics.StartNodeTracking(classify.NodeId, "FewShotClassifier", $"exec_{i}");
    await Task.Delay(Random.Shared.Next(30, 90));
    metrics.CompleteNodeTracking(tracker, success: true);
}

var optimizationResult = await executor.OptimizeAsync(metrics);
Console.WriteLine($"🔧 Optimizer suggestions: {optimizationResult.TotalOptimizations} " +
                 $"(paths: {optimizationResult.PathOptimizations.Count}, " +
                 $"nodes: {optimizationResult.NodeOptimizations.Count})");
```

### 7. Machine Learning Training and Prediction

The example demonstrates lightweight ML training and performance prediction.

```csharp
// Lightweight ML training + prediction
var history = GenerateTinyPerformanceHistory();
var training = await executor.TrainModelsAsync(history);

if (training.Success)
{
    var prediction = await executor.PredictPerformanceAsync(new GraphConfiguration
    {
        NodeCount = 2,
        AveragePathLength = 2,
        ConditionalNodeCount = 0,
        LoopNodeCount = 0,
        ParallelNodeCount = 0
    });
    
    Console.WriteLine($"🔮 Predicted latency: {prediction.PredictedLatency.TotalMilliseconds:F1}ms | " +
                     $"Confidence: {prediction.Confidence:P1}");
}
```

### 8. Performance History Generation

The example generates simulated performance history for ML training.

```csharp
private static List<GraphPerformanceHistory> GenerateTinyPerformanceHistory()
{
    var random = new Random(42); // Fixed seed for consistent results
    var history = new List<GraphPerformanceHistory>();

    for (int i = 0; i < 10; i++)
    {
        var entry = new GraphPerformanceHistory
        {
            Timestamp = DateTimeOffset.UtcNow.AddHours(-i),
            GraphConfiguration = new GraphConfiguration
            {
                NodeCount = 2,
                AveragePathLength = 2.0,
                ConditionalNodeCount = 0,
                LoopNodeCount = 0,
                ParallelNodeCount = 0
            },
            AverageLatency = TimeSpan.FromMilliseconds(50 + random.Next(20)),
            Throughput = 80 + random.Next(40),
            SuccessRate = 95 + random.Next(5),
            AppliedOptimizations = random.Next(100) < 30 ? 
                new[] { "caching", "parallel" } : Array.Empty<string>()
        };

        history.Add(entry);
    }

    return history;
}
```

## Expected Output

The example produces comprehensive output showing:

- 🧑‍💻 User input processing with few-shot classification
- 📂 Category classification results
- 🤖 Generated answers using few-shot guidance
- 🔧 Optimization suggestions and performance metrics
- 🔮 ML-based performance predictions
- ✅ Complete few-shot workflow with optimization

## Troubleshooting

### Common Issues

1. **Few-Shot Classification Failures**: Ensure examples are clear and cover expected input types
2. **Optimization Errors**: Check that performance metrics are properly collected
3. **ML Training Failures**: Verify sufficient historical data for training
4. **Performance Issues**: Monitor optimization suggestions and adjust thresholds

### Debugging Tips

- Enable detailed logging to trace few-shot classification
- Monitor performance metrics collection and optimization results
- Verify ML training data quality and quantity
- Check optimization engine configuration and thresholds

## See Also

- [Advanced Optimizations](../how-to/advanced-optimizations.md)
- [Few-Shot Learning](../concepts/few-shot-learning.md)
- [Performance Metrics](../how-to/metrics-and-observability.md)
- [Machine Learning Optimization](../concepts/ml-optimization.md)
- [Graph Optimization](../how-to/graph-optimization.md)
