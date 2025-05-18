using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Moq;
using R2.Data.Entities;
using R2.UI.Components.Pages.AuthPages;
using System.Threading.Tasks;
using Xunit;

namespace R2.Tests.Components
{
    public class LoginTests : TestContext
    {
        private readonly Mock<SignInManager<User>> _mockSignInManager;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<ILogger<Login>> _mockLogger;
        private readonly Mock<IJSRuntime> _mockJSRuntime;
        private readonly Mock<IAntiforgery> _mockAntiforgery;
        private readonly User _testUser;
        private readonly NavigationManager _navigationManager;

        public LoginTests()
        {
            _testUser = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                IsAccountActivated = true
            };

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

            _mockLogger = new Mock<ILogger<Login>>();
            _mockJSRuntime = new Mock<IJSRuntime>();
            _mockAntiforgery = new Mock<IAntiforgery>();

            var antiforgeryTokenSet = new AntiforgeryTokenSet("requestToken", "cookieToken", "formFieldName", "headerName");
            _mockAntiforgery.Setup(a => a.GetAndStoreTokens(It.IsAny<HttpContext>()))
                .Returns(antiforgeryTokenSet);

            Services.AddSingleton(_mockSignInManager.Object);
            Services.AddSingleton(_mockUserManager.Object);
            Services.AddSingleton(_mockLogger.Object);
            Services.AddSingleton(_mockJSRuntime.Object);
            Services.AddSingleton(_mockAntiforgery.Object);
            Services.AddSingleton(Mock.Of<IHttpContextAccessor>());

            _navigationManager = Services.GetRequiredService<NavigationManager>();
        }

