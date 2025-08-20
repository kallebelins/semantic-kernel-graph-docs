# Server and APIs

This guide explains how to expose SemanticKernel.Graph functionality through REST APIs, streaming endpoints, and integration patterns for external services and applications. The framework provides a comprehensive API layer that enables external systems to execute graphs remotely, monitor execution in real-time, and integrate with event-driven architectures.

## Overview

SemanticKernel.Graph provides a robust API infrastructure that enables:

* **Remote graph execution** through REST endpoints with authentication and rate limiting
* **Real-time streaming** of execution events via WebSockets, Server-Sent Events, or HTTP polling
* **Execution monitoring** and asynchronous result retrieval
* **Framework-agnostic design** that adapts to any HTTP framework (ASP.NET Core, Minimal APIs, etc.)
* **Enterprise features** including idempotency, queuing, and security context enrichment

## Core REST API Components

### GraphRestApi: Main API Service

The `GraphRestApi` class provides a framework-agnostic service layer that can be adapted to any HTTP framework:

```csharp
using SemanticKernel.Graph.Integration;

var api = new GraphRestApi(
    registry: graphRegistry,
    serviceProvider: serviceProvider,
    options: new GraphRestApiOptions
    {
        ApiKey = "your-api-key",
        MaxConcurrentExecutions = 32,
        EnableRateLimiting = true,
        EnableExecutionQueue = true
    }
);
```

**Key Capabilities:**
* **Graph Execution**: Execute graphs with parameters and retrieve results
* **Graph Management**: List, register, and manage graph definitions
* **Execution Monitoring**: Track execution status and retrieve results
* **Security**: API key and bearer token authentication
* **Rate Limiting**: Configurable request throttling and quotas
* **Execution Queue**: Background processing with priority scheduling
* **Idempotency**: Safe request retry and deduplication

### API Configuration Options

Configure the REST API behavior through `GraphRestApiOptions`:

```csharp
var apiOptions = new GraphRestApiOptions
{
    // Authentication
    ApiKey = "your-api-key",
    RequireAuthentication = true,
    EnableBearerTokenAuth = true,
    
    // Concurrency and Performance
    MaxConcurrentExecutions = 64,
    DefaultTimeout = TimeSpan.FromMinutes(5),
    
    // Rate Limiting
    EnableRateLimiting = true,
    GlobalRequestsPerWindow = 1000,
    PerApiKeyRequestsPerWindow = 100,
    RateLimitWindow = TimeSpan.FromMinutes(1),
    
    // Execution Queue
    EnableExecutionQueue = true,
    QueueMaxLength = 1000,
    
    // Idempotency
    EnableIdempotency = true,
    IdempotencyWindow = TimeSpan.FromMinutes(10),
    
    // Security Context
    EnableSecurityContextEnrichment = true,
    CorrelationIdHeaderName = "X-Correlation-Id"
};
```

## REST API Endpoints

### Basic Graph Operations

```csharp
// List registered graphs
app.MapGet("/graphs", async () => await graphApi.ListGraphsAsync());

// Execute a graph
app.MapPost("/graphs/execute", async (ExecuteGraphRequest req, HttpContext http) =>
{
    var apiKey = http.Request.Headers["x-api-key"].FirstOrDefault();
    var response = await graphApi.ExecuteAsync(req, apiKey, http.RequestAborted);
    return Results.Json(response);
});

// Enqueue execution for background processing
app.MapPost("/graphs/enqueue", async (EnqueueExecutionRequest req, HttpContext http) =>
{
    var apiKey = http.Request.Headers["x-api-key"].FirstOrDefault();
    var response = await graphApi.EnqueueAsync(req, apiKey, http.RequestAborted);
    return Results.Json(response);
});

// List active executions
app.MapGet("/graphs/executions", (ExecutionPageRequest page) => 
    graphApi.ListActiveExecutions(page));
```

### Authentication and Security

The API supports multiple authentication mechanisms:

```csharp
// API Key Authentication
var options = new GraphRestApiOptions
{
    ApiKey = "your-secret-key",
    RequireAuthentication = true
};

// Bearer Token Authentication (Azure AD, etc.)
var options = new GraphRestApiOptions
{
    EnableBearerTokenAuth = true,
    RequiredScopes = new[] { "api://your-app/.default" },
    RequiredAppRoles = new[] { "GraphExecutor" }
};

// Security Context Enrichment
var options = new GraphRestApiOptions
{
    EnableSecurityContextEnrichment = true,
    CorrelationIdHeaderName = "X-Correlation-Id"
};
```

