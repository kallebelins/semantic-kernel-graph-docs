using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.State;

namespace Examples
{
    /// <summary>
    /// Demonstrates basic usage of StateHelpers to create and restore checkpoints.
    /// This example is intentionally minimal and uses in-memory state only.
    /// </summary>
    public static class CheckpointingConceptsExample
    {
        /// <summary>
        /// Runs the checkpointing concept demonstration.
        /// </summary>
        public static Task RunAsync()
        {
            // Create initial kernel arguments and graph state
            var args = new KernelArguments();
            args["input"] = "initial-value";

            var state = new GraphState(args);

            Console.WriteLine("Original state parameter 'input': " + state.GetValue<string>("input"));

            // Create a checkpoint
            var checkpointId = StateHelpers.CreateCheckpoint(state, "concept-checkpoint");
            Console.WriteLine("Created checkpoint: " + checkpointId);

            // Mutate state
            state.SetValue("input", "mutated-value");
            Console.WriteLine("Mutated state parameter 'input': " + state.GetValue<string>("input"));

            // Restore from checkpoint
            var restored = StateHelpers.RestoreCheckpoint(state, checkpointId);
            Console.WriteLine("Restored state parameter 'input': " + restored.GetValue<string>("input"));

            return Task.CompletedTask;
        }
    }
}


