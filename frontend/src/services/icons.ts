// Ikonsættet fra ydelseskortene (enkle streg-ikoner, 24x24). Nøglerne gemmes på ydelsen (Service.Icon)
// og skal matche listen i Backend/Content/ServiceData.cs.
export const SERVICE_ICONS = [
  { key: "roof", label: "Tag", path: "M2.5 12.5 12 4l9.5 8.5M5.5 10.2V20h13v-9.8M10 20v-5h4v5" },
  { key: "deck", label: "Terrasse", path: "M3 9.5h18M3 13.5h18M3 17.5h18M6 9.5v10M18 9.5v10M8 5.5h8" },
  { key: "carport", label: "Carport / tilbygning", path: "M2.5 9.5 12 4.5l9.5 5M5 9.5V20M19 9.5V20M7.5 17h9M8.5 17l1-3h5l1 3" },
  { key: "window", label: "Vindue / dør", path: "M5 3.5h14v17H5zM12 3.5v17M5 12h14" },
  { key: "interior", label: "Indvendigt", path: "M3.5 20.5h17M6 20.5v-8M18 20.5v-8M4 12.5h16M8 12.5V8h8v4.5M10 8V5h4v3" },
  { key: "facade", label: "Facade", path: "M4 20.5V8.5l8-5 8 5v12M8 20.5V10M12 20.5V8M16 20.5V10" },
] as const;

export type ServiceIconKey = (typeof SERVICE_ICONS)[number]["key"];

export function iconPath(key: string): string {
  return (SERVICE_ICONS.find((i) => i.key === key) ?? SERVICE_ICONS[0]).path;
}
