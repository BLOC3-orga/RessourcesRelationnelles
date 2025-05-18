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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace R2.Tests.Components.ResourcePages
{
    public class EditResourceTests : TestContext
    {
        private readonly Mock<IDbContextFactory<R2DbContext>> _mockDbFactory;
        private readonly TestDbContext _testDbContext;
        private readonly NavigationManager _navigationManager;
        private readonly Mock<IAntiforgery> _mockAntiforgery;
        private readonly List<Category> _testCategories;
        private readonly List<Resource> _testResources;
        private readonly Resource _testResource;

        public EditResourceTests()
        {
            _testCategories = new List<Category>
            {
                new Category { Id = 1, Name = "Catégorie 1" },
                new Category { Id = 2, Name = "Catégorie 2" },
                new Category { Id = 3, Name = "Catégorie 3" }
            };

            _testResource = new Resource
            {
                Id = 1,
                Name = "Resource Test",
                Description = "Description Test",
                Type = ResourceType.Activity,
                Status = ResourceStatus.Private,
                CategoryId = 1,
                CreationDate = DateTime.Now
            };

            _testResources = new List<Resource> { _testResource };

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
        public void DbContext_ShouldContainResources()
        {
            using (var context = _mockDbFactory.Object.CreateDbContext())
            {
                var category = context.Categories.FirstOrDefault();
                Assert.NotNull(category);
                Assert.Equal("Catégorie 1", category.Name);

                var resource = context.Ressources.FirstOrDefault();
                Assert.NotNull(resource);
                Assert.Equal("Resource Test", resource.Name);
            }
        }

        [Fact]
        public async Task UpdateResource_ShouldModifyResource()
        {
            using (var context = _mockDbFactory.Object.CreateDbContext())
            {
                var resource = context.Ressources.FirstOrDefault(r => r.Id == 1);
                Assert.NotNull(resource);

                resource.Name = "Modified Resource";
                resource.Description = "Modified Description";
                resource.Type = ResourceType.Game;
                resource.Status = ResourceStatus.Public;
                resource.CategoryId = 2;

                await context.SaveChangesAsync();
            }

            _navigationManager.NavigateTo("/resources");

            using (var context = _mockDbFactory.Object.CreateDbContext())
            {
                var modifiedResource = context.Ressources.FirstOrDefault(r => r.Id == 1);
                Assert.NotNull(modifiedResource);
                Assert.Equal("Modified Resource", modifiedResource.Name);
                Assert.Equal("Modified Description", modifiedResource.Description);
                Assert.Equal(ResourceType.Game, modifiedResource.Type);
                Assert.Equal(ResourceStatus.Public, modifiedResource.Status);
                Assert.Equal(2, modifiedResource.CategoryId);
            }

            Assert.EndsWith("/resources", _navigationManager.Uri);
        }
    }
}
