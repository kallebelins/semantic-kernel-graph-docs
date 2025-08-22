# Human-in-the-Loop (HITL)

The Human-in-the-Loop system in SemanticKernel.Graph provides comprehensive capabilities for integrating human decision-making into automated workflows. It enables you to pause graph execution, request human approval, validate confidence levels, and manage batch approval processes.

## Overview

The HITL system consists of several key components:

* **`HumanApprovalGraphNode`**: Pauses execution for human approval decisions
* **`ConfidenceGateGraphNode`**: Monitors confidence levels and triggers human intervention when thresholds are not met
* **`HumanApprovalBatchManager`**: Groups and manages multiple approval requests efficiently
* **`HumanInteractionStore`**: Persists and coordinates HITL requests and responses
* **`WebApiHumanInteractionChannel`**: Web-based interaction channel for production deployments

## Core Classes

### HumanApprovalGraphNode

Pauses graph execution and waits for human approval before continuing:

```csharp
public sealed class HumanApprovalGraphNode : IGraphNode
{
    public HumanApprovalGraphNode(
        string approvalTitle,
        string approvalMessage,
        IHumanInteractionChannel interactionChannel,
        string? nodeId = null,
        IGraphLogger? logger = null);
    
    public static HumanApprovalGraphNode CreateConditional(
        string approvalTitle,
        string approvalMessage,
        Func<GraphState, bool> activationCondition,
        IHumanInteractionChannel interactionChannel,
        string? nodeId = null,
        IGraphLogger? logger = null);
}
```

**Constructor Parameters:**
* `approvalTitle`: Title of the approval request
* `approvalMessage`: Detailed message presented to the user
* `interactionChannel`: Communication channel for user interaction
* `nodeId`: Optional unique identifier for the node
* `logger`: Optional logger for diagnostics

**Key Features:**
* **Conditional activation**: Optional activation conditions based on graph state
* **Multiple channels**: Support for console, web API, CLI, and custom channels
* **Configurable timeouts**: Automatic fallback actions when no response is received
* **Batch approval**: Integration with batch approval management
* **Context awareness**: Rich context information for human decision-making
* **State modifications**: Optional user-approved state changes
* **Audit trail**: Comprehensive tracking of approval decisions and metadata

**Input Parameters:**
* `execution_context`: Current execution context
* `previous_result`: Result from previous node execution
* `graph_state`: Current graph state
* `user_context`: User-specific context information

**Output Parameters:**
* `approval_result`: Boolean result of the approval decision
* `user_response`: Full user response object
* `approved_by`: Identifier of the user who made the decision
* `approval_timestamp`: When the decision was made
* `user_modifications`: Any state modifications requested by the user

### ConfidenceGateGraphNode

Monitors confidence levels and triggers human intervention when thresholds are not met:

```csharp
public sealed class ConfidenceGateGraphNode : IGraphNode
{
    public ConfidenceGateGraphNode(
        double confidenceThreshold = 0.7,
        string? nodeId = null,
        IGraphLogger? logger = null);
    
    public static ConfidenceGateGraphNode CreateWithInteraction(
        double confidenceThreshold,
        IHumanInteractionChannel interactionChannel,
        string? nodeId = null,
        IGraphLogger? logger = null);
}
```

**Constructor Parameters:**
* `confidenceThreshold`: Minimum confidence threshold (0.0 to 1.0)
* `nodeId`: Optional unique identifier for the node
* `logger`: Optional logger for diagnostics

**Key Features:**
* **Configurable threshold**: Set minimum confidence levels for automatic processing
* **Multiple sources**: Support for LLM confidence, similarity scores, and custom metrics
* **Automatic analysis**: Detects patterns of uncertainty and potential issues
* **Escalation levels**: Different approval requirements based on confidence severity
* **Learning capabilities**: Adjusts thresholds based on historical feedback
* **Manual bypass**: Optional manual override capabilities

**Input Parameters:**
* `confidence_score`: Overall confidence score
* `uncertainty_factors`: Factors contributing to uncertainty
* `llm_confidence`: LLM-generated confidence metrics
* `similarity_scores`: Similarity-based confidence indicators
* `validation_results`: Validation outcome confidence
* `previous_confidence_history`: Historical confidence data

**Output Parameters:**
* `gate_result`: Whether the gate passed or blocked execution
* `confidence_level`: Current confidence level assessment
* `uncertainty_analysis`: Detailed uncertainty factor analysis
* `human_override`: Whether manual override was applied
* `confidence_sources`: Sources contributing to confidence calculation
* `gate_decision_reason`: Explanation of gate decision

