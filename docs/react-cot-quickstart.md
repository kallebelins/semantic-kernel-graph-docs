# ReAct and Chain of Thought Quickstart

Learn how to implement reasoning and acting patterns in SemanticKernel.Graph using ReAct (Reasoning + Acting) loops and Chain of Thought reasoning. This guide shows you how to build intelligent agents that can think, act, and learn from their actions.

## Concepts and Techniques

**ReAct Pattern**: A reasoning loop where the agent analyzes the current situation (Reasoning), executes actions (Acting), and observes results (Observation) in iterative cycles until achieving its goal.

**Chain of Thought**: A structured reasoning approach that breaks down complex problems into sequential, validated steps with backtracking capabilities for robust problem-solving.

**Reasoning Nodes**: Specialized nodes like `ReasoningGraphNode` and `ChainOfThoughtGraphNode` that implement different reasoning strategies and can be composed into complex reasoning workflows.

## Prerequisites and Minimum Configuration

- .NET 6.0 or later
- SemanticKernel.Graph package installed
- Semantic Kernel with chat completion capabilities
- Basic understanding of graph execution and node composition

## Quick Setup

### 1. Create a Simple ReAct Loop

Build a basic reasoning loop with three core components:

```csharp
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Nodes;

// Create the reasoning node
var reasoningNode = new FunctionGraphNode(
    CreateReasoningFunction(kernel),
    "reasoning",
    "Problem Analysis");

// Create the action execution node
var actionNode = new ActionGraphNode(
    kernel,
    "action",
    "Action Execution");

// Create the observation node
var observationNode = new FunctionGraphNode(
    CreateObservationFunction(kernel),
    "observation",
    "Result Analysis");

// Build the ReAct loop
var executor = new GraphExecutor("SimpleReAct", "Basic ReAct reasoning loop");
executor.AddNode(reasoningNode)
        .AddNode(actionNode)
        .AddNode(observationNode)
        .Connect("reasoning", "action")
        .Connect("action", "observation")
        .SetStartNode("reasoning");
```

### 2. Implement the Core Functions

Create the reasoning, action, and observation functions:

```csharp
private static KernelFunction CreateReasoningFunction(Kernel kernel)
{
    return kernel.CreateFunctionFromMethod(
        (KernelArguments args) =>
        {
            var query = args["user_query"]?.ToString() ?? string.Empty;
            
            // Simple reasoning logic
            var suggestedAction = query.ToLowerInvariant() switch
            {
                var q when q.Contains("weather") => "get_weather",
                var q when q.Contains("calculate") => "calculate",
                var q when q.Contains("search") => "search",
                _ => "search"
            };

            args["suggested_action"] = suggestedAction;
            args["reasoning_result"] = $"Selected action '{suggestedAction}' based on query analysis.";
            
            return $"Reasoning completed. Proposed action: {suggestedAction}";
        },
        functionName: "simple_reasoning",
        description: "Analyzes user query and suggests appropriate actions"
    );
}

private static KernelFunction CreateObservationFunction(Kernel kernel)
{
    return kernel.CreateFunctionFromMethod(
        (KernelArguments args) =>
        {
            var actionResult = args["action_result"]?.ToString() ?? "No result";
            var reasoningResult = args["reasoning_result"]?.ToString() ?? "No reasoning";
            
            var observation = $"Based on reasoning: {reasoningResult}\n" +
                            $"Action executed with result: {actionResult}\n" +
                            $"Task completed successfully.";
            
            args["final_answer"] = observation;
            return observation;
        },
        functionName: "simple_observation",
        description: "Analyzes action results and provides final answer"
    );
}
```

### 3. Execute the ReAct Loop

Run your reasoning agent:

```csharp
var arguments = new KernelArguments
{
    ["user_query"] = "What's the weather like today?",
    ["max_steps"] = 3
};

var result = await executor.ExecuteAsync(kernel, arguments);
var answer = result.GetValue<string>() ?? "No answer produced";
Console.WriteLine($"🤖 Agent: {answer}");
```

## Advanced ReAct with ReActLoopGraphNode

### Using the Built-in ReAct Loop Node

Leverage the specialized `ReActLoopGraphNode` for more sophisticated reasoning:

```csharp
using SemanticKernel.Graph.Nodes;

// Create specialized reasoning, action, and observation nodes
var reasoningNode = new ReasoningGraphNode(
    "reasoning",
    "Problem Analysis",
    maxReasoningSteps: 3,
    domain: "general");

var actionNode = new ActionGraphNode(
    kernel,
    "action",
    "Action Execution");

var observationNode = new FunctionGraphNode(
    CreateAdvancedObservationFunction(kernel),
    "observation",
    "Result Analysis");

// Create the ReAct loop node
var reactLoopNode = new ReActLoopGraphNode(
    reasoningNode,
    actionNode,
    observationNode,
    maxIterations: 5,
    goalAchievementThreshold: 0.8)
{
    EarlyTerminationEnabled = true,
    IterationTimeout = TimeSpan.FromSeconds(30),
    TotalTimeout = TimeSpan.FromMinutes(5)
};

// Build the executor
var executor = new GraphExecutor("AdvancedReAct", "Advanced ReAct reasoning agent");
executor.AddNode(reactLoopNode);
executor.SetStartNode(reactLoopNode.NodeId);
```

### Configure ReAct Loop Behavior

Customize the reasoning loop parameters:

```csharp
var reactLoopNode = new ReActLoopGraphNode(
    reasoningNode,
    actionNode,
    observationNode,
    maxIterations: 10,
    goalAchievementThreshold: 0.9)
{
    // Enable early termination when goal is achieved
    EarlyTerminationEnabled = true,
    
    // Set timeouts for iterations and total execution
    IterationTimeout = TimeSpan.FromSeconds(60),
    TotalTimeout = TimeSpan.FromMinutes(10),
    
    // Configure goal evaluation
    GoalEvaluationFunction = (context, result) =>
    {
        var confidence = result.GetValueOrDefault("confidence", 0.0);
        var completeness = result.GetValueOrDefault("completeness", 0.0);
        return (confidence + completeness) / 2.0;
    }
};
```

## Chain of Thought Reasoning

### Basic Chain of Thought

Implement step-by-step reasoning with the `ChainOfThoughtGraphNode`:

```csharp
using SemanticKernel.Graph.Nodes;

// Create a Chain of Thought node for problem solving
var cotNode = new ChainOfThoughtGraphNode(
    ChainOfThoughtType.ProblemSolving,
    maxSteps: 5,
    templateEngine: null,  // Use default templates
    logger: null)
{
    BacktrackingEnabled = true,
    MinimumStepConfidence = 0.6,
    CachingEnabled = true
};

// Build the executor
var executor = new GraphExecutor("ChainOfThought", "Chain-of-Thought reasoning example");
executor.AddNode(cotNode);
executor.SetStartNode(cotNode.NodeId);

// Prepare arguments for reasoning
var arguments = new KernelArguments
{
    ["problem_statement"] = "A company needs to reduce operational costs by 20% while maintaining employee satisfaction.",
    ["context"] = "The company operates in a competitive tech market with high talent retention challenges.",
    ["constraints"] = "Cannot reduce headcount by more than 5%, must maintain current benefit levels.",
    ["expected_outcome"] = "A comprehensive cost reduction plan with specific actionable steps",
    ["reasoning_depth"] = 4
};

var result = await executor.ExecuteAsync(kernel, arguments);
var finalAnswer = result.GetValue<string>() ?? "(no result)";
Console.WriteLine($"🧠 Final Answer: {finalAnswer}");
```

### Custom Chain of Thought Templates

Create specialized reasoning templates for different domains:

```csharp
// Create custom templates for analysis
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

// Create the Chain of Thought node with custom templates
var cotNode = ChainOfThoughtGraphNode.CreateWithCustomization(
    ChainOfThoughtType.Analysis,
    customTemplates,
    customRules: null,  // Use default validation rules
    maxSteps: 4,
    templateEngine: null,
    logger: null);
```

## Problem-Solving Examples

### Business Problem Analysis

Solve complex business problems using ReAct:

```csharp
var arguments = new KernelArguments
{
    ["problem_title"] = "Budget Planning",
    ["task_description"] = "Our team needs to reduce operational costs by 20% while maintaining service quality. Current monthly spending is $50,000 across 5 departments.",
    ["max_iterations"] = 3,
    ["solver_mode"] = "systematic",
    ["domain"] = "business"
};

var result = await problemSolver.ExecuteAsync(kernel, arguments);
var solution = result.GetValue<string>() ?? "No solution generated";
Console.WriteLine($"💡 ReAct Solution: {solution}");
```

