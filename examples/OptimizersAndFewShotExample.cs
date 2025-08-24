using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Integration;
using SemanticKernel.Graph.Nodes;

namespace Examples;

/// <summary>
/// Demonstrates few-shot prompting combined with lightweight optimizer usage.
/// This example mirrors the documentation and is intended to compile and run
/// inside the examples project for validation.
/// </summary>
public static class OptimizersAndFewShotExample
{
    /// <summary>
    /// Run the example: build a small graph, run sample inputs, collect metrics,
    /// and run a lightweight optimization step.
    /// </summary>
    public static async Task RunAsync()
    {
        // Create a kernel with no real API keys (safe for local tests)
        var kernel = Kernel.CreateBuilder()
            .AddGraphSupport()
            .Build();

        Console.WriteLine("=== Optimizers + Few-Shot Example - Docs Demo ===\n");

        // Build executor and nodes
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

        // Enable simple optimizer behaviors (no external services required)
        executor.WithAdvancedOptimizations();
        executor.WithMachineLearningOptimization(options => { options.EnableIncrementalLearning = true; });

        // Run sample inputs
        var samples = new[]
        {
            "Summarize this article about distributed systems in simple terms.",
            "Translate the following text to Portuguese: 'The system achieved 99.9% uptime.'",
            "Classify the sentiment of: 'I love how responsive this app is!'"
        };

        foreach (var input in samples)
        {
            Console.WriteLine($"🧑‍💻 User: {input}");
            var args = new KernelArguments { ["input"] = input };

            var result = await executor.ExecuteAsync(kernel, args);
            var state = args.GetOrCreateGraphState();
            var category = state.GetValue<string>("category") ?? "(unknown)";
            var answer = state.GetValue<string>("final_answer") ?? result.GetValue<string>() ?? "No answer produced";
            Console.WriteLine($"📂 Category: {category}");
            Console.WriteLine($"🤖 Answer: {answer}\n");
            await Task.Delay(100);
        }

        // Generate metrics and run a light optimizer pass to verify integration
        var metrics = new GraphPerformanceMetrics(new GraphMetricsOptions(), executor.GetService<IGraphLogger>());

        for (int i = 0; i < 4; i++)
        {
            var tracker = metrics.StartNodeTracking(classify.NodeId, "FewShotClassifier", $"exec_{i}");
            await Task.Delay(20 + i * 10);
            metrics.CompleteNodeTracking(tracker, success: true);
        }

        var optimizationResult = await executor.OptimizeAsync(metrics);
        Console.WriteLine($"🔧 Optimizer suggestions: {optimizationResult.TotalOptimizations} " +
                          $"(paths: {optimizationResult.PathOptimizations.Count}, nodes: {optimizationResult.NodeOptimizations.Count})");

        // Generate tiny history and run a training pass
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

            Console.WriteLine($"🔮 Predicted latency: {prediction.PredictedLatency.TotalMilliseconds:F1}ms | Confidence: {prediction.Confidence:P1}");
        }

        Console.WriteLine("✅ Optimizers + Few-Shot docs demo completed!\n");
    }

    private static KernelFunction CreateFewShotClassifierFunction(Kernel kernel)
    {
        // Create a simple kernel function that classifies based on keywords and stores 'category'
        return kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var input = args.TryGetValue("input", out var i) ? i?.ToString() ?? string.Empty : string.Empty;

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

    private static KernelFunction CreateFewShotAnswerFunction(Kernel kernel)
    {
        // Create a simple response generator based on the category
        return kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var input = args.TryGetValue("input", out var i) ? i?.ToString() ?? string.Empty : string.Empty;
                var category = args.TryGetValue("category", out var c) ? c?.ToString() ?? string.Empty : string.Empty;

                var response = category switch
                {
                    "summarization" => $"Here's a simple summary: {input.Replace("summarize", "").Replace("in simple terms", "").Trim()}.",
                    "translation" => $"Translation: {input.Replace("Translate the following text to Portuguese:", "").Trim()}",
                    "sentiment_analysis" => $"Sentiment Analysis: {input.Replace("Classify the sentiment of:", "").Trim()} shows positive sentiment.",
                    "explanation" => $"Explanation: {input.Replace("Explain", "").Replace("in simple terms", "").Trim()}.",
                    "creative_writing" => $"Creative Response: A creative take on {input.Replace("Generate a creative story about", "").Trim()}.",
                    _ => $"General Response: {input}"
                };

                args["final_answer"] = response;
                return response;
            },
            functionName: "fewshot_answer",
            description: "Generates responses using few-shot guidance patterns"
        );
    }

    private static List<GraphPerformanceHistory> GenerateTinyPerformanceHistory()
    {
        var random = new Random(42);
        var history = new List<GraphPerformanceHistory>();

        for (int i = 0; i < 8; i++)
        {
            var entry = new GraphPerformanceHistory
            {
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-i),
                GraphConfiguration = new GraphConfiguration
                {
                    NodeCount = 2,
                    AveragePathLength = 2.0,
                    ConditionalNodeCount = 0,
                    LoopNodeCount = 0,
                    ParallelNodeCount = 0
                },
                AverageLatency = TimeSpan.FromMilliseconds(40 + random.Next(40)),
                Throughput = 50 + random.Next(50),
                SuccessRate = 90 + random.Next(10),
                AppliedOptimizations = random.Next(100) < 30 ? new[] { "caching" } : Array.Empty<string>()
            };

            history.Add(entry);
        }

        return history;
    }
}


