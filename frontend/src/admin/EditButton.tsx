import type { MouseEvent } from "react";
import { useAdmin } from "./AdminContext";
import type { TextId } from "../texts/defaultTexts";
import "./Admin.css";

type EditButtonProps = {
  // Tekst(er) der redigeres – eller onClick for fx galleriet
  ids?: TextId[];
  onClick?: () => void;
  label?: string;
  className?: string;
};

// Den lille bolle med kryds, som kun vises for admins
function EditButton({ ids, onClick, label = "Rediger", className = "" }: EditButtonProps) {
  const { isAdmin, editTexts } = useAdmin();
  if (!isAdmin) return null;

  const handleClick = (e: MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (onClick) onClick();
    else if (ids) editTexts(ids);
  };

  return (
    <button type="button" className={`edit-btn ${className}`} onClick={handleClick} aria-label={label} title={label}>
      <svg viewBox="0 0 24 24" width="14" height="14" aria-hidden="true">
        <path d="M12 5v14M5 12h14" stroke="currentColor" strokeWidth="3.2" strokeLinecap="round" />
      </svg>
    </button>
  );
}

export default EditButton;
