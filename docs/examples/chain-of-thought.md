# Chain of Thought Example

This example demonstrates Chain-of-Thought reasoning patterns using the Semantic Kernel Graph. It shows different reasoning types, validation, backtracking, and template customization for step-by-step problem solving.

## Objective

Learn how to implement Chain-of-Thought reasoning in graph-based workflows to:
* Break down complex problems into logical steps
* Validate reasoning quality at each step
* Enable backtracking when reasoning fails
* Customize reasoning templates for different use cases
* Monitor and optimize reasoning performance

## Prerequisites

* **.NET 8.0** or later
* **OpenAI API Key** configured in `appsettings.json`
* **Semantic Kernel Graph package** installed
* Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Node Types](../concepts/node-types.md)

## Key Components

### Concepts and Techniques

* **Chain of Thought**: A reasoning pattern where the AI breaks down complex problems into sequential steps, showing its thinking process
* **Reasoning Validation**: Quality assessment of each reasoning step using confidence scoring and validation rules
* **Backtracking**: Ability to retry failed reasoning steps with different approaches
* **Template Engine**: Customizable prompts and reasoning patterns for different problem domains

### Core Classes

* `ChainOfThoughtGraphNode`: Main node for implementing CoT reasoning
* `ChainOfThoughtTemplateEngine`: Manages reasoning templates and validation
* `ChainOfThoughtType`: Enumeration of reasoning strategies (ProblemSolving, Analysis, DecisionMaking)
* `ChainOfThoughtValidator`: Validates reasoning quality and confidence

## Running the Example

### Getting Started

This example demonstrates Chain-of-Thought reasoning patterns with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

### Implementation Overview

The example below shows how to implement Chain-of-Thought reasoning in your own applications:

## Step-by-Step Implementation

### 1. Problem Solving with Chain-of-Thought

This example demonstrates basic problem-solving with step-by-step reasoning.

```csharp
// Create a Chain-of-Thought node configured for problem solving.
// The node supports backtracking, minimum confidence thresholds and optional caching.
var cotNode = new ChainOfThoughtGraphNode(
    ChainOfThoughtType.ProblemSolving,
    maxSteps: 5,
    templateEngine: templateEngine,
    logger: logger)
{
    BacktrackingEnabled = true,
    MinimumStepConfidence = 0.6,
    CachingEnabled = true
};

// Create a graph executor, register the node and set it as the start node.
var executor = new GraphExecutor("ChainOfThought-ProblemSolving", "Chain-of-Thought problem solving example", logger);
executor.AddNode(cotNode);
executor.SetStartNode(cotNode.NodeId);

// Prepare input arguments for the Chain-of-Thought execution.
var arguments = new KernelArguments
{
    ["problem_statement"] = "A company needs to reduce operational costs by 20% while maintaining employee satisfaction. They have 1000 employees and current annual costs of $50M.",
    ["context"] = "Competitive tech market with high talent retention challenges.",
    ["constraints"] = "Cannot reduce headcount by more than 5%; must maintain benefit levels; implement within 6 months.",
    ["expected_outcome"] = "A concrete cost reduction plan with prioritized actions.",
    ["reasoning_depth"] = 4
};

// Execute the graph and obtain the final reasoning result.
var result = await executor.ExecuteAsync(kernel, arguments, CancellationToken.None);
```

### 2. Analysis with Custom Templates

Demonstrates custom reasoning templates and validation rules.

