import { useMemo } from "react";
import "./Gallery.css";
import { useText } from "../../texts/TextContext";
import { useGallery } from "../../gallery/GalleryContext";
import { useServices } from "../../services/ServicesContext";
import { uniqueGalleryImages } from "../../media/unique";
import EditButton from "../../admin/EditButton";
import MediaGallery from "../media/MediaGallery";

// Galleriet på forsiden: billeder, videoer og YouTube i samme ensartede gitter og lightbox som ydelserne
function Gallery() {
  const { images: allImages } = useGallery();
  const { services } = useServices();
  const companyName = useText("company.name");
  // Samme billede vises kun én gang (heller ikke hvis det allerede er forsidebillede på en ydelse)
  const items = useMemo(() => uniqueGalleryImages(allImages, services), [allImages, services]);
  const emptyText = useText("gallery.empty");

  if (items.length === 0) {
    return (
      <p className="gallery-empty">
        {emptyText}
        <EditButton ids={["gallery.empty"]} label="Rediger tekst" />
      </p>
    );
  }

  return <MediaGallery media={items} title={`Opgave udført af ${companyName}`} variant="gallery" trackPrefix="gallery" />;
}

export default Gallery;
