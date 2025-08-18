# Debug and Inspection

Debug and inspection capabilities in SemanticKernel.Graph provide comprehensive tools for understanding, troubleshooting, and analyzing graph execution. This guide covers debug sessions, breakpoints, the GraphInspectionApi, graph visualization, and execution replay.

## What You'll Learn

- How to create and configure debug sessions
- Setting up different types of breakpoints
- Using the GraphInspectionApi for runtime monitoring
- Generating graph visualizations in multiple formats
- Replaying execution history for analysis
- Best practices for debugging complex workflows

## Concepts and Techniques

**Debug Session**: A controlled execution environment that allows step-by-step execution, breakpoint management, and state inspection during graph execution.

**Breakpoint**: A condition that pauses execution at a specific node, allowing inspection of state and variables at that point.

**GraphInspectionApi**: Runtime API for inspecting graph structure, execution state, and performance metrics in real-time.

**Graph Visualization**: Export capabilities for generating diagrams in DOT, Mermaid, JSON, and other formats with execution path highlighting.

**Execution Replay**: Ability to replay completed executions step-by-step for analysis and debugging.

## Prerequisites

- [First Graph Tutorial](../first-graph-5-minutes.md) completed
- Basic understanding of graph execution concepts
- Familiarity with state management and conditional nodes

## Debug Sessions

### Creating a Debug Session

Debug sessions provide step-by-step execution control and comprehensive debugging capabilities:

```csharp
using SemanticKernel.Graph.Debug;
using SemanticKernel.Graph.Extensions;

// Create a debug session using the fluent builder
var debugSession = await executor
    .CreateDebugSession(context)
    .WithInitialMode(DebugExecutionMode.StepOver)
    .WithBreakpoint("decision_node", "{{user_score}} > 80", "High score breakpoint")
    .WithBreakpoint("error_handler", state => state.GetValue<bool>("has_error"), "Error condition")
    .BuildAsync();

// Alternative: Create directly with extensions
var (result, debugSession) = await executor.ExecuteWithDebugAsync(
    kernel, 
    arguments, 
    DebugExecutionMode.StepOver
);
```

### Debug Execution Modes

Different modes control how execution flows during debugging:

```csharp
// Step-over: Execute current node and pause at next
await debugSession.ResumeAsync(DebugExecutionMode.StepOver);

// Step-into: Enter subgraph execution if available
await debugSession.ResumeAsync(DebugExecutionMode.StepInto);

// Step-out: Continue until exiting current context
await debugSession.ResumeAsync(DebugExecutionMode.StepOut);

// Continue: Run to next breakpoint
await debugSession.ResumeAsync(DebugExecutionMode.Continue);

// Pause: Stop execution at current point
await debugSession.PauseAsync();
```

### Debug Session Control

Manage the debug session lifecycle and execution flow:

```csharp
// Start the session
await debugSession.StartAsync(DebugExecutionMode.StepOver);

// Check session status
if (debugSession.IsPaused)
{
    var pausedNode = debugSession.PausedAtNode;
    var currentState = debugSession.CurrentState;
    Console.WriteLine($"Paused at: {pausedNode?.Name}");
}

// Step through execution
await debugSession.StepAsync();

// Resume execution
await debugSession.ResumeAsync(DebugExecutionMode.Continue);

// Stop the session
await debugSession.StopAsync();
```

## Breakpoints

### Types of Breakpoints

SemanticKernel.Graph supports multiple breakpoint types for different debugging scenarios:

#### Conditional Breakpoints

Break when a specific condition is met:

```csharp
// Function-based condition
var breakpointId = debugSession.AddBreakpoint(
    "validation_node",
    state => state.GetValue<int>("attempt_count") > 3,
    "Break after 3 attempts"
);

// Expression-based condition using templates
var expressionBreakpoint = debugSession.AddBreakpoint(
    "decision_node",
    "{{user_role}} == 'admin' && {{permission_level}} >= 5",
    "Admin permission check"
);
```

#### Data Breakpoints

Pause execution when specific variables change:

```csharp
// Break when error_count changes
var dataBreakpoint = debugSession.AddDataBreakpoint(
    "error_count",
    "Monitor error count changes"
);

// Break when any variable in a group changes
var groupBreakpoint = debugSession.AddDataBreakpoint(
    "user_preferences",
    "Monitor user preference changes"
);
```

#### Auto-expiring Breakpoints

Breakpoints that automatically remove themselves after being hit:

```csharp
// Break only the first 3 times
var limitedBreakpoint = debugSession.AddBreakpoint(
    "retry_node",
    "{{retry_count}} > 0",
    3, // Max hit count
    "Break on first 3 retries"
);

// Expression-based with hit limit
var expressionLimited = debugSession.AddBreakpoint(
    "validation_node",
    "{{validation_errors}}.Count > 5",
    2, // Max hit count
    "Break when validation errors exceed 5"
);
```

### Breakpoint Management

Manage breakpoints throughout the debugging session:

```csharp
// Get all active breakpoints
var breakpoints = debugSession.GetBreakpoints();
foreach (var bp in breakpoints)
{
    Console.WriteLine($"Breakpoint: {bp.BreakpointId} at {bp.NodeId}");
    Console.WriteLine($"Description: {bp.Description}");
    Console.WriteLine($"Hit count: {bp.HitCount}");
}

// Remove specific breakpoint
debugSession.RemoveBreakpoint(breakpointId);

// Clear all breakpoints
debugSession.ClearBreakpoints();
```

## State Inspection

### Examining Current State

Inspect variables and state during debugging:

```csharp
// Get all current variables
var variables = debugSession.GetCurrentVariables();
foreach (var kvp in variables)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}

// Get specific variable with type safety
var userName = debugSession.GetVariable<string>("user_name");
var userScore = debugSession.GetVariable<int>("user_score");
var isActive = debugSession.GetVariable<bool>("is_active");

// Set variables during debugging
debugSession.SetVariable("debug_mode", true);
debugSession.SetVariable("test_value", 42);
```

### Execution History

Track and analyze execution steps:

```csharp
// Get complete execution history
var history = debugSession.GetExecutionHistory();
foreach (var step in history)
{
    Console.WriteLine($"Step: {step.Node.Name} ({step.Node.NodeId})");
    Console.WriteLine($"Duration: {step.Duration.TotalMilliseconds:F2}ms");
    Console.WriteLine($"Status: {step.Status}");
    
    // Access state before and after execution
    var stateBefore = step.StateBefore;
    var stateAfter = step.StateAfter;
}

// Get available next nodes from current position
var nextNodes = debugSession.GetAvailableNextNodes();
foreach (var node in nextNodes)
{
    Console.WriteLine($"Next: {node.Name} ({node.NodeId})");
}
```

## GraphInspectionApi

### Runtime Inspection

The GraphInspectionApi provides comprehensive runtime monitoring capabilities:

```csharp
using SemanticKernel.Graph.Core;

// Create inspection API
var inspectionApi = new GraphInspectionApi(
    new GraphInspectionOptions
    {
        IncludeDebugInfo = true,
        IncludePerformanceHeatmaps = true
    }
);

// Get graph structure information
var structureInfo = inspectionApi.GetGraphStructure(executionId);
Console.WriteLine($"Graph: {structureInfo.GraphName}");
Console.WriteLine($"Nodes: {structureInfo.NodeCount}");
Console.WriteLine($"Edges: {structureInfo.EdgeCount}");

// Get execution status
var status = inspectionApi.GetExecutionStatus(executionId);
Console.WriteLine($"Status: {status.Status}");
Console.WriteLine($"Start time: {status.StartTime}");
Console.WriteLine($"Duration: {status.Duration}");
```

### Performance Monitoring

Monitor execution performance and metrics:

```csharp
// Get performance metrics for specific node
var nodeMetrics = inspectionApi.GetNodeMetrics(executionId, "processing_node");
if (nodeMetrics != null)
{
    Console.WriteLine($"Total executions: {nodeMetrics.TotalExecutions}");
    Console.WriteLine($"Average time: {nodeMetrics.AverageExecutionTime.TotalMilliseconds:F2}ms");
    Console.WriteLine($"Success rate: {nodeMetrics.SuccessRate:P}");
}

// Get overall performance summary
var performanceSummary = inspectionApi.GetPerformanceSummary(executionId);
Console.WriteLine($"Total execution time: {performanceSummary.TotalExecutionTime.TotalMilliseconds:F2}ms");
Console.WriteLine($"Average node time: {performanceSummary.AverageNodeExecutionTime.TotalMilliseconds:F2}ms");
Console.WriteLine($"Slowest node: {performanceSummary.SlowestNode?.NodeId}");
```

### Debug Information

Access debug-specific information during execution:

```csharp
// Get debug info for a specific node
var debugInfo = inspectionApi.GetNodeDebugInfo(executionId, "decision_node");
if (debugInfo != null)
{
    Console.WriteLine($"Has breakpoints: {debugInfo.HasBreakpoints}");
    Console.WriteLine($"Is paused: {debugInfo.IsPaused}");
    
    if (debugInfo.Breakpoints != null)
    {
        foreach (var bp in debugInfo.Breakpoints)
        {
            Console.WriteLine($"Breakpoint: {bp.Description}");
            Console.WriteLine($"Hit count: {bp.HitCount}");
        }
    }
}
```

## Graph Visualization

### Export Formats

Generate visualizations in multiple formats for different use cases:

```csharp
using SemanticKernel.Graph.Core;

var visualizationEngine = new GraphVisualizationEngine(
    new GraphVisualizationOptions
    {
        EnableExecutionPathHighlighting = true,
        IncludePerformanceMetrics = true
    }
);

// Generate DOT format for GraphViz
var dotOptions = new DotSerializationOptions
{
    GraphName = "My Workflow",
    LayoutDirection = DotLayoutDirection.LeftToRight,
    EnableClustering = true
};
var dotGraph = visualizationEngine.SerializeToDot(visualizationData, dotOptions);

// Generate Mermaid diagram
var mermaidOptions = new MermaidGenerationOptions
{
    Direction = "TB", // Top to bottom
    IncludeTitle = true,
    EnableStyling = true,
    HighlightExecutionPath = true
};
var mermaidDiagram = visualizationEngine.GenerateEnhancedMermaidDiagram(
    visualizationData, 
    mermaidOptions
);

// Generate JSON for API consumption
var jsonOptions = new JsonSerializationOptions
{
    Indented = true,
    UseCamelCase = true,
    IncludeMetadata = true
};
var jsonGraph = visualizationEngine.SerializeToJson(visualizationData, jsonOptions);
```

### Debug Session Visualization

Generate visualizations during debug sessions:

```csharp
// Generate visualization with current state highlighting
var visualization = debugSession.GenerateVisualization(highlightCurrent: true);

// Export session data for analysis
var sessionData = debugSession.ExportSessionData(includeHistory: true);

// Generate Mermaid diagram for debug context
var debugDiagram = debugSession.GenerateMermaidDiagram(highlightCurrent: true);
Console.WriteLine(debugDiagram);
```

### Real-time Updates

Visualization can include real-time execution information:

```csharp
// Create visualization data with execution path
var visualizationData = new GraphVisualizationData
{
    Nodes = graphNodes.Select(n => new NodeVisualizationData
    {
        NodeId = n.NodeId,
        Name = n.Name,
        NodeType = n.GetType().Name,
        IsExecuted = executedNodes.Contains(n.NodeId),
        ExecutionTime = nodeMetrics.GetValueOrDefault(n.NodeId)?.AverageExecutionTime
    }).ToList(),
    
    Edges = graphEdges.Select(e => new EdgeVisualizationData
    {
        FromNodeId = e.FromNodeId,
        ToNodeId = e.ToNodeId,
        Label = e.Condition?.ToString(),
        IsExecuted = executedEdges.Contains($"{e.FromNodeId}->{e.ToNodeId}")
    }).ToList(),
    
    ExecutionPath = executedNodes.ToList(),
    CurrentNode = currentExecutingNode,
    GeneratedAt = DateTimeOffset.UtcNow
};
```

## Execution Replay

### Creating Replays

Replay completed executions for analysis and debugging:

```csharp
// Create replay from debug session
var replay = debugSession.CreateReplay();

// Or create from execution history
var replayFromHistory = new ExecutionReplay(
    executionHistory,
    initialVariables,
    executionId,
    startTime,
    endTime,
    status
);
```

### Replay Control

Control replay execution and analysis:

```csharp
// Start replay
await replay.StartAsync();

// Step through replay
await replay.StepForwardAsync();
await replay.StepBackwardAsync();

// Jump to specific step
await replay.JumpToStepAsync(5);

// Get current replay state
var currentStep = replay.CurrentStep;
var currentState = replay.CurrentState;
var stepIndex = replay.CurrentStepIndex;

// Check replay status
if (replay.IsAtEnd)
{
    Console.WriteLine("Replay completed");
}
else if (replay.IsAtBeginning)
{
    Console.WriteLine("Replay at start");
}
```

### What-if Analysis

Modify variables during replay to test different scenarios:

```csharp
// Modify variables at current step
replay.ModifyVariable("user_score", 95);
replay.ModifyVariable("user_role", "admin");

// Apply changes and continue
await replay.ApplyChangesAsync();
await replay.StepForwardAsync();

// Compare with original execution
var originalResult = replay.GetOriginalResult();
var modifiedResult = replay.GetModifiedResult();
```

## Advanced Debugging Patterns

### Conditional Debugging

Use conditional expressions for sophisticated breakpoint logic:

```csharp
// Complex conditional breakpoint
var complexBreakpoint = debugSession.AddBreakpoint(
    "workflow_node",
    "{{user_role}} == 'admin' && {{permission_level}} >= 5 && {{session_duration}}.TotalMinutes > 30",
    "Admin with high permissions and long session"
);

// Breakpoint with state comparison
var stateBreakpoint = debugSession.AddBreakpoint(
    "validation_node",
    state => {
        var errors = state.GetValue<List<string>>("validation_errors") ?? new();
        var warnings = state.GetValue<List<string>>("validation_warnings") ?? new();
        return errors.Count > 5 || warnings.Count > 10;
    },
    "High error/warning count"
);
```

### Performance Debugging

Debug performance issues with specialized breakpoints:

```csharp
// Break on slow execution
var performanceBreakpoint = debugSession.AddBreakpoint(
    "slow_node",
    state => {
        var executionTime = state.GetValue<TimeSpan>("node_execution_time");
        return executionTime.TotalMilliseconds > 1000; // 1 second
    },
    "Break on slow execution"
);

// Break on memory usage
var memoryBreakpoint = debugSession.AddBreakpoint(
    "memory_intensive_node",
    state => {
        var memoryUsage = state.GetValue<long>("memory_usage_bytes");
        return memoryUsage > 100 * 1024 * 1024; // 100 MB
    },
    "Break on high memory usage"
);
```

### Error Debugging

Debug error conditions and recovery scenarios:

```csharp
// Break on error conditions
var errorBreakpoint = debugSession.AddBreakpoint(
    "error_handler",
    state => {
        var hasError = state.GetValue<bool>("has_error");
        var errorCount = state.GetValue<int>("error_count");
        var lastError = state.GetValue<string>("last_error_message");
        
        return hasError && (errorCount > 3 || lastError?.Contains("timeout") == true);
    },
    "Break on critical errors"
);

// Break on retry attempts
var retryBreakpoint = debugSession.AddBreakpoint(
    "retry_node",
    "{{retry_count}} > {{max_retries}} * 0.8", // Break at 80% of max retries
    "Break near retry limit"
);
```

## Best Practices

### Debug Session Management

- **Session Lifecycle**: Always dispose of debug sessions to free resources
- **Mode Selection**: Choose appropriate debug modes for different scenarios
- **Breakpoint Strategy**: Use conditional breakpoints sparingly to avoid excessive pausing
- **State Inspection**: Inspect state at key decision points rather than every node

### Performance Considerations

- **Breakpoint Impact**: Each breakpoint adds overhead to execution
- **History Size**: Large execution histories consume memory
- **Visualization Generation**: Complex visualizations can be expensive for large graphs
- **Replay Memory**: Long replays require significant memory for state snapshots

### Debugging Workflows

- **Start Simple**: Begin with basic step-over debugging to understand flow
- **Add Breakpoints**: Gradually add breakpoints at critical decision points
- **Use Replay**: Leverage replay for post-execution analysis
- **Document Issues**: Use debug session export for issue reporting

## Troubleshooting

### Common Issues

**Debug session not pausing**: Ensure breakpoints are properly configured and conditions are met.

**Performance degradation**: Limit the number of active breakpoints and use conditional breakpoints judiciously.

**Memory issues**: Monitor execution history size and clear old debug sessions.

**Visualization errors**: Check that graph structure data is valid and complete.

### Debug Session Recovery

```csharp
// Recover from disposed session
if (debugSession.IsDisposed)
{
    // Create new session from existing context
    var newSession = executor.CreateDebugSession(context);
    await newSession.StartAsync(DebugExecutionMode.Continue);
}

// Export session data before disposal
var sessionData = debugSession.ExportSessionData(includeHistory: true);
// Save to file or database for later analysis
```

## See Also

- [Conditional Nodes](../conditional-nodes.md) - Understanding conditional logic and routing
- [State Management](../state.md) - Working with graph state and variables
- [Graph Execution](../execution.md) - Understanding execution flow and lifecycle
- [Examples](../../examples/) - Practical examples of debugging workflows