### Technical Problem Solving

Apply ReAct to technical challenges:

```csharp
var arguments = new KernelArguments
{
    ["problem_title"] = "System Performance",
    ["task_description"] = "Our web application is experiencing slow response times (>3 seconds) during peak hours. The database queries seem to be the bottleneck.",
    ["max_iterations"] = 4,
    ["solver_mode"] = "technical",
    ["domain"] = "software"
};

var result = await problemSolver.ExecuteAsync(kernel, arguments);
var solution = result.GetValue<string>() ?? "No solution generated";
Console.WriteLine($"💻 Technical Solution: {solution}");
```

## Monitoring and Debugging

### Track Reasoning Performance

Monitor your reasoning agents:

```csharp
// Get execution statistics
var executionCount = reactLoopNode.Statistics.ExecutionCount;
var successRate = reactLoopNode.Statistics.SuccessRate;
var averageIterations = reactLoopNode.Statistics.AverageIterationsPerExecution;

Console.WriteLine($"ReAct Loop Statistics:");
Console.WriteLine($"  Executions: {executionCount}");
Console.WriteLine($"  Success Rate: {successRate:P1}");
Console.WriteLine($"  Avg Iterations: {averageIterations:F1}");

// Chain of Thought statistics
var cotStats = cotNode.Statistics;
Console.WriteLine($"Chain of Thought Statistics:");
Console.WriteLine($"  Executions: {cotStats.ExecutionCount}");
Console.WriteLine($"  Quality Score: {cotStats.AverageQualityScore:P1}");
Console.WriteLine($"  Steps Used: {cotStats.AverageStepsUsed:F1}");
```

### Debug Reasoning Steps

Inspect the reasoning process:

```csharp
// Get detailed execution metadata
var metadata = result.Metadata;
if (metadata.ContainsKey("reasoning_steps"))
{
    var steps = metadata["reasoning_steps"] as List<object>;
    Console.WriteLine("Reasoning Steps:");
    foreach (var step in steps ?? new List<object>())
    {
        Console.WriteLine($"  - {step}");
    }
}

if (metadata.ContainsKey("iterations"))
{
    var iterations = metadata["iterations"] as List<object>;
    Console.WriteLine($"Total Iterations: {iterations?.Count ?? 0}");
}
```

## Troubleshooting

### Common Issues

**ReAct loop gets stuck**: Check your goal evaluation function and ensure it can properly detect when the goal is achieved.

**Chain of Thought validation fails**: Adjust the `MinimumStepConfidence` threshold or improve your validation rules.

**Reasoning quality is poor**: Review your reasoning templates and ensure they provide clear guidance for each step.

**Actions not executing**: Verify that your action nodes have access to the required kernel functions and parameters.

### Performance Recommendations

- Use appropriate iteration limits based on problem complexity
- Enable caching for Chain of Thought to avoid redundant reasoning
- Set reasonable timeouts to prevent infinite loops
- Monitor reasoning quality scores and adjust confidence thresholds
- Use early termination when possible to improve efficiency

## See Also

- **Reference**: [ReActLoopGraphNode](../api/ReActLoopGraphNode.md), [ChainOfThoughtGraphNode](../api/ChainOfThoughtGraphNode.md), [ReasoningGraphNode](../api/ReasoningGraphNode.md)
- **Guides**: [Advanced Reasoning Patterns](../guides/advanced-reasoning.md), [Agent Architecture](../guides/agent-architecture.md)
- **Examples**: [ReActAgentExample](../examples/react-agent.md), [ChainOfThoughtExample](../examples/chain-of-thought.md), [ReActProblemSolvingExample](../examples/react-problem-solving.md)

## Reference APIs

- **[ReActLoopGraphNode](../api/nodes.md#react-loop-graph-node)**: ReAct reasoning loop implementation
- **[ChainOfThoughtGraphNode](../api/nodes.md#chain-of-thought-graph-node)**: Chain of Thought reasoning node
- **[ReasoningGraphNode](../api/nodes.md#reasoning-graph-node)**: Base reasoning node interface
- **[ActionGraphNode](../api/nodes.md#action-graph-node)**: Action execution node
