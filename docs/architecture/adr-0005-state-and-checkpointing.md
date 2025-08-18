---
title: ADR-0005 State & Checkpointing
---

# ADR-0005: State & Checkpointing

Date: 2025-08-14
Status: Accepted
Context: Long-running and fault-tolerant graphs require durable state and resumability.

Decision

- Base state on KernelArguments with typed helpers.
- Provide JSON serialization with versioning and migrations.
- Implement configurable checkpoint intervals and cleanup.

Consequences

- Pros: Easy persistence, compatibility over time.
- Cons: Serialization overhead; mitigated by compression and selective snapshotting.

Alternatives Considered

- Custom state container → more code without added value over SK.


