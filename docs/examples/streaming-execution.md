# Streaming Execution Example

This example demonstrates streaming execution capabilities of the Semantic Kernel Graph system, showing real-time event streaming, buffering, and reconnection features.

## Objective

Learn how to implement streaming execution in graph-based workflows to:
- Enable real-time event streaming during graph execution
- Implement event filtering and buffering strategies
- Support web API streaming for long-running operations
- Handle connection management and reconnection scenarios
- Monitor execution progress in real-time

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Streaming Execution](../concepts/streaming.md)
- Familiarity with [Event Streaming](../concepts/events.md)

## Key Components

### Concepts and Techniques

- **Streaming Execution**: Real-time event streaming during graph execution
- **Event Filtering**: Selective event processing based on type and content
- **Buffered Streaming**: Event buffering for batch processing
- **Web API Streaming**: HTTP streaming for web applications
- **Connection Management**: Handling disconnections and reconnections

### Core Classes

- `StreamingGraphExecutor`: Executor with streaming capabilities
- `GraphExecutionEventStream`: Stream of execution events
- `StreamingExtensions`: Configuration utilities for streaming
- `GraphExecutionEvent`: Individual execution events
- `FunctionGraphNode`: Graph nodes for workflow execution

## Running the Example

### Getting Started

This example demonstrates streaming execution and real-time monitoring with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Basic Streaming Execution

The example starts with basic streaming execution showing real-time events.

```csharp
private static async Task RunBasicStreamingExample(Kernel kernel)
{
    Console.WriteLine("📡 Basic Streaming Execution Example");
    Console.WriteLine("Demonstrates real-time event streaming during graph execution.\n");

    // Create a streaming executor
    var streamingExecutor = new StreamingGraphExecutor("StreamingDemo", "Demo of streaming execution");

    // Add nodes to create a simple graph
    var node1 = new FunctionGraphNode(
        KernelFunctionFactory.CreateFromMethod(() => "Hello from node 1", "node1_function", "First node function"),
        "node1", "First Node");
    var node2 = new FunctionGraphNode(
        KernelFunctionFactory.CreateFromMethod(() => "Hello from node 2", "node2_function", "Second node function"),
        "node2", "Second Node");
    var node3 = new FunctionGraphNode(
        KernelFunctionFactory.CreateFromMethod(() => "Hello from node 3", "node3_function", "Third node function"),
        "node3", "Third Node");

    streamingExecutor.AddNode(node1);
    streamingExecutor.AddNode(node2);
    streamingExecutor.AddNode(node3);

    // Chain the nodes using the executor's Connect method
    streamingExecutor.Connect("node1", "node2");
    streamingExecutor.Connect("node2", "node3");

    streamingExecutor.SetStartNode("node1");

    // Configure streaming options
    var options = StreamingExtensions.CreateStreamingOptions()
        .Configure()
        .WithBufferSize(10)
        .WithEventTypes(
            GraphExecutionEventType.ExecutionStarted,
            GraphExecutionEventType.NodeStarted,
            GraphExecutionEventType.NodeCompleted,
            GraphExecutionEventType.ExecutionCompleted)
        .Build();

    // Start streaming execution
    var arguments = new KernelArguments();
    var eventStream = streamingExecutor.ExecuteStreamAsync(kernel, arguments, options);

    Console.WriteLine("⚡ Starting streaming execution...\n");

    // Process streaming events
    await foreach (var evt in eventStream)
    {
        Console.WriteLine($"📡 Event: {evt.EventType} | Node: {evt.NodeId} | Time: {evt.Timestamp:HH:mm:ss.fff}");
        
        if (evt.EventType == GraphExecutionEventType.NodeCompleted)
        {
            Console.WriteLine($"  ✅ Node {evt.NodeId} completed with result: {evt.Result}");
        }
    }
}
```

### 2. Event Filtering

The example demonstrates filtering events based on type and content.

