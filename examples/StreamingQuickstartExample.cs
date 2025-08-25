using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace Examples;

/// <summary>
/// Runnable copy of the streaming execution documentation examples.
/// This file contains a minimal, self-contained "streaming" executor used
/// only for documentation/testing purposes so the examples can be compiled
/// and executed inside the examples runner without relying on unreleased
/// library types.
/// </summary>
public static class StreamingQuickstartExample
{
    /// <summary>
    /// Entry point used by the examples runner. It executes each documented
    /// streaming scenario sequentially so maintainers can validate behavior.
    /// </summary>
    public static async Task RunAsync(Kernel? kernel = null)
    {
        Console.WriteLine("[doc] 🎯 Running Streaming Quickstart Example...\n");

        // Use a default kernel if null was passed for local testing convenience
        kernel ??= Kernel.CreateBuilder().Build();

        await RunBasicStreamingExample(kernel);
        await RunEventFilteringExample(kernel);
        await RunBufferedStreamingExample(kernel);
        await RunWebApiStreamingExample(kernel);
        await RunReconnectionExample(kernel);

        Console.WriteLine("[doc] ✅ Streaming Quickstart Example finished.");
    }

    // ----------------- Minimal supporting types for the docs example -----------------

    private enum GraphExecutionEventType { ExecutionStarted, NodeStarted, NodeCompleted, ExecutionCompleted, ErrorOccurred }

    private class GraphExecutionEvent
    {
        public GraphExecutionEventType EventType { get; init; }
        public string NodeId { get; init; } = string.Empty;
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
        public object? Result { get; init; }
        public string? Status { get; init; }
        public string? ErrorMessage { get; init; }
    }

    private delegate Task<string> NodeHandler(Kernel kernel, KernelArguments args);

    private class FunctionGraphNode
    {
        public string NodeId { get; }
        public string Name { get; }
        public NodeHandler Handler { get; }

        public FunctionGraphNode(NodeHandler handler, string nodeId, string? name = null)
        {
            Handler = handler;
            NodeId = nodeId;
            Name = name ?? nodeId;
        }
    }

    private class StreamingOptions
    {
        public int BufferSize { get; set; } = 1;
        public Func<GraphExecutionEvent, bool>? EventFilter { get; set; }
    }

    // Very small streaming executor used only for the docs.
    private class StreamingGraphExecutor
    {
        private readonly List<FunctionGraphNode> _nodes = new();
        private readonly Dictionary<string, List<string>> _edges = new(StringComparer.Ordinal);
        private string _startNode = string.Empty;

        public StreamingGraphExecutor(string id, string description) { /* metadata only for docs */ }

        public void AddNode(FunctionGraphNode node) => _nodes.Add(node);

        public void Connect(string from, string to)
        {
            if (!_edges.TryGetValue(from, out var list))
            {
                list = new List<string>();
                _edges[from] = list;
            }
            list.Add(to);
        }

        public void SetStartNode(string nodeId) => _startNode = nodeId;

        // Execute nodes in a simple linear traversal starting from the start node.
        public async IAsyncEnumerable<GraphExecutionEvent> ExecuteStreamAsync(Kernel kernel, KernelArguments args, StreamingOptions options)
        {
            yield return new GraphExecutionEvent { EventType = GraphExecutionEventType.ExecutionStarted };

            var visited = new HashSet<string>(StringComparer.Ordinal);
            var queue = new Queue<string>();
            if (!string.IsNullOrEmpty(_startNode)) queue.Enqueue(_startNode);

            while (queue.Any())
            {
                var nodeId = queue.Dequeue();
                if (visited.Contains(nodeId)) continue;
                visited.Add(nodeId);

                var node = _nodes.FirstOrDefault(n => string.Equals(n.NodeId, nodeId, StringComparison.Ordinal));
                if (node == null)
                {
                    yield return new GraphExecutionEvent { EventType = GraphExecutionEventType.ErrorOccurred, NodeId = nodeId, ErrorMessage = "Node not found" };
                    continue;
                }

                var startEvt = new GraphExecutionEvent { EventType = GraphExecutionEventType.NodeStarted, NodeId = node.NodeId };
                if (options.EventFilter == null || options.EventFilter(startEvt))
                {
                    yield return startEvt;
                }

                // Execute handler and capture result or error. Avoid yielding inside try/catch.
                string? resultValue = null;
                string? errorMessage = null;
                try
                {
                    resultValue = await node.Handler(kernel, args);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }

                if (errorMessage != null)
                {
                    var errEvt = new GraphExecutionEvent { EventType = GraphExecutionEventType.ErrorOccurred, NodeId = node.NodeId, ErrorMessage = errorMessage };
                    if (options.EventFilter == null || options.EventFilter(errEvt))
                    {
                        yield return errEvt;
                    }
                }
                else
                {
                    var completedEvt = new GraphExecutionEvent { EventType = GraphExecutionEventType.NodeCompleted, NodeId = node.NodeId, Result = resultValue };
                    if (options.EventFilter == null || options.EventFilter(completedEvt))
                    {
                        yield return completedEvt;
                    }
                }

                if (_edges.TryGetValue(node.NodeId, out var next))
                {
                    foreach (var n in next) queue.Enqueue(n);
                }
            }

            yield return new GraphExecutionEvent { EventType = GraphExecutionEventType.ExecutionCompleted };
        }
    }

