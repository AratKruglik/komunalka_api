using KomunalkaAPI.DTO.Auth;
using KomunalkaAPI.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Asp.Versioning.ApiVersion("1.0")]
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
            return BadRequest(new { Message = "Користувач з таким email вже існує" });
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
            return Unauthorized(new { Message = "Невірний email або пароль" });
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
            return BadRequest(new { Message = "Недійсний або прострочений токен оновлення" });
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
            return BadRequest(new { Message = "Недійсний токен оновлення" });
        }

        return Ok(new { Message = "Токен успішно відкликано" });
    }

    [Authorize]
    [HttpGet("validate-token")]
    public IActionResult ValidateToken()
    {
        return Ok(new { Message = "Токен дійсний" });
    }
}
