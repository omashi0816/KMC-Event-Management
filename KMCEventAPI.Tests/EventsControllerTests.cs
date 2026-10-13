using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using KMCEventAPI.Data;
using KMCEventAPI.Models;
using Xunit;

namespace KMCEventAPI.Tests
{
    public class EventsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public EventsControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetEvents_ReturnsSuccessAndJson()
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // replace real DbContext with InMemory for tests
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<KMCEventDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<KMCEventDbContext>(options =>
                        options.UseInMemoryDatabase("TestDb"));

                    // build a provider to seed test data
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<KMCEventDbContext>();
                    db.Database.EnsureCreated();
                    db.Events.Add(new Event { Title = "Test Event" });
                    db.SaveChanges();
                });
            }).CreateClient();

            var response = await client.GetAsync("/api/events");
            response.EnsureSuccessStatusCode();

            var events = await response.Content.ReadFromJsonAsync<List<Event>>();
            Assert.NotNull(events);
            Assert.NotEmpty(events);
        }
    }
}
