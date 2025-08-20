# Conditional Nodes Example

This example demonstrates conditional routing and decision-making in graph-based workflows using `ConditionalGraphNode` and `ConditionalEdge`. It shows how to implement dynamic execution paths based on state conditions and user input.

## Objective

Learn how to implement conditional logic in graph-based workflows to:
* Route execution based on dynamic conditions
* Implement if/else branching logic
* Use conditional expressions for complex routing
* Handle multiple execution paths efficiently
* Integrate conditional nodes with other graph patterns

## Prerequisites

* **.NET 8.0** or later
* **OpenAI API Key** configured in `appsettings.json`
* **Semantic Kernel Graph package** installed
* Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Node Types](../concepts/node-types.md)

## Key Components

### Concepts and Techniques

* **Conditional Routing**: Dynamic execution path selection based on state conditions
* **Conditional Expressions**: Boolean expressions that determine execution flow
* **Branching Logic**: Multiple execution paths with different outcomes
* **State Evaluation**: Runtime evaluation of graph state for decision making

### Core Classes

* `ConditionalGraphNode`: Node that evaluates conditions and routes execution
* `ConditionalEdge`: Edge that connects nodes based on conditional logic
* `ConditionalExpressionEvaluator`: Evaluates boolean expressions for routing
* `GraphState`: Carries state information used in conditional evaluation

## Running the Example

### Getting Started

This example demonstrates conditional routing and decision-making with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Basic Conditional Routing

This example demonstrates simple if/else branching based on input values.

```csharp
// Create kernel with mock configuration
var kernel = CreateKernel();

// Create conditional node for basic routing
var conditionalNode = new ConditionalGraphNode(
    "BasicConditional",
    "Basic conditional routing example",
    logger)
{
    ConditionExpression = "input_value > 10",
    TrueNodeId = "high-value-processor",
    FalseNodeId = "low-value-processor"
};

// Create processing nodes
var highValueProcessor = new FunctionGraphNode(
    "high-value-processor",
    "Process high value inputs",
    async (context) =>
    {
        var inputValue = context.GetValue<int>("input_value");
        var result = inputValue * 2;
        context.SetValue("processed_result", result);
        context.SetValue("processing_type", "high-value");
        return result;
    });

var lowValueProcessor = new FunctionGraphNode(
    "low-value-processor",
    "Process low value inputs",
    async (context) =>
    {
        var inputValue = context.GetValue<int>("input_value");
        var result = inputValue + 5;
        context.SetValue("processed_result", result);
        context.SetValue("processing_type", "low-value");
        return result;
    });

// Create executor and add nodes
var executor = new GraphExecutor("ConditionalExample", "Basic conditional routing", logger);
executor.AddNode(conditionalNode);
executor.AddNode(highValueProcessor);
executor.AddNode(lowValueProcessor);

// Set start node
executor.SetStartNode(conditionalNode.NodeId);

// Test with different input values
var testValues = new[] { 5, 15, 8, 20 };

foreach (var testValue in testValues)
{
    var arguments = new KernelArguments
    {
        ["input_value"] = testValue
    };

    Console.WriteLine($"🧪 Testing with input value: {testValue}");
    var result = await executor.ExecuteAsync(kernel, arguments);
    
    var processedResult = result.GetValue<int>("processed_result");
    var processingType = result.GetValue<string>("processing_type");
    
    Console.WriteLine($"   Result: {processedResult} (Type: {processingType})");
}
```

### 2. Complex Conditional Logic

Demonstrates advanced conditional expressions with multiple conditions.

