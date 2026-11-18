# Geo-database til besøgsstatistikken

Statistikken (kun synlig for admin) viser land og by for de besøgende. Opslaget sker **lokalt på
serveren** i en fil fra DB-IP. Der sendes intet til tredjepart, og IP-adressen bliver aldrig gemt.

- **Kilde:** DB-IP "IP to City Lite" (gratis, ingen konto), https://db-ip.com/db/download/ip-to-city-lite
- **Licens:** Creative Commons Attribution 4.0 (CC BY 4.0). Kræver kreditering. Den står i
  Statistik-pop-up'en som "IP-geolokation fra DB-IP.com" med link til https://db-ip.com.
- **Hentes:** automatisk ved `docker build` / `flyctl deploy` (se Dockerfile, trin "geoip"). Filen
  lægges i imaget som `/app/geo/dbip-city-lite.mmdb` og ligger ikke i git.

## Opdatering (fx en gang om måneden)

DB-IP udgiver en ny fil den 1. i hver måned og har kun den nuværende og forrige måned liggende.

1. Skriv den nye måned i `Backend/GeoData/dbip-month.txt`, fx `2026-11`.
2. Deploy som normalt til Custombikes egen Fly-app, fx `flyctl deploy --app custombike --ha=false`. Opret appen først. Deployér aldrig til en anden kundes app.

Ændringen i filen gør, at Docker henter databasen igen. Hvis den angivne måned ikke findes
længere, prøver bygget automatisk den nuværende og den forrige måned. Hvis intet kan hentes,
bygges siden alligevel, og statistikken gemmes så bare uden land/by. Se loggen ved opstart:
"Geo-database indlaest" eller "Geo-database ikke fundet".

## Lokalt (valgfrit)

Læg den udpakkede fil som `Backend/geo/dbip-city-lite.mmdb` (ignoreres af git og docker), eller
peg på en anden sti med `Analytics__GeoDbPath`.
