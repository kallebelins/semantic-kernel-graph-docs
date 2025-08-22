# Assert and Suggest Example

This example demonstrates how to enforce constraints on LLM outputs, validate content against business rules, and provide actionable suggestions for corrections.

## Objective

Learn how to implement content validation and suggestion workflows in graph-based systems to:
* Enforce business constraints and content policies on LLM outputs
* Validate content quality and compliance automatically
* Generate actionable suggestions for content improvements
* Implement feedback loops for continuous content quality improvement
* Handle constraint violations gracefully with fallback mechanisms

## Prerequisites

* **.NET 8.0** or later
* **OpenAI API Key** configured in `appsettings.json`
* **Semantic Kernel Graph package** installed
* Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Node Types](../concepts/node-types.md)
* Familiarity with [State Management](../concepts/state.md)

## Key Components

### Concepts and Techniques

* **Content Validation**: Automated checking of content against predefined constraints
* **Constraint Enforcement**: Business rule validation with clear error reporting
* **Suggestion Generation**: Actionable recommendations for content improvement
* **Feedback Loops**: Continuous improvement through validation and correction cycles
* **State Management**: Tracking validation results and suggestions through graph execution

### Core Classes

* `FunctionGraphNode`: Nodes for content generation, validation, and rewriting
* `KernelFunctionFactory`: Factory for creating kernel functions from methods
* `GraphExecutor`: Executor for running validation workflows
* `GraphState`: State management for validation results and suggestions
* `KernelArguments`: Input/output management for graph execution

## Running the Example

### Getting Started

This example demonstrates validation and suggestion patterns with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Creating the Validation Graph

The example creates a simple graph with three main nodes: draft generation, validation, and rewriting.

```csharp
// Create a graph executor that will host the validation workflow.
// This is a minimal, in-memory graph used for documentation and local testing.
var graph = new GraphExecutor("AssertAndSuggest", "Validate output and suggest fixes");

// 1) Draft node: simulate an LLM draft that intentionally violates constraints.
// The node stores its produced draft into the graph state under the key "draft_output".
var draftNode = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod(
        () =>
        {
            // Intentionally violating draft: promotional terms and an overly long summary
            var draft = "Title: Super Gadget Pro Max\n" +
                        "Summary: This is a free, absolutely unbeatable gadget with unlimited features, " +
                        "best in class performance, and a comprehensive set of accessories included for everyone right now.";
            return draft;
        },
        functionName: "generate_draft",
        description: "Generates an initial draft (simulated LLM output)"),
    nodeId: "draft",
    description: "Draft generation")
    .StoreResultAs("draft_output");
```

### 2. Implementing Content Validation

The validation node checks content against business constraints and generates suggestions.

```csharp
// 2) Validate node: inspects the draft, asserts constraints, and emits suggestions into state.
// It writes the validation result flags and textual suggestions into KernelArguments for downstream nodes.
var validateNode = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod((KernelArguments args) =>
    {
        var text = args.TryGetValue("draft_output", out var o) ? o?.ToString() ?? string.Empty : string.Empty;
        var (valid, errors, suggestions) = ValidateConstraints(text);

        // Store boolean validity and serialized messages in the shared arguments
        args["assert_valid"] = valid;
        args["assert_errors"] = string.Join(" | ", errors ?? Array.Empty<string>());
        args["suggestions"] = string.Join(" | ", suggestions ?? Array.Empty<string>());

        return valid ? "valid" : "invalid";
    }, functionName: "validate_output", description: "Validates output and provides suggestions"),
    nodeId: "validate", description: "Validation").StoreResultAs("validation_result");
```

### 3. Implementing Content Rewriting

The rewrite node applies suggestions to produce corrected content.

```csharp
// 3) Rewrite node: applies suggestions and writes the corrected draft back into state.
// After applying fixes, it re-runs validation to update the validity flag.
var rewriteNode = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod((KernelArguments args) =>
    {
        var text = args.TryGetValue("draft_output", out var o) ? o?.ToString() ?? string.Empty : string.Empty;
        var fixedText = ApplySuggestions(text);
        args["rewritten_output"] = fixedText;

        // Re-validate the corrected text to update assert_valid/assert_errors
        var (valid, errors, _) = ValidateConstraints(fixedText);
        args["assert_valid"] = valid;
        args["assert_errors"] = valid ? string.Empty : string.Join(" | ", errors ?? Array.Empty<string>());
        return fixedText;
    }, functionName: "rewrite_with_suggestions", description: "Produces a corrected rewrite"),
    nodeId: "rewrite", description: "Rewrite");
```

### 4. Content Presentation and Results

The present node displays the final results and validation status.

```csharp
// 4) Present node: prints the original draft, any validation errors, suggestions, and the rewritten text.
var presentNode = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod((KernelArguments args) =>
    {
        var original = args.TryGetValue("draft_output", out var o) ? o?.ToString() ?? string.Empty : string.Empty;
        var rewritten = args.TryGetValue("rewritten_output", out var r) ? r?.ToString() ?? string.Empty : string.Empty;
        var errors = args.TryGetValue("assert_errors", out var e) ? e?.ToString() ?? string.Empty : string.Empty;
        var suggestions = args.TryGetValue("suggestions", out var s) ? s?.ToString() ?? string.Empty : string.Empty;
        var finalValid = args.TryGetValue("assert_valid", out var v) && v is bool b && b;

        Console.WriteLine("\n📋 Content Validation Results:");
        Console.WriteLine(new string('=', 60));

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

        if (!string.IsNullOrWhiteSpace(rewritten))
        {
            Console.WriteLine("\n✅ Corrected Version:");
            Console.WriteLine(rewritten);
        }

        Console.WriteLine($"\n🎯 Final Validation: {(finalValid ? "PASSED" : "FAILED")}");
        return "Content validation and correction completed";
    }, functionName: "present_results", description: "Presents validation results"),
    nodeId: "present", description: "Results Presentation");
```

