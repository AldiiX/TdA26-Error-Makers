ARG APP_UID=1000

# base stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# stage s node
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS with-node
RUN apt-get update && apt-get install -y --no-install-recommends curl ca-certificates \
  && curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
  && apt-get install -y --no-install-recommends nodejs \
  && rm -rf /var/lib/apt/lists/*

# build backendu
FROM with-node AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["tda26.Server/tda26.Server.csproj", "tda26.Server/"]
COPY ["tda26.client/tda26.client.esproj", "tda26.client/"]
RUN dotnet restore "./tda26.Server/tda26.Server.csproj"
COPY . .
WORKDIR "/src/tda26.Server"
RUN dotnet build "./tda26.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# publish backendu
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./tda26.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root

# instalace nginx + nodejs
RUN apt-get update && apt-get install -y --no-install-recommends \
      curl ca-certificates nginx \
  && curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
  && apt-get install -y --no-install-recommends nodejs \
  && rm -rf /var/lib/apt/lists/*

# frontend build
COPY ["tda26.client/", "/app/client/"]
RUN chown -R $APP_UID:$APP_UID /app/client
WORKDIR /app/client
RUN npm config set cache /app/.npm
RUN npm install --unsafe-perm
RUN npm run build

# nginx konfigurace
COPY nginx.conf /etc/nginx/nginx.conf

# porty: app
EXPOSE 80

WORKDIR /app
COPY --chmod=0755 start.sh .
CMD ["./start.sh"]