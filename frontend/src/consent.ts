// Samtykke til Google-cookies (statistik og marketing). Vores egen cookiefri
// besøgsstatistik er uafhængig af dette og røres ikke.
export const CONSENT_STORAGE_KEY = "custombike.consent";
export const CONSENT_VERSION = 1;
export const CONSENT_MAX_AGE_MS = 365 * 24 * 60 * 60 * 1000;
export const CONSENT_OPEN_EVENT = "custombike:consent-open";

export type ConsentChoice = {
  analytics: boolean;
  marketing: boolean;
};

type StoredConsent = ConsentChoice & { v: number; ts: number };

function gtag(...args: unknown[]) {
  if (typeof window === "undefined") return;
  if (typeof window.gtag === "function") window.gtag(...args);
  else {
    window.dataLayer = window.dataLayer || [];
    window.dataLayer.push(args);
  }
}

export function applyConsent(choice: ConsentChoice) {
  if (typeof window === "undefined") return;
  const marketing = choice.marketing ? "granted" : "denied";
  gtag("consent", "update", {
    analytics_storage: choice.analytics ? "granted" : "denied",
    ad_storage: marketing,
    ad_user_data: marketing,
    ad_personalization: marketing,
  });
  window.dataLayer = window.dataLayer || [];
  window.dataLayer.push({ event: "cookie_consent_update" });
}

function clearGoogleCookies(choice: ConsentChoice) {
  if (typeof document === "undefined") return;
  const names = document.cookie.split(";").map((s) => s.split("=")[0].trim());
  const drop = names.filter((n) => {
    if (!choice.analytics && /^(?:_ga|_gid|_gat)/.test(n)) return true;
    if (!choice.marketing && /^(?:_gcl|_gac)/.test(n)) return true;
    return false;
  });
  const hosts = [location.hostname];
  if (location.hostname.includes(".")) hosts.push("." + location.hostname);
  for (const name of drop) {
    document.cookie = `${name}=; Max-Age=0; Path=/`;
    for (const host of hosts) document.cookie = `${name}=; Max-Age=0; Path=/; Domain=${host}`;
  }
}

export function saveConsent(choice: ConsentChoice) {
  const stored: StoredConsent = { v: CONSENT_VERSION, ts: Date.now(), ...choice };
  try {
    localStorage.setItem(CONSENT_STORAGE_KEY, JSON.stringify(stored));
  } catch {
    // Lagring må ikke vælte siden (privat tilstand, fuldt lager m.m.)
  }
  applyConsent(choice);
  clearGoogleCookies(choice);
}

export function readConsent(): ConsentChoice | null {
  if (typeof window === "undefined") return null;
  try {
    const raw = localStorage.getItem(CONSENT_STORAGE_KEY);
    if (!raw) return null;
    const c = JSON.parse(raw) as StoredConsent;
    if (!c || c.v !== CONSENT_VERSION || typeof c.ts !== "number") return null;
    if (Date.now() - c.ts >= CONSENT_MAX_AGE_MS) return null;
    if (typeof c.analytics !== "boolean" || typeof c.marketing !== "boolean") return null;
    return { analytics: c.analytics, marketing: c.marketing };
  } catch {
    return null;
  }
}

export function openConsentSettings() {
  if (typeof window === "undefined") return;
  window.dispatchEvent(new Event(CONSENT_OPEN_EVENT));
}
