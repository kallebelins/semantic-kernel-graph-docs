using Microsoft.SemanticKernel;
using SemanticKernel.Graph.State;

namespace Examples;

/// <summary>
/// Demonstrates state creation, serialization, deserialization and basic merge/validation flows.
/// This example intentionally avoids external services and is suitable for automated testing.
/// </summary>
public static class StateAndSerializationExample
{
    /// <summary>
    /// Runs the example asynchronously.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("🔧 Running State and Serialization Example...");

        // Create kernel arguments to populate the GraphState
        var arguments = new KernelArguments
        {
            ["input"] = "Hello World",
            ["timestamp"] = DateTimeOffset.UtcNow,
            ["user"] = "Alice"
        };

        // Create a GraphState from the kernel arguments
        var graphState = new GraphState(arguments);

        // Add metadata to the state
        graphState.SetMetadata("source", "doc-example");
        graphState.SetMetadata("priority", "normal");

        // Access values from the state
        var input = graphState.GetValue<string>("input");
        Console.WriteLine($"State input: {input}");

        // Demonstrate serialization with compact and verbose options
        var compact = SerializationOptions.Compact;
        var verbose = SerializationOptions.Verbose;

        var compactSerialized = graphState.Serialize(compact);
        var verboseSerialized = graphState.Serialize(verbose);

        // Persist compact serialization to a temporary file and read it back
        var tempFile = Path.Combine(Path.GetTempPath(), $"state-{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(tempFile, compactSerialized);
        var loaded = await File.ReadAllTextAsync(tempFile);

        // Deserialize back into a GraphState
        var restoredState = StateHelpers.DeserializeState(loaded);

        // Validate integrity and print results
        var validation = restoredState.ValidateIntegrity();
        Console.WriteLine($"Validation is valid: {validation.IsValid}");
        if (!validation.IsValid)
        {
            foreach (var err in validation.Errors)
            {
                Console.WriteLine($"  Error: {err}");
            }
        }

        // Create checksum before and after a modification
        var originalChecksum = restoredState.CreateChecksum();
        restoredState.SetValue("modified", true);
        var newChecksum = restoredState.CreateChecksum();
        Console.WriteLine($"Checksum changed: {originalChecksum != newChecksum}");

        // Demonstrate cloning and merging of states
        var cloned = StateHelpers.CloneState(restoredState);

        var overlay = new GraphState(new KernelArguments
        {
            ["count"] = 10,
            ["settings"] = new Dictionary<string, object> { ["language"] = "en" }
        });

        var merged = StateHelpers.MergeStates(cloned, overlay, StateMergeConflictPolicy.PreferSecond);
        Console.WriteLine($"Merged contains 'count': {merged.ContainsValue("count")}");

        // Validate required parameters example (non-throwing demonstration)
        var required = new[] { "user", "input" };
        var missing = StateHelpers.ValidateRequiredParameters(merged, required);
        Console.WriteLine($"Missing required parameters count: {missing.Count}");

        // Clean up temporary file
        try
        {
            File.Delete(tempFile);
        }
        catch
        {
            // Ignore cleanup errors in example
        }

        Console.WriteLine("✅ State and Serialization Example completed.");
    }
}


