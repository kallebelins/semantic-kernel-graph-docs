# Exposing REST APIs

This guide explains how to expose SemanticKernel.Graph functionality through REST APIs, streaming endpoints, webhooks, and GraphQL subscriptions for integration with external services and applications.

## Overview

SemanticKernel.Graph provides a comprehensive API layer that enables external systems to:

- **Execute graphs remotely** through REST endpoints with authentication and rate limiting
- **Stream execution events** in real-time via WebSockets, Server-Sent Events, or HTTP polling
- **Monitor execution status** and retrieve results asynchronously
- **Integrate with webhooks** for event-driven architectures
- **Subscribe to GraphQL subscriptions** for real-time updates and inspection

## Core REST API Components

### GraphRestApi: Main API Service

The `GraphRestApi` class provides a framework-agnostic service layer that can be adapted to any HTTP framework:

- **Graph Execution**: Execute graphs with parameters and retrieve results
- **Graph Management**: List, register, and manage graph definitions
- **Execution Monitoring**: Track execution status and retrieve results
- **Security**: API key and bearer token authentication
- **Rate Limiting**: Configurable request throttling and quotas

### API Configuration Options

Configure the REST API behavior through `GraphRestApiOptions`:

```csharp
using SemanticKernel.Graph.Integration;

var apiOptions = new GraphRestApiOptions
{
    // Authentication
    ApiKey = "your-api-key",
    RequireAuthentication = true,
    EnableBearerTokenAuth = true,
    RequiredScopes = new[] { "graph.execute", "graph.read" },
    RequiredAppRoles = new[] { "GraphUser" },
    
    // Rate Limiting
    EnableRateLimiting = true,
    RateLimitWindow = TimeSpan.FromMinutes(1),
    GlobalRequestsPerWindow = 1000,
    PerApiKeyRequestsPerWindow = 100,
    PerTenantRequestsPerWindow = 50,
    
    // Execution Control
    MaxConcurrentExecutions = 64,
    DefaultTimeout = TimeSpan.FromMinutes(5),
    EnableExecutionQueue = true,
    QueueMaxLength = 1000,
    
    // Idempotency
    EnableIdempotency = true,
    MaxIdempotencyEntries = 10000
};
```

## REST API Endpoints

### Basic Graph Operations

#### List Registered Graphs

```csharp
// GET /graphs
app.MapGet("/graphs", async () => 
{
    var graphs = await graphApi.ListGraphsAsync();
    return Results.Ok(graphs);
});
```

#### Execute Graph

```csharp
// POST /graphs/execute
app.MapPost("/graphs/execute", async (ExecuteGraphRequest request, HttpContext http) =>
{
    var apiKey = http.Request.Headers["x-api-key"].FirstOrDefault();
    var response = await graphApi.ExecuteAsync(request, apiKey, http.RequestAborted);
    return Results.Json(response);
});
```

#### Enqueue Execution

```csharp
// POST /graphs/enqueue
app.MapPost("/graphs/enqueue", async (EnqueueExecutionRequest request, HttpContext http) =>
{
    var apiKey = http.Request.Headers["x-api-key"].FirstOrDefault();
    var response = await graphApi.EnqueueAsync(request, apiKey, http.RequestAborted);
    return Results.Json(response);
});
```

### Request and Response Models

#### ExecuteGraphRequest

```csharp
var request = new ExecuteGraphRequest
{
    GraphName = "my-workflow",
    Variables = new Dictionary<string, object?>
    {
        ["input"] = "Hello, World!",
        ["user_id"] = "user123",
        ["priority"] = "high"
    },
    StartNodeId = "start-node", // Optional
    TimeoutSeconds = 300,       // Optional
    IdempotencyKey = "req-123"  // Optional
};
```

#### ExecuteGraphResponse

```csharp
var response = new ExecuteGraphResponse
{
    ExecutionId = "exec-456",
    GraphName = "my-workflow",
    Success = true,
    Result = "Processed: Hello, World!",
    ExecutionTime = TimeSpan.FromSeconds(2.5),
    NodeCount = 5
};
```

## Streaming Execution APIs

### Streaming API Interface

The `IGraphStreamingApi` provides streaming execution capabilities:

```csharp
using SemanticKernel.Graph.Streaming;

var streamingApi = serviceProvider.GetService<IGraphStreamingApi>();

// Start streaming execution
var streamingRequest = new StreamingExecutionRequest
{
    Arguments = new Dictionary<string, object>
    {
        ["input"] = "Streaming test"
    },
    Options = new StreamingExecutionOptions
    {
        BufferSize = 10,
        EnableCompression = true,
        ConnectionType = StreamingConnectionType.WebSocket
    }
};

var streamingResponse = await streamingApi.StartExecutionAsync(
    "my-graph", 
    streamingRequest
);
```

### Streaming Endpoints

#### WebSocket Streaming

```csharp
// GET /graphs/{graphId}/stream/ws
app.MapGet("/graphs/{graphId}/stream/ws", async (string graphId, HttpContext http) =>
{
    if (http.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await http.WebSockets.AcceptWebSocketAsync();
        await HandleWebSocketStreamingAsync(graphId, webSocket);
    }
    else
    {
        http.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

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
```

#### Server-Sent Events

```csharp
// GET /graphs/{graphId}/stream/sse
app.MapGet("/graphs/{graphId}/stream/sse", async (string graphId, HttpContext http) =>
{
    http.Response.Headers.Add("Content-Type", "text/event-stream");
    http.Response.Headers.Add("Cache-Control", "no-cache");
    http.Response.Headers.Add("Connection", "keep-alive");
    
    var executor = GetExecutor(graphId);
    var arguments = new KernelArguments();
    
    var eventStream = executor.ExecuteStreamAsync(kernel, arguments);
    
    try
    {
        await foreach (var @event in eventStream)
        {
            var eventJson = JsonSerializer.Serialize(@event);
            var sseData = $"data: {eventJson}\n\n";
            
            await http.Response.WriteAsync(sseData);
            await http.Response.Body.FlushAsync();
        }
    }
    catch (Exception ex)
    {
        var errorData = $"data: {{\"error\":\"{ex.Message}\"}}\n\n";
        await http.Response.WriteAsync(errorData);
        await http.Response.Body.FlushAsync();
    }
});
```

#### HTTP Polling

```csharp
// GET /graphs/{graphId}/stream/poll
app.MapGet("/graphs/{graphId}/stream/poll", async (string graphId, string? lastEventId, HttpContext http) =>
{
    var executor = GetExecutor(graphId);
    var arguments = new KernelArguments();
    
    var options = new StreamingExecutionOptions
    {
        BufferSize = 50,
        EnableCompression = false
    };
    
    var eventStream = executor.ExecuteStreamAsync(kernel, arguments, options);
    var events = new List<GraphExecutionEvent>();
    
    // Collect events since last poll
    await foreach (var @event in eventStream)
    {
        if (string.IsNullOrEmpty(lastEventId) || 
            string.Compare(@event.EventId, lastEventId, StringComparison.Ordinal) > 0)
        {
            events.Add(@event);
        }
        
        if (events.Count >= 10) break; // Limit batch size
    }
    
    var response = new PollingResponse
    {
        Events = events,
        LastEventId = events.LastOrDefault()?.EventId,
        HasMore = events.Count >= 10
    };
    
    return Results.Json(response);
});
```

## Webhook Integration

### Webhook Configuration

Configure webhooks for external service notifications:

```csharp
public sealed class WebhookConfiguration
{
    public string Url { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public GraphExecutionEventType[] EventTypes { get; set; } = Array.Empty<GraphExecutionEventType>();
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxRetries { get; set; } = 3;
}

public sealed class WebhookService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookService> _logger;
    private readonly List<WebhookConfiguration> _webhooks = new();
    
    public void RegisterWebhook(WebhookConfiguration config)
    {
        _webhooks.Add(config);
    }
    
    public async Task NotifyWebhooksAsync(GraphExecutionEvent @event)
    {
        var relevantWebhooks = _webhooks.Where(w => 
            w.EventTypes.Contains(@event.EventType)).ToList();
            
        foreach (var webhook in relevantWebhooks)
        {
            await SendWebhookAsync(webhook, @event);
        }
    }
    
    private async Task SendWebhookAsync(WebhookConfiguration webhook, GraphExecutionEvent @event)
    {
        try
        {
            var payload = new
            {
                event_type = @event.EventType.ToString(),
                execution_id = @event.ExecutionId,
                node_id = @event.NodeId,
                timestamp = @event.Timestamp,
                data = @event
            };
            
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Add signature for security
            var signature = ComputeSignature(json, webhook.Secret);
            content.Headers.Add("X-Webhook-Signature", signature);
            
            var response = await _httpClient.PostAsync(webhook.Url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Webhook delivery failed: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send webhook to {Url}", webhook.Url);
        }
    }
    
    private string ComputeSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash);
    }
}
```

