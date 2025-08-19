# Graph Visualization Example

This example demonstrates how to visualize and inspect graph structures using the Semantic Kernel Graph's visualization capabilities. It shows how to export graphs in various formats, create real-time visualizations, and implement interactive graph inspection.

## Objective

Learn how to implement graph visualization and inspection in graph-based workflows to:
- Export graphs in multiple formats (DOT, JSON, Mermaid)
- Create real-time visualizations with execution highlights
- Implement interactive graph inspection and debugging
- Generate visual representations for documentation and analysis
- Monitor graph execution with visual feedback

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Visualization Concepts](../concepts/visualization.md)

## Key Components

### Concepts and Techniques

- **Graph Visualization**: Converting graph structures to visual representations
- **Export Formats**: Supporting multiple visualization formats (DOT, JSON, Mermaid)
- **Real-Time Highlights**: Visual feedback during graph execution
- **Interactive Inspection**: Debugging and analyzing graph structures
- **Execution Overlays**: Visual representation of execution flow

### Core Classes

- `GraphVisualizationEngine`: Core visualization engine
- `GraphRealtimeHighlighter`: Real-time execution highlighting
- `GraphInspectionApi`: Interactive graph inspection
- `GraphVisualizationOptions`: Configuration for visualization

## Running the Example

### Getting Started

This example demonstrates graph visualization and export capabilities with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Basic Graph Visualization

This example demonstrates basic graph export and visualization capabilities.

```csharp
// Create kernel with mock configuration
var kernel = CreateKernel();

// Create visualization-enabled workflow
var visualizationWorkflow = new GraphExecutor("VisualizationWorkflow", "Basic graph visualization", logger);

// Configure visualization options
var visualizationOptions = new GraphVisualizationOptions
{
    EnableDOTExport = true,
    EnableJSONExport = true,
    EnableMermaidExport = true,
    EnableRealTimeHighlights = true,
    EnableExecutionOverlays = true,
    ExportPath = "./graph-exports"
};

visualizationWorkflow.ConfigureVisualization(visualizationOptions);

// Sample processing nodes
var inputProcessor = new FunctionGraphNode(
    "input-processor",
    "Process input data",
    async (context) =>
    {
        var inputData = context.GetValue<string>("input_data");
        var processedData = $"Processed: {inputData}";
        
        context.SetValue("processed_data", processedData);
        context.SetValue("processing_step", "input_processed");
        
        return processedData;
    });

var dataTransformer = new FunctionGraphNode(
    "data-transformer",
    "Transform processed data",
    async (context) =>
    {
        var processedData = context.GetValue<string>("processed_data");
        var transformedData = $"Transformed: {processedData.ToUpper()}";
        
        context.SetValue("transformed_data", transformedData);
        context.SetValue("processing_step", "data_transformed");
        
        return transformedData;
    });

var outputGenerator = new FunctionGraphNode(
    "output-generator",
    "Generate final output",
    async (context) =>
    {
        var transformedData = context.GetValue<string>("transformed_data");
        var finalOutput = $"Final Output: {transformedData}";
        
        context.SetValue("final_output", finalOutput);
        context.SetValue("processing_step", "output_generated");
        
        return finalOutput;
    });

// Add nodes to workflow
visualizationWorkflow.AddNode(inputProcessor);
visualizationWorkflow.AddNode(dataTransformer);
visualizationWorkflow.AddNode(outputGenerator);

// Set start node
visualizationWorkflow.SetStartNode(inputProcessor.NodeId);

// Export graph in different formats
Console.WriteLine("📊 Exporting graph in different formats...");

// DOT format export
var dotExport = await visualizationWorkflow.ExportToDOTAsync();
Console.WriteLine($"   DOT Export: {dotExport.Length} characters");
File.WriteAllText("./graph-exports/workflow.dot", dotExport);

// JSON format export
var jsonExport = await visualizationWorkflow.ExportToJSONAsync();
Console.WriteLine($"   JSON Export: {jsonExport.Length} characters");
File.WriteAllText("./graph-exports/workflow.json", jsonExport);

// Mermaid format export
var mermaidExport = await visualizationWorkflow.ExportToMermaidAsync();
Console.WriteLine($"   Mermaid Export: {mermaidExport.Length} characters");
File.WriteAllText("./graph-exports/workflow.mmd", mermaidExport);

// Test visualization workflow
var testData = new[]
{
    "Sample data 1",
    "Sample data 2",
    "Sample data 3"
};

foreach (var data in testData)
{
    var arguments = new KernelArguments
    {
        ["input_data"] = data
    };

    Console.WriteLine($"🧪 Testing visualization workflow: {data}");
    var result = await visualizationWorkflow.ExecuteAsync(kernel, arguments);
    
    var processingStep = result.GetValue<string>("processing_step");
    var finalOutput = result.GetValue<string>("final_output");
    
    Console.WriteLine($"   Processing Step: {processingStep}");
    Console.WriteLine($"   Final Output: {finalOutput}");
    Console.WriteLine();
}
```

