---
title: ADR-0015 Python Bridge
---

# ADR-0015: Python Bridge

Date: 2025-08-14
Status: Accepted

Context

Some advanced tooling and models are Python-only. We need a safe, bounded way to invoke Python from .NET nodes.

Decision

- Provide PythonGraphNode for controlled execution of Python code/scripts.
- Define timeouts, resource limits, and serialization of inputs/outputs.
- Keep language boundary explicit; no in-process embedding by default.

Consequences

- Pros: Interoperability with Python ecosystem; parity with LangGraph bridges.
- Cons: IPC and serialization overhead; sandboxing needed in production.

Alternatives Considered

- Rewrite Python tools in .NET → not always feasible or cost-effective.