### HumanApprovalBatchManager

Efficiently manages and processes multiple approval requests:

```csharp
public sealed class HumanApprovalBatchManager : IDisposable
{
    public HumanApprovalBatchManager(
        IHumanInteractionChannel defaultChannel,
        BatchApprovalOptions? options = null,
        IGraphLogger? logger = null);
}
```

**Constructor Parameters:**
* `defaultChannel`: Default interaction channel for processing batches
* `options`: Configuration options for batch processing
* `logger`: Optional logger for diagnostics

**Key Features:**
* **Smart grouping**: Groups requests by type, priority, user, and context
* **Configurable timeouts**: Batch formation and processing timeouts
* **Partial approval**: Option to process incomplete batches
* **Detailed statistics**: Performance and usage metrics
* **Thread safety**: Safe concurrent operations
* **Persistence**: Optional batch state persistence

**Events:**
* `BatchFormed`: Raised when a batch is ready for processing
* `BatchCompleted`: Raised when a batch processing is finished
* `RequestAddedToBatch`: Raised when a request is added to a batch

**Batch Configuration Options:**
```csharp
public class BatchApprovalOptions
{
    public int MaxBatchSize { get; set; } = 10;
    public TimeSpan BatchFormationTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan MaxBatchAge { get; set; } = TimeSpan.FromHours(1);
    public bool AllowPartialBatches { get; set; } = true;
    public string[] GroupingCriteria { get; set; } = { "type", "priority" };
}
```

### HumanInteractionStore

Abstracts persistence and coordination of HITL requests and responses:

```csharp
public interface IHumanInteractionStore
{
    Task<bool> AddPendingAsync(HumanInterruptionRequest request, CancellationToken cancellationToken = default);
    Task<HumanInterruptionRequest?> GetRequestAsync(string requestId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HumanInterruptionRequest>> ListPendingAsync(string? executionId = null, string? nodeId = null, CancellationToken cancellationToken = default);
    Task<bool> SubmitResponseAsync(HumanInterruptionResponse response, CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(string requestId, CancellationToken cancellationToken = default);
    Task<ApprovalStatus> GetStatusAsync(string requestId, CancellationToken cancellationToken = default);
    Task<HumanInterruptionResponse> AwaitResponseAsync(string requestId, CancellationToken cancellationToken = default);
}
```

**Key Features:**
* **Request management**: Add, retrieve, and track pending requests
* **Response handling**: Submit and process user responses
* **Status tracking**: Monitor request status throughout lifecycle
* **Filtering**: List requests by execution ID or node ID
* **Async operations**: Non-blocking request/response handling
* **Cancellation support**: Graceful cancellation of pending requests

**Events:**
* `RequestAdded`: Raised when a new request is added
* `ResponseSubmitted`: Raised when a response is submitted
* `RequestCancelled`: Raised when a request is cancelled

### WebApiHumanInteractionChannel

Web-based interaction channel for production deployments:

```csharp
public sealed class WebApiHumanInteractionChannel : IHumanInteractionChannel, IDisposable
{
    public WebApiHumanInteractionChannel(IHumanInteractionStore store);
    
    public async Task InitializeAsync(Dictionary<string, object>? configuration = null);
}
```

**Constructor Parameters:**
* `store`: Backing store for request/response coordination

**Key Features:**
* **Web API integration**: Exposes requests via backing store for REST consumption
* **Event-driven**: Raises events for request lifecycle changes
* **Configuration**: Flexible configuration options
* **Store cooperation**: Works with any `IHumanInteractionStore` implementation
* **Production ready**: Designed for web-based approval interfaces

**Events:**
* `ResponseReceived`: Raised when a response is received
* `RequestTimedOut`: Raised when a request times out
* `RequestAvailable`: Raised when a new request becomes available
* `RequestCancelled`: Raised when a request is cancelled

## Configuration and Options

### HumanInteractionTimeout

Configures timeout behavior for human interactions:

```csharp
public class HumanInteractionTimeout
{
    public TimeSpan PrimaryTimeout { get; set; } = TimeSpan.FromMinutes(30);
    public TimeSpan EscalationTimeout { get; set; } = TimeSpan.FromHours(2);
    public TimeoutAction DefaultAction { get; set; } = TimeoutAction.Reject;
    public bool EnableEscalation { get; set; } = false;
    public string[] EscalationRecipients { get; set; } = Array.Empty<string>();
}
```

