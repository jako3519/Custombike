import type { GalleryImage, Service, SiteText } from "./Api";

// Data the server embeds in the page (window.__INITIAL_DATA__) so the first render
// already has the current texts, gallery and services - same shape as GET /api/texts, /api/gallery
// and /api/services. `path` is the page the server rendered ("/", "/produkter/<slug>",
// "/cookiepolitik", "/kontakt" or "/tak").
export type InitialData = {
  texts?: SiteText[];
  gallery?: GalleryImage[];
  services?: Service[];
  path?: string;
};

declare global {
  interface Window {
    __INITIAL_DATA__?: InitialData;
    dataLayer?: unknown[];
    gtag?: (...args: unknown[]) => void;
  }
}
