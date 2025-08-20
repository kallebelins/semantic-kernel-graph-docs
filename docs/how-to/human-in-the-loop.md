# Human-in-the-Loop (HITL)

Human-in-the-Loop (HITL) in SemanticKernel.Graph provides sophisticated mechanisms for human intervention during graph execution. This system enables conditional pauses, human approvals, confidence-based routing, and comprehensive auditing for scenarios requiring human oversight or decision-making.

## What You'll Learn

* How to implement human approval nodes with configurable conditions
* Using confidence gates to route execution based on uncertainty levels
* Configuring multiple interaction channels (Console, Web API, CLI)
* Setting up SLAs, timeouts, and automatic fallback actions
* Implementing batch approval systems for efficient processing
* Comprehensive auditing and tracking of human interactions
* Best practices for HITL integration in production workflows

## Concepts and Techniques

**HumanApprovalGraphNode**: Specialized node that pauses graph execution and waits for human approval before continuing, with support for conditional activation and multiple approval options.

**ConfidenceGateGraphNode**: Node that monitors confidence scores and automatically triggers human intervention when confidence falls below configured thresholds.

**IHumanInteractionChannel**: Abstract interface for different communication channels (Console, Web API, CLI) that handle human interactions.

**HumanApprovalBatchManager**: Manages grouping and processing of multiple approval requests for efficient batch operations.

**HumanInteractionTimeout**: Configurable timeout settings with automatic fallback actions when human responses are not received.

**InterruptionType**: Classification system for different types of human intervention (ManualApproval, ConfidenceGate, HumanInput, ResultValidation).

## Prerequisites

* [First Graph Tutorial](../first-graph-5-minutes.md) completed
* Basic understanding of graph execution concepts
* Familiarity with conditional nodes and routing
* Understanding of confidence scoring and quality metrics

## Human Approval Nodes

### Basic Human Approval

Create nodes that require human approval before continuing execution:

```csharp
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.Core;

// Create a console interaction channel
var consoleChannel = new ConsoleHumanInteractionChannel();

// Create human approval node
var approvalNode = new HumanApprovalGraphNode(
    "approval-1",
    "Document Review Required",
    "Please review the generated document for accuracy and completeness",
    consoleChannel);

// Add to graph
graph.AddNode(approvalNode);
graph.AddEdge(startNode, approvalNode);
```

### Conditional Human Approval

Configure approval nodes that only activate under specific conditions:

```csharp
// Create conditional approval node
var conditionalApproval = HumanApprovalGraphNode.CreateConditional(
    "conditional-approval",
    "High-Risk Transaction Approval",
    "Approval required for transactions above threshold",
    state => state.GetValue<decimal>("transaction_amount") > 10000m,
    consoleChannel,
    "conditional-approval");

// Add routing based on approval result
graph.AddConditionalEdge(conditionalApproval, approvedNode,
    edge => edge.Condition = "ApprovalResult == true");
graph.AddConditionalEdge(conditionalApproval, rejectedNode,
    edge => edge.Condition = "ApprovalResult == false");
```

### Approval Options and Routing

Configure multiple approval options with custom routing:

```csharp
// Add approval options
approvalNode.ApprovalOptions.Add(new HumanInteractionOption
{
    OptionId = "approve",
    DisplayText = "Approve and Continue",
    Value = true,
    IsDefault = true,
    Description = "Approve the document and continue processing"
});

approvalNode.ApprovalOptions.Add(new HumanInteractionOption
{
    OptionId = "reject",
    DisplayText = "Reject and Stop",
    Value = false,
    Description = "Reject the document and stop processing"
});

approvalNode.ApprovalOptions.Add(new HumanInteractionOption
{
    OptionId = "modify",
    DisplayText = "Request Modifications",
    Value = "modify",
    Description = "Request changes before approval"
});

// Configure routing based on approval options
graph.AddConditionalEdge(approvalNode, approvedNode,
    edge => edge.Condition = "ApprovalResult == true");
graph.AddConditionalEdge(approvalNode, rejectedNode,
    edge => edge.Condition = "ApprovalResult == false");
graph.AddConditionalEdge(approvalNode, modificationNode,
    edge => edge.Condition = "ApprovalResult == 'modify'");
```

## Confidence Gates

### Basic Confidence Gate

Create nodes that automatically trigger human intervention based on confidence levels:

