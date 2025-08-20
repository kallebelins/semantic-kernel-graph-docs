# Streaming Quickstart

This quick tutorial will teach you how to use streaming execution in SemanticKernel.Graph. You'll learn how to execute graphs with `StreamingGraphExecutor` and consume real-time events via `IGraphExecutionEventStream`.

## What You'll Learn

* Creating and configuring `StreamingGraphExecutor`
* Consuming real-time execution events
* Filtering and buffering event streams
* Handling streaming completion and errors
* Real-time monitoring of graph execution

## Concepts and Techniques

**StreamingGraphExecutor**: A specialized executor that provides real-time execution updates through event streams, enabling live monitoring and responsive applications.

**IGraphExecutionEventStream**: An event stream interface that delivers real-time updates about graph execution progress, node completion, and state changes.

**Streaming Events**: Real-time notifications about graph execution that include node start/complete events, state updates, and execution progress.

**Backpressure Management**: The ability to control the flow of events to prevent overwhelming consumers and maintain system stability.

## Prerequisites

* [First Graph Tutorial](first-graph-5-minutes.md) completed
* [State Quickstart](state-quickstart.md) completed
* [Conditional Nodes Quickstart](conditional-nodes-quickstart.md) completed
* Basic understanding of SemanticKernel.Graph concepts

## Step 1: Basic Streaming Setup

### Create a Streaming Executor

```csharp
using SemanticKernel.Graph.Streaming;

// Create a streaming-enabled graph executor
var streamingExecutor = new StreamingGraphExecutor("StreamingDemo", "Demo of streaming execution");

// Or convert an existing GraphExecutor
var regularExecutor = new GraphExecutor("MyGraph", "Regular graph");
var streamingExecutor2 = regularExecutor.AsStreamingExecutor();
```

### Add Nodes to Your Streaming Graph

```csharp
// Add function nodes
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

// Connect nodes
streamingExecutor.Connect("node1", "node2");
streamingExecutor.Connect("node2", "node3");
streamingExecutor.SetStartNode("node1");
```

## Step 2: Configure Streaming Options

### Basic Streaming Configuration

```csharp
using SemanticKernel.Graph.Extensions;

// Create streaming options with defaults
var options = StreamingExtensions.CreateStreamingOptions();

// Or configure specific options
var configuredOptions = StreamingExtensions.CreateStreamingOptions()
    .Configure()
    .WithBufferSize(20)
    .WithEventTypes(
        GraphExecutionEventType.ExecutionStarted,
        GraphExecutionEventType.NodeStarted,
        GraphExecutionEventType.NodeCompleted,
        GraphExecutionEventType.ExecutionCompleted
    )
    .Build();
```

### Advanced Streaming Options

```csharp
var advancedOptions = StreamingExtensions.CreateStreamingOptions()
    .Configure()
    .WithBufferSize(50)
    .WithEventTypes(
        GraphExecutionEventType.ExecutionStarted,
        GraphExecutionEventType.NodeStarted,
        GraphExecutionEventType.NodeCompleted,
        GraphExecutionEventType.NodeFailed,
        GraphExecutionEventType.ExecutionCompleted,
        GraphExecutionEventType.ExecutionFailed
    )
    .WithCompression(true)
    .WithMaxConcurrentConsumers(5)
    .Build();
```

## Step 3: Execute and Consume the Stream

### Basic Event Consumption

```csharp
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
            
        case NodeStartedEvent nodeStarted:
            Console.WriteLine($"   ▶️  Node started: {nodeStarted.NodeName}");
            break;
            
        case NodeCompletedEvent nodeCompleted:
            Console.WriteLine($"   ✅ Node completed: {nodeCompleted.NodeName} in {nodeCompleted.Duration.TotalMilliseconds:F0}ms");
            break;
            
        case ExecutionCompletedEvent completed:
            Console.WriteLine($"   🎯 Execution completed in {completed.Duration.TotalMilliseconds:F0}ms");
            break;
    }
    
    // Add small delay to demonstrate real-time nature
    await Task.Delay(100);
}
```

## Step 4: Advanced Streaming Features

### Event Filtering

