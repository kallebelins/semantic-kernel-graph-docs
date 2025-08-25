# Human-in-the-loop

This guide explains how to implement Human-in-the-Loop (HITL) workflows in SemanticKernel.Graph. You'll learn how to pause execution for human approval, implement confidence gates, and integrate multiple interaction channels for seamless human oversight.

## Overview

Human-in-the-Loop workflows enable you to:
* **Pause execution** and wait for human approval or input
* **Implement confidence gates** for quality control
* **Support multiple channels** including CLI, web, and API interfaces
* **Batch approvals** for multiple pending decisions
* **Set timeouts and SLAs** for human response requirements

## Core HITL Components

### Human Approval Node

Use `HumanApprovalGraphNode` to pause execution for human decision. The node requires an
`IHumanInteractionChannel` instance (for example `ConsoleHumanInteractionChannel`).

```csharp
// Create a console channel that will prompt the approver via the terminal.
var consoleChannel = new ConsoleHumanInteractionChannel();

// Build a HumanApprovalGraphNode that pauses graph execution and requests
// human approval. Provide a short, clear title and message so approvers
// understand the requested action.
var approvalNode = new HumanApprovalGraphNode(
    approvalTitle: "Document Approval",
    approvalMessage: "Please review and approve the generated document (type 'approve' or 'reject')",
    interactionChannel: consoleChannel,
    nodeId: "approve_step"
)
    // Configure a reasonable timeout and a default action to avoid
    // indefinitely blocking automated runs. TimeoutAction.Reject will
    // choose the rejection path if the approver does not respond.
    .WithTimeout(TimeSpan.FromMinutes(10), TimeoutAction.Reject);

// Register the node in a GraphExecutor and wire the two possible outcomes.
executor.AddNode(approvalNode);
executor.Connect(approvalNode.NodeId, "approved_path");
executor.Connect(approvalNode.NodeId, "rejected_path");
```

### Confidence Gate Node

Implement confidence-based decision gates. If you want an interaction channel for
human overrides, use the factory method that accepts a channel.

```csharp
// Create a confidence gate node that can route automatically based on
// a numeric score. Optionally attach an interaction channel so humans
// can override or inspect low-confidence cases.
var consoleChannel = new ConsoleHumanInteractionChannel();
var confidenceNode = ConfidenceGateGraphNode.CreateWithInteraction(
    confidenceThreshold: 0.8f,
    interactionChannel: consoleChannel,
    nodeId: "confidence_check"
);

// Add the node and route outcomes using ConnectWhen with a predicate
// that inspects graph state. Use GetOrCreateGraphState() to read
// values produced earlier in the graph.
executor.AddNode(confidenceNode);
executor.ConnectWhen("confidence_check", "high_confidence", args =>
    args.GetOrCreateGraphState().GetFloat("confidence_score", 0f) >= 0.8f);
executor.ConnectWhen("confidence_check", "human_review", args =>
    args.GetOrCreateGraphState().GetFloat("confidence_score", 0f) < 0.8f);
```

## Multiple Interaction Channels

### Console Channel

Use console-based human interaction. Initialize the channel with a simple
configuration dictionary (the channel exposes `InitializeAsync` for configuration).

```csharp
// Configure and initialize a console-based interaction channel.
var consoleChannel = new ConsoleHumanInteractionChannel();
await consoleChannel.InitializeAsync(new Dictionary<string, object>
{
    // Choose a prompt style that suits the approval scenario.
    ["prompt_style"] = "detailed",
    // Enable colors for readability when running in a supporting terminal.
    ["enable_colors"] = true,
    // Optionally show timestamps to help approvers track requests.
    ["show_timestamps"] = true
});

// Create an approval node using the initialized console channel and
// protect against long-running waits with a timeout.
var approvalNode = new HumanApprovalGraphNode(
    approvalTitle: "Console Approval",
    approvalMessage: "Enter 'approve' or 'reject'",
    interactionChannel: consoleChannel,
    nodeId: "console_approval"
).WithTimeout(TimeSpan.FromMinutes(10));
```

### Web API Channel

Implement web-based approval interfaces. The library provides a Web API-backed
channel that cooperates with an `IHumanInteractionStore`. Use the provided
kernel builder extension to register it, or construct the channel with a store.

```csharp
// Register the Web API human interaction support via the kernel builder
// so the example wiring is handled by DI and the framework.
kernelBuilder.AddWebApiHumanInteraction();

// Alternatively, construct the channel manually using a backing store.
var store = new InMemoryHumanInteractionStore();
var webApiChannel = new WebApiHumanInteractionChannel(store);

// Create an approval node routed to the web channel. Web interactions
// typically require longer timeouts to allow external approvers to respond.
var webApprovalNode = new HumanApprovalGraphNode(
    approvalTitle: "Web Approval",
    approvalMessage: "Approve via the web UI",
    interactionChannel: webApiChannel,
    nodeId: "web_approval"
).WithTimeout(TimeSpan.FromMinutes(30));
```

