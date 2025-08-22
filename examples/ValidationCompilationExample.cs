using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.State;

namespace Examples
{
    /// <summary>
    /// Example that demonstrates validation and state merge/serialization helpers.
    /// </summary>
    public static class ValidationCompilationExample
    {
        /// <summary>
        /// Runs the validation and merge examples from documentation to ensure they compile and behave.
        /// </summary>
        public static Task RunAsync()
        {
            // Create a workflow to validate
            var workflow = new MultiAgentWorkflow
            {
                Id = "document-analysis-001",
                Name = "Document Analysis Pipeline",
                RequiredAgents = new List<string> { "analysis-agent", "processing-agent" },
                Tasks = new List<WorkflowTask>
                {
                    new WorkflowTask
                    {
                        Id = "extract-text",
                        Name = "Text Extraction",
                        RequiredCapabilities = new HashSet<string> { "text_extraction", "ocr" },
                        DependsOn = new List<string>()
                    },
                    new WorkflowTask
                    {
                        Id = "process-content",
                        Name = "Content Processing",
                        RequiredCapabilities = new HashSet<string> { "text_processing", "nlp" },
                        DependsOn = new List<string> { "extract-text" }
                    }
                }
            };

            // Create validator and run
            var validator = new WorkflowValidator(null);
            var workflowResult = validator.Validate(workflow);

            Console.WriteLine($"Workflow valid: {workflowResult.IsValid}, Errors: {workflowResult.ErrorCount}, Warnings: {workflowResult.WarningCount}");

            // Prepare states for merge demonstration
            var baseState = new GraphState(new KernelArguments()) ;
            baseState.SetValue("count", 5);
            baseState.SetValue("settings", new Dictionary<string, object>
            {
                ["theme"] = "light",
                ["language"] = "en"
            });

            var overlayState = new GraphState(new KernelArguments());
            overlayState.SetValue("count", 10);
            overlayState.SetValue("settings", new Dictionary<string, object>
            {
                ["theme"] = "dark",
                ["language"] = "pt"
            });

            // Simple merge using PreferSecond
            var merged = StateHelpers.MergeStates(baseState, overlayState, StateMergeConflictPolicy.PreferSecond);

            Console.WriteLine($"Merged count: {merged.GetValue<int>("count")}");
            var mergedSettings = merged.GetValue<Dictionary<string, object>>("settings");
            Console.WriteLine($"Merged theme: {mergedSettings?["theme"]}");

            // Advanced merge configuration with custom reducer
            var config = new StateMergeConfiguration { DefaultPolicy = StateMergeConflictPolicy.PreferSecond };
            config.SetKeyPolicy("count", StateMergeConflictPolicy.Reduce);
            config.SetReduceFunction(typeof(int), (a, b) =>
            {
                if (a is int ai && b is int bi) return Math.Max(ai, bi);
                return b;
            });

            var advancedMerged = StateHelpers.MergeStates(baseState, overlayState, config);
            Console.WriteLine($"Advanced merged count: {advancedMerged.GetValue<int>("count")}");

            // State validation demo
            var stateToValidate = new GraphState(new KernelArguments());
            stateToValidate.SetValue("user_id", 123);
            stateToValidate.SetValue("user_name", "John Doe");

            var stateValidation = StateValidator.ValidateState(stateToValidate);
            Console.WriteLine($"State valid: {stateValidation.IsValid}, Errors: {stateValidation.ErrorCount}, Warnings: {stateValidation.WarningCount}");

            // Serialize and deserialize to ensure helpers work
            var json = StateHelpers.SerializeState(advancedMerged, indented: true, enableCompression: false, useCache: false, out var metrics);
            Console.WriteLine($"Serialized size: {metrics.ResultSizeBytes} bytes, Compressed: {metrics.WasCompressed}");

            var deserialized = StateHelpers.DeserializeState(json);
            Console.WriteLine($"Deserialized contains 'count': {deserialized.ContainsValue("count")}");

            return Task.CompletedTask;
        }
    }
}
