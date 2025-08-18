---
title: ADR-0003 Dynamic Routing
---

# ADR-0003: Dynamic Routing

Date: 2025-08-14
Status: Accepted
Context: Some graphs must choose the next node based on semantic context or results, beyond static edges.

Decision

- Introduce DynamicRoutingEngine and pluggable strategies (semantic, probabilistic, historical).
  - Default is simple first-candidate selection for deterministic flows.

Consequences

- Pros: Adaptivity, better outcomes on ambiguous paths.
- Cons: Non-determinism if misused; mitigate with metrics and tracing.

Alternatives Considered

- Only static ConditionalEdge → reduced flexibility.


