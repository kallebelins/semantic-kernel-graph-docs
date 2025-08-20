# FAQ - Frequently Asked Questions

Common questions and answers about SemanticKernel.Graph.

## Basic Concepts

### What is SemanticKernel.Graph?
**SemanticKernel.Graph** is an extension of Semantic Kernel that adds computational graph execution capabilities, allowing you to create complex workflows with nodes, conditional edges and controlled execution.

### How does it relate to Semantic Kernel?
It's an extension that maintains full compatibility with the existing Semantic Kernel, adding graph orchestration capabilities without changing the base functionality.

### What's the difference from LangGraph?
It offers similar functionality to LangGraph, but with a focus on native integration with the .NET ecosystem and Semantic Kernel, optimized for enterprise applications.

## Requirements and Compatibility

### Which .NET versions are supported?
**.NET 8+** is the minimum recommended version, with full support for all modern features.

### Does it work with existing SK code?
**Yes**, with minimal changes. It leverages existing plugins, services and connectors, only adding graph capabilities.

### Does it need external services?
**Not necessarily**. It works with minimal configuration, but can integrate with telemetry, memory and monitoring services when available.

## Features

### Is streaming supported?
**Yes**, with automatic reconnection, intelligent buffering and backpressure control.

### Does checkpointing work in production?
**Yes**, with support for persistence, compression, versioning and robust recovery.

### Does it support parallel execution?
**Yes**, with deterministic scheduler, concurrency control and state merging.

### Is visualization interactive?
**Yes**, with export to DOT, Mermaid, JSON and real-time execution overlays.

## Integration and Development

### How to integrate with existing applications?
```csharp
// Add graph support with basic configuration
builder.AddGraphSupport();

// Build the kernel
var kernel = builder.Build();

// Get the graph executor factory
var executor = kernel.GetRequiredService<IGraphExecutorFactory>();

// For advanced configuration, you can also use:
// builder.AddGraphSupport(options =>
// {
//     options.EnableLogging = true;
//     options.EnableMetrics = true;
//     options.MaxExecutionSteps = 100;
//     options.ExecutionTimeout = TimeSpan.FromMinutes(5);
// });
```

### Does it support custom plugins?
**Yes**, all existing SK plugins work as graph nodes.

### How to debug complex graphs?
* Interactive debug sessions
* Breakpoints on specific nodes
* Real-time visualization
* Detailed metrics per node

### Is there testing support?
**Yes**, with integrated testing framework and mocks for development.

## Performance and Scalability

### What's the performance overhead?
**Minimal** - only what's necessary for orchestration, with no impact on node execution.

### Does it support distributed execution?
**Yes**, with support for multiple processes and machines.

### How to handle failures?
* Configurable retry policies
* Circuit breakers
* Automatic fallbacks
* Checkpoint recovery

## Configuration and Deployment

### Does it need special configuration?
**No**, it works with zero configuration, but offers advanced options when needed.

### Does it support Docker containers?
**Yes**, with full support for containerized environments.

### How to monitor in production?
* Native metrics (.NET Metrics)
* Structured logging
* Application Insights integration
* Export to Prometheus/Grafana

## Support and Community

### Where to find help?
* [Documentation](../index.md)
* [Examples](../examples/index.md)
* [GitHub Issues](https://github.com/your-org/semantic-kernel-graph/issues)
* [Discussions](https://github.com/your-org/semantic-kernel-graph/discussions)

### How to contribute?
* Report bugs
* Suggest improvements
* Contribute examples
* Improve documentation

### Is there a public roadmap?
**Yes**, available at [Roadmap](../architecture/implementation-roadmap.md).

## Use Cases

### What types of applications is it ideal for?
* Complex AI workflows
* Data processing pipelines
* Automated decision systems
* Microservice orchestration
* Advanced chatbot applications

### Examples of production usage?
* Automated document analysis
* Content classification at scale
* Recommendation systems
* Approval workflows
* Form processing

---

## See Also

* [Getting Started](../getting-started.md)
* [Installation](../installation.md)
* [Examples](../examples/index.md)
* [Architecture](../architecture/index.md)
* [Troubleshooting](../troubleshooting.md)

## References

* [Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel/)
* [LangGraph Python](https://langchain-ai.github.io/langgraph/)
* [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)

## Complete Integration Example

Here's a complete working example that demonstrates the integration patterns described in this FAQ:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Integration;

// Create a kernel builder
var kernelBuilder = Kernel.CreateBuilder();

// Add basic graph support
kernelBuilder.AddGraphSupport();

// Add memory support
kernelBuilder.AddGraphMemory();

// Add checkpoint support with custom options
kernelBuilder.AddCheckpointSupport(options =>
{
    options.EnableCompression = true;
    options.MaxCacheSize = 1000;
    options.EnableAutoCleanup = true;
    options.AutoCleanupInterval = TimeSpan.FromHours(1);
});

// Build the kernel
var kernel = kernelBuilder.Build();

// Get the graph executor factory
var executor = kernel.GetRequiredService<IGraphExecutorFactory>();

Console.WriteLine("✅ Graph support added successfully!");
Console.WriteLine($"✅ Graph executor factory: {executor.GetType().Name}");
```

This example demonstrates:
- Basic graph integration with `AddGraphSupport()`
- Memory integration with `AddGraphMemory()`
- Checkpoint support with `AddCheckpointSupport()`
- Service retrieval from the built kernel
- Proper error handling and validation
