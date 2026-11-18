using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    // Et billede eller en video i en ydelses eget galleri.
    // Kind: "image" (fil), "video" (mp4/webm-fil) eller "youtube" (kun video-id'et gemmes).
    public class ServiceMedia
    {
        public int Id { get; set; }

        public int ServiceId { get; set; }
        public Service? Service { get; set; }

        [Required]
        [MaxLength(16)]
        public string Kind { get; set; } = "image";

        // Filnavn i wwwroot/gallery/services (image/video), ellers null
        [MaxLength(255)]
        public string? FileName { get; set; }

        // SHA-256 af filens indhold (64 hex-tegn). Bruges til at vise det samme billede kun én gang.
        [MaxLength(64)]
        public string? ContentHash { get; set; }

        // YouTube-video-id (11 tegn), kun for Kind = "youtube"
        [MaxLength(11)]
        public string? YouTubeId { get; set; }

        // Lavest vises først
        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
