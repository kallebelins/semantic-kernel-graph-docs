using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.State;

namespace Examples;

/// <summary>
/// A minimal example implementation of IGraphNode demonstrating execution, validation,
/// and lifecycle hooks. This example mirrors the documentation's `SimpleNode` sample
/// and is intended to be compiled and executed by the examples runner.
/// </summary>
public class SimpleNodeExample : IGraphNode
{
    public SimpleNodeExample()
    {
        NodeId = Guid.NewGuid().ToString();
        Name = "SimpleNodeExample";
        Description = "Processes an 'input' parameter and writes 'output' to the state.";
        Metadata = new Dictionary<string, object>();
    }

    public string NodeId { get; }
    public string Name { get; }
    public string Description { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
    public bool IsExecutable => true;

    public IReadOnlyList<string> InputParameters => new[] { "input" };
    public IReadOnlyList<string> OutputParameters => new[] { "output" };

    /// <summary>
    /// ExecuteAsync processes the 'input' argument and stores a processed value in 'output'.
    /// It returns a FunctionResult wrapping the produced output string.
    /// </summary>
    public async Task<FunctionResult> ExecuteAsync(Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken = default)
    {
        // Guard clauses for required arguments
        if (kernel == null) throw new ArgumentNullException(nameof(kernel));
        if (arguments == null) throw new ArgumentNullException(nameof(arguments));

        // Retrieve input and build output
        var input = string.Empty;
        if (arguments.ContainsKey("input") && arguments["input"] != null)
        {
            input = arguments["input"].ToString() ?? string.Empty;
        }
        var output = $"Processed: {input}";

        // Store the output back into arguments state
        arguments["output"] = output;

        // Build a FunctionResult using a lightweight kernel function factory to create
        // an in-memory KernelFunction that represents this produced value. This avoids
        // passing null to the FunctionResult constructor which would throw.
        var tempFunction = KernelFunctionFactory.CreateFromMethod(() => output);
        var functionResult = new FunctionResult(tempFunction, output);

        await Task.CompletedTask; // Maintain async signature
        return functionResult;
    }

    /// <summary>
    /// ValidateExecution performs a lightweight check to ensure the required 'input' parameter exists.
    /// </summary>
    public ValidationResult ValidateExecution(KernelArguments arguments)
    {
        if (arguments == null) throw new ArgumentNullException(nameof(arguments));

        var result = new ValidationResult();
        if (!arguments.ContainsKey("input") || string.IsNullOrEmpty(arguments["input"]?.ToString()))
        {
            result.AddError("Required parameter 'input' is missing or empty");
        }

        return result;
    }

    /// <summary>
    /// GetNextNodes returns an empty enumeration for this simple example.
    /// </summary>
    public IEnumerable<IGraphNode> GetNextNodes(FunctionResult? executionResult, GraphState graphState)
    {
        return Enumerable.Empty<IGraphNode>();
    }

    /// <summary>
    /// ShouldExecute checks if the state provides a non-empty 'input' value.
    /// </summary>
    public bool ShouldExecute(GraphState graphState)
    {
        if (graphState == null) throw new ArgumentNullException(nameof(graphState));
        return graphState.KernelArguments.ContainsKey("input") && !string.IsNullOrEmpty(graphState.KernelArguments["input"]?.ToString());
    }

    public Task OnBeforeExecuteAsync(Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken = default)
    {
        // No-op for this example
        return Task.CompletedTask;
    }

    public Task OnAfterExecuteAsync(Kernel kernel, KernelArguments arguments, FunctionResult result, CancellationToken cancellationToken = default)
    {
        // No-op for this example
        return Task.CompletedTask;
    }

    public Task OnExecutionFailedAsync(Kernel kernel, KernelArguments arguments, Exception exception, CancellationToken cancellationToken = default)
    {
        // Log or handle error in real implementations; kept minimal here
        return Task.CompletedTask;
    }
}
