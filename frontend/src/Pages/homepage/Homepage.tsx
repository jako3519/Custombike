import { useEffect, useRef } from "react";
import "./Homepage.css";
import { useText } from "../../texts/TextContext";
import RichText from "../../texts/RichText";
import { assetUrl } from "../../Api";
import type { Service } from "../../Api";
import Gallery from "../../Components/gallery/Gallery";
import Footer from "../../Components/footer/Footer";
import PageBackdrop from "../../Components/backdrop/PageBackdrop";
import EditButton from "../../admin/EditButton";
import { useAdmin } from "../../admin/AdminContext";
import { useServices } from "../../services/ServicesContext";
import ServiceIcon from "../../services/ServiceIcon";
import logo from "../../assets/logo.png";

// Et ydelseskort: ikon, navn (oven på forsidebilledet, hvis der er et), kort tekst og "Læs mere".
// Hele kortet er ét link (ét tab-stop); linkets navn er ydelsens titel. Admins rediger-knap ligger
// uden for linket (en knap må ikke ligge inde i et link), så et klik på den ikke åbner ydelsessiden.
function ServiceCard({ service, readMore }: { service: Service; readMore: string }) {
  const { editService } = useAdmin();
  const href = `/produkter/${service.slug}`;
  const titleId = `service-title-${service.id}`;
  const title = (
    <h3 className="service-title" id={titleId}>
      {service.title}
    </h3>
  );

  return (
    <article className="service-card-wrap">
      <a
        className={`service-card${service.coverUrl ? " has-cover" : ""}`}
        href={href}
        aria-labelledby={titleId}
        data-track={`service:${service.slug}`}
      >
        {service.coverUrl ? (
          <div className="service-cover">
            <img src={assetUrl(service.coverUrl)} alt="" loading="lazy" />
            <span className="service-icon" aria-hidden="true">
              <ServiceIcon icon={service.icon} />
            </span>
            {title}
          </div>
        ) : (
          <>
            <span className="service-icon" aria-hidden="true">
              <ServiceIcon icon={service.icon} />
            </span>
            {title}
          </>
        )}
        <div className="service-card-body">
          <RichText text={service.shortText} headingLevel={4} className="service-text" />
          {/* Kun et synligt tegn på, at kortet kan klikkes – selve linket er hele kortet */}
          <span className="service-more" aria-hidden="true">
            {readMore}
            <svg viewBox="0 0 24 24" width="18" height="18">
              <path d="M5 12h14m-5-5 5 5-5 5" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" />
            </svg>
          </span>
        </div>
      </a>
      <EditButton onClick={() => editService(service.id)} label={`Rediger produkt: ${service.title}`} className="service-card-edit" />
    </article>
  );
}

