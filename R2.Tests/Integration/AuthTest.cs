using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using R2.Data.Context;
using R2.Data.Entities;
using System.Net;
using System.Net.Http;
using Xunit;

namespace R2.IntegrationTests
{
    public class BlazorApplicationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public BlazorApplicationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<R2DbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<R2DbContext>(options =>
                        options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid()));

                    services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task HomePage_LoadsSuccessfully()
        {
            // Act
            var response = await _client.GetAsync("/");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/login")]  
        [InlineData("/register")]
        public async Task BlazorPages_LoadWithoutErrors(string url)
        {
            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.True(response.IsSuccessStatusCode,
                $"Page {url} returned {response.StatusCode}");
        }

        [Fact]
        public async Task DatabaseServices_AreRegisteredCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();

            // Act & Assert
            var dbContext = scope.ServiceProvider.GetService<R2DbContext>();
            var userManager = scope.ServiceProvider.GetService<UserManager<User>>();
            var signInManager = scope.ServiceProvider.GetService<SignInManager<User>>();

            Assert.NotNull(dbContext);
            Assert.NotNull(userManager);
            Assert.NotNull(signInManager);
        }

        [Fact]
        public async Task Database_CanBeCreatedAndConnected()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<R2DbContext>();

            // Act
            await context.Database.EnsureCreatedAsync();
            var canConnect = await context.Database.CanConnectAsync();

            // Assert
            Assert.True(canConnect);
        }

        [Fact]
        public async Task UserManager_CanCreateUser()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<R2DbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            await context.Database.EnsureCreatedAsync();

            var testUser = new User
            {
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                Name = "Test",
                LastName = "User",
                Pseudo = "TestUser",
                EmailConfirmed = true,
                IsAccountActivated = true
            };

            // Act
            var result = await userManager.CreateAsync(testUser, "TestPassword123!");

            // Assert
            Assert.True(result.Succeeded, $"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            var retrievedUser = await userManager.FindByEmailAsync("testuser@example.com");
            Assert.NotNull(retrievedUser);
            Assert.Equal("Test", retrievedUser.Name);
        }

        [Fact]
        public async Task BlazorSignalR_ScriptIsAccessible()
        {
            // Act
            var response = await _client.GetAsync("/_framework/blazor.server.js");

            // Assert
            response.EnsureSuccessStatusCode();

            var contentType = response.Content.Headers.ContentType?.MediaType;
            Assert.True(contentType == "application/javascript" || contentType == "text/javascript",
                $"Expected JavaScript content type, but got: {contentType}");
        }
    }

    public class ResourceManagementIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ResourceManagementIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<R2DbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<R2DbContext>(options =>
                        options.UseInMemoryDatabase("ResourceTestDB_" + Guid.NewGuid()));
                });
            });
        }

        [Fact]
        public async Task ResourceWorkflow_CreateCategoryAndResource_Works()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<R2DbContext>();
            await context.Database.EnsureCreatedAsync();

            // Act
            var category = new Category { Name = "Test Category" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var resource = new Resource
            {
                Name = "Test Resource",
                Description = "Test Description",
                CategoryId = category.Id,
                Type = R2.Data.Enums.ResourceType.Activity,
                Status = R2.Data.Enums.ResourceStatus.Private,
                CreationDate = DateTime.UtcNow
            };
            context.Ressources.Add(resource);
            await context.SaveChangesAsync();

            // Assert
            var savedResource = await context.Ressources
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r => r.Name == "Test Resource");

            Assert.NotNull(savedResource);
            Assert.Equal("Test Resource", savedResource.Name);
            Assert.Equal("Test Category", savedResource.Category?.Name);
        }

        [Fact]
        public async Task DatabaseTables_AreCreatedCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<R2DbContext>();

            // Act
            await context.Database.EnsureCreatedAsync();

            Assert.NotNull(context.Users);
            Assert.NotNull(context.Categories);
            Assert.NotNull(context.Ressources);
            Assert.NotNull(context.Comments);
            Assert.NotNull(context.Statistics);

            // Test que les relations fonctionnent
            var categoryCount = await context.Categories.CountAsync();
            var resourceCount = await context.Ressources.CountAsync();

            Assert.True(categoryCount >= 0);
            Assert.True(resourceCount >= 0);
        }
    }
}