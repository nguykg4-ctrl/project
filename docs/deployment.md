# [screen working] Deployment Guide

## Docker Compose Deployment
To launch the server with PostgreSQL:
```bash
cd server
docker-compose up -d --build
```

The server will start on port `5000` (`http://localhost:5000`) with WebSockets at `ws://localhost:5000/ws`.

## Production Checklist
1. Enable TLS 1.3 reverse proxy (Nginx or Caddy) for `wss://`.
2. Configure production PostgreSQL connection strings.
3. Update `Jwt:Secret` with a high-entropy secret key.
