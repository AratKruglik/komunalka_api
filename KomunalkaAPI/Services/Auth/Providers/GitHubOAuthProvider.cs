using KomunalkaAPI.DTO.Auth;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KomunalkaAPI.Services.Auth.Providers;

public class GitHubOAuthProvider : IOAuthProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GitHubOAuthProvider> _logger;
    private readonly HttpClient _httpClient;

    private const string TokenEndpoint = "https://github.com/login/oauth/access_token";
    private const string UserEndpoint = "https://api.github.com/user";
    private const string EmailsEndpoint = "https://api.github.com/user/emails";
    private const string AuthorizationEndpoint = "https://github.com/login/oauth/authorize";

    public string ProviderName => "GitHub";

    public GitHubOAuthProvider(
        IConfiguration configuration,
        ILogger<GitHubOAuthProvider> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<OAuthUserInfo?> ValidateTokenAsync(string accessToken)
    {
        try
        {
            var clientId = _configuration["OAuth:GitHub:ClientId"];
            if (string.IsNullOrEmpty(clientId))
            {
                _logger.LogError("GitHub OAuth ClientId not configured");
                return null;
            }

            return await GetUserInfoAsync(accessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating GitHub access token");
            return null;
        }
    }

    public async Task<OAuthUserInfo?> ExchangeCodeAsync(string code, string? redirectUri = null)
    {
        try
        {
            var clientId = _configuration["OAuth:GitHub:ClientId"];
            var clientSecret = _configuration["OAuth:GitHub:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogError("GitHub OAuth credentials not configured");
                return null;
            }

            var requestData = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "code", code }
            };

            if (!string.IsNullOrEmpty(redirectUri))
            {
                requestData["redirect_uri"] = redirectUri;
            }
            else
            {
                var configRedirectUri = _configuration["OAuth:GitHub:RedirectUri"];
                if (!string.IsNullOrEmpty(configRedirectUri))
                {
                    requestData["redirect_uri"] = configRedirectUri;
                }
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint);
            request.Content = new FormUrlEncodedContent(requestData);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to exchange GitHub code for token: {StatusCode}",
                    response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<GitHubTokenResponse>(content);

            if (string.IsNullOrEmpty(tokenResponse?.AccessToken))
            {
                _logger.LogWarning("GitHub token response missing access_token. Response: {Response}", content);
                return null;
            }

            return await GetUserInfoAsync(tokenResponse.AccessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging GitHub authorization code");
            return null;
        }
    }

    public string GetAuthorizationUrl(string state, string? redirectUri = null)
    {
        var clientId = _configuration["OAuth:GitHub:ClientId"];
        var redirect = redirectUri ?? _configuration["OAuth:GitHub:RedirectUri"];
        var scope = "user:email";

        var authUrl = AuthorizationEndpoint +
            $"?client_id={Uri.EscapeDataString(clientId ?? "")}" +
            $"&redirect_uri={Uri.EscapeDataString(redirect ?? "")}" +
            $"&scope={Uri.EscapeDataString(scope)}" +
            $"&state={Uri.EscapeDataString(state)}";

        return authUrl;
    }

    private async Task<OAuthUserInfo?> GetUserInfoAsync(string accessToken)
    {
        void ConfigureRequest(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
            request.Headers.UserAgent.ParseAdd("Komunalka-API");
        }

        using var userRequest = new HttpRequestMessage(HttpMethod.Get, UserEndpoint);
        ConfigureRequest(userRequest);

        var userResponse = await _httpClient.SendAsync(userRequest);
        if (!userResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Failed to get GitHub user info: {StatusCode}",
                userResponse.StatusCode);
            return null;
        }

        var userContent = await userResponse.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<GitHubUserResponse>(userContent);

        if (userInfo == null || userInfo.Id == 0)
        {
            _logger.LogWarning("Invalid GitHub user response");
            return null;
        }

        string? primaryEmail = userInfo.Email;
        bool emailVerified = false;

        using var emailsRequest = new HttpRequestMessage(HttpMethod.Get, EmailsEndpoint);
        ConfigureRequest(emailsRequest);

        var emailsResponse = await _httpClient.SendAsync(emailsRequest);
        if (emailsResponse.IsSuccessStatusCode)
        {
            var emailsContent = await emailsResponse.Content.ReadAsStringAsync();
            var emails = JsonSerializer.Deserialize<List<GitHubEmailResponse>>(emailsContent);

            var primaryEmailInfo = emails?.FirstOrDefault(e => e.Primary && e.Verified)
                ?? emails?.FirstOrDefault(e => e.Verified)
                ?? emails?.FirstOrDefault(e => e.Primary);

            if (primaryEmailInfo != null)
            {
                primaryEmail = primaryEmailInfo.Email;
                emailVerified = primaryEmailInfo.Verified;
            }
        }
        else
        {
            _logger.LogWarning(
                "Failed to get GitHub user emails: {StatusCode}. Using profile email.",
                emailsResponse.StatusCode);
        }

        if (string.IsNullOrEmpty(primaryEmail))
        {
            _logger.LogWarning("Could not obtain email for GitHub user {Login}", userInfo.Login);
            return null;
        }

        _logger.LogInformation(
            "GitHub token validated for user: {Login}",
            userInfo.Login);

        string? firstName = null;
        string? lastName = null;
        if (!string.IsNullOrEmpty(userInfo.Name))
        {
            var nameParts = userInfo.Name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            firstName = nameParts.Length > 0 ? nameParts[0] : null;
            lastName = nameParts.Length > 1 ? nameParts[1] : null;
        }

        return new OAuthUserInfo
        {
            Provider = ProviderName,
            ExternalId = userInfo.Id.ToString(),
            Email = primaryEmail,
            EmailVerified = emailVerified,
            FirstName = firstName,
            LastName = lastName,
            Username = userInfo.Login,
            AvatarUrl = userInfo.AvatarUrl,
            AdditionalData = new Dictionary<string, object>
            {
                { "login", userInfo.Login ?? "" },
                { "bio", userInfo.Bio ?? "" },
                { "location", userInfo.Location ?? "" },
                { "company", userInfo.Company ?? "" },
                { "html_url", userInfo.HtmlUrl ?? "" }
            }
        };
    }

    #region Response Models

    private class GitHubTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; set; }
    }

    private class GitHubUserResponse
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("login")]
        public string? Login { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }

        [JsonPropertyName("bio")]
        public string? Bio { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("company")]
        public string? Company { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }
    }

    private class GitHubEmailResponse
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("primary")]
        public bool Primary { get; set; }

        [JsonPropertyName("verified")]
        public bool Verified { get; set; }

        [JsonPropertyName("visibility")]
        public string? Visibility { get; set; }
    }

    #endregion
}
