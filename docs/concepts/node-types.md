# Node Types

This guide covers all the available node types in SemanticKernel.Graph, explaining their purpose, capabilities, and how to use them effectively in your workflows.

## Overview

Nodes are the fundamental building blocks of graphs in SemanticKernel.Graph. Each node type serves a specific purpose and can be configured and connected to create complex workflows. Understanding the different node types helps you choose the right components for your use case.

## Core Node Types

### FunctionGraphNode

The `FunctionGraphNode` encapsulates a Semantic Kernel function and provides graph-specific functionality around existing `ISKFunction` instances.

**Key Features:**
- **Function Encapsulation**: Wraps any `KernelFunction` with graph-aware behavior
- **Navigation Control**: Connects to unconditional successors or conditional transitions
- **Metadata Hooks**: Custom setup, cleanup, and error handling logic
- **Result Storage**: Automatically stores execution results in graph state

**Usage Example:**
```csharp
// Create a function node from a kernel plugin
var mathNode = FunctionGraphNode.FromPlugin(kernel, "math", "add")
    .StoreResultAs("sum_result");

// Create a function node from a custom function
var customFunction = KernelFunctionFactory.CreateFromMethod(
    (string input) => $"Processed: {input}",
    "ProcessInput",
    "Processes input text"
);

var processNode = new FunctionGraphNode(customFunction, "process_input")
    .StoreResultAs("processed_result");

// Connect nodes
mathNode.ConnectTo(processNode);
```

**Metadata Keys:**
- `"StoreResultAs"` (string): Stores the last result into `GraphState` under the given key
- `"BeforeExecute"` (Action/Func): Custom setup logic executed in `OnBeforeExecuteAsync`
- `"AfterExecute"` (Action/Func): Custom cleanup logic executed in `OnAfterExecuteAsync`
- `"OnExecutionFailed"` (Action/Func): Error handling hook executed in `OnExecutionFailedAsync`
- `"StrictValidation"` (bool): If true, pre-execution validation failures cause an exception

### ConditionalGraphNode

The `ConditionalGraphNode` implements conditional if/else logic based on graph state, enabling dynamic routing without executing functions.

**Key Features:**
- **Function-based Conditions**: Direct evaluation using `Func<GraphState, bool>`
- **Template-based Conditions**: Handlebars-like templates with variable substitution
- **Advanced Caching**: Automatic cache of evaluation results for performance
- **Debugging Support**: Integration with `ConditionalDebugger` for step-by-step analysis
- **Metrics Tracking**: Comprehensive execution metrics and performance monitoring

**Usage Example:**
```csharp
// Function-based condition
var discountCondition = new ConditionalGraphNode(
    state =>
    {
        var age = state.GetValue<int>("user_age");
        var isPremium = state.GetValue<bool>("user_premium");
        return age >= 18 && isPremium;
    },
    nodeId: "discount_check",
    name: "DiscountEligibilityCheck"
);

// Template-based condition
var templateCondition = new ConditionalGraphNode(
    "{{user_age}} >= 18 && {{user_premium}} == true",
    nodeId: "template_discount_check",
    name: "TemplateDiscountCheck"
);

// Configure conditional paths
discountCondition
    .AddTrueNode(discountNode)   // Execute if condition is true
    .AddFalseNode(noDiscountNode); // Execute if condition is false
```

**Metadata Keys:**
- `"ExecutionCount"`, `"FailureCount"`: Aggregate counters (int)
- `"AverageExecutionTime"`, `"LastExecutedAt"`: Metrics
- `"ConditionTemplate"`: Template string for template-based conditions
- `"CacheEnabled"`, `"CacheTimeout"`: Caching configuration

### SwitchGraphNode

The `SwitchGraphNode` provides multi-way branching logic similar to a switch statement, with multiple cases and associated nodes.

**Key Features:**
- **Multiple Cases**: Each case has its own condition and associated nodes
- **Template Support**: Handlebars templates for case conditions
- **Case Management**: Add, remove, and configure cases dynamically
- **Default Case**: Optional default case for unmatched conditions

**Usage Example:**
```csharp
var switchNode = new SwitchGraphNode("user_type_switch", "Routes users based on their type");

// Add cases with function-based conditions
switchNode.AddCase(new SwitchCase("premium", state => 
    state.GetValue<bool>("is_premium") && state.GetValue<int>("account_age") > 365));

// Add cases with template-based conditions
switchNode.AddCase(new SwitchCase("new_user", "{{account_age}} <= 30"));

// Add cases with associated nodes
var premiumCase = switchNode.GetCase("premium");
premiumCase.AddNode(premiumFeaturesNode);

var newUserCase = switchNode.GetCase("new_user");
newUserCase.AddNode(onboardingNode);

// Set default case
switchNode.SetDefaultCase(defaultCase);
```

