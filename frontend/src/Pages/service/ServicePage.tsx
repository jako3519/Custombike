import "./ServicePage.css";
import type { Service } from "../../Api";
import { useText } from "../../texts/TextContext";
import RichText from "../../texts/RichText";
import EditButton from "../../admin/EditButton";
import { useAdmin } from "../../admin/AdminContext";
import { useServices } from "../../services/ServicesContext";
import ServiceIcon from "../../services/ServiceIcon";
import MediaGallery from "../../Components/media/MediaGallery";
import Footer from "../../Components/footer/Footer";
import PageBackdrop from "../../Components/backdrop/PageBackdrop";

type ServicePageProps = {
  service: Service;
  onContactClick: () => void;
};

// En ydelses egen side: /produkter/<slug>
function ServicePage({ service, onContactClick }: ServicePageProps) {
  const { services } = useServices();
  const { isAdmin, editService } = useAdmin();
  const companyName = useText("company.name");
  const servicesTitle = useText("services.title");
  const phone = useText("contact.phone");
  const back = useText("service.back");
  const mediaTitle = useText("service.mediaTitle");
  const ctaTitle = useText("service.ctaTitle");
  const ctaText = useText("service.ctaText");
  const ctaButton = useText("service.ctaButton");
  const otherTitle = useText("service.otherTitle");

  const others = services.filter((s) => s.id !== service.id);

  return (
    <div className="service-page" id="top">
      {/* Ydelsens forsidebillede (ellers forsidens billede) ligger fast bag hele siden */}
      <PageBackdrop src={service.coverUrl} />
      <header className={`service-hero${service.coverUrl ? " has-cover" : ""}`}>
        <div className="content-container service-hero-inner">
          <nav className="breadcrumbs" aria-label="Du er her">
            <a href="/">{companyName}</a>
            <span aria-hidden="true">›</span>
            <a href="/#produkter">{servicesTitle}</a>
            <span aria-hidden="true">›</span>
            <span aria-current="page">{service.title}</span>
          </nav>
          <span className="service-hero-icon" aria-hidden="true">
            <ServiceIcon icon={service.icon} size={32} />
          </span>
          <h1>
            {service.title}
            <EditButton onClick={() => editService(service.id)} label="Rediger produkt" />
          </h1>
        </div>
      </header>

      <div className="service-content glass-sheet">
        <div className="content-container service-layout">
          <article className="service-body">
            <RichText text={service.body || service.shortText} headingLevel={2} />
          </article>

          <aside className="service-cta">
            <h2>
              {ctaTitle}
              <EditButton ids={["service.ctaTitle", "service.ctaText", "service.ctaButton"]} label="Rediger kontaktboksen" />
            </h2>
            <RichText text={ctaText} />
            {phone.trim() && (
              <a className="service-cta-phone" href={`tel:${phone.replace(/\s+/g, "")}`}>
                <svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor" aria-hidden="true">
                  <path d="M6.62 10.79a15.05 15.05 0 0 0 6.59 6.59l2.2-2.2a1 1 0 0 1 1.02-.24c1.12.37 2.33.57 3.57.57a1 1 0 0 1 1 1V20a1 1 0 0 1-1 1A17 17 0 0 1 3 4a1 1 0 0 1 1-1h3.5a1 1 0 0 1 1 1c0 1.25.2 2.45.57 3.57a1 1 0 0 1-.25 1.02l-2.2 2.2Z" />
                </svg>
                {phone}
              </a>
            )}
            <button type="button" className="service-cta-button" onClick={onContactClick}>
              {ctaButton}
            </button>
          </aside>
        </div>

        {(service.media.length > 0 || isAdmin) && (
          <section className="content-container service-media" aria-labelledby="service-media-title">
            <h2 className="section-title" id="service-media-title">
              {mediaTitle}
              <EditButton ids={["service.mediaTitle"]} label="Rediger overskrift" />
            </h2>
            {isAdmin && (
              <button type="button" className="gallery-edit-cta" onClick={() => editService(service.id, true)}>
                <span className="edit-btn" aria-hidden="true">
                  <svg viewBox="0 0 24 24" width="14" height="14">
                    <path d="M12 5v14M5 12h14" stroke="currentColor" strokeWidth="3.2" strokeLinecap="round" />
                  </svg>
                </span>
                Rediger billeder og video – upload, YouTube, slet og rækkefølge
              </button>
            )}
            {service.media.length > 0 && <MediaGallery media={service.media} title={service.title} />}
          </section>
        )}

        {others.length > 0 && (
          <section className="content-container service-others" aria-labelledby="service-others-title">
            <h2 className="service-others-title" id="service-others-title">
              {otherTitle}
              <EditButton ids={["service.otherTitle", "service.back"]} label="Rediger overskrift" />
            </h2>
            <ul className="service-others-list">
              {others.map((s) => (
                <li key={s.id}>
                  <a href={`/produkter/${s.slug}`}>
                    <span className="service-others-icon" aria-hidden="true">
                      <ServiceIcon icon={s.icon} size={22} />
                    </span>
                    {s.title}
                  </a>
                </li>
              ))}
            </ul>
            <a className="service-back" href="/#produkter">
              <svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true">
                <path d="M19 12H5m5-5-5 5 5 5" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" />
              </svg>
              {back}
            </a>
          </section>
        )}
      </div>

      <Footer />
    </div>
  );
}

export default ServicePage;
