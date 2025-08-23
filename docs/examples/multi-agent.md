# Multi-Agent Example

This example demonstrates multi-agent coordination capabilities in Semantic Kernel Graph, showing how to create, configure, and execute workflows with multiple coordinated agents.

## Objective

Learn how to implement multi-agent coordination in graph-based workflows to:
* Create and manage specialized agents with specific capabilities
* Distribute work across multiple agents using different strategies
* Coordinate complex workflows with explicit task definitions
* Monitor agent health and system performance
* Aggregate results from multiple agents using various strategies

## Prerequisites

* **.NET 8.0** or later
* **OpenAI API Key** configured in `appsettings.json`
* **Semantic Kernel Graph package** installed
* Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Multi-Agent Coordination](../how-to/multi-agent-and-shared-state.md)
* Familiarity with [Workflow Management](../concepts/execution.md)

## Key Components

### Concepts and Techniques

* **Multi-Agent Coordination**: Managing multiple specialized agents in coordinated workflows
* **Work Distribution**: Automatic and manual distribution of tasks across agents
* **Capability Management**: Defining and requiring specific agent capabilities
* **Health Monitoring**: Tracking agent status and system performance
* **Result Aggregation**: Combining results from multiple agents using various strategies

### Core Classes

* `MultiAgentCoordinator`: Main coordinator for managing multiple agents
* `AgentInstance`: Individual agent instances with specific capabilities
* `MultiAgentOptions`: Configuration options for coordination behavior
* `WorkflowBuilder`: Builder pattern for creating complex workflows
* `AgentHealthMonitor`: Monitoring agent health and system status

## Running the Example

### Getting Started

This example demonstrates multi-agent coordination and workflow orchestration with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Creating Multi-Agent Coordinator

The example starts by creating a coordinator with custom configuration options.

```csharp
// Create coordinator options used to configure multi-agent behavior.
// Comments explain each setting so readers can adapt them safely.
var options = new MultiAgentOptions
{
    // Maximum number of agents that may run concurrently.
    MaxConcurrentAgents = 5,

    // Overall timeout for coordination operations.
    CoordinationTimeout = TimeSpan.FromMinutes(10),

    // Configuration for shared state handling between agents.
    SharedStateOptions = new SharedStateOptions
    {
        ConflictResolutionStrategy = ConflictResolutionStrategy.Merge,
        AllowOverwrite = true
    },

    // How work is distributed among agents (role-based, round-robin, etc.).
    WorkDistributionOptions = new WorkDistributionOptions
    {
        DistributionStrategy = WorkDistributionStrategy.RoleBased,
        EnablePrioritization = true
    },

    // How results from multiple agents are aggregated.
    ResultAggregationOptions = new ResultAggregationOptions
    {
        DefaultAggregationStrategy = AggregationStrategy.Consensus,
        ConsensusThreshold = 0.6
    }
};

// Create the coordinator instance using the options above. The loggerFactory
// symbol is assumed to be available in the surrounding example application
// (it's shown elsewhere in the docs). Use a real ILoggerFactory in production.
using var coordinator = new MultiAgentCoordinator(options,
    new SemanticKernelGraphLogger(loggerFactory.CreateLogger<SemanticKernelGraphLogger>(), new GraphOptions()));
```

### 2. Basic Multi-Agent Scenario

#### Creating Specialized Agents

```csharp
// Create and register specialized agents used by this example. Each helper
// method returns an AgentInstance already registered with the coordinator.
var analysisAgent = await CreateAnalysisAgentAsync(coordinator, kernel, loggerFactory);
var processingAgent = await CreateProcessingAgentAsync(coordinator, kernel, loggerFactory);
var reportingAgent = await CreateReportingAgentAsync(coordinator, kernel, loggerFactory);

// Prepare kernel arguments that will be passed to the workflow. Use explicit
// keys so downstream functions can retrieve values safely.
var arguments = new KernelArguments
{
    ["input_text"] = "The quick brown fox jumps over the lazy dog. This is a sample text for analysis.",
    ["analysis_type"] = "comprehensive",
    ["output_format"] = "detailed_report"
};

// Execute a small workflow with automatic distribution and merge aggregation.
// The coordinator will dispatch tasks to the registered agents as required.
var result = await coordinator.ExecuteSimpleWorkflowAsync(
    kernel,
    arguments,
    new[] { analysisAgent.AgentId, processingAgent.AgentId, reportingAgent.AgentId },
    AggregationStrategy.Merge
);
```

#### Agent Creation Example

