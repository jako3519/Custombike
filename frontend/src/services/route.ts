// /produkter/<slug> -> slug, ellers null (alle andre stier viser forsiden, som hidtil)
export function serviceSlugFromPath(path: string): string | null {
  const m = /^\/produkter\/([^/]+)\/?$/i.exec(path);
  if (!m) return null;
  try {
    return decodeURIComponent(m[1]);
  } catch {
    return m[1];
  }
}

export function productPath(slug: string): string {
  return `/produkter/${encodeURIComponent(slug)}`;
}

// Gamle adresser /ydelser og /ydelser/<slug> sendes videre til /produkter.
// /ydelser uden navn lander på produktsektionen på forsiden.
export function legacyProductRedirect(path: string): string | null {
  const m = /^\/ydelser(?:\/([^/]*))?\/?$/i.exec(path);
  if (!m) return null;
  const raw = m[1];
  if (!raw) return "/#produkter";
  try {
    return `/produkter/${decodeURIComponent(raw)}`;
  } catch {
    return `/produkter/${raw}`;
  }
}
