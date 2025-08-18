---
title: ADR-0011 Tool Registry & REST Tools
---

# ADR-0011: Tool Registry & REST Tools

Date: 2025-08-14
Status: Accepted

Context

Graph nodes often wrap external tools/APIs. We need a standardized registry and schema→node conversion for REST tools.

Decision

- Provide IToolRegistry for metadata-based tool registration and factory creation of IGraphNode.
- Define RestToolSchema and IToolSchemaConverter with a default RestToolSchemaConverter.
- Ensure thread-safe registration and idempotency.

Consequences

- Pros: Pluggable ecosystem, parity with LangGraph tools, safe metadata-driven wiring.
- Cons: Requires careful input/output validation and error handling at boundaries.

Alternatives Considered

- Ad-hoc node wrappers per API → duplication and inconsistent UX.


