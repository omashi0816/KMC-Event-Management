using System.ComponentModel.DataAnnotations;

namespace KMCEventAPI.Models
{
    public class Registration
    {
        [Key]
        public int RegistrationId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int ParticipantId { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public Event? Event { get; set; }

        public Participant? Participant { get; set; }
    }
}