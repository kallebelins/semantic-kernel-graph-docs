# REST API Example

This example demonstrates how to expose Semantic Kernel Graph workflows as REST APIs, enabling external systems to execute graphs remotely.

## Objective

Learn how to create REST APIs for graph execution in Semantic Kernel Graph to:
- Expose graph workflows as HTTP endpoints
- Enable remote graph execution with authentication
- Provide graph discovery and management APIs
- Integrate graph execution with web applications
- Support external system integration via HTTP

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- **ASP.NET Core** development experience
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [REST API Integration](../how-to/exposing-rest-apis.md)

## Key Components

### Concepts and Techniques

- **REST API Exposure**: HTTP endpoints for graph execution and management
- **Graph Registry**: Centralized management of available graphs
- **Remote Execution**: Graph execution from external systems
- **Authentication**: API key-based access control
- **Service Integration**: Integration with ASP.NET Core dependency injection

### Core Classes

- `WebApplication`: ASP.NET Core web application host
- `IGraphRegistry`: Registry for managing available graphs
- `IGraphExecutorFactory`: Factory for creating graph executors
- `GraphRestApi`: REST API service for graph operations
- `FunctionGraphNode`: Graph nodes for workflow execution

## Running the Example

### Getting Started

This example demonstrates REST API integration with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Web Application Setup

The example starts by creating an ASP.NET Core web application.

```csharp
public static async Task RunAsync(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    // Logging
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();

    // Services: Kernel + Graph services
    builder.Services.AddKernel().AddGraphSupport(options =>
    {
        options.EnableLogging = true;
        options.EnableMetrics = true;
    });

    // Minimal SK kernel (no real LLM for the example)
    builder.Services.AddSingleton<Kernel>(_ => Kernel.CreateBuilder().Build());

    // Build app
    var app = builder.Build();
```

### 2. Service Resolution

The application resolves required services from the dependency injection container.

```csharp
// Resolve required services
var registry = app.Services.GetRequiredService<IGraphRegistry>();
var factory = app.Services.GetRequiredService<IGraphExecutorFactory>();
var graphApi = app.Services.GetRequiredService<GraphRestApi>();
var kernel = app.Services.GetRequiredService<Kernel>();
```

### 3. Graph Creation and Registration

A simple graph is created and registered for demonstration purposes.

```csharp
// Create a simple graph and register it
var echoFunc = KernelFunctionFactory.CreateFromMethod(
    (string input) => $"echo:{input}",
    functionName: "echo",
    description: "Echoes the input string");

var echoNode = new FunctionGraphNode(echoFunc, nodeId: "echo");
var graph = new GraphExecutor("sample-graph", "Simple echo graph");
graph.AddNode(echoNode).SetStartNode("echo");

await factory.RegisterAsync(graph);
```

### 4. Graph Discovery Endpoint

The API provides an endpoint to list all available graphs.

```csharp
// Endpoints
app.MapGet("/graphs", async () => await graphApi.ListGraphsAsync());
```

### 5. Graph Execution Endpoint

The main endpoint for executing graphs with authentication.

```csharp
app.MapPost("/graphs/execute", async (ExecuteGraphRequest req, HttpContext http) =>
{
    var apiKey = http.Request.Headers["x-api-key"].FirstOrDefault();
    var response = await graphApi.ExecuteAsync(req, apiKey, http.RequestAborted);
    return Results.Json(response);
});
```

### 6. Request Model

The execution request model defines the structure for graph execution requests.

```csharp
public class ExecuteGraphRequest
{
    public string GraphName { get; set; } = string.Empty;
    public Dictionary<string, object> Variables { get; set; } = new();
    public GraphExecutionOptions? Options { get; set; }
}

public class GraphExecutionOptions
{
    public int? MaxSteps { get; set; }
    public TimeSpan? Timeout { get; set; }
    public bool EnableStreaming { get; set; }
    public string? ExecutionId { get; set; }
}
```

### 7. Response Model

The execution response model defines the structure for graph execution results.

```csharp
public class ExecuteGraphResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public object? Result { get; set; }
    public string ExecutionId { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> State { get; set; } = new();
    public List<string> ExecutionPath { get; set; } = new();
}
```

### 8. Authentication and Security

The API implements basic API key authentication for security.

```csharp
// Extract API key from request headers
var apiKey = http.Request.Headers["x-api-key"].FirstOrDefault();

// Validate API key (in production, implement proper validation)
if (string.IsNullOrEmpty(apiKey))
{
    return Results.Unauthorized();
}

// Execute graph with authentication
var response = await graphApi.ExecuteAsync(req, apiKey, http.RequestAborted);
```

### 9. Error Handling

The API includes comprehensive error handling for various failure scenarios.