```csharp
// Create confidence gate with 0.7 threshold
var confidenceGate = new ConfidenceGateGraphNode(
    0.7,  // Confidence threshold
    "quality-gate");

// Configure confidence sources
confidenceGate.SetConfidenceSource(state => 
    state.GetValue<double>("llm_confidence_score"));

// Add routing paths
confidenceGate.AddHighConfidenceNode(highQualityProcessNode);
confidenceGate.AddLowConfidenceNode(humanReviewNode);

// Add to graph
graph.AddNode(confidenceGate);
graph.AddEdge(previousNode, confidenceGate);
```

### Advanced Confidence Analysis

Configure comprehensive confidence evaluation with multiple sources:

```csharp
// Create confidence gate with multiple sources
var advancedGate = new ConfidenceGateGraphNode(
    0.8,  // Higher threshold for critical decisions
    "critical-quality-gate");

// Configure multiple confidence sources with weights
advancedGate.SetConfidenceSources(new Dictionary<string, Func<GraphState, double>>
{
    ["llm_confidence"] = state => state.GetValue<double>("llm_confidence") * 0.6,
    ["similarity_score"] = state => state.GetValue<double>("similarity_score") * 0.3,
    ["validation_score"] = state => state.GetValue<double>("validation_score") * 0.1
});

// Configure uncertainty analysis
advancedGate.EnableUncertaintyAnalysis = true;
advancedGate.SetUncertaintyThreshold(0.3);

// Add human interaction channel for low confidence
advancedGate.SetInteractionChannel(consoleChannel);
```

### Confidence Gate Modes

Configure different operational modes for confidence gates:

```csharp
// Permissive mode - allows execution with warnings
var permissiveGate = new ConfidenceGateGraphNode(0.6, "permissive-gate")
{
    Mode = ConfidenceGateMode.Permissive,
    AllowManualBypass = true
};

// Strict mode - requires human approval for low confidence
var strictGate = new ConfidenceGateGraphNode(0.8, "strict-gate")
{
    Mode = ConfidenceGateMode.Strict,
    RequireHumanApproval = true
};

// Learning mode - adjusts thresholds based on feedback
var learningGate = new ConfidenceGateGraphNode(0.7, "learning-gate")
{
    Mode = ConfidenceGateMode.Learning,
    EnableThresholdAdjustment = true
};
```

## Interaction Channels

### Console Channel

Use console-based interaction for development and testing:

```csharp
using SemanticKernel.Graph.Core;

// Create console channel with custom configuration
var consoleChannel = new ConsoleHumanInteractionChannel();

// Configure console settings
await consoleChannel.InitializeAsync(new Dictionary<string, object>
{
    ["enable_colors"] = true,
    ["show_timestamps"] = true,
    ["clear_screen_on_new_request"] = false,
    ["prompt_style"] = "detailed"
});

// Use in approval nodes
var approvalNode = new HumanApprovalGraphNode(
    "console-approval",
    "Console Approval",
    "Please approve this action",
    consoleChannel);
```

### Web API Channel

Implement web-based interaction for production deployments:

```csharp
using SemanticKernel.Graph.Core;

// Create web API channel with backing store
var interactionStore = new InMemoryHumanInteractionStore();
var webApiChannel = new WebApiHumanInteractionChannel(interactionStore);

// Configure web API settings
await webApiChannel.InitializeAsync(new Dictionary<string, object>
{
    ["api_base_url"] = "https://api.example.com/approvals",
    ["timeout_seconds"] = 300,
    ["enable_notifications"] = true
});

// Subscribe to events
webApiChannel.ResponseReceived += (sender, response) =>
{
    Console.WriteLine($"Received response: {response.Status}");
};

webApiChannel.RequestTimedOut += (sender, request) =>
{
    Console.WriteLine($"Request timed out: {request.RequestId}");
};
```

### Custom Channel Implementation

Create custom interaction channels for specific requirements:

```csharp
public class EmailInteractionChannel : IHumanInteractionChannel
{
    public HumanInteractionChannelType ChannelType => HumanInteractionChannelType.Email;
    public string ChannelName => "Email Interaction Channel";
    public bool IsAvailable => true;
    public bool SupportsBatchOperations => false;

    public async Task<HumanInterruptionResponse> SendInterruptionRequestAsync(
        HumanInterruptionRequest request,
        CancellationToken cancellationToken = default)
    {
        // Send email with approval link
        var emailContent = CreateApprovalEmail(request);
        await SendEmailAsync(request.UserEmail, emailContent);
        
        // Wait for response via webhook or polling
        return await WaitForEmailResponseAsync(request.RequestId, cancellationToken);
    }

    // Implement other interface methods...
}

// Use custom channel
var emailChannel = new EmailInteractionChannel();
var approvalNode = new HumanApprovalGraphNode(
    "email-approval",
    "Email Approval",
    "Please check your email for approval",
    emailChannel);
```

