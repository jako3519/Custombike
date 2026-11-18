import { API_URL } from "./Api";

// Indbygget, cookiefri besøgsstatistik. Sender kun anonyme hændelser til vores egen server
// (ingen tredjepart, ingen cookies). Slås først til, når vi ved at brugeren IKKE er admin.
let enabled = false;

export function setAnalyticsEnabled(on: boolean) {
  enabled = on;
}

type AnalyticsEvent = { type: "pageview" | "click"; path: string; target?: string; referrer?: string | null };

function send(event: AnalyticsEvent) {
  if (!enabled || typeof window === "undefined") return;
  const url = `${API_URL}/api/analytics/event`;
  const body = JSON.stringify(event);
  try {
    // sendBeacon overlever, at man forlader siden (fx klik på et tlf.-link). Kun på samme domæne.
    if (!API_URL && typeof navigator.sendBeacon === "function") {
      if (navigator.sendBeacon(url, new Blob([body], { type: "application/json" }))) return;
    }
    void fetch(url, {
      method: "POST",
      body,
      headers: { "Content-Type": "application/json" },
      keepalive: true,
      credentials: API_URL ? "include" : "same-origin",
    }).catch(() => {});
  } catch {
    // Statistik må aldrig få siden til at fejle
  }
}

export function trackPageview(withReferrer: boolean) {
  send({
    type: "pageview",
    path: window.location.pathname,
    referrer: withReferrer ? document.referrer || null : null,
  });
}

export function trackClick(target: string) {
  send({ type: "click", path: window.location.pathname, target });
}

// Hvilket "mål" et klik ramte: data-track="gallery:12", ellers tlf-, mail- og Facebook-links
export function clickTarget(el: Element | null): string | null {
  if (!el) return null;
  const tracked = el.closest("[data-track]");
  if (tracked) return tracked.getAttribute("data-track");
  const href = el.closest("a[href]")?.getAttribute("href") ?? "";
  if (href.startsWith("tel:")) return "phone";
  if (href.startsWith("mailto:")) return "email";
  if (/facebook\.com/i.test(href)) return "facebook";
  return null;
}
