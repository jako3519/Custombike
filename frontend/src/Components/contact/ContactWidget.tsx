import { useEffect, useId, useRef } from "react";
import "./ContactWidget.css";
import { useText } from "../../texts/TextContext";
import EditButton from "../../admin/EditButton";
import { useAdmin } from "../../admin/AdminContext";
import ContactForm from "./ContactForm";

const ChatIcon = () => (
  <svg viewBox="0 0 32 32" width="28" height="28" aria-hidden="true">
    <path
      fill="currentColor"
      d="M13 5C7.5 5 3 8.6 3 13c0 2.3 1.2 4.4 3.2 5.9-.2 1.6-.9 3-2.1 4.1 2.3 0 4.3-.8 5.8-2 1 .3 2 .4 3.1.4 5.5 0 10-3.6 10-8.4S18.5 5 13 5z"
    />
    <path
      fill="currentColor"
      opacity="0.85"
      d="M27.1 22.6c1.2-1.2 1.9-2.6 1.9-4.2 0-1.9-.9-3.6-2.4-4.9-.6 5.3-6 9.4-12.5 9.6 1.7 1.8 4.4 2.9 7.4 2.9.9 0 1.7-.1 2.5-.3 1.1.8 2.6 1.4 4.3 1.4-.8-.9-1.2-2.1-1.2-3.4z"
    />
  </svg>
);

const CloseIcon = () => (
  <svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true">
    <path d="M6 6l12 12M18 6L6 18" stroke="currentColor" strokeWidth="2.6" strokeLinecap="round" />
  </svg>
);

const MailIcon = () => (
  <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true">
    <path fill="currentColor" d="M2 6.5A2.5 2.5 0 0 1 4.5 4h15A2.5 2.5 0 0 1 22 6.5v.4l-10 6.2L2 6.9z" />
    <path fill="currentColor" d="M2 9.2l10 6.2 10-6.2v8.3a2.5 2.5 0 0 1-2.5 2.5h-15A2.5 2.5 0 0 1 2 17.5z" />
  </svg>
);

const PhoneIcon = () => (
  <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true">
    <path
      fill="currentColor"
      d="M6.62 10.79a15.05 15.05 0 0 0 6.59 6.59l2.2-2.2a1 1 0 0 1 1.02-.24c1.12.37 2.33.57 3.57.57a1 1 0 0 1 1 1V20a1 1 0 0 1-1 1A17 17 0 0 1 3 4a1 1 0 0 1 1-1h3.5a1 1 0 0 1 1 1c0 1.25.2 2.45.57 3.57a1 1 0 0 1-.25 1.02l-2.2 2.2Z"
    />
  </svg>
);

const FacebookIcon = () => (
  <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true">
    <path
      fill="currentColor"
      d="M14.5 8.5V6.8c0-.7.5-1 1.2-1H17V3.2h-2.1C12.3 3.2 11 4.6 11 6.7v1.8H9v2.7h2V21h3.2v-9.8h2.2l.3-2.7z"
    />
  </svg>
);

const FOCUSABLE = "a[href], button:not([disabled]), input:not([disabled]):not([type=hidden]), select:not([disabled]), textarea:not([disabled])";

type ContactWidgetProps = {
  open: boolean;
  onToggle: (open: boolean) => void;
  preselectedSlug?: string;
};

