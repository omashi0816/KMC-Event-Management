namespace KMCEventAPI.Models
{
    public class Participant
    {
        public int ParticipantId { get; set; }

        public string Name { get; set; } = "";

        public string Email { get; set; } = "";

        public string Phone { get; set; } = "";
    }
}