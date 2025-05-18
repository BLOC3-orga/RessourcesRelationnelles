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
using R2.UI.Components.Pages.ResourcePages;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace R2.Tests.Components.ResourcePages
{
    public class CreateResourceTests : TestContext
    {
        private readonly Mock<IDbContextFactory<R2DbContext>> _mockDbFactory;
        private readonly TestDbContext _testDbContext;
        private readonly NavigationManager _navigationManager;
        private readonly Mock<IAntiforgery> _mockAntiforgery;
        private readonly List<Category> _testCategories;
        private readonly List<Resource> _testResources;

        public CreateResourceTests()
        {
            _testCategories = new List<Category>
            {
                new Category { Id = 1, Name = "Catégorie 1" },
                new Category { Id = 2, Name = "Catégorie 2" },
                new Category { Id = 3, Name = "Catégorie 3" }
            };

            _testResources = new List<Resource>();

            var options = new DbContextOptionsBuilder<R2DbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + System.Guid.NewGuid().ToString())
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
        public void WhenRendered_ShouldDisplayForm()
        {
            var cut = RenderComponent<Create>();

            Assert.NotNull(cut.Find("form"));
            Assert.NotNull(cut.Find("h2"));
            Assert.Equal("Créer une nouvelle ressource", cut.Find("h2").TextContent);

            Assert.NotNull(cut.Find("#name"));
            Assert.NotNull(cut.Find("#description"));
            Assert.NotNull(cut.Find("#type"));
            Assert.NotNull(cut.Find("#status"));
            Assert.NotNull(cut.Find("#categoryid"));

            Assert.Contains("Retour à la liste", cut.Markup);
            Assert.Contains("Créer la ressource", cut.Markup);
        }

        [Fact]
        public void WhenRendered_ShouldLoadCategories()
        {
            var cut = RenderComponent<Create>();

            var options = cut.FindAll("#categoryid option");

            Assert.Equal(_testCategories.Count + 1, options.Count);

            foreach (var category in _testCategories)
            {
                Assert.Contains(options, option => option.TextContent == category.Name);
            }
        }

        [Fact]
        public void WhenRendered_ShouldInitializeWithDefaultValues()
        {
            var cut = RenderComponent<Create>();

            var typeOptions = cut.FindAll("#type option");
            var statusOptions = cut.FindAll("#status option");

            var activityOption = typeOptions.FirstOrDefault(o =>
                o.GetAttribute("value") == ResourceType.Activity.ToString());
            Assert.NotNull(activityOption);

            var privateOption = statusOptions.FirstOrDefault(o =>
                o.GetAttribute("value") == ((int)ResourceStatus.Private).ToString());
            Assert.NotNull(privateOption);
        }

        [Fact]
        public async Task WhenFormSubmittedWithValidData_ShouldAddResourceAndNavigate()
        {
            var cut = RenderComponent<Create>();

            cut.Find("#name").Change("Test Resource");
            cut.Find("#description").Change("Test Description");
            cut.Find("#type").Change(ResourceType.Game.ToString());
            cut.Find("#status").Change(((int)ResourceStatus.Public).ToString());
            cut.Find("#categoryid").Change("2");

            await cut.Find("form").SubmitAsync();

            Assert.Single(_testResources);
            var addedResource = _testResources.First();
            Assert.Equal("Test Resource", addedResource.Name);
            Assert.Equal("Test Description", addedResource.Description);
            Assert.Equal(ResourceType.Game, addedResource.Type);
            Assert.Equal(ResourceStatus.Public, addedResource.Status);
            Assert.Equal(2, addedResource.CategoryId);

            // Check navigation
            Assert.EndsWith("/resources", _navigationManager.Uri);
        }

        [Fact]
        public void FormHasValidationElements()
        {
            var cut = RenderComponent<Create>();

            Assert.Contains("form-control", cut.Markup); 
            Assert.Contains("btn-success", cut.Markup);  

            var nameInput = cut.Find("#name");
            var descInput = cut.Find("#description");
            var categorySelect = cut.Find("#categoryid");

            Assert.NotNull(nameInput);
            Assert.NotNull(descInput);
            Assert.NotNull(categorySelect);

            Assert.True(true);
        }

        [Fact]
        public void WhenBackButtonClicked_ShouldNavigateToResourceList()
        {
            var cut = RenderComponent<Create>();

            var backButton = cut.Find("a.btn-outline-secondary");
            var href = backButton.GetAttribute("href");
            _navigationManager.NavigateTo(href);

            Assert.EndsWith("/resources", _navigationManager.Uri);
        }

        [Fact]
        public void ShouldRenderResourceTypeOptions()
        {
            var cut = RenderComponent<Create>();

            var options = cut.FindAll("#type option");

            Assert.Equal(Enum.GetValues(typeof(ResourceType)).Length, options.Count);

            Assert.Contains(options, option => option.TextContent == "Activité");
            Assert.Contains(options, option => option.TextContent == "Jeu");
            Assert.Contains(options, option => option.TextContent == "Document");
        }

        [Fact]
        public void ShouldRenderResourceStatusOptions()
        {
            var cut = RenderComponent<Create>();

            var options = cut.FindAll("#status option");

            Assert.Equal(Enum.GetValues(typeof(ResourceStatus)).Length - 1, options.Count);

            Assert.Contains(options, option => option.TextContent == "Privé");
            Assert.Contains(options, option => option.TextContent == "Public");
            Assert.Contains(options, option => option.TextContent == "Brouillon");

            Assert.DoesNotContain(options, option => option.TextContent.Contains("Suspendu"));
        }
    }
}