**SwitchCase Properties:**
- `CaseId`: Unique identifier for the case
- `Name`: Human-readable name
- `Condition`: Function that evaluates the case condition
- `ConditionTemplate`: Handlebars template (if provided)
- `Nodes`: List of nodes associated with this case
- `CreatedAt`: Timestamp when the case was created

## ReAct Pattern Nodes

### ReasoningGraphNode

The `ReasoningGraphNode` implements reasoning capabilities for analyzing the current situation and planning next actions, designed to be used as part of the ReAct pattern.

**Key Features:**
- **Context-aware Reasoning**: Analyzes current situation and available data
- **Template-based Prompts**: Uses customizable templates for different reasoning patterns
- **Quality Metrics**: Tracks reasoning quality and consistency
- **Domain Specialization**: Can be configured for specific problem domains
- **Chain-of-thought Support**: Supports step-by-step reasoning patterns

**Usage Example:**
```csharp
// Create reasoning node with custom prompt
var reasoningNode = new ReasoningGraphNode(
    "Analyze the current situation: {{context}}. " +
    "Based on available actions: {{available_actions}}, " +
    "what should be done next?",
    nodeId: "analyze_situation",
    name: "Situation Analysis"
);

// Create domain-specific reasoning node
var mathReasoning = ReasoningGraphNode.CreateForDomain(
    ReasoningDomain.Mathematics,
    nodeId: "math_reasoning"
);

// Configure reasoning behavior
reasoningNode.SetMetadata("ChainOfThoughtEnabled", true);
reasoningNode.SetMetadata("MaxReasoningSteps", 5);
reasoningNode.SetMetadata("ConfidenceThreshold", 0.8);
```

**Metadata Keys:**
- `"ExecutionCount"`, `"FailureCount"`: Aggregate counters (int)
- `"AverageExecutionTime"`, `"AverageConfidenceScore"`, `"LastExecutedAt"`: Metrics
- `"Domain"`, `"ChainOfThoughtEnabled"`, `"MaxReasoningSteps"`, `"ConfidenceThreshold"`: Configuration

### ActionGraphNode

The `ActionGraphNode` performs action selection and execution for ReAct-style workflows, choosing functions to invoke based on reasoning output and context.

**Key Features:**
- **Action Selection**: Direct, intelligent (context/reasoning-aware), or random strategies
- **Safe Execution**: Time-bounded execution with retry and cancellation support
- **Parameter Mapping**: Optional mapping from graph arguments to function parameters
- **Observability**: Per-action success/failure counts, average latency, last execution time
- **Dynamic Routing**: Next-node selection driven by edge predicates and success/failure outcomes

**Usage Example:**
```csharp
// Create action node with auto-discovered actions
var actions = ActionGraphNode.CreateWithActions(
    kernel,
    new ActionSelectionCriteria
    {
        // Keep open by default; can be restricted via IncludedPlugins/FunctionNamePattern
    },
    "react_act"
);

// Configure execution strategy
actions.ConfigureExecution(
    ActionSelectionStrategy.Intelligent, 
    enableParameterValidation: true
);

// Add custom actions
actions.AddAction("custom_action", customFunction);

// Configure parameter mapping
actions.ConfigureParameterMapping("custom_action", args =>
{
    var mappedArgs = new KernelArguments();
    mappedArgs["input"] = args.GetValue<string>("user_input");
    mappedArgs["context"] = args.GetValue<string>("context");
    return mappedArgs;
});
```

**Metadata Keys:**
- `"ExecutionCount"`, `"FailureCount"`: Aggregate counters (int)
- `"SuccessfulActionCounts"`, `"FailedActionCounts"`: Per-action counters (Dictionary<string,int>)
- `"AverageExecutionTime"`, `"TotalExecutionTime"`, `"LastExecutedAt"`: Timings
- `"SelectionStrategy"`, `"ParameterValidationEnabled"`, `"MaxExecutionTime"`, `"EnableRetryOnFailure"`, `"MaxRetries"`: Configuration

### ObservationGraphNode

The `ObservationGraphNode` analyzes action results, extracts insights, and determines if goals have been achieved for the ReAct pattern.

