---
title: Troubleshooting
---

## Troubleshooting

### Execution hangs or is slow
- Enable metrics and check per‑node timings
- Look for unbounded loops; set MaxIterations

### Missing service or null provider
- Ensure `KernelBuilderExtensions.AddGraphSupport()` was called
- Verify DI registrations for telemetry/memory if used

### REST tool fails
- Validate `RestToolSchema` mappings and timeouts
- Inspect dependency telemetry for status codes

### Checkpoint not restored
- Ensure `CheckpointingExtensions` are configured and collection exists
- Verify state version compatibility

### Python node errors
- Confirm `python` is on PATH; pass `env` overrides if needed
- Set conservative timeouts and capture stderr