**Properties:**
* **`PrimaryTimeout`**: Initial timeout for the interaction
* **`EscalationTimeout`**: Extended timeout for escalation scenarios
* **`DefaultAction`**: Action to take when timeout occurs
* **`EnableEscalation`**: Whether to enable timeout escalation
* **`EscalationRecipients`**: Recipients for escalation notifications

### HumanInteractionOption

Defines available options for user selection:

```csharp
public class HumanInteractionOption
{
    public string OptionId { get; set; } = string.Empty;
    public string DisplayText { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object? Value { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool RequiresJustification { get; set; } = false;
    public string[] AllowedUsers { get; set; } = Array.Empty<string>();
}
```

**Properties:**
* **`OptionId`**: Unique identifier for the option
* **`DisplayText`**: Human-readable text for display
* **`Description`**: Detailed description of the option
* **`Value`**: Value returned when option is selected
* **`IsDefault`**: Whether this is the default option
* **`RequiresJustification`**: Whether justification is required
* **`AllowedUsers`**: Users allowed to select this option

### BatchApprovalOptions

Configures batch approval processing behavior:

```csharp
public class BatchApprovalOptions
{
    public int MaxBatchSize { get; set; } = 10;
    public TimeSpan BatchFormationTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan MaxBatchAge { get; set; } = TimeSpan.FromHours(1);
    public bool AllowPartialBatches { get; set; } = true;
    public string[] GroupingCriteria { get; set; } = { "type", "priority" };
    public bool EnableNotifications { get; set; } = true;
    public string[] NotificationChannels { get; set; } = { "email", "webhook" };
}
```

**Properties:**
* **`MaxBatchSize`**: Maximum number of requests per batch
* **`BatchFormationTimeout`**: Time to wait for batch formation
* **`MaxBatchAge`**: Maximum age of a batch before processing
* **`AllowPartialBatches`**: Whether to process incomplete batches
* **`GroupingCriteria`**: Criteria for grouping requests
* **`EnableNotifications`**: Whether to enable batch notifications
* **`NotificationChannels`**: Channels for batch notifications

## Usage Examples

### Basic Human Approval

Create a simple approval node with console interaction:

```csharp
// Create console interaction channel
var consoleChannel = new ConsoleHumanInteractionChannel();

// Create approval node
var approvalNode = new HumanApprovalGraphNode(
    "Document Review",
    "Please review the generated document for accuracy and completeness",
    consoleChannel,
    "doc_review_approval");

// Configure approval options
approvalNode.AddApprovalOption("approve", "Approve and Continue", true, true)
            .AddApprovalOption("reject", "Reject and Stop", false)
            .AddApprovalOption("modify", "Approve with Modifications", "modified");

// Configure timeout
approvalNode.WithTimeout(TimeSpan.FromHours(24), TimeoutAction.Reject);

// Add to graph
graph.AddNode(approvalNode);
graph.AddEdge(previousNode, approvalNode);
```

### Conditional Approval

Create approval nodes that only activate under specific conditions:

```csharp
// Create conditional approval node
var conditionalApproval = HumanApprovalGraphNode.CreateConditional(
    "High-Risk Transaction",
    "Approval required for transactions above threshold",
    state => state.GetValue<decimal>("transaction_amount") > 10000m,
    consoleChannel,
    "high_risk_approval");

// Configure priority and timeout
conditionalApproval.WithPriority(InteractionPriority.High)
                  .WithTimeout(TimeSpan.FromHours(4), TimeoutAction.Escalate);

// Add approval options
conditionalApproval.AddApprovalOption("approve", "Approve Transaction", true)
                  .AddApprovalOption("reject", "Reject Transaction", false)
                  .AddApprovalOption("review", "Request Additional Review", "review");

// Add routing based on approval result
// Add conditional routing using executor helpers. Use KernelArguments predicates
// to evaluate approval_result returned by the approval node.
graph.ConnectWhen(
    conditionalApproval.NodeId,
    approvedNode.NodeId,
    args => args.TryGetValue("approval_result", out var v) && v is bool b && b,
    "approval_result_true");

graph.ConnectWhen(
    conditionalApproval.NodeId,
    rejectedNode.NodeId,
    args => args.TryGetValue("approval_result", out var v) && v is bool b && !b,
    "approval_result_false");

graph.ConnectWhen(
    conditionalApproval.NodeId,
    reviewNode.NodeId,
    args => args.TryGetValue("approval_result", out var v) && v?.ToString() == "review",
    "approval_result_review");
```

