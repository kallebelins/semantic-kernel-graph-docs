# FAQ - Perguntas Frequentes

Perguntas e respostas comuns sobre o SemanticKernel.Graph.

## Conceitos Básicos

### O que é o SemanticKernel.Graph?
**SemanticKernel.Graph** é uma extensão do Semantic Kernel que adiciona capacidades de execução de grafos computacionais, permitindo criar workflows complexos com nós, arestas condicionais e execução controlada.

### Como se relaciona com o Semantic Kernel?
É uma extensão que mantém total compatibilidade com o Semantic Kernel existente, adicionando capacidades de orquestração de grafos sem alterar a funcionalidade base.

### Qual a diferença para LangGraph?
Oferece funcionalidades similares ao LangGraph, mas com foco em integração nativa com o ecossistema .NET e Semantic Kernel, otimizado para aplicações empresariais.

## Requisitos e Compatibilidade

### Quais versões do .NET são suportadas?
**NET 8+** é a versão mínima recomendada, com suporte completo a todas as funcionalidades modernas.

### Funciona com código SK existente?
**Sim**, com mudanças mínimas. Aproveita plugins, serviços e conectores existentes, apenas adicionando capacidades de grafo.

### Precisa de serviços externos?
**Não obrigatoriamente**. Funciona com configuração mínima, mas pode integrar com serviços de telemetria, memória e monitoramento quando disponíveis.

## Funcionalidades

### Streaming é suportado?
**Sim**, com reconexão automática, buffering inteligente e controle de backpressure.

### Checkpointing funciona em produção?
**Sim**, com suporte a persistência, compressão, versionamento e recuperação robusta.

### Suporta execução paralela?
**Sim**, com scheduler determinístico, controle de concorrência e merge de estado.

### Visualização é interativa?
**Sim**, com export para DOT, Mermaid, JSON e overlays de execução em tempo real.

## Integração e Desenvolvimento

### Como integrar com aplicações existentes?
```csharp
// Adicionar suporte a grafos
builder.AddGraphSupport();

// Usar normalmente
var executor = kernel.GetRequiredService<IGraphExecutor>();
```

### Suporta plugins customizados?
**Sim**, todos os plugins SK existentes funcionam como nós de grafo.

### Como debugar grafos complexos?
- Sessões de debug interativas
- Breakpoints em nós específicos
- Visualização em tempo real
- Métricas detalhadas por nó

### Existe suporte a testes?
**Sim**, com framework de testes integrado e mocks para desenvolvimento.

## Performance e Escalabilidade

### Qual o overhead de performance?
**Mínimo** - apenas o necessário para orquestração, sem impacto na execução dos nós.

### Suporta execução distribuída?
**Sim**, com suporte a múltiplos processos e máquinas.

### Como lidar com falhas?
- Políticas de retry configuráveis
- Circuit breakers
- Fallbacks automáticos
- Recuperação por checkpoint

## Configuração e Deploy

### Precisa de configuração especial?
**Não**, funciona com configuração zero, mas oferece opções avançadas quando necessário.

### Suporta containers Docker?
**Sim**, com suporte completo a ambientes containerizados.

### Como monitorar em produção?
- Métricas nativas (.NET Metrics)
- Logging estruturado
- Integração com Application Insights
- Export para Prometheus/Grafana

## Suporte e Comunidade

### Onde encontrar ajuda?
- [Documentação](../index.md)
- [Exemplos](../examples/index.md)
- [GitHub Issues](https://github.com/your-org/semantic-kernel-graph/issues)
- [Discussions](https://github.com/your-org/semantic-kernel-graph/discussions)

### Como contribuir?
- Reportar bugs
- Sugerir melhorias
- Contribuir com exemplos
- Melhorar documentação

### Existe roadmap público?
**Sim**, disponível em [Roadmap](../architecture/implementation-roadmap.md).

## Casos de Uso

### Para que tipos de aplicações é ideal?
- Workflows de IA complexos
- Pipelines de processamento de dados
- Sistemas de decisão automatizada
- Orquestração de microserviços
- Aplicações de chatbot avançadas

### Exemplos de uso em produção?
- Análise de documentos automatizada
- Classificação de conteúdo em escala
- Sistemas de recomendação
- Workflows de aprovação
- Processamento de formulários

---

## Veja Também

- [Getting Started](../getting-started.md)
- [Installation](../installation.md)
- [Examples](../examples/index.md)
- [Architecture](../architecture/index.md)
- [Troubleshooting](../troubleshooting.md)

## Referências

- [Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel/)
- [LangGraph Python](https://langchain-ai.github.io/langgraph/)
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