```csharp
private static async Task RunEventFilteringExample(Kernel kernel)
{
    Console.WriteLine("🔍 Event Filtering Example");
    Console.WriteLine("Demonstrates filtering events by type and content.\n");

    var streamingExecutor = new StreamingGraphExecutor("FilteringDemo", "Demo of event filtering");

    // Create a more complex graph for filtering demonstration
    var startNode = new FunctionGraphNode(
        KernelFunctionFactory.CreateFromMethod(() => "Starting execution", "start_function", "Start function"),
        "start", "Start Node");

    var processNode = new FunctionGraphNode(
        KernelFunctionFactory.CreateFromMethod(() => "Processing data", "process_function", "Process function"),
        "process", "Process Node");

    var decisionNode = new FunctionGraphNode(
        KernelFunctionFactory.CreateFromMethod(() => "Making decision", "decision_function", "Decision function"),
        "decision", "Decision Node");

    streamingExecutor.AddNode(startNode);
    streamingExecutor.AddNode(processNode);
    streamingExecutor.AddNode(decisionNode);

    streamingExecutor.Connect("start", "process");
    streamingExecutor.Connect("process", "decision");

    streamingExecutor.SetStartNode("start");

    // Configure filtering options
    var options = StreamingExtensions.CreateStreamingOptions()
        .Configure()
        .WithBufferSize(5)
        .WithEventTypes(
            GraphExecutionEventType.ExecutionStarted,
            GraphExecutionEventType.NodeStarted,
            GraphExecutionEventType.NodeCompleted)
        .WithEventFilter(evt => 
            evt.EventType == GraphExecutionEventType.NodeCompleted && 
            evt.NodeId != "start") // Filter out start node completions
        .Build();

    var arguments = new KernelArguments();
    var eventStream = streamingExecutor.ExecuteStreamAsync(kernel, arguments, options);

    Console.WriteLine("🔍 Starting filtered streaming execution...\n");

    await foreach (var evt in eventStream)
    {
        Console.WriteLine($"🔍 Filtered Event: {evt.EventType} | Node: {evt.NodeId} | Time: {evt.Timestamp:HH:mm:ss.fff}");
        
        if (evt.EventType == GraphExecutionEventType.NodeCompleted)
        {
            Console.WriteLine($"  📊 Node {evt.NodeId} completed successfully");
        }
    }
}
```

### 3. Buffered Streaming

The example shows buffered streaming for batch event processing.

```csharp
private static async Task RunBufferedStreamingExample(Kernel kernel)
{
    Console.WriteLine("📦 Buffered Streaming Example");
    Console.WriteLine("Demonstrates buffered event processing for batch operations.\n");

    var streamingExecutor = new StreamingGraphExecutor("BufferedDemo", "Demo of buffered streaming");

    // Create multiple nodes for buffering demonstration
    var nodes = Enumerable.Range(1, 5).Select(i => new FunctionGraphNode(
        KernelFunctionFactory.CreateFromMethod(
            () => $"Processed by node {i}", 
            $"node{i}_function", 
            $"Node {i} function"),
        $"node{i}", $"Node {i}"));

    foreach (var node in nodes)
    {
        streamingExecutor.AddNode(node);
    }

    // Chain nodes sequentially
    for (int i = 0; i < nodes.Count - 1; i++)
    {
        streamingExecutor.Connect($"node{i + 1}", $"node{i + 2}");
    }

    streamingExecutor.SetStartNode("node1");

    // Configure buffered streaming
    var options = StreamingExtensions.CreateStreamingOptions()
        .Configure()
        .WithBufferSize(3) // Buffer 3 events before processing
        .WithEventTypes(
            GraphExecutionEventType.NodeStarted,
            GraphExecutionEventType.NodeCompleted)
        .WithBufferingStrategy(BufferingStrategy.Batch) // Process events in batches
        .Build();

    var arguments = new KernelArguments();
    var eventStream = streamingExecutor.ExecuteStreamAsync(kernel, arguments, options);

    Console.WriteLine("📦 Starting buffered streaming execution...\n");

    var eventBuffer = new List<GraphExecutionEvent>();
    
    await foreach (var evt in eventStream)
    {
        eventBuffer.Add(evt);
        
        if (eventBuffer.Count >= 3) // Process buffer when full
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

    // Process remaining events
    if (eventBuffer.Any())
    {
        Console.WriteLine($"📦 Processing final batch of {eventBuffer.Count} events:");
        foreach (var evt in eventBuffer)
        {
            Console.WriteLine($"  📡 {evt.EventType} | Node: {evt.NodeId}");
        }
    }
}
```

