# Changelog

Este documento registra as principais mudanças e melhorias em cada versão do SemanticKernel.Graph.

## [Unreleased] - Em Desenvolvimento

### Adicionado
- Sistema de documentação completo com MkDocs
- Páginas de conceitos para roteamento, visualização, execução, grafos e nós
- Guias práticos para todas as funcionalidades principais
- Exemplos abrangentes para todos os padrões e recursos

### Alterado
- Reestruturação completa da documentação
- Padronização de todas as páginas com seções "Conceitos e Técnicas" e "Referências"

## [0.1.0] - Versão Inicial

### Adicionado
- Estrutura base do projeto SemanticKernel.Graph
- Sistema de execução de grafos com nós e arestas condicionais
- Suporte a checkpointing e recuperação de estado
- Execução streaming com eventos em tempo real
- Sistema de métricas e observabilidade
- Integração com Semantic Kernel existente
- Suporte a múltiplos tipos de nós (função, condicional, raciocínio, loop)
- Sistema de roteamento dinâmico e estratégias de roteamento
- Visualização de grafos em múltiplos formatos (DOT, Mermaid, JSON)
- Sistema de templates para workflows comuns
- Suporte a multi-agente e coordenação
- Sistema de Human-in-the-Loop (HITL)
- Integração com ferramentas REST e APIs externas
- Sistema de validação e compilação de grafos
- Políticas de erro e resiliência
- Sistema de governança de recursos e concorrência

### Arquitetura
- Design baseado em ADRs (Architecture Decision Records)
- Separação clara entre Core, Execution, State, Streaming e Integration
- Sistema de extensibilidade para plugins e customizações
- Suporte a execução distribuída e paralela

## [0.0.1] - Scaffolding Inicial

### Adicionado
- Estrutura inicial do projeto
- Configuração básica do MkDocs
- Páginas de documentação básicas
- Estrutura de diretórios para conceitos, guias e exemplos

---

## Como Contribuir

Para contribuir com o changelog:

1. **Adicione entradas** para todas as mudanças significativas
2. **Use categorias** claras: Adicionado, Alterado, Removido, Corrigido
3. **Mantenha consistência** com o formato existente
4. **Inclua detalhes** sobre breaking changes e migrações

## Histórico de Commits

Para mudanças detalhadas, consulte:
- [Repository Releases](https://github.com/your-org/semantic-kernel-graph/releases)
- [Commit History](https://github.com/your-org/semantic-kernel-graph/commits/main)

## Notas de Versão

### Breaking Changes
- **0.1.0**: Mudanças na API de execução para melhorar performance e usabilidade
- **0.0.1**: Estrutura inicial sem breaking changes

### Migrações
- **0.1.0**: Guia de migração disponível em [Migrations Guide](../migrations/index.md)

---

*Este changelog segue o [Keep a Changelog](https://keepachangelog.com/) e [Semantic Versioning](https://semver.org/).*
