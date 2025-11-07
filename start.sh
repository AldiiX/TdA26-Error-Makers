#!/usr/bin/env bash
set -euo pipefail

# decode backend dotenv from env var if provided (runtime only)
# pokud .env uz existuje, tak ho nahravanim z DOTENV_B64 neprepisuj
if [ -f "/app/.env" ]; then
  echo "[init] /app/.env already exists, skipping generation from DOTENV_B64"
elif [ -n "${DOTENV_B64:-}" ]; then
  echo "[init] writing /app/.env from DOTENV_B64"
  umask 077
  printf '%s' "$DOTENV_B64" | base64 -d > /app/.env
  chmod 600 /app/.env
else
  echo "[init] no DOTENV_B64 provided and /app/.env not found; continuing without it"
fi

# start the backend in the background
dotnet tda26.Server.dll &

# start the frontend (vite preview) in the background
cd /app/client/.output
node server/index.mjs &

# start nginx (pid 1)
nginx -g 'daemon off;'