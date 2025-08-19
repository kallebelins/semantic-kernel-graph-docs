# ReAct Problem Solving Example

This example demonstrates advanced ReAct (Reasoning → Acting → Observing) agents for systematic problem solving using Semantic Kernel Graph workflows.

## Objective

Learn how to implement sophisticated ReAct agents that can:
- Solve complex, multi-step problems through systematic analysis
- Handle iterative problem refinement with feedback loops
- Manage stakeholder analysis, constraint evaluation, and risk assessment
- Generate comprehensive solutions with implementation roadmaps
- Support different problem-solving modes (basic, comprehensive, iterative)

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- Basic understanding of [ReAct Pattern](../concepts/react-pattern.md) and [Graph Execution](../concepts/graph-execution.md)
- Familiarity with [Action Nodes](../concepts/action-nodes.md) and [Conditional Routing](../concepts/conditional-routing.md)

## Key Components

### Concepts and Techniques

- **ReAct Pattern**: Systematic problem-solving through reasoning, action, and observation cycles
- **Multi-Stage Analysis**: Breaking complex problems into manageable analysis phases
- **Iterative Refinement**: Continuous improvement through feedback loops and convergence checking
- **Stakeholder Management**: Identifying and analyzing all parties affected by the problem
- **Risk Assessment**: Evaluating potential risks and mitigation strategies
- **Solution Synthesis**: Combining analysis results into actionable implementation plans

### Core Classes

- `GraphExecutor`: Orchestrates the ReAct problem-solving workflow
- `FunctionGraphNode`: Executes reasoning, analysis, and synthesis functions
- `ActionGraphNode`: Selects and executes appropriate actions based on context
- `ConditionalEdge`: Routes execution based on convergence criteria and iteration state
- `ReActTemplateEngine`: Provides templates for ReAct pattern execution

## Running the Example

### Getting Started

This example demonstrates ReAct problem-solving patterns with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Basic Problem Solving

The first example demonstrates fundamental ReAct problem-solving capabilities.

```csharp
private static async Task RunBasicProblemSolvingAsync(Kernel kernel)
{
    Console.WriteLine("--- Example 1: Basic ReAct Problem Solving ---");

    try
    {
        // Create template engine
        var templateEngine = new ReActTemplateEngine();

        // Create basic ReAct problem solver
        var problemSolver = await CreateBasicReActSolverAsync(kernel, templateEngine);

        // Problem scenarios
        var problems = new[]
        {
            new {
                Title = "Budget Planning",
                Description = "Our team needs to reduce operational costs by 20% while maintaining service quality. Current monthly spending is $50,000 across 5 departments."
            },
            new {
                Title = "System Performance",
                Description = "Our web application is experiencing slow response times (>3 seconds) during peak hours. The database queries seem to be the bottleneck."
            },
            new {
                Title = "Team Productivity",
                Description = "Project deadlines are being missed consistently. Team members report feeling overwhelmed with current workload and unclear priorities."
            }
        };

        Console.WriteLine("🤔 Solving problems using ReAct pattern...\n");

        foreach (var problem in problems)
        {
            Console.WriteLine($"🎯 Problem: {problem.Title}");
            Console.WriteLine($"📝 Description: {problem.Description}");
            Console.WriteLine();

            var arguments = new KernelArguments
            {
                ["problem_title"] = problem.Title,
                ["task_description"] = problem.Description,
                ["max_iterations"] = 3,
                ["solver_mode"] = "systematic",
                ["domain"] = "general"
            };

            var result = await problemSolver.ExecuteAsync(kernel, arguments);
            var solution = result.GetValue<string>() ?? "No solution generated";

            Console.WriteLine($"💡 ReAct Solution:");
            Console.WriteLine($"   {solution}");
            Console.WriteLine();
            Console.WriteLine("─────────────────────────────────────────");

            await Task.Delay(1000);
        }

        Console.WriteLine("✅ Basic ReAct problem solving example completed successfully!\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error in basic ReAct problem solving example: {ex.Message}\n");
    }
}
```