function ContactWidget({ open, onToggle, preselectedSlug }: ContactWidgetProps) {
  const titleId = useId();
  const dialogId = useId();
  const title = useText("contact.title");
  const email = useText("contact.email").trim();
  const phone = useText("contact.phone").trim();
  const facebookLabel = useText("contact.facebookLabel").trim() || "Facebook";
  const facebookUrl = useText("contact.facebookUrl").trim();
  const wrapperRef = useRef<HTMLDivElement>(null);
  const closeRef = useRef<HTMLButtonElement>(null);
  const bubbleRef = useRef<HTMLButtonElement>(null);
  const wasOpen = useRef(false);
  const { isAdmin } = useAdmin();

  const showEmail = Boolean(email) || isAdmin;
  const showPhone = Boolean(phone) || isAdmin;
  const showFacebook = Boolean(facebookUrl) || isAdmin;

  useEffect(() => {
    if (!open) {
      if (wasOpen.current) bubbleRef.current?.focus();
      wasOpen.current = false;
      return;
    }
    wasOpen.current = true;
    const t = window.setTimeout(() => closeRef.current?.focus(), 0);
    const prevOverflow = document.body.style.overflow;
    if (window.matchMedia("(max-width: 640px)").matches) document.body.style.overflow = "hidden";

    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        e.preventDefault();
        onToggle(false);
        return;
      }
      if (e.key !== "Tab" || !wrapperRef.current) return;
      const items = [...wrapperRef.current.querySelectorAll<HTMLElement>(FOCUSABLE)].filter((el) => el.tabIndex !== -1);
      if (items.length === 0) return;
      const first = items[0];
      const last = items[items.length - 1];
      if (e.shiftKey && document.activeElement === first) {
        e.preventDefault();
        last.focus();
      } else if (!e.shiftKey && document.activeElement === last) {
        e.preventDefault();
        first.focus();
      }
    };

    const onPointer = (e: PointerEvent) => {
      const target = e.target as Element | null;
      if (target?.closest?.("[data-admin-modal]")) return;
      if (wrapperRef.current && target && !wrapperRef.current.contains(target)) onToggle(false);
    };

    document.addEventListener("keydown", onKey);
    document.addEventListener("pointerdown", onPointer);
    return () => {
      window.clearTimeout(t);
      document.body.style.overflow = prevOverflow;
      document.removeEventListener("keydown", onKey);
      document.removeEventListener("pointerdown", onPointer);
    };
  }, [open, onToggle]);

  return (
    <div className={`contact-widget${open ? " is-open" : ""}`} ref={wrapperRef}>
      {open && (
        <div
          className="contact-popup"
          role="dialog"
          id={dialogId}
          aria-modal="true"
          aria-labelledby={titleId}
        >
          <div className="contact-header">
            <span className="contact-title" id={titleId}>
              {title}
              <EditButton ids={["contact.title"]} label="Rediger overskrift" />
            </span>
            <div className="contact-quick">
              {showEmail && (
                <span className="contact-quick-item">
                  {email ? (
                    <a href={`mailto:${email}`} className="contact-quick-btn" aria-label={`Send e-mail til ${email}`}>
                      <MailIcon />
                    </a>
                  ) : (
                    <span className="contact-quick-btn is-missing" aria-label="E-mail er ikke sat">
                      <MailIcon />
                    </span>
                  )}
                  <EditButton ids={["contact.email"]} label="Rediger e-mail" />
                </span>
              )}
              {showPhone && (
                <span className="contact-quick-item">
                  {phone ? (
                    <a href={`tel:${phone.replace(/\s+/g, "")}`} className="contact-quick-btn" aria-label={`Ring til ${phone}`}>
                      <PhoneIcon />
                    </a>
                  ) : (
                    <span className="contact-quick-btn is-missing" aria-label="Telefonnummer er ikke sat">
                      <PhoneIcon />
                    </span>
                  )}
                  <EditButton ids={["contact.phone"]} label="Rediger telefonnummer" />
                </span>
              )}
              {showFacebook && (
                <span className="contact-quick-item">
                  {facebookUrl ? (
                    <a
                      href={facebookUrl}
                      className="contact-quick-btn"
                      target="_blank"
                      rel="noopener noreferrer"
                      aria-label={`${facebookLabel} (åbner i nyt vindue)`}
                    >
                      <FacebookIcon />
                    </a>
                  ) : (
                    <span className="contact-quick-btn is-missing" aria-label={`${facebookLabel} er ikke sat`}>
                      <FacebookIcon />
                    </span>
                  )}
                  <EditButton ids={["contact.facebookLabel", "contact.facebookUrl"]} label="Rediger Facebook" />
                </span>
              )}
            </div>
            <button
              ref={closeRef}
              type="button"
              className="contact-close"
              onClick={() => onToggle(false)}
              aria-label="Luk kontakt"
            >
              <CloseIcon />
            </button>
          </div>

          <div className="contact-widget-form">
            <ContactForm variant="widget" preselectedSlug={preselectedSlug} />
          </div>
        </div>
      )}

      <button
        ref={bubbleRef}
        type="button"
        className="contact-bubble"
        onClick={() => onToggle(!open)}
        aria-label={open ? "Luk kontakt" : "Åbn kontakt"}
        aria-expanded={open}
        aria-controls={open ? dialogId : undefined}
        aria-haspopup="dialog"
      >
        {open ? <CloseIcon /> : <ChatIcon />}
      </button>
    </div>
  );
}

export default ContactWidget;
