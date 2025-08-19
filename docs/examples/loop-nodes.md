# Loop Nodes Example

This example demonstrates how to implement various types of loop patterns in Semantic Kernel Graph workflows. It shows how to create controlled loops, iterative processing, and loop-based decision making using different loop node types.

## Objective

Learn how to implement loop patterns in graph-based workflows to:
- Create controlled loops with exit conditions
- Implement iterative processing with state management
- Handle loop-based decision making and routing
- Manage loop performance and resource consumption
- Implement loop monitoring and debugging

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Loop Patterns](../concepts/loops.md)

## Key Components

### Concepts and Techniques

- **Loop Control**: Managing loop execution and exit conditions
- **Iterative Processing**: Processing data in repeated cycles
- **State Management**: Maintaining state across loop iterations
- **Loop Monitoring**: Tracking loop performance and progress
- **Resource Management**: Controlling resource consumption in loops

### Core Classes

- `LoopGraphNode`: Base loop node implementation
- `ReActLoopGraphNode`: Reasoning and action loop pattern
- `IterativeGraphNode`: Simple iterative processing
- `LoopControlManager`: Loop execution control
- `LoopPerformanceMetrics`: Loop performance monitoring

## Running the Example

### Command Line

```bash
# Navigate to examples project
cd semantic-kernel-graph/src/SemanticKernel.Graph.Examples

# Run the Loop Nodes example
dotnet run -- --example loop-nodes
```

### Programmatic Execution

```csharp
// Run the example directly
await LoopNodesExample.RunAsync();

// Or run with custom kernel
var kernel = CreateCustomKernel();
await LoopNodesExample.RunAsync(kernel);
```

## Step-by-Step Implementation

### 1. Basic Loop Implementation

This example demonstrates basic loop creation and control.

