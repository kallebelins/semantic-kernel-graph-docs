# Visualization and Realtime Highlighting

SemanticKernel.Graph provides comprehensive visualization capabilities with multi-format export support and real-time execution highlighting. This reference covers the complete visualization ecosystem including export formats, real-time highlighting, and execution overlays.

## GraphVisualizationEngine

The central component for generating visualizations in multiple formats with advanced styling and customization options.

### Constructor

```csharp
/// <summary>
/// Creates a new <see cref="GraphVisualizationEngine"/> with optional configuration
/// and an optional logger for diagnostics.
/// </summary>
/// <param name="options">Optional visualization options (null uses defaults).</param>
/// <param name="logger">Optional logger used to emit diagnostic messages.</param>
public GraphVisualizationEngine(GraphVisualizationOptions? options = null, IGraphLogger? logger = null)
{
    // Implementation note: if options is null, the engine should construct default options
    // to ensure predictable behavior. The logger is optional and may be null in tests.
}
```

**Parameters:**
* `options`: Visualization configuration options
* `logger`: Logger instance for diagnostics

### Methods

#### Graph Structure Serialization

```csharp
/// <summary>
/// Serializes the graph structure to DOT format for GraphViz rendering.
/// </summary>
/// <remarks>
/// The returned string is a complete DOT document. Use <see cref="DotSerializationOptions"/>
/// to control layout and styling.
/// </remarks>
public string SerializeToDot(GraphVisualizationData visualizationData, DotSerializationOptions? options = null)
{
    // Validate input and apply default options if necessary.
    // Implementation produces a DOT formatted string representing nodes and edges.
    throw new NotImplementedException();
}

/// <summary>
/// Serializes the graph structure to JSON format for API consumption.
/// </summary>
public string SerializeToJson(GraphVisualizationData visualizationData, JsonSerializationOptions? options = null)
{
    // Use a safe serializer with configured options (indentation, camelCase, depth limits).
    throw new NotImplementedException();
}

/// <summary>
/// Generates an enhanced Mermaid diagram with advanced styling and features.
/// </summary>
public string GenerateEnhancedMermaidDiagram(GraphVisualizationData visualizationData, MermaidGenerationOptions? options = null)
{
    // Convert graph structure into a Mermaid DSL string and apply any custom styles.
    throw new NotImplementedException();
}

/// <summary>
/// Serializes the graph structure to SVG format for web display.
/// </summary>
public string SerializeToSvg(GraphVisualizationData visualizationData, SvgSerializationOptions? options = null)
{
    // Produce an SVG string; consider performance for large graphs.
    throw new NotImplementedException();
}
```

**Parameters:**
* `visualizationData`: Graph visualization data containing nodes, edges, and execution state
* `options`: Format-specific serialization options

**Returns:** Formatted string representation in the requested format

### Supported Export Formats

#### DOT Format (GraphViz)
Professional-grade graph visualization format for GraphViz rendering with advanced layout options.

**Features:**
* Layout control (top-to-bottom, left-to-right, bottom-to-top, right-to-left)
* Node clustering and grouping
* Custom styling for nodes and edges
* Execution path highlighting
* Node type information display

**Example Output:**
```dot
digraph "My Workflow" {
    rankdir=TB;
    node [shape=box, style=rounded];
    
    "start" [shape=oval, label="Start", style="filled,bold", fillcolor="gold"];
    "process" [shape=box, label="Process Data", style="filled", fillcolor="lightgreen"];
    "end" [shape=oval, label="End"];
    
    "start" -> "process" [color="#32CD32", penwidth=2];
    "process" -> "end";
}
```

#### JSON Format
Structured data format for API consumption, data exchange, and programmatic visualization.

**Features:**
* Hierarchical representation of graph structure
* Metadata inclusion (timestamps, execution metrics, layout information)
* API-ready format for web and mobile consumption
* Extensible structure with custom properties
* Efficient serialization for large graphs

**Example Output:**
```json
{
  "metadata": {
    "generatedAt": "2025-08-15T10:30:00Z",
    "nodeCount": 3,
    "edgeCount": 2,
    "hasExecutionPath": true,
    "currentNodeId": "process"
  },
  "nodes": [
    {
      "id": "start",
      "name": "Start",
      "type": "FunctionGraphNode",
      "isExecutable": true,
      "isCurrentNode": false,
      "isInExecutionPath": true
    }
  ],
  "edges": [
    {
      "from": "start",
      "to": "process",
      "label": "success",
      "type": "directed"
    }
  ],
  "executionPath": [
    {
      "nodeId": "start",
      "nodeName": "Start",
      "order": 0
    }
  ]
}
```

