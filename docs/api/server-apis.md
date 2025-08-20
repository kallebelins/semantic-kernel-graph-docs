# Server and APIs

SemanticKernel.Graph provides a comprehensive server infrastructure with REST APIs, authentication, authorization, rate limiting, webhooks, and GraphQL subscriptions. This reference covers the complete server ecosystem for exposing graph functionality to external services and applications.

## GraphRestApi

Framework-agnostic service layer for exposing graph execution and inspection over REST-like APIs. Can be adapted to ASP.NET Minimal APIs, Controllers, or any HTTP framework.

### Constructor

```csharp
public GraphRestApi(
    IGraphRegistry registry,
    IServiceProvider serviceProvider,
    GraphRestApiOptions? options = null,
    ILogger<GraphRestApi>? logger = null,
    IGraphTelemetry? telemetry = null)
```

**Parameters:**
* `registry`: Graph registry for managing graph definitions
* `serviceProvider`: Dependency injection service provider
* `options`: Configuration options for API behavior
* `logger`: Logger instance for diagnostics
* `telemetry`: Optional telemetry service for metrics

### Methods

#### Graph Management

```csharp
/// <summary>
/// Lists registered graphs.
/// </summary>
public Task<IReadOnlyList<RegisteredGraphInfo>> ListGraphsAsync()

/// <summary>
/// Gets information about a specific graph.
/// </summary>
public Task<RegisteredGraphInfo?> GetGraphAsync(string graphName)
```

#### Graph Execution

```csharp
/// <summary>
/// Executes a graph request and returns a structured response.
/// </summary>
public Task<ExecuteGraphResponse> ExecuteAsync(
    ExecuteGraphRequest request, 
    string? apiKeyHeader = null, 
    CancellationToken cancellationToken = default)

/// <summary>
/// Enqueues an execution for background processing.
/// </summary>
public Task<EnqueueExecutionResponse> EnqueueAsync(
    EnqueueExecutionRequest request, 
    string? apiKeyHeader = null, 
    CancellationToken cancellationToken = default)
```

#### Execution Monitoring

```csharp
/// <summary>
/// Lists active executions with pagination.
/// </summary>
public Task<ExecutionPageResponse> ListActiveExecutionsAsync(ExecutionPageRequest page)

/// <summary>
/// Gets execution status and results.
/// </summary>
public Task<ExecutionStatusResponse?> GetExecutionStatusAsync(string executionId)

/// <summary>
/// Cancels an active execution.
/// </summary>
public Task<bool> CancelExecutionAsync(string executionId)
```

#### Graph Inspection

```csharp
/// <summary>
/// Gets graph structure and metadata.
/// </summary>
public Task<GraphStructureResponse?> GetGraphStructureAsync(string graphName)

/// <summary>
/// Gets execution metrics and performance data.
/// </summary>
public Task<ExecutionMetricsResponse?> GetExecutionMetricsAsync(string executionId)
```

### Key Features

* **Framework Agnostic**: Adapts to any HTTP framework (ASP.NET Core, Minimal APIs, etc.)
* **Authentication**: API key and bearer token authentication
* **Rate Limiting**: Configurable request throttling and quotas
* **Execution Queue**: Background processing with priority scheduling
* **Idempotency**: Safe request retry and deduplication
* **Security Context**: Request correlation and tenant isolation
* **Human-in-the-Loop**: Integration with HITL workflows

## GraphRestApiOptions

Configuration options for REST API behavior including authentication, rate limiting, and execution control.

### Authentication Options

```csharp
public sealed class GraphRestApiOptions
{
    /// <summary>
    /// Optional API key for simple header-based authentication ("x-api-key").
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// When true, requests must present valid authentication according to the enabled mechanisms.
    /// </summary>
    public bool RequireAuthentication { get; set; } = false;

    /// <summary>
    /// Enables bearer token authentication (e.g., Azure AD). Requires a validator to be registered in DI.
    /// </summary>
    public bool EnableBearerTokenAuth { get; set; } = false;

    /// <summary>
    /// Optional list of required OAuth scopes for bearer token auth.
    /// </summary>
    public string[]? RequiredScopes { get; set; }

    /// <summary>
    /// Optional list of required application roles for bearer token auth.
    /// </summary>
    public string[]? RequiredAppRoles { get; set; }
}
```

