# Custombike

Dansk hjemmeside for **It Service v/Jakob Lund** (CVR 42637688): specialfremstillede, 3D-printede cykeldele, ofte i TPU, tilpasset den enkelte cykel.

Siden er bygget som Carlton-skabelonen: ASP.NET 9, React 19 med Vite og server-rendering, admin med Google-login, kontaktformular, galleri, cookie-banner og sitemap. Der er ingen kurv og ingen betaling. En bestilling er kontaktformularen, åbnet fra et produkt, med mindst ét billede af cyklen.

## Kør lokalt

1. Opret en Postgres-database og sæt `DATABASE_URL` (se `.env.example`). Læg den i `.env.local` i roden eller i `Backend/.env`.
2. `dotnet run --project Backend` (lytter på http://localhost:5248 og kører migrationer ved opstart).
3. `cd frontend && npm ci && npm run dev` (http://localhost:5173).

`npm run build` i `frontend` bygger både browseren og server-bundtet. Docker-filen i roden samler begge dele.

## Det Jakob skal sætte, før siden går i drift

Intet af dette er oprettet endnu. Opret det selv, og peg ikke på Carltons Fly-app eller database.

| Hvad | Variabel |
| --- | --- |
| Database | `DATABASE_URL` og til Neon-migrationer `DATABASE_URL_UNPOOLED` |
| Offentligt domæne | `Site__Url`, `Site__CanonicalHost`, evt. `Site__RedirectHosts`. Indtil `Site__Url` er sat, står `https://custombike.example/` som pladsholder i delings-tags |
| Google-login | `GoogleOAuth__ClientId`, `GoogleOAuth__ClientSecret`. Redirect-URI: `https://<domæne>/signin-google` |
| Hvem der er admin | `Backend/Security/AdminWhitelist.cs` (nu `jako3519@gmail.com`) |
| Mail fra kontaktformularen | `SMTP_HOST`, `SMTP_PORT`, `SMTP_USERNAME`, `SMTP_PASSWORD`, `CONTACT_TO_EMAIL`, `CONTACT_FROM_EMAIL` |
| Google Tag Manager | `Analytics__GtmId` (tom = slået fra) |
| Search Console | `Site__GoogleSiteVerification` (tom = intet verificerings-meta) |
| Fly.io | App-navnet i `fly.toml` er `custombike`. Opret appen selv, og sæt hemmelighederne som Fly secrets. |

Tekster, produkter, billeder og galleri rettes i admin efter login. Eksempelproduktet **Garmin Varia-holder** er en placeholder og skal skrives om.
