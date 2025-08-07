using System.Security.Claims;
using KomunalkaAPI.DTO.Auth;
using KomunalkaAPI.Models;

namespace KomunalkaAPI.Services.Auth;

public interface IJwtService
{
    string GenerateJwtToken(User? user);
    RefreshToken GenerateRefreshToken(User? user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    DateTime GetTokenExpirationTime(string token);
}
