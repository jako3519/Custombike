// Enkel opmærkning i de redigerbare tekster:
//   ## Overskrift ##   (en linje for sig) -> overskrift
//   tom linje          -> nyt afsnit
//   linjeskift         -> linjeskift i samme afsnit
// Teksten bliver aldrig til HTML: den deles op i blokke, som React selv bygger elementer af.
export type RichBlock = { kind: "heading"; text: string } | { kind: "paragraph"; lines: string[] };

const HEADING = /^##(.*?)(?:##)?$/;

export function parseRichText(text: string | null | undefined): RichBlock[] {
  const blocks: RichBlock[] = [];
  let lines: string[] = [];
  const flush = () => {
    if (lines.length > 0) blocks.push({ kind: "paragraph", lines });
    lines = [];
  };

  for (const raw of (text ?? "").replace(/\r\n?/g, "\n").split("\n")) {
    const line = raw.trim();
    if (!line) {
      flush(); // en eller flere tomme linjer = ét nyt afsnit
      continue;
    }
    const heading = HEADING.exec(line);
    if (heading) {
      flush();
      const title = heading[1].replace(/##/g, "").trim();
      if (title) blocks.push({ kind: "heading", text: title });
      continue;
    }
    lines.push(line);
  }
  flush();
  return blocks;
}

// Samme tekst uden opmærkning på én linje (fx til alt-tekster)
export function plainText(text: string | null | undefined): string {
  return parseRichText(text)
    .map((b) => (b.kind === "heading" ? b.text : b.lines.join(" ")))
    .join(" ");
}
