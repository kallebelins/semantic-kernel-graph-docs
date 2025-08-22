# Multi-Agent

This document covers the comprehensive multi-agent coordination system in SemanticKernel.Graph, including agent management, work distribution, result aggregation, and connection pooling. The system enables complex workflows where multiple specialized agents collaborate on shared tasks with intelligent coordination and state management.

## MultiAgentCoordinator

The `MultiAgentCoordinator` coordinates multiple graph executor instances to enable multi-agent execution, providing agent lifecycle management, shared state coordination, and result aggregation.

### Overview

This central orchestrator manages the complete lifecycle of multiple agents, coordinates their execution, and provides intelligent work distribution and result aggregation. It integrates with distributed tracing, health monitoring, and failover mechanisms for resilient multi-agent workflows.

### Key Features

* **Agent Lifecycle Management**: Registration, execution, and disposal of agent instances
* **Shared State Coordination**: Integration with `SharedStateManager` for state synchronization
* **Work Distribution**: Delegates to `WorkDistributor` for intelligent task assignment
* **Result Aggregation**: Uses `ResultAggregator` to combine agent outputs
* **Health Monitoring**: Circuit breaker patterns and failover mechanisms
* **Distributed Tracing**: OpenTelemetry integration for cross-agent observability
* **Concurrent Execution**: Bounded parallelism with configurable limits

### Configuration

```csharp
var options = new MultiAgentOptions
{
    MaxConcurrentAgents = 5,
    CoordinationTimeout = TimeSpan.FromMinutes(10),
    EnableDistributedTracing = true,
    EnableAgentFailover = true,
    MaxFailoverAttempts = 3,
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

var coordinator = new MultiAgentCoordinator(options, logger);
```

### Agent Registration

```csharp
// Register agents with specific roles
var analysisAgent = await coordinator.RegisterAgentAsync(
    agentId: "analysis-agent-1",
    executor: analysisExecutor,
    role: new AgentRole
    {
        Name = "Data Analyst",
        Priority = 7,
        Capabilities = new HashSet<string> { "text_analysis", "data_extraction", "pattern_recognition" },
        Specializations = new HashSet<string> { "nlp", "ml" }
    },
    initialState: new GraphState(new KernelArguments { ["agent_type"] = "analyst" })
);

var processingAgent = await coordinator.RegisterAgentAsync(
    agentId: "processing-agent-1",
    executor: processingExecutor,
    role: new AgentRole
    {
        Name = "Data Processor",
        Priority = 6,
        Capabilities = new HashSet<string> { "data_transformation", "validation", "enrichment" }
    }
);
```

### Workflow Execution

```csharp
// Create a multi-agent workflow
var workflow = new MultiAgentWorkflow
{
    Id = "document-analysis-001",
    Name = "Document Analysis Pipeline",
    Description = "Analyze documents using multiple specialized agents",
    Tasks = new List<WorkflowTask>
    {
        // Tasks do not embed a specific agent assignment; the coordinator/distributor
        // assigns tasks based on the workflow's RequiredAgents and configured distribution strategy.
        new WorkflowTask
        {
            Id = "extract-text",
            Name = "Text Extraction",
            Description = "Extract text from documents",
            Priority = 8,
            DependsOn = new List<string>()
        },
        new WorkflowTask
        {
            Id = "process-content",
            Name = "Content Processing",
            Description = "Process extracted content",
            Priority = 7,
            DependsOn = new List<string> { "extract-text" }
        }
    },
    RequiredAgents = new List<string> { "analysis-agent-1", "processing-agent-1" },
    AggregationStrategy = AggregationStrategy.Merge
};

// Execute the workflow
var arguments = new KernelArguments
{
    ["document_path"] = "/path/to/document.pdf",
    ["analysis_type"] = "comprehensive"
};

// Execute the workflow (note the signature: workflow, kernel, arguments)
var result = await coordinator.ExecuteWorkflowAsync(
    workflow,
    kernel,
    arguments,
    CancellationToken.None
);
```

### Simple Workflow Execution

