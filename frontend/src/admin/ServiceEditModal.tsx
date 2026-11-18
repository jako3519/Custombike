import { useEffect, useRef, useState } from "react";
import type { FormEvent } from "react";
import Modal from "./Modal";
import "./ServiceAdmin.css";
import {
  addServiceYouTube,
  assetUrl,
  createService,
  deleteService,
  deleteServiceCover,
  deleteServiceMedia,
  errorMessage,
  reorderServiceMedia,
  updateService,
  uploadServiceCover,
  uploadServiceMedia,
} from "../Api";
import { useServices } from "../services/ServicesContext";
import { SERVICE_ICONS } from "../services/icons";
import ServiceIcon from "../services/ServiceIcon";
import { slugify } from "../services/slug";
import { MediaAdder, MediaThumb } from "./MediaUpload";
import { IMAGE_TYPES, checkFile, kindLabel } from "./mediaRules";

type ServiceEditModalProps = {
  id: number | null; // null = ny ydelse
  focusMedia?: boolean;
  onClose: () => void;
  onCreated: (id: number) => void;
};

function ServiceEditModal({ id, focusMedia, onClose, onCreated }: ServiceEditModalProps) {
  const { services, setServices, reload } = useServices();
  const existing = id !== null ? services.find((s) => s.id === id) : undefined;
  const isNew = id === null;

  const [title, setTitle] = useState(existing?.title ?? "");
  const [slug, setSlug] = useState(existing?.slug ?? "");
  const [slugTouched, setSlugTouched] = useState(!isNew);
  const [icon, setIcon] = useState(existing?.icon ?? SERVICE_ICONS[0].key);
  const [shortText, setShortText] = useState(existing?.shortText ?? "");
  const [body, setBody] = useState(existing?.body ?? "");

  const [busy, setBusy] = useState(false);
  const [progress, setProgress] = useState<number | null>(null);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [confirmDelete, setConfirmDelete] = useState<number | "service" | null>(null);

  const coverInput = useRef<HTMLInputElement>(null);
  const mediaSection = useRef<HTMLElement>(null);

  const shownSlug = slugTouched ? slugify(slug) : slugify(title);

  useEffect(() => {
    if (focusMedia) mediaSection.current?.scrollIntoView({ block: "start" });
  }, [focusMedia]);

  // true, hvis det lykkedes
  const run = async (action: () => Promise<void>): Promise<boolean> => {
    setBusy(true);
    setError("");
    setMessage("");
    try {
      await action();
      return true;
    } catch (err) {
      setError(errorMessage(err));
      await reload();
      return false;
    } finally {
      setBusy(false);
      setProgress(null);
    }
  };

  // --- Navn, adresse, ikon og tekster ---
  const save = (e: FormEvent) => {
    e.preventDefault();
    if (!title.trim()) {
      setError("Produktet skal have et navn.");
      return;
    }
    const input = {
      title: title.trim(),
      // Ved redigering sendes adressen kun, hvis den er ændret (så gamle links ikke går i stykker)
      slug: isNew ? (slugTouched ? shownSlug : "") : shownSlug !== existing?.slug ? shownSlug : "",
      shortText,
      body,
      icon,
    };
    void run(async () => {
      if (isNew) {
        const res = await createService(input);
        await reload();
        onCreated(res.data.id); // fortsæt med at redigere den nye ydelse (forsidebillede, billeder, video)
        return;
      }
      const oldSlug = existing!.slug;
      const res = await updateService(id!, input);
      setServices(res.data);
      const updated = res.data.find((s) => s.id === id);
      setMessage("Gemt.");
      // Står man på ydelsens side, og er adressen ændret: gå til den nye adresse
      if (updated && updated.slug !== oldSlug && window.location.pathname === `/produkter/${oldSlug}`) {
        window.location.replace(`/produkter/${updated.slug}`);
      }
    });
  };

  // --- Forsidebillede ---
  const uploadCover = (file: File | undefined) => {
    if (!file || id === null) return;
    const problem = checkFile(file, false);
    if (problem) {
      setError(problem);
      return;
    }
    void run(async () => {
      const res = await uploadServiceCover(id, file, setProgress);
      setServices(res.data);
    });
  };

  const removeCover = () => {
    if (id === null) return;
    void run(async () => {
      const res = await deleteServiceCover(id);
      setServices(res.data);
    });
  };

  // --- Billeder og video ---
  const uploadMedia = (files: File[]) => {
    if (id === null) return;
    void run(async () => {
      const res = await uploadServiceMedia(id, files, setProgress);
      setServices(res.data);
    });
  };

  const addYouTube = (url: string) =>
    id === null
      ? Promise.resolve(false)
      : run(async () => {
          const res = await addServiceYouTube(id, url);
          setServices(res.data);
        });

  const moveMedia = (index: number, dir: -1 | 1) => {
    if (!existing) return;
    const target = index + dir;
    if (target < 0 || target >= existing.media.length) return;
    const next = [...existing.media];
    [next[index], next[target]] = [next[target], next[index]];
    setServices(services.map((s) => (s.id === existing.id ? { ...s, media: next } : s))); // vis med det samme
    void run(async () => {
      const res = await reorderServiceMedia(existing.id, next.map((m) => m.id));
      setServices(res.data);
    });
  };

  const removeMedia = (mediaId: number) => {
    if (id === null) return;
    setConfirmDelete(null);
    void run(async () => {
      const res = await deleteServiceMedia(id, mediaId);
      setServices(res.data);
    });
  };

  const removeService = () => {
    if (!existing) return;
    setConfirmDelete(null);
    void run(async () => {
      const res = await deleteService(existing.id);
      setServices(res.data);
      if (window.location.pathname === `/produkter/${existing.slug}`) {
        window.location.assign("/#produkter");
        return;
      }
      onClose();
    });
  };

  const markupHint = (
    <span className="admin-field-help">
      Skriv <code>## Overskrift ##</code> på en linje for sig for at lave en overskrift. En tom linje giver et nyt afsnit.
    </span>
  );

  return (
    <Modal title={isNew ? "Nyt produkt" : `Rediger produkt: ${existing?.title ?? ""}`} onClose={onClose} wide>
      {!isNew && !existing ? (
        <p className="admin-empty">Produktet findes ikke længere.</p>
      ) : (
        <form onSubmit={save} className="admin-form">
          <label className="admin-field">
            <span className="admin-field-label">Navn på produktet</span>
            <input type="text" value={title} maxLength={120} onChange={(e) => setTitle(e.target.value)} placeholder="Fx Garmin Varia-holder" />
          </label>

          <label className="admin-field">
            <span className="admin-field-label">Adresse på siden</span>
            <span className="svc-slug">
              <span className="svc-slug-prefix">/produkter/</span>
              <input
                type="text"
                value={slugTouched ? slug : shownSlug}
                maxLength={80}
                onChange={(e) => {
                  setSlugTouched(true);
                  setSlug(e.target.value);
                }}
                onBlur={() => setSlug(shownSlug)}
              />
            </span>
            <span className="admin-field-help">
              Laves automatisk ud fra navnet (æ → ae, ø → oe, å → aa). Ændrer du den senere, virker gamle links ikke længere.
            </span>
          </label>

          <fieldset className="admin-field svc-icons">
            <legend className="admin-field-label">Ikon på forsiden og i menuen</legend>
            <div className="svc-icon-grid" role="radiogroup" aria-label="Ikon">
              {SERVICE_ICONS.map((i) => (
                <button
                  key={i.key}
                  type="button"
                  role="radio"
                  aria-checked={icon === i.key}
                  className={`svc-icon-option${icon === i.key ? " selected" : ""}`}
                  onClick={() => setIcon(i.key)}
                >
                  <span className="svc-icon-preview">
                    <ServiceIcon icon={i.key} size={28} />
                  </span>
                  <span>{i.label}</span>
                </button>
              ))}
            </div>
          </fieldset>

          <label className="admin-field">
            <span className="admin-field-label">Tekst på kortet på forsiden</span>
            <textarea rows={4} value={shortText} maxLength={2000} onChange={(e) => setShortText(e.target.value)} />
            {markupHint}
          </label>

          <label className="admin-field">
            <span className="admin-field-label">Tekst på produktets egen side</span>
            <textarea rows={12} value={body} maxLength={20000} onChange={(e) => setBody(e.target.value)} />
            {markupHint}
          </label>

          {error && <div className="admin-error">{error}</div>}
          {message && <div className="admin-status">{message}</div>}

          <div className="admin-actions">
            <button type="button" className="admin-btn secondary" onClick={onClose}>
              {isNew ? "Annuller" : "Luk"}
            </button>
            <button type="submit" className="admin-btn" disabled={busy}>
              {busy ? "Gemmer…" : isNew ? "Opret produkt" : "Gem tekster"}
            </button>
          </div>
        </form>
      )}

      {isNew && <p className="admin-hint">Når produktet er oprettet, kan du tilføje forsidebillede, billeder og video.</p>}

      {existing && (
        <>
          <section className="svc-section">
            <h3 className="svc-section-title">Forsidebillede</h3>
            <p className="admin-hint">Vises på kortet på forsiden (med navnet oven på) og øverst på produktets side. Uden billede vises ikonet.</p>
            <div className="svc-cover">
              {existing.coverUrl ? (
                <img src={assetUrl(existing.coverUrl)} alt="Forsidebillede" />
              ) : (
                <span className="svc-cover-empty">
                  <ServiceIcon icon={existing.icon} size={34} />
                  Intet billede
                </span>
              )}
              <div className="svc-cover-actions">
                <button type="button" className="admin-btn" onClick={() => coverInput.current?.click()} disabled={busy}>
                  {existing.coverUrl ? "Skift billede" : "Vælg billede"}
                </button>
                {existing.coverUrl && (
                  <button type="button" className="admin-btn secondary" onClick={removeCover} disabled={busy}>
                    Fjern billede
                  </button>
                )}
              </div>
              <input
                ref={coverInput}
                type="file"
                accept={IMAGE_TYPES.join(",")}
                hidden
                onChange={(e) => {
                  uploadCover(e.target.files?.[0]);
                  e.target.value = "";
                }}
              />
            </div>
          </section>

          <section className="svc-section" ref={mediaSection}>
            <h3 className="svc-section-title">Billeder og video på produktets side</h3>

            <MediaAdder busy={busy} progress={progress} onFiles={uploadMedia} onYouTube={addYouTube} onError={setError} />
            {error && <div className="admin-error">{error}</div>}

            {existing.media.length === 0 ? (
              <p className="admin-empty">Der er ingen billeder eller videoer endnu.</p>
            ) : (
              <ul className="admin-gallery">
                {existing.media.map((m, index) => (
                  <li key={m.id} className="admin-gallery-item">
                    <div className="admin-thumb">
                      <MediaThumb media={m} index={index} />
                      <span className="admin-thumb-number">{index + 1}</span>
                      {kindLabel(m) && <span className="admin-thumb-hero">{kindLabel(m)}</span>}
                    </div>
                    <div className="admin-thumb-actions">
                      <button type="button" onClick={() => moveMedia(index, -1)} disabled={busy || index === 0} aria-label="Flyt til venstre" title="Flyt til venstre">
                        ←
                      </button>
                      <button type="button" onClick={() => moveMedia(index, 1)} disabled={busy || index === existing.media.length - 1} aria-label="Flyt til højre" title="Flyt til højre">
                        →
                      </button>
                      {confirmDelete === m.id ? (
                        <button type="button" className="danger confirm" onClick={() => removeMedia(m.id)} disabled={busy}>
                          Slet?
                        </button>
                      ) : (
                        <button type="button" className="danger" onClick={() => setConfirmDelete(m.id)} disabled={busy} aria-label="Slet" title="Slet">
                          <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true">
                            <path d="M4 7h16M9 7V4h6v3M6 7l1 13h10l1-13" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                          </svg>
                        </button>
                      )}
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </section>

          <section className="svc-section svc-danger">
            {confirmDelete === "service" ? (
              <>
                <span>Slet "{existing.title}" med alle billeder og videoer? Det kan ikke fortrydes.</span>
                <button type="button" className="admin-btn secondary" onClick={() => setConfirmDelete(null)} disabled={busy}>
                  Nej
                </button>
                <button type="button" className="admin-btn svc-delete" onClick={removeService} disabled={busy}>
                  Ja, slet produktet
                </button>
              </>
            ) : (
              <button type="button" className="admin-btn secondary svc-delete-link" onClick={() => setConfirmDelete("service")} disabled={busy}>
                Slet produktet
              </button>
            )}
          </section>
        </>
      )}
    </Modal>
  );
}

export default ServiceEditModal;
