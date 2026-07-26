# [screen working] Conflict Resolution & Deterministic Convergence

## Overview
ScreenWorking guarantees that all clients connected to a collaboration room converge to the exact same scene state hash regardless of network latency, out-of-order delivery, or temporary disconnects.

## Metric & Total Ordering
Any two operations $Op_1$ and $Op_2$ are ordered deterministically by comparing:
1. `LamportTimestamp` (higher wins)
2. `ActorId` (lexicographical comparison)
3. `ActorSequence` (higher wins)

## Deletion Wins & Tombstones
- Deleting an object writes a tombstone entry into the CRDT engine.
- Property modifications targeted at tombstoned objects with older/equal timestamps are discarded.
- Restoring a deleted object removes the tombstone and restores state.

## Cycle-Free Reparenting
Before applying reparenting operations, the engine checks candidate ancestry. If an operation would make an object its own ancestor (a cyclic loop), the operation is rejected and rolled back.
