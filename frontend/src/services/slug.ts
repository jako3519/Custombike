// Samme regler som Backend/Content/Slug.cs: "Vinduer og døre" -> "vinduer-og-doere"
export function slugify(input: string): string {
  const s = input
    .trim()
    .toLowerCase()
    .replace(/æ/g, "ae")
    .replace(/ø/g, "oe")
    .replace(/å/g, "aa")
    .replace(/ä/g, "ae")
    .replace(/ö/g, "oe")
    .replace(/ü/g, "ue")
    .replace(/ß/g, "ss")
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "");
  return s.length > 80 ? s.slice(0, 80).replace(/-+$/, "") : s;
}
