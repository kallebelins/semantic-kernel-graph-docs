# Streaming Execution

Streaming execution in SemanticKernel.Graph enables real-time monitoring and event-driven processing of graph execution. This guide explains how to use `IStreamingGraphExecutor`, consume `GraphExecutionEventStream`, implement filters, manage backpressure, and configure streaming options for production use.

## What You'll Learn

* How to create and configure `StreamingGraphExecutor` for real-time execution
* Consuming execution events through `IGraphExecutionEventStream`
* Implementing event filtering and buffering for performance optimization
* Managing backpressure and connection stability
* Configuring streaming options for different production scenarios
* Building real-time monitoring and responsive applications

## Concepts and Techniques

**Streaming Execution**: Real-time execution of graphs with live event emission, enabling immediate monitoring and responsive applications.

**Event Streams**: Asynchronous streams of execution events that provide real-time updates about graph progress, node completion, and state changes.

**Backpressure Management**: Built-in mechanisms to control event flow and prevent overwhelming consumers while maintaining system stability.

**Event Filtering**: Selective consumption of specific event types to focus monitoring on relevant execution aspects.

**Connection Management**: Automatic reconnection, heartbeat monitoring, and connection type selection for robust streaming.

## Prerequisites

* [Execution Model](execution-model.md) guide completed
* [State Management](state.md) guide completed
* Basic understanding of `GraphExecutor` and graph execution
* Familiarity with asynchronous programming and event-driven patterns

## Core Streaming Components

### StreamingGraphExecutor: Real-Time Execution

`StreamingGraphExecutor` wraps standard graph execution with streaming capabilities:

```csharp
using SemanticKernel.Graph.Streaming;

// Create a streaming-enabled executor
var streamingExecutor = new StreamingGraphExecutor("StreamingDemo", "Real-time execution demo");

// Or convert an existing GraphExecutor
var regularExecutor = new GraphExecutor("MyGraph", "Regular graph");
var streamingExecutor2 = regularExecutor.AsStreamingExecutor();

// Add nodes and configure as usual
streamingExecutor.AddNode(node1);
streamingExecutor.AddNode(node2);
streamingExecutor.Connect("node1", "node2");
streamingExecutor.SetStartNode("node1");
```

### IGraphExecutionEventStream: Event Consumption

`IGraphExecutionEventStream` provides asynchronous iteration over execution events:

```csharp
// Execute with streaming
var eventStream = streamingExecutor.ExecuteStreamAsync(kernel, arguments);

// Consume events in real-time
await foreach (var @event in eventStream)
{
    switch (@event)
    {
        case GraphExecutionStartedEvent started:
            Console.WriteLine($"Execution started: {started.ExecutionId}");
            break;
            
        case NodeStartedEvent nodeStarted:
            Console.WriteLine($"Node started: {nodeStarted.NodeId}");
            break;
            
        case NodeCompletedEvent nodeCompleted:
            Console.WriteLine($"Node completed: {nodeCompleted.NodeId} in {nodeCompleted.Duration}ms");
            break;
            
        case ExecutionCompletedEvent completed:
            Console.WriteLine($"Execution completed in {completed.Duration}ms");
            break;
    }
}
```

## Streaming Configuration and Options

### StreamingExecutionOptions: Fine-Tuned Control

Configure streaming behavior with comprehensive options:

```csharp
var options = new StreamingExecutionOptions
{
    // Buffer configuration
    BufferSize = 100,                    // Initial buffer size
    MaxBufferSize = 1000,                // Maximum buffer before backpressure
    
    // Event filtering
    EventTypesToEmit = new[]
    {
        GraphExecutionEventType.ExecutionStarted,
        GraphExecutionEventType.NodeStarted,
        GraphExecutionEventType.NodeCompleted,
        GraphExecutionEventType.ExecutionCompleted
    },
    
    // Reconnection settings
    EnableAutoReconnect = true,
    MaxReconnectAttempts = 3,
    InitialReconnectDelay = TimeSpan.FromSeconds(1),
    MaxReconnectDelay = TimeSpan.FromSeconds(30),
    
    // Performance optimization
    ProducerBatchSize = 5,               // Batch events before flushing
    ProducerFlushInterval = TimeSpan.FromMilliseconds(100),
    
    // Compression and serialization
    EnableEventCompression = true,
    CompressionAlgorithm = CompressionAlgorithm.Gzip,
    CompressionThresholdBytes = 8 * 1024, // 8KB threshold
    
    // Memory management
    EnableMemoryMappedBuffers = true,
    MemoryMappedFileSizeBytes = 64L * 1024 * 1024, // 64MB
    
    // Monitoring and metrics
    EnableMetrics = true,
    EnableHeartbeat = true,
    HeartbeatInterval = TimeSpan.FromSeconds(30)
};
```

