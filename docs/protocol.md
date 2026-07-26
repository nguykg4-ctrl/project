# [screen working] Network Protocol Specification

## Overview
The ScreenWorking wire protocol runs over RFC 6455 WebSockets (`ws://` or `wss://`), utilizing MessagePack binary payloads for production operations and JSON for diagnostic logging.

## Operation Frame Schema (`CollaborationOperation`)
| Field Name | Type | Description |
| :--- | :--- | :--- |
| `ProtocolVersion` | `ushort` | Protocol version identifier (default `1`). |
| `ProjectId` | `string` | Unique project identifier. |
| `RoomId` | `string` | Active room identifier. |
| `OperationId` | `string` | Unique operation GUID. |
| `ActorId` | `string` | Unique user/actor identifier. |
| `ActorSequence` | `long` | Monotonically increasing sequence number per client. |
| `ServerSequence` | `long` | Monotonically increasing sequence number assigned by server. |
| `LamportTimestamp` | `long` | Lamport clock value for deterministic ordering. |
| `OpType` | `enum` | Type of operation (`CreateGameObject`, `ModifyProperty`, etc.). |
| `TargetObjectId` | `string` | Target object GUID. |
| `Payload` | `SerializedValue` | Polymorphic serialized value payload. |

## Polymorphic Value Container (`SerializedValue`)
Values are encoded with explicit type discriminators (`Integer`, `Float`, `Vector3`, `Quaternion`, `Color`, `AssetRef`, `SceneObjectRef`) preventing arbitrary code execution.
