---
title: ADR-0002 Error Recovery Engine
---

# ADR-0002: Error Recovery Engine

Date: 2025-08-14
Status: Accepted
Context: Robust execution requires first-class recovery (retry, rollback, continue) driven by policies and contextualized errors.

Decision

- Provide ErrorRecoveryEngine with:
  - Error categorization and severity mapping.
  - Policy registry and application pipeline.
  - Optional snapshots via CheckpointManager for rollback.

Consequences

- Pros: Predictable failure handling, safer long-running graphs.
- Cons: Slight runtime overhead; mitigated by opting in per-graph.

Alternatives Considered

- Ad-hoc try/catch in nodes → duplicates logic and reduces consistency.