```csharp
// Execute a simple workflow with automatic distribution
var result = await coordinator.ExecuteSimpleWorkflowAsync(
    kernel: kernel,
    arguments: new KernelArguments { ["input"] = "sample data" },
    agentIds: new[] { "agent-1", "agent-2", "agent-3" },
    aggregationStrategy: AggregationStrategy.Consensus
);

if (result.Success)
{
    Console.WriteLine($"Workflow completed in {result.Duration.TotalMilliseconds}ms");
    Console.WriteLine($"Aggregated result: {result.AggregatedResult?.GetValue<object>()}");
}
```

### Health Monitoring and Failover

```csharp
// Check agent health status
var healthStatus = coordinator.HealthMonitor.GetAgentHealthStatus("agent-1");
if (healthStatus.Status == AgentHealthStatus.Unhealthy)
{
    // Agent is unhealthy, consider failover
    var failoverResult = await coordinator.AttemptAgentFailoverAsync("agent-1");
    if (failoverResult.Success)
    {
        Console.WriteLine($"Agent failover successful: {failoverResult.NewAgentId}");
    }
}

// Get overall system health
var systemHealth = coordinator.HealthMonitor.GetSystemHealth();
Console.WriteLine($"System health: {systemHealth.OverallStatus}");
Console.WriteLine($"Healthy agents: {systemHealth.HealthyAgentCount}/{systemHealth.TotalAgentCount}");
```

## ResultAggregator

The `ResultAggregator` aggregates results from multiple agents into consolidated outputs, providing various aggregation strategies and result transformation capabilities.

### Overview

This component provides intelligent result combination using configurable strategies, caching for performance, and comprehensive result validation. It supports multiple aggregation approaches from simple merging to complex consensus-based aggregation.

### Key Features

* **Multiple Aggregation Strategies**: Merge, Weighted, Consensus, and custom strategies
* **Result Caching**: Configurable caching with expiration and invalidation
* **Result Validation**: Built-in validation and size limits
* **Metadata Preservation**: Maintains individual result metadata and provenance
* **Performance Optimization**: Efficient aggregation algorithms and caching
* **Extensible Architecture**: Custom aggregation strategy registration

### Configuration

```csharp
var options = new ResultAggregationOptions
{
    DefaultAggregationStrategy = AggregationStrategy.Consensus,
    AggregationTimeout = TimeSpan.FromMinutes(5),
    IncludeMetadata = true,
    PreserveIndividualResults = true,
    ConsensusThreshold = 0.6,
    MaxResultSizeBytes = 10 * 1024 * 1024, // 10MB
    EnableResultValidation = true,
    EnableCaching = true,
    MaxCacheEntries = 1000,
    CacheExpiration = TimeSpan.FromHours(1)
};

var aggregator = new ResultAggregator(options, logger);
```

### Aggregation Strategies

```csharp
// Available built-in strategies (matches library implementation)
public enum AggregationStrategy
{
    Merge = 0,    // Combine all successful results
    Concat = 1,   // Concatenate successful results
    First = 2,    // Use first successful result
    Last = 3,     // Use last successful result
    Majority = 4, // Majority / vote-based aggregation
    Weighted = 5, // Weighted combination based on agent priority or confidence
    Consensus = 6 // Require consensus among agents
}

// Aggregate results using specific strategy
var aggregatedResult = await aggregator.AggregateResultsAsync(
    workflowId: "workflow-001",
    agentResults: agentResults,
    strategy: AggregationStrategy.Consensus
);

if (aggregatedResult.Success)
{
    Console.WriteLine($"Aggregation successful using {aggregatedResult.Strategy}");
    Console.WriteLine($"Result: {aggregatedResult.Result?.GetValue<object>()}");
    Console.WriteLine($"Duration: {aggregatedResult.AggregationDuration.TotalMilliseconds}ms");
}
```

### Custom Aggregation Strategies