### Extension Methods for Configuration

Use fluent configuration methods for common scenarios:

```csharp
using SemanticKernel.Graph.Streaming;

// Basic configuration
var basicOptions = StreamingExtensions.CreateStreamingOptions()
    .Configure()
    .WithBufferSize(50)
    .WithEventTypes(GraphExecutionEventType.NodeStarted, GraphExecutionEventType.NodeCompleted)
    .Build();

// High-performance configuration
var performanceOptions = StreamingExtensions.CreateStreamingOptions()
    .Configure()
    .WithBufferSize(1000)
    .WithMaxBufferSize(10000)
    .WithProducerBatchSize(20)
    .WithProducerFlushInterval(TimeSpan.FromMilliseconds(50))
    .WithCompression(CompressionAlgorithm.Brotli)
    .Build();

// Monitoring configuration
var monitoringOptions = StreamingExtensions.CreateStreamingOptions()
    .Configure()
    .WithEventTypes(GraphExecutionEventType.ExecutionStarted, 
                   GraphExecutionEventType.NodeStarted, 
                   GraphExecutionEventType.NodeCompleted, 
                   GraphExecutionEventType.ExecutionCompleted)
    .WithHeartbeat(TimeSpan.FromSeconds(10))
    .WithMetrics()
    .Build();
```

## Event Types and Handling

### GraphExecutionEvent: Event Hierarchy

All streaming events inherit from `GraphExecutionEvent`:

```csharp
// Event type enumeration
public enum GraphExecutionEventType
{
    ExecutionStarted = 0,        // Graph execution began
    NodeStarted = 1,             // Node execution started
    NodeCompleted = 2,           // Node execution completed
    NodeFailed = 3,              // Node execution failed
    ExecutionCompleted = 4,      // Graph execution completed
    ExecutionFailed = 5,         // Graph execution failed
    ExecutionCancelled = 6,      // Graph execution cancelled
    NodeEntered = 7,             // Executor entered a node
    NodeExited = 8,              // Executor exited a node
    ConditionEvaluated = 9,      // Conditional expression evaluated
    StateMergeConflictDetected = 10, // State merge conflict
    CircuitBreakerStateChanged = 11,  // Circuit breaker state change
    ResourceBudgetExhausted = 14,     // Resource budget exceeded
    RetryAttempted = 15,              // Retry operation attempted
    CheckpointCreated = 16,           // Checkpoint created
    CheckpointRestored = 17           // Checkpoint restored
}
```

### Event Handling Patterns

Implement different event handling strategies:

```csharp
// Comprehensive event handling
public class ExecutionMonitor
{
    public async Task MonitorExecutionAsync(IGraphExecutionEventStream eventStream)
    {
        var nodeTimings = new Dictionary<string, TimeSpan>();
        var startTimes = new Dictionary<string, DateTimeOffset>();
        
        await foreach (var @event in eventStream)
        {
            switch (@event)
            {
                case NodeStartedEvent nodeStarted:
                    startTimes[nodeStarted.NodeId] = nodeStarted.Timestamp;
                    Console.WriteLine($"🚀 Node started: {nodeStarted.NodeId}");
                    break;
                    
                case NodeCompletedEvent nodeCompleted:
                    var duration = nodeCompleted.Timestamp - startTimes[nodeCompleted.NodeId];
                    nodeTimings[nodeCompleted.NodeId] = duration;
                    Console.WriteLine($"✅ Node completed: {nodeCompleted.NodeId} in {duration.TotalMilliseconds:F0}ms");
                    break;
                    
                case NodeFailedEvent nodeFailed:
                    Console.WriteLine($"❌ Node failed: {nodeFailed.NodeId} - {nodeFailed.ErrorMessage}");
                    break;
                    
                case ExecutionCompletedEvent completed:
                    Console.WriteLine($"🎯 Execution completed in {completed.Duration.TotalMilliseconds:F0}ms");
                    Console.WriteLine("Node performance summary:");
                    foreach (var timing in nodeTimings)
                    {
                        Console.WriteLine($"  {timing.Key}: {timing.Value.TotalMilliseconds:F0}ms");
                    }
                    break;
                    
                case ExecutionFailedEvent failed:
                    Console.WriteLine($"💥 Execution failed: {failed.ErrorMessage}");
                    break;
            }
        }
    }
}
```

