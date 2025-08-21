# ReAct and Chain of Thought Quickstart

Learn how to implement reasoning and acting patterns in SemanticKernel.Graph using ReAct (Reasoning + Acting) loops and Chain of Thought reasoning. This guide shows you how to build intelligent agents that can think, act, and learn from their actions.

## Concepts and Techniques

**ReAct Pattern**: A reasoning loop where the agent analyzes the current situation (Reasoning), executes actions (Acting), and observes results (Observation) in iterative cycles until achieving its goal.

**Chain of Thought**: A structured reasoning approach that breaks down complex problems into sequential, validated steps with backtracking capabilities for robust problem-solving.

**Reasoning Nodes**: Specialized nodes like `FunctionGraphNode` (for custom reasoning logic), `ReActLoopGraphNode` (for built-in ReAct loops), and `ChainOfThoughtGraphNode` that implement different reasoning strategies and can be composed into complex reasoning workflows.

## Prerequisites and Minimum Configuration

* .NET 8.0 or later
* SemanticKernel.Graph package installed
* Semantic Kernel with chat completion capabilities
* Basic understanding of graph execution and node composition

## Quick Setup

**Important**: Before creating any ReAct loops, ensure that mock actions are registered with the kernel. This is required for `ActionGraphNode` to discover and execute functions.

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

// Create the action execution node using CreateWithActions to auto-discover kernel functions
var actionNode = ActionGraphNode.CreateWithActions(
    kernel,
    new ActionSelectionCriteria
    {
        MinRequiredParameters = 0,
        MaxRequiredParameters = 5
    },
    "action");
actionNode.ConfigureExecution(ActionSelectionStrategy.Intelligent, enableParameterValidation: true);

// Create the observation node
var observationNode = new FunctionGraphNode(
    CreateObservationFunction(kernel),
    "observation",
    "Result Analysis");

// Configure nodes to store results for downstream consumption
reasoningNode.StoreResultAs("reasoning_result");
// ActionGraphNode automatically stores result as "action_result"
observationNode.StoreResultAs("final_answer");

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

Create the reasoning, action, and observation functions. First, ensure mock actions are registered with the kernel:

```csharp
private void AddMockActionsToKernel()
{
    // Check if plugin already exists to avoid duplicates
    if (kernel.Plugins.Any(p => p.Name == "react_actions"))
    {
        return;
    }

    // Import all functions as a plugin so ActionGraphNode can discover them
    kernel.ImportPluginFromFunctions("react_actions", "Mock actions for ReAct examples", new[]
    {
        // Weather action
        kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var location = args.GetValueOrDefault("location", "unknown location");
                return $"The weather in {location} is sunny with 22°C temperature and light breeze.";
            },
            functionName: "get_weather",
            description: "Gets weather information for a specified location"
        ),
        // Calculator action
        kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var expression = args.GetValueOrDefault("expression", "0");
                return $"Calculation result for '{expression}': 42 (mock result)";
            },
            functionName: "calculate",
            description: "Performs mathematical calculations"
        ),
        // Search action
        kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var query = args.GetValueOrDefault("query", "unknown query");
                return $"Search results for '{query}': Found 5 relevant articles about the topic.";
            },
            functionName: "search",
            description: "Searches for information on the internet"
        ),
        // Generic action for business problems
        kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var problem = args.GetValueOrDefault("problem", "unknown problem");
                return $"Analysis of '{problem}': Identified 3 key areas for improvement with cost reduction potential.";
            },
            functionName: "analyze_problem",
            description: "Analyzes business problems and provides insights"
        ),
        // Solution evaluation action
        kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var solution = args.GetValueOrDefault("solution", "unknown solution");
                return $"Evaluation of '{solution}': Feasible solution with 85% success probability and moderate implementation complexity.";
            },
            functionName: "evaluate_solution",
            description: "Evaluates proposed solutions for feasibility and impact"
        )
    });
}
```

Now create the reasoning, action, and observation functions:

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

