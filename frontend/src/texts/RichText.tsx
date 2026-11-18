import { Fragment, createElement } from "react";
import { parseRichText } from "./richTextParse";

type RichTextProps = {
  text: string;
  // Overskrifter fra "## ... ##" bliver h2/h3/h4 - vælg det niveau der passer under sektionens overskrift
  headingLevel?: 2 | 3 | 4 | 5;
  className?: string;
};

// Viser en redigerbar tekst med afsnit, linjeskift og "## Overskrift ##"-overskrifter
function RichText({ text, headingLevel = 3, className }: RichTextProps) {
  const blocks = parseRichText(text);
  if (blocks.length === 0) return null;

  return (
    <div className={className ? `rich-text ${className}` : "rich-text"}>
      {blocks.map((block, i) =>
        block.kind === "heading" ? (
          createElement(`h${headingLevel}`, { key: i, className: "rich-heading" }, block.text)
        ) : (
          <p key={i}>
            {block.lines.map((line, j) => (
              <Fragment key={j}>
                {j > 0 && <br />}
                {line}
              </Fragment>
            ))}
          </p>
        )
      )}
    </div>
  );
}

export default RichText;