## Event Filtering and Buffering

### Selective Event Consumption

Filter events to focus on specific execution aspects:

```csharp
// Filter by event type
var nodeEvents = eventStream.Filter(GraphExecutionEventType.NodeStarted, GraphExecutionEventType.NodeCompleted);
var errorEvents = eventStream.Filter(GraphExecutionEventType.NodeFailed, GraphExecutionEventType.ExecutionFailed);

// Filter by node ID
var specificNodeEvents = eventStream.FilterByNode("critical-node");

// Filter by custom criteria
var longRunningEvents = eventStream.Filter(@event => 
    @event is NodeCompletedEvent completed && 
    completed.Duration > TimeSpan.FromSeconds(5)
);

// Combine filters
var criticalEvents = eventStream
    .Filter(GraphExecutionEventType.NodeFailed, GraphExecutionEventType.ExecutionFailed)
    .FilterByNode("critical-node");
```

### Event Buffering and Batching

Optimize performance with event buffering:

```csharp
// Buffer events for batch processing
var bufferedStream = eventStream.Buffer(10);

await foreach (var batch in bufferedStream)
{
    Console.WriteLine($"Processing batch of {batch.Count} events");
    
    // Process events in batch
    foreach (var @event in batch)
    {
        ProcessEvent(@event);
    }
    
    // Simulate batch processing delay
    await Task.Delay(100);
}

// Adaptive buffering based on throughput
var adaptiveStream = eventStream.WithAdaptiveBuffering(
    minBatchSize: 5,
    maxBatchSize: 50,
    targetLatency: TimeSpan.FromMilliseconds(100)
);
```

## Backpressure Management

### Built-in Backpressure Controls

Streaming execution automatically manages backpressure:

```csharp
var options = new StreamingExecutionOptions
{
    // Buffer size controls
    BufferSize = 100,                    // Initial buffer
    MaxBufferSize = 1000,                // Maximum before backpressure
    
    // Producer batching
    ProducerBatchSize = 5,               // Batch before flushing
    ProducerFlushInterval = TimeSpan.FromMilliseconds(50),
    
    // Memory management
    EnableMemoryMappedBuffers = true,    // Use disk for large buffers
    MemoryMappedFileSizeBytes = 64L * 1024 * 1024
};

var eventStream = streamingExecutor.ExecuteStreamAsync(kernel, arguments, options);

// Consumer-driven backpressure
await foreach (var @event in eventStream)
{
    // Process event
    await ProcessEventAsync(@event);
    
    // Control consumption rate
    if (IsOverloaded())
    {
        await Task.Delay(100); // Slow down consumption
    }
}
```

### Custom Backpressure Strategies

Implement custom backpressure handling:

```csharp
public class AdaptiveEventConsumer
{
    private readonly SemaphoreSlim _processingSemaphore;
    private readonly int _maxConcurrentProcessing;
    
    public AdaptiveEventConsumer(int maxConcurrent = 5)
    {
        _maxConcurrentProcessing = maxConcurrent;
        _processingSemaphore = new SemaphoreSlim(maxConcurrent);
    }
    
    public async Task ConsumeEventsAsync(IGraphExecutionEventStream eventStream)
    {
        await foreach (var @event in eventStream)
        {
            // Wait for processing slot
            await _processingSemaphore.WaitAsync();
            
            // Process event asynchronously
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessEventAsync(@event);
                }
                finally
                {
                    _processingSemaphore.Release();
                }
            });
        }
    }
    
    private async Task ProcessEventAsync(GraphExecutionEvent @event)
    {
        // Simulate event processing
        await Task.Delay(TimeSpan.FromMilliseconds(100));
    }
}
```

