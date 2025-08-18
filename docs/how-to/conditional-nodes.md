# Conditional nodes

Use predicates or template expressions to branch.

```csharp
.AddConditionalNode("branch", predicate: s => s.ContainsKey("ok") && (bool)s["ok"]) 
  .Then("success")
  .Else("fallback")
```

Tips:
- Cache evaluations for performance
- Add debug annotations to inspect decisions
