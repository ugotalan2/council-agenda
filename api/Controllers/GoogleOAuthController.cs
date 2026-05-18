using Asp.Versioning;
using CouncilAgendaApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v1/google")]
public class GoogleOAuthController : BaseController
{
    private readonly IConfiguration _config;

    public GoogleOAuthController(AppDbContext db, IConfiguration config) : base(db)
    {
        _config = config;
    }

    [HttpGet("connect/{orgId}")]
    public IActionResult Connect(Guid orgId)
    {
        var clientId = _config["Google:OAuthClientId"];
        var redirectUri = _config["Google:OAuthRedirectUri"];

        var scope = Uri.EscapeDataString(
            "https://www.googleapis.com/auth/drive " +
            "https://www.googleapis.com/auth/documents"
        );

        var state = orgId.ToString();

        var url = $"https://accounts.google.com/o/oauth2/v2/auth" +
            $"?client_id={clientId}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri!)}" +
            $"&response_type=code" +
            $"&scope={scope}" +
            $"&access_type=offline" +
            $"&prompt=consent" +
            $"&state={state}";

        return Redirect(url);
    }

    [HttpGet("oauth/callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
    {
        var clientId = _config["Google:OAuthClientId"];
        var clientSecret = _config["Google:OAuthClientSecret"];
        var redirectUri = _config["Google:OAuthRedirectUri"];

        // Exchange code for tokens
        using var http = new HttpClient();
        var tokenResponse = await http.PostAsync("https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId!,
                ["client_secret"] = clientSecret!,
                ["redirect_uri"] = redirectUri!,
                ["grant_type"] = "authorization_code"
            }));

        var json = await tokenResponse.Content.ReadAsStringAsync();
        var tokens = System.Text.Json.JsonDocument.Parse(json).RootElement;

        if (!tokens.TryGetProperty("refresh_token", out var refreshTokenEl))
        {
            return BadRequest("No refresh token returned. Make sure prompt=consent is set.");
        }

        var refreshToken = refreshTokenEl.GetString();
        var orgId = Guid.Parse(state);

        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == orgId);
        if (org == null) return NotFound("Organization not found");

        org.GoogleRefreshToken = refreshToken;
        await _db.SaveChangesAsync();

        // Redirect back to org settings page in the frontend
        var frontendUrl = _config["Frontend:BaseUrl"];
        return Redirect($"{frontendUrl}/organizations/{orgId}?googleConnected=true");
    }
}