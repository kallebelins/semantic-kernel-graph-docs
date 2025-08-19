# Visualization

Visualization allows you to generate diagrams and export graph structures for documentation and external tools.

## Concepts and Techniques

**Graph Visualization**: Graphical representation of the structure and execution flow of a computational graph.

**Visualization Engine**: System responsible for converting the internal graph structure into visual formats.

**Execution Overlay**: Visual layer that shows the current state and execution history in real time.

## Export Formats

### DOT (Graphviz)
- **Format**: Graph description language for Graphviz
- **Usage**: Generation of static and interactive diagrams
- **Advantages**: Industry standard, automatic layout support
- **Example**: `digraph { A -> B [label="condition"]; }`

### Mermaid
- **Format**: Text-based diagramming language
- **Usage**: Integration with tools like GitHub, GitLab, Notion
- **Advantages**: Simple syntax, automatic rendering
- **Example**: `graph TD; A-->B; B-->C;`

### JSON
- **Format**: Structured representation of graph data
- **Usage**: Integration with external tools and APIs
- **Advantages**: Hierarchical structure, easy parsing
- **Example**: `{"nodes": [...], "edges": [...], "metadata": {...}}`

## Main Components

### GraphVisualizationEngine
```csharp
var visualizer = new GraphVisualizationEngine(
    options: new GraphVisualizationOptions
    {
        IncludeMetadata = true,
        ShowExecutionState = true,
        ExportFormat = VisualizationFormat.DOT
    }
);
```

### VisualGraphDefinition
```csharp
var graphDef = new VisualGraphDefinition
{
    Nodes = graph.Nodes.Select(n => new VisualNode
    {
        Id = n.Id,
        Label = n.Name,
        Type = n.GetType().Name,
        Position = n.Position
    }),
    Edges = graph.Edges.Select(e => new VisualEdge
    {
        Source = e.Source.Id,
        Target = e.Target.Id,
        Label = e.Condition?.ToString()
    })
};
```

## Visualization Features

### Static Visualization
- **Graph Structure**: Nodes, edges and hierarchy
- **Metadata**: Information about types, configurations and documentation
- **Automatic Layouts**: Automatic organization of elements

### Real-Time Visualization
- **Execution State**: Current node, execution history
- **Metrics**: Execution times, usage counters
- **Highlights**: Visual emphasis for active nodes and traversed paths

### Interactive Inspection
- **Zoom and Navigation**: Detailed exploration of specific parts
- **Filters**: Selective visualization by node type or state
- **Tooltips**: Detailed information on hover

## Configuration and Options

### GraphVisualizationOptions
```csharp
var options = new GraphVisualizationOptions
{
    IncludeMetadata = true,
    ShowExecutionState = true,
    ShowPerformanceMetrics = true,
    ExportFormat = VisualizationFormat.Mermaid,
    Theme = VisualizationTheme.Dark,
    NodeSpacing = 100,
    EdgeRouting = EdgeRoutingType.Orthogonal
};
```

### Visualization Themes
- **Light**: Light theme for printed documentation
- **Dark**: Dark theme for presentations and demos
- **Custom**: Custom themes with specific colors

## Usage Examples

### Basic Export
```csharp
// Export to DOT
var dotContent = await visualizer.ExportAsync(graph, VisualizationFormat.DOT);

// Export to Mermaid
var mermaidContent = await visualizer.ExportAsync(graph, VisualizationFormat.Mermaid);

// Export to JSON
var jsonContent = await visualizer.ExportAsync(graph, VisualizationFormat.JSON);
```

### State Visualization
```csharp
// Visualize graph with execution state
var executionState = await executor.GetExecutionStateAsync();
var visualGraph = await visualizer.CreateExecutionVisualizationAsync(graph, executionState);

// Export complete visualization
var visualization = await visualizer.ExportAsync(visualGraph, VisualizationFormat.Mermaid);
```

### Execution Overlay
```csharp
// Create real-time overlay
var realtimeVisualizer = new GraphRealtimeHighlighter(graph);
realtimeVisualizer.StartHighlighting();

// Update visual state
await realtimeVisualizer.UpdateExecutionStateAsync(executionState);
```

## Tool Integration

### GitHub/GitLab
- **Mermaid**: Automatic rendering in markdown
- **PlantUML**: Integration via extensions
- **Graphviz**: Rendering via GitHub Actions

### Documentation Tools
- **DocFX**: Integration with API documentation
- **MkDocs**: Native Mermaid support
- **Sphinx**: Extensions for diagrams

### IDEs and Editors
- **VS Code**: Extensions for graph visualization
- **JetBrains**: Plugins for diagramming
- **Vim/Emacs**: Modes for graph editing

## Monitoring and Debugging

### Visualization Metrics
- **Render Time**: Latency to generate visualizations
- **Export Size**: Size of generated files
- **Layout Quality**: Visual organization metrics

### Visualization Debugging
```csharp
var debugOptions = new GraphVisualizationOptions
{
    EnableDebugMode = true,
    LogVisualizationSteps = true,
    ValidateVisualizationOutput = true
};
```

## See Also

- [Real-Time Visualization](../how-to/real-time-visualization-and-highlights.md)
- [Visualization Examples](../examples/graph-visualization.md)
- [Debug and Inspection](../how-to/debug-and-inspection.md)
- [Graphs](../concepts/graph-concepts.md)
- [Execution](../concepts/execution-model.md)

## References

- `GraphVisualizationEngine`: Main visualization engine
- `VisualGraphDefinition`: Data structure for visualization
- `GraphVisualizationOptions`: Visualization configurations
- `GraphRealtimeHighlighter`: Real-time highlighting
- `VisualizationFormat`: Supported export formats