### Webhook Event Handling

```csharp
// Configure webhooks for specific event types
var webhookService = new WebhookService(httpClient, logger);

webhookService.RegisterWebhook(new WebhookConfiguration
{
    Url = "https://external-service.com/webhook",
    Secret = "webhook-secret-123",
    EventTypes = new[] 
    { 
        GraphExecutionEventType.NodeCompleted,
        GraphExecutionEventType.ExecutionCompleted 
    },
    RetryInterval = TimeSpan.FromMinutes(2),
    MaxRetries = 5
});

// Integrate with streaming execution
var eventStream = executor.ExecuteStreamAsync(kernel, arguments);

await foreach (var @event in eventStream)
{
    // Process event locally
    await ProcessEventAsync(@event);
    
    // Notify webhooks
    await webhookService.NotifyWebhooksAsync(@event);
}
```

## GraphQL Subscriptions

### GraphQL Schema

Define GraphQL schema for real-time subscriptions:

```graphql
type GraphExecutionEvent {
  eventId: ID!
  executionId: ID!
  nodeId: ID
  eventType: String!
  timestamp: String!
  data: JSON
}

type Subscription {
  graphExecutionEvents(
    executionId: ID
    nodeId: ID
    eventTypes: [String!]
  ): GraphExecutionEvent!
  
  nodeExecutionStatus(
    executionId: ID!
    nodeId: ID!
  ): NodeStatus!
  
  graphExecutionProgress(
    executionId: ID!
  ): ExecutionProgress!
}

type NodeStatus {
  nodeId: ID!
  status: String!
  startTime: String
  endTime: String
  duration: Float
  result: String
  error: String
}

type ExecutionProgress {
  executionId: ID!
  totalNodes: Int!
  completedNodes: Int!
  failedNodes: Int!
  progress: Float!
  estimatedTimeRemaining: Float
}
```

### GraphQL Subscription Implementation

```csharp
public sealed class GraphQLSubscriptionService
{
    private readonly IGraphExecutionEventStream _eventStream;
    private readonly ILogger<GraphQLSubscriptionService> _logger;
    
    public IAsyncEnumerable<GraphExecutionEvent> SubscribeToExecutionEvents(
        string? executionId = null,
        string? nodeId = null,
        GraphExecutionEventType[]? eventTypes = null)
    {
        var options = new StreamingExecutionOptions
        {
            BufferSize = 100,
            EnableCompression = true,
            EventTypesToReceive = eventTypes
        };
        
        return _eventStream.GetEventsAsync(executionId, nodeId, options);
    }
    
    public IAsyncEnumerable<NodeStatus> SubscribeToNodeStatus(
        string executionId, 
        string nodeId)
    {
        return SubscribeToExecutionEvents(executionId, nodeId, new[] 
        { 
            GraphExecutionEventType.NodeStarted,
            GraphExecutionEventType.NodeCompleted,
            GraphExecutionEventType.NodeFailed 
        })
        .Select(@event => MapToNodeStatus(@event));
    }
    
    public IAsyncEnumerable<ExecutionProgress> SubscribeToExecutionProgress(
        string executionId)
    {
        return SubscribeToExecutionEvents(executionId, null, new[] 
        { 
            GraphExecutionEventType.NodeStarted,
            GraphExecutionEventType.NodeCompleted,
            GraphExecutionEventType.NodeFailed,
            GraphExecutionEventType.ExecutionCompleted 
        })
        .Buffer(TimeSpan.FromSeconds(1))
        .Select(events => CalculateProgress(executionId, events));
    }
    
    private NodeStatus MapToNodeStatus(GraphExecutionEvent @event)
    {
        return new NodeStatus
        {
            NodeId = @event.NodeId ?? string.Empty,
            Status = @event.EventType.ToString(),
            StartTime = @event.Timestamp.ToString("O"),
            // Map other properties based on event type
        };
    }
    
    private ExecutionProgress CalculateProgress(string executionId, IEnumerable<GraphExecutionEvent> events)
    {
        // Calculate progress based on event batch
        var totalNodes = events.Count(e => e.EventType == GraphExecutionEventType.NodeStarted);
        var completedNodes = events.Count(e => e.EventType == GraphExecutionEventType.NodeCompleted);
        var failedNodes = events.Count(e => e.EventType == GraphExecutionEventType.NodeFailed);
        
        return new ExecutionProgress
        {
            ExecutionId = executionId,
            TotalNodes = totalNodes,
            CompletedNodes = completedNodes,
            FailedNodes = failedNodes,
            Progress = totalNodes > 0 ? (float)completedNodes / totalNodes : 0f
        };
    }
}
```

