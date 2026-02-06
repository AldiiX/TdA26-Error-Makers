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

wait_for_deps() {
  local host="127.0.0.1"

  log "waiting for mysql on ${host}:3306..."
  if ! wait_for_tcp "$host" "3306" "120"; then
    log "mysql not reachable on ${host}:3306"
    exit 1
  fi

  log "waiting for redis on ${host}:6379..."
  if ! wait_for_tcp "$host" "6379" "120"; then
    log "redis not reachable on ${host}:6379"
    exit 1
  fi

  log "waiting for minio on ${host}:9000..."
  if ! wait_for_tcp "$host" "9000" "120"; then
    log "minio not reachable on ${host}:9000"
    exit 1
  fi

  # optional: zkus i http ready endpoint, kdyz mas v image curl/wget
  if command -v curl >/dev/null 2>&1; then
    log "waiting for minio http readiness..."
    for i in $(seq 1 120); do
      if curl -fsS "http://${host}:9000/minio/health/ready" >/dev/null 2>&1; then
        break
      fi
      sleep 1
    done
  elif command -v wget >/dev/null 2>&1; then
    log "waiting for minio http readiness..."
    for i in $(seq 1 120); do
      if wget -qO- "http://${host}:9000/minio/health/ready" >/dev/null 2>&1; then
        break
      fi
      sleep 1
    done
  fi

  log "all dependencies are reachable"
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

shutdown() {
  log "shutting down..."

  # kill app processes first
  kill "${API_PID:-0}" >/dev/null 2>&1 || true
  kill "${FRONT_PID:-0}" >/dev/null 2>&1 || true

  # nginx je foreground proces, ukonci se spolu s kontejnerem
  exit 0
}

trap shutdown SIGINT SIGTERM

# -------------------------
# wait for mysql/minio/redis
# -------------------------
#wait_for_deps

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