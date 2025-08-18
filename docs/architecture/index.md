---
title: Architecture Overview
---

# Architecture Overview

This page introduces the high-level architecture of SemanticKernel.Graph and how it integrates with Semantic Kernel.

## Core Components

- GraphExecutor: Orchestrates node execution, routing, error handling, metrics, and resource governance.
- IGraphNode: Contract for nodes with lifecycle hooks, navigation, and validation.
- GraphState: Execution state wrapper around KernelArguments with helpers and serialization.
- ConditionalEdge: Encodes navigation with predicate conditions.
- DynamicRoutingEngine: Optional smart routing for next-node selection.
- ErrorRecoveryEngine: Policy-based recovery (retry, rollback, continue) with snapshots.

## Execution Flow

1. Validate graph integrity (nodes, edges, reachability, typed schema hints).
2. Initialize GraphExecutionContext and mark start.
3. Execute node → lifecycle hooks → record metrics → select next.
4. Parallel branches (optional) → merge state with conflict policy.
5. On failures → categorize → recover (if enabled) → continue/retry.

## Observability

- Logging: Structured logs via IGraphLogger.
- Telemetry: Request/Dependency/Metrics via IGraphTelemetry.
- Metrics: Per-node timings, paths, error stats, resources.
- Visualization: DOT/Mermaid/JSON for structure and heatmaps.

## Extensibility

- Nodes: Implement IGraphNode or typed schema interface.
- Tools: Register via IToolRegistry and RestToolSchema.
- Templates: IWorkflowTemplate for reusable blueprints.
- Import/Export: IGraphImporter / IGraphExporter.


