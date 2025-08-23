using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Integration;

namespace Examples;

/// <summary>
/// Minimal multi-agent example that demonstrates registering two mock agents,
/// distributing two simple tasks and executing them with the coordinator.
/// </summary>
public static class MultiAgentExample
{
    /// <summary>
    /// Runs the multi-agent example.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("▶ Running Multi-Agent example...");

        // Create a simple coordinator with defaults
        using var coordinator = new MultiAgentCoordinator();

        // Create two very simple executors that echo input
        var echoExecutor1 = new SimpleEchoExecutor("agent-1");
        var echoExecutor2 = new SimpleEchoExecutor("agent-2");

        // Register agents
        var role = new AgentRole { Name = "EchoAgent", Priority = 1 };
        var agent1 = await coordinator.RegisterAgentAsync("agent-1", echoExecutor1, role);
        var agent2 = await coordinator.RegisterAgentAsync("agent-2", echoExecutor2, role);

        // Build a simple workflow with two tasks
        var workflow = new MultiAgentWorkflow
        {
            Id = "sample-multi-agent-workflow",
            Name = "Sample Multi-Agent Workflow",
            RequiredAgents = new List<string> { "agent-1", "agent-2" },
            Tasks = new List<WorkflowTask>
            {
                new WorkflowTask { Id = "t1", Name = "Task 1" },
                new WorkflowTask { Id = "t2", Name = "Task 2", DependsOn = new List<string>{ "t1" } }
            }
        };

        // Create kernel and initial arguments
        var kernel = Kernel.CreateBuilder().Build();
        var args = new KernelArguments();
        args["input"] = "hello from multi-agent example";

        // Execute workflow
        var result = await coordinator.ExecuteWorkflowAsync(workflow, kernel, args, CancellationToken.None);

        Console.WriteLine($"Workflow success: {result.Success}");
        if (result.AggregatedResult != null)
        {
            Console.WriteLine($"Aggregated: {result.AggregatedResult.GetValue<object>()}");
        }

        Console.WriteLine("✅ Multi-Agent example finished.");
    }
}

/// <summary>
/// Very small IGraphExecutor implementation used only for documentation examples.
/// It returns a FunctionResult containing an echo of the 'input' argument.
/// </summary>
internal sealed class SimpleEchoExecutor : IGraphExecutor
{
    public string Name { get; }

    public SimpleEchoExecutor(string name) => Name = name;

    public Task<FunctionResult> ExecuteAsync(Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken = default)
    {
        // Read input argument and return as FunctionResult
        var input = arguments.ContainsKey("input") ? arguments["input"]?.ToString() ?? string.Empty : string.Empty;
        var value = $"[{Name}] Echo: {input}";
        var function = KernelFunctionFactory.CreateFromMethod(() => value, "Echo");
        return Task.FromResult(new FunctionResult(function, value));
    }

    public Task<FunctionResult> ExecuteNodeAsync(IGraphNode node, Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken = default)
        => ExecuteAsync(kernel, arguments, cancellationToken);

    public Task<FunctionResult> ExecuteGraphAsync(IEnumerable<IGraphNode> nodes, Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken = default)
        => ExecuteAsync(kernel, arguments, cancellationToken);
}


