# How to build a graph

Step-by-step:

1. Create and configure a `Kernel`
2. Define nodes (functions, conditionals, loops)
3. Connect with conditional edges
4. Execute with `GraphExecutor`

```csharp
var graph = GraphBuilder.Create()
    .AddFunctionNode("plan", kernel, "Planner", "Plan")
    .When(state => state.GetString("needs_analysis") == "yes")
        .AddFunctionNode("analyze", kernel, "Analyzer", "Analyze")
    .AddFunctionNode("act", kernel, "Executor", "Act")
    .Build();
```
