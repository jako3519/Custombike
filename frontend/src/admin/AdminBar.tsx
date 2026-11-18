import { useState } from "react";
import { useAdmin } from "./AdminContext";
import StatsModal from "./StatsModal";
import EnquiriesModal from "./EnquiriesModal";
import "./Admin.css";

// Lille bjælke nederst til venstre, der viser at man er i redigeringstilstand
function AdminBar() {
  const { me, isAdmin, logout, notice, dismissNotice } = useAdmin();
  const [statsOpen, setStatsOpen] = useState(false);
  const [enquiriesOpen, setEnquiriesOpen] = useState(false);

  return (
    <>
      {isAdmin && (
        <div className="admin-bar" role="status">
          <span className="admin-bar-dot" />
          <span className="admin-bar-text">
            Redigering · <strong>{me?.name || me?.email}</strong>
          </span>
          <button className="admin-bar-logout admin-bar-stats" onClick={() => setEnquiriesOpen(true)}>
            Henvendelser
          </button>
          <button className="admin-bar-logout admin-bar-stats" onClick={() => setStatsOpen(true)}>
            Statistik
          </button>
          <button className="admin-bar-logout" onClick={() => void logout()}>
            Log ud
          </button>
        </div>
      )}
      {isAdmin && statsOpen && <StatsModal onClose={() => setStatsOpen(false)} />}
      {isAdmin && enquiriesOpen && <EnquiriesModal onClose={() => setEnquiriesOpen(false)} />}
      {notice && (
        <div className="admin-toast" role="alert">
          <span>{notice}</span>
          <button onClick={dismissNotice} aria-label="Luk">
            ✕
          </button>
        </div>
      )}
    </>
  );
}

export default AdminBar;