### 5. Graph Assembly and Execution

The nodes are connected to form a validation workflow.

```csharp
// Assemble the graph and wire the nodes together. Use clear, deterministic routing
// so the documentation example remains easy to follow.
graph.AddNode(draftNode);
graph.AddNode(validateNode);
graph.AddNode(rewriteNode);
graph.AddNode(presentNode);

// Routing: draft -> validate
graph.ConnectWhen(draftNode.NodeId, validateNode.NodeId, _ => true);
// If validation succeeds go straight to present
graph.ConnectWhen(validateNode.NodeId, presentNode.NodeId, ka => ka.TryGetValue("assert_valid", out var v) && v is bool vb && vb);
// Otherwise rewrite then present
graph.ConnectWhen(validateNode.NodeId, rewriteNode.NodeId, ka => !(ka.TryGetValue("assert_valid", out var v) && v is bool vb2 && vb2));
graph.ConnectWhen(rewriteNode.NodeId, presentNode.NodeId, _ => true);

// Execute the validation workflow
var args = new KernelArguments();
var result = await graph.ExecuteAsync(kernel, args);
```

### 6. Constraint Validation Logic

The example implements specific business constraints for content validation.

```csharp
private static (bool valid, string[] errors, string[] suggestions) ValidateConstraints(string text)
{
    // Return collections with clear messages. Keep logic simple and deterministic for docs.
    var errors = new List<string>();
    var suggestions = new List<string>();

    if (string.IsNullOrWhiteSpace(text))
    {
        errors.Add("Content is empty");
        suggestions.Add("Provide a draft containing a Title and Summary lines");
        return (false, errors.ToArray(), suggestions.ToArray());
    }

    // Promotional language checks
    if (text.Contains("free", StringComparison.OrdinalIgnoreCase) ||
        text.Contains("unlimited", StringComparison.OrdinalIgnoreCase) ||
        text.Contains("best in class", StringComparison.OrdinalIgnoreCase))
    {
        errors.Add("Contains promotional language");
        suggestions.Add("Remove promotional terms like 'free', 'unlimited', or 'best in class'");
    }

    // Length limit (documentation example uses a generous threshold)
    if (text.Length > 500)
    {
        errors.Add($"Content too long ({text.Length} characters)");
        suggestions.Add("Keep content concise, consider shortening the Summary");
    }

    // Urgency language
    if (text.Contains("right now", StringComparison.OrdinalIgnoreCase) ||
        text.Contains("immediately", StringComparison.OrdinalIgnoreCase))
    {
        errors.Add("Contains urgency language");
        suggestions.Add("Avoid urgency words such as 'right now' or 'immediately'");
    }

    return (errors.Count == 0, errors.ToArray(), suggestions.ToArray());
}
```

### 7. Suggestion Application Logic

The rewrite logic applies suggestions to correct content.

```csharp
private static string ApplySuggestions(string text)
{
    if (string.IsNullOrWhiteSpace(text)) return string.Empty;

    // Deterministic, rule-based corrections used for documentation purposes.
    var corrected = text.Replace("free", "premium", StringComparison.OrdinalIgnoreCase);
    corrected = corrected.Replace("unlimited", "comprehensive", StringComparison.OrdinalIgnoreCase);
    corrected = corrected.Replace("best in class", "high-quality", StringComparison.OrdinalIgnoreCase);

    // Remove urgency terms
    corrected = corrected.Replace("right now", "available", StringComparison.OrdinalIgnoreCase);
    corrected = corrected.Replace("immediately", "promptly", StringComparison.OrdinalIgnoreCase);

    // Truncate long content to keep examples concise
    if (corrected.Length > 500)
    {
        corrected = corrected.Substring(0, 497) + "...";
    }

    return corrected;
}
```

## Expected Output

The example produces comprehensive output showing:

* 📝 Original draft content with constraint violations
* ❌ Validation errors and business rule violations
* 💡 Actionable suggestions for content improvement
* ✅ Corrected version with constraints applied
* 🎯 Final validation status (PASSED/FAILED)
* 📋 Complete validation workflow results

## Troubleshooting

### Common Issues

1. **Constraint Validation Failures**: Ensure constraint logic handles edge cases and null values
2. **Suggestion Application Errors**: Verify suggestion logic doesn't introduce new violations
3. **State Management Issues**: Check that validation results are properly stored and retrieved
4. **Content Length Issues**: Monitor content length constraints and truncation logic

### Debugging Tips

* Enable detailed logging to trace validation steps
* Monitor state transitions between validation nodes
* Verify constraint logic handles all content types
* Check suggestion application for completeness

## See Also

* [Content Validation](../how-to/content-validation.md)
* [State Management](../concepts/state.md)
* [Node Types](../concepts/node-types.md)
* [Graph Concepts](../concepts/graph-concepts.md)
* [Error Handling](../how-to/error-handling-and-resilience.md)
