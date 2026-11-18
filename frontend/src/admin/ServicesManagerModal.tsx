import { useState } from "react";
import Modal from "./Modal";
import "./ServiceAdmin.css";
import { deleteService, errorMessage, reorderServices } from "../Api";
import type { Service } from "../Api";
import { useServices } from "../services/ServicesContext";
import ServiceIcon from "../services/ServiceIcon";

type ServicesManagerModalProps = {
  onClose: () => void;
  // Åbn redigering af en ydelse (null = ny ydelse)
  onEdit: (id: number | null) => void;
};

// Liste over alle ydelser: rækkefølge, slet, rediger og opret ny
function ServicesManagerModal({ onClose, onEdit }: ServicesManagerModalProps) {
  const { services, setServices, reload } = useServices();
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [confirmDelete, setConfirmDelete] = useState<number | null>(null);

  const run = async (action: () => Promise<void>) => {
    setBusy(true);
    setError("");
    try {
      await action();
    } catch (err) {
      setError(errorMessage(err));
      await reload();
    } finally {
      setBusy(false);
    }
  };

  const move = (index: number, dir: -1 | 1) => {
    const target = index + dir;
    if (target < 0 || target >= services.length) return;
    const next: Service[] = [...services];
    [next[index], next[target]] = [next[target], next[index]];
    setServices(next); // vis med det samme
    void run(async () => {
      const res = await reorderServices(next.map((s) => s.id));
      setServices(res.data);
    });
  };

  const remove = (id: number) => {
    setConfirmDelete(null);
    void run(async () => {
      const res = await deleteService(id);
      setServices(res.data);
    });
  };

  return (
    <Modal title="Rediger produkter" onClose={onClose} wide>
      <p className="admin-hint">
        Produkterne vises som kort på forsiden og i menuen under "Produkter". Hvert produkt har sin egen side med tekst, billeder og video.
      </p>

      {error && <div className="admin-error">{error}</div>}
      {busy && <div className="admin-status">Gemmer…</div>}

      {services.length === 0 ? (
        <p className="admin-empty">Der er ingen produkter endnu.</p>
      ) : (
        <ul className="svc-list">
          {services.map((s, index) => (
            <li key={s.id} className="svc-row">
              <span className="svc-row-icon" aria-hidden="true">
                <ServiceIcon icon={s.icon} size={22} />
              </span>
              <span className="svc-row-main">
                <strong>{s.title}</strong>
                <a href={`/produkter/${s.slug}`} className="svc-row-url">
                  /produkter/{s.slug}
                </a>
              </span>
              <span className="svc-row-meta">
                {s.media.length} {s.media.length === 1 ? "billede/video" : "billeder/videoer"}
              </span>
              <span className="admin-thumb-actions svc-row-actions">
                <button type="button" onClick={() => move(index, -1)} disabled={busy || index === 0} aria-label="Flyt op" title="Flyt op">
                  ↑
                </button>
                <button type="button" onClick={() => move(index, 1)} disabled={busy || index === services.length - 1} aria-label="Flyt ned" title="Flyt ned">
                  ↓
                </button>
                <button type="button" className="svc-edit" onClick={() => onEdit(s.id)} disabled={busy}>
                  Rediger
                </button>
                {confirmDelete === s.id ? (
                  <button type="button" className="danger confirm" onClick={() => remove(s.id)} disabled={busy}>
                    Slet?
                  </button>
                ) : (
                  <button type="button" className="danger" onClick={() => setConfirmDelete(s.id)} disabled={busy} aria-label={`Slet ${s.title}`} title="Slet produkt">
                    <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true">
                      <path d="M4 7h16M9 7V4h6v3M6 7l1 13h10l1-13" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                    </svg>
                  </button>
                )}
              </span>
            </li>
          ))}
        </ul>
      )}

      <div className="admin-actions">
        <button type="button" className="admin-btn secondary" onClick={() => onEdit(null)} disabled={busy}>
          + Nyt produkt
        </button>
        <button type="button" className="admin-btn" onClick={onClose}>
          Færdig
        </button>
      </div>
    </Modal>
  );
}

export default ServicesManagerModal;