```csharp
private static async Task<AgentInstance> CreateAnalysisAgentAsync(
    MultiAgentCoordinator coordinator,
    Kernel kernel,
    ILoggerFactory loggerFactory)
{
    // Create a GraphExecutor configured for analysis tasks. The GraphExecutor
    // is a lightweight example executor used in examples; replace with a real
    // executor implementation in production code.
    var executor = new GraphExecutor(
        "Analysis Graph",
        "Specialized in text analysis",
        new SemanticKernelGraphLogger(loggerFactory.CreateLogger<SemanticKernelGraphLogger>(), new GraphOptions()));

    // Create a single analysis node that wraps a Kernel function. We set
    // storage and metadata to guide downstream agents and relax strict prompt
    // validation for examples.
    var analysisNode = new FunctionGraphNode(CreateAnalysisFunction(kernel), "analyze-text", "Text Analysis");
    analysisNode.StoreResultAs("input");
    analysisNode.SetMetadata("StrictValidation", false);

    executor.AddNode(analysisNode);
    executor.SetStartNode(analysisNode.NodeId);

    // Register the agent with the coordinator. Capabilities help the
    // coordinator select suitable agents for tasks.
    var agent = await coordinator.RegisterAgentAsync(
        agentId: "analysis-agent",
        name: "Text Analysis Agent",
        description: "Specialized in comprehensive text analysis",
        executor: executor,
        capabilities: new[] { "text-analysis", "pattern-recognition", "insight-extraction" },
        metadata: new Dictionary<string, object>
        {
            ["specialization"] = "text_analysis",
            ["version"] = "1.0",
            ["performance_profile"] = "high_accuracy"
        });

    return agent;
}
```

### 3. Advanced Workflow Scenario

The advanced workflow uses a builder pattern with explicit task definitions.

```csharp
// Build a complex workflow using the coordinator's fluent builder. The
// builder allows declaring required agents, tasks, parameters, and
// aggregation strategies in a readable way.
var workflow = coordinator.CreateWorkflow("advanced-analysis", "Advanced Text Analysis Workflow")
    .WithDescription("Comprehensive text analysis using multiple specialized agents")
    .RequireAgents("analysis-agent", "processing-agent", "reporting-agent")
    .AddTask("analyze-content", "Content Analysis", task => task
        .WithDescription("Analyze text content for patterns and insights")
        .WithPriority(10)
        .RequireCapabilities("text-analysis", "pattern-recognition")
        .WithParameter("analysis_depth", "deep")
        .WithEstimatedDuration(TimeSpan.FromMinutes(2)))
    .AddTask("process-results", "Result Processing", task => task
        .WithDescription("Process analysis results and extract key findings")
        .WithPriority(8)
        .RequireCapabilities("data-processing", "extraction")
        .WithParameter("processing_mode", "comprehensive")
        .WithEstimatedDuration(TimeSpan.FromMinutes(3)))
    .AddTask("generate-report", "Report Generation", task => task
        .WithDescription("Generate comprehensive report from processed data")
        .WithPriority(5)
        .RequireCapabilities("report-generation", "formatting")
        .WithParameter("report_format", "executive_summary")
        .WithEstimatedDuration(TimeSpan.FromMinutes(1)))
    .WithAggregationStrategy(AggregationStrategy.Weighted)
    .WithMetadata("workflow_type", "analysis")
    .WithMetadata("complexity", "high")
    .Build();

// Prepare arguments for the workflow. Keep keys explicit and documented so
// tasks can read them without ambiguity.
var arguments = new KernelArguments
{
    ["document_content"] = GetSampleDocument(),
    ["analysis_requirements"] = "sentiment, topics, key_phrases, entities",
    ["output_preferences"] = "json_structured"
};

// Execute the workflow and await the aggregated result.
var result = await coordinator.ExecuteWorkflowAsync(workflow, kernel, arguments);
```

### 4. Health Monitoring Scenario

The health monitoring scenario tracks agent status and system performance.

```csharp
// Retrieve all registered agents for monitoring and diagnostics.
var agents = coordinator.GetAllAgents();
logger.LogInformation($"Monitoring {agents.Count} agents...");

// Iterate agents and perform synchronous and asynchronous health checks.
foreach (var agent in agents)
{
    // Get cached or computed health status for quick inspection.
    var healthStatus = agent.GetHealthStatus(coordinator);
    logger.LogInformation($"Agent {agent.AgentId}: {healthStatus?.Status ?? HealthStatus.Unknown}");

    // Perform an explicit health check which may perform network or runtime checks.
    var healthCheck = await agent.PerformHealthCheckAsync(coordinator);
    logger.LogInformation($"  Health Check: {(healthCheck.Success ? "Passed" : "Failed")} " +
                        $"(Response: {healthCheck.ResponseTime.TotalMilliseconds:F2} ms)");
}

// Log aggregated system health metrics from the coordinator's monitor.
var healthMonitor = coordinator.HealthMonitor;
logger.LogInformation($"System Health: {healthMonitor.HealthyAgentCount}/{healthMonitor.MonitoredAgentCount} agents healthy ({healthMonitor.SystemHealthRatio:P})");
```

