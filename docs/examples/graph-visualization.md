# Graph Visualization Example

This example demonstrates how to visualize and inspect graph structures using the Semantic Kernel Graph's visualization capabilities. It shows how to export graphs in various formats, create real-time visualizations, and implement interactive graph inspection.

## Objective

Learn how to implement graph visualization and inspection in graph-based workflows to:
* Export graphs in multiple formats (DOT, JSON, Mermaid)
* Create real-time visualizations with execution highlights
* Implement interactive graph inspection and debugging
* Generate visual representations for documentation and analysis
* Monitor graph execution with visual feedback

## Prerequisites

* **.NET 8.0** or later
* **OpenAI API Key** configured in `appsettings.json`
* **Semantic Kernel Graph package** installed
* Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Visualization Concepts](../concepts/visualization.md)

## Key Components

### Concepts and Techniques

* **Graph Visualization**: Converting graph structures to visual representations
* **Export Formats**: Supporting multiple visualization formats (DOT, JSON, Mermaid)
* **Real-Time Highlights**: Visual feedback during graph execution
* **Interactive Inspection**: Debugging and analyzing graph structures
* **Execution Overlays**: Visual representation of execution flow

### Core Classes

* `GraphVisualizationEngine`: Core visualization engine
* `GraphRealtimeHighlighter`: Real-time execution highlighting
* `GraphInspectionApi`: Interactive graph inspection
* `GraphVisualizationOptions`: Configuration for visualization

## Running the Example

### Getting Started

This example demonstrates graph visualization and export capabilities with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Basic Graph Visualization

This example demonstrates basic graph export and visualization capabilities. The snippet below is a minimal, self-contained sample that mirrors the tested example in `semantic-kernel-graph-docs/examples/GraphVisualizationExample.cs`.

```csharp
// Create a minimal kernel instance for APIs that require it.
var kernel = Kernel.CreateBuilder().Build();

// Create two simple function nodes using a factory helper.
// These functions are trivial and return static strings for demonstration.
var fn1 = KernelFunctionFactory.CreateFromMethod(() => "node1-output", "Fn1");
var fn2 = KernelFunctionFactory.CreateFromMethod(() => "node2-output", "Fn2");

var node1 = new FunctionGraphNode(fn1, "node1", "Node 1");
var node2 = new FunctionGraphNode(fn2, "node2", "Node 2");

// Build visualization data manually for demonstration purposes.
var nodes = new List<IGraphNode> { node1, node2 };
var edges = new List<GraphEdgeInfo> { new GraphEdgeInfo("node1", "node2", "to-node2") };
var visualizationData = new GraphVisualizationData(nodes, edges, currentNode: node2, executionPath: nodes);

// Create the engine and produce outputs in several formats.
using var engine = new GraphVisualizationEngine();

// DOT (Graphviz) output
var dot = engine.SerializeToDot(visualizationData, new DotSerializationOptions { GraphName = "VizExample" });
Console.WriteLine("--- DOT Output ---");
Console.WriteLine(dot);

// Mermaid output
var mermaid = engine.GenerateEnhancedMermaidDiagram(visualizationData, new MermaidGenerationOptions { Direction = "TD" });
Console.WriteLine("--- Mermaid Output ---");
Console.WriteLine(mermaid);

// JSON output (pretty-printed)
var json = engine.SerializeToJson(visualizationData, new JsonSerializationOptions { Indented = true });
Console.WriteLine("--- JSON Output ---");
Console.WriteLine(json);

// Note: This snippet is intentionally minimal. For a runnable, fully-commented example,
// see `semantic-kernel-graph-docs/examples/GraphVisualizationExample.cs` in the repository.
```

### 2. Real-Time Execution Visualization

This snippet shows a simplified real-time visualization flow using a real-time highlighter. The purpose is to demonstrate the pattern; the runnable, fully-commented implementation is available in the docs examples project.

