import { useEffect, useId, useRef, useState } from "react";
import { COOKIE_POLICY_PATH } from "../../consent/path";
import { CONSENT_OPEN_EVENT, readConsent, saveConsent } from "../../consent";
import type { ConsentChoice } from "../../consent";
import "./CookieBanner.css";

// Vises først i browseren (ikke i SSR), så hydreringen ikke fejler.
// Bundkortet spærrer ikke siden: man kan stadig læse og navigere bagved.
function CookieBanner() {
  const titleId = useId();
  const bannerRef = useRef<HTMLDivElement>(null);
  const [ready, setReady] = useState(false);
  const [open, setOpen] = useState(false);
  const [settings, setSettings] = useState(false);
  const [choice, setChoice] = useState<ConsentChoice>({ analytics: false, marketing: false });

  useEffect(() => {
    const stored = readConsent();
    if (stored) setChoice(stored);
    else setOpen(true);
    setReady(true);

    const onOpen = () => {
      setChoice(readConsent() ?? { analytics: false, marketing: false });
      setSettings(true);
      setOpen(true);
    };
    window.addEventListener(CONSENT_OPEN_EVENT, onOpen);
    return () => window.removeEventListener(CONSENT_OPEN_EVENT, onOpen);
  }, []);

  // Kontakt-boblen flyttes op over banneret, så den ikke dækker knapperne
  useEffect(() => {
    if (!ready || !open) {
      document.documentElement.style.removeProperty("--cookie-banner-height");
      return;
    }
    const el = bannerRef.current;
    if (!el) return;
    const apply = () => {
      document.documentElement.style.setProperty("--cookie-banner-height", `${Math.ceil(el.getBoundingClientRect().height)}px`);
    };
    apply();
    const ro = new ResizeObserver(apply);
    ro.observe(el);
    window.addEventListener("resize", apply);
    return () => {
      ro.disconnect();
      window.removeEventListener("resize", apply);
      document.documentElement.style.removeProperty("--cookie-banner-height");
    };
  }, [ready, open, settings]);

  if (!ready || !open) return null;

  const acceptAll = () => {
    saveConsent({ analytics: true, marketing: true });
    setOpen(false);
    setSettings(false);
  };
  const rejectAll = () => {
    saveConsent({ analytics: false, marketing: false });
    setOpen(false);
    setSettings(false);
  };
  const saveSelection = () => {
    saveConsent(choice);
    setOpen(false);
    setSettings(false);
  };

  return (
    <div className="cookie-banner" ref={bannerRef} role="dialog" aria-modal="false" aria-labelledby={titleId}>
      <div className="cookie-banner-card">
        <p className="cookie-banner-text" id={titleId}>
          Vi bruger cookies til statistik og marketing (Google), men kun hvis du giver samtykke. Nødvendige
          cookies er altid slået til. Du kan læse mere i vores{" "}
          <a href={COOKIE_POLICY_PATH}>cookiepolitik</a>.
        </p>

        {settings && (
          <div className="cookie-settings">
            <Toggle label="Nødvendige" checked locked description="Skal til, for at siden virker, og for at huske dit valg." />
            <Toggle
              label="Statistik"
              checked={choice.analytics}
              description="Google Analytics via Google Tag Manager, så vi kan se, hvordan siden bruges."
              onChange={(on) => setChoice((c) => ({ ...c, analytics: on }))}
            />
            <Toggle
              label="Marketing"
              checked={choice.marketing}
              description="Google Ads via Google Tag Manager, til annoncemåling og relevante annoncer."
              onChange={(on) => setChoice((c) => ({ ...c, marketing: on }))}
            />
          </div>
        )}

        <div className="cookie-banner-actions">
          <button type="button" className="cookie-btn" onClick={rejectAll}>
            Afvis
          </button>
          {settings && (
            <button type="button" className="cookie-btn" onClick={saveSelection}>
              Gem valg
            </button>
          )}
          <button type="button" className="cookie-btn" onClick={acceptAll}>
            Accepter alle
          </button>
          <button
            type="button"
            className="cookie-btn-settings"
            onClick={() => setSettings((v) => !v)}
            aria-expanded={settings}
          >
            {settings ? "Skjul indstillinger" : "Indstillinger"}
          </button>
        </div>
      </div>
    </div>
  );
}

function Toggle({
  label,
  checked,
  locked,
  description,
  onChange,
}: {
  label: string;
  checked: boolean;
  locked?: boolean;
  description: string;
  onChange?: (on: boolean) => void;
}) {
  const id = useId();
  return (
    <div className="cookie-toggle">
      <button
        type="button"
        className="cookie-switch"
        id={id}
        role="switch"
        aria-checked={checked}
        aria-label={label}
        disabled={locked}
        onClick={() => onChange?.(!checked)}
      >
        <span className="cookie-switch-knob" aria-hidden="true" />
      </button>
      <div className="cookie-toggle-text">
        <label htmlFor={id}>
          {label}
          {locked && <span className="cookie-always"> (altid slået til)</span>}
        </label>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default CookieBanner;