## Real-Time Visualization

### Real-Time Highlighter

Use `GraphRealtimeHighlighter` for live execution visualization:

```csharp
using SemanticKernel.Graph.Core;

var highlighter = new GraphRealtimeHighlighter(
    visualizationEngine, 
    eventStream, 
    logger, 
    options
);

// Subscribe to real-time updates
highlighter.NodeExecutionStarted += (sender, e) =>
{
    Console.WriteLine($"Node {e.NodeId} started execution");
    // Update UI with node highlighting
};

highlighter.NodeExecutionCompleted += (sender, e) =>
{
    Console.WriteLine($"Node {e.NodeId} completed in {e.Duration}");
    // Update UI with completion status
};

// Start highlighting for an execution
await highlighter.StartHighlightingAsync("exec-123");

// Get real-time highlight state
var highlightState = highlighter.GetHighlightState("exec-123");
```

### WebSocket Visualization Endpoint

```csharp
// GET /graphs/{graphId}/visualize/ws
app.MapGet("/graphs/{graphId}/visualize/ws", async (string graphId, HttpContext http) =>
{
    if (http.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await http.WebSockets.AcceptWebSocketAsync();
        await HandleVisualizationWebSocketAsync(graphId, webSocket);
    }
});

private async Task HandleVisualizationWebSocketAsync(string graphId, WebSocket webSocket)
{
    var highlighter = GetHighlighter(graphId);
    
    // Subscribe to highlight events
    highlighter.NodeExecutionStarted += async (sender, e) =>
    {
        var highlightEvent = new
        {
            type = "node_started",
            nodeId = e.NodeId,
            timestamp = e.Timestamp,
            executionId = e.ExecutionId
        };
        
        await SendWebSocketMessageAsync(webSocket, highlightEvent);
    };
    
    highlighter.NodeExecutionCompleted += async (sender, e) =>
    {
        var highlightEvent = new
        {
            type = "node_completed",
            nodeId = e.NodeId,
            timestamp = e.Timestamp,
            duration = e.Duration?.TotalMilliseconds,
            result = e.Result
        };
        
        await SendWebSocketMessageAsync(webSocket, highlightEvent);
    };
    
    // Keep connection alive
    var buffer = new byte[1024];
    while (webSocket.State == WebSocketState.Open)
    {
        try
        {
            var result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
                
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                    string.Empty, CancellationToken.None);
            }
        }
        catch
        {
            break;
        }
    }
}

private async Task SendWebSocketMessageAsync(WebSocket webSocket, object message)
{
    try
    {
        var json = JsonSerializer.Serialize(message);
        var buffer = Encoding.UTF8.GetBytes(json);
        
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }
    catch
    {
        // Handle connection errors
    }
}
```

## Integration Examples

### Minimal API Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddKernel().AddGraphSupport();
builder.Services.AddSingleton<GraphRestApi>();
builder.Services.AddSingleton<GraphQLSubscriptionService>();
builder.Services.AddSingleton<WebhookService>();

var app = builder.Build();

// Configure REST API
var graphApi = app.Services.GetRequiredService<GraphRestApi>();
var subscriptionService = app.Services.GetRequiredService<GraphQLSubscriptionService>();

// REST endpoints
app.MapGet("/graphs", () => graphApi.ListGraphsAsync());
app.MapPost("/graphs/execute", async (ExecuteGraphRequest req) => 
    await graphApi.ExecuteAsync(req));

// Streaming endpoints
app.MapGet("/graphs/{graphId}/stream/ws", HandleWebSocketStreaming);
app.MapGet("/graphs/{graphId}/stream/sse", HandleServerSentEvents);

// Visualization endpoints
app.MapGet("/graphs/{graphId}/visualize/ws", HandleVisualizationWebSocket);

