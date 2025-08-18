---
title: ADR-0008 Resource Governance
---

# ADR-0008: Resource Governance

Date: 2025-08-14
Status: Accepted

Context

Graph executions can contend for CPU/memory and external API quotas. We need first-class controls to rate
limit, prioritize, and throttle work to maintain SLOs and avoid overload.

Decision

- Introduce ResourceGovernor with:
  - Token/leaky-bucket permits for concurrency and weighted node cost.
  - Priority-aware scheduling (Critical/High/Normal/Low) and starvation protection.
  - Periodic system load sampling via GraphPerformanceMetrics for adaptive throttling.
  - Options class (GraphResourceOptions) and integration via GraphExecutor.ConfigureResources.

Consequences

- Pros: Predictable resource usage, stability under load, aligns with production SLOs.
- Cons: Adds complexity and overhead; opt-in keeps default lightweight.

Alternatives Considered

- OS-level throttling only → too coarse, no domain priority awareness.


