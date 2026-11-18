using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    // En henvendelse fra kontaktformularen. Gemmes i op til 12 måneder (ryddes op automatisk).
    // IP-adresser gemmes ikke. Vedhæftede billeder ligger i wwwroot/gallery/contact (ikke offentligt).
    public class ContactRequest
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = "";

        [Required]
        [MaxLength(40)]
        public string Phone { get; set; } = "";

        [MaxLength(254)]
        public string? Email { get; set; }

        // Ydelsens slug, eller "andet"
        [Required]
        [MaxLength(80)]
        public string ServiceKey { get; set; } = "";

        // Titel på indsendelsestidspunktet (så den stadig vises, hvis ydelsen slettes)
        [MaxLength(120)]
        public string? ServiceTitle { get; set; }

        [Required]
        [MaxLength(4000)]
        public string Message { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? HandledAt { get; set; }

        public List<ContactAttachment> Attachments { get; set; } = new();
    }
}
