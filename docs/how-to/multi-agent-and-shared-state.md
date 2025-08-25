# Multi-Agent and Shared State

Multi-agent coordination and shared state management in SemanticKernel.Graph enable complex workflows where multiple specialized agents collaborate on shared tasks. This guide covers agent coordination, shared state management, conflict resolution strategies, and versioning mechanisms.

## What You'll Learn

* How to create and coordinate multiple specialized agents
* Managing shared state between agents with conflict resolution
* Implementing different work distribution strategies
* Handling agent failures and health monitoring
* Best practices for multi-agent workflow design

## Concepts and Techniques

**MultiAgentCoordinator**: Central orchestrator that manages agent lifecycle, work distribution, and result aggregation across multiple graph executor instances.

**SharedStateManager**: Thread-safe state synchronization manager that handles conflicts, versioning, and state persistence between collaborating agents.

**Conflict Resolution Strategies**: Configurable policies for resolving state conflicts including LastWriterWins, FirstWriterWins, Merge, and AgentPriority approaches.

**Work Distribution**: Intelligent task assignment strategies including RoundRobin, LoadBased, RoleBased, and CapacityBased distribution.

**Agent Health Monitoring**: Circuit breaker patterns and failover mechanisms for resilient multi-agent execution.

## Prerequisites

* [First Graph Tutorial](../first-graph-5-minutes.md) completed
* Understanding of [Graph Concepts](../concepts/graph-concepts.md)
* Familiarity with [State Management](state-tutorial.md)
* Basic knowledge of concurrent programming concepts

## Multi-Agent Architecture

### Core Components

The multi-agent system consists of several key components:

```csharp
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Integration;

// Multi-agent coordinator orchestrates all agents
var coordinator = new MultiAgentCoordinator(options);

// Shared state manager handles state synchronization
var sharedStateManager = new SharedStateManager(sharedStateOptions);

// Work distributor assigns tasks to agents
var workDistributor = new WorkDistributor(distributionOptions);

// Result aggregator combines agent outputs
var resultAggregator = new ResultAggregator(aggregationOptions);
```

### Agent Types and Roles

Define specialized agents with specific capabilities:

```csharp
public class AgentRole
{
    public string Name { get; set; } = string.Empty;
    public int Priority { get; set; } = 5;
    public List<string> Capabilities { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
}

// Example agent roles
var analysisAgentRole = new AgentRole
{
    Name = "Data Analyst",
    Priority = 7,
    Capabilities = { "text_analysis", "data_extraction", "pattern_recognition" },
    Configuration = { ["max_text_length"] = 10000 }
};

var processingAgentRole = new AgentRole
{
    Name = "Data Processor",
    Priority = 6,
    Capabilities = { "data_transformation", "validation", "enrichment" },
    Configuration = { ["batch_size"] = 1000 }
};

var reportingAgentRole = new AgentRole
{
    Name = "Report Generator",
    Priority = 5,
    Capabilities = { "report_generation", "formatting", "export" },
    Configuration = { ["output_formats"] = new[] { "json", "csv", "pdf" } }
};
```

## Creating Multi-Agent Workflows

### Basic Multi-Agent Setup

Create a simple multi-agent workflow:

```csharp
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;

// Configure multi-agent options
var multiAgentOptions = new MultiAgentOptions
{
    MaxConcurrentAgents = 5,
    CoordinationTimeout = TimeSpan.FromMinutes(10),
    EnableDistributedTracing = true,
    EnableAgentFailover = true,
    MaxFailoverAttempts = 3
};

// Configure shared state options
var sharedStateOptions = new SharedStateOptions
{
    ConflictResolutionStrategy = ConflictResolutionStrategy.Merge,
    AllowOverwrite = true,
    MinimumPriorityForOverride = 5,
    EnableAutomaticCleanup = true,
    CleanupInterval = TimeSpan.FromMinutes(30),
    StateRetentionPeriod = TimeSpan.FromHours(6)
};

// Configure work distribution options
var workDistributionOptions = new WorkDistributionOptions
{
    DistributionStrategy = WorkDistributionStrategy.RoleBased,
    EnablePrioritization = true,
    MaxParallelWorkItems = 3,
    LoadBalancingThreshold = 0.8
};

// Configure result aggregation options
var resultAggregationOptions = new ResultAggregationOptions
{
    DefaultAggregationStrategy = AggregationStrategy.Consensus,
    ConsensusThreshold = 0.6,
    EnableConflictResolution = true,
    MaxAggregationTime = TimeSpan.FromMinutes(5)
};

// Create coordinator with all options
var coordinator = new MultiAgentCoordinator(
    new MultiAgentOptions
    {
        SharedStateOptions = sharedStateOptions,
        WorkDistributionOptions = workDistributionOptions,
        ResultAggregationOptions = resultAggregationOptions
    },
    logger
);
```

### Agent Registration

Register agents with the coordinator:

```csharp
// Create and register analysis agent
var analysisAgent = new AgentInstance(
    "analysis-agent-001",
    "Data Analysis Specialist",
    analysisAgentRole,
    analysisGraph,
    new GraphState()
);

await coordinator.RegisterAgentAsync(analysisAgent);

// Create and register processing agent
var processingAgent = new AgentInstance(
    "processing-agent-001",
    "Data Processing Specialist",
    processingAgentRole,
    processingGraph,
    new GraphState()
);

await coordinator.RegisterAgentAsync(processingAgent);

// Create and register reporting agent
var reportingAgent = new AgentInstance(
    "reporting-agent-001",
    "Report Generation Specialist",
    reportingAgentRole,
    reportingGraph,
    new GraphState()
);

await coordinator.RegisterAgentAsync(reportingAgent);
```

### Simple Workflow Execution

Execute a basic workflow with automatic task distribution:

```csharp
// Prepare input arguments
var arguments = new KernelArguments
{
    ["input_text"] = "The quick brown fox jumps over the lazy dog. This is a sample text for analysis.",
    ["analysis_type"] = "comprehensive",
    ["output_format"] = "detailed_report"
};

// Execute workflow with specified agents
var result = await coordinator.ExecuteSimpleWorkflowAsync(
    kernel,
    arguments,
    new[] { "analysis-agent-001", "processing-agent-001", "reporting-agent-001" },
    AggregationStrategy.Merge
);

// Check result
if (result.Success)
{
    Console.WriteLine($"Workflow completed in {result.Duration.TotalMilliseconds}ms");
    Console.WriteLine($"Final result: {result.AggregatedResult}");
    Console.WriteLine($"Agents involved: {string.Join(", ", result.AgentsInvolved)}");
}
else
{
    Console.WriteLine($"Workflow failed: {result.Error?.Message}");
}
```

## Advanced Workflow Definition

### Complex Workflow with Dependencies

Create workflows with explicit task dependencies:

```csharp
// Define workflow tasks with dependencies
var workflow = new MultiAgentWorkflow
{
    Id = "document-analysis-workflow",
    Name = "Document Analysis and Reporting",
    Description = "Multi-stage document processing workflow",
    RequiredAgents = { "analysis-agent-001", "processing-agent-001", "reporting-agent-001" },
    Tasks = new List<WorkflowTask>
    {
        new WorkflowTask
        {
            Id = "text-extraction",
            Name = "Extract Text Content",
            Description = "Extract and clean text from input documents",
            AgentId = "analysis-agent-001",
            RequiredCapabilities = { "text_analysis", "data_extraction" },
            DependsOn = new List<string>(), // No dependencies
            Priority = 1
        },
        new WorkflowTask
        {
            Id = "content-analysis",
            Name = "Analyze Content",
            Description = "Perform semantic analysis and extract insights",
            AgentId = "analysis-agent-001",
            RequiredCapabilities = { "text_analysis", "pattern_recognition" },
            DependsOn = { "text-extraction" },
            Priority = 2
        },
        new WorkflowTask
        {
            Id = "data-enrichment",
            Name = "Enrich Data",
            Description = "Add metadata and enhance extracted information",
            AgentId = "processing-agent-001",
            RequiredCapabilities = { "data_transformation", "enrichment" },
            DependsOn = { "content-analysis" },
            Priority = 3
        },
        new WorkflowTask
        {
            Id = "report-generation",
            Name = "Generate Report",
            Description = "Create final report in requested format",
            AgentId = "reporting-agent-001",
            RequiredCapabilities = { "report_generation", "formatting" },
            DependsOn = { "data-enrichment" },
            Priority = 4
        }
    },
    AggregationStrategy = AggregationStrategy.Sequential
};

// Execute complex workflow
var result = await coordinator.ExecuteWorkflowAsync(workflow, kernel, arguments);
```

