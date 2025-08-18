---
title: ADR-0004 Streaming Execution
---

# ADR-0004: Streaming Execution

Date: 2025-08-14
Status: Accepted
Context: Real-time feedback and intermediate results are needed for interactive experiences and monitoring.

Decision

- Provide IGraphExecutionEventStream and GraphExecutionEvent types.
- Emit start/completion/failure and per-node events.
- Keep transport-agnostic; adapters can push to web/CLI.

Consequences

- Pros: Better UX, observability, and debugging.
- Cons: Slight overhead; can be disabled.

Alternatives Considered

- Only final result → poor visibility.


