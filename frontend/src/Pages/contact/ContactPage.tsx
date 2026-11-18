import "../service/ServicePage.css";
import { useText } from "../../texts/TextContext";
import EditButton from "../../admin/EditButton";
import Footer from "../../Components/footer/Footer";
import PageBackdrop from "../../Components/backdrop/PageBackdrop";

// /kontakt – direkte adresse og gammel landingsside. Formularen åbnes i kontakt-boblen.
type ContactPageProps = {
  onContactClick: () => void;
};

function ContactPage({ onContactClick }: ContactPageProps) {
  const title = useText("contact.formTitle");
  const intro = useText("contact.formIntro");
  const submitLabel = useText("contact.submit");
  const companyName = useText("company.name");
  const phone = useText("contact.phone");
  const email = useText("contact.email");

  return (
    <div className="service-page contact-page" id="top">
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
            <EditButton ids={["contact.formTitle", "contact.formIntro"]} label="Rediger overskrift" />
          </h1>
          <p className="contact-page-direct">
            {phone.trim() && (
              <>
                Tlf.: <a href={`tel:${phone.replace(/\s+/g, "")}`}>{phone}</a>
              </>
            )}
            {phone.trim() && email.trim() && " · "}
            {email.trim() && (
              <>
                Mail: <a href={`mailto:${email}`}>{email}</a>
              </>
            )}
          </p>
        </div>
      </header>
      <div className="service-content glass-sheet">
        <div className="content-container">
          {intro && <p className="contact-form-intro">{intro}</p>}
          <button type="button" className="service-cta-button" onClick={onContactClick}>
            {submitLabel}
          </button>
        </div>
      </div>
      <Footer />
    </div>
  );
}

export default ContactPage;
