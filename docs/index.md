# Semantic Kernel Graph

Bem-vindo à documentação do SemanticKernel.Graph. Este site espelha a estrutura da documentação do LangGraph, focando em uma implementação .NET enxuta e pragmática, totalmente integrada com o Semantic Kernel.

## Conceitos e Técnicas

**SemanticKernel.Graph**: Extensão do Semantic Kernel que adiciona capacidades de execução de grafos computacionais, permitindo criar workflows complexos com orquestração inteligente.

**Grafos Computacionais**: Estruturas que representam fluxos de trabalho através de nós conectados por arestas, com execução controlada e roteamento condicional.

**Integração Nativa**: Funciona como extensão do Semantic Kernel existente, mantendo total compatibilidade e aproveitando plugins e serviços existentes.

## O que o SemanticKernel.Graph Resolve

### Problemas de Orquestração
- **Workflows Complexos**: Criação de pipelines de IA com múltiplos passos
- **Roteamento Inteligente**: Decisões baseadas em estado e contexto
- **Controle de Fluxo**: Loops, condicionais e iterações controladas
- **Composição**: Reutilização de componentes e subgrafos

### Desafios de Produção
- **Escalabilidade**: Execução paralela e distribuída
- **Resiliência**: Checkpointing, retry e circuit breakers
- **Observabilidade**: Métricas, logging e visualização em tempo real
- **Manutenibilidade**: Debug, inspeção e documentação automática

## Funcionalidades Principais

### 🚀 **Execução de Grafos**
- Nós de função, condicionais, raciocínio e loops
- Arestas com condições e roteamento dinâmico
- Execução sequencial, paralela e distribuída
- Scheduler determinístico para reprodutibilidade

### 🔄 **Streaming e Eventos**
- Execução streaming com eventos em tempo real
- Reconexão automática e controle de backpressure
- Consumo assíncrono de eventos de execução
- Integração com sistemas de mensageria

### 💾 **Estado e Persistência**
- Sistema de estado tipado e validado
- Checkpointing automático e manual
- Serialização e compressão de estado
- Recuperação e replay de execuções

### 🎯 **Roteamento Inteligente**
- Roteamento baseado em condições de estado
- Estratégias dinâmicas e adaptativas
- Similaridade semântica para decisões
- Aprendizado por feedback

### 👥 **Human-in-the-Loop**
- Nós de aprovação humana
- Canais múltiplos (console, web, email)
- Timeouts e políticas de SLA
- Auditoria e rastreamento de decisões

### 🔧 **Integração e Extensibilidade**
- Ferramentas REST integradas
- Sistema de plugins extensível
- Integração com serviços externos
- Templates para workflows comuns

## Comece em Minutos

### 1. **Instalação Rápida**
```bash
dotnet add package SemanticKernel.Graph
```

### 2. **Primeiro Grafo**
```csharp
builder.AddGraphSupport();
var kernel = builder.Build();

var graph = new Graph
{
    StartNode = startNode,
    Nodes = new[] { startNode, processNode, endNode }
};

var result = await kernel.GetRequiredService<IGraphExecutor>()
    .ExecuteAsync(graph, arguments);
```

### 3. **Explore Exemplos**
```bash
cd examples
dotnet run -- --list
dotnet run -- --example chatbot
```

## Estrutura da Documentação

