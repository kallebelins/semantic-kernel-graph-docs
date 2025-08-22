# Main Node Types

This document covers the five main node types in SemanticKernel.Graph that provide the foundation for building complex workflows: `FunctionGraphNode`, `ConditionalGraphNode`, `ReasoningGraphNode`, `ReActLoopGraphNode`, and `ObservationGraphNode`.

## FunctionGraphNode

The `FunctionGraphNode` encapsulates a Semantic Kernel function and provides graph-specific functionality around existing `KernelFunction` instances.

### Overview

This node forwards execution to an underlying `KernelFunction` and augments it with graph-aware behavior including navigation, metadata hooks, and lifecycle management.

### Key Features

* **Function Encapsulation**: Wraps any `KernelFunction` from Semantic Kernel
* **Navigation Support**: Connect unconditional successors and conditional transitions
* **Metadata Hooks**: Custom setup, cleanup, and error handling logic
* **Thread Safety**: Connection lists are guarded by private locks for mutation
* **Result Storage**: Automatic storage of execution results in graph state

### Metadata Hooks

The following metadata keys enable custom behavior:

* **`"StoreResultAs"`** (string): Stores the last result into `GraphState` under the given key
* **`"BeforeExecute"`** (Action/Func): Custom setup logic executed in `OnBeforeExecuteAsync`
* **`"AfterExecute"`** (Action/Func): Custom cleanup logic executed in `OnAfterExecuteAsync`
* **`"OnExecutionFailed"`** (Action/Func): Error handling hook executed in `OnExecutionFailedAsync`
* **`"StrictValidation"`** (bool): If true, pre-execution validation failures cause an exception

### Usage Examples

#### Basic Function Wrapping

```csharp
// Create a lightweight kernel function that transforms an input string.
var function = kernel.CreateFunctionFromMethod(
    (string input) => $"Processed: {input}",
    functionName: "process_input"
);

// Wrap the kernel function in a FunctionGraphNode and persist the result in state.
var node = new FunctionGraphNode(function, "process_node")
    .StoreResultAs("processed_result");

// You can connect nodes directly or add them to a GraphExecutor and use graph-level connect
// Example: connect to another node instance
node.ConnectTo(nextNode);
```

#### From Plugin Function

```csharp
// Create from plugin function
var node = FunctionGraphNode.FromPlugin(kernel, "math", "add")
    .StoreResultAs("sum");

// Add conditional edge
node.AddConditionalEdge(new ConditionalEdge(
    condition: state => state.GetValue<int>("sum") > 10,
    targetNode: highValueNode
));
```

#### With Custom Metadata Hooks

```csharp
var node = new FunctionGraphNode(function, "custom_node")
    .StoreResultAs("result");

// Add custom lifecycle hooks
node.SetMetadata("BeforeExecute", (Action<KernelArguments>)(args => {
    args["execution_start"] = DateTime.UtcNow;
}));

node.SetMetadata("AfterExecute", (Action<KernelArguments, FunctionResult>)((args, result) => {
    args["execution_duration"] = DateTime.UtcNow - (DateTime)args["execution_start"];
}));
```

## ConditionalGraphNode

The `ConditionalGraphNode` implements conditional if/else logic based on graph state, evaluating conditions and routing execution to different paths without executing functions.

### Overview

This node provides function-based and template-based conditions with advanced caching, debugging support, and comprehensive metrics tracking.

### Key Features

* **Function-based Conditions**: Direct evaluation using `Func<GraphState, bool>`
* **Template-based Conditions**: Handlebars-like templates with variable substitution
* **Advanced Caching**: Automatic cache of evaluation results for performance
* **Debugging Support**: Integration with `ConditionalDebugger` for step-by-step analysis
* **Metrics Tracking**: Comprehensive execution metrics and performance monitoring
* **Thread Safety**: All operations are thread-safe for concurrent execution

### Condition Types

#### Function-based Conditions

```csharp
// Simple boolean condition
var conditionNode = new ConditionalGraphNode(
    condition: state => state.GetValue<int>("score") > 80,
    name: "High Score Check"
);

conditionNode.AddTrueNode(successNode);
conditionNode.AddFalseNode(retryNode);
```

#### Template-based Conditions

```csharp
// Handlebars-like template
var templateNode = new ConditionalGraphNode(
    conditionTemplate: "{{ gt score 0.8 }}",
    name: "Score Threshold Check"
);

templateNode.AddTrueNode(successNode);
templateNode.AddFalseNode(retryNode);
```