#### Mermaid Format
Web-friendly diagram format with enhanced styling and real-time highlighting support.

**Features:**
* Multiple layout directions (TB, LR, BT, RL)
* Advanced styling with CSS classes
* Execution path highlighting
* Node type-based styling
* Performance indicators
* Multiple themes (Default, Dark, Forest, Base, Neutral)

**Example Output:**
```mermaid
graph TD
    %% Graph generated at 2025-08-15 10:30:00
    
    start((Start))
    process[Process Data]
    end((End))
    
    start -->|success| process
    process -->|completed| end
    
    %% Real-time highlight styles
    classDef currentNode fill:#FFD700,stroke:#FF8C00,stroke-width:3px
    classDef executedNode fill:#90EE90,stroke:#32CD32,stroke-width:2px
    classDef pendingNode fill:#F0F0F0,stroke:#808080
    
    class process currentNode
    class start executedNode
```

#### SVG Format
Vector graphics format for web display and static image generation.

**Features:**
* Configurable canvas dimensions
* Customizable node spacing
* Metrics overlay display
* Execution path highlighting
* Current node emphasis

## GraphRealtimeHighlighter

Live execution path tracking with visual highlights and real-time updates.

### Constructor

```csharp
/// <summary>
/// Initializes a new instance of the <see cref="GraphRealtimeHighlighter"/>,
/// optionally subscribing to an execution event stream for automatic updates.
/// </summary>
/// <param name="eventStream">Optional source of execution events used to drive highlights.</param>
/// <param name="options">Optional configuration for highlighting behavior.</param>
/// <param name="logger">Optional logger for diagnostics.</param>
public GraphRealtimeHighlighter(
    IGraphExecutionEventStream? eventStream = null,
    GraphRealtimeHighlightOptions? options = null,
    IGraphLogger? logger = null)
{
    // Subscribe to eventStream if provided and apply options with sensible defaults.
}
```

**Parameters:**
* `eventStream`: Optional event stream for automatic updates
* `options`: Highlighting configuration options
* `logger`: Logger instance for diagnostics

### Methods

#### Execution Highlighting

```csharp
/// <summary>
/// Starts highlighting for a specific execution.
/// </summary>
public void StartHighlighting(string executionId, GraphVisualizationData visualizationData)
{
    // Guard clauses: validate executionId and visualizationData before proceeding.
}

/// <summary>
/// Updates the current node for an execution.
/// </summary>
public void UpdateCurrentNode(string executionId, IGraphNode currentNode, IReadOnlyList<IGraphNode> executionPath)
{
    // Update internal state and raise events to notify subscribers of the change.
}

/// <summary>
/// Adds completion highlight for a node.
/// </summary>
public void AddNodeCompletionHighlight(string executionId, IGraphNode node, bool success, TimeSpan executionTime)
{
    // Record completion metrics and update highlight styles accordingly.
}

/// <summary>
/// Stops highlighting for a specific execution.
/// </summary>
public void StopHighlighting(string executionId)
{
    // Clean up resources and unsubscribe from event streams related to this execution.
}
```

#### Highlighted Visualization Generation

```csharp
/// <summary>
/// Generates a string representation of the highlighted visualization for the given
/// execution and format (Mermaid, JSON, DOT, SVG).
/// </summary>
public string GenerateHighlightedVisualization(string executionId, HighlightVisualizationFormat format)
{
    // Select formatter based on <paramref name="format"/> and return the generated string.
    throw new NotImplementedException();
}
```

**Supported Formats:**
* `HighlightVisualizationFormat.Mermaid`: Enhanced Mermaid with highlights
* `HighlightVisualizationFormat.Json`: JSON with execution state
* `HighlightVisualizationFormat.Dot`: DOT with execution path
* `HighlightVisualizationFormat.Svg`: SVG with real-time overlays

#### Event Handling

