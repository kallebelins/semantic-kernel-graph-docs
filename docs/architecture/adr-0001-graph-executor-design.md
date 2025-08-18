---
title: ADR-0001 GraphExecutor Design
---

# ADR-0001: GraphExecutor Design

Date: 2025-08-14
Status: Accepted
Context: We need a single orchestrator with strong integration to Semantic Kernel and optional advanced features (routing, recovery, metrics, resource governance) without over-engineering.

Decision

- Implement a sealed GraphExecutor that:
  - Owns node/edge registry with thread-safe access.
  - Uses optional DynamicRoutingEngine and ErrorRecoveryEngine.
  - Supports parallel branches with merge policy and resource governance.
  - Emits mutation events and exposes diagnostics helpers.

Consequences

- Pros: Simple entry point, clear ownership, testability via IGraphExecutor.
- Cons: Feature growth could increase class size; mitigated by optional services and regions.

Alternatives Considered

- Multiple smaller executors per concern → more composition but harder orchestration.
- Pure functional pipeline → reduced mutability but complex state threading.


