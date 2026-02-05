#!/bin/sh
set -e

# spust minio na pozadi
minio "$@" &
minioPid="$!"

# pockej dokud nebude minio odpovidat
until mc alias set local http://127.0.0.1:9000 "$MINIO_ROOT_USER" "$MINIO_ROOT_PASSWORD" >/dev/null 2>&1; do
  sleep 1
done

# vytvor bucket, kdyz neexistuje
mc mb --ignore-existing local/tda26 >/dev/null 2>&1 || true

# nech minio bezet v popredi
wait "$minioPid"