### Concurrency and Performance

```csharp
/// <summary>
/// Gets or sets the maximum concurrent executions allowed per process.
/// </summary>
public int MaxConcurrentExecutions { get; set; } = 64;

/// <summary>
/// Gets or sets the default execution timeout.
/// </summary>
public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromMinutes(2);

/// <summary>
/// Enables lightweight request logging.
/// </summary>
public bool EnableRequestLogging { get; set; } = true;
```

### Rate Limiting

```csharp
/// <summary>
/// Enables rate limiting for API requests.
/// </summary>
public bool EnableRateLimiting { get; set; } = false;

/// <summary>
/// Gets or sets the rate limit window duration.
/// </summary>
public TimeSpan RateLimitWindow { get; set; } = TimeSpan.FromMinutes(1);

/// <summary>
/// Gets or sets the global requests per window limit.
/// </summary>
public int GlobalRequestsPerWindow { get; set; } = 1000;

/// <summary>
/// Gets or sets the per API key requests per window limit.
/// </summary>
public int PerApiKeyRequestsPerWindow { get; set; } = 100;

/// <summary>
/// Gets or sets the per tenant requests per window limit.
/// </summary>
public int PerTenantRequestsPerWindow { get; set; } = 50;
```

### Execution Queue

```csharp
/// <summary>
/// Enables execution queue for background processing.
/// </summary>
public bool EnableExecutionQueue { get; set; } = false;

/// <summary>
/// Gets or sets the maximum queue length.
/// </summary>
public int QueueMaxLength { get; set; } = 1000;

/// <summary>
/// Gets or sets the queue processing interval.
/// </summary>
public TimeSpan QueueProcessingInterval { get; set; } = TimeSpan.FromSeconds(1);
```

### Idempotency

```csharp
/// <summary>
/// Enables idempotency for safe request retry.
/// </summary>
public bool EnableIdempotency { get; set; } = false;

/// <summary>
/// Gets or sets the idempotency window duration.
/// </summary>
public TimeSpan IdempotencyWindow { get; set; } = TimeSpan.FromMinutes(10);

/// <summary>
/// Gets or sets the maximum number of idempotency entries to cache.
/// </summary>
public int MaxIdempotencyEntries { get; set; } = 10000;
```

### Security Context

```csharp
/// <summary>
/// Enables security context enrichment for request correlation.
/// </summary>
public bool EnableSecurityContextEnrichment { get; set; } = false;

/// <summary>
/// Gets or sets the correlation ID header name.
/// </summary>
public string CorrelationIdHeaderName { get; set; } = "X-Correlation-Id";

/// <summary>
/// Gets or sets the tenant ID header name.
/// </summary>
public string TenantIdHeaderName { get; set; } = "X-Tenant-Id";
```

## Authentication and Authorization

### API Key Authentication

Simple header-based authentication using the `x-api-key` header:

```csharp
var options = new GraphRestApiOptions
{
    ApiKey = "your-secure-api-key-12345",
    RequireAuthentication = true
};

// In HTTP request
// GET /graphs
// x-api-key: your-secure-api-key-12345
```

### Bearer Token Authentication

OAuth 2.0 bearer token authentication with scope and role validation:

```csharp
var options = new GraphRestApiOptions
{
    RequireAuthentication = true,
    EnableBearerTokenAuth = true,
    RequiredScopes = new[] { "graph.execute", "graph.read" },
    RequiredAppRoles = new[] { "GraphUser", "GraphAdmin" }
};

// Register validator in DI
builder.Services.AddSingleton<IBearerTokenValidator, AzureAdBearerTokenValidator>();

// In HTTP request
// GET /graphs
// Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1Ni...
```

### IBearerTokenValidator

Interface for validating bearer tokens with scope and role checks:

```csharp
public interface IBearerTokenValidator
{
    /// <summary>
    /// Validates the incoming bearer token according to implementation.
    /// </summary>
    /// <param name="bearerToken">Bearer token string without the "Bearer " prefix</param>
    /// <param name="requiredScopes">Optional required OAuth scopes</param>
    /// <param name="requiredAppRoles">Optional required application roles</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True when token is valid and contains required claims</returns>
    Task<bool> ValidateAsync(
        string bearerToken, 
        IEnumerable<string>? requiredScopes = null, 
        IEnumerable<string>? requiredAppRoles = null, 
        CancellationToken cancellationToken = default);
}
```

### AzureAdBearerTokenValidator

Azure AD JWT validator implementation:

```csharp
public sealed class AzureAdBearerTokenValidator : IBearerTokenValidator
{
    public Task<bool> ValidateAsync(
        string bearerToken, 
        IEnumerable<string>? requiredScopes = null, 
        IEnumerable<string>? requiredAppRoles = null, 
        CancellationToken cancellationToken = default)
    {
        // Validates JWT token claims, scopes, and app roles
        // Performs basic time validity checks (nbf, exp)
        // Note: Signature validation should be handled by hosting framework
    }
}
```

## Rate Limiting

### Rate Limiting Configuration

Configure rate limiting at multiple levels:

```csharp
var options = new GraphRestApiOptions
{
    EnableRateLimiting = true,
    RateLimitWindow = TimeSpan.FromMinutes(1),
    
    // Global rate limit
    GlobalRequestsPerWindow = 1000,
    
    // Per API key rate limit
    PerApiKeyRequestsPerWindow = 100,
    
    // Per tenant rate limit
    PerTenantRequestsPerWindow = 50
};
```

### Rate Limiting Implementation

The API implements sliding window rate limiting:

* **Global Limits**: Overall API request rate across all clients
* **Per API Key**: Rate limits per individual API key
* **Per Tenant**: Rate limits per tenant/organization
* **Sliding Window**: Rolling time window for accurate rate calculation
* **Automatic Cleanup**: Expired entries are automatically removed

### Rate Limit Responses

When rate limits are exceeded:

```json
{
  "executionId": "",
  "graphName": "my-workflow",
  "success": false,
  "error": "Rate limit exceeded (api key)"
}
```

## Webhooks

### Webhook Configuration

Configure webhooks for external service notifications:

```csharp
public sealed class WebhookConfiguration
{
    /// <summary>
    /// Gets or sets the webhook URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the webhook secret for signature validation.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event types to send to this webhook.
    /// </summary>
    public GraphExecutionEventType[] EventTypes { get; set; } = Array.Empty<GraphExecutionEventType>();

    /// <summary>
    /// Gets or sets the retry interval for failed deliveries.
    /// </summary>
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}
```

### Webhook Service

Service for managing webhook deliveries:

```csharp
public sealed class WebhookService
{
    /// <summary>
    /// Registers a new webhook configuration.
    /// </summary>
    public void RegisterWebhook(WebhookConfiguration configuration);

    /// <summary>
    /// Notifies all registered webhooks of an event.
    /// </summary>
    public Task NotifyWebhooksAsync(GraphExecutionEvent @event);

    /// <summary>
    /// Removes a webhook configuration.
    /// </summary>
    public void RemoveWebhook(string url);
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

### Webhook Security

Webhooks include security features:

* **Secret Validation**: HMAC-SHA256 signature validation
* **Retry Logic**: Configurable retry intervals and maximum attempts
* **Event Filtering**: Selective event type delivery
* **Error Handling**: Graceful failure handling and logging

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
```

### GraphQL Subscription Service

Service for managing GraphQL subscriptions:

```csharp
public sealed class GraphQLSubscriptionService
{
    private readonly IGraphExecutionEventStream _eventStream;
    private readonly ILogger<GraphQLSubscriptionService> _logger;
    
    /// <summary>
    /// Subscribes to execution events with optional filtering.
    /// </summary>
    public IAsyncEnumerable<GraphExecutionEvent> SubscribeToExecutionEvents(
        string? executionId = null,
        string? nodeId = null,
        GraphExecutionEventType[]? eventTypes = null);

    /// <summary>
    /// Subscribes to node status updates.
    /// </summary>
    public IAsyncEnumerable<NodeStatus> SubscribeToNodeStatus(
        string executionId, 
        string nodeId);

    /// <summary>
    /// Subscribes to execution progress updates.
    /// </summary>
    public IAsyncEnumerable<ExecutionProgress> SubscribeToExecutionProgress(
        string executionId);
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
}
```

