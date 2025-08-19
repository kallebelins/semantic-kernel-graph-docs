# Template: Seções Padrão para Exemplos

Este template define as seções padrão que devem ser incluídas em todos os exemplos do Semantic Kernel Graph, conforme especificado no item 4 do backlog de documentação.

## Estrutura Padrão

### 1. Cabeçalho e Objetivo
```markdown
# [Nome do Exemplo]

Este exemplo demonstra [descrição breve do que o exemplo faz].

## Objetivo

Aprenda como implementar [funcionalidade] em workflows baseados em grafo para:
- [Benefício 1]
- [Benefício 2]
- [Benefício 3]
- [Benefício 4]
```

### 2. Pré-requisitos
```markdown
## Pré-requisitos

- **.NET 8.0** ou posterior
- **OpenAI API Key** configurado em `appsettings.json`
- **Pacote Semantic Kernel Graph** instalado
- Compreensão básica de [Conceitos de Grafo](../concepts/graph-concepts.md) e [Modelo de Execução](../concepts/execution-model.md)
- Familiaridade com [Guia relacionado](../how-to/guia-relacionado.md) (quando aplicável)
```

### 3. Conceitos e Técnicas
```markdown
## Conceitos e Técnicas

Esta seção define os componentes e padrões utilizados no exemplo, com links para a documentação de referência.

### Definições dos Componentes

- **[Nome do Componente]**: [Definição clara e concisa]
- **[Padrão/Conceito]**: [Explicação do padrão ou conceito utilizado]
- **[Técnica]**: [Descrição da técnica implementada]

### Classes Principais

- `[ClassePrincipal]`: [Descrição da funcionalidade principal]
- `[ClasseSecundaria]`: [Descrição da funcionalidade secundária]
- `[Interface]`: [Descrição da interface ou contrato]
```

### 4. Executando o Exemplo
```markdown
## Executando o Exemplo

### Como Usar

Este template fornece uma estrutura padrão para documentar exemplos. Use os códigos abaixo como referência para implementar os padrões em suas próprias aplicações.
```

### 5. Implementação Passo a Passo
```markdown
## Implementação Passo a Passo

### 1. [Primeiro Passo]

Este exemplo demonstra [descrição do primeiro passo].

```csharp
// Código do primeiro passo
var exemplo = new ExemploClass();
// ... mais código
```

### 2. [Segundo Passo]

[Descrição do segundo passo]

```csharp
// Código do segundo passo
// ... código relevante
```

### 3. [Terceiro Passo]

[Descrição do terceiro passo]

```csharp
// Código do terceiro passo
// ... código relevante
```
```

### 6. Saída Esperada
```markdown
## Saída Esperada

O exemplo produz uma saída mostrando:

- ✅ [Resultado esperado 1]
- 🎯 [Resultado esperado 2]
- 📊 [Resultado esperado 3]
- 🔍 [Resultado esperado 4]
```

### 7. Troubleshooting
```markdown
## Troubleshooting

### Problemas Comuns

1. **[Problema 1]**: [Descrição do problema]
   - **Sintoma**: [Como identificar o problema]
   - **Causa**: [Causa provável]
   - **Solução**: [Passos para resolver]

2. **[Problema 2]**: [Descrição do problema]
   - **Sintoma**: [Como identificar o problema]
   - **Causa**: [Causa provável]
   - **Solução**: [Passos para resolver]

### Dicas de Debug

- [Dica 1 de debug]
- [Dica 2 de debug]
- [Dica 3 de debug]
```

### 8. Padrões Avançados (quando aplicável)
```markdown
## Padrões Avançados

### [Padrão Avançado 1]

```csharp
// Implementação do padrão avançado
var padraoAvancado = new PadraoAvancado();
// ... código do padrão
```

### [Padrão Avançado 2]

```csharp
// Implementação do segundo padrão
// ... código relevante
```
```

### 9. Exemplos Relacionados
```markdown
## Exemplos Relacionados

- [Exemplo Relacionado 1](./exemplo-relacionado-1.md): [Breve descrição]
- [Exemplo Relacionado 2](./exemplo-relacionado-2.md): [Breve descrição]
- [Exemplo Relacionado 3](./exemplo-relacionado-3.md): [Breve descrição]
```

### 10. Veja Também (Links para Referência e Guias)
```markdown
## Veja Também

- [Conceitos Relacionados](../concepts/conceito-relacionado.md): [Descrição do que encontrar]
- [Guia de Implementação](../how-to/guia-relacionado.md): [Descrição do guia]
- [Referência da API](../api/): [Descrição da documentação da API]
- [Monitoramento de Performance](../how-to/metrics-and-observability.md): [Descrição das métricas]
```

## Checklist de Implementação

Para cada exemplo, verificar se contém:

- [ ] **Conceitos e Técnicas**: Definição dos componentes e padrões usados (com links para Reference)
- [ ] **Referências**: Links para APIs e Guides relacionados
- [ ] **Pré-requisitos**: Requisitos técnicos e conhecimentos necessários
- [ ] **Passos**: Implementação passo a passo com snippets principais
- [ ] **Saída Esperada**: Resultados esperados do exemplo
- [ ] **Troubleshooting**: Problemas comuns e soluções
- [ ] **Variantes Sugeridas**: Padrões avançados quando aplicável
- [ ] **Links Cruzados**: Seção "Veja Também" com links para Reference e Guides

## Notas de Implementação

1. **Consistência**: Manter o mesmo formato e estilo em todos os exemplos
2. **Links Ativos**: Todos os links internos devem funcionar e apontar para páginas existentes
3. **Código Executável**: Snippets de código devem compilar e refletir as APIs atuais
4. **Navegação**: Facilitar a navegação entre exemplos, guias e referência da API
5. **Busca**: Incluir termos relevantes para facilitar a busca na documentação
