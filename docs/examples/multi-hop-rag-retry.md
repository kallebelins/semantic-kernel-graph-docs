# Multi-Hop RAG Retry Example

This example demonstrates multi-hop Retrieval-Augmented Generation (RAG) with retry mechanisms and query refinement over a knowledge base.

## Objective

Learn how to implement advanced RAG workflows in graph-based systems to:
* Implement iterative retrieval loops with multiple attempts
* Refine search queries based on context evaluation
* Dynamically adjust search parameters (top_k, min_score) for better results
* Synthesize comprehensive answers from accumulated context
* Handle complex queries that require multiple retrieval hops

## Prerequisites

* **.NET 8.0** or later
* **OpenAI API Key** configured in `appsettings.json`
* **Semantic Kernel Graph package** installed
* **Kernel Memory** configured for knowledge base operations
* Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [RAG Patterns](../patterns/rag.md)
* Familiarity with [Retrieval and Memory](../concepts/memory.md)

## Key Components

### Concepts and Techniques

* **Multi-Hop RAG**: Iterative retrieval process that refines queries and accumulates context
* **Query Refinement**: Dynamic adjustment of search parameters based on context evaluation
* **Retry Mechanisms**: Multiple retrieval attempts with widening search parameters
* **Context Evaluation**: Assessment of retrieved content quality and sufficiency
* **Answer Synthesis**: Combining multiple retrieval results into comprehensive answers

### Core Classes

* `GraphExecutor`: Executor for multi-hop RAG workflows
* `FunctionGraphNode`: Nodes for query analysis, retrieval, evaluation, and synthesis
* `KernelMemoryGraphProvider`: Provider for knowledge base operations
* `ConditionalEdge`: Edges that control retry loops and query refinement
* `GraphState`: State management for accumulated context and search parameters

## Running the Example

### Getting Started

This example demonstrates multi-hop RAG with retry mechanisms using the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Creating the Multi-Hop RAG Executor

The example creates a specialized executor for multi-hop RAG workflows.

```csharp
private static GraphExecutor CreateMultiHopRagExecutor(Kernel kernel, KernelMemoryGraphProvider provider, string collection)
{
    var executor = new GraphExecutor("MultiHopRagRetry", "Multi-hop RAG with retry and refinement");

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

    // Conditional routing: continue refining vs finalize
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
```

### 2. Configuring the Workflow

The workflow is configured with conditional edges to control the retry loop. The example uses
`ConditionalEdge` instances (instead of inline predicates) so routing logic is explicit and testable.

```csharp
// Note: the executor wiring is performed inside CreateMultiHopRagExecutor in the example
// Set the start node and connect nodes; conditional edges decide whether to retry or finalize.
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
```

### 3. Query Analysis Function

The initial query analysis function prepares a compact, normalized search query and initializes
loop control arguments when missing.

```csharp
private static KernelFunction CreateInitialQueryFunction(Kernel kernel)
{
    return kernel.CreateFunctionFromMethod(
        (KernelArguments args) =>
        {
            var question = args.GetValueOrDefault("user_question")?.ToString() ?? string.Empty;

            // Initialize loop state if missing
            if (!args.ContainsName("attempt")) args["attempt"] = 0;
            if (!args.ContainsName("max_attempts")) args["max_attempts"] = 3;
            if (!args.ContainsName("min_required_chunks")) args["min_required_chunks"] = 2;
            if (!args.ContainsName("top_k")) args["top_k"] = 4;
            if (!args.ContainsName("min_score")) args["min_score"] = 0.45;

            // Produce a compact query removing question words and punctuation
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
```

### 4. Retrieval Function

The retrieval function attempts to fetch relevant context from the knowledge base; it streams
results from the provider and returns concatenated text snippets.

```csharp
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

            // Cap accumulated context length to avoid overly long downstream prompts
            var joined = string.Join(" \n--- \n", snippets);
            return joined.Length > 4000 ? joined[..4000] + "…" : joined;
        },
        functionName: "attempt_retrieval",
        description: "Retrieves relevant context from the knowledge base for the current query"
    );
}
```

### 5. Context Evaluation Function

The evaluation function inspects retrieved counts and thresholds and returns a human-friendly
status message used by the conditional edges.

```csharp
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
```

### 6. Query Refinement Function

The refinement function increments the attempt counter, relaxes thresholds and applies simple
heuristics to broaden the query for subsequent retrieval attempts.

```csharp
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

            // Widen search window progressively
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
```

### 7. Answer Synthesis Function

The synthesis function formats a final answer from whatever context was collected across
attempts. If no context was retrieved, it returns an informative message.

```csharp
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
```

### 8. Knowledge Base Seeding

The example seeds a small knowledge base with several documents to encourage different
retrieval behaviors (policy vs report vs customer content).

```csharp
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
```

### 9. Execution Scenarios

The example runs multiple scenarios to demonstrate different retrieval patterns.

```csharp
var scenarios = new[]
{
    // Likely to be answered in 1-2 hops
    "What does the data privacy policy mandate about encryption and retention?",
    // Intentionally vague to trigger refinement and threshold relaxation
    "Tell me about customer docs and secure handling",
    // Another query that may need widened search
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
```

## Expected Output

The example produces comprehensive output showing:

* 🧑‍💻 User questions and search queries
* 🔍 Retrieval attempts with varying parameters
* 📊 Context evaluation and quality assessment
* 🔄 Query refinement and retry mechanisms
* 🤖 Final synthesized answers from accumulated context
* ✅ Multi-hop RAG workflow completion

## Troubleshooting

### Common Issues

1. **Knowledge Base Connection Failures**: Ensure Kernel Memory is properly configured
2. **Retrieval Quality Issues**: Adjust top_k and min_score parameters for better results
3. **Infinite Retry Loops**: Set appropriate max_attempts and evaluation criteria
4. **Context Insufficiency**: Verify knowledge base content and query refinement logic

### Debugging Tips

* Monitor retrieval scores and chunk counts for each attempt
* Check query refinement parameters and their progression
* Verify context evaluation logic and sufficiency criteria
* Monitor the retry loop to prevent infinite iterations

## See Also

* [RAG Patterns](../patterns/rag.md)
* [Memory and Retrieval](../concepts/memory.md)
* [Conditional Nodes](../concepts/node-types.md)
* [Graph Execution](../concepts/execution.md)
* [Query Optimization](../how-to/query-optimization.md)
