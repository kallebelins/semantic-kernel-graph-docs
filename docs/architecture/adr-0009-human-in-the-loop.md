---
title: ADR-0009 Human-in-the-Loop
---

# ADR-0009: Human-in-the-Loop

Date: 2025-08-14
Status: Accepted

Context

Certain decisions require human approval or intervention. We need an engine-native interruption mechanism
with pluggable channels (CLI, web, API) and timeouts.

Decision

- Provide ConsoleHumanInteractionChannel and IHumanInteractionChannel abstraction.
- Add HumanApprovalGraphNode and ConfidenceGateGraphNode for conditional HITL.
- Support timeouts, batching (HumanApprovalBatchManager), and cancellation paths.

Consequences

- Pros: Safe handling of sensitive decisions, parity with LangGraph HITL features.
- Cons: Requires UX integration for real deployments; console adapter provided as reference.

Alternatives Considered

- Ad-hoc prompts inside nodes → mixes concerns and complicates testing.


