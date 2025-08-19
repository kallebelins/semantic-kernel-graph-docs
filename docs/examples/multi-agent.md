# Multi-Agent Example

This example demonstrates multi-agent coordination capabilities in Semantic Kernel Graph, showing how to create, configure, and execute workflows with multiple coordinated agents.

## Objective

Learn how to implement multi-agent coordination in graph-based workflows to:
- Create and manage specialized agents with specific capabilities
- Distribute work across multiple agents using different strategies
- Coordinate complex workflows with explicit task definitions
- Monitor agent health and system performance
- Aggregate results from multiple agents using various strategies

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Multi-Agent Coordination](../how-to/multi-agent-and-shared-state.md)
- Familiarity with [Workflow Management](../concepts/execution.md)

## Key Components

### Concepts and Techniques

- **Multi-Agent Coordination**: Managing multiple specialized agents in coordinated workflows
- **Work Distribution**: Automatic and manual distribution of tasks across agents
- **Capability Management**: Defining and requiring specific agent capabilities
- **Health Monitoring**: Tracking agent status and system performance
- **Result Aggregation**: Combining results from multiple agents using various strategies

### Core Classes

- `MultiAgentCoordinator`: Main coordinator for managing multiple agents
- `AgentInstance`: Individual agent instances with specific capabilities
- `MultiAgentOptions`: Configuration options for coordination behavior
- `WorkflowBuilder`: Builder pattern for creating complex workflows
- `AgentHealthMonitor`: Monitoring agent health and system status

## Running the Example

### Command Line

```bash
# Navigate to examples project
cd semantic-kernel-graph/src/SemanticKernel.Graph.Examples

# Run the Multi-Agent example
dotnet run -- --example multi-agent
```

### Programmatic Execution

```csharp
// Run the example with custom kernel and logger factory
var kernel = CreateCustomKernel();
var loggerFactory = CreateCustomLoggerFactory();

await MultiAgentExample.ExecuteAsync(kernel, loggerFactory);
```

## Step-by-Step Implementation

### 1. Creating Multi-Agent Coordinator

The example starts by creating a coordinator with custom configuration options.

```csharp
// Create multi-agent coordinator with custom options
var options = new MultiAgentOptions
{
    MaxConcurrentAgents = 5,
    CoordinationTimeout = TimeSpan.FromMinutes(10),
    SharedStateOptions = new SharedStateOptions
    {
        ConflictResolutionStrategy = ConflictResolutionStrategy.Merge,
        AllowOverwrite = true
    },
    WorkDistributionOptions = new WorkDistributionOptions
    {
        DistributionStrategy = WorkDistributionStrategy.RoleBased,
        EnablePrioritization = true
    },
    ResultAggregationOptions = new ResultAggregationOptions
    {
        DefaultAggregationStrategy = AggregationStrategy.Consensus,
        ConsensusThreshold = 0.6
    }
};

using var coordinator = new MultiAgentCoordinator(options,
    new SemanticKernelGraphLogger(loggerFactory.CreateLogger<SemanticKernelGraphLogger>(), new GraphOptions()));
```

### 2. Basic Multi-Agent Scenario

#### Creating Specialized Agents

```csharp
// Create specialized agents
var analysisAgent = await CreateAnalysisAgentAsync(coordinator, kernel, loggerFactory);
var processingAgent = await CreateProcessingAgentAsync(coordinator, kernel, loggerFactory);
var reportingAgent = await CreateReportingAgentAsync(coordinator, kernel, loggerFactory);

// Prepare input data
var arguments = new KernelArguments
{
    ["input_text"] = "The quick brown fox jumps over the lazy dog. This is a sample text for analysis.",
    ["analysis_type"] = "comprehensive",
    ["output_format"] = "detailed_report"
};

// Execute simple workflow with automatic distribution
var result = await coordinator.ExecuteSimpleWorkflowAsync(
    kernel,
    arguments,
    new[] { analysisAgent.AgentId, processingAgent.AgentId, reportingAgent.AgentId },
    AggregationStrategy.Merge
);
```

#### Agent Creation Example

```csharp
private static async Task<AgentInstance> CreateAnalysisAgentAsync(MultiAgentCoordinator coordinator,
    Kernel kernel, ILoggerFactory loggerFactory)
{
    // Create graph executor for analysis tasks
    var executor = new GraphExecutor("Analysis Graph", "Specialized in text analysis",
        new SemanticKernelGraphLogger(loggerFactory.CreateLogger<SemanticKernelGraphLogger>(), new GraphOptions()));

    // Add analysis nodes
    var analysisNode = new FunctionGraphNode(CreateAnalysisFunction(kernel), "analyze-text", "Text Analysis");
    // Ensure downstream agents receive the analysis output and relax validation for prompts
    analysisNode.StoreResultAs("input");
    analysisNode.SetMetadata("StrictValidation", false);

    executor.AddNode(analysisNode);
    executor.SetStartNode(analysisNode.NodeId);

    // Register agent with coordinator
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
// Create complex workflow using builder pattern
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

var arguments = new KernelArguments
{
    ["document_content"] = GetSampleDocument(),
    ["analysis_requirements"] = "sentiment, topics, key_phrases, entities",
    ["output_preferences"] = "json_structured"
};

var result = await coordinator.ExecuteWorkflowAsync(workflow, kernel, arguments);
```