```csharp
// Create complex conditional node
var complexConditional = new ConditionalGraphNode(
    "ComplexConditional",
    "Complex conditional logic example",
    logger)
{
    ConditionExpression = "(user_age >= 18) && (user_income > 50000) && (credit_score >= 700)",
    TrueNodeId = "approve-loan",
    FalseNodeId = "review-application"
};

// Create loan approval nodes
var approveLoan = new FunctionGraphNode(
    "approve-loan",
    "Approve loan application",
    async (context) =>
    {
        var userName = context.GetValue<string>("user_name");
        var loanAmount = context.GetValue<decimal>("loan_amount");
        
        context.SetValue("loan_status", "approved");
        context.SetValue("approval_reason", "All criteria met");
        context.SetValue("interest_rate", 0.045m);
        
        return $"Loan approved for {userName}: ${loanAmount:N2} at 4.5% interest";
    });

var reviewApplication = new FunctionGraphNode(
    "review-application",
    "Review loan application",
    async (context) =>
    {
        var userName = context.GetValue<string>("user_name");
        var userAge = context.GetValue<int>("user_age");
        var userIncome = context.GetValue<decimal>("user_income");
        var creditScore = context.GetValue<int>("credit_score");
        
        var reasons = new List<string>();
        if (userAge < 18) reasons.Add("Underage");
        if (userIncome <= 50000) reasons.Add("Insufficient income");
        if (creditScore < 700) reasons.Add("Low credit score");
        
        context.SetValue("loan_status", "under_review");
        context.SetValue("review_reasons", reasons);
        context.SetValue("next_steps", "Manual review required");
        
        return $"Application under review for {userName}. Reasons: {string.Join(", ", reasons)}";
    });

// Add nodes to executor
executor.AddNode(complexConditional);
executor.AddNode(approveLoan);
executor.AddNode(reviewApplication);

// Test complex conditions
var testApplications = new[]
{
    new { Name = "Alice", Age = 25, Income = 75000m, CreditScore = 750, LoanAmount = 50000m },
    new { Name = "Bob", Age = 17, Income = 60000m, CreditScore = 720, LoanAmount = 30000m },
    new { Name = "Carol", Age = 30, Income = 45000m, CreditScore = 800, LoanAmount = 40000m },
    new { Name = "David", Age = 28, Income = 80000m, CreditScore = 650, LoanAmount = 60000m }
};

foreach (var app in testApplications)
{
    var arguments = new KernelArguments
    {
        ["user_name"] = app.Name,
        ["user_age"] = app.Age,
        ["user_income"] = app.Income,
        ["credit_score"] = app.CreditScore,
        ["loan_amount"] = app.LoanAmount
    };

    Console.WriteLine($"\n🏦 Processing loan application for {app.Name}:");
    Console.WriteLine($"   Age: {app.Age}, Income: ${app.Income:N0}, Credit: {app.CreditScore}");
    
    var result = await executor.ExecuteAsync(kernel, arguments);
    var loanStatus = result.GetValue<string>("loan_status");
    
    Console.WriteLine($"   Status: {loanStatus}");
    
    if (loanStatus == "approved")
    {
        var interestRate = result.GetValue<decimal>("interest_rate");
        Console.WriteLine($"   Interest Rate: {interestRate:P1}");
    }
    else
    {
        var reasons = result.GetValue<List<string>>("review_reasons");
        var nextSteps = result.GetValue<string>("next_steps");
        Console.WriteLine($"   Review Reasons: {string.Join(", ", reasons)}");
        Console.WriteLine($"   Next Steps: {nextSteps}");
    }
}
```

### 3. Dynamic Conditional Routing

Shows how to implement dynamic routing based on runtime state changes.

