# Streaming Execution Example

This example demonstrates streaming execution capabilities of the Semantic Kernel Graph system, showing real-time event streaming, buffering, and reconnection features.

## Objective

Learn how to implement streaming execution in graph-based workflows to:
* Enable real-time event streaming during graph execution
* Implement event filtering and buffering strategies
* Support web API streaming for long-running operations
* Handle connection management and reconnection scenarios
* Monitor execution progress in real-time

## Prerequisites

* **.NET 8.0** or later
* **OpenAI API Key** configured in `appsettings.json`
* **Semantic Kernel Graph package** installed
* Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Streaming Execution](../concepts/streaming.md)
* Familiarity with [Event Streaming](../concepts/events.md)

## Key Components

### Concepts and Techniques

* **Streaming Execution**: Real-time event streaming during graph execution
* **Event Filtering**: Selective event processing based on type and content
* **Buffered Streaming**: Event buffering for batch processing
* **Web API Streaming**: HTTP streaming for web applications
* **Connection Management**: Handling disconnections and reconnections

### Core Classes

* `StreamingGraphExecutor`: Executor with streaming capabilities
* `GraphExecutionEventStream`: Stream of execution events
* `StreamingExtensions`: Configuration utilities for streaming
* `GraphExecutionEvent`: Individual execution events
* `FunctionGraphNode`: Graph nodes for workflow execution

## Running the Example

### Getting Started

This example demonstrates streaming execution and real-time monitoring with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Basic Streaming Execution

The example starts with basic streaming execution showing real-time events.

```csharp
private static async Task RunBasicStreamingExample(Kernel kernel)
{
    // Use the runnable example in the examples folder to ensure code compiles and runs
    await StreamingQuickstartExample.RunAsync(kernel);
}
```

### 2. Event Filtering

The example demonstrates filtering events based on type and content.

```csharp
// The full, runnable filtering example is available in:
// `semantic-kernel-graph-docs/examples/StreamingQuickstartExample.cs`
// Run the complete scenario via the examples runner:
await StreamingQuickstartExample.RunAsync(kernel);
```

### 3. Buffered Streaming

The example shows buffered streaming for batch event processing.

```csharp
// The buffered streaming scenario is implemented and tested in:
// `semantic-kernel-graph-docs/examples/StreamingQuickstartExample.cs`
// Execute the scenario using the examples runner:
await StreamingQuickstartExample.RunAsync(kernel);
```

### 4. Web API Streaming

The example demonstrates streaming for web API scenarios.

```csharp
// Web API streaming example (SSE) is provided and validated in:
// `semantic-kernel-graph-docs/examples/StreamingQuickstartExample.cs`
// Use the examples runner to execute the scenario:
await StreamingQuickstartExample.RunAsync(kernel);
```

### 5. Reconnection Example

The example demonstrates handling disconnections and reconnections.

```csharp
// Reconnection handling demo is implemented and tested here:
// `semantic-kernel-graph-docs/examples/StreamingQuickstartExample.cs`
// Execute it via the examples runner:
await StreamingQuickstartExample.RunAsync(kernel);
```

### 6. Advanced Streaming Configuration

The example shows advanced configuration options for streaming.

```csharp
// Advanced configuration options are demonstrated in the runnable example:
// `semantic-kernel-graph-docs/examples/StreamingQuickstartExample.cs`
await StreamingQuickstartExample.RunAsync(kernel);
```

### 7. Event Processing and Handling

The example demonstrates comprehensive event processing.

```csharp
// Example event processing and handling is available in the runnable example.
// Run the sample to see complete event handling logic and outputs:
await StreamingQuickstartExample.RunAsync(kernel);
```

## Expected Output

The example produces comprehensive output showing:

* 📡 Basic streaming execution with real-time events
* 🔍 Event filtering by type and content
* 📦 Buffered streaming for batch processing
* 🌐 Web API streaming with SSE format
* 🔌 Reconnection handling and recovery
* ⚡ Real-time execution monitoring
* ✅ Complete streaming workflow execution

## Troubleshooting

### Common Issues

1. **Streaming Connection Failures**: Check network connectivity and streaming configuration
2. **Event Processing Errors**: Verify event type handling and error management
3. **Buffering Issues**: Adjust buffer size and timeout settings
4. **Reconnection Failures**: Configure reconnection options and retry logic

### Debugging Tips

* Enable detailed logging for streaming operations
* Monitor event stream health and connection status
* Verify event filtering and buffering configuration
* Check reconnection settings and error handling

## See Also

* [Streaming Execution](../concepts/streaming.md)
* [Event Streaming](../concepts/events.md)
* [Web API Integration](../how-to/exposing-rest-apis.md)
* [Real-time Monitoring](../how-to/metrics-and-observability.md)
* [Connection Management](../how-to/connection-management.md)