```csharp
/// <summary>
/// Event raised when node highlighting changes. Subscribers should handle minimal work
/// and avoid blocking the caller; consider using a background queue for heavy work.
/// </summary>
public event EventHandler<NodeHighlightEventArgs>? NodeHighlightChanged;

/// <summary>
/// Event raised when execution path is updated.
/// </summary>
public event EventHandler<ExecutionPathUpdatedEventArgs>? ExecutionPathUpdated;
```

### Highlight Styles

#### NodeHighlightStyle
```csharp
/// <summary>
/// Represents visual styling options applied to a node when highlighted.
/// All color values are expected to be valid CSS color strings (e.g. hex codes).
/// </summary>
public sealed class NodeHighlightStyle
{
    /// <summary>Fill color used for the node background.</summary>
    public string FillColor { get; set; } = "#FFFFFF";

    /// <summary>Stroke color used for the node border.</summary>
    public string StrokeColor { get; set; } = "#000000";

    /// <summary>Width of the stroke in pixels.</summary>
    public int StrokeWidth { get; set; } = 1;

    /// <summary>Opacity value between 0.0 (transparent) and 1.0 (opaque).</summary>
    public double Opacity { get; set; } = 1.0;

    /// <summary>Optional CSS border style (e.g. "dashed").</summary>
    public string? BorderStyle { get; set; }
}
```

#### EdgeHighlightStyle
```csharp
/// <summary>
/// Represents visual styling options applied to an edge when highlighted.
/// </summary>
public sealed class EdgeHighlightStyle
{
    /// <summary>Stroke color for the edge.</summary>
    public string StrokeColor { get; set; } = "#000000";

    /// <summary>Stroke width in pixels.</summary>
    public int StrokeWidth { get; set; } = 1;

    /// <summary>Opacity value between 0.0 and 1.0.</summary>
    public double Opacity { get; set; } = 1.0;
}
```

## Configuration Options

### GraphVisualizationOptions

```csharp
/// <summary>
/// Global options used by <see cref="GraphVisualizationEngine"/> to control
/// theme, caching, and performance-related behavior.
/// </summary>
public sealed class GraphVisualizationOptions
{
    public VisualizationTheme Theme { get; set; } = VisualizationTheme.Default;
    public bool EnableRealtimeUpdates { get; set; } = true;
    public bool EnableCaching { get; set; } = true;
    public int MaxCacheSize { get; set; } = 100;
    public int CacheExpirationMinutes { get; set; } = 30;
    public bool IncludePerformanceMetrics { get; set; } = true;
    public int MaxNodesPerVisualization { get; set; } = 1000;
    public bool EnableAdvancedStyling { get; set; } = true;
}
```

### GraphRealtimeHighlightOptions

```csharp
/// <summary>
/// Options that control the behavior of real-time highlighting, including
/// update frequency and resource limits.
/// </summary>
public sealed class GraphRealtimeHighlightOptions
{
    /// <summary>If true, updates will be applied immediately without batching.</summary>
    public bool EnableImmediateUpdates { get; set; } = true;

    /// <summary>Interval used for batched updates when immediate updates are disabled.</summary>
    public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromMilliseconds(100);

    public bool EnableAnimations { get; set; } = true;
    public bool EnablePerformanceTracking { get; set; } = true;

    /// <summary>Maximum number of nodes that can be highlighted at the same time.</summary>
    public int MaxHighlightedNodes { get; set; } = 100;
}
```

### Format-Specific Options

#### DotSerializationOptions
```csharp
/// <summary>
/// Options to customize DOT serialization (GraphViz). Use <see cref="CustomNodeStyles"/>
/// and <see cref="CustomEdgeStyles"/> to inject additional style directives per type.
/// </summary>
public sealed class DotSerializationOptions
{
    public string GraphName { get; set; } = "SemanticKernelGraph";
    public bool EnableClustering { get; set; } = false;
    public bool HighlightExecutionPath { get; set; } = true;
    public bool HighlightCurrentNode { get; set; } = true;
    public DotLayoutDirection LayoutDirection { get; set; } = DotLayoutDirection.TopToBottom;
    public bool IncludeNodeTypeInfo { get; set; } = true;
    public Dictionary<string, string> CustomNodeStyles { get; set; } = new();
    public Dictionary<string, string> CustomEdgeStyles { get; set; } = new();
}
```

