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
```

### 2. Configuring the Workflow

The workflow is configured with conditional edges to control the retry loop.

```csharp
// Set the start node
executor.SetStartNode(analyze.NodeId);

// Connect the main flow
executor.Connect(analyze.NodeId, retrieve.NodeId);
executor.Connect(retrieve.NodeId, evaluate.NodeId);

// Conditional edge: retry if context is insufficient
executor.ConnectWhen(evaluate.NodeId, refine.NodeId, state =>
{
    var evaluation = state.TryGetValue("evaluation_message", out var eval) ? eval?.ToString() ?? string.Empty : string.Empty;
    return evaluation.Contains("insufficient") || evaluation.Contains("retry");
});

// Conditional edge: proceed to answer if context is sufficient
executor.ConnectWhen(evaluate.NodeId, answer.NodeId, state =>
{
    var evaluation = state.TryGetValue("evaluation_message", out var eval) ? eval?.ToString() ?? string.Empty : string.Empty;
    return evaluation.Contains("sufficient") || evaluation.Contains("proceed");
});

// Connect refinement back to retrieval
executor.Connect(refine.NodeId, retrieve.NodeId);

return executor;
```

### 3. Query Analysis Function

The initial query analysis function prepares the search query.

```csharp
private static KernelFunction CreateInitialQueryFunction(Kernel kernel)
{
    return KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var question = args.TryGetValue("user_question", out var q) ? q?.ToString() ?? string.Empty : string.Empty;
            
            // Analyze the question and create an optimized search query
            var searchQuery = question.ToLowerInvariant()
                .Replace("what does", "")
                .Replace("tell me about", "")
                .Replace("summarize", "")
                .Trim();

            args["search_query"] = searchQuery;
            return $"Search query prepared: {searchQuery}";
        },
        functionName: "analyze_question",
        description: "Analyzes user question and prepares search query"
    );
}
```

### 4. Retrieval Function

The retrieval function attempts to fetch relevant context from the knowledge base.

```csharp
private static KernelFunction CreateAttemptRetrievalFunction(Kernel kernel, KernelMemoryGraphProvider provider, string collection)
{
    return KernelFunctionFactory.CreateFromMethod(
        async (KernelArguments args) =>
        {
            var query = args.TryGetValue("search_query", out var q) ? q?.ToString() ?? string.Empty : string.Empty;
            var topK = args.TryGetValue("top_k", out var tk) && tk is int k ? k : 4;
            var minScore = args.TryGetValue("min_score", out var ms) && ms is double s ? s : 0.45;

            // Attempt retrieval with current parameters
            var results = await provider.SearchAsync(collection, query, topK, minScore);
            
            var context = string.Join("\n\n", results.Select(r => r.Text));
            args["retrieved_context"] = context;
            args["retrieval_count"] = results.Count;
            args["retrieval_score"] = results.Any() ? results.Max(r => r.Score) : 0.0;

            return $"Retrieved {results.Count} chunks with max score {results.Max(r => r.Score):F3}";
        },
        functionName: "attempt_retrieval",
        description: "Attempts to retrieve relevant context from knowledge base"
    );
}
```

### 5. Context Evaluation Function

The evaluation function determines if the retrieved context is sufficient.

```csharp
private static KernelFunction CreateEvaluateContextFunction(Kernel kernel)
{
    return KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var context = args.TryGetValue("retrieved_context", out var c) ? c?.ToString() ?? string.Empty : string.Empty;
            var count = args.TryGetValue("retrieval_count", out var cnt) && cnt is int n ? n : 0;
            var score = args.TryGetValue("retrieval_score", out var s) && s is double sc ? sc : 0.0;
            var minRequired = args.TryGetValue("min_required_chunks", out var mrc) && mrc is int min ? min : 2;

            var evaluation = new
            {
                ChunkCount = count,
                MaxScore = score,
                MinRequired = minRequired,
                IsSufficient = count >= minRequired && score >= 0.6,
                Quality = score >= 0.8 ? "high" : score >= 0.6 ? "medium" : "low"
            };

            args["evaluation"] = evaluation;

            if (evaluation.IsSufficient)
            {
                return "Context is sufficient, proceeding to answer synthesis";
            }
            else
            {
                return $"Context insufficient: {count}/{minRequired} chunks, score {score:F3}. Need to retry with refined parameters.";
            }
        },
        functionName: "evaluate_context",
        description: "Evaluates if retrieved context is sufficient for answer synthesis"
    );
}
```

### 6. Query Refinement Function

The refinement function adjusts search parameters for better results.

```csharp
private static KernelFunction CreateRefineQueryFunction(Kernel kernel)
{
    return KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var currentQuery = args.TryGetValue("search_query", out var q) ? q?.ToString() ?? string.Empty : string.Empty;
            var attemptCount = args.TryGetValue("attempt_count", out var ac) && ac is int count ? count : 0;
            var topK = args.TryGetValue("top_k", out var tk) && tk is int k ? k : 4;
            var minScore = args.TryGetValue("min_score", out var ms) && ms is double s ? s : 0.45;

            // Refine parameters based on attempt count
            var refinedTopK = Math.Min(topK + 2, 10); // Increase top_k, max 10
            var refinedMinScore = Math.Max(minScore - 0.1, 0.3); // Decrease min_score, min 0.3

            // Refine query if needed
            var refinedQuery = currentQuery;
            if (attemptCount > 1)
            {
                // Add broader terms for subsequent attempts
                refinedQuery = $"{currentQuery} overview general information";
            }

            args["search_query"] = refinedQuery;
            args["top_k"] = refinedTopK;
            args["min_score"] = refinedMinScore;
            args["attempt_count"] = attemptCount + 1;

            return $"Refined query: '{refinedQuery}', top_k: {refinedTopK}, min_score: {refinedMinScore:F3}";
        },
        functionName: "refine_query",
        description: "Refines search query and parameters for retry attempts"
    );
}
```

### 7. Answer Synthesis Function

The synthesis function combines accumulated context into a final answer.

```csharp
private static KernelFunction CreateSynthesizeAnswerFunction(Kernel kernel)
{
    return KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var context = args.TryGetValue("retrieved_context", out var c) ? c?.ToString() ?? string.Empty : string.Empty;
            var question = args.TryGetValue("user_question", out var q) ? q?.ToString() ?? string.Empty : string.Empty;
            var evaluation = args.TryGetValue("evaluation", out var eval) ? eval : null;

            // Synthesize answer from accumulated context
            var answer = $"Based on the retrieved information:\n\n{context}\n\n" +
                        $"This answer was synthesized from {evaluation?.GetType().GetProperty("ChunkCount")?.GetValue(evaluation)} " +
                        $"context chunks with quality level: {evaluation?.GetType().GetProperty("Quality")?.GetValue(evaluation)}.";

            args["final_answer"] = answer;
            return answer;
        },
        functionName: "synthesize_answer",
        description: "Synthesizes final answer from accumulated retrieved context"
    );
}
```

### 8. Knowledge Base Seeding

The example seeds a knowledge base with sample documents for testing.

```csharp
private static async Task SeedKnowledgeBaseAsync(KernelMemoryGraphProvider provider, string collection)
{
    var documents = new[]
    {
        new { Title = "Data Privacy Policy", Content = "Our data privacy policy mandates encryption of all customer data and retention for 7 years..." },
        new { Title = "Customer Documentation", Content = "Customer documentation must be handled securely with access controls and audit logging..." },
        new { Title = "Business Reports", Content = "Business reports include performance metrics, revenue analysis, and growth projections..." },
        new { Title = "Performance Tracking", Content = "Performance tracking systems monitor KPIs, SLA compliance, and operational efficiency..." }
    };

    foreach (var doc in documents)
    {
        await provider.StoreAsync(collection, doc.Title, doc.Content);
    }

    Console.WriteLine($"✅ Knowledge base seeded with {documents.Length} documents");
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
