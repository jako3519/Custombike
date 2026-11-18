import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import type { ReactNode } from "react";
import { getMe, logout as apiLogout } from "../Api";
import type { Me } from "../Api";
import type { TextId } from "../texts/defaultTexts";
import TextEditModal from "./TextEditModal";
import GalleryEditModal from "./GalleryEditModal";
import ServicesManagerModal from "./ServicesManagerModal";
import ServiceEditModal from "./ServiceEditModal";

type Editor =
  | { kind: "text"; ids: TextId[] }
  | { kind: "gallery" }
  | { kind: "services" }
  // id null = ny ydelse. fromList: tilbage til listen, når man lukker. media: rul ned til billeder/video
  | { kind: "service"; id: number | null; fromList?: boolean; media?: boolean }
  | null;

type AdminContextValue = {
  me: Me | null;
  isAdmin: boolean;
  notice: string;
  dismissNotice: () => void;
  logout: () => Promise<void>;
  editTexts: (ids: TextId[]) => void;
  editGallery: () => void;
  editServices: () => void;
  editService: (id: number | null, media?: boolean) => void;
};

const AdminContext = createContext<AdminContextValue>({
  me: null,
  isAdmin: false,
  notice: "",
  dismissNotice: () => {},
  logout: async () => {},
  editTexts: () => {},
  editGallery: () => {},
  editServices: () => {},
  editService: () => {},
});

export function AdminProvider({ children }: { children: ReactNode }) {
  const [me, setMe] = useState<Me | null>(null);
  const [editor, setEditor] = useState<Editor>(null);
  const [notice, setNotice] = useState("");

  useEffect(() => {
    getMe()
      .then((res) => setMe(res.data))
      .catch(() => setMe({ isAuthenticated: false, isAdmin: false }));

    // Backend sender ?login=denied tilbage, hvis kontoen ikke er admin
    const params = new URLSearchParams(window.location.search);
    const login = params.get("login");
    if (login === "denied" || login === "failed") {
      setNotice(
        login === "denied"
          ? "Den Google-konto har ikke adgang til at redigere siden."
          : "Login lykkedes ikke. Prøv igen via /admin."
      );
      params.delete("login");
      const query = params.toString();
      window.history.replaceState(null, "", window.location.pathname + (query ? `?${query}` : "") + window.location.hash);
    }
  }, []);

  const logout = useCallback(async () => {
    try {
      await apiLogout();
    } finally {
      setMe({ isAuthenticated: false, isAdmin: false });
      setEditor(null);
    }
  }, []);

  const editTexts = useCallback((ids: TextId[]) => setEditor({ kind: "text", ids }), []);
  const editGallery = useCallback(() => setEditor({ kind: "gallery" }), []);
  const editServices = useCallback(() => setEditor({ kind: "services" }), []);
  const editService = useCallback((id: number | null, media?: boolean) => setEditor({ kind: "service", id, media }), []);
  const close = useCallback(() => setEditor(null), []);
  const closeService = useCallback(
    () => setEditor((e) => (e?.kind === "service" && e.fromList ? { kind: "services" } : null)),
    []
  );
  const openFromList = useCallback((id: number | null) => setEditor({ kind: "service", id, fromList: true }), []);
  const dismissNotice = useCallback(() => setNotice(""), []);

  const value = useMemo(
    () => ({ me, isAdmin: !!me?.isAdmin, notice, dismissNotice, logout, editTexts, editGallery, editServices, editService }),
    [me, notice, dismissNotice, logout, editTexts, editGallery, editServices, editService]
  );

  return (
    <AdminContext.Provider value={value}>
      {children}
      {value.isAdmin && editor?.kind === "text" && <TextEditModal ids={editor.ids} onClose={close} />}
      {value.isAdmin && editor?.kind === "gallery" && <GalleryEditModal onClose={close} />}
      {value.isAdmin && editor?.kind === "services" && <ServicesManagerModal onClose={close} onEdit={openFromList} />}
      {value.isAdmin && editor?.kind === "service" && (
        <ServiceEditModal
          key={editor.id ?? "new"}
          id={editor.id}
          focusMedia={editor.media}
          onClose={closeService}
          onCreated={(id) => setEditor((e) => (e?.kind === "service" ? { ...e, id } : e))}
        />
      )}
    </AdminContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAdmin() {
  return useContext(AdminContext);
}
