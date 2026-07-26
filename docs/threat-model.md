# [screen working] Threat Model

| Threat Scenario | Risk Level | Mitigation Strategy |
| :--- | :--- | :--- |
| **Malicious Room Member** | High | Server-side role authorization validation; room snapshot history backups. |
| **Replay Attacks** | Medium | Server sequence validation; unique `OperationId` idempotency cache. |
| **Oversized Snapshots** | Medium | Frame size limits; snapshot chunking and compression. |
| **Unsafe Type Deserialization** | High | Discriminated union payload model; no raw CLR type reflection. |