### Advanced Usage

#### Complex Conditions

```csharp
var complexCondition = new ConditionalGraphNode(
    condition: state => {
        var score = state.GetValue<int>("score");
        var attempts = state.GetValue<int>("attempts");
        var maxAttempts = state.GetValue<int>("max_attempts");
        
        return score >= 90 || attempts >= maxAttempts;
    },
    name: "Complex Decision Logic"
);
```

#### Multiple True/False Paths

```csharp
var decisionNode = new ConditionalGraphNode(
    condition: state => state.GetValue<string>("status") == "completed",
    name: "Status Check"
);

// Multiple true paths
decisionNode.AddTrueNode(successNode);
decisionNode.AddTrueNode(notificationNode);

// Multiple false paths
decisionNode.AddFalseNode(retryNode);
decisionNode.AddFalseNode(errorHandlerNode);
```

## ReasoningGraphNode

The `ReasoningGraphNode` implements reasoning capabilities for analyzing current situations and planning next actions, designed to be used as part of the ReAct pattern for structured decision-making.

### Overview

This node analyzes current context, evaluates available information, and generates structured reasoning about what action should be taken next using configurable prompt templates.

### Key Features

* **Context-aware Reasoning**: Analyzes current situation and available data
* **Template-based Prompts**: Uses customizable templates for different reasoning patterns
* **Quality Metrics**: Tracks reasoning quality and consistency
* **Domain Specialization**: Can be configured for specific problem domains
* **Chain-of-thought Support**: Supports step-by-step reasoning patterns

### Domain Specialization

The node supports predefined domains with specialized prompts:

```csharp
// Create for specific domain
var mathReasoningNode = ReasoningGraphNode.CreateForDomain(
    ReasoningDomain.Mathematics,
    nodeId: "math_reasoning"
);

// Create for general reasoning
var generalReasoningNode = new ReasoningGraphNode(
    reasoningPrompt: "Analyze the current situation and determine the next logical step.",
    name: "General Reasoning"
);
```

### Configuration Options

#### Metadata Configuration

```csharp
var reasoningNode = new ReasoningGraphNode(
    "Analyze the problem and plan the solution step by step.",
    name: "Problem Analysis"
);

// Configure reasoning behavior
reasoningNode.SetMetadata("ChainOfThoughtEnabled", true);
reasoningNode.SetMetadata("MaxReasoningSteps", 5);
reasoningNode.SetMetadata("ConfidenceThreshold", 0.8);
reasoningNode.SetMetadata("Domain", "problem_solving");
```

#### Template Engine Integration

```csharp
var templateEngine = new GraphTemplateEngine();
var reasoningNode = new ReasoningGraphNode(
    "Current context: {{context}}\nProblem: {{problem}}\nPlan the next action:",
    templateEngine: templateEngine
);
```

### Usage in ReAct Patterns

```csharp
// Create reasoning node for ReAct loop
var reasoningNode = new ReasoningGraphNode(
    "Based on the current situation: {{situation}}\n" +
    "Available actions: {{available_actions}}\n" +
    "What should be the next action?",
    name: "ReAct Reasoning"
);

// Configure for iterative reasoning
reasoningNode.SetMetadata("MaxReasoningSteps", 3);
reasoningNode.SetMetadata("ConfidenceThreshold", 0.7);
```

## ReActLoopGraphNode

The `ReActLoopGraphNode` orchestrates the complete ReAct (Reasoning + Acting) pattern loop, coordinating reasoning, action execution, and observation in iterative cycles until goal achievement.

### Overview

This node implements the full ReAct pattern by orchestrating reasoning, acting, observation, and loop control in a coordinated manner with configurable limits and sophisticated goal evaluation.

### Key Features

* **Complete ReAct Orchestration**: Manages the full reasoning-acting-observation cycle
* **Flexible Node Composition**: Can use custom reasoning, action, and observation nodes
* **Iteration Limits**: Configurable maximum iterations with early termination
* **Goal Evaluation**: Sophisticated goal achievement detection
* **Performance Tracking**: Comprehensive metrics and timing information
* **Error Handling**: Robust error handling with recovery strategies
* **Context Management**: Maintains and updates context across iterations

### Component Configuration

#### Basic Setup