    // ----------------- Documented scenarios implemented below -----------------

    private static async Task RunBasicStreamingExample(Kernel kernel)
    {
        Console.WriteLine("📡 [doc] Basic Streaming Execution Example");

        var streamingExecutor = new StreamingGraphExecutor("StreamingDemo", "Demo of streaming execution");

        // Create three simple nodes that return static strings.
        var node1 = new FunctionGraphNode(async (k, a) => { await Task.Yield(); return "Hello from node 1"; }, "node1", "First Node");
        var node2 = new FunctionGraphNode(async (k, a) => { await Task.Yield(); return "Hello from node 2"; }, "node2", "Second Node");
        var node3 = new FunctionGraphNode(async (k, a) => { await Task.Yield(); return "Hello from node 3"; }, "node3", "Third Node");

        streamingExecutor.AddNode(node1);
        streamingExecutor.AddNode(node2);
        streamingExecutor.AddNode(node3);

        streamingExecutor.Connect("node1", "node2");
        streamingExecutor.Connect("node2", "node3");
        streamingExecutor.SetStartNode("node1");

        var options = new StreamingOptions { BufferSize = 10 };

        var arguments = new KernelArguments();
        await foreach (var evt in streamingExecutor.ExecuteStreamAsync(kernel, arguments, options))
        {
            Console.WriteLine($"📡 Event: {evt.EventType} | Node: {evt.NodeId} | Time: {evt.Timestamp:HH:mm:ss.fff}");
            if (evt.EventType == GraphExecutionEventType.NodeCompleted)
            {
                Console.WriteLine($"  ✅ Node {evt.NodeId} completed with result: {evt.Result}");
            }
        }
    }

    private static async Task RunEventFilteringExample(Kernel kernel)
    {
        Console.WriteLine("\n🔍 [doc] Event Filtering Example");

        var streamingExecutor = new StreamingGraphExecutor("FilteringDemo", "Demo of event filtering");

        var startNode = new FunctionGraphNode((k, a) => Task.FromResult("Starting execution"), "start", "Start Node");
        var processNode = new FunctionGraphNode((k, a) => Task.FromResult("Processing data"), "process", "Process Node");
        var decisionNode = new FunctionGraphNode((k, a) => Task.FromResult("Making decision"), "decision", "Decision Node");

        streamingExecutor.AddNode(startNode);
        streamingExecutor.AddNode(processNode);
        streamingExecutor.AddNode(decisionNode);

        streamingExecutor.Connect("start", "process");
        streamingExecutor.Connect("process", "decision");
        streamingExecutor.SetStartNode("start");

        // Filter out start node completions
        var options = new StreamingOptions { EventFilter = evt => evt.EventType != GraphExecutionEventType.NodeCompleted || evt.NodeId != "start" };

        await foreach (var evt in streamingExecutor.ExecuteStreamAsync(kernel, new KernelArguments(), options))
        {
            Console.WriteLine($"🔍 Filtered Event: {evt.EventType} | Node: {evt.NodeId} | Time: {evt.Timestamp:HH:mm:ss.fff}");
            if (evt.EventType == GraphExecutionEventType.NodeCompleted)
            {
                Console.WriteLine($"  📊 Node {evt.NodeId} completed successfully");
            }
        }
    }