## Connection Management and Reliability

### Automatic Reconnection

Handle connection interruptions gracefully:

```csharp
var options = new StreamingExecutionOptions
{
    EnableAutoReconnect = true,
    MaxReconnectAttempts = 5,
    InitialReconnectDelay = TimeSpan.FromSeconds(1),
    MaxReconnectDelay = TimeSpan.FromSeconds(30)
};

var eventStream = streamingExecutor.ExecuteStreamAsync(kernel, arguments, options);

// Handle reconnection events
eventStream.StatusChanged += (sender, args) =>
{
    switch (args.NewStatus)
    {
        case StreamStatus.Reconnecting:
            Console.WriteLine("🔄 Reconnecting to stream...");
            break;
        case StreamStatus.Connected:
            Console.WriteLine("✅ Stream reconnected");
            break;
        case StreamStatus.Failed:
            Console.WriteLine("❌ Stream connection failed");
            break;
    }
};
```

### Connection Type Selection

Choose appropriate connection types for different scenarios:

```csharp
// WebSocket for bidirectional communication
var webSocketOptions = new StreamingExecutionOptions
{
    ConnectionType = StreamingConnectionType.WebSocket,
    EnableCompression = true,
    HeartbeatInterval = TimeSpan.FromSeconds(15)
};

// Server-Sent Events for unidirectional streaming
var sseOptions = new StreamingExecutionOptions
{
    ConnectionType = StreamingConnectionType.ServerSentEvents,
    EnableCompression = false,  // SSE doesn't support compression
    HeartbeatInterval = TimeSpan.FromSeconds(30)
};

// HTTP polling for compatibility
var pollingOptions = new StreamingExecutionOptions
{
    ConnectionType = StreamingConnectionType.HttpPolling,
    BufferSize = 10,  // Smaller buffer for polling
    ProducerFlushInterval = TimeSpan.FromSeconds(1)
};
```

## Performance Optimization

### Memory-Mapped Buffers

Use disk-based buffering for high-throughput scenarios:

```csharp
var highThroughputOptions = new StreamingExecutionOptions
{
    // Enable memory-mapped buffers
    EnableMemoryMappedBuffers = true,
    MemoryMappedFileSizeBytes = 128L * 1024 * 1024, // 128MB
    
    // Optimize for throughput
    BufferSize = 1000,
    MaxBufferSize = 10000,
    ProducerBatchSize = 50,
    ProducerFlushInterval = TimeSpan.FromMilliseconds(25),
    
    // Compression for large payloads
    EnableEventCompression = true,
    CompressionAlgorithm = CompressionAlgorithm.Brotli,
    CompressionThresholdBytes = 4 * 1024 // 4KB threshold
};
```

### Adaptive Compression

Enable intelligent compression decisions:

```csharp
var adaptiveOptions = new StreamingExecutionOptions
{
    EnableEventCompression = true,
    EnableAdaptiveCompression = true,
    
    // Adaptive compression settings
    AdaptiveCompressionWindowSize = 100,
    AdaptiveCompressionMinSavingsRatio = 0.1, // 10% minimum savings
    AdaptiveCompressionMinThresholdBytes = 2 * 1024, // 2KB minimum
    AdaptiveCompressionMaxThresholdBytes = 64 * 1024, // 64KB maximum
    
    // Compression algorithms
    CompressionAlgorithm = CompressionAlgorithm.Gzip
};
```

## Real-Time Monitoring and Dashboards

### Live Execution Monitoring

Build real-time monitoring dashboards:

```csharp
public class ExecutionDashboard
{
    private readonly Dictionary<string, NodeMetrics> _nodeMetrics = new();
    private readonly List<ExecutionEvent> _recentEvents = new();
    
    public async Task MonitorExecutionAsync(IGraphExecutionEventStream eventStream)
    {
        await foreach (var @event in eventStream)
        {
            UpdateMetrics(@event);
            UpdateDashboard();
            
            // Keep only recent events
            if (_recentEvents.Count > 100)
            {
                _recentEvents.RemoveAt(0);
            }
        }
    }
    
    private void UpdateMetrics(GraphExecutionEvent @event)
    {
        switch (@event)
        {
            case NodeStartedEvent nodeStarted:
                if (!_nodeMetrics.ContainsKey(nodeStarted.NodeId))
                {
                    _nodeMetrics[nodeStarted.NodeId] = new NodeMetrics();
                }
                _nodeMetrics[nodeStarted.NodeId].StartTime = nodeStarted.Timestamp;
                break;
                
            case NodeCompletedEvent nodeCompleted:
                if (_nodeMetrics.TryGetValue(nodeCompleted.NodeId, out var metrics))
                {
                    metrics.ExecutionCount++;
                    metrics.TotalDuration += nodeCompleted.Duration ?? TimeSpan.Zero;
                    metrics.AverageDuration = metrics.TotalDuration / metrics.ExecutionCount;
                }
                break;
        }
        
        _recentEvents.Add(new ExecutionEvent
        {
            Timestamp = @event.Timestamp,
            EventType = @event.EventType.ToString(),
            NodeId = @event is NodeEvent nodeEvent ? nodeEvent.NodeId : null
        });
    }
    
    private void UpdateDashboard()
    {
        Console.Clear();
        Console.WriteLine("=== Real-Time Execution Dashboard ===\n");
        
        // Node performance summary
        Console.WriteLine("Node Performance:");
        foreach (var kvp in _nodeMetrics)
        {
            var metrics = kvp.Value;
            Console.WriteLine($"  {kvp.Key}: {metrics.ExecutionCount} executions, " +
                           $"avg: {metrics.AverageDuration.TotalMilliseconds:F0}ms");
        }
        
        // Recent events
        Console.WriteLine("\nRecent Events:");
        foreach (var evt in _recentEvents.TakeLast(10))
        {
            Console.WriteLine($"  [{evt.Timestamp:HH:mm:ss}] {evt.EventType} " +
                           (evt.NodeId != null ? $"({evt.NodeId})" : ""));
        }
    }
}

public class NodeMetrics
{
    public DateTimeOffset StartTime { get; set; }
    public int ExecutionCount { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan AverageDuration { get; set; }
}

public class ExecutionEvent
{
    public DateTimeOffset Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? NodeId { get; set; }
}
```

### Metrics and Telemetry

Enable comprehensive metrics collection:

```csharp
var metricsOptions = new StreamingExecutionOptions
{
    EnableMetrics = true,
    MetricsMeterName = "MyApp.GraphExecution",
    
    // Custom event handlers for metrics
    EventHandlers = new List<IGraphExecutionEventHandler>
    {
        new MetricsEventHandler(),
        new PerformanceEventHandler()
    }
};

public class MetricsEventHandler : IGraphExecutionEventHandler
{
    private readonly Meter _meter = new("MyApp.GraphExecution");
    private readonly Counter<long> _eventCounter;
    private readonly Histogram<double> _nodeDurationHistogram;
    
    public MetricsEventHandler()
    {
        _eventCounter = _meter.CreateCounter<long>("events_total");
        _nodeDurationHistogram = _meter.CreateHistogram<double>("node_duration_ms");
    }
    
    public async Task HandleEventAsync(GraphExecutionEvent @event)
    {
        _eventCounter.Add(1, new KeyValuePair<string, object?>("event_type", @event.EventType.ToString()));
        
        if (@event is NodeCompletedEvent nodeCompleted && nodeCompleted.Duration.HasValue)
        {
            _nodeDurationHistogram.Record(nodeCompleted.Duration.Value.TotalMilliseconds,
                new KeyValuePair<string, object?>("node_id", nodeCompleted.NodeId));
        }
        
        await Task.CompletedTask;
    }
}
```

## Web API Integration

### REST API Streaming

Expose streaming execution through REST APIs:

```csharp
[ApiController]
[Route("api/[controller]")]
public class GraphExecutionController : ControllerBase
{
    [HttpPost("{graphId}/execute/stream")]
    public async Task<IActionResult> ExecuteStreamAsync(
        string graphId,
        [FromBody] ExecutionRequest request,
        CancellationToken cancellationToken)
    {
        var executor = GetExecutor(graphId);
        var arguments = new KernelArguments(request.Parameters);
        
        var options = new StreamingExecutionOptions
        {
            BufferSize = 10,
            EnableEventCompression = true,
            ProducerBatchSize = 1,  // Immediate delivery for API
            ProducerFlushInterval = TimeSpan.FromMilliseconds(50)
        };
        
        var eventStream = executor.ExecuteStreamAsync(kernel, arguments, options, cancellationToken);
        
        // Return Server-Sent Events stream
        Response.Headers.Add("Content-Type", "text/event-stream");
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");
        
        await foreach (var @event in eventStream)
        {
            var eventData = JsonSerializer.Serialize(@event);
            var sseMessage = $"data: {eventData}\n\n";
            
            await Response.WriteAsync(sseMessage, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
            
            if (cancellationToken.IsCancellationRequested)
                break;
        }
        
        return new EmptyResult();
    }
}
```

