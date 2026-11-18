// Læser en videos længde og laver et stillbillede (JPEG) af den i browseren - uden ffmpeg på serveren.
// Bruges, når en video vælges som baggrund på forsiden.

const once = (el: HTMLVideoElement, event: string, ms = 20000) =>
  new Promise<void>((resolve, reject) => {
    const timer = window.setTimeout(() => reject(new Error("Videoen kunne ikke indlæses.")), ms);
    el.addEventListener(event, () => { window.clearTimeout(timer); resolve(); }, { once: true });
    el.addEventListener("error", () => { window.clearTimeout(timer); reject(new Error("Videoen kunne ikke indlæses.")); }, { once: true });
  });

export type VideoProbe = { duration: number | null; bytes: number | null; poster: Blob | null };

export async function probeVideo(url: string, capturePoster: boolean): Promise<VideoProbe> {
  const bytes = await fetch(url, { method: "HEAD" })
    .then((r) => Number(r.headers.get("content-length")) || null)
    .catch(() => null);

  const video = document.createElement("video");
  video.muted = true;
  video.playsInline = true;
  video.preload = "auto";
  video.src = url;
  try {
    await once(video, "loadedmetadata");
    const duration = Number.isFinite(video.duration) ? video.duration : null;
    let poster: Blob | null = null;
    if (capturePoster && video.videoWidth > 0) {
      try {
        // Et billede lidt inde i videoen (det allerførste er tit sort)
        video.currentTime = Math.min(1, (duration ?? 0) / 10);
        await once(video, "seeked");
        const scale = Math.min(1, 1600 / video.videoWidth);
        const canvas = document.createElement("canvas");
        canvas.width = Math.round(video.videoWidth * scale);
        canvas.height = Math.round(video.videoHeight * scale);
        canvas.getContext("2d")?.drawImage(video, 0, 0, canvas.width, canvas.height);
        poster = await new Promise<Blob | null>((resolve) => canvas.toBlob(resolve, "image/jpeg", 0.85));
      } catch {
        poster = null; // så bruges første billede i galleriet som stillbillede
      }
    }
    return { duration, bytes, poster };
  } finally {
    video.removeAttribute("src");
    video.load();
  }
}
