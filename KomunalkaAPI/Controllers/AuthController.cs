using KomunalkaAPI.DTO.Auth;
using KomunalkaAPI.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Asp.Versioning.ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IOAuthService _oauthService;

    public AuthController(IAuthService authService, IOAuthService oauthService)
    {
        _authService = authService;
        _oauthService = oauthService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterAsync(request);
        if (result == null)
        {
            return BadRequest(new { Message = "User with this email already exists" });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthenticationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.AuthenticateAsync(request);
        if (result == null)
        {
            return Unauthorized(new { Message = "Invalid email or password" });
        }

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        if (result == null)
        {
            return BadRequest(new { Message = "Invalid or expired refresh token" });
        }

        return Ok(result);
    }

    [Authorize]
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RevokeTokenAsync(request.RefreshToken);
        if (!result)
        {
            return BadRequest(new { Message = "Invalid refresh token" });
        }

        return Ok(new { Message = "Token successfully revoked" });
    }

    [Authorize]
    [HttpGet("validate-token")]
    public IActionResult ValidateToken()
    {
        return Ok(new { Message = "Token is valid" });
    }

    // OAuth Endpoints

    /// <summary>
    /// Login with OAuth provider (ID token flow)
    /// Used by mobile apps and SPAs
    /// </summary>
    [HttpPost("oauth/login")]
    public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _oauthService.AuthenticateWithProviderAsync(
            request.Provider,
            request.Token);

        if (result == null)
        {
            return BadRequest(new { Message = "OAuth authentication failed" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get OAuth authorization URL for provider
    /// Used to redirect user to provider's login page
    /// </summary>
    [HttpGet("oauth/{provider}/authorize")]
    public IActionResult GetAuthorizationUrl(string provider, [FromQuery] string? redirectUri = null)
    {
        try
        {
            var url = _oauthService.GetAuthorizationUrl(provider, redirectUri);
            return Ok(new { AuthorizationUrl = url });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// OAuth callback (authorization code flow)
    /// Called by provider after user authorizes app
    /// </summary>
    [HttpPost("oauth/callback")]
    public async Task<IActionResult> OAuthCallback([FromBody] OAuthCallbackRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!string.IsNullOrEmpty(request.Error))
        {
            return BadRequest(new
            {
                Error = request.Error,
                Description = request.ErrorDescription
            });
        }

        var result = await _oauthService.HandleCallbackAsync(request);

        if (result == null)
        {
            return BadRequest(new { Message = "OAuth callback processing failed" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Link OAuth provider to current user account
    /// Allows user to add OAuth login to existing account
    /// </summary>
    [Authorize]
    [HttpPost("oauth/link")]
    public async Task<IActionResult> LinkOAuthProvider([FromBody] OAuthLoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _oauthService.LinkProviderAsync(
            userId,
            request.Provider,
            request.Token);

        if (!result)
        {
            return BadRequest(new { Message = "Failed to link OAuth provider" });
        }

        return Ok(new { Message = "OAuth provider linked successfully" });
    }

    /// <summary>
    /// Unlink OAuth provider from current user account
    /// Removes OAuth login method (requires password to be set)
    /// </summary>
    [Authorize]
    [HttpDelete("oauth/unlink/{provider}")]
    public async Task<IActionResult> UnlinkOAuthProvider(string provider)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _oauthService.UnlinkProviderAsync(userId, provider);

        if (!result)
        {
            return BadRequest(new { Message = "Failed to unlink OAuth provider" });
        }

        return Ok(new { Message = "OAuth provider unlinked successfully" });
    }
}
