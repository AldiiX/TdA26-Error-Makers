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
# redis
# -------------------------
log "starting redis..."
redis-server --protected-mode no >/var/log/redis.log 2>&1 &
REDIS_PID="$!"
if ! wait_for_tcp "127.0.0.1" "6379" 30; then
  echo "[error] redis did not start" >&2
  exit 1
fi





# -------------------------
# mysql (mariadb)
# -------------------------
MYSQL_DATABASE="${MYSQL_DATABASE:-tda26}"
MYSQL_USER="${MYSQL_USER:-tda26}"
MYSQL_PASSWORD="${MYSQL_PASSWORD:-tda26}"

# charset/collation kompatibilni s mariadb
MYSQL_CHARSET="${MYSQL_CHARSET:-utf8mb4}"
MYSQL_COLLATION="${MYSQL_COLLATION:-utf8mb4_unicode_ci}"

# sjednoceny socket (na debianu je bezne /run; /var/run je casto jen symlink)
MYSQL_SOCKET="/run/mysqld/mysqld.sock"
MYSQL_PID_FILE="/run/mysqld/mysqld.pid"

log "preparing mysql dirs..."
mkdir -p /run/mysqld
chown -R mysql:mysql /run/mysqld /var/lib/mysql

# init datadir jen pri prvnim startu
if [ ! -d "/var/lib/mysql/mysql" ]; then
  log "initializing mysql datadir..."
  mariadb-install-db --user=mysql --datadir=/var/lib/mysql >/var/log/mysql-init.log 2>&1
fi

log "starting mysql..."
mariadbd \
  --user=mysql \
  --datadir=/var/lib/mysql \
  --bind-address=127.0.0.1 \
  --port=3306 \
  --socket="${MYSQL_SOCKET}" \
  --pid-file="${MYSQL_PID_FILE}" \
  >/var/log/mysql.log 2>&1 &
MYSQL_PID="$!"

# pockej, nez server fakt jede (socket + ping)
log "waiting for mysql socket..."
for i in $(seq 1 60); do
  if [ -S "${MYSQL_SOCKET}" ] && mariadb-admin --protocol=socket --socket="${MYSQL_SOCKET}" -uroot ping >/dev/null 2>&1; then
    break
  fi

  # kdyby mysql hned spadla, ukonci to s logem
  if ! kill -0 "${MYSQL_PID}" >/dev/null 2>&1; then
    echo "[error] mysql process exited early, last mysql.log:" >&2
    tail -n 120 /var/log/mysql.log >&2 || true
    exit 1
  fi

  sleep 1
done

if ! [ -S "${MYSQL_SOCKET}" ]; then
  echo "[error] mysql socket not created: ${MYSQL_SOCKET}" >&2
  tail -n 120 /var/log/mysql.log >&2 || true
  exit 1
fi

log "ensuring database + user..."
mysql -uroot --protocol=socket --socket="${MYSQL_SOCKET}" <<SQL
CREATE DATABASE IF NOT EXISTS \`${MYSQL_DATABASE}\` CHARACTER SET ${MYSQL_CHARSET} COLLATE ${MYSQL_COLLATION};
CREATE USER IF NOT EXISTS '${MYSQL_USER}'@'%' IDENTIFIED BY '${MYSQL_PASSWORD}';
GRANT ALL PRIVILEGES ON \`${MYSQL_DATABASE}\`.* TO '${MYSQL_USER}'@'%';
FLUSH PRIVILEGES;
SQL

# seed jen jednou (marker soubor)
SEED_MARKER="/var/lib/mysql/.seeded_${MYSQL_DATABASE}"
if [ -f "/app/seed/tda26.sql" ] && [ ! -f "$SEED_MARKER" ]; then
  log "importing seed sql into ${MYSQL_DATABASE}..."

  # v dumpu je mysql8 collation utf8mb4_0900_ai_ci; mariadb ji nezna -> prepis na kompatibilni
  TMP_SEED="/tmp/tda26_seed_fixed.sql"
  sed "s/utf8mb4_0900_ai_ci/${MYSQL_COLLATION}/g" /app/seed/tda26.sql > "$TMP_SEED"

  mysql -uroot --protocol=socket --socket="${MYSQL_SOCKET}" "${MYSQL_DATABASE}" < "$TMP_SEED"

  touch "$SEED_MARKER"
  log "seed done"
else
  log "seed skipped (already done or missing /app/seed/tda26.sql)"
fi




# -------------------------
# minio
# -------------------------
MINIO_ROOT_USER="${MINIO_ROOT_USER:-admin}"
MINIO_ROOT_PASSWORD="${MINIO_ROOT_PASSWORD:-adminadmin}"
MINIO_BUCKET="${MINIO_BUCKET:-tda26}"

export MINIO_ROOT_USER
export MINIO_ROOT_PASSWORD

log "starting minio..."
mkdir -p /data/minio
minio server /data/minio --address ":9000" --console-address ":9001" >/var/log/minio.log 2>&1 &
MINIO_PID="$!"

if ! wait_for_tcp "127.0.0.1" "9000" 60; then
  echo "[error] minio did not start" >&2
  exit 1
fi

log "creating minio bucket ${MINIO_BUCKET}..."
mc alias set local http://127.0.0.1:9000 "${MINIO_ROOT_USER}" "${MINIO_ROOT_PASSWORD}" >/dev/null 2>&1 || true
mc mb --ignore-existing "local/${MINIO_BUCKET}" >/dev/null 2>&1 || true





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