import { useCallback, useEffect, useState } from "react";
import type { DragEvent } from "react";
import Modal from "./Modal";
import {
  addGalleryYouTube,
  assetUrl,
  deleteImage,
  errorMessage,
  getGalleryStorage,
  reorderImages,
  updateText,
  uploadGalleryPoster,
  uploadImages,
} from "../Api";
import type { GalleryImage, Storage } from "../Api";
import { MediaAdder, MediaThumb } from "./MediaUpload";
import { kindLabel } from "./mediaRules";
import { probeVideo } from "./videoProbe";

// Over dette advares (men det er tilladt): baggrundsvideoen kan gøre forsiden langsom at åbne
const WARN_BYTES = 15 * 1024 * 1024;
const WARN_SECONDS = 20;
import { useGallery } from "../gallery/GalleryContext";
import { useTextStore } from "../texts/TextContext";

const MB = 1024 * 1024;
const formatMb = (bytes: number) => (bytes >= 1024 * MB ? `${(bytes / (1024 * MB)).toFixed(1).replace(".", ",")} GB` : `${Math.round(bytes / MB)} MB`);

const heroTitle = (kind: GalleryImage["kind"], isHero: boolean) =>
  kind === "video"
    ? isHero ? "Fjern som baggrundsvideo på forsiden" : "Brug som baggrundsvideo på forsiden"
    : isHero ? "Fjern som forsidebillede" : "Brug som forsidebillede";

type GalleryEditModalProps = {
  onClose: () => void;
};

