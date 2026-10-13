namespace KMCEventClient.Models
{
    public record RegisterRequest(string Name, string Email, string Password, string? Role);
}
