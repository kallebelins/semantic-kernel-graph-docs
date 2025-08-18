---
title: ADR-0010 Security & Auth
---

# ADR-0010: Security & Auth

Date: 2025-08-14
Status: Accepted

Context

Production APIs and remote execution require authentication and authorization without hard-coding vendor SDKs.

Decision

- Abstract bearer token validation via IBearerTokenValidator with an Azure AD implementation.
- Use AzureKeyVaultSecretResolver for secret resolution without embedding credentials.
- Keep GraphRestApi free of hard dependencies; allow host to inject security adapters.

Consequences

- Pros: Enterprise-ready integration, minimal coupling, testable.
- Cons: Requires host configuration and secret management policies.

Alternatives Considered

- Embed JWT validation directly → harder to customize and test, increases attack surface.