```csharp
// Build an ActionGraphNode that discovers executable actions from the kernel.
var actionNode = ActionGraphNode.CreateWithActions(
    kernel,
    new ActionSelectionCriteria { MinRequiredParameters = 0, MaxRequiredParameters = 5 },
    "action_node"
);

// Create a ReAct loop node and compose reasoning, action and observation nodes.
var reactLoopNode = new ReActLoopGraphNode(nodeId: "react_loop", name: "react_loop");
reactLoopNode.ConfigureNodes(reasoningNode, actionNode, observationNode);
```

#### Factory Method

```csharp
var reactLoopNode = ReActLoopGraphNode.CreateWithNodes(
    reasoningNode: reasoningNode,
    actionNode: actionNode,
    observationNode: observationNode,
    nodeId: "react_loop"
);
```

### Configuration Options

#### Loop Behavior

```csharp
// Configure iteration limits
reactLoopNode.SetMetadata("MaxIterations", 10);
reactLoopNode.SetMetadata("IterationTimeout", TimeSpan.FromMinutes(5));
reactLoopNode.SetMetadata("TotalTimeout", TimeSpan.FromMinutes(30));

// Configure goal achievement
reactLoopNode.SetMetadata("GoalAchievementThreshold", 0.95);
reactLoopNode.SetMetadata("EarlyTerminationEnabled", true);
```

#### Performance Tracking

```csharp
// Enable comprehensive metrics
reactLoopNode.SetMetadata("TrackMetrics", true);
reactLoopNode.SetMetadata("TrackTiming", true);
reactLoopNode.SetMetadata("TrackIterations", true);
```

### Metadata Keys

#### Counters (int)
* `"ExecutionCount"`: Total number of executions
* `"FailureCount"`: Number of failed executions
* `"SuccessfulCompletions"`: Number of successful completions
* `"TotalIterations"`: Total iterations across all executions

#### Metrics
* `"AverageExecutionTime"`: Average time per execution
* `"AverageIterationsPerExecution"`: Average iterations per execution
* `"SuccessRate"`: Success rate percentage
* `"LastExecutedAt"`: Timestamp of last execution

#### Configuration
* `"MaxIterations"`: Maximum iterations per execution
* `"GoalAchievementThreshold"`: Threshold for goal achievement
* `"EarlyTerminationEnabled"`: Whether to enable early termination
* `"IterationTimeout"`: Timeout per iteration
* `"TotalTimeout"`: Total timeout for execution
* `"Domain"`: Domain-specific configuration

## ObservationGraphNode

The `ObservationGraphNode` implements observation and analysis capabilities for the ReAct pattern, analyzing action results, extracting insights, and determining if goals have been achieved.

### Overview

This node analyzes the results of executed actions, evaluates their success, extracts relevant information, and determines the next steps in the reasoning cycle.

### Key Features

* **Result Analysis**: Deep analysis of action execution results
* **Goal Evaluation**: Determines if objectives have been met
* **Information Extraction**: Extracts key insights and data from results
* **Quality Assessment**: Evaluates the quality and relevance of results
* **Context Update**: Updates context for next reasoning iteration
* **Decision Making**: Determines whether to continue or conclude the ReAct loop

### Domain Specialization

```csharp
// Create for specific domain
var mathObservationNode = ObservationGraphNode.CreateForDomain(
    ObservationDomain.Mathematics,
    nodeId: "math_observation"
);

// Create for general observation
var generalObservationNode = new ObservationGraphNode(
    observationPrompt: "Analyze the action result and determine if the goal was achieved.",
    name: "General Observation"
);
```

### Configuration Options

#### Analysis Behavior

```csharp
var observationNode = new ObservationGraphNode(
    "Analyze the result: {{result}}\nGoal: {{goal}}\nWas the goal achieved?",
    name: "Result Analysis"
);

// Configure observation behavior
observationNode.SetMetadata("DeepAnalysisEnabled", true);
observationNode.SetMetadata("GoalAchievementThreshold", 0.9);
observationNode.SetMetadata("ExtractionPatterns", new[] { "result", "insight", "next_step" });
```

#### Template Engine Integration

```csharp
var templateEngine = new GraphTemplateEngine();
var observationNode = new ObservationGraphNode(
    "Result: {{action_result}}\n" +
    "Expected: {{expected_result}}\n" +
    "Analysis: {{analysis_request}}",
    templateEngine: templateEngine
);
```

### Metadata Keys