```csharp
// Create the real-time highlighter (null used for an optional transport/logger in this snippet).
var highlightOptions = new GraphRealtimeHighlightOptions
{
    EnableImmediateUpdates = false,
    UpdateInterval = TimeSpan.FromMilliseconds(500),
    EnableAnimations = true
};

using var highlighter = new GraphRealtimeHighlighter(null, highlightOptions, logger);

// Start a highlighting session for a fake execution id and pre-built visualization data.
var executionId = Guid.NewGuid().ToString();
highlighter.StartHighlighting(executionId, visualizationData, new ExecutionHighlightStyle());

// Subscribe to a couple of events to observe progress (handlers should be lightweight).
highlighter.NodeExecutionStarted += (_, e) => Console.WriteLine($"Node started: {e.Node.NodeId}");
highlighter.NodeExecutionCompleted += (_, e) => Console.WriteLine($"Node completed: {e.Node.NodeId}");

// Simulate progress: in a real system you would call UpdateCurrentNode/AddNodeCompletionHighlight.
for (var i = 0; i < 3; i++)
{
    // Simulate some work and updates
    await Task.Delay(300);
    Console.WriteLine($"Simulated iteration {i + 1}");
}

// Produce highlighted exports
var highlightedMermaid = highlighter.GenerateHighlightedVisualization(executionId, HighlightVisualizationFormat.Mermaid);
var highlightedJson = highlighter.GenerateHighlightedVisualization(executionId, HighlightVisualizationFormat.Json);

// Persist or print the results
Console.WriteLine("--- Highlighted Mermaid ---");
Console.WriteLine(highlightedMermaid);
Console.WriteLine("--- Highlighted JSON ---");
Console.WriteLine(highlightedJson);

// Stop the highlighting session when done
highlighter.StopHighlighting(executionId);
```

### 3. Interactive Graph Inspection

This snippet demonstrates a simplified interactive inspection pattern. In practice, the inspection API provides richer capabilities; see the docs example for a tested implementation.

```csharp
// Create an inspection API instance (options are illustrative).
var inspectionOptions = new GraphInspectionOptions
{
    EnableDetailedNodeInspection = true,
    EnablePerformanceMetrics = true,
    EnableRealtimeMonitoring = true
};

using var inspectionApi = new GraphInspectionApi(inspectionOptions, logger);

// Health check example
var health = inspectionApi.GetHealthCheck();
Console.WriteLine($"Inspection API health: {(health.IsSuccess ? "OK" : "FAIL")}");

// Get active executions (returns a result wrapper in the real API)
var active = inspectionApi.GetActiveExecutions();
if (active.IsSuccess)
{
    Console.WriteLine($"Active executions retrieved: {active.Data}");
}

// Inspect graph structure for a given execution id (illustrative).
var executionId = Guid.NewGuid().ToString();
// var structureJson = inspectionApi.GetGraphStructure(executionId, InspectionFormat.Json);
// Console.WriteLine(structureJson);

// Note: interactive breakpoints and step-through are best demonstrated
// with the full example at `semantic-kernel-graph-docs/examples/GraphVisualizationExample.cs`.
```

### 4. Advanced Visualization Features

Demonstrates advanced visualization features including custom styling and export options.

