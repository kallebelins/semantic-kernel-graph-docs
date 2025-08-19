---
title: Troubleshooting
---

# Troubleshooting

Guia para resolver problemas comuns e diagnosticar questões no SemanticKernel.Graph.

## Conceitos e Técnicas

**Troubleshooting**: Processo sistemático de identificar, diagnosticar e resolver problemas em sistemas de grafos computacionais.

**Diagnóstico**: Análise de sintomas, logs e métricas para determinar a causa raiz de um problema.

**Recuperação**: Estratégias para restaurar a funcionalidade normal após a resolução de um problema.

## Problemas de Execução

### Execução Pausa ou é Lenta

**Sintomas**:
- Grafo não progride após um nó específico
- Tempo de execução muito maior que o esperado
- Aplicação parece "travada"

**Causas Prováveis**:
- Loops infinitos ou muito longos
- Nós com timeout muito alto
- Bloqueios em recursos externos
- Condições de roteamento que nunca são atendidas

**Diagnóstico**:
```csharp
// Habilitar métricas detalhadas
var options = new GraphExecutionOptions
{
    EnableMetrics = true,
    EnableLogging = true,
    MaxExecutionTime = TimeSpan.FromMinutes(5)
};

// Verificar logs de execução
var logger = kernel.GetRequiredService<ILogger<GraphExecutor>>();
```

**Solução**:
```csharp
// Definir limites de iteração
var loopNode = new ReActLoopGraphNode(
    maxIterations: 10,  // Limite explícito
    timeout: TimeSpan.FromMinutes(2)
);

// Adicionar timeouts aos nós
var nodeOptions = new GraphNodeOptions
{
    MaxExecutionTime = TimeSpan.FromSeconds(30)
};
```

**Prevenção**:
- Sempre definir `MaxIterations` para nós de loop
- Configurar timeouts apropriados
- Usar métricas para monitorar performance
- Implementar circuit breakers para recursos externos

### Serviço Ausente ou Provider Nulo

**Sintomas**:
- `NullReferenceException` ao executar grafos
- Erro "Service not registered" ou similar
- Funcionalidades específicas não funcionam

**Causas Prováveis**:
- `AddGraphSupport()` não foi chamado
- Dependências não registradas no DI container
- Ordem incorreta de registro de serviços

**Diagnóstico**:
```csharp
// Verificar se o suporte a grafos foi adicionado
var graphExecutor = kernel.GetService<IGraphExecutor>();
if (graphExecutor == null)
{
    Console.WriteLine("Graph support not enabled!");
}
```

**Solução**:
```csharp
// Configuração correta
var builder = Kernel.CreateBuilder();

// Adicionar suporte a grafos ANTES de outros serviços
builder.AddGraphSupport(options => {
    options.EnableMetrics = true;
    options.EnableCheckpointing = true;
});

// Adicionar outros serviços
builder.AddOpenAIChatCompletion(/* ... */);
builder.AddMemory(/* ... */);

var kernel = builder.Build();
```

**Prevenção**:
- Sempre chamar `AddGraphSupport()` primeiro
- Verificar ordem de registro de serviços
- Usar testes de integração para validar configuração

### Falha em Ferramentas REST

**Sintomas**:
- Erros de timeout em chamadas HTTP
- Falhas de autenticação
- Respostas inesperadas de APIs externas

**Causas Prováveis**:
- Schemas de validação incorretos
- Timeouts muito baixos
- Problemas de autenticação
- APIs externas indisponíveis

**Diagnóstico**:
```csharp
// Verificar telemetria de dependências
var telemetry = kernel.GetRequiredService<ITelemetryService>();
var httpMetrics = telemetry.GetHttpMetrics();

// Verificar logs de erro
var logger = kernel.GetRequiredService<ILogger<RestToolGraphNode>>();
```

**Solução**:
```csharp
// Configurar timeouts apropriados
var restToolOptions = new RestToolOptions
{
    Timeout = TimeSpan.FromSeconds(30),
    RetryPolicy = new ExponentialBackoffRetryPolicy(maxRetries: 3),
    CircuitBreaker = new CircuitBreakerOptions
    {
        FailureThreshold = 5,
        RecoveryTimeout = TimeSpan.FromMinutes(1)
    }
};

// Validar schemas
var schema = new RestToolSchema
{
    InputValidation = true,
    OutputValidation = true
};
```

