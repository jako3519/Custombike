import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import type { ReactNode } from "react";
import { getServices } from "../Api";
import type { Service } from "../Api";

type ServicesContextValue = {
  services: Service[];
  // false indtil listen er hentet (bruges så en ydelsesside ikke viser "findes ikke" for tidligt)
  loaded: boolean;
  setServices: (services: Service[]) => void;
  reload: () => Promise<void>;
};

const ServicesContext = createContext<ServicesContextValue>({
  services: [],
  loaded: false,
  setServices: () => {},
  reload: async () => {},
});

// initial: ydelser som serveren har lagt ind i siden
export function ServicesProvider({ children, initial }: { children: ReactNode; initial?: Service[] }) {
  const [services, setServicesState] = useState<Service[]>(initial ?? []);
  const [loaded, setLoaded] = useState(initial !== undefined);

  const setServices = useCallback((next: Service[]) => {
    setServicesState(next);
    setLoaded(true);
  }, []);

  const reload = useCallback(async () => {
    try {
      const res = await getServices();
      if (Array.isArray(res.data)) setServicesState(res.data);
    } catch {
      // Backend svarer ikke - det, serveren sendte med siden, bliver stående
    } finally {
      setLoaded(true);
    }
  }, []);

  useEffect(() => {
    void reload();
  }, [reload]);

  const value = useMemo(() => ({ services, loaded, setServices, reload }), [services, loaded, setServices, reload]);
  return <ServicesContext.Provider value={value}>{children}</ServicesContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useServices() {
  return useContext(ServicesContext);
}