### Confidence Gate

Monitor confidence levels and trigger human intervention:

```csharp
// Create confidence gate node
var confidenceGate = new ConfidenceGateGraphNode(
    confidenceThreshold: 0.8,
    nodeId: "quality_gate");

// Configure confidence sources
confidenceGate.ConfidenceSources.Add(new LLMConfidenceSource("llm_confidence"));
confidenceGate.ConfidenceSources.Add(new SimilarityConfidenceSource("similarity_score"));
confidenceGate.ConfidenceSources.Add(new ValidationConfidenceSource("validation_result"));

// Configure aggregation strategy
confidenceGate.AggregationStrategy = ConfidenceAggregationStrategy.WeightedAverage;

// Configure timeout for human interaction
confidenceGate.TimeoutConfiguration.PrimaryTimeout = TimeSpan.FromMinutes(30);
confidenceGate.TimeoutConfiguration.DefaultAction = TimeoutAction.Reject;

// Allow manual bypass for urgent cases
confidenceGate.WithManualBypass(true);

// Add to graph
graph.AddNode(confidenceGate);
graph.AddEdge(previousNode, confidenceGate);
```

### Batch Approval Management

Efficiently manage multiple approval requests:

```csharp
// Create batch manager
var batchManager = new HumanApprovalBatchManager(
    defaultChannel: consoleChannel,
    options: new BatchApprovalOptions
    {
        MaxBatchSize = 20,
        BatchFormationTimeout = TimeSpan.FromMinutes(10),
        AllowPartialBatches = true,
        GroupingCriteria = new[] { "type", "priority" }
    });

// Subscribe to batch events
batchManager.BatchFormed += (sender, batch) =>
{
    Console.WriteLine($"Batch {batch.BatchId} formed with {batch.Requests.Count} requests");
};

batchManager.BatchCompleted += (sender, args) =>
{
    Console.WriteLine($"Batch {args.Batch.BatchId} completed in {args.ProcessingTime}");
};

// Use batch manager in approval nodes
var batchApprovalNode = new HumanApprovalGraphNode(
    "Batch Document Review",
    "Review multiple documents for approval",
    consoleChannel,
    "batch_doc_review");

// Configure batch processing
batchApprovalNode.WithTimeout(TimeSpan.FromHours(2), TimeoutAction.Escalate);
```

### Web API Integration

Implement web-based approval interfaces:

```csharp
// Create web API interaction channel
var interactionStore = new InMemoryHumanInteractionStore();
var webApiChannel = new WebApiHumanInteractionChannel(interactionStore);

// Initialize web API channel
await webApiChannel.InitializeAsync(new Dictionary<string, object>
{
    ["api_base_url"] = "https://api.example.com/approvals",
    ["timeout_seconds"] = 300,
    ["enable_notifications"] = true
});

// Subscribe to channel events
webApiChannel.ResponseReceived += (sender, response) =>
{
    Console.WriteLine($"Received response: {response.Status} for request {response.RequestId}");
};

webApiChannel.RequestTimedOut += (sender, request) =>
{
    Console.WriteLine($"Request {request.RequestId} timed out");
};

// Create web-based approval node
var webApprovalNode = new HumanApprovalGraphNode(
    "Web Document Approval",
    "Approve document via web interface",
    webApiChannel,
    "web_doc_approval");

// Configure web-specific options
webApprovalNode.WithTimeout(TimeSpan.FromHours(24), TimeoutAction.Escalate)
               .WithPriority(InteractionPriority.Normal);

// Add approval options
webApprovalNode.AddApprovalOption("approve", "Approve Document", true, true)
               .AddApprovalOption("reject", "Reject Document", false)
               .AddApprovalOption("modify", "Request Modifications", "modify");
```

## Integration with Graph Executor

### Kernel Builder Extensions

Add HITL support to the kernel builder:

```csharp
var builder = Kernel.CreateBuilder();

// Add basic HITL support
builder.AddHumanInTheLoop(consoleChannel);

// Add console-based HITL
builder.AddConsoleHumanInteraction(new Dictionary<string, object>
{
    ["prompt_format"] = "APPROVAL REQUIRED: {message}\nEnter 'approve' or 'reject': ",
    ["validation_pattern"] = @"^(approve|reject)$",
    ["retry_count"] = 3
});

// Add web API-based HITL
builder.AddWebApiHumanInteraction();

// Build kernel with HITL support
var kernel = builder.Build();
```

