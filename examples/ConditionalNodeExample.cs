using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.State;

namespace Examples;

/// <summary>
/// A conditional routing node example that demonstrates returning next nodes based on a predicate.
/// Mirrors the documentation's `ConditionalNode` sample and is intended for compilation testing.
/// </summary>
public class ConditionalNodeExample : IGraphNode
{
    private readonly List<IGraphNode> _nextNodes = new();
    private readonly Func<KernelArguments, bool> _condition;

    public ConditionalNodeExample(string nodeId, string name, Func<KernelArguments, bool> condition)
    {
        NodeId = nodeId ?? Guid.NewGuid().ToString();
        Name = name ?? "ConditionalNodeExample";
        Description = "Conditional routing node";
        Metadata = new Dictionary<string, object>();
        IsExecutable = false; // This node routes but does not execute a function
        _condition = condition ?? throw new ArgumentNullException(nameof(condition));
    }

    public string NodeId { get; }
    public string Name { get; }
    public string Description { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
    public bool IsExecutable { get; }

    public IReadOnlyList<string> InputParameters => Array.Empty<string>();
    public IReadOnlyList<string> OutputParameters => Array.Empty<string>();

    /// <summary>
    /// ExecuteAsync returns a FunctionResult indicating no execution took place.
    /// </summary>
    public Task<FunctionResult> ExecuteAsync(Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken = default)
    {
        var result = new FunctionResult(null, "No execution");
        return Task.FromResult(result);
    }

    /// <summary>
    /// ValidateExecution always returns a successful, empty ValidationResult for this routing node.
    /// </summary>
    public ValidationResult ValidateExecution(KernelArguments arguments)
    {
        return new ValidationResult();
    }

    /// <summary>
    /// GetNextNodes returns the configured next nodes when the condition evaluates to true.
    /// </summary>
    public IEnumerable<IGraphNode> GetNextNodes(FunctionResult? executionResult, GraphState graphState)
    {
        if (_condition(graphState.KernelArguments))
        {
            return _nextNodes;
        }

        return Enumerable.Empty<IGraphNode>();
    }

    /// <summary>
    /// ShouldExecute always returns false because this node only routes.
    /// </summary>
    public bool ShouldExecute(GraphState graphState)
    {
        return false;
    }

    /// <summary>
    /// Adds a next node to the routing list.
    /// </summary>
    public void AddNextNode(IGraphNode node)
    {
        if (node == null) throw new ArgumentNullException(nameof(node));
        _nextNodes.Add(node);
    }

    public Task OnBeforeExecuteAsync(Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task OnAfterExecuteAsync(Kernel kernel, KernelArguments arguments, FunctionResult result, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task OnExecutionFailedAsync(Kernel kernel, KernelArguments arguments, Exception exception, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
