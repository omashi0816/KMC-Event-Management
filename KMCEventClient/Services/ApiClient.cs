using System.Net.Http.Json;
using KMCEventClient.Models;

namespace KMCEventClient.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiClient(
        HttpClient http,
        IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthHeaderFromSession()
        {
            var context = _httpContextAccessor.HttpContext;
            var token = context?.Session.GetString("JwtToken");

            _http.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer",
                        token);
            }
        }

        // =========================
        // AUTHENTICATION
        // =========================

        public async Task<string?> LoginAsync(LoginRequest req)
        {
            var response = await _http.PostAsJsonAsync("api/Auth/login", req);
            if (!response.IsSuccessStatusCode) return null;

            var doc = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonDocument>();
            if (doc == null) return null;
            if (doc.RootElement.TryGetProperty("token", out var t))
            {
                return t.GetString();
            }
            return null;
        }

        public async Task<bool> RegisterAsync(RegisterRequest req)
        {
            var response = await _http.PostAsJsonAsync(
                "api/Auth/register",
                req);

            return response.StatusCode ==
                   System.Net.HttpStatusCode.Created;
        }

        // =========================
        // EVENTS - PUBLIC GET ALL
        // =========================

        public async Task<IEnumerable<Event>> GetEventsAsync()
        {
            try
            {
                var result = await _http.GetFromJsonAsync<IEnumerable<Event>>("api/Events");
                return result ?? Enumerable.Empty<Event>();
            }
            catch
            {
                // network or deserialization error - return empty list
                return Enumerable.Empty<Event>();
            }
        }

        // =========================
        // EVENTS - GET ONE
        // =========================

        public async Task<Event?> GetEventAsync(int id)
        {
            var response = await _http.GetAsync(
                $"api/Events/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<Event>();
            }
            catch
            {
                return null;
            }
        }

        // =========================
        // EVENTS - SEARCH
        // =========================

        public async Task<IEnumerable<Event>> SearchEventsAsync(
            string? type,
            DateTime? date,
            string? location)
        {
            var query = new List<string>();

            if (!string.IsNullOrWhiteSpace(type))
            {
                query.Add(
                    $"type={Uri.EscapeDataString(type)}");
            }

            if (date.HasValue)
            {
                query.Add(
                    $"date={date.Value:yyyy-MM-dd}");
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query.Add(
                    $"location={Uri.EscapeDataString(location)}");
            }

            var url = "api/Events/search";

            if (query.Count > 0)
            {
                url += "?" + string.Join("&", query);
            }

            try
            {
                var result = await _http.GetFromJsonAsync<IEnumerable<Event>>(url);
                return result ?? Enumerable.Empty<Event>();
            }
            catch
            {
                return Enumerable.Empty<Event>();
            }
        }

        // =========================
        // EVENTS - CREATE
        // =========================

        public async Task<Event?> CreateEventAsync(Event ev)
        {
            AddAuthHeaderFromSession();

            var dto = new
            {
                Title = ev.Title,
                Description = ev.Description ?? "",
                Type = string.IsNullOrWhiteSpace(ev.Type)
                    ? "General"
                    : ev.Type,
                Date = ev.Date,
                Time = ev.Time ?? ev.StartDate.ToString("HH:mm"),
                Location = ev.Location ?? "",
                Capacity = ev.Capacity
            };

            var response = await _http.PostAsJsonAsync(
                "api/Events",
                dto);

            if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
            {
                ClearSessionToken();
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<Event>();
        }

        // =========================
        // EVENTS - UPDATE
        // =========================

        public async Task<bool> UpdateEventAsync(
            int id,
            Event ev)
        {
            AddAuthHeaderFromSession();

            var dto = new
            {
                Title = ev.Title,
                Description = ev.Description ?? "",
                Type = string.IsNullOrWhiteSpace(ev.Type)
                    ? "General"
                    : ev.Type,
                Date = ev.Date,
                Time = ev.Time ?? ev.StartDate.ToString("HH:mm"),
                Location = ev.Location ?? "",
                Capacity = ev.Capacity
            };

            var response = await _http.PutAsJsonAsync(
                $"api/Events/{id}",
                dto);

            if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
            {
                ClearSessionToken();
                return false;
            }

            return response.IsSuccessStatusCode;
        }

        // =========================
        // EVENTS - DELETE
        // =========================

        public async Task<bool> DeleteEventAsync(int id)
        {
            AddAuthHeaderFromSession();

            var response = await _http.DeleteAsync(
                $"api/Events/{id}");

            if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
            {
                ClearSessionToken();
                return false;
            }

            return response.IsSuccessStatusCode;
        }

        // =========================
        // EVENTS - MY EVENTS
        // =========================

        public async Task<IEnumerable<Event>> GetMyEventsAsync()
        {
            AddAuthHeaderFromSession();

            var response = await _http.GetAsync(
                "api/Events/my-events");

            if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
            {
                ClearSessionToken();
                return Enumerable.Empty<Event>();
            }

            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<Event>();
            }

            var result =
                await response.Content
                    .ReadFromJsonAsync<IEnumerable<Event>>();

            return result ?? Enumerable.Empty<Event>();
        }

        // =========================
        // PARTICIPANTS - GET ALL
        // =========================

        public async Task<IEnumerable<Participant>>
            GetParticipantsAsync()
        {
            AddAuthHeaderFromSession();
            try
            {
                var result = await _http.GetFromJsonAsync<IEnumerable<Participant>>("api/Participants");
                return result ?? Enumerable.Empty<Participant>();
            }
            catch
            {
                return Enumerable.Empty<Participant>();
            }
        }

        // =========================
        // PARTICIPANTS - GET ONE
        // =========================

        public async Task<Participant?> GetParticipantAsync(int id)
        {
            AddAuthHeaderFromSession();

            var response = await _http.GetAsync(
                $"api/Participants/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<Participant>();
        }

        // =========================
        // PARTICIPANTS - CREATE
        // =========================

        public async Task<Participant?> CreateParticipantAsync(
            Participant participant)
        {
            AddAuthHeaderFromSession();

            var dto = new
            {
                ParticipantId = participant.Id,
                Name = string.IsNullOrWhiteSpace(participant.Name)
                    ? participant.FullName
                    : participant.Name,
                Email = participant.Email,
                Phone = participant.Phone
            };

            var response = await _http.PostAsJsonAsync(
                "api/Participants",
                dto);

            if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
            {
                ClearSessionToken();
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<Participant>();
        }

        // =========================
        // PARTICIPANTS - UPDATE
        // =========================

        public async Task<bool> UpdateParticipantAsync(
            int id,
            Participant participant)
        {
            AddAuthHeaderFromSession();

            var dto = new
            {
                ParticipantId = id,
                Name = string.IsNullOrWhiteSpace(participant.Name)
                    ? participant.FullName
                    : participant.Name,
                Email = participant.Email,
                Phone = participant.Phone
            };

            var response = await _http.PutAsJsonAsync(
                $"api/Participants/{id}",
                dto);

            if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
            {
                ClearSessionToken();
                return false;
            }

            return response.IsSuccessStatusCode;
        }

        // =========================
        // PARTICIPANTS - DELETE
        // =========================

        public async Task<bool> DeleteParticipantAsync(int id)
        {
            AddAuthHeaderFromSession();

            var response = await _http.DeleteAsync(
                $"api/Participants/{id}");

            if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
            {
                ClearSessionToken();
                return false;
            }

            return response.IsSuccessStatusCode;
        }

        // =========================
        // REGISTRATIONS - GET ALL
        // =========================

        public async Task<IEnumerable<Registration>>
            GetRegistrationsAsync()
        {
            AddAuthHeaderFromSession();
            try
            {
                var result = await _http.GetFromJsonAsync<IEnumerable<Registration>>("api/Registrations");
                return result ?? Enumerable.Empty<Registration>();
            }
            catch
            {
                return Enumerable.Empty<Registration>();
            }
        }

        // =========================
        // REGISTRATIONS - GET ONE
        // =========================

        public async Task<Registration?> GetRegistrationAsync(int id)
        {
            AddAuthHeaderFromSession();

            var response = await _http.GetAsync(
                $"api/Registrations/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<Registration>();
        }

        // =========================
        // REGISTRATIONS - CREATE
        // =========================

        public async Task<(Registration? registration, string? errorMessage)> CreateRegistrationAsync(
      Registration registration)
        {
            AddAuthHeaderFromSession();

            var dto = new
            {
                EventId = registration.EventId,
                ParticipantId = registration.ParticipantId
            };

            try
            {
                var response = await _http.PostAsJsonAsync(
                    "api/Registrations",
                    dto);

                // Authentication error
                if (response.StatusCode ==
                        System.Net.HttpStatusCode.Unauthorized ||
                    response.StatusCode ==
                        System.Net.HttpStatusCode.Forbidden)
                {
                    ClearSessionToken();
                    return (null, "Unauthorized");
                }

                // Read API error so we can surface the real problem to the UI
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    Console.WriteLine(
                        $"Registration API Error: {response.StatusCode}");
                    Console.WriteLine(error);

                    // Prefer the response body as the error message, but fall back to status code text
                    var message = string.IsNullOrWhiteSpace(error)
                        ? response.ReasonPhrase ?? response.StatusCode.ToString()
                        : error;

                    return (null, message);
                }

                var created = await response.Content
                    .ReadFromJsonAsync<Registration>();

                return (created, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Registration Exception: {ex.Message}");

                return (null, ex.Message);
            }
        }

        // =========================
        // REGISTRATIONS - DELETE
        // =========================

        public async Task<bool>
            DeleteRegistrationAsync(int id)
        {
            AddAuthHeaderFromSession();

            var response = await _http.DeleteAsync(
                $"api/Registrations/{id}");

            if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
            {
                ClearSessionToken();
                return false;
            }

            return response.IsSuccessStatusCode;
        }

        // =========================
        // CLEAR SESSION TOKEN
        // =========================

        private void ClearSessionToken()
        {
            try
            {
                _httpContextAccessor.HttpContext?
                    .Session.Remove("JwtToken");
            }
            catch
            {
                // Ignore session errors
            }

            _http.DefaultRequestHeaders.Authorization = null;
        }
    }


}
