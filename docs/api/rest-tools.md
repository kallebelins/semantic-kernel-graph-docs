# REST Tools

The REST Tools system in SemanticKernel.Graph provides comprehensive integration capabilities for external REST APIs and services. It enables you to seamlessly incorporate external HTTP endpoints into your graph workflows with built-in validation, caching, and idempotency support.

## Overview

The REST tools system consists of several key components:

* **`RestToolSchema`**: Defines the structure and behavior of REST API operations
* **`RestToolGraphNode`**: Executable graph node that performs HTTP requests
* **`RestToolSchemaConverter`**: Converts schemas into executable nodes
* **`IToolSchemaConverter`**: Interface for custom tool converters
* **Built-in caching and idempotency**: Performance optimization and request safety

## Core Classes

### RestToolSchema

Defines the contract for a REST API operation with comprehensive configuration options:

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

**Key Properties:**
* **`Id`**: Unique identifier for the REST tool
* **`Name`**: Human-friendly name for the tool
* **`Description`**: Purpose and behavior description
* **`BaseUri`**: Base URI of the remote service
* **`Path`**: Relative path for the operation
* **`Method`**: HTTP method (GET, POST, PUT, DELETE, etc.)
* **`JsonBodyTemplate`**: Optional JSON body template with variable substitution
* **`QueryParameters`**: Mapping of query parameters to graph state variables
* **`Headers`**: Header mappings with support for literals and secrets
* **`TimeoutSeconds`**: Request timeout in seconds
* **`CacheEnabled`**: Whether response caching is enabled
* **`CacheTtlSeconds`**: Cache time-to-live in seconds
* **`TelemetryDependencyName`**: Custom name for telemetry tracking

### RestToolGraphNode

Executable graph node that performs HTTP requests based on schema definitions:

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

**Constructor Parameters:**
* `schema`: REST tool schema defining the operation
* `httpClient`: HTTP client for making requests
* `logger`: Optional logger for diagnostics
* `secretResolver`: Optional secret resolver for sensitive data
* `telemetry`: Optional telemetry adapter for dependency tracking

**Key Features:**
* **Input mapping**: Automatically maps graph state to HTTP parameters
* **Response handling**: Parses JSON responses and provides structured output
* **Error handling**: Comprehensive error handling with telemetry
* **Schema validation**: Implements `ITypedSchemaNode` for type safety
* **Secret resolution**: Secure handling of API keys and sensitive data
* **Caching**: Built-in response caching with configurable TTL
* **Timeout management**: Per-request timeout handling
* **Telemetry integration**: Dependency tracking for monitoring

### RestToolSchemaConverter

Default converter that transforms `RestToolSchema` instances into executable `RestToolGraphNode` instances:

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

**Constructor Parameters:**
* `httpClient`: HTTP client shared across all REST operations
* `logger`: Optional logger for conversion diagnostics
* `secretResolver`: Optional secret resolver for secure operations
* `telemetry`: Optional telemetry adapter for monitoring

### IToolSchemaConverter

Interface for custom tool schema converters:

```csharp
public interface IToolSchemaConverter
{
    IGraphNode CreateNode(RestToolSchema schema);
}
```

**Purpose:**
* Provides pluggable architecture for tool conversion
* Enables custom conversion logic for specialized use cases
* Supports different tool types beyond REST (future extensibility)

## Schema Configuration

### Basic GET Operation

