using Microsoft.SemanticKernel;

namespace Examples;

/// <summary>
/// Documentation example that mirrors the runnable example in the Examples project.
/// This file is intended to be included in the documentation and shows a minimal
/// Kernel-based echo function. All comments are in English and the code is
/// suitable for copy/paste into the Examples project for testing.
/// </summary>
public static class ExecutionGuideExample
{
    /// <summary>
    /// Runs a minimal echo function using Kernel to validate the documentation snippet.
    /// </summary>
    public static async Task RunAsync()
    {
        // Create a kernel without external AI providers for fast local execution
        var kernel = Kernel.CreateBuilder().Build();

        // Register a simple function named 'echo' and keep a reference to the KernelFunction
        var echoFunction = kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var input = args["input"]?.ToString() ?? string.Empty;
                return $"echo:{input}";
            },
            functionName: "echo",
            description: "Echoes the input string for demonstration purposes");

        // Prepare KernelArguments and invoke the KernelFunction directly
        var arguments = new KernelArguments
        {
            ["input"] = "Hello from docs example"
        };

        var invocationResult = await echoFunction.InvokeAsync(kernel, arguments);
        var typed = invocationResult.GetValue<string>() ?? string.Empty;
        Console.WriteLine("ExecutionGuideExample result: " + typed);
    }
}


