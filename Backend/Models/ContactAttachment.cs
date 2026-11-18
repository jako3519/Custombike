using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    // Et billede vedhæftet en henvendelse (jpg, png, webp eller heic).
    public class ContactAttachment
    {
        public int Id { get; set; }
        public int ContactRequestId { get; set; }

        // Navn på disken (tilfældigt); ligger i wwwroot/gallery/contact
        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = "";

        [MaxLength(255)]
        public string OriginalName { get; set; } = "";

        [MaxLength(64)]
        public string ContentType { get; set; } = "application/octet-stream";

        public long Size { get; set; }

        public ContactRequest Request { get; set; } = null!;
    }
}
