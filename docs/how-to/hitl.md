# Human-in-the-loop

Pause execution and wait for approval or input.

```csharp
.AddHumanApprovalNode("approve_step", timeout: TimeSpan.FromMinutes(10))
```

- Supports CLI, web, API channels
- Batch approvals for multiple pending decisions