#### Counters (int)
* `"ExecutionCount"`: Total number of executions
* `"FailureCount"`: Number of failed executions
* `"GoalAchievedCount"`: Number of times goals were achieved

#### Metrics
* `"AverageExecutionTime"`: Average time per execution
* `"GoalAchievementRate"`: Rate of goal achievement
* `"AverageSuccessAssessment"`: Average success assessment score
* `"LastExecutedAt"`: Timestamp of last execution

#### Configuration
* `"Domain"`: Domain-specific configuration
* `"DeepAnalysisEnabled"`: Whether to perform deep analysis
* `"GoalAchievementThreshold"`: Threshold for goal achievement

#### Behavior Customization
* `"ExtractionPatterns"`: Patterns for information extraction
* `"ResultTypePatterns"`: Patterns for result type analysis
* `"GoalCriteria"`: Criteria for goal evaluation

## Integration Patterns

### Building a Complete ReAct Workflow

```csharp
// 1. Create component nodes
var reasoningNode = new ReasoningGraphNode(
    "Analyze the problem: {{problem}}\nPlan the solution:",
    name: "Problem Analysis"
);

var actionNode = new ActionGraphNode(kernel, "action_execution");

var observationNode = new ObservationGraphNode(
    "Analyze result: {{action_result}}\nGoal achieved?",
    name: "Result Analysis"
);

// 2. Create and configure ReAct loop
var actionNode = ActionGraphNode.CreateWithActions(
    kernel,
    new ActionSelectionCriteria { MinRequiredParameters = 0, MaxRequiredParameters = 5 },
    "action_execution"
);

var reactLoopNode = new ReActLoopGraphNode(nodeId: "complete_react_workflow", name: "complete_react_workflow");
reactLoopNode.ConfigureNodes(reasoningNode, actionNode, observationNode);

// 3. Configure loop behavior
reactLoopNode.SetMetadata("MaxIterations", 5);
reactLoopNode.SetMetadata("GoalAchievementThreshold", 0.9);
reactLoopNode.SetMetadata("EarlyTerminationEnabled", true);

// 4. Add to executor and set start node (use the node id)
executor.AddNode(reactLoopNode);
executor.SetStartNode(reactLoopNode.NodeId);
```

### Conditional Routing with Function Nodes

```csharp
// Create function nodes
var processNode = new FunctionGraphNode(processFunction, "process")
    .StoreResultAs("processed_result");

var validateNode = new FunctionGraphNode(validateFunction, "validate")
    .StoreResultAs("validation_result");

// Create conditional routing
var qualityCheck = new ConditionalGraphNode(
    condition: state => state.GetValue<double>("quality_score") > 0.8,
    name: "Quality Check"
);

qualityCheck.AddTrueNode(highQualityNode);
qualityCheck.AddFalseNode(improvementNode);

// Connect the workflow
processNode.ConnectTo(validateNode);
validateNode.ConnectTo(qualityCheck);
```

## Performance Considerations

### Caching and Optimization

* **Conditional Nodes**: Use template-based conditions for complex logic to leverage caching
* **Function Nodes**: Enable result storage only when needed to avoid memory overhead
* **ReAct Loops**: Set appropriate iteration limits and timeouts to prevent infinite loops

### Memory Management

* **Metadata**: Use metadata sparingly for configuration; avoid storing large objects
* **State**: Leverage `StoreResultAs` for important results rather than storing everything
* **Connections**: Minimize the number of conditional edges for better performance

### Monitoring and Debugging

* **Metrics**: Enable metrics tracking for performance-critical nodes
* **Logging**: Use the built-in logging capabilities for debugging complex workflows
* **Validation**: Implement proper validation to catch issues early

## Related Types

* **IGraphNode**: Base interface for all graph nodes
* **GraphState**: Wrapper around KernelArguments with execution metadata
* **ConditionalEdge**: Defines conditional transitions between nodes
* **ActionGraphNode**: Specialized node for executing actions
* **GraphExecutor**: Orchestrates the execution of graph workflows

## See Also

* [Node Types](../concepts/node-types.md) - Comprehensive overview of available node implementations
* [Execution Model](../concepts/execution-model.md) - How nodes participate in execution
* [ReAct Pattern](../patterns/react.md) - Understanding the ReAct reasoning pattern
* [Conditional Nodes](../how-to/conditional-nodes.md) - Building conditional workflows
* [Getting Started](../getting-started.md) - Building your first graph