    private static async Task RunBufferedStreamingExample(Kernel kernel)
    {
        Console.WriteLine("\n📦 [doc] Buffered Streaming Example");

        var streamingExecutor = new StreamingGraphExecutor("BufferedDemo", "Demo of buffered streaming");

        var nodes = Enumerable.Range(1, 5).Select(i => new FunctionGraphNode((k, a) => Task.FromResult($"Processed by node {i}"), $"node{i}", $"Node {i}")).ToList();

        foreach (var n in nodes) streamingExecutor.AddNode(n);
        for (int i = 0; i < nodes.Count - 1; i++) streamingExecutor.Connect($"node{i + 1}", $"node{i + 2}");
        streamingExecutor.SetStartNode("node1");

        var options = new StreamingOptions { BufferSize = 3 };

        var eventBuffer = new List<GraphExecutionEvent>();
        await foreach (var evt in streamingExecutor.ExecuteStreamAsync(kernel, new KernelArguments(), options))
        {
            eventBuffer.Add(evt);
            if (eventBuffer.Count >= 3)
            {
                Console.WriteLine($"📦 Processing batch of {eventBuffer.Count} events:");
                foreach (var bufferedEvt in eventBuffer)
                {
                    Console.WriteLine($"  📡 {bufferedEvt.EventType} | Node: {bufferedEvt.NodeId}");
                }
                eventBuffer.Clear();
                Console.WriteLine();
            }
        }

        if (eventBuffer.Any())
        {
            Console.WriteLine($"📦 Processing final batch of {eventBuffer.Count} events:");
            foreach (var evt in eventBuffer) Console.WriteLine($"  📡 {evt.EventType} | Node: {evt.NodeId}");
        }
    }

    private static async Task RunWebApiStreamingExample(Kernel kernel)
    {
        Console.WriteLine("\n🌐 [doc] Web API Streaming Example");

        var streamingExecutor = new StreamingGraphExecutor("WebApiDemo", "Demo of web API streaming");

        var authNode = new FunctionGraphNode((k, a) => Task.FromResult("User authenticated"), "auth", "Authentication Node");
        var dataNode = new FunctionGraphNode((k, a) => Task.FromResult("Data retrieved"), "data", "Data Node");
        var responseNode = new FunctionGraphNode((k, a) => Task.FromResult("Response prepared"), "response", "Response Node");

        streamingExecutor.AddNode(authNode);
        streamingExecutor.AddNode(dataNode);
        streamingExecutor.AddNode(responseNode);

        streamingExecutor.Connect("auth", "data");
        streamingExecutor.Connect("data", "response");
        streamingExecutor.SetStartNode("auth");

        var options = new StreamingOptions { BufferSize = 1 };

        await foreach (var evt in streamingExecutor.ExecuteStreamAsync(kernel, new KernelArguments(), options))
        {
            var sseEvent = $"event: {evt.EventType}\ndata: {{\"nodeId\":\"{evt.NodeId}\",\"timestamp\":\"{evt.Timestamp:O}\",\"status\":\"{evt.Status}\"}}\n\n";
            Console.WriteLine($"🌐 SSE Event: {evt.EventType} | Node: {evt.NodeId}");
            Console.WriteLine($"   Data: {sseEvent.Trim()}");
            if (evt.EventType == GraphExecutionEventType.NodeCompleted)
            {
                Console.WriteLine($"  ✅ Web API step completed: {evt.NodeId}");
            }
        }
    }

    private static async Task RunReconnectionExample(Kernel kernel)
    {
        Console.WriteLine("\n🔌 [doc] Reconnection Example");

        var streamingExecutor = new StreamingGraphExecutor("ReconnectionDemo", "Demo of reconnection handling");

        var longRunningNode = new FunctionGraphNode(async (k, a) =>
        {
            await Task.Delay(200); // shortened for docs test speed
            return "Long operation completed";
        }, "long", "Long Running Node");

        streamingExecutor.AddNode(longRunningNode);
        streamingExecutor.SetStartNode("long");

        var options = new StreamingOptions { BufferSize = 5 };

        try
        {
            await foreach (var evt in streamingExecutor.ExecuteStreamAsync(kernel, new KernelArguments(), options))
            {
                Console.WriteLine($"🔌 Event: {evt.EventType} | Node: {evt.NodeId} | Time: {evt.Timestamp:HH:mm:ss.fff}");
                if (evt.EventType == GraphExecutionEventType.NodeCompleted)
                {
                    Console.WriteLine($"  🎉 Long operation completed successfully");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Stream interrupted: {ex.Message}");
            Console.WriteLine("🔄 Attempting to reconnect...");
            await Task.Delay(100);
            Console.WriteLine("✅ Reconnected successfully");
        }
    }
}
