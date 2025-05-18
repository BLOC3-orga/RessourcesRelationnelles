using Bunit;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Components;
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
using System.Threading.Tasks;
using Xunit;

namespace R2.Tests.Components.Pages
{
    public class IndexTests : TestContext
    {
        private readonly Mock<IDbContextFactory<R2DbContext>> _mockDbFactory;
        private readonly TestDbContext _testDbContext;
        private readonly NavigationManager _navigationManager;
        private readonly Mock<IAntiforgery> _mockAntiforgery;
        private readonly List<Category> _testCategories;
        private readonly List<Resource> _testResources;

        public IndexTests()
        {
            _testCategories = new List<Category>
            {
                new Category { Id = 1, Name = "Catégorie 1" },
                new Category { Id = 2, Name = "Catégorie 2" },
                new Category { Id = 3, Name = "Catégorie 3" }
            };

            _testResources = new List<Resource>
            {
                new Resource
                {
                    Id = 1,
                    Name = "Ressource 1",
                    Description = "Description de la ressource 1",
                    Type = ResourceType.Activity,
                    Status = ResourceStatus.Public,
                    CategoryId = 1,
                    Category = _testCategories[0],
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
                    Category = _testCategories[1],
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
                    Category = _testCategories[2],
                    CreationDate = DateTime.Now.AddDays(-1)
                }
            };

            var options = new DbContextOptionsBuilder<R2DbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid().ToString())
                .Options;

            _testDbContext = new TestDbContext(options);
            _testDbContext.SetupTestData(_testCategories, _testResources);

            _mockDbFactory = new Mock<IDbContextFactory<R2DbContext>>();
            _mockDbFactory.Setup(f => f.CreateDbContext()).Returns(_testDbContext);

            _mockAntiforgery = new Mock<IAntiforgery>();
            var antiforgeryTokenSet = new AntiforgeryTokenSet("requestToken", "cookieToken", "formFieldName", "headerName");
            _mockAntiforgery.Setup(a => a.GetAndStoreTokens(It.IsAny<HttpContext>()))
                .Returns(antiforgeryTokenSet);

            Services.AddSingleton(_mockDbFactory.Object);
            Services.AddSingleton(_mockAntiforgery.Object);
            Services.AddSingleton(Mock.Of<IHttpContextAccessor>());

            _navigationManager = Services.GetRequiredService<NavigationManager>();
        }

        [Fact]
        public void WhenRendered_ShouldDisplayResourcesList()
        {
            var cut = RenderComponent<R2.UI.Components.Pages.ResourcePages.Index>();

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
            var cut = RenderComponent<R2.UI.Components.Pages.ResourcePages.Index>();

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
        public void WhenFilterButtonsClicked_ShouldCallFilterMethods()
        {
            var cut = RenderComponent<R2.UI.Components.Pages.ResourcePages.Index>();

            cut.Find("#statusFilter").Change(((int)ResourceStatus.Public).ToString());
            cut.Find("#typeFilter").Change(((int)ResourceType.Activity).ToString());
            cut.Find("#categoryFilter").Change("1");

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
        public void ResourceCards_ShouldHaveCorrectElements()
        {
            var cut = RenderComponent<R2.UI.Components.Pages.ResourcePages.Index>();

            var cards = cut.FindAll(".card");
            Assert.True(cards.Count >= 3, $"Attendu au moins 3 cartes, trouvé {cards.Count}");

            Assert.Equal(3, cut.FindAll("a[href^='resources/details']").Count);
            Assert.Equal(3, cut.FindAll("a[href^='resources/edit']").Count);
            Assert.Equal(3, cut.FindAll("a[href^='resources/delete']").Count);

            Assert.Equal(3, cut.FindAll("button.btn-outline-success").Count); 
            Assert.Equal(3, cut.FindAll("button.btn-outline-info").Count);   
            Assert.Equal(3, cut.FindAll("button.btn-outline-warning").Count); 
        }

        [Fact]
        public void EditButton_ShouldNavigateToEditPage()
        {
            var cut = RenderComponent<R2.UI.Components.Pages.ResourcePages.Index>();

            var editButton = cut.FindAll("a[href^='resources/edit']").First();
            var href = editButton.GetAttribute("href");

            _navigationManager.NavigateTo(href);

            Assert.Contains("resources/edit", _navigationManager.Uri);
            Assert.Contains("id=1", _navigationManager.Uri);
        }

        [Fact]
        public void ResourceFiltersByStatus_ShouldFilterCorrectly()
        {
            var cut = RenderComponent<R2.UI.Components.Pages.ResourcePages.Index>();

            cut.Find("#statusFilter").Change(((int)ResourceStatus.Public).ToString());
            cut.Find("button.btn-outline-primary").Click();

            Assert.Contains("Ressource 1", cut.Markup);
            Assert.DoesNotContain("Ressource 2", cut.Markup);
            Assert.DoesNotContain("Ressource 3", cut.Markup);

            cut.Find("#statusFilter").Change(((int)ResourceStatus.Private).ToString());
            cut.Find("button.btn-outline-primary").Click();

            Assert.DoesNotContain("Ressource 1", cut.Markup);
            Assert.Contains("Ressource 2", cut.Markup);
            Assert.DoesNotContain("Ressource 3", cut.Markup);
        }

        [Fact]
        public void ResourceFiltersByType_ShouldFilterCorrectly()
        {
            var cut = RenderComponent<R2.UI.Components.Pages.ResourcePages.Index>();

            cut.Find("#typeFilter").Change(((int)ResourceType.Game).ToString());
            cut.Find("button.btn-outline-primary").Click();

            Assert.DoesNotContain("Ressource 1", cut.Markup);
            Assert.Contains("Ressource 2", cut.Markup);
            Assert.DoesNotContain("Ressource 3", cut.Markup);
        }

        [Fact]
        public void ResourceFiltersByCategory_ShouldFilterCorrectly()
        {
            var cut = RenderComponent<R2.UI.Components.Pages.ResourcePages.Index>();

            cut.Find("#categoryFilter").Change("3");
            cut.Find("button.btn-outline-primary").Click();

            Assert.DoesNotContain("Ressource 1", cut.Markup);
            Assert.DoesNotContain("Ressource 2", cut.Markup);
            Assert.Contains("Ressource 3", cut.Markup);
        }

        [Fact]
        public void CreateNewResourceButton_ShouldNavigateToCreatePage()
        {
            var cut = RenderComponent<R2.UI.Components.Pages.ResourcePages.Index>();

            var createButton = cut.Find("a[href='resources/create']");
            var href = createButton.GetAttribute("href");

            _navigationManager.NavigateTo(href);

            Assert.EndsWith("resources/create", _navigationManager.Uri);
        }
    }
}