**Key Features:**
- **Result Analysis**: Deep analysis of action execution results
- **Goal Evaluation**: Determines if objectives have been met
- **Information Extraction**: Extracts key insights and data from results
- **Quality Assessment**: Evaluates the quality and relevance of results
- **Context Update**: Updates context for next reasoning iteration
- **Decision Making**: Determines whether to continue or conclude the ReAct loop

**Usage Example:**
```csharp
// Create observation node with custom prompt
var observationNode = new ObservationGraphNode(
    "Analyze the result: {{action_result}}. " +
    "Has the goal been achieved? Extract key insights.",
    nodeId: "analyze_results",
    name: "Result Analysis"
);

// Create domain-specific observation node
var mathObservation = ObservationGraphNode.CreateForDomain(
    ObservationDomain.Mathematics,
    nodeId: "math_observation"
);

// Configure observation behavior
observationNode.SetMetadata("DeepAnalysisEnabled", true);
observationNode.SetMetadata("GoalAchievementThreshold", 0.9);
observationNode.SetMetadata("ExtractionPatterns", new[] { "result", "insight", "next_step" });
```

**Metadata Keys:**
- `"ExecutionCount"`, `"FailureCount"`, `"GoalAchievedCount"`: Aggregate counters (int)
- `"AverageExecutionTime"`, `"GoalAchievementRate"`, `"AverageSuccessAssessment"`, `"LastExecutedAt"`: Metrics
- `"Domain"`, `"DeepAnalysisEnabled"`, `"GoalAchievementThreshold"`: Configuration
- `"ExtractionPatterns"`, `"ResultTypePatterns"`, `"GoalCriteria"`: Behavior customization

### ReActLoopGraphNode

The `ReActLoopGraphNode` orchestrates the complete ReAct (Reasoning + Acting) pattern loop, coordinating reasoning, action execution, and observation in iterative cycles until goal achievement.

**Key Features:**
- **Complete ReAct Orchestration**: Manages the full reasoning-acting-observation cycle
- **Flexible Node Composition**: Can use custom reasoning, action, and observation nodes
- **Iteration Limits**: Configurable maximum iterations with early termination
- **Goal Evaluation**: Sophisticated goal achievement detection
- **Performance Tracking**: Comprehensive metrics and timing information
- **Error Handling**: Robust error handling with recovery strategies
- **Context Management**: Maintains and updates context across iterations

**Usage Example:**
```csharp
// Create ReAct loop node
var reactNode = new ReActLoopGraphNode(
    nodeId: "react_loop",
    name: "ReAct Problem Solver",
    description: "Solves problems using reasoning, action, and observation cycles"
);

// Configure component nodes
reactNode.ConfigureNodes(
    reasoningNode,    // Analyze and plan
    actionNode,       // Execute actions
    observationNode   // Observe results
);

// Configure loop behavior
reactNode.SetMetadata("MaxIterations", 10);
reactNode.SetMetadata("GoalAchievementThreshold", 0.95);
reactNode.SetMetadata("EarlyTerminationEnabled", true);
reactNode.SetMetadata("IterationTimeout", TimeSpan.FromMinutes(5));

// Connect to next nodes after loop completion
reactNode.ConnectTo(finalResultNode);
```

**Metadata Keys:**
- `"ExecutionCount"`, `"FailureCount"`, `"SuccessfulCompletions"`, `"TotalIterations"`: Aggregate counters (int)
- `"AverageExecutionTime"`, `"AverageIterationsPerExecution"`, `"SuccessRate"`, `"LastExecutedAt"`: Metrics
- `"MaxIterations"`, `"GoalAchievementThreshold"`, `"EarlyTerminationEnabled"`, `"IterationTimeout"`, `"TotalTimeout"`, `"Domain"`: Configuration

## Loop Control Nodes

### WhileLoopGraphNode

The `WhileLoopGraphNode` implements while-loop semantics with configurable conditions and iteration limits.

**Key Features:**
- **Condition-based Looping**: Continues while a condition evaluates to true
- **Iteration Limits**: Configurable maximum iterations to prevent infinite loops
- **State Management**: Maintains loop state across iterations
- **Early Termination**: Support for breaking out of loops early
- **Performance Tracking**: Monitors loop performance and efficiency

**Usage Example:**
```csharp
var whileLoop = new WhileLoopGraphNode(
    state => state.GetValue<int>("counter") < 100,
    nodeId: "counter_loop",
    name: "Counter Loop"
);

// Configure loop behavior
whileLoop.SetMetadata("MaxIterations", 1000);
whileLoop.SetMetadata("IterationTimeout", TimeSpan.FromMinutes(10));

// Add loop body nodes
whileLoop.AddLoopBodyNode(incrementNode);
whileLoop.AddLoopBodyNode(processNode);

// Set loop exit condition
whileLoop.SetExitCondition(state => 
    state.GetValue<bool>("should_exit") || 
    state.GetValue<int>("counter") >= 100);
```

