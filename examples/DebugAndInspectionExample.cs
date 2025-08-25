using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Debug;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;
using SemanticKernel.Graph.Execution;

namespace Examples;

/// <summary>
/// Minimal example demonstrating debug and inspection APIs: creates a tiny graph,
/// registers an execution with the inspection API, starts a debug session,
/// generates visualization outputs (DOT, Mermaid, JSON) and prints them.
/// All comments are in English and the example is self-contained for smoke testing.
/// </summary>
public static class DebugAndInspectionExample
{
    /// <summary>
    /// Runs the debug and inspection example.
    /// </summary>
    public static async System.Threading.Tasks.Task RunAsync()
    {
        // Create a minimal kernel used by GraphExecutionContext and nodes
        var kernel = Kernel.CreateBuilder().Build();

        // Create two simple function nodes using the lightweight factory helpers
        var fnA = KernelFunctionFactory.CreateFromMethod(() => "output-a", "FnA");
        var fnB = KernelFunctionFactory.CreateFromMethod(() => "output-b", "FnB");

        var nodeA = new FunctionGraphNode(fnA, "node-a", "Node A");
        var nodeB = new FunctionGraphNode(fnB, "node-b", "Node B");

        // Wire nodes: nodeA -> nodeB
        var edges = new List<GraphEdgeInfo> { new GraphEdgeInfo("node-a", "node-b", "to-b") };
        var nodes = new List<IGraphNode> { nodeA, nodeB };

        // Create a GraphExecutionContext with an initial empty state
        var graphState = new GraphState(new KernelArguments { ["input"] = "demo" });
        var execContext = new GraphExecutionContext(kernel, graphState);

        // Create inspection API and register the execution
        using var inspection = new GraphInspectionApi(new GraphInspectionOptions { IncludeDebugInfo = true });
        inspection.RegisterExecution(execContext);

        // Create a lightweight graph visualization data object
        var visualizationData = new GraphVisualizationData(nodes, edges, currentNode: nodeA, executionPath: nodes);

        // Use the engine to produce outputs
        using var engine = new GraphVisualizationEngine();

        var dot = engine.SerializeToDot(visualizationData, new DotSerializationOptions { GraphName = "DebugInspectDemo" });
        var mermaid = engine.GenerateEnhancedMermaidDiagram(visualizationData, new MermaidGenerationOptions { Direction = "TB" });
        var json = engine.SerializeToJson(visualizationData, new JsonSerializationOptions { Indented = true });

        Console.WriteLine("--- Debug & Inspection: DOT ---");
        Console.WriteLine(dot);
        Console.WriteLine("--- Debug & Inspection: Mermaid ---");
        Console.WriteLine(mermaid);
        Console.WriteLine("--- Debug & Inspection: JSON ---");
        Console.WriteLine(json);

        // Create a debug session for the execution context and register it with inspection
        // GraphExecutor does not implement IDisposable, so do not use 'using' here.
        var graphExecutor = new GraphExecutor(kernel);
        using var debugSession = new DebugSession(graphExecutor, execContext);
        inspection.RegisterDebugSession(debugSession);

        // Add a breakpoint on node-b using a simple expression
        debugSession.AddBreakpoint("node-b", "{{input}} == 'demo'", "Breakpoint on node-b when input == 'demo'");

        // Start the debug session in StepOver mode, then immediately stop (smoke test)
        await debugSession.StartAsync(DebugExecutionMode.StepOver);
        Console.WriteLine($"Debug session started: {debugSession.SessionId}");

        // Export session data and print a short summary. Export may fail when objects contain cycles,
        // so fall back to a safe, limited projection if serialization errors occur.
        string exported;
        try
        {
            exported = debugSession.ExportSessionData(includeHistory: true);
        }
        catch (Exception ex)
        {
            // Build a compact, safe summary to avoid deep/cyclic serialization
            var safe = new
            {
                debugSession.SessionId,
                ExecutionId = debugSession.ExecutionContext.ExecutionId,
                debugSession.IsActive,
                debugSession.IsPaused,
                Breakpoints = debugSession.GetBreakpoints().Select(bp => new
                {
                    bp.BreakpointId,
                    bp.NodeId,
                    bp.Description,
                    bp.HitCount,
                    bp.IsEnabled
                }).ToList()
            };

            exported = System.Text.Json.JsonSerializer.Serialize(safe, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine($"Warning: full export failed: {ex.Message}. Using safe fallback summary.");
        }

        Console.WriteLine("--- Exported Debug Session Data (truncated) ---");
        Console.WriteLine(exported.Length > 1000 ? exported[..1000] + "..." : exported);

        await debugSession.StopAsync();
        Console.WriteLine("Debug session stopped.");

        await System.Threading.Tasks.Task.CompletedTask;
    }
}
