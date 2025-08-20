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

### How to migrate existing Semantic Kernel projects?
**Migration steps:**
1. **Add package reference** - Install `SemanticKernel.Graph` NuGet package
2. **Update kernel builder** - Add `AddGraphSupport()` call before `Build()`
3. **Register graph services** - Use existing DI container for service registration
4. **Test integration** - Verify plugins and services work as graph nodes
5. **Gradual adoption** - Start with simple graphs, then add complexity

### Does it need external services?
**Not necessarily**. It works with minimal configuration, but can integrate with telemetry, memory and monitoring services when available.

### What configuration files are needed?
**None required**. It works out-of-the-box, but you can optionally use:
- `appsettings.json` for environment-specific settings
- Environment variables for sensitive configuration
- Custom configuration providers for enterprise deployments

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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Integration;

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
* **Interactive debug sessions** - Step-through execution with state inspection
* **Breakpoints on specific nodes** - Conditional breakpoints and execution pauses
* **Real-time visualization** - Live graph execution with node highlighting
* **Detailed metrics per node** - Performance profiling and bottleneck identification
* **State inspection** - View and modify graph state during execution
* **Execution history** - Complete audit trail of all execution steps
* **Error context** - Detailed error information with stack traces

### Is there testing support?
**Yes**, with integrated testing framework and mocks for development.

### What are common integration issues?
**Common issues and solutions:**
- **Service not found**: Ensure `AddGraphSupport()` is called before `Build()`
- **Configuration errors**: Check that all required services are registered
- **Memory issues**: Verify `AddGraphMemory()` is called if using memory features
- **Checkpoint failures**: Ensure proper file permissions for persistence

## Performance and Scalability

### What's the performance overhead?
**Minimal** - only what's necessary for orchestration, with no impact on node execution.

### How to optimize graph performance?
**Performance optimization strategies:**
- **Parallel execution** - Enable concurrent node execution where possible
- **Caching** - Use checkpointing and state caching for repeated operations
- **Resource pooling** - Configure connection pools and resource limits
- **Async operations** - Ensure all nodes use async/await patterns
- **Memory management** - Configure appropriate memory limits and cleanup intervals
- **Monitoring** - Use built-in metrics to identify bottlenecks

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

### What about security considerations?
**Security features include:**
- **Secret management** - Integration with Azure Key Vault and environment variables
- **Authentication** - Support for Azure AD, OAuth, and custom auth providers
- **Data encryption** - At-rest and in-transit encryption for sensitive data
- **Access control** - Role-based permissions for graph execution
- **Audit logging** - Comprehensive execution audit trails

### How to monitor in production?
* **Native metrics** (.NET Metrics) - Built-in performance counters
* **Structured logging** - JSON-formatted logs with correlation IDs
* **Application Insights integration** - Azure monitoring with custom events
* **Export to Prometheus/Grafana** - Open-source monitoring stack support
* **Real-time execution tracking** - Live graph execution visualization
* **Custom telemetry** - Extensible metrics collection framework

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
* **Complex AI workflows** - Multi-step reasoning, chain-of-thought, and agent systems
* **Data processing pipelines** - ETL workflows, data validation, and transformation chains
* **Automated decision systems** - Business rule engines, approval workflows, and decision trees
* **Microservice orchestration** - Service coordination, circuit breakers, and fallback strategies
* **Advanced chatbot applications** - Multi-turn conversations, context management, and intent routing
* **Content generation** - Document creation, code generation, and creative writing workflows
* **Quality assurance** - Testing automation, validation pipelines, and quality gates

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
- **Basic graph integration** with `AddGraphSupport()`
- **Memory integration** with `AddGraphMemory()`
- **Checkpoint support** with `AddCheckpointSupport()`
- **Service retrieval** from the built kernel
- **Proper error handling** and validation

## Next Steps

After reading this FAQ, you can:

1. **Start building** - Use the integration examples above to add graph support to your project
2. **Explore examples** - Check out the [Examples](../examples/index.md) section for complete working samples
3. **Learn concepts** - Read about [Graph Concepts](../concepts/graph-concepts.md) and [Node Types](../concepts/node-types.md)
4. **Advanced features** - Discover [Advanced Patterns](../examples/advanced-patterns.md) and [Dynamic Routing](../examples/dynamic-routing.md)
5. **Get help** - Join the community discussions and report issues on GitHub

## Additional Resources

- **API Reference** - Complete [API documentation](../api/core.md)
- **Architecture Guide** - Deep dive into [system architecture](../architecture/index.md)
- **Best Practices** - Learn from [real-world examples](../examples/index.md)
- **Performance Guide** - Optimize your graphs with [performance tips](../how-to/metrics-and-observability.md)
