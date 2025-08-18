---
title: Tutorial - Chatbot with Memory
---

## Chatbot with Memory (step‑by‑step)

1) Configure SK and Graph
```csharp
var builder = Kernel.CreateBuilder();
// model + memory
builder.AddGraphSupport();
var kernel = builder.Build();
```

2) Build nodes
```csharp
var chat = FunctionGraphNode.FromPlugin("ChatPlugin", "Chat");
var recall = new RestToolGraphNode(new RestToolSchema { /* semantic recall */ });
var observe = new ObservationGraphNode();
```

3) Wire control flow
```csharp
var decideRecall = new ConditionalGraphNode(s => (bool)(s["need_recall"] ?? false));
var graph = new GraphExecutor("chatbot")
    .AddNode(chat).AddNode(recall).AddNode(observe).AddNode(decideRecall)
    .Connect(chat, decideRecall)
    .AddConditionalEdge(decideRecall, whenTrue: recall, whenFalse: observe)
    .Connect(recall, observe);
```

4) Run with state and persist
```csharp
var args = new KernelArguments { ["input"] = userMessage };
var result = await graph.ExecuteAsync(kernel, args);
// store dialog in memory provider if desired
```

Tips
- Enable `CheckpointingExtensions` for long sessions
- Use `IGraphTelemetry` for live monitoring
