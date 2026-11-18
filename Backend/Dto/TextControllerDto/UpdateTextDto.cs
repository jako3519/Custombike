using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Dto
{
    public class UpdateTextDto
    {
        [Required]
        public string Id { get; set; } = "";

        [Required(AllowEmptyStrings = true)]
        public string Value { get; set; } = "";
    }
}
