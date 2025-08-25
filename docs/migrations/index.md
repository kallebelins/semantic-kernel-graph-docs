---
title: Migrating from Semantic Kernel to SemanticKernel.Graph
---

## Migrating from plain Semantic Kernel

This guide shows how to move from ad-hoc pipelines built with Semantic Kernel (SK) to structured graphs using SemanticKernel.Graph with minimal changes.

### Prerequisites
* SK configured with your LLM provider
* Existing plugins/functions you want to reuse

### 1) Install and configure
```csharp
var builder = Kernel.CreateBuilder();
// ... your SK config (models, memory, logging)
// Add graph support
builder.AddGraphSupport(options =>
{
    options.EnableLogging = true;
});
var kernel = builder.Build();
```

### 2) Wrap existing functions as nodes
```csharp
// From plugin function
var myNode = FunctionGraphNode.FromFunction(myPlugin["MyFunction"], nodeId: "my-node");

// REST tool → node
var restNode = new RestToolGraphNode(new RestToolSchema { /* map inputs/outputs */ });
```

### 3) Connect nodes into a graph
```csharp
// Create executor and add nodes
var graph = new GraphExecutor("my-graph");
graph.AddNode(myNode);
graph.AddNode(restNode);

// Connect by node ids (recommended) or use Connect overloads that accept node instances
graph.Connect(myNode.NodeId, restNode.NodeId);
```

### 4) Add control-flow when needed
```csharp
var decision = new ConditionalGraphNode(state => (bool)state["should_call_api"]);
graph.AddNode(decision);

// Wire conditional paths explicitly using AddTrueNode/AddFalseNode
decision.AddTrueNode(restNode);
decision.AddFalseNode(myNode);

// Connect decision into the main flow
graph.Connect(myNode.NodeId, decision.NodeId);
```

### 5) Execute with shared state
```csharp
var args = new KernelArguments { ["input"] = userMessage };
var result = await graph.ExecuteAsync(kernel, args);
```

### Feature mapping
* SK function pipeline → `FunctionGraphNode` chain
* Conditional branching → `ConditionalGraphNode`/`SwitchGraphNode`
* Loops → `WhileLoopGraphNode`/`ForeachLoopGraphNode`
* Error handling/retry → `ErrorHandlerGraphNode`/`RetryPolicyGraphNode`
* Observability → `GraphPerformanceMetrics` + `IGraphTelemetry`
* Checkpoint/recovery → `CheckpointingExtensions`

### Migration checklist
* Reuse plugins/functions via `FunctionGraphNode`
* External APIs via `RestToolGraphNode` + `IToolSchemaConverter`
* Replace custom flow code with nodes/edges
* Enable metrics/telemetry and (optionally) checkpointing
* Add streaming if you need real-time events
