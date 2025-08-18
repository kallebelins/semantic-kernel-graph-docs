---
title: ADR-0013 Workflow Templates
---

# ADR-0013: Workflow Templates

Date: 2025-08-14
Status: Accepted

Context

Common graph patterns (chatbot, document analysis, ReAct) should be shareable and versioned.

Decision

- Provide IWorkflowTemplate and TemplateParameter models.
- Template registry to materialize configured GraphExecutor instances.
- Validate parameters and declare capabilities for compatibility checks.

Consequences

- Pros: Reuse, consistency, faster onboarding.
- Cons: Requires governance for template versions and deprecations.

Alternatives Considered

- Hand-coded graphs per project → duplication and drift.


