import "../service/ServicePage.css";
import { useText } from "../../texts/TextContext";
import RichText from "../../texts/RichText";
import EditButton from "../../admin/EditButton";
import Footer from "../../Components/footer/Footer";
import PageBackdrop from "../../Components/backdrop/PageBackdrop";
import { openConsentSettings } from "../../consent";
import "../../Components/consent/CookieBanner.css";

// /cookiepolitik – redegør for cookies, formål, varighed og hvordan samtykke trækkes tilbage
function CookiePolicyPage() {
  const title = useText("cookie.title");
  const body = useText("cookie.body");
  const companyName = useText("company.name");

  return (
    <div className="service-page cookie-page" id="top">
      <PageBackdrop />
      <header className="service-hero">
        <div className="content-container service-hero-inner">
          <nav className="breadcrumbs" aria-label="Du er her">
            <a href="/">{companyName}</a>
            <span aria-hidden="true">›</span>
            <span aria-current="page">{title}</span>
          </nav>
          <h1>
            {title}
            <EditButton ids={["cookie.title", "cookie.body"]} label="Rediger cookiepolitik" />
          </h1>
        </div>
      </header>
      <div className="service-content glass-sheet">
        <div className="content-container">
          <article className="service-body">
            <RichText text={body} headingLevel={2} />
            <p>
              <button type="button" className="cookie-policy-open" onClick={openConsentSettings}>
                Åbn cookie-indstillinger
              </button>
            </p>
          </article>
        </div>
      </div>
      <Footer />
    </div>
  );
}

export default CookiePolicyPage;