## Advanced ReAct with Custom Functions

### Using FunctionGraphNode for Advanced Reasoning

For self-contained examples that don't require external LLM calls, you can implement advanced reasoning using custom functions:

Here are the implementations for the advanced reasoning and observation functions:

```csharp
using SemanticKernel.Graph.Nodes;

// Create specialized reasoning, action, and observation nodes
// Use FunctionGraphNode with custom functions to avoid LLM dependency
var reasoningNode = new FunctionGraphNode(
    CreateAdvancedReasoningFunction(kernel),
    "advanced_reasoning",
    "Advanced Problem Analysis");

var actionNode = ActionGraphNode.CreateWithActions(
    kernel,
    new ActionSelectionCriteria
    {
        MinRequiredParameters = 0,
        MaxRequiredParameters = 5
    },
    "advanced_action");
actionNode.ConfigureExecution(ActionSelectionStrategy.Intelligent, enableParameterValidation: true);

var observationNode = new FunctionGraphNode(
    CreateAdvancedObservationFunction(kernel),
    "advanced_observation",
    "Advanced Result Analysis");

// Configure nodes to store results for downstream consumption
reasoningNode.StoreResultAs("advanced_reasoning_result");
observationNode.StoreResultAs("advanced_final_answer");

// Build the advanced ReAct loop by connecting the nodes in sequence
var executor = new GraphExecutor("AdvancedReAct", "Advanced ReAct reasoning agent");
executor.AddNode(reasoningNode)
        .AddNode(actionNode)
        .AddNode(observationNode)
        .Connect("advanced_reasoning", "advanced_action")
        .Connect("advanced_action", "advanced_observation")
        .SetStartNode("advanced_reasoning");
```

### Advanced Function Implementations

Here are the implementations for the advanced reasoning and observation functions:

```csharp
private static KernelFunction CreateAdvancedReasoningFunction(Kernel kernel)
{
    return kernel.CreateFunctionFromMethod(
        (KernelArguments args) =>
        {
            var problemTitle = args.GetValueOrDefault("problem_title", "Unknown Problem")?.ToString() ?? "Unknown Problem";
            var taskDescription = args.GetValueOrDefault("task_description", "No description provided")?.ToString() ?? "No description provided";
            
            // Advanced reasoning logic for business problems
            var suggestedAction = taskDescription.ToLowerInvariant() switch
            {
                var desc when desc.Contains("cost") && desc.Contains("reduce") => "analyze_problem",
                var desc when desc.Contains("budget") => "analyze_problem", 
                var desc when desc.Contains("performance") => "analyze_problem",
                var desc when desc.Contains("efficiency") => "analyze_problem",
                _ => "analyze_problem"
            };

            // Generate comprehensive reasoning result
            var reasoning = $"Advanced Analysis of '{problemTitle}':\n" +
                          $"1. Problem Context: {taskDescription}\n" +
                          $"2. Strategic Assessment: This appears to be a {(taskDescription.Contains("cost") ? "cost optimization" : "operational improvement")} challenge.\n" +
                          $"3. Recommended Approach: Systematic analysis with stakeholder consideration.\n" +
                          $"4. Next Action: {suggestedAction} - Deep dive into root causes and impact analysis.";

            // Store the reasoning results in the arguments for later use
            args["suggested_action"] = suggestedAction;
            args["reasoning_result"] = reasoning;
            args["problem_title"] = problemTitle;
            args["task_description"] = taskDescription;
            
            return reasoning;
        },
        functionName: "advanced_reasoning",
        description: "Performs advanced business problem analysis and strategic reasoning"
    );
}

private static KernelFunction CreateAdvancedObservationFunction(Kernel kernel)
{
    return kernel.CreateFunctionFromMethod(
        (KernelArguments args) =>
        {
            var actionResult = args.GetValueOrDefault("action_result", "No action result")?.ToString() ?? "No action result";
            var reasoningResult = args.GetValueOrDefault("advanced_reasoning_result", "No reasoning result")?.ToString() ?? "No reasoning result";
            
            // Advanced observation analysis
            var observation = $"Advanced Result Analysis:\n" +
                            $"1. Previous Reasoning: {reasoningResult}\n" +
                            $"2. Action Execution: {actionResult}\n" +
                            $"3. Strategic Insights: The analysis reveals key improvement opportunities.\n" +
                            $"4. Recommendations: Implement systematic changes with stakeholder buy-in.\n" +
                            $"5. Next Steps: Evaluate solution feasibility and create implementation timeline.";

            args["advanced_final_answer"] = observation;
            return observation;
        },
        functionName: "advanced_observation",
        description: "Performs advanced result analysis and provides strategic recommendations"
    );
}
```

