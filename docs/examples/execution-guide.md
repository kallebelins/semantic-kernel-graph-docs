# Examples Execution Guide

This guide provides comprehensive instructions for running examples in the Semantic Kernel Graph package using the command-line interface and programmatic execution.

## Quick Start

### Prerequisites

1. **.NET 8.0** or later installed
2. **OpenAI API Key** or **Azure OpenAI** credentials
3. **Semantic Kernel Graph package** dependencies

### Basic Commands

```bash
# Navigate to examples project
cd semantic-kernel-graph/src/SemanticKernel.Graph.Examples

# List all available examples
dotnet run -- --list

# Run a specific example
dotnet run -- --example chain-of-thought

# Run multiple examples
dotnet run -- --example chain-of-thought,chatbot,checkpointing

# Run all examples sequentially
dotnet run

# Start REST API server
dotnet run -- --rest-api
```

## Command Line Options

### Core Options

| Option | Short | Description | Example |
|--------|-------|-------------|---------|
| `--list` | - | Display all available examples | `dotnet run -- --list` |
| `--example <names>` | `-e <names>` | Run specific example(s) | `dotnet run -- --example react-agent` |
| `--rest-api` | - | Start REST API server | `dotnet run -- --rest-api` |

### Example Selection

Examples can be specified using:
- **Single example**: `--example chain-of-thought`
- **Multiple examples**: `--example chain-of-thought,chatbot,checkpointing`
- **Comma-separated list**: `--example "react-agent,multi-agent,subgraphs"`

### REST API Mode

When using `--rest-api`, the program starts a web server that provides:
- **Graph execution endpoints** for external integration
- **Graph management APIs** for registration and discovery
- **Streaming support** for real-time execution monitoring

## Available Examples

### Core Graph Patterns
- `chain-of-thought` - Chain of Thought reasoning patterns
- `conditional-nodes` - Dynamic routing with conditional logic
- `loop-nodes` - Controlled iteration and loop management
- `subgraphs` - Modular graph composition and isolation

### Agent Patterns
- `react-agent` - Reasoning and action loops
- `react` - Complex problem solving with ReAct
- `memory-agent` - Persistent memory across conversations
- `retrieval-agent` - Information retrieval and synthesis
- `multi-agent` - Coordinated multi-agent workflows

### Advanced Workflows
- `advanced-patterns` - Complex workflow compositions
- `advanced-routing` - Dynamic routing with semantic similarity
- `dynamic-routing` - Runtime routing decisions
- `documents` - Multi-stage document processing
- `multihop-rag-retry` - Resilient information retrieval

### State and Persistence
- `checkpointing` - Execution state persistence and recovery
- `streaming-execution` - Real-time execution monitoring

### Observability and Debugging
- `metrics` - Performance monitoring and metrics collection
- `graph-visualization` - Graph structure visualization
- `logging` - Comprehensive logging and tracing

### Integration and Extensions
- `plugins` - Dynamic plugin loading and execution
- `rest-api` - External API integration via REST tools
- `assert-suggest` - Validation and suggestion patterns

### AI and Optimization
- `optimizers-fewshot` - Prompt optimization and few-shot learning
- `chatbot` - Conversational AI with persistent context

## Configuration

### Environment Setup

The examples use configuration from `appsettings.json` and environment variables:

```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "Model": "gpt-3.5-turbo",
    "MaxTokens": 4000,
    "Temperature": 0.7
  },
  "AzureOpenAI": {
    "ApiKey": "your-azure-openai-key",
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "your-deployment-name"
  }
}
```

### Environment Variables

Set these environment variables for secure configuration:

```bash
# OpenAI
export OPENAI_API_KEY="your-api-key"

# Azure OpenAI
export AZURE_OPENAI_API_KEY="your-azure-key"
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export AZURE_OPENAI_DEPLOYMENT_NAME="your-deployment"
```

### Configuration Priority

1. **Environment variables** (highest priority)
2. **appsettings.json** file
3. **Default values** (lowest priority)

## Execution Flow

### 1. Program Initialization

```csharp
// Kernel configuration with graph support
var kernel = await CreateConfiguredKernelAsync();

// Logger factory setup
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});
```

### 2. Example Registration

Examples are registered in a dictionary for dynamic execution:

```csharp
var examples = new Dictionary<string, Func<Task>>(StringComparer.OrdinalIgnoreCase)
{
    ["chain-of-thought"] = async () => await ChainOfThoughtExample.RunAsync(),
    ["chatbot"] = async () => await ChatbotExample.RunAsync(),
    ["checkpointing"] = async () => await CheckpointingExample.RunAsync(),
    // ... more examples
};
```

### 3. Execution Process

1. **Argument Parsing**: Parse command-line arguments
2. **Example Selection**: Identify examples to run
3. **Kernel Setup**: Configure Semantic Kernel with graph support
4. **Example Execution**: Run selected examples sequentially
5. **Result Display**: Show execution results and statistics

## REST API Mode

### Starting the Server

```bash
dotnet run -- --rest-api
```

### Available Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/graphs` | GET | List registered graphs |
| `/graphs/execute` | POST | Execute a specific graph |

### API Usage

