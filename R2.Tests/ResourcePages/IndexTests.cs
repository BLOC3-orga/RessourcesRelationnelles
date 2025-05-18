using Bunit;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using R2.Data.Context;
using R2.Data.Entities;
using R2.Data.Enums;
using R2.Tests.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using AngleSharp.Dom;

namespace R2.Tests.Components.Pages.ResourcePages
{
    public class IndexTests
    {
        private class TestAuthenticationStateProvider : AuthenticationStateProvider
        {
            private readonly ClaimsPrincipal _user;

            public TestAuthenticationStateProvider(bool authenticated)
            {
                var identity = authenticated
                    ? new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "TestUser") }, "TestAuthType")
                    : new ClaimsIdentity();

                _user = new ClaimsPrincipal(identity);
            }

            public override Task<AuthenticationState> GetAuthenticationStateAsync()
            {
                return Task.FromResult(new AuthenticationState(_user));
            }
        }

        private class TestAuthorizationPolicyProvider : IAuthorizationPolicyProvider
        {
            public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
            {
                return Task.FromResult(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
            }

            public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
            {
                return Task.FromResult<AuthorizationPolicy?>(null);
            }

            public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
            {
                return Task.FromResult<AuthorizationPolicy?>(
                    new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
            }
        }

        private TestContext CreateTestContext(bool authenticated = true)
        {
            var context = new TestContext();

            var testCategories = new List<Category>
            {
                new Category { Id = 1, Name = "Catégorie 1" },
                new Category { Id = 2, Name = "Catégorie 2" },
                new Category { Id = 3, Name = "Catégorie 3" }
            };

            var testResources = new List<Resource>
            {
                new Resource
                {
                    Id = 1,
                    Name = "Ressource 1",
                    Description = "Description de la ressource 1",
                    Type = ResourceType.Activity,
                    Status = ResourceStatus.Public,
                    CategoryId = 1,
                    Category = testCategories[0],
                    CreationDate = DateTime.Now.AddDays(-10)
                },
                new Resource
                {
                    Id = 2,
                    Name = "Ressource 2",
                    Description = "Description de la ressource 2",
                    Type = ResourceType.Game,
                    Status = ResourceStatus.Private,
                    CategoryId = 2,
                    Category = testCategories[1],
                    CreationDate = DateTime.Now.AddDays(-5)
                },
                new Resource
                {
                    Id = 3,
                    Name = "Ressource 3",
                    Description = "Description de la ressource 3",
                    Type = ResourceType.Document,
                    Status = ResourceStatus.Draft,
                    CategoryId = 3,
                    Category = testCategories[2],
                    CreationDate = DateTime.Now.AddDays(-1)
                }
            };

            var options = new DbContextOptionsBuilder<R2DbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid().ToString())
                .Options;

            var testDbContext = new TestDbContext(options);
            testDbContext.SetupTestData(testCategories, testResources);

            var mockDbFactory = new Mock<IDbContextFactory<R2DbContext>>();
            mockDbFactory.Setup(f => f.CreateDbContext()).Returns(testDbContext);

            var mockAntiforgery = new Mock<IAntiforgery>();
            var antiforgeryTokenSet = new AntiforgeryTokenSet("requestToken", "cookieToken", "formFieldName", "headerName");
            mockAntiforgery.Setup(a => a.GetAndStoreTokens(It.IsAny<HttpContext>()))
                .Returns(antiforgeryTokenSet);

            context.Services.AddAuthorizationCore();

            context.Services.AddSingleton<IAuthorizationPolicyProvider>(new TestAuthorizationPolicyProvider());

            var mockAuthService = new Mock<IAuthorizationService>();
            mockAuthService
                .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(),
                                           It.IsAny<object>(),
                                           It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());
            context.Services.AddSingleton(mockAuthService.Object);

            var authStateProvider = new TestAuthenticationStateProvider(authenticated);
            context.Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);

            context.Services.AddSingleton(mockDbFactory.Object);
            context.Services.AddSingleton(mockAntiforgery.Object);
            context.Services.AddSingleton(Mock.Of<IHttpContextAccessor>());

            return context;
        }

        private IRenderedComponent<R2.UI.Components.Pages.ResourcePages.Index> RenderIndexComponent(TestContext context)
        {
            var authStateProvider = context.Services.GetRequiredService<AuthenticationStateProvider>();

            var authState = authStateProvider.GetAuthenticationStateAsync().Result;

            // Afficher l'état d'authentification pour le débogage
            Console.WriteLine($"État d'authentification: {authState.User.Identity?.IsAuthenticated}");

            var cut = context.RenderComponent<CascadingAuthenticationState>(parameters => {
                parameters.AddChildContent<R2.UI.Components.Pages.ResourcePages.Index>();
            });

            return cut.FindComponent<R2.UI.Components.Pages.ResourcePages.Index>();
        }

        [Fact]
        public void WhenRendered_ShouldDisplayResourcesList()
        {
            var context = CreateTestContext();

            var cut = RenderIndexComponent(context);

            Assert.Contains("Ressources", cut.Markup);
            Assert.Contains("Créer une nouvelle ressource", cut.Markup);

            Assert.Contains("Ressource 1", cut.Markup);
            Assert.Contains("Ressource 2", cut.Markup);
            Assert.Contains("Ressource 3", cut.Markup);

            Assert.Contains("Catégorie 1", cut.Markup);
            Assert.Contains("Catégorie 2", cut.Markup);
            Assert.Contains("Catégorie 3", cut.Markup);

            Assert.Contains("Public", cut.Markup);
            Assert.Contains("Privé", cut.Markup);
            Assert.Contains("Brouillon", cut.Markup);

            Assert.Contains("Activité", cut.Markup);
            Assert.Contains("Jeu", cut.Markup);
            Assert.Contains("Document", cut.Markup);
        }

        [Fact]
        public void WhenRendered_ShouldDisplayFilters()
        {
            var context = CreateTestContext();

            var cut = RenderIndexComponent(context);

            var statusFilterSelect = cut.Find("#statusFilter");
            var typeFilterSelect = cut.Find("#typeFilter");
            var categoryFilterSelect = cut.Find("#categoryFilter");

            Assert.NotNull(statusFilterSelect);
            Assert.NotNull(typeFilterSelect);
            Assert.NotNull(categoryFilterSelect);

            Assert.Contains("Appliquer", cut.Markup);
            Assert.Contains("Réinitialiser", cut.Markup);
        }

        [Fact]
        public void ShouldDisplayFilterOptions()
        {
            var context = CreateTestContext();

            var cut = RenderIndexComponent(context);

            var statusOptions = cut.FindAll("#statusFilter option");
            Assert.Contains(statusOptions, option => option.TextContent == "Tous");
            Assert.Contains(statusOptions, option => option.TextContent == "Privé");
            Assert.Contains(statusOptions, option => option.TextContent == "Public");
            Assert.Contains(statusOptions, option => option.TextContent == "Brouillon");

            var typeOptions = cut.FindAll("#typeFilter option");
            Assert.Contains(typeOptions, option => option.TextContent == "Tous");
            Assert.Contains(typeOptions, option => option.TextContent == "Activité");
            Assert.Contains(typeOptions, option => option.TextContent == "Jeu");
            Assert.Contains(typeOptions, option => option.TextContent == "Document");

            var categoryOptions = cut.FindAll("#categoryFilter option");
            Assert.Contains(categoryOptions, option => option.TextContent == "Toutes");
            Assert.Contains(categoryOptions, option => option.TextContent == "Catégorie 1");
            Assert.Contains(categoryOptions, option => option.TextContent == "Catégorie 2");
            Assert.Contains(categoryOptions, option => option.TextContent == "Catégorie 3");
        }

        [Fact]
        public void ShouldDisplaySortOptions()
        {
            var context = CreateTestContext();

            var cut = RenderIndexComponent(context);

            var sortOptions = cut.FindAll("#sortBy option");
            Assert.Equal(4, sortOptions.Count);
            Assert.Contains(sortOptions, option => option.TextContent == "Nom (A-Z)");
            Assert.Contains(sortOptions, option => option.TextContent == "Nom (Z-A)");
            Assert.Contains(sortOptions, option => option.TextContent == "Date (Plus récente)");
            Assert.Contains(sortOptions, option => option.TextContent == "Date (Plus ancienne)");
        }

        [Fact]
        public void WhenUserIsNotAuthenticated_ShouldOnlyShowPublicResources()
        {
            var context = CreateTestContext(authenticated: false);

            var cut = RenderIndexComponent(context);

            Assert.Contains("Ressource 1", cut.Markup); 
            Assert.DoesNotContain("Ressource 2", cut.Markup); 
            Assert.DoesNotContain("Ressource 3", cut.Markup); 
        }

        [Fact]
        public void WhenFilterButtonsClicked_ShouldFilterResources()
        {
            var context = CreateTestContext();
            var cut = RenderIndexComponent(context);

            cut.Find("#statusFilter").Change(((int)ResourceStatus.Public).ToString());
            cut.Find("button.btn-outline-primary").Click();

            Assert.Contains("Ressource 1", cut.Markup);
            Assert.DoesNotContain("Ressource 2", cut.Markup);
            Assert.DoesNotContain("Ressource 3", cut.Markup);

            cut.Find("button.btn-outline-secondary").Click();

            Assert.Contains("Ressource 1", cut.Markup);
            Assert.Contains("Ressource 2", cut.Markup);
            Assert.Contains("Ressource 3", cut.Markup);
        }

        [Fact]
        public void WhenSortOptionChanged_ShouldSortResources()
        {
            var context = CreateTestContext();
            var cut = RenderIndexComponent(context);

            cut.Find("#sortBy").Change("0");
            cut.Find("button.btn-outline-primary").Click();

            Assert.Contains("Tri actuel : Nom (A-Z)", cut.Markup);

            cut.Find("#sortBy").Change("2");
            cut.Find("button.btn-outline-primary").Click();

            Assert.Contains("Tri actuel : Date (Plus récente)", cut.Markup);
        }

        [Fact]
        public void EditButton_ShouldNavigateToEditPage()
        {
            var context = CreateTestContext();
            var cut = RenderIndexComponent(context);
            var navigationManager = context.Services.GetRequiredService<NavigationManager>();

            var editButtons = cut.FindAll("a[href^='resources/edit']");
            var firstEditButton = editButtons.First();
            var href = firstEditButton.GetAttribute("href");

            navigationManager.NavigateTo(href);

            Assert.Contains("resources/edit", navigationManager.Uri);
            Assert.Contains("id=", navigationManager.Uri);
        }

        [Fact]
        public void CreateNewResourceButton_ShouldNavigateToCreatePage()
        {
            var context = CreateTestContext();
            var cut = RenderIndexComponent(context);
            var navigationManager = context.Services.GetRequiredService<NavigationManager>();

            var createButton = cut.Find("a[href='resources/create']");
            var href = createButton.GetAttribute("href");

            navigationManager.NavigateTo(href);

            Assert.EndsWith("resources/create", navigationManager.Uri);
        }
    }
}