namespace KMCEventClient.Models
{
    public class RegistrationViewModel
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int ParticipantId { get; set; }
        public string EventTitle { get; set; } = "";
        public string ParticipantName { get; set; } = "";
        public System.DateTime RegistrationDate { get; set; }
    }
}