#### JsonSerializationOptions
```csharp
/// <summary>
/// Options controlling JSON serialization behavior used by the visualization APIs.
/// </summary>
public sealed class JsonSerializationOptions
{
    public bool Indented { get; set; } = true;
    public bool UseCamelCase { get; set; } = true;
    public bool IncludeNodeProperties { get; set; } = true;
    public bool IncludeLayoutInfo { get; set; } = true;
    public bool IncludeExecutionMetrics { get; set; } = false;
    public bool IncludeTimestamps { get; set; } = true;
    public int MaxSerializationDepth { get; set; } = 10;
}
```

#### MermaidGenerationOptions
```csharp
/// <summary>
/// Options for generating Mermaid diagrams. The <see cref="Direction"/> property
/// controls layout (e.g. "TD" for top-down).
/// </summary>
public sealed class MermaidGenerationOptions
{
    public string Direction { get; set; } = "TD";
    public bool IncludeTitle { get; set; } = true;
    public bool EnableStyling { get; set; } = true;
    public bool HighlightExecutionPath { get; set; } = true;
    public bool HighlightCurrentNode { get; set; } = true;
    public bool StyleByNodeType { get; set; } = true;
    public bool IncludePerformanceIndicators { get; set; } = false;
    public MermaidTheme Theme { get; set; } = MermaidTheme.Default;
    public Dictionary<string, string> CustomStyles { get; set; } = new();
}
```

#### SvgSerializationOptions
```csharp
/// <summary>
/// Options to control SVG output size and layout spacing used by the SVG serializer.
/// </summary>
public sealed class SvgSerializationOptions
{
    public int Width { get; set; } = 960;
    public int Height { get; set; } = 540;
    public int HorizontalSpacing { get; set; } = 180;
    public int VerticalSpacing { get; set; } = 120;
    public bool IncludeMetricsOverlay { get; set; } = true;
    public bool HighlightExecutionPath { get; set; } = true;
    public bool HighlightCurrentNode { get; set; } = true;
}
```

## Data Structures

### GraphVisualizationData

```csharp
/// <summary>
/// Immutable container representing the data required to render a graph visualization.
/// </summary>
public sealed class GraphVisualizationData
{
    /// <summary>List of nodes that compose the graph.</summary>
    public IReadOnlyList<IGraphNode> Nodes { get; }

    /// <summary>List of edges connecting nodes.</summary>
    public IReadOnlyList<GraphEdgeInfo> Edges { get; }

    /// <summary>The current node in execution, if any.</summary>
    public IGraphNode? CurrentNode { get; }

    /// <summary>Ordered list representing the execution path.</summary>
    public IReadOnlyList<IGraphNode> ExecutionPath { get; }

    /// <summary>Timestamp indicating when this visualization data was generated.</summary>
    public DateTimeOffset GeneratedAt { get; } = DateTimeOffset.UtcNow;
}
```

### GraphEdgeInfo

```csharp
/// <summary>
/// Lightweight representation of an edge used by the visualization system.
/// </summary>
public sealed class GraphEdgeInfo
{
    /// <summary>Source node identifier.</summary>
    public string FromNodeId { get; }

    /// <summary>Destination node identifier.</summary>
    public string ToNodeId { get; }

    /// <summary>Optional label for the edge (e.g. condition name).</summary>
    public string? Label { get; }

    /// <summary>Optional condition expression associated with this edge.</summary>
    public string? Condition { get; }

    /// <summary>Indicates whether this edge should be visually highlighted.</summary>
    public bool IsHighlighted { get; set; }
}
```

## Enumerations

### HighlightVisualizationFormat
```csharp
/// <summary>
/// Supported formats for highlighted visualization exports.
/// </summary>
public enum HighlightVisualizationFormat
{
    Mermaid,    // Mermaid diagram format
    Json,       // JSON format
    Dot,        // DOT format for GraphViz
    Svg         // SVG format
}
```

### DotLayoutDirection
```csharp
/// <summary>
/// Layout directions used by DOT serializer.
/// </summary>
public enum DotLayoutDirection
{
    TopToBottom,    // TD - Top to bottom layout
    LeftToRight,    // LR - Left to right layout
    BottomToTop,    // BT - Bottom to top layout
    RightToLeft     // RL - Right to left layout
}
```

### VisualizationTheme
```csharp
public enum VisualizationTheme
{
    Default,        // Default theme
    Dark,           // Dark theme
    Light,          // Light theme
    HighContrast,   // High contrast theme for accessibility
    Professional,   // Professional theme for business presentations
    Colorful        // Colorful theme with vibrant colors
}
```