function HomePage() {
  const overlayRef = useRef<HTMLDivElement>(null);
  const { services } = useServices();
  const { isAdmin, editGallery, editServices } = useAdmin();

  const companyName = useText("company.name");
  const heroTitle = useText("hero.title");
  const heroSubtitle = useText("hero.subtitle");
  const aboutTitle = useText("about.title");
  const aboutBody = useText("about.body");
  const galleryTitle = useText("gallery.title");
  const gallerySubtitle = useText("gallery.subtitle");
  const servicesTitle = useText("services.title");
  const servicesSubtitle = useText("services.subtitle");
  const servicesNote = useText("services.note");
  const readMore = useText("services.readMore");

  // Teksten i toppen fader ud når man scroller (samme effekt som før)
  useEffect(() => {
    const handleScroll = () => {
      if (overlayRef.current) {
        const fadeOutDistance = 500;
        const newOpacity = Math.max(0, 1 - window.scrollY / fadeOutDistance);
        overlayRef.current.style.opacity = newOpacity.toString();
      }
    };
    window.addEventListener("scroll", handleScroll, { passive: true });
    return () => window.removeEventListener("scroll", handleScroll);
  }, []);

  return (
    <div className="homepage-wrapper" id="top">
      {/* Forsidebilledet ligger fast bag hele siden; indholdet scroller ovenpå som glas */}
      <PageBackdrop allowVideo />

      {/* SEKTION 1: Hero (logo og overskrift oven på billedet) */}
      <div className="sticky-hero-container">
        <div className="content-overlay" ref={overlayRef}>
          <img className="hero-logo" src={logo} alt={companyName} />
          <h1>
            {heroTitle}
            <EditButton ids={["hero.title"]} label="Rediger overskrift" />
          </h1>
          <div className="hero-subtitle">
            <RichText text={heroSubtitle} />
            <EditButton ids={["hero.subtitle"]} label="Rediger undertekst" />
          </div>
          <a href="#produkter" className="hero-scroll" aria-label="Scroll ned">
            <svg viewBox="0 0 24 24" width="28" height="28" aria-hidden="true">
              <path d="M6 9l6 6 6-6" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" />
            </svg>
          </a>
        </div>
      </div>

      {/* SEKTION 2: Indholdet, der scroller op over hero */}
      <div className="scrollable-content">
        <section id="produkter" className="services-section glass-sheet">
          <div className="content-container">
            <div className="services-head">
              <h2 className="section-title">
                {servicesTitle}
                <EditButton ids={["services.title", "services.subtitle"]} label="Rediger overskrift" />
              </h2>
              <RichText text={servicesSubtitle} className="section-subtitle" />
            </div>
            {isAdmin && (
              <button type="button" className="gallery-edit-cta services-edit-cta" onClick={editServices}>
                <span className="edit-btn" aria-hidden="true">
                  <svg viewBox="0 0 24 24" width="14" height="14">
                    <path d="M12 5v14M5 12h14" stroke="currentColor" strokeWidth="3.2" strokeLinecap="round" />
                  </svg>
                </span>
                Rediger produkter – tilføj, slet og rækkefølge
              </button>
            )}
            <div className="services-grid">
              {services.map((service) => (
                <ServiceCard key={service.id} service={service} readMore={readMore} />
              ))}
            </div>
            {(servicesNote || isAdmin) && (
              <div className="services-note">
                <svg viewBox="0 0 24 24" width="22" height="22" aria-hidden="true">
                  <path d="M5 12.5l4.5 4.5L19 7.5" fill="none" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" strokeLinejoin="round" />
                </svg>
                <div>
                  <RichText text={servicesNote} />
                  <EditButton ids={["services.note"]} label="Rediger tekst om småopgaver" />
                </div>
              </div>
            )}
          </div>
        </section>

        <section id="om-mig" className="about-section glass-sheet">
          <div className="content-container about-grid">
            <h2 className="section-title">
              {aboutTitle}
              <EditButton ids={["about.title"]} label="Rediger overskrift" />
            </h2>
            <div className="about-body">
              <RichText text={aboutBody} />
              <EditButton ids={["about.body"]} label="Rediger tekst om mig" className="edit-btn-block" />
            </div>
          </div>
        </section>

        <section id="billeder" className="gallery-section glass-sheet">
          <div className="content-container">
            <h2 className="section-title">
              {galleryTitle}
              <EditButton ids={["gallery.title"]} label="Rediger overskrift" />
            </h2>
            {(gallerySubtitle || isAdmin) && (
              <div className="section-subtitle">
                <RichText text={gallerySubtitle} />
                <EditButton ids={["gallery.subtitle"]} label="Rediger undertekst" />
              </div>
            )}
            {isAdmin && (
              <button type="button" className="gallery-edit-cta" onClick={editGallery}>
                <span className="edit-btn" aria-hidden="true">
                  <svg viewBox="0 0 24 24" width="14" height="14">
                    <path d="M12 5v14M5 12h14" stroke="currentColor" strokeWidth="3.2" strokeLinecap="round" />
                  </svg>
                </span>
                Rediger billeder – upload, slet og rækkefølge
              </button>
            )}
            <Gallery />
          </div>
        </section>

        <Footer />
      </div>
    </div>
  );
}

export default HomePage;
