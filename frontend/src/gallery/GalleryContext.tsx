import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import type { ReactNode } from "react";
import { getGallery } from "../Api";
import type { GalleryImage } from "../Api";

type GalleryContextValue = {
  images: GalleryImage[];
  setImages: (images: GalleryImage[]) => void;
  reload: () => Promise<void>;
};

const GalleryContext = createContext<GalleryContextValue>({
  images: [],
  setImages: () => {},
  reload: async () => {},
});

// initial: billeder som serveren har lagt ind i siden
export function GalleryProvider({ children, initial }: { children: ReactNode; initial?: GalleryImage[] }) {
  const [images, setImages] = useState<GalleryImage[]>(initial ?? []);

  const reload = useCallback(async () => {
    try {
      const res = await getGallery();
      setImages(Array.isArray(res.data) ? res.data : []);
    } catch {
      setImages([]);
    }
  }, []);

  useEffect(() => {
    void reload();
  }, [reload]);

  const value = useMemo(() => ({ images, setImages, reload }), [images, reload]);
  return <GalleryContext.Provider value={value}>{children}</GalleryContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useGallery() {
  return useContext(GalleryContext);
}