### ForeachLoopGraphNode

The `ForeachLoopGraphNode` iterates over collections, executing nodes for each item.

**Key Features:**
- **Collection Iteration**: Processes each item in a collection
- **Parallel Execution**: Optional parallel processing of collection items
- **State Isolation**: Each iteration gets its own state context
- **Progress Tracking**: Monitors iteration progress and completion
- **Error Handling**: Configurable error handling for individual iterations

**Usage Example:**
```csharp
var foreachLoop = new ForeachLoopGraphNode(
    state => state.GetValue<List<string>>("items"),
    nodeId: "process_items",
    name: "Process Items Loop"
);

// Configure loop behavior
foreachLoop.SetMetadata("MaxConcurrency", 5);
foreachLoop.SetMetadata("ContinueOnError", true);

// Add loop body nodes
foreachLoop.AddLoopBodyNode(processItemNode);
foreachLoop.AddLoopBodyNode(validateItemNode);

// Set item processing logic
foreachLoop.ConfigureItemProcessing(
    itemKey: "current_item",
    resultKey: "processed_result"
);
```

## Specialized Nodes

### HumanApprovalGraphNode

The `HumanApprovalGraphNode` pauses execution for human approval, enabling human-in-the-loop workflows.

**Key Features:**
- **Human Interaction**: Pauses execution until human approval is received
- **Multiple Channels**: Console, Web API, and custom interaction channels
- **Timeout Support**: Configurable timeouts for approval requests
- **Audit Trail**: Tracks approval decisions and timestamps
- **Conditional Routing**: Routes execution based on approval decisions

**Usage Example:**
```csharp
var approvalNode = new HumanApprovalGraphNode(
    "approval_required",
    "Requires human approval to proceed",
    nodeId: "human_approval"
);

// Configure approval behavior
approvalNode.SetMetadata("ApprovalTimeout", TimeSpan.FromHours(24));
approvalNode.SetMetadata("RequireJustification", true);
approvalNode.SetMetadata("ApprovalThreshold", 1); // Number of approvals required

// Set approval channels
approvalNode.AddChannel(new ConsoleHumanInteractionChannel());
approvalNode.AddChannel(new WebApiHumanInteractionChannel("https://api.example.com/approvals"));

// Configure routing based on approval result
approvalNode.AddApprovedNode(approvedProcessNode);
approvalNode.AddRejectedNode(rejectedProcessNode);
```

### ConfidenceGateGraphNode

The `ConfidenceGateGraphNode` routes execution based on confidence scores, enabling quality-based decision making.

**Key Features:**
- **Confidence Evaluation**: Routes based on confidence scores from previous nodes
- **Configurable Thresholds**: Set minimum confidence levels for different paths
- **Quality Metrics**: Tracks confidence distribution and quality trends
- **Fallback Paths**: Routes to fallback nodes when confidence is insufficient
- **Dynamic Thresholds**: Adjustable thresholds based on context

**Usage Example:**
```csharp
var confidenceGate = new ConfidenceGateGraphNode(
    nodeId: "quality_gate",
    name: "Quality Confidence Gate"
);

// Configure confidence thresholds
confidenceGate.SetMetadata("HighConfidenceThreshold", 0.9);
confidenceGate.SetMetadata("MediumConfidenceThreshold", 0.7);
confidenceGate.SetMetadata("LowConfidenceThreshold", 0.5);

// Set routing paths
confidenceGate.AddHighConfidenceNode(highQualityProcessNode);
confidenceGate.AddMediumConfidenceNode(mediumQualityProcessNode);
confidenceGate.AddLowConfidenceNode(lowQualityProcessNode);
confidenceGate.AddInsufficientConfidenceNode(fallbackNode);

// Configure confidence source
confidenceGate.SetConfidenceSource(state => 
    state.GetValue<double>("confidence_score"));
```

### ErrorHandlerGraphNode

The `ErrorHandlerGraphNode` provides centralized error handling and recovery for graph execution.

**Key Features:**
- **Error Categorization**: Classifies errors by type and severity
- **Recovery Strategies**: Implements retry, rollback, and compensation logic
- **Error Metrics**: Tracks error patterns and recovery success rates
- **Context Preservation**: Maintains execution context during error handling
- **Fallback Mechanisms**: Routes to alternative execution paths on errors

