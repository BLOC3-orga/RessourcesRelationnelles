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
using System.Threading.Tasks;
using Xunit;

namespace R2.Tests.Components
{
    public class LogoutTests : TestContext
    {
        private readonly Mock<SignInManager<User>> _mockSignInManager;
        private readonly NavigationManager _navigationManager;
        private readonly Mock<IAntiforgery> _mockAntiforgery;

        public LogoutTests()
        {
            // Setup mocks
            var userStoreMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(
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
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                mockUserClaimsPrincipalFactory.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<ILogger<SignInManager<User>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<User>>());

            _mockSignInManager.Setup(x => x.SignOutAsync())
                .Returns(Task.CompletedTask);

            _mockAntiforgery = new Mock<IAntiforgery>();
            var antiforgeryTokenSet = new AntiforgeryTokenSet("requestToken", "cookieToken", "formFieldName", "headerName");
            _mockAntiforgery.Setup(a => a.GetAndStoreTokens(It.IsAny<HttpContext>()))
                .Returns(antiforgeryTokenSet);

            Services.AddSingleton(_mockSignInManager.Object);
            Services.AddSingleton(_mockAntiforgery.Object);
            Services.AddSingleton(Mock.Of<IHttpContextAccessor>());

            _navigationManager = Services.GetRequiredService<NavigationManager>();
        }

        [Fact]
        public void WhenRendered_ShouldSignOutAndRedirectToHomePage()
        {
            var cut = RenderComponent<Logout>();

            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);

            Assert.EndsWith("/", _navigationManager.Uri);
        }

        [Fact]
        public void NavigationShouldOccurAfterSignOut()
        {
            var taskCompletionSource = new TaskCompletionSource<object>();

            _mockSignInManager.Setup(x => x.SignOutAsync())
                .Returns(taskCompletionSource.Task);

            var cut = RenderComponent<Logout>();

            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);

            var initialUri = _navigationManager.Uri;

            taskCompletionSource.SetResult(null);

            Assert.EndsWith("/", _navigationManager.Uri);
        }
    }
}
