# Inspection and Visualization APIs Reference

This reference documents the inspection and visualization APIs in SemanticKernel.Graph, which enable real-time monitoring, debugging, and visual representation of graph execution.

## GraphInspectionApi

Runtime API for inspecting graph structure, execution state, and performance metrics. Provides endpoints for monitoring, debugging, and analyzing graph execution in real-time.

### Features

- Real-time graph structure inspection
- Execution state monitoring
- Performance metrics access
- Node and edge details retrieval
- Execution path tracking
- Health checks and status monitoring
- Custom query capabilities

### Constructors

```csharp
public GraphInspectionApi(GraphInspectionOptions? options = null, IGraphLogger? logger = null)
```

### Methods

#### Execution Context Management

```csharp
public void RegisterExecution(GraphExecutionContext executionContext, GraphPerformanceMetrics? performanceMetrics = null)
public void UnregisterExecution(string executionId)
public void RegisterDebugSession(string executionId, IDebugSession debugSession)
public void UnregisterDebugSession(string executionId)
```

#### Structure Inspection Endpoints

```csharp
public GraphInspectionResponse GetGraphStructure(string executionId, InspectionFormat format = InspectionFormat.Json)
public GraphInspectionResponse GetNodeDetails(string executionId, string nodeId)
public GraphInspectionResponse GetActiveExecutions()
```

#### Performance Metrics Endpoints

```csharp
public GraphInspectionResponse GetPerformanceMetrics(string executionId, TimeSpan? timeWindow = null, MetricsExportFormat format = MetricsExportFormat.Json)
public GraphInspectionResponse GetPerformanceHeatmap(string executionId)
```

#### Real-time Monitoring Endpoints

```csharp
public GraphInspectionResponse GetExecutionStatus(string executionId)
public GraphInspectionResponse GetExecutionHealth(string executionId)
public GraphInspectionResponse GetExecutionSummary(string executionId)
```

#### Debug and Analysis Endpoints

```csharp
public GraphInspectionResponse GetDebugSessionInfo(string executionId)
public GraphInspectionResponse GetExecutionTrace(string executionId, int maxSteps = 100)
public GraphInspectionResponse GetErrorAnalysis(string executionId)
```

### Properties

- `IsDisposed`: Gets whether the API has been disposed
- `ActiveExecutionCount`: Gets the number of active executions being monitored

## GraphVisualizationEngine

Comprehensive graph visualization engine for serialization and export to multiple formats. Provides methods for generating DOT, JSON, Mermaid, and other visualization formats.

### Supported Formats

- **DOT (GraphViz)**: For professional graph visualizations
- **JSON**: For API consumption and data exchange
- **Mermaid**: For documentation and web-based diagrams
- **SVG**: For static image exports

### Features

- Real-time execution path highlighting
- Performance metrics integration
- Customizable styling and themes
- Multi-format export capabilities

### Constructors

```csharp
public GraphVisualizationEngine(GraphVisualizationOptions? options = null, IGraphLogger? logger = null)
```

### Methods

#### Graph Structure Serialization

```csharp
public string SerializeToDot(GraphVisualizationData visualizationData, DotSerializationOptions? options = null)
public string SerializeToSvg(GraphVisualizationData visualizationData, SvgSerializationOptions? options = null)
public string SerializeToJson(GraphVisualizationData visualizationData, JsonSerializationOptions? options = null)
public string GenerateEnhancedMermaidDiagram(GraphVisualizationData visualizationData, MermaidGenerationOptions? options = null)
```

#### Metrics Integration

```csharp
public string ExportMetricsForVisualization(GraphPerformanceMetrics performanceMetrics, MetricsExportFormat format = MetricsExportFormat.Json)
public string GeneratePerformanceHeatmap(GraphPerformanceMetrics performanceMetrics, GraphVisualizationData visualizationData)
```

#### Real-time Highlighting