```csharp
// Create kernel with mock configuration
var kernel = CreateKernel();

// Create basic loop workflow
var basicLoopWorkflow = new GraphExecutor("BasicLoopWorkflow", "Basic loop implementation", logger);

// Configure loop options
var loopOptions = new LoopOptions
{
    MaxIterations = 10,
    EnableLoopMonitoring = true,
    EnablePerformanceMetrics = true,
    EnableStatePersistence = true,
    LoopTimeout = TimeSpan.FromMinutes(5)
};

basicLoopWorkflow.ConfigureLoop(loopOptions);

// Loop control node
var loopController = new LoopGraphNode(
    "loop-controller",
    "Control loop execution",
    async (context) =>
    {
        var currentIteration = context.GetValue<int>("iteration", 0);
        var maxIterations = context.GetValue<int>("max_iterations", 10);
        var shouldContinue = context.GetValue<bool>("should_continue", true);
        
        // Check loop conditions
        var canContinue = currentIteration < maxIterations && shouldContinue;
        
        // Update loop state
        context.SetValue("can_continue", canContinue);
        context.SetValue("loop_complete", !canContinue);
        context.SetValue("current_iteration", currentIteration);
        context.SetValue("max_iterations", maxIterations);
        
        if (canContinue)
        {
            context.SetValue("next_iteration", currentIteration + 1);
        }
        
        return $"Loop iteration {currentIteration} - Continue: {canContinue}";
    });

// Loop processing node
var loopProcessor = new FunctionGraphNode(
    "loop-processor",
    "Process data in loop iteration",
    async (context) =>
    {
        var currentIteration = context.GetValue<int>("current_iteration");
        var inputData = context.GetValue<string>("input_data", "default");
        
        // Simulate processing
        await Task.Delay(Random.Shared.Next(100, 300));
        
        var processedData = $"Processed iteration {currentIteration}: {inputData}";
        var processingResult = $"Result_{currentIteration}_{DateTime.UtcNow:HHmmss}";
        
        // Update state
        context.SetValue("processed_data", processedData);
        context.SetValue("processing_result", processingResult);
        context.SetValue("processing_timestamp", DateTime.UtcNow);
        context.SetValue("iteration_complete", true);
        
        return processedData;
    });

// Loop state manager
var loopStateManager = new FunctionGraphNode(
    "loop-state-manager",
    "Manage loop state and prepare for next iteration",
    async (context) =>
    {
        var currentIteration = context.GetValue<int>("current_iteration");
        var processedData = context.GetValue<string>("processed_data");
        var processingResult = context.GetValue<string>("processing_result");
        var canContinue = context.GetValue<bool>("can_continue");
        
        // Prepare for next iteration
        if (canContinue)
        {
            var nextIteration = currentIteration + 1;
            context.SetValue("iteration", nextIteration);
            context.SetValue("iteration_data", $"Data for iteration {nextIteration}");
            context.SetValue("should_continue", true);
        }
        else
        {
            context.SetValue("should_continue", false);
            context.SetValue("final_result", $"Loop completed after {currentIteration} iterations");
        }
        
        // Update loop summary
        var loopSummary = new Dictionary<string, object>
        {
            ["total_iterations"] = currentIteration,
            ["last_processed_data"] = processedData,
            ["last_processing_result"] = processingResult,
            ["loop_complete"] = !canContinue,
            ["completion_timestamp"] = DateTime.UtcNow
        };
        
        context.SetValue("loop_summary", loopSummary);
        
        return $"State updated for iteration {currentIteration}";
    });

// Add nodes to workflow
basicLoopWorkflow.AddNode(loopController);
basicLoopWorkflow.AddNode(loopProcessor);
basicLoopWorkflow.AddNode(loopStateManager);

// Set start node
basicLoopWorkflow.SetStartNode(loopController.NodeId);

// Test basic loop
Console.WriteLine("🔄 Testing basic loop implementation...");

var loopArguments = new KernelArguments
{
    ["iteration"] = 0,
    ["max_iterations"] = 5,
    ["input_data"] = "Sample loop data",
    ["should_continue"] = true
};

var result = await basicLoopWorkflow.ExecuteAsync(kernel, loopArguments);

var loopSummary = result.GetValue<Dictionary<string, object>>("loop_summary");
var totalIterations = result.GetValue<int>("total_iterations");
var loopComplete = result.GetValue<bool>("loop_complete");

Console.WriteLine($"   Total Iterations: {totalIterations}");
Console.WriteLine($"   Loop Complete: {loopComplete}");
Console.WriteLine($"   Summary Keys: {string.Join(", ", loopSummary.Keys)}");
```

### 2. ReAct Loop Pattern

Demonstrates the Reasoning and Action loop pattern for iterative problem solving.