```csharp
// Register custom aggregation strategy
aggregator.RegisterStrategy("custom-merge", new CustomMergeStrategy());

// Custom strategy implementation
public class CustomMergeStrategy : IAggregationStrategy
{
    public async Task<FunctionResult> AggregateAsync(string workflowId, 
        IReadOnlyList<AgentExecutionResult> results)
    {
        // Custom aggregation logic
        var successfulResults = results.Where(r => r.Success).ToList();
        
        if (successfulResults.Count == 0)
        {
            throw new InvalidOperationException("No successful results to aggregate");
        }

        // Custom merge logic
        var mergedData = new Dictionary<string, object>();
        foreach (var result in successfulResults)
        {
            if (result.Result?.GetValue<object>() is Dictionary<string, object> data)
            {
                foreach (var kvp in data)
                {
                    if (!mergedData.ContainsKey(kvp.Key))
                    {
                        mergedData[kvp.Key] = kvp.Value;
                    }
                }
            }
        }

        return new FunctionResult(
            function: null,
            value: mergedData,
            culture: null,
            metadata: new Dictionary<string, object>
            {
                ["strategy"] = "custom-merge",
                ["source_count"] = successfulResults.Count
            }
        );
    }
}
```

### Caching and Performance

```csharp
// Check cache statistics
var cacheStats = aggregator.GetCacheStatistics();
Console.WriteLine($"Cache size: {cacheStats["CacheSize"]}");
Console.WriteLine($"Active entries: {cacheStats["ActiveEntries"]}");
Console.WriteLine($"Expired entries: {cacheStats["ExpiredEntries"]}");

// Invalidate cache for specific workflow
var invalidatedCount = aggregator.InvalidateCacheForWorkflow("workflow-001");
Console.WriteLine($"Invalidated {invalidatedCount} cache entries");

// Clear entire cache
aggregator.ClearCache();
```

## AgentConnectionPool

The `AgentConnectionPool` manages reusable `IAgentConnection` instances per agent, offering fair selection among healthy connections and basic health-aware rent semantics.

### Overview

This thread-safe pool provides efficient connection management for remote agent communication, with health monitoring, load balancing, and automatic connection lifecycle management. It supports both local and remote agent connections with configurable pooling strategies.

### Key Features

* **Connection Pooling**: Efficient reuse of agent connections
* **Health-Aware Selection**: Automatic filtering of unhealthy connections
* **Load Balancing**: Round-robin and health-based connection selection
* **Thread Safety**: Concurrent access with proper synchronization
* **Metrics Integration**: Built-in performance and health metrics
* **Automatic Cleanup**: Connection disposal and resource management

### Configuration

```csharp
var options = new AgentConnectionPoolOptions
{
    MaxConcurrentRentals = 100,
    EnableMetrics = true,
    MetricsMeterName = "skg.agent_pool",
    ConnectionHealthCheckInterval = TimeSpan.FromSeconds(30),
    MaxConnectionAge = TimeSpan.FromMinutes(10),
    EnableConnectionCompression = true
};

var connectionPool = new AgentConnectionPool(options);
```

### Connection Management

```csharp
// Register connections for an agent
var connection1 = new MockAgentConnection("agent-1", "instance-1");
var connection2 = new MockAgentConnection("agent-1", "instance-2");

// The public API on the pool is `Register` and `RentAsync`
connectionPool.Register("agent-1", connection1);
connectionPool.Register("agent-1", connection2);

// Rent a connection for use
var rented = await connectionPool.RentAsync("agent-1", CancellationToken.None);
if (rented != null)
{
    try
    {
        // Use the connection's Executor to perform the work
        var execResult = await rented.Executor.ExecuteAsync(kernel, workItem.Arguments, CancellationToken.None);
        Console.WriteLine($"Execution result: {execResult.GetValue<object>()}");
    }
    finally
    {
        // Return connection resources by disposing the connection when done
        await rented.DisposeAsync();
    }
}
```

### Connection Health and Metrics

```csharp
// Inspect registered connection counts (public pool API exposes registration counts)
var counts = connectionPool.GetRegisteredConnectionCounts();
Console.WriteLine($"Registered connections for agent-1: {counts.GetValueOrDefault("agent-1", 0)}");

// Rent a connection and optionally probe its health via the connection's IsHealthyAsync API
var rented = await connectionPool.RentAsync("agent-1", CancellationToken.None);
if (rented != null)
{
    try
    {
        // Probe health (the pool itself will also use health probes internally)
        var isHealthy = await rented.IsHealthyAsync(CancellationToken.None);
        Console.WriteLine($"Rented connection {rented.Endpoint ?? rented.Executor.Name} healthy: {isHealthy}");
    }
    finally
    {
        await rented.DisposeAsync();
    }
}
```

