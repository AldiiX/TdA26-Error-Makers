#!/usr/bin/env bash
set -euo pipefail

log() {
  echo "[init] $*"
}

wait_for_tcp() {
  local host="$1"
  local port="$2"
  local tries="${3:-60}"

  for i in $(seq 1 "$tries"); do
    if (echo >"/dev/tcp/${host}/${port}") >/dev/null 2>&1; then
      return 0
    fi
    sleep 1
  done

  return 1
}

# decode backend dotenv from env var if provided (runtime only)
# pokud .env uz existuje, tak ho nahravanim z DOTENV_B64 neprepisuj
if [ -f "/app/.env" ]; then
  log "/app/.env already exists, skipping generation from DOTENV_B64"
elif [ -n "${DOTENV_B64:-}" ]; then
  log "writing /app/.env from DOTENV_B64"
  umask 077
  printf '%s' "$DOTENV_B64" | base64 -d > /app/.env
  chmod 600 /app/.env
else
  log "no DOTENV_B64 provided and /app/.env not found; continuing without it"
fi



# -------------------------
# app + frontend + nginx
# -------------------------
shutdown() {
  log "shutting down..."

  # kill app processes first
  kill "${API_PID:-0}" >/dev/null 2>&1 || true
  kill "${FRONT_PID:-0}" >/dev/null 2>&1 || true

  # services
  kill "${MINIO_PID:-0}" >/dev/null 2>&1 || true
  kill "${MYSQL_PID:-0}" >/dev/null 2>&1 || true
  kill "${REDIS_PID:-0}" >/dev/null 2>&1 || true

  # nginx bude ukoncen jako foreground child
  exit 0
}

trap shutdown SIGINT SIGTERM

log "starting backend..."
dotnet tda26.Server.dll &
API_PID="$!"

log "starting frontend..."
cd /app/client/.output
node server/index.mjs &
FRONT_PID="$!"

log "starting nginx..."
nginx -g 'daemon off;'