# Nodes

Nodes are the fundamental components of a graph, each encapsulating a specific unit of work or control logic.

## Concepts and Techniques

**Graph Node**: Fundamental processing unit that encapsulates work, control logic or specific operations.

**Node Lifecycle**: Sequence of events that occurs during execution: Before → Execute → After.

**IGraphNode Interface**: Base contract that all nodes must implement for system integration.

## Node Types

### Function Nodes
```csharp
// Encapsulates a Semantic Kernel function
var functionNode = new FunctionGraphNode(
    function: kernel.GetFunction("analyze_document"),
    name: "analyze_document",
    description: "Analyzes the content of a document"
);
```

**Characteristics**:
* **Encapsulation**: Wraps `KernelFunction`
* **Synchronous Execution**: Direct function processing
* **Shared State**: Access to global `GraphState`
* **Metrics**: Automatic performance collection

### Conditional Nodes
```csharp
// Node that makes decisions based on conditions
var conditionalNode = new ConditionalGraphNode(
    condition: async (state) => {
        var score = state.GetValue<double>("confidence");
        return score > 0.8;
    },
    name: "confidence_gate",
    description: "Filters results by confidence level"
);
```

**Characteristics**:
* **Condition Evaluation**: Boolean expressions over state
* **Dynamic Routing**: Runtime decisions
* **SK Templates**: Use of Semantic Kernel functions for decisions
* **Fallbacks**: Fallback strategies for unmet conditions

### Reasoning Nodes
```csharp
// Node that implements reasoning patterns
var reasoningNode = new ReasoningGraphNode(
    kernel: kernel,
    prompt: "Analyze the problem and suggest a solution",
    maxSteps: 5,
    name: "problem_solver",
    description: "Solves problems using step-by-step reasoning"
);
```

**Characteristics**:
* **Chain of Thought**: Step-by-step reasoning
* **Few-shot Learning**: Examples to guide reasoning
* **Result Validation**: Quality verification of responses
* **Controlled Iteration**: Limits to avoid infinite loops

### Loop Nodes
```csharp
// Node that implements controlled iterations
var loopNode = new ReActLoopGraphNode(
    kernel: kernel,
    objective: "Classify all documents",
    maxIterations: 10,
    name: "document_classifier",
    description: "Iteratively classifies documents"
);
```

**Characteristics**:
* **ReAct Pattern**: Observation → Thinking → Action
* **Clear Objectives**: Specific goals for each iteration
* **Iteration Control**: Limits to avoid infinite loops
* **Persistent State**: Context maintenance between iterations

### Observation Nodes
```csharp
// Node that observes and records information
var observationNode = new ObservationGraphNode(
    observer: new ConsoleObserver(),
    name: "console_logger",
    description: "Records observations in console"
);
```

**Characteristics**:
* **Passive Observation**: Does not modify state
* **Logging**: Recording of execution information
* **Metrics**: Performance data collection
* **Debug**: Troubleshooting support

### Subgraph Nodes
```csharp
// Node that encapsulates another graph
var subgraphNode = new SubgraphGraphNode(
    subgraph: documentAnalysisGraph,
    name: "document_analysis",
    description: "Complete document analysis pipeline"
);
```

**Characteristics**:
* **Composition**: Reuse of existing graphs
* **Encapsulation**: Clean interface for complex graphs
* **Isolated State**: Variable scope control
* **Reusability**: Reusable modules in different contexts

### Error Handler Nodes
```csharp
// Node that handles exceptions and failures
var errorHandlerNode = new ErrorHandlerGraphNode(
    errorPolicy: new RetryPolicy(maxRetries: 3),
    name: "error_handler",
    description: "Handles errors and implements retry policies"
);
```

**Characteristics**:
* **Error Policies**: Retry, backoff, circuit breaker
* **Recovery**: Strategies for handling failures
* **Logging**: Detailed error recording
* **Fallbacks**: Alternatives when main operation fails

### Human Approval Nodes
```csharp
// Node that pauses for human interaction
var approvalNode = new HumanApprovalGraphNode(
    channel: new ConsoleHumanInteractionChannel(),
    timeout: TimeSpan.FromMinutes(30),
    name: "human_approval",
    description: "Waits for human approval to continue"
);
```

