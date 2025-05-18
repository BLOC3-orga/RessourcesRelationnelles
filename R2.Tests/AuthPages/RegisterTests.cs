using Bunit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using R2.Data.Entities;
using R2.UI.Components.Pages.AuthPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace R2.Tests.Components
{
    public class RegisterTests : TestContext
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<SignInManager<User>> _mockSignInManager;
        private readonly Mock<ILogger<Register>> _mockLogger;
        private readonly Mock<IAntiforgery> _mockAntiforgery;
        private readonly NavigationManager _navigationManager;

        public RegisterTests()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStoreMock.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<User>>(),
                Array.Empty<IUserValidator<User>>(),
                Array.Empty<IPasswordValidator<User>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<User>>>());

            var mockUserClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            _mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                mockUserClaimsPrincipalFactory.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<ILogger<SignInManager<User>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<User>>());

            _mockLogger = new Mock<ILogger<Register>>();

            _mockAntiforgery = new Mock<IAntiforgery>();
            var antiforgeryTokenSet = new AntiforgeryTokenSet("requestToken", "cookieToken", "formFieldName", "headerName");
            _mockAntiforgery.Setup(a => a.GetAndStoreTokens(It.IsAny<HttpContext>()))
                .Returns(antiforgeryTokenSet);

            Services.AddSingleton(_mockUserManager.Object);
            Services.AddSingleton(_mockSignInManager.Object);
            Services.AddSingleton(_mockLogger.Object);
            Services.AddSingleton(_mockAntiforgery.Object);
            Services.AddSingleton(Mock.Of<IHttpContextAccessor>());

            _navigationManager = Services.GetRequiredService<NavigationManager>();
        }

        [Fact]
        public async Task RegisterForm_WhenSubmittedWithValidData_ShouldCreateUserAndSignIn()
        {
            var userEmail = "test@example.com";
            var userPseudo = "testuser";
            var userPassword = "Test123!";

            _mockUserManager.Setup(x => x.FindByNameAsync(userPseudo))
                .ReturnsAsync((User)null);

            _mockUserManager.Setup(x => x.FindByEmailAsync(userEmail))
                .ReturnsAsync((User)null);

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), userPassword))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "Citoyen"))
                .ReturnsAsync(IdentityResult.Success);

            _mockSignInManager.Setup(x => x.SignInAsync(It.IsAny<User>(), false, null))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent<Register>();

            cut.Find("#email").Change(userEmail);
            cut.Find("#name").Change("John");
            cut.Find("#lastname").Change("Doe");
            cut.Find("#pseudo").Change(userPseudo);
            cut.Find("#city").Change("Paris");
            cut.Find("#address").Change("123 Test St");
            cut.Find("#password").Change(userPassword);
            cut.Find("#confirmPassword").Change(userPassword);

            await cut.Find("form").SubmitAsync();

            _mockUserManager.Verify(x => x.CreateAsync(
                It.Is<User>(u => u.Email == userEmail && u.UserName == userPseudo),
                userPassword),
                Times.Once);

            _mockUserManager.Verify(x => x.AddToRoleAsync(
                It.IsAny<User>(),
                "Citoyen"),
                Times.Once);

            _mockSignInManager.Verify(x => x.SignInAsync(
                It.IsAny<User>(),
                false,
                null),
                Times.Once);

            Assert.EndsWith("/", _navigationManager.Uri);

            Assert.Empty(cut.FindAll(".alert-danger"));
        }

        [Fact]
        public async Task RegisterForm_WhenPseudoAlreadyExists_ShouldShowErrorMessage()
        {
            var existingUser = new User
            {
                Id = 1,
                UserName = "existinguser",
                Email = "existing@example.com"
            };

            _mockUserManager.Setup(x => x.FindByNameAsync("existinguser"))
                .ReturnsAsync(existingUser);

            var cut = RenderComponent<Register>();

            cut.Find("#email").Change("new@example.com");
            cut.Find("#name").Change("John");
            cut.Find("#lastname").Change("Doe");
            cut.Find("#pseudo").Change("existinguser");
            cut.Find("#password").Change("Test123!");
            cut.Find("#confirmPassword").Change("Test123!");

            await cut.Find("form").SubmitAsync();

            _mockUserManager.Verify(x => x.CreateAsync(
                It.IsAny<User>(),
                It.IsAny<string>()),
                Times.Never);

            var errorMessage = cut.Find(".alert-danger");
            Assert.Contains("Ce pseudo est déjà utilisé", errorMessage.TextContent);
        }

        [Fact]
        public async Task RegisterForm_WhenEmailAlreadyExists_ShouldShowErrorMessage()
        {
            var existingUser = new User
            {
                Id = 1,
                UserName = "existinguser",
                Email = "existing@example.com"
            };

            _mockUserManager.Setup(x => x.FindByNameAsync("newuser"))
                .ReturnsAsync((User)null);

            _mockUserManager.Setup(x => x.FindByEmailAsync("existing@example.com"))
                .ReturnsAsync(existingUser);

            var cut = RenderComponent<Register>();

            cut.Find("#email").Change("existing@example.com");
            cut.Find("#name").Change("John");
            cut.Find("#lastname").Change("Doe");
            cut.Find("#pseudo").Change("newuser");
            cut.Find("#password").Change("Test123!");
            cut.Find("#confirmPassword").Change("Test123!");

            await cut.Find("form").SubmitAsync();

            _mockUserManager.Verify(x => x.CreateAsync(
                It.IsAny<User>(),
                It.IsAny<string>()),
                Times.Never);

            var errorMessage = cut.Find(".alert-danger");
            Assert.Contains("Cet email est déjà utilisé", errorMessage.TextContent);
        }

        [Fact]
        public async Task RegisterForm_WhenPasswordsDontMatch_ShouldShowErrorMessage()
        {
            var cut = RenderComponent<Register>();

            cut.Find("#email").Change("test@example.com");
            cut.Find("#name").Change("John");
            cut.Find("#lastname").Change("Doe");
            cut.Find("#pseudo").Change("testuser");
            cut.Find("#password").Change("Test123!");
            cut.Find("#confirmPassword").Change("DifferentPassword123!");

            await cut.Find("form").SubmitAsync();

            _mockUserManager.Verify(x => x.CreateAsync(
                It.IsAny<User>(),
                It.IsAny<string>()),
                Times.Never);

            var validationMessages = cut.FindAll(".validation-message");
            Assert.True(validationMessages.Any(m => m.TextContent.Contains("Les mots de passe ne correspondent pas")));
        }

        [Fact]
        public async Task RegisterForm_WhenCreateUserFails_ShouldShowErrorMessage()
        {
            var identityErrors = new List<IdentityError>
            {
                new IdentityError { Description = "Le mot de passe doit contenir au moins un caractère spécial." }
            };

            var failedResult = IdentityResult.Failed(identityErrors.ToArray());

            _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(failedResult);

            var cut = RenderComponent<Register>();

            cut.Find("#email").Change("test@example.com");
            cut.Find("#name").Change("John");
            cut.Find("#lastname").Change("Doe");
            cut.Find("#pseudo").Change("testuser");
            cut.Find("#password").Change("weakpassword");
            cut.Find("#confirmPassword").Change("weakpassword");

            await cut.Find("form").SubmitAsync();

            var errorMessage = cut.Find(".alert-danger");
            Assert.Contains("Erreur lors de l'inscription", errorMessage.TextContent);
            Assert.Contains("Le mot de passe doit contenir au moins un caractère spécial", errorMessage.TextContent);
        }

        [Fact]
        public async Task RegisterForm_WhenAddToRoleFails_ShouldShowErrorMessage()
        {
            var identityErrors = new List<IdentityError>
            {
                new IdentityError { Description = "Le rôle 'Citoyen' n'existe pas." }
            };

            var failedRoleResult = IdentityResult.Failed(identityErrors.ToArray());

            _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "Citoyen"))
                .ReturnsAsync(failedRoleResult);

            var cut = RenderComponent<Register>();

            cut.Find("#email").Change("test@example.com");
            cut.Find("#name").Change("John");
            cut.Find("#lastname").Change("Doe");
            cut.Find("#pseudo").Change("testuser");
            cut.Find("#password").Change("Test123!");
            cut.Find("#confirmPassword").Change("Test123!");

            await cut.Find("form").SubmitAsync();

            var errorMessage = cut.Find(".alert-danger");
            Assert.Contains("Compte créé mais erreur lors de l'attribution du rôle", errorMessage.TextContent);
            Assert.Contains("Le rôle 'Citoyen' n'existe pas", errorMessage.TextContent);
        }

        [Fact]
        public async Task RegisterForm_WhenExceptionThrown_ShouldShowErrorMessage()
        {
            _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test exception"));

            var cut = RenderComponent<Register>();

            cut.Find("#email").Change("test@example.com");
            cut.Find("#name").Change("John");
            cut.Find("#lastname").Change("Doe");
            cut.Find("#pseudo").Change("testuser");
            cut.Find("#password").Change("Test123!");
            cut.Find("#confirmPassword").Change("Test123!");

            await cut.Find("form").SubmitAsync();

            var errorMessage = cut.Find(".alert-danger");
            Assert.Contains("Une erreur s'est produite", errorMessage.TextContent);
            Assert.Contains("Test exception", errorMessage.TextContent);
        }

        [Fact]
        public void RegisterForm_WhenSubmittedWithEmptyRequiredFields_ShouldShowValidationErrors()
        {
            var cut = RenderComponent<Register>();

            cut.Find("form").Submit();

            // Assert - Validatin messages should be displayed
            var validationMessages = cut.FindAll(".validation-message, .text-danger");
            Assert.True(validationMessages.Count > 0, "Au moins un message de validation devrait être affiché");

            var markup = cut.Markup;
            Assert.Contains("L'email est requis", markup);
            Assert.Contains("Le prénom est requis", markup);
            Assert.Contains("Le nom est requis", markup);
            Assert.Contains("Le pseudo est requis", markup);
            Assert.Contains("Le mot de passe est requis", markup);
            Assert.Contains("La confirmation du mot de passe est requise", markup);
        }

        [Fact]
        public async Task Form_ShouldShowProcessingStateWhileSubmitting()
        {
            var tcs = new TaskCompletionSource<User>();

            _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                .Returns(tcs.Task);

            var cut = RenderComponent<Register>();

            cut.Find("#email").Change("test@example.com");
            cut.Find("#name").Change("John");
            cut.Find("#lastname").Change("Doe");
            cut.Find("#pseudo").Change("testuser");
            cut.Find("#password").Change("Test123!");
            cut.Find("#confirmPassword").Change("Test123!");

            var submitTask = cut.Find("form").SubmitAsync();

            // Vérifier la présence de l'indicateur de chargement
            Assert.Contains("spinner-border", cut.Markup);

            // Rechercher le texte dans les spans du bouton
            var loadingSpans = cut.FindAll("button[type='submit'] span");
            var loadingText = "";
            foreach (var span in loadingSpans)
            {
                if (span.TextContent.Contains("Inscription"))
                {
                    loadingText = span.TextContent;
                    break;
                }
            }
            Assert.Contains("Inscription en cours", loadingText);

            tcs.SetResult(null);

            await submitTask;
        }
    }
}
