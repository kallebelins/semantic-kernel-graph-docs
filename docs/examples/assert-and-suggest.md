# Assert and Suggest Example

This example demonstrates how to enforce constraints on LLM outputs, validate content against business rules, and provide actionable suggestions for corrections.

## Objective

Learn how to implement content validation and suggestion workflows in graph-based systems to:
- Enforce business constraints and content policies on LLM outputs
- Validate content quality and compliance automatically
- Generate actionable suggestions for content improvements
- Implement feedback loops for continuous content quality improvement
- Handle constraint violations gracefully with fallback mechanisms

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Node Types](../concepts/node-types.md)
- Familiarity with [State Management](../concepts/state.md)

## Key Components

### Concepts and Techniques

- **Content Validation**: Automated checking of content against predefined constraints
- **Constraint Enforcement**: Business rule validation with clear error reporting
- **Suggestion Generation**: Actionable recommendations for content improvement
- **Feedback Loops**: Continuous improvement through validation and correction cycles
- **State Management**: Tracking validation results and suggestions through graph execution

### Core Classes

- `FunctionGraphNode`: Nodes for content generation, validation, and rewriting
- `KernelFunctionFactory`: Factory for creating kernel functions from methods
- `GraphExecutor`: Executor for running validation workflows
- `GraphState`: State management for validation results and suggestions
- `KernelArguments`: Input/output management for graph execution

## Running the Example

### Command Line

```bash
# Navigate to examples project
cd semantic-kernel-graph/src/SemanticKernel.Graph.Examples

# Run the Assert and Suggest example
dotnet run -- --example assert-and-suggest
```

### Programmatic Execution

```csharp
// Run the example directly
await AssertAndSuggestExample.RunAsync();

// Or run with custom kernel
var kernel = CreateCustomKernel();
await AssertAndSuggestExample.RunAsync();
```

## Step-by-Step Implementation

### 1. Creating the Validation Graph

The example creates a simple graph with three main nodes: draft generation, validation, and rewriting.

```csharp
// Create a simple graph
var graph = new GraphExecutor("AssertAndSuggest", "Validate output and suggest fixes");

// 1) Draft node: simulates an LLM draft that violates constraints
var draftNode = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod(
        () =>
        {
            // Intentionally violates constraints: contains "free" and is too long
            var draft = "Title: Super Gadget Pro Max\n" +
                        "Summary: This is a free, absolutely unbeatable gadget with unlimited features, " +
                        "best in class performance, and a comprehensive set of accessories included for everyone right now.";
            return draft;
        },
        functionName: "generate_draft",
        description: "Generates an initial draft (simulated LLM output)"
    ),
    nodeId: "draft",
    description: "Draft generation")
    .StoreResultAs("draft_output");
```

### 2. Implementing Content Validation

The validation node checks content against business constraints and generates suggestions.

```csharp
// 2) Validate node: asserts constraints and emits suggestions into state
var validateNode = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var text = args.TryGetValue("draft_output", out var o) ? o?.ToString() ?? string.Empty : string.Empty;
            var (valid, errors, suggestions) = ValidateConstraints(text);

            args["assert_valid"] = valid;
            if (!valid)
            {
                args["assert_errors"] = string.Join(" | ", errors);
                args["suggestions"] = string.Join(" | ", suggestions);
            }
            else
            {
                args["assert_errors"] = string.Empty;
                args["suggestions"] = string.Empty;
            }

            return valid ? "valid" : "invalid";
        },
        functionName: "validate_output",
        description: "Validates output against constraints and provides suggestions"
    ),
    nodeId: "validate",
    description: "Validation")
    .StoreResultAs("validation_result");
```

### 3. Implementing Content Rewriting

The rewrite node applies suggestions to produce corrected content.

```csharp
// 3) Rewrite node: applies suggestions to produce a corrected version
var rewriteNode = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var text = args.TryGetValue("draft_output", out var o) ? o?.ToString() ?? string.Empty : string.Empty;
            var suggestions = args.TryGetValue("suggestions", out var s) ? s?.ToString() ?? string.Empty : string.Empty;
            var fixedText = ApplySuggestions(text);
            args["rewritten_output"] = fixedText;

            // Re-validate to demonstrate closure of the loop
            var (valid, errors, _) = ValidateConstraints(fixedText);
            args["assert_valid"] = valid;
            args["assert_errors"] = valid ? string.Empty : string.Join(" | ", errors);
            return fixedText;
        },
        functionName: "rewrite_with_suggestions",
        description: "Produces a corrected rewrite using the suggestions"
    ),
    nodeId: "rewrite",
    description: "Rewrite");
```

### 4. Content Presentation and Results

The present node displays the final results and validation status.

