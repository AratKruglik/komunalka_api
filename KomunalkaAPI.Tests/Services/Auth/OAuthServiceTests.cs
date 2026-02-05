using FluentAssertions;
using KomunalkaAPI.DTO.Auth;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;
using KomunalkaAPI.Repositories.RefreshToken;
using KomunalkaAPI.Repositories.User;
using KomunalkaAPI.Services.Auth;
using KomunalkaAPI.Services.Auth.Providers;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace KomunalkaAPI.Tests.Services.Auth;

public class OAuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IOAuthProvider> _googleProviderMock;
    private readonly Mock<IOAuthProvider> _gitHubProviderMock;
    private readonly Mock<ILogger<OAuthService>> _loggerMock;
    private readonly IMemoryCache _stateCache;
    private readonly OAuthService _sut;

    public OAuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _googleProviderMock = new Mock<IOAuthProvider>();
        _gitHubProviderMock = new Mock<IOAuthProvider>();
        _loggerMock = new Mock<ILogger<OAuthService>>();
        _stateCache = new MemoryCache(new MemoryCacheOptions());

        _googleProviderMock.Setup(p => p.ProviderName).Returns("Google");
        _gitHubProviderMock.Setup(p => p.ProviderName).Returns("GitHub");

        _googleProviderMock.Setup(p => p.GetAuthorizationUrl(It.IsAny<string>(), It.IsAny<string?>()))
            .Returns((string state, string? _) => $"https://accounts.google.com/oauth?state={state}");
        _gitHubProviderMock.Setup(p => p.GetAuthorizationUrl(It.IsAny<string>(), It.IsAny<string?>()))
            .Returns((string state, string? _) => $"https://github.com/login/oauth?state={state}");

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.RefreshTokens).Returns(_refreshTokenRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        _refreshTokenRepositoryMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((EntityEntry<RefreshToken>)null!);

        _sut = new OAuthService(
            _unitOfWorkMock.Object,
            _jwtServiceMock.Object,
            new[] { _googleProviderMock.Object, _gitHubProviderMock.Object },
            _loggerMock.Object,
            _stateCache);
    }

    #region AuthenticateWithProviderAsync Tests

    [Fact]
    public async Task AuthenticateWithProviderAsync_ValidToken_ReturnsAuthResponse()
    {
        // Arrange
        var userInfo = CreateOAuthUserInfo("Google");
        var user = CreateUser(userInfo);
        var refreshToken = CreateRefreshToken(user);

        _googleProviderMock.Setup(p => p.ValidateTokenAsync("valid-token"))
            .ReturnsAsync(userInfo);
        _userRepositoryMock.Setup(r => r.GetByProviderAsync("Google", "ext-123"))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(j => j.GenerateJwtToken(user))
            .Returns("jwt-token");
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken(user))
            .Returns(refreshToken);
        _jwtServiceMock.Setup(j => j.GetTokenExpirationTime("jwt-token"))
            .Returns(DateTime.UtcNow.AddMinutes(30));

        // Act
        var result = await _sut.AuthenticateWithProviderAsync("Google", "valid-token");

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
        result.Token.Should().Be("jwt-token");
        result.AuthProvider.Should().Be("Google");
    }

    [Fact]
    public async Task AuthenticateWithProviderAsync_InvalidProvider_ReturnsNull()
    {
        // Act
        var result = await _sut.AuthenticateWithProviderAsync("InvalidProvider", "token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateWithProviderAsync_InvalidToken_ReturnsNull()
    {
        // Arrange
        _googleProviderMock.Setup(p => p.ValidateTokenAsync("invalid-token"))
            .ReturnsAsync((OAuthUserInfo?)null);

        // Act
        var result = await _sut.AuthenticateWithProviderAsync("Google", "invalid-token");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region FindOrCreateUser Tests

    [Fact]
    public async Task AuthenticateWithProviderAsync_ExistingUserByProvider_ReturnsUser()
    {
        // Arrange
        var userInfo = CreateOAuthUserInfo("Google");
        var existingUser = CreateUser(userInfo);
        var refreshToken = CreateRefreshToken(existingUser);

        _googleProviderMock.Setup(p => p.ValidateTokenAsync("token"))
            .ReturnsAsync(userInfo);
        _userRepositoryMock.Setup(r => r.GetByProviderAsync("Google", "ext-123"))
            .ReturnsAsync(existingUser);
        SetupJwtService(existingUser, refreshToken);

        // Act
        var result = await _sut.AuthenticateWithProviderAsync("Google", "token");

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(existingUser.Id);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task AuthenticateWithProviderAsync_ExistingUserByEmail_NoPassword_LinksProvider()
    {
        // Arrange
        var userInfo = CreateOAuthUserInfo("Google");
        var existingUser = new User
        {
            Id = 1,
            Username = "existing",
            Email = "test@example.com",
            Password = null,
            AuthProvider = "Local"
        };
        var refreshToken = CreateRefreshToken(existingUser);

        _googleProviderMock.Setup(p => p.ValidateTokenAsync("token"))
            .ReturnsAsync(userInfo);
        _userRepositoryMock.Setup(r => r.GetByProviderAsync("Google", "ext-123"))
            .ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(existingUser);
        SetupJwtService(existingUser, refreshToken);

        // Act
        var result = await _sut.AuthenticateWithProviderAsync("Google", "token");

        // Assert
        result.Should().NotBeNull();
        existingUser.AuthProvider.Should().Be("Google");
        existingUser.ExternalId.Should().Be("ext-123");
        _userRepositoryMock.Verify(r => r.Update(existingUser), Times.Once);
    }

    [Fact]
    public async Task AuthenticateWithProviderAsync_ExistingUserByEmail_HasPassword_ReturnsNull()
    {
        // Arrange
        var userInfo = CreateOAuthUserInfo("Google");
        var existingUser = new User
        {
            Id = 1,
            Username = "existing",
            Email = "test@example.com",
            Password = "hashed-password",
            AuthProvider = "Local"
        };

        _googleProviderMock.Setup(p => p.ValidateTokenAsync("token"))
            .ReturnsAsync(userInfo);
        _userRepositoryMock.Setup(r => r.GetByProviderAsync("Google", "ext-123"))
            .ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _sut.AuthenticateWithProviderAsync("Google", "token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateWithProviderAsync_NewUser_CreatesUser()
    {
        // Arrange
        var userInfo = CreateOAuthUserInfo("Google");
        User? capturedUser = null;

        _googleProviderMock.Setup(p => p.ValidateTokenAsync("token"))
            .ReturnsAsync(userInfo);
        _userRepositoryMock.Setup(r => r.GetByProviderAsync("Google", "ext-123"))
            .ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(Array.Empty<User>());
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((EntityEntry<User>)null!);

        _jwtServiceMock.Setup(j => j.GenerateJwtToken(It.IsAny<User>()))
            .Returns("jwt-token");
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken(It.IsAny<User>()))
            .Returns(new RefreshToken { Token = "refresh", UserId = 0, ExpiryDate = DateTime.UtcNow.AddDays(7) });
        _jwtServiceMock.Setup(j => j.GetTokenExpirationTime("jwt-token"))
            .Returns(DateTime.UtcNow.AddMinutes(30));

        // Act
        var result = await _sut.AuthenticateWithProviderAsync("Google", "token");

        // Assert
        result.Should().NotBeNull();
        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be("test@example.com");
        capturedUser.AuthProvider.Should().Be("Google");
        capturedUser.ExternalId.Should().Be("ext-123");
    }

    #endregion

    #region HandleCallbackAsync Tests

    [Fact]
    public async Task HandleCallbackAsync_ValidState_ProcessesSuccessfully()
    {
        // Arrange - setup all mocks before getting auth URL
        var userInfo = CreateOAuthUserInfo("Google");
        var user = CreateUser(userInfo);
        var refreshToken = CreateRefreshToken(user);

        _googleProviderMock.Setup(p => p.ExchangeCodeAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(userInfo);
        _userRepositoryMock.Setup(r => r.GetByProviderAsync("Google", "ext-123"))
            .ReturnsAsync(user);
        SetupJwtService(user, refreshToken);

        // Get the auth URL which registers the state in cache
        var authUrl = _sut.GetAuthorizationUrl("Google");
        var state = ExtractStateFromUrl(authUrl);

        var request = new OAuthCallbackRequest
        {
            Provider = "Google",
            Code = "auth-code",
            State = state
        };

        // Act
        var result = await _sut.HandleCallbackAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task HandleCallbackAsync_MissingState_ReturnsNull()
    {
        // Arrange
        var request = new OAuthCallbackRequest
        {
            Provider = "Google",
            Code = "auth-code",
            State = null
        };

        // Act
        var result = await _sut.HandleCallbackAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleCallbackAsync_InvalidState_ReturnsNull()
    {
        // Arrange
        var request = new OAuthCallbackRequest
        {
            Provider = "Google",
            Code = "auth-code",
            State = "invalid-state"
        };

        // Act
        var result = await _sut.HandleCallbackAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleCallbackAsync_ExpiredState_ReturnsNull()
    {
        // Arrange
        var expiredCache = new MemoryCache(new MemoryCacheOptions());
        var serviceWithExpiredCache = new OAuthService(
            _unitOfWorkMock.Object,
            _jwtServiceMock.Object,
            new[] { _googleProviderMock.Object },
            _loggerMock.Object,
            expiredCache);

        var request = new OAuthCallbackRequest
        {
            Provider = "Google",
            Code = "auth-code",
            State = "some-expired-state"
        };

        // Act
        var result = await serviceWithExpiredCache.HandleCallbackAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleCallbackAsync_ProviderMismatch_ReturnsNull()
    {
        // Arrange
        var authUrl = _sut.GetAuthorizationUrl("Google");
        var state = ExtractStateFromUrl(authUrl);

        var request = new OAuthCallbackRequest
        {
            Provider = "GitHub",
            Code = "auth-code",
            State = state
        };

        // Act
        var result = await _sut.HandleCallbackAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleCallbackAsync_WithError_ReturnsNull()
    {
        // Arrange
        var request = new OAuthCallbackRequest
        {
            Provider = "Google",
            Error = "access_denied",
            ErrorDescription = "User denied access"
        };

        // Act
        var result = await _sut.HandleCallbackAsync(request);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region LinkProviderAsync Tests

    [Fact]
    public async Task LinkProviderAsync_ValidRequest_LinksSuccessfully()
    {
        // Arrange
        var user = new User { Id = 1, Username = "test", Email = "test@example.com", AuthProvider = "Local" };
        var userInfo = CreateOAuthUserInfo("Google");

        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _googleProviderMock.Setup(p => p.ValidateTokenAsync("token")).ReturnsAsync(userInfo);
        _userRepositoryMock.Setup(r => r.GetByProviderAsync("Google", "ext-123"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.LinkProviderAsync(1, "Google", "token");

        // Assert
        result.Should().BeTrue();
        user.AuthProvider.Should().Be("Google");
        user.ExternalId.Should().Be("ext-123");
        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
    }

    [Fact]
    public async Task LinkProviderAsync_UserNotFound_ReturnsFalse()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

        // Act
        var result = await _sut.LinkProviderAsync(999, "Google", "token");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task LinkProviderAsync_ProviderAlreadyLinkedToOtherUser_ReturnsFalse()
    {
        // Arrange
        var user = new User { Id = 1, Username = "test", Email = "test@example.com", AuthProvider = "Local" };
        var otherUser = new User { Id = 2, Username = "other", Email = "other@example.com", AuthProvider = "Google", ExternalId = "ext-123" };
        var userInfo = CreateOAuthUserInfo("Google");

        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _googleProviderMock.Setup(p => p.ValidateTokenAsync("token")).ReturnsAsync(userInfo);
        _userRepositoryMock.Setup(r => r.GetByProviderAsync("Google", "ext-123"))
            .ReturnsAsync(otherUser);

        // Act
        var result = await _sut.LinkProviderAsync(1, "Google", "token");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UnlinkProviderAsync Tests

    [Fact]
    public async Task UnlinkProviderAsync_HasPassword_UnlinksSuccessfully()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "test",
            Email = "test@example.com",
            Password = "hashed-password",
            AuthProvider = "Google",
            ExternalId = "ext-123"
        };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _sut.UnlinkProviderAsync(1, "Google");

        // Assert
        result.Should().BeTrue();
        user.AuthProvider.Should().Be("Local");
        user.ExternalId.Should().BeNull();
        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
    }

    [Fact]
    public async Task UnlinkProviderAsync_NoPassword_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "test",
            Email = "test@example.com",
            Password = null,
            AuthProvider = "Google",
            ExternalId = "ext-123"
        };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _sut.UnlinkProviderAsync(1, "Google");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UnlinkProviderAsync_DifferentProvider_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "test",
            Email = "test@example.com",
            Password = "hashed-password",
            AuthProvider = "GitHub",
            ExternalId = "ext-123"
        };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _sut.UnlinkProviderAsync(1, "Google");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetAuthorizationUrl Tests

    [Fact]
    public void GetAuthorizationUrl_ValidProvider_ReturnsUrl()
    {
        // Arrange
        _googleProviderMock.Setup(p => p.GetAuthorizationUrl(It.IsAny<string>(), null))
            .Returns("https://accounts.google.com/o/oauth2/auth?state=abc");

        // Act
        var result = _sut.GetAuthorizationUrl("Google");

        // Assert
        result.Should().NotBeNullOrEmpty();
        _googleProviderMock.Verify(p => p.GetAuthorizationUrl(It.IsAny<string>(), null), Times.Once);
    }

    [Fact]
    public void GetAuthorizationUrl_InvalidProvider_ThrowsException()
    {
        // Act
        var act = () => _sut.GetAuthorizationUrl("InvalidProvider");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("OAuth provider not found: InvalidProvider");
    }

    #endregion

    #region Helper Methods

    private static OAuthUserInfo CreateOAuthUserInfo(string provider) => new()
    {
        Provider = provider,
        ExternalId = "ext-123",
        Email = "test@example.com",
        EmailVerified = true,
        FirstName = "Test",
        LastName = "User"
    };

    private static User CreateUser(OAuthUserInfo userInfo) => new()
    {
        Id = 1,
        Username = "testuser",
        Email = userInfo.Email,
        FirstName = userInfo.FirstName,
        LastName = userInfo.LastName,
        AuthProvider = userInfo.Provider,
        ExternalId = userInfo.ExternalId,
        EmailVerified = userInfo.EmailVerified,
        Role = "User"
    };

    private static RefreshToken CreateRefreshToken(User user) => new()
    {
        Id = 1,
        Token = "refresh-token",
        UserId = user.Id,
        ExpiryDate = DateTime.UtcNow.AddDays(7)
    };

    private void SetupJwtService(User user, RefreshToken refreshToken)
    {
        _jwtServiceMock.Setup(j => j.GenerateJwtToken(user)).Returns("jwt-token");
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken(user)).Returns(refreshToken);
        _jwtServiceMock.Setup(j => j.GetTokenExpirationTime("jwt-token"))
            .Returns(DateTime.UtcNow.AddMinutes(30));
    }

    private static string ExtractStateFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            throw new InvalidOperationException("URL is null or empty");

        string queryString;
        if (url.Contains('?'))
        {
            queryString = url.Substring(url.IndexOf('?'));
        }
        else
        {
            queryString = "?" + url;
        }

        var uri = new Uri("https://example.com" + queryString);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        return query["state"] ?? throw new InvalidOperationException("State not found in URL");
    }

    #endregion
}
