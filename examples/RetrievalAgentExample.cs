// Retrieval agent example for documentation - runnable and validated against the examples project
// Comments are in English and intended for readers of all levels.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;

namespace Examples;

/// <summary>
/// Minimal retrieval-augmented generation (RAG) example used by the documentation.
/// This example implements a tiny in-memory knowledge base and a three-step graph:
/// analyze -> retrieve -> synthesize. It is self-contained so it can be run inside
/// the examples project without external dependencies.
/// </summary>
public static class RetrievalAgentExample
{
    /// <summary>
    /// Runs the retrieval agent demo.
    /// </summary>
    public static async Task RunAsync()
    {
        // Create a local kernel (no external AI calls required for this basic demo)
        var kernel = Kernel.CreateBuilder().Build();

        // Create a minimal in-memory memory provider used only for documentation examples
        var memoryProvider = new SimpleMemoryProvider();
        var collection = "kb_general";

        // Seed the in-memory knowledge base
        await SeedKnowledgeBaseAsync(memoryProvider, collection);

        // Create the graph executor and nodes
        var executor = new GraphExecutor(kernel);

        var analyze = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod((KernelArguments args) =>
            {
                var question = args.TryGetValue("user_question", out var q) ? q?.ToString() ?? string.Empty : string.Empty;

                // Simple question analysis: remove filler words and expand some synonyms
                var searchQuery = question.ToLowerInvariant()
                    .Replace("what", string.Empty)
                    .Replace("how", string.Empty)
                    .Replace("benefits", "benefits advantages features")
                    .Replace("handled", "handled managed implemented")
                    .Replace("improvements", "improvements enhancements progress")
                    .Trim();

                if (question.Contains("semantic kernel graph", StringComparison.OrdinalIgnoreCase))
                {
                    searchQuery += " semantic kernel graph workflow";
                }

                args["search_query"] = searchQuery;
                return $"Search query generated: {searchQuery}";
            }, functionName: "analyze_question", description: "Analyze question and generate search query"),
            "analyze_question");