### Connection Interface

```csharp
// The pool's public contract for reusable remote connections
public interface IAgentConnection : IAsyncDisposable
{
    string AgentId { get; }
    IGraphExecutor Executor { get; }
    string? Endpoint { get; }
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}

// Minimal example connection used for documentation only
public class MockAgentConnection : IAgentConnection
{
    public string AgentId { get; }
    public string ConnectionId { get; }
    public IGraphExecutor Executor { get; }
    public string? Endpoint { get; }

    public MockAgentConnection(string agentId, string connectionId, IGraphExecutor executor, string? endpoint = null)
    {
        AgentId = agentId;
        ConnectionId = connectionId;
        Executor = executor;
        Endpoint = endpoint;
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
```

## WorkDistributor

The `WorkDistributor` distributes work items among multiple agents based on configured strategies, providing load balancing, capacity management, and work item prioritization.

### Overview

This component implements intelligent work distribution algorithms that consider agent capabilities, current load, and configured strategies. It supports multiple distribution approaches and provides comprehensive metrics for monitoring and optimization.

### Key Features

* **Multiple Distribution Strategies**: RoundRobin, LoadBased, RoleBased, CapacityBased
* **Load Balancing**: Automatic distribution based on agent capacity and current load
* **Priority Support**: Work item prioritization and scheduling
* **Capacity Management**: Dynamic agent capacity tracking and updates
* **Metrics Integration**: Comprehensive distribution metrics and monitoring
* **Strategy Fallbacks**: Automatic fallback to alternative distribution methods

### Configuration

```csharp
var options = new WorkDistributionOptions
{
    DistributionStrategy = WorkDistributionStrategy.LoadBased,
    EnablePrioritization = true,
    MaxParallelWorkItems = 10,
    DefaultAgentMaxCapacity = 5,
    LoadBalancingThreshold = 0.8,
    EnableMetrics = true,
    MetricsMeterName = "skg.work_distributor"
};

var workDistributor = new WorkDistributor(options, logger);
```

### Distribution Strategies

```csharp
// Available distribution strategies
public enum WorkDistributionStrategy
{
    RoundRobin = 0,      // Distribute work in round-robin fashion
    LoadBased = 1,       // Distribute based on current agent load
    RoleBased = 2,       // Distribute based on agent roles and capabilities
    CapacityBased = 3,   // Distribute based on agent capacity
    Custom = 4           // Custom distribution logic
}

// Distribute work for a workflow
var workItems = await workDistributor.DistributeWorkAsync(workflow, arguments);
Console.WriteLine($"Distributed {workItems.Count} work items");

foreach (var workItem in workItems)
{
    Console.WriteLine($"Work item {workItem.Id} assigned to agent {workItem.AgentId}");
    Console.WriteLine($"Priority: {workItem.Priority}, Status: {workItem.Status}");
}
```

### Agent Capacity Management

```csharp
// Update agent capacity information
var capacity = new AgentCapacity
{
    AgentId = "agent-1",
    MaxCapacity = 10,
    CurrentLoad = 3,
    Capabilities = { "text_processing", "data_analysis" },
    Metadata = { ["region"] = "us-east", ["version"] = "1.0" }
};

workDistributor.UpdateAgentCapacity("agent-1", capacity);

// Get capacity information
var agentCapacity = workDistributor.GetAgentCapacity("agent-1");
if (agentCapacity != null)
{
    Console.WriteLine($"Agent {agentCapacity.AgentId}: {agentCapacity.CurrentLoad}/{agentCapacity.MaxCapacity}");
    Console.WriteLine($"Available capacity: {agentCapacity.AvailableCapacity}");
}
```

### Work Item Structure

```csharp
public class WorkItem
{
    public required string Id { get; set; }
    public required string WorkflowId { get; set; }
    public required string AgentId { get; set; }
    public required WorkflowTask Task { get; set; }
    public required KernelArguments Arguments { get; set; }
    public int Priority { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public WorkItemStatus Status { get; set; } = WorkItemStatus.Pending;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

// Work item status enumeration
public enum WorkItemStatus
{
    Pending = 0,      // Work item is pending execution
    InProgress = 1,   // Work item is currently being executed
    Completed = 2,    // Work item completed successfully
    Failed = 3,       // Work item failed during execution
    Cancelled = 4     // Work item was cancelled
}
```

