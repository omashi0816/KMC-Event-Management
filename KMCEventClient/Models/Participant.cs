namespace KMCEventClient.Models
{
    public class Participant
    {
        // Server uses ParticipantId; keep both for compatibility
        public int ParticipantId { get; set; }

        // Backwards-compatible Id property used by views/controllers
        public int Id
        {
            get => ParticipantId;
            set => ParticipantId = value;
        }

        // Server sends a single Name field; client keeps FirstName/LastName for UI
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                _name = value ?? string.Empty;
                // If FirstName/LastName not set, attempt to split
                if (string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName))
                {
                    var parts = _name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 1)
                    {
                        FirstName = parts[0];
                        LastName = string.Empty;
                    }
                    else if (parts.Length >= 2)
                    {
                        FirstName = parts[0];
                        LastName = parts[1];
                    }
                }
            }
        }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public string FullName => string.IsNullOrWhiteSpace(Name) ? $"{FirstName} {LastName}".Trim() : Name;
    }
}
