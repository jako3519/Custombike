import { useEffect, useRef } from "react";
import type { ReactNode } from "react";
import { createPortal } from "react-dom";
import "./Admin.css";

type ModalProps = {
  title: string;
  onClose: () => void;
  children: ReactNode;
  wide?: boolean;
};

function Modal({ title, onClose, children, wide }: ModalProps) {
  const dialogRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => e.key === "Escape" && onClose();
    document.addEventListener("keydown", onKey);
    // Fokus på første felt
    const first = dialogRef.current?.querySelector<HTMLElement>("textarea, input, button");
    first?.focus();
    return () => document.removeEventListener("keydown", onKey);
  }, [onClose]);

  return createPortal(
    <div className="admin-backdrop" data-admin-modal onMouseDown={(e) => e.target === e.currentTarget && onClose()}>
      <div
        className={`admin-modal${wide ? " wide" : ""}`}
        role="dialog"
        aria-modal="true"
        aria-label={title}
        ref={dialogRef}
      >
        <div className="admin-modal-header">
          <span>{title}</span>
          <button className="admin-modal-close" onClick={onClose} aria-label="Luk">
            <svg viewBox="0 0 24 24" width="20" height="20" aria-hidden="true">
              <path d="M6 6l12 12M18 6L6 18" stroke="currentColor" strokeWidth="3" strokeLinecap="round" />
            </svg>
          </button>
        </div>
        <div className="admin-modal-body">{children}</div>
      </div>
    </div>,
    document.body
  );
}

export default Modal;