### 2. Basic ReAct Solver Creation

The basic solver implements the core ReAct cycle with four main nodes.

```csharp
private static async Task<GraphExecutor> CreateBasicReActSolverAsync(
    Kernel kernel,
    ReActTemplateEngine templateEngine)
{
    var executor = new GraphExecutor("BasicReActSolver", "Basic ReAct problem solving agent");

    // ReAct cycle nodes - using mock functions to avoid LLM dependencies
    var reasoningNode = new FunctionGraphNode(
        CreateMockReasoningFunction(kernel),
        "reasoning_node",
        "Problem Solving Reasoning"
    );

    var actionNode = ActionGraphNode.CreateWithActions(
        kernel,
        new ActionSelectionCriteria
        {
            // Keep broad; examples add a few mock functions into the kernel
            FunctionNamePattern = null,
            MinRequiredParameters = 0,
            MaxRequiredParameters = 5
        },
        "action_node");
    actionNode.ConfigureExecution(ActionSelectionStrategy.Intelligent, enableParameterValidation: true);

    var observationNode = new FunctionGraphNode(
        CreateMockObservationFunction(kernel),
        "observation_node",
        "Problem Solving Observation"
    );

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

    // Set up ReAct flow
    executor.SetStartNode(reasoningNode.NodeId);
    executor.AddEdge(ConditionalEdge.CreateUnconditional(reasoningNode, actionNode));
    executor.AddEdge(ConditionalEdge.CreateUnconditional(actionNode, observationNode));
    executor.AddEdge(ConditionalEdge.CreateUnconditional(observationNode, solutionNode));

    // Ensure required inputs for downstream prompts before validation of next node
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
```

### 3. Complex Multi-Step Problem Solving

The second example handles complex problems requiring multiple analysis stages.

```csharp
private static async Task RunComplexProblemSolvingAsync(Kernel kernel)
{
    Console.WriteLine("--- Example 2: Complex Multi-Step Problem Solving ---");

    try
    {
        var templateEngine = new ReActTemplateEngine();

        var complexSolver = await CreateComplexReActSolverAsync(kernel, templateEngine);

        // Complex problem scenario
        var complexProblem = @"
PROBLEM: Digital Transformation Strategy

CONTEXT:
Our traditional manufacturing company (500 employees) needs to undergo digital transformation to remain competitive. We face multiple challenges:

1. TECHNICAL CHALLENGES:
   - Legacy systems (20+ years old) running critical operations
   - Limited IT infrastructure and expertise
   - Cybersecurity concerns with increased connectivity
   - Integration difficulties between old and new systems

2. ORGANIZATIONAL CHALLENGES:
   - Resistance to change from long-term employees
   - Lack of digital skills across workforce
   - Limited budget for comprehensive transformation
   - Competing priorities and unclear ROI

3. MARKET PRESSURES:
   - Competitors adopting Industry 4.0 technologies
   - Customer expectations for digital services
   - Supply chain digitization requirements
   - Regulatory compliance for data handling

CONSTRAINTS:
- Budget: $2M over 24 months
- Cannot halt current operations during transition
- Must maintain current quality standards
- Regulatory compliance requirements

GOALS:
- Increase operational efficiency by 30%
- Reduce manual processes by 50%
- Improve customer satisfaction scores
- Establish foundation for future innovations
";

        Console.WriteLine("🎯 Solving Complex Digital Transformation Problem...\n");
        Console.WriteLine("📋 Problem Context:");
        Console.WriteLine(complexProblem.Substring(0, Math.Min(500, complexProblem.Length)) + "...");
        Console.WriteLine();

        var arguments = new KernelArguments
        {
            ["problem_title"] = "Digital Transformation Strategy",
            ["task_description"] = complexProblem,
            ["max_iterations"] = 5,
            ["solver_mode"] = "comprehensive",
            ["domain"] = "business_strategy",
            ["complexity_level"] = "high"
        };

        var result = await complexSolver.ExecuteAsync(kernel, arguments);
        var comprehensiveSolution = result.GetValue<string>() ?? "Complex solution not generated";

        Console.WriteLine($"💡 Comprehensive ReAct Solution:");
        Console.WriteLine($"   {comprehensiveSolution}");
        Console.WriteLine();

        Console.WriteLine("✅ Complex ReAct problem solving example completed successfully!\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error in complex ReAct problem solving example: {ex.Message}\n");
    }
}
```

