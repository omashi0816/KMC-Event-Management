using KMCEventClient.Models;

namespace KMCEventClient.Services
{
    public interface IApiClient
    {
        // =========================================================
        // EVENTS
        // =========================================================

        Task<IEnumerable<Event>> GetEventsAsync();

        Task<Event?> GetEventAsync(int id);

        Task<IEnumerable<Event>> SearchEventsAsync(string? type, DateTime? date, string? location);

        Task<Event?> CreateEventAsync(Event ev);

        Task<bool> UpdateEventAsync(
            int id,
            Event ev);

        Task<bool> DeleteEventAsync(int id);

        Task<IEnumerable<Event>> GetMyEventsAsync();


        // =========================================================
        // PARTICIPANTS
        // =========================================================

        Task<IEnumerable<Participant>>
            GetParticipantsAsync();

        Task<Participant?>
            GetParticipantAsync(int id);

        Task<Participant?>
            CreateParticipantAsync(
                Participant participant);

        Task<bool>
            UpdateParticipantAsync(
                int id,
                Participant participant);

        Task<bool>
            DeleteParticipantAsync(int id);


        // =========================================================
        // REGISTRATIONS
        // =========================================================

        Task<IEnumerable<Registration>>
            GetRegistrationsAsync();

        Task<Registration?>
            GetRegistrationAsync(int id);

        Task<(Registration? registration, string? errorMessage)>
            CreateRegistrationAsync(
                Registration registration);

        Task<bool>
            DeleteRegistrationAsync(int id);

        // Authentication
        Task<string?> LoginAsync(LoginRequest req);
        Task<bool> RegisterAsync(RegisterRequest req);
    }
}