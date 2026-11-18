import { useEffect, useId, useRef, useState } from "react";
import type { FormEvent } from "react";
import { errorMessage, submitContact } from "../../Api";
import { useText } from "../../texts/TextContext";
import { useServices } from "../../services/ServicesContext";
import EditButton from "../../admin/EditButton";
import { COOKIE_POLICY_PATH } from "../../consent/path";
import { THANKS_PATH } from "../../contact/path";
import "./ContactForm.css";

const MAX_FILES = 5;
const MAX_BYTES = 15 * 1024 * 1024;
const ACCEPT = ".jpg,.jpeg,.png,.webp,.heic,.heif,image/jpeg,image/png,image/webp,image/heic,image/heif";

function isAllowedImage(file: File) {
  const type = (file.type || "").toLowerCase();
  const ext = file.name.slice(file.name.lastIndexOf(".")).toLowerCase();
  if ([".jpg", ".jpeg", ".png", ".webp", ".heic", ".heif"].includes(ext)) return true;
  return ["image/jpeg", "image/png", "image/webp", "image/heic", "image/heif"].includes(type);
}

type ContactFormProps = {
  // På en ydelsesside er den ydelse valgt på forhånd
  preselectedSlug?: string;
  // I kontakt-boblen vises en kompakt udgave uden stor overskrift
  variant?: "page" | "widget";
};