### 4. Complex ReAct Solver with Multiple Stages

The complex solver implements a multi-stage analysis workflow.

```csharp
private static async Task<GraphExecutor> CreateComplexReActSolverAsync(
    Kernel kernel,
    ReActTemplateEngine templateEngine)
{
    var executor = new GraphExecutor("ComplexReActSolver", "Advanced multi-stage ReAct problem solver");

    // Multi-stage ReAct nodes - using mock functions to avoid LLM dependencies
    var initialAnalysisNode = new FunctionGraphNode(
        CreateMockReasoningFunction(kernel),
        "initial_analysis",
        "Initial Problem Analysis"
    );

    var stakeholderAnalysisNode = new FunctionGraphNode(
        CreateStakeholderAnalysisFunction(kernel),
        "stakeholder_analysis",
        "Stakeholder Analysis"
    );

    var constraintAnalysisNode = new FunctionGraphNode(
        CreateConstraintAnalysisFunction(kernel),
        "constraint_analysis",
        "Constraint Analysis"
    );

    var optionGenerationNode = ActionGraphNode.CreateWithActions(
        kernel,
        new ActionSelectionCriteria
        {
            MinRequiredParameters = 0,
            MaxRequiredParameters = 6
        },
        "option_generation");
    optionGenerationNode.ConfigureExecution(ActionSelectionStrategy.Intelligent, enableParameterValidation: true);

    var riskAssessmentNode = new FunctionGraphNode(
        CreateRiskAssessmentFunction(kernel),
        "risk_assessment",
        "Risk Assessment"
    );

    var implementationPlanNode = ActionGraphNode.CreateWithActions(
        kernel,
        new ActionSelectionCriteria
        {
            MinRequiredParameters = 0,
            MaxRequiredParameters = 6
        },
        "implementation_plan");
    implementationPlanNode.ConfigureExecution(ActionSelectionStrategy.Intelligent, enableParameterValidation: true);

    var evaluationNode = new FunctionGraphNode(
        CreateMockObservationFunction(kernel),
        "solution_evaluation",
        "Solution Evaluation"
    );

    var strategicSynthesisNode = new FunctionGraphNode(
        CreateStrategicSynthesisFunction(kernel),
        "strategic_synthesis",
        "Strategic Solution Synthesis"
    );

    // Add all nodes
    executor.AddNode(initialAnalysisNode);
    executor.AddNode(stakeholderAnalysisNode);
    executor.AddNode(constraintAnalysisNode);
    executor.AddNode(optionGenerationNode);
    executor.AddNode(riskAssessmentNode);
    executor.AddNode(implementationPlanNode);
    executor.AddNode(evaluationNode);
    executor.AddNode(strategicSynthesisNode);

    // Complex multi-stage flow
    executor.SetStartNode(initialAnalysisNode.NodeId);
    executor.AddEdge(ConditionalEdge.CreateUnconditional(initialAnalysisNode, stakeholderAnalysisNode));
    executor.AddEdge(ConditionalEdge.CreateUnconditional(stakeholderAnalysisNode, constraintAnalysisNode));
    executor.AddEdge(ConditionalEdge.CreateUnconditional(constraintAnalysisNode, optionGenerationNode));
    executor.AddEdge(ConditionalEdge.CreateUnconditional(optionGenerationNode, riskAssessmentNode));
    executor.AddEdge(ConditionalEdge.CreateUnconditional(riskAssessmentNode, implementationPlanNode));
    executor.AddEdge(ConditionalEdge.CreateUnconditional(implementationPlanNode, evaluationNode));
    executor.AddEdge(ConditionalEdge.CreateUnconditional(evaluationNode, strategicSynthesisNode));

    // Map or provide defaults for required inputs before next-node validation
    initialAnalysisNode.SetMetadata("AfterExecute",
        new Func<Kernel, KernelArguments, FunctionResult, CancellationToken, Task>((k, args, result, ct) =>
        {
            if (!args.ContainsName("problem_description") && args.TryGetValue("task_description", out var desc))
            {
                args["problem_description"] = desc;
            }
            if (!args.ContainsName("solution_options"))
            {
                args["solution_options"] = "Option A; Option B; Option C";
            }
            return Task.CompletedTask;
        }));

    return executor;
}
```