```csharp
// Create dynamic conditional node
var dynamicConditional = new ConditionalGraphNode(
    "DynamicConditional",
    "Dynamic conditional routing example",
    logger)
{
    ConditionExpression = "current_step < max_steps && !is_complete",
    TrueNodeId = "continue-processing",
    FalseNodeId = "finalize-workflow"
};

// Create processing nodes
var continueProcessing = new FunctionGraphNode(
    "continue-processing",
    "Continue workflow processing",
    async (context) =>
    {
        var currentStep = context.GetValue<int>("current_step");
        var maxSteps = context.GetValue<int>("max_steps");
        
        // Simulate processing
        await Task.Delay(100);
        
        // Update state
        context.SetValue("current_step", currentStep + 1);
        context.SetValue("last_processed_step", currentStep);
        
        // Check if we should continue
        if (currentStep + 1 >= maxSteps)
        {
            context.SetValue("is_complete", true);
        }
        
        return $"Processed step {currentStep} of {maxSteps}";
    });

var finalizeWorkflow = new FunctionGraphNode(
    "finalize-workflow",
    "Finalize workflow execution",
    async (context) =>
    {
        var totalSteps = context.GetValue<int>("max_steps");
        var finalResult = context.GetValue<string>("workflow_result") ?? "Default result";
        
        context.SetValue("workflow_status", "completed");
        context.SetValue("completion_timestamp", DateTime.UtcNow);
        
        return $"Workflow completed after {totalSteps} steps. Final result: {finalResult}";
    });

// Add nodes to executor
executor.AddNode(dynamicConditional);
executor.AddNode(continueProcessing);
executor.AddNode(finalizeWorkflow);

// Test dynamic routing
var workflowArgs = new KernelArguments
{
    ["current_step"] = 0,
    ["max_steps"] = 5,
    ["is_complete"] = false,
    ["workflow_result"] = "Dynamic processing completed"
};

Console.WriteLine("🔄 Starting dynamic workflow...");

var workflowResult = await executor.ExecuteAsync(kernel, workflowArgs);
var workflowStatus = workflowResult.GetValue<string>("workflow_status");
var finalStep = workflowResult.GetValue<int>("current_step");

Console.WriteLine($"✅ Workflow {workflowStatus} at step {finalStep}");
```

### 4. Multi-Conditional Workflow

Demonstrates a workflow with multiple conditional branches and complex routing.

```csharp
// Create multi-conditional workflow
var multiConditional = new ConditionalGraphNode(
    "MultiConditional",
    "Multi-conditional workflow example",
    logger)
{
    ConditionExpression = "request_type == 'urgent' && priority_level >= 8",
    TrueNodeId = "urgent-processor",
    FalseNodeId = "standard-processor"
};

var urgentProcessor = new ConditionalGraphNode(
    "UrgentConditional",
    "Urgent request conditional",
    logger)
{
    ConditionExpression = "available_resources >= 2",
    TrueNodeId = "immediate-processing",
    FalseNodeId = "resource-wait"
});

var standardProcessor = new ConditionalGraphNode(
    "StandardConditional",
    "Standard request conditional",
    logger)
{
    ConditionExpression = "queue_length < 10",
    TrueNodeId = "queue-processing",
    FalseNodeId = "delayed-processing"
});

// Create processing nodes
var immediateProcessing = new FunctionGraphNode(
    "immediate-processing",
    "Process urgent request immediately",
    async (context) =>
    {
        var requestId = context.GetValue<string>("request_id");
        context.SetValue("processing_time", "immediate");
        context.SetValue("priority_handled", "urgent");
        return $"Urgent request {requestId} processed immediately";
    });

var resourceWait = new FunctionGraphNode(
    "resource-wait",
    "Wait for available resources",
    async (context) =>
    {
        var requestId = context.GetValue<string>("request_id");
        context.SetValue("processing_time", "delayed");
        context.SetValue("wait_reason", "insufficient_resources");
        return $"Urgent request {requestId} waiting for resources";
    });

var queueProcessing = new FunctionGraphNode(
    "queue-processing",
    "Process standard request from queue",
    async (context) =>
    {
        var requestId = context.GetValue<string>("request_id");
        context.SetValue("processing_time", "queued");
        context.SetValue("queue_position", context.GetValue<int>("queue_length"));
        return $"Standard request {requestId} processed from queue";
    });

var delayedProcessing = new FunctionGraphNode(
    "delayed-processing",
    "Delay standard request processing",
    async (context) =>
    {
        var requestId = context.GetValue<string>("request_id");
        context.SetValue("processing_time", "delayed");
        context.SetValue("delay_reason", "queue_full");
        return $"Standard request {requestId} delayed due to queue capacity";
    });

// Add all nodes to executor
executor.AddNode(multiConditional);
executor.AddNode(urgentProcessor);
executor.AddNode(standardProcessor);
executor.AddNode(immediateProcessing);
executor.AddNode(resourceWait);
executor.AddNode(queueProcessing);
executor.AddNode(delayedProcessing);

// Test multi-conditional workflow
var testRequests = new[]
{
    new { Id = "REQ-001", Type = "urgent", Priority = 9, Resources = 3, QueueLength = 5 },
    new { Id = "REQ-002", Type = "urgent", Priority = 7, Resources = 1, QueueLength = 8 },
    new { Id = "REQ-003", Type = "standard", Priority = 5, Resources = 2, QueueLength = 8 },
    new { Id = "REQ-004", Type = "standard", Priority = 3, Resources = 2, QueueLength = 15 }
};

foreach (var req in testRequests)
{
    var arguments = new KernelArguments
    {
        ["request_id"] = req.Id,
        ["request_type"] = req.Type,
        ["priority_level"] = req.Priority,
        ["available_resources"] = req.Resources,
        ["queue_length"] = req.QueueLength
    };

    Console.WriteLine($"\n📋 Processing request {req.Id}:");
    Console.WriteLine($"   Type: {req.Type}, Priority: {req.Priority}, Resources: {req.Resources}, Queue: {req.QueueLength}");
    
    var result = await executor.ExecuteAsync(kernel, arguments);
    var processingTime = result.GetValue<string>("processing_time");
    var processingDetails = result.GetValue<string>();
    
    Console.WriteLine($"   Processing: {processingTime}");
    Console.WriteLine($"   Details: {processingDetails}");
}
```