## Request and Response Models

### ExecuteGraphRequest

```csharp
public sealed class ExecuteGraphRequest
{
    /// <summary>
    /// Gets or sets the name of the graph to execute.
    /// </summary>
    public required string GraphName { get; init; }

    /// <summary>
    /// Gets or sets the input variables for the graph execution.
    /// </summary>
    public Dictionary<string, object> Variables { get; init; } = new();

    /// <summary>
    /// Gets or sets the optional idempotency key for safe retry.
    /// </summary>
    public string? IdempotencyKey { get; init; }

    /// <summary>
    /// Gets or sets the execution priority (higher values = higher priority).
    /// </summary>
    public int Priority { get; init; } = 0;

    /// <summary>
    /// Gets or sets the execution timeout.
    /// </summary>
    public TimeSpan? Timeout { get; init; }
}
```

### ExecuteGraphResponse

```csharp
public sealed class ExecuteGraphResponse
{
    /// <summary>
    /// Gets or sets the unique execution identifier.
    /// </summary>
    public required string ExecutionId { get; init; }

    /// <summary>
    /// Gets or sets the name of the executed graph.
    /// </summary>
    public required string GraphName { get; init; }

    /// <summary>
    /// Gets or sets whether the execution was successful.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Gets or sets the execution result data.
    /// </summary>
    public object? Result { get; init; }

    /// <summary>
    /// Gets or sets the error message if execution failed.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Gets or sets the execution duration.
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// Gets or sets the execution status.
    /// </summary>
    public string Status { get; init; } = "Unknown";
}
```

### EnqueueExecutionRequest

```csharp
public sealed class EnqueueExecutionRequest
{
    /// <summary>
    /// Gets or sets the name of the graph to execute.
    /// </summary>
    public required string GraphName { get; init; }

    /// <summary>
    /// Gets or sets the input variables for the graph execution.
    /// </summary>
    public Dictionary<string, object> Variables { get; init; } = new();

    /// <summary>
    /// Gets or sets the execution priority (higher values = higher priority).
    /// </summary>
    public int Priority { get; init; } = 0;

    /// <summary>
    /// Gets or sets the optional idempotency key.
    /// </summary>
    public string? IdempotencyKey { get; init; }

    /// <summary>
    /// Gets or sets the webhook URL for completion notification.
    /// </summary>
    public string? CompletionWebhookUrl { get; init; }
}
```

## Usage Examples

### Basic REST API Setup

```csharp
using SemanticKernel.Graph.Integration;

// Create REST API with options
var apiOptions = new GraphRestApiOptions
{
    ApiKey = "your-api-key",
    MaxConcurrentExecutions = 32,
    EnableRateLimiting = true,
    EnableExecutionQueue = true,
    RequireAuthentication = true
};

var graphApi = new GraphRestApi(
    registry: graphRegistry,
    serviceProvider: serviceProvider,
    options: apiOptions
);

// Execute graph
var request = new ExecuteGraphRequest
{
    GraphName = "my-workflow",
    Variables = new Dictionary<string, object>
    {
        ["input"] = "Hello, World!",
        ["maxIterations"] = 5
    }
};

var response = await graphApi.ExecuteAsync(request);
Console.WriteLine($"Execution ID: {response.ExecutionId}");
```

### ASP.NET Core Integration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<IGraphRegistry, GraphRegistry>();
builder.Services.AddSingleton<IBearerTokenValidator, AzureAdBearerTokenValidator>();

var app = builder.Build();

