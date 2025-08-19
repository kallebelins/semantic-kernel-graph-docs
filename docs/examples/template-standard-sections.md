# Template: Standard Sections for Examples

This template defines the standard sections that should be included in all Semantic Kernel Graph examples, as specified in item 4 of the documentation backlog.

## Standard Structure

### 1. Header and Objective
```markdown
# [Example Name]

This example demonstrates [brief description of what the example does].

## Objective

Learn how to implement [functionality] in graph-based workflows for:
- [Benefit 1]
- [Benefit 2]
- [Benefit 3]
- [Benefit 4]
```

### 2. Prerequisites
```markdown
## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Execution Model](../concepts/execution-model.md)
- Familiarity with [Related Guide](../how-to/related-guide.md) (when applicable)
```

### 3. Concepts and Techniques
```markdown
## Concepts and Techniques

This section defines the components and patterns used in the example, with links to reference documentation.

### Component Definitions

- **[Component Name]**: [Clear and concise definition]
- **[Pattern/Concept]**: [Explanation of the pattern or concept used]
- **[Technique]**: [Description of the implemented technique]

### Main Classes

- `[MainClass]`: [Description of main functionality]
- `[SecondaryClass]`: [Description of secondary functionality]
- `[Interface]`: [Description of interface or contract]
```

### 4. Running the Example
```markdown
## Running the Example

### How to Use

This template provides a standard structure for documenting examples. Use the codes below as reference to implement the patterns in your own applications.
```

### 5. Step-by-Step Implementation
```markdown
## Step-by-Step Implementation

### 1. [First Step]

This example demonstrates [description of first step].

```csharp
// Code for first step
var example = new ExampleClass();
// ... more code
```

### 2. [Second Step]

[Description of second step]

```csharp
// Code for second step
// ... relevant code
```

### 3. [Third Step]

[Description of third step]

```csharp
// Code for third step
// ... relevant code
```
```

### 6. Expected Output
```markdown
## Expected Output

The example produces output showing:

- ✅ [Expected result 1]
```

### 7. Troubleshooting
```markdown
## Troubleshooting

### Common Problems

1. **[Problem 1]**: [Description of problem]
   - **Symptom**: [How to identify the problem]
   - **Cause**: [Probable cause]
   - **Solution**: [Steps to resolve]

2. **[Problem 2]**: [Description of problem]
   - **Symptom**: [How to identify the problem]
   - **Cause**: [Probable cause]
   - **Solution**: [Steps to resolve]

### Debug Tips

- [Debug tip 1]
- [Debug tip 2]
- [Debug tip 3]
```

### 8. Advanced Patterns (when applicable)
```markdown
## Advanced Patterns

### [Advanced Pattern 1]

```csharp
// Implementation of advanced pattern
var advancedPattern = new AdvancedPattern();
// ... pattern code
```

### [Advanced Pattern 2]

```csharp
// Implementation of second pattern
// ... relevant code
```
```

### 9. Related Examples
```markdown
## Related Examples

- [Related Example 1](./related-example-1.md): [Brief description]
- [Related Example 2](./related-example-2.md): [Brief description]
- [Related Example 3](./related-example-3.md): [Brief description]
```

### 10. See Also (Links for Reference and Guides)
```markdown
## See Also

- [Related Concepts](../concepts/related-concept.md): [Description of what to find]
- [Implementation Guide](../how-to/related-guide.md): [Description of guide]
- [API Reference](../api/): [Description of API documentation]
- [Performance Monitoring](../how-to/metrics-and-observability.md): [Description of metrics]
```

## Implementation Checklist

For each example, check if it contains:

- [ ] **Concepts and Techniques**: Definition of components and patterns used (with links to Reference)
- [ ] **References**: Links to related APIs and Guides
- [ ] **Prerequisites**: Technical requirements and required knowledge
- [ ] **Steps**: Step-by-step implementation with main snippets
- [ ] **Expected Output**: Expected results of the example
- [ ] **Troubleshooting**: Common problems and solutions
- [ ] **Suggested Variants**: Advanced patterns when applicable
- [ ] **Cross-linked Sections**: "See Also" section with links to Reference and Guides

## Implementation Notes

1. **Consistency**: Maintain the same format and style across all examples
2. **Active Links**: All internal links must work and point to existing pages
3. **Executable Code**: Code snippets must compile and reflect current APIs
4. **Navigation**: Facilitate navigation between examples, guides, and API reference
5. **Search**: Include relevant terms to facilitate search in documentation
