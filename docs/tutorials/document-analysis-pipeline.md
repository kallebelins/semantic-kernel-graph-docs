---
title: Tutorial - Document Analysis Pipeline
---

## Document Analysis Pipeline (step‑by‑step)

1) Nodes
```csharp
var extract = FunctionGraphNode.FromPlugin("Doc", "ExtractText");
var classify = FunctionGraphNode.FromPlugin("Doc", "Classify");
var summarize = FunctionGraphNode.FromPlugin("Doc", "Summarize");
var route = new SwitchGraphNode();
```

2) Routing
```csharp
route.AddCase("invoice", classify);
route.AddCase("report", summarize);
route.SetDefault(summarize);
```

3) Graph
```csharp
var graph = new GraphExecutor("doc-pipeline")
    .AddNode(extract).AddNode(route).AddNode(classify).AddNode(summarize)
    .Connect(extract, route)
    .Connect(classify, summarize);
```

4) Execute
```csharp
var args = new KernelArguments { ["file_path"] = path };
var result = await graph.ExecuteAsync(kernel, args);
```

Tips
- Use `TypedSchema` on nodes to validate inputs
- Export to Mermaid/DOT for docs
