// Bruges indtil teksterne er hentet fra backend (og hvis backend ikke svarer).
// Holdes ens med Backend/Data/SiteTextSeed.cs
export const defaultTexts = {
  "company.name": "Carlton på taget",

  "nav.about": "Om mig",
  "nav.gallery": "Billeder",
  "nav.contact": "Kontakt",

  "hero.title": "Håndværk i træ",
  "hero.subtitle": "Tømrerarbejde, terrasser og byggeri – udført med omhu",
  "hero.image": "",

  "about.title": "Om mig",
  "about.body": "Skriv lidt om dig selv og firmaet her.\n\nTom linje giver nyt afsnit.",

  "gallery.title": "Billeder",
  "gallery.subtitle": "Et udvalg af opgaver jeg har lavet",
  "gallery.empty": "Der er ingen billeder endnu.",

  "contact.title": "Kontakt os i dag",
  "contact.email": "cemmertsenlund@gmail.com",
  "contact.phone": "+45 60 20 56 20",
  "contact.facebookLabel": "Facebook",
  "contact.facebookUrl": "https://www.facebook.com/",

  "footer.text": "Alle rettigheder forbeholdes",

  "company.legalName": "Carlton på Taget, enkeltmandsvirksomhed v/Carl Anton Emmertsen Lund",
  "company.address": "Sviegade 2B, 1., 6760 Ribe",
  "company.cvr": "43633104",
  "company.footerText": "Carlton på Taget holder til i Ribe og arbejder i hele byen og omegnen. Carl laver tømrerarbejde, tag og terrasser – og du kan se færdige opgaver i galleriet her på siden, før du beslutter dig.",

  "nav.services": "Ydelser",
  "services.title": "Ydelser",
  "services.subtitle": "Tømrerarbejde for private i Ribe og omegn – fra tag og terrasse til opgaver inden døre.",
  "services.1.title": "Tagarbejde",
  "services.1.text": "Nyt tag, tagrenovering, udskiftning af tagplader og reparation af utætte tage.",
  "services.2.title": "Terrasser",
  "services.2.text": "Nye træterrasser, hævede terrasser og renovering af gamle terrasser.",
  "services.3.title": "Tilbygninger og carporte",
  "services.3.text": "Carporte, skure, overdækninger og mindre tilbygninger.",
  "services.4.title": "Vinduer og døre",
  "services.4.text": "Udskiftning og montering af vinduer, døre og ovenlysvinduer.",
  "services.5.title": "Indvendigt tømrerarbejde",
  "services.5.text": "Gulve, lofter, skillevægge og efterisolering.",
  "services.6.title": "Facader og beklædning",
  "services.6.text": "Træbeklædning, sternbrædder og vindskeder.",
  "services.note": "Småopgaver og reparationer: Carl tager også mindre opgaver for private i Ribe og omegn.",

  "services.readMore": "Læs mere",
  "service.back": "Alle ydelser",
  "service.mediaTitle": "Billeder og video",
  "service.ctaTitle": "Skal Carl hjælpe med opgaven?",
  "service.ctaText": "Ring eller skriv og fortæl, hvad du har brug for.",
  "service.ctaButton": "Skriv til Carl",
  "service.otherTitle": "Andre ydelser",
  "notFound.title": "Siden findes ikke",
  "notFound.text": "Siden er måske flyttet eller slettet. Se alle ydelser på forsiden.",
  "notFound.back": "Til forsiden",

  "contact.formTitle": "Skriv til Carl",
  "contact.formIntro": "Fortæl kort om opgaven, så ringer eller skriver Carl tilbage.",
  "contact.gdpr": "Dine oplysninger bruges kun til at besvare din henvendelse. Vi gemmer dem i op til 12 måneder.",
  "contact.submit": "Send besked",
  "contact.thanksTitle": "Tak for din henvendelse",
  "contact.thanksText": "Carl har modtaget din besked og vender tilbage hurtigst muligt.",

  "cookie.title": "Cookiepolitik",
  "cookie.body": "Carlton på Taget (CVR 43633104), Sviegade 2B, 1., 6760 Ribe, er dataansvarlig for behandlingen af personoplysninger på denne hjemmeside.\n\nVi bruger cookies og lignende teknikker. Nødvendige funktioner virker altid. Cookies til statistik og marketing (Google) sættes kun, hvis du giver samtykke.\n\n## Nødvendige ##\n\nDisse er nødvendige for, at siden virker, og kræver ikke samtykke.\n\nDit valg om cookies huskes i din browser (localStorage, navn carlton.consent) i 12 måneder, så vi ikke spørger dig igen ved hvert besøg.\n\nHvis du er logget ind som administrator, bruges en login-cookie (carlton.auth) til at holde dig logget ind.\n\n## Statistik og marketing (Google) ##\n\nKun med dit samtykke indlæser vi Google Tag Manager (beholder GTM-K2F2GWD6). Den kan starte tags fra Google Analytics (statistik) og Google Ads (marketing og annoncemåling).\n\nDe typiske cookies, Google kan sætte, er blandt andet:\n\nStatistik: _ga og _ga_* (typisk op til 13–24 måneder) samt _gid (24 timer). De bruges til at skelne besøg og forstå, hvordan siden bruges.\n\nMarketing: _gcl_au, _gcl_aw og lignende (typisk op til 90 dage). De bruges til at måle annoncer og eventuelt vise mere relevante annoncer.\n\nDen præcise liste afhænger af de tags, der er sat op i Google Tag Manager. Data sendes til Google Ireland Limited og kan blive videregivet til Google LLC i USA. Google behandler oplysningerne efter deres privatlivspolitik (policies.google.com/privacy).\n\n## Vores egen statistik ##\n\nSiden har også en indbygget, cookiefri besøgsstatistik, som ikke bruger cookies og ikke kræver samtykke. Den kører uafhængigt af Google.\n\n## Sådan trækker du samtykket tilbage ##\n\nDu kan altid ændre eller trække dit samtykke tilbage via linket \"Cookie-indstillinger\" i bunden af siden. Valget huskes i 12 måneder, eller indtil du ændrer det. Du kan også slette gemte data i din browser.\n\nHar du spørgsmål til vores brug af cookies, kan du kontakte os på de kontaktoplysninger, der står i bunden af siden.",
} as const;

export type TextId = keyof typeof defaultTexts;