**Prevenção**:
- Testar APIs externas antes de usar
- Implementar circuit breakers
- Configurar timeouts realistas
- Validar schemas de entrada/saída

## Problemas de Estado e Checkpointing

### Checkpoint Não Restaurado

**Sintomas**:
- Estado perdido entre execuções
- Erro ao restaurar checkpoint
- Dados inconsistentes após recuperação

**Causas Prováveis**:
- Extensões de checkpointing não configuradas
- Coleção de banco de dados não existe
- Incompatibilidade de versão de estado
- Problemas de serialização

**Diagnóstico**:
```csharp
// Verificar configuração de checkpointing
var checkpointManager = kernel.GetService<ICheckpointManager>();
if (checkpointManager == null)
{
    Console.WriteLine("Checkpointing not enabled!");
}

// Verificar conectividade com banco
var connection = await checkpointManager.TestConnectionAsync();
```

**Solução**:
```csharp
// Configurar checkpointing corretamente
builder.AddGraphSupport(options => {
    options.Checkpointing = new CheckpointingOptions
    {
        Enabled = true,
        Provider = "MongoDB", // ou outro provider
        ConnectionString = "mongodb://localhost:27017",
        DatabaseName = "semantic-kernel-graph",
        CollectionName = "checkpoints"
    };
});
```

**Prevenção**:
- Sempre testar conectividade com banco
- Implementar validação de versão de estado
- Usar serialização robusta
- Monitorar espaço em disco

### Problemas de Serialização

**Sintomas**:
- Erro "Cannot serialize type X"
- Checkpoints corrompidos
- Falhas ao salvar estado

**Causas Prováveis**:
- Tipos não serializáveis
- Referências circulares
- Tipos complexos não suportados

**Diagnóstico**:
```csharp
// Verificar se o tipo é serializável
var state = new GraphState();
try
{
    state.SetValue("test", new NonSerializableType());
    var serialized = await state.SerializeAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Serialization error: {ex.Message}");
}
```

**Solução**:
```csharp
// Implementar ISerializableState
public class MyState : ISerializableState
{
    public string Serialize() => JsonSerializer.Serialize(this);
    public static MyState Deserialize(string data) => JsonSerializer.Deserialize<MyState>(data);
}

// Ou usar tipos simples
state.SetValue("simple", "string value");
state.SetValue("number", 42);
state.SetValue("array", new[] { 1, 2, 3 });
```

**Prevenção**:
- Usar tipos primitivos quando possível
- Implementar `ISerializableState` para tipos complexos
- Evitar referências circulares
- Testar serialização durante desenvolvimento

## Problemas de Nós Python

### Erros de Execução Python

**Sintomas**:
- Erro "python not found"
- Timeouts em execução Python
- Falhas de comunicação entre .NET e Python

**Causas Prováveis**:
- Python não está no PATH
- Versão incorreta do Python
- Problemas de permissão
- Dependências Python ausentes

**Diagnóstico**:
```csharp
// Verificar se Python está disponível
var pythonNode = new PythonGraphNode("python");
var isAvailable = await pythonNode.CheckAvailabilityAsync();
Console.WriteLine($"Python available: {isAvailable}");
```

**Solução**:
```csharp
// Configurar Python explicitamente
var pythonOptions = new PythonNodeOptions
{
    PythonPath = @"C:\Python39\python.exe", // Caminho explícito
    EnvironmentVariables = new Dictionary<string, string>
    {
        ["PYTHONPATH"] = @"C:\my-python-libs",
        ["PYTHONUNBUFFERED"] = "1"
    },
    Timeout = TimeSpan.FromMinutes(5)
};

var pythonNode = new PythonGraphNode("python", pythonOptions);
```

**Prevenção**:
- Usar caminhos absolutos para Python
- Verificar dependências Python
- Configurar variáveis de ambiente
- Implementar fallbacks para nós Python

## Problemas de Performance

### Execução Muito Lenta

**Sintomas**:
- Tempo de execução muito maior que o esperado
- Uso excessivo de CPU/memória
- Grafos simples demoram muito

