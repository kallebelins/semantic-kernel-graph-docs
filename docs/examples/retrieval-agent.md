# Retrieval Agent Example

This example demonstrates a question-answering agent that retrieves relevant context from a knowledge base and synthesizes an answer using retrieval-augmented generation (RAG).

## Objective

Learn how to implement retrieval-augmented generation workflows in Semantic Kernel Graph to:
- Create a linear retrieval QA pipeline with three steps
- Implement question analysis and search query generation
- Retrieve relevant context from a knowledge base
- Synthesize comprehensive answers using retrieved context
- Demonstrate RAG-style question answering capabilities

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- **Kernel Memory** configured for knowledge base operations
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [RAG Patterns](../patterns/rag.md)
- Familiarity with [Memory and Retrieval](../concepts/memory.md)

## Key Components

### Concepts and Techniques

- **Retrieval-Augmented Generation (RAG)**: Combining retrieval with generation for accurate answers
- **Question Analysis**: Understanding and reformulating user questions for better retrieval
- **Context Retrieval**: Finding relevant information from knowledge bases
- **Answer Synthesis**: Generating comprehensive answers using retrieved context
- **Knowledge Base Management**: Indexing and searching structured information

### Core Classes

- `GraphExecutor`: Executor for retrieval agent workflows
- `FunctionGraphNode`: Nodes for question analysis, retrieval, and answer synthesis
- `KernelMemoryGraphProvider`: Provider for knowledge base operations
- `ConditionalEdge`: Graph edges for workflow control
- `GraphState`: State management for retrieval results and context

## Running the Example

### Getting Started

This example demonstrates retrieval-augmented generation (RAG) patterns with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Knowledge Base Setup

The example starts by setting up a knowledge base with sample content.

```csharp
var memoryProvider = new KernelMemoryGraphProvider();
var collection = "kb_general";

await SeedKnowledgeBaseAsync(memoryProvider, collection);

private static async Task SeedKnowledgeBaseAsync(KernelMemoryGraphProvider provider, string collection)
{
    await provider.SaveInformationAsync(collection,
        "The Semantic Kernel Graph is a powerful extension to build complex workflows with graphs, enabling conditional routing, memory integration, and performance metrics.",
        "kb-001",
        "Project overview",
        "category:overview");

    await provider.SaveInformationAsync(collection,
        "Data privacy is handled through encryption at rest and in transit, with role-based access controls and audit logging for compliance with GDPR and other regulations.",
        "kb-002",
        "Data privacy",
        "category:security");

    await provider.SaveInformationAsync(collection,
        "The quarterly business report shows 25% improvement in system performance, 40% reduction in response times, and 15% increase in user satisfaction scores.",
        "kb-003",
        "Business report",
        "category:performance");

    Console.WriteLine("✅ Knowledge base seeded with sample content");
}
```

### 2. Creating the Retrieval Agent

The agent is built with a linear three-step pipeline: analyze, retrieve, and answer.

```csharp
private static GraphExecutor CreateRetrievalAgent(Kernel kernel, KernelMemoryGraphProvider memoryProvider, string collection)
{
    var executor = new GraphExecutor("RetrievalAgent", "Retrieval-augmented Q&A agent");

    var analyze = new FunctionGraphNode(
        CreateAnalyzeQuestionFunction(kernel),
        "analyze_question",
        "Analyze the user question and produce a focused search query"
    ).StoreResultAs("search_query");

    var retrieve = new FunctionGraphNode(
        CreateRetrieveContextFunction(kernel, memoryProvider, collection),
        "retrieve_context",
        "Retrieve relevant context from the knowledge base"
    ).StoreResultAs("retrieved_context");

    var answer = new FunctionGraphNode(
        CreateSynthesizeAnswerFunction(kernel),
        "synthesize_answer",
        "Synthesize a final answer using the retrieved context"
    ).StoreResultAs("final_answer");

    executor.AddNode(analyze);
    executor.AddNode(retrieve);
    executor.AddNode(answer);

    executor.SetStartNode(analyze.NodeId);
    executor.AddEdge(ConditionalEdge.CreateUnconditional(analyze, retrieve));
    executor.AddEdge(ConditionalEdge.CreateUnconditional(retrieve, answer));

    return executor;
}
```

### 3. Question Analysis Function

The analysis function understands user questions and generates focused search queries.