### CLI Channel

CLI interactions can be provided by the console channel. There is no separate
`CliHumanInteractionChannel` type in this codebase — reuse `ConsoleHumanInteractionChannel`.

```csharp
// CLI interactions reuse the console channel. Use a compact prompt
// style when approvers expect short commands and limited output.
var cliChannel = new ConsoleHumanInteractionChannel();
await cliChannel.InitializeAsync(new Dictionary<string, object>
{
    ["prompt_style"] = "compact",
    ["enable_colors"] = false
});

var cliApprovalNode = new HumanApprovalGraphNode(
    approvalTitle: "CLI Approval",
    approvalMessage: "Respond via terminal",
    interactionChannel: cliChannel,
    nodeId: "cli_approval"
).WithTimeout(TimeSpan.FromMinutes(15));
```

## Advanced HITL Patterns

### Batch Approval Management

Handle multiple pending approvals efficiently with `HumanApprovalBatchManager`.

```csharp
// Create the manager with a default channel and batch options. The
// batch manager groups multiple requests to reduce notification noise.
var defaultChannel = new ConsoleHumanInteractionChannel();
var batchOptions = new BatchApprovalOptions
{
    MaxBatchSize = 20,
    BatchFormationTimeout = TimeSpan.FromHours(1),
    AllowPartialApproval = false
};

var batchManager = new HumanApprovalBatchManager(defaultChannel, batchOptions);

// Create an approval node that may be routed into a batch by the manager.
var batchApprovalNode = new HumanApprovalGraphNode(
    approvalTitle: "Batch Approval",
    approvalMessage: "This request may be processed in a batch",
    interactionChannel: defaultChannel,
    nodeId: "batch_approval"
).WithTimeout(TimeSpan.FromHours(2));
```

### Conditional Human Review

Implement smart routing for human review using a conditional node and approval node.

```csharp
// Create a conditional node that evaluates graph state to decide whether
// a human review is necessary. The predicate reads values stored earlier
// in the graph state such as confidence, risk level, and transaction amount.
var conditionalReview = new ConditionalGraphNode(
    condition: state =>
    {
        var confidence = state.GetFloat("confidence_score", 0f);
        var riskLevel = state.GetString("risk_level", "low");
        var amount = state.GetDecimal("transaction_amount", 0m);

        // Request human review when confidence is low, risk is high, or
        // the transaction amount exceeds a business-defined threshold.
        return confidence < 0.7f ||
               riskLevel == "high" ||
               amount > 10000m;
    },
    nodeId: "review_decision"
);

// Create a human approval node that will be executed when the conditional
// node evaluates to true.
var humanReviewNode = new HumanApprovalGraphNode(
    approvalTitle: "Human Review",
    approvalMessage: "Please review this transaction",
    interactionChannel: new ConsoleHumanInteractionChannel(),
    nodeId: "human_review"
);

// Wire nodes into the executor and route the true branch to the human node.
executor.AddNode(conditionalReview);
executor.AddNode(humanReviewNode);
conditionalReview.AddTrueNode(humanReviewNode);
// Add an auto-processing node on the false branch as appropriate.
```

### Multi-Stage Approval

Implement complex approval workflows:

```csharp
// Example multi-stage approval flow. Each stage is represented by a
// HumanApprovalGraphNode. Approvals are chained so that the output of
// one stage triggers the next.
var firstApproval = new HumanApprovalGraphNode(
    approvalTitle: "First Approval",
    approvalMessage: "Stage 1 approval",
    interactionChannel: new ConsoleHumanInteractionChannel(),
    nodeId: "first_approval"
);

var secondApproval = new HumanApprovalGraphNode(
    approvalTitle: "Second Approval",
    approvalMessage: "Stage 2 approval",
    interactionChannel: new ConsoleHumanInteractionChannel(),
    nodeId: "second_approval"
);

var finalApproval = new HumanApprovalGraphNode(
    approvalTitle: "Final Approval",
    approvalMessage: "Final stage approval",
    interactionChannel: new ConsoleHumanInteractionChannel(),
    nodeId: "final_approval"
);

// Wire the multi-stage flow into the executor.
executor.AddNode(firstApproval);
executor.AddNode(secondApproval);
executor.AddNode(finalApproval);
executor.Connect(firstApproval.NodeId, secondApproval.NodeId);
executor.Connect(secondApproval.NodeId, finalApproval.NodeId);
executor.Connect(finalApproval.NodeId, "approved");
```

## Configuration and Options

### Approval Node Configuration

Configure approval behavior and appearance:

```csharp
// Configure an approval node with explicit approval options and a timeout.
var configuredApproval = new HumanApprovalGraphNode(
    approvalTitle: "Document Review Required",
    approvalMessage: "Please review the generated document for accuracy",
    interactionChannel: new ConsoleHumanInteractionChannel(),
    nodeId: "configured_approval"
);

// Add named approval options that the approver can choose from. The
// API stores the chosen value in the result so graph routing can use it.
configuredApproval.AddApprovalOption("approve", "Approve", value: true, isDefault: true)
                  .AddApprovalOption("reject", "Reject", value: false);

// Apply a reasonable timeout to avoid indefinite blocking.
configuredApproval.WithTimeout(TimeSpan.FromMinutes(15), TimeoutAction.Reject);
```

### Channel Configuration

Configure interaction channels for optimal user experience:

```csharp
// Web API channel is registered via kernel builder or constructed with a store.
kernelBuilder.AddWebApiHumanInteraction();

// Or construct manually with a backing store and initialize it.
var store = new InMemoryHumanInteractionStore();
var webChannel = new WebApiHumanInteractionChannel(store);
await webChannel.InitializeAsync();
```

## Integration with External Systems

### Email Notifications

Integrate email notifications for approvals:

```csharp
// Email channel not included by default in the codebase; implement a channel that
// implements IHumanInteractionChannel and plugs into the approval nodes.
```

## Monitoring and Auditing

### Approval Tracking

Track approval status and timing:

```csharp
// Create an approval node and attach hooks to record timing and result
// metadata for monitoring and auditing purposes.
var trackedApproval = new HumanApprovalGraphNode(
    approvalTitle: "Tracked Approval",
    approvalMessage: "Please review and track timing",
    interactionChannel: new ConsoleHumanInteractionChannel(),
    nodeId: "tracked_approval"
);

// Record when the approval was requested.
trackedApproval.OnBeforeExecuteAsync = async (kernel, args, ct) =>
{
    var state = args.GetOrCreateGraphState();
    state["approval_requested_at"] = DateTimeOffset.UtcNow;
    return Task.CompletedTask;
};

// Record when the approval completed and store the outcome for auditing.
trackedApproval.OnAfterExecuteAsync = async (kernel, args, result, ct) =>
{
    var state = args.GetOrCreateGraphState();
    var requestedAt = state.GetValue<DateTimeOffset>("approval_requested_at");
    state["approval_completed_at"] = DateTimeOffset.UtcNow;
    state["approval_result"] = result.IsSuccess ? "approved" : "rejected";
    return Task.CompletedTask;
};
```

## Best Practices

### Approval Design

1. **Clear instructions** - Provide clear, actionable instructions for approvers
2. **Reasonable timeouts** - Set appropriate timeouts based on approval complexity
3. **Escalation paths** - Define escalation procedures for overdue approvals
4. **Batch processing** - Group related approvals for efficiency

### Channel Selection

1. **User preferences** - Choose channels based on user preferences and availability
2. **Response urgency** - Use faster channels for urgent approvals
3. **Integration capabilities** - Leverage existing communication infrastructure
4. **Accessibility** - Ensure channels are accessible to all approvers

### Security and Compliance

1. **Authentication** - Implement proper authentication for all channels
2. **Audit trails** - Maintain comprehensive audit logs for compliance
3. **Data privacy** - Protect sensitive information in approval requests
4. **Access control** - Restrict approval access to authorized users

## Troubleshooting

### Common Issues

**Approval timeouts**: Check timeout settings and approver availability

**Channel failures**: Verify channel configuration and network connectivity

**Batch processing issues**: Check batch size limits and grouping logic

**Audit log gaps**: Verify audit logger configuration and permissions

### Debugging Tips

1. **Enable detailed logging** to trace approval flow
2. **Check channel status** for connectivity issues
3. **Monitor approval metrics** for performance issues
4. **Test channels independently** to isolate problems

## Concepts and Techniques

**HumanApprovalGraphNode**: A specialized graph node that pauses execution to wait for human input or approval. It supports multiple interaction channels and configurable timeouts.

**ConfidenceGateGraphNode**: A node that automatically routes execution based on confidence scores, only requiring human intervention when confidence falls below a threshold.

**HumanInteractionChannel**: An interface that defines how human interactions are handled, supporting various communication methods like console, web API, email, and Slack.

**HumanApprovalBatchManager**: A service that groups multiple approval requests into batches for efficient processing, reducing notification overhead and improving approval workflow management.

**Approval Workflow**: A pattern where graph execution is paused at specific points to allow human decision-making, enabling oversight and quality control in automated processes.

## See Also

* [Human-in-the-Loop](human-in-the-loop.md) - Comprehensive guide to HITL workflows
* [Build a Graph](build-a-graph.md) - Learn how to construct graphs with approval nodes
* [Error Handling and Resilience](error-handling-and-resilience.md) - Handle approval failures gracefully
* [Security and Data](security-and-data.md) - Secure HITL implementation practices
* [Examples: HITL Workflows](../examples/hitl-example.md) - Complete working examples of human-in-the-loop workflows