### 4. Web API Streaming

The example demonstrates streaming for web API scenarios.

```csharp
private static async Task RunWebApiStreamingExample(Kernel kernel)
{
    Console.WriteLine("🌐 Web API Streaming Example");
    Console.WriteLine("Demonstrates streaming suitable for web API integration.\n");

    var streamingExecutor = new StreamingGraphExecutor("WebApiDemo", "Demo of web API streaming");

    // Create nodes that simulate web API operations
    var authNode = new FunctionGraphNode(
        KernelFunctionFactory.CreateFromMethod(() => "User authenticated", "auth_function", "Authentication function"),
        "auth", "Authentication Node");

    var dataNode = new FunctionGraphNode(
        KernelFunctionFactory.CreateFromMethod(() => "Data retrieved", "data_function", "Data retrieval function"),
        "data", "Data Node");

    var responseNode = new FunctionGraphNode(
        KernelFunctionFactory.CreateFromMethod(() => "Response prepared", "response_function", "Response preparation function"),
        "response", "Response Node");

    streamingExecutor.AddNode(authNode);
    streamingExecutor.AddNode(dataNode);
    streamingExecutor.AddNode(responseNode);

    streamingExecutor.Connect("auth", "data");
    streamingExecutor.Connect("data", "response");

    streamingExecutor.SetStartNode("auth");

    // Configure web API streaming options
    var options = StreamingExtensions.CreateStreamingOptions()
        .Configure()
        .WithBufferSize(1) // Minimal buffering for real-time web updates
        .WithEventTypes(
            GraphExecutionEventType.ExecutionStarted,
            GraphExecutionEventType.NodeStarted,
            GraphExecutionEventType.NodeCompleted,
            GraphExecutionEventType.ExecutionCompleted)
        .WithStreamingFormat(StreamingFormat.ServerSentEvents) // SSE format for web
        .Build();

    var arguments = new KernelArguments();
    var eventStream = streamingExecutor.ExecuteStreamAsync(kernel, arguments, options);

    Console.WriteLine("🌐 Starting web API streaming execution...\n");

    await foreach (var evt in eventStream)
    {
        // Simulate web API response format
        var sseEvent = $"event: {evt.EventType}\ndata: {{\"nodeId\":\"{evt.NodeId}\",\"timestamp\":\"{evt.Timestamp:O}\",\"status\":\"{evt.Status}\"}}\n\n";
        
        Console.WriteLine($"🌐 SSE Event: {evt.EventType} | Node: {evt.NodeId}");
        Console.WriteLine($"   Data: {sseEvent.Trim()}");
        
        if (evt.EventType == GraphExecutionEventType.NodeCompleted)
        {
            Console.WriteLine($"  ✅ Web API step completed: {evt.NodeId}");
        }
    }
}
```

### 5. Reconnection Example

The example demonstrates handling disconnections and reconnections.

```csharp
private static async Task RunReconnectionExample(Kernel kernel)
{
    Console.WriteLine("🔌 Reconnection Example");
    Console.WriteLine("Demonstrates handling disconnections and reconnections.\n");

    var streamingExecutor = new StreamingGraphExecutor("ReconnectionDemo", "Demo of reconnection handling");

    // Create a long-running graph for reconnection testing
    var longRunningNode = new FunctionGraphNode(
        KernelFunctionFactory.CreateFromMethod(async () => 
        {
            await Task.Delay(2000); // Simulate long-running operation
            return "Long operation completed";
        }, "long_function", "Long-running function"),
        "long", "Long Running Node");

    streamingExecutor.AddNode(longRunningNode);
    streamingExecutor.SetStartNode("long");

    // Configure reconnection options
    var options = StreamingExtensions.CreateStreamingOptions()
        .Configure()
        .WithBufferSize(5)
        .WithEventTypes(
            GraphExecutionEventType.ExecutionStarted,
            GraphExecutionEventType.NodeStarted,
            GraphExecutionEventType.NodeCompleted)
        .WithReconnectionOptions(new ReconnectionOptions
        {
            MaxReconnectionAttempts = 3,
            ReconnectionDelay = TimeSpan.FromSeconds(1),
            EnableAutoReconnection = true
        })
        .Build();

    var arguments = new KernelArguments();
    var eventStream = streamingExecutor.ExecuteStreamAsync(kernel, arguments, options);

    Console.WriteLine("🔌 Starting reconnection-enabled streaming execution...\n");

    try
    {
        await foreach (var evt in eventStream)
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
        
        // Simulate reconnection
        await Task.Delay(1000);
        Console.WriteLine("✅ Reconnected successfully");
    }
}
```

