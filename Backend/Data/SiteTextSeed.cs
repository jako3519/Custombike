using Backend.Models;

namespace Backend.Data
{
    // Standardtekster der lægges i databasen første gang. Ændres bagefter via POST /api/texts.
    public static class SiteTextSeed
    {
        public static readonly SiteText[] Texts =
        {
            new() { Id = "company.name", Value = "Byg & Hyg", Description = "Firmanavn i navbar og footer" },

            new() { Id = "nav.about", Value = "Om mig", Description = "Menupunkt til Om mig" },
            new() { Id = "nav.gallery", Value = "Billeder", Description = "Menupunkt til Billeder" },
            new() { Id = "nav.contact", Value = "Kontakt", Description = "Kontakt-knap i navbar" },

            new() { Id = "hero.title", Value = "Håndværk i træ", Description = "Stor overskrift på forsiden" },
            new() { Id = "hero.subtitle", Value = "Tømrerarbejde, terrasser og byggeri – udført med omhu", Description = "Undertekst på forsiden" },
            new() { Id = "hero.image", Value = "", Description = "Forsidebillede – vælges i billed-redigeringen (tom = første billede i galleriet)" },

            new() { Id = "about.title", Value = "Om mig", Description = "Overskrift i Om mig" },
            new() { Id = "about.body", Value = "Skriv lidt om dig selv og firmaet her.\n\nTom linje giver nyt afsnit.", Description = "Brødtekst i Om mig (tom linje = nyt afsnit)" },

            new() { Id = "gallery.title", Value = "Billeder", Description = "Overskrift i Billeder" },
            new() { Id = "gallery.subtitle", Value = "Et udvalg af opgaver jeg har lavet", Description = "Undertekst i Billeder" },
            new() { Id = "gallery.empty", Value = "Der er ingen billeder endnu.", Description = "Vises når galleriet er tomt" },

            new() { Id = "contact.title", Value = "Kontakt os i dag", Description = "Overskrift i kontakt-boksen" },
            new() { Id = "contact.email", Value = "bygoghyg.is@gmail.com", Description = "E-mail i kontakt-boksen" },
            new() { Id = "contact.phone", Value = "+45 50 36 63 92", Description = "Telefonnummer i kontakt-boksen" },
            new() { Id = "contact.facebookLabel", Value = "Facebook", Description = "Tekst på Facebook-linket" },
            new() { Id = "contact.facebookUrl", Value = "https://www.facebook.com/", Description = "Adresse på Facebook-siden" },

            new() { Id = "footer.text", Value = "Alle rettigheder forbeholdes", Description = "Tekst i bunden af siden" },

            new() { Id = "company.legalName", Value = "Carlton på Taget, enkeltmandsvirksomhed v/Carl Anton Emmertsen Lund", Description = "Juridisk navn/virksomhedsform i bunden af siden" },
            new() { Id = "company.address", Value = "Sviegade 2B, 1., 6760 Ribe", Description = "Adresse i bunden af siden (gade, postnr. by)" },
            new() { Id = "company.cvr", Value = "43633104", Description = "CVR-nummer i bunden af siden" },
            new() { Id = "company.footerText", Value = "Carlton på Taget holder til i Ribe og arbejder i hele byen og omegnen. Carl laver tømrerarbejde, tag og terrasser – og du kan se færdige opgaver i galleriet her på siden, før du beslutter dig.", Description = "Kort tekst om firmaet i bunden af siden" },

            new() { Id = "nav.services", Value = "Ydelser", Description = "Menupunkt til Ydelser" },
            new() { Id = "services.title", Value = "Ydelser", Description = "Overskrift i Ydelser" },
            new() { Id = "services.subtitle", Value = "Tømrerarbejde for private i Ribe og omegn – fra tag og terrasse til opgaver inden døre.", Description = "Undertekst i Ydelser" },
            new() { Id = "services.1.title", Value = "Tagarbejde", Description = "Ydelse 1 – titel" },
            new() { Id = "services.1.text", Value = "Nyt tag, tagrenovering, udskiftning af tagplader og reparation af utætte tage.", Description = "Ydelse 1 – tekst" },
            new() { Id = "services.2.title", Value = "Terrasser", Description = "Ydelse 2 – titel" },
            new() { Id = "services.2.text", Value = "Nye træterrasser, hævede terrasser og renovering af gamle terrasser.", Description = "Ydelse 2 – tekst" },
            new() { Id = "services.3.title", Value = "Tilbygninger og carporte", Description = "Ydelse 3 – titel" },
            new() { Id = "services.3.text", Value = "Carporte, skure, overdækninger og mindre tilbygninger.", Description = "Ydelse 3 – tekst" },
            new() { Id = "services.4.title", Value = "Vinduer og døre", Description = "Ydelse 4 – titel" },
            new() { Id = "services.4.text", Value = "Udskiftning og montering af vinduer, døre og ovenlysvinduer.", Description = "Ydelse 4 – tekst" },
            new() { Id = "services.5.title", Value = "Indvendigt tømrerarbejde", Description = "Ydelse 5 – titel" },
            new() { Id = "services.5.text", Value = "Gulve, lofter, skillevægge og efterisolering.", Description = "Ydelse 5 – tekst" },
            new() { Id = "services.6.title", Value = "Facader og beklædning", Description = "Ydelse 6 – titel" },
            new() { Id = "services.6.text", Value = "Træbeklædning, sternbrædder og vindskeder.", Description = "Ydelse 6 – tekst" },
            new() { Id = "services.note", Value = "Småopgaver og reparationer: Carl tager også mindre opgaver for private i Ribe og omegn.", Description = "Linje under ydelserne om småopgaver" },

            // Ydelsernes egne sider (/ydelser/<navn>) - selve ydelserne ligger i tabellen Services
            new() { Id = "services.readMore", Value = "Læs mere", Description = "Link på ydelseskortene til ydelsens egen side" },
            new() { Id = "service.back", Value = "Alle ydelser", Description = "Ydelsesside – link tilbage til alle ydelser" },
            new() { Id = "service.mediaTitle", Value = "Billeder og video", Description = "Ydelsesside – overskrift over billeder og videoer" },
            new() { Id = "service.ctaTitle", Value = "Skal Carl hjælpe med opgaven?", Description = "Ydelsesside – overskrift i kontaktboksen" },
            new() { Id = "service.ctaText", Value = "Ring eller skriv og fortæl, hvad du har brug for.", Description = "Ydelsesside – tekst i kontaktboksen" },
            new() { Id = "service.ctaButton", Value = "Skriv til Carl", Description = "Ydelsesside – knap der åbner kontaktboksen" },
            new() { Id = "service.otherTitle", Value = "Andre ydelser", Description = "Ydelsesside – overskrift over de andre ydelser" },
            new() { Id = "notFound.title", Value = "Siden findes ikke", Description = "Side der ikke findes – overskrift" },
            new() { Id = "notFound.text", Value = "Siden er måske flyttet eller slettet. Se alle ydelser på forsiden.", Description = "Side der ikke findes – tekst" },
            new() { Id = "notFound.back", Value = "Til forsiden", Description = "Side der ikke findes – knap til forsiden" },

            new() { Id = "contact.formTitle", Value = "Skriv til Carl", Description = "Overskrift over kontaktformularen" },
            new() { Id = "contact.formIntro", Value = "Fortæl kort om opgaven, så ringer eller skriver Carl tilbage.", Description = "Kort tekst over kontaktformularen" },
            new() { Id = "contact.gdpr", Value = "Dine oplysninger bruges kun til at besvare din henvendelse. Vi gemmer dem i op til 12 måneder.", Description = "Tekst under kontaktformularen om brug af oplysninger" },
            new() { Id = "contact.submit", Value = "Send besked", Description = "Knap på kontaktformularen" },
            new() { Id = "contact.thanksTitle", Value = "Tak for din henvendelse", Description = "Overskrift på tak-siden efter en henvendelse" },
            new() { Id = "contact.thanksText", Value = "Carl har modtaget din besked og vender tilbage hurtigst muligt.", Description = "Tekst på tak-siden efter en henvendelse" },

            new() { Id = "cookie.title", Value = "Cookiepolitik", Description = "Overskrift på siden Cookiepolitik" },
            new() { Id = "cookie.body", Value = "Carlton på Taget (CVR 43633104), Sviegade 2B, 1., 6760 Ribe, er dataansvarlig for behandlingen af personoplysninger på denne hjemmeside.\n\nVi bruger cookies og lignende teknikker. Nødvendige funktioner virker altid. Cookies til statistik og marketing (Google) sættes kun, hvis du giver samtykke.\n\n## Nødvendige ##\n\nDisse er nødvendige for, at siden virker, og kræver ikke samtykke.\n\nDit valg om cookies huskes i din browser (localStorage, navn carlton.consent) i 12 måneder, så vi ikke spørger dig igen ved hvert besøg.\n\nHvis du er logget ind som administrator, bruges en login-cookie (carlton.auth) til at holde dig logget ind.\n\n## Statistik og marketing (Google) ##\n\nKun med dit samtykke indlæser vi Google Tag Manager (beholder GTM-K2F2GWD6). Den kan starte tags fra Google Analytics (statistik) og Google Ads (marketing og annoncemåling).\n\nDe typiske cookies, Google kan sætte, er blandt andet:\n\nStatistik: _ga og _ga_* (typisk op til 13–24 måneder) samt _gid (24 timer). De bruges til at skelne besøg og forstå, hvordan siden bruges.\n\nMarketing: _gcl_au, _gcl_aw og lignende (typisk op til 90 dage). De bruges til at måle annoncer og eventuelt vise mere relevante annoncer.\n\nDen præcise liste afhænger af de tags, der er sat op i Google Tag Manager. Data sendes til Google Ireland Limited og kan blive videregivet til Google LLC i USA. Google behandler oplysningerne efter deres privatlivspolitik (policies.google.com/privacy).\n\n## Vores egen statistik ##\n\nSiden har også en indbygget, cookiefri besøgsstatistik, som ikke bruger cookies og ikke kræver samtykke. Den kører uafhængigt af Google.\n\n## Sådan trækker du samtykket tilbage ##\n\nDu kan altid ændre eller trække dit samtykke tilbage via linket \"Cookie-indstillinger\" i bunden af siden. Valget huskes i 12 måneder, eller indtil du ændrer det. Du kan også slette gemte data i din browser.\n\nHar du spørgsmål til vores brug af cookies, kan du kontakte os på de kontaktoplysninger, der står i bunden af siden.", Description = "Brødtekst på siden Cookiepolitik (tom linje = nyt afsnit, ## Overskrift ##)" },
        };
    }
}