```csharp
public GraphVisualizationData UpdateRealtimeHighlights(GraphVisualizationData visualizationData, GraphExecutionContext currentExecutionContext)
```

## GraphInspectionOptions

Configuration options for the graph inspection API.

### Properties

```csharp
public int MaxActiveExecutions { get; set; } = 100;           // Maximum active executions to monitor
public bool EnableDetailedNodeInspection { get; set; } = true; // Enable detailed node inspection
public bool EnablePerformanceMetrics { get; set; } = true;     // Enable performance metrics collection
public bool EnableRealtimeMonitoring { get; set; } = true;     // Enable real-time monitoring
public bool EnableDebugSessions { get; set; } = true;          // Enable debug session support
public TimeSpan MetricsRetentionPeriod { get; set; } = TimeSpan.FromHours(24); // Metrics retention period
public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow; // API start time
public bool IncludeDebugInfo { get; set; } = true;             // Include debug information in responses
public bool IncludePerformanceHeatmaps { get; set; } = true;   // Include performance heatmaps
```

## GraphVisualizationOptions

Configuration options for graph visualization engine.

### Properties

```csharp
public VisualizationTheme Theme { get; set; } = VisualizationTheme.Default;  // Default visualization theme
public bool EnableRealtimeUpdates { get; set; } = true;                       // Enable real-time updates
public bool EnableCaching { get; set; } = true;                               // Enable caching
public int MaxCacheSize { get; set; } = 100;                                  // Maximum cache size
public int CacheExpirationMinutes { get; set; } = 30;                         // Cache expiration time
public bool IncludePerformanceMetrics { get; set; } = true;                   // Include performance metrics
public int MaxNodesPerVisualization { get; set; } = 1000;                     // Maximum nodes per visualization
public bool EnableAdvancedStyling { get; set; } = true;                       // Enable advanced styling
```

## Serialization Options

### DotSerializationOptions

Options specific to DOT format serialization.

```csharp
public string GraphName { get; set; } = "SemanticKernelGraph";  // Graph name in DOT output
public bool EnableClustering { get; set; } = false;             // Enable node clustering
public bool HighlightExecutionPath { get; set; } = true;        // Highlight execution path
public bool HighlightCurrentNode { get; set; } = true;          // Highlight current node
public DotLayoutDirection LayoutDirection { get; set; } = DotLayoutDirection.TopToBottom; // Layout direction
public bool IncludeNodeTypeInfo { get; set; } = true;           // Include node type information
public Dictionary<string, string> CustomNodeStyles { get; set; } = new(); // Custom node styles
public Dictionary<string, string> CustomEdgeStyles { get; set; } = new(); // Custom edge styles
```

### SvgSerializationOptions

Options specific to SVG serialization.

```csharp
public int Width { get; set; } = 960;                          // Canvas width in pixels
public int Height { get; set; } = 540;                          // Canvas height in pixels
public int HorizontalSpacing { get; set; } = 180;               // Horizontal spacing between nodes
public int VerticalSpacing { get; set; } = 120;                 // Vertical spacing between nodes
public bool IncludeMetricsOverlay { get; set; } = true;         // Include metrics overlay
public bool HighlightExecutionPath { get; set; } = true;        // Highlight execution path
public bool HighlightCurrentNode { get; set; } = true;          // Highlight current node
```

### JsonSerializationOptions

Options specific to JSON serialization.

```csharp
public bool Indented { get; set; } = true;                      // Format JSON with indentation
public bool UseCamelCase { get; set; } = true;                  // Use camelCase property naming
public bool IncludeNodeProperties { get; set; } = true;          // Include detailed node properties
public bool IncludeLayoutInfo { get; set; } = true;             // Include layout information
public bool IncludeExecutionMetrics { get; set; } = false;      // Include execution metrics
public bool IncludeTimestamps { get; set; } = true;             // Include timestamp information
public int MaxSerializationDepth { get; set; } = 10;            // Maximum serialization depth
```

