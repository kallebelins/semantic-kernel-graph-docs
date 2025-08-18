---
title: ADR-0014 Distributed Execution
---

# ADR-0014: Distributed Execution

Date: 2025-08-14
Status: Accepted

Context

Large graphs and workloads benefit from scaling across processes/nodes while keeping a simple programming model.

Decision

- Define WorkDistributor and federated node abstractions to enable remote execution.
- Keep GraphExecutor single-process, with composition for distribution (subgraphs, remote nodes).
- Provide federation hooks and state synchronization policies.

Consequences

- Pros: Scales horizontally without complicating single-process dev flows.
- Cons: Requires eventual consistency and robust checkpointing; added operational complexity.

Alternatives Considered

- Make GraphExecutor distributed by default → increases complexity for basic scenarios.