```csharp
// Create advanced visualization workflow
var advancedVisualizationWorkflow = new GraphExecutor("AdvancedVisualizationWorkflow", "Advanced visualization features", logger);

// Configure advanced visualization
var advancedVisualizationOptions = new GraphVisualizationOptions
{
    EnableDOTExport = true,
    EnableJSONExport = true,
    EnableMermaidExport = true,
    EnableRealTimeHighlights = true,
    EnableExecutionOverlays = true,
    EnableCustomStyling = true,
    EnableThemeSupport = true,
    EnableExportCompression = true,
    ExportPath = "./advanced-exports",
    CustomStyles = new Dictionary<string, string>
    {
        ["node_color"] = "#4CAF50",
        ["edge_color"] = "#2196F3",
        ["highlight_color"] = "#FF9800",
        ["error_color"] = "#F44336"
    },
    ExportFormats = new[] { "dot", "json", "mermaid", "svg", "png" }
};

advancedVisualizationWorkflow.ConfigureVisualization(advancedVisualizationOptions);

// Advanced processing node with custom styling
var advancedProcessor = new FunctionGraphNode(
    "advanced-processor",
    "Advanced processing with custom styling",
    async (context) =>
    {
        var inputData = context.GetValue<string>("input_data");
        var processingType = context.GetValue<string>("processing_type", "standard");
        
        // Apply custom styling based on processing type
        var nodeStyle = processingType switch
        {
            "priority" => "priority_style",
            "error" => "error_style",
            "success" => "success_style",
            _ => "standard_style"
        };
        
        context.SetValue("node_style", nodeStyle);
        context.SetValue("processing_type", processingType);
        
        // Simulate processing
        await Task.Delay(Random.Shared.Next(200, 600));
        
        var processedData = $"Advanced processed: {inputData} ({processingType})";
        context.SetValue("processed_data", processedData);
        context.SetValue("processing_step", "advanced_processed");
        
        return processedData;
    });

// Advanced visualizer with custom export
var advancedVisualizer = new FunctionGraphNode(
    "advanced-visualizer",
    "Advanced visualization with custom export",
    async (context) =>
    {
        var processedData = context.GetValue<string>("processed_data");
        var processingType = context.GetValue<string>("processing_type");
        var nodeStyle = context.GetValue<string>("node_style");
        
        // Generate custom visualization
        var customVisualization = new Dictionary<string, object>
        {
            ["node_styles"] = new Dictionary<string, object>
            {
                ["advanced-processor"] = new
                {
                    Style = nodeStyle,
                    Color = GetStyleColor(nodeStyle),
                    BorderWidth = GetStyleBorderWidth(nodeStyle),
                    Shape = GetStyleShape(nodeStyle)
                }
            },
            ["processing_metadata"] = new
            {
                Type = processingType,
                Timestamp = DateTime.UtcNow,
                Style = nodeStyle
            }
        };
        
        context.SetValue("custom_visualization", customVisualization);
        
        // Export with custom styling
        var styledDOT = await advancedVisualizationWorkflow.ExportToDOTAsync(customVisualization);
        var styledJSON = await advancedVisualizationWorkflow.ExportToJSONAsync(customVisualization);
        var styledMermaid = await advancedVisualizationWorkflow.ExportToMermaidAsync(customVisualization);
        
        // Save styled exports
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        File.WriteAllText($"./advanced-exports/styled_{timestamp}.dot", styledDOT);
        File.WriteAllText($"./advanced-exports/styled_{timestamp}.json", styledJSON);
        File.WriteAllText($"./advanced-exports/styled_{timestamp}.mmd", styledMermaid);
        
        return $"Advanced visualization completed with {processingType} styling";
    });

// Add nodes to advanced workflow
advancedVisualizationWorkflow.AddNode(advancedProcessor);
advancedVisualizationWorkflow.AddNode(advancedVisualizer);

// Set start node
advancedVisualizationWorkflow.SetStartNode(advancedProcessor.NodeId);

// Test advanced visualization
var advancedTestScenarios = new[]
{
    new { Data = "Standard processing", Type = "standard" },
    new { Data = "Priority processing", Type = "priority" },
    new { Data = "Success processing", Type = "success" },
    new { Data = "Error processing", Type = "error" }
};

foreach (var scenario in advancedTestScenarios)
{
    var arguments = new KernelArguments
    {
        ["input_data"] = scenario.Data,
        ["processing_type"] = scenario.Type
    };

    Console.WriteLine($"🎨 Testing advanced visualization: {scenario.Data}");
    Console.WriteLine($"   Processing Type: {scenario.Type}");
    
    var result = await advancedVisualizationWorkflow.ExecuteAsync(kernel, arguments);
    
    var customVisualization = result.GetValue<Dictionary<string, object>>("custom_visualization");
    var nodeStyle = result.GetValue<string>("node_style");
    
    if (customVisualization != null)
    {
        var metadata = customVisualization["processing_metadata"] as dynamic;
        Console.WriteLine($"   Node Style: {nodeStyle}");
        Console.WriteLine($"   Style Color: {GetStyleColor(nodeStyle)}");
        Console.WriteLine($"   Export Files: styled_{DateTime.UtcNow:yyyyMMdd_HHmmss}.*");
    }
    
    Console.WriteLine();
}

// Helper methods for custom styling
string GetStyleColor(string style) => style switch
{
    "priority_style" => "#FF9800",
    "error_style" => "#F44336",
    "success_style" => "#4CAF50",
    _ => "#2196F3"
};

int GetStyleBorderWidth(string style) => style switch
{
    "priority_style" => 3,
    "error_style" => 2,
    "success_style" => 2,
    _ => 1
};

string GetStyleShape(string style) => style switch
{
    "priority_style" => "diamond",
    "error_style" => "octagon",
    "success_style" => "ellipse",
    _ => "box"
};
```

## Expected Output

### Basic Graph Visualization Example

```
📊 Exporting graph in different formats...
   DOT Export: 1,234 characters
   JSON Export: 2,345 characters
   Mermaid Export: 1,567 characters

🧪 Testing visualization workflow: Sample data 1
   Processing Step: output_generated
   Final Output: Final Output: Transformed: Processed: Sample data 1
```

### Real-Time Execution Visualization Example