### 5. Iterative Problem Solving with Refinement

The third example demonstrates iterative problem solving with feedback loops.

```csharp
private static async Task RunIterativeProblemSolvingAsync(Kernel kernel)
{
    Console.WriteLine("--- Example 3: Iterative Problem Solving with Refinement ---");

    try
    {
        var templateEngine = new ReActTemplateEngine();

        var iterativeSolver = await CreateIterativeReActSolverAsync(kernel, templateEngine);

        // Iterative problem scenario
        var iterativeProblem = @"
EVOLVING PROBLEM: Customer Service Optimization

INITIAL STATE:
- Customer satisfaction: 3.2/5.0
- Average response time: 24 hours
- Resolution rate: 65%
- Customer churn: 15% monthly

FEEDBACK CYCLE:
This problem requires iterative refinement based on:
1. Initial solution testing
2. Customer feedback analysis
3. Performance metrics monitoring
4. Continuous improvement adjustments

TARGET STATE:
- Customer satisfaction: >4.5/5.0
- Average response time: <4 hours
- Resolution rate: >90%
- Customer churn: <5% monthly
";

        Console.WriteLine("🔄 Solving problem with iterative refinement...\n");
        Console.WriteLine("📋 Iterative Problem Context:");
        Console.WriteLine(iterativeProblem);
        Console.WriteLine();

        var arguments = new KernelArguments
        {
            ["problem_title"] = "Customer Service Optimization",
            ["task_description"] = iterativeProblem,
            ["max_iterations"] = 4,
            ["solver_mode"] = "iterative",
            ["domain"] = "customer_service",
            ["refinement_cycles"] = 3,
            ["feedback_integration"] = true
        };

        var result = await iterativeSolver.ExecuteAsync(kernel, arguments);
        var iterativeSolution = result.GetValue<string>() ?? "Iterative solution not generated";

        Console.WriteLine($"💡 Iterative ReAct Solution:");
        Console.WriteLine($"   {iterativeSolution}");
        Console.WriteLine();

        Console.WriteLine("✅ Iterative ReAct problem solving example completed successfully!\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error in iterative ReAct problem solving example: {ex.Message}\n");
    }
}
```

### 6. Iterative ReAct Solver with Feedback Loops

The iterative solver implements refinement cycles with convergence checking.

