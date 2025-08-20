# Core API

Key types:

* `GraphExecutor`
* `GraphExecutionContext`
* `IGraphNode`
* `ConditionalEdge`
* `GraphState`
* `ErrorPolicyRegistry`
* `RetryPolicyGraphNode`
* `ErrorHandlerGraphNode`
* `ErrorMetricsCollector`
* `MultiAgentCoordinator`
* `ResultAggregator`
* `AgentConnectionPool`
* `WorkDistributor`
* `WorkflowValidator`
* `GraphTypeInferenceEngine`
* `StateValidator`
* `StateMergeConflictPolicy`

## Performance and Metrics

* `GraphPerformanceMetrics` - Comprehensive performance metrics collector
* `NodeExecutionMetrics` - Node-level execution statistics
* `GraphMetricsOptions` - Metrics configuration options
* `GraphMetricsExporter` - Metrics export and visualization

Refer to [Metrics APIs Reference](./metrics.md) for detailed metrics documentation.

## See Also

* [Graph Concepts](../concepts/graph-concepts.md) - Core graph concepts and terminology
* [Execution Model](../concepts/execution-model.md) - How graph execution works
* [Node Types](../concepts/node-types.md) - Available node types and their capabilities
* [Build a Graph](../how-to/build-a-graph.md) - Step-by-step guide to creating graphs
* [Error Handling and Resilience](../how-to/error-handling-and-resilience.md) - Error policies and recovery
* [Multi-Agent and Shared State](../how-to/multi-agent-and-shared-state.md) - Multi-agent coordination
* [Integration and Extensions](../how-to/integration-and-extensions.md) - Extending the framework
* [Metrics and Observability](../how-to/metrics-and-observability.md) - Performance monitoring

Refer to XML docs in source for full signatures.
