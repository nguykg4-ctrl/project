# [screen working] Security Model

## Authentication & Authorization
- All WebSocket connections require a valid JWT token signed by HMAC SHA-256 or RSA.
- Roles include `Owner`, `Administrator`, `Editor`, `Reviewer`, and `Viewer`.

## Payload Safety
- Arbitrary C# object deserialization is strictly prohibited.
- Payloads use explicit `SerializedValue` discriminator schemas.
- Payload sizes are enforced with a configurable 10 MB frame cap.
