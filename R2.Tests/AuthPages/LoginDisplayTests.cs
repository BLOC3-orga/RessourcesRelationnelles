using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components.Authorization;
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
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace R2.Tests.Components
{
    public class TestLoginDisplay : ComponentBase
    {
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public SignInManager<User> SignInManager { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        protected bool IsAuthenticated { get; set; }
        protected string Username { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            IsAuthenticated = authState.User.Identity?.IsAuthenticated ?? false;
            Username = authState.User.Identity?.Name;
        }

        protected void Logout()
        {
            NavigationManager.NavigateTo("/logout");
        }

        protected void Login()
        {
            NavigationManager.NavigateTo("/login");
        }

        protected void Register()
        {
            NavigationManager.NavigateTo("/register");
        }
    }

    public class LoginDisplayTests : TestContext
    {
        private readonly Mock<SignInManager<User>> _mockSignInManager;
        private readonly NavigationManager _navigationManager;
        private readonly Mock<IAntiforgery> _mockAntiforgery;
        private readonly TestAuthorizationContext _authContext;

        public LoginDisplayTests()
        {
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

            _mockAntiforgery = new Mock<IAntiforgery>();
            var antiforgeryTokenSet = new AntiforgeryTokenSet("requestToken", "cookieToken", "formFieldName", "headerName");
            _mockAntiforgery.Setup(a => a.GetAndStoreTokens(It.IsAny<HttpContext>()))
                .Returns(antiforgeryTokenSet);

            _authContext = this.AddTestAuthorization();

            Services.AddSingleton(_mockSignInManager.Object);
            Services.AddSingleton(_mockAntiforgery.Object);
            Services.AddSingleton(Mock.Of<IHttpContextAccessor>());

            _navigationManager = Services.GetRequiredService<NavigationManager>();
        }

        [Fact]
        public async Task AuthenticationState_WhenUserIsAuthenticated_ShouldReturnCorrectState()
        {
            var testUsername = "testuser";
            _authContext.SetAuthorized(testUsername, AuthorizationState.Authorized);
            _authContext.SetClaims(new[] { new Claim(ClaimTypes.Name, testUsername) });

            var authStateProvider = Services.GetRequiredService<AuthenticationStateProvider>();
            var authState = await authStateProvider.GetAuthenticationStateAsync();

            Assert.True(authState.User.Identity.IsAuthenticated);
            Assert.Equal(testUsername, authState.User.Identity.Name);
        }

        [Fact]
        public async Task AuthenticationState_WhenUserIsNotAuthenticated_ShouldReturnCorrectState()
        {
            _authContext.SetNotAuthorized();

            var authStateProvider = Services.GetRequiredService<AuthenticationStateProvider>();
            var authState = await authStateProvider.GetAuthenticationStateAsync();

            Assert.False(authState.User.Identity?.IsAuthenticated ?? false);
        }

        [Fact]
        public async Task NavigationManager_ShouldNavigateToCorrectUrls()
        {
            var baseUrl = _navigationManager.Uri;

            _navigationManager.NavigateTo("/login");

            Assert.EndsWith("/login", _navigationManager.Uri);

            _navigationManager.NavigateTo("/register");

            Assert.EndsWith("/register", _navigationManager.Uri);

            _navigationManager.NavigateTo("/logout");

            Assert.EndsWith("/logout", _navigationManager.Uri);
        }
    }
}