```bash
# List available graphs
curl http://localhost:5000/graphs

# Execute a graph
curl -X POST http://localhost:5000/graphs/execute \
  -H "Content-Type: application/json" \
  -H "x-api-key: your-api-key" \
  -d '{"graphName": "sample-graph", "variables": {"input": "Hello World"}}'
```

### Graph Registration

Graphs are automatically registered when the server starts:

```csharp
// Create and register a sample graph
var echoFunc = KernelFunctionFactory.CreateFromMethod(
    (string input) => $"echo:{input}",
    functionName: "echo",
    description: "Echoes the input string");

var echoNode = new FunctionGraphNode(echoFunc, nodeId: "echo");
var graph = new GraphExecutor("sample-graph", "Simple echo graph");
graph.AddNode(echoNode).SetStartNode("echo");

await factory.RegisterAsync(graph);
```

## Advanced Usage

### Running Examples Programmatically

You can also run examples programmatically from your own code:

```csharp
// Run specific examples
await ChainOfThoughtExample.RunAsync();
await ChatbotExample.RunAsync();
await CheckpointingExample.RunAsync();

// Run with custom kernel
var kernel = CreateCustomKernel();
await ReActAgentExample.RunAsync(kernel);
```

### Custom Configuration

Modify the `CreateConfiguredKernelAsync()` method to customize:

```csharp
static async Task<Kernel> CreateConfiguredKernelAsync()
{
    var kernelBuilder = Kernel.CreateBuilder()
        .AddOpenAIChatCompletion("gpt-4", "your-api-key")
        .AddGraphSupport(options =>
        {
            options.EnableLogging = true;
            options.EnableMetrics = true;
            options.MaxExecutionSteps = 200;
            options.ExecutionTimeout = TimeSpan.FromMinutes(10);
        })
        .AddGraphMemory()
        .AddGraphTemplates()
        .AddCheckpointSupport(options =>
        {
            options.EnableCompression = true;
            options.MaxCacheSize = 2000;
            options.EnableAutoCleanup = true;
        });

    return kernelBuilder.Build();
}
```

### Batch Execution

Run multiple examples in sequence with custom logic:

```csharp
var examplesToRun = new[] { "chain-of-thought", "chatbot", "checkpointing" };
foreach (var exampleName in examplesToRun)
{
    if (examples.TryGetValue(exampleName, out var run))
    {
        Console.WriteLine($"Running: {exampleName}");
        await run();
        Console.WriteLine($"Completed: {exampleName}");
    }
}
```

## Troubleshooting

### Common Issues

#### API Key Configuration
```bash
# Error: OpenAI API Key not found
# Solution: Set environment variable or update appsettings.json
export OPENAI_API_KEY="your-actual-api-key"
```

#### Package Dependencies
```bash
# Error: Package not found
# Solution: Restore packages
dotnet restore
```

#### Memory Issues
```bash
# Error: Out of memory
# Solution: Increase memory limits
export DOTNET_GCHeapHardLimit=0x40000000
```

#### Timeout Issues
```bash
# Error: Execution timeout
# Solution: Increase timeout in configuration
{
  "GraphSettings": {
    "DefaultTimeout": "00:15:00"
  }
}
```

### Debug Mode

Enable debug logging for troubleshooting:

```json
{
  "Logging": {
    "LogLevel": {
      "SemanticKernel.Graph": "Debug"
    }
  },
  "GraphSettings": {
    "EnableDebugMode": true
  }
}
```

### Performance Monitoring

Monitor execution performance with metrics:

```csharp
// Enable metrics collection
kernelBuilder.AddGraphSupport(options =>
{
    options.EnableMetrics = true;
    options.EnableProfiling = true;
});
```

## Integration Examples

### CI/CD Integration

```yaml
# GitHub Actions example
- name: Run Examples
  run: |
    cd semantic-kernel-graph/src/SemanticKernel.Graph.Examples
    dotnet run -- --example chain-of-thought,chatbot
```

### Docker Integration

```dockerfile
# Dockerfile for examples
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet build
CMD ["dotnet", "run", "--", "--example", "chain-of-thought"]
```

### External Tool Integration

```bash
# Run examples from external tools
semantic-kernel-graph-examples --example chain-of-thought --output json
semantic-kernel-graph-examples --example chatbot --config custom-config.json
```

## Best Practices

### 1. Environment Management
- Use environment variables for sensitive configuration
- Keep `appsettings.json` for non-sensitive defaults
- Use different configurations for development/staging/production

### 2. Example Selection
- Start with simple examples (chain-of-thought, conditional-nodes)
- Progress to complex patterns (multi-agent, advanced-patterns)
- Use `--list` to discover available examples

### 3. Error Handling
- Monitor execution logs for errors
- Use debug mode for troubleshooting
- Check API key configuration first

### 4. Performance
- Enable metrics for performance monitoring
- Use appropriate timeouts for long-running examples
- Monitor memory usage for large graphs

## Related Documentation

- [Examples Index](./index.md): Complete list of available examples
- [Getting Started](../getting-started.md): Quick start guide
- [Installation](../installation.md): Setup and configuration
- [API Reference](../api/): Complete API documentation
- [Troubleshooting](../troubleshooting.md): Common issues and solutions
