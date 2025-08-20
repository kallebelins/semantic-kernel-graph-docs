# Templates and Memory Integration

This guide explains how to use SemanticKernel.Graph's template engine system and integrate with memory services for enhanced graph execution capabilities.

## Overview

SemanticKernel.Graph provides a comprehensive template and memory system that enables:

* **Dynamic prompt generation** using template engines with variable substitution
* **Workflow templates** for reusable graph patterns
* **Memory integration** for storing and retrieving execution context
* **Vector and semantic search** capabilities for enhanced decision-making

## Template Engine System

### Core Components

The template system consists of several key components:

* **`IGraphTemplateEngine`**: Interface for template rendering engines
* **`GraphTemplateOptions`**: Configuration options for template functionality
* **`IWorkflowTemplate`**: Blueprint for creating executable graphs
* **`WorkflowTemplateRegistry`**: Central registry for discovering and using templates

### Template Engine Types

#### HandlebarsGraphTemplateEngine

The default template engine that provides Handlebars-like syntax:

```csharp
// Enable Handlebars templates
builder.AddGraphTemplates(opts =>
{
    opts.EnableHandlebars = true;
    opts.EnableCustomHelpers = true;
    opts.TemplateCacheSize = 100;
});
```

**Features:**
* Variable substitution: `{{variable}}`
* Helper functions: `{{helper arg1 arg2}}`
* Conditional statements: `{{#if condition}}...{{else}}...{{/if}}`
* Template compilation and caching
* Custom helper registration

#### Specialized Template Engines

**ChainOfThoughtTemplateEngine**: Optimized for reasoning patterns with domain-specific templates and progressive step adaptation.

**ReActTemplateEngine**: Specialized for ReAct pattern prompts with domain optimization and context-aware rendering.

### Using Templates in Graph Nodes

Templates can be used directly in graph nodes for dynamic content generation:

```csharp
var templateEngine = serviceProvider.GetService<IGraphTemplateEngine>();

// Render template with context
var prompt = await templateEngine.RenderAsync(
    "Hello {{name}}, your current status is {{status}}",
    new { name = "User", status = "active" }
);

// Render with graph state
var statePrompt = await templateEngine.RenderWithStateAsync(
    "Previous result: {{previous_result}}",
    graphState
);
```

### Custom Template Helpers

Register custom functions for use in templates:

```csharp
// Synchronous helper
templateEngine.RegisterHelper("formatDate", args =>
{
    if (args.Length > 0 && args[0] is DateTime date)
        return date.ToString("yyyy-MM-dd");
    return "Invalid date";
});

// Asynchronous helper (useful for memory lookups)
templateEngine.RegisterAsyncHelper("searchMemory", async args =>
{
    var query = args[0]?.ToString() ?? "";
    var memoryService = serviceProvider.GetService<IGraphMemoryService>();
    var results = await memoryService.SearchRelevantMemoryAsync("node1", query);
    return string.Join(", ", results.Select(r => r.Content));
});
```

## Workflow Templates

### Built-in Templates

SemanticKernel.Graph includes several pre-built workflow templates:

#### Chatbot Template (`chatbot/basic`)

Creates a basic chatbot workflow with optional reasoning:

```csharp
var template = registry.GetLatest("chatbot/basic");
var parameters = new Dictionary<string, object>
{
    ["graph_name"] = "my-chatbot",
    ["use_reasoning"] = true,
    ["responder_plugin"] = "ChatPlugin",
    ["responder_function"] = "Respond"
};

var executor = template.BuildGraph(kernel, parameters, serviceProvider);
```

#### Chain-of-Thought Template (`reasoning/cot-basic`)

Creates a reasoning node with configurable parameters:

```csharp
var template = registry.GetLatest("reasoning/cot-basic");
var parameters = new Dictionary<string, object>
{
    ["graph_name"] = "reasoning-graph",
    ["type"] = ChainOfThoughtType.ProblemSolving,
    ["max_steps"] = 8
};

var executor = template.BuildGraph(kernel, parameters, serviceProvider);
```

#### ReAct Template (`react/loop-basic`)

Creates a ReAct loop with reasoning and action selection:

```csharp
var template = registry.GetLatest("react/loop-basic");
var parameters = new Dictionary<string, object>
{
    ["graph_name"] = "react-agent",
    ["domain"] = ReActDomain.ProblemSolving,
    ["max_iterations"] = 15,
    ["goal_threshold"] = 0.9
};

var executor = template.BuildGraph(kernel, parameters, serviceProvider);
```