### MermaidGenerationOptions

Options specific to Mermaid diagram generation.

```csharp
public string Direction { get; set; } = "TD";                   // Diagram direction (TD, LR, BT, RL)
public bool IncludeTitle { get; set; } = true;                  // Include title in diagram
public bool EnableStyling { get; set; } = true;                 // Enable styling classes
public bool HighlightExecutionPath { get; set; } = true;        // Highlight execution path
public bool HighlightCurrentNode { get; set; } = true;          // Highlight current node
public bool StyleByNodeType { get; set; } = true;               // Style nodes by type
public bool IncludePerformanceIndicators { get; set; } = false; // Include performance indicators
public MermaidTheme Theme { get; set; } = MermaidTheme.Default; // Diagram theme
public Dictionary<string, string> CustomStyles { get; set; } = new(); // Custom CSS styles
```

## InspectionFormat Enum

Enumeration of supported inspection response formats.

```csharp
public enum InspectionFormat
{
    Json,      // JSON format for API responses
    Dot,       // DOT format for GraphViz visualization
    Mermaid,   // Mermaid format for web-based diagrams
    Xml        // XML format for structured data exchange
}
```

## MetricsExportFormat Enum

Enumeration of supported metrics export formats.

```csharp
public enum MetricsExportFormat
{
    Json,        // JSON format
    Csv,         // CSV format
    Prometheus   // Prometheus metrics format
}
```

## DotLayoutDirection Enum

Enumeration of DOT layout directions.

```csharp
public enum DotLayoutDirection
{
    TopToBottom,    // Top to bottom layout
    LeftToRight,    // Left to right layout
    BottomToTop,    // Bottom to top layout
    RightToLeft     // Right to left layout
}
```

## VisualizationTheme Enum

Enumeration of supported visualization themes.

```csharp
public enum VisualizationTheme
{
    Default,    // Default theme with standard colors and styling
    Dark,       // Dark theme optimized for dark backgrounds
    Light,      // Light theme optimized for light backgrounds
    HighContrast // High contrast theme for accessibility
}
```

## MermaidTheme Enum

Enumeration of Mermaid diagram themes.

```csharp
public enum MermaidTheme
{
    Default,    // Default Mermaid theme
    Forest,     // Forest theme with green tones
    Dark,       // Dark theme
    Neutral     // Neutral theme
}
```

## GraphInspectionResponse

Represents a response from the graph inspection API.

### Properties

```csharp
public bool IsSuccess { get; private set; }                     // Whether the request was successful
public string Data { get; private set; } = string.Empty;        // Response data
public string? ErrorMessage { get; private set; }               // Error message if failed
public object? Metadata { get; private set; }                   // Additional metadata
public DateTimeOffset Timestamp { get; private set; } = DateTimeOffset.UtcNow; // Response timestamp
```

### Static Factory Methods

```csharp
public static GraphInspectionResponse Success(string data, object? metadata = null)
public static GraphInspectionResponse NotFound(string message)
public static GraphInspectionResponse Error(string message)
```

## GraphVisualizationData

Data structure for graph visualization information.

### Properties

```csharp
public IReadOnlyList<IGraphNode> Nodes { get; }                 // Graph nodes
public IReadOnlyList<GraphEdgeInfo> Edges { get; }              // Graph edges
public IGraphNode? CurrentNode { get; }                          // Currently executing node
public IReadOnlyList<IGraphNode> ExecutionPath { get; }         // Execution path
public DateTimeOffset GeneratedAt { get; } = DateTimeOffset.UtcNow; // Generation timestamp
```

### Constructors

```csharp
public GraphVisualizationData(IReadOnlyList<IGraphNode> nodes, IReadOnlyList<GraphEdgeInfo> edges, IGraphNode? currentNode = null, IReadOnlyList<IGraphNode>? executionPath = null)
```

## GraphEdgeInfo

Information about a graph edge for visualization.

### Properties

