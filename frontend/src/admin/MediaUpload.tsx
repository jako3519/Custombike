import { useRef, useState } from "react";
import type { FormEvent } from "react";
import "./ServiceAdmin.css";
import { assetUrl } from "../Api";
import type { ServiceMedia } from "../Api";
import { IMAGE_TYPES, VIDEO_TYPES, checkFile } from "./mediaRules";

// Fælles for galleriet og ydelserne: tilføj billeder/video (upload) eller et YouTube-link,
// og små forhåndsvisninger i admin-listerne.

export function MediaThumb({ media, index }: { media: ServiceMedia; index: number }) {
  if (media.kind === "image" && media.url) return <img src={assetUrl(media.url)} alt={`Billede ${index + 1}`} draggable={false} />;
  if (media.kind === "video" && media.url)
    return <video src={assetUrl(media.url)} poster={media.posterUrl ? assetUrl(media.posterUrl) : undefined} preload="metadata" muted playsInline />;
  if (media.kind === "youtube" && media.youTubeId)
    return <img src={`https://i.ytimg.com/vi/${media.youTubeId}/mqdefault.jpg`} alt={`YouTube-video ${index + 1}`} draggable={false} />;
  return null;
}


type MediaAdderProps = {
  busy: boolean;
  progress: number | null;
  onFiles: (files: File[]) => void;
  // Resolver til true, når videoen er tilføjet (så feltet tømmes)
  onYouTube: (url: string) => Promise<boolean>;
  onError: (message: string) => void;
  // Fx mens man trækker et billede for at ændre rækkefølgen
  ignoreDrop?: boolean;
};

// Vælg mellem upload af filer og et YouTube-link
export function MediaAdder({ busy, progress, onFiles, onYouTube, onError, ignoreDrop }: MediaAdderProps) {
  const [mode, setMode] = useState<"file" | "youtube">("file");
  const [youtubeUrl, setYoutubeUrl] = useState("");
  const [dropActive, setDropActive] = useState(false);
  const input = useRef<HTMLInputElement>(null);

  const handle = (list: FileList | null) => {
    const files = Array.from(list ?? []);
    if (files.length === 0) return;
    for (const f of files) {
      const problem = checkFile(f, true);
      if (problem) {
        onError(problem);
        return;
      }
    }
    onFiles(files);
  };

  const addYouTube = async (e: FormEvent) => {
    e.preventDefault();
    if (!youtubeUrl.trim()) return;
    if (await onYouTube(youtubeUrl.trim())) setYoutubeUrl("");
  };

  return (
    <>
      <div className="svc-mode" role="tablist" aria-label="Tilføj">
        <button type="button" role="tab" aria-selected={mode === "file"} className={mode === "file" ? "active" : ""} onClick={() => setMode("file")}>
          Upload billeder / video
        </button>
        <button type="button" role="tab" aria-selected={mode === "youtube"} className={mode === "youtube" ? "active" : ""} onClick={() => setMode("youtube")}>
          YouTube-link
        </button>
      </div>

      {mode === "file" ? (
        <div
          className={`admin-dropzone${dropActive ? " active" : ""}`}
          onDragOver={(e) => {
            if (ignoreDrop) return;
            e.preventDefault();
            setDropActive(true);
          }}
          onDragLeave={() => setDropActive(false)}
          onDrop={(e) => {
            if (ignoreDrop) return;
            e.preventDefault();
            setDropActive(false);
            handle(e.dataTransfer.files);
          }}
        >
          <svg viewBox="0 0 24 24" width="30" height="30" aria-hidden="true">
            <path d="M12 16V4m0 0l-5 5m5-5l5 5M4 16v3a1 1 0 001 1h14a1 1 0 001-1v-3" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
          </svg>
          <span>Træk filer hertil eller</span>
          <button type="button" className="admin-btn" onClick={() => input.current?.click()} disabled={busy}>
            Vælg filer
          </button>
          <span className="svc-dropzone-help">Billeder (jpg, png, webp, avif, gif – højst 15 MB) og video (mp4 eller webm – højst 100 MB)</span>
          <input
            ref={input}
            type="file"
            accept={[...IMAGE_TYPES, ...VIDEO_TYPES].join(",")}
            multiple
            hidden
            onChange={(e) => {
              handle(e.target.files);
              e.target.value = "";
            }}
          />
        </div>
      ) : (
        <form className="svc-youtube" onSubmit={addYouTube}>
          <input
            type="url"
            inputMode="url"
            placeholder="https://www.youtube.com/watch?v=…  eller  https://youtu.be/…"
            value={youtubeUrl}
            onChange={(e) => setYoutubeUrl(e.target.value)}
            aria-label="YouTube-link"
          />
          <button type="submit" className="admin-btn" disabled={busy || !youtubeUrl.trim()}>
            Tilføj video
          </button>
          <span className="admin-field-help">Videoen vises først, når man trykker afspil, og bruger YouTubes privatlivsvenlige afspiller.</span>
        </form>
      )}

      {progress !== null && (
        <div className="svc-progress" role="progressbar" aria-valuenow={Math.round(progress * 100)} aria-valuemin={0} aria-valuemax={100}>
          <span style={{ width: `${Math.round(progress * 100)}%` }} />
          <em>Uploader… {Math.round(progress * 100)} %</em>
        </div>
      )}
    </>
  );
}