        var retrieve = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod(async (KernelArguments args) =>
            {
                var query = args.TryGetValue("search_query", out var q) ? q?.ToString() ?? string.Empty : string.Empty;
                var topK = args.TryGetValue("top_k", out var tk) && tk is int k ? k : 5;
                var minScore = args.TryGetValue("min_score", out var ms) && ms is double s ? s : 0.0;

                var results = await memoryProvider.SearchAsync(collection, query, topK, minScore);

                if (!results.Any())
                {
                    args["retrieved_context"] = "No relevant context found for the query.";
                    return "No relevant context retrieved";
                }

                var context = string.Join("\n\n", results.Select(r => $"Source: {r.Metadata.GetValueOrDefault("source", "Unknown")}\nContent: {r.Text}"));

                args["retrieved_context"] = context;
                args["retrieval_count"] = results.Count;
                args["retrieval_score"] = results.Max(r => r.Score);

                return $"Retrieved {results.Count} relevant context items with max score {results.Max(r => r.Score):F3}";
            }, functionName: "retrieve_context", description: "Retrieve relevant context"),
            "retrieve_context").StoreResultAs("retrieved_context");

        var synth = new FunctionGraphNode(
            kernel.CreateFunctionFromMethod((KernelArguments args) =>
            {
                var question = args.TryGetValue("user_question", out var q) ? q?.ToString() ?? string.Empty : string.Empty;
                var context = args.TryGetValue("retrieved_context", out var c) ? c?.ToString() ?? string.Empty : string.Empty;
                var retrievalCount = args.TryGetValue("retrieval_count", out var rc) && rc is int count ? count : 0;
                var retrievalScore = args.TryGetValue("retrieval_score", out var rs) && rs is double score ? score : 0.0;

                if (string.IsNullOrEmpty(context) || context.Contains("No relevant context found"))
                {
                    return "I don't have enough information to answer that question accurately. Please try rephrasing.";
                }

                var answer = $"Based on the available information:\n\n{context}\n\nThis answer was synthesized from {retrievalCount} relevant sources (confidence: {retrievalScore:F2}).";

                args["final_answer"] = answer;
                return answer;
            }, functionName: "synthesize_answer", description: "Synthesize final answer"),
            "synthesize_answer");

        // Compose graph
        executor.AddNode(analyze);
        executor.AddNode(retrieve);
        executor.AddNode(synth);

        executor.SetStartNode(analyze.NodeId);
        executor.AddEdge(ConditionalEdge.CreateUnconditional(analyze, retrieve));
        executor.AddEdge(ConditionalEdge.CreateUnconditional(retrieve, synth));

        // Run sample questions
        var questions = new[]
        {
            "What benefits does the Semantic Kernel Graph provide?",
            "How are data privacy and encryption handled?",
            "What improvements were reported in the quarterly business report?"
        };

        Console.WriteLine("[doc] 🔎 Running Retrieval Agent demo...\n");

        foreach (var q in questions)
        {
            Console.WriteLine($"[doc] User: {q}");

            var args = new KernelArguments
            {
                ["user_question"] = q,
                ["top_k"] = 5,
                ["min_score"] = 0.0
            };

            var result = await executor.ExecuteAsync(kernel, args);
            var answer = result.GetValue<string>() ?? "No answer produced";
            Console.WriteLine($"[doc] Agent: {answer}\n");
            await Task.Delay(200);
        }
    }

    /// <summary>
    /// Seeds the in-memory knowledge base with sample documents.
    /// </summary>
    private static async Task SeedKnowledgeBaseAsync(SimpleMemoryProvider provider, string collection)
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
        await Task.CompletedTask;
    }

    // ---------- Minimal in-memory provider and helper types for the example ----------

    private class DocItem
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    private class SearchResult
    {
        public string Text { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
        public double Score { get; set; }
    }

    /// <summary>
    /// Very small in-memory provider used only for documentation examples.
    /// Implements minimal Save/Query surface compatible with documented snippets.
    /// </summary>
    private class SimpleMemoryProvider
    {
        private readonly Dictionary<string, List<DocItem>> _collections = new(StringComparer.Ordinal);

        public Task SaveInformationAsync(string collection, string content, string id, string source, string tags)
        {
            if (!_collections.TryGetValue(collection, out var list))
            {
                list = new List<DocItem>();
                _collections[collection] = list;
            }

            list.Add(new DocItem { Id = id, Text = content, Metadata = new Dictionary<string, string> { ["source"] = source, ["tags"] = tags } });
            return Task.CompletedTask;
        }

        public Task<List<SearchResult>> SearchAsync(string collection, string query, int topK, double minScore)
        {
            var results = new List<SearchResult>();
            if (string.IsNullOrWhiteSpace(query) || !_collections.TryGetValue(collection, out var docs))
            {
                return Task.FromResult(results);
            }

            var qTokens = query.ToLowerInvariant().Split(new[] { ' ', '\t', '\n', '\r', '.', ',', '?', '!', ';', ':' }, StringSplitOptions.RemoveEmptyEntries).Where(t => t.Length > 2).ToArray();
            foreach (var d in docs)
            {
                var tokens = d.Text.ToLowerInvariant().Split(new[] { ' ', '\t', '\n', '\r', '.', ',', '?', '!', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                var common = tokens.Intersect(qTokens).Count();
                var score = qTokens.Length == 0 ? 0.0 : (double)common / qTokens.Length;
                if (score >= minScore && score > 0)
                {
                    results.Add(new SearchResult { Text = d.Text, Metadata = d.Metadata, Score = score });
                }
            }

            // Return top K by score
            var ordered = results.OrderByDescending(r => r.Score).Take(topK).ToList();
            return Task.FromResult(ordered);
        }
    }
}


