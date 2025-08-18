# Human-in-the-loop

This guide explains how to implement Human-in-the-Loop (HITL) workflows in SemanticKernel.Graph. You'll learn how to pause execution for human approval, implement confidence gates, and integrate multiple interaction channels for seamless human oversight.

## Overview

Human-in-the-Loop workflows enable you to:
- **Pause execution** and wait for human approval or input
- **Implement confidence gates** for quality control
- **Support multiple channels** including CLI, web, and API interfaces
- **Batch approvals** for multiple pending decisions
- **Set timeouts and SLAs** for human response requirements

## Core HITL Components

### Human Approval Node

Use `HumanApprovalGraphNode` to pause execution for human decision:

```csharp
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Nodes;

// Create a human approval node with timeout
var approvalNode = new HumanApprovalGraphNode(
    timeout: TimeSpan.FromMinutes(10),
    nodeId: "approve_step"
);

// Add to your graph
graph.AddNode(approvalNode)
     .AddEdge("start", "approve_step")
     .AddEdge("approve_step", "approved_path")
     .AddEdge("approve_step", "rejected_path");
```

### Confidence Gate Node

Implement confidence-based decision gates:

```csharp
var confidenceNode = new ConfidenceGateGraphNode(
    confidenceThreshold: 0.8f,
    timeout: TimeSpan.FromMinutes(5),
    nodeId: "confidence_check"
);

// Route based on confidence level
graph.AddConditionalEdge("confidence_check", "high_confidence", 
    condition: state => state.GetFloat("confidence_score", 0f) >= 0.8f)
.AddConditionalEdge("confidence_check", "human_review", 
    condition: state => state.GetFloat("confidence_score", 0f) < 0.8f);
```

## Multiple Interaction Channels

### Console Channel

Use console-based human interaction:

```csharp
var consoleChannel = new ConsoleHumanInteractionChannel();

var approvalNode = new HumanApprovalGraphNode(
    timeout: TimeSpan.FromMinutes(10),
    channel: consoleChannel,
    nodeId: "console_approval"
);

// Configure console interaction
consoleChannel.Configure(new ConsoleChannelOptions
{
    PromptFormat = "APPROVAL REQUIRED: {message}\nEnter 'approve' or 'reject': ",
    ValidationPattern = @"^(approve|reject)$",
    RetryCount = 3
});
```

### Web API Channel

Implement web-based approval interfaces:

```csharp
var webApiChannel = new WebApiHumanInteractionChannel(
    baseUrl: "https://approval.example.com",
    apiKey: "your-api-key"
);

var webApprovalNode = new HumanApprovalGraphNode(
    timeout: TimeSpan.FromMinutes(30),
    channel: webApiChannel,
    nodeId: "web_approval"
);

// Configure web API options
webApiChannel.Configure(new WebApiChannelOptions
{
    EndpointPath = "/api/approvals",
    RequestTimeout = TimeSpan.FromSeconds(30),
    RetryPolicy = RetryPolicy.ExponentialBackoff(3, TimeSpan.FromSeconds(1))
});
```

### CLI Channel

Use command-line interface for approvals:

```csharp
var cliChannel = new CliHumanInteractionChannel();

var cliApprovalNode = new HumanApprovalGraphNode(
    timeout: TimeSpan.FromMinutes(15),
    channel: cliChannel,
    nodeId: "cli_approval"
);

// Configure CLI options
cliChannel.Configure(new CliChannelOptions
{
    CommandPrefix = "approval",
    InteractiveMode = true,
    DefaultResponse = "pending"
});
```

## Advanced HITL Patterns

### Batch Approval Management

Handle multiple pending approvals efficiently:

```csharp
var batchManager = new HumanApprovalBatchManager(
    batchSize: 10,
    batchTimeout: TimeSpan.FromHours(1)
);

var batchApprovalNode = new HumanApprovalGraphNode(
    timeout: TimeSpan.FromHours(2),
    batchManager: batchManager,
    nodeId: "batch_approval"
);

// Configure batch processing
batchManager.Configure(new BatchOptions
{
    GroupBy = "approver_id",
    MaxBatchSize = 20,
    NotificationStrategy = NotificationStrategy.Email
});
```

### Conditional Human Review

Implement smart routing for human review:

```csharp
var conditionalReview = new ConditionalGraphNode(
    predicate: state => {
        var confidence = state.GetFloat("confidence_score", 0f);
        var riskLevel = state.GetString("risk_level", "low");
        var amount = state.GetDecimal("transaction_amount", 0m);
        
        return confidence < 0.7f || 
               riskLevel == "high" || 
               amount > 10000m;
    },
    nodeId: "review_decision"
);

var humanReviewNode = new HumanApprovalGraphNode(
    timeout: TimeSpan.FromMinutes(20),
    nodeId: "human_review"
);

// Route to human review when needed
graph.AddConditionalEdge("review_decision", "human_review", 
    condition: state => state.GetBool("needs_review", false))
.AddConditionalEdge("review_decision", "auto_process", 
    condition: state => !state.GetBool("needs_review", true));
```

### Multi-Stage Approval

Implement complex approval workflows:

