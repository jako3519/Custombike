using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Dto
{
    public class ReorderDto
    {
        // Alle billed-id'er i den nye rækkefølge
        [Required]
        public List<int> Ids { get; set; } = new();
    }
}