Define a simple GET operation for retrieving data:

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
    BaseUri = new Uri("https://api.ecommerce.com"),
    Path = "/v1/orders",
    Method = HttpMethod.Post,
    JsonBodyTemplate = @"{
        ""customer_id"": ""{customer_id}"",
        ""items"": {items},
        ""shipping_address"": {
            ""street"": ""{street}"",
            ""city"": ""{city}"",
            ""postal_code"": ""{postal_code}""
        },
        ""payment_method"": ""{payment_method}""
    }",
    Headers = 
    {
        ["Content-Type"] = ":application/json",
        ["Authorization"] = "Bearer {auth_token}",
        ["X-Request-ID"] = "request_id"
    },
    CacheEnabled = false, // POST operations typically not cached
    TimeoutSeconds = 30
};
```

### PUT with Dynamic Path

Define a PUT operation with path parameter substitution:

```csharp
var updateSchema = new RestToolSchema
{
    Id = "users.update",
    Name = "Update User",
    Description = "Update user information",
    BaseUri = new Uri("https://api.users.com"),
    Path = "/v1/users/{user_id}", // Path parameter
    Method = HttpMethod.Put,
    JsonBodyTemplate = @"{
        ""name"": ""{name}"",
        ""email"": ""{email}"",
        ""phone"": ""{phone}""
    }",
    Headers = 
    {
        ["Content-Type"] = ":application/json",
        ["Authorization"] = "Bearer {auth_token}",
        ["If-Match"] = "etag"
    },
    TimeoutSeconds = 15
};
```

## Parameter Mapping

### Query Parameters

Map graph state variables to HTTP query parameters:

```csharp
var searchSchema = new RestToolSchema
{
    // ... other properties
    QueryParameters = 
    {
        ["q"] = "search_term",        // Maps to graph state "search_term"
        ["limit"] = "result_limit",    // Maps to graph state "result_limit"
        ["offset"] = "page_offset",    // Maps to graph state "page_offset"
        ["sort"] = "sort_order",       // Maps to graph state "sort_order"
        ["filter"] = "filter_criteria" // Maps to graph state "filter_criteria"
    }
};
```

**Usage:**
```csharp
var args = new KernelArguments
{
    ["search_term"] = "machine learning",
    ["result_limit"] = 20,
    ["page_offset"] = 0,
    ["sort_order"] = "relevance",
    ["filter_criteria"] = "recent"
};

// Results in: ?q=machine%20learning&limit=20&offset=0&sort=relevance&filter=recent
```

### Headers

Configure HTTP headers with flexible mapping options:

```csharp
var apiSchema = new RestToolSchema
{
    // ... other properties
    Headers = 
    {
        // Literal values (prefixed with ":")
        ["Content-Type"] = ":application/json",
        ["User-Agent"] = ":MyApp/1.0",
        
        // Graph state variables
        ["X-Request-ID"] = "request_id",
        ["X-Correlation-ID"] = "correlation_id",
        ["X-User-ID"] = "user_id",
        
        // Secret references (prefixed with "secret:")
        ["Authorization"] = "secret:api_key",
        ["X-API-Key"] = "secret:service_key"
    }
};
```

**Header Types:**
* **Literal values**: Prefixed with `:` for static header values
* **State variables**: Map to graph state variables
* **Secret references**: Prefixed with `secret:` for secure resolution

### JSON Body Templates

Create dynamic request bodies with variable substitution:

```csharp
var complexSchema = new RestToolSchema
{
    // ... other properties
    JsonBodyTemplate = @"{
        ""metadata"": {
            ""request_id"": ""{request_id}"",
            ""timestamp"": ""{timestamp}"",
            ""source"": ""{source}""
        },
        ""data"": {
            ""user"": {
                ""id"": ""{user_id}"",
                ""name"": ""{user_name}"",
                ""email"": ""{user_email}""
            },
            ""preferences"": {user_preferences},
            ""settings"": {
                ""language"": ""{language}"",
                ""timezone"": ""{timezone}"",
                ""notifications"": {notifications_enabled}
            }
        }
    }"
};
```

**Template Features:**
* **Variable substitution**: `{variable_name}` syntax
* **Nested objects**: Support for complex JSON structures
* **Array support**: Direct array variable insertion
* **Conditional logic**: Basic conditional rendering (future enhancement)

## Caching and Performance

### Response Caching

Enable caching for improved performance and reduced API calls:

```csharp
var cachedSchema = new RestToolSchema
{
    // ... other properties
    CacheEnabled = true,
    CacheTtlSeconds = 3600 // 1 hour
};
```

**Cache Behavior:**
* **Cache key**: Generated from HTTP method, URL, and request body hash
* **TTL management**: Automatic expiration based on schema configuration
* **Memory efficient**: Uses concurrent dictionary with expiration tracking
* **Thread safe**: Concurrent access support for high-performance scenarios

**Cache Configuration Options:**
```csharp
// Short-term caching for frequently changing data
var shortCache = new RestToolSchema
{
    CacheEnabled = true,
    CacheTtlSeconds = 60 // 1 minute
};

