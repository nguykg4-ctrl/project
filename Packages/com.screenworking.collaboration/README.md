# [screen working] Real-Time Scene Collaboration

`com.screenworking.collaboration` is a clean-room, enterprise-grade real-time multi-developer collaboration package for the Unity Editor (Unity 2022.3 LTS and Unity 6).

## Features

- **Real-Time Scene Synchronization**: Synchronizes GameObjects, Transforms, RectTransforms, components, and SerializedProperty modifications concurrently across editors.
- **Deterministic CRDT Convergence**: Built with Lamport Timestamps, Actor IDs, and Tombstone deletion handling to guarantee convergent scene states across all clients.
- **Echo Suppression**: Implements thread-local suppression scopes preventing local change capture loops.
- **Presence & Lock Management**: Live selection highlighting, camera view frustums, user colors, and soft/hard locking.
- **Backend Server**: ASP.NET Core 8 WebSockets server backed by PostgreSQL/SQLite with JWT authentication and RBAC.

## Installation

Add the package to your `Packages/manifest.json` or install via Unity Package Manager using git or local file path:

```json
"com.screenworking.collaboration": "file:../Packages/com.screenworking.collaboration"
```
