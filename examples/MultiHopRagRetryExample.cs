using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Integration;
using SemanticKernel.Graph.Nodes;

namespace Examples;

/// <summary>
/// Multi-hop RAG with retry and query refinement over a knowledge base.
/// Implements an iterative retrieval loop that attempts multiple times to fetch
/// sufficient context before synthesizing an answer. Each retry widens the search
/// (e.g., increases <c>top_k</c>, reduces <c>min_score</c>) and refines the query.
/// </summary>
public static class MultiHopRagRetryExample
{
    /// <summary>
    /// Runs the multi-hop RAG example. The flow is:
    /// analyze_question → attempt_retrieval → evaluate_context → (refine_query → attempt_retrieval)* → synthesize_answer.
    /// This copy is intended for documentation and demonstration purposes.
    /// </summary>
    public static async Task RunAsync()
    {
        // Create a minimal kernel instance for the example and pass it to the RunAsync method
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-3.5-turbo", Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "mock-api-key")
            .AddGraphSupport()
            .Build();

        Console.WriteLine("=== Multi-Hop RAG (Retry + Refinement) - Docs Example ===\n");

        var memoryProvider = new KernelMemoryGraphProvider();
        var collection = "kb_multihop_docs";

        await SeedKnowledgeBaseAsync(memoryProvider, collection);

        var executor = CreateMultiHopRagExecutor(kernel, memoryProvider, collection);

        var scenarios = new[]
        {
            "What does the data privacy policy mandate about encryption and retention?",
            "Tell me about customer docs and secure handling",
            "Summarize insights from the business reports and performance tracking"
        };

        foreach (var question in scenarios)
        {
            Console.WriteLine($"🧑‍💻 User: {question}");
            var args = new KernelArguments
            {
                ["user_question"] = question,
                ["max_attempts"] = 4,
                ["min_required_chunks"] = 2,
                ["top_k"] = 4,
                ["min_score"] = 0.45
            };

            var result = await executor.ExecuteAsync(kernel, args);
            var answer = result.GetValue<string>() ?? "No answer produced";
            Console.WriteLine($"🤖 Agent: {answer}\n");
            await Task.Delay(250);
        }

        Console.WriteLine("✅ Multi-Hop RAG docs demo completed!\n");
    }

    // The following methods are a trimmed copy of the implementation used in the examples
    // They mirror the real example to ensure documentation code compiles and runs when copied.

    private static GraphExecutor CreateMultiHopRagExecutor(Kernel kernel, KernelMemoryGraphProvider provider, string collection)
    {
        var executor = new GraphExecutor("MultiHopRagRetryDocs", "Multi-hop RAG with retry and refinement (docs)");

        var analyze = new FunctionGraphNode(
            CreateInitialQueryFunction(kernel),
            "analyze_question",
            "Analyze the user question and produce an initial search query"
        ).StoreResultAs("search_query");

        var retrieve = new FunctionGraphNode(
            CreateAttemptRetrievalFunction(kernel, provider, collection),
            "attempt_retrieval",
            "Attempt to retrieve relevant context from the knowledge base"
        ).StoreResultAs("retrieved_context");

        var evaluate = new FunctionGraphNode(
            CreateEvaluateContextFunction(kernel),
            "evaluate_context",
            "Evaluate if retrieved context is sufficient or if we should retry"
        ).StoreResultAs("evaluation_message");

        var refine = new FunctionGraphNode(
            CreateRefineQueryFunction(kernel),
            "refine_query",
            "Refine the query and retry with wider parameters"
        ).StoreResultAs("search_query");

        var answer = new FunctionGraphNode(
            CreateSynthesizeAnswerFunction(kernel),
            "synthesize_answer",
            "Synthesize a final answer using the accumulated retrieved context"
        ).StoreResultAs("final_answer");

        executor.AddNode(analyze);
        executor.AddNode(retrieve);
        executor.AddNode(evaluate);
        executor.AddNode(refine);
        executor.AddNode(answer);

        executor.SetStartNode(analyze.NodeId);
        executor.AddEdge(ConditionalEdge.CreateUnconditional(analyze, retrieve));
        executor.AddEdge(ConditionalEdge.CreateUnconditional(retrieve, evaluate));

        executor.AddEdge(new ConditionalEdge(
            evaluate,
            refine,
            args => ShouldContinueRetrieval(args),
            "Retry Retrieval"
        ));

        executor.AddEdge(new ConditionalEdge(
            evaluate,
            answer,
            args => !ShouldContinueRetrieval(args),
            "Finalize Answer"
        ));

        executor.AddEdge(ConditionalEdge.CreateUnconditional(refine, retrieve));

        return executor;
    }

