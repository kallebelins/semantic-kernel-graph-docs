# Running Examples from the Repository

Learn how to execute the comprehensive examples included in the SemanticKernel.Graph repository. This guide shows you how to use the examples program to explore different features, run specific examples, and understand the capabilities of the framework.

## Concepts and Techniques

**Examples Program**: A centralized console application that provides access to all implemented examples, allowing you to explore different features and patterns.

**Command Line Interface**: Use flags like `--list` and `--example` to navigate and execute specific examples without modifying code.

**Configuration Management**: Examples automatically load settings from `appsettings.json` and environment variables for seamless execution.

**Example Categories**: Examples are organized into logical groups covering core functionality, graph execution, reasoning patterns, and advanced workflows.

## Prerequisites and Minimum Configuration

- .NET 8.0 or later
- SemanticKernel.Graph repository cloned locally
- Basic understanding of command line interfaces
- Optional: OpenAI API key for LLM-powered examples

## Quick Start

### 1. Explore the Examples

The examples are available in the [examples/](examples/) directory of this documentation. Each example includes:

- **Code snippets** demonstrating specific features
- **Step-by-step explanations** of how the code works
- **Configuration examples** for different scenarios
- **Best practices** and usage patterns

### 2. Choose Your Example

Browse the examples by category:
- **Core Functionality**: Basic setup and configuration
- **Graph Execution**: Workflow patterns and routing
- **Advanced Patterns**: Complex AI workflows and reasoning
- **Enterprise Features**: Production-ready implementations

### 3. Follow the Tutorials

Each example includes detailed explanations and can be used as a reference for building your own graphs.

## Example Categories

The examples are organized into logical categories to help you find the right starting point:

## Available Examples

### Core Functionality Examples

- **`zero-config`**: Zero-configuration setup using KernelBuilder extensions
- **`memory`**: Integrated memory system demonstration
- **`templates`**: Handlebars template system usage
- **`logging`**: Advanced logging configuration and usage

### Graph Execution Examples

- **`conditional-nodes`**: Conditional routing with `ConditionalGraphNode`
- **`dynamic-routing`**: Dynamic routing patterns and strategies
- **`metrics`**: Performance metrics collection and analysis
- **`checkpointing`**: State persistence and recovery systems

### Reasoning and AI Examples

- **`cot`**: Chain of Thought reasoning patterns
- **`react-agent`**: ReAct (Reasoning + Acting) agent implementation
- **`react`**: ReAct problem-solving workflows
- **`memory-agent`**: Memory-augmented ReAct agents

### Advanced Patterns

- **`advanced-patterns`**: Comprehensive advanced pattern demonstrations
- **`multi-agent`**: Multi-agent coordination and workflows
- **`subgraphs`**: Subgraph isolation and scoping patterns
- **`plugins`**: Plugin system integration examples

### Specialized Workflows

- **`chatbot`**: Conversational AI chatbot implementation
- **`documents`**: Document analysis pipeline examples
- **`retrieval-agent`**: RAG (Retrieval-Augmented Generation) agents
- **`multihop-rag-retry`**: Multi-hop RAG with retry mechanisms

## Configuration

### Environment Setup

The examples program automatically loads configuration from `appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "your-api-key-here",
    "Model": "gpt-3.5-turbo",
    "MaxTokens": 4000,
    "Temperature": 0.7
  },
  "AzureOpenAI": {
    "ApiKey": "your-azure-key-here",
    "Endpoint": "https://your-endpoint.openai.azure.com/",
    "DeploymentName": "gpt-4o"
  }
}
```

### Environment Variables

You can also set configuration via environment variables:

```bash
# OpenAI Configuration
export OPENAI_API_KEY="your-api-key"
export OPENAI_MODEL="gpt-4"

# Azure OpenAI Configuration
export AZURE_OPENAI_API_KEY="your-azure-key"
export AZURE_OPENAI_ENDPOINT="https://your-endpoint.openai.azure.com/"
export AZURE_OPENAI_DEPLOYMENT_NAME="gpt-4o"

# Test Configuration
export ENABLE_REAL_API_TESTS="true"
```

## Example Execution Workflow

### 1. Explore Available Examples

Start by listing all available examples:

```bash
dotnet run -- --list
```

### 2. Run a Simple Example

Begin with a basic example to verify your setup:

```bash
dotnet run -- --example zero-config
```

### 3. Run Related Examples

Execute examples that demonstrate related functionality:

```bash
dotnet run -- --example memory,memory-agent
```

### 4. Run Complex Examples

Execute more sophisticated examples once you're comfortable:

```bash
dotnet run -- --example advanced-patterns,multi-agent
```

## Troubleshooting

### Common Issues

**Build errors**: Ensure you're in the correct directory and have all dependencies installed.

**Configuration errors**: Check that your `appsettings.json` is properly formatted and API keys are valid.

**Example not found**: Use `--list` to verify the exact example name (case-insensitive).

**API key issues**: Verify your OpenAI or Azure OpenAI configuration in `appsettings.json`.

### Performance Tips

- Start with simple examples to understand the framework
- Use specific example selection (`--example`) rather than running all examples
- Monitor console output for execution progress and any warnings
- Check the configuration section of each example for specific requirements

## Advanced Usage

### Custom Example Execution

You can modify the examples program to add your own examples:

```csharp
// Add your custom example to the examples dictionary
examples["my-custom-example"] = async () => await MyCustomExample.RunAsync();

// Your example will then be available via --example my-custom-example
```

### Integration with Development

Use examples during development to test new features:

```bash
# Test specific functionality
dotnet run -- --example metrics,checkpointing

# Verify changes work correctly
dotnet run -- --example advanced-patterns
```

### Continuous Integration

Examples can be integrated into CI/CD pipelines:

```bash
# Run all examples in CI
dotnet run -- --example advanced-patterns,multi-agent,subgraphs

# Exit after completion (no user interaction)
# Modify Program.cs to remove Console.ReadKey() calls for CI
```

## See Also

- **Reference**: [Examples Index](../examples/index.md)
- **Guides**: [Getting Started](../getting-started.md), [Installation](../installation.md)
- **Examples**: Individual example documentation for detailed explanations
- **Configuration**: [Environment Setup](../how-to/environment-setup.md)

## Reference APIs

- **[Examples Index](../examples/index.md)**: Overview of available example types
- **[Configuration Options](../how-to/environment-setup.md)**: Environment and configuration setup
- **[CLI Interface](../examples/cli-interface.md)**: Command line interface documentation
- **[Example Categories](../examples/index.md)**: Detailed breakdown of example types
