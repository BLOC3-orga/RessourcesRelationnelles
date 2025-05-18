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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace R2.Tests.Components.Pages.ResourcePages
{
    public class DeleteResourceTests : TestContext
    {
        private readonly Mock<IDbContextFactory<R2DbContext>> _mockDbFactory;
        private readonly TestDbContext _testDbContext;
        private readonly NavigationManager _navigationManager;
        private readonly Mock<IAntiforgery> _mockAntiforgery;
        private readonly List<Category> _testCategories;
        private readonly List<Resource> _testResources;

        public DeleteResourceTests()
        {
            _testCategories = new List<Category>
            {
                new Category { Id = 1, Name = "Catégorie 1" },
                new Category { Id = 2, Name = "Catégorie 2" },
                new Category { Id = 3, Name = "Catégorie 3" }
            };

            var testResource = new Resource
            {
                Id = 1,
                Name = "Resource Test",
                Description = "Description Test",
                Type = ResourceType.Activity,
                Status = ResourceStatus.Private,
                CategoryId = 1,
                Category = _testCategories[0],
                CreationDate = DateTime.Now
            };

            _testResources = new List<Resource> { testResource };

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
        public void DbContext_ShouldContainResource()
        {
            using (var context = _mockDbFactory.Object.CreateDbContext())
            {
                var resource = context.Ressources.FirstOrDefault(r => r.Id == 1);
                Assert.NotNull(resource);
                Assert.Equal("Resource Test", resource.Name);
                Assert.Equal("Description Test", resource.Description);
            }
        }

        [Fact]
        public void DeleteResource_ShouldRemoveFromDatabase()
        {
            using (var context = _mockDbFactory.Object.CreateDbContext())
            {
                var resource = context.Ressources.FirstOrDefault(r => r.Id == 1);
                Assert.NotNull(resource);

                context.Ressources.Remove(resource);
                context.SaveChanges();
            }

            using (var context = _mockDbFactory.Object.CreateDbContext())
            {
                var resource = context.Ressources.FirstOrDefault(r => r.Id == 1);
                Assert.Null(resource);
            }

            _navigationManager.NavigateTo("/resources");
            Assert.EndsWith("/resources", _navigationManager.Uri);
        }

        [Fact]
        public void NavigationManager_ShouldRedirectToNotFound()
        {
            _navigationManager.NavigateTo("notfound");
            Assert.EndsWith("notfound", _navigationManager.Uri);
        }
    }
}