### Custom Workflow Builder

Use the fluent workflow builder for complex scenarios:

```csharp
var workflow = MultiAgentWorkflowBuilder
    .Create("advanced-workflow")
    .WithDescription("Advanced multi-agent workflow with custom logic")
    .RequiringAgents("analysis-agent-001", "processing-agent-001", "reporting-agent-001")
    .WithTask("extract", "Text Extraction")
        .AssignedTo("analysis-agent-001")
        .WithCapabilities("text_analysis", "data_extraction")
        .WithPriority(1)
        .Build()
    .WithTask("analyze", "Content Analysis")
        .AssignedTo("analysis-agent-001")
        .WithCapabilities("text_analysis", "pattern_recognition")
        .DependsOn("extract")
        .WithPriority(2)
        .Build()
    .WithTask("enrich", "Data Enrichment")
        .AssignedTo("processing-agent-001")
        .WithCapabilities("data_transformation", "enrichment")
        .DependsOn("analyze")
        .WithPriority(3)
        .Build()
    .WithTask("report", "Report Generation")
        .AssignedTo("reporting-agent-001")
        .WithCapabilities("report_generation", "formatting")
        .DependsOn("enrich")
        .WithPriority(4)
        .Build()
    .WithAggregationStrategy(AggregationStrategy.Sequential)
    .Build();
```

## Shared State Management

### State Initialization and Access

Initialize shared state for workflows:

```csharp
// Initialize shared state for a workflow
await coordinator.SharedStateManager.InitializeWorkflowStateAsync(
    "workflow-001",
    new GraphState
    {
        ["workflow_id"] = "workflow-001",
        ["start_time"] = DateTimeOffset.UtcNow,
        ["status"] = "running"
    }
);

// Get current shared state
var sharedState = await coordinator.SharedStateManager.GetSharedStateAsync("workflow-001");

// Access shared state values
var workflowId = sharedState.GetValue<string>("workflow_id");
var startTime = sharedState.GetValue<DateTimeOffset>("start_time");
var status = sharedState.GetValue<string>("status");
```

### State Updates and Conflict Resolution

Update shared state with automatic conflict resolution:

```csharp
// Update shared state from an agent
var updatedState = new GraphState();
updatedState.Set("analysis_results", analysisResults);
updatedState.Set("processing_status", "completed");
updatedState.Set("last_updated", DateTimeOffset.UtcNow);

// Update shared state (conflicts resolved automatically)
await coordinator.SharedStateManager.UpdateSharedStateAsync(
    "workflow-001",
    "analysis-agent-001",
    updatedState
);

// Get updated state
var currentState = await coordinator.SharedStateManager.GetSharedStateAsync("workflow-001");
```

### Conflict Resolution Strategies

Configure different conflict resolution approaches:

```csharp
// Last Writer Wins (default for high-priority updates)
var lastWriterWinsOptions = new SharedStateOptions
{
    ConflictResolutionStrategy = ConflictResolutionStrategy.LastWriterWins,
    AllowOverwrite = true
};

// First Writer Wins (preserves initial values)
var firstWriterWinsOptions = new SharedStateOptions
{
    ConflictResolutionStrategy = ConflictResolutionStrategy.FirstWriterWins,
    AllowOverwrite = false
};

// Merge Strategy (combines changes intelligently)
var mergeOptions = new SharedStateOptions
{
    ConflictResolutionStrategy = ConflictResolutionStrategy.Merge,
    AllowOverwrite = true
};

// Agent Priority (higher priority agents can override)
var priorityOptions = new SharedStateOptions
{
    ConflictResolutionStrategy = ConflictResolutionStrategy.AgentPriority,
    MinimumPriorityForOverride = 7,
    AllowOverwrite = true
};
```

## Work Distribution Strategies

### Round Robin Distribution

Distribute work evenly across agents:

```csharp
var roundRobinOptions = new WorkDistributionOptions
{
    DistributionStrategy = WorkDistributionStrategy.RoundRobin,
    EnablePrioritization = false,
    MaxParallelWorkItems = 5
};

// Work is distributed sequentially: Agent1, Agent2, Agent3, Agent1, Agent2...
```

### Load-Based Distribution

Distribute work based on agent load:

```csharp
var loadBasedOptions = new WorkDistributionOptions
{
    DistributionStrategy = WorkDistributionStrategy.LoadBased,
    LoadBalancingThreshold = 0.8,
    MaxParallelWorkItems = 3
};

// Work is assigned to agents with lowest current load
```

### Role-Based Distribution

Assign work based on agent capabilities:

```csharp
var roleBasedOptions = new WorkDistributionOptions
{
    DistributionStrategy = WorkDistributionStrategy.RoleBased,
    EnablePrioritization = true,
    MaxParallelWorkItems = 4
};

// Work is assigned to agents with matching capabilities
```

### Capacity-Based Distribution

Consider agent capacity and health:

```csharp
var capacityBasedOptions = new WorkDistributionOptions
{
    DistributionStrategy = WorkDistributionStrategy.CapacityBased,
    EnablePrioritization = true,
    MaxParallelWorkItems = 2
};

// Work is assigned considering agent capacity, health, and current workload
```

## Result Aggregation

### Aggregation Strategies

Configure how agent results are combined:

```csharp
// Merge all results (default)
var mergeOptions = new ResultAggregationOptions
{
    DefaultAggregationStrategy = AggregationStrategy.Merge,
    EnableConflictResolution = true
};

// Consensus-based aggregation
var consensusOptions = new ResultAggregationOptions
{
    DefaultAggregationStrategy = AggregationStrategy.Consensus,
    ConsensusThreshold = 0.7,
    EnableConflictResolution = true
};

// Weighted aggregation
var weightedOptions = new ResultAggregationOptions
{
    DefaultAggregationStrategy = AggregationStrategy.Weighted,
    EnableConflictResolution = true
};

// Sequential aggregation
var sequentialOptions = new ResultAggregationOptions
{
    DefaultAggregationStrategy = AggregationStrategy.Sequential,
    EnableConflictResolution = false
};
```

### Custom Aggregation Logic

Implement custom aggregation strategies:

```csharp
public class CustomAggregator : IResultAggregator
{
    public async Task<AggregationResult> AggregateResultsAsync(
        string workflowId,
        IReadOnlyList<AgentExecutionResult> results,
        AggregationStrategy strategy)
    {
        // Custom aggregation logic
        var aggregatedData = new Dictionary<string, object>();
        
        foreach (var result in results)
        {
            if (result.Success && result.Result != null)
            {
                // Merge results based on custom logic
                foreach (var kvp in result.Result)
                {
                    if (!aggregatedData.ContainsKey(kvp.Key))
                    {
                        aggregatedData[kvp.Key] = kvp.Value;
                    }
                    else
                    {
                        // Custom merge logic
                        aggregatedData[kvp.Key] = MergeValues(aggregatedData[kvp.Key], kvp.Value);
                    }
                }
            }
        }
        
        return new AggregationResult
        {
            Success = true,
            Result = new GraphState(aggregatedData),
            Strategy = strategy,
            AgentCount = results.Count
        };
    }
    
    private object MergeValues(object existing, object newValue)
    {
        // Implement custom merge logic
        if (existing is List<object> existingList && newValue is List<object> newList)
        {
            return existingList.Concat(newList).ToList();
        }
        
        // Default to new value
        return newValue;
    }
}
```