### 📚 **Get Started**
- [Instalação](../installation.md) - Configuração e requisitos
- [Primeiro Grafo](../first-graph-5-minutes.md) - Hello World em 5 minutos
- [Quickstarts](../index.md#quickstarts) - Guias rápidos por funcionalidade

### 🧠 **Conceitos**
- [Grafos](../concepts/graphs.md) - Estrutura e componentes
- [Nós](../concepts/nodes.md) - Tipos e ciclo de vida
- [Execução](../concepts/execution.md) - Modos e controle
- [Roteamento](../concepts/routing.md) - Estratégias e condições
- [Estado](../concepts/state.md) - Gerenciamento e persistência

### 🛠️ **How-To Guides**
- [Construindo Grafos](../how-to/build-a-graph.md) - Criação e validação
- [Nós Condicionais](../how-to/conditional-nodes.md) - Roteamento dinâmico
- [Checkpointing](../how-to/checkpointing.md) - Persistência e recuperação
- [Streaming](../how-to/streaming.md) - Execução em tempo real
- [Métricas](../how-to/metrics-and-observability.md) - Monitoramento

### 📖 **Reference**
- [APIs](../api/index.md) - Documentação completa das APIs
- [Configurações](../api/configuration.md) - Opções e parâmetros
- [Tipos](../api/types.md) - Estruturas de dados
- [Extensões](../api/extensions.md) - Métodos de extensão

### 🎯 **Examples**
- [Índice](../examples/index.md) - Todos os exemplos disponíveis
- [Chatbot](../examples/chatbot.md) - Conversação com memória
- [ReAct](../examples/react-agent.md) - Raciocínio e ação
- [Multi-Agente](../examples/multi-agent.md) - Coordenação de agentes
- [Documentos](../examples/document-analysis-pipeline.md) - Análise de documentos

### 🏗️ **Architecture**
- [ADRs](../architecture/index.md) - Decisões de arquitetura
- [Roadmap](../architecture/implementation-roadmap.md) - Planejamento futuro
- [Padrões](../patterns/index.md) - Padrões de design

## Casos de Uso

### 🤖 **Agentes de IA**
- Chatbots com memória e contexto
- Agentes de raciocínio (ReAct, Chain of Thought)
- Coordenação de múltiplos agentes
- Workflows de decisão automatizada

### 📄 **Processamento de Documentos**
- Análise e classificação automática
- Extração de informações estruturadas
- Pipelines de validação e aprovação
- Processamento em lote com checkpoints

### 🔍 **Sistemas de Recomendação**
- Roteamento baseado em similaridade
- Aprendizado por feedback do usuário
- Filtros condicionais e personalização
- Otimização contínua de resultados

### 🚀 **Orquestração de Microserviços**
- Coordenação de chamadas de API
- Circuit breakers e retry policies
- Balanceamento de carga inteligente
- Monitoramento e observabilidade

## Comparação com Alternativas

| Feature | SemanticKernel.Graph | LangGraph | Temporal | Durable Functions |
|---------|----------------------|-----------|----------|-------------------|
| **Integração SK** | ✅ Nativa | ❌ Python | ❌ Java/Go | ❌ Azure |
| **Performance** | ✅ .NET Nativo | ⚠️ Python | ✅ JVM | ✅ Azure Runtime |
| **Checkpointing** | ✅ Avançado | ✅ Básico | ✅ Robusto | ✅ Nativo |
| **Streaming** | ✅ Eventos | ✅ Streaming | ❌ | ⚠️ Limitado |
| **Visualização** | ✅ Tempo Real | ✅ Estática | ❌ | ❌ |
| **HITL** | ✅ Múltiplos Canais | ⚠️ Básico | ❌ | ❌ |

## Comunidade e Suporte

### 🌟 **Contribuir**
- [GitHub Repository](https://github.com/your-org/semantic-kernel-graph)
- [Issues](https://github.com/your-org/semantic-kernel-graph/issues)
- [Discussions](https://github.com/your-org/semantic-kernel-graph/discussions)
- [Contributing Guide](https://github.com/your-org/semantic-kernel-graph/CONTRIBUTING.md)

### 📚 **Recursos Adicionais**
- [Blog](https://your-blog.com/semantic-kernel-graph)
- [Videos](https://your-channel.com/semantic-kernel-graph)
- [Workshops](https://your-events.com/semantic-kernel-graph)
- [Slack](https://your-slack.com/semantic-kernel-graph)

### 🆘 **Precisa de Ajuda?**
- [FAQ](../faq.md) - Perguntas frequentes
- [Troubleshooting](../troubleshooting.md) - Resolução de problemas
- [Examples](../examples/index.md) - Exemplos práticos
- [API Reference](../api/index.md) - Documentação técnica

## Quickstarts

### ⚡ **5 Minutos**
- [Primeiro Grafo](../first-graph-5-minutes.md) - Hello World básico
- [Estado](../state-quickstart.md) - Gerenciamento de variáveis
- [Condicionais](../conditional-nodes-quickstart.md) - Roteamento simples
- [Streaming](../streaming-quickstart.md) - Eventos em tempo real

### 🚀 **15 Minutos**
- [Checkpointing](../checkpointing-quickstart.md) - Persistência de estado
- [Métricas](../metrics-logging-quickstart.md) - Monitoramento básico
- [ReAct/CoT](../react-cot-quickstart.md) - Padrões de raciocínio

### 🎯 **30 Minutos**
- [Tutorial de Condicionais](../conditional-nodes-tutorial.md) - Roteamento avançado
- [Tutorial de Estado](../state-tutorial.md) - Gerenciamento complexo
- [Multi-Agente](../examples/multi-agent.md) - Coordenação de agentes

---

> **💡 Dica**: Esta documentação usa Material for MkDocs. Use a navegação à esquerda e a barra de pesquisa para encontrar tópicos rapidamente.

> **🚀 Pronto para começar?** Vá para [Instalação](../installation.md) ou [Primeiro Grafo](../first-graph-5-minutes.md) para começar em minutos!
