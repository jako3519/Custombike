using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    // Et billede eller en video i galleriet ("Billeder" på forsiden).
    // Kind: "image" (fil), "video" (mp4/webm-fil) eller "youtube" (kun video-id'et gemmes) - som ydelsernes medier.
    // Filerne ligger i wwwroot/gallery.
    public class GalleryImage
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(16)]
        public string Kind { get; set; } = "image";

        // Filnavn i wwwroot/gallery (image/video), ellers null
        [MaxLength(255)]
        public string? FileName { get; set; }

        // YouTube-video-id (11 tegn), kun for Kind = "youtube"
        [MaxLength(11)]
        public string? YouTubeId { get; set; }

        // Stillbillede til en video (i wwwroot/gallery/posters). Laves i admins browser, når videoen
        // vælges som baggrund på forsiden, og vises før/i stedet for videoen (mobil, spar-data osv.).
        [MaxLength(255)]
        public string? PosterFileName { get; set; }

        // SHA-256 af filens indhold (64 hex-tegn). Bruges til at vise det samme billede kun én gang.
        [MaxLength(64)]
        public string? ContentHash { get; set; }

        // Lavest vises først
        public int SortOrder { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