## Streaming Execution

### WebSocket Streaming

Implement bidirectional WebSocket streaming for real-time execution monitoring:

```csharp
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

### Server-Sent Events

Implement unidirectional streaming with Server-Sent Events:

```csharp
app.MapGet("/graphs/{graphId}/stream/sse", async (string graphId, HttpContext http) =>
{
    http.Response.Headers.Add("Content-Type", "text/event-stream");
    http.Response.Headers.Add("Cache-Control", "no-cache");
    http.Response.Headers.Add("Connection", "keep-alive");
    
    var executor = GetExecutor(graphId);
    var arguments = new KernelArguments();
    
    var options = new StreamingExecutionOptions
    {
        BufferSize = 10,
        EnableEventCompression = true,
        ProducerBatchSize = 1,
        ProducerFlushInterval = TimeSpan.FromMilliseconds(50)
    };
    
    var eventStream = executor.ExecuteStreamAsync(kernel, arguments, options);
    
    await foreach (var @event in eventStream)
    {
        var eventData = JsonSerializer.Serialize(@event);
        var sseMessage = $"data: {eventData}\n\n";
        
        await http.Response.WriteAsync(sseMessage, http.RequestAborted);
        await http.Response.Body.FlushAsync(http.RequestAborted);
        
        if (http.RequestAborted.IsCancellationRequested)
            break;
    }
});
```

### HTTP Polling

For clients that don't support streaming, implement polling-based event retrieval:

```csharp
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

## Execution Queue and Background Processing

### Queue Configuration

Enable background execution processing for long-running operations:

```csharp
var options = new GraphRestApiOptions
{
    EnableExecutionQueue = true,
    QueueMaxLength = 1000,
    QueuePollInterval = TimeSpan.FromMilliseconds(25)
};

// Enqueue execution
var enqueueRequest = new EnqueueExecutionRequest
{
    GraphName = "data-processing-graph",
    Arguments = new Dictionary<string, object>
    {
        ["inputFile"] = "large-dataset.csv",
        ["priority"] = 1
    },
    Priority = 1
};

var enqueueResponse = await graphApi.EnqueueAsync(enqueueRequest);
```

### Priority Scheduling

The execution queue supports priority-based scheduling:

```csharp
// High priority execution
var highPriorityRequest = new EnqueueExecutionRequest
{
    GraphName = "urgent-analysis",
    Arguments = new Dictionary<string, object>(),
    Priority = 10  // Higher number = higher priority
};

// Low priority batch processing
var batchRequest = new EnqueueExecutionRequest
{
    GraphName = "batch-processing",
    Arguments = new Dictionary<string, object>(),
    Priority = 1   // Lower number = lower priority
};
```

## Rate Limiting and Throttling

### Global Rate Limiting

Configure application-wide request limits:

```csharp
var options = new GraphRestApiOptions
{
    EnableRateLimiting = true,
    GlobalRequestsPerWindow = 1000,
    RateLimitWindow = TimeSpan.FromMinutes(1)
};
```

### Per-API Key Limits

Implement per-client rate limiting:

```csharp
var options = new GraphRestApiOptions
{
    EnableRateLimiting = true,
    PerApiKeyRequestsPerWindow = 100,
    RateLimitWindow = TimeSpan.FromMinutes(1)
};
```

### Per-Tenant Limits

Support multi-tenant rate limiting:

```csharp
var options = new GraphRestApiOptions
{
    EnableRateLimiting = true,
    PerTenantRequestsPerWindow = 500,
    RateLimitWindow = TimeSpan.FromMinutes(1)
};

// Provide tenant context in requests
var securityContext = new ApiRequestSecurityContext
{
    TenantId = "tenant-123",
    ApiKeyHeader = "api-key-456"
};

var response = await graphApi.ExecuteWithSecurityAsync(request, securityContext);
```

## Idempotency and Request Safety

### Idempotency Configuration

Enable safe request retry and deduplication:

```csharp
var options = new GraphRestApiOptions
{
    EnableIdempotency = true,
    IdempotencyWindow = TimeSpan.FromMinutes(10),
    MaxIdempotencyEntries = 10000
};
```

### Using Idempotency Keys

```csharp
// Client provides idempotency key
var request = new ExecuteGraphRequest
{
    GraphName = "data-processing",
    Arguments = new Dictionary<string, object>(),
    IdempotencyKey = "unique-request-id-123"
};

// Or via header
http.Request.Headers.Add("Idempotency-Key", "unique-request-id-123");
```

