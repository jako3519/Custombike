import { iconPath } from "./icons";

// Et ikon fra ydelsernes ikonsæt (dekorativt)
function ServiceIcon({ icon, size = 30 }: { icon: string; size?: number }) {
  return (
    <svg viewBox="0 0 24 24" width={size} height={size} aria-hidden="true">
      <path d={iconPath(icon)} fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

export default ServiceIcon;
