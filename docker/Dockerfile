# syntax=docker/dockerfile:1

# ---- Stage 1: build the React SPA ----
FROM node:24-alpine AS web
WORKDIR /web
COPY src/Web/package.json src/Web/package-lock.json ./
RUN npm ci
COPY src/Web/ ./
RUN npm run build

# ---- Stage 2: restore, build and publish the .NET API ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api
WORKDIR /src
COPY src/.editorconfig src/Directory.Build.props src/Directory.Packages.props src/nuget.config ./
COPY src/Foundation/ ./Foundation/
RUN dotnet publish Foundation/Foundation.Api/Foundation.Api.csproj -c Release -o /app/publish

# ---- Stage 3: runtime image ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=api /app/publish ./
COPY --from=web /web/dist ./wwwroot

ENV PORT=8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD ["dotnet", "LocalStackWeb.dll", "--health-check"]

ENTRYPOINT ["dotnet", "LocalStackWeb.dll"]