### MermaidTheme
```csharp
public enum MermaidTheme
{
    Default,    // Default Mermaid theme
    Dark,       // Dark theme
    Forest,     // Forest theme with green colors
    Base,       // Base theme with neutral colors
    Neutral     // Neutral theme
}
```

## Usage Examples

### Basic Visualization Setup

```csharp
using SemanticKernel.Graph.Core;

// Create visualization engine with options
var visualizationOptions = new GraphVisualizationOptions
{
    Theme = VisualizationTheme.Professional,
    EnableCaching = true,
    IncludePerformanceMetrics = true
};

var visualizationEngine = new GraphVisualizationEngine(visualizationOptions);

// Generate visualizations in different formats
var dotGraph = visualizationEngine.SerializeToDot(visualizationData);
var jsonGraph = visualizationEngine.SerializeToJson(visualizationData);
var mermaidDiagram = visualizationEngine.GenerateEnhancedMermaidDiagram(visualizationData);
```

### Real-time Highlighting Setup

```csharp
using SemanticKernel.Graph.Core;

// Create real-time highlighter
var highlightOptions = new GraphRealtimeHighlightOptions
{
    UpdateInterval = TimeSpan.FromMilliseconds(100),
    EnableAnimations = true,
    EnablePerformanceTracking = true
};

var highlighter = new GraphRealtimeHighlighter(
    eventStream: executionEventStream,
    options: highlightOptions
);

// Start highlighting for an execution
highlighter.StartHighlighting(executionId, visualizationData);

// Update current execution state
highlighter.UpdateCurrentNode(executionId, currentNode, executionPath);

// Generate highlighted visualization
var highlightedMermaid = highlighter.GenerateHighlightedVisualization(
    executionId, 
    HighlightVisualizationFormat.Mermaid
);
```

### Custom Export Options

```csharp
// DOT export with custom styling
var dotOptions = new DotSerializationOptions
{
    GraphName = "My Workflow",
    LayoutDirection = DotLayoutDirection.LeftToRight,
    EnableClustering = true,
    HighlightExecutionPath = true,
    HighlightCurrentNode = true,
    CustomNodeStyles = new Dictionary<string, string>
    {
        ["FunctionGraphNode"] = "shape=box, style=filled, fillcolor=lightblue",
        ["ConditionalGraphNode"] = "shape=diamond, style=filled, fillcolor=lightyellow"
    }
};

var dotOutput = visualizationEngine.SerializeToDot(visualizationData, dotOptions);

// Mermaid export with custom theme
var mermaidOptions = new MermaidGenerationOptions
{
    Direction = "TB",
    IncludeTitle = true,
    EnableStyling = true,
    HighlightExecutionPath = true,
    Theme = MermaidTheme.Professional,
    CustomStyles = new Dictionary<string, string>
    {
        [".currentNode"] = "fill:#FFD700;stroke:#FF8C00;stroke-width:3px",
        [".executedNode"] = "fill:#90EE90;stroke:#32CD32;stroke-width:2px"
    }
};

var mermaidOutput = visualizationEngine.GenerateEnhancedMermaidDiagram(
    visualizationData, 
    mermaidOptions
);
```

## Performance Considerations

* **Caching**: Enable visualization caching for repeated exports
* **Batch Updates**: Use appropriate update intervals for real-time highlighting
* **Node Limits**: Configure maximum nodes per visualization for large graphs
* **Memory Management**: Dispose of visualization engines when no longer needed
* **Async Operations**: Use async methods for large graph processing

## Integration Points

* **GraphInspectionApi**: Runtime inspection and visualization data generation
* **GraphMetricsExporter**: Performance metrics integration with visualizations
* **GraphDocumentationGenerator**: Automatic documentation with visual diagrams
* **Event Streams**: Real-time updates from graph execution events
* **Debug Sessions**: Visualization during debugging and development

## See Also

* [Real-time Visualization and Highlights](../how-to/real-time-visualization-and-highlights.md) - Guide for implementing real-time visualization
* [Debug and Inspection](../how-to/debug-and-inspection.md) - Using visualization for debugging
* [GraphVisualizationExample](../examples/graph-visualization-example.md) - Complete example implementation
* [GraphInspectionApi](inspection-visualization.md) - Runtime inspection and visualization API
