using KomunalkaAPI.DTO.Auth;

namespace KomunalkaAPI.Services.Auth;

public interface IAuthService
{
    Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request);
    Task<AuthenticationResponse?> RegisterAsync(RegisterUserRequest request);
    Task<AuthenticationResponse?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
}
