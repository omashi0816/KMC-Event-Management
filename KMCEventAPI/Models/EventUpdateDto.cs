using System.ComponentModel.DataAnnotations;

namespace KMCEventAPI.Models
{
    public class EventUpdateDto
    {
        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public string Type { get; set; } = "";

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Time { get; set; } = "";

        [Required]
        public string Location { get; set; } = "";

        [Range(0, int.MaxValue)]
        public int Capacity { get; set; }
    }
}
