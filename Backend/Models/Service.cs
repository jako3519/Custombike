using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    // En ydelse (fx "Tagarbejde"). Vises som kort på forsiden og har sin egen side /ydelser/<slug>.
    // Billeder/videoer ligger i ServiceMedia; filerne ligger i wwwroot/gallery/services (samme volume som galleriet).
    public class Service
    {
        public int Id { get; set; }

        // Adressen: /ydelser/<slug> (kun a-z, 0-9 og bindestreg; æ->ae, ø->oe, å->aa)
        [Required]
        [MaxLength(80)]
        public string Slug { get; set; } = "";

        [Required]
        [MaxLength(120)]
        public string Title { get; set; } = "";

        // Teksten på kortet på forsiden (understøtter ## Overskrift ##)
        [MaxLength(2000)]
        public string ShortText { get; set; } = "";

        // Teksten på ydelsens egen side (understøtter ## Overskrift ##)
        [MaxLength(20000)]
        public string Body { get; set; } = "";

        // Nøgle fra ikonsættet på ydelseskortene (roof, deck, carport, window, interior, facade)
        [Required]
        [MaxLength(32)]
        public string Icon { get; set; } = "roof";

        // Filnavn på forsidebilledet til kortet/siden (i wwwroot/gallery/services), eller null
        [MaxLength(255)]
        public string? CoverImage { get; set; }

        // SHA-256 af forsidebilledets indhold (64 hex-tegn)
        [MaxLength(64)]
        public string? CoverHash { get; set; }

        // Lavest vises først
        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<ServiceMedia> Media { get; set; } = new();
    }
}
