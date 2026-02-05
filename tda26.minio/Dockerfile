FROM quay.io/minio/minio:latest

ENV MINIO_ROOT_USER=admin \
    MINIO_ROOT_PASSWORD=adminadmin

# mc klient pridame z oficialniho mc image
COPY --from=quay.io/minio/mc:latest /usr/bin/mc /usr/bin/mc

COPY docker-entrypoint.sh /usr/local/bin/docker-entrypoint.sh
RUN chmod +x /usr/local/bin/docker-entrypoint.sh

VOLUME ["/data"]

EXPOSE 9000 9001

ENTRYPOINT ["/usr/local/bin/docker-entrypoint.sh"]

# entrypoint v puvodnim image byl "minio", tady ho spoustime ze skriptu
CMD ["server", "/data", "--console-address", ":9001"]