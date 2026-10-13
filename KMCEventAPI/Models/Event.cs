namespace KMCEventAPI.Models
{
    public class Event
    {
        public int EventId { get; set; }

        public int OrganizerId { get; set; }

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public string Type { get; set; } = "";

        public DateTime Date { get; set; }

        public string Time { get; set; } = "";

        public string Location { get; set; } = "";

        public int Capacity { get; set; }
    }
}