## Health Monitoring and Failover

### Agent Health Monitoring

Monitor agent health and performance:

```csharp
// Configure health monitoring
var healthOptions = new AgentHealthMonitorOptions
{
    EnableAgentCircuitBreaker = true,
    CircuitBreakerThreshold = 5,
    CircuitBreakerTimeout = TimeSpan.FromMinutes(5),
    HealthCheckInterval = TimeSpan.FromSeconds(30),
    MaxConsecutiveFailures = 3
};

// Get agent health status
var healthStatus = coordinator.HealthMonitor.GetAgentHealth("analysis-agent-001");
if (healthStatus?.IsHealthy == true)
{
    Console.WriteLine($"Agent {healthStatus.AgentId} is healthy");
    Console.WriteLine($"Last successful execution: {healthStatus.LastSuccessfulExecution}");
    Console.WriteLine($"Success rate: {healthStatus.SuccessRate:P2}");
}
else
{
    Console.WriteLine($"Agent {healthStatus?.AgentId} is unhealthy");
    Console.WriteLine($"Circuit breaker status: {healthStatus?.CircuitBreakerStatus}");
    Console.WriteLine($"Last error: {healthStatus?.LastError?.Message}");
}
```

### Automatic Failover

Configure automatic failover when agents fail:

```csharp
var failoverOptions = new MultiAgentOptions
{
    EnableAgentFailover = true,
    MaxFailoverAttempts = 3,
    CoordinationTimeout = TimeSpan.FromMinutes(10)
};

// When an agent fails, the coordinator automatically tries alternative agents
// with matching capabilities
```

## Advanced Patterns

### Parallel Execution with Dependencies

Execute tasks in parallel when dependencies allow:

```csharp
var workflow = MultiAgentWorkflowBuilder
    .Create("parallel-workflow")
    .RequiringAgents("agent-1", "agent-2", "agent-3", "agent-4")
    .WithTask("task-a", "Task A")
        .AssignedTo("agent-1")
        .Build()
    .WithTask("task-b", "Task B")
        .AssignedTo("agent-2")
        .Build()
    .WithTask("task-c", "Task C")
        .AssignedTo("agent-3")
        .DependsOn("task-a", "task-b") // Parallel execution of A and B, then C
        .Build()
    .WithTask("task-d", "Task D")
        .AssignedTo("agent-4")
        .DependsOn("task-c")
        .Build()
    .WithAggregationStrategy(AggregationStrategy.Sequential)
    .Build();
```

### State Versioning and History

Track state changes over time:

```csharp
// Get state version history
var stateHistory = await coordinator.SharedStateManager.GetStateHistoryAsync("workflow-001");

foreach (var version in stateHistory)
{
    Console.WriteLine($"Version {version.Version}: {version.Timestamp}");
    Console.WriteLine($"Updated by: {version.UpdatedBy}");
    Console.WriteLine($"Changes: {string.Join(", ", version.Changes)}");
}

// Restore to specific version
var restoredState = await coordinator.SharedStateManager.RestoreStateVersionAsync(
    "workflow-001",
    targetVersion
);
```

### Distributed Tracing

Enable distributed tracing across agents:

```csharp
var tracingOptions = new MultiAgentOptions
{
    EnableDistributedTracing = true,
    DistributedTracingSourceName = "MyMultiAgentSystem"
};

// Activities are automatically created and correlated across agents
// Use Activity.Current to access current execution context
if (Activity.Current != null)
{
    Activity.Current.SetTag("workflow.id", workflowId);
    Activity.Current.SetTag("agent.id", agentId);
    Activity.Current.SetTag("task.id", taskId);
}
```

## Best Practices

### Workflow Design