#### Document Analysis Template (`analysis/document-basic`)

Creates a document processing pipeline:

```csharp
var template = registry.GetLatest("analysis/document-basic");
var parameters = new Dictionary<string, object>
{
    ["graph_name"] = "doc-pipeline",
    ["ingest_plugin"] = "DocumentPlugin",
    ["ingest_function"] = "IngestDocument",
    ["analyze_plugin"] = "AnalysisPlugin",
    ["analyze_function"] = "AnalyzeContent",
    ["summarize_plugin"] = "SummaryPlugin",
    ["summarize_function"] = "GenerateSummary"
};

var executor = template.BuildGraph(kernel, parameters, serviceProvider);
```

### Template Categories and Capabilities

Templates are categorized by purpose and declare required capabilities:

```csharp
public enum TemplateCategory
{
    Chatbot,      // Conversational workflows
    Analysis,     // Data processing pipelines
    Reasoning,    // Cognitive reasoning patterns
    MultiAgent,   // Multi-agent coordination
    Integration,  // External system integration
    Custom        // User-defined templates
}

[Flags]
public enum TemplateCapabilities
{
    None = 0,
    Templates = 1 << 0,        // Basic template support
    DynamicRouting = 1 << 1,   // Dynamic routing capabilities
    Memory = 1 << 2,           // Memory integration required
    Streaming = 1 << 3,        // Streaming execution support
    Checkpointing = 1 << 4,    // Checkpointing capabilities
    Recovery = 1 << 5,         // Recovery mechanisms
}
```

### Creating Custom Templates

Implement `IWorkflowTemplate` to create custom workflow templates:

```csharp
public sealed class CustomWorkflowTemplate : IWorkflowTemplate
{
    public string TemplateId => "custom/my-workflow";
    public string Name => "Custom Workflow";
    public string Version => "1.0.0";
    public TemplateCategory Category => TemplateCategory.Custom;
    public string Description => "Custom workflow for specific use case";
    public TemplateCapabilities RequiredCapabilities => TemplateCapabilities.Templates;

    public IReadOnlyList<TemplateParameter> Parameters => new List<TemplateParameter>
    {
        new() { Name = "graph_name", Required = true },
        new() { Name = "custom_param", Description = "Custom parameter", DefaultValue = "default" }
    }.AsReadOnly();

    public WorkflowTemplateValidationResult ValidateParameters(IDictionary<string, object> parameters)
    {
        var errors = new List<string>();
        if (!parameters.ContainsKey("graph_name"))
            errors.Add("Missing required parameter 'graph_name'");
        
        return errors.Count == 0 
            ? WorkflowTemplateValidationResult.Success() 
            : WorkflowTemplateValidationResult.Failure(errors);
    }

    public GraphExecutor BuildGraph(Kernel kernel, IDictionary<string, object> parameters, IServiceProvider serviceProvider)
    {
        var validation = ValidateParameters(parameters);
        if (!validation.IsValid)
            throw new ArgumentException($"Invalid parameters: {string.Join(", ", validation.Errors)}");

        var name = parameters["graph_name"].ToString()!;
        var executor = new GraphExecutor(name, $"Custom workflow ({Name} {Version})");
        
        // Build your custom graph here
        // Add nodes, connections, etc.
        
        return executor;
    }
}
```

## Memory Integration

### Memory Service Configuration

Enable memory integration with configurable options:

```csharp
builder.AddGraphMemory(opts =>
{
    opts.EnableVectorSearch = true;
    opts.EnableSemanticSearch = true;
    opts.DefaultCollectionName = "graph-memory";
    opts.SimilarityThreshold = 0.7;
});
```

### Memory Service Features

The `IGraphMemoryService` provides several key capabilities:

#### Storing Execution Context

```csharp
var memoryService = serviceProvider.GetService<IGraphMemoryService>();

await memoryService.StoreExecutionContextAsync(
    executionId: "exec-123",
    graphState: currentState,
    metadata: new Dictionary<string, object>
    {
        ["nodeCount"] = 5,
        ["executionTime"] = TimeSpan.FromSeconds(10)
    }
);
```

#### Finding Similar Executions

```csharp
var similarExecutions = await memoryService.FindSimilarExecutionsAsync(
    currentState: currentState,
    limit: 5,
    minSimilarity: 0.8
);

foreach (var execution in similarExecutions)
{
    Console.WriteLine($"Similar execution: {execution.ExecutionId} (score: {execution.SimilarityScore:F3})");
}
```