```csharp
// Define a custom analysis template and validation rules for business analysis.
var customTemplates = new Dictionary<string, string>
{
    ["step_1"] = @"You are analyzing a complex situation. This is step {{step_number}}.

Situation: {{problem_statement}}
Context: {{context}}

Start by identifying the key stakeholders and their interests. Who are the main parties involved and what do they care about?

Your analysis:",

    ["analysis_step"] = @"Continue your analysis. This is step {{step_number}} of {{max_steps}}.

Previous analysis:
{{previous_steps}}

Now examine the following aspect: What are the underlying causes and contributing factors? Look deeper than surface-level observations.

Your analysis:"
};

// Create custom validation rules (examples shown later in this document)
var customRules = new List<IChainOfThoughtValidationRule>
{
    new StakeholderAnalysisRule(),
    new CausalAnalysisRule()
};

// Create a Chain-of-Thought node using the custom templates and rules.
var analysisNode = ChainOfThoughtGraphNode.CreateWithCustomization(
    ChainOfThoughtType.Analysis,
    customTemplates,
    customRules,
    maxSteps: 4,
    templateEngine: templateEngine,
    logger: logger);
```

### 3. Decision Making with Backtracking

Shows how to implement backtracking when reasoning fails.

```csharp
// Configure a decision-making node with backtracking enabled.
var decisionNode = new ChainOfThoughtGraphNode(
    ChainOfThoughtType.DecisionMaking,
    maxSteps: 4,
    templateEngine: templateEngine,
    logger: logger)
{
    BacktrackingEnabled = true,
    // Note: MaxBacktrackAttempts and BacktrackStrategy are illustrative
    // and may be implemented at the validator/backtracking layer.
    MinimumStepConfidence = 0.8
};

// The node will attempt backtracking when validation confidence falls
// below the configured threshold (if backtracking support is enabled).
```

### 4. Performance and Cache Demonstration

Optimizes reasoning performance with caching and metrics.

```csharp
// Create a node with caching and optional performance monitoring enabled.
var performanceNode = new ChainOfThoughtGraphNode(
    ChainOfThoughtType.ProblemSolving,
    maxSteps: 5,
    templateEngine: templateEngine,
    logger: logger)
{
    CachingEnabled = true
    // CacheExpiration and performance thresholds are implementation details
    // available on some node implementations. Use them when supported.
};

// Print basic node statistics for quick verification.
Console.WriteLine($"Node Statistics: {cotNode.Statistics.ExecutionCount} executions, " +
                  $"{cotNode.Statistics.AverageQualityScore:P1} avg quality, " +
                  $"{cotNode.Statistics.SuccessRate:P1} success rate");
```

## Expected Output

### Problem Solving Example

```
🧠 Starting problem-solving reasoning...
📝 Step 1: Analyzing cost structure and employee satisfaction factors
✅ Step 1 completed with confidence: 0.85
📝 Step 2: Identifying cost reduction opportunities
✅ Step 2 completed with confidence: 0.78
📝 Step 3: Evaluating impact on employee satisfaction
✅ Step 3 completed with confidence: 0.82
📝 Step 4: Developing implementation plan
✅ Step 4 completed with confidence: 0.79

✅ Final Answer: Comprehensive cost reduction plan including:
* Process optimization (8% savings)
* Technology automation (7% savings)
* Vendor renegotiation (5% savings)
* Total: 20% cost reduction while maintaining satisfaction

📊 Node Statistics: 1 executions, 81.0% avg quality, 100% success rate
```

### Analysis Example

```
🔍 Starting business analysis with custom template...
📋 Using template: BusinessAnalysis
📝 Step 1: Identify the core business problem
✅ Step 1 completed with confidence: 0.88
📝 Step 2: Analyze current state and constraints
✅ Step 2 completed with confidence: 0.85
📝 Step 3: Generate potential solutions
✅ Step 3 completed with confidence: 0.82
📝 Step 4: Evaluate solutions against criteria
✅ Step 4 completed with confidence: 0.86
📝 Step 5: Recommend optimal approach
✅ Step 5 completed with confidence: 0.89

🎯 Analysis Complete: Strategic recommendations with implementation roadmap
```

## Configuration Options

### Chain of Thought Settings

