import { useEffect, useRef, useState } from "react";
import "./Navbar.css";
import { useText } from "../../texts/TextContext";
import EditButton from "../../admin/EditButton";
import { useServices } from "../../services/ServicesContext";
import ServiceIcon from "../../services/ServiceIcon";
import logo from "../../assets/logo.png";

type NavbarProps = {
  onContactClick: () => void;
  // Ydelsen man står på (markeres i menuen)
  currentSlug?: string;
};

function Navbar({ onContactClick, currentSlug }: NavbarProps) {
  const [scrolled, setScrolled] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false); // mobilmenuen
  const [servicesOpen, setServicesOpen] = useState(false); // "Ydelser"-dropdown (klik/tastatur)
  const headerRef = useRef<HTMLElement>(null);
  const { services } = useServices();
  const companyName = useText("company.name");
  const servicesLabel = useText("nav.services");
  const aboutLabel = useText("nav.about");
  const galleryLabel = useText("nav.gallery");
  const contactLabel = useText("nav.contact");
  const phone = useText("contact.phone");

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 40);
    onScroll();
    window.addEventListener("scroll", onScroll, { passive: true });
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  // Menuerne lukkes med Escape og ved klik udenfor
  useEffect(() => {
    if (!menuOpen && !servicesOpen) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        setMenuOpen(false);
        setServicesOpen(false);
      }
    };
    const onPointer = (e: PointerEvent) => {
      if (headerRef.current && !headerRef.current.contains(e.target as Node)) {
        setMenuOpen(false);
        setServicesOpen(false);
      }
    };
    document.addEventListener("keydown", onKey);
    document.addEventListener("pointerdown", onPointer);
    return () => {
      document.removeEventListener("keydown", onKey);
      document.removeEventListener("pointerdown", onPointer);
    };
  }, [menuOpen, servicesOpen]);

  // Et klik på et link i menuen lukker den (også når linket kun scroller på samme side)
  const closeMenus = () => {
    setMenuOpen(false);
    setServicesOpen(false);
  };

  return (
    <header
      ref={headerRef}
      className={`header-container${scrolled ? " scrolled" : ""}${menuOpen ? " menu-open" : ""}`}
    >
      <nav className="top-navbar" aria-label="Hovedmenu">
        <span className="nav-item nav-brand">
          <a href="/#top" className="nav-logo" aria-label={`${companyName} – til forsiden`} onClick={closeMenus}>
          <img className="nav-logo-mark" src={logo} alt="" />
          <span>{companyName}</span>
          </a>
          <EditButton ids={["company.name"]} label="Rediger firmanavn" />
        </span>

        <div className={`nav-links${menuOpen ? " open" : ""}`} id="nav-menu">
          {/* "Ydelser" scroller til sektionen; pilen (eller hover) viser alle ydelser.
              Linkene til ydelserne ligger altid i HTML'en (også når menuen er lukket). */}
          <div className={`nav-item nav-dropdown${servicesOpen ? " open" : ""}`}>
            <a href="/#produkter" onClick={closeMenus}>
              {servicesLabel}
            </a>
            {services.length > 0 && (
              <button
                type="button"
                className="nav-dropdown-toggle"
                aria-expanded={servicesOpen}
                aria-controls="nav-services"
                aria-label={`Vis alle ${servicesLabel.toLowerCase()}`}
                onClick={() => setServicesOpen((o) => !o)}
              >
                <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true">
                  <path d="M6 9l6 6 6-6" fill="none" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" strokeLinejoin="round" />
                </svg>
              </button>
            )}
            <EditButton ids={["nav.services"]} label="Rediger menupunkt" />
            {services.length > 0 && (
              <ul className="nav-dropdown-menu" id="nav-services">
                {services.map((s) => (
                  <li key={s.id}>
                    <a
                      href={`/produkter/${s.slug}`}
                      aria-current={s.slug === currentSlug ? "page" : undefined}
                      onClick={closeMenus}
                    >
                      <span className="nav-dropdown-icon" aria-hidden="true">
                        <ServiceIcon icon={s.icon} size={18} />
                      </span>
                      {s.title}
                    </a>
                  </li>
                ))}
              </ul>
            )}
          </div>
          <span className="nav-item">
            <a href="/#om-mig" onClick={closeMenus}>{aboutLabel}</a>
            <EditButton ids={["nav.about"]} label="Rediger menupunkt" />
          </span>
          <span className="nav-item">
            <a href="/#billeder" onClick={closeMenus}>{galleryLabel}</a>
            <EditButton ids={["nav.gallery"]} label="Rediger menupunkt" />
          </span>
        </div>

        {phone.trim() && (
          <span className="nav-item">
            <a
              className="nav-phone"
              href={`tel:${phone.replace(/\s+/g, "")}`}
              aria-label={`Ring ${phone}`}
            >
              <span className="nav-phone-icon" aria-hidden="true">
                <svg viewBox="0 0 24 24" width="16" height="16" fill="currentColor">
                  <path d="M6.62 10.79a15.05 15.05 0 0 0 6.59 6.59l2.2-2.2a1 1 0 0 1 1.02-.24c1.12.37 2.33.57 3.57.57a1 1 0 0 1 1 1V20a1 1 0 0 1-1 1A17 17 0 0 1 3 4a1 1 0 0 1 1-1h3.5a1 1 0 0 1 1 1c0 1.25.2 2.45.57 3.57a1 1 0 0 1-.25 1.02l-2.2 2.2Z" />
                </svg>
              </span>
              <span className="nav-phone-number">{phone}</span>
            </a>
            <EditButton ids={["contact.phone"]} label="Rediger telefonnummer" />
          </span>
        )}

        <span className="nav-item">
          <button
            type="button"
            className="contact-button"
            onClick={() => {
              closeMenus();
              onContactClick();
            }}
          >
            {contactLabel}
          </button>
          <EditButton ids={["nav.contact"]} label="Rediger knaptekst" />
        </span>

        {/* Kun på mobil: åbner menuen med Ydelser, Om mig og Billeder */}
        <button
          type="button"
          className="nav-menu-toggle"
          aria-expanded={menuOpen}
          aria-controls="nav-menu"
          aria-label={menuOpen ? "Luk menu" : "Åbn menu"}
          onClick={() => setMenuOpen((o) => !o)}
        >
          <svg viewBox="0 0 24 24" width="22" height="22" aria-hidden="true">
            {menuOpen ? (
              <path d="M6 6l12 12M18 6L6 18" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" />
            ) : (
              <path d="M4 7h16M4 12h16M4 17h16" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" />
            )}
          </svg>
        </button>
      </nav>
    </header>
  );
}

export default Navbar;