### WebSocket Streaming

Implement bidirectional WebSocket streaming:

```csharp
[ApiController]
[Route("api/[controller]")]
public class WebSocketController : ControllerBase
{
    [HttpGet("{graphId}/ws")]
    public async Task GetWebSocketAsync(string graphId)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await HandleWebSocketStreamingAsync(graphId, webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    
    private async Task HandleWebSocketStreamingAsync(string graphId, WebSocket webSocket)
    {
        var executor = GetExecutor(graphId);
        var arguments = new KernelArguments();
        
        var options = new StreamingExecutionOptions
        {
            BufferSize = 5,
            EnableEventCompression = true,
            ProducerBatchSize = 1
        };
        
        var eventStream = executor.ExecuteStreamAsync(kernel, arguments, options);
        
        try
        {
            await foreach (var @event in eventStream)
            {
                var eventJson = JsonSerializer.Serialize(@event);
                var buffer = Encoding.UTF8.GetBytes(eventJson);
                
                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            var errorMessage = JsonSerializer.Serialize(new { error = ex.Message });
            var buffer = Encoding.UTF8.GetBytes(errorMessage);
            
            await webSocket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }
    }
}
```

## Best Practices

### Performance Optimization

1. **Buffer Sizing**: Choose appropriate buffer sizes based on consumer processing speed
2. **Event Filtering**: Filter events at the source to reduce unnecessary processing
3. **Batch Processing**: Use producer batching for high-throughput scenarios
4. **Compression**: Enable compression for large event payloads
5. **Memory Management**: Use memory-mapped buffers for very high throughput

### Reliability and Monitoring

1. **Reconnection**: Always enable automatic reconnection for production use
2. **Heartbeats**: Use heartbeat events to detect connection issues
3. **Metrics**: Collect comprehensive metrics for monitoring and alerting
4. **Error Handling**: Implement proper error handling for all event types
5. **Backpressure**: Monitor backpressure and adjust buffer sizes accordingly

### Production Considerations

1. **Connection Types**: Choose appropriate connection types for your infrastructure
2. **Security**: Implement proper authentication and authorization for streaming APIs
3. **Scalability**: Use load balancing and horizontal scaling for multiple consumers
4. **Monitoring**: Implement comprehensive monitoring and alerting
5. **Testing**: Test streaming behavior under various load conditions

## Troubleshooting

### Common Issues

**High Memory Usage**
```
Memory usage is high during streaming execution
```
**Solution**: Reduce buffer sizes, enable memory-mapped buffers, or implement consumer backpressure.

**Event Processing Delays**
```
Events are processed with significant delays
```
**Solution**: Increase producer batch sizes, reduce flush intervals, or optimize consumer processing.

**Connection Instability**
```
Frequent connection drops and reconnections
```
**Solution**: Check network stability, increase reconnection delays, or implement connection pooling.

**High CPU Usage**
```
CPU usage spikes during event processing
```
**Solution**: Optimize event serialization, reduce compression overhead, or implement event batching.

**Buffer Overflow**
```
Events are being dropped due to buffer overflow
```
**Solution**: Increase buffer sizes, implement consumer backpressure, or optimize consumer processing speed.

## See Also

* [Streaming Quickstart](../streaming-quickstart.md) - Quick introduction to streaming execution
* [Execution Model](execution-model.md) - How execution flows through graphs
* [State Management](state.md) - Managing state during streaming execution
* [Checkpointing and Recovery](checkpointing.md) - State persistence during streaming
* [Graph Execution Events](../api/graph-execution-events.md) - API reference for execution events
* [Streaming Examples](../examples/streaming-examples.md) - Practical streaming examples