```csharp
// Example configuration object representing Chain-of-Thought settings.
// Not all implementations expose a single options class; many node
// properties are configured directly on the node instance itself.
var cotOptions = new
{
    MaxSteps = 5,
    MinimumStepConfidence = 0.6,
    EnableBacktracking = true,
    MaxBacktrackAttempts = 3,
    CachingEnabled = true,
    CacheExpiration = TimeSpan.FromHours(24),
    EnableStepValidation = true,
    PerformanceThreshold = TimeSpan.FromSeconds(30)
};
```

### Template Configuration

```csharp
var templateOptions = new ChainOfThoughtTemplateOptions
{
    DefaultTemplate = ChainOfThoughtType.ProblemSolving,
    CustomTemplates = new Dictionary<string, ChainOfThoughtTemplate>
    {
        ["BusinessAnalysis"] = businessAnalysisTemplate,
        ["TechnicalReview"] = technicalReviewTemplate,
        ["RiskAssessment"] = riskAssessmentTemplate
    },
    ValidationRules = new[]
    {
        "All steps must be logical and sequential",
        "Each step must build on previous steps",
        "Final answer must address the original problem"
    }
};
```

## Troubleshooting

### Common Issues

#### Low Confidence Scores
```bash
# Problem: Steps consistently fail confidence validation
# Solution: Adjust confidence threshold or improve prompt quality
MinimumStepConfidence = 0.5; // Lower threshold for development
```

#### Excessive Backtracking
```bash
# Problem: Too many backtrack attempts
# Solution: Limit backtrack attempts or improve initial reasoning
MaxBacktrackAttempts = 2; // Reduce retry attempts
```

#### Performance Issues
```bash
# Problem: Slow reasoning execution
# Solution: Enable caching and set performance thresholds
CachingEnabled = true;
PerformanceThreshold = TimeSpan.FromSeconds(60);
```

### Debug Mode

Enable detailed logging for troubleshooting:

```csharp
// Create a console logger for debugging and configure a node to emit
// detailed reasoning logs (when supported by the node implementation).
var logger = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
}).CreateLogger<ChainOfThoughtGraphNode>();

var debugNode = new ChainOfThoughtGraphNode(
    ChainOfThoughtType.ProblemSolving,
    maxSteps: 5,
    templateEngine: templateEngine,
    logger: logger)
{
    // These flags are examples; actual property names may vary by version.
    // Enable detailed logging inside the node implementation where present.
};
```

## Advanced Patterns

### Multi-Step Validation

```csharp
// Implement custom validation logic
var customValidator = new ChainOfThoughtValidator
{
    StepValidators = new[]
    {
        new StepValidator("LogicalFlow", step => ValidateLogicalFlow(step)),
        new StepValidator("DataReference", step => ValidateDataReference(step)),
        new StepValidator("Actionability", step => ValidateActionability(step))
    }
};

cotNode.Validator = customValidator;
```

### Dynamic Template Selection

```csharp
// Select template based on problem type
var templateSelector = new TemplateSelector
{
    Selector = (context) =>
    {
        var problemType = context.GetValue<string>("problem_type");
        return problemType switch
        {
            "business" => "BusinessAnalysis",
            "technical" => "TechnicalReview",
            "risk" => "RiskAssessment",
            _ => "ProblemSolving"
        };
    }
};

cotNode.TemplateSelector = templateSelector;
```

## Related Examples

* [ReAct Agent](./react-agent.md): Reasoning and action loops
* [ReAct Problem Solving](./react-problem-solving.md): Complex problem decomposition
* [Conditional Nodes](./conditional-nodes.md): Dynamic routing based on reasoning results
* [Graph Metrics](./graph-metrics.md): Performance monitoring and optimization

## See Also

* [Chain of Thought Concepts](../concepts/chain-of-thought.md): Understanding reasoning patterns
* [Node Types](../concepts/node-types.md): Graph node fundamentals
* [Performance Monitoring](../how-to/metrics-and-observability.md): Metrics and optimization
* [API Reference](../api/): Complete API documentation