## Timeout Configuration and SLAs

### Basic Timeout Configuration

Configure timeouts with automatic fallback actions:

```csharp
using SemanticKernel.Graph.Core;

// Create timeout configuration
var timeoutConfig = new HumanInteractionTimeout
{
    PrimaryTimeout = TimeSpan.FromMinutes(15),
    WarningTimeout = TimeSpan.FromMinutes(10),
    DefaultAction = TimeoutAction.Reject,
    EnableEscalation = true,
    EscalationTimeout = TimeSpan.FromMinutes(30)
};

// Apply to approval node
approvalNode.TimeoutConfiguration = timeoutConfig;

// Configure timeout actions
approvalNode.SetTimeoutAction(TimeoutAction.Escalate, escalationNode);
approvalNode.SetTimeoutAction(TimeoutAction.UseDefault, defaultNode);
```

### SLA-Based Timeouts

Implement Service Level Agreement (SLA) compliance:

```csharp
// Configure SLA-based timeouts
var slaTimeouts = new Dictionary<string, HumanInteractionTimeout>
{
    ["critical"] = new HumanInteractionTimeout
    {
        PrimaryTimeout = TimeSpan.FromMinutes(5),
        DefaultAction = TimeoutAction.Escalate,
        EscalationTimeout = TimeSpan.FromMinutes(10)
    },
    
    ["high"] = new HumanInteractionTimeout
    {
        PrimaryTimeout = TimeSpan.FromMinutes(15),
        DefaultAction = TimeoutAction.Reject,
        WarningTimeout = TimeSpan.FromMinutes(10)
    },
    
    ["normal"] = new HumanInteractionTimeout
    {
        PrimaryTimeout = TimeSpan.FromMinutes(30),
        DefaultAction = TimeoutAction.UseDefault,
        WarningTimeout = TimeSpan.FromMinutes(20)
    }
};

// Apply SLA timeouts based on priority
approvalNode.SetPriorityBasedTimeouts(slaTimeouts);
```

### Escalation and Fallback

Configure automatic escalation when primary approvers don't respond:

```csharp
// Configure escalation chain
var escalationConfig = new EscalationConfiguration
{
    EnableEscalation = true,
    EscalationLevels = new List<EscalationLevel>
    {
        new EscalationLevel
        {
            Level = 1,
            ApproverRole = "Manager",
            Timeout = TimeSpan.FromMinutes(10),
            NotificationChannel = "email"
        },
        new EscalationLevel
        {
            Level = 2,
            ApproverRole = "Director",
            Timeout = TimeSpan.FromMinutes(20),
            NotificationChannel = "sms"
        }
    }
};

approvalNode.EscalationConfiguration = escalationConfig;
```

## Batch Approval Systems

### Basic Batch Configuration

Group multiple approval requests for efficient processing:

```csharp
using SemanticKernel.Graph.Core;

// Create batch manager with configuration
var batchOptions = new BatchApprovalOptions
{
    MaxBatchSize = 10,
    BatchFormationTimeout = TimeSpan.FromMinutes(5),
    AllowPartialApproval = true,
    GroupByInterruptionType = true,
    GroupByPriority = true
};

var batchManager = new HumanApprovalBatchManager(
    consoleChannel,
    batchOptions,
    graphLogger);

// Configure executor to use batch system
executor.WithBatchApproval(batchManager);

// Subscribe to batch events
batchManager.BatchFormed += (sender, batch) =>
{
    Console.WriteLine($"Batch formed: {batch.BatchId} with {batch.Requests.Count} requests");
};

batchManager.BatchCompleted += (sender, args) =>
{
    Console.WriteLine($"Batch completed: {args.BatchId} in {args.ProcessingTime}");
};
```

### Smart Batch Grouping

Configure intelligent grouping strategies:

```csharp
// Configure advanced grouping
var smartBatchOptions = new BatchApprovalOptions
{
    MaxBatchSize = 15,
    BatchFormationTimeout = TimeSpan.FromMinutes(3),
    AllowPartialApproval = false,
    GroupByInterruptionType = true,
    GroupByPriority = true,
    GroupByUser = true,
    GroupByContext = true
};

// Custom grouping criteria
batchManager.SetCustomGroupingCriteria(request =>
{
    var criteria = new List<string>();
    
    // Group by business unit
    if (request.Context.TryGetValue("business_unit", out var bu))
        criteria.Add($"bu_{bu}");
    
    // Group by risk level
    if (request.Context.TryGetValue("risk_level", out var risk))
        criteria.Add($"risk_{risk}");
    
    return criteria;
});
```

