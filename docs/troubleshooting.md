# [screen working] Troubleshooting Guide

## Common Issues & Diagnostics
1. **Echo Loops**: Verify `ScreenWorkingSyncScope.SuppressLocalCapture()` wraps remote application logic.
2. **Missing GUIDs**: Ensure `ScreenWorkingIdentity` components are attached and registered via `ScreenWorkingIdentityManager`.
3. **Reconnection Issues**: Inspect WebSocket connection state in `ScreenWorkingWindow` or `/ws` endpoint logs.