```csharp
app.MapPost("/graphs/execute", async (ExecuteGraphRequest req, HttpContext http) =>
{
    try
    {
        var apiKey = http.Request.Headers["x-api-key"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(apiKey))
        {
            return Results.Unauthorized(new { error = "API key required" });
        }

        if (string.IsNullOrEmpty(req.GraphName))
        {
            return Results.BadRequest(new { error = "Graph name is required" });
        }

        var response = await graphApi.ExecuteAsync(req, apiKey, http.RequestAborted);
        return Results.Json(response);
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error executing graph {GraphName}", req.GraphName);
        
        return Results.Problem(
            title: "Graph execution failed",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError
        );
    }
});
```

### 10. Graph Management Endpoints

Additional endpoints for comprehensive graph management.

```csharp
// List all available graphs
app.MapGet("/graphs", async () => await graphApi.ListGraphsAsync());

// Get graph metadata
app.MapGet("/graphs/{graphName}", async (string graphName) => 
    await graphApi.GetGraphMetadataAsync(graphName));

// Get graph execution history
app.MapGet("/graphs/{graphName}/history", async (string graphName, 
    [FromQuery] int limit = 10) => 
    await graphApi.GetExecutionHistoryAsync(graphName, limit));

// Cancel running execution
app.MapPost("/graphs/{graphName}/cancel", async (string graphName, 
    [FromBody] CancelExecutionRequest req) => 
    await graphApi.CancelExecutionAsync(graphName, req.ExecutionId));
```

### 11. Streaming Support

The API can support streaming execution for long-running graphs.

```csharp
// Streaming execution endpoint
app.MapPost("/graphs/{graphName}/stream", async (string graphName, 
    ExecuteGraphRequest req, HttpContext http) =>
{
    var apiKey = http.Request.Headers["x-api-key"].FirstOrDefault();
    
    if (string.IsNullOrEmpty(apiKey))
        return Results.Unauthorized();

    // Set up streaming response
    http.Response.Headers.Add("Content-Type", "text/event-stream");
    http.Response.Headers.Add("Cache-Control", "no-cache");
    http.Response.Headers.Add("Connection", "keep-alive");

    var response = http.Response;
    var writer = new StreamWriter(response.Body);

    try
    {
        await foreach (var event in graphApi.ExecuteStreamingAsync(req, apiKey, http.RequestAborted))
        {
            var eventData = $"data: {JsonSerializer.Serialize(event)}\n\n";
            await writer.WriteAsync(eventData);
            await writer.FlushAsync();
        }
    }
    catch (Exception ex)
    {
        var errorEvent = $"data: {JsonSerializer.Serialize(new { error = ex.Message })}\n\n";
        await writer.WriteAsync(errorEvent);
        await writer.FlushAsync();
    }

    return Results.Empty;
});
```

### 12. Configuration and Options

The API supports various configuration options for customization.

```csharp
// Configure graph support with options
builder.Services.AddKernel().AddGraphSupport(options =>
{
    options.EnableLogging = true;
    options.EnableMetrics = true;
    options.EnableCheckpointing = true;
    options.EnableStreaming = true;
    options.MaxConcurrentExecutions = 10;
    options.DefaultTimeout = TimeSpan.FromMinutes(5);
});

// Configure REST API options
builder.Services.Configure<GraphRestApiOptions>(options =>
{
    options.EnableAuthentication = true;
    options.RequireApiKey = true;
    options.MaxRequestSize = 1024 * 1024; // 1MB
    options.EnableCors = true;
    options.CorsOrigins = new[] { "http://localhost:3000", "https://myapp.com" };
});
```

## Expected Output

The example produces a running web server with:

- 🌐 Web application running on http://localhost:5000
- 📋 GET /graphs endpoint for graph discovery
- 🚀 POST /graphs/execute endpoint for graph execution
- 🔐 API key authentication support
- 📊 Graph execution results and error handling
- ✅ Complete REST API for graph management

## API Usage Examples

### List Available Graphs

```bash
curl -X GET http://localhost:5000/graphs
```

### Execute a Graph

```bash
curl -X POST http://localhost:5000/graphs/execute \
  -H "Content-Type: application/json" \
  -H "x-api-key: your-api-key" \
  -d '{
    "graphName": "sample-graph",
    "variables": {
      "input": "Hello, World!"
    }
  }'
```

### Get Graph Metadata

```bash
curl -X GET http://localhost:5000/graphs/sample-graph
```

## Troubleshooting

### Common Issues

1. **Service Resolution Failures**: Ensure all required services are registered in DI container
2. **Authentication Errors**: Verify API key is provided in request headers
3. **Graph Registration Issues**: Check that graphs are properly registered before API exposure
4. **Execution Failures**: Monitor graph execution logs and error responses

### Debugging Tips

- Enable detailed logging for service resolution and graph execution
- Monitor HTTP request/response logs for API debugging
- Verify graph registration and availability in the registry
- Check authentication and authorization configuration

## See Also

- [Exposing REST APIs](../how-to/exposing-rest-apis.md)
- [Graph Registry](../concepts/graph-registry.md)
- [Service Integration](../how-to/integration-and-extensions.md)
- [Authentication and Security](../how-to/security-and-data.md)
- [Streaming Execution](../concepts/streaming.md)