```csharp
// Create ReAct loop workflow
var reActLoopWorkflow = new GraphExecutor("ReActLoopWorkflow", "ReAct loop pattern implementation", logger);

// Configure ReAct loop options
var reActLoopOptions = new ReActLoopOptions
{
    MaxIterations = 8,
    EnableReasoningValidation = true,
    EnableActionValidation = true,
    EnableGoalTracking = true,
    EnableProgressMonitoring = true,
    ReasoningTimeout = TimeSpan.FromSeconds(30),
    ActionTimeout = TimeSpan.FromSeconds(60)
};

reActLoopWorkflow.ConfigureReActLoop(reActLoopOptions);

// ReAct reasoning node
var reActReasoning = new ReActLoopGraphNode(
    "react-reasoning",
    "Perform reasoning step in ReAct loop",
    async (context) =>
    {
        var currentIteration = context.GetValue<int>("iteration", 0);
        var problem = context.GetValue<string>("problem", "Solve a complex problem");
        var previousActions = context.GetValue<List<string>>("previous_actions", new List<string>());
        var currentState = context.GetValue<string>("current_state", "initial");
        
        // Simulate reasoning process
        await Task.Delay(Random.Shared.Next(200, 500));
        
        var reasoning = $"Reasoning step {currentIteration + 1}: Analyze current state '{currentState}' and plan next action";
        var nextAction = $"Action_{currentIteration + 1}";
        var confidence = Random.Shared.Next(70, 95);
        
        // Update reasoning state
        context.SetValue("reasoning_step", reasoning);
        context.SetValue("next_action", nextAction);
        context.SetValue("reasoning_confidence", confidence);
        context.SetValue("reasoning_complete", true);
        
        return $"Reasoning completed: {reasoning}";
    });

// ReAct action node
var reActAction = new FunctionGraphNode(
    "react-action",
    "Execute action based on reasoning",
    async (context) =>
    {
        var currentIteration = context.GetValue<int>("iteration");
        var nextAction = context.GetValue<string>("next_action");
        var reasoningConfidence = context.GetValue<int>("reasoning_confidence");
        
        // Simulate action execution
        await Task.Delay(Random.Shared.Next(300, 800));
        
        var actionResult = $"Executed {nextAction} with confidence {reasoningConfidence}%";
        var actionSuccess = reasoningConfidence > 80;
        var newState = actionSuccess ? $"State_{currentIteration + 1}" : $"Error_State_{currentIteration + 1}";
        
        // Update action state
        context.SetValue("action_result", actionResult);
        context.SetValue("action_success", actionSuccess);
        context.SetValue("new_state", newState);
        context.SetValue("action_complete", true);
        
        return actionResult;
    });

// ReAct loop controller
var reActController = new FunctionGraphNode(
    "react-controller",
    "Control ReAct loop execution and determine continuation",
    async (context) =>
    {
        var currentIteration = context.GetValue<int>("iteration");
        var maxIterations = context.GetValue<int>("max_iterations", 8);
        var actionSuccess = context.GetValue<bool>("action_success");
        var newState = context.GetValue<string>("new_state");
        var problem = context.GetValue<string>("problem");
        
        // Determine if loop should continue
        var shouldContinue = currentIteration < maxIterations && actionSuccess && !newState.StartsWith("Error");
        var goalAchieved = newState.Contains("Final") || newState.Contains("Solution");
        
        // Update loop control state
        context.SetValue("should_continue", shouldContinue);
        context.SetValue("goal_achieved", goalAchieved);
        context.SetValue("loop_complete", !shouldContinue || goalAchieved);
        
        if (shouldContinue && !goalAchieved)
        {
            context.SetValue("next_iteration", currentIteration + 1);
            context.SetValue("current_state", newState);
        }
        
        // Update ReAct summary
        var reActSummary = new Dictionary<string, object>
        {
            ["iteration"] = currentIteration,
            ["action_success"] = actionSuccess,
            ["new_state"] = newState,
            ["should_continue"] = shouldContinue,
            ["goal_achieved"] = goalAchieved,
            ["loop_complete"] = !shouldContinue || goalAchieved
        };
        
        context.SetValue("react_summary", reActSummary);
        
        return $"ReAct loop control: Continue={shouldContinue}, Goal={goalAchieved}";
    });

// Add nodes to ReAct workflow
reActLoopWorkflow.AddNode(reActReasoning);
reActLoopWorkflow.AddNode(reActAction);
reActLoopWorkflow.AddNode(reActController);

// Set start node
reActLoopWorkflow.SetStartNode(reActReasoning.NodeId);

// Test ReAct loop
Console.WriteLine("🧠 Testing ReAct loop pattern...");

var reActArguments = new KernelArguments
{
    ["iteration"] = 0,
    ["max_iterations"] = 6,
    ["problem"] = "Solve a complex mathematical problem step by step",
    ["current_state"] = "initial",
    ["previous_actions"] = new List<string>()
};

var reActResult = await reActLoopWorkflow.ExecuteAsync(kernel, reActArguments);

var reActSummary = reActResult.GetValue<Dictionary<string, object>>("react_summary");
var iteration = reActResult.GetValue<int>("iteration");
var goalAchieved = reActResult.GetValue<bool>("goal_achieved");

Console.WriteLine($"   Iteration: {iteration}");
Console.WriteLine($"   Goal Achieved: {goalAchieved}");
Console.WriteLine($"   Summary: {string.Join(", ", reActSummary.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
```

