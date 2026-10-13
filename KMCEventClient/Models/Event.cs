using System;
using System.Text.Json.Serialization;

namespace KMCEventClient.Models
{
    public class Event
    {
        // API uses EventId; keep EventId for deserialization and expose Id for compatibility
        public int EventId { get; set; }

        [JsonIgnore]
        public int Id
        {
            get => EventId;
            set => EventId = value;
        }

        public int OrganizerId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Type { get; set; }

        // API separates Date and Time; keep them for deserialization
        public DateTime Date { get; set; }
        public string? Time { get; set; }

        // Backwards-compatible property used by views
        [JsonIgnore]
        public DateTime StartDate
        {
            get
            {
                if (Date == default) return DateTime.MinValue;
                if (Time != null && Time.Length >= 4)
                {
                    if (TimeSpan.TryParse(Time, out var ts))
                    {
                        return Date.Date + ts;
                    }
                }
                return Date;
            }
            set
            {
                Date = value.Date;
                Time = value.ToString("HH:mm");
            }
        }

        // Backwards-compatible optional EndDate used by views; not serialized to API
        [JsonIgnore]
        public DateTime? EndDate { get; set; }

        // Optional fields
        public int Capacity { get; set; }
        public string? Location { get; set; }
    }
}
