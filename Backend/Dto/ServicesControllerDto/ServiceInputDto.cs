using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Dto
{
    // Oprettelse/redigering af en ydelse (POST /api/services og PUT /api/services/5)
    public class ServiceInputDto
    {
        [Required]
        public string Title { get; set; } = "";

        // Tom = laves ud fra titlen (kun ved oprettelse; ved redigering beholdes den gamle adresse)
        public string? Slug { get; set; }

        public string ShortText { get; set; } = "";
        public string Body { get; set; } = "";

        [Required]
        public string Icon { get; set; } = "roof";
    }

    // POST /api/services/5/media/youtube { "url": "https://youtu.be/..." }
    public class YouTubeLinkDto
    {
        [Required]
        public string Url { get; set; } = "";
    }
}