**Usage Example:**
```csharp
var errorHandler = new ErrorHandlerGraphNode(
    nodeId: "error_handler",
    name: "Centralized Error Handler"
);

// Configure error handling policies
errorHandler.SetMetadata("MaxRetries", 3);
errorHandler.SetMetadata("RetryDelay", TimeSpan.FromSeconds(5));
errorHandler.SetMetadata("EnableCircuitBreaker", true);

// Add error handling strategies
errorHandler.AddRetryStrategy(RetryStrategy.ExponentialBackoff);
errorHandler.AddRollbackStrategy(RollbackStrategy.PartialState);

// Set error routing
errorHandler.AddErrorRoute(ErrorType.Transient, retryNode);
errorHandler.AddErrorRoute(ErrorType.Permanent, fallbackNode);
errorHandler.AddErrorRoute(ErrorType.Critical, emergencyNode);
```

## Utility Nodes

### SubgraphGraphNode

The `SubgraphGraphNode` allows you to embed one graph within another, enabling modular graph composition.

**Key Features:**
- **Graph Composition**: Embeds complete graphs as nodes
- **State Isolation**: Maintains separate state contexts for subgraphs
- **Parameter Passing**: Passes parameters between parent and child graphs
- **Result Aggregation**: Collects and processes subgraph results
- **Error Propagation**: Handles errors from subgraph execution

**Usage Example:**
```csharp
var subgraphNode = new SubgraphGraphNode(
    childGraph,
    nodeId: "data_processing",
    name: "Data Processing Subgraph"
);

// Configure parameter mapping
subgraphNode.SetInputMapping("input_data", "raw_data");
subgraphNode.SetInputMapping("processing_config", "config");

// Configure result mapping
subgraphNode.SetOutputMapping("processed_result", "final_result");
subgraphNode.SetOutputMapping("processing_metrics", "metrics");

// Set execution options
subgraphNode.SetMetadata("IsolationLevel", "Full");
subgraphNode.SetMetadata("Timeout", TimeSpan.FromMinutes(30));
```

### PythonGraphNode

The `PythonGraphNode` enables integration with Python code, allowing you to leverage Python libraries and scripts.

**Key Features:**
- **Python Integration**: Executes Python scripts and functions
- **Environment Management**: Manages Python virtual environments
- **Parameter Passing**: Passes data between C# and Python
- **Result Handling**: Processes Python execution results
- **Error Handling**: Manages Python execution errors

**Usage Example:**
```csharp
var pythonNode = new PythonGraphNode(
    "process_data.py",
    nodeId: "python_processor",
    name: "Python Data Processor"
);

// Configure Python environment
pythonNode.SetMetadata("PythonPath", "/usr/bin/python3");
pythonNode.SetMetadata("VirtualEnv", "data_processing_env");
pythonNode.SetMetadata("WorkingDirectory", "/scripts");

// Set input/output parameters
pythonNode.SetInputParameter("input_file", "data.csv");
pythonNode.SetOutputParameter("result_file", "processed_data.csv");
pythonNode.SetOutputParameter("metrics", "processing_metrics.json");
```

## Best Practices

### Node Selection

- **Choose the Right Type**: Select node types that match your workflow requirements
- **Composition over Complexity**: Use simple nodes in combination rather than complex monolithic nodes
- **Reuse Patterns**: Leverage existing node types for common patterns like ReAct loops
- **Custom Nodes**: Create custom nodes only when existing types don't meet your needs

### Configuration

- **Metadata Management**: Use metadata to configure node behavior and track metrics
- **Parameter Mapping**: Configure parameter mapping for seamless data flow between nodes
- **Error Handling**: Implement appropriate error handling strategies for each node type
- **Performance Monitoring**: Enable metrics collection to monitor node performance

### Integration

- **State Management**: Ensure proper state flow between different node types
- **Edge Configuration**: Use conditional edges to create dynamic routing based on node results
- **Middleware**: Leverage middleware for cross-cutting concerns across all node types
- **Testing**: Test individual nodes and their combinations to ensure proper integration

## See Also

- [Graph Concepts](graph-concepts.md) - Fundamental graph concepts and components
- [Execution Model](execution-model.md) - How nodes are executed and managed
- [State Management](state.md) - How data flows between nodes
- [Routing Strategies](routing.md) - How to connect and route between nodes
- [Examples](../examples/) - Practical examples of node usage patterns