```csharp
private static async Task<GraphExecutor> CreateIterativeReActSolverAsync(
    Kernel kernel,
    ReActTemplateEngine templateEngine)
{
    var executor = new GraphExecutor("IterativeReActSolver", "Iterative ReAct solver with refinement loops");

    // Add some functions to the kernel for the ActionGraphNode to discover
    kernel.ImportPluginFromFunctions("react_actions", "Actions for ReAct pattern", new[]
    {
        kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var action = args["action"]?.ToString() ?? "unknown";
                return $"Executed action: {action}";
            },
            functionName: "execute_action",
            description: "Executes a specified action"
        ),

        kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var problem = args["problem"]?.ToString() ?? "unknown";
                return $"Analyzed problem: {problem}";
            },
            functionName: "analyze_problem",
            description: "Analyzes a given problem"
        ),

        kernel.CreateFunctionFromMethod(
            (KernelArguments args) =>
            {
                var solution = args["solution"]?.ToString() ?? "unknown";
                return $"Evaluated solution: {solution}";
            },
            functionName: "evaluate_solution",
            description: "Evaluates a proposed solution"
        )
    });

    // Create individual ReAct components manually to avoid the complex ReActLoopGraphNode
    var reasoningNode = new FunctionGraphNode(
        CreateMockReasoningFunction(kernel),
        "iterative_reasoning",
        "Iterative Problem Solving Reasoning"
    );

    var actionNode = ActionGraphNode.CreateWithActions(
        kernel,
        new ActionSelectionCriteria
        {
            MinRequiredParameters = 0,
            MaxRequiredParameters = 6
        },
        "iterative_action");
    actionNode.ConfigureExecution(ActionSelectionStrategy.Intelligent, enableParameterValidation: true);

    var observationNode = new FunctionGraphNode(
        CreateMockObservationFunction(kernel),
        "iterative_observation",
        "Iterative Problem Solving Observation"
    );

    var feedbackAnalysisNode = new FunctionGraphNode(
        CreateFeedbackAnalysisFunction(kernel),
        "feedback_analysis",
        "Feedback Analysis"
    );

    var refinementNode = new FunctionGraphNode(
        CreateSolutionRefinementFunction(kernel),
        "solution_refinement",
        "Solution Refinement"
    );

    var convergenceNode = new FunctionGraphNode(
        CreateConvergenceCheckFunction(kernel),
        "convergence_check",
        "Convergence Assessment"
    );

    var finalSolutionNode = new FunctionGraphNode(
        CreateFinalSolutionFunction(kernel),
        "final_solution",
        "Final Solution Generation"
    );

    // Add nodes
    executor.AddNode(reasoningNode);
    executor.AddNode(actionNode);
    executor.AddNode(observationNode);
    executor.AddNode(feedbackAnalysisNode);
    executor.AddNode(refinementNode);
    executor.AddNode(convergenceNode);
    executor.AddNode(finalSolutionNode);

    // Iterative flow with feedback loops
    executor.SetStartNode(reasoningNode.NodeId);
    executor.AddEdge(ConditionalEdge.CreateUnconditional(reasoningNode, actionNode));
    executor.AddEdge(ConditionalEdge.CreateUnconditional(actionNode, observationNode));
    executor.AddEdge(ConditionalEdge.CreateUnconditional(observationNode, feedbackAnalysisNode));
    executor.AddEdge(ConditionalEdge.CreateUnconditional(feedbackAnalysisNode, refinementNode));
    executor.AddEdge(ConditionalEdge.CreateUnconditional(refinementNode, convergenceNode));

    // Conditional edges for iteration vs completion
    executor.AddEdge(new ConditionalEdge(
        convergenceNode,
        reasoningNode,
        args => ShouldContinueIterating(args),
        "Continue Iteration"
    ));

    executor.AddEdge(new ConditionalEdge(
        convergenceNode,
        finalSolutionNode,
        args => !ShouldContinueIterating(args),
        "Finalize Solution"
    ));

    // Persist intermediate results required by downstream prompts
    feedbackAnalysisNode.StoreResultAs("feedback_analysis");
    refinementNode.StoreResultAs("current_solution");

    // Provide defaults/mappings for required inputs prior to validation of subsequent nodes
    observationNode.SetMetadata("AfterExecute",
        new Func<Kernel, KernelArguments, FunctionResult, CancellationToken, Task>((k, args, result, ct) =>
        {
            if (!args.ContainsName("iteration_count")) args["iteration_count"] = 1;
            if (!args.ContainsName("previous_results")) args["previous_results"] = "";
            if (!args.ContainsName("problem_description") && args.TryGetValue("task_description", out var desc))
            {
                args["problem_description"] = desc;
            }
            if (!args.ContainsName("target_criteria")) args["target_criteria"] = "Meets goals and constraints";
            return Task.CompletedTask;
        }));

    refinementNode.SetMetadata("AfterExecute",
        new Func<Kernel, KernelArguments, FunctionResult, CancellationToken, Task>((k, args, result, ct) =>
        {
            if (!args.ContainsName("current_solution")) args["current_solution"] = result.GetValue<string>() ?? "Initial proposal";
            return Task.CompletedTask;
        }));

    convergenceNode.SetMetadata("AfterExecute",
        new Func<Kernel, KernelArguments, FunctionResult, CancellationToken, Task>((k, args, result, ct) =>
        {
            if (!args.ContainsName("target_criteria"))
            {
                args["target_criteria"] = "Meets goals and constraints";
            }

            // Increment iteration counter and update a simple quality score to ensure convergence
            int currentIteration;
            try { currentIteration = Convert.ToInt32(args.GetValueOrDefault("iteration_count", 1), System.Globalization.CultureInfo.InvariantCulture); }
            catch { currentIteration = 1; }

            int maxIterations;
            try { maxIterations = Convert.ToInt32(args.GetValueOrDefault("max_iterations", 3), System.Globalization.CultureInfo.InvariantCulture); }
            catch { maxIterations = 3; }

            var nextIteration = currentIteration + 1;
            args["iteration_count"] = nextIteration;

            // Quality score increases with iterations toward 1.0, ensuring eventual convergence
            var denominator = Math.Max(1, maxIterations);
            double progress = Math.Min(1.0, nextIteration / (double)denominator);
            args["quality_score"] = progress;

            return Task.CompletedTask;
        }));

    finalSolutionNode.SetMetadata("AfterExecute",
        new Func<Kernel, KernelArguments, FunctionResult, CancellationToken, Task>((k, args, result, ct) =>
        {
            if (!args.ContainsName("refinement_history")) args["refinement_history"] = "No history";
            if (!args.ContainsName("final_analysis")) args["final_analysis"] = args.GetValueOrDefault("current_solution", "");
            return Task.CompletedTask;
        }));

    return executor;
}
```