### 5. Agent Function Creation

The example creates specialized functions for different agent types.

```csharp
// Create a lightweight KernelFunction used in examples to perform analysis.
// The function reads arguments, performs a small deterministic computation,
// stores results back into the arguments, and returns a human-friendly message.
private static KernelFunction CreateAnalysisFunction(Kernel kernel)
{
    return KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            // Safely read the input text and analysis type from arguments.
            var input = args.TryGetValue("input_text", out var i) ? i?.ToString() ?? string.Empty : string.Empty;
            var analysisType = args.TryGetValue("analysis_type", out var a) ? a?.ToString() ?? "basic" : "basic";

            // Simulate a simple analysis result. In real scenarios replace with
            // calls to LLMs or other processing components.
            var analysisResult = new
            {
                TextLength = input.Length,
                WordCount = input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                AnalysisType = analysisType,
                Insights = new[] { "Sample insight 1", "Sample insight 2" },
                Confidence = 0.95
            };

            // Store the result so downstream tasks can use it.
            args["analysis_result"] = analysisResult;
            return $"Analysis completed: {analysisResult.WordCount} words, {analysisResult.Insights.Length} insights";
        },
        functionName: "analyze_text",
        description: "Performs comprehensive text analysis"
    );
}

// Create a processing function that consumes the analysis result and prepares
// a richer processed result for reporting or aggregation.
private static KernelFunction CreateProcessingFunction(Kernel kernel)
{
    return KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var analysisResult = args.TryGetValue("analysis_result", out var ar) ? ar : null;

            // Simulate enhancement of insights and metadata tagging.
            var processedResult = new
            {
                ProcessedAt = DateTime.UtcNow,
                EnhancedInsights = new[] { "Enhanced insight 1", "Enhanced insight 2", "Enhanced insight 3" },
                ProcessingQuality = "high",
                Metadata = new { Source = "analysis_agent", Version = "1.0" }
            };

            // Store processed result for later tasks.
            args["processed_result"] = processedResult;
            return $"Processing completed: {processedResult.EnhancedInsights.Length} enhanced insights";
        },
        functionName: "process_analysis",
        description: "Processes analysis results and enhances insights"
    );
}
```

### 6. Workflow Result Logging

The example includes comprehensive result logging for workflow execution.

```csharp
// Helper to log a WorkflowExecutionResult in a concise, human-friendly way.
// This function shows success, timing, agents involved, aggregated result and
// a sample of errors if present.
private static void LogWorkflowResult(WorkflowExecutionResult result, ILogger logger)
{
    logger.LogInformation("\nWorkflow Execution Results:");
    logger.LogInformation($"  Success: {result.Success}");
    logger.LogInformation($"  Execution ID: {result.ExecutionId}");
    logger.LogInformation($"  Duration: {result.Duration.TotalMilliseconds:F2} ms");
    logger.LogInformation($"  Agents Used: {result.AgentsUsed.Count}");

    foreach (var agent in result.AgentsUsed)
    {
        logger.LogInformation($"    - {agent.AgentId}: {agent.Status}");
    }

    if (result.AggregatedResult != null)
    {
        logger.LogInformation($"  Aggregated Result: {result.AggregatedResult}");
    }

    if (result.Errors.Any())
    {
        logger.LogInformation($"  Errors (showing up to 3): {result.Errors.Count}");
        foreach (var error in result.Errors.Take(3))
        {
            logger.LogInformation($"    - {error}");
        }
    }
}
```

## Expected Output

The example produces comprehensive output showing:

* 🤖 Multi-agent coordination setup and configuration
* 📋 Basic multi-agent scenario with task distribution
* 🔄 Advanced workflow with explicit task definitions
* 🏥 Health monitoring and agent status tracking
* 📊 Workflow execution results and performance metrics
* ✅ Successful coordination across multiple specialized agents

## Troubleshooting

### Common Issues

1. **Agent Registration Failures**: Ensure agent IDs are unique and capabilities are properly defined
2. **Workflow Execution Errors**: Check that required agents and capabilities are available
3. **Health Check Failures**: Verify agent connectivity and resource availability
4. **Coordination Timeouts**: Adjust timeout settings for complex workflows

### Debugging Tips

* Enable detailed logging to trace agent interactions
* Monitor agent health status and performance metrics
* Verify workflow requirements and agent capabilities match
* Check coordination timeout and concurrency settings

## See Also

* [Multi-Agent and Shared State](../how-to/multi-agent-and-shared-state.md)
* [Workflow Management](../concepts/execution.md)
* [Agent Coordination](../concepts/agent-coordination.md)
* [Health Monitoring](../how-to/health-monitoring.md)
* [Result Aggregation](../concepts/result-aggregation.md)