```csharp
var firstApproval = new HumanApprovalGraphNode(
    timeout: TimeSpan.FromMinutes(30),
    nodeId: "first_approval"
);

var secondApproval = new HumanApprovalGraphNode(
    timeout: TimeSpan.FromMinutes(60),
    nodeId: "second_approval"
);

var finalApproval = new HumanApprovalGraphNode(
    timeout: TimeSpan.FromMinutes(120),
    nodeId: "final_approval"
);

// Multi-stage approval flow
graph.AddEdge("start", "first_approval")
     .AddEdge("first_approval", "second_approval")
     .AddEdge("second_approval", "final_approval")
     .AddEdge("final_approval", "approved");
```

## Configuration and Options

### Approval Node Configuration

Configure approval behavior and appearance:

```csharp
var configuredApproval = new HumanApprovalGraphNode(
    timeout: TimeSpan.FromMinutes(15),
    nodeId: "configured_approval"
);

// Configure approval options
configuredApproval.Configure(new HumanApprovalOptions
{
    Title = "Document Review Required",
    Description = "Please review the generated document for accuracy",
    Instructions = "Check grammar, content, and formatting",
    RequiredFields = new[] { "reviewer_name", "approval_notes" },
    OptionalFields = new[] { "suggestions", "priority_level" },
    ApprovalThreshold = 1, // Number of approvals required
    RejectionThreshold = 1, // Number of rejections to fail
    AllowPartialApproval = false
});
```

### Channel Configuration

Configure interaction channels for optimal user experience:

```csharp
var webChannel = new WebApiHumanInteractionChannel(
    baseUrl: "https://approval.example.com"
);

webChannel.Configure(new WebApiChannelOptions
{
    EndpointPath = "/api/approvals",
    RequestTimeout = TimeSpan.FromSeconds(30),
    RetryPolicy = RetryPolicy.ExponentialBackoff(3, TimeSpan.FromSeconds(1)),
    Authentication = new BearerTokenAuth("your-token"),
    CustomHeaders = new Dictionary<string, string>
    {
        ["X-Client-Version"] = "1.0.0",
        ["X-Environment"] = "production"
    }
});
```

## Integration with External Systems

### Email Notifications

Integrate email notifications for approvals:

```csharp
var emailChannel = new EmailHumanInteractionChannel(
    smtpServer: "smtp.example.com",
    port: 587,
    username: "approvals@example.com",
    password: "your-password"
);

var emailApproval = new HumanApprovalGraphNode(
    timeout: TimeSpan.FromHours(2),
    channel: emailChannel,
    nodeId: "email_approval"
);

// Configure email options
emailChannel.Configure(new EmailChannelOptions
{
    FromAddress = "approvals@example.com",
    SubjectTemplate = "Approval Required: {workflow_name}",
    BodyTemplate = "Please review and approve the following request:\n\n{details}\n\nClick here to approve: {approval_url}",
    ReplyToAddress = "support@example.com"
});
```

### Slack Integration

Integrate with Slack for team approvals:

```csharp
var slackChannel = new SlackHumanInteractionChannel(
    webhookUrl: "https://hooks.slack.com/services/your-webhook",
    channel: "#approvals"
);

var slackApproval = new HumanApprovalGraphNode(
    timeout: TimeSpan.FromMinutes(45),
    channel: slackChannel,
    nodeId: "slack_approval"
);

// Configure Slack options
slackChannel.Configure(new SlackChannelOptions
{
    Username = "Approval Bot",
    IconEmoji = ":white_check_mark:",
    ThreadReplies = true,
    MentionUsers = new[] { "@approver1", "@approver2" }
});
```

## Monitoring and Auditing

### Approval Tracking

Track approval status and timing:

```csharp
var trackedApproval = new HumanApprovalGraphNode(
    timeout: TimeSpan.FromMinutes(30),
    nodeId: "tracked_approval"
);

// Add tracking information
trackedApproval.BeforeExecution = async (context) => {
    context.GraphState["approval_requested_at"] = DateTimeOffset.UtcNow;
    context.GraphState["approval_id"] = Guid.NewGuid().ToString();
    return Task.CompletedTask;
};

trackedApproval.AfterExecution = async (context, result) => {
    var requestedAt = context.GraphState.GetValue<DateTimeOffset>("approval_requested_at");
    var completedAt = DateTimeOffset.UtcNow;
    var duration = completedAt - requestedAt;
    
    context.GraphState["approval_completed_at"] = completedAt;
    context.GraphState["approval_duration"] = duration;
    context.GraphState["approval_result"] = result.IsSuccess ? "approved" : "rejected";
    
    return Task.CompletedTask;
};
```

### Audit Logging

Implement comprehensive audit logging:

```csharp
var auditLogger = new AuditLogger(new AuditOptions
{
    LogLevel = LogLevel.Information,
    IncludeState = true,
    IncludeTiming = true
});

var auditedApproval = new HumanApprovalGraphNode(
    timeout: TimeSpan.FromMinutes(20),
    auditLogger: auditLogger,
    nodeId: "audited_approval"
);

// Audit events are automatically logged
auditLogger.LogApprovalRequested("audited_approval", new { 
    workflow_id = "workflow_123", 
    requestor = "user@example.com" 
});
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

- [Human-in-the-Loop](human-in-the-loop.md) - Comprehensive guide to HITL workflows
- [Build a Graph](build-a-graph.md) - Learn how to construct graphs with approval nodes
- [Error Handling and Resilience](error-handling-and-resilience.md) - Handle approval failures gracefully
- [Security and Data](security-and-data.md) - Secure HITL implementation practices
- [Examples: HITL Workflows](../examples/hitl-example.md) - Complete working examples of human-in-the-loop workflows
