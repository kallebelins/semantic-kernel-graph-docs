---
title: ADR-0007 Multi-Agent Coordination
---

# ADR-0007: Multi-Agent Coordination

Date: 2025-08-14
Status: Accepted

Context

Complex tasks benefit from multiple cooperating executors/agents sharing state and balancing work. We need
coordination primitives without tightly coupling to any distributed runtime.

Decision

- Provide MultiAgentCoordinator with:
  - Work distribution (queue of tasks, fair scheduling, backpressure).
  - SharedStateManager for cross-agent communication and conflict policies.
  - Health monitoring (AgentHealthMonitor) and recovery hooks.
  - Options to enable distributed execution later (WorkDistributor interfaces).

- Agents are regular GraphExecutors with an agent identity; composition over inheritance.

Consequences

- Pros: Scales from single-process to distributed; minimal API surface.
- Cons: Requires careful state merge policies; eventual consistency by design.

Alternatives Considered

- Single global executor with partitions → simpler but limits autonomy and isolation per agent.


