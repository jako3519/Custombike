using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class Custombike : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "about.body",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Brødtekst i Om os (tom linje = nyt afsnit)", "PLACEHOLDER – Jakob skal skrive denne tekst om.\n\nCustombike er It Service v/Jakob Lund (CVR 42637688). Vi printer cykeltilebehør i 3D, ofte i TPU, og tilpasser det til den enkelte cykel – for eksempel en holder til Garmin Varia, vinklet efter sadelpinden." });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "about.title",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Overskrift i Om os", "Om os" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "company.address",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Adresse i bunden af siden (gade, postnr. by). Tom indtil den er udfyldt.", "" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "company.cvr",
                column: "Value",
                value: "42637688");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "company.footerText",
                column: "Value",
                value: "Custombike printer cykeltilebehør til den enkelte cykel. PLACEHOLDER – Jakob skal skrive denne tekst om.");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "company.legalName",
                column: "Value",
                value: "It Service v/Jakob Lund");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "company.name",
                column: "Value",
                value: "Custombike");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.email",
                column: "Value",
                value: "jako3519@gmail.com");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.facebookUrl",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Adresse på Facebook-siden (tom = linket vises ikke)", "" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.formIntro",
                column: "Value",
                value: "Fortæl kort, hvad du har brug for, så vender vi tilbage.");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.formTitle",
                column: "Value",
                value: "Skriv til os");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.phone",
                column: "Value",
                value: "+45 40 26 26 64");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.submit",
                column: "Description",
                value: "Knap på kontaktformularen, når der ikke er valgt et produkt");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.thanksText",
                column: "Value",
                value: "Vi har modtaget din besked og vender tilbage hurtigst muligt.");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.title",
                column: "Value",
                value: "Kontakt");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "cookie.body",
                column: "Value",
                value: "Custombike ved It Service v/Jakob Lund (CVR 42637688) er dataansvarlig for behandlingen af personoplysninger på denne hjemmeside.\n\nVi bruger cookies og lignende teknikker. Nødvendige funktioner virker altid. Cookies til statistik og marketing (Google) sættes kun, hvis du giver samtykke, og kun hvis Google Tag Manager er sat op.\n\n## Nødvendige ##\n\nDisse er nødvendige for, at siden virker, og kræver ikke samtykke.\n\nDit valg om cookies huskes i din browser (localStorage, navn custombike.consent) i 12 måneder, så vi ikke spørger dig igen ved hvert besøg.\n\nHvis du er logget ind som administrator, bruges en login-cookie (custombike.auth) til at holde dig logget ind.\n\n## Statistik og marketing (Google) ##\n\nKun med dit samtykke, og kun når et container-id er konfigureret, indlæser vi Google Tag Manager. Den kan starte tags fra Google Analytics (statistik) og Google Ads (marketing og annoncemåling). Uden container-id indlæses Google ikke.\n\nDe typiske cookies, Google kan sætte, er blandt andet:\n\nStatistik: _ga og _ga_* (typisk op til 13–24 måneder) samt _gid (24 timer). De bruges til at skelne besøg og forstå, hvordan siden bruges.\n\nMarketing: _gcl_au, _gcl_aw og lignende (typisk op til 90 dage). De bruges til at måle annoncer og eventuelt vise mere relevante annoncer.\n\nDen præcise liste afhænger af de tags, der er sat op i Google Tag Manager. Data sendes til Google Ireland Limited og kan blive videregivet til Google LLC i USA. Google behandler oplysningerne efter deres privatlivspolitik (policies.google.com/privacy).\n\n## Vores egen statistik ##\n\nSiden har også en indbygget, cookiefri besøgsstatistik, som ikke bruger cookies og ikke kræver samtykke. Den kører uafhængigt af Google.\n\n## Sådan trækker du samtykket tilbage ##\n\nDu kan altid ændre eller trække dit samtykke tilbage via linket \"Cookie-indstillinger\" i bunden af siden. Valget huskes i 12 måneder, eller indtil du ændrer det. Du kan også slette gemte data i din browser.\n\nHar du spørgsmål til vores brug af cookies, kan du kontakte os på de kontaktoplysninger, der står i bunden af siden.");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "gallery.subtitle",
                column: "Value",
                value: "Eksempler på dele, der er printet til en bestemt cykel.");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "hero.subtitle",
                column: "Value",
                value: "3D-printede holdere og dele i TPU, tilpasset cykelmodellen.");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "hero.title",
                column: "Value",
                value: "Printet til din cykel");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "nav.about",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Menupunkt til Om os", "Om os" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "nav.services",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Menupunkt til Produkter", "Produkter" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "notFound.text",
                column: "Value",
                value: "Siden er måske flyttet eller slettet. Se alle produkter på forsiden.");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.back",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Produktside – link tilbage til alle produkter", "Alle produkter" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.ctaButton",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Produktside – knap der åbner bestillingsformularen", "Bestil – send billede af din cykel" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.ctaText",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Produktside – tekst i bestillingsboksen", "Send et billede af cyklen fra siden, så vinklen kan måles, og delen kan printes til netop din cykel." });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.ctaTitle",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Produktside – overskrift i bestillingsboksen", "Bestil til din cykel" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.mediaTitle",
                column: "Description",
                value: "Produktside – overskrift over billeder og videoer");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.otherTitle",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Produktside – overskrift over de andre produkter", "Andre produkter" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.1.text",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ikke i brug – gammelt felt fra før produkterne lå i deres egen tabel", "" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.1.title",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ikke i brug – gammelt felt fra før produkterne lå i deres egen tabel", "" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.2.text",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ikke i brug – gammelt felt fra før produkterne lå i deres egen tabel", "" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.2.title",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ikke i brug – gammelt felt fra før produkterne lå i deres egen tabel", "" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.3.text",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ikke i brug – gammelt felt fra før produkterne lå i deres egen tabel", "" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.3.title",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ikke i brug – gammelt felt fra før produkterne lå i deres egen tabel", "" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.4.text",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ikke i brug – gammelt felt fra før produkterne lå i deres egen tabel", "" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.4.title",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ikke i brug – gammelt felt fra før produkterne lå i deres egen tabel", "" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.5.text",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ikke i brug – gammelt felt fra før produkterne lå i deres egen tabel", "" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.5.title",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ikke i brug – gammelt felt fra før produkterne lå i deres egen tabel", "" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.6.text",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ikke i brug – gammelt felt fra før produkterne lå i deres egen tabel", "" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.6.title",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ikke i brug – gammelt felt fra før produkterne lå i deres egen tabel", "" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.note",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Linje under produkterne", "Der er ingen kurv eller betaling endnu. En bestilling er en besked med billede, så delen kan tilpasses." });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.readMore",
                column: "Description",
                value: "Link på produktkortene til produktets egen side");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.subtitle",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Undertekst i Produkter", "Hver kategori er en type del. Vælg den, og bestil ved at sende et billede af cyklen." });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.title",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Overskrift i Produkter", "Produkter" });

            migrationBuilder.InsertData(
                table: "SiteTexts",
                columns: new[] { "Id", "Description", "Value" },
                values: new object[,]
                {
                    { "contact.orderIntro", "Tekst over formularen, når den åbnes fra et produkt", "Bestil ved at sende mindst ét billede af cyklen fra siden, så vinklen kan måles." },
                    { "contact.orderSubmit", "Knap på formularen, når et produkt er valgt", "Bestil – send billede af din cykel" },
                    { "contact.photoHelp", "Hjælpetekst når et billede af cyklen er påkrævet", "Fotografer cyklen fra siden, i omtrent samme højde som sadelpinden, så hele pinden er med. Så kan vinklen måles. Jpg, png, webp eller heic – højst 5 billeder, 15 MB pr. billede." }
                });

            // De seks tømrer-ydelser fra den tidligere skabelon erstattes af ét eksempelprodukt.
            // Teksten er en placeholder, som skal skrives om i admin.
            migrationBuilder.Sql(
                "DELETE FROM \"ServiceMedia\"; " +
                "DELETE FROM \"Services\"; " +
                "INSERT INTO \"Services\" (\"Slug\", \"Title\", \"ShortText\", \"Body\", \"Icon\", \"CoverImage\", \"CoverHash\", \"SortOrder\", \"CreatedAt\", \"UpdatedAt\") " +
                "VALUES (" +
                "'garmin-varia-holder', " +
                "'Garmin Varia-holder', " +
                "'PLACEHOLDER – Jakob skal skrive denne tekst om. Holder til en Garmin Varia-radar, printet i TPU og vinklet til sadelpinden på netop din cykel.', " +
                "'## Sådan virker det ##\n\nPLACEHOLDER – Jakob skal skrive denne tekst om.\n\nHoldere printes i TPU, et blødt og sejt plast, så de kan give sig lidt og sidde på sadelpinden. Vinklen tilpasses den enkelte cykel, så Garmin Varia-radaren peger rigtigt bagud.\n\n## Sådan bestiller du ##\n\nPLACEHOLDER – Jakob skal skrive denne tekst om.\n\nÅbn bestillingen her fra siden, og send mindst ét billede af cyklen fra siden. Der er ingen kurv eller betaling endnu. Bestillingen er en besked, og vi vender tilbage.\n\n## Sådan fotograferer du cyklen ##\n\nPLACEHOLDER – Jakob skal skrive denne tekst om.\n\nTag billedet fra siden, omtrent i højde med sadelpinden, så hele pinden og sadlen er med. Stå ikke skråt foran eller bagved. Så kan vinklen måles, og holderen printes til din cykel.', " +
                "'radar', NULL, NULL, 0, now(), now());");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DELETE FROM "Services" WHERE "Slug" = 'garmin-varia-holder';""");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.orderIntro");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.orderSubmit");

            migrationBuilder.DeleteData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.photoHelp");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "about.body",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Brødtekst i Om mig (tom linje = nyt afsnit)", "Skriv lidt om dig selv og firmaet her.\n\nTom linje giver nyt afsnit." });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "about.title",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Overskrift i Om mig", "Om mig" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "company.address",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Adresse i bunden af siden (gade, postnr. by)", "Sviegade 2B, 1., 6760 Ribe" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "company.cvr",
                column: "Value",
                value: "43633104");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "company.footerText",
                column: "Value",
                value: "Carlton på Taget holder til i Ribe og arbejder i hele byen og omegnen. Carl laver tømrerarbejde, tag og terrasser – og du kan se færdige opgaver i galleriet her på siden, før du beslutter dig.");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "company.legalName",
                column: "Value",
                value: "Carlton på Taget, enkeltmandsvirksomhed v/Carl Anton Emmertsen Lund");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "company.name",
                column: "Value",
                value: "Byg & Hyg");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.email",
                column: "Value",
                value: "bygoghyg.is@gmail.com");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.facebookUrl",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Adresse på Facebook-siden", "https://www.facebook.com/" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.formIntro",
                column: "Value",
                value: "Fortæl kort om opgaven, så ringer eller skriver Carl tilbage.");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.formTitle",
                column: "Value",
                value: "Skriv til Carl");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.phone",
                column: "Value",
                value: "+45 50 36 63 92");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.submit",
                column: "Description",
                value: "Knap på kontaktformularen");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.thanksText",
                column: "Value",
                value: "Carl har modtaget din besked og vender tilbage hurtigst muligt.");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "contact.title",
                column: "Value",
                value: "Kontakt os i dag");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "cookie.body",
                column: "Value",
                value: "Carlton på Taget (CVR 43633104), Sviegade 2B, 1., 6760 Ribe, er dataansvarlig for behandlingen af personoplysninger på denne hjemmeside.\n\nVi bruger cookies og lignende teknikker. Nødvendige funktioner virker altid. Cookies til statistik og marketing (Google) sættes kun, hvis du giver samtykke.\n\n## Nødvendige ##\n\nDisse er nødvendige for, at siden virker, og kræver ikke samtykke.\n\nDit valg om cookies huskes i din browser (localStorage, navn carlton.consent) i 12 måneder, så vi ikke spørger dig igen ved hvert besøg.\n\nHvis du er logget ind som administrator, bruges en login-cookie (carlton.auth) til at holde dig logget ind.\n\n## Statistik og marketing (Google) ##\n\nKun med dit samtykke indlæser vi Google Tag Manager (beholder GTM-K2F2GWD6). Den kan starte tags fra Google Analytics (statistik) og Google Ads (marketing og annoncemåling).\n\nDe typiske cookies, Google kan sætte, er blandt andet:\n\nStatistik: _ga og _ga_* (typisk op til 13–24 måneder) samt _gid (24 timer). De bruges til at skelne besøg og forstå, hvordan siden bruges.\n\nMarketing: _gcl_au, _gcl_aw og lignende (typisk op til 90 dage). De bruges til at måle annoncer og eventuelt vise mere relevante annoncer.\n\nDen præcise liste afhænger af de tags, der er sat op i Google Tag Manager. Data sendes til Google Ireland Limited og kan blive videregivet til Google LLC i USA. Google behandler oplysningerne efter deres privatlivspolitik (policies.google.com/privacy).\n\n## Vores egen statistik ##\n\nSiden har også en indbygget, cookiefri besøgsstatistik, som ikke bruger cookies og ikke kræver samtykke. Den kører uafhængigt af Google.\n\n## Sådan trækker du samtykket tilbage ##\n\nDu kan altid ændre eller trække dit samtykke tilbage via linket \"Cookie-indstillinger\" i bunden af siden. Valget huskes i 12 måneder, eller indtil du ændrer det. Du kan også slette gemte data i din browser.\n\nHar du spørgsmål til vores brug af cookies, kan du kontakte os på de kontaktoplysninger, der står i bunden af siden.");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "gallery.subtitle",
                column: "Value",
                value: "Et udvalg af opgaver jeg har lavet");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "hero.subtitle",
                column: "Value",
                value: "Tømrerarbejde, terrasser og byggeri – udført med omhu");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "hero.title",
                column: "Value",
                value: "Håndværk i træ");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "nav.about",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Menupunkt til Om mig", "Om mig" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "nav.services",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Menupunkt til Ydelser", "Ydelser" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "notFound.text",
                column: "Value",
                value: "Siden er måske flyttet eller slettet. Se alle ydelser på forsiden.");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.back",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelsesside – link tilbage til alle ydelser", "Alle ydelser" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.ctaButton",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelsesside – knap der åbner kontaktboksen", "Skriv til Carl" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.ctaText",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelsesside – tekst i kontaktboksen", "Ring eller skriv og fortæl, hvad du har brug for." });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.ctaTitle",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelsesside – overskrift i kontaktboksen", "Skal Carl hjælpe med opgaven?" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.mediaTitle",
                column: "Description",
                value: "Ydelsesside – overskrift over billeder og videoer");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "service.otherTitle",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelsesside – overskrift over de andre ydelser", "Andre ydelser" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.1.text",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelse 1 – tekst", "Nyt tag, tagrenovering, udskiftning af tagplader og reparation af utætte tage." });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.1.title",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelse 1 – titel", "Tagarbejde" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.2.text",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelse 2 – tekst", "Nye træterrasser, hævede terrasser og renovering af gamle terrasser." });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.2.title",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelse 2 – titel", "Terrasser" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.3.text",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelse 3 – tekst", "Carporte, skure, overdækninger og mindre tilbygninger." });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.3.title",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelse 3 – titel", "Tilbygninger og carporte" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.4.text",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelse 4 – tekst", "Udskiftning og montering af vinduer, døre og ovenlysvinduer." });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.4.title",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelse 4 – titel", "Vinduer og døre" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.5.text",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelse 5 – tekst", "Gulve, lofter, skillevægge og efterisolering." });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.5.title",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelse 5 – titel", "Indvendigt tømrerarbejde" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.6.text",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelse 6 – tekst", "Træbeklædning, sternbrædder og vindskeder." });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.6.title",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Ydelse 6 – titel", "Facader og beklædning" });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.note",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Linje under ydelserne om småopgaver", "Småopgaver og reparationer: Carl tager også mindre opgaver for private i Ribe og omegn." });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.readMore",
                column: "Description",
                value: "Link på ydelseskortene til ydelsens egen side");

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.subtitle",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Undertekst i Ydelser", "Tømrerarbejde for private i Ribe og omegn – fra tag og terrasse til opgaver inden døre." });

            migrationBuilder.UpdateData(
                table: "SiteTexts",
                keyColumn: "Id",
                keyValue: "services.title",
                columns: new[] { "Description", "Value" },
                values: new object[] { "Overskrift i Ydelser", "Ydelser" });
        }
    }
}
