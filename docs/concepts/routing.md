# Roteamento

O roteamento determina qual nó será executado em seguida usando arestas condicionais ou estratégias dinâmicas.

## Conceitos e Técnicas

**Roteamento**: Processo de determinar o próximo nó a ser executado baseado em condições, estado ou estratégias dinâmicas.

**Aresta Condicional**: Conexão entre nós que só permite a passagem quando uma condição específica é atendida.

**Estratégia de Roteamento**: Algoritmo ou lógica que decide o caminho de execução baseado em critérios predefinidos.

## Tipos de Roteamento

### Roteamento Simples por Predicados
- **Condições de Estado**: Avaliação direta de propriedades do `GraphState`
- **Expressões Booleanas**: Condições simples como `state.Value > 10`
- **Comparações**: Operadores de igualdade, desigualdade e range

### Roteamento Baseado em Templates
- **Avaliação com SK**: Uso de funções do Semantic Kernel para decisões complexas
- **Prompt-based Routing**: Decisões baseadas em análise de texto ou contexto
- **Semantic Matching**: Roteamento por similaridade semântica

### Roteamento Avançado
- **Similaridade Semântica**: Uso de embeddings para encontrar o melhor caminho
- **Roteamento Probabilístico**: Decisões com pesos e probabilidades
- **Aprendizado por Feedback**: Adaptação baseada em resultados anteriores

## Componentes Principais

### ConditionalEdge
```csharp
var edge = new ConditionalEdge(
    sourceNode: nodeA,
    targetNode: nodeB,
    condition: state => state.GetValue<int>("counter") > 5
);
```

### DynamicRoutingEngine
```csharp
var routingEngine = new DynamicRoutingEngine(
    strategies: new[] { new SemanticRoutingStrategy() },
    fallbackStrategy: new DefaultRoutingStrategy()
);
```

### RoutingStrategies
- **SemanticRoutingStrategy**: Roteamento por similaridade semântica
- **ProbabilisticRoutingStrategy**: Roteamento com pesos probabilísticos
- **ContextualRoutingStrategy**: Roteamento baseado no histórico de execução

## Exemplos de Uso

### Roteamento Condicional Simples
```csharp
// Roteamento baseado em valor numérico
var edge = new ConditionalEdge(
    sourceNode: processNode,
    targetNode: successNode,
    condition: state => state.GetValue<int>("status") == 200
);

var failureEdge = new ConditionalEdge(
    sourceNode: processNode,
    targetNode: failureNode,
    condition: state => state.GetValue<int>("status") != 200
);
```

### Roteamento por Template
```csharp
// Roteamento baseado em análise de texto
var routingNode = new ConditionalGraphNode(
    condition: async (state) => {
        var text = state.GetValue<string>("input");
        var result = await kernel.InvokeAsync("analyze_sentiment", text);
        return result.GetValue<string>("sentiment") == "positive";
    }
);
```

### Roteamento Dinâmico
```csharp
// Roteamento adaptativo baseado em métricas
var dynamicRouter = new DynamicRoutingEngine(
    strategies: new[] {
        new PerformanceBasedRoutingStrategy(),
        new LoadBalancingRoutingStrategy()
    }
);
```

## Configuração e Opções

### GraphRoutingOptions
```csharp
var options = new GraphRoutingOptions
{
    EnableDynamicRouting = true,
    MaxRoutingAttempts = 3,
    RoutingTimeout = TimeSpan.FromSeconds(30),
    FallbackStrategy = RoutingFallbackStrategy.Random
};
```

### Políticas de Roteamento
- **Retry Policy**: Tentativas múltiplas em caso de falha
- **Circuit Breaker**: Interrupção temporária em caso de problemas
- **Load Balancing**: Distribuição equilibrada de carga

## Monitoramento e Debug

### Métricas de Roteamento
- **Tempo de Decisão**: Latência para determinar o próximo nó
- **Taxa de Sucesso**: Percentual de roteamentos bem-sucedidos
- **Distribuição de Caminhos**: Frequência de uso de cada rota

### Debug de Roteamento
```csharp
var debugger = new ConditionalDebugger();
debugger.EnableRoutingLogging = true;
debugger.LogRoutingDecisions = true;
```

## Veja Também

- [Nós Condicionais](../how-to/conditional-nodes.md)
- [Roteamento Avançado](../how-to/advanced-routing.md)
- [Exemplos de Roteamento](../examples/dynamic-routing.md)
- [Exemplos de Roteamento Avançado](../examples/advanced-routing.md)
- [Nós](../concepts/node-types.md)
- [Execução](../concepts/execution-model.md)

## Referências

- `ConditionalEdge`: Classe para criar arestas com condições
- `DynamicRoutingEngine`: Motor de roteamento adaptativo
- `RoutingStrategies`: Estratégias de roteamento predefinidas
- `GraphRoutingOptions`: Configurações de roteamento
- `ConditionalDebugger`: Ferramentas de debug para roteamento
