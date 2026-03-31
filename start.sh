#!/bin/bash
set -euo pipefail

log() {
  echo "[init] $*"
}

wait_for_tcp() {
  local host="$1"
  local port="$2"

  if (echo >"/dev/tcp/${host}/${port}") >/dev/null 2>&1; then
    return 0
  fi

  return 1
}

wait_for_deps() {
  local host="127.0.0.1"
  local timeout="${1:-10}"
  local elapsed=0

  log "waiting up to ${timeout}s for mysql:${host}:3306, redis:${host}:6379, minio:${host}:9000..."

  while [ "$elapsed" -lt "$timeout" ]; do
    local mysql_ready=1
    local redis_ready=1
    local minio_ready=1

    if wait_for_tcp "$host" "3306"; then
      mysql_ready=0
    fi

    if wait_for_tcp "$host" "6379"; then
      redis_ready=0
    fi

    if wait_for_tcp "$host" "9000"; then
      minio_ready=0
    fi

    if [ "$mysql_ready" -eq 0 ] && [ "$redis_ready" -eq 0 ] && [ "$minio_ready" -eq 0 ]; then
      log "all dependencies are reachable"
      return 0
    fi

    sleep 1
    elapsed=$((elapsed + 1))
  done

  log "dependencies not fully reachable within ${timeout}s, starting backend anyway"
  return 0
}

# decode backend dotenv from env var if provided (runtime only)
# pokud .env uz existuje, tak ho nahravanim z DOTENV_B64 neprepisuj
if ! command -v log >/dev/null 2>&1; then
  log() { printf '%s\n' "$*"; }
fi

read_b64() {
  if [ -n "${DOTNET_B64:-}" ]; then printf '%s' "$DOTNET_B64"; return 0; fi
  if [ -n "${DOTENV_B64:-}" ]; then printf '%s' "$DOTENV_B64"; return 0; fi
  if [ -n "${BACKEND_ENV_B64:-}" ]; then printf '%s' "$BACKEND_ENV_B64"; return 0; fi
  if [ -f "/run/secrets/BACKEND_ENV_B64" ]; then cat /run/secrets/BACKEND_ENV_B64; return 0; fi
  return 1
}

if [ -f "/app/.env" ]; then
  log "/app/.env already exists, skipping generation"
elif b64="$(read_b64)"; then
  log "writing /app/.env from base64"
  umask 077
  printf '%s' "$b64" | base64 -d > /app/.env
  chmod 600 /app/.env
else
  log "no base64 provided and /app/.env not found; continuing without it"
fi

shutdown() {
  log "shutting down..."

  kill "${API_PID:-0}" >/dev/null 2>&1 || true
  kill "${FRONT_PID:-0}" >/dev/null 2>&1 || true

  exit 0
}

trap shutdown SIGINT SIGTERM

# -------------------------
# wait for mysql/minio/redis
# -------------------------
wait_for_deps 10

# -------------------------
# app + frontend + nginx
# -------------------------
log "starting backend..."
dotnet tda26.Server.dll &
API_PID="$!"

log "starting frontend..."
cd /app/client/.output
node server/index.mjs &
FRONT_PID="$!"

log "starting nginx..."
nginx -g 'daemon off;'