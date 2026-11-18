import { useEffect, useRef, useState } from "react";
import Navbar from "./Components/navBar/Navbar";
import ContactWidget from "./Components/contact/ContactWidget";
import HomePage from "./Pages/homepage/Homepage";
import ServicePage from "./Pages/service/ServicePage";
import NotFoundPage from "./Pages/notfound/NotFoundPage";
import CookiePolicyPage from "./Pages/cookie/CookiePolicyPage";
import ContactPage from "./Pages/contact/ContactPage";
import ThanksPage from "./Pages/contact/ThanksPage";
import CookieBanner from "./Components/consent/CookieBanner";
import AdminBar from "./admin/AdminBar";
import { useAdmin } from "./admin/AdminContext";
import { useServices } from "./services/ServicesContext";
import { serviceSlugFromPath } from "./services/route";
import { isCookiePolicyPath } from "./consent/path";
import { isContactPath, isThanksPath } from "./contact/path";
import { clickTarget, setAnalyticsEnabled, trackClick, trackPageview } from "./analytics";
import "./App.css";

type AppProps = {
  // Siden der vises: "/", "/ydelser/<slug>", "/cookiepolitik", "/kontakt" eller "/tak".
  path: string;
};

function App({ path }: AppProps) {
  const [contactOpen, setContactOpen] = useState(false);
  const { me, isAdmin } = useAdmin();
  const { services, loaded } = useServices();
  const lastPath = useRef<string | null>(null);

  const slug = serviceSlugFromPath(path);
  const service = slug ? services.find((s) => s.slug === slug) : undefined;

  // Besøgsstatistik: først når vi ved, om man er admin (admin tælles aldrig med)
  useEffect(() => {
    if (!me) return;
    setAnalyticsEnabled(!isAdmin);
    if (isAdmin || lastPath.current !== null) return;
    lastPath.current = window.location.pathname;
    trackPageview(true);
  }, [me, isAdmin]);

  // Sideskift (tilbage/frem i browseren) og klik på telefon, mail, Facebook, ydelser og billeder
  useEffect(() => {
    const onPop = () => {
      if (lastPath.current !== null && window.location.pathname !== lastPath.current) {
        lastPath.current = window.location.pathname;
        trackPageview(false);
      }
    };
    const onClick = (e: MouseEvent) => {
      const target = clickTarget(e.target as Element | null);
      if (target) trackClick(target);
    };
    window.addEventListener("popstate", onPop);
    document.addEventListener("click", onClick, true);
    return () => {
      window.removeEventListener("popstate", onPop);
      document.removeEventListener("click", onClick, true);
    };
  }, []);

  // Kontakt-pop-up'en åbnet (fra menuen eller den runde knap)
  useEffect(() => {
    if (contactOpen) trackClick("contact");
  }, [contactOpen]);

  // /kontakt og gamle #kontakt-links åbner boblen, så formularen stadig kan findes
  useEffect(() => {
    const shouldOpen = () => isContactPath(path) || window.location.hash.replace(/^#/, "") === "kontakt";
    if (shouldOpen()) setContactOpen(true);
    const onHash = () => {
      if (shouldOpen()) setContactOpen(true);
    };
    window.addEventListener("hashchange", onHash);
    return () => window.removeEventListener("hashchange", onHash);
  }, [path]);

  return (
    <>
      <Navbar onContactClick={() => setContactOpen(true)} currentSlug={service?.slug} />
      <main>
        {isThanksPath(path) ? (
          <ThanksPage />
        ) : isContactPath(path) ? (
          <ContactPage onContactClick={() => setContactOpen(true)} />
        ) : isCookiePolicyPath(path) ? (
          <CookiePolicyPage />
        ) : !slug ? (
          <HomePage />
        ) : service ? (
          <ServicePage service={service} onContactClick={() => setContactOpen(true)} />
        ) : loaded ? (
          <NotFoundPage />
        ) : null}
      </main>
      <ContactWidget open={contactOpen} onToggle={setContactOpen} preselectedSlug={service?.slug} />
      <CookieBanner />
      <AdminBar />
    </>
  );
}

export default App;
