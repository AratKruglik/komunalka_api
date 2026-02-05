# OAuth Setup Guide

This guide explains how to configure OAuth authentication providers for Komunalka API.

## Overview

Komunalka API supports OAuth 2.0 authentication with the following providers:
- **Google** - ID Token + Authorization Code flow
- **GitHub** - Authorization Code flow with access token

### Authentication Flow

1. Client requests authorization URL from API
2. User is redirected to provider's consent screen
3. Provider redirects back with authorization code
4. API exchanges code for tokens and user info
5. API returns JWT tokens to client

## Google OAuth Setup

### Step 1: Create Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Click "Select a project" → "New Project"
3. Enter project name (e.g., "Komunalka") and click "Create"

### Step 2: Configure OAuth Consent Screen

1. Navigate to **APIs & Services** → **OAuth consent screen**
2. Select **External** user type (or Internal for Workspace)
3. Fill in required fields:
   - App name: `Komunalka`
   - User support email: your email
   - Developer contact: your email
4. Click "Save and Continue"

### Step 3: Configure Scopes

1. Click "Add or Remove Scopes"
2. Add these scopes:
   - `openid`
   - `email`
   - `profile`
3. Click "Update" → "Save and Continue"

### Step 4: Create OAuth Credentials

1. Go to **APIs & Services** → **Credentials**
2. Click **Create Credentials** → **OAuth client ID**
3. Select **Web application**
4. Configure:
   - Name: `Komunalka Web Client`
   - Authorized redirect URIs:
     - Development: `http://localhost:5095/api/v1/auth/oauth/callback`
     - Production: `https://your-domain.com/api/v1/auth/oauth/callback`
5. Click "Create"
6. Copy **Client ID** and **Client Secret**

### Step 5: Configure Environment

Add to your `.env` file:

```env
OAUTH__Google__ClientId=your-client-id.apps.googleusercontent.com
OAUTH__Google__ClientSecret=your-client-secret
OAUTH__Google__RedirectUri=http://localhost:5095/api/v1/auth/oauth/callback
```

## GitHub OAuth Setup

### Step 1: Create GitHub OAuth App

1. Go to [GitHub Developer Settings](https://github.com/settings/developers)
2. Click **OAuth Apps** → **New OAuth App**
3. Fill in:
   - Application name: `Komunalka`
   - Homepage URL: `http://localhost:5095`
   - Authorization callback URL: `http://localhost:5095/api/v1/auth/oauth/callback`
4. Click "Register application"

### Step 2: Get Credentials

1. Copy the **Client ID**
2. Click "Generate a new client secret"
3. Copy the **Client Secret** (shown only once)

### Step 3: Configure Environment

Add to your `.env` file:

```env
OAUTH__GitHub__ClientId=your-github-client-id
OAUTH__GitHub__ClientSecret=your-github-client-secret
OAUTH__GitHub__RedirectUri=http://localhost:5095/api/v1/auth/oauth/callback
```

## API Endpoints

### Get Authorization URL

```http
GET /api/v1/auth/oauth/{provider}/authorize
```

**Parameters:**
- `provider` - `google` or `github`
- `redirectUri` (optional) - Custom redirect URI

**Response:**
```json
{
  "authorizationUrl": "https://accounts.google.com/o/oauth2/v2/auth?..."
}
```

### Handle Callback

```http
POST /api/v1/auth/oauth/callback
```

**Request Body:**
```json
{
  "provider": "google",
  "code": "authorization-code-from-provider",
  "state": "state-parameter-from-url"
}
```

**Success Response:**
```json
{
  "userId": 1,
  "username": "user",
  "email": "user@example.com",
  "token": "jwt-token",
  "refreshToken": "refresh-token",
  "expiration": "2024-01-01T12:00:00Z",
  "authProvider": "Google",
  "emailVerified": true
}
```

### Authenticate with Token (Mobile/SPA)

```http
POST /api/v1/auth/oauth/{provider}
```

**Request Body:**
```json
{
  "token": "id-token-or-access-token"
}
```

### Link Provider to Existing Account

```http
POST /api/v1/auth/oauth/{provider}/link
Authorization: Bearer {jwt-token}
```

**Request Body:**
```json
{
  "token": "id-token-from-provider"
}
```

### Unlink Provider

```http
DELETE /api/v1/auth/oauth/{provider}/unlink
Authorization: Bearer {jwt-token}
```

## Testing OAuth Flow

### Using cURL

1. Get authorization URL:
```bash
curl http://localhost:5095/api/v1/auth/oauth/google/authorize
```

2. Open the URL in browser and complete authentication

3. Extract `code` and `state` from redirect URL

4. Exchange code for tokens:
```bash
curl -X POST http://localhost:5095/api/v1/auth/oauth/callback \
  -H "Content-Type: application/json" \
  -d '{"provider":"google","code":"...","state":"..."}'
```

### Using Swagger UI

1. Start the API: `dotnet run`
2. Navigate to `http://localhost:5095/swagger`
3. Find OAuth endpoints under **Auth** section
4. Use "Try it out" to test endpoints

## Troubleshooting

### "redirect_uri_mismatch" Error

**Cause:** Redirect URI in request doesn't match configured URIs in provider console.

**Solution:**
1. Check the exact URI in your `.env` file
2. Ensure it matches exactly in Google/GitHub console (including trailing slashes)
3. For development, use `http://localhost:5095/api/v1/auth/oauth/callback`

### "Invalid or expired OAuth state" Error

**Cause:** State parameter validation failed. Possible reasons:
- State expired (5-minute TTL)
- State was already used
- State was tampered with

**Solution:**
1. Request a new authorization URL
2. Complete OAuth flow within 5 minutes
3. Don't reuse authorization URLs

### "User with email already exists" Error

**Cause:** Email is associated with a password-based account.

**Solution:**
1. User should log in with password
2. Then link OAuth provider via `/api/v1/auth/oauth/{provider}/link`

### "Cannot unlink OAuth provider" Error

**Cause:** User has no password set and OAuth is the only auth method.

**Solution:**
User must set a password before unlinking OAuth provider.

### "OAuth provider not found" Error

**Cause:** Provider name is misspelled or not configured.

**Solution:**
1. Use exact provider names: `google`, `github` (case-insensitive)
2. Verify provider is configured in `.env`

## Security Considerations

1. **State Parameter** - Always validate state to prevent CSRF attacks
2. **HTTPS in Production** - Use HTTPS for all redirect URIs in production
3. **Client Secrets** - Never expose client secrets in client-side code
4. **Token Storage** - Store tokens securely, prefer httpOnly cookies for web
5. **Scope Minimization** - Request only necessary scopes

## Production Configuration

Update `.env` for production:

```env
# Google OAuth
OAUTH__Google__ClientId=production-client-id
OAUTH__Google__ClientSecret=production-secret
OAUTH__Google__RedirectUri=https://your-domain.com/api/v1/auth/oauth/callback

# GitHub OAuth
OAUTH__GitHub__ClientId=production-client-id
OAUTH__GitHub__ClientSecret=production-secret
OAUTH__GitHub__RedirectUri=https://your-domain.com/api/v1/auth/oauth/callback
```

Remember to add production redirect URIs to each provider's console.