// Configure GraphRestApi
var apiOptions = new GraphRestApiOptions
{
    RequireAuthentication = true,
    EnableBearerTokenAuth = true,
    RequiredScopes = new[] { "graph.execute", "graph.read" },
    EnableRateLimiting = true,
    GlobalRequestsPerWindow = 1000,
    PerApiKeyRequestsPerWindow = 100
};

var graphApi = new GraphRestApi(
    registry: app.Services.GetRequiredService<IGraphRegistry>(),
    serviceProvider: app.Services,
    options: apiOptions
);

// Define endpoints
app.MapGet("/graphs", async () => await graphApi.ListGraphsAsync());

app.MapPost("/graphs/execute", async (ExecuteGraphRequest req, HttpContext http) =>
{
    var apiKey = http.Request.Headers["x-api-key"].FirstOrDefault();
    var response = await graphApi.ExecuteAsync(req, apiKey, http.RequestAborted);
    return Results.Json(response);
});

app.MapGet("/graphs/executions/{executionId}", async (string executionId) =>
{
    var status = await graphApi.GetExecutionStatusAsync(executionId);
    return status != null ? Results.Json(status) : Results.NotFound();
});
```

### Webhook Integration

```csharp
// Configure webhooks
var webhookService = new WebhookService(httpClient, logger);

webhookService.RegisterWebhook(new WebhookConfiguration
{
    Url = "https://external-service.com/webhook",
    Secret = "webhook-secret-123",
    EventTypes = new[] 
    { 
        GraphExecutionEventType.ExecutionCompleted,
        GraphExecutionEventType.NodeFailed 
    },
    RetryInterval = TimeSpan.FromMinutes(2),
    MaxRetries = 5
});

// Execute with webhook notification
var request = new EnqueueExecutionRequest
{
    GraphName = "my-workflow",
    Variables = new Dictionary<string, object> { ["input"] = "data" },
    CompletionWebhookUrl = "https://my-service.com/completion"
};

var response = await graphApi.EnqueueAsync(request);
```

### GraphQL Subscriptions

```csharp
// Create subscription service
var subscriptionService = new GraphQLSubscriptionService(eventStream, logger);

// Subscribe to execution events
await foreach (var @event in subscriptionService.SubscribeToExecutionEvents(
    executionId: "exec-123",
    eventTypes: new[] { GraphExecutionEventType.NodeCompleted }))
{
    Console.WriteLine($"Node {@event.NodeId} completed at {@event.Timestamp}");
}

// Subscribe to execution progress
await foreach (var progress in subscriptionService.SubscribeToExecutionProgress("exec-123"))
{
    Console.WriteLine($"Progress: {progress.Progress:P0} ({progress.CompletedNodes}/{progress.TotalNodes})");
}
```

## Performance Considerations

* **Connection Pooling**: Use connection pooling for external service calls
* **Event Batching**: Batch events when possible to reduce overhead
* **Compression**: Enable compression for large event payloads
* **Caching**: Cache frequently accessed graph definitions and results
* **Async Operations**: Use async methods for all I/O operations
* **Rate Limiting**: Configure appropriate rate limits for your use case

## Security Considerations

* **HTTPS**: Always use HTTPS in production
* **API Keys**: Use strong, randomly generated API keys
* **Bearer Tokens**: Implement proper JWT validation and scope checking
* **Input Validation**: Validate all input parameters and sanitize data
* **Rate Limiting**: Implement appropriate rate limiting per client/tenant
* **Webhook Security**: Use secrets for webhook signature validation

## Monitoring and Observability

* **Metrics**: Track API usage, response times, and error rates
* **Logging**: Log all API requests and responses for debugging
* **Tracing**: Use distributed tracing for request correlation
* **Health Checks**: Implement health check endpoints
* **Alerting**: Set up alerts for rate limit violations and errors

## See Also

* [Exposing REST APIs](../how-to/exposing-rest-apis.md) - Guide for implementing REST APIs
* [Server and APIs](../how-to/server-and-apis.md) - Complete server implementation guide
* [Security and Data](../how-to/security-and-data.md) - Security best practices
* [Streaming Execution](../concepts/streaming.md) - Real-time execution concepts
* [Human-in-the-Loop](../how-to/hitl.md) - HITL integration patterns
