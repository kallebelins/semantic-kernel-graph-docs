# Checkpointing

Persist state during long runs to enable recovery and replay.

- Save every N nodes (configurable)
- Compress and prune old checkpoints
- Resume from last valid checkpoint after failures

See `CheckpointManager`, `CheckpointingGraphExecutor`.
