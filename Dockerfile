# Bygger hele siden (React-frontend + .NET-backend) til ét image.
# Backend serverer frontenden, så alt kører på samme domæne.

# ---------- 1) Byg frontend ----------
FROM node:22-alpine AS frontend
WORKDIR /src/frontend
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build

# ---------- 2) Byg backend ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend
WORKDIR /src/Backend
COPY Backend/Backend.csproj ./
RUN dotnet restore Backend.csproj
COPY Backend/ ./
RUN dotnet publish Backend.csproj -c Release -o /app/publish --no-restore

# ---------- 3) Geo-database til besøgsstatistikken (DB-IP IP to City Lite, CC BY 4.0) ----------
# Måneden styres af Backend/GeoData/dbip-month.txt (se LAES-MIG.md). Findes den ikke længere hos
# DB-IP, prøves nuværende og forrige måned. Fejler alt, bygges siden uden geo (land/by bliver tomme).
FROM alpine:3.20 AS geoip
RUN apk add --no-cache curl
WORKDIR /geo
COPY Backend/GeoData/dbip-month.txt ./
RUN set -u; mkdir -p out; \
    want=$(tr -d ' \r\n' < dbip-month.txt); now=$(date -u +%Y-%m); \
    y=$(date -u +%Y); m=$(date -u +%m); m=${m#0}; \
    if [ "$m" -eq 1 ]; then prev=$((y-1))-12; else prev=$(printf '%04d-%02d' "$y" $((m-1))); fi; \
    for month in "$want" "$now" "$prev"; do \
      if curl -fsSL --retry 2 -m 600 "https://download.db-ip.com/free/dbip-city-lite-$month.mmdb.gz" | gunzip > out/dbip-city-lite.mmdb \
         && [ "$(wc -c < out/dbip-city-lite.mmdb)" -gt 10000000 ]; then echo "DB-IP $month hentet"; break; fi; \
      rm -f out/dbip-city-lite.mmdb; echo "DB-IP $month kunne ikke hentes"; \
    done; ls -la out

# ---------- 4) Kør ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=backend /app/publish ./
COPY --from=frontend /src/frontend/dist ./wwwroot
# Server-bundle til prerendering af siden (Backend/Prerender/PrerenderService.cs)
COPY --from=frontend /src/frontend/dist-ssr ./ssr
# Geo-databasen (mappen kan være tom, hvis hentningen fejlede)
COPY --from=geoip /geo/out/ ./geo/
RUN mkdir -p /app/wwwroot/gallery

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "Backend.dll"]