### 3. Iterative Processing Loop

Shows how to implement iterative processing with data transformation.

```csharp
// Create iterative processing workflow
var iterativeWorkflow = new GraphExecutor("IterativeWorkflow", "Iterative data processing", logger);

// Configure iterative processing options
var iterativeOptions = new IterativeProcessingOptions
{
    MaxIterations = 15,
    EnableBatchProcessing = true,
    EnableProgressTracking = true,
    EnableQualityMetrics = true,
    BatchSize = 5,
    QualityThreshold = 0.8
};

iterativeWorkflow.ConfigureIterativeProcessing(iterativeOptions);

// Data generator node
var dataGenerator = new FunctionGraphNode(
    "data-generator",
    "Generate data for iterative processing",
    async (context) =>
    {
        var iteration = context.GetValue<int>("iteration", 0);
        var batchSize = context.GetValue<int>("batch_size", 5);
        
        // Generate sample data
        var data = new List<string>();
        for (int i = 0; i < batchSize; i++)
        {
            data.Add($"Data_{iteration}_{i}_{DateTime.UtcNow:HHmmss}");
        }
        
        context.SetValue("generated_data", data);
        context.SetValue("data_count", data.Count);
        context.SetValue("generation_timestamp", DateTime.UtcNow);
        
        return $"Generated {data.Count} data items for iteration {iteration}";
    });

// Data processor node
var dataProcessor = new FunctionGraphNode(
    "data-processor",
    "Process data in current iteration",
    async (context) =>
    {
        var iteration = context.GetValue<int>("iteration");
        var generatedData = context.GetValue<List<string>>("generated_data");
        var batchSize = context.GetValue<int>("batch_size");
        
        // Simulate data processing
        await Task.Delay(Random.Shared.Next(200, 600));
        
        var processedData = new List<string>();
        var processingQuality = new List<double>();
        
        foreach (var data in generatedData)
        {
            var processed = $"Processed_{data}";
            processedData.Add(processed);
            
            // Simulate quality score
            var quality = Random.Shared.NextDouble();
            processingQuality.Add(quality);
        }
        
        // Calculate quality metrics
        var averageQuality = processingQuality.Average();
        var qualityThreshold = context.GetValue<double>("quality_threshold", 0.8);
        var qualityMet = averageQuality >= qualityThreshold;
        
        // Update processing state
        context.SetValue("processed_data", processedData);
        context.SetValue("processing_quality", processingQuality);
        context.SetValue("average_quality", averageQuality);
        context.SetValue("quality_threshold_met", qualityMet);
        context.SetValue("processing_complete", true);
        
        return $"Processed {processedData.Count} items with quality {averageQuality:F2}";
    });

// Iteration controller
var iterationController = new FunctionGraphNode(
    "iteration-controller",
    "Control iteration flow and determine continuation",
    async (context) =>
    {
        var iteration = context.GetValue<int>("iteration");
        var maxIterations = context.GetValue<int>("max_iterations", 15);
        var qualityThresholdMet = context.GetValue<bool>("quality_threshold_met");
        var averageQuality = context.GetValue<double>("average_quality");
        
        // Determine if iteration should continue
        var shouldContinue = iteration < maxIterations && qualityThresholdMet;
        var iterationComplete = !shouldContinue;
        
        // Update iteration state
        context.SetValue("should_continue", shouldContinue);
        context.SetValue("iteration_complete", iterationComplete);
        
        if (shouldContinue)
        {
            context.SetValue("next_iteration", iteration + 1);
        }
        
        // Update iteration summary
        var iterationSummary = new Dictionary<string, object>
        {
            ["current_iteration"] = iteration,
            ["max_iterations"] = maxIterations,
            ["quality_threshold_met"] = qualityThresholdMet,
            ["average_quality"] = averageQuality,
            ["should_continue"] = shouldContinue,
            ["iteration_complete"] = iterationComplete
        };
        
        context.SetValue("iteration_summary", iterationSummary);
        
        return $"Iteration {iteration} control: Continue={shouldContinue}";
    });

// Add nodes to iterative workflow
iterativeWorkflow.AddNode(dataGenerator);
iterativeWorkflow.AddNode(dataProcessor);
iterativeWorkflow.AddNode(iterationController);

// Set start node
iterativeWorkflow.SetStartNode(dataGenerator.NodeId);

// Test iterative processing
Console.WriteLine("📊 Testing iterative processing...");

var iterativeArguments = new KernelArguments
{
    ["iteration"] = 0,
    ["max_iterations"] = 8,
    ["batch_size"] = 3,
    ["quality_threshold"] = 0.75
};

var iterativeResult = await iterativeWorkflow.ExecuteAsync(kernel, iterativeArguments);

var iterationSummary = iterativeResult.GetValue<Dictionary<string, object>>("iteration_summary");
var currentIteration = iterativeResult.GetValue<int>("current_iteration");
var qualityThresholdMet = iterativeResult.GetValue<bool>("quality_threshold_met");

Console.WriteLine($"   Current Iteration: {currentIteration}");
Console.WriteLine($"   Quality Threshold Met: {qualityThresholdMet}");
Console.WriteLine($"   Summary: {string.Join(", ", iterationSummary.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
```

