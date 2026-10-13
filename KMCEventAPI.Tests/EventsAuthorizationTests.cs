using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using KMCEventAPI.Data;
using KMCEventAPI.Models;
using Xunit;

namespace KMCEventAPI.Tests
{
    public class EventsAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public EventsAuthorizationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Organizer_CanCreateButOtherCannotUpdate()
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace DbContext with InMemory
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<KMCEventDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<KMCEventDbContext>(options =>
                        options.UseInMemoryDatabase("AuthTestDb"));

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<KMCEventDbContext>();
                    db.Database.EnsureCreated();

                    // seed two users
                    db.Users.Add(new User { UserId = 10, Name = "Org1", Email = "org1@example.com", Password = "pwd1", Role = "Organizer" });
                    db.Users.Add(new User { UserId = 11, Name = "Org2", Email = "org2@example.com", Password = "pwd2", Role = "Organizer" });
                    db.SaveChanges();
                });
            }).CreateClient();

            // login org1
            var login1 = await client.PostAsJsonAsync("/api/auth/login", new { Email = "org1@example.com", Password = "pwd1" });
            login1.EnsureSuccessStatusCode();
            var token1 = (await login1.Content.ReadFromJsonAsync<Dictionary<string,string>>())["token"];

            // create event as org1
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
            var createResp = await client.PostAsJsonAsync("/api/events", new
            {
                Title = "Owned Event",
                Description = "desc",
                Type = "Public",
                Date = DateTime.UtcNow.Date,
                Time = "10:00",
                Location = "Hall",
                Capacity = 100
            });
            createResp.EnsureSuccessStatusCode();
            var created = await createResp.Content.ReadFromJsonAsync<Event>();

            // login org2 and attempt update
            var login2 = await client.PostAsJsonAsync("/api/auth/login", new { Email = "org2@example.com", Password = "pwd2" });
            login2.EnsureSuccessStatusCode();
            var token2 = (await login2.Content.ReadFromJsonAsync<Dictionary<string,string>>())["token"];

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
            var updateResp = await client.PutAsJsonAsync($"/api/events/{created.EventId}", new
            {
                Title = "Hacked",
                Description = "x",
                Type = "Public",
                Date = DateTime.UtcNow.Date,
                Time = "11:00",
                Location = "Hall",
                Capacity = 10
            });

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, updateResp.StatusCode);
        }
    }
}
