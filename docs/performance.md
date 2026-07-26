# [screen working] Performance Targets & Optimization

## Benchmarks & Limits
- **Frame Overhead**: Under 2ms Editor frame overhead during active editing.
- **Scene Size**: Tested up to 10,000 active synchronized GameObjects.
- **Concurrent Editors**: Supports 10+ active simultaneous editors per room.
- **Batching**: Inbound and outbound operation queue processing on Unity main thread.
