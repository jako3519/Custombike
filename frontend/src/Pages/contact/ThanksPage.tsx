import { useEffect } from "react";
import "../service/ServicePage.css";
import { useText } from "../../texts/TextContext";
import RichText from "../../texts/RichText";
import EditButton from "../../admin/EditButton";
import Footer from "../../Components/footer/Footer";
import PageBackdrop from "../../Components/backdrop/PageBackdrop";

// /tak – vises efter en sendt henvendelse. Bruges også som konverteringsside i Google Ads.
function ThanksPage() {
  const title = useText("contact.thanksTitle");
  const text = useText("contact.thanksText");
  const back = useText("notFound.back");

  useEffect(() => {
    window.dataLayer = window.dataLayer || [];
    window.dataLayer.push({ event: "contact_form_submit" });
  }, []);

  return (
    <div className="service-page thanks-page" id="top">
      <PageBackdrop />
      <header className="service-hero">
        <div className="content-container service-hero-inner">
          <h1>
            {title}
            <EditButton ids={["contact.thanksTitle", "contact.thanksText"]} label="Rediger tak-siden" />
          </h1>
        </div>
      </header>
      <div className="service-content glass-sheet">
        <div className="content-container">
          <RichText text={text} className="notfound-text" />
          <a className="notfound-home" href="/">
            {back}
          </a>
        </div>
      </div>
      <Footer />
    </div>
  );
}

export default ThanksPage;
