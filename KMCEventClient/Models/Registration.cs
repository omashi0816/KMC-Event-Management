namespace KMCEventClient.Models
{
    public class Registration
    {
        // Server uses RegistrationId; keep both for compatibility
        public int RegistrationId { get; set; }

        public int Id
        {
            get => RegistrationId;
            set => RegistrationId = value;
        }
        public int EventId { get; set; }
        public int ParticipantId { get; set; }

        public DateTime RegistrationDate { get; set; }

        public Event? Event { get; set; }
        public Participant? Participant { get; set; }
    }
}