    private static bool ShouldContinueRetrieval(KernelArguments args)
    {
        var attempt = TryGetInt(args, "attempt", 0);
        var maxAttempts = TryGetInt(args, "max_attempts", 3);
        var retrievedCount = TryGetInt(args, "retrieved_count", 0);
        var minRequired = TryGetInt(args, "min_required_chunks", 2);

        if (retrievedCount >= minRequired)
        {
            return false;
        }

        return attempt < maxAttempts;
    }

    private static KernelFunction CreateInitialQueryFunction(Kernel kernel)
    {
        return kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var question = args.GetValueOrDefault("user_question")?.ToString() ?? string.Empty;

                if (!args.ContainsName("attempt")) args["attempt"] = 0;
                if (!args.ContainsName("max_attempts")) args["max_attempts"] = 3;
                if (!args.ContainsName("min_required_chunks")) args["min_required_chunks"] = 2;
                if (!args.ContainsName("top_k")) args["top_k"] = 4;
                if (!args.ContainsName("min_score")) args["min_score"] = 0.45;

                var query = question
                    .Replace("What", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("How", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("Explain", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("?", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Trim();

                return string.IsNullOrWhiteSpace(query) ? question : query;
            },
            functionName: "analyze_question",
            description: "Analyzes the user question and outputs a compact search query"
        );
    }

    private static KernelFunction CreateAttemptRetrievalFunction(Kernel kernel, KernelMemoryGraphProvider provider, string collection)
    {
        return kernel.CreateFunctionFromMethod(
            async (KernelArguments args) =>
            {
                var query = args.GetValueOrDefault("search_query")?.ToString()
                    ?? args.GetValueOrDefault("user_question")?.ToString()
                    ?? string.Empty;

                var topK = TryGetInt(args, "top_k", 4);
                var minScore = TryGetDouble(args, "min_score", 0.45);

                var enumerator = await provider.SearchAsync(collection, query, Math.Max(1, topK), Math.Clamp(minScore, 0.0, 1.0));
                var snippets = new List<string>();
                await foreach (var item in enumerator)
                {
                    snippets.Add(item.Text);
                }

                args["retrieved_count"] = snippets.Count;

                if (snippets.Count == 0)
                {
                    return string.Empty;
                }

                var joined = string.Join(" \n--- \n", snippets);
                return joined.Length > 4000 ? joined[..4000] + "…" : joined;
            },
            functionName: "attempt_retrieval",
            description: "Retrieves relevant context from the knowledge base for the current query"
        );
    }

    private static KernelFunction CreateEvaluateContextFunction(Kernel kernel)
    {
        return kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var attempt = TryGetInt(args, "attempt", 0);
                var maxAttempts = TryGetInt(args, "max_attempts", 3);
                var retrievedCount = TryGetInt(args, "retrieved_count", 0);
                var minRequired = TryGetInt(args, "min_required_chunks", 2);

                var status = retrievedCount >= minRequired
                    ? $"✅ Sufficient context collected (chunks={retrievedCount})."
                    : $"ℹ️ Insufficient context (chunks={retrievedCount} < required={minRequired}). Attempt {attempt}/{maxAttempts}.";

                return status;
            },
            functionName: "evaluate_context",
            description: "Evaluates sufficiency of the retrieved context"
        );
    }