## Expected Output

### Basic Conditional Routing Example

```
🧪 Testing with input value: 5
   Result: 10 (Type: low-value)
🧪 Testing with input value: 15
   Result: 30 (Type: high-value)
🧪 Testing with input value: 8
   Result: 13 (Type: low-value)
🧪 Testing with input value: 20
   Result: 40 (Type: high-value)
```

### Complex Conditional Logic Example

```
🏦 Processing loan application for Alice:
   Age: 25, Income: $75,000, Credit: 750
   Status: approved
   Interest Rate: 4.5%

🏦 Processing loan application for Bob:
   Age: 17, Income: $60,000, Credit: 720
   Status: under_review
   Review Reasons: Underage
   Next Steps: Manual review required

🏦 Processing loan application for Carol:
   Age: 30, Income: $45,000, Credit: 800
   Status: under_review
   Review Reasons: Insufficient income
   Next Steps: Manual review required

🏦 Processing loan application for David:
   Age: 28, Income: $80,000, Credit: 650
   Status: under_review
   Review Reasons: Low credit score
   Next Steps: Manual review required
```

### Dynamic Conditional Routing Example

```
🔄 Starting dynamic workflow...
✅ Workflow completed at step 5
```

### Multi-Conditional Workflow Example

```
📋 Processing request REQ-001:
   Type: urgent, Priority: 9, Resources: 3, Queue: 5
   Processing: immediate
   Details: Urgent request REQ-001 processed immediately

📋 Processing request REQ-002:
   Type: urgent, Priority: 7, Resources: 1, Queue: 8
   Processing: delayed
   Details: Urgent request REQ-002 waiting for resources

📋 Processing request REQ-003:
   Type: standard, Priority: 5, Resources: 2, Queue: 8
   Processing: queued
   Details: Standard request REQ-003 processed from queue

📋 Processing request REQ-004:
   Type: standard, Priority: 3, Resources: 2, Queue: 15
   Processing: delayed
   Details: Standard request REQ-004 delayed due to queue capacity
```

## Configuration Options

### Conditional Node Configuration

```csharp
var conditionalOptions = new ConditionalNodeOptions
{
    ConditionExpression = "input_value > threshold",  // Boolean expression
    TrueNodeId = "success-path",                     // Node ID for true condition
    FalseNodeId = "failure-path",                    // Node ID for false condition
    EnableExpressionCaching = true,                  // Cache expression evaluation
    ExpressionTimeout = TimeSpan.FromSeconds(5),     // Expression evaluation timeout
    EnableDetailedLogging = true,                    // Log condition evaluation details
    FallbackNodeId = "default-path"                  // Fallback node if evaluation fails
};
```

### Conditional Edge Configuration

```csharp
var conditionalEdge = new ConditionalEdge
{
    SourceNodeId = "source-node",
    TargetNodeId = "target-node",
    Condition = "state_value == 'expected'",
    Priority = 1,                                    // Edge priority for routing
    Metadata = new Dictionary<string, object>        // Additional metadata
    {
        ["edge_type"] = "conditional",
        ["description"] = "Route based on state value"
    }
};
```