// Long-term caching for stable data
var longCache = new RestToolSchema
{
    CacheEnabled = true,
    CacheTtlSeconds = 86400 // 24 hours
};

// Disable caching for dynamic content
var noCache = new RestToolSchema
{
    CacheEnabled = false
};
```

### Cache Key Generation

The caching system generates unique keys based on request characteristics:

```csharp
// Cache key components:
// 1. HTTP method (GET, POST, PUT, DELETE)
// 2. Full URL (base URI + path + query parameters)
// 3. Request body hash (for POST/PUT operations)
// 4. Header values that affect response (optional)

// Example cache key: "GET:https://api.example.com/v1/data?q=test"
// Example cache key: "POST:https://api.example.com/v1/orders:abc123def456"
```

## Timeout and Error Handling

### Request Timeouts

Configure per-request timeout settings:

```csharp
var timeoutSchema = new RestToolSchema
{
    // ... other properties
    TimeoutSeconds = 15 // 15 second timeout
};
```

**Timeout Behavior:**
* **Per-request**: Each schema can have different timeout values
* **Cancellation**: Integrates with graph execution cancellation tokens
* **Fallback**: Graceful handling of timeout scenarios
* **Logging**: Comprehensive timeout logging and telemetry

### Error Handling

Comprehensive error handling with detailed diagnostics:

```csharp
try
{
    var result = await restNode.ExecuteAsync(kernel, arguments, cancellationToken);
    
    // Check response status
    var statusCode = result.GetValue<int>("status_code");
    if (statusCode >= 400)
    {
        // Handle error response
        var errorBody = result.GetValue<string>("response_body");
        _logger.LogError("API request failed with status {StatusCode}: {ErrorBody}", 
            statusCode, errorBody);
    }
}
catch (HttpRequestException ex)
{
    // Handle HTTP-specific errors
    _logger.LogError(ex, "HTTP request failed: {Message}", ex.Message);
}
catch (TaskCanceledException ex)
{
    // Handle timeout and cancellation
    _logger.LogWarning("Request was cancelled or timed out: {Message}", ex.Message);
}
catch (Exception ex)
{
    // Handle other errors
    _logger.LogError(ex, "Unexpected error during REST operation: {Message}", ex.Message);
}
```

**Error Types:**
* **HTTP errors**: Status codes, network issues, timeouts
* **Validation errors**: Schema validation failures
* **Secret resolution errors**: Missing or invalid secrets
* **Serialization errors**: JSON parsing and formatting issues

## Secret Resolution

### Secret References

Secure handling of sensitive data like API keys:

```csharp
var secureSchema = new RestToolSchema
{
    // ... other properties
    Headers = 
    {
        ["Authorization"] = "secret:weather_api_key",
        ["X-API-Key"] = "secret:service_api_key"
    }
};
```

**Secret Types:**
* **API keys**: `"secret:weather_api_key"`
* **Authentication tokens**: `"secret:bearer_token"`
* **Connection strings**: `"secret:database_connection"`
* **Service credentials**: `"secret:service_username"`

### Secret Resolver Implementation

Implement custom secret resolution logic:

```csharp
public class AzureKeyVaultSecretResolver : ISecretResolver
{
    private readonly SecretClient _secretClient;
    
    public AzureKeyVaultSecretResolver(SecretClient secretClient)
    {
        _secretClient = secretClient;
    }
    