### 4. Advanced Loop Patterns

Demonstrates advanced loop patterns including nested loops and conditional loops.

```csharp
// Create advanced loop workflow
var advancedLoopWorkflow = new GraphExecutor("AdvancedLoopWorkflow", "Advanced loop patterns", logger);

// Configure advanced loop options
var advancedLoopOptions = new AdvancedLoopOptions
{
    EnableNestedLoops = true,
    EnableConditionalLoops = true,
    EnableLoopOptimization = true,
    EnableResourceMonitoring = true,
    MaxNestingDepth = 3,
    ResourceThreshold = 0.8
};

advancedLoopWorkflow.ConfigureAdvancedLoop(advancedLoopOptions);

// Nested loop controller
var nestedLoopController = new FunctionGraphNode(
    "nested-loop-controller",
    "Control nested loop execution",
    async (context) =>
    {
        var outerIteration = context.GetValue<int>("outer_iteration", 0);
        var innerIteration = context.GetValue<int>("inner_iteration", 0);
        var maxOuterIterations = context.GetValue<int>("max_outer_iterations", 3);
        var maxInnerIterations = context.GetValue<int>("max_inner_iterations", 4);
        
        // Determine loop flow
        var outerComplete = outerIteration >= maxOuterIterations;
        var innerComplete = innerIteration >= maxInnerIterations;
        
        if (!outerComplete)
        {
            if (!innerComplete)
            {
                // Continue inner loop
                context.SetValue("next_inner_iteration", innerIteration + 1);
                context.SetValue("loop_level", "inner");
            }
            else
            {
                // Move to next outer iteration
                context.SetValue("next_outer_iteration", outerIteration + 1);
                context.SetValue("next_inner_iteration", 0);
                context.SetValue("loop_level", "outer");
            }
        }
        
        // Update loop state
        context.SetValue("outer_complete", outerComplete);
        context.SetValue("inner_complete", innerComplete);
        context.SetValue("nested_loop_complete", outerComplete);
        
        var loopState = new Dictionary<string, object>
        {
            ["outer_iteration"] = outerIteration,
            ["inner_iteration"] = innerIteration,
            ["loop_level"] = context.GetValue<string>("loop_level", "unknown"),
            ["outer_complete"] = outerComplete,
            ["inner_complete"] = innerComplete,
            ["nested_loop_complete"] = nestedLoopComplete
        };
        
        context.SetValue("nested_loop_state", loopState);
        
        return $"Nested loop: Outer={outerIteration}, Inner={innerIteration}, Level={loopState["loop_level"]}";
    });

// Conditional loop processor
var conditionalLoopProcessor = new FunctionGraphNode(
    "conditional-loop-processor",
    "Process data with conditional loop logic",
    async (context) =>
    {
        var iteration = context.GetValue<int>("iteration", 0);
        var condition = context.GetValue<string>("condition", "default");
        var data = context.GetValue<string>("data", "sample");
        
        // Simulate conditional processing
        await Task.Delay(Random.Shared.Next(150, 400));
        
        var processingResult = "";
        var shouldContinue = false;
        
        switch (condition)
        {
            case "quality_check":
                var quality = Random.Shared.NextDouble();
                processingResult = $"Quality check result: {quality:F2}";
                shouldContinue = quality < 0.9; // Continue if quality < 90%
                break;
                
            case "convergence_check":
                var convergence = Random.Shared.NextDouble();
                processingResult = $"Convergence result: {convergence:F2}";
                shouldContinue = convergence < 0.95; // Continue if convergence < 95%
                break;
                
            case "error_check":
                var error = Random.Shared.NextDouble();
                processingResult = $"Error check result: {error:F2}";
                shouldContinue = error > 0.1; // Continue if error > 10%
                break;
                
            default:
                processingResult = $"Default processing: {data}";
                shouldContinue = iteration < 5; // Default limit
                break;
        }
        
        // Update conditional state
        context.SetValue("processing_result", processingResult);
        context.SetValue("should_continue", shouldContinue);
        context.SetValue("condition_met", !shouldContinue);
        context.SetValue("conditional_processing_complete", true);
        
        return processingResult;
    });

// Add nodes to advanced workflow
advancedLoopWorkflow.AddNode(nestedLoopController);
advancedLoopWorkflow.AddNode(conditionalLoopProcessor);

// Set start node
advancedLoopWorkflow.SetStartNode(nestedLoopController.NodeId);

// Test advanced loop patterns
Console.WriteLine("🚀 Testing advanced loop patterns...");

var advancedArguments = new KernelArguments
{
    ["outer_iteration"] = 0,
    ["inner_iteration"] = 0,
    ["max_outer_iterations"] = 3,
    ["max_inner_iterations"] = 4,
    ["condition"] = "quality_check",
    ["data"] = "Advanced loop data"
};

var advancedResult = await advancedLoopWorkflow.ExecuteAsync(kernel, advancedArguments);

var nestedLoopState = advancedResult.GetValue<Dictionary<string, object>>("nested_loop_state");
var conditionalProcessingComplete = advancedResult.GetValue<bool>("conditional_processing_complete");

Console.WriteLine($"   Nested Loop State: {string.Join(", ", nestedLoopState.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
Console.WriteLine($"   Conditional Processing Complete: {conditionalProcessingComplete}");
```