### 2. Real-Time Execution Visualization

Demonstrates real-time visualization with execution highlights and overlays.

```csharp
// Create real-time visualization workflow
var realTimeVisualizationWorkflow = new GraphExecutor("RealTimeVisualizationWorkflow", "Real-time execution visualization", logger);

// Configure real-time visualization
var realTimeVisualizationOptions = new GraphVisualizationOptions
{
    EnableRealTimeHighlights = true,
    EnableExecutionOverlays = true,
    EnableLiveUpdates = true,
    UpdateInterval = TimeSpan.FromMilliseconds(500),
    EnableExecutionTracking = true,
    EnableNodeStateHighlighting = true,
    ExportPath = "./real-time-exports"
};

realTimeVisualizationWorkflow.ConfigureVisualization(realTimeVisualizationOptions);

// Real-time data processor
var realTimeProcessor = new FunctionGraphNode(
    "real-time-processor",
    "Process data with real-time visualization",
    async (context) =>
    {
        var inputData = context.GetValue<string>("input_data");
        var iteration = context.GetValue<int>("iteration", 0);
        
        // Simulate processing with delays
        await Task.Delay(Random.Shared.Next(100, 300));
        
        var processedData = $"Real-time processed: {inputData} (iteration {iteration + 1})";
        
        context.SetValue("processed_data", processedData);
        context.SetValue("iteration", iteration + 1);
        context.SetValue("processing_timestamp", DateTime.UtcNow);
        context.SetValue("node_state", "completed");
        
        return processedData;
    });

// Real-time visualizer
var realTimeVisualizer = new FunctionGraphNode(
    "real-time-visualizer",
    "Update real-time visualization",
    async (context) =>
    {
        var processedData = context.GetValue<string>("processed_data");
        var iteration = context.GetValue<int>("iteration");
        var timestamp = context.GetValue<DateTime>("processing_timestamp");
        var nodeState = context.GetValue<string>("node_state");
        
        // Update real-time visualization
        var visualizationUpdate = new Dictionary<string, object>
        {
            ["current_iteration"] = iteration,
            ["last_processed_data"] = processedData,
            ["last_timestamp"] = timestamp,
            ["node_states"] = new Dictionary<string, string>
            {
                ["real-time-processor"] = nodeState,
                ["real-time-visualizer"] = "active"
            },
            ["execution_progress"] = (double)iteration / 10.0, // Assuming 10 iterations
            ["visualization_updated"] = true
        };
        
        context.SetValue("visualization_update", visualizationUpdate);
        
        // Export current state
        var currentDOT = await realTimeVisualizationWorkflow.ExportToDOTAsync();
        var currentJSON = await realTimeVisualizationWorkflow.ExportToJSONAsync();
        
        File.WriteAllText($"./real-time-exports/iteration_{iteration}.dot", currentDOT);
        File.WriteAllText($"./real-time-exports/iteration_{iteration}.json", currentJSON);
        
        return $"Visualization updated for iteration {iteration}";
    });

// Add nodes to real-time workflow
realTimeVisualizationWorkflow.AddNode(realTimeProcessor);
realTimeVisualizationWorkflow.AddNode(realTimeVisualizer);

// Set start node
realTimeVisualizationWorkflow.SetStartNode(realTimeProcessor.NodeId);

// Test real-time visualization
Console.WriteLine("🎬 Starting real-time visualization...");
Console.WriteLine("   Visualization will update every 500ms");
Console.WriteLine("   Exports will be saved to ./real-time-exports/");

var realTimeArguments = new KernelArguments
{
    ["input_data"] = "Real-time test data",
    ["iteration"] = 0
};

// Run real-time visualization for several iterations
for (int i = 0; i < 5; i++)
{
    var result = await realTimeVisualizationWorkflow.ExecuteAsync(kernel, realTimeArguments);
    
    var visualizationUpdate = result.GetValue<Dictionary<string, object>>("visualization_update");
    var iteration = result.GetValue<int>("iteration");
    
    if (visualizationUpdate != null)
    {
        Console.WriteLine($"   Iteration {iteration}: " +
                         $"Data: {visualizationUpdate["last_processed_data"]}, " +
                         $"Progress: {visualizationUpdate["execution_progress"]:P0}");
    }
    
    // Update arguments for next iteration
    realTimeArguments["iteration"] = iteration;
    
    await Task.Delay(1000); // Wait 1 second between iterations
}

Console.WriteLine("✅ Real-time visualization completed");
```

### 3. Interactive Graph Inspection

Shows how to implement interactive graph inspection and debugging capabilities.

