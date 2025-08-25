# REST Tools Integration

SemanticKernel.Graph provides comprehensive integration capabilities for external REST APIs and tools, enabling you to seamlessly incorporate external services into your graph workflows. This guide covers the complete REST tools ecosystem including schema definition, validation, caching, and idempotency.

## Overview

The REST tools integration system consists of several key components:

* **RestToolSchema**: Defines the structure and behavior of REST API operations
* **RestToolGraphNode**: Executable graph node that performs HTTP requests
* **IToolRegistry**: Central registry for managing and discovering available tools
* **IToolSchemaConverter**: Converts schemas into executable nodes
* **Built-in caching and idempotency**: Performance optimization and request safety

## Core Components

### RestToolSchema

The `RestToolSchema` class defines the contract for a REST API operation:

```csharp
public sealed class RestToolSchema
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string Description { get; init; } = string.Empty;
    public required Uri BaseUri { get; init; }
    public required string Path { get; init; }
    public required HttpMethod Method { get; init; }
    public string? JsonBodyTemplate { get; init; }
    public Dictionary<string, string> QueryParameters { get; init; } = new();
    public Dictionary<string, string> Headers { get; init; } = new();
    public int TimeoutSeconds { get; init; } = 30;
    public bool CacheEnabled { get; init; } = true;
    public int CacheTtlSeconds { get; init; } = 60;
    public string? TelemetryDependencyName { get; init; }
}
```

**Key Features:**
* **Flexible parameter mapping**: Map query parameters and headers to graph state variables
* **Template support**: JSON body templates with variable substitution
* **Configurable timeouts**: Per-request timeout settings
* **Built-in caching**: Response caching with configurable TTL
* **Telemetry integration**: Dependency tracking for monitoring

### RestToolGraphNode

The `RestToolGraphNode` executes REST operations based on schema definitions:

```csharp
public sealed class RestToolGraphNode : IGraphNode, ITypedSchemaNode
{
    public RestToolGraphNode(
        RestToolSchema schema, 
        HttpClient httpClient, 
        ILogger<RestToolGraphNode>? logger = null, 
        ISecretResolver? secretResolver = null, 
        IGraphTelemetry? telemetry = null);
}
```

**Capabilities:**
* **Input mapping**: Automatically maps graph state to HTTP parameters
* **Response handling**: Parses JSON responses and provides structured output
* **Error handling**: Comprehensive error handling with telemetry
* **Schema validation**: Implements `ITypedSchemaNode` for type safety
* **Secret resolution**: Secure handling of API keys and sensitive data

## Schema Definition

### Basic REST Schema

Define a simple GET operation:

```csharp
var weatherSchema = new RestToolSchema
{
    Id = "weather.get",
    Name = "Get Weather",
    Description = "Retrieve current weather information",
    BaseUri = new Uri("https://api.weatherapi.com"),
    Path = "/v1/current.json",
    Method = HttpMethod.Get,
    QueryParameters = 
    {
        ["q"] = "location",           // Maps to graph state "location"
        ["key"] = "secret:weather_key" // Resolved via secret resolver
    },
    Headers = 
    {
        ["User-Agent"] = ":SemanticKernel.Graph/1.0", // Literal value
        ["X-Correlation-ID"] = "correlation_id"        // Maps to graph state
    },
    CacheEnabled = true,
    CacheTtlSeconds = 300, // 5 minutes
    TimeoutSeconds = 10
};
```

### POST with JSON Body

Define a POST operation with dynamic body content:

```csharp
var orderSchema = new RestToolSchema
{
    Id = "orders.create",
    Name = "Create Order",
    Description = "Create a new order in the system",
    BaseUri = new Uri("https://api.store.com"),
    Path = "/v1/orders",
    Method = HttpMethod.Post,
    JsonBodyTemplate = """
    {
        "customer_id": "{{customer_id}}",
        "items": {{items_json}},
        "shipping_address": {
            "street": "{{street}}",
            "city": "{{city}}",
            "postal_code": "{{postal_code}}"
        }
    }
    """,
    Headers = 
    {
        ["Content-Type"] = ":application/json",
        ["Authorization"] = "secret:store_api_key",
        ["X-Request-ID"] = "request_id"
    },
    CacheEnabled = false, // POST operations typically not cached
    TimeoutSeconds = 30
};
```

### Advanced Schema with Route Parameters

Define operations with dynamic path segments:

```csharp
var userSchema = new RestToolSchema
{
    Id = "users.get",
    Name = "Get User",
    Description = "Retrieve user information by ID",
    BaseUri = new Uri("https://api.users.com"),
    Path = "/v1/users/{user_id}/profile", // Route parameter
    Method = HttpMethod.Get,
    QueryParameters = 
    {
        ["include"] = "include_fields",
        ["format"] = ":json"
    },
    Headers = 
    {
        ["Accept"] = ":application/json",
        ["Authorization"] = "secret:users_api_key"
    }
};
```

## Parameter Mapping

### Query Parameters

Map graph state variables to HTTP query parameters:

```csharp
var schema = new RestToolSchema
{
    // ... other properties
    QueryParameters = 
    {
        ["search"] = "query_text",        // ?search=value_from_state
        ["page"] = "page_number",         // &page=value_from_state
        ["limit"] = "max_results",        // &limit=value_from_state
        ["sort"] = ":name_asc"            // &sort=name_asc (literal)
    }
};

// Graph state
var args = new KernelArguments
{
    ["query_text"] = "semantic kernel",
    ["page_number"] = 1,
    ["max_results"] = 20
};

// Results in: ?search=semantic%20kernel&page=1&limit=20&sort=name_asc
```

### Headers

Map graph state variables to HTTP headers:

```csharp
var schema = new RestToolSchema
{
    // ... other properties
    Headers = 
    {
        ["Authorization"] = "secret:api_key",           // Resolved via secret resolver
        ["X-Correlation-ID"] = "correlation_id",        // From graph state
        ["User-Agent"] = ":MyApp/1.0",                  // Literal value
        ["Accept-Language"] = "language_code",          // From graph state
        ["X-Tenant"] = "tenant_id"                      // From graph state
    }
};
```

**Header Value Types:**
* **Literal values**: Prefixed with `:` (e.g., `":application/json"`)
* **Secret references**: Prefixed with `secret:` (e.g., `"secret:api_key"`)
* **State variables**: No prefix (e.g., `"correlation_id"`)

### JSON Body Templates

Use Handlebars-style templates for dynamic request bodies:

```csharp
var schema = new RestToolSchema
{
    // ... other properties
    JsonBodyTemplate = """
    {
        "user": {
            "id": "{{user_id}}",
            "name": "{{user_name}}",
            "email": "{{user_email}}"
        },
        "preferences": {{preferences_json}},
        "metadata": {
            "created_at": "{{timestamp}}",
            "source": "{{source}}"
        }
    }
    """
};
```

**Template Features:**
* **Variable substitution**: `{{variable_name}}` replaced with graph state values
* **JSON embedding**: `{{json_variable}}` for complex objects
* **Conditional logic**: Support for basic Handlebars expressions
* **Escape handling**: Automatic JSON escaping for safe output

## Tool Registry

### IToolRegistry Interface

The `IToolRegistry` provides centralized tool management:

```csharp
public interface IToolRegistry
{
    Task<bool> RegisterAsync(ToolMetadata metadata, Func<IServiceProvider, IGraphNode> factory);
    Task<bool> UnregisterAsync(string toolId);
    Task<ToolMetadata?> GetAsync(string toolId);
    Task<IGraphNode?> CreateNodeAsync(string toolId, IServiceProvider serviceProvider);
    Task<IReadOnlyList<ToolMetadata>> ListAsync(ToolSearchCriteria? criteria = null);
}
```

### Tool Registration

Register REST tools in the registry:

```csharp
// Create tool metadata
var metadata = new ToolMetadata
{
    Id = "weather.api",
    Name = "Weather API",
    Description = "Weather information services",
    Type = ToolType.Rest,
    Tags = new HashSet<string> { "weather", "external", "api" },
    Version = "1.0.0"
};

// Create factory function
Func<IServiceProvider, IGraphNode> factory = (services) =>
{
    var httpClient = services.GetRequiredService<HttpClient>();
    var logger = services.GetService<ILogger<RestToolGraphNode>>();
    var secretResolver = services.GetService<ISecretResolver>();
    var telemetry = services.GetService<IGraphTelemetry>();
    
    return new RestToolGraphNode(weatherSchema, httpClient, logger, secretResolver, telemetry);
};

// Register the tool
await toolRegistry.RegisterAsync(metadata, factory);
```

### Tool Discovery

Discover and list available tools:

```csharp
// List all tools
var allTools = await toolRegistry.ListAsync();

// Search by criteria
var searchCriteria = new ToolSearchCriteria
{
    SearchText = "weather",
    Type = ToolType.Rest,
    Tags = new HashSet<string> { "api" }
};

var weatherTools = await toolRegistry.ListAsync(searchCriteria);

// Get specific tool
var toolMetadata = await toolRegistry.GetAsync("weather.api");
if (toolMetadata != null)
{
    var toolNode = await toolRegistry.CreateNodeAsync("weather.api", serviceProvider);
    // Use the tool node in your graph
}
```

## Schema Conversion

### IToolSchemaConverter

Convert schemas into executable nodes:

```csharp
public interface IToolSchemaConverter
{
    IGraphNode CreateNode(RestToolSchema schema);
}
```

### RestToolSchemaConverter

Default implementation for REST tools:

```csharp
public sealed class RestToolSchemaConverter : IToolSchemaConverter
{
    public RestToolSchemaConverter(
        HttpClient httpClient, 
        ILogger<RestToolSchemaConverter>? logger = null, 
        ISecretResolver? secretResolver = null, 
        IGraphTelemetry? telemetry = null);
    
    public IGraphNode CreateNode(RestToolSchema schema);
}
```

**Usage:**
```csharp
var converter = new RestToolSchemaConverter(
    httpClient: httpClient,
    logger: logger,
    secretResolver: secretResolver,
    telemetry: telemetry
);

var restNode = converter.CreateNode(weatherSchema);
```

## Caching and Performance

### Response Caching

Enable caching for improved performance:

```csharp
var schema = new RestToolSchema
{
    // ... other properties
    CacheEnabled = true,
    CacheTtlSeconds = 600 // 10 minutes
};
```

**Cache Behavior:**
* **Cache key**: Generated from HTTP method, URL, and request body hash
* **TTL management**: Automatic expiration based on schema configuration
* **Memory efficient**: Uses concurrent dictionary with expiration tracking
* **Thread safe**: Concurrent access support for high-performance scenarios

### Cache Configuration

Configure caching behavior:

```csharp
// Enable caching with custom TTL
var schema = new RestToolSchema
{
    CacheEnabled = true,
    CacheTtlSeconds = 3600, // 1 hour
    // ... other properties
};

// Disable caching for dynamic content
var dynamicSchema = new RestToolSchema
{
    CacheEnabled = false,
    // ... other properties
};
```

## Idempotency

### Request Idempotency

Ensure safe retry behavior with idempotency keys:

```csharp
// The GraphRestApi automatically handles idempotency
var request = new ExecuteGraphRequest
{
    GraphName = "weather-workflow",
    Arguments = new KernelArguments { ["location"] = "New York" },
    IdempotencyKey = "weather-ny-2025-08-15" // Unique key for this operation
};

var response = await graphApi.ExecuteGraphAsync(request, cancellationToken);
```

**Idempotency Features:**
* **Automatic handling**: Built into the GraphRestApi service
* **Request deduplication**: Prevents duplicate executions
* **Hash validation**: Ensures request consistency
* **Configurable window**: Adjustable idempotency time window

## Validation and Error Handling

### Input Validation

Validate inputs before execution:

```csharp
var node = new RestToolGraphNode(schema, httpClient);

// Validate execution arguments
var validationResult = node.ValidateExecution(arguments);
if (!validationResult.IsValid)
{
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"Validation error: {error.Message}");
    }
}
```

### Schema Validation

Validate schema integrity:

```csharp
// Validate schema configuration
if (string.IsNullOrEmpty(schema.Id))
    throw new ArgumentException("Schema ID is required");

if (schema.BaseUri == null)
    throw new ArgumentException("Base URI is required");

if (schema.TimeoutSeconds <= 0)
    throw new ArgumentException("Timeout must be positive");

if (schema.CacheTtlSeconds <= 0 && schema.CacheEnabled)
    throw new ArgumentException("Cache TTL must be positive when caching is enabled");
```

### Error Handling

Comprehensive error handling in REST operations:

```csharp
try
{
    var result = await restNode.ExecuteAsync(kernel, arguments, cancellationToken);
    
    // Safely read HTTP metadata from the execution result.
    // The RestToolGraphNode stores HTTP metadata in the FunctionResult.Metadata dictionary.
    var statusCode = result.Metadata.TryGetValue("status_code", out var scObj) && scObj is int sc ? sc : -1;
    if (statusCode >= 400)
    {
        // Handle HTTP errors: read the response body safely and log.
        var errorBody = result.Metadata.TryGetValue("response_body", out var ebObj) ? ebObj?.ToString() ?? string.Empty : string.Empty;
        _logger.LogError("HTTP error {StatusCode}: {ErrorBody}", statusCode, errorBody);
    }
}
catch (OperationCanceledException)
{
    // Handle timeout or cancellation
    _logger.LogWarning("Request was cancelled or timed out");
}
catch (HttpRequestException ex)
{
    // Handle HTTP request errors
    _logger.LogError(ex, "HTTP request failed");
}
catch (Exception ex)
{
    // Handle other errors
    _logger.LogError(ex, "Unexpected error during REST operation");
}
```

## Telemetry and Monitoring

### Dependency Tracking

Track external API dependencies:

```csharp
var schema = new RestToolSchema
{
    // ... other properties
    TelemetryDependencyName = "Weather API Service"
};

// Telemetry is automatically emitted when IGraphTelemetry is available
```

**Telemetry Data:**
* **Dependency type**: HTTP
* **Target**: API hostname
* **Operation**: HTTP method + path
* **Duration**: Request execution time
* **Success**: HTTP status code range
* **Properties**: Node ID, graph name, URI

### Performance Monitoring

Monitor REST tool performance:

```csharp
// Access performance metrics
var metrics = restNode.GetPerformanceMetrics();
Console.WriteLine($"Total requests: {metrics.TotalExecutions}");
Console.WriteLine($"Average duration: {metrics.AverageExecutionTime}");
Console.WriteLine($"Cache hit rate: {metrics.CacheHitRate:P2}");
```

## Security Features

### Secret Resolution

Secure handling of sensitive data:

```csharp
public interface ISecretResolver
{
    Task<string?> ResolveSecretAsync(string secretName, CancellationToken cancellationToken = default);
}

// Configure secret resolver
var secretResolver = new AzureKeyVaultSecretResolver(keyVaultClient);
var restNode = new RestToolGraphNode(schema, httpClient, secretResolver: secretResolver);
```

**Secret Types:**
* **API keys**: `"secret:weather_api_key"`
* **Authentication tokens**: `"secret:bearer_token"`
* **Connection strings**: `"secret:database_connection"`

### Data Sanitization

Automatic sanitization of sensitive data:

```csharp
// Sensitive data is automatically sanitized in logs and telemetry
var sanitizer = new SensitiveDataSanitizer(new SensitiveDataPolicy
{
    SanitizeApiKeys = true,
    SanitizeTokens = true,
    SanitizeUrls = false
});
```

## Usage Examples

### Weather API Integration

Complete example of integrating a weather API:

```csharp
// 1. Define the schema
var weatherSchema = new RestToolSchema
{
    Id = "weather.current",
    Name = "Current Weather",
    Description = "Get current weather for a location",
    BaseUri = new Uri("https://api.weatherapi.com"),
    Path = "/v1/current.json",
    Method = HttpMethod.Get,
    QueryParameters = 
    {
        ["q"] = "location",
        ["key"] = "secret:weather_api_key"
    },
    CacheEnabled = true,
    CacheTtlSeconds = 1800, // 30 minutes
    TimeoutSeconds = 10
};

// 2. Create the node
var httpClient = new HttpClient();
var secretResolver = new EnvironmentSecretResolver();
var weatherNode = new RestToolGraphNode(weatherSchema, httpClient, secretResolver: secretResolver);

// 3. Use in a graph
var graph = new GraphExecutor("weather-graph");
graph.AddNode(weatherNode).SetStartNode("weather.current");

// 4. Execute with location
var args = new KernelArguments { ["location"] = "London" };
var result = await graph.ExecuteAsync(kernel, args);

// 5. Process results
var weatherData = result.GetValue<object>("response_json");
var statusCode = result.GetValue<int>("status_code");
```

### E-commerce API Integration

Example of integrating an e-commerce system:

```csharp
// Product search schema
var productSearchSchema = new RestToolSchema
{
    Id = "products.search",
    Name = "Search Products",
    BaseUri = new Uri("https://api.store.com"),
    Path = "/v1/products/search",
    Method = HttpMethod.Get,
    QueryParameters = 
    {
        ["q"] = "search_query",
        ["category"] = "category_id",
        ["price_min"] = "min_price",
        ["price_max"] = "max_price",
        ["sort"] = "sort_order"
    },
    Headers = 
    {
        ["Authorization"] = "secret:store_api_key",
        ["Accept-Language"] = "language"
    },
    CacheEnabled = true,
    CacheTtlSeconds = 300
};

// Order creation schema
var orderCreateSchema = new RestToolSchema
{
    Id = "orders.create",
    Name = "Create Order",
    BaseUri = new Uri("https://api.store.com"),
    Path = "/v1/orders",
    Method = HttpMethod.Post,
    JsonBodyTemplate = """
    {
        "customer_id": "{{customer_id}}",
        "items": {{items_json}},
        "shipping_address": {
            "street": "{{street}}",
            "city": "{{city}}",
            "postal_code": "{{postal_code}}"
        }
    }
    """,
    Headers = 
    {
        ["Content-Type"] = ":application/json",
        ["Authorization"] = "secret:store_api_key"
    },
    CacheEnabled = false,
    TimeoutSeconds = 30
};
```

## Best Practices

### 1. Schema Design

* **Use descriptive IDs**: Clear, hierarchical naming (e.g., `"api.weather.current"`)
* **Provide descriptions**: Help developers understand tool purpose
* **Set appropriate timeouts**: Balance responsiveness with reliability
* **Configure caching**: Enable for read operations, disable for mutations

### 2. Security

* **Use secret resolver**: Never hardcode API keys
* **Validate inputs**: Sanitize user-provided data
* **Set timeouts**: Prevent hanging requests
* **Monitor usage**: Track API consumption and errors

### 3. Performance

* **Enable caching**: Cache responses for read operations
* **Set TTL appropriately**: Balance freshness with performance
* **Use connection pooling**: Reuse HttpClient instances
* **Monitor metrics**: Track response times and error rates

### 4. Error Handling

* **Handle HTTP errors**: Check status codes and error responses
* **Implement retries**: Use exponential backoff for transient failures
* **Log failures**: Provide context for debugging
* **Graceful degradation**: Continue execution when possible

### 5. Testing

* **Mock external APIs**: Use test doubles for development
* **Validate schemas**: Ensure schema definitions are correct
* **Test error scenarios**: Verify error handling behavior
* **Performance testing**: Validate caching and timeout behavior

## Troubleshooting

### Common Issues

**Timeout errors:**
* Check network connectivity
* Verify API endpoint availability
* Adjust timeout settings in schema
* Monitor API response times

**Authentication failures:**
* Verify secret resolver configuration
* Check API key validity
* Ensure proper header formatting
* Validate authorization scopes

**Cache issues:**
* Verify cache configuration
* Check TTL settings
* Monitor memory usage
* Validate cache key generation

**Schema validation errors:**
* Check required properties
* Verify URI format
* Validate HTTP method
* Ensure parameter mappings

### Debug Information

Enable detailed logging for troubleshooting:

```csharp
// Enable debug logging
var logger = LoggerFactory.Create(builder => 
    builder.SetMinimumLevel(LogLevel.Debug)
).CreateLogger<RestToolGraphNode>();

var restNode = new RestToolGraphNode(schema, httpClient, logger: logger);

// Check execution details using safe metadata access.
var result = await restNode.ExecuteAsync(kernel, arguments);
var status = result.Metadata.TryGetValue("status_code", out var stObj) && stObj is int sti ? sti : -1;
var responseBody = result.Metadata.TryGetValue("response_body", out var respObj) ? respObj?.ToString() ?? string.Empty : string.Empty;
var fromCache = result.Metadata.TryGetValue("from_cache", out var fcObj) && fcObj is bool fcb && fcb;
Console.WriteLine($"Status: {status}");
Console.WriteLine($"Response: {responseBody}");
Console.WriteLine($"From cache: {fromCache}");
```

## See Also

* [Exposing REST APIs](./exposing-rest-apis.md) - Expose graph functionality via REST
* [State Management](./state.md) - Graph state and argument handling
* [Performance Metrics](./metrics-and-observability.md) - Monitoring and observability
* [Security and Data](./security-and-data.md) - Security best practices
* [Integration and Extensions](./integration-and-extensions.md) - General integration patterns

## Examples

* [REST API Example](../../examples/rest-api.md) - Complete REST integration demonstration
* [Multi-Agent Workflow](../../tutorials/multi-agent-workflow.md) - Complex tool orchestration
* [Document Analysis Pipeline](../../tutorials/document-analysis-pipeline.md) - External service integration
