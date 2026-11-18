import { useEffect, useState } from "react";
import Modal from "./Modal";
import {
  deleteContactRequest,
  downloadContactAttachment,
  errorMessage,
  getContactRequests,
  toggleContactHandled,
} from "../Api";
import type { ContactRequest } from "../Api";
import "./EnquiriesModal.css";

const fmtWhen = (iso: string) =>
  new Date(iso).toLocaleString("da-DK", { dateStyle: "short", timeStyle: "short" });

const fmtSize = (n: number) => (n < 1024 * 1024 ? `${Math.max(1, Math.round(n / 1024))} KB` : `${(n / (1024 * 1024)).toFixed(1)} MB`);

type EnquiriesModalProps = { onClose: () => void };

function EnquiriesModal({ onClose }: EnquiriesModalProps) {
  const [rows, setRows] = useState<ContactRequest[] | null>(null);
  const [error, setError] = useState("");
  const [busyId, setBusyId] = useState<number | null>(null);

  const load = async () => {
    try {
      const res = await getContactRequests();
      setRows(res.data);
      setError("");
    } catch (err) {
      setError(errorMessage(err));
    }
  };

  useEffect(() => {
    void load();
  }, []);

  const onToggle = async (row: ContactRequest) => {
    setBusyId(row.id);
    try {
      const res = await toggleContactHandled(row.id);
      setRows((all) => (all ? all.map((r) => (r.id === row.id ? res.data : r)) : all));
    } catch (err) {
      setError(errorMessage(err));
    } finally {
      setBusyId(null);
    }
  };

  const onDelete = async (row: ContactRequest) => {
    if (!window.confirm(`Slet henvendelsen fra ${row.name}? Det kan ikke fortrydes.`)) return;
    setBusyId(row.id);
    try {
      await deleteContactRequest(row.id);
      setRows((all) => (all ? all.filter((r) => r.id !== row.id) : all));
    } catch (err) {
      setError(errorMessage(err));
    } finally {
      setBusyId(null);
    }
  };

  const onDownload = async (row: ContactRequest, attachmentId: number, name: string) => {
    try {
      await downloadContactAttachment(row.id, attachmentId, name);
    } catch (err) {
      setError(errorMessage(err));
    }
  };

  return (
    <Modal title="Henvendelser" onClose={onClose} wide>
      {error && (
        <p className="enquiries-error" role="alert">
          {error}
        </p>
      )}
      {rows === null && !error && <p className="enquiries-empty">Henter henvendelser…</p>}
      {rows && rows.length === 0 && <p className="enquiries-empty">Ingen henvendelser endnu.</p>}
      {rows && rows.length > 0 && (
        <ul className="enquiries-list">
          {rows.map((row) => (
            <li key={row.id} className={row.handledAt ? "handled" : undefined}>
              <header className="enquiries-head">
                <div>
                  <strong>{row.name}</strong>
                  <span className="enquiries-meta">
                    {fmtWhen(row.createdAt)}
                    {row.handledAt ? ` · Besvaret ${fmtWhen(row.handledAt)}` : ""}
                  </span>
                </div>
                <div className="enquiries-actions">
                  <button type="button" disabled={busyId === row.id} onClick={() => void onToggle(row)}>
                    {row.handledAt ? "Markér som åben" : "Markér som besvaret"}
                  </button>
                  <button type="button" className="danger" disabled={busyId === row.id} onClick={() => void onDelete(row)}>
                    Slet
                  </button>
                </div>
              </header>
              <p className="enquiries-contact">
                Tlf.: <a href={`tel:${row.phone.replace(/\D/g, "")}`}>{row.phone}</a>
                {row.email && (
                  <>
                    {" · "}
                    Mail: <a href={`mailto:${row.email}`}>{row.email}</a>
                  </>
                )}
                {row.serviceTitle && <> · {row.serviceTitle}</>}
              </p>
              <p className="enquiries-message">{row.message}</p>
              {row.attachments.length > 0 && (
                <ul className="enquiries-files">
                  {row.attachments.map((file) => (
                    <li key={file.id}>
                      <button type="button" onClick={() => void onDownload(row, file.id, file.originalName)}>
                        {file.originalName} ({fmtSize(file.size)})
                      </button>
                    </li>
                  ))}
                </ul>
              )}
            </li>
          ))}
        </ul>
      )}
    </Modal>
  );
}

export default EnquiriesModal;