        [Fact]
        public async Task LoginForm_WhenSubmittedWithValidCredentials_ShouldSignInUser()
        {
            // Arrange
            _mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
                .ReturnsAsync(_testUser);

            _mockUserManager.Setup(x => x.FindByNameAsync("test@example.com"))
                .ReturnsAsync((User)null);

            _mockSignInManager.Setup(x => x.PasswordSignInAsync(
                    _testUser.UserName,
                    "Password123",
                    false,
                    false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var cut = RenderComponent<Login>();

            cut.Find("#username").Change("test@example.com");
            cut.Find("#password").Change("Password123");

            await cut.Find("form").SubmitAsync();

            // Assert
            _mockSignInManager.Verify(x => x.PasswordSignInAsync(
                _testUser.UserName,
                "Password123",
                false,
                false), Times.Once);

            Assert.EndsWith("/", _navigationManager.Uri);

            Assert.Empty(cut.FindAll(".alert-danger"));
        }

        [Fact]
        public async Task LoginForm_WhenSubmittedWithInvalidCredentials_ShouldShowErrorMessage()
        {
            _mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
                .ReturnsAsync(_testUser);

            _mockUserManager.Setup(x => x.FindByNameAsync("test@example.com"))
                .ReturnsAsync((User)null);

            _mockSignInManager.Setup(x => x.PasswordSignInAsync(
                    _testUser.UserName,
                    "WrongPassword",
                    false,
                    false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var cut = RenderComponent<Login>();

            cut.Find("#username").Change("test@example.com");
            cut.Find("#password").Change("WrongPassword");

            await cut.Find("form").SubmitAsync();

            _mockSignInManager.Verify(x => x.PasswordSignInAsync(
                _testUser.UserName,
                "WrongPassword",
                false,
                false), Times.Once);

            var errorMessage = cut.Find(".alert-danger");
            Assert.Contains("Nom d'utilisateur ou mot de passe incorrect", errorMessage.TextContent);
        }

        [Fact]
        public async Task LoginForm_WhenUserNotFound_ShouldShowErrorMessage()
        {
            _mockUserManager.Setup(x => x.FindByEmailAsync("nonexistent@example.com"))
                .ReturnsAsync((User)null);

            _mockUserManager.Setup(x => x.FindByNameAsync("nonexistent@example.com"))
                .ReturnsAsync((User)null);

            var cut = RenderComponent<Login>();

            cut.Find("#username").Change("nonexistent@example.com");
            cut.Find("#password").Change("Password123");

            await cut.Find("form").SubmitAsync();

            _mockSignInManager.Verify(x => x.PasswordSignInAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()), Times.Never);

            var errorMessage = cut.Find(".alert-danger");
            Assert.Contains("Nom d'utilisateur ou mot de passe incorrect", errorMessage.TextContent);
        }

        [Fact]
        public async Task LoginForm_WhenAccountIsDeactivated_ShouldShowErrorMessage()
        {
            var deactivatedUser = new User
            {
                Id = 2,
                UserName = "deactivateduser",
                Email = "deactivated@example.com",
                IsAccountActivated = false
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync("deactivated@example.com"))
                .ReturnsAsync(deactivatedUser);

            _mockUserManager.Setup(x => x.FindByNameAsync("deactivated@example.com"))
                .ReturnsAsync((User)null);

            var cut = RenderComponent<Login>();

            cut.Find("#username").Change("deactivated@example.com");
            cut.Find("#password").Change("Password123");

            await cut.Find("form").SubmitAsync();

            _mockSignInManager.Verify(x => x.PasswordSignInAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()), Times.Never);

            var errorMessage = cut.Find(".alert-danger");
            Assert.Contains("Impossible de se connecter à ce compte, il est actuellement désactivé", errorMessage.TextContent);
        }

        [Fact]
        public async Task LoginForm_WhenAccountIsLockedOut_ShouldShowErrorMessage()
        {
            _mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
                .ReturnsAsync(_testUser);

            _mockUserManager.Setup(x => x.FindByNameAsync("test@example.com"))
                .ReturnsAsync((User)null);

            _mockSignInManager.Setup(x => x.PasswordSignInAsync(
                    _testUser.UserName,
                    "Password123",
                    false,
                    false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

            var cut = RenderComponent<Login>();

            cut.Find("#username").Change("test@example.com");
            cut.Find("#password").Change("Password123");

            await cut.Find("form").SubmitAsync();

            _mockSignInManager.Verify(x => x.PasswordSignInAsync(
                _testUser.UserName,
                "Password123",
                false,
                false), Times.Once);

            var errorMessage = cut.Find(".alert-danger");
            Assert.Contains("Votre compte est verrouillé", errorMessage.TextContent);
        }

        [Fact]
        public async Task LoginForm_WhenRequiresTwoFactor_ShouldShowErrorMessage()
        {
            _mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
                .ReturnsAsync(_testUser);

            _mockUserManager.Setup(x => x.FindByNameAsync("test@example.com"))
                .ReturnsAsync((User)null);

            _mockSignInManager.Setup(x => x.PasswordSignInAsync(
                    _testUser.UserName,
                    "Password123",
                    false,
                    false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.TwoFactorRequired);

            var cut = RenderComponent<Login>();

            cut.Find("#username").Change("test@example.com");
            cut.Find("#password").Change("Password123");

            await cut.Find("form").SubmitAsync();

            _mockSignInManager.Verify(x => x.PasswordSignInAsync(
                _testUser.UserName,
                "Password123",
                false,
                false), Times.Once);

            var errorMessage = cut.Find(".alert-danger");
            Assert.Contains("L'authentification à deux facteurs est requise", errorMessage.TextContent);
        }

        [Fact]
        public void LoginForm_WhenSubmittedWithEmptyFields_ShouldShowValidationErrors()
        {
            var cut = RenderComponent<Login>();

            cut.Find("form").Submit();

            var validationMessages = cut.FindAll(".validation-message");
            Assert.Equal(4, validationMessages.Count); // Il y a 4 messages de validation

            var usernameValidation = cut.Find("#username + .validation-message");
            Assert.NotNull(usernameValidation);

            var passwordValidation = cut.Find("#password + .validation-message");
            Assert.NotNull(passwordValidation);
        }

        [Fact]
        public async Task LoginForm_ShouldHandleException_AndShowErrorMessage()
        {
            _mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
                .ThrowsAsync(new System.Exception("Test exception"));

            var cut = RenderComponent<Login>();

            cut.Find("#username").Change("test@example.com");
            cut.Find("#password").Change("Password123");

            await cut.Find("form").SubmitAsync();

            var errorMessage = cut.Find(".alert-danger");
            Assert.Contains("Une erreur est survenue", errorMessage.TextContent);
            Assert.Contains("Test exception", errorMessage.TextContent);
        }

        [Fact]
        public async Task RememberMe_ShouldBePassedToSignInManager()
        {
            _mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
                .ReturnsAsync(_testUser);

            _mockUserManager.Setup(x => x.FindByNameAsync("test@example.com"))
                .ReturnsAsync((User)null);

            _mockSignInManager.Setup(x => x.PasswordSignInAsync(
                    _testUser.UserName,
                    "Password123",
                    true,
                    false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var cut = RenderComponent<Login>();

            cut.Find("#username").Change("test@example.com");
            cut.Find("#password").Change("Password123");
            cut.Find("#rememberMe").Change(true);

            await cut.Find("form").SubmitAsync();

            _mockSignInManager.Verify(x => x.PasswordSignInAsync(
                _testUser.UserName,
                "Password123",
                true, 
                false), Times.Once);
        }

        [Fact]
        public async Task Form_ShouldShowProcessingStateWhileSubmitting()
        {
            var tcs = new TaskCompletionSource<User>();
            _mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
                .Returns(tcs.Task);

            var cut = RenderComponent<Login>();

            cut.Find("#username").Change("test@example.com");
            cut.Find("#password").Change("Password123");

            var submitTask = cut.Find("form").SubmitAsync();

            var buttonText = cut.Find("button[type='submit'] span").TextContent;
            Assert.Equal("Connexion en cours...", buttonText);

            tcs.SetResult(_testUser);

            await submitTask;
        }
    }
}