## Expected Output

### Basic Loop Implementation Example

```
🔄 Testing basic loop implementation...
   Total Iterations: 5
   Loop Complete: True
   Summary Keys: total_iterations, last_processed_data, last_processing_result, loop_complete, completion_timestamp
```

### ReAct Loop Pattern Example

```
🧠 Testing ReAct loop pattern...
   Iteration: 6
   Goal Achieved: False
   Summary: iteration=6, action_success=True, new_state=State_6, should_continue=False, goal_achieved=False, loop_complete=True
```

### Iterative Processing Example

```
📊 Testing iterative processing...
   Current Iteration: 8
   Quality Threshold Met: True
   Summary: current_iteration=8, max_iterations=8, quality_threshold_met=True, average_quality=0.82, should_continue=False, iteration_complete=True
```

### Advanced Loop Patterns Example

```
🚀 Testing advanced loop patterns...
   Nested Loop State: outer_iteration=0, inner_iteration=0, loop_level=inner, outer_complete=False, inner_complete=False, nested_loop_complete=False
   Conditional Processing Complete: True
```

## Configuration Options

### Loop Configuration

```csharp
var loopOptions = new LoopOptions
{
    MaxIterations = 10,                           // Maximum number of iterations
    EnableLoopMonitoring = true,                   // Enable loop monitoring
    EnablePerformanceMetrics = true,               // Enable performance metrics
    EnableStatePersistence = true,                 // Enable state persistence
    LoopTimeout = TimeSpan.FromMinutes(5),         // Loop execution timeout
    EnableResourceMonitoring = true,               // Monitor resource usage
    ResourceThreshold = 0.8,                       // Resource usage threshold
    EnableLoopOptimization = true,                 // Enable loop optimization
    EnableNestedLoops = true,                      // Allow nested loops
    MaxNestingDepth = 3                           // Maximum nesting depth
};
```