**Characteristics**:
* **Human Interaction**: Pause for human input
* **Timeouts**: Time limits for response
* **Multiple Channels**: Console, web, email
* **Audit**: Recording of human decisions

## Node Lifecycle

### Before Phase
```csharp
public override async Task BeforeExecutionAsync(GraphExecutionContext context)
{
    // Input validation
    await ValidateInputAsync(context.State);
    
    // Resource initialization
    await InitializeResourcesAsync();
    
    // Start logging
    _logger.LogInformation($"Starting execution of node {Id}");
}
```

### Execute Phase
```csharp
public override async Task<GraphExecutionResult> ExecuteAsync(GraphExecutionContext context)
{
    try
    {
        // Main execution
        var result = await ProcessAsync(context.State);
        
        // State update
        context.State.SetValue("output", result);
        
        return GraphExecutionResult.Success(result);
    }
    catch (Exception ex)
    {
        return GraphExecutionResult.Failure(ex);
    }
}
```

### After Phase
```csharp
public override async Task AfterExecutionAsync(GraphExecutionContext context)
{
    // Resource cleanup
    await CleanupResourcesAsync();
    
    // Performance logging
    _logger.LogInformation($"Node {Id} completed in {context.ExecutionTime}");
    
    // Counter updates
    UpdateExecutionMetrics(context);
}
```

## Configuration and Options

### Node Options
```csharp
var nodeOptions = new GraphNodeOptions
{
    EnableMetrics = true,
    EnableLogging = true,
    MaxExecutionTime = TimeSpan.FromMinutes(5),
    RetryPolicy = new ExponentialBackoffRetryPolicy(maxRetries: 3),
    CircuitBreaker = new CircuitBreakerOptions
    {
        FailureThreshold = 5,
        RecoveryTimeout = TimeSpan.FromMinutes(1)
    }
};
```

### Input/Output Validation
```csharp
var inputSchema = new GraphNodeInputSchema
{
    Required = new[] { "document", "language" },
    Optional = new[] { "confidence_threshold" },
    Types = new Dictionary<string, Type>
    {
        ["document"] = typeof(string),
        ["language"] = typeof(string),
        ["confidence_threshold"] = typeof(double)
    }
};

var outputSchema = new GraphNodeOutputSchema
{
    Properties = new[] { "analysis_result", "confidence_score" },
    Types = new Dictionary<string, Type>
    {
        ["analysis_result"] = typeof(string),
        ["confidence_score"] = typeof(double)
    }
};
```

## Monitoring and Observability

### Node Metrics
```csharp
var nodeMetrics = new NodeExecutionMetrics
{
    ExecutionCount = 150,
    AverageExecutionTime = TimeSpan.FromMilliseconds(250),
    SuccessRate = 0.98,
    LastExecutionTime = DateTime.UtcNow,
    ErrorCount = 3,
    ResourceUsage = new ResourceUsageMetrics()
};
```

### Structured Logging
```csharp
_logger.LogInformation("Node execution started", new
{
    NodeId = Id,
    NodeType = GetType().Name,
    InputKeys = context.State.GetKeys(),
    ExecutionId = context.ExecutionId,
    Timestamp = DateTime.UtcNow
});
```

## See Also

* [Node Types](../concepts/node-types.md)
* [Conditional Nodes](../how-to/conditional-nodes.md)
* [Loops](../how-to/loops.md)
* [Human-in-the-Loop](../how-to/hitl.md)
* [Error Handling](../how-to/error-handling-and-resilience.md)
* [Node Examples](../examples/conditional-nodes.md)

## References

* `IGraphNode`: Base interface for all nodes
* `FunctionGraphNode`: Node that encapsulates SK functions
* `ConditionalGraphNode`: Node for conditional decisions
* `ReasoningGraphNode`: Node for step-by-step reasoning
* `ReActLoopGraphNode`: Node for ReAct loops
* `ObservationGraphNode`: Node for observation and logging
* `SubgraphGraphNode`: Node that encapsulates other graphs
* `ErrorHandlerGraphNode`: Node for error handling
* `HumanApprovalGraphNode`: Node for human interaction
