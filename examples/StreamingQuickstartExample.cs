using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.Streaming;

namespace SemanticKernel.Graph.Examples;

/// <summary>
/// Streaming Quickstart Example demonstrating real-time graph execution monitoring.
/// This example shows how to use StreamingGraphExecutor to consume real-time events
/// during graph execution, including event filtering, buffering, and completion handling.
/// </summary>
public static class StreamingQuickstartExample
{
    /// <summary>
    /// Main entry point for the streaming quickstart example.
    /// Demonstrates basic streaming setup, event consumption, and real-time monitoring.
    /// </summary>
    /// <param name="kernel">The configured Kernel instance with LLM provider</param>
    /// <returns>Task representing the async operation</returns>
    public static async Task RunAsync(Kernel kernel)
    {
        Console.WriteLine("=== Streaming Quickstart Example ===\n");
        
        // Run all streaming examples
        await BasicStreamingSetupAsync(kernel);
        await EventFilteringAsync(kernel);
        await BufferedConsumptionAsync(kernel);
        await RealTimeMonitoringAsync(kernel);
        await ApiResponseStreamingAsync(kernel);
    }

    /// <summary>
    /// Demonstrates basic streaming executor setup and event consumption.
    /// Shows how to create a streaming executor, add nodes, and consume events.
    /// </summary>
    private static async Task BasicStreamingSetupAsync(Kernel kernel)
    {
        Console.WriteLine("🚀 Basic Streaming Setup\n");

        // Create a streaming-enabled graph executor
        var streamingExecutor = new StreamingGraphExecutor("StreamingDemo", "Demo of streaming execution");

        // Add function nodes with simulated work
        var node1 = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                () => 
                {
                    Thread.Sleep(1000); // Simulate work
                    return "Hello from node 1";
                },
                "node1_function",
                "First node function"
            ),
            "node1",
            "First Node"
        );

        var node2 = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                () => 
                {
                    Thread.Sleep(1500); // Simulate work
                    return "Hello from node 2";
                },
                "node2_function",
                "Second node function"
            ),
            "node2",
            "Second Node"
        );

        var node3 = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                () => 
                {
                    Thread.Sleep(800); // Simulate work
                    return "Hello from node 3";
                },
                "node3_function",
                "Third node function"
            ),
            "node3",
            "Third Node"
        );

        // Add nodes to executor
        streamingExecutor.AddNode(node1);
        streamingExecutor.AddNode(node2);
        streamingExecutor.AddNode(node3);

        // Connect nodes in sequence
        streamingExecutor.Connect("node1", "node2");
        streamingExecutor.Connect("node2", "node3");
        streamingExecutor.SetStartNode("node1");

        // Configure streaming options with correct property names
        var options = new StreamingExecutionOptions
        {
            BufferSize = 20,
            EnableHeartbeat = true,
            HeartbeatInterval = TimeSpan.FromSeconds(5),
            EventTypesToEmit = new[]
            {
                GraphExecutionEventType.ExecutionStarted,
                GraphExecutionEventType.NodeStarted,
                GraphExecutionEventType.NodeCompleted,
                GraphExecutionEventType.ExecutionCompleted
            }
        };

        // Start streaming execution
        var arguments = new KernelArguments();
        var eventStream = streamingExecutor.ExecuteStreamAsync(kernel, arguments, options);

        Console.WriteLine("⚡ Starting streaming execution...\n");

        // Consume events in real-time
        await foreach (var @event in eventStream)
        {
            Console.WriteLine($"📡 Event: {@event.EventType} at {@event.Timestamp:HH:mm:ss.fff}");
            
            // Handle different event types
            switch (@event)
            {
                case GraphExecutionStartedEvent started:
                    Console.WriteLine($"   🚀 Execution started with ID: {started.ExecutionId}");
                    break;
                    
                case NodeExecutionStartedEvent nodeStarted:
                    Console.WriteLine($"   ▶️  Node started: {nodeStarted.Node.Name}");
                    break;
                    
                case NodeExecutionCompletedEvent nodeCompleted:
                    Console.WriteLine($"   ✅ Node completed: {nodeCompleted.Node.Name} in {nodeCompleted.ExecutionDuration.TotalMilliseconds:F0}ms");
                    break;
                    
                case GraphExecutionCompletedEvent completed:
                    Console.WriteLine($"   🎯 Execution completed in {completed.TotalDuration.TotalMilliseconds:F0}ms");
                    break;
            }
            
            // Add small delay to demonstrate real-time nature
            await Task.Delay(100);
        }

        Console.WriteLine("✅ Basic streaming setup completed!\n");
    }

    /// <summary>
    /// Demonstrates event filtering to monitor only specific event types.
    /// Shows how to use the Filter extension method to focus on node events.
    /// </summary>
    private static async Task EventFilteringAsync(Kernel kernel)
    {
        Console.WriteLine("🎯 Event Filtering Example\n");

        // Create simple executor for filtering demo
        var executor = new StreamingGraphExecutor("FilterDemo", "Event filtering demo");

        // Add a few nodes
        var startNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() => "Start", "start_function", "Start function"),
            "start", "Start Node");
        var processNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() => "Process", "process_function", "Process function"),
            "process", "Process Node");
        var endNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() => "End", "end_function", "End function"),
            "end", "End Node");

        executor.AddNode(startNode);
        executor.AddNode(processNode);
        executor.AddNode(endNode);

        executor.Connect("start", "process");
        executor.Connect("process", "end");
        executor.SetStartNode("start");

        var eventStream = executor.ExecuteStreamAsync(kernel, new KernelArguments());

        // Filter only node-related events using the Filter extension method
        var nodeEventsStream = eventStream.Filter(
            GraphExecutionEventType.NodeStarted,
            GraphExecutionEventType.NodeCompleted
        );

        Console.WriteLine("🎯 Node Events Only:");
        await foreach (var @event in nodeEventsStream)
        {
            Console.WriteLine($"   Node Event: {@event.EventType} - {GetNodeName(@event)}");
        }

        Console.WriteLine("✅ Event filtering completed!\n");
    }

    /// <summary>
    /// Demonstrates buffered event consumption for high-throughput scenarios.
    /// Shows how to use the WithBuffering extension method to process events in batches.
    /// </summary>
    private static async Task BufferedConsumptionAsync(Kernel kernel)
    {
        Console.WriteLine("🚀 Buffered Consumption Example\n");

        // Create executor with multiple nodes for buffering demo
        var executor = new StreamingGraphExecutor("BufferDemo", "Buffering demo");

        // Add multiple nodes to generate more events
        for (int i = 1; i <= 5; i++)
        {
            var node = new FunctionGraphNode(
                KernelFunctionFactory.CreateFromMethod(() => $"Node {i} result", $"node{i}_function", $"Node {i} function"),
                $"node{i}", $"Node {i}");
            executor.AddNode(node);
            
            if (i > 1)
            {
                executor.Connect($"node{i-1}", $"node{i}");
            }
        }
        executor.SetStartNode("node1");

        var eventStream = executor.ExecuteStreamAsync(kernel, new KernelArguments());

        // Create buffered stream using WithBuffering extension method
        var bufferedStream = eventStream.WithBuffering(3);

        Console.WriteLine("🚀 Buffered Events (buffer size: 3):");
        var eventCount = 0;
        await foreach (var @event in bufferedStream)
        {
            eventCount++;
            Console.WriteLine($"   Event #{eventCount}: {@event.EventType}");
        }

        Console.WriteLine($"✅ Processed {eventCount} events with buffering!\n");
    }

    /// <summary>
    /// Demonstrates comprehensive real-time monitoring with a complex graph.
    /// Shows advanced streaming features including conditional nodes and state management.
    /// </summary>
    private static async Task RealTimeMonitoringAsync(Kernel kernel)
    {
        Console.WriteLine("📊 Real-Time Monitoring Example\n");

        // Create streaming executor for monitoring
        var streamingExecutor = new StreamingGraphExecutor("RealTimeMonitor", "Real-time execution monitoring");

        // Create nodes with different execution patterns
        var inputNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args["startTime"] = DateTimeOffset.UtcNow;
                    args["input"] = "Sample input data";
                    return "Input processed";
                },
                "ProcessInput",
                "Processes input data"
            ),
            "input_node"
        ).StoreResultAs("inputResult");

        var analysisNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                async (KernelArguments args) =>
                {
                    // Simulate AI analysis with delay
                    await Task.Delay(1000);
                    args["analysis"] = "AI analysis completed";
                    args["analysisTime"] = DateTimeOffset.UtcNow;
                    return "Analysis completed";
                },
                "AnalyzeData",
                "Performs AI analysis"
            ),
            "analysis_node"
        ).StoreResultAs("analysisResult");

        var summaryNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    var startTime = args.ContainsName("startTime") ? (DateTimeOffset)args["startTime"] : DateTimeOffset.UtcNow;
                    var endTime = DateTimeOffset.UtcNow;
                    var duration = endTime - startTime;
                    
                    args["totalDuration"] = duration.TotalMilliseconds;
                    args["finalResult"] = "Processing completed successfully";
                    
                    return $"Processing completed in {duration.TotalMilliseconds:F0}ms";
                },
                "CreateSummary",
                "Creates execution summary"
            ),
            "summary_node"
        ).StoreResultAs("summaryResult");

        // Build the graph
        streamingExecutor.AddNode(inputNode);
        streamingExecutor.AddNode(analysisNode);
        streamingExecutor.AddNode(summaryNode);

        // Connect nodes in sequence
        streamingExecutor.Connect("input_node", "analysis_node");
        streamingExecutor.Connect("analysis_node", "summary_node");

        streamingExecutor.SetStartNode("input_node");

        // Configure comprehensive streaming options
        var options = new StreamingExecutionOptions
        {
            BufferSize = 15,
            EnableHeartbeat = true,
            HeartbeatInterval = TimeSpan.FromSeconds(10),
            EventTypesToEmit = new[]
            {
                GraphExecutionEventType.ExecutionStarted,
                GraphExecutionEventType.NodeStarted,
                GraphExecutionEventType.NodeCompleted,
                GraphExecutionEventType.ExecutionCompleted,
                GraphExecutionEventType.ExecutionFailed
            },
            IncludeStateSnapshots = true
        };

        // Execute with streaming
        var arguments = new KernelArguments();
        var eventStream = streamingExecutor.ExecuteStreamAsync(kernel, arguments, options);

        Console.WriteLine("=== Real-Time Execution Monitoring ===\n");
        Console.WriteLine("⚡ Starting streaming execution...\n");

        // Monitor execution in real-time
        var eventCount = 0;
        await foreach (var @event in eventStream)
        {
            eventCount++;
            var timestamp = @event.Timestamp.ToString("HH:mm:ss.fff");
            
            Console.WriteLine($"[{timestamp}] #{eventCount} {@event.EventType}");
            
            switch (@event)
            {
                case GraphExecutionStartedEvent started:
                    Console.WriteLine($"   🚀 Execution ID: {started.ExecutionId}");
                    break;
                    
                case NodeExecutionStartedEvent nodeStarted:
                    Console.WriteLine($"   ▶️  Node: {nodeStarted.Node.Name}");
                    break;
                    
                case NodeExecutionCompletedEvent nodeCompleted:
                    var duration = nodeCompleted.ExecutionDuration.TotalMilliseconds;
                    Console.WriteLine($"   ✅ Node: {nodeCompleted.Node.Name} ({duration:F0}ms)");
                    break;
                    
                case GraphExecutionCompletedEvent completed:
                    var totalDuration = completed.TotalDuration.TotalMilliseconds;
                    Console.WriteLine($"   🎯 Total Duration: {totalDuration:F0}ms");
                    break;
            }
            
            // Small delay for readability
            await Task.Delay(100);
        }

        Console.WriteLine($"\n=== Execution Summary ===");
        Console.WriteLine($"Total Events: {eventCount}");
        Console.WriteLine("✅ Real-time monitoring completed!\n");
    }

    /// <summary>
    /// Demonstrates API response streaming with heartbeat functionality.
    /// Shows how to convert streaming events to API-friendly responses with heartbeat monitoring.
    /// </summary>
    private static async Task ApiResponseStreamingAsync(Kernel kernel)
    {
        Console.WriteLine("📡 API Response Streaming Example\n");

        // Create simple executor for API demo
        var executor = new StreamingGraphExecutor("ApiDemo", "Web API streaming demo");

        // Add nodes with different behaviors
        var startNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() => "Starting execution", "start_function", "Start node function"),
            "start", "Start Node");
        var processNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() => "Processing data", "process_function", "Processing node function"),
            "process", "Process Node");
        var endNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() => "Execution complete", "end_function", "End node function"),
            "end", "End Node");

        executor.AddNode(startNode);
        executor.AddNode(processNode);
        executor.AddNode(endNode);

        executor.Connect("start", "process");
        executor.Connect("process", "end");
        executor.SetStartNode("start");

        var eventStream = executor.ExecuteStreamAsync(kernel, new KernelArguments());

        Console.WriteLine("📡 API Response Stream:");
        Console.WriteLine("(Formatted as JSON responses for web clients)\n");

        // Convert to API responses with heartbeat using extension methods
        var apiResponses = eventStream.ToApiResponses()
            .WithHeartbeat(TimeSpan.FromSeconds(2));

        await foreach (var response in apiResponses)
        {
            if (response.IsHeartbeat)
            {
                Console.WriteLine($"💗 Heartbeat #{response.SequenceNumber}");
            }
            else
            {
                Console.WriteLine($"📨 Event #{response.SequenceNumber}:");
                Console.WriteLine($"   Type: {response.EventType}");
                Console.WriteLine($"   Time: {response.Timestamp:HH:mm:ss.fff}");
                Console.WriteLine($"   Payload: {System.Text.Json.JsonSerializer.Serialize(response.Payload)}");
            }

            await Task.Delay(100); // Simulate network delay
        }

        Console.WriteLine("✅ API response streaming completed!\n");
    }

    /// <summary>
    /// Helper method to extract node name from various event types.
    /// Provides a consistent way to get node information from different event types.
    /// </summary>
    /// <param name="event">The graph execution event</param>
    /// <returns>Node name if available, otherwise "Unknown"</returns>
    private static string GetNodeName(GraphExecutionEvent @event)
    {
        return @event switch
        {
            NodeExecutionStartedEvent nodeStarted => nodeStarted.Node.Name,
            NodeExecutionCompletedEvent nodeCompleted => nodeCompleted.Node.Name,
            NodeExecutionFailedEvent nodeFailed => nodeFailed.Node.Name,
            _ => "Unknown"
        };
    }
}
