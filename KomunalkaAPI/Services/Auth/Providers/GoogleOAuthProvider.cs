using Google.Apis.Auth;
using KomunalkaAPI.DTO.Auth;
using System.Text.Json;

namespace KomunalkaAPI.Services.Auth.Providers;

public class GoogleOAuthProvider : IOAuthProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleOAuthProvider> _logger;
    private readonly HttpClient _httpClient;

    public string ProviderName => "Google";

    public GoogleOAuthProvider(
        IConfiguration configuration,
        ILogger<GoogleOAuthProvider> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<OAuthUserInfo?> ValidateTokenAsync(string idToken)
    {
        try
        {
            var clientId = _configuration["OAuth:Google:ClientId"];
            if (string.IsNullOrEmpty(clientId))
            {
                _logger.LogError("Google OAuth ClientId not configured");
                return null;
            }

            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

            _logger.LogInformation(
                "Google ID token validated successfully for user: {Email}",
                payload.Email);

            return new OAuthUserInfo
            {
                Provider = ProviderName,
                ExternalId = payload.Subject,
                Email = payload.Email,
                EmailVerified = payload.EmailVerified,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                Username = payload.Name,
                AvatarUrl = payload.Picture,
                AdditionalData = new Dictionary<string, object>
                {
                    { "locale", payload.Locale ?? "" },
                    { "hosted_domain", payload.HostedDomain ?? "" }
                }
            };
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid Google ID token");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google ID token");
            return null;
        }
    }

    public async Task<OAuthUserInfo?> ExchangeCodeAsync(string code, string? redirectUri = null)
    {
        try
        {
            var clientId = _configuration["OAuth:Google:ClientId"];
            var clientSecret = _configuration["OAuth:Google:ClientSecret"];
            var tokenEndpoint = "https://oauth2.googleapis.com/token";

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogError("Google OAuth credentials not configured");
                return null;
            }

            var requestData = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "grant_type", "authorization_code" },
                { "redirect_uri", redirectUri ?? _configuration["OAuth:Google:RedirectUri"] ?? "" }
            };

            var response = await _httpClient.PostAsync(
                tokenEndpoint,
                new FormUrlEncodedContent(requestData));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to exchange Google code for tokens: {StatusCode}",
                    response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(content);

            if (tokenResponse?.IdToken == null)
            {
                _logger.LogWarning("Google token response missing id_token");
                return null;
            }

            return await ValidateTokenAsync(tokenResponse.IdToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging Google authorization code");
            return null;
        }
    }

    public string GetAuthorizationUrl(string state, string? redirectUri = null)
    {
        var clientId = _configuration["OAuth:Google:ClientId"];
        var redirect = redirectUri ?? _configuration["OAuth:Google:RedirectUri"];
        var scopes = "openid profile email";

        var authUrl = "https://accounts.google.com/o/oauth2/v2/auth" +
            $"?client_id={Uri.EscapeDataString(clientId ?? "")}" +
            $"&redirect_uri={Uri.EscapeDataString(redirect ?? "")}" +
            $"&response_type=code" +
            $"&scope={Uri.EscapeDataString(scopes)}" +
            $"&state={Uri.EscapeDataString(state)}" +
            $"&access_type=offline" +
            $"&prompt=consent";

        return authUrl;
    }

    private class GoogleTokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("id_token")]
        public string? IdToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
    }
}
