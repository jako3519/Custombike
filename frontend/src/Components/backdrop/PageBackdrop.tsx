import { useEffect, useState } from "react";
import "./PageBackdrop.css";
import { assetUrl } from "../../Api";
import { useText } from "../../texts/TextContext";
import { useGallery } from "../../gallery/GalleryContext";

// Videoen spilles kun på store skærme, når brugeren ikke har bedt om mindre bevægelse eller om at spare data.
// Ellers (og før siden er indlæst i browseren) vises stillbilledet.
function useVideoAllowed(enabled: boolean) {
  const [allowed, setAllowed] = useState(false);
  useEffect(() => {
    if (!enabled || typeof window.matchMedia !== "function") return;
    const wide = window.matchMedia("(min-width: 900px)");
    const calm = window.matchMedia("(prefers-reduced-motion: reduce)");
    const saveData = (navigator as Navigator & { connection?: { saveData?: boolean } }).connection?.saveData === true;
    const update = () => setAllowed(wide.matches && !calm.matches && !saveData);
    update();
    wide.addEventListener("change", update);
    calm.addEventListener("change", update);
    return () => {
      wide.removeEventListener("change", update);
      calm.removeEventListener("change", update);
    };
  }, [enabled]);
  return enabled && allowed;
}

const isVideoUrl = (url: string) => /\.(mp4|webm)$/i.test(url);

// Fast baggrund bag hele siden (helt ned til bunden). Indholdet ligger ovenpå som "glas".
// Et position: fixed-lag i stedet for background-attachment: fixed, som ikke virker ordentligt på iPhone.
// Billede: det angivne (fx en ydelses forsidebillede), ellers forsidens billede, ellers første billede i
// galleriet, ellers en mørk træ-tone. Er forsidens baggrund en video, vises den kun på forsiden (allowVideo)
// og ellers dens stillbillede (eller første billede i galleriet).
function PageBackdrop({ src, allowVideo = false }: { src?: string | null; allowVideo?: boolean }) {
  const heroValue = useText("hero.image").trim();
  const { images } = useGallery();
  const firstPhoto = images.find((i) => i.kind === "image" && i.url)?.url;
  const heroVideo = heroValue ? images.find((i) => i.kind === "video" && i.url === heroValue) : undefined;
  // En video der ikke findes mere (eller et link til en video) bruges aldrig som billede
  const heroPhoto = heroVideo ? heroVideo.posterUrl || firstPhoto : heroValue && !isVideoUrl(heroValue) ? heroValue : firstPhoto;
  const image = src || heroPhoto;
  const videoUrl = !src && heroVideo?.url ? heroVideo.url : null;
  const playVideo = useVideoAllowed(allowVideo && !!videoUrl);
  const [playing, setPlaying] = useState(false);

  const style = image ? { backgroundImage: `url("${assetUrl(image)}")` } : undefined;
  return (
    <div className="page-backdrop" aria-hidden="true">
      <div className="page-backdrop-image" style={style} />
      {playVideo && videoUrl && (
        <video
          key={videoUrl}
          className={`page-backdrop-video${playing ? " playing" : ""}`}
          src={assetUrl(videoUrl)}
          poster={image ? assetUrl(image) : undefined}
          muted
          loop
          playsInline
          autoPlay
          preload="auto"
          tabIndex={-1}
          disablePictureInPicture
          ref={(el) => {
            // React sætter ikke altid muted-attributten; uden den må browseren ikke starte videoen selv
            if (el && !el.muted) {
              el.muted = true;
              void el.play().catch(() => undefined);
            }
          }}
          onPlaying={() => setPlaying(true)}
        />
      )}
      <div className="page-backdrop-shade" />
    </div>
  );
}

export default PageBackdrop;
