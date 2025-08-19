# Visualização

A visualização permite gerar diagramas e exportar estruturas de grafos para documentação e ferramentas externas.

## Conceitos e Técnicas

**Visualização de Grafo**: Representação gráfica da estrutura e fluxo de execução de um grafo computacional.

**Engine de Visualização**: Sistema responsável por converter a estrutura interna do grafo em formatos visuais.

**Overlay de Execução**: Camada visual que mostra o estado atual e histórico de execução em tempo real.

## Formatos de Exportação

### DOT (Graphviz)
- **Formato**: Linguagem de descrição de grafos para Graphviz
- **Uso**: Geração de diagramas estáticos e interativos
- **Vantagens**: Padrão da indústria, suporte a layouts automáticos
- **Exemplo**: `digraph { A -> B [label="condition"]; }`

### Mermaid
- **Formato**: Linguagem de diagramação baseada em texto
- **Uso**: Integração com ferramentas como GitHub, GitLab, Notion
- **Vantagens**: Sintaxe simples, renderização automática
- **Exemplo**: `graph TD; A-->B; B-->C;`

### JSON
- **Formato**: Representação estruturada dos dados do grafo
- **Uso**: Integração com ferramentas externas e APIs
- **Vantagens**: Estrutura hierárquica, fácil parsing
- **Exemplo**: `{"nodes": [...], "edges": [...], "metadata": {...}}`

## Componentes Principais

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

## Funcionalidades de Visualização

### Visualização Estática
- **Estrutura do Grafo**: Nós, arestas e hierarquia
- **Metadados**: Informações sobre tipos, configurações e documentação
- **Layouts Automáticos**: Organização automática de elementos

### Visualização em Tempo Real
- **Estado de Execução**: Nó atual, histórico de execução
- **Métricas**: Tempos de execução, contadores de uso
- **Highlights**: Destaque visual para nós ativos e caminhos percorridos

### Inspeção Interativa
- **Zoom e Navegação**: Exploração detalhada de partes específicas
- **Filtros**: Visualização seletiva por tipo de nó ou estado
- **Tooltips**: Informações detalhadas ao passar o mouse

## Configuração e Opções

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

### Temas de Visualização
- **Light**: Tema claro para documentação impressa
- **Dark**: Tema escuro para apresentações e demos
- **Custom**: Temas personalizados com cores específicas

## Exemplos de Uso

### Exportação Básica
```csharp
// Exportar para DOT
var dotContent = await visualizer.ExportAsync(graph, VisualizationFormat.DOT);

// Exportar para Mermaid
var mermaidContent = await visualizer.ExportAsync(graph, VisualizationFormat.Mermaid);

// Exportar para JSON
var jsonContent = await visualizer.ExportAsync(graph, VisualizationFormat.JSON);
```

### Visualização com Estado
```csharp
// Visualizar grafo com estado de execução
var executionState = await executor.GetExecutionStateAsync();
var visualGraph = await visualizer.CreateExecutionVisualizationAsync(graph, executionState);

// Exportar visualização completa
var visualization = await visualizer.ExportAsync(visualGraph, VisualizationFormat.Mermaid);
```

### Overlay de Execução
```csharp
// Criar overlay em tempo real
var realtimeVisualizer = new GraphRealtimeHighlighter(graph);
realtimeVisualizer.StartHighlighting();

// Atualizar estado visual
await realtimeVisualizer.UpdateExecutionStateAsync(executionState);
```

## Integração com Ferramentas

### GitHub/GitLab
- **Mermaid**: Renderização automática em markdown
- **PlantUML**: Integração via extensões
- **Graphviz**: Renderização via GitHub Actions

### Ferramentas de Documentação
- **DocFX**: Integração com documentação de API
- **MkDocs**: Suporte nativo a Mermaid
- **Sphinx**: Extensões para diagramas

### IDEs e Editores
- **VS Code**: Extensões para visualização de grafos
- **JetBrains**: Plugins para diagramação
- **Vim/Emacs**: Modos para edição de grafos

## Monitoramento e Debug

### Métricas de Visualização
- **Tempo de Renderização**: Latência para gerar visualizações
- **Tamanho de Exportação**: Tamanho dos arquivos gerados
- **Qualidade de Layout**: Métricas de organização visual

### Debug de Visualização
```csharp
var debugOptions = new GraphVisualizationOptions
{
    EnableDebugMode = true,
    LogVisualizationSteps = true,
    ValidateVisualizationOutput = true
};
```

## Veja Também

- [Visualização em Tempo Real](../how-to/real-time-visualization-and-highlights.md)
- [Exemplos de Visualização](../examples/graph-visualization.md)
- [Debug e Inspeção](../how-to/debug-and-inspection.md)
- [Grafos](../concepts/graph-concepts.md)
- [Execução](../concepts/execution-model.md)

## Referências

- `GraphVisualizationEngine`: Motor principal de visualização
- `VisualGraphDefinition`: Estrutura de dados para visualização
- `GraphVisualizationOptions`: Configurações de visualização
- `GraphRealtimeHighlighter`: Destaque em tempo real
- `VisualizationFormat`: Formatos de exportação suportados
