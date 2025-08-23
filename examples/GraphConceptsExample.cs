using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;

namespace Examples;

/// <summary>
/// Runnable examples that mirror the `graph-concepts.md` documentation snippets.
/// Each snippet is implemented and executed to ensure the documented code compiles
/// and runs correctly in the examples project.
/// </summary>
public static class GraphConceptsExample
{
    /// <summary>
    /// Runs all graph concept snippets.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("📘 Running Graph Concepts Examples...");
        Console.WriteLine(new string('=', 60));

        await DemonstrateGetNextNodesSnippet();
        DemonstrateConditionalEdgeSnippet();
        DemonstrateLoopShouldExecuteSnippet();
        DemonstrateStateManagementSnippet();

        Console.WriteLine("\n✅ Graph Concepts Examples completed successfully.");
    }

    /// <summary>
    /// Demonstrates the `GetNextNodes` snippet from the docs using a small local node
    /// implementation that decides the next node based on a `should_continue` flag.
    /// </summary>
    private static async Task DemonstrateGetNextNodesSnippet()
    {
        Console.WriteLine("\n-- GetNextNodes snippet --");

        // Create a minimal kernel (used only to satisfy signatures in the example)
        var kernel = Kernel.CreateBuilder().Build();

        // Create a simple next node (reuse SimpleNodeExample available in project)
        var nextNode = new SimpleNodeExample();

        // Local mock node that implements IGraphNode and returns nextNode when should_continue is true
        var controller = new MockControllerNode(nextNode);

        // Prepare graph state with should_continue = true
        var args = new KernelArguments();
        args["should_continue"] = true;
        var graphState = new GraphState(args);

        // Call GetNextNodes with a null executionResult to simulate initial navigation
        var next = controller.GetNextNodes(null, graphState);
        Console.WriteLine($"Next nodes count when should_continue=true: {next.Count()} (expected 1)");

        // Now set should_continue = false and check termination
        args["should_continue"] = false;
        var graphState2 = new GraphState(args);
        var next2 = controller.GetNextNodes(null, graphState2);
        Console.WriteLine($"Next nodes count when should_continue=false: {next2.Count()} (expected 0)");

        await Task.CompletedTask;
    }

    /// <summary>
    /// Demonstrates conditional edge creation and evaluation using KernelArguments and GraphState.
    /// </summary>
    private static void DemonstrateConditionalEdgeSnippet()
    {
        Console.WriteLine("\n-- ConditionalEdge snippet --");

        var start = new FunctionGraphNode(KernelFunctionFactory.CreateFromMethod((KernelArguments a) => "start", "startFn", "start node"), "start");
        var target = new FunctionGraphNode(KernelFunctionFactory.CreateFromMethod((KernelArguments a) => "target", "targetFn", "target node"), "target");

        // KernelArguments-based condition: parameter 'status' must equal 'ok'
        var edge = new ConditionalEdge(start, target, args => args.ContainsName("status") && string.Equals(args.TryGetValue("status", out var v) ? v?.ToString() : null, "ok", StringComparison.Ordinal), "status==ok");

        var testArgs = new KernelArguments();
        testArgs.Add("status", "ok");
        Console.WriteLine($"Edge condition (status=ok): {edge.EvaluateCondition(testArgs)} (expected True)");

        // GraphState-based condition: use state helper
        var stateArgs = new KernelArguments();
        stateArgs.Add("score", 85);
        var state = new GraphState(stateArgs);

        var stateEdge = new ConditionalEdge(start, target, (GraphState gs) =>
        {
            // Safely try to read an integer 'score' from the graph state, defaulting to 0 when missing
            var score = gs.TryGetValue<int>("score", out var s) ? s : 0;
            return score >= 80;
        }, "score>=80");
        Console.WriteLine($"State-edge condition (score=85): {stateEdge.EvaluateCondition(state)} (expected True)");
    }

    /// <summary>
    /// Demonstrates the ShouldExecute snippet for loop control based on iteration count.
    /// </summary>
    private static void DemonstrateLoopShouldExecuteSnippet()
    {
        Console.WriteLine("\n-- ShouldExecute (loop control) snippet --");

        var loopNode = new LoopExampleNode();

        var args = new KernelArguments();
        args.Add("iteration_count", 3);
        args.Add("max_iterations", 5);
        var graphState = new GraphState(args);

        Console.WriteLine($"Should execute (3 < 5): {loopNode.ShouldExecute(graphState)} (expected True)");

        args["iteration_count"] = 6;
        var graphState2 = new GraphState(args);
        Console.WriteLine($"Should execute (6 < 5): {loopNode.ShouldExecute(graphState2)} (expected False)");
    }

    /// <summary>
    /// Demonstrates basic state SetValue/GetValue usage.
    /// </summary>
    private static void DemonstrateStateManagementSnippet()
    {
        Console.WriteLine("\n-- State Management snippet --");

        var state = new GraphState(new KernelArguments());
        state.SetValue("user_input", "Hello, world!");
        state.SetValue("processing_step", 1);

        var input = state.GetValue<string>("user_input");
        var step = state.GetValue<int>("processing_step");

        Console.WriteLine($"user_input: {input}");
        Console.WriteLine($"processing_step: {step}");
    }

    #region Local helper types used only by the example

    /// <summary>
    /// Minimal controller node that navigates to a configured next node when the kernel
    /// arguments contain `should_continue=true`.
    /// </summary>
    private sealed class MockControllerNode : IGraphNode
    {
        private readonly IGraphNode _next;

        public MockControllerNode(IGraphNode next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            NodeId = Guid.NewGuid().ToString();
            Name = "MockController";
            Description = "Decides whether to continue based on 'should_continue' flag";
            Metadata = new Dictionary<string, object>();
        }

        public string NodeId { get; }
        public string Name { get; }
        public string Description { get; }
        public IReadOnlyDictionary<string, object> Metadata { get; }
        public bool IsExecutable => true;
        public IReadOnlyList<string> InputParameters => new[] { "should_continue" };
        public IReadOnlyList<string> OutputParameters => Array.Empty<string>();

        public Task<FunctionResult> ExecuteAsync(Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken = default)
        {
            // This controller does not perform work; return a simple result
            var fn = KernelFunctionFactory.CreateFromMethod(() => "controller-executed");
            return Task.FromResult(new FunctionResult(fn, "controller-executed"));
        }

        public ValidationResult ValidateExecution(KernelArguments arguments)
        {
            return new ValidationResult();
        }

        public IEnumerable<IGraphNode> GetNextNodes(FunctionResult? executionResult, GraphState graphState)
        {
            if (graphState == null) throw new ArgumentNullException(nameof(graphState));

            if (graphState.KernelArguments.TryGetValue("should_continue", out var val) && val is bool b && b)
            {
                return new[] { _next };
            }
            return Enumerable.Empty<IGraphNode>();
        }

        public bool ShouldExecute(GraphState graphState)
        {
            return true;
        }

        public Task OnBeforeExecuteAsync(Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task OnAfterExecuteAsync(Kernel kernel, KernelArguments arguments, FunctionResult result, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task OnExecutionFailedAsync(Kernel kernel, KernelArguments arguments, Exception exception, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    /// <summary>
    /// Minimal loop node example used to demonstrate ShouldExecute loop-control snippet.
    /// </summary>
    private sealed class LoopExampleNode : IGraphNode
    {
        public LoopExampleNode()
        {
            NodeId = Guid.NewGuid().ToString();
            Name = "LoopExample";
            Description = "Demonstrates ShouldExecute by comparing iteration_count and max_iterations";
            Metadata = new Dictionary<string, object>();
        }

        public string NodeId { get; }
        public string Name { get; }
        public string Description { get; }
        public IReadOnlyDictionary<string, object> Metadata { get; }
        public bool IsExecutable => true;
        public IReadOnlyList<string> InputParameters => new[] { "iteration_count", "max_iterations" };
        public IReadOnlyList<string> OutputParameters => Array.Empty<string>();

        public Task<FunctionResult> ExecuteAsync(Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken = default)
        {
            var fn = KernelFunctionFactory.CreateFromMethod(() => "loop-executed");
            return Task.FromResult(new FunctionResult(fn, "loop-executed"));
        }

        public ValidationResult ValidateExecution(KernelArguments arguments) => new ValidationResult();

        public IEnumerable<IGraphNode> GetNextNodes(FunctionResult? executionResult, GraphState graphState) => Enumerable.Empty<IGraphNode>();

        public bool ShouldExecute(GraphState graphState)
        {
            // Safely read iteration counters with defaults
            var iterationCount = graphState.TryGetValue<int>("iteration_count", out var ic) ? ic : 0;
            var maxIterations = graphState.TryGetValue<int>("max_iterations", out var mi) ? mi : 10;
            return iterationCount < maxIterations;
        }

        public Task OnBeforeExecuteAsync(Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task OnAfterExecuteAsync(Kernel kernel, KernelArguments arguments, FunctionResult result, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task OnExecutionFailedAsync(Kernel kernel, KernelArguments arguments, Exception exception, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    #endregion
}