### 4. Health Monitoring Scenario

The health monitoring scenario tracks agent status and system performance.

```csharp
// Get all registered agents
var agents = coordinator.GetAllAgents();
logger.LogInformation($"Monitoring {agents.Count} agents...");

// Perform health checks
foreach (var agent in agents)
{
    var healthStatus = agent.GetHealthStatus(coordinator);
    logger.LogInformation($"Agent {agent.AgentId}: {healthStatus?.Status ?? HealthStatus.Unknown}");

    // Perform manual health check
    var healthCheck = await agent.PerformHealthCheckAsync(coordinator);
    logger.LogInformation($"  Health Check: {(healthCheck.Success ? "✅ Passed" : "❌ Failed")} " +
                        $"(Response: {healthCheck.ResponseTime.TotalMilliseconds:F2}ms)");
}

// Display system metrics
var healthMonitor = coordinator.HealthMonitor;
logger.LogInformation($"System Health: {healthMonitor.HealthyAgentCount}/{healthMonitor.MonitoredAgentCount} agents healthy " +
                    $"({healthMonitor.SystemHealthRatio:P})");
```

### 5. Agent Function Creation

The example creates specialized functions for different agent types.

```csharp
private static KernelFunction CreateAnalysisFunction(Kernel kernel)
{
    return KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var input = args.TryGetValue("input_text", out var i) ? i?.ToString() ?? string.Empty : string.Empty;
            var analysisType = args.TryGetValue("analysis_type", out var a) ? a?.ToString() ?? "basic" : "basic";

            // Simulate analysis processing
            var analysisResult = new
            {
                TextLength = input.Length,
                WordCount = input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                AnalysisType = analysisType,
                Insights = new[] { "Sample insight 1", "Sample insight 2" },
                Confidence = 0.95
            };

            args["analysis_result"] = analysisResult;
            return $"Analysis completed: {analysisResult.WordCount} words, {analysisResult.Insights.Length} insights";
        },
        functionName: "analyze_text",
        description: "Performs comprehensive text analysis"
    );
}

private static KernelFunction CreateProcessingFunction(Kernel kernel)
{
    return KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var analysisResult = args.TryGetValue("analysis_result", out var ar) ? ar : null;
            
            // Simulate processing
            var processedResult = new
            {
                ProcessedAt = DateTime.UtcNow,
                EnhancedInsights = new[] { "Enhanced insight 1", "Enhanced insight 2", "Enhanced insight 3" },
                ProcessingQuality = "high",
                Metadata = new { Source = "analysis_agent", Version = "1.0" }
            };

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
private static void LogWorkflowResult(WorkflowExecutionResult result, ILogger logger)
{
    logger.LogInformation("\n📊 Workflow Execution Results:");
    logger.LogInformation($"  ✅ Success: {result.Success}");
    logger.LogInformation($"  🆔 Execution ID: {result.ExecutionId}");
    logger.LogInformation($"  ⏱️ Duration: {result.Duration.TotalMilliseconds:F2}ms");
    logger.LogInformation($"  🤖 Agents Used: {result.AgentsUsed.Count}");
    
    foreach (var agent in result.AgentsUsed)
    {
        logger.LogInformation($"    - {agent.AgentId}: {agent.Status}");
    }

    if (result.AggregatedResult != null)
    {
        logger.LogInformation($"  📋 Aggregated Result: {result.AggregatedResult}");
    }

    if (result.Errors.Any())
    {
        logger.LogInformation($"  ❌ Errors: {result.Errors.Count}");
        foreach (var error in result.Errors.Take(3))
        {
            logger.LogInformation($"    - {error}");
        }
    }
}
```

## Expected Output

The example produces comprehensive output showing:

- 🤖 Multi-agent coordination setup and configuration
- 📋 Basic multi-agent scenario with task distribution
- 🔄 Advanced workflow with explicit task definitions
- 🏥 Health monitoring and agent status tracking
- 📊 Workflow execution results and performance metrics
- ✅ Successful coordination across multiple specialized agents

## Troubleshooting

### Common Issues

1. **Agent Registration Failures**: Ensure agent IDs are unique and capabilities are properly defined
2. **Workflow Execution Errors**: Check that required agents and capabilities are available
3. **Health Check Failures**: Verify agent connectivity and resource availability
4. **Coordination Timeouts**: Adjust timeout settings for complex workflows

### Debugging Tips

- Enable detailed logging to trace agent interactions
- Monitor agent health status and performance metrics
- Verify workflow requirements and agent capabilities match
- Check coordination timeout and concurrency settings

## See Also

- [Multi-Agent and Shared State](../how-to/multi-agent-and-shared-state.md)
- [Workflow Management](../concepts/execution.md)
- [Agent Coordination](../concepts/agent-coordination.md)
- [Health Monitoring](../how-to/health-monitoring.md)
- [Result Aggregation](../concepts/result-aggregation.md)
