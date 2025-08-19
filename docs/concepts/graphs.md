# Graphs

A graph is a directed network of nodes connected by edges. Execution starts at an entry node and proceeds by evaluating routing conditions.

## Concepts and Techniques

**Computational Graph**: Data structure that represents a workflow or processing pipeline through nodes and connections.

**Entry Node**: Starting point of graph execution, defined as `StartNode`.

**Directed Edge**: Connection between two nodes that defines the direction of execution flow.

**Graph Validation**: Integrity verification before execution to ensure the graph is valid.

## Graph Structure

### Basic Components
```csharp
var graph = new Graph
{
    Id = "workflow-001",
    Name = "Document Processing",
    Description = "Pipeline for document analysis and classification",
    StartNode = startNode,
    Nodes = new[] { startNode, processNode, classifyNode, endNode },
    Edges = new[] { edge1, edge2, edge3 }
};
```

### Nodes and Edges
- **Nodes**: Encapsulate work (SK functions, loops, subgraphs, tools)
- **Edges**: Carry optional conditions to control flow
- **Validation**: The engine ensures validity before execution

## Graph Types

### Linear Graph
```csharp
// Simple sequence: A → B → C
var linearGraph = new Graph
{
    StartNode = nodeA,
    Nodes = new[] { nodeA, nodeB, nodeC },
    Edges = new[] 
    { 
        new Edge(nodeA, nodeB),
        new Edge(nodeB, nodeC)
    }
};
```

### Conditional Graph
```csharp
// Graph with conditional branches
var conditionalGraph = new Graph
{
    StartNode = startNode,
    Nodes = new[] { startNode, processNode, successNode, failureNode },
    Edges = new[] 
    { 
        new ConditionalEdge(startNode, processNode),
        new ConditionalEdge(processNode, successNode, 
            condition: state => state.GetValue<int>("status") == 200),
        new ConditionalEdge(processNode, failureNode, 
            condition: state => state.GetValue<int>("status") != 200)
    }
};
```

### Loop Graph
```csharp
// Graph with controlled iteration
var loopGraph = new Graph
{
    StartNode = startNode,
    Nodes = new[] { startNode, loopNode, endNode },
    Edges = new[] 
    { 
        new Edge(startNode, loopNode),
        new ConditionalEdge(loopNode, loopNode, 
            condition: state => state.GetValue<int>("counter") < 10),
        new ConditionalEdge(loopNode, endNode, 
            condition: state => state.GetValue<int>("counter") >= 10)
    }
};
```

## Validation and Integrity

### Validation Checks
```csharp
var validator = new WorkflowValidator();
var validationResult = await validator.ValidateAsync(graph);

if (!validationResult.IsValid)
{
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"Validation error: {error.Message}");
    }
}
```

### Validation Rules
- **Connectivity**: All nodes must be reachable
- **Cycles**: Detection of infinite loops
- **Types**: Validation of input/output types
- **Dependencies**: Circular dependency checks

## Graph Construction

### Programmatic Construction
```csharp
var graphBuilder = new GraphBuilder();

var graph = await graphBuilder
    .AddNode(startNode)
    .AddNode(processNode)
    .AddNode(endNode)
    .AddEdge(startNode, processNode)
    .AddEdge(processNode, endNode)
    .SetStartNode(startNode)
    .BuildAsync();
```

### Template Construction
```csharp
var template = new ChainOfThoughtWorkflowTemplate();
var graph = await template.CreateGraphAsync(
    kernel: kernel,
    options: new TemplateOptions
    {
        MaxSteps = 5,
        EnableReasoning = true
    }
);
```

### DSL Construction
```csharp
var dslParser = new GraphDslParser();
var graphDefinition = @"
    start -> process -> classify -> end
    process -> retry if error
    retry -> process if attempts < 3
";

var graph = await dslParser.ParseAsync(dslDefinition);
```

## Execution and Control

### Basic Execution
```csharp
var executor = new GraphExecutor();
var arguments = new KernelArguments
{
    ["input"] = "documento.pdf",
    ["maxRetries"] = 3
};

var result = await executor.ExecuteAsync(graph, arguments);
```

### Streaming Execution
```csharp
var streamingExecutor = new StreamingGraphExecutor();
var eventStream = await streamingExecutor.ExecuteStreamingAsync(graph, arguments);

await foreach (var evt in eventStream)
{
    Console.WriteLine($"Event: {evt.Type} at node {evt.NodeId}");
}
```

### Checkpointing Execution
```csharp
var checkpointingExecutor = new CheckpointingGraphExecutor();
var result = await checkpointingExecutor.ExecuteAsync(graph, arguments);

// Save checkpoint
var checkpoint = await checkpointingExecutor.CreateCheckpointAsync();

// Restore execution
var restoredResult = await checkpointingExecutor.RestoreFromCheckpointAsync(checkpoint);
```

## Metadata and Documentation

### Graph Information
```csharp
var graphMetadata = new GraphMetadata
{
    Version = "1.0.0",
    Author = "Development Team",
    CreatedAt = DateTime.UtcNow,
    Tags = new[] { "documents", "classification", "AI" },
    EstimatedExecutionTime = TimeSpan.FromMinutes(5),
    ResourceRequirements = new ResourceRequirements
    {
        MaxMemory = "2GB",
        MaxCpu = "4 cores"
    }
};
```

### Automatic Documentation
```csharp
var docGenerator = new GraphDocumentationGenerator();
var documentation = await docGenerator.GenerateAsync(graph, 
    new DocumentationOptions
    {
        IncludeCodeExamples = true,
        IncludeDiagrams = true,
        Format = DocumentationFormat.Markdown
    }
);
```

## Monitoring and Observability

### Execution Metrics
```csharp
var metrics = new GraphPerformanceMetrics
{
    TotalExecutionTime = TimeSpan.FromSeconds(45),
    NodeExecutionTimes = new Dictionary<string, TimeSpan>(),
    ExecutionPath = new[] { "start", "process", "classify", "end" },
    ResourceUsage = new ResourceUsageMetrics()
};
```

### Logging and Tracing
```csharp
var logger = new SemanticKernelGraphLogger();
logger.LogGraphExecutionStart(graph.Id, executionId);
logger.LogGraphExecutionComplete(graph.Id, executionId, result);
logger.LogGraphValidation(graph.Id, validationResult);
```

## See Also

- [Graph Concepts](../concepts/graph-concepts.md)
- [Node Types](../concepts/node-types.md)
- [Routing](../concepts/routing.md)
- [Execution](../concepts/execution.md)
- [Building a Graph](../how-to/build-a-graph.md)
- [Subgraph Examples](../examples/subgraph-examples.md)

## References

- `Graph`: Main class for representing computational graphs
- `GraphBuilder`: Fluent builder for graphs
- `WorkflowValidator`: Graph integrity validator
- `GraphExecutor`: Main graph executor
- `GraphDocumentationGenerator`: Automatic documentation generator
- `GraphPerformanceMetrics`: Execution performance metrics
