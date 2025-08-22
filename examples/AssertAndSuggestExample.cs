using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Nodes;

namespace Examples;

/// <summary>
/// Example runner for the Assert & Suggest documentation. Adapted and
/// verified to compile and run as part of the examples project.
/// </summary>
public static class AssertAndSuggestExample
{
    /// <summary>
    /// Runs the Assert & Suggest demonstration used in the docs.
    /// </summary>
    public static async Task RunAsync()
    {
        // Create a minimal kernel and a graph executor
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));
        var kernel = kernelBuilder.Build();

        var graph = new GraphExecutor("AssertAndSuggest", "Validate output and suggest fixes");

        // Draft node (simulated LLM output)
        var draftNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(() =>
            {
                var draft = "Title: Super Gadget Pro Max\n" +
                            "Summary: This is a free, absolutely unbeatable gadget with unlimited features, " +
                            "best in class performance, and a comprehensive set of accessories included for everyone right now.";
                return draft;
            }, functionName: "generate_draft", description: "Generates an initial draft"),
            nodeId: "draft", description: "Draft generation").StoreResultAs("draft_output");

        // Validation node
        var validateNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) =>
            {
                var text = args.TryGetValue("draft_output", out var o) ? o?.ToString() ?? string.Empty : string.Empty;
                var (valid, errors, suggestions) = ValidateConstraints(text);

                args["assert_valid"] = valid;
                args["assert_errors"] = string.Join(" | ", errors);
                args["suggestions"] = string.Join(" | ", suggestions);

                return valid ? "valid" : "invalid";
            }, functionName: "validate_output", description: "Validates output"),
            nodeId: "validate", description: "Validation").StoreResultAs("validation_result");

        // Rewrite node
        var rewriteNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) =>
            {
                var text = args.TryGetValue("draft_output", out var o) ? o?.ToString() ?? string.Empty : string.Empty;
                var fixedText = ApplySuggestions(text);
                args["rewritten_output"] = fixedText;

                var (valid, errors, _) = ValidateConstraints(fixedText);
                args["assert_valid"] = valid;
                args["assert_errors"] = string.Join(" | ", errors);
                return fixedText;
            }, functionName: "rewrite_with_suggestions", description: "Rewrite using suggestions"),
            nodeId: "rewrite", description: "Rewrite");

        // Present node
        var presentNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod((KernelArguments args) =>
            {
                var original = args.TryGetValue("draft_output", out var o) ? o?.ToString() ?? string.Empty : string.Empty;
                var rewritten = args.TryGetValue("rewritten_output", out var r) ? r?.ToString() ?? string.Empty : string.Empty;
                var errors = args.TryGetValue("assert_errors", out var e) ? e?.ToString() ?? string.Empty : string.Empty;
                var suggestions = args.TryGetValue("suggestions", out var s) ? s?.ToString() ?? string.Empty : string.Empty;
                var isValid = args.TryGetValue("assert_valid", out var v) && v is bool b && b;

                Console.WriteLine("\n📋 Content Validation Results:");
                Console.WriteLine("Original Draft:\n" + original);
                if (!string.IsNullOrEmpty(errors))
                {
                    Console.WriteLine("\n❌ Validation Errors:\n" + errors);
                }
                if (!string.IsNullOrEmpty(suggestions))
                {
                    Console.WriteLine("\n💡 Suggestions:\n" + suggestions);
                }
                Console.WriteLine("\n✅ Corrected Version:\n" + rewritten);
                Console.WriteLine($"\n🎯 Final Validation: {(isValid ? "PASSED" : "FAILED")}");

                return "completed";
            }, functionName: "present_results", description: "Presents results"),
            nodeId: "present", description: "Present");

        // Assemble graph
        graph.AddNode(draftNode)
             .AddNode(validateNode)
             .AddNode(rewriteNode)
             .AddNode(presentNode)
             .SetStartNode("draft");

        graph.ConnectWhen("draft", "validate", _ => true);
        graph.ConnectWhen("validate", "present", args => args.TryGetValue("assert_valid", out var v) && v is bool vb && vb);
        graph.ConnectWhen("validate", "rewrite", args => !(args.TryGetValue("assert_valid", out var v) && v is bool vb2 && vb2));
        graph.ConnectWhen("rewrite", "present", _ => true);

        // Execute
        var argsBag = new KernelArguments();
        var result = await graph.ExecuteAsync(kernel, argsBag);
        Console.WriteLine($"\n➡️  Execution result: {result?.GetValue<object>()}");
    }

    // Validate constraints and return lists of errors and suggestions
    private static (bool valid, List<string> errors, List<string> suggestions) ValidateConstraints(string text)
    {
        var errors = new List<string>();
        var suggestions = new List<string>();

        if (string.IsNullOrWhiteSpace(text))
        {
            errors.Add("Empty content");
            suggestions.Add("Provide a draft with Title: and Summary:");
            return (false, errors, suggestions);
        }

        if (text.Contains("free", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Contains promotional language: 'free'");
            suggestions.Add("Remove promotional words and use factual language");
        }

        if (text.Length > 500)
        {
            errors.Add("Content too long");
            suggestions.Add("Keep content concise");
        }

        return (errors.Count == 0, errors, suggestions);
    }

    // Apply simplistic suggestions to the draft text
    private static string ApplySuggestions(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var corrected = text.Replace("free", "premium", StringComparison.OrdinalIgnoreCase);
        if (corrected.Length > 500) corrected = corrected.Substring(0, 497) + "...";
        return corrected;
    }
}