function ContactForm({ preselectedSlug, variant = "page" }: ContactFormProps) {
  const titleId = useId();
  const filesId = useId();
  const { services } = useServices();
  const title = useText("contact.formTitle");
  const intro = useText("contact.formIntro");
  const orderIntro = useText("contact.orderIntro");
  const photoHelp = useText("contact.photoHelp");
  const gdpr = useText("contact.gdpr");
  const submitLabel = useText("contact.submit");
  const orderSubmit = useText("contact.orderSubmit");

  const [name, setName] = useState("");
  const [phone, setPhone] = useState("");
  const [email, setEmail] = useState("");
  const [service, setService] = useState(preselectedSlug ?? "");
  const [message, setMessage] = useState("");
  const [hpNote, setHpNote] = useState(""); // honeypot – meningsløst navn, så autofill ikke rammer det
  const startedAt = useRef(Date.now());
  const [files, setFiles] = useState<File[]>([]);
  const [error, setError] = useState("");
  const [sending, setSending] = useState(false);
  const compact = variant === "widget";

  useEffect(() => {
    setService(preselectedSlug ?? "");
  }, [preselectedSlug]);

  const onFiles = (list: FileList | null) => {
    if (!list) return;
    const next = [...files];
    for (const file of Array.from(list)) {
      if (!isAllowedImage(file)) {
        setError(`"${file.name}" er ikke et tilladt billedformat (jpg, png, webp eller heic).`);
        return;
      }
      if (file.size > MAX_BYTES) {
        setError(`"${file.name}" er større end 15 MB.`);
        return;
      }
      if (next.length >= MAX_FILES) {
        setError(`Du kan højst vedhæfte ${MAX_FILES} billeder.`);
        return;
      }
      next.push(file);
    }
    setError("");
    setFiles(next);
  };

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (sending) return;
    setError("");
    if (name.trim().length < 2) return setError("Skriv dit navn.");
    if (phone.trim().replace(/\D/g, "").length < 8) return setError("Skriv et telefonnummer, vi kan ringe til.");
    if (!service) return setError("Vælg et produkt, eller vælg Andet.");
    if (service !== "andet" && files.length < 1) return setError("Vedhæft mindst ét billede af cyklen fra siden.");
    if (!message.trim()) return setError("Skriv en kort besked om opgaven.");
    setSending(true);
    try {
      const form = new FormData();
      form.append("name", name.trim());
      form.append("phone", phone.trim());
      if (email.trim()) form.append("email", email.trim());
      form.append("service", service);
      form.append("message", message.trim());
      form.append("hp_note_x", hpNote);
      form.append("hp_t", String(startedAt.current));
      for (const file of files) form.append("files", file, file.name);
      await submitContact(form);
      window.location.assign(THANKS_PATH);
    } catch (err) {
      setError(errorMessage(err));
      setSending(false);
    }
  };

  const ordering = service !== "" && service !== "andet";
  const shownIntro = ordering ? orderIntro : intro;
  const shownSubmit = ordering ? orderSubmit : submitLabel;

  const edit = (
    <EditButton
      ids={["contact.formTitle", "contact.formIntro", "contact.orderIntro", "contact.photoHelp", "contact.gdpr", "contact.submit", "contact.orderSubmit"]}
      label="Rediger kontaktformularen"
    />
  );

  return (
    <section
      className={`contact-form-section${compact ? " is-widget" : ""}`}
      id={compact ? undefined : "kontakt"}
      aria-labelledby={titleId}
    >
      <div className={compact ? "contact-form-wrap" : "content-container"}>
        {compact ? (
          <p className="contact-form-intro" id={titleId}>
            {shownIntro}
            {edit}
          </p>
        ) : (
          <>
            <h2 className="section-title" id={titleId}>
              {ordering ? orderSubmit : title}
              {edit}
            </h2>
            {shownIntro && <p className="section-subtitle contact-form-intro">{shownIntro}</p>}
          </>
        )}

        <form className="contact-form" onSubmit={(e) => void onSubmit(e)} noValidate>
          <div className="contact-hp" aria-hidden="true">
            <input
              type="text"
              name="hp_note_x"
              value={hpNote}
              onChange={(e) => setHpNote(e.target.value)}
              tabIndex={-1}
              autoComplete="off"
              autoCorrect="off"
              autoCapitalize="off"
              spellCheck={false}
            />
          </div>

          <label className="contact-field">
            <span>Navn</span>
            <input type="text" name="name" autoComplete="name" required maxLength={120} value={name} onChange={(e) => setName(e.target.value)} />
          </label>
          <label className="contact-field">
            <span>Telefon</span>
            <input type="tel" name="phone" autoComplete="tel" required maxLength={40} value={phone} onChange={(e) => setPhone(e.target.value)} />
          </label>
          <label className="contact-field">
            <span>
              E-mail <em>(valgfri)</em>
            </span>
            <input type="email" name="email" autoComplete="email" maxLength={254} value={email} onChange={(e) => setEmail(e.target.value)} />
          </label>
          <label className="contact-field">
            <span>Produkt</span>
            <select name="service" required value={service} onChange={(e) => setService(e.target.value)}>
              <option value="" disabled>
                Vælg et produkt
              </option>
              {services.map((s) => (
                <option key={s.id} value={s.slug}>
                  {s.title}
                </option>
              ))}
              <option value="andet">Andet</option>
            </select>
          </label>
          <label className="contact-field contact-field-full">
            <span>Besked</span>
            <textarea name="message" required maxLength={4000} rows={compact ? 3 : 6} value={message} onChange={(e) => setMessage(e.target.value)} />
          </label>

          <div className={`contact-field contact-field-full${compact ? " contact-files-compact" : ""}`}>
            {ordering ? (
              <p className="contact-files-help">{photoHelp}</p>
            ) : compact ? null : (
              <p className="contact-files-help">Billeder er valgfrie. Jpg, png, webp eller heic – højst 15 MB pr. billede.</p>
            )}
            {compact ? (
              <div className="contact-files-row">
                <input
                  id={filesId}
                  className="contact-files-input"
                  type="file"
                  accept={ACCEPT}
                  multiple
                  onChange={(e) => {
                    onFiles(e.target.files);
                    e.target.value = "";
                  }}
                />
                <label htmlFor={filesId} className="contact-files-btn">
                  {ordering ? "Vedhæft billede af cyklen" : "Vedhæft billeder"}
                </label>
                <span className="contact-files-count">{files.length}/{MAX_FILES}</span>
              </div>
            ) : (
              <>
                <span className="contact-files-label">
                  {ordering ? `Billede af cyklen (påkrævet, højst ${MAX_FILES})` : `Billeder (valgfri, højst ${MAX_FILES})`}
                </span>
                <input
                  type="file"
                  accept={ACCEPT}
                  multiple
                  onChange={(e) => {
                    onFiles(e.target.files);
                    e.target.value = "";
                  }}
                />
              </>
            )}
            {files.length > 0 && (
              <ul className="contact-file-list">
                {files.map((file, i) => (
                  <li key={`${file.name}-${file.size}-${i}`}>
                    <span>{file.name}</span>
                    <button type="button" onClick={() => setFiles((all) => all.filter((_, j) => j !== i))} aria-label={`Fjern ${file.name}`}>
                      Fjern
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          {error && (
            <p className="contact-error" role="alert">
              {error}
            </p>
          )}

          <button type="submit" className="contact-submit" disabled={sending}>
            {sending ? "Sender…" : shownSubmit}
          </button>

          <p className="contact-gdpr">
            {gdpr} Læs mere i vores <a href={COOKIE_POLICY_PATH}>cookiepolitik</a>.
          </p>
        </form>
      </div>
    </section>
  );
}

export default ContactForm;
