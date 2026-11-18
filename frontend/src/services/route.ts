// /ydelser/<slug> -> slug, ellers null (alle andre stier viser forsiden, som hidtil)
export function serviceSlugFromPath(path: string): string | null {
  const m = /^\/ydelser\/([^/]+)\/?$/i.exec(path);
  if (!m) return null;
  try {
    return decodeURIComponent(m[1]);
  } catch {
    return m[1];
  }
}
