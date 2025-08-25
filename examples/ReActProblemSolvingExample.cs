using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph;
using SemanticKernel.Graph.Extensions;

namespace Examples;

/// <summary>
/// Minimal ReAct problem solving example extracted from documentation.
/// This example uses deterministic, mock functions so it can run without external LLMs.
/// </summary>
public static class ReActProblemSolvingExample
{
    /// <summary>
    /// Entry point invoked by the examples runner to demonstrate a simple ReAct workflow.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("--- ReAct Problem Solving Example ---\n");

        // Create a minimal kernel with graph support. No external LLM is required for this demo.
        var kernel = Kernel.CreateBuilder()
            .AddGraphSupport()
            .Build();

        // Create the executor implementing a small ReAct cycle using mock functions.
        var executor = CreateBasicReActSolver(kernel);

        // Example problem arguments
        var arguments = new KernelArguments
        {
            ["problem_title"] = "Budget Planning",
            ["task_description"] = "Reduce operational costs by 20% while maintaining service quality.",
            ["max_iterations"] = 3,
            ["solver_mode"] = "systematic",
            ["domain"] = "general"
        };

        // Execute the graph and print the resulting solution synthesis
        var result = await executor.ExecuteAsync(kernel, arguments);
        var solution = result?.GetValue<string>() ?? "No solution generated";

        Console.WriteLine("💡 ReAct Solution:");
        Console.WriteLine($"   {solution}\n");
        Console.WriteLine("✅ ReAct problem solving example completed successfully!\n");
    }

    /// <summary>
    /// Builds a simple GraphExecutor that models the ReAct cycle using mock functions.
    /// </summary>
    private static GraphExecutor CreateBasicReActSolver(Kernel kernel)
    {
        var executor = new GraphExecutor("BasicReActSolver", "Basic ReAct problem solving agent");

        // Reasoning node - deterministic mock function
        var reasoningNode = new FunctionGraphNode(
            CreateMockReasoningFunction(kernel),
            "reasoning_node",
            "Problem Solving Reasoning"
        );

        // Action node - will discover functions from the kernel (we keep it permissive for the example)
        var actionNode = ActionGraphNode.CreateWithActions(
            kernel,
            new ActionSelectionCriteria
            {
                FunctionNamePattern = null,
                MinRequiredParameters = 0,
                MaxRequiredParameters = 5
            },
            "action_node");
        actionNode.ConfigureExecution(ActionSelectionStrategy.Intelligent, enableParameterValidation: true);

        // Observation node - deterministic mock
        var observationNode = new FunctionGraphNode(
            CreateMockObservationFunction(kernel),
            "observation_node",
            "Problem Solving Observation"
        );

        // Solution synthesis node - prompt-less deterministic synthesis for the demo
        var solutionNode = new FunctionGraphNode(
            CreateSolutionSynthesisFunction(kernel),
            "solution_synthesis",
            "Solution Synthesis"
        );

        // Add nodes to executor
        executor.AddNode(reasoningNode);
        executor.AddNode(actionNode);
        executor.AddNode(observationNode);
        executor.AddNode(solutionNode);

        // Define flow: reasoning -> action -> observation -> solution
        executor.SetStartNode(reasoningNode.NodeId);
        executor.AddEdge(ConditionalEdge.CreateUnconditional(reasoningNode, actionNode));
        executor.AddEdge(ConditionalEdge.CreateUnconditional(actionNode, observationNode));
        executor.AddEdge(ConditionalEdge.CreateUnconditional(observationNode, solutionNode));

        // Ensure downstream nodes have required inputs mapped from upstream
        observationNode.SetMetadata("AfterExecute",
            new Func<Kernel, KernelArguments, FunctionResult, CancellationToken, Task>((k, args, result, ct) =>
            {
                if (!args.ContainsName("problem_description") && args.TryGetValue("task_description", out var desc))
                {
                    args["problem_description"] = desc;
                }
                return Task.CompletedTask;
            }));

        return executor;
    }

    /// <summary>
    /// Creates a deterministic mock reasoning function that analyzes the problem and suggests a next step.
    /// </summary>
    private static KernelFunction CreateMockReasoningFunction(Kernel kernel)
    {
        return kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var taskDescription = args["task_description"]?.ToString() ?? "unknown task";
                var problemTitle = args["problem_title"]?.ToString() ?? "unknown problem";

                return $"Analyzed problem '{problemTitle}': {taskDescription}. Next step: identify stakeholders and constraints.";
            },
            functionName: "mock_reasoning",
            description: "Mock reasoning function for problem solving"
        );
    }

    /// <summary>
    /// Creates a deterministic mock observation function used to produce intermediate observations.
    /// </summary>
    private static KernelFunction CreateMockObservationFunction(Kernel kernel)
    {
        return kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                // Create a simple observation based on available inputs
                var problem = args.GetValueOrDefault("problem_description")?.ToString() ?? args.GetValueOrDefault("task_description")?.ToString() ?? "unknown";
                return $"Observed context for problem: {problem.Substring(0, Math.Min(200, problem.Length))}";
            },
            functionName: "mock_observation",
            description: "Mock observation function for problem solving"
        );
    }

    /// <summary>
    /// Creates a simple solution synthesis function that assembles a human-friendly solution string.
    /// </summary>
    private static KernelFunction CreateSolutionSynthesisFunction(Kernel kernel)
    {
        return kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var title = args.GetValueOrDefault("problem_title")?.ToString() ?? "Untitled";
                var description = args.GetValueOrDefault("problem_description")?.ToString() ?? args.GetValueOrDefault("task_description")?.ToString() ?? "No description";

                // Synthesize a concise solution summary for the demo
                return $"Solution for '{title}': 1) Assess stakeholders and constraints. 2) Prioritize cost-saving actions. 3) Implement phased monitoring and rollback. Context: {description.Substring(0, Math.Min(200, description.Length))}...";
            },
            functionName: "solution_synthesis",
            description: "Synthesizes comprehensive solutions from ReAct analysis"
        );
    }
}


