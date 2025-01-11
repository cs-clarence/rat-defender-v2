FROM imbios/bun-node:latest-current-alpine AS build-web

WORKDIR /usr/local/src/app
COPY ../../ ./
RUN bun install
WORKDIR /usr/local/src/app/apps/ts/web
RUN bun run build


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-api
WORKDIR /usr/local/src/app

# Copy everything
COPY ../../ ./
WORKDIR /usr/local/src/app/services/cs/WebApi/ApiGateway
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /usr/local/bin/app
COPY --from=build-api /usr/local/src/app/services/cs/WebApi/ApiGateway/out .
COPY --from=build-web /usr/local/src/app/apps/ts/web/.output/public ./spa
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
