// Server-side entry: renders the same React tree as main.tsx to an HTML string.
// Built with `vite build --ssr` (see vite.config.ts) and executed by the .NET backend
// (Backend/Prerender/PrerenderService.cs) on every page request, using the current
// texts, gallery and services from the database, for the requested page (data.path).
// The browser then hydrates this markup.
import React from "react";
import { renderToString } from "react-dom/server";
import App from "./App";
import { TextProvider } from "./texts/TextContext";
import { GalleryProvider } from "./gallery/GalleryContext";
import { AdminProvider } from "./admin/AdminContext";
import { ServicesProvider } from "./services/ServicesContext";
import type { InitialData } from "./initialData";

export function render(dataJson: string): string {
  const data = JSON.parse(dataJson) as InitialData;
  return renderToString(
    <React.StrictMode>
      <TextProvider initial={data.texts}>
        <GalleryProvider initial={data.gallery}>
          <ServicesProvider initial={data.services ?? []}>
            <AdminProvider>
              <App path={data.path ?? "/"} />
            </AdminProvider>
          </ServicesProvider>
        </GalleryProvider>
      </TextProvider>
    </React.StrictMode>
  );
}