### 7. Function Creation and Templates

The example demonstrates various approaches to creating functions for the ReAct workflow.

```csharp
// Mock reasoning function for problem solving
private static KernelFunction CreateMockReasoningFunction(Kernel kernel)
{
    return kernel.CreateFunctionFromMethod(
        (KernelArguments args) =>
        {
            var taskDescription = args["task_description"]?.ToString() ?? "unknown task";
            var problemTitle = args["problem_title"]?.ToString() ?? "unknown problem";

            return $"Analyzed problem '{problemTitle}': {taskDescription}. Based on analysis, the next step should be to identify key stakeholders and constraints.";
        },
        functionName: "mock_reasoning",
        description: "Mock reasoning function for problem solving"
    );
}

// Solution synthesis function using prompt templates
private static KernelFunction CreateSolutionSynthesisFunction(Kernel kernel)
{
    var prompt = @"
Synthesize a comprehensive solution based on ReAct analysis:

Problem: {{$problem_title}}
Description: {{$problem_description}}
Solver Mode: {{$solver_mode}}

Based on the ReAct reasoning, action planning, and observation:

1. Synthesize key insights from analysis
2. Prioritize the most effective actions
3. Create implementation roadmap
4. Identify success metrics
5. Highlight potential risks and mitigation

Provide comprehensive solution synthesis:";

    return kernel.CreateFunctionFromPrompt(
        prompt,
        functionName: "solution_synthesis",
        description: "Synthesizes comprehensive solutions from ReAct analysis"
    );
}

// Constraint analysis function with mock implementation
private static KernelFunction CreateConstraintAnalysisFunction(Kernel kernel)
{
    return kernel.CreateFunctionFromMethod(
        (KernelArguments args) =>
        {
            var problemDescription = args["problem_description"]?.ToString()
                ?? args["task_description"]?.ToString()
                ?? "unknown problem";
            var domain = args["domain"]?.ToString() ?? "general";

            var analysis = $"Constraint analysis for domain '{domain}':\n" +
                           "1) Resource constraints: Budget, time, personnel must be prioritized across phases; enforce strict scope control and staged funding.\n" +
                           "2) Technical constraints: Legacy systems and integrations require strangler patterns, API gateways, and phased modernization with strong observability.\n" +
                           "3) Organizational constraints: Change management, capability gaps, and training cadence must be embedded into the plan; designate transformation champions.\n" +
                           "4) Regulatory constraints: Data residency, privacy, and auditability shape architecture choices; implement policy-as-code and compliance-by-design.\n" +
                           "5) Market constraints: Customer expectations and competitive benchmarks set minimum viable feature baselines and SLAs.\n" +
                           "6) Risk tolerance: Define acceptable risk bands and mitigation triggers; adopt progressive rollouts and kill-switches.\n\n" +
                           $"Context considered: {problemDescription.Substring(0, Math.Min(200, problemDescription.Length))}...";

            return analysis;
        },
        functionName: "constraint_analysis",
        description: "Analyzes constraints and limitations"
    );
}
```

