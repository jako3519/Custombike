import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import type { ReactNode } from "react";
import { getTexts } from "../Api";
import type { SiteText } from "../Api";
import { defaultTexts } from "./defaultTexts";
import type { TextId } from "./defaultTexts";

type Texts = Record<string, string>;

type TextContextValue = {
  texts: Texts;
  descriptions: Record<string, string>;
  setText: (id: string, value: string) => void;
};

const TextContext = createContext<TextContextValue>({
  texts: defaultTexts,
  descriptions: {},
  setText: () => {},
});

// Omdanner listen fra /api/texts til opslag: id -> tekst og id -> beskrivelse
function toMaps(list: SiteText[]) {
  const values: Texts = {};
  const descs: Record<string, string> = {};
  for (const t of list) {
    values[t.id] = t.value;
    if (t.description) descs[t.id] = t.description;
  }
  return { values, descs };
}

// initial: tekster som serveren har lagt ind i siden, så der ikke vises standardtekster først
export function TextProvider({ children, initial }: { children: ReactNode; initial?: SiteText[] }) {
  const [texts, setTexts] = useState<Texts>(() =>
    initial ? { ...defaultTexts, ...toMaps(initial).values } : defaultTexts
  );
  const [descriptions, setDescriptions] = useState<Record<string, string>>(() =>
    initial ? toMaps(initial).descs : {}
  );

  useEffect(() => {
    getTexts()
      .then((res) => {
        if (!Array.isArray(res.data)) return;
        const values: Texts = {};
        const descs: Record<string, string> = {};
        for (const t of res.data) {
          values[t.id] = t.value;
          if (t.description) descs[t.id] = t.description;
        }
        setTexts({ ...defaultTexts, ...values });
        setDescriptions(descs);
      })
      .catch(() => {
        // Backend svarer ikke – standardteksterne bliver stående
      });
  }, []);

  const setText = useCallback((id: string, value: string) => {
    setTexts((prev) => ({ ...prev, [id]: value }));
  }, []);

  const value = useMemo(() => ({ texts, descriptions, setText }), [texts, descriptions, setText]);
  return <TextContext.Provider value={value}>{children}</TextContext.Provider>;
}

// Hent en tekst ud fra dens id, fx useText("about.title")
// eslint-disable-next-line react-refresh/only-export-components
export function useText(id: TextId): string {
  const { texts } = useContext(TextContext);
  return texts[id] ?? defaultTexts[id];
}

// Adgang til alle tekster, beskrivelser og opdatering (bruges af redigering)
// eslint-disable-next-line react-refresh/only-export-components
export function useTextStore() {
  return useContext(TextContext);
}