**Causas Prováveis**:
- Nós ineficientes
- Falta de paralelização
- Bloqueios desnecessários
- Configurações subótimas

**Diagnóstico**:
```csharp
// Analisar métricas de performance
var metrics = await executor.GetPerformanceMetricsAsync();
foreach (var nodeMetric in metrics.NodeMetrics)
{
    Console.WriteLine($"Node {nodeMetric.NodeId}: {nodeMetric.AverageExecutionTime}");
}
```

**Solução**:
```csharp
// Habilitar execução paralela
var options = new GraphExecutionOptions
{
    MaxParallelNodes = Environment.ProcessorCount,
    EnableOptimizations = true
};

// Usar nós otimizados
var optimizedNode = new OptimizedFunctionGraphNode(
    function: kernelFunction,
    options: new NodeOptimizationOptions
    {
        EnableCaching = true,
        EnableBatching = true
    }
);
```

**Prevenção**:
- Monitorar métricas regularmente
- Usar profiling para identificar gargalos
- Implementar cache quando apropriado
- Otimizar nós críticos

## Problemas de Integração

### Falhas de Autenticação

**Sintomas**:
- Erros 401/403 em APIs externas
- Falhas de autenticação com LLMs
- Problemas de autorização

**Causas Prováveis**:
- Chaves de API inválidas
- Tokens expirados
- Configuração incorreta de credenciais
- Problemas de permissão

**Diagnóstico**:
```csharp
// Verificar configuração de autenticação
var authService = kernel.GetService<IAuthenticationService>();
var isValid = await authService.ValidateCredentialsAsync();
```

**Solução**:
```csharp
// Configurar autenticação corretamente
builder.AddOpenAIChatCompletion(
    modelId: "gpt-4",
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")
);

// Ou usar Azure AD
builder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4",
    endpoint: "https://your-endpoint.openai.azure.com/",
    apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")
);
```

**Prevenção**:
- Usar variáveis de ambiente para credenciais
- Implementar rotação automática de tokens
- Monitorar expiração de credenciais
- Usar gerenciadores de segredos

## Estratégias de Recuperação

### Recuperação Automática
```csharp
// Configurar políticas de retry
var retryPolicy = new ExponentialBackoffRetryPolicy(
    maxRetries: 3,
    initialDelay: TimeSpan.FromSeconds(1)
);

// Implementar circuit breaker
var circuitBreaker = new CircuitBreaker(
    failureThreshold: 5,
    recoveryTimeout: TimeSpan.FromMinutes(1)
);
```

### Fallbacks e Alternativas
```csharp
// Implementar nós de fallback
var fallbackNode = new FallbackGraphNode(
    primaryNode: primaryNode,
    fallbackNode: backupNode,
    condition: state => state.GetValue<bool>("use_fallback")
);
```

## Monitoramento e Alertas

### Configuração de Alertas
```csharp
// Configurar alertas para problemas críticos
var alertingService = new GraphAlertingService();
alertingService.AddAlert(new AlertRule
{
    Condition = metrics => metrics.ErrorRate > 0.1,
    Severity = AlertSeverity.Critical,
    Message = "Error rate exceeded threshold"
});
```

### Logging Estruturado
```csharp
// Configurar logging detalhado
var logger = new SemanticKernelGraphLogger();
logger.LogExecutionStart(graphId, executionId);
logger.LogNodeExecution(nodeId, executionId, duration);
logger.LogExecutionComplete(graphId, executionId, result);
```

## Veja Também

- [Error Handling](../how-to/error-handling-and-resilience.md)
- [Performance Tuning](../how-to/performance-tuning.md)
- [Monitoring](../how-to/metrics-and-observability.md)
- [Configuration](../how-to/configuration.md)
- [Examples](../examples/index.md)

## Referências

- `GraphExecutionOptions`: Configurações de execução
- `CheckpointingOptions`: Configurações de checkpointing
- `PythonNodeOptions`: Configurações de nós Python
- `RetryPolicy`: Políticas de retry
- `CircuitBreaker`: Circuit breakers para resiliência
- `GraphAlertingService`: Sistema de alertas