### 6. Advanced Streaming Configuration

The example shows advanced configuration options for streaming.

```csharp
// Advanced streaming configuration
var advancedOptions = StreamingExtensions.CreateStreamingOptions()
    .Configure()
    .WithBufferSize(20)
    .WithEventTypes(
        GraphExecutionEventType.ExecutionStarted,
        GraphExecutionEventType.NodeStarted,
        GraphExecutionEventType.NodeCompleted,
        GraphExecutionEventType.ExecutionCompleted,
        GraphExecutionEventType.ErrorOccurred)
    .WithEventFilter(evt => 
        evt.EventType != GraphExecutionEventType.ExecutionStarted || 
        evt.NodeId == "start")
    .WithBufferingStrategy(BufferingStrategy.TimeBased)
    .WithBufferTimeout(TimeSpan.FromSeconds(2))
    .WithStreamingFormat(StreamingFormat.Json)
    .WithCompression(true)
    .WithReconnectionOptions(new ReconnectionOptions
    {
        MaxReconnectionAttempts = 5,
        ReconnectionDelay = TimeSpan.FromSeconds(2),
        EnableAutoReconnection = true,
        ExponentialBackoff = true
    })
    .Build();
```

### 7. Event Processing and Handling

The example demonstrates comprehensive event processing.

```csharp
// Process different event types
await foreach (var evt in eventStream)
{
    switch (evt.EventType)
    {
        case GraphExecutionEventType.ExecutionStarted:
            Console.WriteLine($"🚀 Execution started at {evt.Timestamp}");
            break;
            
        case GraphExecutionEventType.NodeStarted:
            Console.WriteLine($"▶️ Node {evt.NodeId} started execution");
            break;
            
        case GraphExecutionEventType.NodeCompleted:
            Console.WriteLine($"✅ Node {evt.NodeId} completed successfully");
            if (evt.Result != null)
            {
                Console.WriteLine($"   Result: {evt.Result}");
            }
            break;
            
        case GraphExecutionEventType.ErrorOccurred:
            Console.WriteLine($"❌ Error in node {evt.NodeId}: {evt.ErrorMessage}");
            break;
            
        case GraphExecutionEventType.ExecutionCompleted:
            Console.WriteLine($"🏁 Execution completed at {evt.Timestamp}");
            break;
            
        default:
            Console.WriteLine($"📡 Unknown event type: {evt.EventType}");
            break;
    }
}
```

## Expected Output

The example produces comprehensive output showing:

- 📡 Basic streaming execution with real-time events
- 🔍 Event filtering by type and content
- 📦 Buffered streaming for batch processing
- 🌐 Web API streaming with SSE format
- 🔌 Reconnection handling and recovery
- ⚡ Real-time execution monitoring
- ✅ Complete streaming workflow execution

## Troubleshooting

### Common Issues

1. **Streaming Connection Failures**: Check network connectivity and streaming configuration
2. **Event Processing Errors**: Verify event type handling and error management
3. **Buffering Issues**: Adjust buffer size and timeout settings
4. **Reconnection Failures**: Configure reconnection options and retry logic

### Debugging Tips

- Enable detailed logging for streaming operations
- Monitor event stream health and connection status
- Verify event filtering and buffering configuration
- Check reconnection settings and error handling

## See Also

- [Streaming Execution](../concepts/streaming.md)
- [Event Streaming](../concepts/events.md)
- [Web API Integration](../how-to/exposing-rest-apis.md)
- [Real-time Monitoring](../how-to/metrics-and-observability.md)
- [Connection Management](../how-to/connection-management.md)
