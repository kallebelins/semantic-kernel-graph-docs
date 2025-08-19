# Grafos

Um grafo é uma rede direcionada de nós conectados por arestas. A execução começa em um nó de entrada e prossegue avaliando condições de roteamento.

## Conceitos e Técnicas

**Grafo Computacional**: Estrutura de dados que representa um fluxo de trabalho ou pipeline de processamento através de nós e conexões.

**Nó de Entrada**: Ponto de início da execução do grafo, definido como `StartNode`.

**Aresta Direcionada**: Conexão entre dois nós que define a direção do fluxo de execução.

**Validação de Grafo**: Verificação de integridade antes da execução para garantir que o grafo é válido.

## Estrutura de um Grafo

### Componentes Básicos
```csharp
var graph = new Graph
{
    Id = "workflow-001",
    Name = "Processamento de Documentos",
    Description = "Pipeline para análise e classificação de documentos",
    StartNode = startNode,
    Nodes = new[] { startNode, processNode, classifyNode, endNode },
    Edges = new[] { edge1, edge2, edge3 }
};
```

### Nós e Arestas
- **Nós**: Encapsulam trabalho (funções SK, loops, subgrafos, ferramentas)
- **Arestas**: Carregam condições opcionais para controlar o fluxo
- **Validação**: O engine garante a validade antes da execução

## Tipos de Grafos

### Grafo Linear
```csharp
// Sequência simples: A → B → C
var linearGraph = new Graph
{
    StartNode = nodeA,
    Nodes = new[] { nodeA, nodeB, nodeC },
    Edges = new[] 
    { 
        new Edge(nodeA, nodeB),
        new Edge(nodeB, nodeC)
    }
};
```

### Grafo com Condições
```csharp
// Grafo com ramificações condicionais
var conditionalGraph = new Graph
{
    StartNode = startNode,
    Nodes = new[] { startNode, processNode, successNode, failureNode },
    Edges = new[] 
    { 
        new ConditionalEdge(startNode, processNode),
        new ConditionalEdge(processNode, successNode, 
            condition: state => state.GetValue<int>("status") == 200),
        new ConditionalEdge(processNode, failureNode, 
            condition: state => state.GetValue<int>("status") != 200)
    }
};
```

### Grafo com Loops
```csharp
// Grafo com iteração controlada
var loopGraph = new Graph
{
    StartNode = startNode,
    Nodes = new[] { startNode, loopNode, endNode },
    Edges = new[] 
    { 
        new Edge(startNode, loopNode),
        new ConditionalEdge(loopNode, loopNode, 
            condition: state => state.GetValue<int>("counter") < 10),
        new ConditionalEdge(loopNode, endNode, 
            condition: state => state.GetValue<int>("counter") >= 10)
    }
};
```

## Validação e Integridade

### Verificações de Validação
```csharp
var validator = new WorkflowValidator();
var validationResult = await validator.ValidateAsync(graph);

if (!validationResult.IsValid)
{
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"Validation error: {error.Message}");
    }
}
```

### Regras de Validação
- **Conectividade**: Todos os nós devem ser alcançáveis
- **Ciclos**: Detecção de loops infinitos
- **Tipos**: Validação de tipos de entrada/saída
- **Dependências**: Verificação de dependências circulares

## Construção de Grafos

### Construção Programática
```csharp
var graphBuilder = new GraphBuilder();

var graph = await graphBuilder
    .AddNode(startNode)
    .AddNode(processNode)
    .AddNode(endNode)
    .AddEdge(startNode, processNode)
    .AddEdge(processNode, endNode)
    .SetStartNode(startNode)
    .BuildAsync();
```

### Construção por Template
```csharp
var template = new ChainOfThoughtWorkflowTemplate();
var graph = await template.CreateGraphAsync(
    kernel: kernel,
    options: new TemplateOptions
    {
        MaxSteps = 5,
        EnableReasoning = true
    }
);
```

### Construção por DSL
```csharp
var dslParser = new GraphDslParser();
var graphDefinition = @"
    start -> process -> classify -> end
    process -> retry if error
    retry -> process if attempts < 3
";

var graph = await dslParser.ParseAsync(dslDefinition);
```

## Execução e Controle

### Execução Básica
```csharp
var executor = new GraphExecutor();
var arguments = new KernelArguments
{
    ["input"] = "documento.pdf",
    ["maxRetries"] = 3
};

var result = await executor.ExecuteAsync(graph, arguments);
```

### Execução com Streaming
```csharp
var streamingExecutor = new StreamingGraphExecutor();
var eventStream = await streamingExecutor.ExecuteStreamingAsync(graph, arguments);

await foreach (var evt in eventStream)
{
    Console.WriteLine($"Event: {evt.Type} at node {evt.NodeId}");
}
```

### Execução com Checkpointing
```csharp
var checkpointingExecutor = new CheckpointingGraphExecutor();
var result = await checkpointingExecutor.ExecuteAsync(graph, arguments);

// Salvar checkpoint
var checkpoint = await checkpointingExecutor.CreateCheckpointAsync();

// Restaurar execução
var restoredResult = await checkpointingExecutor.RestoreFromCheckpointAsync(checkpoint);
```

## Metadados e Documentação

### Informações do Grafo
```csharp
var graphMetadata = new GraphMetadata
{
    Version = "1.0.0",
    Author = "Equipe de Desenvolvimento",
    CreatedAt = DateTime.UtcNow,
    Tags = new[] { "documentos", "classificação", "IA" },
    EstimatedExecutionTime = TimeSpan.FromMinutes(5),
    ResourceRequirements = new ResourceRequirements
    {
        MaxMemory = "2GB",
        MaxCpu = "4 cores"
    }
};
```

### Documentação Automática
```csharp
var docGenerator = new GraphDocumentationGenerator();
var documentation = await docGenerator.GenerateAsync(graph, 
    new DocumentationOptions
    {
        IncludeCodeExamples = true,
        IncludeDiagrams = true,
        Format = DocumentationFormat.Markdown
    }
);
```

## Monitoramento e Observabilidade

### Métricas de Execução
```csharp
var metrics = new GraphPerformanceMetrics
{
    TotalExecutionTime = TimeSpan.FromSeconds(45),
    NodeExecutionTimes = new Dictionary<string, TimeSpan>(),
    ExecutionPath = new[] { "start", "process", "classify", "end" },
    ResourceUsage = new ResourceUsageMetrics()
};
```

### Logging e Tracing
```csharp
var logger = new SemanticKernelGraphLogger();
logger.LogGraphExecutionStart(graph.Id, executionId);
logger.LogGraphExecutionComplete(graph.Id, executionId, result);
logger.LogGraphValidation(graph.Id, validationResult);
```

## Veja Também

- [Conceitos de Grafo](../concepts/graph-concepts.md)
- [Tipos de Nós](../concepts/node-types.md)
- [Roteamento](../concepts/routing.md)
- [Execução](../concepts/execution.md)
- [Construindo um Grafo](../how-to/build-a-graph.md)
- [Exemplos de Subgrafos](../examples/subgraph-examples.md)

## Referências

- `Graph`: Classe principal para representar grafos computacionais
- `GraphBuilder`: Construtor fluente para grafos
- `WorkflowValidator`: Validador de integridade de grafos
- `GraphExecutor`: Executor principal de grafos
- `GraphDocumentationGenerator`: Gerador de documentação automática
- `GraphPerformanceMetrics`: Métricas de performance de execução
