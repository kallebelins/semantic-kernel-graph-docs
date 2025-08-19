# Execução

A execução define como os grafos são processados, incluindo modos sequenciais, paralelos e distribuídos.

## Conceitos e Técnicas

**Execução de Grafo**: Processo de navegar pelos nós de um grafo seguindo as regras de roteamento e executando as operações definidas.

**Ciclo de Execução**: Sequência de eventos que ocorre durante a execução: Before → Execute → After.

**Checkpointing**: Capacidade de salvar e restaurar o estado de execução para recuperação e análise.

## Modos de Execução

### Execução Sequencial
- **Processamento Linear**: Nós executam um após o outro
- **Dependências Respeitadas**: Ordem baseada na estrutura do grafo
- **Estado Compartilhado**: Dados passam de um nó para o próximo
- **Debug Simples**: Fácil rastreamento do fluxo de execução

### Execução Paralela (Fork/Join)
- **Processamento Simultâneo**: Múltiplos nós executam ao mesmo tempo
- **Scheduler Determinístico**: Garantia de reprodutibilidade
- **Merge de Estado**: Combinação de resultados de execuções paralelas
- **Controle de Concorrência**: Limites e políticas de recursos

### Execução Distribuída
- **Processamento Remoto**: Execução em processos ou máquinas separadas
- **Comunicação Assíncrona**: Troca de mensagens entre componentes
- **Tolerância a Falhas**: Recuperação de falhas de rede ou processo
- **Balanceamento de Carga**: Distribuição equilibrada de trabalho

## Componentes Principais

### GraphExecutor
```csharp
var executor = new GraphExecutor(
    options: new GraphExecutionOptions
    {
        MaxExecutionTime = TimeSpan.FromMinutes(5),
        EnableCheckpointing = true,
        MaxParallelNodes = 4
    }
);

var result = await executor.ExecuteAsync(graph, arguments);
```

### StreamingGraphExecutor
```csharp
var streamingExecutor = new StreamingGraphExecutor(
    options: new StreamingExecutionOptions
    {
        BufferSize = 1000,
        EnableBackpressure = true,
        EventTimeout = TimeSpan.FromSeconds(30)
    }
);

var eventStream = await streamingExecutor.ExecuteStreamingAsync(graph, arguments);
```

### CheckpointManager
```csharp
var checkpointManager = new CheckpointManager(
    options: new CheckpointOptions
    {
        AutoCheckpointInterval = TimeSpan.FromSeconds(30),
        MaxCheckpoints = 10,
        CompressionEnabled = true
    }
);
```

## Ciclo de Execução

### Fase Before
```csharp
// Validação de entrada e preparação
await node.BeforeExecutionAsync(context);
// Verificação de pré-condições
// Inicialização de recursos
```

### Fase Execute
```csharp
// Execução principal do nó
var result = await node.ExecuteAsync(context);
// Processamento da lógica de negócio
// Atualização do estado
```

### Fase After
```csharp
// Limpeza e finalização
await node.AfterExecutionAsync(context);
// Liberação de recursos
// Logging de métricas
```

## Gerenciamento de Estado

### Estado de Execução
```csharp
var executionState = new ExecutionState
{
    CurrentNode = nodeId,
    ExecutionPath = new[] { "start", "process", "current" },
    Variables = new Dictionary<string, object>(),
    Metadata = new ExecutionMetadata()
};
```

### Histórico de Execução
```csharp
var executionHistory = new ExecutionHistory
{
    Steps = new List<ExecutionStep>(),
    Timestamps = new List<DateTime>(),
    PerformanceMetrics = new Dictionary<string, TimeSpan>()
};
```

## Recuperação e Checkpointing

### Salvamento de Estado
```csharp
// Salvar estado atual
var checkpoint = await checkpointManager.CreateCheckpointAsync(
    graphId: graph.Id,
    executionId: context.ExecutionId,
    state: context.State
);
```

### Restauração de Estado
```csharp
// Restaurar execução de um checkpoint
var restoredContext = await checkpointManager.RestoreFromCheckpointAsync(
    checkpointId: checkpoint.Id
);

var result = await executor.ExecuteAsync(graph, restoredContext);
```

## Streaming e Eventos

### Eventos de Execução
```csharp
var events = new[]
{
    new GraphExecutionEvent
    {
        Type = ExecutionEventType.NodeStarted,
        NodeId = "process",
        Timestamp = DateTime.UtcNow,
        Data = new { input = "data" }
    },
    new GraphExecutionEvent
    {
        Type = ExecutionEventType.NodeCompleted,
        NodeId = "process",
        Timestamp = DateTime.UtcNow,
        Data = new { output = "result" }
    }
};
```

### Consumo de Eventos
```csharp
await foreach (var evt in eventStream)
{
    switch (evt.Type)
    {
        case ExecutionEventType.NodeStarted:
            Console.WriteLine($"Node {evt.NodeId} started");
            break;
        case ExecutionEventType.NodeCompleted:
            Console.WriteLine($"Node {evt.NodeId} completed");
            break;
    }
}
```

## Configuração e Opções

### GraphExecutionOptions
```csharp
var options = new GraphExecutionOptions
{
    MaxExecutionTime = TimeSpan.FromMinutes(10),
    EnableCheckpointing = true,
    MaxParallelNodes = 8,
    EnableMetrics = true,
    EnableLogging = true,
    RetryPolicy = new ExponentialBackoffRetryPolicy(maxRetries: 3)
};
```

### StreamingExecutionOptions
```csharp
var streamingOptions = new StreamingExecutionOptions
{
    BufferSize = 1000,
    EnableBackpressure = true,
    EventTimeout = TimeSpan.FromSeconds(60),
    BatchSize = 100,
    EnableCompression = true
};
```

## Monitoramento e Métricas

### Métricas de Performance
- **Tempo de Execução**: Latência total e por nó
- **Throughput**: Número de nós executados por segundo
- **Utilização de Recursos**: CPU, memória e I/O
- **Taxa de Sucesso**: Percentual de execuções bem-sucedidas

### Logging e Tracing
```csharp
var logger = new SemanticKernelGraphLogger();
logger.LogExecutionStart(graph.Id, context.ExecutionId);
logger.LogNodeExecution(node.Id, context.ExecutionId, stopwatch.Elapsed);
logger.LogExecutionComplete(graph.Id, context.ExecutionId, result);
```

## Veja Também

- [Modelo de Execução](../concepts/execution-model.md)
- [Checkpointing](../concepts/checkpointing.md)
- [Streaming](../concepts/streaming.md)
- [Métricas e Logging](../how-to/metrics-and-observability.md)
- [Exemplos de Execução](../examples/execution-guide.md)
- [Exemplos de Streaming](../examples/streaming-execution.md)

## Referências

- `GraphExecutor`: Executor principal de grafos
- `StreamingGraphExecutor`: Executor com streaming de eventos
- `CheckpointManager`: Gerenciador de checkpoints
- `GraphExecutionOptions`: Configurações de execução
- `StreamingExecutionOptions`: Configurações de streaming
- `ExecutionState`: Estado da execução
- `GraphExecutionEvent`: Eventos de execução
