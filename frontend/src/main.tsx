import React from "react";
import ReactDOM from "react-dom/client";
import "./index.css";
import App from "./App";
import { TextProvider } from "./texts/TextContext";
import { GalleryProvider } from "./gallery/GalleryContext";
import { AdminProvider } from "./admin/AdminContext";
import { ServicesProvider } from "./services/ServicesContext";
import { loginUrl } from "./Api";

// /admin (også /Admin) starter det almindelige login og vender derefter tilbage til forsiden
if (window.location.pathname.replace(/\/+$/, "").toLowerCase() === "/admin") {
  window.history.replaceState(null, "", "/");
  window.location.replace(loginUrl());
} else {
  // Tekster, billeder og ydelser, som serveren har lagt ind i siden (tom i Vite dev)
  const initial = window.__INITIAL_DATA__ ?? {};
  const container = document.getElementById("root")!;
  const app = (
    <React.StrictMode>
      <TextProvider initial={initial.texts}>
        <GalleryProvider initial={initial.gallery}>
          <ServicesProvider initial={initial.services}>
            <AdminProvider>
              <App path={window.location.pathname} />
            </AdminProvider>
          </ServicesProvider>
        </GalleryProvider>
      </TextProvider>
    </React.StrictMode>
  );

  // Serveren har allerede renderet siden ind i #root: genbrug den (hydrate). Ellers render fra bunden.
  if (container.firstElementChild) {
    ReactDOM.hydrateRoot(container, app);
  } else {
    ReactDOM.createRoot(container).render(app);
  }
}