```csharp
private static KernelFunction CreateAnalyzeQuestionFunction(Kernel kernel)
{
    return KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var question = args.TryGetValue("user_question", out var q) ? q?.ToString() ?? string.Empty : string.Empty;
            
            // Analyze the question and extract key concepts for search
            var searchQuery = question.ToLowerInvariant()
                .Replace("what", "")
                .Replace("how", "")
                .Replace("benefits", "benefits advantages features")
                .Replace("handled", "handled managed implemented")
                .Replace("improvements", "improvements enhancements progress")
                .Trim();

            // Add context-specific terms for better retrieval
            if (question.Contains("Semantic Kernel Graph"))
                searchQuery += " semantic kernel graph workflow";
            
            if (question.Contains("data privacy") || question.Contains("encryption"))
                searchQuery += " data privacy encryption security compliance";
            
            if (question.Contains("business report") || question.Contains("quarterly"))
                searchQuery += " business report quarterly performance metrics";

            args["search_query"] = searchQuery;
            return $"Search query generated: {searchQuery}";
        },
        functionName: "analyze_question",
        description: "Analyzes user questions and generates focused search queries"
    );
}
```

### 4. Context Retrieval Function

The retrieval function searches the knowledge base for relevant information.

```csharp
private static KernelFunction CreateRetrieveContextFunction(Kernel kernel, KernelMemoryGraphProvider memoryProvider, string collection)
{
    return KernelFunctionFactory.CreateFromMethod(
        async (KernelArguments args) =>
        {
            var query = args.TryGetValue("search_query", out var q) ? q?.ToString() ?? string.Empty : string.Empty;
            var topK = args.TryGetValue("top_k", out var tk) && tk is int k ? k : 5;
            var minScore = args.TryGetValue("min_score", out var ms) && ms is double s ? s : 0.35;

            // Retrieve relevant context from the knowledge base
            var results = await memoryProvider.SearchAsync(collection, query, topK, minScore);
            
            if (!results.Any())
            {
                args["retrieved_context"] = "No relevant context found for the query.";
                return "No relevant context retrieved";
            }

            // Format retrieved context for answer synthesis
            var context = string.Join("\n\n", results.Select(r => 
                $"Source: {r.Metadata?.Source ?? "Unknown"}\nContent: {r.Text}"));
            
            args["retrieved_context"] = context;
            args["retrieval_count"] = results.Count;
            args["retrieval_score"] = results.Max(r => r.Score);

            return $"Retrieved {results.Count} relevant context items with max score {results.Max(r => r.Score):F3}";
        },
        functionName: "retrieve_context",
        description: "Retrieves relevant context from the knowledge base"
    );
}
```

### 5. Answer Synthesis Function

The synthesis function combines retrieved context into comprehensive answers.

```csharp
private static KernelFunction CreateSynthesizeAnswerFunction(Kernel kernel)
{
    return KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var question = args.TryGetValue("user_question", out var q) ? q?.ToString() ?? string.Empty : string.Empty;
            var context = args.TryGetValue("retrieved_context", out var c) ? c?.ToString() ?? string.Empty : string.Empty;
            var retrievalCount = args.TryGetValue("retrieval_count", out var rc) && rc is int count ? count : 0;
            var retrievalScore = args.TryGetValue("retrieval_score", out var rs) && rs is double score ? score : 0.0;

            if (string.IsNullOrEmpty(context) || context.Contains("No relevant context found"))
            {
                return "I don't have enough information to answer that question accurately. Please try rephrasing or ask about a different topic.";
            }

            // Synthesize answer based on retrieved context
            var answer = $"Based on the available information:\n\n{context}\n\n" +
                        $"This answer was synthesized from {retrievalCount} relevant sources " +
                        $"(confidence: {retrievalScore:F2}).";

            args["final_answer"] = answer;
            return answer;
        },
        functionName: "synthesize_answer",
        description: "Synthesizes final answers using retrieved context"
    );
}
```

### 6. Question Processing

The example processes multiple questions to demonstrate the retrieval capabilities.

```csharp
var questions = new[]
{
    "What benefits does the Semantic Kernel Graph provide?",
    "How are data privacy and encryption handled?",
    "What improvements were reported in the quarterly business report?"
};

foreach (var q in questions)
{
    Console.WriteLine($"🧑‍💻 User: {q}");
    var args = new KernelArguments
    {
        ["user_question"] = q,
        ["top_k"] = 5,
        ["min_score"] = 0.35
    };

    var result = await executor.ExecuteAsync(kernel, args);
    var answer = result.GetValue<string>() ?? "No answer produced";
    Console.WriteLine($"🤖 Agent: {answer}\n");
    await Task.Delay(200);
}
```