### ReAct Loop Configuration

```csharp
var reActLoopOptions = new ReActLoopOptions
{
    MaxIterations = 8,                             // Maximum reasoning-action cycles
    EnableReasoningValidation = true,               // Validate reasoning steps
    EnableActionValidation = true,                  // Validate action results
    EnableGoalTracking = true,                      // Track goal achievement
    EnableProgressMonitoring = true,                // Monitor progress
    ReasoningTimeout = TimeSpan.FromSeconds(30),    // Reasoning step timeout
    ActionTimeout = TimeSpan.FromSeconds(60),       // Action execution timeout
    EnableConfidenceScoring = true,                 // Score reasoning confidence
    EnableActionSuccessTracking = true,             // Track action success rates
    GoalAchievementThreshold = 0.9                 // Goal achievement threshold
};
```

### Iterative Processing Configuration

```csharp
var iterativeOptions = new IterativeProcessingOptions
{
    MaxIterations = 15,                             // Maximum iterations
    EnableBatchProcessing = true,                    // Enable batch processing
    EnableProgressTracking = true,                   // Track progress
    EnableQualityMetrics = true,                    // Track quality metrics
    BatchSize = 5,                                  // Items per batch
    QualityThreshold = 0.8,                         // Quality threshold
    EnableConvergenceChecking = true,               // Check for convergence
    ConvergenceThreshold = 0.001,                   // Convergence threshold
    EnableErrorTracking = true,                     // Track errors
    ErrorThreshold = 0.1                            // Error threshold
};
```

## Troubleshooting

### Common Issues

#### Infinite Loops
```bash
# Problem: Loop runs indefinitely
# Solution: Set proper exit conditions and max iterations
MaxIterations = 10;
EnableLoopMonitoring = true;
LoopTimeout = TimeSpan.FromMinutes(5);
```

#### Performance Issues
```bash
# Problem: Loop performance degrades over iterations
# Solution: Enable optimization and resource monitoring
EnableLoopOptimization = true;
EnableResourceMonitoring = true;
ResourceThreshold = 0.8;
```

#### State Corruption
```bash
# Problem: Loop state becomes corrupted
# Solution: Enable state persistence and validation
EnableStatePersistence = true;
EnableStateValidation = true;
EnableStateRecovery = true;
```

### Debug Mode

Enable detailed loop monitoring for troubleshooting:

```csharp
// Enable debug loop monitoring
var debugLoopOptions = new LoopOptions
{
    MaxIterations = 10,
    EnableLoopMonitoring = true,
    EnablePerformanceMetrics = true,
    EnableDebugLogging = true,
    EnableStateInspection = true,
    EnableLoopVisualization = true,
    LogLoopIterations = true,
    LogLoopState = true
};
```

