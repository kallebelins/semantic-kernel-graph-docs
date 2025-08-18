---
title: Tutorial - Multi‑Agent Workflow
---

## Multi‑Agent Workflow (step‑by‑step)

1) Create coordinator
```csharp
var coordinator = new MultiAgentCoordinator(new MultiAgentOptions());
```

2) Register agents (graphs)
```csharp
var planner = new GraphExecutor("planner");
var worker = new GraphExecutor("worker");
coordinator.Register("planner", planner);
coordinator.Register("worker", worker);
```

3) Distribute work
```csharp
await coordinator.EnqueueAsync(new { task = "research topic" });
await coordinator.RunAsync(kernel, CancellationToken.None);
```

Tips
- Share state via `SharedStateManager`
- Set priorities with `ResourceGovernor`