### Metrics and Monitoring

```csharp
// Get distribution metrics
var metrics = workDistributor.GetDistributionMetrics();
Console.WriteLine($"Distribution requests: {metrics.DistributionRequests}");
Console.WriteLine($"Distribution items: {metrics.DistributionItems}");
Console.WriteLine($"Strategy fallbacks: {metrics.StrategyFallbacks}");
Console.WriteLine($"Distribution errors: {metrics.DistributionErrors}");

// Get agent assignment statistics
var assignmentStats = workDistributor.GetAgentAssignmentStatistics();
foreach (var stat in assignmentStats)
{
    Console.WriteLine($"Agent {stat.AgentId}: {stat.AssignedWorkItems} work items");
    Console.WriteLine($"  Average load: {stat.AverageLoad:F2}");
    Console.WriteLine($"  Peak load: {stat.PeakLoad}");
}
```

## Multi-Agent Types and Enums

### Agent Types

```csharp
// Agent instance representing a running agent
public sealed class AgentInstance : IDisposable
{
    public string AgentId { get; }
    public AgentRole Role { get; }
    public GraphState State { get; set; }
    public AgentStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset LastActivity { get; set; }
}

// Agent role definition
public sealed class AgentRole
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Priority { get; set; } = 0;
    public HashSet<string> Capabilities { get; set; } = new();
    public HashSet<string> Specializations { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

// Agent status enumeration
public enum AgentStatus
{
    Idle = 0,         // Agent is idle and ready for work
    Running = 1,      // Agent is currently executing work
    Stopping = 2,     // Agent is in the process of stopping
    Stopped = 3,      // Agent has been stopped
    Failed = 4,       // Agent has failed
    Disposed = 5      // Agent has been disposed
}
```

### Workflow Types

```csharp
// Multi-agent workflow definition
public sealed class MultiAgentWorkflow
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required List<WorkflowTask> Tasks { get; set; }
    public required List<string> RequiredAgents { get; set; }
    public AggregationStrategy AggregationStrategy { get; set; } = AggregationStrategy.Merge;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

// Workflow task definition
public sealed class WorkflowTask
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string AgentId { get; set; }
    public int Priority { get; set; } = 5;
    public List<string> DependsOn { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
}
```

### Result Types

```csharp
// Multi-agent workflow result
public sealed class MultiAgentResult
{
    public required string WorkflowId { get; set; }
    public bool Success { get; set; }
    public List<AgentExecutionResult> Results { get; set; } = new();
    public FunctionResult? AggregatedResult { get; set; }
    public TimeSpan Duration { get; set; }
    public List<string> AgentsInvolved { get; set; } = new();
    public Exception? Error { get; set; }
}

// Individual agent execution result
public sealed class AgentExecutionResult
{
    public required string AgentId { get; set; }
    public required string WorkflowId { get; set; }
    public required string TaskId { get; set; }
    public bool Success { get; set; }
    public FunctionResult? Result { get; set; }
    public TimeSpan Duration { get; set; }
    public Exception? Error { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

// Aggregated result
public sealed class AggregatedResult
{
    public required string WorkflowId { get; set; }
    public AggregationStrategy Strategy { get; set; }
    public bool Success { get; set; }
    public FunctionResult? Result { get; set; }
    public List<AgentExecutionResult> SourceResults { get; set; } = new();
    public TimeSpan AggregationDuration { get; set; }
    public Exception? Error { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
```

## See Also

* [Multi-Agent and Shared State Guide](../how-to/multi-agent-and-shared-state.md) - Comprehensive guide to implementing multi-agent workflows
* [State and Serialization Reference](state-and-serialization.md) - State management for agent coordination
* [Error Policies Reference](error-policies.md) - Error handling and resilience in multi-agent systems
* [Graph Executor Reference](graph-executor.md) - Core execution engine for individual agents
* [Integration Reference](integration.md) - External system integration for multi-agent workflows
* [Multi-Agent Examples](../examples/multi-agent-examples.md) - Practical examples of multi-agent implementations