## Advanced Patterns

### Custom Loop Controllers

```csharp
// Implement custom loop controller
public class CustomLoopController : ILoopController
{
    public async Task<LoopControlDecision> ShouldContinueAsync(LoopContext context)
    {
        var iteration = context.GetValue<int>("iteration");
        var customCondition = context.GetValue<string>("custom_condition");
        
        // Custom loop logic
        switch (customCondition)
        {
            case "adaptive":
                return await HandleAdaptiveLoop(context);
            case "quality_based":
                return await HandleQualityBasedLoop(context);
            case "resource_based":
                return await HandleResourceBasedLoop(context);
            default:
                return new LoopControlDecision { ShouldContinue = iteration < 10 };
        }
    }
    
    private async Task<LoopControlDecision> HandleAdaptiveLoop(LoopContext context)
    {
        // Implement adaptive loop logic
        var performance = context.GetValue<double>("performance", 0.0);
        var shouldContinue = performance < 0.9;
        
        return new LoopControlDecision 
        { 
            ShouldContinue = shouldContinue,
            Reason = $"Performance {performance:F2} below threshold 0.9"
        };
    }
}
```

### Loop Performance Optimization

```csharp
// Implement loop performance optimizer
public class LoopPerformanceOptimizer : ILoopOptimizer
{
    public async Task<LoopOptimizationResult> OptimizeLoopAsync(LoopContext context)
    {
        var optimization = new LoopOptimizationResult();
        
        // Analyze loop performance
        var iterations = context.GetValue<int>("iteration");
        var averageTime = context.GetValue<double>("average_iteration_time");
        var resourceUsage = context.GetValue<double>("resource_usage");
        
        // Suggest optimizations
        if (averageTime > 1000) // More than 1 second
        {
            optimization.Suggestions.Add("Consider reducing processing complexity");
            optimization.Suggestions.Add("Enable parallel processing if possible");
        }
        
        if (resourceUsage > 0.8) // More than 80%
        {
            optimization.Suggestions.Add("Reduce batch size");
            optimization.Suggestions.Add("Implement resource throttling");
        }
        
        if (iterations > 20)
        {
            optimization.Suggestions.Add("Consider early termination conditions");
            optimization.Suggestions.Add("Implement convergence checking");
        }
        
        return optimization;
    }
}
```

### Loop State Management

```csharp
// Implement advanced loop state management
public class AdvancedLoopStateManager : ILoopStateManager
{
    public async Task<LoopState> GetLoopStateAsync(string loopId)
    {
        // Retrieve loop state from persistent storage
        var state = await LoadStateFromStorage(loopId);
        
        // Validate state integrity
        if (!await ValidateStateIntegrity(state))
        {
            state = await RecoverState(loopId);
        }
        
        return state;
    }
    
    public async Task SaveLoopStateAsync(string loopId, LoopState state)
    {
        // Add metadata
        state.Metadata["last_updated"] = DateTime.UtcNow;
        state.Metadata["version"] = state.Version + 1;
        
        // Compress state if large
        if (state.Size > 1024 * 1024) // 1MB
        {
            state = await CompressState(state);
        }
        
        // Save to persistent storage
        await SaveStateToStorage(loopId, state);
    }
}
```

## Related Examples

- [ReAct Agent](./react-agent.md): Advanced reasoning and action patterns
- [Graph Metrics](./graph-metrics.md): Loop performance monitoring
- [State Management](./state-tutorial.md): Loop state persistence
- [Performance Optimization](./performance-optimization.md): Loop optimization techniques

## See Also

- [Loop Patterns](../concepts/loops.md): Understanding loop concepts
- [Performance Monitoring](../how-to/performance-monitoring.md): Loop performance analysis
- [State Management](../how-to/state-management.md): Loop state handling
- [API Reference](../api/): Complete API documentation
