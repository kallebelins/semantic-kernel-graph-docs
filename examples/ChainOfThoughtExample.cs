using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Integration;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;

namespace Examples;

/// <summary>
/// Example demonstrating Chain-of-Thought reasoning patterns using the Semantic Kernel Graph.
/// Shows different reasoning types, validation, backtracking, and template customization.
/// </summary>
public static class ChainOfThoughtExample
{
    /// <summary>
    /// Runs comprehensive Chain-of-Thought examples.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Chain-of-Thought Reasoning Examples ===\n");

        // Initialize kernel and helper components
        var kernel = CreateKernel();
        var logger = (IGraphLogger?)null; // Use null logger for simplicity in example
        var templateEngine = new ChainOfThoughtTemplateEngine(options: null, logger: null);

        // Example 1: Problem Solving with Chain-of-Thought
        await RunProblemSolvingExampleAsync(kernel, templateEngine, logger);

        // Example 2: Analysis with Custom Templates
        await RunAnalysisExampleAsync(kernel, templateEngine, logger);

        // Example 3: Decision Making with Backtracking
        await RunDecisionMakingExampleAsync(kernel, templateEngine, logger);

        // Example 4: Performance and Cache Demonstration
        await RunPerformanceExampleAsync(kernel, templateEngine, logger);

