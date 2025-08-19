# Nós

Os nós são os componentes fundamentais de um grafo, cada um encapsulando uma unidade específica de trabalho ou lógica de controle.

## Conceitos e Técnicas

**Nó de Grafo**: Unidade fundamental de processamento que encapsula trabalho, lógica de controle ou operações específicas.

**Ciclo de Vida do Nó**: Sequência de eventos que ocorre durante a execução: Before → Execute → After.

**Interface IGraphNode**: Contrato base que todos os nós devem implementar para integração com o sistema.

## Tipos de Nós

### Nós de Função
```csharp
// Encapsula uma função do Semantic Kernel
var functionNode = new FunctionGraphNode(
    function: kernel.GetFunction("analyze_document"),
    name: "analyze_document",
    description: "Analisa o conteúdo de um documento"
);
```

**Características**:
- **Encapsulamento**: Wraps de `KernelFunction`
- **Execução Síncrona**: Processamento direto da função
- **Estado Compartilhado**: Acesso ao `GraphState` global
- **Métricas**: Coleta automática de performance

### Nós Condicionais
```csharp
// Nó que toma decisões baseadas em condições
var conditionalNode = new ConditionalGraphNode(
    condition: async (state) => {
        var score = state.GetValue<double>("confidence");
        return score > 0.8;
    },
    name: "confidence_gate",
    description: "Filtra resultados por nível de confiança"
);
```

**Características**:
- **Avaliação de Condições**: Expressões booleanas sobre o estado
- **Roteamento Dinâmico**: Decisões em tempo de execução
- **Templates SK**: Uso de funções do Semantic Kernel para decisões
- **Fallbacks**: Estratégias de fallback para condições não atendidas

### Nós de Raciocínio
```csharp
// Nó que implementa padrões de raciocínio
var reasoningNode = new ReasoningGraphNode(
    kernel: kernel,
    prompt: "Analise o problema e sugira uma solução",
    maxSteps: 5,
    name: "problem_solver",
    description: "Resolve problemas usando raciocínio passo a passo"
);
```

**Características**:
- **Chain of Thought**: Raciocínio passo a passo
- **Few-shot Learning**: Exemplos para guiar o raciocínio
- **Validação de Resultados**: Verificação da qualidade das respostas
- **Iteração Controlada**: Limites para evitar loops infinitos

### Nós de Loop
```csharp
// Nó que implementa iterações controladas
var loopNode = new ReActLoopGraphNode(
    kernel: kernel,
    objective: "Classificar todos os documentos",
    maxIterations: 10,
    name: "document_classifier",
    description: "Classifica documentos iterativamente"
);
```

**Características**:
- **ReAct Pattern**: Observação → Pensamento → Ação
- **Objetivos Claros**: Metas específicas para cada iteração
- **Controle de Iteração**: Limites para evitar loops infinitos
- **Estado Persistente**: Manutenção de contexto entre iterações

### Nós de Observação
```csharp
// Nó que observa e registra informações
var observationNode = new ObservationGraphNode(
    observer: new ConsoleObserver(),
    name: "console_logger",
    description: "Registra observações no console"
);
```

**Características**:
- **Observação Passiva**: Não modifica o estado
- **Logging**: Registro de informações de execução
- **Métricas**: Coleta de dados de performance
- **Debug**: Suporte para troubleshooting

### Nós de Subgrafo
```csharp
// Nó que encapsula outro grafo
var subgraphNode = new SubgraphGraphNode(
    subgraph: documentAnalysisGraph,
    name: "document_analysis",
    description: "Pipeline completo de análise de documentos"
);
```

**Características**:
- **Composição**: Reutilização de grafos existentes
- **Encapsulamento**: Interface limpa para grafos complexos
- **Estado Isolado**: Controle de escopo de variáveis
- **Reutilização**: Módulos reutilizáveis em diferentes contextos

### Nós de Tratamento de Erro
```csharp
// Nó que trata exceções e falhas
var errorHandlerNode = new ErrorHandlerGraphNode(
    errorPolicy: new RetryPolicy(maxRetries: 3),
    name: "error_handler",
    description: "Trata erros e implementa políticas de retry"
);
```

**Características**:
- **Políticas de Erro**: Retry, backoff, circuit breaker
- **Recuperação**: Estratégias para lidar com falhas
- **Logging**: Registro detalhado de erros
- **Fallbacks**: Alternativas quando a operação principal falha