function GalleryEditModal({ onClose }: GalleryEditModalProps) {
  const { images, setImages, reload } = useGallery();
  const { texts, setText } = useTextStore();
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [confirmDelete, setConfirmDelete] = useState<number | null>(null);
  const [dragId, setDragId] = useState<number | null>(null);
  const [progress, setProgress] = useState<number | null>(null);
  const [storage, setStorage] = useState<Storage | null>(null);
  const [warning, setWarning] = useState("");

  const heroUrl = texts["hero.image"] ?? "";

  // Plads på serveren (volumen er lille, så den vises her)
  const loadStorage = useCallback(() => {
    getGalleryStorage()
      .then((res) => setStorage(res.data))
      .catch(() => setStorage(null));
  }, []);
  useEffect(loadStorage, [loadStorage]);

  // true, hvis det lykkedes
  const run = async (action: () => Promise<void>): Promise<boolean> => {
    setBusy(true);
    setError("");
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
      loadStorage();
    }
  };

  // --- Upload og YouTube ---
  const upload = (files: File[]) => {
    void run(async () => {
      const res = await uploadImages(files, setProgress);
      setImages(res.data);
    });
  };

  const addYouTube = (url: string) =>
    run(async () => {
      const res = await addGalleryYouTube(url);
      setImages(res.data);
    });

  // --- Rækkefølge ---
  const saveOrder = (next: GalleryImage[]) => {
    setImages(next); // vis med det samme
    void run(async () => {
      const res = await reorderImages(next.map((i) => i.id));
      setImages(res.data);
    });
  };

  const move = (index: number, dir: -1 | 1) => {
    const target = index + dir;
    if (target < 0 || target >= images.length) return;
    const next = [...images];
    [next[index], next[target]] = [next[target], next[index]];
    saveOrder(next);
  };

  const onDragOverItem = (e: DragEvent, overId: number) => {
    if (dragId === null || dragId === overId) return;
    e.preventDefault();
  };

  const onDropItem = (e: DragEvent, overId: number) => {
    e.preventDefault();
    if (dragId === null || dragId === overId) return;
    const next = [...images];
    const from = next.findIndex((i) => i.id === dragId);
    const to = next.findIndex((i) => i.id === overId);
    const [moved] = next.splice(from, 1);
    next.splice(to, 0, moved);
    setDragId(null);
    saveOrder(next);
  };

  // --- Slet ---
  const remove = (id: number) => {
    const removed = images.find((i) => i.id === id);
    setConfirmDelete(null);
    void run(async () => {
      const res = await deleteImage(id);
      setImages(res.data);
      if (removed?.url && removed.url === heroUrl) setText("hero.image", "");
    });
  };

  // --- Forsidebillede ---
  // --- Baggrund på forsiden: et billede eller en uploadet video (ikke YouTube) ---
  const setHero = (item: GalleryImage) => {
    if (!item.url) return;
    const value = item.url === heroUrl ? "" : item.url;
    setWarning("");
    void run(async () => {
      if (value && item.kind === "video") {
        // Stillbillede (vises før videoen, og på mobil) + advarsel om store/lange videoer
        const probe = await probeVideo(assetUrl(item.url!), !item.posterUrl);
        if (probe.poster) {
          const res = await uploadGalleryPoster(item.id, probe.poster);
          setImages(res.data);
        }
        const notes: string[] = [];
        if (probe.bytes && probe.bytes > WARN_BYTES) notes.push(`fylder ${Math.round(probe.bytes / (1024 * 1024))} MB`);
        if (probe.duration && probe.duration > WARN_SECONDS) notes.push(`varer ${Math.round(probe.duration)} sekunder`);
        if (notes.length > 0)
          setWarning(
            `Videoen ${notes.join(" og ")}, så forsiden kan være langsom at åbne. En kort video (under ${WARN_SECONDS} sekunder og ${WARN_BYTES / (1024 * 1024)} MB) er bedst. På mobil vises stillbilledet.`
          );
      }
      const res = await updateText("hero.image", value);
      setText("hero.image", res.data.value);
    });
  };

  return (
    <Modal title="Rediger billeder og video" onClose={onClose} wide>
      <MediaAdder
        busy={busy}
        progress={progress}
        onFiles={upload}
        onYouTube={addYouTube}
        onError={setError}
        ignoreDrop={dragId !== null}
      />
      {storage && storage.freeBytes !== null && storage.totalBytes !== null && (
        <p className={`admin-hint gallery-storage${storage.freeBytes < 100 * MB ? " low" : ""}`}>
          Plads på serveren: {formatMb(storage.freeBytes)} ledig af {formatMb(storage.totalBytes)} (billeder og video fylder{" "}
          {formatMb(storage.usedBytes)}).
        </p>
      )}

      {error && <div className="admin-error">{error}</div>}
      {warning && <div className="admin-warning">{warning}</div>}
      {busy && <div className="admin-status">Gemmer…</div>}

      {images.length === 0 ? (
        <p className="admin-empty">Der er ingen billeder eller videoer endnu.</p>
      ) : (
        <>
          <p className="admin-hint">Træk billederne og videoerne for at ændre rækkefølgen, eller brug pilene.</p>
          <ul className="admin-gallery">
            {images.map((img, index) => {
              const isHero = !!img.url && img.url === heroUrl;
              const label = kindLabel(img);
              return (
                <li
                  key={img.id}
                  className={`admin-gallery-item${dragId === img.id ? " dragging" : ""}`}
                  draggable={!busy}
                  onDragStart={() => setDragId(img.id)}
                  onDragEnd={() => setDragId(null)}
                  onDragOver={(e) => onDragOverItem(e, img.id)}
                  onDrop={(e) => onDropItem(e, img.id)}
                >
                  <div className="admin-thumb">
                    <MediaThumb media={img} index={index} />
                    <span className="admin-thumb-number">{index + 1}</span>
                    {isHero && <span className="admin-thumb-hero">Forside</span>}
                    {label && <span className="admin-thumb-hero">{label}</span>}
                  </div>
                  <div className="admin-thumb-actions">
                    <button type="button" onClick={() => move(index, -1)} disabled={busy || index === 0} aria-label="Flyt til venstre" title="Flyt til venstre">
                      ←
                    </button>
                    <button type="button" onClick={() => move(index, 1)} disabled={busy || index === images.length - 1} aria-label="Flyt til højre" title="Flyt til højre">
                      →
                    </button>
                    {(img.kind === "image" || img.kind === "video") && img.url && (
                      <button
                        type="button"
                        className={isHero ? "active" : ""}
                        onClick={() => setHero(img)}
                        disabled={busy}
                        title={heroTitle(img.kind, isHero)}
                        aria-label={heroTitle(img.kind, isHero)}
                      >
                        ★
                      </button>
                    )}
                    {confirmDelete === img.id ? (
                      <button type="button" className="danger confirm" onClick={() => remove(img.id)} disabled={busy}>
                        Slet?
                      </button>
                    ) : (
                      <button
                        type="button"
                        className="danger"
                        onClick={() => setConfirmDelete(img.id)}
                        disabled={busy}
                        aria-label={label ? "Slet video" : "Slet billede"}
                        title={label ? "Slet video" : "Slet billede"}
                      >
                        <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true">
                          <path d="M4 7h16M9 7V4h6v3M6 7l1 13h10l1-13" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                        </svg>
                      </button>
                    )}
                  </div>
                </li>
              );
            })}
          </ul>
        </>
      )}

      <div className="admin-actions">
        <button type="button" className="admin-btn" onClick={onClose}>
          Færdig
        </button>
      </div>
    </Modal>
  );
}

export default GalleryEditModal;