## Troubleshooting

### Common Issues

#### Condition Evaluation Fails
```bash
# Problem: Conditional expression fails to evaluate
# Solution: Check expression syntax and variable names
ConditionExpression = "simple_condition == true";
EnableDetailedLogging = true;
```

#### Routing Not Working
```bash
# Problem: Execution doesn't follow expected path
# Solution: Verify node IDs and edge connections
TrueNodeId = "correct-node-id";
FalseNodeId = "correct-node-id";
```

#### Performance Issues
```bash
# Problem: Conditional evaluation is slow
# Solution: Enable expression caching and optimize expressions
EnableExpressionCaching = true;
ExpressionTimeout = TimeSpan.FromSeconds(1);
```

### Debug Mode

Enable detailed logging for troubleshooting:

```csharp
// Enable debug logging
var logger = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
}).CreateLogger<ConditionalNodesExample>();

// Configure conditional node with debug logging
var debugConditional = new ConditionalGraphNode(
    "DebugConditional",
    "Debug conditional example",
    logger)
{
    EnableDetailedLogging = true,
    LogConditionEvaluation = true,
    LogRoutingDecisions = true
};
```

## Advanced Patterns

### Dynamic Condition Generation

```csharp
// Generate conditions dynamically based on context
var dynamicCondition = new DynamicConditionGenerator
{
    ConditionTemplate = "{field_name} {operator} {threshold_value}",
    FieldMappings = new Dictionary<string, string>
    {
        ["age"] = "user_age",
        ["income"] = "user_income",
        ["credit"] = "credit_score"
    },
    OperatorMappings = new Dictionary<string, string>
    {
        ["greater_than"] = ">",
        ["less_than"] = "<",
        ["equals"] = "=="
    }
};

var generatedCondition = dynamicCondition.GenerateCondition(
    field: "age",
    operator: "greater_than",
    threshold: "18"
);
// Result: "user_age > 18"
```

### Conditional Expression Builder

```csharp
// Build complex conditional expressions programmatically
var expressionBuilder = new ConditionalExpressionBuilder();

var complexExpression = expressionBuilder
    .StartGroup()
        .AddCondition("user_age >= 18")
        .And()
        .AddCondition("user_income > 50000")
        .And()
        .StartGroup()
            .AddCondition("credit_score >= 700")
            .Or()
            .AddCondition("co_signer_available == true")
        .EndGroup()
    .EndGroup()
    .Build();

// Result: "(user_age >= 18) && (user_income > 50000) && ((credit_score >= 700) || (co_signer_available == true))"
```

### Conditional Workflow Orchestration

```csharp
// Orchestrate multiple conditional workflows
var orchestrator = new ConditionalWorkflowOrchestrator
{
    WorkflowDefinitions = new Dictionary<string, ConditionalWorkflowDefinition>
    {
        ["loan_approval"] = new ConditionalWorkflowDefinition
        {
            EntryCondition = "request_type == 'loan'",
            WorkflowGraph = loanApprovalGraph,
            Priority = 1
        },
        ["insurance_quote"] = new ConditionalWorkflowDefinition
        {
            EntryCondition = "request_type == 'insurance'",
            WorkflowGraph = insuranceQuoteGraph,
            Priority = 2
        }
    },
    DefaultWorkflow = defaultProcessingGraph
};

var selectedWorkflow = orchestrator.SelectWorkflow(workflowContext);
```

## Related Examples

* [Chain of Thought](./chain-of-thought.md): Reasoning and decision making
* [Dynamic Routing](./dynamic-routing.md): Advanced routing patterns
* [Multi-Agent](./multi-agent.md): Coordinated decision making
* [State Management](./state-management.md): Graph state and argument handling

## See Also

* [Conditional Nodes Concepts](../concepts/conditional-nodes.md): Understanding conditional routing
* [Graph Concepts](../concepts/graph-concepts.md): Graph-based workflow fundamentals
* [Node Types](../concepts/node-types.md): Graph node fundamentals
* [API Reference](../api/): Complete API documentation