### Graph Executor Configuration

Configure HITL behavior at the executor level:

```csharp
var executor = new GraphExecutor("hitl-enabled-graph");

// Set default HITL timeout
executor.WithHumanApprovalTimeout(TimeSpan.FromHours(24), TimeoutAction.Reject);

// Configure HITL audit service
executor.WithHitlAuditService(new HitlAuditService(
    interactionStore: interactionStore,
    memoryService: memoryService,
    logger: logger));

// Add HITL metrics collection
executor.WithHitlMetrics(new HitlMetricsCollector());
```

## Advanced Patterns

### Multi-Level Approval

Implement hierarchical approval workflows:

```csharp
// Level 1: Basic approval
var level1Approval = new HumanApprovalGraphNode(
    "Level 1 Approval",
    "Initial review and approval",
    consoleChannel,
    "level1_approval");

// Level 2: Manager approval (only if amount > threshold)
var level2Approval = HumanApprovalGraphNode.CreateConditional(
    "Level 2 Approval",
    "Manager approval required for high-value transactions",
    state => state.GetValue<decimal>("transaction_amount") > 50000m,
    managerChannel,
    "level2_approval");

// Level 3: Executive approval (only if amount > threshold)
var level3Approval = HumanApprovalGraphNode.CreateConditional(
    "Level 3 Approval",
    "Executive approval required for critical transactions",
    state => state.GetValue<decimal>("transaction_amount") > 100000m,
    executiveChannel,
    "level3_approval");

// Configure approval chain
level1Approval.WithTimeout(TimeSpan.FromHours(4), TimeoutAction.Escalate);
level2Approval.WithTimeout(TimeSpan.FromHours(8), TimeoutAction.Escalate);
level3Approval.WithTimeout(TimeSpan.FromHours(24), TimeoutAction.Reject);

// Add to graph with conditional routing
graph.AddEdge(level1Approval, level2Approval);
// Route based on transaction_amount in KernelArguments
graph.ConnectWhen(
    level2Approval.NodeId,
    level3Approval.NodeId,
    args => args.TryGetValue("transaction_amount", out var v) &&
            decimal.TryParse(v?.ToString(), out var amt) && amt > 100000m,
    "transaction_amount_gt_100000");
```

### Confidence-Based Routing

Route execution based on confidence levels:

```csharp
// Create confidence gate
var confidenceGate = new ConfidenceGateGraphNode(0.7, "quality_gate");

// Configure confidence sources
confidenceGate.ConfidenceSources.Add(new LLMConfidenceSource("llm_confidence"));
confidenceGate.ConfidenceSources.Add(new ValidationConfidenceSource("validation_score"));

// Add routing based on confidence
// Route based on gate_result produced by the confidence gate node
graph.ConnectWhen(
    confidenceGate.NodeId,
    highConfidenceNode.NodeId,
    args => args.TryGetValue("gate_result", out var v) && v is bool bv && bv,
    "gate_result_true");

graph.ConnectWhen(
    confidenceGate.NodeId,
    lowConfidenceNode.NodeId,
    args => args.TryGetValue("gate_result", out var v) && v is bool bv2 && !bv2,
    "gate_result_false");

// Configure low confidence path with human approval
var humanReviewNode = new HumanApprovalGraphNode(
    "Human Review Required",
    "Low confidence result requires human review",
    consoleChannel,
    "human_review");

lowConfidenceNode.AddEdge(humanReviewNode);
```

### Batch Processing with Escalation

Handle batch approvals with escalation logic:

```csharp
// Create batch manager with escalation
var batchManager = new HumanApprovalBatchManager(
    defaultChannel: consoleChannel,
    options: new BatchApprovalOptions
    {
        MaxBatchSize = 15,
        BatchFormationTimeout = TimeSpan.FromMinutes(15),
        AllowPartialBatches = false
    });

// Subscribe to batch events
batchManager.BatchFormed += async (sender, batch) =>
{
    // Send batch notification
    await SendBatchNotificationAsync(batch);
    
    // Start escalation timer
    _ = Task.Run(async () =>
    {
        await Task.Delay(TimeSpan.FromMinutes(30));
        if (batch.Requests.Any(r => r.Status == ApprovalStatus.Pending))
        {
            await EscalateBatchAsync(batch);
        }
    });
};

// Use batch manager in approval workflow
var batchApprovalNode = new HumanApprovalGraphNode(
    "Batch Document Review",
    "Review multiple documents for approval",
    consoleChannel,
    "batch_doc_review");

// Configure batch processing
batchApprovalNode.WithTimeout(TimeSpan.FromHours(1), TimeoutAction.Escalate);
```