### Configure ReAct Loop Behavior

For advanced ReAct loops using `ReActLoopGraphNode`, you can customize the reasoning loop parameters:

```csharp
// Note: This example shows the ReActLoopGraphNode API structure
// For self-contained examples, consider using the sequential approach shown above
var reactLoopNode = ReActLoopGraphNode.CreateWithNodes(
    reasoningNode,
    actionNode,
    observationNode,
    "custom_react_loop")
    .ConfigureLoop(
        maxIterations: 10,
        goalAchievementThreshold: 0.9,
        enableEarlyTermination: true,
        iterationTimeout: TimeSpan.FromSeconds(60),
        totalTimeout: TimeSpan.FromMinutes(10));
```

## Chain of Thought Reasoning

### Basic Chain of Thought

Implement step-by-step reasoning with the `ChainOfThoughtGraphNode`:

```csharp
using SemanticKernel.Graph.Nodes;

// Create a Chain of Thought function that performs step-by-step reasoning
var cotNode = new FunctionGraphNode(
    CreateChainOfThoughtFunction(kernel),
    "chain_of_thought",
    "Step-by-step Problem Solving");

// Configure to store the result
cotNode.StoreResultAs("chain_of_thought_result");

// Build the executor
var executor = new GraphExecutor("ChainOfThought", "Chain-of-Thought reasoning example");
executor.AddNode(cotNode);
executor.SetStartNode(cotNode.NodeId);
```

### Chain of Thought Function Implementation

Here's the implementation for the Chain of Thought function:

```csharp
private static KernelFunction CreateChainOfThoughtFunction(Kernel kernel)
{
    return kernel.CreateFunctionFromMethod(
        (KernelArguments args) =>
        {
            var problemStatement = args.GetValueOrDefault("problem_statement", "No problem statement")?.ToString() ?? "No problem statement";
            var context = args.GetValueOrDefault("context", "No context provided")?.ToString() ?? "No context provided";
            var constraints = args.GetValueOrDefault("constraints", "No constraints specified")?.ToString() ?? "No constraints specified";
            var expectedOutcome = args.GetValueOrDefault("expected_outcome", "No expected outcome")?.ToString() ?? "No expected outcome";
            var reasoningDepth = args.GetValueOrDefault("reasoning_depth", 3);

            // Simulate step-by-step reasoning process
            var reasoningSteps = new List<string>
            {
                $"Step 1: Problem Analysis\nAnalyzing the problem: {problemStatement}\nContext: {context}",
                $"Step 2: Constraint Identification\nConstraints identified: {constraints}",
                $"Step 3: Solution Development\nExpected outcome: {expectedOutcome}\nDeveloping solution approach...",
                $"Step 4: Solution Validation\nValidating solution against constraints and requirements...",
                $"Step 5: Final Recommendation\nProviding comprehensive solution with implementation steps."
            };

            // Take only the requested number of steps
            var stepsToUse = reasoningSteps.Take(Math.Min((int)reasoningDepth, reasoningSteps.Count)).ToList();
            
            // Generate final answer
            var finalAnswer = $"Chain of Thought Analysis Complete:\n\n" +
                             $"Problem: {problemStatement}\n" +
                             $"Context: {context}\n" +
                             $"Constraints: {constraints}\n" +
                             $"Expected Outcome: {expectedOutcome}\n\n" +
                             $"Reasoning Steps ({stepsToUse.Count}):\n" +
                             string.Join("\n\n", stepsToUse) +
                             $"\n\nFinal Recommendation: Implement a systematic approach focusing on stakeholder engagement, " +
                             $"data-driven analysis, and phased implementation to achieve the desired {expectedOutcome}.";

            // Store the reasoning results in the arguments
            args["reasoning_steps"] = stepsToUse;
            args["final_answer"] = finalAnswer;
            args["problem_statement"] = problemStatement;
            args["context"] = context;
            args["constraints"] = constraints;
            args["expected_outcome"] = expectedOutcome;
            args["reasoning_depth"] = reasoningDepth;
            
            return finalAnswer;
        },
        functionName: "chain_of_thought",
        description: "Performs step-by-step reasoning for complex problem solving"
    );
}

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
// Note: This example shows the API structure, but for self-contained examples,
// consider using FunctionGraphNode with custom reasoning logic
var cotNode = ChainOfThoughtGraphNode.CreateWithCustomization(
    ChainOfThoughtType.Analysis,
    customTemplates,
    customRules: null,  // Use default validation rules
    maxSteps: 4);
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

var result = await executor.ExecuteAsync(kernel, arguments);
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

var result = await executor.ExecuteAsync(kernel, arguments);
var solution = result.GetValue<string>() ?? "No solution generated";
Console.WriteLine($"💻 Technical Solution: {solution}");
```