## Auditing and Tracking

### Basic Audit Trail

Track all human interactions for compliance:

```csharp
// Enable comprehensive auditing
approvalNode.EnableAuditTrail = true;
approvalNode.AuditConfiguration = new AuditConfiguration
{
    TrackUserActions = true,
    TrackTiming = true,
    TrackContext = true,
    EnableAuditLogging = true
};

// Subscribe to audit events
approvalNode.AuditEventRaised += (sender, auditEvent) =>
{
    var auditLog = new
    {
        Timestamp = auditEvent.Timestamp,
        UserId = auditEvent.UserId,
        Action = auditEvent.Action,
        Context = auditEvent.Context,
        Duration = auditEvent.Duration
    };
    
    // Log to audit system
    auditLogger.LogAuditEvent(auditLog);
};
```

### Compliance Reporting

Generate compliance reports for regulatory requirements:

```csharp
// Configure compliance tracking
var complianceConfig = new ComplianceConfiguration
{
    EnableComplianceTracking = true,
    RequiredFields = new[] { "user_id", "approval_reason", "risk_assessment" },
    RetentionPeriod = TimeSpan.FromDays(2555), // 7 years
    EnableDataExport = true
};

approvalNode.ComplianceConfiguration = complianceConfig;

// Generate compliance report
var complianceReport = await approvalNode.GenerateComplianceReportAsync(
    DateTimeOffset.UtcNow.AddDays(-30),
    DateTimeOffset.UtcNow);

Console.WriteLine($"Compliance Report: {complianceReport.TotalApprovals} approvals");
Console.WriteLine($"Average Response Time: {complianceReport.AverageResponseTime}");
Console.WriteLine($"SLA Compliance: {complianceReport.SlaComplianceRate:P}");
```

### Performance Metrics

Track HITL performance and efficiency:

```csharp
// Get HITL performance metrics
var hitlMetrics = approvalNode.GetHITLMetrics();

Console.WriteLine($"Total Approvals: {hitlMetrics.TotalApprovals}");
Console.WriteLine($"Average Response Time: {hitlMetrics.AverageResponseTime}");
Console.WriteLine($"Timeout Rate: {hitlMetrics.TimeoutRate:P}");
Console.WriteLine($"Escalation Rate: {hitlMetrics.EscalationRate:P}");

// Get confidence gate metrics
var gateMetrics = confidenceGate.GetGateMetrics();

Console.WriteLine($"Gate Passed: {gateMetrics.GatePassed}");
Console.WriteLine($"Gate Blocked: {gateMetrics.GateBlocked}");
Console.WriteLine($"Human Overrides: {gateMetrics.HumanOverrides}");
Console.WriteLine($"Average Confidence: {gateMetrics.AverageConfidence:F2}");
```

## Integration with Graph Execution

### Fluent Configuration

Use extension methods for clean, readable configuration:

```csharp
using SemanticKernel.Graph.Extensions;

// Configure HITL with fluent API
var executor = new GraphExecutor("HITLGraph", "Graph with human approval")
    .AddHumanApproval(
        "document-approval",
        "Document Review Required",
        "Please review the generated document",
        consoleChannel)
    .AddConfidenceGate(
        "quality-gate",
        0.8,
        consoleChannel)
    .WithBatchApproval(batchManager)
    .WithHumanApprovalTimeout(
        TimeSpan.FromMinutes(15),
        TimeoutAction.Reject);
```

### Kernel Builder Integration

Integrate HITL capabilities at the kernel level:

```csharp
using SemanticKernel.Graph.Extensions;

// Add HITL support to kernel builder
var builder = Kernel.CreateBuilder()
    .AddConsoleHumanInteraction(new Dictionary<string, object>
    {
        ["enable_colors"] = true,
        ["show_timestamps"] = true
    })
    .AddWebApiHumanInteraction()
    .WithBatchApprovalOptions(new BatchApprovalOptions
    {
        MaxBatchSize = 20,
        BatchFormationTimeout = TimeSpan.FromMinutes(10)
    });

var kernel = builder.Build();
```

### Streaming Integration

Monitor HITL events in real-time:

```csharp
using var eventStream = executor.CreateStreamingExecutor()
    .CreateEventStream();

// Subscribe to HITL events
eventStream.SubscribeToEvents<GraphExecutionEvent>(event =>
{
    if (event.EventType == GraphExecutionEventType.HumanInteractionRequested)
    {
        var hitlEvent = event as HumanInteractionRequestedEvent;
        Console.WriteLine($"HITL Request: {hitlEvent.NodeId} - {hitlEvent.Title}");
    }
    
    if (event.EventType == GraphExecutionEventType.HumanInteractionCompleted)
    {
        var hitlEvent = event as HumanInteractionCompletedEvent;
        Console.WriteLine($"HITL Completed: {hitlEvent.NodeId} - {hitlEvent.Result}");
    }
});

// Execute with streaming
await executor.ExecuteAsync(arguments, eventStream);
```

## Best Practices

### Human Approval Design

* **Clear Approval Criteria**: Provide specific, actionable approval requests
* **Context-Rich Information**: Include relevant data and reasoning for decisions
* **Multiple Approval Options**: Offer approve/reject/modify choices when appropriate
* **Conditional Activation**: Only request approval when necessary
* **Timeout Handling**: Always configure fallback actions for timeouts

### Confidence Gate Configuration

* **Appropriate Thresholds**: Set thresholds based on business risk and quality requirements
* **Multiple Sources**: Combine multiple confidence indicators for robust evaluation
* **Learning Mode**: Use learning gates to improve thresholds over time
* **Uncertainty Analysis**: Enable detailed uncertainty tracking for debugging
* **Fallback Paths**: Provide clear routing for low-confidence scenarios

### Channel Selection

* **Development**: Use console channels for testing and development
* **Production**: Implement web API channels for scalable deployments
* **User Experience**: Choose channels based on user preferences and workflows
* **Integration**: Ensure channels integrate with existing approval systems
* **Monitoring**: Monitor channel availability and performance

### Performance and Scalability

* **Batch Processing**: Use batch approval systems for high-volume scenarios
* **Timeout Optimization**: Balance SLA requirements with user experience
* **Escalation Chains**: Implement efficient escalation to prevent bottlenecks
* **Audit Efficiency**: Configure audit logging to minimize performance impact
* **Resource Management**: Monitor HITL resource usage and optimize accordingly

### Compliance and Security

* **Audit Trails**: Maintain comprehensive audit logs for all human interactions
* **User Authentication**: Implement proper user identification and authorization
* **Data Sanitization**: Sanitize sensitive data in approval requests
* **Retention Policies**: Configure appropriate data retention for compliance
* **Access Controls**: Restrict HITL access based on user roles and permissions

## Troubleshooting

### Common Issues

**Approval requests not appearing**: Check that interaction channels are properly initialized and available.

**Timeouts not working**: Verify timeout configuration and ensure fallback actions are properly configured.

**Batch processing issues**: Check batch manager configuration and ensure proper event handling.

**Confidence gates not triggering**: Verify confidence source configuration and threshold settings.

**Audit logs missing**: Ensure audit configuration is enabled and audit event handlers are properly registered.

### Debugging HITL

Enable detailed logging for troubleshooting:

```csharp
// Configure detailed HITL logging
var graphOptions = new GraphOptions
{
    LogLevel = LogLevel.Debug,
    EnableHITLLogging = true,
    HITLLogLevel = LogLevel.Trace
};

var graphLogger = new SemanticKernelGraphLogger(logger, graphOptions);

// Enable channel-specific debugging
consoleChannel.EnableDebugMode = true;
webApiChannel.EnableDebugMode = true;
```

### Performance Monitoring

Monitor HITL performance metrics:

```csharp
// Get comprehensive HITL metrics
var overallMetrics = await executor.GetHITLMetricsAsync();

Console.WriteLine($"HITL Performance Summary:");
Console.WriteLine($"  Total Interactions: {overallMetrics.TotalInteractions}");
Console.WriteLine($"  Average Response Time: {overallMetrics.AverageResponseTime}");
Console.WriteLine($"  SLA Compliance: {overallMetrics.SlaComplianceRate:P}");
Console.WriteLine($"  User Satisfaction: {overallMetrics.UserSatisfactionScore:F2}");
```

## See Also

* [Conditional Nodes](conditional-nodes-quickstart.md) - Understanding conditional execution and routing
* [Error Handling and Resilience](error-handling-and-resilience.md) - Managing failures and recovery
* [State Management](state-quickstart.md) - Persisting HITL state and decisions
* [Streaming Execution](streaming-quickstart.md) - Real-time monitoring of HITL events
* [Graph Execution](execution.md) - Understanding the execution lifecycle
