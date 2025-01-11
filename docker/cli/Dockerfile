FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-cli
WORKDIR /usr/local/src/app

# Copy everything
COPY ../../ ./
WORKDIR /usr/local/src/app/cli/cs/WebApiCli
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /usr/local/bin/app
COPY --from=build-cli /usr/local/src/app/cli/cs/WebApiCli/out .
ENTRYPOINT ["dotnet", "WebApiCli.dll"]