## Monitoring and Debugging

### Track Reasoning Performance

Monitor your reasoning agents:

```csharp
// Get execution statistics
// Note: For FunctionGraphNode-based implementations, you can track execution
// through custom metadata or by implementing your own statistics tracking
var executionCount = 1; // Example value
var successRate = 1.0; // Example value
var averageIterations = 1.0; // Example value

Console.WriteLine($"ReAct Loop Statistics:");
Console.WriteLine($"  Executions: {executionCount}");
Console.WriteLine($"  Success Rate: {successRate:P1}");
Console.WriteLine($"  Avg Iterations: {averageIterations:F1}");

// Chain of Thought statistics
// Note: For FunctionGraphNode-based implementations, you can track execution
// through custom metadata or by implementing your own statistics tracking
var cotStats = new { ExecutionCount = 1, AverageQualityScore = 0.85, AverageStepsUsed = 3.0 }; // Example values
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

* Use appropriate iteration limits based on problem complexity
* Enable caching for Chain of Thought to avoid redundant reasoning
* Set reasonable timeouts to prevent infinite loops
* Monitor reasoning quality scores and adjust confidence thresholds
* Use early termination when possible to improve efficiency

## See Also

* **Reference**: [FunctionGraphNode](../api/FunctionGraphNode.md), [ActionGraphNode](../api/ActionGraphNode.md), [ReActLoopGraphNode](../api/ReActLoopGraphNode.md), [ChainOfThoughtGraphNode](../api/ChainOfThoughtGraphNode.md)
* **Guides**: [Advanced Reasoning Patterns](../guides/advanced-reasoning.md), [Agent Architecture](../guides/agent-architecture.md)
* **Examples**: [ReActAgentExample](../examples/react-agent.md), [ChainOfThoughtExample](../examples/chain-of-thought.md), [ReActProblemSolvingExample](../examples/react-problem-solving.md)

## Reference APIs

* **[FunctionGraphNode](../api/nodes.md#function-graph-node)**: Function execution node for custom logic
* **[ActionGraphNode](../api/nodes.md#action-graph-node)**: Action execution node with function discovery
* **[ReActLoopGraphNode](../api/nodes.md#react-loop-graph-node)**: ReAct reasoning loop implementation
* **[ChainOfThoughtGraphNode](../api/nodes.md#chain-of-thought-graph-node)**: Chain of Thought reasoning node