```csharp
public string FromNodeId { get; }                                // Source node ID
public string ToNodeId { get; }                                  // Target node ID
public string? Label { get; }                                    // Edge label
public string? Condition { get; }                                // Conditional expression
public bool IsHighlighted { get; set; }                          // Whether edge is highlighted
```

### Constructors

```csharp
public GraphEdgeInfo(string fromNodeId, string toNodeId, string? label = null, string? condition = null)
```

## Usage Examples

### Basic Inspection Setup

```csharp
// Create inspection API with options
var inspectionOptions = new GraphInspectionOptions
{
    MaxActiveExecutions = 50,
    EnableDetailedNodeInspection = true,
    EnablePerformanceMetrics = true,
    EnableRealtimeMonitoring = true
};

var inspectionApi = new GraphInspectionApi(inspectionOptions, logger);

// Register an execution for monitoring
inspectionApi.RegisterExecution(executionContext, performanceMetrics);
```

### Graph Structure Inspection

```csharp
// Get graph structure in different formats
var jsonStructure = inspectionApi.GetGraphStructure(executionId, InspectionFormat.Json);
var dotStructure = inspectionApi.GetGraphStructure(executionId, InspectionFormat.Dot);
var mermaidStructure = inspectionApi.GetGraphStructure(executionId, InspectionFormat.Mermaid);

if (jsonStructure.IsSuccess)
{
    var graphData = jsonStructure.Data;
    // Process graph structure data
}
```

### Visualization Export

```csharp
// Create visualization engine
var visualizationEngine = new GraphVisualizationEngine(logger: logger);

// Export to DOT format
var dotOptions = new DotSerializationOptions
{
    GraphName = "MyGraph",
    HighlightExecutionPath = true,
    HighlightCurrentNode = true,
    LayoutDirection = DotLayoutDirection.TopToBottom
};

var dotOutput = visualizationEngine.SerializeToDot(visualizationData, dotOptions);

// Export to Mermaid
var mermaidOptions = new MermaidGenerationOptions
{
    Direction = "TD",
    EnableStyling = true,
    HighlightExecutionPath = true,
    StyleByNodeType = true
};

var mermaidOutput = visualizationEngine.GenerateEnhancedMermaidDiagram(visualizationData, mermaidOptions);
```

### Performance Metrics Export

```csharp
// Get performance metrics
var metricsResponse = inspectionApi.GetPerformanceMetrics(executionId, TimeSpan.FromHours(1), MetricsExportFormat.Json);

if (metricsResponse.IsSuccess)
{
    var metricsData = metricsResponse.Data;
    // Process performance metrics
}

// Generate performance heatmap
var heatmapResponse = inspectionApi.GetPerformanceHeatmap(executionId);
if (heatmapResponse.IsSuccess)
{
    var heatmapData = heatmapResponse.Data;
    // Process heatmap data
}
```

### Real-time Monitoring

```csharp
// Get current execution status
var statusResponse = inspectionApi.GetExecutionStatus(executionId);
if (statusResponse.IsSuccess)
{
    var statusData = statusResponse.Data;
    // Process real-time status
}

// Get execution health
var healthResponse = inspectionApi.GetExecutionHealth(executionId);
if (healthResponse.IsSuccess)
{
    var healthData = healthResponse.Data;
    // Process health information
}
```

## See Also

- [Debug and Inspection Guide](../how-to/debug-and-inspection.md) - How to use inspection and debugging features
- [Metrics and Observability Guide](../how-to/metrics-and-observability.md) - Performance monitoring and metrics
- [Real-time Visualization Guide](../how-to/real-time-visualization-and-highlights.md) - Live visualization features
- [GraphVisualizationExample](../examples/graph-visualization.md) - Practical visualization examples
- [Streaming APIs Reference](./streaming.md) - Real-time execution monitoring
- [Metrics and Logging Reference](./metrics-logging.md) - Performance metrics and logging