    private static KernelFunction CreateRefineQueryFunction(Kernel kernel)
    {
        return kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var attempt = TryGetInt(args, "attempt", 0) + 1;
                args["attempt"] = attempt;

                var topK = TryGetInt(args, "top_k", 4);
                var minScore = TryGetDouble(args, "min_score", 0.45);
                var baseQuery = args.GetValueOrDefault("search_query")?.ToString() ?? string.Empty;

                var newTopK = Math.Min(topK + 2, 12);
                var newMinScore = Math.Max(0.20, minScore - 0.05);
                args["top_k"] = newTopK;
                args["min_score"] = newMinScore;

                var refined = ApplyHeuristicRefinements(baseQuery, args.GetValueOrDefault("user_question")?.ToString() ?? string.Empty, attempt);
                return refined;
            },
            functionName: "refine_query",
            description: "Refines the search query and relaxes thresholds for the next attempt"
        );
    }

    private static KernelFunction CreateSynthesizeAnswerFunction(Kernel kernel)
    {
        return kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var question = args.GetValueOrDefault("user_question")?.ToString() ?? string.Empty;
                var context = args.GetValueOrDefault("retrieved_context")?.ToString() ?? string.Empty;
                var attempt = TryGetInt(args, "attempt", 0);
                var retrievedCount = TryGetInt(args, "retrieved_count", 0);

                if (string.IsNullOrWhiteSpace(context))
                {
                    return $"I could not retrieve sufficient information to answer: '{question}'. Attempts: {attempt}, retrieved chunks: {retrievedCount}.";
                }

                var preview = context.Length > 600 ? context[..600] + "…" : context;
                return $"Answer to: '{question}'\n\nBased on retrieved context (chunks={retrievedCount}, attempts={attempt}):\n{preview}";
            },
            functionName: "synthesize_answer",
            description: "Formats a final answer using retrieved context"
        );
    }

    private static string ApplyHeuristicRefinements(string query, string question, int attempt)
    {
        var cleaned = (string.IsNullOrWhiteSpace(query) ? question : query).Trim();

        var expansions = new List<string>();
        var ql = (cleaned + " " + question).ToLowerInvariant();

        if (ql.Contains("policy") || ql.Contains("privacy") || ql.Contains("security"))
        {
            expansions.Add("policy");
            expansions.Add("privacy");
            expansions.Add("security");
            expansions.Add("encryption");
            expansions.Add("retention");
            expansions.Add("mfa");
        }
        if (ql.Contains("report") || ql.Contains("business") || ql.Contains("performance"))
        {
            expansions.Add("report");
            expansions.Add("business");
            expansions.Add("performance");
            expansions.Add("quarterly");
            expansions.Add("metrics");
        }
        if (ql.Contains("customer") || ql.Contains("document") || ql.Contains("docs"))
        {
            expansions.Add("customer");
            expansions.Add("documents");
            expansions.Add("handling");
            expansions.Add("storage");
            expansions.Add("access");
        }

        expansions = expansions.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var takeCount = Math.Min(expansions.Count, Math.Max(0, attempt + 1) * 3);
        var chosen = expansions.Take(takeCount);

        var refined = string.Join(" ", new[] { cleaned }.Concat(chosen).Where(s => !string.IsNullOrWhiteSpace(s)));
        return refined;
    }

    private static int TryGetInt(KernelArguments args, string name, int fallback)
    {
        if (args.TryGetValue(name, out var value) && int.TryParse(value?.ToString(), out var parsed))
        {
            return parsed;
        }
        return fallback;
    }

    private static double TryGetDouble(KernelArguments args, string name, double fallback)
    {
        if (args.TryGetValue(name, out var value) && double.TryParse(value?.ToString(), out var parsed))
        {
            return parsed;
        }
        return fallback;
    }

    private static async Task SeedKnowledgeBaseAsync(KernelMemoryGraphProvider provider, string collection)
    {
        await provider.SaveInformationAsync(collection,
            "The data privacy policy mandates encryption at rest and in transit, requires multi-factor authentication (MFA), and limits data retention to 24 months.",
            "mh-001",
            "Corporate data privacy policy",
            "category:policy");

        await provider.SaveInformationAsync(collection,
            "Customer documentation must be handled securely with restricted access controls and audited storage locations.",
            "mh-002",
            "Customer documentation handling guidelines",
            "category:customer");

        await provider.SaveInformationAsync(collection,
            "Quarterly business reports indicate improved performance metrics due to optimized workflows and better resource allocation.",
            "mh-003",
            "Quarterly business report summary",
            "category:report");

        await provider.SaveInformationAsync(collection,
            "Performance tracking dashboards show a steady increase in throughput and a reduction in processing latency.",
            "mh-004",
            "Performance tracking overview",
            "category:metrics");
    }
}