#### Node-Specific Memory

```csharp
// Store node execution results
await memoryService.StoreNodeExecutionAsync(
    nodeId: "reasoning-node",
    input: inputArgs,
    output: functionResult,
    executionContext: "exec-123"
);

// Search relevant memory for a node
var relevantMemory = await memoryService.SearchRelevantMemoryAsync(
    nodeId: "reasoning-node",
    query: "How to solve this type of problem?",
    limit: 10
);
```

### Memory Provider Integration

For advanced memory scenarios, implement `IGraphMemoryProvider`:

```csharp
public sealed class CustomMemoryProvider : IGraphMemoryProvider
{
    public bool IsAvailable => true;

    public async Task SaveInformationAsync(string collectionName, string text, string id,
        string? description = null, string? additionalMetadata = null,
        CancellationToken cancellationToken = default)
    {
        // Implement custom storage logic
        // e.g., save to database, vector store, etc.
    }

    public async Task<IAsyncEnumerable<MemorySearchResult>> SearchAsync(
        string collectionName, string query, int limit = 10, 
        double minRelevanceScore = 0.7, CancellationToken cancellationToken = default)
    {
        // Implement custom search logic
        // e.g., vector similarity search, semantic search, etc.
    }
}
```

## Advanced Configuration

### Complete Graph Support

Use the comprehensive configuration for all features:

```csharp
builder.AddCompleteGraphSupport(opts =>
{
    opts.EnableTemplates = true;
    opts.EnableMemory = true;
    opts.EnableVectorSearch = true;
    opts.EnableSemanticSearch = true;
    opts.EnableCustomHelpers = true;
    opts.MaxExecutionSteps = 1000;
});
```

### Template Engine Selection

Choose specific template engines based on your needs:

```csharp
// For reasoning-heavy workflows
builder.Services.AddSingleton<IGraphTemplateEngine, ChainOfThoughtTemplateEngine>();

// For ReAct patterns
builder.Services.AddSingleton<IGraphTemplateEngine, ReActTemplateEngine>();

// For general purpose
builder.Services.AddSingleton<IGraphTemplateEngine, HandlebarsGraphTemplateEngine>();
```

## Best Practices

### Template Design

1. **Parameter Validation**: Always validate template parameters before building graphs
2. **Default Values**: Provide sensible defaults for optional parameters
3. **Capability Declaration**: Accurately declare required capabilities
4. **Versioning**: Use semantic versioning for template updates

### Memory Usage

1. **Context Storage**: Store execution context for future reference and learning
2. **Similarity Thresholds**: Adjust similarity thresholds based on your use case
3. **Memory Cleanup**: Implement cleanup strategies for old memory entries
4. **Metadata**: Use metadata to categorize and filter memory entries

### Performance Considerations

1. **Template Caching**: Leverage template compilation and caching
2. **Memory Limits**: Set appropriate limits for memory storage and search
3. **Async Operations**: Use async helpers for external service calls
4. **Batch Operations**: Group memory operations when possible

## Troubleshooting

### Common Issues

**Template Rendering Errors**
* Ensure all required variables are provided in the context
* Check template syntax for proper Handlebars formatting
* Verify custom helpers are properly registered

**Memory Integration Issues**
* Confirm memory service is properly configured and registered
* Check similarity thresholds are appropriate for your data
* Verify memory provider availability

**Template Registry Problems**
* Ensure templates are properly registered in the DI container
* Check template ID and version uniqueness
* Verify template parameter validation logic

### Debugging Tips

1. **Enable Logging**: Use the built-in logging to trace template and memory operations
2. **Template Validation**: Use `ValidateParameters` to catch configuration issues early
3. **Memory Inspection**: Check memory service logs for storage and retrieval operations
4. **Template Compilation**: Verify templates compile correctly before execution

## See Also

* [Graph Concepts](../concepts/graph-concepts.md) - Core graph concepts and terminology
* [State Management](../how-to/state-management.md) - Working with graph state and arguments
* [Conditional Nodes](../how-to/conditional-nodes.md) - Dynamic routing and decision making
* [Multi-Agent Workflows](../how-to/multi-agent-and-shared-state.md) - Coordinating multiple agents
* [API Reference](../api/) - Complete API documentation for template and memory types