```csharp
// Create interactive inspection workflow
var interactiveInspectionWorkflow = new GraphExecutor("InteractiveInspectionWorkflow", "Interactive graph inspection", logger);

// Configure interactive inspection
var interactiveInspectionOptions = new GraphVisualizationOptions
{
    EnableInteractiveInspection = true,
    EnableBreakpoints = true,
    EnableExecutionPause = true,
    EnableStepThrough = true,
    EnableStateInspection = true,
    EnableNodeInspection = true,
    ExportPath = "./interactive-exports"
};

interactiveInspectionWorkflow.ConfigureVisualization(interactiveInspectionOptions);

// Interactive processing node
var interactiveProcessor = new FunctionGraphNode(
    "interactive-processor",
    "Process with interactive inspection",
    async (context) =>
    {
        var inputData = context.GetValue<string>("input_data");
        var inspectionMode = context.GetValue<string>("inspection_mode", "normal");
        
        // Check for breakpoint
        if (inspectionMode == "breakpoint")
        {
            context.SetValue("breakpoint_hit", true);
            context.SetValue("breakpoint_data", inputData);
            context.SetValue("node_state", "paused");
            
            // Simulate breakpoint pause
            await Task.Delay(2000);
        }
        
        var processedData = $"Interactive processed: {inputData}";
        
        context.SetValue("processed_data", processedData);
        context.SetValue("processing_step", "interactive_processed");
        context.SetValue("node_state", "completed");
        
        return processedData;
    });

// Interactive inspector
var interactiveInspector = new FunctionGraphNode(
    "interactive-inspector",
    "Provide interactive inspection capabilities",
    async (context) =>
    {
        var processedData = context.GetValue<string>("processed_data");
        var inspectionMode = context.GetValue<string>("inspection_mode");
        var breakpointHit = context.GetValue<bool>("breakpoint_hit", false);
        
        // Interactive inspection logic
        var inspectionResults = new Dictionary<string, object>
        {
            ["node_id"] = "interactive-processor",
            ["input_data"] = context.GetValue<string>("input_data"),
            ["processed_data"] = processedData,
            ["inspection_mode"] = inspectionMode,
            ["breakpoint_hit"] = breakpointHit,
            ["node_state"] = context.GetValue<string>("node_state"),
            ["execution_time"] = DateTime.UtcNow,
            ["inspection_available"] = true
        };
        
        if (breakpointHit)
        {
            inspectionResults["breakpoint_data"] = context.GetValue<string>("breakpoint_data");
            inspectionResults["pause_duration"] = "2 seconds";
        }
        
        context.SetValue("inspection_results", inspectionResults);
        
        return $"Interactive inspection completed for {inspectionMode} mode";
    });

// Add nodes to interactive workflow
interactiveInspectionWorkflow.AddNode(interactiveProcessor);
interactiveInspectionWorkflow.AddNode(interactiveInspector);

// Set start node
interactiveInspectionWorkflow.SetStartNode(interactiveProcessor.NodeId);

// Test interactive inspection
var inspectionTestScenarios = new[]
{
    new { Data = "Normal processing", Mode = "normal" },
    new { Data = "Breakpoint processing", Mode = "breakpoint" },
    new { Data = "Step-through processing", Mode = "step" }
};

foreach (var scenario in inspectionTestScenarios)
{
    var arguments = new KernelArguments
    {
        ["input_data"] = scenario.Data,
        ["inspection_mode"] = scenario.Mode
    };

    Console.WriteLine($"🔍 Testing interactive inspection: {scenario.Data}");
    Console.WriteLine($"   Inspection Mode: {scenario.Mode}");
    
    var result = await interactiveInspectionWorkflow.ExecuteAsync(kernel, arguments);
    
    var inspectionResults = result.GetValue<Dictionary<string, object>>("inspection_results");
    var breakpointHit = result.GetValue<bool>("breakpoint_hit", false);
    
    if (inspectionResults != null)
    {
        Console.WriteLine($"   Node State: {inspectionResults["node_state"]}");
        Console.WriteLine($"   Breakpoint Hit: {breakpointHit}");
        
        if (breakpointHit)
        {
            Console.WriteLine($"   Breakpoint Data: {inspectionResults["breakpoint_data"]}");
            Console.WriteLine($"   Pause Duration: {inspectionResults["pause_duration"]}");
        }
    }
    
    Console.WriteLine();
}
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

- [Graph Metrics](./graph-metrics.md): Metrics collection and monitoring
- [Debug and Inspection](./debug-inspection.md): Graph debugging techniques
- [Streaming Execution](./streaming-execution.md): Real-time execution monitoring
- [Performance Optimization](./performance-optimization.md): Using visualization for optimization

## See Also

- [Graph Visualization Concepts](../concepts/visualization.md): Understanding visualization concepts
- [Debug and Inspection](../how-to/debug-and-inspection.md): Debugging and inspection patterns
- [Performance Monitoring](../how-to/performance-monitoring.md): Performance visualization
- [API Reference](../api/): Complete API documentation
