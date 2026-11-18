import { useState } from "react";
import type { FormEvent } from "react";
import Modal from "./Modal";
import { updateText, errorMessage } from "../Api";
import { useTextStore } from "../texts/TextContext";
import "./ServiceAdmin.css";
import type { TextId } from "../texts/defaultTexts";

// Tekster der skal have et stort tekstfelt. De understøtter også afsnit og "## Overskrift ##".
const MULTILINE: TextId[] = [
  "about.body",
  "hero.subtitle",
  "gallery.subtitle",
  "services.subtitle",
  "services.note",
  "company.footerText",
  "cookie.body",
  "contact.formIntro",
  "contact.gdpr",
  "contact.thanksText",
];

type TextEditModalProps = {
  ids: TextId[];
  onClose: () => void;
};

function TextEditModal({ ids, onClose }: TextEditModalProps) {
  const { texts, descriptions, setText } = useTextStore();
  const [values, setValues] = useState<Record<string, string>>(() =>
    Object.fromEntries(ids.map((id) => [id, texts[id] ?? ""]))
  );
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError("");
    try {
      // Gem kun de felter der er ændret
      for (const id of ids) {
        if (values[id] !== texts[id]) {
          const res = await updateText(id, values[id]);
          setText(id, res.data.value);
        }
      }
      onClose();
    } catch (err) {
      setError(errorMessage(err));
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal title="Rediger tekst" onClose={onClose}>
      <form onSubmit={handleSubmit} className="admin-form">
        {ids.map((id) => {
          const multiline = MULTILINE.includes(id) || (values[id] ?? "").includes("\n");
          return (
            <label key={id} className="admin-field">
              <span className="admin-field-label">{descriptions[id] ?? id}</span>
              {multiline ? (
                <textarea
                  rows={id === "about.body" || id === "cookie.body" ? 9 : 3}
                  value={values[id]}
                  onChange={(e) => setValues((v) => ({ ...v, [id]: e.target.value }))}
                />
              ) : (
                <input
                  type="text"
                  value={values[id]}
                  onChange={(e) => setValues((v) => ({ ...v, [id]: e.target.value }))}
                />
              )}
              {MULTILINE.includes(id) && (
                <span className="admin-field-help">
                  Skriv <code>## Overskrift ##</code> på en linje for sig for at lave en overskrift. En tom linje giver et nyt afsnit.
                </span>
              )}
              <span className="admin-field-id">{id}</span>
            </label>
          );
        })}

        {error && <div className="admin-error">{error}</div>}

        <div className="admin-actions">
          <button type="button" className="admin-btn secondary" onClick={onClose}>
            Annuller
          </button>
          <button type="submit" className="admin-btn" disabled={saving}>
            {saving ? "Gemmer…" : "Gem"}
          </button>
        </div>
      </form>
    </Modal>
  );
}

export default TextEditModal;