```
🎬 Starting real-time visualization...
   Visualization will update every 500ms
   Exports will be saved to ./real-time-exports/
   Iteration 1: Data: Real-time processed: Real-time test data (iteration 1), Progress: 10%
   Iteration 2: Data: Real-time processed: Real-time test data (iteration 2), Progress: 20%
✅ Real-time visualization completed
```

### Interactive Graph Inspection Example

```
🔍 Testing interactive inspection: Normal processing
   Inspection Mode: normal
   Node State: completed
   Breakpoint Hit: False

🔍 Testing interactive inspection: Breakpoint processing
   Inspection Mode: breakpoint
   Node State: completed
   Breakpoint Hit: True
   Breakpoint Data: Breakpoint processing
   Pause Duration: 2 seconds
```

### Advanced Visualization Example

```
🎨 Testing advanced visualization: Priority processing
   Processing Type: priority
   Node Style: priority_style
   Style Color: #FF9800
   Export Files: styled_20250801_143022.*
```

## Configuration Options

### Visualization Configuration

```csharp
var visualizationOptions = new GraphVisualizationOptions
{
    EnableDOTExport = true,                           // Enable DOT format export
    EnableJSONExport = true,                          // Enable JSON format export
    EnableMermaidExport = true,                       // Enable Mermaid format export
    EnableRealTimeHighlights = true,                  // Enable real-time execution highlights
    EnableExecutionOverlays = true,                   // Enable execution flow overlays
    EnableInteractiveInspection = true,               // Enable interactive inspection
    EnableBreakpoints = true,                         // Enable execution breakpoints
    EnableExecutionPause = true,                      // Enable execution pausing
    EnableStepThrough = true,                         // Enable step-through execution
    EnableStateInspection = true,                     // Enable state inspection
    EnableNodeInspection = true,                      // Enable node-level inspection
    EnableCustomStyling = true,                       // Enable custom node/edge styling
    EnableThemeSupport = true,                        // Enable theme support
    EnableExportCompression = true,                   // Enable export compression
    EnableLiveUpdates = true,                         // Enable live visualization updates
    EnableExecutionTracking = true,                   // Enable execution path tracking
    EnableNodeStateHighlighting = true,               // Enable node state highlighting
    UpdateInterval = TimeSpan.FromMilliseconds(500),  // Update interval for real-time
    ExportPath = "./graph-exports",                   // Export directory path
    ExportFormats = new[] { "dot", "json", "mermaid", "svg", "png" }, // Supported formats
    CustomStyles = new Dictionary<string, string>     // Custom styling options
    {
        ["node_color"] = "#4CAF50",
        ["edge_color"] = "#2196F3",
        ["highlight_color"] = "#FF9800",
        ["error_color"] = "#F44336"
    }
};
```

### Real-Time Visualization Configuration

```csharp
var realTimeOptions = new RealTimeVisualizationOptions
{
    EnableLiveUpdates = true,                         // Enable live visualization updates
    UpdateInterval = TimeSpan.FromMilliseconds(500),  // Update frequency
    EnableExecutionTracking = true,                   // Track execution paths
    EnableNodeStateHighlighting = true,               // Highlight node states
    EnableProgressIndicators = true,                  // Show execution progress
    EnableTimelineView = true,                        // Show execution timeline
    EnablePerformanceMetrics = true,                  // Show performance metrics
    MaxHistorySize = 1000,                            // Maximum history size
    EnableAutoExport = true,                          // Auto-export on updates
    ExportOnCompletion = true,                        // Export when execution completes
    EnableAnimation = true,                           // Enable smooth animations
    AnimationDuration = TimeSpan.FromMilliseconds(300) // Animation duration
};
```

## Troubleshooting

### Common Issues

#### Visualization Not Working
```bash
# Problem: Graph visualization is not working
# Solution: Check visualization configuration and enable required features
EnableDOTExport = true;
EnableRealTimeHighlights = true;
ExportPath = "./valid-path";
```

#### Export Failures
```bash
# Problem: Graph export is failing
# Solution: Check export path and permissions
ExportPath = "./graph-exports";
Directory.CreateDirectory(ExportPath); // Ensure directory exists
```

#### Real-Time Updates Not Working
```bash
# Problem: Real-time updates are not working
# Solution: Enable real-time features and check update interval
EnableRealTimeHighlights = true;
EnableLiveUpdates = true;
UpdateInterval = TimeSpan.FromMilliseconds(500);
```

### Debug Mode

Enable detailed logging for troubleshooting:

```csharp
// Enable debug logging
var logger = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
}).CreateLogger<GraphVisualizationExample>();

// Configure visualization with debug logging
var debugVisualizationOptions = new GraphVisualizationOptions
{
    EnableDOTExport = true,
    EnableJSONExport = true,
    EnableRealTimeHighlights = true,
    EnableDebugLogging = true,
    LogVisualizationUpdates = true,
    LogExportOperations = true
};
```

## Advanced Patterns

### Custom Visualization Styles

```csharp
// Implement custom visualization styles
public class CustomVisualizationStyle : IVisualizationStyle
{
    public async Task<Dictionary<string, object>> ApplyStyleAsync(GraphNode node, GraphState state)
    {
        var customStyle = new Dictionary<string, object>();
        
        // Apply custom styling based on node type
        switch (node.NodeType)
        {
            case "FunctionGraphNode":
                customStyle["shape"] = "box";
                customStyle["color"] = "#4CAF50";
                customStyle["style"] = "filled";
                break;
            case "ConditionalGraphNode":
                customStyle["shape"] = "diamond";
                customStyle["color"] = "#2196F3";
                customStyle["style"] = "filled";
                break;
            case "ReActLoopGraphNode":
                customStyle["shape"] = "ellipse";
                customStyle["color"] = "#FF9800";
                customStyle["style"] = "filled";
                break;
        }
        
        // Apply state-based styling
        if (state.GetValue<bool>("is_error", false))
        {
            customStyle["color"] = "#F44336";
            customStyle["style"] = "filled,diagonals";
        }
        
        return customStyle;
    }
}
```

### Custom Export Formats

```csharp
// Implement custom export format
public class CustomExportFormat : IGraphExportFormat
{
    public string FormatName => "custom";
    public string FileExtension => ".custom";
    
    public async Task<string> ExportAsync(GraphExecutor executor, Dictionary<string, object> options = null)
    {
        var customExport = new StringBuilder();
        
        // Generate custom format
        customExport.AppendLine("CUSTOM GRAPH EXPORT");
        customExport.AppendLine("==================");
        customExport.AppendLine();
        
        foreach (var node in executor.Nodes)
        {
            customExport.AppendLine($"Node: {node.NodeId}");
            customExport.AppendLine($"  Type: {node.NodeType}");
            customExport.AppendLine($"  Description: {node.Description}");
            customExport.AppendLine();
        }
        
        return customExport.ToString();
    }
}
```

### Interactive Debugging

```csharp
// Implement interactive debugging
public class InteractiveDebugger : IGraphDebugger
{
    private readonly Dictionary<string, Breakpoint> _breakpoints = new();
    
    public async Task<bool> ShouldPauseAsync(GraphNode node, GraphState state)
    {
        if (_breakpoints.TryGetValue(node.NodeId, out var breakpoint))
        {
            return await breakpoint.EvaluateAsync(node, state);
        }
        return false;
    }
    
    public async Task<DebugAction> HandleBreakpointAsync(GraphNode node, GraphState state)
    {
        Console.WriteLine($"🔴 Breakpoint hit at node: {node.NodeId}");
        Console.WriteLine($"   Current state: {string.Join(", ", state.Keys)}");
        
        Console.WriteLine("Debug commands: [c]ontinue, [s]tep, [i]nspect, [q]uit");
        var command = Console.ReadLine()?.ToLower();
        
        return command switch
        {
            "c" => DebugAction.Continue,
            "s" => DebugAction.Step,
            "i" => await InspectStateAsync(state),
            "q" => DebugAction.Quit,
            _ => DebugAction.Continue
        };
    }
    
    private async Task<DebugAction> InspectStateAsync(GraphState state)
    {
        Console.WriteLine("📊 State inspection:");
        foreach (var kvp in state)
        {
            Console.WriteLine($"   {kvp.Key}: {kvp.Value}");
        }
        return DebugAction.Continue;
    }
}
```

## Related Examples

* [Graph Metrics](./graph-metrics.md): Metrics collection and monitoring
* [Debug and Inspection](./debug-inspection.md): Graph debugging techniques
* [Streaming Execution](./streaming-execution.md): Real-time execution monitoring
* [Performance Optimization](./performance-optimization.md): Using visualization for optimization

## See Also

* [Graph Visualization Concepts](../concepts/visualization.md): Understanding visualization concepts
* [Debug and Inspection](../how-to/debug-and-inspection.md): Debugging and inspection patterns
* [Performance Monitoring](../how-to/performance-monitoring.md): Performance visualization
* [API Reference](../api/): Complete API documentation