        Console.WriteLine("=== All Chain-of-Thought examples completed! ===");
    }

    /// <summary>
    /// Example 1: Problem-solving with step-by-step reasoning.
    /// </summary>
    private static async Task RunProblemSolvingExampleAsync(
        Kernel kernel,
        ChainOfThoughtTemplateEngine templateEngine,
        IGraphLogger? logger)
    {
        Console.WriteLine("--- Example 1: Problem Solving Chain-of-Thought ---");

        // Create Chain-of-Thought node for problem solving
        var cotNode = new ChainOfThoughtGraphNode(
            ChainOfThoughtType.ProblemSolving,
            maxSteps: 5,
            templateEngine: templateEngine,
            logger: logger)
        {
            BacktrackingEnabled = true,
            MinimumStepConfidence = 0.6,
            CachingEnabled = true
        };

        // Create executor and add node
        var executor = new GraphExecutor("ChainOfThought-ProblemSolving", "Chain-of-Thought problem solving example", logger);
        executor.AddNode(cotNode);
        executor.SetStartNode(cotNode.NodeId);

        // Prepare arguments
        var arguments = new KernelArguments
        {
            ["problem_statement"] = "A company needs to reduce operational costs by 20% while maintaining employee satisfaction. They have 1000 employees, current annual costs of $50M, and recent employee survey shows 75% satisfaction.",
            ["context"] = "The company operates in a competitive tech market with high talent retention challenges.",
            ["constraints"] = "Cannot reduce headcount by more than 5%, must maintain current benefit levels, changes must be implemented within 6 months.",
            ["expected_outcome"] = "A comprehensive cost reduction plan with specific actionable steps",
            ["reasoning_depth"] = 4
        };

        try
        {
            Console.WriteLine("🧠 Starting problem-solving reasoning...");
            var result = await executor.ExecuteAsync(kernel, arguments, CancellationToken.None);
            var finalAnswer = result.GetValue<string>() ?? "(no result)";

            Console.WriteLine($"✅ Final Answer: {finalAnswer}");
            Console.WriteLine($"📊 Node Statistics: {cotNode.Statistics.ExecutionCount} executions, " +
                            $"{cotNode.Statistics.AverageQualityScore:P1} avg quality, " +
                            $"{cotNode.Statistics.SuccessRate:P1} success rate");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Example 2: Analysis with custom templates and validation rules.
    /// </summary>
    private static async Task RunAnalysisExampleAsync(
        Kernel kernel,
        ChainOfThoughtTemplateEngine templateEngine,
        IGraphLogger? logger)
    {
        Console.WriteLine("--- Example 2: Analysis with Custom Templates ---");

        // Create custom templates for analysis
        var customTemplates = new Dictionary<string, string>
        {
            ["step_1"] = @"You are analyzing a complex situation. This is step {{step_number}}.

Situation: {{problem_statement}}
Context: {{context}}

Start by identifying the key stakeholders and their interests. Who are the main parties involved and what do they care about?

Your analysis:",

            ["analysis_step"] = @"Continue your analysis. This is step {{step_number}} of {{max_steps}}.

Previous analysis:
{{previous_steps}}

Now examine the following aspect: What are the underlying causes and contributing factors? Look deeper than surface-level observations.

Your analysis:"
        };

        // Create custom validation rules
        var customRules = new List<IChainOfThoughtValidationRule>
        {
            new StakeholderAnalysisRule(),
            new CausalAnalysisRule()
        };

        // Create Chain-of-Thought node with customizations
        var cotNode = ChainOfThoughtGraphNode.CreateWithCustomization(
            ChainOfThoughtType.Analysis,
            customTemplates,
            customRules,
            maxSteps: 4,
            templateEngine: templateEngine,
            logger: logger);

        var executor = new GraphExecutor("ChainOfThought-Example", "Chain-of-Thought example", logger);
        executor.AddNode(cotNode);
        executor.SetStartNode(cotNode.NodeId);

        var arguments = new KernelArguments
        {
            ["problem_statement"] = "Analyze the factors contributing to declining user engagement in a social media platform that has seen a 30% drop in daily active users over 6 months.",
            ["context"] = "The platform has 10M registered users, primarily aged 18-35, competing with TikTok and Instagram. Recent algorithm changes were implemented 8 months ago.",
            ["constraints"] = "Focus on actionable insights that can be implemented within 3 months with current development resources.",
            ["reasoning_depth"] = 4
        };

        try
        {
            Console.WriteLine("🔍 Starting analytical reasoning with custom templates...");
            var result = await executor.ExecuteAsync(kernel, arguments, CancellationToken.None);
            var analysisResult = result.GetValue<string>() ?? "(no result)";

            Console.WriteLine($"✅ Analysis Result: {analysisResult}");
            Console.WriteLine($"📊 Validation Stats: {cotNode.Statistics.BacktrackingCount} backtracks needed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Example 3: Decision making with backtracking demonstration.
    /// </summary>
    private static async Task RunDecisionMakingExampleAsync(
        Kernel kernel,
        ChainOfThoughtTemplateEngine templateEngine,
        IGraphLogger? logger)
    {
        Console.WriteLine("--- Example 3: Decision Making with Backtracking ---");

        var cotNode = new ChainOfThoughtGraphNode(
            ChainOfThoughtType.DecisionMaking,
            maxSteps: 6,
            templateEngine: templateEngine,
            logger: logger)
        {
            BacktrackingEnabled = true,
            MinimumStepConfidence = 0.7 // Higher threshold to trigger backtracking
        };

        var executor = new GraphExecutor("ChainOfThought-Example", "Chain-of-Thought example", logger);
        executor.AddNode(cotNode);
        executor.SetStartNode(cotNode.NodeId);

        var arguments = new KernelArguments
        {
            ["problem_statement"] = "Should a startup with $2M funding accept a $15M acquisition offer or continue growing independently?",
            ["context"] = "The startup has 25 employees, growing at 15% MoM, $200K monthly revenue, 18 months runway. The acquirer is a Fortune 500 company offering integration into their platform.",
            ["constraints"] = "Decision must be made within 2 weeks, founders want to maintain some control, team retention is crucial.",
            ["expected_outcome"] = "A clear recommendation with supporting rationale",
            ["reasoning_depth"] = 5
        };

        try
        {
            Console.WriteLine("⚖️ Starting decision-making reasoning with backtracking...");
            var result = await executor.ExecuteAsync(kernel, arguments, CancellationToken.None);
            var decision = result.GetValue<string>() ?? "(no result)";

            Console.WriteLine($"✅ Decision Recommendation: {decision}");

            var stats = cotNode.Statistics;
            if (stats.BacktrackingCount > 0)
            {
                Console.WriteLine($"🔄 Backtracking was used {stats.BacktrackingCount} times to improve reasoning quality");
            }

            Console.WriteLine($"📊 Final Quality Score: {stats.AverageQualityScore:P1}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Example 4: Performance demonstration with caching.
    /// </summary>
    private static async Task RunPerformanceExampleAsync(
        Kernel kernel,
        ChainOfThoughtTemplateEngine templateEngine,
        IGraphLogger? logger)
    {
        Console.WriteLine("--- Example 4: Performance and Caching ---");

        var cotNode = new ChainOfThoughtGraphNode(
            ChainOfThoughtType.Planning,
            maxSteps: 3,
            templateEngine: templateEngine,
            logger: logger)
        {
            CachingEnabled = true
        };

        var executor = new GraphExecutor("ChainOfThought-Example", "Chain-of-Thought example", logger);
        executor.AddNode(cotNode);
        executor.SetStartNode(cotNode.NodeId);

        var baseArguments = new KernelArguments
        {
            ["problem_statement"] = "Create a 3-month marketing plan for launching a new mobile app targeting fitness enthusiasts.",
            ["context"] = "The app includes workout tracking, nutrition logging, and community features. Target market is 25-45 year olds.",
            ["constraints"] = "Budget is $100K, need to achieve 10K downloads in first month",
            ["reasoning_depth"] = 3
        };

        try
        {
            Console.WriteLine("🚀 Running first execution (cold start)...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result1 = await executor.ExecuteAsync(kernel, baseArguments, CancellationToken.None);
            stopwatch.Stop();
            var firstExecutionTime = stopwatch.Elapsed;

            Console.WriteLine($"⏱️ First execution: {firstExecutionTime.TotalSeconds:F2}s");

            // Run similar request to test caching
            Console.WriteLine("🔄 Running similar request (should hit cache)...");
            stopwatch.Restart();
            var result2 = await executor.ExecuteAsync(kernel, baseArguments, CancellationToken.None);
            stopwatch.Stop();
            var secondExecutionTime = stopwatch.Elapsed;

            Console.WriteLine($"⏱️ Second execution: {secondExecutionTime.TotalSeconds:F2}s");

            var stats = cotNode.Statistics;
            Console.WriteLine($"📊 Cache Performance: {stats.CacheHitRate:P1} hit rate");

            if (secondExecutionTime < firstExecutionTime)
            {
                var improvement = (1 - (secondExecutionTime.TotalSeconds / firstExecutionTime.TotalSeconds)) * 100;
                Console.WriteLine($"⚡ Performance improvement: {improvement:F1}% faster");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Creates a kernel with real OpenAI configuration from appsettings.json.
    /// </summary>
    private static Kernel CreateKernel()
    {
        // Load configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var openAiApiKey = configuration["OpenAI:ApiKey"];
        var openAiModel = configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";

        if (string.IsNullOrEmpty(openAiApiKey))
        {
            Console.WriteLine("⚠️  OpenAI API Key not found in appsettings.json. Using mock key for demonstration.");
            openAiApiKey = "mock-api-key";
        }

        var builder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(openAiModel, openAiApiKey)
            .AddGraphSupport();

        return builder.Build();
    }
}

// Custom validation rules used in examples
public class StakeholderAnalysisRule : IChainOfThoughtValidationRule
{
    public string Name => "StakeholderAnalysis";
    public string Description => "Validates that analysis includes stakeholder considerations";

    public async Task<ChainOfThoughtValidationResult> ValidateAsync(
        ChainOfThoughtStep step,
        ChainOfThoughtContext context,
        ChainOfThoughtResult previousResult,
        CancellationToken cancellationToken = default)
    {
        var result = new ChainOfThoughtValidationResult
        {
            StepNumber = step.StepNumber,
            IsValid = true,
            Issues = new List<ValidationIssue>()
        };

        var content = step.Content.ToLowerInvariant();
        var stakeholderKeywords = new[] { "stakeholder", "party", "user", "customer", "employee", "investor", "user" };

        if (step.StepNumber <= 2 && !stakeholderKeywords.Any(keyword => content.Contains(keyword)))
        {
            result.Issues.Add(new ValidationIssue(ValidationSeverity.Warning,
                "Early analysis steps should consider stakeholder perspectives",
                "StakeholderAnalysis"));
        }

        await Task.CompletedTask;
        return result;
    }
}

public class CausalAnalysisRule : IChainOfThoughtValidationRule
{
    public string Name => "CausalAnalysis";
    public string Description => "Validates that analysis explores underlying causes";

    public async Task<ChainOfThoughtValidationResult> ValidateAsync(
        ChainOfThoughtStep step,
        ChainOfThoughtContext context,
        ChainOfThoughtResult previousResult,
        CancellationToken cancellationToken = default)
    {
        var result = new ChainOfThoughtValidationResult
        {
            StepNumber = step.StepNumber,
            IsValid = true,
            Issues = new List<ValidationIssue>()
        };

        var content = step.Content.ToLowerInvariant();
        var causalKeywords = new[] { "because", "cause", "reason", "factor", "lead", "result", "due to", "stems from" };

        if (step.StepNumber > 1 && !causalKeywords.Any(keyword => content.Contains(keyword)))
        {
            result.Issues.Add(new ValidationIssue(ValidationSeverity.Info,
                "Consider exploring causal relationships in your analysis",
                "CausalAnalysis"));
        }

        await Task.CompletedTask;
        return result;
    }
}


