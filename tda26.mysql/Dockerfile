FROM mysql:8.0.45-debian

ENV MYSQL_ROOT_PASSWORD=root \
    MYSQL_DATABASE=tda26 \
    MYSQL_USER=tda26 \
    MYSQL_PASSWORD=tda26

COPY ./data.sql /docker-entrypoint-initdb.d/01_data.sql