### Nós de Aprovação Humana
```csharp
// Nó que pausa para interação humana
var approvalNode = new HumanApprovalGraphNode(
    channel: new ConsoleHumanInteractionChannel(),
    timeout: TimeSpan.FromMinutes(30),
    name: "human_approval",
    description: "Aguarda aprovação humana para continuar"
);
```

**Características**:
- **Interação Humana**: Pausa para input humano
- **Timeouts**: Limites de tempo para resposta
- **Canais Múltiplos**: Console, web, email
- **Auditoria**: Registro de decisões humanas

## Ciclo de Vida do Nó

### Fase Before
```csharp
public override async Task BeforeExecutionAsync(GraphExecutionContext context)
{
    // Validação de entrada
    await ValidateInputAsync(context.State);
    
    // Inicialização de recursos
    await InitializeResourcesAsync();
    
    // Logging de início
    _logger.LogInformation($"Starting execution of node {Id}");
}
```

### Fase Execute
```csharp
public override async Task<GraphExecutionResult> ExecuteAsync(GraphExecutionContext context)
{
    try
    {
        // Execução principal
        var result = await ProcessAsync(context.State);
        
        // Atualização do estado
        context.State.SetValue("output", result);
        
        return GraphExecutionResult.Success(result);
    }
    catch (Exception ex)
    {
        return GraphExecutionResult.Failure(ex);
    }
}
```

### Fase After
```csharp
public override async Task AfterExecutionAsync(GraphExecutionContext context)
{
    // Limpeza de recursos
    await CleanupResourcesAsync();
    
    // Logging de métricas
    _logger.LogInformation($"Node {Id} completed in {context.ExecutionTime}");
    
    // Atualização de contadores
    UpdateExecutionMetrics(context);
}
```

## Configuração e Opções

### Opções de Nó
```csharp
var nodeOptions = new GraphNodeOptions
{
    EnableMetrics = true,
    EnableLogging = true,
    MaxExecutionTime = TimeSpan.FromMinutes(5),
    RetryPolicy = new ExponentialBackoffRetryPolicy(maxRetries: 3),
    CircuitBreaker = new CircuitBreakerOptions
    {
        FailureThreshold = 5,
        RecoveryTimeout = TimeSpan.FromMinutes(1)
    }
};
```

### Validação de Entrada/Saída
```csharp
var inputSchema = new GraphNodeInputSchema
{
    Required = new[] { "document", "language" },
    Optional = new[] { "confidence_threshold" },
    Types = new Dictionary<string, Type>
    {
        ["document"] = typeof(string),
        ["language"] = typeof(string),
        ["confidence_threshold"] = typeof(double)
    }
};

var outputSchema = new GraphNodeOutputSchema
{
    Properties = new[] { "analysis_result", "confidence_score" },
    Types = new Dictionary<string, Type>
    {
        ["analysis_result"] = typeof(string),
        ["confidence_score"] = typeof(double)
    }
};
```

## Monitoramento e Observabilidade

### Métricas de Nó
```csharp
var nodeMetrics = new NodeExecutionMetrics
{
    ExecutionCount = 150,
    AverageExecutionTime = TimeSpan.FromMilliseconds(250),
    SuccessRate = 0.98,
    LastExecutionTime = DateTime.UtcNow,
    ErrorCount = 3,
    ResourceUsage = new ResourceUsageMetrics()
};
```

### Logging Estruturado
```csharp
_logger.LogInformation("Node execution started", new
{
    NodeId = Id,
    NodeType = GetType().Name,
    InputKeys = context.State.GetKeys(),
    ExecutionId = context.ExecutionId,
    Timestamp = DateTime.UtcNow
});
```

## Veja Também

- [Tipos de Nós](../concepts/node-types.md)
- [Nós Condicionais](../how-to/conditional-nodes.md)
- [Loops](../how-to/loops.md)
- [Human-in-the-Loop](../how-to/hitl.md)
- [Tratamento de Erros](../how-to/error-handling-and-resilience.md)
- [Exemplos de Nós](../examples/conditional-nodes.md)

## Referências

- `IGraphNode`: Interface base para todos os nós
- `FunctionGraphNode`: Nó que encapsula funções SK
- `ConditionalGraphNode`: Nó para decisões condicionais
- `ReasoningGraphNode`: Nó para raciocínio passo a passo
- `ReActLoopGraphNode`: Nó para loops ReAct
- `ObservationGraphNode`: Nó para observação e logging
- `SubgraphGraphNode`: Nó que encapsula outros grafos
- `ErrorHandlerGraphNode`: Nó para tratamento de erros
- `HumanApprovalGraphNode`: Nó para interação humana