### 8. Convergence Logic and Iteration Control

The example implements sophisticated convergence checking for iterative refinement.

```csharp
// Determines if iteration should continue based on convergence criteria
private static bool ShouldContinueIterating(KernelArguments args)
{
    // Robust convergence check: tolerate int/double/string values
    static int ToInt(object? value, int defaultValue)
    {
        if (value is null) return defaultValue;
        try
        {
            return Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            return defaultValue;
        }
    }

    static double ToDouble(object? value, double defaultValue)
    {
        if (value is null) return defaultValue;
        try
        {
            return Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            return defaultValue;
        }
    }

    var iterationCount = ToInt(args.GetValueOrDefault("iteration_count", 0), 0);
    var maxIterations = ToInt(args.GetValueOrDefault("max_iterations", 3), 3);
    var qualityScore = ToDouble(args.GetValueOrDefault("quality_score", 0.5), 0.5);
    var convergenceThreshold = ToDouble(args.GetValueOrDefault("convergence_threshold", 0.85), 0.85);

    return iterationCount < maxIterations && qualityScore < convergenceThreshold;
}

// Convergence check function
private static KernelFunction CreateConvergenceCheckFunction(Kernel kernel)
{
    // Use a deterministic, method-based function to avoid external LLM dependency and
    // eliminate transient failures (e.g., HTTP 503) in example runs.
    return kernel.CreateFunctionFromMethod(
        (KernelArguments args) =>
        {
            var shouldContinue = ShouldContinueIterating(args);
            var iteration = args.GetValueOrDefault("iteration_count", 1)?.ToString();
            var quality = args.GetValueOrDefault("quality_score", 0.0)?.ToString();
            var threshold = args.GetValueOrDefault("convergence_threshold", 0.85)?.ToString();

            return shouldContinue
                ? $"Convergence check (iteration {iteration}): quality={quality}, threshold={threshold}. Not converged yet — continue refinement."
                : $"Convergence check (iteration {iteration}): quality={quality}, threshold={threshold}. Converged — finalize solution.";
        },
        functionName: "convergence_check",
        description: "Checks if solution has converged to acceptable quality without external calls"
    );
}

## Advanced Patterns

### Multi-Objective Problem Solving

```csharp
// Implement multi-objective optimization with weighted scoring
var multiObjectiveAgent = new MultiObjectiveReActAgent
{
    ObjectiveWeights = new Dictionary<string, double>
    {
        ["cost"] = 0.3,
        ["quality"] = 0.4,
        ["time"] = 0.2,
        ["risk"] = 0.1
    },
    ParetoFrontierAnalysis = new ParetoFrontierAnalyzer
    {
        MaxSolutions = 10,
        DominanceThreshold = 0.1,
        ConvergenceCriteria = new MultiObjectiveConvergenceCriteria
        {
            HypervolumeImprovement = 0.01,
            MaxGenerations = 50
        }
    }
};

