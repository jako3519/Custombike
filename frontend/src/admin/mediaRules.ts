import type { ServiceMedia } from "../Api";

// Regler for upload af billeder og video (galleriet og ydelserne). Serveren tjekker det samme igen.

export const IMAGE_TYPES = ["image/jpeg", "image/png", "image/webp", "image/avif", "image/gif"];
export const VIDEO_TYPES = ["video/mp4", "video/webm"];
const MAX_IMAGE = 15 * 1024 * 1024;
const MAX_VIDEO = 100 * 1024 * 1024;

// Tjek i browseren før upload (serveren tjekker det samme igen, inkl. filens indhold)
export function checkFile(file: File, allowVideo: boolean): string | null {
  if (IMAGE_TYPES.includes(file.type)) return file.size > MAX_IMAGE ? `"${file.name}" er større end 15 MB.` : null;
  if (allowVideo && VIDEO_TYPES.includes(file.type)) return file.size > MAX_VIDEO ? `"${file.name}" er større end 100 MB.` : null;
  return allowVideo
    ? `"${file.name}" er ikke tilladt. Brug billeder (jpg, png, webp, avif, gif) eller video (mp4, webm).`
    : `"${file.name}" er ikke et tilladt billedformat (jpg, png, webp, avif, gif).`;
}

export const kindLabel = (m: ServiceMedia) => (m.kind === "youtube" ? "YouTube" : m.kind === "video" ? "Video" : null);
