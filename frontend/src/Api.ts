import axios from "axios";

// Backend-adresse. Lokalt kører backend på port 5248; i drift ligger API'et på samme domæne ("").
// Kan overskrives med VITE_API_URL i en .env-fil i frontend-mappen.
export const API_URL: string =
  import.meta.env.VITE_API_URL ?? (import.meta.env.DEV ? "http://localhost:5248" : "");

// withCredentials sender login-cookien med til backend
const api = axios.create({ baseURL: API_URL, withCredentials: true });

export type SiteText = {
  id: string;
  value: string;
  description?: string | null;
};

// Et billede eller en video i galleriet - samme form som en ydelses medier (ServiceMedia nedenfor)
export type GalleryImage = ServiceMedia;

export type Me = {
  isAuthenticated: boolean;
  isAdmin: boolean;
  email?: string;
  name?: string;
};

// --- Tekster ---
export const getTexts = () => api.get<SiteText[]>("/api/texts");
export const updateText = (id: string, value: string) => api.post<SiteText>("/api/texts", { id, value });

// --- Galleri ---
type Progress = (fraction: number) => void;
// Typet som Record<string, never>, fordi projektets gamle @types/axios (fra før axios 1.x) ikke kender
// onUploadProgress; axios 1.x understøtter det fint ved kørsel.
const progressOf = (onProgress?: Progress) =>
  (onProgress
    ? {
        onUploadProgress: (e: { loaded: number; total?: number }) => {
          if (e.total) onProgress(e.loaded / e.total);
        },
      }
    : {}) as unknown as Record<string, never>;

export const getGallery = () => api.get<GalleryImage[]>("/api/gallery");
export const uploadImages = (files: File[], onProgress?: Progress) => {
  const form = new FormData();
  files.forEach((f) => form.append("files", f));
  return api.post<GalleryImage[]>("/api/gallery", form, progressOf(onProgress));
};
export const uploadGalleryPoster = (id: number, poster: Blob) => {
  const form = new FormData();
  form.append("file", poster, "poster.jpg");
  return api.post<GalleryImage[]>(`/api/gallery/${id}/poster`, form);
};
export const addGalleryYouTube = (url: string) => api.post<GalleryImage[]>("/api/gallery/youtube", { url });
export type Storage = { usedBytes: number; freeBytes: number | null; totalBytes: number | null };
export const getGalleryStorage = () => api.get<Storage>("/api/gallery/storage");
export const deleteImage = (id: number) => api.delete<GalleryImage[]>(`/api/gallery/${id}`);
export const reorderImages = (ids: number[]) => api.put<GalleryImage[]>("/api/gallery/order", { ids });

// --- Ydelser ---
export type ServiceMedia = {
  id: number;
  kind: "image" | "video" | "youtube";
  url: string | null;
  youTubeId: string | null;
  // Stillbillede til en video (kun galleriets videoer, når de er valgt som baggrund)
  posterUrl?: string | null;
  // SHA-256 af filens indhold (samme fil = samme hash); kan mangle på gamle data
  hash?: string | null;
};

export type Service = {
  id: number;
  slug: string;
  title: string;
  shortText: string;
  body: string;
  icon: string;
  coverUrl: string | null;
  coverHash?: string | null;
  updatedAt: string;
  media: ServiceMedia[];
};

export type ServiceInput = {
  title: string;
  slug?: string;
  shortText: string;
  body: string;
  icon: string;
};