* **Task Granularity**: Design tasks that are neither too small nor too large
* **Dependency Management**: Minimize unnecessary dependencies to maximize parallelism
* **Agent Specialization**: Create agents with focused capabilities rather than general-purpose ones
* **Resource Planning**: Consider agent capacity and workload when designing workflows

### State Management

* **Conflict Resolution**: Choose appropriate conflict resolution strategies for your use case
* **State Size**: Keep shared state minimal to reduce synchronization overhead
* **Versioning**: Use state versioning for audit trails and debugging
* **Cleanup**: Configure automatic cleanup to prevent memory leaks

### Performance Optimization

* **Parallelism**: Maximize parallel execution where dependencies allow
* **Load Balancing**: Use appropriate distribution strategies for your workload
* **Health Monitoring**: Monitor agent health to prevent bottlenecks
* **Failover**: Configure failover to handle agent failures gracefully

### Monitoring and Debugging

* **Distributed Tracing**: Enable tracing for end-to-end visibility
* **Metrics Collection**: Monitor execution times and success rates
* **Logging**: Use structured logging for better observability
* **Health Checks**: Implement comprehensive health monitoring

## Troubleshooting

### Common Issues

**Agent Registration Failures**: Ensure all required agents are registered before executing workflows.

**State Conflicts**: Review conflict resolution strategies and consider using more specific strategies for critical data.

**Performance Issues**: Check agent health, workload distribution, and parallel execution opportunities.

**Failover Failures**: Verify that alternative agents have the required capabilities and are healthy.

### Debugging Multi-Agent Workflows

```csharp
// Enable detailed logging
var debugOptions = new MultiAgentOptions
{
    EnableDistributedTracing = true,
    SharedStateOptions = new SharedStateOptions
    {
        EnableAutomaticCleanup = false, // Keep state for debugging
        StateRetentionPeriod = TimeSpan.FromDays(1)
    }
};

// Monitor workflow execution
var workflow = await coordinator.ExecuteWorkflowAsync(workflow, kernel, arguments);

// Check individual agent results
foreach (var result in workflow.Results)
{
    Console.WriteLine($"Agent {result.AgentId}: {(result.Success ? "Success" : "Failed")}");
    if (!result.Success)
    {
        Console.WriteLine($"  Error: {result.Error?.Message}");
    }
    Console.WriteLine($"  Duration: {result.Duration.TotalMilliseconds}ms");
}
```

## See Also

* [State Management](state-tutorial.md) - Managing graph state and persistence
* [Resource Governance](resource-governance-and-concurrency.md) - Managing resource allocation and execution policies
* [Metrics and Observability](metrics-and-observability.md) - Monitoring and performance analysis
* [Examples](../../examples/) - Practical examples of multi-agent coordination

Addendum: A runnable example is available at `semantic-kernel-graph-docs/examples/MultiAgentExample.cs`. To execute it from the examples project use the Program example runner and the `multi-agent` key as follows:

```bash
dotnet run --project semantic-kernel-graph-docs/examples/Examples.csproj -- "multi-agent"
```

## Concepts and Techniques

**MultiAgentCoordinator**: Central orchestrator that manages agent lifecycle, work distribution, and result aggregation across multiple graph executor instances. Provides coordination, failover, and health monitoring capabilities.

**SharedStateManager**: Thread-safe state synchronization manager that handles conflicts, versioning, and state persistence between collaborating agents. Supports multiple conflict resolution strategies and automatic cleanup.

**Conflict Resolution Strategies**: Configurable policies for resolving state conflicts including LastWriterWins, FirstWriterWins, Merge, and AgentPriority approaches. Each strategy handles concurrent updates differently based on business requirements.

**Work Distribution**: Intelligent task assignment strategies including RoundRobin, LoadBased, RoleBased, and CapacityBased distribution. Considers agent capabilities, current load, and health status for optimal assignment.

**Agent Health Monitoring**: Circuit breaker patterns and failover mechanisms for resilient multi-agent execution. Monitors agent performance and automatically routes work to healthy agents.