app.Run();
```

### ASP.NET Core Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class GraphExecutionController : ControllerBase
{
    private readonly GraphRestApi _graphApi;
    private readonly GraphQLSubscriptionService _subscriptionService;
    
    public GraphExecutionController(
        GraphRestApi graphApi,
        GraphQLSubscriptionService subscriptionService)
    {
        _graphApi = graphApi;
        _subscriptionService = subscriptionService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RegisteredGraphInfo>>> ListGraphs()
    {
        var graphs = await _graphApi.ListGraphsAsync();
        return Ok(graphs);
    }
    
    [HttpPost("execute")]
    public async Task<ActionResult<ExecuteGraphResponse>> ExecuteGraph(
        [FromBody] ExecuteGraphRequest request)
    {
        var apiKey = Request.Headers["x-api-key"].FirstOrDefault();
        var response = await _graphApi.ExecuteAsync(request, apiKey, Request.HttpContext.RequestAborted);
        
        if (response.Success)
            return Ok(response);
        else
            return BadRequest(response);
    }
    
    [HttpPost("enqueue")]
    public async Task<ActionResult<EnqueueExecutionResponse>> EnqueueExecution(
        [FromBody] EnqueueExecutionRequest request)
    {
        var apiKey = Request.Headers["x-api-key"].FirstOrDefault();
        var response = await _graphApi.EnqueueAsync(request, apiKey, Request.HttpContext.RequestAborted);
        
        return Ok(response);
    }
    
    [HttpGet("{executionId}/status")]
    public async Task<ActionResult<ExecutionStatusResponse>> GetExecutionStatus(
        string executionId)
    {
        var status = await _graphApi.GetExecutionStatusAsync(executionId);
        return Ok(status);
    }
}
```

## Security and Authentication

### API Key Authentication

```csharp
var apiOptions = new GraphRestApiOptions
{
    ApiKey = Environment.GetEnvironmentVariable("GRAPH_API_KEY"),
    RequireAuthentication = true
};

// In middleware
app.Use(async (context, next) =>
{
    var apiKey = context.Request.Headers["x-api-key"].FirstOrDefault();
    
    if (string.IsNullOrEmpty(apiKey) || apiKey != apiOptions.ApiKey)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
    }
    
    await next();
});
```

### Bearer Token Authentication

```csharp
var apiOptions = new GraphRestApiOptions
{
    RequireAuthentication = true,
    EnableBearerTokenAuth = true,
    RequiredScopes = new[] { "graph.execute", "graph.read" },
    RequiredAppRoles = new[] { "GraphUser" }
};

// Register bearer token validator
builder.Services.AddSingleton<IBearerTokenValidator, AzureAdBearerTokenValidator>();
```

## Best Practices

### Performance Optimization

1. **Connection Pooling**: Use connection pooling for external service calls
2. **Event Batching**: Batch events when possible to reduce overhead
3. **Compression**: Enable compression for large event payloads
4. **Caching**: Cache frequently accessed graph definitions and results

### Security Considerations

1. **Rate Limiting**: Implement appropriate rate limiting per client/tenant
2. **Input Validation**: Validate all input parameters and sanitize data
3. **Authentication**: Use strong authentication mechanisms (API keys, OAuth)
4. **HTTPS**: Always use HTTPS in production

### Monitoring and Observability

1. **Metrics**: Track API usage, response times, and error rates
2. **Logging**: Log all API requests and responses for debugging
3. **Tracing**: Use distributed tracing for request correlation
4. **Health Checks**: Implement health check endpoints

## Troubleshooting

### Common Issues

**WebSocket Connection Failures**
- Check WebSocket protocol support
- Verify connection upgrade headers
- Check firewall and proxy settings

**Streaming Performance Issues**
- Adjust buffer sizes based on consumer speed
- Enable compression for large payloads
- Monitor backpressure indicators

**Authentication Problems**
- Verify API key format and validity
- Check bearer token expiration and scopes
- Validate required app roles

### Debugging Tips

1. **Enable Detailed Logging**: Use structured logging for API requests
2. **Monitor Event Streams**: Check event stream health and performance
3. **Test Endpoints**: Use tools like Postman or curl to test endpoints
4. **Check Network**: Verify network connectivity and firewall rules

## See Also

- [Streaming Execution](../concepts/streaming.md) - Real-time execution and event streaming
- [Security and Data Handling](../how-to/security-and-data.md) - API security and authentication
- [Integration and Extensions](../how-to/integration-and-extensions.md) - Core integration patterns
- [API Reference](../api/) - Complete API documentation for REST and streaming types
