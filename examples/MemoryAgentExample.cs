// Memory agent example for documentation - runnable and validated against the examples project
// Comments are in English and intended for readers of all levels.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;

namespace Examples;

/// <summary>
/// Minimal, self-contained memory agent example used by the documentation.
/// This example mirrors documented snippets but provides simple in-memory implementations
/// so it is runnable inside the examples project without external dependencies.
/// </summary>
public static class MemoryAgentExample
{
    /// <summary>
    /// Runs a basic memory agent workflow that demonstrates storing and retrieving simple memories.
    /// </summary>
    public static async Task RunAsync()
    {
        // Create a local kernel (no external AI calls required for this basic demo)
        var kernel = Kernel.CreateBuilder().Build();

        // Create a simple in-memory store to simulate memory operations
        var memoryStore = new InMemoryMemoryStore();

        // Create graph executor using the kernel so the graph can access kernel services if needed
        var workflow = new GraphExecutor(kernel);

        // Memory agent node: create a KernelFunction and wrap it in a FunctionGraphNode
        var memoryFn = kernel.CreateFunctionFromMethod(
            async (KernelArguments args) =>
            {
                // Use KernelArguments and GraphState to read/write values used by GraphExecutor
                var userInput = args.GetValueOrDefault("user_input")?.ToString() ?? "Hello";
                var sessionId = args.GetValueOrDefault("session_id")?.ToString() ?? Guid.NewGuid().ToString();

                // Retrieve relevant memories (simple substring match)
                var relevant = await memoryStore.RetrieveAsync(userInput, sessionId);

                // Simple processing: echo input and mention retrieved memories count
                var response = $"Echo: {userInput} (found {relevant.Count} memories)";

                // Store new memory entry
                var entry = new MemoryEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    Content = userInput,
                    Response = response,
                    SessionId = sessionId,
                    Timestamp = DateTime.UtcNow,
                    Tags = new List<string> { "doc-example" },
                    Importance = 0.5
                };

                await memoryStore.StoreAsync(entry);

                // Persist results in the graph state so callers can inspect them
                var state = args.GetOrCreateGraphState();
                state.SetValue("agent_response", response);
                state.SetValue("memories_retrieved", relevant.Count);
                state.SetValue("new_memory_stored", true);
                state.SetValue("memory_entry_id", entry.Id);

                // Return a compact delimited payload so callers running this example can read both
                // the main response and simple diagnostics without relying on advanced SDK APIs.
                // Format: response<DELIM>memoriesCount<DELIM>newMemoryStored
                const char DELIM = '\u0001';
                return $"{response}{DELIM}{relevant.Count}{DELIM}true";
            },
            functionName: "doc_memory_agent_fn",
            description: "Documentation memory agent function"
        );

        var memoryAgent = new FunctionGraphNode(memoryFn, "memory-agent");

        workflow.AddNode(memoryAgent);
        workflow.SetStartNode(memoryAgent.NodeId);

        // Test the workflow with a few inputs
        Console.WriteLine("[doc] 🧠 Testing memory agent...\n");

        var inputs = new[] { "What is machine learning?", "Tell me about neural networks", "How does deep learning work?" };

        foreach (var input in inputs)
        {
            var args = new KernelArguments
            {
                ["user_input"] = input,
                ["session_id"] = "doc-session-001"
            };

            Console.WriteLine($"[doc] Input: {input}");
            var result = await workflow.ExecuteAsync(kernel, args);
            // The function returns a delimited string with response and diagnostics
            var raw = result.GetValue<string>() ?? string.Empty;
            const char DELIM = '\u0001';
            var parts = raw.Split(DELIM);
            var agentResponse = parts.Length > 0 ? parts[0] : string.Empty;
            var memoriesRetrieved = parts.Length > 1 && int.TryParse(parts[1], out var m) ? m : 0;
            var newMemoryStored = parts.Length > 2 && bool.TryParse(parts[2], out var b) ? b : false;

            Console.WriteLine($"[doc] Response: {agentResponse}");
            Console.WriteLine($"[doc] Memories Retrieved: {memoriesRetrieved}");
            Console.WriteLine($"[doc] New Memory Stored: {newMemoryStored}\n");
        }
    }

    // ---------- Simple in-memory helpers for the example ----------

    /// <summary>
    /// Minimal representation of a memory entry for documentation examples.
    /// </summary>
    private class MemoryEntry
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public double Importance { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Lightweight in-memory memory store used only for documentation examples.
    /// </summary>
    private class InMemoryMemoryStore
    {
        private readonly List<MemoryEntry> _store = new();

        /// <summary>
        /// Stores a memory entry in memory.
        /// </summary>
        public Task StoreAsync(MemoryEntry entry)
        {
            _store.Add(entry);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Retrieves memories using simple token-overlap matching.
        /// This is still a demo heuristic but captures more realistic retrieval than raw substring checks.
        /// </summary>
        public Task<List<MemoryEntry>> RetrieveAsync(string query, string sessionId)
        {
            var results = new List<MemoryEntry>();
            if (string.IsNullOrWhiteSpace(query)) return Task.FromResult(results);

            // Normalize and extract tokens from the query, ignoring short tokens
            var queryTokens = query
                .ToLowerInvariant()
                .Split(new[] { ' ', '\t', '\n', '\r', '.', ',', '?', '!', ';', ':' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length > 3)
                .ToArray();

            foreach (var e in _store)
            {
                if (e.SessionId != sessionId) continue;

                var content = e.Content ?? string.Empty;
                var contentTokens = content
                    .ToLowerInvariant()
                    .Split(new[] { ' ', '\t', '\n', '\r', '.', ',', '?', '!', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);

                // If any query token appears in the content tokens, consider it relevant
                if (queryTokens.Length > 0 && contentTokens.Intersect(queryTokens).Any())
                {
                    results.Add(e);
                    continue;
                }

                // Fallback: short-query substring match
                if (content.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    results.Add(e);
                }
            }

            return Task.FromResult(results);
        }
    }
}


