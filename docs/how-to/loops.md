# Loops

Use `While` and `Foreach` nodes with iteration limits.

```csharp
.AddWhileNode("retry", condition: s => (int)s["attempt"] < 3)
```

- Configure break/continue
- Monitor loop metrics