    public async Task<string?> ResolveSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        try
        {
            var secret = await _secretClient.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            return secret.Value.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve secret {SecretName}", secretName);
            return null;
        }
    }
}

// Usage
var secretResolver = new AzureKeyVaultSecretResolver(keyVaultClient);
var restNode = new RestToolGraphNode(schema, httpClient, secretResolver: secretResolver);
```

## Telemetry and Monitoring

### Dependency Tracking

Monitor REST API performance and reliability:

```csharp
var monitoredSchema = new RestToolSchema
{
    // ... other properties
    TelemetryDependencyName = "Weather API"
};
```

**Telemetry Data:**
* **Request duration**: Time taken for each API call
* **Success rates**: Percentage of successful requests
* **Error patterns**: Common failure modes and status codes
* **Performance metrics**: Response times, throughput, latency
* **Dependency mapping**: Service dependencies and relationships

### Custom Telemetry

Implement custom telemetry for specialized monitoring:

```csharp
public class CustomTelemetry : IGraphTelemetry
{
    public void TrackDependency(
        string dependencyType,
        string target,
        string name,
        DateTimeOffset startTime,
        TimeSpan duration,
        bool success,
        string resultCode,
        Dictionary<string, string?> properties)
    {
        // Custom telemetry implementation
        var metric = new
        {
            Type = dependencyType,
            Target = target,
            Name = name,
            Duration = duration.TotalMilliseconds,
            Success = success,
            ResultCode = resultCode,
            Properties = properties
        };
        
        // Send to monitoring system
        _monitoringService.TrackMetric(metric);
    }
}

// Usage
var telemetry = new CustomTelemetry();
var restNode = new RestToolGraphNode(schema, httpClient, telemetry: telemetry);
```

## Validation and Schema Safety

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

**Validation Features:**
* **Parameter presence**: Check required parameters are available
* **Type validation**: Ensure parameter types match expectations
* **Schema compliance**: Validate against REST schema requirements
* **Secret availability**: Verify secrets can be resolved

### Schema Validation

Validate schema integrity and configuration:

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

if (schema.Method == HttpMethod.Post && string.IsNullOrEmpty(schema.JsonBodyTemplate))
    throw new ArgumentException("POST operations should include JSON body template");
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
// 1. Product search schema
var searchSchema = new RestToolSchema
{
    Id = "products.search",
    Name = "Search Products",
    Description = "Search for products in the catalog",
    BaseUri = new Uri("https://api.ecommerce.com"),
    Path = "/v1/products/search",
    Method = HttpMethod.Get,
    QueryParameters = 
    {
        ["q"] = "search_term",
        ["category"] = "category_id",
        ["price_min"] = "min_price",
        ["price_max"] = "max_price",
        ["sort"] = "sort_order",
        ["limit"] = "result_limit"
    },
    CacheEnabled = true,
    CacheTtlSeconds = 300, // 5 minutes
    TimeoutSeconds = 15
};

// 2. Order creation schema
var orderSchema = new RestToolSchema
{
    Id = "orders.create",
    Name = "Create Order",
    Description = "Create a new order",
    BaseUri = new Uri("https://api.ecommerce.com"),
    Path = "/v1/orders",
    Method = HttpMethod.Post,
    JsonBodyTemplate = @"{
        ""customer_id"": ""{customer_id}"",
        ""items"": {cart_items},
        ""shipping_address"": {
            ""street"": ""{street}"",
            ""city"": ""{city}"",
            ""postal_code"": ""{postal_code}""
        },
        ""payment_method"": ""{payment_method}""
    }",
    Headers = 
    {
        ["Content-Type"] = ":application/json",
        ["Authorization"] = "Bearer {auth_token}",
        ["X-Request-ID"] = "request_id"
    },
    CacheEnabled = false, // POST operations not cached
    TimeoutSeconds = 30
};

// 3. Create nodes
var searchNode = new RestToolGraphNode(searchSchema, httpClient, secretResolver: secretResolver);
var orderNode = new RestToolGraphNode(orderSchema, httpClient, secretResolver: secretResolver);

// 4. Build workflow
var workflow = new GraphExecutor("ecommerce-workflow");
workflow.AddNode(searchNode)
        .AddNode(orderNode)
        .SetStartNode("products.search");

// 5. Execute workflow
var workflowArgs = new KernelArguments
{
    ["search_term"] = "laptop",
    ["category_id"] = "electronics",
    ["min_price"] = "500",
    ["max_price"] = "2000",
    ["sort_order"] = "price_asc",
    ["result_limit"] = "10"
};

var workflowResult = await workflow.ExecuteAsync(kernel, workflowArgs);
```

