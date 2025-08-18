# Integrate tools

This guide explains how to integrate external tools and REST APIs into your SemanticKernel.Graph workflows. You'll learn how to use the tool registry, create tool schemas, and implement automatic converters for seamless integration.

## Overview

The tools integration system enables you to:
- **Wrap external APIs** as executable graph nodes
- **Validate input/output schemas** automatically
- **Cache responses** for improved performance
- **Handle authentication** and error scenarios
- **Integrate with any REST service** through standardized schemas

## Core Components

### Tool Registry

The `IToolRegistry` provides centralized management of available tools:

```csharp
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Integration;

// Get the tool registry from your service provider
var toolRegistry = serviceProvider.GetRequiredService<IToolRegistry>();

// List available tools
var availableTools = await toolRegistry.ListAsync();
foreach (var tool in availableTools)
{
    Console.WriteLine($"Tool: {tool.Name} - {tool.Description}");
}
```

### Tool Schema

Define the structure and behavior of external tools using `RestToolSchema`:

```csharp
var weatherTool = new RestToolSchema
{
    Id = "weather_api",
    Name = "Weather Service",
    Description = "Get current weather information for a location",
    BaseUri = new Uri("https://api.weatherapi.com/v1"),
    Path = "/current.json",
    Method = HttpMethod.Get,
    QueryParameters = new Dictionary<string, string>
    {
        ["key"] = "{api_key}",
        ["q"] = "{location}",
        ["aqi"] = "no"
    },
    Headers = new Dictionary<string, string>
    {
        ["User-Agent"] = "SemanticKernel.Graph/1.0"
    },
    TimeoutSeconds = 30,
    CacheEnabled = true,
    CacheTtlSeconds = 300 // 5 minutes
};
```

### Tool Schema Converter

Automatically convert schemas into executable nodes:

```csharp
var converter = serviceProvider.GetRequiredService<IToolSchemaConverter>();

// Convert schema to executable node
var weatherNode = converter.ConvertToNode(weatherTool);

// Add to your graph
graph.AddNode(weatherNode)
     .AddEdge("start", "weather_api")
     .AddEdge("weather_api", "process_weather");
```

## REST API Integration

### Basic REST Tool

Create a simple REST tool for external API calls:

```csharp
var restTool = new RestToolSchema
{
    Id = "external_api",
    Name = "External Service",
    Description = "Call external REST API",
    BaseUri = new Uri("https://api.example.com"),
    Path = "/data",
    Method = HttpMethod.Post,
    JsonBodyTemplate = @"{
        ""user_id"": ""{user_id}"",
        ""action"": ""{action}"",
        ""timestamp"": ""{timestamp}""
    }",
    Headers = new Dictionary<string, string>
    {
        ["Authorization"] = "Bearer {auth_token}",
        ["Content-Type"] = "application/json"
    },
    TimeoutSeconds = 60,
    CacheEnabled = false
};
```

### Dynamic Parameter Mapping

Map graph state variables to API parameters:

```csharp
var dynamicTool = new RestToolSchema
{
    Id = "dynamic_api",
    Name = "Dynamic API Call",
    Description = "API with dynamic parameter mapping",
    BaseUri = new Uri("https://api.example.com"),
    Path = "/search",
    Method = HttpMethod.Get,
    QueryParameters = new Dictionary<string, string>
    {
        ["query"] = "{search_term}",
        ["limit"] = "{result_limit}",
        ["offset"] = "{page_offset}",
        ["sort"] = "{sort_order}"
    },
    Headers = new Dictionary<string, string>
    {
        ["X-API-Key"] = "{api_key}",
        ["X-Request-ID"] = "{request_id}"
    }
};
```

## Advanced Tool Patterns

### Chained Tool Calls

Create workflows that chain multiple tool calls:

```csharp
var dataFetchTool = new RestToolSchema
{
    Id = "fetch_data",
    Name = "Data Fetcher",
    Description = "Fetch data from external source",
    BaseUri = new Uri("https://api.datasource.com"),
    Path = "/data/{data_id}",
    Method = HttpMethod.Get
};

var dataProcessTool = new RestToolSchema
{
    Id = "process_data",
    Name = "Data Processor",
    Description = "Process fetched data",
    BaseUri = new Uri("https://api.processor.com"),
    Path = "/process",
    Method = HttpMethod.Post,
    JsonBodyTemplate = @"{
        ""input_data"": ""{fetched_data}"",
        ""processing_options"": ""{processing_options}""
    }"
};

// Chain the tools in your graph
graph.AddNode(converter.ConvertToNode(dataFetchTool))
     .AddNode(converter.ConvertToNode(dataProcessTool))
     .AddEdge("start", "fetch_data")
     .AddEdge("fetch_data", "process_data");
```

### Conditional Tool Execution

Use conditional logic to determine which tools to execute:

```csharp
var highPriorityTool = new RestToolSchema
{
    Id = "high_priority_api",
    Name = "High Priority Service",
    Description = "Fast, premium API service",
    BaseUri = new Uri("https://api.premium.com"),
    Path = "/process",
    Method = HttpMethod.Post,
    TimeoutSeconds = 10
};

var standardTool = new RestToolSchema
{
    Id = "standard_api",
    Name = "Standard Service",
    Description = "Standard API service",
    BaseUri = new Uri("https://api.standard.com"),
    Path = "/process",
    Method = HttpMethod.Post,
    TimeoutSeconds = 30
};

// Route to different tools based on priority
graph.AddConditionalEdge("decision", "high_priority_api", 
    condition: state => state.GetString("priority") == "high")
.AddConditionalEdge("decision", "standard_api", 
    condition: state => state.GetString("priority") != "high");
```

### Tool Result Processing

Process and transform tool results:

```csharp
var resultProcessor = new FunctionGraphNode(
    kernelFunction: (state) => {
        var apiResult = state.GetValue<object>("api_response");
        var processedData = ProcessApiResult(apiResult);
        
        // Store processed data
        state["processed_data"] = processedData;
        state["processing_timestamp"] = DateTimeOffset.UtcNow;
        
        return $"Processed {processedData.Count} items";
    },
    nodeId: "process_results"
);

graph.AddEdge("external_api", "process_results")
     .AddEdge("process_results", "next_step");
```

## Authentication and Security

### API Key Authentication

Configure tools with API key authentication:

```csharp
var authenticatedTool = new RestToolSchema
{
    Id = "secure_api",
    Name = "Secure API",
    Description = "API requiring authentication",
    BaseUri = new Uri("https://api.secure.com"),
    Path = "/protected",
    Method = HttpMethod.Get,
    Headers = new Dictionary<string, string>
    {
        ["X-API-Key"] = "{api_key}",
        ["Authorization"] = "Bearer {bearer_token}"
    }
};

// Store sensitive credentials in state
var authNode = new FunctionGraphNode(
    kernelFunction: (state) => {
        state["api_key"] = Environment.GetEnvironmentVariable("API_KEY");
        state["bearer_token"] = GetBearerToken();
        return "Authentication configured";
    },
    nodeId: "configure_auth"
);
```

### Secret Management

Use secret resolvers for secure credential handling:

```csharp
var secretResolver = new EnvironmentSecretResolver();

var secureTool = new RestToolSchema
{
    Id = "secret_api",
    Name = "Secret API",
    Description = "API with secret credentials",
    BaseUri = new Uri("https://api.secret.com"),
    Path = "/secret",
    Method = HttpMethod.Post,
    Headers = new Dictionary<string, string>
    {
        ["X-Secret-Key"] = "{secret_key}"
    }
};

// The secret resolver will automatically resolve {secret_key}
var secureNode = converter.ConvertToNode(secureTool, secretResolver);
```

## Caching and Performance

### Response Caching

Enable caching for improved performance:

```csharp
var cachedTool = new RestToolSchema
{
    Id = "cached_api",
    Name = "Cached API",
    Description = "API with response caching",
    BaseUri = new Uri("https://api.cacheable.com"),
    Path = "/data",
    Method = HttpMethod.Get,
    CacheEnabled = true,
    CacheTtlSeconds = 600 // 10 minutes
};

// Cache key is automatically generated based on method, URL, and parameters
var cachedNode = converter.ConvertToNode(cachedTool);
```

### Custom Cache Keys

Implement custom caching strategies:

```csharp
var customCacheTool = new RestToolSchema
{
    Id = "custom_cache_api",
    Name = "Custom Cache API",
    Description = "API with custom caching",
    BaseUri = new Uri("https://api.custom.com"),
    Path = "/custom",
    Method = HttpMethod.Get,
    CacheEnabled = true,
    CacheTtlSeconds = 300
};

// Custom cache key generation
var customCacheNode = new RestToolGraphNode(customCacheTool, httpClient)
{
    CustomCacheKeyGenerator = (request) => {
        var userId = request.GetValue<string>("user_id");
        var timestamp = DateTimeOffset.UtcNow.Date.ToString("yyyy-MM-dd");
        return $"user_{userId}_date_{timestamp}";
    }
};
```

## Error Handling

### Tool Error Recovery

Handle tool failures gracefully:

```csharp
var errorHandlingTool = new RestToolSchema
{
    Id = "resilient_api",
    Name = "Resilient API",
    Description = "API with error handling",
    BaseUri = new Uri("https://api.resilient.com"),
    Path = "/resilient",
    Method = HttpMethod.Get,
    TimeoutSeconds = 30
};

var errorHandler = new ErrorHandlerGraphNode(
    errorTypes: new[] { ErrorType.External, ErrorType.Timeout },
    recoveryAction: RecoveryAction.Retry,
    maxRetries: 3,
    nodeId: "tool_error_handler"
);

graph.AddEdge("resilient_api", "tool_error_handler")
     .AddEdge("tool_error_handler", "fallback_service");
```

### Fallback Tools

Provide alternative tools when primary tools fail:

```csharp
var primaryTool = new RestToolSchema
{
    Id = "primary_api",
    Name = "Primary API",
    Description = "Primary API service",
    BaseUri = new Uri("https://api.primary.com"),
    Path = "/service",
    Method = HttpMethod.Get
};

var fallbackTool = new RestToolSchema
{
    Id = "fallback_api",
    Name = "Fallback API",
    Description = "Fallback API service",
    BaseUri = new Uri("https://api.fallback.com"),
    Path = "/service",
    Method = HttpMethod.Get
};

// Route to fallback on primary failure
graph.AddConditionalEdge("primary_api", "success", 
    condition: state => state.GetBool("primary_succeeded", false))
.AddConditionalEdge("primary_api", "fallback_api", 
    condition: state => !state.GetBool("primary_succeeded", true));
```

## Monitoring and Observability

### Tool Metrics

Track tool performance and usage:

```csharp
var monitoredTool = new RestToolSchema
{
    Id = "monitored_api",
    Name = "Monitored API",
    Description = "API with performance monitoring",
    BaseUri = new Uri("https://api.monitored.com"),
    Path = "/monitored",
    Method = HttpMethod.Get,
    TelemetryDependencyName = "external_api_service"
};

// Metrics are automatically collected via IGraphTelemetry
var monitoredNode = converter.ConvertToNode(monitoredTool);
```

### Custom Telemetry

Implement custom telemetry for tools:

```csharp
var customTelemetryTool = new RestToolSchema
{
    Id = "custom_telemetry_api",
    Name = "Custom Telemetry API",
    Description = "API with custom telemetry",
    BaseUri = new Uri("https://api.telemetry.com"),
    Path = "/telemetry",
    Method = HttpMethod.Get
};

var telemetryNode = new RestToolGraphNode(customTelemetryTool, httpClient)
{
    BeforeExecution = async (context) => {
        context.GraphState["tool_start_time"] = DateTimeOffset.UtcNow;
        context.GraphState["tool_request_id"] = Guid.NewGuid().ToString();
        return Task.CompletedTask;
    },
    AfterExecution = async (context, result) => {
        var startTime = context.GraphState.GetValue<DateTimeOffset>("tool_start_time");
        var duration = DateTimeOffset.UtcNow - startTime;
        
        // Log custom metrics
        context.GraphState["tool_duration"] = duration;
        context.GraphState["tool_success"] = result.IsSuccess;
        
        return Task.CompletedTask;
    }
};
```

## Best Practices

### Tool Design

1. **Clear naming** - Use descriptive names for tool IDs and descriptions
2. **Parameter validation** - Validate required parameters before tool execution
3. **Error handling** - Implement proper error handling and fallback strategies
4. **Performance optimization** - Use caching and connection pooling where appropriate

### Security Considerations

1. **Credential management** - Use secret resolvers for sensitive information
2. **Input validation** - Validate all inputs to prevent injection attacks
3. **HTTPS enforcement** - Always use HTTPS for external API calls
4. **Rate limiting** - Implement rate limiting to prevent API abuse

### Monitoring and Maintenance

1. **Performance tracking** - Monitor tool response times and success rates
2. **Error logging** - Log all tool failures for debugging and improvement
3. **Health checks** - Implement health checks for critical external tools
4. **Documentation** - Keep tool schemas and usage examples up to date

## Troubleshooting

### Common Issues

**Authentication failures**: Verify API keys and bearer tokens are correctly configured

**Timeout errors**: Check network connectivity and adjust timeout settings

**Cache issues**: Verify cache configuration and clear cache if needed

**Parameter mapping errors**: Ensure state variables match schema parameter names

### Debugging Tips

1. **Enable detailed logging** to trace tool execution
2. **Check HTTP requests** in network monitoring tools
3. **Validate tool schemas** before execution
4. **Test tools independently** to isolate issues

## Concepts and Techniques

**RestToolSchema**: A configuration object that defines the structure and behavior of external REST API tools. It specifies the API endpoint, HTTP method, parameters, headers, and caching behavior.

**IToolRegistry**: A centralized service that manages the registration, discovery, and lifecycle of available tools in the graph system.

**IToolSchemaConverter**: A service that automatically converts tool schemas into executable graph nodes, enabling seamless integration of external APIs.

**RestToolGraphNode**: An executable graph node that wraps external REST API calls. It handles parameter mapping, HTTP requests, response processing, and error handling.

**Tool Caching**: A performance optimization mechanism that stores API responses to avoid repeated calls for identical requests, improving response times and reducing external API usage.

## See Also

- [REST Tools Integration](rest-tools-integration.md) - Comprehensive guide to REST tool integration
- [Build a Graph](build-a-graph.md) - Learn how to construct graphs with tool nodes
- [Error Handling and Resilience](error-handling-and-resilience.md) - Handle tool failures gracefully
- [Security and Data](security-and-data.md) - Secure tool integration practices
- [Examples: Tool Integration](../examples/plugin-system-example.md) - Complete working examples of tool integration
