import { useCallback, useEffect, useMemo, useState } from "react";
import { createPortal } from "react-dom";
import "../gallery/Gallery.css";
import "./MediaGallery.css";
import { assetUrl } from "../../Api";
import type { ServiceMedia } from "../../Api";
import { uniqueServiceMedia } from "../../media/unique";

// Billeder og videoer for en ydelse i et ensartet gitter: alle felter har samme størrelse (4:3, beskåret).
// Et klik åbner det store vindue (lightbox) med hele billedet eller videoen.
// YouTube: i gitteret vises kun videoens stillbillede (i.ytimg.com, uden cookies). Afspilleren indlæses
// først, når man åbner videoen - fra youtube-nocookie.com (YouTubes "privatlivsforbedrede" tilstand).

const PlayIcon = () => (
  <span className="media-play" aria-hidden="true">
    <svg viewBox="0 0 24 24" width="30" height="30">
      <path d="M8 5.5v13l11-6.5z" fill="currentColor" />
    </svg>
  </span>
);

function isShown(m: ServiceMedia) {
  return m.kind === "image" || m.kind === "video" ? !!m.url : m.kind === "youtube" && !!m.youTubeId;
}

type MediaGalleryProps = {
  media: ServiceMedia[];
  title: string;
  // "gallery": forsidens bredere gitter (flere kolonner på store skærme)
  variant?: "service" | "gallery";
  // Klik-statistik: data-track="<prefix>:<id>"
  trackPrefix?: string;
};

function MediaGallery({ media, title, variant = "service", trackPrefix }: MediaGalleryProps) {
  const items = useMemo(() => uniqueServiceMedia(media.filter(isShown)), [media]);
  const [active, setActive] = useState<number | null>(null);

  const close = useCallback(() => setActive(null), []);
  const step = useCallback(
    (dir: number) => setActive((i) => (i === null ? i : (i + dir + items.length) % items.length)),
    [items.length]
  );

  useEffect(() => {
    if (active === null) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") close();
      if (e.key === "ArrowRight") step(1);
      if (e.key === "ArrowLeft") step(-1);
    };
    document.addEventListener("keydown", onKey);
    document.body.style.overflow = "hidden";
    return () => {
      document.removeEventListener("keydown", onKey);
      document.body.style.overflow = "";
    };
  }, [active, close, step]);

  // "billede 2" / "video 1" - nummereret hver for sig
  const labels: string[] = [];
  let imageNo = 0;
  let videoNo = 0;
  for (const m of items) labels.push(m.kind === "image" ? `billede ${++imageNo}` : `video ${++videoNo}`);

  const current = active !== null ? items[active] : undefined;

  return (
    <>
      <ul className={`media-grid${variant === "gallery" ? " media-grid-wide" : ""}`}>
        {items.map((m, index) => (
          <li key={m.id} className="media-cell">
            <button
              type="button"
              className={`media-item media-${m.kind}`}
              data-track={trackPrefix ? `${trackPrefix}:${m.id}` : undefined}
              onClick={() => setActive(index)}
              aria-label={m.kind === "image" ? `Vis ${labels[index]}` : `Afspil ${labels[index]}`}
            >
              {m.kind === "image" && <img src={assetUrl(m.url!)} alt={`${title} – ${labels[index]}`} loading="lazy" />}
              {m.kind === "video" && (
                // Kun til visning af første billede i videoen; selve afspilningen sker i det store vindue
                <video
                  src={`${assetUrl(m.url!)}#t=0.1`}
                  poster={m.posterUrl ? assetUrl(m.posterUrl) : undefined}
                  preload="metadata"
                  muted
                  playsInline
                  tabIndex={-1}
                  aria-hidden="true"
                />
              )}
              {m.kind === "youtube" && <img src={`https://i.ytimg.com/vi/${m.youTubeId}/hqdefault.jpg`} alt="" loading="lazy" />}
              {m.kind !== "image" && <PlayIcon />}
            </button>
          </li>
        ))}
      </ul>

      {current &&
        active !== null &&
        createPortal(
          <div className="lightbox" role="dialog" aria-modal="true" aria-label={`${title} – ${labels[active]}`} onClick={close}>
            {current.kind === "image" && (
              <img src={assetUrl(current.url!)} alt={`${title} – ${labels[active]}`} onClick={(e) => e.stopPropagation()} />
            )}
            {current.kind === "video" && (
              // Starter, fordi man selv har trykket på videoen
              <video
                key={current.id}
                className="lightbox-video"
                src={assetUrl(current.url!)}
                controls
                autoPlay
                playsInline
                onClick={(e) => e.stopPropagation()}
              />
            )}
            {current.kind === "youtube" && (
              <div className="lightbox-frame" onClick={(e) => e.stopPropagation()}>
                <iframe
                  key={current.id}
                  src={`https://www.youtube-nocookie.com/embed/${current.youTubeId}?autoplay=1&rel=0`}
                  title={`${title} – ${labels[active]}`}
                  allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
                  referrerPolicy="strict-origin-when-cross-origin"
                  allowFullScreen
                />
              </div>
            )}
            <button className="lightbox-btn lightbox-close" onClick={close} aria-label="Luk">
              ✕
            </button>
            {items.length > 1 && (
              <>
                <button className="lightbox-btn lightbox-prev" onClick={(e) => { e.stopPropagation(); step(-1); }} aria-label="Forrige">
                  ‹
                </button>
                <button className="lightbox-btn lightbox-next" onClick={(e) => { e.stopPropagation(); step(1); }} aria-label="Næste">
                  ›
                </button>
                <span className="lightbox-count">
                  {active + 1} / {items.length}
                </span>
              </>
            )}
          </div>,
          document.body
        )}
    </>
  );
}

export default MediaGallery;
