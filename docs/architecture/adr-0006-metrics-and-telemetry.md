---
title: ADR-0006 Metrics & Telemetry
---

# ADR-0006: Metrics & Telemetry

Date: 2025-08-14
Status: Accepted

Context

Reliable operation at scale requires insight into execution timing, resource use, and failures. We need a
lightweight, provider-agnostic approach that integrates with host telemetry (e.g., Application Insights)
without introducing hard dependencies.

Decision

- Split observability into metrics and telemetry:
  - Metrics: In-process collectors for per-node timing, execution paths, error rates, and resource samples.
    - Key types: GraphPerformanceMetrics, NodeExecutionMetrics, ExecutionPathMetrics, GraphPerformanceSummary.
    - Config via GraphMetricsOptions; opt-in using ConfigureMetrics on GraphExecutor.
  - Telemetry: Thin abstraction IGraphTelemetry for Request/Dependency/Trace/Metric emission, mapped by host.
    - Default no-op implementation (NullGraphTelemetry) to avoid coupling.

- Sampling and cardinality:
  - Enforce low-cardinality property names/values; avoid high-cardinality dimensions on hot paths.
  - Support host-level sampling; in-process collectors remain fast and non-allocating where possible.

Consequences

- Pros: Works with any APM, minimal overhead, clear separation of concerns.
- Cons: Two layers to reason about; requires coordination to avoid double-counting.

Alternatives Considered

- Direct dependency on a specific APM SDK → simpler integration but reduces portability and increases bloat.