```csharp
// Filter only node-related events
var nodeEventsStream = eventStream.Filter(
    GraphExecutionEventType.NodeStarted,
    GraphExecutionEventType.NodeCompleted
);

Console.WriteLine("🎯 Node Events Only:");
await foreach (var @event in nodeEventsStream)
{
    Console.WriteLine($"   Node Event: {@event.EventType}");
}
```

### Buffered Consumption

```csharp
// Create buffered stream for high-throughput scenarios
var bufferedStream = eventStream.Buffer(10);

Console.WriteLine("🚀 Buffered Events (batch of 10):");
var eventBatch = new List<GraphExecutionEvent>();
await foreach (var @event in bufferedStream)
{
    eventBatch.Add(@event);
    
    if (eventBatch.Count >= 10)
    {
        Console.WriteLine($"   Batch: {eventBatch.Count} events");
        eventBatch.Clear();
    }
}
```

### Wait for Completion

```csharp
// Wait for execution to complete
var completionResult = await eventStream.WaitForCompletionAsync(TimeSpan.FromSeconds(30));

Console.WriteLine($"\n✅ Execution completed!");
Console.WriteLine($"   Status: {completionResult.Status}");
Console.WriteLine($"   Duration: {completionResult.Duration.TotalMilliseconds:F0}ms");
Console.WriteLine($"   Total Events: {completionResult.TotalEvents}");
```

## Step 5: Complete Streaming Example

### Building a Real-Time Monitoring Graph

```csharp
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.Streaming;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-3.5-turbo", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        builder.AddGraphSupport();
        var kernel = builder.Build();

        // Create streaming executor
        var streamingExecutor = new StreamingGraphExecutor("RealTimeMonitor", "Real-time execution monitoring");

        // Create nodes with different execution times
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
                    // Simulate AI analysis
                    await Task.Delay(2000);
                    args["analysis"] = "AI analysis completed";
                    args["analysisTime"] = DateTimeOffset.UtcNow;
                    return "Analysis completed";
                },
                "AnalyzeData",
                "Performs AI analysis"
            ),
            "analysis_node"
        ).StoreResultAs("analysisResult");

        var decisionNode = new ConditionalGraphNode(
            state => state.GetValue<string>("analysis")?.Contains("completed") == true,
            "decision_node",
            "DecisionMaker",
            "Makes routing decision based on analysis"
        );

        var successNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args["result"] = "Success path taken";
                    return "Success processing completed";
                },
                "ProcessSuccess",
                "Handles success path"
            ),
            "success_node"
        ).StoreResultAs("successResult");

        var fallbackNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    args["result"] = "Fallback path taken";
                    return "Fallback processing completed";
                },
                "ProcessFallback",
                "Handles fallback path"
            ),
            "fallback_node"
        ).StoreResultAs("fallbackResult");

        var summaryNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    var startTime = args.GetValue<DateTimeOffset>("startTime");
                    var endTime = DateTimeOffset.UtcNow;
                    var duration = endTime - startTime;
                    
                    args["totalDuration"] = duration.TotalMilliseconds;
                    args["finalResult"] = args.GetValue<string>("result");
                    
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
        streamingExecutor.AddNode(decisionNode);
        streamingExecutor.AddNode(successNode);
        streamingExecutor.AddNode(fallbackNode);
        streamingExecutor.AddNode(summaryNode);

        // Connect nodes
        streamingExecutor.Connect(inputNode, analysisNode);
        streamingExecutor.Connect(analysisNode, decisionNode);
        streamingExecutor.Connect(decisionNode, successNode, 
            edge => edge.When(state => state.GetValue<string>("analysis")?.Contains("completed") == true));
        streamingExecutor.Connect(decisionNode, fallbackNode, 
            edge => edge.When(state => !(state.GetValue<string>("analysis")?.Contains("completed") == true)));
        streamingExecutor.Connect(successNode, summaryNode);
        streamingExecutor.Connect(fallbackNode, summaryNode);

        streamingExecutor.SetStartNode(inputNode);

        // Configure streaming options
        var options = StreamingExtensions.CreateStreamingOptions()
            .Configure()
            .WithBufferSize(15)
            .WithEventTypes(
                GraphExecutionEventType.ExecutionStarted,
                GraphExecutionEventType.NodeStarted,
                GraphExecutionEventType.NodeCompleted,
                GraphExecutionEventType.ExecutionCompleted
            )
            .Build();

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
                    
                case NodeStartedEvent nodeStarted:
                    Console.WriteLine($"   ▶️  Node: {nodeStarted.NodeName}");
                    break;
                    
                case NodeCompletedEvent nodeCompleted:
                    var duration = nodeCompleted.Duration.TotalMilliseconds;
                    Console.WriteLine($"   ✅ Node: {nodeCompleted.NodeName} ({duration:F0}ms)");
                    break;
                    
                case ExecutionCompletedEvent completed:
                    var totalDuration = completed.Duration.TotalMilliseconds;
                    Console.WriteLine($"   🎯 Total Duration: {totalDuration:F0}ms");
                    break;
            }
            
            // Small delay for readability
            await Task.Delay(200);
        }

        // Wait for completion and show results
        var completionResult = await eventStream.WaitForCompletionAsync(TimeSpan.FromSeconds(30));
        
        Console.WriteLine($"\n=== Execution Summary ===");
        Console.WriteLine($"Status: {completionResult.Status}");
        Console.WriteLine($"Total Events: {completionResult.TotalEvents}");
        Console.WriteLine($"Duration: {completionResult.Duration.TotalMilliseconds:F0}ms");
        
        // Show final state
        var finalState = await streamingExecutor.ExecuteAsync(kernel, arguments);
        Console.WriteLine($"Final Result: {finalState.GetValue<string>("finalResult")}");
        Console.WriteLine($"Total Duration: {finalState.GetValue<double>("totalDuration"):F0}ms");
        
        Console.WriteLine("\n✅ Streaming execution completed successfully!");
    }
}
```