## Integration Examples

### Minimal API Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddKernel().AddGraphSupport();
builder.Services.AddSingleton<GraphRestApi>();

var app = builder.Build();

// Configure REST API
var graphApi = app.Services.GetRequiredService<GraphRestApi>();

// REST endpoints
app.MapGet("/graphs", () => graphApi.ListGraphsAsync());
app.MapPost("/graphs/execute", async (ExecuteGraphRequest req) => 
    await graphApi.ExecuteAsync(req));

// Streaming endpoints
app.MapGet("/graphs/{graphId}/stream/ws", HandleWebSocketStreaming);
app.MapGet("/graphs/{graphId}/stream/sse", HandleServerSentEvents);

app.Run();
```

### ASP.NET Core Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class GraphExecutionController : ControllerBase
{
    private readonly GraphRestApi _graphApi;
    
    public GraphExecutionController(GraphRestApi graphApi)
    {
        _graphApi = graphApi;
    }
    
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteAsync([FromBody] ExecuteGraphRequest request)
    {
        var apiKey = Request.Headers["x-api-key"].FirstOrDefault();
        var response = await _graphApi.ExecuteAsync(request, apiKey, RequestAborted);
        return Ok(response);
    }
    
    [HttpGet("stream/{graphId}/sse")]
    public async Task<IActionResult> StreamAsync(string graphId)
    {
        Response.Headers.Add("Content-Type", "text/event-stream");
        Response.Headers.Add("Cache-Control", "no-cache");
        
        var executor = GetExecutor(graphId);
        var eventStream = executor.ExecuteStreamAsync(kernel, new KernelArguments());
        
        await foreach (var @event in eventStream)
        {
            var eventData = JsonSerializer.Serialize(@event);
            var sseMessage = $"data: {eventData}\n\n";
            
            await Response.WriteAsync(sseMessage, RequestAborted);
            await Response.Body.FlushAsync(RequestAborted);
            
            if (RequestAborted.IsCancellationRequested)
                break;
        }
        
        return new EmptyResult();
    }
}
```

## Best Practices

### Performance Optimization

1. **Buffer Sizing**: Choose appropriate buffer sizes based on consumer processing speed
2. **Compression**: Enable event compression for high-volume streaming
3. **Batch Processing**: Use producer batching for efficient event delivery
4. **Connection Management**: Implement proper WebSocket lifecycle management

### Security Considerations

1. **API Key Rotation**: Regularly rotate API keys and use secure storage
2. **Rate Limiting**: Implement appropriate limits to prevent abuse
3. **Input Validation**: Validate all input parameters and sanitize data
4. **HTTPS**: Always use HTTPS in production environments

### Monitoring and Observability

1. **Request Logging**: Enable request logging for debugging and audit trails
2. **Metrics Collection**: Use built-in metrics for performance monitoring
3. **Correlation IDs**: Include correlation IDs for request tracing
4. **Health Checks**: Implement health check endpoints for load balancers

### Error Handling

1. **Graceful Degradation**: Handle streaming failures gracefully
2. **Retry Logic**: Implement exponential backoff for transient failures
3. **Circuit Breaker**: Use circuit breaker patterns for external dependencies
4. **User Feedback**: Provide meaningful error messages to API consumers

## Concepts and Techniques

**GraphRestApi**: Framework-agnostic service layer that provides REST API functionality for graph execution, management, and monitoring. It handles authentication, rate limiting, execution queuing, and idempotency.

**StreamingExecutionOptions**: Configuration object that controls streaming behavior including buffer sizes, compression settings, and event filtering.

**GraphExecutionEventStream**: Async enumerable stream that provides real-time access to graph execution events during processing.

**ApiRequestSecurityContext**: Security context object that encapsulates authentication and authorization information for API requests.

**IdempotencyEntry**: Cache entry that ensures duplicate requests return the same response, preventing unintended side effects.

## See Also

* [Streaming Execution](streaming-quickstart.md) - Learn about streaming execution patterns
* [Error Handling and Resilience](error-handling-and-resilience.md) - Understand error handling strategies
* [Security and Data](security-and-data.md) - Security best practices and data protection
* [REST Tools Integration](rest-tools-integration.md) - Integrate external REST APIs into your graphs
* [Examples: RestApiExample](../examples/rest-api-example.md) - Complete working example of REST API integration
