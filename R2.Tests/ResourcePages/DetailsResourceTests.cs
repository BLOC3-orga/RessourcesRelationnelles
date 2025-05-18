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
    public class DetailsResourceTests : TestContext
    {
        private readonly Mock<IDbContextFactory<R2DbContext>> _mockDbFactory;
        private readonly TestDbContext _testDbContext;
        private readonly NavigationManager _navigationManager;
        private readonly Mock<IAntiforgery> _mockAntiforgery;
        private readonly List<Category> _testCategories;
        private readonly List<Resource> _testResources;
        private readonly List<Comment> _testComments;
        private readonly List<Progression> _testProgressions;

        public DetailsResourceTests()
        {
            // Données de test
            _testCategories = new List<Category>
            {
                new Category { Id = 1, Name = "Catégorie 1" },
                new Category { Id = 2, Name = "Catégorie 2" },
                new Category { Id = 3, Name = "Catégorie 3" }
            };

            // Ressources de test
            _testResources = new List<Resource>
            {
                new Resource
                {
                    Id = 1,
                    Name = "Resource Test",
                    Description = "Description Test",
                    Type = ResourceType.Activity,
                    Status = ResourceStatus.Private,
                    CategoryId = 1,
                    Category = _testCategories[0],
                    CreationDate = DateTime.Now.AddDays(-10)
                },
                new Resource
                {
                    Id = 2,
                    Name = "Resource Draft",
                    Description = "Draft Description",
                    Type = ResourceType.Game,
                    Status = ResourceStatus.Draft,
                    CategoryId = 2,
                    Category = _testCategories[1],
                    CreationDate = DateTime.Now.AddDays(-5)
                }
            };

            // Commentaires de test
            _testComments = new List<Comment>
            {
                new Comment
                {
                    Id = 1,
                    Content = "Premier commentaire",
                    //ResourceId = 1 // Si votre modèle a cette propriété
                },
                new Comment
                {
                    Id = 2,
                    Content = "Deuxième commentaire",
                    //ResourceId = 1 // Si votre modèle a cette propriété
                }
            };

            // Progression de test
            _testProgressions = new List<Progression>
            {
                new Progression
                {
                    Id = 1,
                    Percentage = 50,
                    Status = ProgressionStatus.InProgress,
                    LastInteractionDate = DateTime.Now.AddDays(-2)
                    //ResourceId = 1, // Si votre modèle a cette propriété
                    //UserId = 1, // Si votre modèle a cette propriété
                }
            };

            // Configuration de la base de données
            var options = new DbContextOptionsBuilder<R2DbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid().ToString())
                .Options;

            _testDbContext = new TestDbContext(options);
            _testDbContext.SetupTestData(_testCategories, _testResources);

            // Ajouter les commentaires et progressions au contexte
            // Note: Ceci devrait être adapté selon la façon dont votre TestDbContext gère ces entités
            using (var context = new R2DbContext(options))
            {
                context.Comments.AddRange(_testComments);
                context.Progressions.AddRange(_testProgressions);
                context.SaveChanges();
            }

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
            // Vérifie que le contexte contient bien les ressources attendues
            using (var context = _mockDbFactory.Object.CreateDbContext())
            {
                var resource = context.Ressources.FirstOrDefault(r => r.Id == 1);
                Assert.NotNull(resource);
                Assert.Equal("Resource Test", resource.Name);
                Assert.Equal("Description Test", resource.Description);
                Assert.Equal(ResourceType.Activity, resource.Type);
                Assert.Equal(ResourceStatus.Private, resource.Status);
            }
        }

        [Fact]
        public void GetStatusDisplayName_ShouldReturnCorrectName()
        {
            // Test la méthode qui convertit les statuts en noms d'affichage
            // Instancier le composant directement pour tester ses méthodes
            var detailsComponent = new Details();

            // Accéder à la méthode par réflexion puisqu'elle est privée
            var methodInfo = detailsComponent.GetType().GetMethod("GetStatusDisplayName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Tester les différentes valeurs de statut
            var privateName = methodInfo.Invoke(detailsComponent, new object[] { ResourceStatus.Private });
            var publicName = methodInfo.Invoke(detailsComponent, new object[] { ResourceStatus.Public });
            var draftName = methodInfo.Invoke(detailsComponent, new object[] { ResourceStatus.Draft });

            Assert.Equal("Privé", privateName);
            Assert.Equal("Public", publicName);
            Assert.Equal("Brouillon", draftName);
        }

        [Fact]
        public void GetTypeDisplayName_ShouldReturnCorrectName()
        {
            // Test la méthode qui convertit les types en noms d'affichage
            var detailsComponent = new Details();

            var methodInfo = detailsComponent.GetType().GetMethod("GetTypeDisplayName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var activityName = methodInfo.Invoke(detailsComponent, new object[] { ResourceType.Activity });
            var gameName = methodInfo.Invoke(detailsComponent, new object[] { ResourceType.Game });
            var documentName = methodInfo.Invoke(detailsComponent, new object[] { ResourceType.Document });

            Assert.Equal("Activité", activityName);
            Assert.Equal("Jeu", gameName);
            Assert.Equal("Document", documentName);
        }

        [Fact]
        public void GetStatusBadgeClass_ShouldReturnCorrectClass()
        {
            // Test la méthode qui retourne les classes CSS pour les badges de statut
            var detailsComponent = new Details();

            var methodInfo = detailsComponent.GetType().GetMethod("GetStatusBadgeClass",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var privateClass = methodInfo.Invoke(detailsComponent, new object[] { ResourceStatus.Private });
            var publicClass = methodInfo.Invoke(detailsComponent, new object[] { ResourceStatus.Public });
            var draftClass = methodInfo.Invoke(detailsComponent, new object[] { ResourceStatus.Draft });

            Assert.Equal("bg-secondary", privateClass);
            Assert.Equal("bg-success", publicClass);
            Assert.Equal("bg-warning", draftClass);
        }

        [Fact]
        public void GetProgressionStatusDisplayName_ShouldReturnCorrectName()
        {
            // Test la méthode qui convertit les statuts de progression en noms d'affichage
            var detailsComponent = new Details();

            var methodInfo = detailsComponent.GetType().GetMethod("GetProgressionStatusDisplayName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var notStartedName = methodInfo.Invoke(detailsComponent, new object[] { ProgressionStatus.NotStarted });
            var inProgressName = methodInfo.Invoke(detailsComponent, new object[] { ProgressionStatus.InProgress });
            var completedName = methodInfo.Invoke(detailsComponent, new object[] { ProgressionStatus.Completed });

            Assert.Equal("Non commencé", notStartedName);
            Assert.Equal("En cours", inProgressName);
            Assert.Equal("Terminé", completedName);
        }

        [Fact]
        public void NavigationManager_ShouldRedirectCorrectly()
        {
            _navigationManager.NavigateTo("/resources");
            Assert.EndsWith("/resources", _navigationManager.Uri);
            _navigationManager.NavigateTo("notfound");
            Assert.EndsWith("notfound", _navigationManager.Uri);
        }

        [Fact]
        public void AddComment_ShouldAddToDatabase()
        {
            using (var context = _mockDbFactory.Object.CreateDbContext())
            {
                var initialCount = context.Comments.Count();
                var newComment = new Comment
                {
                    Id = 3, 
                    Content = "Nouveau commentaire de test"
                };

                context.Comments.Add(newComment);
                context.SaveChanges();

                var newCount = context.Comments.Count();
                Assert.Equal(initialCount + 1, newCount);
            }
        }

        [Fact]
        public void StartProgression_ShouldCreateProgressionRecord()
        {
            using (var context = _mockDbFactory.Object.CreateDbContext())
            {
                var initialCount = context.Progressions.Count();

                var newProgression = new Progression
                {
                    Id = 2, 
                    Percentage = 0,
                    Status = ProgressionStatus.NotStarted,
                    LastInteractionDate = DateTime.Now
                };

                context.Progressions.Add(newProgression);
                context.SaveChanges();

                var newCount = context.Progressions.Count();
                Assert.Equal(initialCount + 1, newCount);
            }
        }
    }
}
