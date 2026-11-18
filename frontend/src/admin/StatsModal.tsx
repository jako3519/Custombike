import { useEffect, useState } from "react";
import Modal from "./Modal";
import { getStats, errorMessage } from "../Api";
import { useServices } from "../services/ServicesContext";
import type { Stats } from "../Api";
import "./StatsModal.css";

type Period = Stats["period"];

const PERIODS: { key: Period; label: string }[] = [
  { key: "today", label: "I dag" },
  { key: "7d", label: "7 dage" },
  { key: "30d", label: "30 dage" },
  { key: "year", label: "I år" },
];

const fmt = (n: number) => n.toLocaleString("da-DK");

const regionNames = (() => {
  try {
    return new Intl.DisplayNames(["da"], { type: "region" });
  } catch {
    return null;
  }
})();
const countryName = (code: string) => {
  try {
    return regionNames?.of(code) ?? code;
  } catch {
    return code;
  }
};

const dateLabel = (iso: string) =>
  new Date(`${iso}T12:00:00`).toLocaleDateString("da-DK", { day: "numeric", month: "short", year: "numeric" });

type Row = { label: string; value: number; extra?: string };

function StatList({ title, rows, empty = "Ingen data endnu" }: { title: string; rows: Row[]; empty?: string }) {
  return (
    <section className="stats-section">
      <h3 className="stats-section-title">{title}</h3>
      {rows.length === 0 ? (
        <p className="stats-empty">{empty}</p>
      ) : (
        <ul className="stats-list">
          {rows.map((r) => (
            <li key={r.label}>
              <span className="stats-label">{r.label}</span>
              <span className="stats-value">
                {fmt(r.value)}
                {r.extra && <small>{r.extra}</small>}
              </span>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}

// Statistik-pop-up (kun admin): tal for den valgte periode, ingen grafer
function StatsModal({ onClose }: { onClose: () => void }) {
  const [period, setPeriod] = useState<Period>("today");
  const [stats, setStats] = useState<Stats | null>(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const { services } = useServices();

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError("");
    (async () => {
      try {
        const res = await getStats(period);
        if (!cancelled) setStats(res.data);
      } catch (err) {
        if (!cancelled) setError(errorMessage(err));
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [period]);

  const targetLabel = (target: string) => {
    if (target === "phone") return "Telefonnummer";
    if (target === "email") return "E-mail";
    if (target === "contact") return "Kontakt-knappen";
    if (target === "facebook") return "Facebook";
    if (target.startsWith("gallery:")) return `Galleri: billede ${target.slice(8)}`;
    if (target.startsWith("service:")) {
      const slug = target.slice(8);
      return `Produkt: ${services.find((s) => s.slug === slug)?.title ?? slug}`;
    }
    return target;
  };

  const devicesTotal = stats ? stats.mobile + stats.desktop : 0;
  const pct = (n: number) => (devicesTotal ? ` ${Math.round((n / devicesTotal) * 100)} %` : "");

  return (
    <Modal title="Statistik" onClose={onClose} wide>
      <div className="stats">
        <div className="stats-periods" role="tablist" aria-label="Periode">
          {PERIODS.map((p) => (
            <button
              key={p.key}
              role="tab"
              aria-selected={period === p.key}
              className={`stats-period${period === p.key ? " active" : ""}`}
              onClick={() => setPeriod(p.key)}
            >
              {p.label}
            </button>
          ))}
        </div>

        {error && <p className="admin-error">{error}</p>}
        {!error && !stats && <p className="stats-empty">Henter tal…</p>}

        {stats && (
          <div className={`stats-content${loading ? " loading" : ""}`} aria-busy={loading}>
            <p className="stats-range">
              {stats.from === stats.to ? dateLabel(stats.from) : `${dateLabel(stats.from)} – ${dateLabel(stats.to)}`}
            </p>

            <div className="stats-summary">
              <div className="stats-card">
                <span className="stats-card-value">{fmt(stats.uniqueVisitors)}</span>
                <span className="stats-card-label">Unikke besøgende</span>
              </div>
              <div className="stats-card">
                <span className="stats-card-value">{fmt(stats.pageviews)}</span>
                <span className="stats-card-label">Sidevisninger</span>
              </div>
              <div className="stats-card">
                <span className="stats-card-value">{fmt(stats.totalClicks)}</span>
                <span className="stats-card-label">Klik</span>
              </div>
            </div>

            <div className="stats-grid">
              <StatList
                title="Enheder"
                rows={[
                  { label: "Mobil", value: stats.mobile, extra: pct(stats.mobile) },
                  { label: "Computer", value: stats.desktop, extra: pct(stats.desktop) },
                ]}
              />
              <StatList title="Klik" rows={stats.clicks.map((c) => ({ label: targetLabel(c.target), value: c.count }))} />
              <StatList
                title="Kom fra"
                rows={[
                  { label: "Google", value: stats.sources.google },
                  { label: "Facebook", value: stats.sources.facebook },
                  { label: "Direkte", value: stats.sources.direct },
                  { label: "Andet", value: stats.sources.other },
                ]}
              />
              <StatList title="Sider" rows={stats.pages.map((p) => ({ label: p.path, value: p.pageviews }))} />
              <StatList
                title="Lande"
                rows={stats.countries.map((c) => ({ label: countryName(c.code), value: c.visitors }))}
                empty={stats.geoAvailable ? "Ingen data endnu" : "Geo-database mangler"}
              />
              <StatList
                title="Byer"
                rows={stats.cities.map((c) => ({ label: c.country ? `${c.city}, ${c.country}` : c.city, value: c.visitors }))}
                empty={stats.geoAvailable ? "Ingen data endnu" : "Geo-database mangler"}
              />
            </div>

            <p className="stats-note">
              Uden cookies og uden at gemme IP-adresser. En besøgende tælles én gang pr. dag, og for en periode
              lægges dagene sammen. Besøg fra admin og søgemaskine-robotter tælles ikke med. Lande, byer, enheder og
              henvisninger er i unikke besøgende; sider i sidevisninger.
            </p>
            <p className="stats-attribution">
              <a href="https://db-ip.com" target="_blank" rel="noopener noreferrer">
                IP-geolokation fra DB-IP.com
              </a>{" "}
              (CC BY 4.0)
            </p>
          </div>
        )}
      </div>
    </Modal>
  );
}

export default StatsModal;
