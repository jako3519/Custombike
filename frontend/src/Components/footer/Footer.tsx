import "./Footer.css";
import { useText } from "../../texts/TextContext";
import RichText from "../../texts/RichText";
import EditButton from "../../admin/EditButton";
import { useAdmin } from "../../admin/AdminContext";
import { openConsentSettings } from "../../consent";
import { COOKIE_POLICY_PATH } from "../../consent/path";

// Bunden af siden (bruges på forsiden og på ydelsernes sider)
function Footer() {
  const { isAdmin } = useAdmin();
  const companyName = useText("company.name");
  const footerText = useText("footer.text");
  const phone = useText("contact.phone");
  const email = useText("contact.email");
  const legalName = useText("company.legalName");
  const address = useText("company.address");
  const cvr = useText("company.cvr");
  const footerIntro = useText("company.footerText");

  return (
    <footer className="site-footer">
      <div className="content-container footer-inner">
        <div className="footer-main">
          {(footerIntro || isAdmin) && (
            <div className="footer-intro">
              <RichText text={footerIntro} headingLevel={3} />
              <EditButton ids={["company.footerText"]} label="Rediger tekst om firmaet" />
            </div>
          )}
          {/* Virksomhedsoplysninger (fra CVR-registret) - alle kan rettes i admin */}
          <address className="footer-company">
            <span className="footer-name">{companyName}</span>
            {(legalName || isAdmin) && (
              <span>
                {legalName}
                <EditButton ids={["company.legalName"]} label="Rediger juridisk navn" />
              </span>
            )}
            {(address || isAdmin) && (
              <span>
                {address}
                <EditButton ids={["company.address"]} label="Rediger adresse" />
              </span>
            )}
            {(cvr || isAdmin) && (
              <span>
                CVR: {cvr}
                <EditButton ids={["company.cvr"]} label="Rediger CVR-nummer" />
              </span>
            )}
            {(phone || isAdmin) && (
              <span>
                Tlf.: <a href={`tel:${phone.replace(/\s+/g, "")}`}>{phone}</a>
                <EditButton ids={["contact.phone"]} label="Rediger telefonnummer" />
              </span>
            )}
            {(email || isAdmin) && (
              <span>
                Mail: <a href={`mailto:${email}`}>{email}</a>
                <EditButton ids={["contact.email"]} label="Rediger e-mail" />
              </span>
            )}
          </address>
        </div>
      </div>

      <div className="content-container footer-bottom">
        {/* Hvem der har lavet hjemmesiden (fast tekst - ikke en af firmaets tekster) */}
        <p className="footer-credit">
          Hjemmesiden er lavet af IT Service v/Jakob Lund · CVR 42637688 · Mobil{" "}
          <a href="tel:+4540262664">40262664</a> · <a href="mailto:jako3519@gmail.com">jako3519@gmail.com</a>
        </p>
        <p className="footer-legal">
          <a href={COOKIE_POLICY_PATH}>Cookiepolitik</a>
          <button type="button" className="footer-cookie-settings" onClick={openConsentSettings}>
            Cookie-indstillinger
          </button>
        </p>
        <span className="footer-copy">
          © {new Date().getFullYear()} · {footerText}
          <EditButton ids={["footer.text"]} label="Rediger footer" />
        </span>
      </div>
    </footer>
  );
}

export default Footer;
