import type { GalleryImage, Service, ServiceMedia } from "../Api";

// Samme fil kan være uploadet flere steder (fx både i galleriet og på en ydelse).
// Filerne genkendes på deres indhold (hash), YouTube-videoer på video-id'et,
// så hvert billede og hver video kun vises én gang på en side.
const keyOf = (m: ServiceMedia) =>
  m.kind === "youtube" ? `y:${m.youTubeId}` : m.hash ? `h:${m.hash}` : `u:${m.url ?? m.id}`;

function unique<T extends ServiceMedia>(items: T[], seen = new Set<string>()): T[] {
  return items.filter((m) => {
    const key = keyOf(m);
    if (seen.has(key)) return false;
    seen.add(key);
    return true;
  });
}

// Galleriet på forsiden: hvert billede/video én gang, og ikke billeder der allerede står som
// forsidebillede på et ydelseskort længere oppe på siden.
export function uniqueGalleryImages(images: GalleryImage[], services: Service[]): GalleryImage[] {
  const seen = new Set<string>();
  for (const s of services) if (s.coverUrl && s.coverHash) seen.add(`h:${s.coverHash}`);
  return unique(images, seen);
}

// En ydelses billeder og videoer: hver fil (og hver YouTube-video) én gang
export function uniqueServiceMedia(media: ServiceMedia[]): ServiceMedia[] {
  return unique(media);
}