## Performance and Monitoring

### Metrics Collection

Monitor HITL performance and usage:

```csharp
// Create HITL metrics collector
var hitlMetrics = new HitlMetricsCollector();

// Subscribe to approval events
approvalNode.ApprovalCompleted += (sender, args) =>
{
    hitlMetrics.RecordApproval(
        nodeId: args.NodeId,
        duration: args.Duration,
        result: args.Result,
        channel: args.ChannelType);
};

// Subscribe to confidence gate events
confidenceGate.GateActivated += (sender, args) =>
{
    hitlMetrics.RecordGateActivation(
        nodeId: args.NodeId,
        confidenceLevel: args.ConfidenceLevel,
        threshold: args.Threshold);
};

// Get HITL performance summary
var summary = hitlMetrics.GetPerformanceSummary();
Console.WriteLine($"Average approval time: {summary.AverageApprovalTime}");
Console.WriteLine($"Confidence gate activation rate: {summary.GateActivationRate:P2}");
```

### Audit Trail

Maintain comprehensive audit records:

```csharp
// Create HITL audit service
var auditService = new HitlAuditService(
    interactionStore: interactionStore,
    memoryService: memoryService,
    logger: logger);

// Configure audit options
auditService.Configure(new HitlAuditOptions
{
    EnableDetailedLogging = true,
    RetainAuditDataFor = TimeSpan.FromDays(90),
    LogSensitiveData = false,
    EnableComplianceReporting = true
});

// Subscribe to audit events
auditService.AuditRecordCreated += (sender, record) =>
{
    Console.WriteLine($"Audit record created: {record.RecordId}");
};

// Get audit summary
var auditSummary = await auditService.GetAuditSummaryAsync(
    startDate: DateTimeOffset.UtcNow.AddDays(-30),
    endDate: DateTimeOffset.UtcNow);
```

## Security and Compliance

### Authentication and Authorization

Secure HITL interactions:

```csharp
// Create authenticated interaction channel
var authenticatedChannel = new AuthenticatedHumanInteractionChannel(
    baseChannel: webApiChannel,
    authProvider: new JwtAuthProvider(),
    userStore: new UserStore());

// Configure authorization policies
authenticatedChannel.ConfigureAuthorization(new AuthorizationPolicy
{
    RequireAuthentication = true,
    RequireApprovalRole = true,
    AllowedRoles = new[] { "approver", "manager", "admin" },
    RequireMFA = true
});

// Use authenticated channel in approval node
var secureApprovalNode = new HumanApprovalGraphNode(
    "Secure Document Approval",
    "Approval requires proper authentication and authorization",
    authenticatedChannel,
    "secure_approval");
```

### Data Privacy

Protect sensitive information in HITL workflows:

```csharp
// Create privacy-aware approval node
var privacyApprovalNode = new HumanApprovalGraphNode(
    "Privacy Review",
    "Review data processing for privacy compliance",
    consoleChannel,
    "privacy_approval");

// Configure privacy options
privacyApprovalNode.AllowStateModifications = false; // Prevent state changes
privacyApprovalNode.WithPrivacyProtection(new PrivacyProtectionOptions
{
    MaskSensitiveData = true,
    LogAccessAttempts = true,
    RequireJustification = true,
    AuditDataAccess = true
});

// Add privacy-specific approval options
privacyApprovalNode.AddApprovalOption("approve", "Approve Processing", true)
                   .AddApprovalOption("reject", "Reject Processing", false)
                   .AddApprovalOption("modify", "Request Modifications", "modify");
```

## See Also

* [Human-in-the-Loop Guide](../how-to/human-in-the-loop.md) - Comprehensive guide to HITL concepts and techniques
* [Conditional Nodes](./conditional-nodes.md) - Conditional execution and routing patterns
* [Graph State](./graph-state.md) - State management for HITL workflows
* [Graph Executor](./graph-executor.md) - Core execution engine that supports HITL
* [HITL Examples](../../examples/hitl-examples.md) - Complete examples demonstrating HITL capabilities
