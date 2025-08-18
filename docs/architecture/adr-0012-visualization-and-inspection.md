---
title: ADR-0012 Visualization & Inspection
---

# ADR-0012: Visualization & Inspection

Date: 2025-08-14
Status: Accepted

Context

Developers need to understand and debug graph structure and execution behavior quickly.

Decision

- Provide GraphVisualizationEngine with DOT/Mermaid/JSON exporters and heatmap overlays.
- Expose GraphInspectionApi for runtime inspection and metrics queries.
- Add GraphRealtimeHighlighter for live path highlighting during execution.

Consequences

- Pros: Improves debuggability and documentation; parity with LangGraph Studio.
- Cons: Additional code paths to maintain; can be optional at runtime.

Alternatives Considered

- Manual diagramming → slow and easily outdated.