export const getServices = () => api.get<Service[]>("/api/services");
export const createService = (input: ServiceInput) => api.post<Service>("/api/services", input);
export const updateService = (id: number, input: ServiceInput) => api.put<Service[]>(`/api/services/${id}`, input);
export const deleteService = (id: number) => api.delete<Service[]>(`/api/services/${id}`);
export const reorderServices = (ids: number[]) => api.put<Service[]>("/api/services/order", { ids });
export const uploadServiceCover = (id: number, file: File, onProgress?: Progress) => {
  const form = new FormData();
  form.append("file", file);
  return api.post<Service[]>(`/api/services/${id}/cover`, form, progressOf(onProgress));
};
export const deleteServiceCover = (id: number) => api.delete<Service[]>(`/api/services/${id}/cover`);
export const uploadServiceMedia = (id: number, files: File[], onProgress?: Progress) => {
  const form = new FormData();
  files.forEach((f) => form.append("files", f));
  return api.post<Service[]>(`/api/services/${id}/media`, form, progressOf(onProgress));
};
export const addServiceYouTube = (id: number, url: string) => api.post<Service[]>(`/api/services/${id}/media/youtube`, { url });
export const deleteServiceMedia = (id: number, mediaId: number) => api.delete<Service[]>(`/api/services/${id}/media/${mediaId}`);
export const reorderServiceMedia = (id: number, ids: number[]) => api.put<Service[]>(`/api/services/${id}/media/order`, { ids });

// --- Statistik (kun admin) ---
export type Stats = {
  period: "today" | "7d" | "30d" | "year";
  from: string;
  to: string;
  uniqueVisitors: number;
  pageviews: number;
  totalClicks: number;
  clicks: { target: string; count: number }[];
  sources: { google: number; facebook: number; direct: number; other: number };
  countries: { code: string; visitors: number }[];
  cities: { city: string; country: string | null; visitors: number }[];
  mobile: number;
  desktop: number;
  pages: { path: string; pageviews: number }[];
  geoAvailable: boolean;
};
export const getStats = (period: Stats["period"]) => api.get<Stats>("/api/analytics/stats", { params: { period } });

// --- Login ---
export const getMe = () => api.get<Me>("/api/auth/me");
export const logout = () => api.post("/api/auth/logout");
export const loginUrl = () =>
  `${API_URL}/api/auth/login?returnUrl=${encodeURIComponent(window.location.origin + window.location.pathname)}`;

// --- Kontaktformular ---
export type ContactAttachment = {
  id: number;
  originalName: string;
  contentType: string;
  size: number;
};

export type ContactRequest = {
  id: number;
  name: string;
  phone: string;
  email: string | null;
  serviceKey: string;
  serviceTitle: string | null;
  message: string;
  createdAt: string;
  handledAt: string | null;
  attachments: ContactAttachment[];
};

export const submitContact = (form: FormData) => api.post<{ ok: boolean }>("/api/contact", form);
export const getContactRequests = () => api.get<ContactRequest[]>("/api/contact");
export const toggleContactHandled = (id: number) => api.post<ContactRequest>(`/api/contact/${id}/handled`);
export const deleteContactRequest = (id: number) => api.delete(`/api/contact/${id}`);
export const downloadContactAttachment = async (requestId: number, attachmentId: number, fileName: string) => {
  const res = await api.get<Blob>(`/api/contact/${requestId}/attachments/${attachmentId}`, { responseType: "blob" } as unknown as Record<string, never>);
  const url = URL.createObjectURL(res.data);
  const a = document.createElement("a");
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
};

// Gør en sti fra backend til en fuld URL
export const assetUrl = (path: string) =>
  /^https?:\/\//i.test(path) ? path : `${API_URL}${path.startsWith("/") ? "" : "/"}${path}`;

// Pæn fejltekst ud fra et axios-svar
type HttpError = { response?: { status?: number; data?: unknown }; request?: unknown };

export const errorMessage = (err: unknown): string => {
  const e = (err ?? {}) as HttpError;
  const status = e.response?.status;
  if (status === 401) return "Du er ikke logget ind længere. Log ind igen.";
  if (status === 403) return "Din konto har ikke adgang til at redigere.";
  if (status === 429) return "Du har sendt for mange henvendelser. Prøv igen om et kvarter.";
  const data = e.response?.data;
  if (typeof data === "string" && data) return data;
  if (data && typeof data === "object" && "error" in data && typeof (data as { error: unknown }).error === "string") {
    return (data as { error: string }).error;
  }
  if (!e.response && e.request) return "Kunne ikke få forbindelse til serveren.";
  return "Noget gik galt. Prøv igen.";
};