## Step 6: Run Your Streaming Example

### Set Up Environment Variables

```bash
# Windows
setx OPENAI_API_KEY "your-api-key-here"

# macOS/Linux
export OPENAI_API_KEY="your-api-key-here"
```

### Execute the Graph

```bash
dotnet run
```

You should see real-time output like:

```
=== Real-Time Execution Monitoring ===

⚡ Starting streaming execution...

[14:30:15.123] #1 ExecutionStarted
   🚀 Execution ID: abc123def456
[14:30:15.125] #2 NodeStarted
   ▶️  Node: input_node
[14:30:15.127] #3 NodeCompleted
   ✅ Node: input_node (4ms)
[14:30:15.129] #4 NodeStarted
   ▶️  Node: analysis_node
[14:30:17.135] #5 NodeCompleted
   ✅ Node: analysis_node (2006ms)
[14:30:17.137] #6 NodeStarted
   ▶️  Node: decision_node
[14:30:17.138] #7 NodeCompleted
   ✅ Node: decision_node (1ms)
[14:30:17.140] #8 NodeStarted
   ▶️  Node: success_node
[14:30:17.141] #9 NodeCompleted
   ✅ Node: success_node (1ms)
[14:30:17.143] #10 NodeStarted
   ▶️  Node: summary_node
[14:30:17.144] #11 NodeCompleted
   ✅ Node: summary_node (1ms)
[14:30:17.145] #12 ExecutionCompleted
   🎯 Total Duration: 2022ms

=== Execution Summary ===
Status: Completed
Total Events: 12
Duration: 2022ms
Final Result: Success path taken
Total Duration: 2022ms

✅ Streaming execution completed successfully!
```

## What Just Happened?

### 1. **Streaming Executor Creation**
```csharp
var streamingExecutor = new StreamingGraphExecutor("RealTimeMonitor");
```
Creates an executor that emits real-time events during graph execution.

### 2. **Event Stream Generation**
```csharp
var eventStream = streamingExecutor.ExecuteStreamAsync(kernel, arguments, options);
```
Starts execution and returns a stream of real-time events.

### 3. **Real-Time Event Consumption**
```csharp
await foreach (var @event in eventStream)
{
    // Process each event as it occurs
}
```
Consumes events as they're generated, providing real-time monitoring.

### 4. **Event Filtering and Buffering**
```csharp
var nodeEventsStream = eventStream.Filter(GraphExecutionEventType.NodeStarted);
var bufferedStream = eventStream.Buffer(10);
```
Filters specific event types and buffers events for high-throughput scenarios.