// Solve multi-objective problem
var multiObjectiveResult = await multiObjectiveAgent.SolveAsync(kernel, multiObjectiveArgs);
```

### Adaptive Problem Decomposition

```csharp
// Implement adaptive problem decomposition based on complexity
var adaptiveDecomposer = new AdaptiveProblemDecomposer
{
    DecompositionStrategies = new Dictionary<string, IDecompositionStrategy>
    {
        ["hierarchical"] = new HierarchicalDecompositionStrategy(),
        ["parallel"] = new ParallelDecompositionStrategy(),
        ["iterative"] = new IterativeDecompositionStrategy()
    },
    ComplexityAnalyzer = new ProblemComplexityAnalyzer
    {
        ComplexityFactors = new[] { "stakeholder_count", "constraint_count", "domain_count" },
        StrategySelectionRules = new Dictionary<string, string>
        {
            ["low"] = "hierarchical",
            ["medium"] = "parallel",
            ["high"] = "iterative"
        }
    }
};

// Automatically decompose complex problems
var decomposition = await adaptiveDecomposer.DecomposeAsync(problemStatement);
var decomposedGraph = await adaptiveDecomposer.CreateDecomposedGraphAsync(decomposition);
```

### Collaborative Problem Solving

```csharp
// Implement collaborative problem solving with multiple agents
var collaborativeSolver = new CollaborativeProblemSolver
{
    AgentSpecializations = new Dictionary<string, AgentSpecialization>
    {
        ["analyst"] = new AnalystAgent { Domain = "business_analysis" },
        ["engineer"] = new EngineerAgent { Domain = "technical_implementation" },
        ["strategist"] = new StrategistAgent { Domain = "strategic_planning" }
    },
    CollaborationProtocol = new ConsensusProtocol
    {
        VotingMechanism = VotingMechanism.WeightedMajority,
        ConsensusThreshold = 0.75,
        ConflictResolution = ConflictResolution.Mediation
    },
    KnowledgeSharing = new KnowledgeSharingStrategy
    {
        SharedMemory = new SharedMemoryManager(),
        KnowledgeTransfer = KnowledgeTransfer.Continuous,
        LearningRate = 0.1
    }
};

// Solve problem collaboratively
var collaborativeResult = await collaborativeSolver.SolveCollaborativelyAsync(kernel, collaborativeArgs);
```
```

## Expected Output

The examples produce comprehensive output showing:

- 🎯 **Basic Problem Solving**: Systematic analysis of budget planning, system performance, and team productivity issues
- 🔍 **Complex Multi-Step Analysis**: Comprehensive stakeholder analysis, constraint evaluation, and risk assessment for digital transformation
- 🔄 **Iterative Refinement**: Customer service optimization with feedback loops and convergence checking
- 💡 **Solution Synthesis**: Actionable implementation roadmaps with success metrics and risk mitigation
- 📊 **Stakeholder Management**: Identification of key parties and communication strategies
- ⚠️ **Risk Assessment**: Comprehensive risk evaluation with mitigation strategies
- 🚀 **Implementation Planning**: Detailed execution plans with resource allocation and timelines

## Troubleshooting

### Common Issues

1. **LLM API Failures**: The example uses mock functions to avoid external dependencies
2. **State Mapping Errors**: Verify input/output mappings between nodes
3. **Convergence Issues**: Check iteration limits and quality thresholds
4. **Action Selection Failures**: Ensure kernel has appropriate functions for ActionGraphNode

### Debugging Tips

- Monitor state transformations in AfterExecute metadata handlers
- Verify convergence logic and iteration counting
- Check conditional edge routing for iterative workflows
- Validate function inputs and outputs between nodes

### Performance Considerations

- Use mock functions for deterministic testing
- Implement appropriate iteration limits to prevent infinite loops
- Monitor state size growth during iterative refinement
- Consider checkpointing for long-running iterative workflows

## See Also

- [ReAct Pattern](../concepts/react-pattern.md)
- [Action Nodes](../concepts/action-nodes.md)
- [Conditional Routing](../concepts/conditional-routing.md)
- [State Management](../concepts/state.md)
- [Graph Execution](../concepts/graph-execution.md)
- [ReAct Agent Example](./react-agent.md)
- [Problem Solving Patterns](../patterns/problem-solving.md)
