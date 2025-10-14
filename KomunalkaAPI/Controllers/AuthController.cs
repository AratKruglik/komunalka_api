using KomunalkaAPI.DTO.Auth;
using KomunalkaAPI.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
            return BadRequest(new { Message = "A user with this email already exists" });
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

        return Ok(new { Message = "Refresh token revoked successfully" });
    }

    [Authorize]
    [HttpGet("validate-token")]
    public IActionResult ValidateToken()
    {
        return Ok(new { Message = "Token is valid" });
    }
}