```csharp
// 4) Present node: renders the final result
var presentNode = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var original = args.TryGetValue("draft_output", out var o) ? o?.ToString() ?? string.Empty : string.Empty;
            var rewritten = args.TryGetValue("rewritten_output", out var r) ? r?.ToString() ?? string.Empty : string.Empty;
            var errors = args.TryGetValue("assert_errors", out var e) ? e?.ToString() ?? string.Empty : string.Empty;
            var suggestions = args.TryGetValue("suggestions", out var s) ? s?.ToString() ?? string.Empty : string.Empty;
            var finalValid = args.TryGetValue("assert_valid", out var v) && v is bool b && b;

            Console.WriteLine("\n📋 Content Validation Results:");
            Console.WriteLine("=" + new string('=', 40));

            Console.WriteLine("\n📝 Original Draft:");
            Console.WriteLine(original);

            if (!string.IsNullOrEmpty(errors))
            {
                Console.WriteLine("\n❌ Validation Errors:");
                Console.WriteLine(errors);
            }

            if (!string.IsNullOrEmpty(suggestions))
            {
                Console.WriteLine("\n💡 Suggestions:");
                Console.WriteLine(suggestions);
            }

            Console.WriteLine("\n✅ Corrected Version:");
            Console.WriteLine(rewritten);

            Console.WriteLine($"\n🎯 Final Validation: {(finalValid ? "PASSED" : "FAILED")}");

            return "Content validation and correction completed";
        },
        functionName: "present_results",
        description: "Presents validation results and corrected content"
    ),
    nodeId: "present",
    description: "Results Presentation");
```

### 5. Graph Assembly and Execution

The nodes are connected to form a validation workflow.

```csharp
// Assemble the graph
graph.AddNode(draftNode);
graph.AddNode(validateNode);
graph.AddNode(rewriteNode);
graph.AddNode(presentNode);

// Connect the workflow
graph.Connect(draftNode.NodeId, validateNode.NodeId);
graph.Connect(validateNode.NodeId, rewriteNode.NodeId);
graph.Connect(rewriteNode.NodeId, presentNode.NodeId);

// Set the start node
graph.SetStartNode(draftNode.NodeId);

// Execute the validation workflow
var args = new KernelArguments();
var result = await graph.ExecuteAsync(kernel, args);
```

### 6. Constraint Validation Logic

The example implements specific business constraints for content validation.

```csharp
private static (bool valid, string[] errors, string[] suggestions) ValidateConstraints(string text)
{
    var errors = new List<string>();
    var suggestions = new List<string>();

    // Constraint 1: No promotional language
    if (text.Contains("free", StringComparison.OrdinalIgnoreCase) ||
        text.Contains("unlimited", StringComparison.OrdinalIgnoreCase) ||
        text.Contains("best in class", StringComparison.OrdinalIgnoreCase))
    {
        errors.Add("Contains promotional language");
        suggestions.Add("Remove promotional terms like 'free', 'unlimited', 'best in class'");
    }

    // Constraint 2: Length limits
    if (text.Length > 200)
    {
        errors.Add("Content too long");
        suggestions.Add("Keep content concise, under 200 characters");
    }

    // Constraint 3: No urgency language
    if (text.Contains("right now", StringComparison.OrdinalIgnoreCase) ||
        text.Contains("immediately", StringComparison.OrdinalIgnoreCase))
    {
        errors.Add("Contains urgency language");
        suggestions.Add("Remove urgency terms like 'right now', 'immediately'");
    }

    // Constraint 4: Professional tone
    if (text.Contains("absolutely unbeatable", StringComparison.OrdinalIgnoreCase))
    {
        errors.Add("Unprofessional tone");
        suggestions.Add("Use professional, factual language");
    }

    return (errors.Count == 0, errors.ToArray(), suggestions.ToArray());
}
```

### 7. Suggestion Application Logic

The rewrite logic applies suggestions to correct content.

```csharp
private static string ApplySuggestions(string text)
{
    var corrected = text;

    // Apply suggestion 1: Remove promotional language
    corrected = corrected.Replace("free", "premium", StringComparison.OrdinalIgnoreCase);
    corrected = corrected.Replace("unlimited", "comprehensive", StringComparison.OrdinalIgnoreCase);
    corrected = corrected.Replace("best in class", "high-quality", StringComparison.OrdinalIgnoreCase);

    // Apply suggestion 2: Reduce length
    if (corrected.Length > 200)
    {
        corrected = corrected.Substring(0, 197) + "...";
    }

    // Apply suggestion 3: Remove urgency language
    corrected = corrected.Replace("right now", "available", StringComparison.OrdinalIgnoreCase);
    corrected = corrected.Replace("immediately", "promptly", StringComparison.OrdinalIgnoreCase);

    // Apply suggestion 4: Professional tone
    corrected = corrected.Replace("absolutely unbeatable", "highly competitive", StringComparison.OrdinalIgnoreCase);

    return corrected;
}
```

## Expected Output

The example produces comprehensive output showing:

- 📝 Original draft content with constraint violations
- ❌ Validation errors and business rule violations
- 💡 Actionable suggestions for content improvement
- ✅ Corrected version with constraints applied
- 🎯 Final validation status (PASSED/FAILED)
- 📋 Complete validation workflow results

## Troubleshooting

### Common Issues

1. **Constraint Validation Failures**: Ensure constraint logic handles edge cases and null values
2. **Suggestion Application Errors**: Verify suggestion logic doesn't introduce new violations
3. **State Management Issues**: Check that validation results are properly stored and retrieved
4. **Content Length Issues**: Monitor content length constraints and truncation logic

### Debugging Tips

- Enable detailed logging to trace validation steps
- Monitor state transitions between validation nodes
- Verify constraint logic handles all content types
- Check suggestion application for completeness

## See Also

- [Content Validation](../how-to/content-validation.md)
- [State Management](../concepts/state.md)
- [Node Types](../concepts/node-types.md)
- [Graph Concepts](../concepts/graph-concepts.md)
- [Error Handling](../how-to/error-handling-and-resilience.md)
