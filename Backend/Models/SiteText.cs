using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    // En redigerbar tekst på hjemmesiden. Id'et bestemmer hvor teksten vises (fx "about.title").
    public class SiteText
    {
        [Key]
        [MaxLength(100)]
        public string Id { get; set; } = "";

        [Required]
        public string Value { get; set; } = "";

        // Kort forklaring af hvor teksten bruges, så det er nemt at finde det rigtige id.
        [MaxLength(200)]
        public string? Description { get; set; }
    }
}