## Performance Considerations

### HTTP Client Management

Optimize HTTP client usage for performance:

```csharp
// Use HttpClientFactory for proper lifecycle management
var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

// Create named client with specific configuration
var httpClient = httpClientFactory.CreateClient("api-client");

// Configure client settings
httpClient.Timeout = TimeSpan.FromSeconds(30);
httpClient.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");

// Use in REST nodes
var restNode = new RestToolGraphNode(schema, httpClient);
```

**Best Practices:**
* **Connection pooling**: Reuse HTTP connections across requests
* **Timeout configuration**: Set appropriate timeouts for different APIs
* **Retry policies**: Implement retry logic for transient failures
* **Circuit breaker**: Add circuit breaker pattern for fault tolerance

### Caching Strategy

Optimize caching for different data types:

```csharp
// Static reference data - long cache
var referenceSchema = new RestToolSchema
{
    CacheEnabled = true,
    CacheTtlSeconds = 86400 // 24 hours
};

// Semi-dynamic data - medium cache
var semiDynamicSchema = new RestToolSchema
{
    CacheEnabled = true,
    CacheTtlSeconds = 3600 // 1 hour
};

// Dynamic data - short cache
var dynamicSchema = new RestToolSchema
{
    CacheEnabled = true,
    CacheTtlSeconds = 60 // 1 minute
};

// Real-time data - no cache
var realTimeSchema = new RestToolSchema
{
    CacheEnabled = false
};
```

## Security Considerations

### Secret Management

Secure handling of sensitive configuration:

```csharp
// Use secure secret storage
var secretResolver = new AzureKeyVaultSecretResolver(keyVaultClient);

// Never hardcode secrets in schemas
var secureSchema = new RestToolSchema
{
    // ... other properties
    Headers = 
    {
        ["Authorization"] = "secret:api_key", // Secure reference
        // Avoid: ["Authorization"] = "Bearer hardcoded-token"
    }
};
```

**Security Best Practices:**
* **Secret rotation**: Regularly rotate API keys and tokens
* **Access control**: Limit access to secret resolution services
* **Audit logging**: Log all secret access for compliance
* **Encryption**: Encrypt secrets at rest and in transit

### Data Sanitization

Protect sensitive data in logs and telemetry:

```csharp
// Sensitive data is automatically sanitized
var sanitizer = new SensitiveDataSanitizer(new SensitiveDataPolicy
{
    SanitizeApiKeys = true,
    SanitizeTokens = true,
    SanitizeUrls = false,
    SanitizeHeaders = new[] { "Authorization", "X-API-Key" }
});

// Apply sanitization to telemetry
var telemetry = new SanitizedTelemetry(originalTelemetry, sanitizer);
var restNode = new RestToolGraphNode(schema, httpClient, telemetry: telemetry);
```

## See Also

* [REST Tools Integration](../how-to/rest-tools-integration.md) - Comprehensive guide to REST tools concepts and techniques
* [Tools and Extensions](../how-to/tools.md) - General tools system and integration patterns
* [Graph Executor](./graph-executor.md) - Core execution engine that runs REST tool nodes
* [Graph State](./graph-state.md) - State management for REST tool parameters
* [REST API Example](../../examples/rest-api-example.md) - Complete example demonstrating REST tools capabilities
