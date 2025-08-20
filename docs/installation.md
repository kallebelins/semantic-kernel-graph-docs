# Installation Guide

This guide will help you install and configure SemanticKernel.Graph in your .NET project. You'll learn how to add the package, configure your environment, and set up the basic requirements.

## Prerequisites

Before installing SemanticKernel.Graph, ensure you have:

* **.NET 8.0** or later installed on your system
* **Visual Studio 2022** (17.8+) or **VS Code** with C# extension
* **Semantic Kernel** package already installed in your project
* **LLM Provider API Key** (OpenAI, Azure OpenAI, or other supported providers)

### .NET Version Check

Verify your .NET version by running:

```bash
dotnet --version
```

You should see version 8.0.0 or higher. If not, download and install the latest .NET 8 SDK from [Microsoft's official site](https://dotnet.microsoft.com/download).

## Package Installation

SemanticKernel.Graph is now available on NuGet! You can install it using the following methods:

### Using dotnet CLI (Recommended)

```bash
dotnet add package SemanticKernel.Graph
```

### Using PackageReference

Add this to your `.csproj` file:

```xml
<ItemGroup>
    <PackageReference Include="SemanticKernel.Graph" Version="25.8.191" />
</ItemGroup>
```

### Using Package Manager Console

```
Install-Package SemanticKernel.Graph
```

## Environment Configuration

### LLM Provider Setup

SemanticKernel.Graph requires a configured LLM provider. Here are the most common setups:

#### OpenAI Configuration

```bash
# Windows
setx OPENAI_API_KEY "your-api-key-here"
setx OPENAI_MODEL_NAME "gpt-4"

# macOS/Linux
export OPENAI_API_KEY="your-api-key-here"
export OPENAI_MODEL_NAME="gpt-4"
```

```csharp
// In your code
var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(
    modelId: Environment.GetEnvironmentVariable("OPENAI_MODEL_NAME") ?? "gpt-4",
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new InvalidOperationException("OPENAI_API_KEY not found")
);
```

#### Azure OpenAI Configuration

```bash
# Windows
setx AZURE_OPENAI_ENDPOINT "https://your-resource.openai.azure.com/"
setx AZURE_OPENAI_API_KEY "your-api-key-here"
setx AZURE_OPENAI_DEPLOYMENT_NAME "your-deployment-name"

# macOS/Linux
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export AZURE_OPENAI_API_KEY="your-api-key-here"
export AZURE_OPENAI_DEPLOYMENT_NAME="your-deployment-name"
```

```csharp
// In your code
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    deploymentName: Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT_NAME not found"),
    endpoint: Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT not found"),
    apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY not found")
);
```

#### Configuration File Setup

For local development, you can use `appsettings.json` (remember to add it to `.gitignore`):

```json
{
  "OpenAI": {
    "ApiKey": "your-api-key-here",
    "Model": "gpt-4",
    "MaxTokens": 4000,
    "Temperature": 0.7
  },
  "AzureOpenAI": {
    "ApiKey": "your-azure-api-key",
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "your-deployment",
    "Model": "GPT4o",
    "MaxTokens": 4000,
    "Temperature": 0.7
  }
}
```

Then load it in your application:

```csharp
// Load configuration from appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(
    modelId: configuration["OpenAI:Model"] ?? "gpt-4",
    apiKey: configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey not found")
);
```

## Basic Project Setup

### 1. Create a New .NET Project

```bash
dotnet new console -n MyGraphApp
cd MyGraphApp
```

### 2. Add Required Packages

```bash
dotnet add package Microsoft.SemanticKernel
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Microsoft.Extensions.Configuration.EnvironmentVariables
```

### 3. Add SemanticKernel.Graph Package

```bash
dotnet add package SemanticKernel.Graph
```

This will automatically add the package reference to your `.csproj` file.

### 4. Basic Configuration

```csharp
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using SemanticKernel.Graph.Extensions;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var builder = Kernel.CreateBuilder();

// Configure your LLM provider
builder.AddOpenAIChatCompletion(
    "gpt-4", 
    configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
);

// Enable graph functionality
builder.AddGraphSupport(options =>
{
    options.EnableLogging = true;
    options.EnableMetrics = true;
});

var kernel = builder.Build();
```

## Advanced Configuration

### Graph Options

You can customize graph behavior with various options:

```csharp
builder.AddGraphSupport(options =>
{
    // Enable logging
    options.EnableLogging = true;
    
    // Enable performance metrics
    options.EnableMetrics = true;
    
    // Configure execution limits
    options.MaxExecutionSteps = 100;
    options.ExecutionTimeout = TimeSpan.FromMinutes(5);
});

// Add additional graph features
builder.AddGraphMemory()
    .AddGraphTemplates()
    .AddCheckpointSupport(options =>
    {
        options.EnableCompression = true;
        options.MaxCacheSize = 1000;
        options.EnableAutoCleanup = true;
        options.AutoCleanupInterval = TimeSpan.FromHours(1);
    });
```

### Dependency Injection Setup

For ASP.NET Core applications, SemanticKernel.Graph integrates seamlessly with the DI container:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add Semantic Kernel with graph support
builder.Services.AddKernel(options =>
{
    options.AddOpenAIChatCompletion("gpt-4", builder.Configuration["OpenAI:ApiKey"]);
    options.AddGraphSupport();
});

var app = builder.Build();
```

**Note**: This example requires the ASP.NET Core runtime and the `Microsoft.AspNetCore.App` framework reference. If you're building a console application, you can use the basic configuration examples above instead.

## Running Examples

The project includes comprehensive examples that demonstrate various features:

### 1. Explore the Examples

The documentation includes comprehensive examples in the [examples/](examples/) directory:

* **Basic Patterns**: Simple workflows and node types
* **Advanced Routing**: Dynamic execution paths and conditional logic
* **Multi-Agent**: Coordinated AI agent workflows
* **Enterprise**: Production-ready patterns with monitoring and resilience

### 2. Follow the Tutorials

Each example includes:
* Complete code snippets you can copy and adapt
* Step-by-step explanations of how the code works
* Configuration examples for different scenarios
* Best practices and usage patterns

### 3. Available Example Categories

* **Core Functionality**: Basic setup and configuration
* **Graph Execution**: Workflow patterns and routing
* **Advanced Patterns**: Complex AI workflows and reasoning
* **Enterprise Features**: Production-ready implementations

## Verification

### Test Your Installation

Create a simple test to verify everything is working:

```csharp
using Microsoft.SemanticKernel;
using SemanticKernel.Graph;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

// Test basic functionality
var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion("gpt-4", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
builder.AddGraphSupport();

var kernel = builder.Build();

// Create a simple test node
var testNode = new FunctionGraphNode(
    kernel.CreateFunctionFromPrompt("Say hello to {{$name}}"),
    "test-node"
);

// Create and execute a minimal graph
var graph = new GraphExecutor("TestGraph");
graph.AddNode(testNode);
graph.SetStartNode(testNode.NodeId);

var state = new KernelArguments { ["name"] = "World" };
var result = await graph.ExecuteAsync(kernel, state);

Console.WriteLine($"Test successful! Result: {result}");
```

### Expected Output

If everything is configured correctly, you should see:

```
Test successful! Result: Hello World!
```

**Note**: The actual output may vary depending on the LLM model used. If you don't have a valid API key configured, the test will show a warning message instead of executing the graph.

## Troubleshooting

### Common Installation Issues

#### Build Errors
```
error CS0234: The type or namespace name 'SemanticKernel' does not exist
```
**Solution**: Ensure you've added the SemanticKernel.Graph package correctly using `dotnet add package SemanticKernel.Graph`.

#### Missing Dependencies
```
error CS0246: The type or namespace name 'Microsoft.Extensions.Configuration' could not be found
```
**Solution**: Add the required NuGet packages for configuration and dependency injection.

#### Environment Variable Issues
```
System.InvalidOperationException: OPENAI_API_KEY not found
```
**Solution**: Verify your environment variables are set correctly and accessible to your application.

#### .NET Version Issues
```
error NETSDK1045: The current .NET SDK does not support targeting .NET 8.0
```
**Solution**: Install .NET 8.0 SDK or update your project to target a supported framework version.

### Getting Help

If you encounter issues:

1. **Check the logs**: Enable detailed logging to see what's happening
2. **Verify configuration**: Ensure all environment variables and API keys are correct
3. **Check versions**: Ensure compatibility between Semantic Kernel and .NET versions
4. **Check package version**: Ensure you have the latest version of the SemanticKernel.Graph package
5. **Run examples**: Use the provided examples to verify your setup

## Next Steps

Now that you have SemanticKernel.Graph installed and configured:

* **[First Graph Tutorial](first-graph.md)**: Build your first complete graph workflow
* **[Core Concepts](concepts/index.md)**: Understand the fundamental concepts
* **[Examples](examples/index.md)**: Explore real-world usage patterns
* **[API Reference](api/core.md)**: Dive into the complete API surface

## Concepts and Techniques

This installation guide covers several key concepts:

* **Package Management**: Installing SemanticKernel.Graph from NuGet package repository
* **Environment Configuration**: Setting up LLM provider credentials and configuration
* **Configuration Management**: Using appsettings.json and environment variables
* **Dependency Injection**: Integrating with .NET's DI container for enterprise applications

## Prerequisites and Minimum Configuration

To successfully install and use SemanticKernel.Graph, you need:
* **.NET 8.0+** runtime and SDK
* **Internet Access** to download packages from NuGet
* **Semantic Kernel** package (compatible version)
* **LLM Provider** configuration (API keys, endpoints)
* **Configuration Files** or environment variables for sensitive data

## See Also

* **[Getting Started](getting-started.md)**: Overview of SemanticKernel.Graph capabilities
* **[Core Concepts](concepts/index.md)**: Understanding graphs, nodes, and execution
* **[Troubleshooting](troubleshooting.md)**: Common issues and solutions
* **[API Reference](api/core.md)**: Complete API documentation