## Key Concepts

* **StreamingGraphExecutor**: Executes graphs while emitting real-time events
* **IGraphExecutionEventStream**: Provides asynchronous iteration over execution events
* **GraphExecutionEvent**: Base class for all execution events (started, completed, failed, etc.)
* **Event Filtering**: Select specific event types for monitoring
* **Event Buffering**: Batch processing of events for performance
* **Real-Time Monitoring**: Observing execution progress as it happens

## Common Patterns

### Monitor Specific Event Types
```csharp
var criticalEvents = eventStream.Filter(
    GraphExecutionEventType.NodeFailed,
    GraphExecutionEventType.ExecutionFailed
);
```

### Buffer Events for Batch Processing
```csharp
var batchStream = eventStream.Buffer(50);
await foreach (var batch in batchStream)
{
    // Process events in batches of 50
}
```

### Handle Different Event Types
```csharp
switch (@event)
{
    case NodeStartedEvent started:
        Console.WriteLine($"Node {started.NodeName} started");
        break;
    case NodeCompletedEvent completed:
        Console.WriteLine($"Node {completed.NodeName} completed in {completed.Duration}ms");
        break;
}
```

### Wait for Completion
```csharp
var result = await eventStream.WaitForCompletionAsync(TimeSpan.FromSeconds(60));
if (result.Status == StreamStatus.Completed)
{
    Console.WriteLine($"Execution completed in {result.Duration}ms");
}
```

## Troubleshooting

### **Stream Never Starts**
```
No events are being emitted
```
**Solution**: Ensure the graph has a start node and is properly configured.

### **Events Stop Mid-Execution**
```
Stream ends unexpectedly
```
**Solution**: Check for exceptions in node execution and verify error handling.

### **High Memory Usage**
```
Memory consumption increases during streaming
```
**Solution**: Use buffering and process events in batches, dispose of streams properly.

### **Events Arrive Out of Order**
```
Event sequence is not chronological
```
**Solution**: Use `HighPrecisionTimestamp` for precise ordering in high-throughput scenarios.

## Next Steps

* **[Streaming Tutorial](streaming-tutorial.md)**: Advanced streaming patterns and best practices
* **[Event Handling](how-to/event-handling.md)**: Custom event handlers and processing
* **[Performance Optimization](how-to/streaming-performance.md)**: High-throughput streaming scenarios
* **[Core Concepts](concepts/index.md)**: Understanding graphs, nodes, and execution

## Concepts and Techniques

This tutorial introduces several key concepts:

* **Streaming Execution**: Real-time monitoring of graph execution progress
* **Event Streams**: Asynchronous consumption of execution events
* **Event Types**: Different categories of execution events (started, completed, failed)
* **Event Filtering**: Selective monitoring of specific event types
* **Event Buffering**: Batch processing of events for performance
* **Real-Time Monitoring**: Observing execution progress as it happens

## Prerequisites and Minimum Configuration

To complete this tutorial, you need:
* **.NET 8.0+** runtime and SDK
* **SemanticKernel.Graph** package installed
* **LLM Provider** configured with valid API keys
* **Environment Variables** set up for your API credentials

## See Also

* **[First Graph Tutorial](first-graph-5-minutes.md)**: Create your first graph workflow
* **[State Quickstart](state-quickstart.md)**: Manage data flow between nodes
* **[Conditional Nodes Quickstart](conditional-nodes-quickstart.md)**: Add decision-making to workflows
* **[Streaming Tutorial](streaming-tutorial.md)**: Advanced streaming concepts
* **[Core Concepts](concepts/index.md)**: Understanding graphs, nodes, and execution
* **[API Reference](api/streaming.md)**: Complete streaming API documentation

## Reference APIs

* **[StreamingGraphExecutor](../api/streaming.md#streaming-graph-executor)**: Streaming execution engine
* **[IGraphExecutionEventStream](../api/streaming.md#igraph-execution-event-stream)**: Event stream interface
* **[GraphExecutionEvent](../api/streaming.md#graph-execution-event)**: Execution event types
* **[StreamingExecutionOptions](../api/streaming.md#streaming-execution-options)**: Streaming configuration