### 7. Enhanced Knowledge Base Content

The example includes more comprehensive knowledge base content for better retrieval.

```csharp
private static async Task SeedKnowledgeBaseAsync(KernelMemoryGraphProvider provider, string collection)
{
    var documents = new[]
    {
        new
        {
            Content = "The Semantic Kernel Graph is a powerful extension to build complex workflows with graphs, enabling conditional routing, memory integration, and performance metrics. It provides benefits such as improved workflow management, better error handling, and enhanced observability.",
            Id = "kb-001",
            Source = "Project overview",
            Tags = "category:overview,benefits,workflow"
        },
        new
        {
            Content = "Data privacy is handled through encryption at rest and in transit, with role-based access controls and audit logging for compliance with GDPR and other regulations. The system implements end-to-end encryption and provides granular permission management.",
            Id = "kb-002",
            Source = "Data privacy",
            Tags = "category:security,encryption,compliance"
        },
        new
        {
            Content = "The quarterly business report shows 25% improvement in system performance, 40% reduction in response times, and 15% increase in user satisfaction scores. These improvements were achieved through optimization efforts and infrastructure upgrades.",
            Id = "kb-003",
            Source = "Business report",
            Tags = "category:performance,metrics,improvements"
        },
        new
        {
            Content = "Semantic Kernel Graph features include advanced routing capabilities, checkpointing for long-running workflows, streaming execution support, and comprehensive monitoring and debugging tools.",
            Id = "kb-004",
            Source = "Feature overview",
            Tags = "category:features,routing,checkpointing"
        },
        new
        {
            Content = "Security measures include multi-factor authentication, encrypted communication channels, regular security audits, and compliance with industry standards like SOC 2 and ISO 27001.",
            Id = "kb-005",
            Source = "Security overview",
            Tags = "category:security,authentication,compliance"
        }
    };

    foreach (var doc in documents)
    {
        await provider.SaveInformationAsync(collection, doc.Content, doc.Id, doc.Source, doc.Tags);
    }

    Console.WriteLine($"✅ Knowledge base seeded with {documents.Length} documents");
}
```

### 8. Advanced Retrieval Options

The example supports configurable retrieval parameters for different use cases.

```csharp
// Configure retrieval parameters based on question complexity
var retrievalParams = question.ToLowerInvariant() switch
{
    var q when q.Contains("benefits") || q.Contains("features") => 
        new { TopK = 3, MinScore = 0.4 }, // Focused retrieval for specific topics
    var q when q.Contains("how") || q.Contains("process") => 
        new { TopK = 5, MinScore = 0.35 }, // Broader retrieval for process questions
    var q when q.Contains("what") && q.Contains("improvements") => 
        new { TopK = 4, MinScore = 0.3 }, // Comprehensive retrieval for improvement questions
    _ => new { TopK = 5, MinScore = 0.35 } // Default parameters
};

var args = new KernelArguments
{
    ["user_question"] = question,
    ["top_k"] = retrievalParams.TopK,
    ["min_score"] = retrievalParams.MinScore
};
```

## Expected Output

The example produces comprehensive output showing:

- 🧑‍💻 User questions and search query analysis
- 🔍 Context retrieval from knowledge base
- 📊 Retrieval scores and result counts
- 🤖 Synthesized answers using retrieved context
- ✅ Complete RAG workflow execution
- 📚 Knowledge base content and retrieval quality

## Troubleshooting

### Common Issues

1. **Knowledge Base Connection Failures**: Ensure Kernel Memory is properly configured
2. **Retrieval Quality Issues**: Adjust top_k and min_score parameters for better results
3. **Context Insufficiency**: Verify knowledge base content and search query generation
4. **Answer Synthesis Failures**: Check context formatting and synthesis logic

### Debugging Tips

- Monitor search query generation and refinement
- Verify knowledge base content indexing and search functionality
- Check retrieval parameters and scoring thresholds
- Monitor answer synthesis quality and context utilization

## See Also

- [RAG Patterns](../patterns/rag.md)
- [Memory and Retrieval](../concepts/memory.md)
- [Question Answering](../concepts/qa-systems.md)
- [Knowledge Base Management](../how-to/knowledge-base.md)
- [Context Retrieval](../concepts/retrieval.md)
